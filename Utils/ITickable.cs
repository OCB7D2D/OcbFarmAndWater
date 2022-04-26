
public interface ITickable
{
    Vector3i ToWorldPos();
    void Tick(WorldBase world, ulong delta);
    bool HasInterval(out ulong interval);
    bool GetBlock<T>(out T var) where T : class;
}
