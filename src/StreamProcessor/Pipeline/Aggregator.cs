namespace StreamProcessor.Pipeline;
using StreamProcessor.Models;

public sealed class Aggregator
{
    // Sequential fallback — works until SWAP-2 in Program.cs
    public AggregationResult ComputeSync(List<ProcessingResult> results)
    {
        if (results.Count == 0) return new AggregationResult();
        double sum=0,min=double.MaxValue,max=double.MinValue;
        foreach(var r in results){sum+=r.NormalizedValue;if(r.NormalizedValue<min)min=r.NormalizedValue;if(r.NormalizedValue>max)max=r.NormalizedValue;}
        return new AggregationResult{Count=results.Count,Sum=sum,Min=min,Max=max,Average=sum/results.Count};
    }
    public void WriteReportSync(AggregationResult r, string path) =>
        File.WriteAllText(path,$"Count:{r.Count}\nSum:{r.Sum:F6}\nMin:{r.Min:F6}\nMax:{r.Max:F6}\nAvg:{r.Average:F6}\n");

    /*
     * ┌─────────────────────────────────────────────────────────────────────┐
     * │ TODO-6.1  Async aggregation                             [8 pts]    │
     * ├─────────────────────────────────────────────────────────────────────┤
     * │ ComputeAsync must use async/await and Task.Run.                    │
     * │ The computation logic is the same as ComputeSync above.            │
     * │ The method must return Task<AggregationResult>.                    │
     * │ Hint: what keyword offloads work to the thread pool?               │
     * └─────────────────────────────────────────────────────────────────────┘
     *
     * ┌─────────────────────────────────────────────────────────────────────┐
     * │ TODO-6.2  Async file write                              [7 pts]    │
     * ├─────────────────────────────────────────────────────────────────────┤
     * │ WriteReportAsync must use StreamWriter and WriteLineAsync.         │
     * │ Every file write must be awaited. Return type: Task.               │
     * └─────────────────────────────────────────────────────────────────────┘
     */
    public async Task<AggregationResult> ComputeAsync(List<ProcessingResult> results)
    {
        await Task.CompletedTask;
        throw new NotImplementedException("TODO-6.1");
    }

    public async Task WriteReportAsync(AggregationResult result, string filePath)
    {
        await Task.CompletedTask;
        throw new NotImplementedException("TODO-6.2");
    }
}
