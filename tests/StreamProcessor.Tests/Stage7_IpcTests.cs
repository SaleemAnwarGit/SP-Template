namespace StreamProcessor.Tests;
using System.IO.Pipes;
using System.Text.Json;
using Xunit;
using StreamProcessor.Infrastructure;
using StreamProcessor.Models;
using StreamProcessor.Naive;
using StreamProcessor.Pipeline;

/// <summary>
/// Tests for TODO-7.1 (PipelineReceiver — server side) and
/// TODO-7.2 (DataGeneratorProcess logic — client side).
///
/// A mock sender (RunMockSender) replaces the real DataGeneratorProcess
/// so the test is self-contained — no second project to launch.
/// </summary>
public class Stage7_IpcTests
{
    /// <summary>
    /// Simulates the DataGeneratorProcess client inside the test.
    /// Returns true if the server was listening and accepted the connection.
    /// Returns false if the server never started (TODO-7.1 not done).
    /// </summary>
    private static bool RunMockSender(int count)
    {
        var ids = new[]{"S01","S02","S03","S04","S05"};
        var rng = new Random(42); int nextId = 1;
        try
        {
            using var client = new NamedPipeClientStream(".", PipelineConfig.PipeName, PipeDirection.Out);
            client.Connect(2_000);
            using var w = new StreamWriter(client){AutoFlush=true};
            for (int i = 0; i < count; i++)
            {
                double v = Math.Round(rng.NextDouble()*200.0-50.0, 2);
                string sid = ids[rng.Next(ids.Length)];
                w.WriteLine(JsonSerializer.Serialize(new{
                    Id=nextId++,SensorId=sid,Value=v,
                    Timestamp=DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}));
            }
            return true;
        }
        catch (TimeoutException) { return false; }
    }

    // ── T7.1 ──────────────────────────────────────────────────────────────
    // What it checks: PipelineReceiver.Receive() opens a server pipe, waits
    //                 for the sender to connect, reads exactly N JSON lines,
    //                 and calls inputBuffer.Complete() when done.
    // Hint: the server-side pipe class is NamedPipeServerStream.
    //       Use PipeDirection.In and call WaitForConnection().
    //       Wrap it with StreamReader and loop on ReadLine().
    [Fact] public void T7_1_PipelineReceiver_ReceivesAllReadings()
    {
        const int N  = 100;
        var inputBuf = new SimpleBuffer<SensorReading>();
        Exception? receiverEx = null;

        var receiver = new Thread(() => {
            try   { new Ipc.PipelineReceiver().Receive(inputBuf); }
            catch (Exception ex) { receiverEx = ex; inputBuf.Complete(); }
        }) { Name = "PipeReceiver", IsBackground = true };

        var sender = new Thread(() => RunMockSender(N)) { IsBackground = true };

        receiver.Start();
        sender.Start();
        receiver.Join(8_000);
        sender.Join(2_000);

        if (receiverEx is NotImplementedException)
            Assert.Fail("TODO-7.1: PipelineReceiver.Receive() is not implemented. " +
                        "Open a NamedPipeServerStream, call WaitForConnection(), " +
                        "read JSON lines, deserialise to SensorReading, Add to buffer.");

        if (receiverEx is not null)
            Assert.Fail($"PipelineReceiver threw: {receiverEx.Message}");

        int received = 0;
        while (inputBuf.TryTake(out _, 50)) received++;
        Assert.Equal(N, received);
    }

    // ── T7.2 ──────────────────────────────────────────────────────────────
    // What it checks: the full pipeline runs in pipe mode and satisfies all
    //                 five invariants (count, normalised values, no violations).
    //                 The mock sender acts as DataGeneratorProcess.
    // Hint: this test passes automatically once T7.1 passes and your
    //       ValidatorStage + TransformerStage implementations are correct.
    [Fact] public async Task T7_2_FullPipeline_InPipeMode_InvariantsHold()
    {
        const int N  = 100;
        var inputBuf = new SimpleBuffer<SensorReading>();
        var validBuf = new SimpleBuffer<SensorReading>();
        var results  = new List<ProcessingResult>();
        var valStage = new ValidatorStage();
        var trnStage = new TransformerStage();
        var monitor  = new InvariantMonitor(inputBuf,validBuf,results,trnStage.ResultStoreLock);
        monitor.Start();

        bool       senderOk   = false;
        Exception? receiverEx = null;

        var receiver = new Thread(() => {
            try   { new Ipc.PipelineReceiver().Receive(inputBuf); }
            catch (Exception ex) { receiverEx = ex; inputBuf.Complete(); }
        }) { Name = "PipeReceiver", IsBackground = true };

        var sender = new Thread(() => { senderOk = RunMockSender(N); })
            { IsBackground = true };

        var trnTask = Task.Run(() => trnStage.Run(validBuf, results));

        receiver.Start();
        sender.Start();
        valStage.Run(inputBuf, validBuf);
        validBuf.Complete();
        receiver.Join(8_000);
        await trnTask;
        monitor.Stop();

        if (receiverEx is NotImplementedException)
            Assert.Fail("TODO-7.1: implement PipelineReceiver.Receive() first.");
        if (receiverEx is not null)
            Assert.Fail($"PipelineReceiver threw: {receiverEx.Message}");
        if (!senderOk)
            Assert.Fail("TODO-7.1: sender timed out — the pipe server never opened.");

        Assert.Equal(N, results.Count + valStage.InvalidCount);
        Assert.All(results, r => Assert.InRange(r.NormalizedValue, 0.0, 1.0));
        Assert.Equal(0, monitor.ViolationCount);
    }
}
