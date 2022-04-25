using System.IO;

public class PipeGridPowered : PipeGridConnection
{

    public PipeGridPowered(Vector3i position, BlockValue block)
        : base(position, block) { }

    public PipeGridPowered(BinaryReader br)
     : base(br) {}

    public PowerItem GetPowerItem(WorldBase world)
    {
        if (world == null) return null;
        return PowerManager.Instance
            .GetPowerItemByWorldPos(WorldPos);
    }

    public bool IsPowered
    {
        get
        {
            if (GameManager.Instance.World is WorldBase world)
            {
                if (GetPowerItem(world) is PowerItem power)
                {
                    return power.IsPowered;
                }
            }
            return false;
        }
    }

}
