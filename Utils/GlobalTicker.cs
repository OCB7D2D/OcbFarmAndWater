using System.Collections.Generic;
using System.Linq;

// Sorted ticks to dispatch work in chunks
// You need to drive it from time to time
// Will make sure to not over utilize CPU
public class GlobalTicker : SingletonInstance<GlobalTicker>
{

    // A set of ticks pre-sorted to dispatch in batches for less CPU strain
    public SortedSet<ScheduledTick> Scheduled = new SortedSet<ScheduledTick>();

    public int Count { get => Scheduled.Count; }

    public ScheduledTick Schedule(ulong ticks, ITickable tickable)
    {
        Log.Out("+++++ Scheduled in {0}", ticks);
        ScheduledTick scheduled = new ScheduledTick(ticks, tickable, this);
        Scheduled.Add(scheduled);
        // Log.Out("Scheduled in {2} (left growing: {0}, ticks: {1})",
        //     Instance.Growing.Count, Instance.ScheduledTicks.Count, ticks);
        return scheduled;
    }

    public bool Unschedule(ScheduledTick scheduled)
    {
        return Scheduled.Remove(scheduled);
    }

    public void OnTick(WorldBase world)
    {
        int done = 0;
        var tick = GameTimer.Instance.ticks;
        int todo = Utils.FastMin(Scheduled.Count, 8);
        while (Scheduled.Count != 0)
        {
            if (done > todo) break;
            var scheduled = Scheduled.First();
            if (scheduled.TickTime == ulong.MaxValue) break;
            if (scheduled.TickTime > tick) break;
            Scheduled.Remove(scheduled);
            ulong delta = tick - scheduled.TickStart;
            scheduled.Object.Tick(world, delta);
            if (scheduled.Object.HasInterval(out ulong iv))
                Schedule(iv, scheduled.Object);
            done += 1;
        }
    }

}
