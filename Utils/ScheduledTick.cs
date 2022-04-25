using System.Collections.Generic;
using UnityEngine;

public class ScheduledTick : IComparer<ScheduledTick>, System.IComparable<ScheduledTick>
{

    public ITickable Object;
    public ulong TickTime;
    public ulong TickStart;

    public int Compare(ScheduledTick x, ScheduledTick y)
    {
        var rv = x.TickTime.CompareTo(y.TickTime);
        if (rv == 0) rv = x.TickStart.CompareTo(y.TickStart);
        if (rv == 0) rv = x.Object.ToWorldPos().Equals(y.Object.ToWorldPos()) ? 0 : -1;
        return rv;
    }

    public int CompareTo(ScheduledTick b)
    {
        return Compare(this, b);
    }

    public ScheduledTick(ulong start, ulong time, ITickable tickable)
    {
        TickStart = start;
        TickTime = time;
        Object = tickable;
    }

    public ScheduledTick(ulong offset, ITickable tickable)
    {
        TickStart = GameTimer.Instance.ticks;
        TickTime = TickStart + offset;
        TickTime += (ulong)Random.Range(0, 30);
        Object = tickable;
    }

}
