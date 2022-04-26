using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PipeGridWell : WorldNode
{

    public uint MaxWaterLevel = 150;
    public float WaterAvailable = 0;
    public int SunLight = 0;

    // Keep a list of pumps to get water from?
    // Add pumps from different grids (sources)?
    HashSet<PipeGridOutput> Outputs =
        new HashSet<PipeGridOutput>();

    // private void ScheduleNextTick(ulong ticks = 30)
    // {
    //     var tick = GameTimer.Instance.ticks + ticks;
    //     GlobalTicker.Instance.Schedule(tick, this);
    // }

    protected override void Init()
    {
        // ScheduleNextTick();
    }

    public PipeGridWell(Vector3i position, BlockValue block)
        : base(position, block)
    { }

    public static int StorageType => 9;

    public float WaterNeeded { get => MaxWaterLevel - WaterAvailable; }

    public PipeGridWell(BinaryReader br)
        : base(br)
    {
        WaterAvailable = br.ReadSingle();
        MaxWaterLevel = br.ReadUInt32();
        SunLight = br.ReadByte();
    }

    public override void Write(BinaryWriter bw)
    {
        // Write base data first
        base.Write(bw);
        // Store additional data
        bw.Write(WaterAvailable);
        bw.Write(MaxWaterLevel);
        bw.Write((byte)SunLight);
    }

    public bool AddOutput(PipeGridOutput output)
    {
        return Outputs.Add(output);
    }

    public bool RemoveOutput(PipeGridOutput output)
    {
        return Outputs.Remove(output);
    }

    public void UpdateBlockEntity(BlockEntityData entity)
    {
        if (entity == null) return;
        if (entity.transform == null) return;
        if (entity.transform.Find("WaterLevel") is Transform transform)
        {
            Vector3 position = transform.localPosition;
            position.y = 0.33f / MaxWaterLevel * WaterAvailable - 0.49f;
            transform.localPosition = position;
        }
    }

    public void UpdateWaterLevel()
    {
        if (GameManager.Instance.World is WorldBase world)
        {
            if (world.GetChunkFromWorldPos(WorldPos) is Chunk chunk)
            {
                UpdateBlockEntity(chunk.GetBlockEntity(WorldPos));
            }
        }
    }

    public bool ConsumeWater(float amount)
    {
        if (WaterAvailable < amount) return false;
        WaterAvailable -= amount;
        return true;
    }

    public void FillWater(float amount)
    {
        if (amount <= 0) return;
        if (WaterAvailable > MaxWaterLevel)
        {
            WaterAvailable = MaxWaterLevel;
            UpdateWaterLevel();
        }
        else if (WaterAvailable < MaxWaterLevel)
        {
            WaterAvailable += amount;
            if (WaterAvailable > MaxWaterLevel)
                WaterAvailable = MaxWaterLevel;
            UpdateWaterLevel();
        }
    }

    public override bool HasInterval(out ulong interval)
    {
        interval = 25 + (ulong)Random.Range(0, 20); ;
        return true;
    }

    public override void Tick(WorldBase world, ulong delta)
    {
        Log.Out("Ticked the well");
        if (WaterAvailable >= MaxWaterLevel) return;
        if (world.GetChunkFromWorldPos(WorldPos) is Chunk chunk)
        {
            SunLight = chunk.GetLight(
                WorldPos.x, WorldPos.y, WorldPos.z,
                Chunk.LIGHT_TYPE.SUN);
        }
        if (SunLight == 15)
        {
            FillWater(0.01f);
        }
    }

}
