# StreamProcessor — Concurrency Assignment

**Hogeschool Rotterdam · Concurrency in C# · .NET 10**

---

## Getting Started

```bash
git clone https://github.com/<your-username>/streamprocessor-assignment.git
cd streamprocessor-assignment
dotnet restore
dotnet run --project src/StreamProcessor -- --quiet
```

Expected output: `Valid: 111 · Invalid: 89 · Violations: 0`

If you see this, your environment is correct and you can start working on the TODOs.

```bash
# Run tests at any time to see your progress
dotnet test

# Run one stage at a time
dotnet test --filter Stage3
```

---

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

```bash
dotnet run --project src/StreamProcessor -- --quiet   # see the pipeline run
dotnet test --filter Stage3                           # test one stage
dotnet test                                           # test everything
```

---

## TODO Map

| TODO| Description | File | Points | Required |
|------|------|------|--------|---------|
| 1.1 | Create validator threads | `ValidatorStage.cs` | 5 | ★ |
| 1.2 | Start and join validator threads | `ValidatorStage.cs` | 5 | ★ |
| 1.3 | Create, start, join transformer threads | `TransformerStage.cs` | 5 | ★ |
| 2.1 | Lock on shared invalid counter | `ValidatorStage.cs` | 8 | ★ |
| 2.2 | Lock on shared result list | `TransformerStage.cs` | 7 | ★ |
| 3.1 | BoundedBuffer — blocking queue | `BoundedBuffer.cs` | 20 | ★ |
| 4.1 | Add two SemaphoreSlim fields | `BoundedBuffer.cs` | 7 | |
| 4.2 | Replace Monitor with SemaphoreSlim | `BoundedBuffer.cs` | 8 | |
| 5.1 | Create a deadlock | `ResourceManager.cs` | 7 | |
| 5.2 | Fix the deadlock | `ResourceManager.cs` | 8 | |
| 6.1 | Async aggregation with Task.Run | `Aggregator.cs` | 8 | |
| 6.2 | Async file write with StreamWriter | `Aggregator.cs` | 7 | |
| 7.1 | Named pipe server (receiver) | `PipelineReceiver.cs` | 5 | |
| 7.2 | Named pipe client (generator process) | `DataGeneratorProcess/Program.cs` | 5 | |

★ = Required. Missing any ★ TODO = automatic fail, regardless of interview score.

---

## Before / After

After each TODO the application behaves visibly differently.

| After TODO | What changes |
|------------|-------------|
| 1.1 + 1.2 + 1.3 | Log shows `[T08] Validator-1` and `[T09] Validator-2` interleaving instead of one sequential thread |
| 2.1 + 2.2 | `Invalid: 89` is the same value on every run instead of varying |
| 3.1 + SWAP-1 | Timestamps show ~50 ms gaps — the buffer is throttling the generator |
| 4.1 + 4.2 | CPU stays near 0 % while the buffer is full (true sleep, no busy-wait) |
| 5.1 | `dotnet run -- --deadlock` hangs forever |
| 5.2 | `dotnet run -- --safe` completes in under 100 ms |
| 6.1 + 6.2 + SWAP-2 | `report.txt` is created after every run |
| 7.1 + 7.2 | Two terminals, one pipeline, data crosses a process boundary |

---

## Swap Points in Program.cs

Two lines in `Program.cs` are marked `SWAP-1` and `SWAP-2`.
These are the **only** lines you change in that file.

- Apply **SWAP-1** after completing TODO-3.1 — switches from `SimpleBuffer` to your `BoundedBuffer`
- Apply **SWAP-2** after completing TODO-6.1 and 6.2 — switches from sync to async aggregation

---

## Test Reference

All 24 tests are named to match their TODO number.

```
dotnet test                       # all 24
dotnet test --filter Stage1       # T1.1, T1.2, T1.3
dotnet test --filter Stage2       # T2.1, T2.2
dotnet test --filter Stage3       # T3.1a through T3.1f
dotnet test --filter Stage4       # T4.1, T4.2
dotnet test --filter Stage5       # T5.1, T5.2a, T5.2b
dotnet test --filter Stage6       # T6.1a through T6.1e, T6.2
dotnet test --filter Stage7       # T7.1, T7.2
```

On the unmodified template: **24 failed, 0 passed**.
On a complete solution: **0 failed, 24 passed**.

---

## Grading

### Code — 70 % of final grade

| Stage | TODO | Description | Points | Required |
|-------|------|-------------|--------|---------|
| 1 — Threads | 1.1 | Create validator threads | 5 | ★ |
| | 1.2 | Start and join validator threads | 5 | ★ |
| | 1.3 | Create, start, join transformer threads | 5 | ★ |
| 2 — Locks | 2.1 | Lock on shared invalid counter | 8 | ★ |
| | 2.2 | Lock on shared result list | 7 | ★ |
| 3 — Buffer | 3.1 | BoundedBuffer blocking queue | 20 | ★ |
| 4 — Semaphores | 4.1 | Two SemaphoreSlim fields | 7 | |
| | 4.2 | Replace Monitor with SemaphoreSlim | 8 | |
| 5 — Deadlock | 5.1 | Create a deadlock | 7 | |
| | 5.2 | Fix the deadlock | 8 | |
| 6 — Async | 6.1 | Async aggregation with Task.Run | 8 | |
| | 6.2 | Async file write with StreamWriter | 7 | |
| 7 — IPC | 7.1 | Named pipe server | 5 | |
| | 7.2 | Named pipe client | 5 | |
| | **Total** | | **100 × 0.70** | |

Score is determined by `dotnet test` — each test maps directly to one TODO number.

### Interview — 30 % of final grade

15 minutes per student. You will be asked to explain your implementation,
trace through a scenario on paper, and justify your design choices.

| Stage | Sample questions |
|-------|----------------|
| 1 — Threads | Why must you call `Join()`? What happens if `Main()` exits first? |
| 2 — Locks | Show the two instructions that can interleave to corrupt the count. |
| 3 — Buffer | Trace `Add()` step by step when the buffer is full. |
| 4 — Semaphores | Why two semaphores and not one? What would break with only one? |
| 5 — Deadlock | At which exact line does each thread stop? Why can neither continue? |
| 6 — Async | Where does execution go during an `await`? Which thread resumes after? |
| 7 — IPC | What happens if the generator process crashes mid-stream? |

### Passing Threshold

```
Final grade = (code / 100) × 0.70 + (interview / 30) × 0.30

Pass requires:  code ≥ 50 pts  (all ★ TODOs)
           AND  interview ≥ 15 / 30
```

Both components must independently meet their threshold.
A perfect code score with a failed interview is an overall fail.

---

## Submission

1. Complete `CHECKLIST.md` — tick every TODO you finished and fill in your final test results.
2. Zip your solution folder, excluding `bin/` and `obj/` directories.
3. Name the file `<StudentNumber>_StreamProcessor.zip`.
4. Submit via the course assignment (teams) before the deadline.

---

## Pipeline Constants

Do not change these values — the test suite depends on them.

| Constant | Value | Meaning |
|----------|-------|---------|
| `BufferCapacity` | 10 | Maximum items in each buffer |
| `ValidatorThreadCount` | 2 | Number of validator threads |
| `TransformerThreadCount` | 2 | Number of transformer threads |
| `MinValidValue` | −10.0 | Lower bound for a valid reading |
| `MaxValidValue` | 100.0 | Upper bound for a valid reading |
| `PipeName` | `StreamProcessorPipe` | Named pipe identifier for Stage 7 |

---

## Key Invariants

These must hold at all times — not just at the end.
The `InvariantMonitor` checks them every 10 ms during integration tests.

1. `validCount + invalidCount == totalGenerated`
2. `resultStore.Count == validCount`
3. `0.0 ≤ NormalizedValue ≤ 1.0` for every result
4. `inputBuffer.Count ≤ BufferCapacity` at all times
5. `validBuffer.Count ≤ BufferCapacity` at all times