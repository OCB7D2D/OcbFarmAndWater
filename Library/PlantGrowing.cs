using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PlantGrowing : WorldNode, ITickable, IGrowParameters
{
    public readonly int ClrIdx;
    public ulong StartTick;

    public int LightValue;
    public int FertilityLevel;
    public float GrowProgress;

    public float GrowthFactor = 0.01f;

    public float HasWater = 0.0f;
    public bool NeedsWater => true;
    public Vector2i SearchArea = new Vector2i(2, 2);
    public Vector2i SearchHeigh = new Vector2i(-2, 2);

    // List of sprinklers where we get water
    public List<Vector3i> Sprinklers
        = new List<Vector3i>();

    // List of wells from where we get water
    public List<PipeGridWell> Wells
        = new List<PipeGridWell>();

    public bool AddWell(PipeGridWell well)
    {
        int idx = Wells.IndexOf(well);
        if (idx != -1) return false;
        Wells.Add(well);
        return true;
    }

    public bool RemoveWell(PipeGridWell well)
    {
        return Wells.Remove(well);
    }

    // How does a plant know where it can get water from?
    // 1) When it is added, scan the perimeter completely
    // 2) During ticks scan if well has run empty? ???

    // public bool IsLoaded { get; set; }

    public bool GetIsLoaded()
    {
        return BlockHelper.IsLoaded(WorldPos);
    }

    public ScheduledTick Scheduled = null;

    public PlantGrowing(Vector3i worldPos, int clrIdx, BlockValue block,
        int lightValue = 0, int fertilityLevel = 0, float growProgress = 0)
        : base(worldPos, block)
    {
        StartTick = GameTimer.Instance.ticks;
        ClrIdx = clrIdx;
        // IsLoaded = loaded;
        LightValue = lightValue;
        FertilityLevel = fertilityLevel;
        GrowProgress = growProgress;
    }

    static readonly FieldInfo FieldNextPlant = AccessTools
        .Field(typeof(BlockPlantGrowing), "nextPlant");

    public void RegisterScheduled(ulong ticks = 60u)
    {
        if (Scheduled != null) PlantManager.DeleteScheduledTick(Scheduled);
        Scheduled = PlantManager.AddScheduleTick(GetIsLoaded() ? ticks : 600u, this);
    }

    public int GetLightLevel(WorldBase world)
    {
        if (GetIsLoaded()) LightValue = world
            .GetBlockLightValue(ClrIdx, WorldPos);
        return LightValue;
    }

    public int GetFertilityLevel(WorldBase world)
    {
        if (WorldPos.y == 0) return 0;
        if (GetIsLoaded()) FertilityLevel = world
            .GetBlock(WorldPos + Vector3i.down)
            .Block.blockMaterial.FertileLevel;
        return FertilityLevel;
    }

    public void OnUnloaded(WorldBase world)
    {
        // Update cached values
        GetLightLevel(world);
        GetFertilityLevel(world);
    }

    public override void Tick(WorldBase world, ulong delta)
    {
        Scheduled = null;
        var light = GetLightLevel(world);
        var fertility = GetFertilityLevel(world);
        // Check if plant needs water to grow
        // Should be the only water grid API use
        if (NeedsWater)
        {
            // 3% without water
            // GrowthFactor = 0.03f;
            HasWater = 0.0f;
            int consumedWater = 0;
            // See if we can consume any water
            foreach (PipeGridWell well in Wells)
            {
                if (well.ConsumeWater(0.01f * delta / 100f))
                    consumedWater++;
            }

            float Grow = consumedWater > 0 ?
                Mathf.Pow(delta / 1000f, 1.25f) :
                -Mathf.Pow(delta / 1000f, 1.75f);

            Log.Out("Grow {0}", Grow);

            GrowthFactor += Grow;
            if (GrowthFactor > 2f) GrowthFactor = 2f;
            else if (GrowthFactor < 0f) GrowthFactor = 0.03f;
        }
        else
        {
            GrowthFactor = 1f; // Base value
        }

        GrowProgress += light / 1024f / 8f * delta * GrowthFactor * 1;
        Log.Out("Ticked {0} (loaded: {1}, progress: {2:0}%, light: {3})",
            GetBlock().GetBlockName(),
            GetIsLoaded(), GrowProgress * 100f, light);
        if (GrowProgress < 1f) RegisterScheduled();
        else GrowToNext(world, PlantManager.Instance);
    }

    public void OnLoaded(WorldBase world, Vector3i position, BlockValue block, bool added = false)
    {
        // Do not search on load only?
        if (added == false) return;
        // Add more wells to our collection
        PipeGridManager.Find(position,
            SearchArea, SearchHeigh, 
            ref Wells);
    }


    public void GrowToNext(WorldBase world, PlantManager manager)
    {
        int current = BlockID;
        // Check if block is currently loaded
        if (GetIsLoaded())
        {
            // Get BlockValue from currently loaded world/chunk
            BlockValue block = world.GetBlock(0, WorldPos);
            if (block.isair) Log.Error("Loaded Plant is Air?");
            else current = block.type;
        }

        // This should hold true for every plant added to the manager!
        if (current == BlockValue.Air.type) Log.Error("Managed Plant is Air?");
        GetBlock(out BlockPlantGrowing grown);
        if (grown == null) throw new Exception("Invalid Block for growing plant!");
        // Get the replacement via protected/dynamic method call
        BlockValue next = (BlockValue)FieldNextPlant.GetValue(grown);
        // Emulate add/remove events directly
        if (next.Block is BlockPlantGrowing)
        {
            Log.Warning("|| Plant moved to next step");
            // Update "cached" state
            BlockID = next.type;
            // Reset growth timers
            StartTick = GameTimer.Instance.ticks;
        }
        else
        {
            Log.Warning("|| Plant is now fully grown {0}", next.type);
            // Register a harvestable crop
            var plant = new PlantHarvestable(WorldPos, next,
                GetLightLevel(world), GetFertilityLevel(world));
            // Add a new plant to grow
            manager.AddHarvestable(plant.WorldPos, plant);
            // Remove plant once fully grown
            manager.RemoveGrowing(WorldPos);
        }
        // Update block to represent new type
        // Either update directly or when loaded
        manager.UpdateBlockValue(world, WorldPos, next, this);
    }


    public override void Write(BinaryWriter bw)
    {
        base.Write(bw);
        // bw.Write(IsLoaded);
        bw.Write(ClrIdx);
        bw.Write(StartTick);
        bw.Write(GrowProgress);
        bw.Write((byte)LightValue);
        bw.Write((byte)FertilityLevel);
    }

    public PlantGrowing(BinaryReader br)
        : base(br)
    {
        // bool isLoaded = br.ReadBoolean();
        ClrIdx = br.ReadInt32();
        StartTick = br.ReadUInt64();
        GrowProgress = br.ReadSingle();
        LightValue = br.ReadByte();
        FertilityLevel = br.ReadByte();
    }

}
