namespace StreamProcessor.Ipc;
using System.IO.Pipes;
using System.Text.Json;
using StreamProcessor.Infrastructure;
using StreamProcessor.Models;
using StreamProcessor.Pipeline;

public sealed class PipelineReceiver
{
    /*
     * ┌─────────────────────────────────────────────────────────────────────┐
     * │ TODO-7.1  Named pipe server                             [5 pts]    │
     * ├─────────────────────────────────────────────────────────────────────┤
     * │ 1. Create a NamedPipeServerStream on PipelineConfig.PipeName.     │
     * │ 2. Call WaitForConnection() — blocks until the client connects.   │
     * │ 3. Wrap a StreamReader around the stream.                         │
     * │ 4. Loop: read a line, deserialise with JsonSerializer, Add to buf.│
     * │ 5. Call inputBuffer.Complete() when the loop ends (client closed).│
     * │ Hint: the classes are NamedPipeServerStream and StreamReader.     │
     * └─────────────────────────────────────────────────────────────────────┘
     */
    public void Receive(IPipelineBuffer<SensorReading> inputBuffer) =>
        throw new NotImplementedException("TODO-7.1");
}
