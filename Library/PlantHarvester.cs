using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PlantHarvester : WorldNode, ITickable
{

    private readonly float BoundHelperSize = 2.59f;

    // How much harvest progress per tick
    public float HarvestSpeed = 0.5f;

    // How many random lookups per tick
    public int LookupsPerTick = 5000;

    // Size of area to auto harvest
    public int HarvestSizeXZ = 5;
    // Block must be on same level
    public int HarvestSizeY = 2;

    // The acquired block to be repaired
    public PlantHarvestable HarvestBlock = null;

    // The position of the block being repaired
    // To check if block is still the same in the world
    public Vector3i HarvestPosition = Vector3i.zero;

    // How much harvesting has progressed
    public float HarvestProgress = 0f;

    public bool IsLoaded = false;

    // Drop crops to the ground if container is full?
    public bool DropSurplusCrops = true;

    // List of harvested crops pending to be put into loot container
    public Dictionary<int, int> Pending = new Dictionary<int, int>();

    static readonly FieldInfo FieldLookedTileEntities = AccessTools
        .Field(typeof(GameManager), "lockedTileEntities");

    public ScheduledTick Scheduled = null;

    public bool GetIsLoaded()
    {
        return BlockHelper.IsLoaded(WorldPos);
    }

    public void RegisterScheduled(ulong ticks = 30u)
    {
        if (Scheduled != null) PlantManager.DeleteScheduledTick(Scheduled);
        Scheduled = PlantManager.AddScheduleTick(GetIsLoaded() ? ticks : 90u, this);
    }

    public void OnLoaded(WorldBase world)
    {

        if (Pending.Count > 0)
        {
            Log.Warning("++++++++++++++++++++++ Try to distribute pendings to chest");
            var container = GetLootContainer(world);
            foreach (var kv in Pending)
            {
                Log.Warning(" Transfer {0} x {1}", kv.Key, kv.Value);
                TransferItems(kv.Key, kv.Value, container);
            }
            Pending.Clear();
        }
        IsLoaded = true;
    }

    public void OnUnloaded()
    {
        IsLoaded = false;
    }

    public bool GetLootContainer(WorldBase world, Vector3i position, out TileEntityLootContainer te)
    {
        if (world.GetTileEntity(0, position) is TileEntityLootContainer tileEntity)
        {
            if (GameManager.IsDedicatedServer)
            {
                Dictionary<TileEntity, int> lockedTileEntities = FieldLookedTileEntities
                    .GetValue(GameManager.Instance) as Dictionary<TileEntity, int>;
                if (lockedTileEntities == null || !lockedTileEntities.ContainsKey(tileEntity))
                {
                    te = tileEntity;
                    return true;
                }
            }
            else
            {
                te = tileEntity;
                return true;
            }
        }
        te = null;
        return false;
    }

    public TileEntityLootContainer GetLootContainer(WorldBase world)
    {
        TileEntityLootContainer te;
        world.GetBlock(WorldPos + Vector3i.up);
        if (GetLootContainer(world, WorldPos + Vector3i.up, out te)) return te;
        if (GetLootContainer(world, WorldPos + Vector3i.down, out te)) return te;
        if (GetLootContainer(world, WorldPos + Vector3i.back, out te)) return te;
        if (GetLootContainer(world, WorldPos + Vector3i.left, out te)) return te;
        if (GetLootContainer(world, WorldPos + Vector3i.forward, out te)) return te;
        if (GetLootContainer(world, WorldPos + Vector3i.right, out te)) return te;
        return null;
    }

    public Vector3i GetRandomPos(Vector3 pos, int dxz, int dy)
    {
        int x = 0; int y = 0; int z = 0;
        // We don't fix ourself!
        while (x == 0 && y == 0 && z == 0 && (dxz != 0 || dy != 0))
        {
            x = Random.Range(-dxz, dxz + 1);
            y = Random.Range(-dy, dy + 1);
            z = Random.Range(-dxz, dxz + 1);
        }
        return new Vector3i(
            pos.x + x,
            pos.y + y,
            pos.z + z
        );

    }

    public PlantHarvester(Vector3i worldPos, BlockValue block)
        : base(worldPos, block) {}

    public bool CanHarvestBlock(Block block)
    {
        return block.Properties.GetBool("AutoHarvestable");
    }

    public void PlayBlockSound(string audio, int distance = 100,
        AudioRolloffMode mode = AudioRolloffMode.Logarithmic)
    {
        if (GetIsLoaded() == false) return;
        if (GameManager.IsDedicatedServer) return;
        GameManager.Instance.PlaySoundAtPositionServer(
            WorldPos, audio, mode, distance);
    }

    public bool SearchBlockToHarvest(WorldBase world)
    {
        // Simple and crude random block acquiring
        // Repair block has slightly further reach
        for (int i = 1; i <= LookupsPerTick; i += 1)
        {
            // Indicate we are still working
            // PlayBlockSound("timer_stop");
            // Get random block and see if it can be harvested
            Vector3i randomPos = GetRandomPos(WorldPos,
                HarvestSizeXZ, HarvestSizeY);
            // BlockValue blockValue = world.GetBlock(randomPos);
            if (PlantManager.Instance.Harvestable.TryGetValue(
                randomPos, out PlantHarvestable type))
            {
                if (type.BlockID == BlockValue.Air.type) continue;
                if (CanHarvestBlock(type.GetBlock()))
                {
//                    Log.Out("Can Harvest");
                    HarvestPosition = randomPos;
                    HarvestBlock = type;
                    HarvestProgress = 0.0f;
                    return true;
                }
//                Log.Out("Found harvestable via manager {0}", type);
            }
            // Only harvest the master block (for multidim)
            //if (CanHarvestBlock(blockValue.Block))
            //{
            //    // Acquire the block to repair
            //}
        }
        return false;
    }

    public void ResetAcquiredBlock(string playSound = "", bool broadcast = true)
    {
        if (HarvestBlock != null)
        {
            // Play optional sound (only at the server to broadcast everywhere)
            if (!string.IsNullOrEmpty(playSound))
            {
                PlayBlockSound(playSound);
            }
            // Reset acquired harvest block
            HarvestBlock = null;
            HarvestPosition = Vector3i.zero;
            HarvestProgress = 0.0f;
            UpdateBoundHelper();
            if (broadcast)
            {
                // SetModified();
            }
        }
    }

    public static bool TransferItems(int type, int count, TileEntityLootContainer container)
    {
        if (container == null)
        {
            // Drop Item
            Log.Out("No container, drop item?");
            return false;
        }

        bool modified = false;
        ItemClass item = ItemClass.GetForId(type);
        if (count > 0)
        {
            // Try to fill existing slots as far as possible
            for (int i = 0; i < container.items.Length; ++i)
            {
                if (container.items[i].itemValue.type != type) continue;
                var space = Utils.FastMin(item.Stacknumber.Value
                    - container.items[i].count, count);
                if (space <= 0) continue;
                Log.Out("Has existing slot that has space left {0}", space);
                container.items[i].count += space;
                count -= space;
                modified = true;
            }
        }
        if (count > 0)
        {
            // Try to add additional slots if needed
            for (int i = 0; i < container.items.Length; ++i)
            {
                if (!container.items[i].IsEmpty()) continue;
                container.items[i].itemValue.type = type;
                var space = Utils.FastMin(item.Stacknumber.Value
                    - container.items[i].count, count);
                if (space <= 0) continue;
                Log.Out("Has new slot that has space left {0}", space);
                container.items[i].count = space;
                count -= space;
                modified = true;
            }
        }
        // Update stuff if modified
        if (modified)
        {
            // Re-implement `tileEntityChanged` since it is private
            for (int index = 0; index < container.listeners.Count; ++index)
                container.listeners[index].OnTileEntityChanged(container, 0);
            container.SetModified();
        }
        return modified;
    }

    public void HarvestCrop(WorldBase world)
    {

        if (HarvestBlock == null || HarvestBlock.GetBlock() == null)
        {
            Log.Warning("HarvestBlockType not existing? {0}", HarvestBlock.BlockID);
            return;
        }

        string replacekName = HarvestBlock.GetBlock().Properties.GetString("HarvestReplaceBlock");
        if (string.IsNullOrEmpty(replacekName)) Log.Error("Missing HarvestReplaceBlock property");

        if (HarvestBlock.GetBlock().itemsToDrop.TryGetValue(EnumDropEvent.Harvest,
            out List<Block.SItemDropProb> sitemDropProbList))
        {
            if (sitemDropProbList == null)
            {
                Log.Warning("sitemDropProbList null");
                return;
            }
            if (sitemDropProbList.Count > 0)
            {
                ItemValue item = ItemClass.GetItem(sitemDropProbList[0].name);

                if (item == null)
                {
                    Log.Warning("item null {0}", sitemDropProbList[0].name);
                    return;
                }
                int count = 1; // Utils.FastMax(1, sitemDropProbList[0].minCount);
                if (GetIsLoaded())
                {
                    TileEntityLootContainer container = GetLootContainer(world);
                    if (TransferItems(item.type, count, container) == false)
                    {
                        if (DropSurplusCrops)
                        {
                            var cntPos = container == null ?
                                WorldPos : container.ToWorldPos();
                            world.GetGameManager().ItemDropServer(
                                new ItemStack(item, count),
                                cntPos + Vector3i.up,
                                Vector3i.zero);
                        }
                        else
                        {
                            world.GetGameManager().PlaySoundAtPositionServer(WorldPos,
                                "ui_denied", AudioRolloffMode.Logarithmic, 100);
                            return; // Abort harvest
                        }
                    }
                }
                else if (Pending.TryGetValue(item.type, out int pending))
                {
                    Log.Out("Now pending {0} {1}", sitemDropProbList[0].name, pending + 1);
                    Pending[item.type] = pending + 1;
                }
                else
                {
                    Log.Out("Now pending {0} {1}", sitemDropProbList[0].name, 1);
                    Pending[item.type] = 1;
                }
            }
        }
        // Change the block type
        //HarvestBlock.set(
        //	Block.GetBlockByName(replacekName).blockID,
        //	HarvestBlock.meta,
        //	(byte)HarvestBlock.damage,
        //	HarvestBlock.rotation);

        // Replace plant after it has been auto harvested
        BlockValue next = Block.GetBlockValue(replacekName);
        // TODO :next.rotation = HarvestBlock.rotation;

        // Assume plant is loaded when harvester is loaded
        PlantManager.Instance.RemoveHarvestable(HarvestPosition);

        // HarvestBlock.LightValue

        PlantManager.Instance.UpdateBlockValue(world, HarvestPosition, next, HarvestBlock);

        // BroadCast the changes done to the block
//        world.SetBlockRPC(chunkFromWorldPos.ClrIdx, HarvestPosition,
//            HarvestBlock, HarvestBlock.Block.Density);
//        // Play harvesting sound at the plant position
//        world.GetGameManager().PlaySoundAtPositionServer(
//            HarvestPosition.ToVector3(), // or at `worldPos`?
//            "item_plant_pickup", AudioRolloffMode.Logarithmic, 100);
//        Log.Out("Harvested it {0}", IsLoaded);
        // Update clients
        // SetModified();

        // Reset acquired block
        ResetAcquiredBlock();
    }

    public bool TickHarvestProgress(WorldBase world)
    {
        var current = PlantManager.GetHarvestable(HarvestPosition);
        // Check if any of the stats changed after we acquired to block
        if (current != HarvestBlock.BlockID)
        {
            // Reset the acquired block and play a sound bit
            // Play different sound according to reason of disconnect
            // Block has been switched (maybe destroyed, upgraded, etc.)
            // Block has been damaged again, abort repair on progress
            ResetAcquiredBlock(current != HarvestBlock.BlockID ?
                "weapon_jam" : "ItemNeedsRepair");
            return false;
        }

        // Increase amount of harvesting done
        HarvestProgress += Time.deltaTime * HarvestSpeed;
        Log.Out("Harvest Progress {0} {1}", Time.deltaTime, HarvestProgress);

        // Check if repaired enough to fully restore
        if (HarvestProgress >= 1f)
        {
            // Safety check if materials have changed
            if (!CanHarvestBlock(HarvestBlock.GetBlock()))
            {
//                Log.Out("Can't harvest {0}", Block.list[HarvestBlockType].GetBlockName());
                // Inventory seems to have changed (not repair possible)
                ResetAcquiredBlock("weapon_jam");
            }
            else
            {
                // Harvest the crop now
                HarvestCrop(world);
                return true;
            }
        }
        else
        {
            // Log.Out("Play repair sound");
            // Play simple click indicating we are working on something
            // world.GetGameManager().PlaySoundAtPositionServer(worldPos,
            //     "repair_block", AudioRolloffMode.Logarithmic, 100);
            // // Update clients
            // SetModified();
        }
        return false;
    }

    public override void Tick(WorldBase world, ulong delta)
    {
        ulong tick = 30u;
        Scheduled = null;
        // Check if we already acquired a block to harvest
        // If not we search for one that can be harvested
        // Otherwise we tick until progress has completed
        if (HarvestBlock == null)
        {
            if (SearchBlockToHarvest(world))
            {
                Log.Out("Found a plant to harvest");
                TickHarvestProgress(world);
                UpdateBoundHelper();
        }
        }
        else
        {
            if (TickHarvestProgress(world)) tick = 15u;
            UpdateBoundHelper();
        }
        // Log.Out("Ticked Harvester (loaded: {0})", GetIsLoaded());
        RegisterScheduled(tick);
    }

    public override void Write(BinaryWriter bw)
    {
        base.Write(bw);
    }

    public PlantHarvester(BinaryReader br)
        : base (br)
    {
    }

    private Color GetProgressColor(Color target, float progress)
    {
        Color delta = target - Color.gray;
        return Color.gray + delta * progress;
    }

    public void UpdateBoundHelper()
    {
        Color color = HarvestPosition == Vector3i.zero ?
            Color.gray : GetProgressColor(Color.green, HarvestProgress);
        Vector3 position = HarvestPosition == Vector3i.zero ?
            WorldPos : HarvestPosition;
        BoundHelperManager.Instance.AdjustHelper(WorldPos,
            position + new Vector3(0.5f, 0.5f, 0.5f),
            new Vector3(BoundHelperSize, BoundHelperSize, BoundHelperSize),
            color * 0.5f);
        // if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient) return;
        // SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(
        //     NetPackageManager.GetPackage<NetPackageBoundHelperMove>()
        //         .Setup(WorldPos, HarvestPosition, HarvestProgress));
    }

}
