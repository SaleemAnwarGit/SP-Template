namespace StreamProcessor.Pipeline;
using StreamProcessor.Infrastructure;
using StreamProcessor.Models;

public sealed class ValidatorStage
{
    private int    _invalidCount;
    private object _invalidLock = new();

    public int  InvalidCount   => _invalidCount;

    /// <summary>
    /// True while the sequential fallback is still running.
    /// Becomes false when you implement TODO-1.1 and remove the fallback block.
    /// Tests check this to verify real threads are in use.
    /// </summary>
    public bool FallbackActive { get; private set; }

    public void Run(IPipelineBuffer<SensorReading> inputBuffer,
                    IPipelineBuffer<SensorReading> validBuffer)
    {
        _invalidCount  = 0;
        FallbackActive = false;   // will be set to true if fallback runs

        /*
         * ┌─────────────────────────────────────────────────────────────────┐
         * │ TODO-1.1  Create threads                              [5 pts ★] │
         * │ TODO-1.2  Start all threads, then Join all threads    [5 pts ★] │
         * ├─────────────────────────────────────────────────────────────────┤
         * │ Create PipelineConfig.ValidatorThreadCount Thread objects.      │
         * │ Each thread must execute WorkerLoop().                          │
         * │ Run() must not return until every thread has finished.          │
         * │ Forbidden: Task.Run, ThreadPool, Parallel.For                  │
         * └─────────────────────────────────────────────────────────────────┘
         */

        // ── FALLBACK — remove this block when TODO-1.1 and 1.2 are done ──
        FallbackActive = true;   // keeps FallbackActive = true while fallback runs
        WorkerLoop(inputBuffer, validBuffer, "Validator-Sequential");
        // ── END FALLBACK ──────────────────────────────────────────────────
    }

    private void WorkerLoop(IPipelineBuffer<SensorReading> inputBuffer,
                             IPipelineBuffer<SensorReading> validBuffer,
                             string threadName)
    {
        while (true)
        {
            if (inputBuffer.TryTake(out var reading, timeoutMs: 50))
            {
                if (reading.IsValid)
                {
                    validBuffer.Add(reading);
                    ConcurrentLogger.Log(threadName, "VALID",
                        $"Id={reading.Id,4} Value={reading.Value,7:F2}");
                }
                else
                {
                    /*
                     * ┌────────────────────────────────────────────────────┐
                     * │ TODO-2.1  Lock on shared invalidCount   [8 pts ★]  │
                     * ├────────────────────────────────────────────────────┤
                     * │ Two threads write to _invalidCount at the same     │
                     * │ time. Without protection, the final count is wrong. │
                     * │ Wrap the increment with a lock statement.           │
                     * │ Forbidden: Interlocked, ConcurrentDictionary       │
                     * └────────────────────────────────────────────────────┘
                     */
                    _invalidCount++;   // ← replace this line
                    ConcurrentLogger.Log(threadName, "REJECTED",
                        $"Id={reading.Id,4} Value={reading.Value,7:F2}");
                }
            }
            else if (inputBuffer.IsCompleted && inputBuffer.Count == 0)
            {
                ConcurrentLogger.Log(threadName, "EXIT", "done");
                return;
            }
        }
    }
}
