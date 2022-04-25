class BlockPipeWell : Block
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
		var well = new PipeGridWell(_blockPos, _blockValue);
		PipeGridManager.Instance.AddWell(well);
	}

	public override void OnBlockRemoved(
	WorldBase _world,
	Chunk _chunk,
	Vector3i _blockPos,
	BlockValue _blockValue)
	{
		base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
		if (_blockValue.ischild) return;
		PipeGridManager.Instance.RemoveWell(_blockPos);
	}

	public override void OnBlockEntityTransformAfterActivated(
		WorldBase _world,
		Vector3i _blockPos,
		int _cIdx,
		BlockValue _blockValue,
		BlockEntityData _ebcd)
    {
		base.OnBlockEntityTransformAfterActivated(
			_world, _blockPos, _cIdx, _blockValue, _ebcd);
		if (_blockValue.ischild) return;
		if (PipeGridManager.Instance.TryGetNode(
			_blockPos, out PipeGridWell well))
		{
			well.UpdateBlockEntity(_ebcd);
		}
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

	public override string GetCustomDescription(
		Vector3i _blockPos,
		BlockValue _bv)
	{
		string desc = base.GetCustomDescription(_blockPos, _bv);
		if (PipeGridManager.Instance.TryGetNode(
			_blockPos, out PipeGridWell well))
		{
			desc += string.Format(
				"\nAvailable: {0:0.00}",
				well.WaterAvailable);
		}
		return desc+"!!";
	}

	// Consume as many items as possible (return amount consumed)
	public int ConsumeWater(Vector3i position, int count = 1, int factor = 1)
	{
		if (count <= 0) return 0;
		if (PipeGridManager.Instance.TryGetNode(
			position, out PipeGridWell well))
		{
			if (well.WaterAvailable / factor >= count)
			{
				well.WaterAvailable -= count * factor;
				well.UpdateWaterLevel();
				return count;
			}
			else
			{
				count = (int)well.WaterAvailable / factor;
				well.WaterAvailable -= count * factor;
				well.UpdateWaterLevel();
				return count;
			}
		}
		return 0;
	}

	public int FillWater(Vector3i position, int count = 1, int factor = 1)
	{
		if (count <= 0) return 0;
		if (PipeGridManager.Instance.TryGetNode(
			position, out PipeGridWell well))
		{
			if (well.WaterNeeded == 0) return 0;
			if (factor > 1 && well.WaterNeeded < factor)
            {
				// Fill up if we use more than 50%
				if (well.WaterNeeded * 2 > factor)
                {
					well.WaterAvailable = well.MaxWaterLevel;
					well.UpdateWaterLevel();
					return 1;
				}
			}
			else if (well.WaterNeeded / factor >= count)
			{
				well.WaterAvailable += count * factor;
				well.UpdateWaterLevel();
				return count;
			}
			else
			{
				count = (int)well.WaterNeeded / factor;
				well.WaterAvailable += count * factor;
				well.UpdateWaterLevel();
				return count;
			}
		}
		return 0;
	}

}