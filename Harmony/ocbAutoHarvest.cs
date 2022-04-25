using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class OcbAutoHarvest : IModApi
{

	// Entry class for A20 patching
	public void InitMod(Mod mod)
	{
		Log.Out("Loading OCB Auto Plant Harvest Patch: " + GetType().ToString());
		var harmony = new Harmony(GetType().ToString());
		harmony.PatchAll(Assembly.GetExecutingAssembly());
		ModEvents.PlayerLogin.RegisterHandler(GameAwakeDone);
	}

	
	//[HarmonyPatch(typeof(TileEntity))]
	//[HarmonyPatch("Instantiate")]
	//public class TileEntity_Instantiate
	//{
	//	public static bool Prefix(TileEntityType type, Chunk _chunk, ref TileEntity __result)
	//	{
	//		if (type == BlockAutoHarvest.TileEntityID) {
	//			__result = new TileEntityAutoHarvest(_chunk);
	//			return false;
	//		}
	//		return true;
	//	}
	//}

	[HarmonyPatch(typeof(ItemActionExchangeItem))]
	[HarmonyPatch("OnHoldingUpdate")]
	public class ItemActionExchangeItem_OnHoldingUpdate
	{
		public static bool Prefix(
			ItemActionExchangeItem __instance,
			ItemActionData _actionData,
			ref string ___changeItemToItem,
			ref string ___changeBlockTo,
			ref string ___doBlockAction,
			ref BlockValue ___hitLiquidBlock,
			ref Vector3i ___hitLiquidPos)
		{
			if (_actionData.lastUseTime == 0.0 || __instance.IsActionRunning(_actionData))
				return false;
			Vector3i blockPos = _actionData.invData.hitInfo.hit.blockPos;
			Log.Out("DO action {0} {1} {2}", ___doBlockAction, ___hitLiquidBlock, ___hitLiquidBlock.Block);
			if (___hitLiquidBlock.Block is BlockPipeWell well)
            {

				QuestEventManager.Current.ExchangedFromItem(_actionData.invData.itemStack);
				ItemValue oldItem = _actionData.invData.holdingEntity.inventory.holdingItemItemValue;
				ItemValue newItem = ItemClass.GetItem(___changeItemToItem);
				var wellMasterPos = !___hitLiquidBlock.ischild ? ___hitLiquidPos :
					___hitLiquidPos + ___hitLiquidBlock.parent;

				int factor = 1;
				bool filling = false;

				switch (___doBlockAction)
				{
					case "deplete3":
						factor = 50;
						break;
					case "fill1":
						filling = true;
						break;
					case "fill3":
						filling = true;
						factor = 50;
						break;
				}

				int holding = _actionData.invData.holdingEntity.inventory.holdingCount;

				int exchanged = filling
					? well.FillWater(wellMasterPos, holding, factor)
					: well.ConsumeWater(wellMasterPos, holding, factor);

				Log.Out("Requested {0} (*{1}) and exchanged {2}", holding, factor, exchanged);

				// exchanged /= factor;

				Log.Out("Adjusted for factor {0}", exchanged);

				if (exchanged == holding)
                {
					// Replace to complete stack we are currently holding
					_actionData.invData.holdingEntity.inventory.SetItem(
						_actionData.invData.slotIdx, new ItemStack(newItem, holding));
				}
				else
                {
					// Reduce stack we are currently holding partially
					_actionData.invData.holdingEntity.inventory.SetItem(
						_actionData.invData.slotIdx, new ItemStack(oldItem, holding - exchanged));
					// Try to distribute wherever we have space
					var stack = new ItemStack(newItem, exchanged);

					if (stack.count > 0) _actionData.invData.holdingEntity.bag.TryStackItem(0, stack);
					if (stack.count > 0) _actionData.invData.holdingEntity.inventory.TryStackItem(0, stack);
					if (stack.count > 0 && _actionData.invData.holdingEntity.bag.AddItem(stack)) stack.count = 0;
					if (stack.count > 0 && _actionData.invData.holdingEntity.inventory.AddItem(stack)) stack.count = 0;
					if (stack.count > 0) _actionData.invData.world.GetGameManager()
						.ItemDropServer(stack, ___hitLiquidPos + Vector3i.up, Vector3i.zero);
				}
				_actionData.lastUseTime = 0.0f;
				Log.Out("Exchange water from block");
				return false;
			}
			else
            {
				// QuestEventManager.Current.ExchangedFromItem(_actionData.invData.itemStack);
				// ItemValue _itemValue = ItemClass.GetItem(___changeItemToItem);
				// _actionData.invData.holdingEntity.inventory.SetItem(_actionData.invData.slotIdx, new ItemStack(_itemValue, _actionData.invData.holdingEntity.inventory.holdingCount));
				// if (___doBlockAction != null && GameManager.Instance.World.IsWater(___hitLiquidPos))
				// 	___hitLiquidBlock.Block.DoExchangeAction((WorldBase)_actionData.invData.world, 0, ___hitLiquidPos, ___hitLiquidBlock, ___doBlockAction, _actionData.invData.holdingEntity.inventory.holdingCount);
				// if (___changeBlockTo == null)
				// 	return false;
				// BlockValue blockValue = ItemClass.GetItem(___changeBlockTo).ToBlockValue();
				// _actionData.invData.world.SetBlockRPC(blockPos, blockValue);
			}
			return true;
		}

		private static void StackItems(Bag bag, ItemStack stack)
		{
			bag.AddItem(stack);
		}

		private static void StackItems(Inventory inv, ref ItemStack stack)
		{
			inv.AddItem(stack);
		}

	}

	public bool GameAwakeDone(ClientInfo client, string a, StringBuilder profile)
	{
		Log.Warning("Patching blocks");
		foreach (Block block in Block.list)
		{
			if (block is BlockPlant)
			{
				// Patch display info for plants (additional data)
				block.DisplayInfo = Block.EnumDisplayInfo.Custom;
			}
		}
		return true;
	}

	// Hook when `PowerManger` is loaded
	[HarmonyPatch(typeof(PowerManager))]
	[HarmonyPatch("LoadPowerManager")]
	public class PowerManager_LoadPowerManager
	{
		public static void Prefix()
		{
			PlantManager.Instance.LoadPersistedData();
			PipeGridManager.Instance.LoadPersistedData();
			var instance = BoundHelperManager.Instance;
		}
	}

	// Hook when `PowerManger` is loaded
	[HarmonyPatch(typeof(VehicleManager))]
	[HarmonyPatch("Cleanup")]
	public class VehicleManager_Cleanup
	{
		public static void Prefix()
		{
			PlantManager.Cleanup();
			PipeGridManager.Cleanup();
		}
	}

	// Hook into the middle of `gmUpdate`
	[HarmonyPatch(typeof(ThreadManager))]
	[HarmonyPatch("UpdateMainThreadTasks")]
	public class UpdateMainThreadTasks
	{
		public static void Prefix()
		{
			PlantManager.TickManager();
		}
	}

	// Hook into the middle of `gmUpdate`
	[HarmonyPatch(typeof(Chunk))]
	[HarmonyPatch("recalcIndexedBlocks")]
	public class ChunkBlockLayer_GetAt
	{
		private static IEnumerator UpdateBlockLater(List<Tuple<Vector3i, int>> changes)
		{
			yield return new WaitForSeconds(0f);
			if (GameManager.Instance.World != null)
            {
				var world = GameManager.Instance.World;
				Log.Out("UpdateBlockValue 3 (delayed calls {0})", changes.Count);
				foreach (var change in changes)
                {
					world.SetBlockRPC(change.Item1, Block.GetBlockValue(change.Item2));
				}
			}
		}

		public static void Prefix(
			Chunk __instance,
			ChunkBlockLayer[] ___m_BlockLayers)
		{
			//Log.Out("==== Recalc {0}", ___m_BlockLayers.Length);
			//Log.Out("  in {0} {1} {2}",
			//	__instance.X,
			//	__instance.Y,
			//	__instance.Z);
			//Log.Out("  vs {0}", __instance.GetWorldPos());

			List<Tuple<Vector3i, int>> changes = new List<Tuple<Vector3i, int>>();

			for (int index1 = 0; index1 < 8; ++index1)
			{
				for (int index2 = 0; index2 < 8; ++index2)
				{
					// off = _x + (_z << 4) + (_y & 3) * 16 * 16 (max 1024)
					ChunkBlockLayer layer = ___m_BlockLayers[index1 + index2 * 8];
					if (layer == null) continue;
					for (int x = 0; x < 16; ++x)
					{
						for (int z = 0; z < 16; ++z)
						{
							for (int y = 0; y < 4; y++)
							{
								BlockValue asd = layer.GetAt(x, y, z);
								var _y = y + index1 * 4 + index2 * 32;
								var _x = x + __instance.X * 16;
								var _z = z + __instance.Z * 16;
								var worldPos = new Vector3i(_x, _y, _z);
								if (PlantManager.Instance.PendingChange.TryGetValue(worldPos, out int value))
                                {
									// asd.Block.OnBlockRemoved(null, __instance, worldPos, asd);
									changes.Add(new Tuple<Vector3i, int>(worldPos, value));
									PlantManager.Instance.PendingChange.Remove(worldPos);
									// Log.Out("   Has Pending Change {0} {1} {2}", _x, _y, _z);
									// asd.type = value;
									// layer.SetAt(x, y, z, asd.rawData);
								}
							}
							// 
						}
					}
				}
			}

			if (changes.Count > 0) GameManager.Instance
					.StartCoroutine(UpdateBlockLater(changes));

			//Log.Out("GatAt {0}", offs);
		}
	}

	[HarmonyPatch(typeof(Block))]
	[HarmonyPatch("GetCustomDescription")]
	public class Block_GetCustomDescription
	{
		public static bool Prefix(
			Vector3i _blockPos,
			BlockValue _bv,
			ref string __result)
		{
			if (_bv.Block is BlockPlantGrowing)
            {
				__result = _bv.Block.GetLocalizedBlockName();
				if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					// Log.Out("Last tick was {0} vs {1}", NetPackageGetPlantInfo.LastTick, GameTimer.Instance.ticks);
					if (NetPkgCustomInfo.LastTick + 300 > GameTimer.Instance.ticks)
					{
						if (NetPkgCustomInfo.LastPosition == _blockPos)
						{
							__result = NetPkgCustomInfo.LastText;
						}
					}
					if (NetPkgCustomInfo.LastAsk + 50 < GameTimer.Instance.ticks)
					{
						Log.Out("Asking the server");
						NetPkgCustomInfo.LastAsk = GameTimer.Instance.ticks;
						// Try to lazy load the information from the server
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
							NetPackageManager.GetPackage<NetPkgCustomInfo>()
								.Setup(_blockPos));
					}
				}
				else
                {
					if (PlantManager.TryGetGrowing(_blockPos, out PlantGrowing plant))
					{
						__result += string.Format("\nLight Level: {0:0}%", plant.LightValue / 0.15f);
						__result += string.Format("\nProgress: {0:0}%", plant.GrowProgress * 100f);
						__result += string.Format("\nAt {0}", plant.WorldPos);
					}
				}
				return false;
			}
			return true;
			// Log.Out("Chunk Loaded {0} {1} {2} {3}", _clrIdx, _x, _y, _z);
		}
	}


	static readonly MethodInfo MethodBlockOnBlockAdded =
		AccessTools.Method(typeof(Block), "OnBlockAdded");



	[HarmonyPatch(typeof(BlockPlantGrowing))]
	[HarmonyPatch("OnBlockAdded")]
	public class BlockPlantGrowing_OnBlockAdded
	{
		public static void Prefix(
			Block __instance,
			WorldBase _world,
			Chunk _chunk,
			Vector3i _blockPos,
			BlockValue _blockValue,
			ref bool ___isPlantGrowingRandom,
			ref bool __state)
		{
			__state = ___isPlantGrowingRandom;
			if (_blockValue.ischild) return;
			if (___isPlantGrowingRandom == false)
			{
				if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) return;
				Log.Out("Added new growing plant");
				PlantManager.LoadedGrowingPlant(_world, _blockPos, _blockValue, true);
				// PlantManager.Instance.SetGrowing(_blockPos, _chunk.ClrIdx, _blockValue, true);
				___isPlantGrowingRandom = true;
			}
		}

		public static void Postfix(bool __state,
			ref bool ___isPlantGrowingRandom)
		{
			___isPlantGrowingRandom = __state;
		}

	}

	static readonly FieldInfo FieldIsPlantGrowingRandom = AccessTools
		.Field(typeof(BlockPlantGrowing), "isPlantGrowingRandom");

	[HarmonyPatch(typeof(Block))]
	[HarmonyPatch("OnBlockAdded")]
	public class Block_OnBlockAdded
	{
		public static void Prefix(
			Block __instance,
			WorldBase _world,
			Chunk _chunk,
			Vector3i _blockPos,
			BlockValue _blockValue)
		{
			if (_blockValue.ischild) return;
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) return;
			//if (_blockValue.Block.Properties.GetBool("AutoHarvestable"))
			//	PlantManager.LoadedAutoHarvester(_world, _blockPos, _blockValue, true);
		}
	}
	/*
	[HarmonyPatch(typeof(Block))]
	[HarmonyPatch("OnBlockLoaded")]
	public class Block_OnBlockLoaded
	{

		private static IEnumerator UpdateBlockLater(WorldBase world, Vector3i _blockPos, int type)
		{
			yield return new WaitForSeconds(0f);
			Log.Out("UpdateBlockValue 3");
			world.SetBlockRPC(_blockPos, Block.GetBlockValue(type));
		}

		public static void Prefix(
			WorldBase _world,
			int _clrIdx,
			Vector3i _blockPos,
			BlockValue _blockValue)
		{
			if (_blockValue.ischild) return;
			if (PlantManager.Instance.PendingChange.TryGetValue(_blockPos, out int type))
			{
				// Ideally we would manipulate the block state directly here,
				// but we can't, since base code will explode due to error:
				// `Collection was modified` (which is somewhat reasonable).
				GameManager.Instance.StartCoroutine(UpdateBlockLater(_world, _blockPos, type));
				PlantManager.Instance.PendingChange.Remove(_blockPos);
//				_world.SetBlockRPC(_blockPos, Block.GetBlockValue(type));
			}
			if (_blockValue.Block is BlockPlantGrowing growing)
			{
				if (!(bool)FieldIsPlantGrowingRandom.GetValue(growing))
				{
					PlantManager.LoadedGrowingPlant(_world, _blockPos, _blockValue);
				}
			}
		}
	}
	*/

	[HarmonyPatch(typeof(Block))]
	[HarmonyPatch("OnBlockUnloaded")]
	public class Block_OnBlockUnloaded
	{

		public static void Prefix(
			Block __instance,
			WorldBase _world,
			int _clrIdx,
			Vector3i _blockPos,
			BlockValue _blockValue)
		{
			if (_blockValue.ischild) return;
			if (_blockValue.Block is BlockPlantGrowing growing)
			{
				if ((bool)FieldIsPlantGrowingRandom.GetValue(growing)) return;
				Log.Out("Unload block {0}", _blockValue.Block.GetBlockName());
				PlantManager.UnloadedGrowingPlant(_world, _blockPos);
			}
		}
	}

	[HarmonyPatch(typeof(Block))]
	[HarmonyPatch("OnBlockRemoved")]
	public class Block_OnBlockRemoved
	{
		public static void Prefix(
			Block __instance,
			WorldBase _world,
			Chunk _chunk,
			Vector3i _blockPos,
			BlockValue _blockValue)
		{
			if (__instance is BlockPlantGrowing growing)
			{
				Log.Warning("Removed the Block forever");
				PlantManager.Instance.RemoveGrowing(_blockPos);
			}
			else if (__instance is BlockCropsGrown harvestable)
			{
				// PlantManager.Instance.AddHarvestable(_blockPos, _blockValue);
			}
			// ToDo: check if this is really needed, as it adds overhead!
			if (_blockValue.Block.Properties.GetBool("AutoHarvestable"))
				// Remove any potential harvestable crops
				PlantManager.Instance.RemoveHarvestable(_blockPos);
		}
	}

	[HarmonyPatch(typeof(Block))]
	[HarmonyPatch("OnBlockValueChanged")]
	public class Block_OnBlockValueChanged
	{
		public static void Prefix(
			Block __instance,
			WorldBase _world,
			Chunk _chunk,
			int _clrIdx,
			Vector3i _blockPos,
			BlockValue _oldBlockValue,
			BlockValue _newBlockValue)
		{
			if (_oldBlockValue.Block.blockID != _newBlockValue.Block.blockID)
			{
				Log.Error("Change that also changes the type, unexpected");
			}
			// if (_newBlockValue.Block is BlockPlantGrowing)
			// {
			// 	PlantManager.Instance.SetGrowing(_blockPos, _clrIdx, _newBlockValue, true);
			// }
			// else if (_oldBlockValue.Block is BlockPlantGrowing)
			// {
			// 	PlantManager.Instance.RemoveGrowing(_blockPos);
			// }

		}
	}


	[HarmonyPatch(typeof(BlockPlantGrowing))]
	[HarmonyPatch("UpdateTick")]
	public class BlockPlantGrowing_UpdateTick
	{
		public static bool Prefix(
			WorldBase _world,
			int _clrIdx,
			Vector3i _blockPos,
			BlockValue _blockValue,
			bool _bRandomTick,
			bool ___isPlantGrowingRandom)
		{
			if (_bRandomTick) return false;

			if (PlantManager.Instance.PendingChange.TryGetValue(_blockPos, out int type))
			{
				var asd = Block.GetBlockValue(type);
				Log.Out("UpdateBlockValue 4");
				_world.SetBlockRPC(_blockPos, asd);
				return false;
			}
			else
			{
				Log.Warning("Who is ticking, don't!");
			}


			// No more random ticks!
			return !_bRandomTick;
			// if (!___isPlantGrowingRandom)
			// {
			// 	Log.Out("Update Tick");
			// 	Log.Out(Environment.StackTrace);
			// }
			// return ___isPlantGrowingRandom;
		}
	}

	// [HarmonyPatch(typeof(BlockPlantGrowing))]
	// [HarmonyPatch("LateInit")]
	// public class BlockPlantGrowing_LateInit
	// {
	// 	public static void Postfix(
	// 		ref bool ___isPlantGrowingRandom)
	// 	{
	// 		// ___isPlantGrowingRandom = true;
	// 	}
	// }

}
