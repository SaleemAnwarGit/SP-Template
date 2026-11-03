namespace StreamProcessor.Tests;
using Xunit;
using StreamProcessor.Infrastructure;
using StreamProcessor.Models;
using StreamProcessor.Naive;
using StreamProcessor.Pipeline;

/// <summary>
/// Tests for TODO-1.1, TODO-1.2, TODO-1.3 — Thread creation, start, join.
///
/// These tests fail when the sequential fallback is still active.
/// FallbackActive is set to true by the fallback block in Run().
/// Remove the fallback and replace it with real threads for these to pass.
/// </summary>
public class Stage1_ThreadTests
{
    // ── T1.1 ──────────────────────────────────────────────────────────────
    // What it checks: ValidatorStage creates real worker threads, not the fallback.
    // Hint: after you implement TODO-1.1, FallbackActive becomes false because
    //       the "FallbackActive = true" line is inside the fallback block you remove.
    [Fact]
    public void T1_1_ValidatorStage_CreatesWorkerThreads()
    {
        var gen  = new DataGenerator(42);
        var buf  = new SimpleBuffer<SensorReading>();
        var vbuf = new SimpleBuffer<SensorReading>();
        foreach (var r in gen.GenerateBatch(40)) buf.Add(r);
        buf.Complete();

        var stage = new ValidatorStage();
        stage.Run(buf, vbuf);

        Assert.False(stage.FallbackActive,
            "TODO-1.1: ValidatorStage is still running the sequential fallback. " +
            "Remove the FALLBACK block and create PipelineConfig.ValidatorThreadCount " +
            "Thread objects, each calling WorkerLoop().");
    }

    // ── T1.2 ──────────────────────────────────────────────────────────────
    // What it checks: Run() blocks the caller until all validator threads have
    //                 finished (Join() was called). Also verifies the worker
    //                 threads are not the same thread as the caller.
    // Hint: Start() tells a thread to begin. What method makes the caller wait
    //       until a thread has finished?
    [Fact]
    public void T1_2_ValidatorStage_StartsAndJoinsAllThreads()
    {
        var gen      = new DataGenerator(42);
        var readings = gen.GenerateBatch(100).Where(r => r.IsValid).ToList();
        var vbuf     = new SimpleBuffer<SensorReading>();
        var results  = new List<ProcessingResult>();
        foreach (var r in readings) vbuf.Add(r);
        vbuf.Complete();

        var trnStage = new TransformerStage();
        var done     = Task.Run(() => trnStage.Run(vbuf, results));
        Assert.True(done.Wait(TimeSpan.FromSeconds(5)),
            "TODO-1.2 / TODO-1.3: TransformerStage.Run() did not return within 5 s. " +
            "Did you call Join() on every thread after Start()?");

        Assert.False(trnStage.FallbackActive,
            "TODO-1.3: TransformerStage is still on the fallback.");

        var workerIds = results.Select(r => r.ProcessedByThreadId).Distinct().ToHashSet();
        Assert.DoesNotContain(Environment.CurrentManagedThreadId, workerIds);
    }

    // ── T1.3 ──────────────────────────────────────────────────────────────
    // What it checks: TransformerStage uses real worker threads (not the fallback)
    //                 and the stage completes within a timeout.
    // Hint: the pattern is identical to TODO-1.1 and 1.2 — same three steps,
    //       different stage, different config constant.
    [Fact]
    public void T1_3_TransformerStage_CreatesStartsAndJoinsThreads()
    {
        var gen      = new DataGenerator(42);
        var validBuf = new SimpleBuffer<SensorReading>();
        var results  = new List<ProcessingResult>();
        foreach (var r in gen.GenerateBatch(60).Where(r => r.IsValid))
            validBuf.Add(r);
        validBuf.Complete();

        var stage = new TransformerStage();
        var done  = Task.Run(() => stage.Run(validBuf, results));
        Assert.True(done.Wait(TimeSpan.FromSeconds(5)),
            "TODO-1.3: TransformerStage.Run() did not return within 5 s — " +
            "did you call Join()?");

        Assert.False(stage.FallbackActive,
            "TODO-1.3: TransformerStage is still using the sequential fallback. " +
            "Remove the FALLBACK block and create PipelineConfig.TransformerThreadCount threads.");

        Assert.True(results.Count > 0, "No results produced — check WorkerLoop is called");
    }
}
