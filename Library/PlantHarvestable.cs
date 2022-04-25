using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PlantHarvestable : WorldNode, IGrowParameters
{

    public int LightValue;
    public int FertilityLevel;
    public float HarvestProgress;

    // public bool IsLoaded { get; set; }

    public bool GetIsLoaded(WorldBase world)
    {
        return BlockHelper.IsLoaded(world, WorldPos);
    }

    public int GetLightLevel(WorldBase world)
    {
        if (GetIsLoaded(world)) LightValue = world.
            GetBlockLightValue(0, WorldPos);
        return LightValue;
    }

    public int GetFertilityLevel(WorldBase world)
    {
        if (GetIsLoaded(world)) LightValue = world.
            GetBlockLightValue(0, WorldPos);
        return LightValue;
    }

    public PlantHarvestable(Vector3i position, BlockValue block,
        int lightValue = 0, int fertilityLevel = 0, float harvestProgress = 0f)
        : base(position, block)
    {
        LightValue = lightValue;
        FertilityLevel = fertilityLevel;
        HarvestProgress = harvestProgress;
    }

    public override void Write(BinaryWriter bw)
    {
        base.Write(bw);
        bw.Write((byte)LightValue);
        bw.Write((byte)FertilityLevel);
        bw.Write(HarvestProgress);
    }

    public PlantHarvestable(BinaryReader br)
        : base(br)
    {
        LightValue = br.ReadByte();
        FertilityLevel = br.ReadByte();
        HarvestProgress = br.ReadSingle();
    }

}
