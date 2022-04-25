using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class BlockPipeSource : BlockPowered, IBlockPipeNode
{

	public int MaxConnections => 1;

	private int MinWaterBlocks = 9;

	private Vector3i WaterBlockRange = Vector3i.one;

	public bool CanConnect(int side, int rotation) => true;

	public override void Init()
	{
		base.Init();
		// Parse optional block XML setting properties
		if (Properties.Contains("MinWaterBlocks")) MinWaterBlocks =
				int.Parse(Properties.GetString("MinWaterBlocks"));
		if (Properties.Contains("WaterBlockRange")) WaterBlockRange =
				Vector3i.Parse(Properties.GetString("WaterBlockRange"));
	}

	public override void OnBlockAdded(
		WorldBase _world,
		Chunk _chunk,
		Vector3i _blockPos,
		BlockValue _blockValue)
	{
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

	// Static structure to be re-used on each call
	// Make sure call is not recursively ever (beware)!
	private static Vector3i _pos = new Vector3i();

	private bool HasEnoughWaterAround(
		WorldBase _world,
		Vector3i _blockPos)
	{
		int found = 0;
		// Use pretty explicit for loops instead of doing an addition with each iteration
		// This avoids an unnecessary vector3i allocation; going easy on the garbage collector
		for (_pos.x = _blockPos.x - WaterBlockRange.x; _pos.x <= _blockPos.x + WaterBlockRange.x; _pos.x++)
		{
			for (_pos.y = _blockPos.y - WaterBlockRange.y; _pos.y <= _blockPos.y + WaterBlockRange.y; _pos.y++)
			{
				for (_pos.z = _blockPos.z - WaterBlockRange.z; _pos.z <= _blockPos.z + WaterBlockRange.z; _pos.z++)
				{
					found += _world.IsWater(_pos) ? 1 : 0;
					if (found >= MinWaterBlocks) return true;
				}
			}
		}
		// ToDo: how should we cache this information?
		// ToDo: must be periodically checked (when loaded)
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
			&& PipeGridManager.Instance.CanConnect(this, _blockPos, _blockValue)
			&& HasEnoughWaterAround(_world, _blockPos);
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