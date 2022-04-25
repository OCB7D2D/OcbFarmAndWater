using System;
using System.Collections.Generic;
using System.IO;

public class PipeGridPump : PipeGridPowered
{

    PipeGridConnection Start;
    HashSet<PipeGridConnection> End;

    public PipeGridPump(Vector3i position, BlockValue block)
        : base(position, block)
    {
    }

    public override bool IsConnection() => true;

    public override bool BreakDistance() => true;

    public override int GetStorageType() => 2;


public PipeGridPump(BinaryReader br)
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
    }

}
