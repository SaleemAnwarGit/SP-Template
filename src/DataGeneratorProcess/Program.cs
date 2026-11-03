// DataGeneratorProcess — Stage 7 companion
// Run: dotnet run --project src/DataGeneratorProcess
// (Start the pipeline with --pipe first, then run this)

using System.IO.Pipes;
using System.Text.Json;

/*
 * ┌─────────────────────────────────────────────────────────────────────────┐
 * │ TODO-7.2  Named pipe client                                  [5 pts]   │
 * ├─────────────────────────────────────────────────────────────────────────┤
 * │ 1. Connect as a named pipe CLIENT to "StreamProcessorPipe".           │
 * │ 2. Generate 100 SensorReadings using Random(seed:42) — same logic    │
 * │    as DataGenerator: value = NextDouble()*200-50, 5 sensor IDs.      │
 * │ 3. Write each as one JSON line and flush.                             │
 * │ 4. Close the connection when done.                                    │
 * │ Hint: the client class is NamedPipeClientStream.                      │
 * └─────────────────────────────────────────────────────────────────────────┘
 */

throw new NotImplementedException("TODO-7.2: implement this Program.cs");
