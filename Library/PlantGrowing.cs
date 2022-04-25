using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PlantGrowing : ITickable, IGrowParameters
{
    public readonly Vector3i WorldPos;
    public readonly int ClrIdx;
    public int BlockId;
    public ulong StartTick;

    public int LightValue;
    public int FertilityLevel;
    public float GrowProgress;

    // public bool IsLoaded { get; set; }

    public bool GetIsLoaded()
    {
        return PlantManager.GetIsLoaded(WorldPos);
    }

    public Vector3i ToWorldPos() => WorldPos;

    public ScheduledTick Scheduled = null;

    public PlantGrowing(Vector3i worldPos, int clrIdx, int blockId,
        int lightValue = 0, int fertilityLevel = 0, float growProgress = 0)
    {
        StartTick = GameTimer.Instance.ticks;
        BlockId = blockId;
        WorldPos = worldPos;
        ClrIdx = clrIdx;
        // IsLoaded = loaded;
        LightValue = lightValue;
        FertilityLevel = fertilityLevel;
        GrowProgress = growProgress;
    }

    static readonly FieldInfo FieldNextPlant = AccessTools
        .Field(typeof(BlockPlantGrowing), "nextPlant");

    public void RegisterScheduled(ulong ticks = 30u)
    {
        if (Scheduled != null) PlantManager.DeleteScheduledTick(Scheduled);
        Scheduled = PlantManager.AddScheduleTick(GetIsLoaded() ? ticks : 90u, this);
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

    public void Tick(WorldBase world, ulong delta)
    {
        Scheduled = null;
        var light = GetLightLevel(world);
        var fertility = GetFertilityLevel(world);
        GrowProgress += light / 1024f / 8f * delta;
        Log.Out("Ticked {0} (loaded: {1}, progress: {2:0}%, light: {3})",
            Block.list[BlockId].GetBlockName(),
            GetIsLoaded(), GrowProgress * 100f, light);
        if (GrowProgress < 1f) RegisterScheduled();
        else GrowToNext(world, PlantManager.Instance);
    }

    public void OnLoaded(WorldBase world, Vector3i position, BlockValue block)
    {
        if (BlockId != block.type)
        {
            // Log.Warning("Loaded Managed Plant has changed from {0} to {1}",
            //     Block.list[BlockId].GetBlockName(), block.Block.GetBlockName());
            // BlockId = block.type;
        }
        // IsLoaded = true;
        // Can't call tick directly
        // Would alter an enumerator
    }


    public void GrowToNext(WorldBase world, PlantManager manager)
    {
        int current = BlockId;
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
        BlockPlantGrowing grown = Block.list[current] as BlockPlantGrowing;
        if (grown == null) throw new Exception("Invalid Block for growing plant!");
        // Get the replacement via protected/dynamic method call
        BlockValue next = (BlockValue)FieldNextPlant.GetValue(grown);
        // Emulate add/remove events directly
        if (next.Block is BlockPlantGrowing)
        {
            Log.Warning("|| Plant moved to next step");
            // Update "cached" state
            BlockId = next.type;
            // Reset growth timers
            StartTick = GameTimer.Instance.ticks;
        }
        else
        {
            Log.Warning("|| Plant is now fully grown {0}", next.type);
            // Register a harvestable crop
            var plant = new PlantHarvestable(WorldPos, next.type,
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


    public void Write(BinaryWriter bw)
    {
        // bw.Write(IsLoaded);
        bw.Write(ClrIdx);
        bw.Write(WorldPos.x);
        bw.Write(WorldPos.y);
        bw.Write(WorldPos.z);
        bw.Write(BlockId);
        bw.Write(StartTick);
        bw.Write(GrowProgress);
        bw.Write((byte)LightValue);
        bw.Write((byte)FertilityLevel);
    }

    public static PlantGrowing Read(BinaryReader br)
    {
        // bool isLoaded = br.ReadBoolean();
        int clrIdx = br.ReadInt32();
        int x = br.ReadInt32();
        int y = br.ReadInt32();
        int z = br.ReadInt32();
        var blockId = br.ReadInt32();
        var plant = new PlantGrowing(
            new Vector3i(x, y, z), clrIdx,
            blockId);
        plant.StartTick = br.ReadUInt64();
        plant.GrowProgress = br.ReadSingle();
        plant.LightValue = br.ReadByte();
        plant.FertilityLevel = br.ReadByte();
        return plant;
    }

}
