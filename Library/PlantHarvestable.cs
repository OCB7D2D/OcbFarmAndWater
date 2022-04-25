using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PlantHarvestable : IGrowParameters
{
    public int BlockId;
    public Vector3i WorldPos;

    public int LightValue;
    public int FertilityLevel;
    public float HarvestProgress;

    // public bool IsLoaded { get; set; }

    public bool GetIsLoaded(WorldBase world)
    {
        return PlantManager.GetIsLoaded(WorldPos);
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

    public PlantHarvestable(Vector3i worldPos, int blockId,
        int lightValue = 0, int fertilityLevel = 0, float harvestProgress = 0f)
    {
        WorldPos = worldPos;
        BlockId = blockId;
        LightValue = lightValue;
        FertilityLevel = fertilityLevel;
        HarvestProgress = harvestProgress;
    }


    public void Write(BinaryWriter bw)
    {
        bw.Write(WorldPos.x);
        bw.Write(WorldPos.y);
        bw.Write(WorldPos.z);
        bw.Write(BlockId);
        bw.Write((byte)LightValue);
        bw.Write((byte)FertilityLevel);
        bw.Write(HarvestProgress);
    }

    public static PlantHarvestable Read(BinaryReader br)
    {
        return new PlantHarvestable(
            new Vector3i(
                br.ReadInt32(), // WorldPos.x
                br.ReadInt32(), // WorldPos.y
                br.ReadInt32()), // WorldPos.z
            br.ReadInt32(), // BlockId
            br.ReadByte(), // LightValue
            br.ReadByte(), // FertilityLevel
            br.ReadSingle()); // HarvestProgress
    }

}
