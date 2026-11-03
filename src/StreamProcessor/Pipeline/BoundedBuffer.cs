namespace StreamProcessor.Pipeline;

public sealed class BoundedBuffer<T> : IPipelineBuffer<T>
{
    /*
     * ┌─────────────────────────────────────────────────────────────────────┐
     * │ TODO-3.1  BoundedBuffer core implementation            [20 pts ★]  │
     * │ TODO-4.1  Add two SemaphoreSlim fields                  [7 pts]    │
     * │ TODO-4.2  Replace Monitor with SemaphoreSlim            [8 pts]    │
     * ├─────────────────────────────────────────────────────────────────────┤
     * │ TODO-3.1: add Queue<T> _queue, object _gate, bool _completed.      │
     * │   Add() — block caller when Count >= Capacity, then enqueue.       │
     * │   Take() — block caller when Count == 0, then dequeue.             │
     * │   TryTake() — wait up to timeoutMs, return false on timeout.       │
     * │   Blocking mechanism for 3.1: lock + Monitor.Wait + Monitor.Pulse  │
     * │   Forbidden: BlockingCollection, ConcurrentQueue                   │
     * │                                                                     │
     * │ TODO-4.1: declare two SemaphoreSlim fields (names are your choice).│
     * │   Hint: one counts empty slots, the other counts filled slots.     │
     * │                                                                     │
     * │ TODO-4.2: replace Monitor.Wait / Monitor.PulseAll with the two    │
     * │   SemaphoreSlim fields. Producers Wait on one, Release the other.  │
     * │   Consumers do the opposite. The lock stays but only guards        │
     * │   the Queue<T> enqueue/dequeue, not the blocking.                  │
     * └─────────────────────────────────────────────────────────────────────┘
     */

    public int  Capacity    { get; }
    public bool IsCompleted { get; private set; }

    public int Count =>
        throw new NotImplementedException(
            "TODO-3.1: implement Count — return _queue.Count inside a lock");

    public BoundedBuffer(int capacity)
    {
        if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        Capacity = capacity;
        throw new NotImplementedException(
            "TODO-3.1: initialise _queue and _gate here, then remove this throw.\n" +
            "TODO-4.1: also initialise your two SemaphoreSlim fields here.");
    }

    public void Add(T item) =>
        throw new NotImplementedException("TODO-3.1");

    public T Take() =>
        throw new NotImplementedException("TODO-3.1");

    public bool TryTake(out T item, int timeoutMs)
    {
        item = default!;
        throw new NotImplementedException("TODO-3.1");
    }

    public void Complete() => IsCompleted = true;
}
