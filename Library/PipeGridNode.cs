using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class PipeGridNode : WorldNode
{
    public PipeGridNode(Vector3i position, BlockValue block)
        : base(position, block) { }

    public BlockPipeConnection Block => global::Block
        .list[BlockID] as BlockPipeConnection;

    public bool CanConnect(int side)
    {
        // Rotates question back into local frame
        return Block.CanConnect(side, Rotation);
    }

}
