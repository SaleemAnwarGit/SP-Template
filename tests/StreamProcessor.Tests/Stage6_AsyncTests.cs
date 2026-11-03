namespace StreamProcessor.Tests;
using Xunit;
using StreamProcessor.Models;
using StreamProcessor.Pipeline;

/// <summary>
/// Tests for TODO-6.1 (ComputeAsync) and TODO-6.2 (WriteReportAsync).
/// T6.1a–T6.1e test individual statistics from ComputeAsync.
/// T6.2 tests that WriteReportAsync creates a readable file.
/// All fail with NotImplementedException until the TODO is done.
/// </summary>
public class Stage6_AsyncTests
{
    private static List<ProcessingResult> Known() => new()
    {
        new(){NormalizedValue=0.00},  // min
        new(){NormalizedValue=0.25},
        new(){NormalizedValue=0.50},
        new(){NormalizedValue=0.75},
        new(){NormalizedValue=1.00},  // max
    };
    // sum=2.5, avg=0.5, count=5

    // ── T6.1a ─────────────────────────────────────────────────────────────
    // What it checks: ComputeAsync returns the correct Count.
    // Hint: Count is simply results.Count. Use await Task.Run(() => { ... }).
    [Fact] public async Task T6_1a_ComputeAsync_Count()
    {
        try { Assert.Equal(5, (await new Aggregator().ComputeAsync(Known())).Count); }
        catch (NotImplementedException) { Assert.Fail("TODO-6.1: implement ComputeAsync using async/await and Task.Run."); }
    }

    // ── T6.1b ─────────────────────────────────────────────────────────────
    [Fact] public async Task T6_1b_ComputeAsync_Sum()
    {
        try { Assert.Equal(2.5, (await new Aggregator().ComputeAsync(Known())).Sum, precision:10); }
        catch (NotImplementedException) { Assert.Fail("TODO-6.1: implement ComputeAsync."); }
    }

    // ── T6.1c ─────────────────────────────────────────────────────────────
    [Fact] public async Task T6_1c_ComputeAsync_Min()
    {
        try { Assert.Equal(0.0, (await new Aggregator().ComputeAsync(Known())).Min, precision:10); }
        catch (NotImplementedException) { Assert.Fail("TODO-6.1: implement ComputeAsync."); }
    }

    // ── T6.1d ─────────────────────────────────────────────────────────────
    [Fact] public async Task T6_1d_ComputeAsync_Max()
    {
        try { Assert.Equal(1.0, (await new Aggregator().ComputeAsync(Known())).Max, precision:10); }
        catch (NotImplementedException) { Assert.Fail("TODO-6.1: implement ComputeAsync."); }
    }

    // ── T6.1e ─────────────────────────────────────────────────────────────
    [Fact] public async Task T6_1e_ComputeAsync_Average()
    {
        try { Assert.Equal(0.5, (await new Aggregator().ComputeAsync(Known())).Average, precision:10); }
        catch (NotImplementedException) { Assert.Fail("TODO-6.1: implement ComputeAsync."); }
    }

    // ── T6.2 ──────────────────────────────────────────────────────────────
    // What it checks: WriteReportAsync creates a file containing at least
    //                 the Count field. Must use StreamWriter + WriteLineAsync.
    // Hint: open a StreamWriter, await WriteLineAsync for each line, dispose with await using.
    [Fact] public async Task T6_2_WriteReportAsync_CreatesFileWithContent()
    {
        string path = Path.Combine(Path.GetTempPath(), $"sp_{Guid.NewGuid()}.txt");
        try
        {
            var agg    = new Aggregator();
            var result = await agg.ComputeAsync(Known());
            await agg.WriteReportAsync(result, path);
            Assert.True(File.Exists(path), "TODO-6.2: WriteReportAsync did not create the file.");
            string content = await File.ReadAllTextAsync(path);
            Assert.Contains("Count", content);
            Assert.Contains("5",     content);
        }
        catch (NotImplementedException e) when (e.Message.Contains("6.1"))
        {
            Assert.Fail("TODO-6.2 requires TODO-6.1 first — implement ComputeAsync.");
        }
        catch (NotImplementedException)
        {
            Assert.Fail("TODO-6.2: implement WriteReportAsync using StreamWriter and WriteLineAsync.");
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }
}
