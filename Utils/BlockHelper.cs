using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class BlockHelper
{

    public static List<T> FindBlock<T>(Vector3i pos, Vector2i area, Vector2i height)
    {
        return null;
    }
    public static bool IsLoaded(WorldBase world, Vector3i position)
    {
        return world.GetChunkFromWorldPos(position) != null;
    }

    public static bool IsLoaded(Vector3i position)
    {
        if (GameManager.Instance == null) return false;
        if (GameManager.Instance.World == null) return false;
        WorldBase world = GameManager.Instance.World;
        return IsLoaded(world, position);
    }

}
