using System.Collections.Generic;

public class IntervalNodePool
{
    private Stack<IntervalNode> pool;
    private int maxPoolSize;

    public IntervalNodePool(int initialSize = 100, int maxPoolSize = 1000)
    {
        this.maxPoolSize = maxPoolSize;
        pool = new(initialSize);

        for (int i = 0; i < initialSize; i++)
        {
            pool.Push(new(new Interval(0, 0)));
        }
    }

    public IntervalNode Get(Interval interval)
    {
        if (pool.Count > 0)
        {
            IntervalNode intervalNode = pool.Pop();
            intervalNode.Interval = interval;
            return intervalNode;
        }
        else
        {
            return new IntervalNode(interval);
        }
    }

    public void Release(IntervalNode intervalNode)
    {
        if (pool.Count < maxPoolSize)
        {
            intervalNode.Reset(); // Reset the interval before reusing it.
            pool.Push(intervalNode);
        }
    }
}