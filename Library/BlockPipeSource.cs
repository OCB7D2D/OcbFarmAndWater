using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class BlockPipeSource : BlockPowered
{

	// public override bool IsSource() => true;
	public override void OnBlockAdded(
		WorldBase _world,
		Chunk _chunk,
		Vector3i _blockPos,
		BlockValue _blockValue)
	{
		Log.Out("Block added");
		base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue);
		if (_blockValue.ischild) return;
		var source = new PipeGridSource(_blockPos, _blockValue);
		PipeGridManager.Instance.AddSource(source);
	}

	public override void OnBlockRemoved(
	WorldBase _world,
	Chunk _chunk,
	Vector3i _blockPos,
	BlockValue _blockValue)
	{
		base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
		if (_blockValue.ischild) return;
		PipeGridManager.Instance.RemoveSource(_blockPos);
	}

	private bool HasEnoughWaterAround(
		WorldBase _world,
		int _clrIdx,
		Vector3i _blockPos,
		int limit = 9)
    {
		int found = 0;
		Vector3i position = new Vector3i();
		for (position.x = _blockPos.x - 1; position.x <= _blockPos.x + 1; position.x++)
		{
			for (position.y = _blockPos.y - 1; position.y <= _blockPos.y + 1; position.y++)
			{
				for (position.z = _blockPos.z - 1; position.z <= _blockPos.z + 1; position.z++)
				{
					found += _world.GetBlock(_clrIdx, position).isWater ? 1 : 0;
					if (found >= limit) return true;
				}
			}
		}
		return false;
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
			&& HasEnoughWaterAround(_world, _clrIdx, _blockPos);
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
			_blockPos, out PipeGridSource pump))
		{
			desc += string.Format(
				"\nFound Grid Source {0}!",
				pump.IsPowered);
		}
		return desc + "!!";
	}

}