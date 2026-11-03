using System.Diagnostics;
using StreamProcessor;
using StreamProcessor.Infrastructure;
using StreamProcessor.Ipc;
using StreamProcessor.Models;
using StreamProcessor.Naive;
using StreamProcessor.Pipeline;
using StreamProcessor.Synchronization;

bool quiet   = args.Contains("--quiet");
bool pipe    = args.Contains("--pipe");
bool dead    = args.Contains("--deadlock");
bool safe    = args.Contains("--safe");

if (dead) { new ResourceManager().RunWithDeadlock(); return; }
if (safe) { new ResourceManager().RunSafely(); Console.WriteLine("Done."); return; }

// SWAP-1: after TODO-3.1 — comment SimpleBuffer lines, uncomment BoundedBuffer lines
IPipelineBuffer<SensorReading> inputBuffer = new SimpleBuffer<SensorReading>();
IPipelineBuffer<SensorReading> validBuffer = new SimpleBuffer<SensorReading>();
// IPipelineBuffer<SensorReading> inputBuffer = new BoundedBuffer<SensorReading>(PipelineConfig.BufferCapacity);
// IPipelineBuffer<SensorReading> validBuffer = new BoundedBuffer<SensorReading>(PipelineConfig.BufferCapacity);

var results     = new List<ProcessingResult>();
var valStage    = new ValidatorStage();
var trnStage    = new TransformerStage();
var aggregator  = new Aggregator();
var monitor     = new InvariantMonitor(inputBuffer, validBuffer, results, trnStage.ResultStoreLock);
monitor.Start();

Console.WriteLine("╔══════════════════════════════════════╗");
Console.WriteLine("║     StreamProcessor — starting       ║");
Console.WriteLine("╚══════════════════════════════════════╝");
var sw = Stopwatch.StartNew();

Thread genThread;
if (pipe)
    genThread = new Thread(() => new PipelineReceiver().Receive(inputBuffer)) { Name = "PipeReceiver" };
else
    genThread = new Thread(() => {
        var gen = new DataGenerator(42);
        foreach (var r in gen.GenerateBatch(200))
        {
            inputBuffer.Add(r);
            if (!quiet) ConcurrentLogger.Log("Generator","ADDED",$"Id={r.Id,4} Value={r.Value,7:F2}  valid={r.IsValid}");
        }
        inputBuffer.Complete();
        ConcurrentLogger.Log("Generator","DONE","200 dispatched");
    }) { Name = "Generator" };

var trnTask = Task.Run(() => trnStage.Run(validBuffer, results));
genThread.Start();
valStage.Run(inputBuffer, validBuffer);
validBuffer.Complete();
genThread.Join();
await trnTask;
monitor.Stop();

// SWAP-2: after TODO-6.1 + 6.2 — comment Sync lines, uncomment Async lines
var result = aggregator.ComputeSync(results);
aggregator.WriteReportSync(result, "report.txt");
// var result = await aggregator.ComputeAsync(results);
// await aggregator.WriteReportAsync(result, "report.txt");

sw.Stop();
Console.WriteLine($"\n  Generated : 200\n  Valid     : {result.Count}\n  Invalid   : {valStage.InvalidCount}\n  Check     : {result.Count + valStage.InvalidCount} == 200\n  Avg       : {result.Average:F6}\n  Violations: {monitor.ViolationCount}\n  Elapsed   : {sw.ElapsedMilliseconds} ms");
