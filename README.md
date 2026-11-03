# StreamProcessor — Concurrency Assignment
**Hogeschool Rotterdam · Concurrency in C# · .NET 10**

## Assignment Description

### Scenario

A network of weather stations sends temperature readings to a central hub.
Readings arrive faster than one thread can handle; some are faulty and must
be rejected; valid readings must be normalised and summarised.

You will build the concurrent pipeline that handles this efficiently —
starting from a working sequential baseline and upgrading it stage by stage.

### What the Application Does

<pre>
Generator → [InputBuffer] → Validator-1 ─┐
                            Validator-2 ─┘→ [ValidBuffer] → Transformer-1 ─┐
                                                            Transformer-2 ─┘→ ResultStore → Aggregator
</pre>

- **Generator** produces 200 sensor readings (temperature −50 to 150 °C)
- **Validators** reject out-of-range readings (below −10 or above 100)
- **Transformers** normalise valid readings to the 0.0 – 1.0 scale
- **Aggregator** computes count, sum, min, max and average

### Your Task

The application already runs using sequential fallbacks.
Complete the 14 TODOs to replace each fallback with a proper concurrent
implementation — one stage at a time.
After each TODO, run the app and the tests to see what changed.

## Getting Started

```bash
git clone https://github.com/<your-username>/SP-Template.git
cd SP-Template
dotnet restore
dotnet run --project src/StreamProcessor -- --quiet
```

Expected output: `Valid: 111 · Invalid: 89 · Violations: 0`
If you see this, your environment is correct and you can start working on the TODOs.

```bash
# Run tests at any time to see your progress
dotnet test
```

## Quick Start

```bash
dotnet restore
dotnet run --project src/StreamProcessor -- --quiet
```

You see the pipeline running with naive fallbacks. Every TODO you complete changes the output.

---

## TODO Map

| TODO | File | Points | Required |
|------|------|--------|---------|
| 1.1 Create validator threads | ValidatorStage.cs | 5 | ★ |
| 1.2 Start and join validator threads | ValidatorStage.cs | 5 | ★ |
| 1.3 Create, start, join transformer threads | TransformerStage.cs | 5 | ★ |
| 2.1 Lock on shared invalidCount | ValidatorStage.cs | 8 | ★ |
| 2.2 Lock on shared resultStore | TransformerStage.cs | 7 | ★ |
| 3.1 BoundedBuffer core (Queue + blocking) | BoundedBuffer.cs | 20 | ★ |
| 4.1 Two SemaphoreSlim fields | BoundedBuffer.cs | 7 | |
| 4.2 Replace Monitor with SemaphoreSlim | BoundedBuffer.cs | 8 | |
| 5.1 Create a deadlock | ResourceManager.cs | 7 | |
| 5.2 Fix the deadlock | ResourceManager.cs | 8 | |
| 6.1 ComputeAsync with Task.Run | Aggregator.cs | 8 | |
| 6.2 WriteReportAsync with StreamWriter | Aggregator.cs | 7 | |
| 7.1 Named pipe server | PipelineReceiver.cs | 5 | |
| 7.2 Named pipe client | DataGeneratorProcess/Program.cs | 5 | |

★ = Required. All ★ TODOs must pass to pass the assignment.

---

## Test Reference

Run all: `dotnet test`
Run one stage: `dotnet test --filter Stage3`

**On the template: all 24 tests fail. On the full solution: all 24 pass.**
Each test name matches its TODO number exactly.

### Stage 1 — Threads (Tests T1.1, T1.2, T1.3)

| Test | Checks | Fails when |
|------|--------|-----------|
| T1.1 | ValidatorStage uses real threads, not the fallback | FallbackActive is still true |
| T1.2 | TransformerStage threads are started AND joined | FallbackActive true, or no Join |
| T1.3 | TransformerStage creates the right number of threads | FallbackActive true |

**Before:** log shows `[Validator-Sequential]` — one thread does everything.
**After:** log shows `[T08] Validator-1` and `[T09] Validator-2` interleaving.

*Hint: the class is `Thread`. How do you give it a task? How do you make the caller wait for it?*

---

### Stage 2 — Locks (Tests T2.1, T2.2)

| Test | Checks | Fails when |
|------|--------|-----------|
| T2.1 | InvalidCount is exactly correct in 20 consecutive runs | No lock on _invalidCount |
| T2.2 | resultStore.Count is exactly correct in 20 consecutive runs | No lock on resultStore.Add |

**Before:** count varies between runs — race condition visible.
**After:** count is always the same exact value.

*Hint: what C# keyword creates a section that only one thread enters at a time?*

---

### Stage 3 — BoundedBuffer (Tests T3.1a through T3.1f)

| Test | Checks |
|------|--------|
| T3.1a | Add then Take returns the same item |
| T3.1b | Count reflects current occupancy |
| T3.1c | Add blocks the caller for at least 100 ms when full |
| T3.1d | TryTake returns false after the timeout expires |
| T3.1e | All items reach consumers with multiple producers and consumers |
| T3.1f | Complete() + TryTake() lets consumers exit cleanly |

**Before:** `new BoundedBuffer(10)` throws NotImplementedException.
**After:** timestamps in the log show ~50 ms gaps — the buffer is throttling the generator.

*Hint: you need a Queue<T>, a lock object, and a way to make a thread wait and wake up.*

---

### Stage 4 — Semaphores (Tests T4.1, T4.2)

| Test | Checks |
|------|--------|
| T4.1 | BoundedBuffer has exactly two SemaphoreSlim private fields (reflection) |
| T4.2 | A blocked producer unblocks within 100 ms after a consumer calls Take() |

*Hint for T4.1: what is the .NET type that can count available resources and block threads?*
*Hint for T4.2: after dequeueing, what must you call to wake a waiting producer?*

---

### Stage 5 — Deadlock (Tests T5.1, T5.2a, T5.2b)

| Test | Checks |
|------|--------|
| T5.1 | RunWithDeadlock() does NOT finish within 3 seconds (hang = pass) |
| T5.2a | RunSafely() DOES finish within 3 seconds |
| T5.2b | RunSafely() finishes correctly 5 times in a row |

**Deadlock demo:** `dotnet run -- --deadlock` (hangs — Ctrl+C to exit)
**Safe demo:** `dotnet run -- --safe`

*Hint for T5.1: what word describes two threads each waiting for something the other holds?*
*Hint for T5.2: change exactly one word in the lock acquisition to break the cycle.*

---

### Stage 6 — Async (Tests T6.1a–T6.1e, T6.2)

| Test | Checks |
|------|--------|
| T6.1a | ComputeAsync returns correct Count |
| T6.1b | ComputeAsync returns correct Sum |
| T6.1c | ComputeAsync returns correct Min |
| T6.1d | ComputeAsync returns correct Max |
| T6.1e | ComputeAsync returns correct Average |
| T6.2 | WriteReportAsync creates a file containing the count |

**After SWAP-2 in Program.cs:** a report.txt appears after each run.

*Hint for T6.1: what keyword offloads work from the current thread to the thread pool?*
*Hint for T6.2: which StreamWriter method has Async in its name?*

---

### Stage 7 — Named Pipe IPC (Tests T7.1, T7.2)

| Test | Checks |
|------|--------|
| T7.1 | PipelineReceiver receives exactly 100 JSON readings from the mock sender |
| T7.2 | Full pipe-mode pipeline satisfies all five invariants |

**Run manually:**
```bash
# Terminal 1
dotnet run --project src/StreamProcessor -- --pipe

# Terminal 2
dotnet run --project src/DataGeneratorProcess
```

*Hint for T7.1: the server-side pipe class is NamedPipeServerStream. It needs PipeDirection.In.*
*Hint for T7.2: this passes automatically once T7.1 is correct.*

---

## Grading

Code × 0.70 + Interview × 0.30 = Final grade.
Pass threshold: code ≥ 50 pts (all ★ TODOs) AND interview ≥ 15/30.

## Swap Points in Program.cs

Two comments marked `// SWAP-1` and `// SWAP-2` are the only lines you change in Program.cs.
Apply SWAP-1 after TODO-3.1. Apply SWAP-2 after TODO-6.1 and 6.2.

## Submission

Zip your solution (exclude bin/ and obj/).
Name: `<StudentNumber>_StreamProcessor.zip`
Include a completed `CHECKLIST.md`.
