namespace StreamProcessor.Pipeline;
public interface IPipelineBuffer<T>
{
    void Add(T item); bool TryTake(out T item, int timeoutMs);
    void Complete(); int Count { get; } int Capacity { get; } bool IsCompleted { get; }
}
