using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PipeGridConnection : PipeGridNode
{

    public PipeGrid Grid = null;

    public int PowerIndex = int.MaxValue;
    public int DistanceToSource = int.MaxValue;

    public static int OppositeDirection(int dir)
    {
        if (dir < 0) throw new ArgumentOutOfRangeException();
        if (dir > 5) throw new ArgumentOutOfRangeException();
        return dir > 2 ? dir - 3 : dir + 3;
    }

    public PipeGridConnection[] Neighbours = new PipeGridConnection[6];

    public PipeGridConnection Up { get => Neighbours[0]; set => Neighbours[0] = value; }
    public PipeGridConnection Left { get => Neighbours[1]; set => Neighbours[1] = value; }
    public PipeGridConnection Forward { get => Neighbours[2]; set => Neighbours[2] = value; }
    public PipeGridConnection Down { get => Neighbours[3]; set => Neighbours[3] = value; }
    public PipeGridConnection Right { get => Neighbours[4]; set => Neighbours[4] = value; }
    public PipeGridConnection Back { get => Neighbours[5]; set => Neighbours[5] = value; }

    public PipeGridConnection(Vector3i position, BlockValue block)
        : base(position, block)
    {
    }

    public override int GetStorageType() => 0;

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
        PipeGridConnection neighbour;
        // Assuming we have only one grid around for now
        if (manager.TryGetNode(WorldPos + Vector3i.up, out neighbour)) Up = neighbour;
        if (manager.TryGetNode(WorldPos + Vector3i.left, out neighbour)) Left = neighbour;
        if (manager.TryGetNode(WorldPos + Vector3i.forward, out neighbour)) Forward = neighbour;
        if (manager.TryGetNode(WorldPos + Vector3i.right, out neighbour)) Right = neighbour;
        if (manager.TryGetNode(WorldPos + Vector3i.back, out neighbour)) Back = neighbour;
        if (manager.TryGetNode(WorldPos + Vector3i.down, out neighbour)) Down = neighbour;
    }

    public bool IsEndPoint()
    {
        return GetNeighbourCount() <= 1;
    }

    public int GetNeighbourCount()
    {
        int count = 0;
        foreach (var neighbour in Neighbours)
            count += neighbour == null ? 0 : 1;
        return count;
    }

    static HashSet<PipeGridConnection> seen =
        new HashSet<PipeGridConnection>();

    public void PropagateGridChange(PipeGridConnection prev)
    {
        if (Grid != null && Grid != prev.Grid)
        {
            // Take the grid from the source
            Grid.RemoveConnection(this);
        }
        if (Grid != prev.Grid)
        {
            Grid = prev.Grid;
            Grid.AddConnection(this);
        }
        for (int i = 0; i < 6; i++)
        {
            var neighbour = Neighbours[i];
            // Don't move backwards
            if (neighbour == prev) continue;
            // Can't propagate to non existing
            if (neighbour == null) continue;
            // Make sure to avoid endless loops
            if (seen.Contains(neighbour)) continue;
            seen.Add(neighbour); // Protection
            neighbour.PropagateGridChange(this);
            // seen.Remove(neighbour); // Protection
        }
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

    private static bool BreakDistance(PipeGridConnection connection) => connection.BreakDistance();

    public int Walk(Func<PipeGridConnection, bool> breaker)
    {
        int dist = 0;
        if (breaker(this)) return dist;
        todo.Enqueue(new Walker(this));
        while (todo.Count > 0)
        {
            Walker walk = todo.Dequeue();
            dist = Utils.FastMax(dist, walk.dist);
            for (var i = 0; i < 6; i++)
            {
                var neighbour = walk.cur.Neighbours[i];
                if (neighbour == null) continue;
                if (neighbour == walk.prev) continue;
                if (breaker(neighbour)) continue;
                todo.Enqueue(new Walker(neighbour,
                    walk.cur, walk.dist + 1));
            }
        }
        todo.Clear();
        return dist;
    }

    public int CountLongestDistance()
    {
        int dist = 0;
        if (BreakDistance()) return dist;
        todo.Enqueue(new Walker(this));
        while (todo.Count > 0)
        {
            Walker walk = todo.Dequeue();
            dist = Utils.FastMax(dist, walk.dist);
            for (var i = 0; i < 6; i++)
            {
                var neighbour = walk.cur.Neighbours[i];
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

    public virtual bool IsConnection() => true;
    public virtual bool BreakDistance() => false;

}
