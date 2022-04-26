using System.IO;
using UnityEngine;

public class PipeGridSource : PipeGridPowered
{

    public PipeGridSource(Vector3i position, BlockValue block)
        : base(position, block)
    {
    }

    public override bool IsConnection() => false;
    public override bool BreakDistance() => true;

    public override int GetStorageType() => 3;

    public PipeGridSource(BinaryReader br)
     : base(br)
    {
    }

    public override void Write(BinaryWriter bw)
    {
        // Write base data first
        base.Write(bw);
    }

    public override bool HasInterval(out ulong interval)
    {
        interval = 110 + (ulong)Random.Range(0, 20);
        return true;
    }

    public override void Tick(WorldBase world, ulong delta)
    {
        Log.Out("Tick Source");
        var block = world.GetBlock(WorldPos + Vector3i.up);
        if (block.isWater)
        {
            // block.damage += 1;
            // world.SetBlockRPC(WorldPos + Vector3i.up, block);
            // Log.Out("Water {0}", block.damage);
        }
    }

}
