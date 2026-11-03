namespace StreamProcessor.Tests;

using Xunit;
using StreamProcessor.Infrastructure;
using StreamProcessor.Models;
using StreamProcessor.Naive;
using StreamProcessor.Pipeline;

/// <summary>
/// Tests for TODO-2.1 (lock on invalidCount) and TODO-2.2 (lock on resultStore).
///
/// These tests first verify Stage 1 is done (FallbackActive == false).
/// Then they run the stage 20 times and assert the count is always exactly right.
/// Without a lock, two concurrent threads will sometimes produce a wrong count.
/// With a lock, every run gives the same correct answer.
/// </summary>
public class Stage2_LockTests
{
    // ── T2.1 ──────────────────────────────────────────────────────────────
    // What it checks: ValidatorStage.InvalidCount is exactly correct every time,
    //                 even with two threads writing to it simultaneously.
    // Hint: two threads doing counter++ at the same time can overwrite each
    //       other's result. What C# keyword creates a mutual exclusion zone?
    [Fact]
    public void T2_1_InvalidCount_IsAlwaysExact()
    {
        var gen = new DataGenerator(42);
        var readings = gen.GenerateBatch(200);
        int expected = readings.Count(r => !r.IsValid);
        int failures = 0;

        for (int run = 0; run < 20; run++)
        {
            var inputBuf = new SimpleBuffer<SensorReading>();
            var validBuf = new SimpleBuffer<SensorReading>();
            foreach (var r in readings) inputBuf.Add(r);
            inputBuf.Complete();

            var stage = new ValidatorStage();
            stage.Run(inputBuf, validBuf);

            Assert.False(stage.FallbackActive,
                "TODO-2.1: complete TODO-1.1 and TODO-1.2 first — " +
                "the race condition on invalidCount only exists when real threads run.");

            if (stage.InvalidCount != expected) failures++;
        }

        Assert.True(failures == 0,
    $"TODO-2.1: InvalidCount was wrong in {failures}/20 runs. " +
    "Two threads increment _invalidCount simultaneously without protection. " +
    "Hint: wrap _invalidCount++ with a lock statement using _invalidLock.");
    }

    // ── T2.2 ──────────────────────────────────────────────────────────────
    // What it checks: resultStore.Count is exactly correct every time,
    //                 even with two transformer threads adding to it simultaneously.
    // Hint: List<T>.Add() is not thread-safe. Look at what lock object is already
    //       declared in TransformerStage and use it.
    [Fact]
    public void T2_2_ResultStore_CountIsAlwaysExact()
    {
        var gen = new DataGenerator(42);
        var readings = gen.GenerateBatch(200);
        int expected = readings.Count(r => r.IsValid);
        int failures = 0;

        for (int run = 0; run < 20; run++)
        {
            var validBuf = new SimpleBuffer<SensorReading>();
            var results = new List<ProcessingResult>();
            foreach (var r in readings.Where(r => r.IsValid)) validBuf.Add(r);
            validBuf.Complete();

            var stage = new TransformerStage();
            stage.Run(validBuf, results);

            Assert.False(stage.FallbackActive,
                "TODO-2.2: complete TODO-1.3 first — " +
                "the race condition on resultStore only exists when real threads run.");

            if (results.Count != expected) failures++;
        }


        Assert.True(failures == 0,
            $"TODO-2.2: resultStore.Count was wrong in {failures}/20 runs. " +
            "Two threads call resultStore.Add() simultaneously. " +
            "Hint: wrap resultStore.Add(result) with a lock statement using ResultStoreLock.");
    }
}
