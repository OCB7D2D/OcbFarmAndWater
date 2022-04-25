using System.IO;

public abstract class PipeGridNode
{

    public int BlockID = 0; // Air
    public byte Rotation = 0;
    protected ulong LastWalker = 0;

    public Vector3i WorldPos { get; private set; }

    // ToDo: Introduce BlockPipeNode as abstract base
    public IBlockPipeNode Block => global::Block
        .list[BlockID] as IBlockPipeNode;

    public PipeGridNode(Vector3i position, BlockValue block)
    {
        BlockID = block.type;
        WorldPos = position;
        Rotation = block.rotation;
    }

    public abstract int GetStorageType();

    public PipeGridNode(BinaryReader br)
    {
        BlockID = br.ReadInt32();
        WorldPos = new Vector3i(
            br.ReadInt32(),
            br.ReadInt32(),
            br.ReadInt32());
        Rotation = br.ReadByte();
    }

    public virtual void Write(BinaryWriter bw)
    {
        bw.Write(BlockID);
        bw.Write(WorldPos.x);
        bw.Write(WorldPos.y);
        bw.Write(WorldPos.z);
        bw.Write(Rotation);
    }

    public bool CanConnect(int side)
    {
        // Rotates question back into local frame
        return Block.CanConnect(side, Rotation);
    }

}
