# Submission Checklist — StreamProcessor

**Student number:** ________________________________

**Submission date:** ________________________________

**dotnet version:** ________________________________

---

## Completed TODOs

Tick every TODO you have fully implemented and tested.

| TODO | Description | Done | Points | Test result |
|------|-------------|------|--------|-------------|
| 1.1 | Create validator threads | ☐ | 5 ★ | |
| 1.2 | Start and join validator threads | ☐ | 5 ★ | |
| 1.3 | Create, start, join transformer threads | ☐ | 5 ★ | |
| 2.1 | Lock on shared invalid counter | ☐ | 8 ★ | |
| 2.2 | Lock on shared result list | ☐ | 7 ★ | |
| 3.1 | BoundedBuffer blocking queue | ☐ | 20 ★ | |
| 4.1 | Two SemaphoreSlim fields | ☐ | 7 | |
| 4.2 | Replace Monitor with SemaphoreSlim | ☐ | 8 | |
| 5.1 | Create a deadlock | ☐ | 7 | |
| 5.2 | Fix the deadlock | ☐ | 8 | |
| 6.1 | Async aggregation with Task.Run | ☐ | 8 | |
| 6.2 | Async file write with StreamWriter | ☐ | 7 | |
| 7.1 | Named pipe server | ☐ | 5 | |
| 7.2 | Named pipe client | ☐ | 5 | |
| **Total** | | | **100** | |

★ = Required. Missing any ★ = automatic fail.

---

## Final Test Run

Fill in from your last `dotnet test` run.

| Metric | Value |
|--------|-------|
| Tests passed | / 24 |
| Tests failed | |
| Total elapsed | ms |

Fill in from your last `dotnet run --project src/StreamProcessor --quiet` run.

| Metric | Value |
|--------|-------|
| Generated | / 200 |
| Valid | |
| Invalid | |
| Valid + Invalid == 200 | ☐ Yes ☐ No |
| Invariant violations | (must be 0) |
| Elapsed | ms |

---

## Swap Points Applied

| Swap | Applied | After which TODO |
|------|---------|-----------------|
| SWAP-1 in Program.cs (SimpleBuffer → BoundedBuffer) | ☐ Yes ☐ No | 3.1 |
| SWAP-2 in Program.cs (sync → async aggregation) | ☐ Yes ☐ No | 6.1 + 6.2 |

---

## Design Choices

Describe exactly **two** choices you made in your implementation.
What did you choose, what was the alternative, and why did you choose this way?
Maximum 100 words each.

### Choice 1 — [give it a short title]

> *Your answer here*

---

### Choice 2 — [give it a short title]

> *Your answer here*

---

## Interview Preparation

Rate your own confidence for each stage (1 = not confident, 5 = very confident).

| Stage | Confidence (1–5) |
|-------|-----------------|
| 1 — Threads | |
| 2 — Locks | |
| 3 — BoundedBuffer | |
| 4 — Semaphores | |
| 5 — Deadlock | |
| 6 — Async | |
| 7 — IPC | |

---

## Declaration

I confirm that this submission is my own work.

**Signature:** ________________________________
