using System.IO;

public abstract class PipeGridNode
{
    public Vector3i WorldPos { get; private set; }

    public PipeGridNode(Vector3i position, BlockValue block)
    {
        WorldPos = position;
    }

    public abstract int GetStorageType();

    public PipeGridNode(BinaryReader br)
    {
        WorldPos = new Vector3i(
            br.ReadInt32(),
            br.ReadInt32(),
            br.ReadInt32());
    }

    public virtual void Write(BinaryWriter bw)
    {
        bw.Write(WorldPos.x);
        bw.Write(WorldPos.y);
        bw.Write(WorldPos.z);
    }

}
