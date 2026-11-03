namespace StreamProcessor.Infrastructure;
using StreamProcessor.Models;
public sealed class DataGenerator
{
    private readonly Random _rng; private readonly string[] _ids = {"S01","S02","S03","S04","S05"};
    private int _nextId = 1;
    public DataGenerator(int seed = 42) => _rng = new Random(seed);
    public SensorReading Next() { double v = _rng.NextDouble()*200.0-50.0; return new SensorReading{Id=_nextId++,SensorId=_ids[_rng.Next(_ids.Length)],Value=Math.Round(v,2),Timestamp=DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}; }
    public List<SensorReading> GenerateBatch(int count) => Enumerable.Range(0,count).Select(_=>Next()).ToList();
}
