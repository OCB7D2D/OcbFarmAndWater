using System.IO;

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

    public void TickUpdate()
    {
        if (GameManager.Instance.World is WorldBase world)
        {
            var block = world.GetBlock(WorldPos + Vector3i.up);
            if (block.isWater)
            {
                // block.damage += 1;
                // world.SetBlockRPC(WorldPos + Vector3i.up, block);
                // Log.Out("Water {0}", block.damage);
            }
        }

    }

}
