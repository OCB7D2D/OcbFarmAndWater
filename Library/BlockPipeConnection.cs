using System.Collections.Generic;
using XMLData.Parsers;

public class BlockPipeConnection : Block, IBlockPipeNode
{

	byte ConnectMask = 63;

	public int MaxConnections => 6;
	
	public override void Init()
    {
        base.Init();
		// Parse potential pipe connectors
		if (Properties.Contains("PipeConnectors"))
        {
			ConnectMask = 0; // Reset the mask first
			string[] connectors = Properties.GetString("PipeConnectors").ToLower()
				.Split(new[] { ',', ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
			foreach (string connector in connectors)
            {
				ConnectMask |= (byte)(1 << (byte)EnumParser
					.Parse<FullRotation.Side>(connector));
            }
		}
	}

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
			&& PipeGridManager.Instance.CanConnect(this, _blockPos, _blockValue);
	}


	public bool CanConnect(int side, int rotation = 0)
    {
		side = FullRotation.InvSide(side, rotation);
		return (ConnectMask & (byte)(1 << (byte)side)) != 0;
    }

	public override void OnBlockAdded(
		WorldBase _world,
		Chunk _chunk,
		Vector3i _blockPos,
		BlockValue _blockValue)
	{
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
