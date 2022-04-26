using System.IO;
using UnityEngine;

public abstract class WorldNode : ITickable
{

    public int BlockID = 0; // Air
    public byte Rotation = 0;
    protected ulong LastWalker = 0;

    public virtual void TickUpdate() { }

    // Return block of given type (may return null)
    public bool GetBlock<T>(out T var) where T : class
        => (var = Block.list[BlockID] as T) != null;

    // Base method to get base block instance
    public Block GetBlock() => Block.list[BlockID];

    protected virtual void Init()
    {
        if (!HasInterval(out ulong interval)) return;
        GlobalTicker.Instance.Schedule(interval, this);
    }

    public virtual bool HasInterval(out ulong interval)
    {
        interval = 0;
        return false;
    }

    public Vector3i WorldPos { get; private set; }

    public Vector3i ToWorldPos() => WorldPos;

    // ToDo: Introduce BlockPipeNode as abstract base
    // public virtual Block Block => Block.list[BlockID];

    public WorldNode(Vector3i position, BlockValue block)
    {
        BlockID = block.type;
        WorldPos = position;
        Rotation = block.rotation;
        Init();
    }

    public WorldNode(BinaryReader br)
    {
        BlockID = br.ReadInt32();
        WorldPos = new Vector3i(
            br.ReadInt32(),
            br.ReadInt32(),
            br.ReadInt32());
        Rotation = br.ReadByte();
        Init();
    }

    public virtual void Write(BinaryWriter bw)
    {
        bw.Write(BlockID);
        bw.Write(WorldPos.x);
        bw.Write(WorldPos.y);
        bw.Write(WorldPos.z);
        bw.Write(Rotation);
    }

    public virtual void Tick(WorldBase world, ulong delta)
    {
        // if (!IsInterval()) return;
    }
}
