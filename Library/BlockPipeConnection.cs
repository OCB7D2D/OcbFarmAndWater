class BlockPipeConnection : Block
{
    public override void Init()
    {
        base.Init();
    }

	// public virtual bool IsSource() => false;


    public override string GetCustomDescription(
		Vector3i _blockPos,
		BlockValue _bv)
    {
		if (PipeGridManager.Instance.TryGetNode(
			_blockPos, out PipeGridConnection connection))
        {
			return string.Format("Water {0}", connection.Grid);
		}
		return base.GetCustomDescription(_blockPos, _bv);
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
			&& PipeGridManager.Instance.CanConnect(_blockPos);
	}

	public override void OnBlockAdded(
		WorldBase _world,
		Chunk _chunk,
		Vector3i _blockPos,
		BlockValue _blockValue)
	{
		Log.Out("Block added");
		base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue);
		if (_blockValue.ischild) return;
		var connection = new PipeGridConnection(_blockPos, _blockValue);
		PipeGridManager.Instance.AddConnection(connection);
	}

	public override void OnBlockRemoved(
		WorldBase _world,
		Chunk _chunk,
		Vector3i _blockPos,
		BlockValue _blockValue)
	{
		base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
		if (_blockValue.ischild) return;
		PipeGridManager.Instance.RemoveConnection(_blockPos);
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
	}

	public override void OnBlockUnloaded(
		WorldBase _world,
		int _clrIdx,
		Vector3i _blockPos,
		BlockValue _blockValue)
	{
		base.OnBlockUnloaded(_world, _clrIdx, _blockPos, _blockValue);
		if (_blockValue.ischild) return;
	}


}
