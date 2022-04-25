
public interface ITickable
{
    Vector3i ToWorldPos();
    void Tick(WorldBase world, ulong delta);
}
