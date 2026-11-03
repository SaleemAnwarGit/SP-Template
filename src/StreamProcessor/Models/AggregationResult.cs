namespace StreamProcessor.Models;
public class AggregationResult
{
    public int Count { get; init; } public double Sum { get; init; }
    public double Min { get; init; } public double Max { get; init; }
    public double Average { get; init; }
}
