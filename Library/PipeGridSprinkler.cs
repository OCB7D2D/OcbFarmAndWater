using System.IO;

public class PipeGridSprinkler : PipeGridConnection
{

    public override int GetStorageType() => 4;

    public PipeGridSprinkler(Vector3i position, BlockValue block)
    : base(position, block)
    {
    }
    public PipeGridSprinkler(BinaryReader br)
     : base(br)
    {
    }

    public override void Write(BinaryWriter bw)
    {
        base.Write(bw);
    }

    public void TickUpdate()
    {

    }

}
