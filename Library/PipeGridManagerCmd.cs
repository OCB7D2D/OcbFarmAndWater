using System.Collections.Generic;
using UnityEngine;

public class WaterManagerCmd : ConsoleCmdAbstract
{

    private static string info = "WaterManager";
    public override string[] GetCommands()
    {
        return new string[2] { info, "wm" };
    }

    public override bool IsExecuteOnClient => true;
    public override bool AllowedInMainMenu => true;

    public override string GetDescription() => "Water Manager Settings";

    public override string GetHelp() => "Fine tune Water Manager Settings\n";

    public static void TestRot(Vector3i org, int rotation, string str)
    {
        Vector3 must = BlockShapeNew.GetRotationStatic(rotation) * org;
        Vector3i rot = FullRotation.Rotate(rotation, org);
        if (must != rot)
        {
            Log.Out("{0} {1}: from {2} to {3} (wrong {4})", str, rotation, org, must, rot);
        }
    }


    public static void TestRot(int dir, int rotation)
    {
        Vector3i org = FullRotation.Vector[dir];
        Vector3 must = BlockShapeNew.GetRotationStatic(rotation) * org;
        // Vector3i rot = Rotations.RotateVector(dir, rotation);

        var opp = FullRotation.GetSide(dir, rotation);
        Vector3i rot = FullRotation.Vector[opp];

        // Log.Out("case {0}: return Vector3i.{1};", i, VecToName(must));

        if (must != rot)
        {
            Log.Out("{0} {1}: from {2} to {3} (wrong {4})", dir,
                rotation, org, FullRotation.VectorToString(must), FullRotation.VectorToString(rot));
        }
    }

    public static void Test01()
    {
        for (var side = 0; side < 6; side++)
        {
            for (var rot = 0; rot < 24; rot++)
            {
                TestRot(side, rot);
            }
        }

        for (var rot = 0; rot < 24; rot++)
        {
            TestRot(Vector3i.up, rot, "up");
            TestRot(Vector3i.right, rot, "right");
            TestRot(Vector3i.left, rot, "left");
            TestRot(Vector3i.down, rot, "down");
            TestRot(Vector3i.forward, rot, "fwd");
            TestRot(Vector3i.back, rot, "back");
        }
    }

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
    {

        Log.Out("Water Manager at {0}:", GameTimer.Instance.ticks);
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
