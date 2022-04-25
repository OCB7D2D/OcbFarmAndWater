﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PlantManager : PersistedData<PlantManager>
{

    public static byte FileVersion = 1;

    // Tick rate at which to persist data
    private float SaveTickRate = 20f;

    // Tick rate to grow plants (partially)
    private float ScheduledTickRate = 0.6f;

    // Tick rate to harvest crops (partially)
    // private float HarvestTickRate = 1.8f;

    // Dictionary of all growing plants in the world (either loaded or unloaded chunks)
    public Dictionary<Vector3i, PlantGrowing> Growing = new Dictionary<Vector3i, PlantGrowing>();

    // A set of ticks pre-sorted to dispatch in batches for less CPU strain
    public SortedSet<ScheduledTick> ScheduledTicks = new SortedSet<ScheduledTick>();

    // Dictionary of all harvesters in the world (either loaded or unloaded chunks)
    public Dictionary<Vector3i, PlantHarvester> Harvester = new Dictionary<Vector3i, PlantHarvester>();

    // Index of harvestable crops (either in loaded or unloaded chunks)
    public Dictionary<Vector3i, PlantHarvestable> Harvestable = new Dictionary<Vector3i, PlantHarvestable>();
    
    // Pending block changes to currently unloaded chunks (applied once chunk loads)
    public Dictionary<Vector3i, int> PendingChange = new Dictionary<Vector3i, int>();

    // Path to persist the data to (filename)
    public override string GetStoragePath() => string.Format(
        "{0}/plant-manager5.dat", GameIO.GetSaveGameDir());
    public override string GetBackupPath() => string.Format(
        "{0}/plant-manager5.dat.bak", GameIO.GetSaveGameDir());
    public override string GetThreadKey() => "silent_planManagerDataSave";

    // Static helper function (ToDo: check CPU cost)
    public static bool GetIsLoaded(Vector3i WorldPos)
    {
        if (GameManager.Instance == null) return false;
        if (GameManager.Instance.World == null) return false;
        WorldBase world = GameManager.Instance.World;
        return world.GetChunkFromWorldPos(WorldPos) != null;
    }

    // Constructor
    public PlantManager()
    {
        foreach (Block block in Block.list)
        {
            if (block is BlockPlant)
            {
                // Patch display info for plants (additional data)
                block.DisplayInfo = Block.EnumDisplayInfo.Custom;
            }
        }
    }

    public static ScheduledTick AddScheduleTick(ulong ticks, ITickable tickable)
    {
        ScheduledTick scheduled = new ScheduledTick(ticks, tickable);
        Instance.ScheduledTicks.Add(scheduled);
        // Log.Out("Scheduled in {2} (left growing: {0}, ticks: {1})",
        //     Instance.Growing.Count, Instance.ScheduledTicks.Count, ticks);
        return scheduled;
    }

    public static void DeleteScheduledTick(ScheduledTick scheduled)
    {
        Instance.ScheduledTicks.Remove(scheduled);
    }

    public static bool TryGetGrowing(Vector3i position, out PlantGrowing plant)
    {
        if (instance == null) { plant = null; return false; }
        return instance.Growing.TryGetValue(position, out plant);
    }

    public static bool TryGetHarvester(Vector3i position, out PlantHarvester harvester)
    {
        if (instance == null) { harvester = null; return false; }
        return instance.Harvester.TryGetValue(position, out harvester);
    }


    public static int GetHarvestable(Vector3i WorldPos)
    {
        if (instance == null) return BlockValue.Air.type;
        if (instance.Harvestable.TryGetValue(WorldPos, out PlantHarvestable plant))
        {
            return plant.BlockId;
        }
        return BlockValue.Air.type;
    }

    // Called when growing to next stage or when being (auto) harvested
    public void UpdateBlockValue(WorldBase world, Vector3i WorldPos, BlockValue next, IGrowParameters current)
    {
        // Check if block is currently loaded
        if (GetIsLoaded(WorldPos))
        {
            // Get current block to copy stuff
            var block = world.GetBlock(WorldPos);
            next.rotation = block.rotation;
            next.meta = block.meta;
            next.meta2 = 0;
            // Update world via remote somehow
            Log.Out("UpdateBlockValue 1");
            world.SetBlockRPC(WorldPos, next);
        }
        else
        {
            PendingChange[WorldPos] = next.type;
        }
        if (next.Block is BlockPlantGrowing growing)
        {
            Log.Out("Block is now growing");
            SetGrowing(WorldPos, 0, next,
                current.GetLightLevel(world),
                current.GetFertilityLevel(world));
        }
    }

    public static void LoadedAutoHarvester(WorldBase world, Vector3i position, BlockValue block, bool addIfNotKnown = false)
    {
        if (instance == null) return;
        if (instance.Harvester.TryGetValue(position, out PlantHarvester harvester))
        {
            harvester.OnLoaded(world);
        }
        else if (addIfNotKnown)
        {
            harvester = new PlantHarvester(position);
            instance.Harvester[position] = harvester;
            harvester.OnLoaded(world);
            harvester.RegisterScheduled(0);
        }
    }

    public static void UnloadedAutoHarvester(Vector3i position)
    {
        if (instance == null) return;
        if (instance.Harvester.TryGetValue(position, out PlantHarvester harvester))
        {
            Log.Warning("Unloaded Harvester");
            harvester.OnUnloaded();
        }
        else Log.Error("Unloaded harvester isn't yet managed!?");
    }

    public static void RemoveAutoHarvester(Vector3i position)
    {
        if (instance == null) return;
        instance.Harvester.Remove(position);
    }

    public static void LoadedGrowingPlant(WorldBase world, Vector3i position, BlockValue block, bool addIfNotKnown = false)
    {
        if (instance == null) return;
        if (instance.Growing.TryGetValue(position, out PlantGrowing plant))
        {
            plant.OnLoaded(world, position, block);
        }
        else if(addIfNotKnown)
        {
            plant = new PlantGrowing(position, 0, block.type);
            instance.Growing[position] = plant;
            plant.OnLoaded(world, position, block);
            plant.RegisterScheduled(0);
        }
    }

    public static void UnloadedGrowingPlant(WorldBase world, Vector3i position)
    {
        if (instance == null) return;
        if (instance.Growing.TryGetValue(position, out PlantGrowing plant))
        {
            Log.Out("UnloadedGrowingPlant");
            plant.OnUnloaded(world);
            // Log.Out("Did unload it {0}", plant.IsLoaded);
            // Log.Out("Did unload it 2 {0} {1}", plant.IsLoaded, instance.Growing[position].IsLoaded);
            // instance.Growing[position] = plant;
        }
        else Log.Error("Unloaded plant isn't yet managed!?");
    }

    public static int GetGrowingBlockValue(Vector3i position)
    {
        if (instance == null) return BlockValue.Air.type;
        if (instance.Growing.TryGetValue(position, out PlantGrowing plant))
            return plant.BlockId;
        return BlockValue.Air.type;
    }

    static List<PlantGrowing> GrowNow = new List<PlantGrowing>();


    public static void Cleanup()
    {
        if (instance == null) return;
        instance.CleanupInstance();
    }

    protected override void CleanupInstance()
    {
        // Save out state first
        base.CleanupInstance();
        Growing.Clear();
        Harvestable.Clear();
        instance = null;
    }


    private float SaveWaitTime = 0f;
    private float TickWaitTime = 0f;

    public void Update(WorldBase world)
    {
        var deltaTime = Time.deltaTime;
        instance.TickWaitTime += deltaTime;
        if (instance.TickWaitTime >= ScheduledTickRate)
        {
            instance.TickWaitTime %= ScheduledTickRate;
            TickScheduled(GameManager.Instance.World);
        }
        // instance.HarvestWaitTime += deltaTime;
        // if (instance.HarvestWaitTime >= HarvestTickRate)
        // {
        //     instance.TickWaitTime %= HarvestTickRate;
        //     TickHarvest(GameManager.Instance.World);
        // }
        instance.SaveWaitTime += deltaTime;
        if (instance.SaveWaitTime >= SaveTickRate)
        {
            instance.SaveWaitTime %= SaveTickRate;
            SaveThreaded();
        }
    }

    // Called on `UpdateMainThreadTasks`
    public static void TickManager()
    {
        if (GameManager.Instance.World == null) return;
        if (GameManager.Instance.World.Players == null) return;
        if (GameManager.Instance.World.Players.Count == 0) return;
        if (!GameManager.Instance.gameStateManager.IsGameStarted()) return;
        Instance.Update(GameManager.Instance.World);
    }

    public override void Write(BinaryWriter bw)
    {
        bw.Write(FileVersion);
        bw.Write(Growing.Count);
        foreach (var kv in Growing)
        {
            kv.Value.Write(bw);
        }
        bw.Write(Harvester.Count);
        foreach (var kv in Harvester)
        {
            kv.Value.Write(bw);
        }
        bw.Write(Harvestable.Count);
        foreach (var kv in Harvestable)
        {
            kv.Value.Write(bw);
        }
    }

    public override void Read(BinaryReader br)
    {
        Growing.Clear();
        Harvester.Clear();
        Harvestable.Clear();
        var version = br.ReadByte();
        int growings = br.ReadInt32();
        for (int index = 0; index < growings; ++index)
        {
            var plant = PlantGrowing.Read(br);
            Growing.Add(plant.WorldPos, plant);
            plant.RegisterScheduled();
        }
        int harvesters = br.ReadInt32();
        for (int index = 0; index < harvesters; ++index)
        {
            var harvester = PlantHarvester.Read(br);
            Harvester.Add(harvester.WorldPos, harvester);
            harvester.RegisterScheduled();
        }
        int harvestables = br.ReadInt32();
        for (int index = 0; index < harvestables; ++index)
        {
            var harvestable = PlantHarvestable.Read(br);
            Harvestable.Add(harvestable.WorldPos, harvestable);
        }
        Log.Warning("!!!!!!! Parsed Growing Plants: {0}", Growing.Count);
        Log.Warning("!!!!!!! Parsed Harvestable Plants: {0}", Harvestable.Count);
        Log.Warning("!!!!!!! Parsed Auto Harvesters: {0}", Harvester.Count);
    }

    public void RemoveHarvestable(Vector3i position)
    {
//        Log.Out("Remove Harvestable");
        Harvestable.Remove(position);
    }
    

    public void SetGrowing(Vector3i position, int clrIdx, BlockValue block,
        int lightValue = 0, int fertilityLevel = 0)
    {
        if (block.ischild) return; // Only add main blocks
        if (Growing.TryGetValue(position, out PlantGrowing plant))
        {
            // plant.IsLoaded = loaded;
            plant.BlockId = block.type;
            plant.GrowProgress = 0;
        }
        else
        {
            // This adds a new growing plant, where to get light from?
            plant = new PlantGrowing(position, clrIdx, block.type,
                lightValue, fertilityLevel);
            instance.Growing[position] = plant;
        }
        plant.RegisterScheduled(0);

        //        Log.Out("+++++ Add Growing {0} {1} {2} {3}", Growing.Count,
        //            block.Block.GetBlockName(), position, block);
    }

    public void TickScheduled(WorldBase world)
    {
        int done = 0;
        GrowNow.Clear();
        var tick = GameTimer.Instance.ticks;
        int todo = Utils.FastMin(ScheduledTicks.Count, 8);
        while (ScheduledTicks.Count != 0)
        {
            if (done > todo) break;
            var scheduled = ScheduledTicks.First();
            if (scheduled.TickTime == ulong.MaxValue) break;
            if (scheduled.TickTime > tick) break;
            ScheduledTicks.Remove(scheduled);
            ulong delta = tick - scheduled.TickStart;
            scheduled.Object.Tick(world, delta);
            done += 1;
        }
    }

    public void RemoveGrowing(Vector3i position)
    {
        if (Growing.TryGetValue(position, out PlantGrowing plant))
            ScheduledTicks.Remove(plant.Scheduled);
        Growing.Remove(position); // Remove the plant
        Log.Out("Unscheduled (left growing: {0}, ticks: {1})",
            Growing.Count, ScheduledTicks.Count);
    }

    public void AddHarvestable(Vector3i position, PlantHarvestable type)
    {
//        Log.Out("Add Havestable {0}", type);
        // Log.Out(Environment.StackTrace);
        Harvestable[position] = type;
    }

}