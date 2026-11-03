namespace StreamProcessor.Models;
public class SensorReading
{
    public int    Id        { get; init; }
    public string SensorId  { get; init; } = string.Empty;
    public double Value     { get; init; }
    public long   Timestamp { get; init; }
    public bool   IsValid   => Value >= PipelineConfig.MinValidValue && Value <= PipelineConfig.MaxValidValue;
}
