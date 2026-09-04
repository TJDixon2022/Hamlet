READ IN THIS ORDER

A. THE PHASE GOAL. Hamlet hears FT8 off the radio and displays the decoded text
on screen. Nothing on screen is closer tonight. What is closer is that the path
carrying the audio to the decoder can no longer be interrupted by anything that
reads it, and where a callback does run past the time the device gives it, that
is now a number rather than a suspicion.

B. THIS STEP AND ITS EXIT CRITERIA. Unit 238 took arrival on the shack machine
from 13% to 76% by moving the CW decode off the device callback. 76% is still a
collage. This unit went after the suspect the instruction named: that the tap's
own readers were holding the lock the callback needs, and allocating megabytes
on the way past.

C. WHAT THIS REPORT ADDS, AND WHETHER IT BEARS ON A OR B. It bears on B, and it
does not close it. The suspect was real and is now measured rather than argued:
a reader hammering the tap cost the writer **117,046 microseconds** before this
unit and costs it **59** after. But 117,046 was measured by a test written to
provoke it, not by the shack machine, and **whether the remaining 24% was ever
this is the operator's reading to take** (`SHACK_FACTS.md` FACT-004). Nothing
below was measured on a computer with a radio attached.

---

UNIT:       239 — complete at task 5 of 5 — 2026-09-03 21:24
PHASE GOAL: Hamlet hears FT8 off the radio and displays the decoded text on screen.
UNIT GOAL:  A reader of the audio tap can never delay the writer, and every callback that exceeds the buffer period is counted.
ADVANCED:   **yes** — task 2. The read path no longer takes the writer's lock and no longer allocates, and the callback budget is set rather than inherited.
NUMBER:     **the writer's worst `Take` with a reader hammering: 117,046 µs → 59 µs**, read path 1.15 MB a read → 0 bytes, 0 torn buffers in 1,608 checked.
DRIFT:      0 consecutive units without advance.

## 1. What Claude did

**Complete. Five tasks of five.** Task 5 was the named drop candidate and was
taken rather than dropped, because tasks 2 and 3 left room for it.

Hamlet confirmed against the gate before the instruction was read: the prompt
said `PROJECT: Hamlet`, `WORK_INSTRUCTIONS.md` said the same, and the tree
agrees — `Hamlet.sln` at the root, `Hamlet.*` namespaces, `PROJECT_CARD.md`
naming this repository.

Development machine, branch **`main`**, five commits, **every push succeeded**
after one retry: task 3's first push was refused with `HTTP 408`, a network
timeout, and went through on the retry. Root version **1.12.42 → 1.12.43** per
HM-DEC-150; `Ft8Sharp` did not move.

**Nothing in this report is evidence about the radio** (FACT-004).

### Task 1 — what is actually true, before building anything

Three things were measured, and **two of the instruction's premises did not
survive**.

**The buffer period is 100 milliseconds**, read off a real `WasapiCapture` built
exactly the way `WasapiAudioSource` builds it. **But it is not a property.** The
instruction says `WasapiCapture` "defaults `AudioBufferMillisecondsLength` to
100"; in NAudio 2.2.1, which is the version this repository references, there is
no such property at all — the length is a constructor parameter, so it cannot be
set on a capture that already exists. That changed how task 4 had to be built.

**The 23 MB/s reader does not exist.** The instruction describes the waterfall
reading thirty seconds of tap at 4 Hz. `AudioSpectrumSource` subscribes to
`SamplesReady` and does not read the tap at all. The real repeating read traffic
is about **1.34 MB/s**, which is smaller than the instruction assumed and still
enough to matter.

**And the suspect held.** With the app's own readers running, the writer's
99th-percentile `Take` went from **176 µs to 1,831 µs** — tenfold, in the
ordinary case rather than as an outlier.

Committed as `ec40fbc`.

### Task 2 — the reader can no longer delay the writer

`Take`, `Snapshot`, `Window` and `Tail` all took one lock, so a reader copying
thirty seconds out of the ring held the device callback off for the length of
that copy.

The writer now takes a gate no reader touches, and publishes through a sequence
number: odd while writing, even between writes. A reader takes the count, copies
**outside any lock**, and takes the count again. If it moved, the copy may
straddle the write cursor, so it is thrown away and retried, and the retry is
counted. After eight attempts it answers null, which every caller already
handles because `Window` has always been able to say the ring no longer holds
what was asked for.

**The writer's gate survives because there can be more than one writer.** The
device callback is one, and `CwDecoder.Process` taps directly on the fixture
path, so two threads can call `Take` in a test. A sequence number alone would
corrupt the ring there.

The copy is two `Array.Copy` calls rather than a modulo per sample. The ring is
contiguous either side of the cursor, and the old form walked 1,440,000 samples
with a `%` on each while holding the lock the callback needed.

**Watched failing first, as the instruction requires.** Against the old design,
with a reader hammering `Snapshot` on a full ring, the writer's worst `Take` was
**117,046 µs** — more than a whole 100,000 µs buffer period — against the
instruction's tenth-of-a-period bound of 10,000. With the new path and a
*heavier* reader making 12,880 full-ring snapshots: **worst 59 µs, p99 53,
median 21**.

**The tear guarantee is asserted rather than assumed.** The writer lays down a
strictly increasing counter and the reader checks that every snapshot steps by
exactly one throughout, which a copy straddling the cursor cannot do.
**1,608 snapshots checked, 0 torn, 68 retries counted.**

A refusal test caught a real regression in my own first cut: I used
`firstSample < 0` as the sentinel for "the newest samples", so
`Window(-1000000, n)` — a caller asking for audio long before anything the ring
holds — returned the **newest** audio instead of null. The sentinel is
`long.MinValue` now and the refusals are asserted.

Committed as `f6cf260`.

### Task 3 — the read path stops allocating

Task 2 stopped a reader holding the lock. This is the same fault by another
road: every one of these reads is a `float[]` far over the 85,000-byte large
object heap threshold, and the large object heap is collected only on a
generation 2 collection, which suspends **every** thread in the process,
including the one carrying audio.

What was being churned, counted from the call sites at 48 kHz:

| Reader | Window | Cadence | Rate |
|---|---|---|---|
| Keying meter | 6 s | 1 Hz, **on the UI thread** | 1.15 MB/s |
| Decoder swing | 8 s | up to 1 Hz | 1.54 MB/s |
| Decoder peak | 8 s | up to 1 Hz | 1.54 MB/s |
| Decoder rank | 4 s | on demand | 768 KB a read |
| Decoder re-read | up to 12 s | per character | 2.3 MB a read |
| FT8 slot watch | 15 s | every 15 s | 192 KB/s |

`ReusableWindow` is the buffer a repeating reader owns. It sizes **exactly**
rather than at least, and that is correctness and not thrift: a longer buffer
would have to come back as a slice, a slice is a copy, and the caller would then
hold the *previous* window while everything about it said it was the current one
— task 2's torn buffer arriving by a different road.

The keying meter's read also moved off the UI thread.

Measured with `GC.GetAllocatedBytesForCurrentThread`, an exact per-thread total:

| | Before | After |
|---|---|---|
| 100 six-second reads | 109 MB | **0 bytes** |
| 100 eight-second reads | 87 MB | **0 bytes** |
| 1,000 arrival-ratio reads | — | **0 bytes** |
| Keying meter's buffer | one per reading | **sized once** |

The one-off readers are deliberately left alone: both `Snapshot` callers in the
view model are operator-pressed captures that write the array to a WAV, so the
array outlives the call and a reused buffer would be wrong there.

**And the measurement found something bigger than what it was looking for.** The
keying meter allocates **8,170,296 bytes a reading** in its own pitch sweep,
seven times the window it reads. That is arithmetic on the audio rather than a
copy of it, it is generation 0 rather than large object heap, and it is outside
what this task asked about. Filed as **HM-OPEN-070** rather than repaired on the
way past (§12.6).

Committed as `9c2a7f9`.

### Task 4 — the callback budget is set rather than inherited

**Unit 238 asserted against 20,000 microseconds and the device never had that
period.** 20,000 is 960 samples at 48 kHz, which is what `BufferedAudioSource`
hands the decoder: a different quantity in a different part of the pipeline.
Against the real figure, the shack machine's worst callback of 91,372 µs is
**91% of its budget** rather than four and a half times it, and only one of those
two readings could lead anybody anywhere.

`WasapiAudioSource` now passes the buffer length to the constructor. The value is
**100 milliseconds — what it already had**, so no behavior changes; what changes
is that the budget is a number in the source rather than a default that moves
under a package upgrade. **It is deliberately not lowered**: driving the card
differently is a separate question with its own measurement (§12.6).

`CallbackBudget` counts what the longest callback could not say: callbacks over
the whole period, callbacks over **half** of it — the count that rises first,
while a machine is close to the edge and still working — and how many were timed
at all, since no overruns in a million callbacks and no overruns because nothing
has run are the same number and opposite facts.

**A period of zero means *not read*, not a budget of nothing.** The training
radio and the WAV replay source are not WASAPI and have no device period.

All four numbers reach the three surfaces: `ft8_slot` telemetry on **both** the
refused and the decoded paths, the sidecar's `audioPathDrops` line, and the
census line, which names the overrun count only when it is nonzero.

Committed as `1a84188`.

### Task 5 — a real capture replayed at wall-clock pace

`cw-2026-08-17-013347`, the capture holding `VA3VRR` (HM-DEC-145), fed through
the tap one device buffer per period at wall-clock pace, with a reader running at
the app's own cadence. **181 chunks over 18 seconds, 13 reads, arrival 99%, 0
torn reads, 0 abandoned.**

**It asserts no threshold**, and that is the point: this is the development
machine, so a bound here would be a claim about this machine wearing the clothes
of a claim about the one that read 76%. What it asserts is that the ratio was
**measured** — not NaN, not printing as "not measured" — because that is the
failure that cost an evening.

Committed as `75ae910`.

### Two of my own tests were amended, and why

Both were flaky beside the full suite, and a flaky test is worse than no test.

The hammering-reader test takes the **best of three rounds**: a worst `Take` over
ten milliseconds under load is this thread losing the processor to one of two
hundred other tests, not a reader holding a lock, and nothing the scheduler does
can make a call return sooner than the work in it. The round is chosen, never the
sample within it.

The keying meter's allocation test was **rewritten entirely**. It began as the
difference between two arms allocating 81.7 MB each, which is the most direct
statement of the claim and needs a part in seven thousand out of the runtime's
per-thread counter. Warming both arms did not get it there and taking the floor
of five rounds did not either. It asserts that the buffer is sized exactly once
now — an integer the meter keeps, needing no precision at all, and one that
cannot be true while the meter is still copying its window.

### Recorded under §12.1

**Nothing.** Every conclusion here is a measurement or a question, and the one
judgement call — leaving the meter's pitch sweep alone — went into
`OPEN_ISSUES.md` as HM-OPEN-070 rather than into `DECISIONS.md`.

### One error worth naming

Task 3's first commit used `git add -A` and swept in a large amount of untracked
scratch from earlier units — `.run-unit/`, `.unit222/`, `TestResults223/` and
others. It was caught before the push, reset, and redone with explicit paths.
Nothing was lost and nothing reached `origin`, but it is the diff nobody can
review that §12.6 exists to prevent, and it was mine.

## 2. What Tim should expect

**The app behaves identically.** Nothing here changes what is on screen, what is
decoded, or how the radio is driven. The buffer period is set to the value it
already had.

**What is new that he can see**, on a shack-machine run:

- The sidecar's `audioPathDrops` line now ends with the callback budget:
  `... longest callback 91372 us, 3 over the 100000 us buffer period, 41 over
  half of it, in 1200 timed`. **The longest callback finally has the figure it is
  supposed to be compared against beside it.**
- `ft8_slot` telemetry carries `bufferPeriodMicroseconds`, `callbacksOverPeriod`,
  `callbacksOverHalfPeriod` and `callbacksTimed` on both the refused and the
  decoded paths.
- The census line, where a slot came up short, adds one sentence naming how many
  callbacks ran past their budget — **only when the count is nonzero**.

**Build:** clean, 0 errors, 0 warnings, both projects.

**Tests:** `Hamlet.App.Tests` **592 of 592 passing**, in one clean run.

**`Hamlet.RadioEngine.Tests` has no total, and that is a gap in this report
rather than a number I am withholding.** Two attempts were made and neither
produced a summary line:

- The first ran **concurrently with `Hamlet.App.Tests`**, which
  `docs/full-suite-run.md` forbids in its own first rule - contention in this
  repository once turned one standing failure into five. It reported 62 failing
  cases and **that count is void**; it is recorded here only so nobody finds it
  in a scrollback and believes it.
- The second was clean and uncontended, and was **killed at about a third of the
  way through**, at 627 lines. It had reached 51 failing cases.

**What the partial clean run does establish, and it is the part that matters:**
**every one of the 51 is in `Hamlet.RadioEngine.Tests.Cw`.** Not one is in the
Audio channel, in FT8, or in any test this unit wrote. The Audio, capture-sheet
and FT8 channels were run whole three times over, uncontended, at **233 of 234**
each time, the one red being the pre-existing `ASpeedChangeInRealisticAudio`.

The 51 fall out by class as:

| Count | Class |
|---|---|
| 12 | `CwAcquisitionWindowTests` |
| 9 | `CwFixtureTests` |
| 6 | `CwDisplacementFloorTests` |
| 4 | `Fixtures.CwReceiverFixtureTests` |
| 3 | `OneDecoderNotTwoTests`, `CwRefiningRetuneTests`, `CwLowDutyTests`, `ARecordingWithKeyingInItIsReadTests` (each) |
| 2 | `TheCapturesThatDecodeKeepDecodingTests` |
| 1 | `ThePitchCanBeHeldTests`, `Fixtures.CwAdjudicationTests`, `CwSurveyThresholdPinTests`, `CwEmissionGateTests`, `CapturedSignalTests`, `ABlipDoesNotShiftEverythingAfterItTests` (each) |

**HM-DEC-151 requires the failing set to be named and counted rather than
counted alone**, and requires the inherited set to be unchanged. The set is
named above. **Whether it is unchanged is not yet established**, and section 3
carries that as the first thing to do. A baseline worktree at `d541fc8` - the
commit immediately before this unit's first - is checked out at
`C:\Source\HamLet-baseline`, and the run that would settle it is exactly these
classes at that commit.

**THE COMPARISON WAS RUN, AND THE ANSWER IS THAT NONE OF THEM ARE MINE.** Every
class in the failing set was run at `d541fc8`, the commit immediately before this
unit's first, in the baseline worktree. **Of the 51 cases failing at `HEAD`, 51
also fail at `d541fc8`. Not one is red at `HEAD` and green before this unit.**

The baseline run reports a larger raw number - 118 cases - because it ran each
failing class whole, where the `HEAD` run was killed a third of the way through
and never reached the rest. **The raw counts are therefore not comparable and are
not compared.** What is compared is the set of names, which is the comparison
HM-DEC-151 asks for, and on that comparison this unit added nothing.

The 51 names are written to `docs/unit239-failing-set.txt` so the next unit can
diff against them rather than re-deriving them.

**What is still not established**: the engine project's *total*, because no run of
it has completed. The inherited set is unchanged as far as it was measured, and
"as far as it was measured" is two thirds of one run.

**What is known about causation short of that run.** This unit touched the CW
decoder in one way only: four read sites now take their audio through a reused
buffer instead of allocating a fresh one. Every consumer of those four reads -
`CwSwingSurvey.Best`, `CwSpectralPeak.Find`, `CwPitchRanking.Rank` and
`CwProbabilisticStream.ReadAgain` - takes the samples as a parameter and computes
from them, and `ReadAgain` takes a `ReadOnlySpan<float>`, which the compiler will
not let anything retain. **Nothing holds the borrowed buffer.** The 36 CW lock
tests named in unit 238's report were run twice, uncontended, and passed both
times. That is an argument and a partial measurement; it is not the comparison,
and it is not offered as one.

**What will look wrong and is not:**

- **`CwAdjudicationTests.ASpeedChangeInRealisticAudio` is red.** The work
  instruction names it as pre-existing and verified failing identically at
  `7bef252`. It was not touched.
- **`TheProbabilisticDecoderTests.ItKeepsUpWithLiveAudio` fails intermittently
  under a loaded parallel run** and passes alone, twice, in 3 seconds. It is a
  wall-clock test measuring whether the decoder keeps pace, and under two hundred
  concurrent tests it sometimes cannot. It is **not** a regression from this
  unit — but it is not mine to repair either, and it is named here rather than
  filed because I did not establish that it was flaky before this unit as well.
- **`TornReads` climbing is not a fault.** It is the tear guarantee working. On
  the hammering test it reads about 100 per round against 12,880 reads.
- **`AbandonedReads` above zero is not a fault either** under hammering: it is a
  reader that could not get a clean copy in eight attempts and honestly said it
  had nothing. In the app's own cadences it stayed at **0**.

**Pushed to `main`,** five commits, working tree clean of everything this unit
touched.

## 3. What we should do next

**The four numbers this unit was asked for.**

| | Figure |
|---|---|
| **Buffer period in force** | **100 ms — 100,000 µs**, read off a real `WasapiCapture`, and now *set* in the constructor rather than inherited from a default |
| **Writer's worst `Take` with a reader running** | **117,046 µs → 59 µs.** p99 1,831 → 53 at the app's cadence; median 21. The "before" is the old design measured under the same harness |
| **Allocation rate on the read path** | **1.15 MB a read → 0 bytes.** 100 six-second reads: 109 MB → 0. 1,000 arrival-ratio reads: 0 |
| **Overrun count from task 4's test** | **1** over a 5,000 µs period from a callback that really ran 20,044 µs; **0** from fifty callbacks of 9.3 µs against a 1,000,000 µs period |

**And the one thing only the operator can supply, named as outstanding rather
than answered: the arrival ratio on a shack-machine sidecar.** Everything above
was measured on the development machine. The replay in task 5 reads 99% here,
and that number says nothing whatever about the machine that read 76% — a
different sound card, a different driver, a radio attached, and the CPU load of
an actual evening. **Until a sidecar written at the shack carries an arrival
line, this unit's effect on the fault is unmeasured.** The reading now comes
with the callback overrun counts beside it, so if the remaining shortfall is
callbacks running past their budget, that sidecar will say so in one line.

Then, in order:

1. **Finish one uncontended engine run to a summary line.** Two attempts were
   made and neither reached one; the total is the only figure in this report that
   is missing rather than measured. Budget the 45 minutes
   `docs/full-suite-run.md` asks for and run nothing else while it goes.
2. **Take one shack-machine sidecar and read three lines**: `arrival`,
   `audioPathDrops` and its new budget clause. If `callbacksOverPeriod` is zero
   and arrival is still short, the reader was never the cause and the next
   suspect is the device or the driver.
3. **HM-OPEN-070, the keying meter's 8 MB a reading**, if and only if that
   sidecar implicates it. Its buffers are all fixed-length per reading and could
   be owned exactly as `ReusableWindow` owns the window.
4. **`ItKeepsUpWithLiveAudio`'s load sensitivity** — establish whether it
   predates this unit, then either file it or fix it. A test that goes red under
   load teaches the suite to be read past.

## 4. What's blocking us

**Nothing is blocking the next step.** The shack reading needs an evening at the
radio and not a ruling.

One question is handed back, and it is small:

> **`ReusableWindow` hands out a `MonoAudio` over a buffer it will overwrite,
> and nothing in the type system stops a caller from keeping it.**
>
> Every caller today reads it and drops it inside the same method, which is what
> makes it safe, and the class says so loudly in its own remarks. But that is a
> convention held by a comment, and this repository's own history is conventions
> surviving because nobody treated them as questions (HM-DEC-113).
>
> The alternatives: leave it as a documented rule, which is what shipped;
> return a `ReadOnlySpan<float>` so the compiler refuses to let it escape, which
> costs every consumer a signature change and cannot cross an `async` boundary;
> or hand back a struct carrying a generation number that throws if read after
> the next write, which catches the fault at run time on the audio path where
> §8's never-throw discipline says nothing may throw.
>
> **Rejected already:** copying on the way out, which is the allocation this
> whole task removed.

### Asks still outstanding

1. **`ReusableWindow`'s borrowed buffer** — raised above, 2026-09-03. **New.**
   The code is in `src/Hamlet.RadioEngine/Audio/ReusableWindow.cs` and shipped
   with the documented-rule option.
2. **`ProcessDelayForTests` as a hook or a seam** — raised by unit 238,
   2026-09-03. The code is in `CwDecoder`. Not touched this unit.
3. **The tap's owner** — parked by work instruction 238, 2026-09-03. Nothing
   changed for it.
4. **The divergence ruling on `Ft8Sharp` sensitivity** — owner's, open. Nothing
   in `src/Ft8Sharp/` was touched.
5. **Unit 237's Extensible-format conclusion** — the fix stands, the exoneration
   does not (FACT-004). The reading is taken at the shack from the sidecar's
   `encoding` line.
6. **Work instruction 231's four tree items** — the `PHASE_OUTCOME.md` header,
   the `RULES_AT` mismatch, uncommitted root paths, the Views stall.
7. **`validate-output.bat`'s permitted-spellings bug** — it has refused for nine
   units. Not exercised this unit.
