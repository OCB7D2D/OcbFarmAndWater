using System;
using UnityEngine;

public class BlockAutoHarvest : BlockPowered
{

	public static TileEntityType TileEntityID = (TileEntityType)245;

	private readonly float BoundHelperSize = 2.59f;

	public float HarvestSpeed = 0.1f;

	public int LookupsPerTick = 8;

	public BlockAutoHarvest() => HasTileEntity = false;

	public override void Init()
	{
		base.Init();
		HarvestSpeed = !Properties.Values.ContainsKey("AutoHarvestSpeed") ? HarvestSpeed
			: StringParsers.ParseFloat(Properties.Values["AutoHarvestSpeed"]);
		LookupsPerTick = !Properties.Values.ContainsKey("LookupsPerTick") ? LookupsPerTick
			: StringParsers.ParseSInt32(Properties.Values["LookupsPerTick"]);
	}

	//	public override TileEntityPowered CreateTileEntity(Chunk chunk)
	//	{
	//		PowerItem.PowerItemTypes powerItemTypes = PowerItem.PowerItemTypes.ConsumerToggle;
	//		TileEntityAutoHarvest entityPoweredBlock = new TileEntityAutoHarvest(chunk);
	//		entityPoweredBlock.PowerItemType = powerItemTypes;
	//		entityPoweredBlock.HarvestSpeed = HarvestSpeed;
	//		entityPoweredBlock.LookupsPerTick = LookupsPerTick;
	//		return entityPoweredBlock;
	//	}

	public void AddBoundHelper(
		WorldBase _world,
		int _clrIdx,
		Vector3i _blockPos)
	{
		Log.Out("Access manager instance");
		BoundHelperManager.Instance.AddHelper(_blockPos,
			_blockPos.ToVector3() + new Vector3(0.5f, 0.5f, 0.5f),
			new Vector3(BoundHelperSize, BoundHelperSize, BoundHelperSize),
			Color.gray * 0.5f);
	}

	public void RemoveBoundHelper(
	WorldBase _world,
	int _clrIdx,
	Vector3i _blockPos)
	{
		BoundHelperManager.Instance.RemoveHelper(_blockPos);
	}

	// Copied from vanilla BlockLandClaim code
	public override void OnBlockEntityTransformAfterActivated(
		WorldBase _world,
		Vector3i _blockPos,
		int _cIdx,
		BlockValue _blockValue,
		BlockEntityData _ebcd)
	{
		if (_ebcd == null) return;
		base.OnBlockEntityTransformAfterActivated(_world,
			_blockPos, _cIdx, _blockValue, _ebcd);
		// if (_world.GetTileEntity(_cIdx, _blockPos) is TileEntityAutoHarvest te)
		// {
		// 	te.HarvestSpeed = HarvestSpeed;
		// 	te.LookupsPerTick = LookupsPerTick;
		// }
	}

	public override void OnBlockAdded(
		WorldBase _world,
		Chunk _chunk,
		Vector3i _blockPos,
		BlockValue _blockValue)
	{
		base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue);
		if (_blockValue.ischild) return;
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			PlantManager.LoadedAutoHarvester(_world, _blockPos, _blockValue, true);
			Log.Out("OnBlockAdded::DispatchLoad");
		}
		AddBoundHelper(_world, _chunk.ClrIdx, _blockPos);
		Log.Out("OnBlockAdded");
	}

	public override void OnBlockRemoved(
		WorldBase _world,
		Chunk _chunk,
		Vector3i _blockPos,
		BlockValue _blockValue)
	{
		base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
		if (_blockValue.ischild) return;
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
			PlantManager.RemoveAutoHarvester(_blockPos);
			Log.Out("OnBlockRemoved::DispatchLoad");
		}
		RemoveBoundHelper(_world, _chunk.ClrIdx, _blockPos);
		Log.Out("OnBlockRemoved");
	}

	public override void OnBlockLoaded(
		WorldBase _world,
		int _clrIdx,
		Vector3i _blockPos,
		BlockValue _blockValue)
	{
		// Check pending stuff
		base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
		if (_blockValue.ischild) return;
		if (ConnectionManager.Instance.IsServer)
		{
			PlantManager.LoadedAutoHarvester(_world, _blockPos, _blockValue, false);
			Log.Out("OnBlockLoaded::DispatchLoad");
		}
		AddBoundHelper(_world, _clrIdx, _blockPos);
		Log.Out("OnBlockLoaded");
	}

	public override void OnBlockUnloaded(
		WorldBase _world,
		int _clrIdx,
		Vector3i _blockPos,
		BlockValue _blockValue)
	{
		base.OnBlockUnloaded(_world, _clrIdx, _blockPos, _blockValue);
		if (_blockValue.ischild) return;
		if (ConnectionManager.Instance.IsServer)
			PlantManager.UnloadedAutoHarvester(_blockPos);
		RemoveBoundHelper(_world, _clrIdx, _blockPos);
	}

	private BlockActivationCommand[] cmds = new BlockActivationCommand[2]
	{
		new BlockActivationCommand("toggle", "electric_switch", true),
		new BlockActivationCommand("take", "hand", false)
	};

	public override BlockActivationCommand[] GetBlockActivationCommands(
		WorldBase _world,
		BlockValue _blockValue,
		int _clrIdx,
		Vector3i _blockPos,
		EntityAlive _entityFocusing)
	{
		cmds[1].enabled = _world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer()) && TakeDelay > 0.0;
		return cmds;
	}

	public override string GetActivationText(
	  WorldBase _world,
	  BlockValue _blockValue,
	  int _clrIdx,
	  Vector3i _blockPos,
	  EntityAlive _entityFocusing)
	{
		if (cmds[0].enabled)
		{
			// if (_world.GetTileEntity(_clrIdx, _blockPos) is TileEntityAutoHarvest tileEntity)
			// {
			// 	return tileEntity.IsToggled ? Localization.Get("xuiTurnOff") : Localization.Get("xuiTurnOn");
			// }
		}
		else if (cmds[1].enabled)
		{
			Block block = _blockValue.Block;
			return string.Format(Localization.Get("pickupPrompt"), block.GetLocalizedBlockName());
		}
		return null;
	}

	public override bool OnBlockActivated(
		int _command,
		WorldBase _world,
		int _cIdx,
		Vector3i _blockPos,
		BlockValue _blockValue,
		EntityAlive _player)
	{
		if (_command == 0)
		{
			// if (_world.GetTileEntity(_cIdx, _blockPos) is TileEntityAutoHarvest tileEntity)
            // {
			// 	tileEntity.IsToggled = !tileEntity.IsToggled;
			// 	return true;
			// }
		}
		else if (_command == 1)
		{
			TakeItemWithTimer(_cIdx, _blockPos, _blockValue, _player);
			return true;
		}
		return false;
	}

	// PoweredItem communicates state change via this hook (indirectly)
	public override void OnBlockValueChanged(
		WorldBase _world,
		Chunk _chunk,
		int _clrIdx,
		Vector3i _blockPos,
		BlockValue _oldBlockValue,
		BlockValue _newBlockValue)
	{
		base.OnBlockValueChanged(_world, _chunk, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
		// We will wait for the ticker to detect the change a little bit later
	}

	public override bool ActivateBlock(
	  WorldBase _world,
	  int _cIdx,
	  Vector3i _blockPos,
	  BlockValue _blockValue,
	  bool isOn,
	  bool isPowered)
	{
		if (((_blockValue.meta & 2) == 2) == isOn) return true;
		_blockValue.meta = (byte)((int)_blockValue.meta & -3 | (isOn ? 2 : 0));
		// This will trigger `OnBlockValueChanged` on all clients
		_world.SetBlockRPC(_cIdx, _blockPos, _blockValue);
		return true;
	}

}
