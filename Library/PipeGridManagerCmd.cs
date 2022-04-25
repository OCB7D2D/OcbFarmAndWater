using System.Collections.Generic;

public class WaterManagerCmd : ConsoleCmdAbstract
{

    private static string info = "WaterManager";
    public override string[] GetCommands()
    {
        return new string[2] { info, "wm" };
    }

    public override bool IsExecuteOnClient => false;
    public override bool AllowedInMainMenu => false;

    public override string GetDescription() => "Water Manager Settings";

    public override string GetHelp() => "Fine tune Water Manager Settings\n";

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
    {

        Log.Out("Water Manager:");
        // foreach (var i in PipeGridManager.Instance.Connections)
        // {
        //     Log.Out("  Connection {0}", i);
        // }
        foreach (var i in PipeGridManager.Instance.Grids)
        {
            Log.Out("  Grid {0}", i);
        }
        foreach (var i in PipeGridManager.Instance.Wells)
        {
            Log.Out("  Well {0}", i);
        }

        if (_params.Count == 1)
        {
            if (_params[0] == "reset")
            {
                PlantManager.Instance.Harvestable.Clear();
                PlantManager.Instance.Growing.Clear();
                PlantManager.Instance.Harvester.Clear();
            }
        }

    }

}
