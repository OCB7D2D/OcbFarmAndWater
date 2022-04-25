﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class BlockPipePump : BlockPowered, IBlockPipeNode
{
	public int MaxConnections => 6;

	public bool CanConnect(int side, int rotation) => true;

	public override void OnBlockAdded(
		WorldBase _world,
		Chunk _chunk,
		Vector3i _blockPos,
		BlockValue _blockValue)
	{
		base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue);
		if (_blockValue.ischild) return;
		var pump = new PipeGridPump(_blockPos, _blockValue);
		PipeGridManager.Instance.AddPump(pump);
		requiredPower = 20;
	}

	public override void OnBlockRemoved(
	WorldBase _world,
	Chunk _chunk,
	Vector3i _blockPos,
	BlockValue _blockValue)
	{
		base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
		if (_blockValue.ischild) return;
		PipeGridManager.Instance.RemovePump(_blockPos);
	}

	public override bool CanPlaceBlockAt(
		WorldBase _world,
		int _clrIdx,
		Vector3i _blockPos,
		BlockValue _blockValue,
		// Ignore existing blocks?
		bool _bOmitCollideCheck = false)
	{
		return base.CanPlaceBlockAt(_world, _clrIdx, _blockPos, _blockValue, _bOmitCollideCheck)
			&& PipeGridManager.Instance.CanConnect(this, _blockPos, _blockValue);
	}

	public override void DoExchangeAction(
	  WorldBase _world,
	  int _clrIdx,
	  Vector3i _blockPos,
	  BlockValue _blockValue,
	  string _action,
	  int _itemCount)
	{
		Log.Out("Do exchange action {0} {1}", _action, _itemCount);
	}

	public override string GetActivationText(
		WorldBase _world,
		BlockValue _blockValue,
		int _clrIdx,
		Vector3i _blockPos,
		EntityAlive _entityFocusing)
	{
		string desc = base.GetActivationText(_world,
			_blockValue, _clrIdx, _blockPos, _entityFocusing);
		if (PipeGridManager.Instance.TryGetNode(
			_blockPos, out PipeGridPump pump))
		{
			desc += string.Format(
				"\nFound Grid Pump {0}!",
				pump.IsPowered);
		}
		return desc + "!!";
	}

}