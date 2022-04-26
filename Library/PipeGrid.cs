using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PipeGrid
{

    public int ID = -1;

    public PipeGridSource Source = null;

    // public List<PipeGridWell> Wells = new List<PipeGridWell>();
    public List<PipeGridPump> Pumps = new List<PipeGridPump>();
    public List<PipeGridOutput> Outputs = new List<PipeGridOutput>();
    public List<PipeGridSprinkler> Sprinklers = new List<PipeGridSprinkler>();

    public bool HasSource => Source != null;
    public Vector3i FirstPosition = Vector3i.invalid;

    int Count = 0;

    public bool IsCyclic = false;

    public void AddConnection(PipeGridConnection connection)
    {
        Count++;
        if (Count == 1)
        {
            PipeGridManager.Instance.AddGrid(this);
            FirstPosition = connection.WorldPos;
        }
    }

    public void RemoveConnection(PipeGridConnection connection)
    {
        Count--;
        if (Count == 0)
        {
            //Console.WriteLine("Removing grid now");
            PipeGridManager.Instance.RemoveGrid(this);
            FirstPosition = Vector3i.invalid;
        }
        // Connections.Remove(connection);
    }

    public bool AddOutput(PipeGridOutput output)
    {
        int idx = Outputs.IndexOf(output);
        if (idx != -1) return false;
        Outputs.Add(output);
        return true;
    }

    public bool RemoveOutput(PipeGridOutput output)
    {
        int idx = Outputs.IndexOf(output);
        if (idx == -1) return false;
        Outputs.RemoveAt(idx);
        return true;
    }

    public bool AddSource(PipeGridSource source)
    {
        if (Source != null) return false;
        Source = source;
        return true;
    }

    public bool RemoveSource(PipeGridSource source)
    {
        if (Source == null) return false;
        Source = null;
        return true;
    }

    public bool AddPump(PipeGridPump pump)
    {
        int idx = Pumps.IndexOf(pump);
        if (idx != -1) return false;
        Pumps.Add(pump);
        return true;
    }

    public bool RemovePump(PipeGridPump pump)
    {
        int idx = Pumps.IndexOf(pump);
        if (idx == -1) return false;
        Pumps.RemoveAt(idx);
        return true;
    }

    public bool AddSprinkler(PipeGridSprinkler sprinkler)
    {
        int idx = Sprinklers.IndexOf(sprinkler);
        if (idx != -1) return false;
        Sprinklers.Add(sprinkler);
        return true;
    }

    public bool RemoveSprinkler(PipeGridSprinkler sprinkler)
    {
        int idx = Sprinklers.IndexOf(sprinkler);
        if (idx == -1) return false;
        Sprinklers.RemoveAt(idx);
        return true;
    }

    //

    public override string ToString()
    {
        return string.Format(
            "PipeGrid {0} has {1} pipes and {2} pumps\nSource: {3} at {4})\nCyclic: {5}",
            ID, Count, Pumps.Count, Source, FirstPosition, IsCyclic);
    }

    // This may be done more efficient, but it
    // is an edge-case and will be optimized last
    public void ResetGridState()
    {
        foreach (var con in PipeGridManager.Instance.Connections)
        {
            // Only work on connection from our grid
            if (con.Value.Grid != this) continue;
        }
    }

}
