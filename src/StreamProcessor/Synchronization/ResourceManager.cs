namespace StreamProcessor.Synchronization;
using StreamProcessor.Infrastructure;

public sealed class ResourceManager
{
    private readonly object _deviceA = new();
    private readonly object _deviceB = new();

    /*
     * ┌─────────────────────────────────────────────────────────────────────┐
     * │ TODO-5.1  Create a deadlock                             [7 pts]    │
     * ├─────────────────────────────────────────────────────────────────────┤
     * │ Create two background threads. Thread-1 acquires _deviceA first,  │
     * │ then _deviceB. Thread-2 acquires _deviceB first, then _deviceA.   │
     * │ Add Thread.Sleep(50) after the first acquisition in each thread.  │
     * │ Start both. Call Join() on both. The method must hang forever.     │
     * │ Hint: what is the word for two threads each waiting for the other? │
     * └─────────────────────────────────────────────────────────────────────┘
     *
     * ┌─────────────────────────────────────────────────────────────────────┐
     * │ TODO-5.2  Fix the deadlock                              [8 pts]    │
     * ├─────────────────────────────────────────────────────────────────────┤
     * │ Both threads must need both devices. Fix by ensuring a consistent │
     * │ acquisition order. The method must complete within 3 seconds.     │
     * │ Hint: the fix is one word — make both threads agree on which      │
     * │ device comes first.                                                │
     * └─────────────────────────────────────────────────────────────────────┘
     */
    public void RunWithDeadlock() => throw new NotImplementedException("TODO-5.1");
    public void RunSafely()       => throw new NotImplementedException("TODO-5.2");
}
