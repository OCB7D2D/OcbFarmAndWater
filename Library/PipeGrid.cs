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
    public Vector3i FirstPosition = Vector3i.min;

    int Count = 0;

    public PipeGrid()
    {
    }

    public void TickUpdate()
    {
        if (Source != null)
            Source.TickUpdate();
        foreach (var pump in Pumps)
            pump.TickUpdate();
        //foreach (var output in Wells)
        //    output.TickUpdate();
        foreach (var output in Outputs)
            output.TickUpdate();
        foreach (var sprinkler in Sprinklers)
            sprinkler.TickUpdate();
    }

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
            Console.WriteLine("Removing grid now");
            PipeGridManager.Instance.RemoveGrid(this);
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
            "PipeGrid {0} has {1} pipes and {2} pumps\nSource: {3} at {4})",
            ID, Count, Pumps.Count, Source, FirstPosition);
    }

}
