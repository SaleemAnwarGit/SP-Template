namespace StreamProcessor.Models;
public class ProcessingResult
{
    public int    OriginalId          { get; init; }
    public string SensorId            { get; init; } = string.Empty;
    public double NormalizedValue     { get; init; }
    public long   ProcessedAt         { get; init; }
    public int    ProcessedByThreadId { get; init; }
}
