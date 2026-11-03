namespace StreamProcessor.Infrastructure;
public static class ConcurrentLogger
{
    private static readonly object _lock = new();
    public static void Log(string actor, string action, string detail)
    { string ts=DateTime.Now.ToString("HH:mm:ss.fff"); int tid=Thread.CurrentThread.ManagedThreadId; lock(_lock) Console.WriteLine($"[{ts}] [T{tid:D2}] {actor,-16} | {action,-10} | {detail}"); }
}
