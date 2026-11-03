namespace StreamProcessor.Naive;
using StreamProcessor.Pipeline;
public sealed class SimpleBuffer<T> : IPipelineBuffer<T>
{
    private readonly Queue<T> _q=new(); private readonly object _lock=new();
    public int Capacity => int.MaxValue; public bool IsCompleted{get;private set;}
    public int Count{get{lock(_lock){return _q.Count;}}}
    public void Add(T item){lock(_lock){_q.Enqueue(item);}}
    public bool TryTake(out T item,int ms){lock(_lock){return _q.TryDequeue(out item!);}}
    public void Complete()=>IsCompleted=true;
}
