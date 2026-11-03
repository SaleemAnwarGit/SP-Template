namespace StreamProcessor.Tests;
using System.Reflection;
using System.Diagnostics;
using Xunit;
using StreamProcessor.Pipeline;

/// <summary>
/// Tests for TODO-4.1 (two SemaphoreSlim fields) and TODO-4.2 (replace Monitor).
/// T4.1 uses reflection — it checks the type's fields, not an instance.
/// T4.2 checks that a producer unblocks promptly after a consumer takes an item.
/// </summary>
public class Stage4_SemaphoreTests
{
    // ── T4.1 ──────────────────────────────────────────────────────────────
    // What it checks: BoundedBuffer<T> declares exactly two SemaphoreSlim fields.
    //                 This uses reflection and works even before the constructor
    //                 is implemented — it inspects the type definition itself.
    // Hint: one semaphore tracks empty slots (starts at capacity), the other
    //       tracks filled slots (starts at 0). What is the .NET type for this?
    [Fact] public void T4_1_BoundedBuffer_HasExactlyTwoSemaphoreSlimFields()
    {
        var fields = typeof(BoundedBuffer<int>)
            .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => f.FieldType == typeof(SemaphoreSlim))
            .ToList();
        Assert.Equal(2, fields.Count);
    }

    // ── T4.2 ──────────────────────────────────────────────────────────────
    // What it checks: when Take() frees a slot, the producer that was blocked
    //                 in Add() unblocks promptly (within 100 ms).
    //                 With Monitor: Release wakes a waiting thread quickly.
    //                 With SemaphoreSlim: Release() does the same, but the
    //                 CPU stays near 0% while waiting (no busy-wait).
    // Hint: in your Add(), after enqueueing, what do you call to signal that
    //       a new item is available? And what do you call to signal a free slot?
    [Fact] public void T4_2_Producer_Unblocks_Promptly_After_Take()
    {
        var buf = new BoundedBuffer<int>(2);
        buf.Add(1); buf.Add(2);  // full

        bool addDone = false;
        var t = new Thread(() => { buf.Add(3); addDone = true; });
        t.Start();
        Thread.Sleep(60);
        Assert.False(addDone,
            "TODO-4.2: buffer full — Add() should be blocking, not returning immediately");

        var sw = Stopwatch.StartNew();
        buf.Take();           // frees one slot
        t.Join(500);
        sw.Stop();

        Assert.True(addDone,
            "TODO-4.2: producer did not unblock after Take() freed a slot");
        Assert.True(sw.ElapsedMilliseconds < 100,
            $"TODO-4.2: producer took {sw.ElapsedMilliseconds} ms to unblock after Take(). " +
            "Hint: does your Add() signal waiting producers when a slot becomes free?");
    }
}
