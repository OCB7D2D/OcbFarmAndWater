using System.Collections.Generic;

public class AutoHarvestCmd : ConsoleCmdAbstract
{

    private static string info = "AutoHarvest";
    public override string[] GetCommands()
    {
        return new string[2] { info, "ah" };
    }

    public override bool IsExecuteOnClient => false;
    public override bool AllowedInMainMenu => false;

    public override string GetDescription() => "Auto Harvest Settings";

    public override string GetHelp() => "Fine tune Auto Harvest Settings\n";

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
    {

        Log.Out("Report Growing:");
        foreach (var i in PlantManager.Instance.Growing)
        {
            Log.Out("  Growing {0}", i);
        }
        Log.Out("Report Harvest:");
        foreach (var i in PlantManager.Instance.Harvestable)
        {
            Log.Out("  Harvestable {0} => {1}", i.Key, Block.GetBlockValue(i.Value.BlockID).Block.GetBlockName());
        }
        Log.Out("Report Harvester:");
        foreach (var i in PlantManager.Instance.Harvester)
        {
            Log.Out("  Harvester {0}", i);
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
