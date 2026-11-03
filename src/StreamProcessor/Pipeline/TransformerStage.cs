namespace StreamProcessor.Pipeline;
using StreamProcessor.Infrastructure;
using StreamProcessor.Models;

public sealed class TransformerStage
{
    public readonly object ResultStoreLock = new();

    /// <summary>True while the sequential fallback is still active.</summary>
    public bool FallbackActive { get; private set; }

    public void Run(IPipelineBuffer<SensorReading> validBuffer,
                    List<ProcessingResult>          resultStore)
    {
        FallbackActive = false;

        /*
         * ┌─────────────────────────────────────────────────────────────────┐
         * │ TODO-1.3  Create, start and join transformer threads  [5 pts ★] │
         * ├─────────────────────────────────────────────────────────────────┤
         * │ Same pattern as TODO-1.1 and 1.2, but for transformers.        │
         * │ Use PipelineConfig.TransformerThreadCount threads.             │
         * │ Each thread calls WorkerLoop(). Join all before returning.     │
         * └─────────────────────────────────────────────────────────────────┘
         */

        // ── FALLBACK — remove when TODO-1.3 is done ───────────────────────
        FallbackActive = true;
        WorkerLoop(validBuffer, resultStore, "Transformer-Sequential");
        // ── END FALLBACK ──────────────────────────────────────────────────
    }

    private void WorkerLoop(IPipelineBuffer<SensorReading> validBuffer,
                             List<ProcessingResult>          resultStore,
                             string threadName)
    {
        while (true)
        {
            if (validBuffer.TryTake(out var reading, timeoutMs: 50))
            {
                double norm = (reading.Value - PipelineConfig.MinValidValue)
                            / (PipelineConfig.MaxValidValue - PipelineConfig.MinValidValue);

                var result = new ProcessingResult
                {
                    OriginalId          = reading.Id,
                    SensorId            = reading.SensorId,
                    NormalizedValue     = norm,
                    ProcessedAt         = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    ProcessedByThreadId = Thread.CurrentThread.ManagedThreadId
                };

                /*
                 * ┌────────────────────────────────────────────────────┐
                 * │ TODO-2.2  Lock on shared resultStore    [7 pts ★]  │
                 * ├────────────────────────────────────────────────────┤
                 * │ List<T>.Add() is not thread-safe. Two transformer  │
                 * │ threads writing simultaneously can corrupt the list.│
                 * │ Use ResultStoreLock. Forbidden: ConcurrentBag      │
                 * └────────────────────────────────────────────────────┘
                 */
                resultStore.Add(result);   // ← replace this line

                ConcurrentLogger.Log(threadName, "TRANSFORMED",
                    $"Id={reading.Id,4} Norm={norm:F4}");
            }
            else if (validBuffer.IsCompleted && validBuffer.Count == 0)
            {
                ConcurrentLogger.Log(threadName, "EXIT", "done");
                return;
            }
        }
    }
}
