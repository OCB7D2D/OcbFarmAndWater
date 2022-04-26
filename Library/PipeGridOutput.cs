using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PipeGridOutput : PipeGridPowered, ITickable
{

    public PipeGridOutput(Vector3i position, BlockValue block)
        : base(position, block)
    { 
    }

    public override bool IsConnection() => false;

    public override bool BreakDistance() => true;

    public override int GetStorageType() => 1;

    public HashSet<PipeGridWell> Wells
        = new HashSet<PipeGridWell>();

    public bool IsWorking { get => IsPowered; }

    public override bool HasInterval(out ulong interval)
    {
        interval = 10 + (ulong)Random.Range(0, 20); ;
        return true;
    }

    public PipeGridOutput(BinaryReader br)
         : base(br)
    {
    }

    public override void Write(BinaryWriter bw)
    {
        // Write base data first
        base.Write(bw);
    }

    public bool AddWell(PipeGridWell well)
    {
        return Wells.Add(well);
    }

    public bool RemoveWell(PipeGridWell well)
    {
        return Wells.Remove(well);
    }

    public override void Tick(WorldBase world, ulong delta)
    {
        if (!IsWorking) return;
        foreach (var well in Wells)
        {
            well.FillWater(0.15f);
        }
    }
}
