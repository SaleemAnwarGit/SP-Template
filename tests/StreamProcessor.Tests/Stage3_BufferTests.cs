namespace StreamProcessor.Tests;
using System.Diagnostics;
using Xunit;
using StreamProcessor.Pipeline;

/// <summary>
/// Tests for TODO-3.1 — BoundedBuffer core implementation (6 tests).
/// All tests directly instantiate BoundedBuffer and test its contract.
/// They all fail with NotImplementedException until TODO-3.1 is done.
/// </summary>
public class Stage3_BufferTests
{
    // ── T3.1a ─────────────────────────────────────────────────────────────
    // What it checks: Add() puts an item in and Take() gets the same item back.
    // Hint: you need an internal Queue<T> to store items.
    [Fact] public void T3_1a_Add_ThenTake_ReturnsSameItem()
    {
        var b = new BoundedBuffer<int>(5);
        b.Add(42);
        Assert.Equal(42, b.Take());
    }

    // ── T3.1b ─────────────────────────────────────────────────────────────
    // What it checks: Count reflects the current number of items.
    // Hint: Count must read _queue.Count inside a lock.
    [Fact] public void T3_1b_Count_TracksOccupancy()
    {
        var b = new BoundedBuffer<int>(5);
        Assert.Equal(0, b.Count);
        b.Add(1); Assert.Equal(1, b.Count);
        b.Take(); Assert.Equal(0, b.Count);
    }

    // ── T3.1c ─────────────────────────────────────────────────────────────
    // What it checks: Add() blocks the calling thread when the buffer is full.
    //                 The thread must be genuinely blocked for at least 100 ms.
    // Hint: how does Monitor.Wait(gate) differ from spinning in a while loop?
    [Fact] public void T3_1c_Add_Blocks_WhenFull()
    {
        var buf = new BoundedBuffer<int>(3);
        buf.Add(1); buf.Add(2); buf.Add(3);

        bool addReturned = false;
        var t = new Thread(() => { buf.Add(99); addReturned = true; });
        t.Start();
        Thread.Sleep(200);
        Assert.False(addReturned,
            "TODO-3.1: Add() returned immediately when buffer was full. " +
            "It must block the calling thread until a slot becomes available. " +
            "Hint: use Monitor.Wait inside a lock, or SemaphoreSlim.Wait() (Stage 4).");
        buf.Take();
        t.Join(1_000);
        Assert.True(addReturned, "Add() should complete after Take() frees a slot");
    }

    // ── T3.1d ─────────────────────────────────────────────────────────────
    // What it checks: TryTake() returns false when the buffer is empty and the
    //                 timeout expires, without blocking forever.
    // Hint: Monitor.Wait has an overload that accepts a timeout in milliseconds.
    [Fact] public void T3_1d_TryTake_ReturnsFalse_OnTimeout()
    {
        var buf = new BoundedBuffer<int>(5);
        var sw  = Stopwatch.StartNew();
        bool got = buf.TryTake(out _, 100);
        sw.Stop();
        Assert.False(got, "TODO-3.1: TryTake should return false on an empty buffer");
        Assert.True(sw.ElapsedMilliseconds >= 80,
            "TODO-3.1: TryTake returned too fast — it must wait for the timeout duration.");
    }

    // ── T3.1e ─────────────────────────────────────────────────────────────
    // What it checks: with multiple producers and consumers, every item produced
    //                 is consumed exactly once — no items lost, no duplicates.
    // Hint: this naturally works if Add() and Take() protect the queue with a lock.
    [Fact] public void T3_1e_MultiProducerConsumer_AllItemsDelivered()
    {
        const int P = 3, N = 100;
        var buf     = new BoundedBuffer<int>(PipelineConfig.BufferCapacity);
        var received = new System.Collections.Concurrent.ConcurrentBag<int>();

        var producers = Enumerable.Range(0, P)
            .Select(p => new Thread(() => { for(int i=0;i<N;i++) buf.Add(p*N+i); })).ToList();
        var consumers = Enumerable.Range(0, 2)
            .Select(_ => new Thread(() => {
                while(true){
                    if(buf.TryTake(out int item,50)) received.Add(item);
                    else if(buf.IsCompleted&&buf.Count==0) return;
                }
            })).ToList();

        producers.ForEach(t=>t.Start());
        consumers.ForEach(t=>t.Start());
        producers.ForEach(t=>t.Join());
        buf.Complete();
        consumers.ForEach(t=>t.Join());
        Assert.Equal(P*N, received.Count);
    }

    // ── T3.1f ─────────────────────────────────────────────────────────────
    // What it checks: after Complete() is called and all items are taken,
    //                 a consumer using TryTake can exit cleanly.
    // Hint: IsCompleted must return true after Complete() is called.
    [Fact] public void T3_1f_Complete_AllowsConsumerToExitCleanly()
    {
        var buf = new BoundedBuffer<int>(5);
        buf.Add(1); buf.Add(2); buf.Complete();
        var taken = new List<int>();
        while(true){
            if(buf.TryTake(out int item,50)) taken.Add(item);
            else if(buf.IsCompleted&&buf.Count==0) break;
        }
        Assert.Equal(new[]{1,2}, taken);
    }
}
