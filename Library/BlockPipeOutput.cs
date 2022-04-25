﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class BlockPipeOutput : BlockPowered
{

	public override void OnBlockAdded(
		WorldBase _world,
		Chunk _chunk,
		Vector3i _blockPos,
		BlockValue _blockValue)
	{
		Log.Out("Block Out added");
		base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue);
		if (_blockValue.ischild) return;
		var output = new PipeGridOutput(_blockPos, _blockValue);
		PipeGridManager.Instance.AddOutput(output);
	}

	public override void OnBlockRemoved(
	WorldBase _world,
	Chunk _chunk,
	Vector3i _blockPos,
	BlockValue _blockValue)
	{
		base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
		if (_blockValue.ischild) return;
		PipeGridManager.Instance.RemoveOutput(_blockPos);
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
			_blockPos, out PipeGridOutput output))
		{
			desc += string.Format(
				"\nFound Grid Output {0}!",
				output.IsPowered);
		}
		return desc + "??";
	}

}