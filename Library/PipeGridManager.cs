using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class PipeGridManager : PersistedData<PipeGridManager>
{
    private const int MaxDistance = 5;
    public static byte FileVersion = 1;

    // Path to persist the data to (filename)
    public override string GetStoragePath() => string.Format(
        "{0}/water-grids.dat", GameIO.GetSaveGameDir());
    public override string GetBackupPath() => string.Format(
        "{0}/water-grids.dat.bak", GameIO.GetSaveGameDir());
    public override string GetThreadKey() => "silent_waterGridManagerDataSave";

    public List<PipeGrid> Grids = new List<PipeGrid>();

    public Dictionary<Vector3i, PipeGridConnection> Connections
        = new Dictionary<Vector3i, PipeGridConnection>();

    public Dictionary<Vector3i, PipeGridWell> Wells
        = new Dictionary<Vector3i, PipeGridWell>();

    public PipeGridManager()
    {
        ModEvents.GameUpdate.RegisterHandler(Update);
    }

    ulong nextTick = ulong.MinValue;

    private void Update()
    {
        if (nextTick > GameTimer.Instance.ticks) return;
        nextTick = GameTimer.Instance.ticks + 30;
        if (GameManager.Instance.World is WorldBase world)
        {
            foreach (var grid in Grids)
                grid.TickUpdate();
            foreach (var well in Wells)
                well.Value.TickUpdate();
        }
    }

    static Vector2i OutputArea = new Vector2i(20, 20);
    static Vector2i OutputHeight = new Vector2i(-2, 2);

    public int GridCount { get => Grids.Count; }

    public static List<T> Find<T>(Vector3i position, Vector2i area, Vector2i height) where T : PipeGridNode
    {
        List<T> list = new List<T>();
        if (instance == null) return list;
        int startX = -area.x + position.x;
        int stopX = area.x + position.x;
        int startY = height.x + position.y;
        int stopY = height.y + position.y;
        int startZ = -area.y + position.z;
        int stopZ = area.y + position.z;
        Vector3i offset = new Vector3i();
        for (offset.y = startY; offset.y <= stopY; offset.y++)
        {
            for (offset.x = startX; offset.x <= stopX; offset.x++)
            {
                for (offset.z = startZ; offset.z <= stopZ; offset.z++)
                {
                    if (instance.TryGetNode<T>(offset, out T node))
                    {
                        list.Add(node);
                    }
                }
            }
        }
        return list;
    }

    public void AddOutput(PipeGridOutput output)
    {
        AddConnection(output);
        if (output.Grid.AddOutput(output))
        {
            foreach (var well in Find<PipeGridWell>(
                output.WorldPos, OutputArea, OutputHeight))
            {
                output.AddWell(well);
                well.AddOutput(output);
            }
        }
    }

    public void RemoveOutput(Vector3i position)
    {
        if (Connections.TryGetValue(position,
            out PipeGridConnection connection))
        {
            if (connection is PipeGridOutput output)
            {
                if (connection.Grid.RemoveOutput(output))
                {
                    foreach (var well in Find<PipeGridWell>(
                        output.WorldPos, OutputArea, OutputHeight))
                    {
                        output.RemoveWell(well);
                        well.RemoveOutput(output);
                    }
                }
            }
            
        }
        RemoveConnection(position);
    }

    public void AddWell(PipeGridWell well)
    {
        foreach (var output in Find<PipeGridOutput>(
            well.WorldPos, OutputArea, OutputHeight))
        {
            output.AddWell(well);
            well.AddOutput(output);
        }
        Wells[well.WorldPos] = well;
    }

    public void RemoveWell(Vector3i position)
    {
        Log.Out("Rmove well");
        if (Wells.TryGetValue(position,
            out PipeGridWell well))
        {
            foreach (var output in Find<PipeGridOutput>(
                position, OutputArea, OutputHeight))
            {
                output.RemoveWell(well);
                well.RemoveOutput(output);
            }
            Wells.Remove(position);
        }
    }

    public void AddSource(PipeGridSource source)
    {
        AddConnection(source);
        source.Grid.AddSource(source);
    }

    public void RemoveSource(Vector3i position)
    {
        if (Connections.TryGetValue(position,
            out PipeGridConnection connection))
        {
            if (connection is PipeGridSource source)
                connection.Grid.RemoveSource(source);
        }
        RemoveConnection(position);
    }

    public void AddPump(PipeGridPump pump)
    {
        AddConnection(pump);
        pump.Grid.AddPump(pump);
    }

    public void RemovePump(Vector3i position)
    {
        if (Connections.TryGetValue(position,
            out PipeGridConnection connection))
        {
            if (connection is PipeGridPump pump)
                connection.Grid.RemovePump(pump);
        }
        RemoveConnection(position);
    }

    public void AddSprinkler(PipeGridSprinkler sprinkler)
    {
        AddConnection(sprinkler);
        sprinkler.Grid.AddSprinkler(sprinkler);
    }

    public void RemoveSprinkler(Vector3i position)
    {
        if (Connections.TryGetValue(position,
            out PipeGridConnection connection))
        {
            if (connection is PipeGridSprinkler sprinkler)
                connection.Grid.RemoveSprinkler(sprinkler);
        }
        RemoveConnection(position);
    }

    public void AddConnection(PipeGridConnection connection)
    {
        PipeGridConnection neighbour;

        // Collect neighbours from existing blocks/connections
        for (int side = 0; side < 6; side++)
        {
            // Check if we can connect to side
            if (!connection.CanConnect(side)) continue;
            // Try to fetch the node at the given side
            var offset = FullRotation.Vector[side];
            if (TryGetNode(connection.WorldPos + offset, out neighbour))
            {
                // Check if other one can connect to use
                int mirrored = FullRotation.Mirror(side);
                if (!neighbour.CanConnect(mirrored)) continue;
                // Update the node connectors
                connection[side] = neighbour;
                neighbour[mirrored] = connection;
            }
        }

        int count = 0; int source = -1; int first = -1;
        PipeGridConnection[] neighbours = connection.Neighbours;

        for (int i = 0; i < neighbours.Length; i++)
        {
            if (neighbours[i] == null) continue;
            if (neighbours[i].Grid == null)
            {
                Log.Error("Neighbour without grid found");
            }
            else if (neighbours[i].Grid.HasSource)
            {
                if (source != -1)
                {
                    Log.Error("Too many sources, can't join!!");
                }
                source = i;
            }
            else if (first == -1)
            {
                first = i;
            }
            count++;
        }

        if (source == -1)
        {
            source = first;
        }


        if (count == 0)
        {
            Connections[connection.WorldPos] = connection;
            var grid = connection.Grid = new PipeGrid();
            grid.AddConnection(connection);
            Console.WriteLine("Created a new grid");
        }
        else
        {
            Connections[connection.WorldPos] = connection;
            connection.PropagateGridChange(neighbours[source]);
            Console.WriteLine("Propagate finished");
        }

    }


    // Reuse list 
    static List<PipeGridConnection> _neighbours
        = new List<PipeGridConnection>();

    public void RemoveConnection(Vector3i position)
    {
        if (Connections.TryGetValue(position,
            out PipeGridConnection connection))
        {
            if (connection.Grid != null)
            {
                // First remove from  the existing grid
                // Simply counts and disposes empty grids
                connection.Grid.RemoveConnection(connection);
                // Get valid neighbours as a list
                connection.GetNeighbours(ref _neighbours);
                // Re-check grid if it was cyclic before
                if (_neighbours[0].Grid.IsCyclic)
                {
                    // Reset cyclic flag and recheck
                    _neighbours[0].Grid.IsCyclic = false;
                    // Propagate that change into neighbour tree
                    _neighbours[0].PropagateGridChange(connection);
                }
                // Check if we should split the grid(s) again
                for (int i = 1; i < _neighbours.Count; i++)
                {
                    // Assign new grid to current connection
                    connection.Grid = new PipeGrid();
                    // Propagate that change into neighbour tree
                    _neighbours[i].PropagateGridChange(connection);
                }
                // Clear re-used list
                _neighbours.Clear();
            }
            else
            {
                Log.Warning("Known connection doesn't have grid!?");
            }
            // Remove from connections
            Connections.Remove(position);
        }
        else
        {
            Log.Warning("Removing connection that isn't known!?");
        }
    }

    public static void Cleanup()
    {
        if (instance == null) return;
        instance.CleanupInstance();
    }

    protected override void CleanupInstance()
    {
        // Save out state first
        base.CleanupInstance();
        Connections.Clear();
        Grids.Clear();
        instance = null;
    }


    public bool TryGetNode<T>(Vector3i position, out T node) where T : PipeGridNode
    {
        if (typeof(PipeGridConnection).IsAssignableFrom(typeof(T)))
        {
            if (TryGetNode(position, out PipeGridConnection connection))
            {
                node = connection as T;
                if (node != null)
                    return true;
            }
        }
        else if (typeof(PipeGridWell).IsAssignableFrom(typeof(T)))
        {
            if (TryGetNode(position, out PipeGridWell well))
            {
                node = well as T;
                if (node != null)
                    return true;
            }
        }
        node = null;
        return false;
    }

    public bool TryGetNode(Vector3i position, out PipeGridWell well)
    {
        return Wells.TryGetValue(position, out well);
    }

    public bool TryGetNode(Vector3i position, out PipeGridConnection connection)
    {
        return Connections.TryGetValue(position, out connection);
    }

    public bool AddGrid(PipeGrid grid)
    {
        var idx = Grids.IndexOf(grid);
        if (idx != -1) return false;
        grid.ID = Grids.Count;
        Grids.Add(grid);
        return true;
    }

    public bool RemoveGrid(PipeGrid grid)
    {
        var idx = Grids.IndexOf(grid);
        //Console.WriteLine("Removing at {0}", idx);
        if (idx == -1) return false;
        Grids.RemoveAt(idx);
        while (idx < Grids.Count) 
            Grids[idx++].ID--;
        //foreach (var i in Grids)
        //    Console.WriteLine("Grid {0}", i);
        return true;
    }

    public bool CanConnect(BlockPipeConnection block, Vector3i position, BlockValue bv)
    {
        bool hasOneAround = false;
        PipeGridConnection neighbour;
        List<PipeGridConnection> neighbours = new List<PipeGridConnection>();

        for (int side = 0; side < 6; side++)
        {
            // Check we can connect at side
            var offset = FullRotation.Vector[side];
            // Try to fetch the node at the given side
            if (TryGetNode(position + offset, out neighbour))
            {
                hasOneAround = true;
                // Condition may look weird, but we allow to place blocks
                // next to each other if they don't share any exit. If only
                // one exit aligns with the new block, we don't allow it.
                bool a = block.CanConnect(side, bv.rotation);
                int mirror = FullRotation.Mirror(side);
                bool b = neighbour.CanConnect(mirror);
                if (b == a) neighbours.Add(neighbour);
                else if (b || a) return false;
            }
        }
        // Somehow it all has to start with one
        if (hasOneAround == false) return true;
        // Log.Out("Has Neighbours {0}", neighbours.Count);
        // OK if no grid to connect yet (first block)
        if (neighbours.Count == 0) return false;
        // Check length requirement for single grid
        if (neighbours.Count == 1)
        {
            int count = neighbours[0].CountLongestDistance();
            // Console.WriteLine("Count {0}", count);
            return count < MaxDistance;
        }
        // OK if no or only one grid has source
        int sources = 0;
        for (int i = 0; i < neighbours.Count; i++)
        {
            if (neighbours[i].Grid == null) continue;
            if (neighbours[i].Grid.HasSource) ++sources;
        }
        if (sources > 1) return false;
        // Get longest and second longest
        if (neighbours.Count > 1)
        {
            int longest = 0; int second = 0;
            for (int i = 0; i < neighbours.Count; i++)
            {
                int count = neighbours[i].CountLongestDistance();
                if (count >= longest)
                {
                    second = longest;
                    longest = count;
                }
            }
            // Console.WriteLine("Count {0} {1}", longest, second);
            return longest + second < MaxDistance;
        }
        // Nothing to complain
        return true;
    }

    public override void Write(BinaryWriter bw)
    {
        bw.Write(FileVersion);
        bw.Write(Connections.Count);
        Console.WriteLine("Writing the whole thing out");
        foreach (var kv in Connections)
        {
            // Make sure to write out the header first
            bw.Write((byte)kv.Value.GetStorageType());
            // Write can be overridden
            kv.Value.Write(bw);
        }
        bw.Write(Wells.Count);
        foreach (var kv in Wells)
        {
            // Write can be overridden
            kv.Value.Write(bw);
        }
    }

    public override void Read(BinaryReader br)
    {
        Connections.Clear();
        var version = br.ReadByte();
        int nodes = br.ReadInt32();
        for (int index = 0; index < nodes; ++index)
        {
            switch (br.ReadByte())
            {
                case 0: AddConnection(new PipeGridConnection(br)); break;
                case 1: AddOutput(new PipeGridOutput(br)); break;
                case 2: AddPump(new PipeGridPump(br)); break;
                case 3: AddSource(new PipeGridSource(br)); break;
                case 4: AddSprinkler(new PipeGridSprinkler(br)); break;
                case 5: AddWell(new PipeGridWell(br)); break;
            }
        }
        int wells = br.ReadInt32();
        for (int index = 0; index < wells; ++index)
        {
            AddWell(new PipeGridWell(br));
        }
    }

    // public PipeGrid GetGridsAround(Vector3i position)
    // {
    //     return;
    // }

}
