namespace StreamProcessor;
public static class PipelineConfig
{
    public const int    BufferCapacity         = 10;
    public const int    ValidatorThreadCount   = 2;
    public const int    TransformerThreadCount = 2;
    public const double MinValidValue          = -10.0;
    public const double MaxValidValue          = 100.0;
    public const string PipeName              = "StreamProcessorPipe";
}
