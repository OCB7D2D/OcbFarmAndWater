using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PipeGridConnection : WorldNode
{

    public PipeGrid Grid = null;

    public int PowerIndex = int.MaxValue;
    public int DistanceToSource = int.MaxValue;

    // Return block of given type (may return null)
    public bool Block<T>(out T var) where T : class =>
        (var = global::Block.list[BlockID] as T) != null;

    // Use public API below to keep Count in sync
    private PipeGridConnection[] Neighbours
        = new PipeGridConnection[6];

    // Amount of valid neighbours
    public int Count { get; private set; }

    public PipeGridConnection this[int index]
    {
        get => Neighbours[index];
        set {
            if (Neighbours[index] == null && value != null) Count++;
            else if (Neighbours[index] != null && value == null) Count--;
            Neighbours[index] = value;
        }
    }

    public PipeGridConnection(Vector3i position, BlockValue block)
        : base(position, block)
    {
    }

    public virtual int GetStorageType() => 0;

    public PipeGridConnection(BinaryReader br)
        : base(br)
    {
    }

    public override void Write(BinaryWriter bw)
    {
        base.Write(bw);
    }

    public void InitFromManager(PipeGridManager manager)
    {
        for (int side = 0; side < 6; side++)
        {
            if (manager.TryGetNode(
                WorldPos + FullRotation.Vector[side],
                out PipeGridConnection neighbour))
            {
                Neighbours[side] = neighbour;
            }
        }
    }

    public bool IsEndPoint()
    {
        return Count <= 1;
    }

    public bool CanConnect(int side)
    {
        if (Block(out IBlockPipeNode node))
        {
            // Rotates question back into local frame
            return node.CanConnect(side, Rotation);
        }
        return false;
    }

    public void GetNeighbours(ref List<PipeGridConnection> neighbours)
    {
        foreach (var neighbour in Neighbours)
        {
            if (neighbour == null) continue;
            neighbours.Add(neighbour);
        }
    }

    public List<PipeGridConnection> GetNeighbours()
    {
        List<PipeGridConnection> neighbours
            = new List<PipeGridConnection>();
        GetNeighbours(ref neighbours);
        return neighbours;
    }

    public static PipeGridOutput Read(BinaryReader br)
    {
        return null;
    }

    public override string ToString()
    {
        return string.Format(
            "Connection (Grid {0})",
            Grid != null ? Grid.ID : -1);
    }

    public List<T> Find<T>(Vector2i area, Vector2i height)
    {
        List<T> list = new List<T>();
        int startX = -area.x + WorldPos.x;
        int stopX = area.x + WorldPos.x;
        int startY = height.x + WorldPos.y;
        int stopY = height.y + WorldPos.y;
        int startZ = -area.y + WorldPos.z;
        int stopZ = area.y + WorldPos.z;
        Vector3i offset = new Vector3i();
        for (offset.y = startY; offset.y <= stopY; offset.y++)
        {
            for (offset.x = startX; offset.x <= stopX; offset.x++)
            {
                for (offset.z = startZ; offset.z <= stopZ; offset.z++)
                {
                    if (PipeGridManager.Instance.TryGetNode(
                        offset, out PipeGridConnection connection))
                    {
                        if (connection is T t) list.Add(t);
                    }
                }
            }
        }
        return list;
    }

    private struct Walker
    {
        public int dist;
        public PipeGridConnection cur;
        public PipeGridConnection prev;
        public Walker(PipeGridConnection cur, PipeGridConnection prev = null, int dist = 1)
        {
            this.cur = cur;
            this.prev = prev;
            this.dist = dist;
        }
    }

    private static Queue<Walker> todo
        = new Queue<Walker>();

    public int CountLongestDistance()
    {
        int dist = 0;
        if (Grid.IsCyclic)
        {
            return int.MaxValue;
        }
        if (BreakDistance()) return dist;
        todo.Enqueue(new Walker(this));
        while (todo.Count > 0)
        {
            Walker walk = todo.Dequeue();
            dist = Utils.FastMax(dist, walk.dist);
            for (var side = 0; side < 6; side++)
            {
                var neighbour = walk.cur.Neighbours[side];
                if (neighbour == null) continue;
                if (neighbour == walk.prev) continue;
                if (neighbour.BreakDistance()) continue;
                todo.Enqueue(new Walker(neighbour,
                    walk.cur, walk.dist + 1));
            }
        }
        todo.Clear();
        return dist;
    }

    static uint WalkerIDs = 0;

    private struct Propagater
    {
        public PipeGridConnection Cur;
        public PipeGridConnection Prev;
        public uint WalkID;
        public Propagater(PipeGridConnection cur, PipeGridConnection prev = null, uint walkID = 0)
        {

            Cur = cur;
            Prev = prev;
            if (walkID != 0) WalkID = walkID;
            else WalkID = ++WalkerIDs;
        }
    }

    private static Queue<Propagater> propagate
        = new Queue<Propagater>();

    public void PropagateGridChange(PipeGridConnection prev)
    {
        // Enqueue ourself as the starting point
        propagate.Enqueue(new Propagater(this, prev));
        // Process until no more tree nodes
        while (propagate.Count > 0)
        {
            // Get first item from the queue to process
            Propagater propagater = propagate.Dequeue();
            // Check if the grid actually changes
            if (propagater.Cur.Grid != propagater.Prev.Grid)
            {
                // Check if old item has a grid
                if (propagater.Cur.Grid != null)
                {
                    // Remove from old grid before changing
                    propagater.Cur.Grid.RemoveConnection(this);
                }
                // Update the current grid to match previous
                propagater.Cur.Grid = propagater.Prev.Grid;
                // Register new connection on new grid
                propagater.Cur.Grid.AddConnection(this);
            }
            // Process all potential neighbours
            for (var side = 0; side < 6; side++)
            {
                // Get optional neighbour of given side
                var neighbour = propagater.Cur.Neighbours[side];
                // Skip non existing neighbour
                if (neighbour == null) continue;
                // Don't walk backwards in the tree
                if (neighbour == propagater.Prev) continue;
                // Detect cyclic case by checking our walk id
                if (propagater.Cur.LastWalker == propagater.WalkID)
                {
                    // Mark the grid cyclic and abort here
                    propagater.Cur.Grid.IsCyclic = true;
                }
                else
                {
                    // Propagate to neighbour tree node
                    propagate.Enqueue(new Propagater(
                        neighbour, propagater.Cur,
                        propagater.WalkID));
                }
            }
            // Store our walk id to detect cyclic cases
            propagater.Cur.LastWalker = propagater.WalkID;
        }
        // Make sure to clean up
        propagate.Clear();
    }


    public virtual bool IsConnection() => true;
    public virtual bool BreakDistance() => false;

}
