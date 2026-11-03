namespace StreamProcessor.Tests;
using Xunit;
using StreamProcessor.Synchronization;

/// <summary>
/// Tests for TODO-5.1 (create deadlock) and TODO-5.2 (fix deadlock).
///
/// T5.1 expects RunWithDeadlock() to HANG — if it completes, the deadlock
///      was not created correctly.
/// T5.2 and T5.2b expect RunSafely() to COMPLETE within 3 seconds.
/// </summary>
public class Stage5_DeadlockTests
{
    // ── T5.1 ──────────────────────────────────────────────────────────────
    // What it checks: RunWithDeadlock() never finishes (it hangs forever).
    //                 The test gives it 3 seconds — if it completes, it fails.
    // Hint: a deadlock requires a circular wait. Thread A holds what B needs,
    //       and B holds what A needs. Thread.Sleep(50) ensures both threads
    //       have grabbed their first resource before trying for the second.
    [Fact] public async Task T5_1_RunWithDeadlock_HangsForever()
    {
        var rm      = new ResourceManager();
        var work    = Task.Run(() => rm.RunWithDeadlock());
        var timeout = Task.Delay(TimeSpan.FromSeconds(3));
        var first   = await Task.WhenAny(work, timeout);

        if (work.IsFaulted)
        {
            var ex = work.Exception?.InnerExceptions.FirstOrDefault();
            if (ex is NotImplementedException)
                Assert.Fail("TODO-5.1: RunWithDeadlock() is not implemented yet. " +
                            "Two threads, opposite lock order, Thread.Sleep(50) after first lock.");
            Assert.Fail($"RunWithDeadlock() threw: {ex?.Message}");
        }

        Assert.Same(timeout, first);  // timeout fires first = deadlock confirmed ✓
    }

    // ── T5.2a ─────────────────────────────────────────────────────────────
    // What it checks: RunSafely() completes within 3 seconds.
    // Hint: what is the only difference between the deadlock and the safe version?
    //       One word in the lock acquisition order.
    [Fact] public async Task T5_2a_RunSafely_CompletesWithinTimeout()
    {
        var rm = new ResourceManager();
        try { await Task.Run(() => rm.RunSafely()).WaitAsync(TimeSpan.FromSeconds(3)); }
        catch (NotImplementedException) { Assert.Fail("TODO-5.2: implement RunSafely() first."); }
        catch (TimeoutException)        { Assert.Fail("TODO-5.2: RunSafely() timed out — it may be deadlocked. Check lock order."); }
    }

    // ── T5.2b ─────────────────────────────────────────────────────────────
    // What it checks: RunSafely() can be called repeatedly without ever hanging.
    //                 A correct implementation never deadlocks regardless of timing.
    [Fact] public async Task T5_2b_RunSafely_IsRepeatable()
    {
        var rm = new ResourceManager();
        for (int i = 0; i < 5; i++)
        {
            try { await Task.Run(() => rm.RunSafely()).WaitAsync(TimeSpan.FromSeconds(3)); }
            catch (NotImplementedException) { Assert.Fail("TODO-5.2: implement RunSafely() first."); }
            catch (TimeoutException)        { Assert.Fail($"TODO-5.2: RunSafely() deadlocked on run {i+1}/5."); }
        }
    }
}
