# Work instruction 248 - the candidate re-synced below the grid it was found on

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project - nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

---

## THE TOOL RULE - read this before task 1

Carried forward from units 244 through 247 unchanged, because it is measured and it
still holds. **Two different faults behave oppositely and have been conflated
before.**

| | What it is | How it announces itself | The answer |
|---|---|---|---|
| **A - the allow-list** | a fixed set of permitted command prefixes, matched against the command **as typed** | `This command requires approval` | **there is usually a permitted spelling that works.** Find it before concluding anything |
| **B - the sandbox** | the shell may not create files or directories | `... was blocked. For security, Claude Code may only write to files in the allowed working directories for this session` | **there is no shell spelling that works.** Use `Write`/`Edit` |

`docs/shell-probe-243.md` has all eight probes verbatim. What runs: `dotnet build
<path>`, `dotnet test <path>`, `git` throughout, `ls`, `grep`, `sed`, `head`,
`tail`, `find`, `wc`, `date`. What never runs: `mkdir`, `cp`, `mv`, `git mv`,
`dotnet sln add`, `dotnet run`, `git clean`, any redirect write, and any compound
line where one part is refused. `dotnet --version` is refused and **is not evidence
about the toolchain.**

Four spellings earlier units paid for, banked here so this unit does not pay again:
**`dotnet sln add` is refused** - edit `Hamlet.sln` with the file tools; **`git
clean` is refused** - a one-shot `.proj` deletes itself with an MSBuild `<Delete>`
task; **`-p:EntryProps=` needs an ABSOLUTE path with FORWARD slashes**, e.g.
`C:/Source/HamLet/.run-unit/scratch/unit248-outcome.props`, because a relative path
resolves against `tools/arbiter/` and Git Bash eats backslashes; and **`git commit
-F -` with a heredoc is refused by the static analyser** - write the message to a
file under `.run-unit/scratch/` and use `git commit -F <file>`.

**`tools\arbiter\*.bat` is a closed loop.** Go through `dotnet build
tools/arbiter/outcome-append.proj` and `dotnet build
tools/arbiter/validate-output.proj`. **No apostrophes in any `PHASE_OUTCOME.md`
field** - one breaks the tool's PowerShell parse.

**A refused shell call is a signal to reach for the other tool, not to stop.**
Nothing in this unit halts the loop.

---

## Why this unit exists

**This is unit 248. It is the sixth unit of this phase, and the first aimed at
step 4 - the only step left whose own must-pass exit is the number the phase is
named for.**

Where the phase stands as this was authored. Steps 0 and 1 are closed. Step 2 is
`partial` at **10.8 per cent (33 of 306) at -21 dB, 0 wrong**, cut down against a
ceiling unit 246 measured before it wrote a line, and not re-opened tonight. Step 6
is `done`: unit 247 combined a repeat and took the same rung from 4.2 to **22.2 per
cent with a realistic placement error between the two hearings** and 70.9 per cent
without one, 0 wrong on all twelve rows. Steps 3, 4 and 5 have never been touched.

**The 50 per cent crossing is where the phase's number lives.** `HM-OPEN-067` put it
near **-19.5 dB** against a published **-21**. Unit 246 measured it at **-19.54 dB**
for the port and **-19.81 dB** with ordered statistics decoding on, by linear
interpolation between the -19 and -20 rungs. **About 0.27 dB of the 1.5 is closed
and about 1.2 dB is not.** Step 4's second exit is that this crossing moves down.
No other unattempted step has the crossing in its exit list.

### Why step 4 now, and why the argument against it does not survive the tree

Unit 247's instruction set step 4 aside on two grounds, and both are answered by
things measured since - the second by reading the tree rather than by any report.

1. **"Unit 222 already tried oracle alignment and landed inside the as-is
   interval."** Look at what that oracle could reach.
   `tests/Ft8Sharp.Tests/Dsp/AlignmentSweep.cs:76` declares its point as
   `Point(int Block, int TimeSub, int Bin, int FreqSub, int Score, int Agreement)`.
   **Every coordinate is an integer grid index.** That sweep asks the question at
   every position *the search itself could have proposed* - which is exactly the
   quantised grid - and picks the best one. **It is oracle selection on the coarse
   grid, and it is not sub-grid alignment.** Nothing in this tree has ever
   extracted a symbol at a time or a frequency the grid does not name. Step 4 is
   the first attempt at it, and unit 222's result is not evidence about it.

2. **"The plan sizes step 4 at a fraction of a decibel."** The plan also says a
   target quoted from a prior measurement moves when the tree says otherwise. Two
   measurements taken since say the grid costs more than a fraction: `HM-OPEN-074`,
   where 2 of 51 trials had no candidate near the signal at all; and `HM-OPEN-075`,
   where moving the second hearing **2.00 Hz and 480 samples** - a third of a tone
   and a quarter of a symbol, both inside one grid cell - took unit 247's combined
   column from **200 of 306 to 55 of 306**. That is 149 trials, about half the
   population, lost to placement inside a single cell of the coarse grid.

**What the grid actually is, from the tree's own constants.** At 12 kHz with
`SymbolPeriodSeconds` 0.160, `BlockSize` is 1920 samples and `SubblockSize` is
**960 samples (0.08 s)**; `TransformLength` is 3840, so `TransformBinSpacingHz` is
**3.125 Hz** against a tone spacing of 6.25 Hz. A candidate is therefore placed to
within **±480 samples (±0.04 s, a quarter of a symbol) in time and ±1.5625 Hz (a
quarter of a tone) in frequency**, and the port reads all 58 data symbols at that
quantised position.

**And the ladder's default placement sits exactly on that grid.**
`Ft8LadderHarness.DefaultFrequencyHz` is 1000.0 Hz, which is 320 × 3.125 exactly;
`DefaultOffsetSamples` is three symbol periods, which is 5760 samples, six whole
sub-blocks exactly. **Every number this phase has recorded was measured at the one
placement where the coarse grid has nothing to lose.** Whether that flatters the
baseline is unknown, and task 1 is where it stops being unknown.

```
PHASE GOAL:   Hamlet reads FT8 as well as the best decoder there is, and then
              reads it further.
UNIT GOAL:    Ft8Sharp.Deep takes each coarse candidate the port's search returns,
              mixes it to baseband from the samples, re-syncs it below the
              waterfall's grid in both time and frequency, extracts its soft
              values there, and submits the result to the port's own parity and
              CRC-14 gates - and measures what that moves on the ladder's 50 per
              cent crossing, separately from steps 2 and 3.
ADVANCES:     step 4. Coarse candidates mixed to baseband, filtered and re-synced
              at sub-symbol time and sub-hertz frequency before extraction; the
              ladder's 50 per cent crossing measured with its trial count; zero
              wrong decodes; and the gain quoted separately from steps 2 and 3 on
              the scoreboard.
```

**This unit is not required to move the crossing by any particular amount.** It is
required to build the re-sync and measure it honestly on step 0's instrument. **A
crossing that does not move, reported with the distributions that say why, is this
unit succeeding** - it closes a question the phase has been reasoning around since
unit 222, and the arbiter will re-order the plan on it.

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Every line below was read from it while this
instruction was written. **Check each one and report mismatches in section 1; do
not repair them and do not repair this instruction.**

The port's surface, all `public`:

- `src/Ft8Sharp/Dsp/Ft8SoftSymbols.cs`: `RatioCount` at `:73`, `NormalisedVariance`
  24.0 at `:84`, `Extract(Ft8Waterfall, Ft8Candidate, Span<float>)` at `:117`,
  **`ExtractSymbol(ReadOnlySpan<double> magnitudes, Span<float> ratios)` at `:213`**,
  `Normalise` at `:287` returning the variance, `Variance` at `:323`, `HardDecision`
  at `:351`.
- **`ExtractSymbol` is the seam this unit is built on.** Its own documentation at
  `:198-:212` says: eight magnitudes **in decibels, indexed by symbol value rather
  than by tone**, in, three ratios out, most significant bit first, with the
  partition derived and pinned against upstream's written one. `Extract` at
  `:182-:191` shows the caller's half: `magnitudes[value] = waterfall.DecibelsAt(...
  candidate.BinOffset + gray[value])` with `gray` from `Ft8Tables.Ft8GrayMap`, and
  sync symbols stepped over by `Ft8SymbolEncoder.IsSyncSymbol`.
- `src/Ft8Sharp/Ldpc/Ft8CodewordDecoder.cs`: `Decode(ratios, cache, maxIterations)`
  at `:70`, **GATE 1 parity at `:80` and GATE 2 the checksum at `:96`**, both
  commented as such in the file.
- `src/Ft8Sharp/Dsp/Ft8Candidate.cs:48`: `Ft8Candidate(int Score, int BlockOffset,
  int TimeSubOffset, int BinOffset, int FrequencySubOffset)`, with
  `FrequencyHz(geometry)` at `:93` and `TimeSeconds(geometry)` at `:103`.
- `src/Ft8Sharp/Dsp/Ft8WaterfallGeometry.cs`: `SymbolPeriodSeconds` 0.160 at `:49`,
  `DefaultSampleRate` 12000 at `:55`, `DefaultTimeOversampling` 2 at `:64`,
  `DefaultFrequencyOversampling` 2 at `:67`, `BlockSize` at `:207`, `SubblockSize` at
  `:210`, `TransformLength` at `:213`, `TransformBinSpacingHz` at `:231`,
  `ToneSpacingHz` at `:234`, `FrequencyHz(bin, sub)` at `:247`, `TimeSeconds(block,
  sub)` at `:263`, `TryBinFor` at `:272`.
- `src/Ft8Sharp/Dsp/Ft8Waterfall.cs`: **magnitudes only, quantised to bytes at
  0.5 dB** (`DecibelsFor` at `:65`). No phase and no samples.
  `src/Ft8Sharp/Dsp/Ft8Monitor.cs:211` `Analyse(ReadOnlySpan<float> samples)` is how
  audio becomes one; `HannSquaredSine` at `:87` is the window it applies.
- `src/Ft8Sharp/Dsp/Ft8SyncSearch.cs`: `SyncGroupLength` 7 at `:53`, `SyncGroupCount`
  3 at `:56`, `SyncGroupOffset` 36 at `:59`, `DefaultCandidateLimit` 140 at `:88`,
  `Find` at `:159` and `:175`, and **`ScoreAt` at `:265`, public and documented as a
  scoring primitive for a caller measuring at a position it already knows.**
  `Ft8Tables.Ft8CostasPattern` and `Ft8Tables.Ft8GrayMap` are public spans in
  `src/Ft8Sharp/Tables/Ft8Tables.g.cs` on a `public static class Ft8Tables` at `:40`.
- `src/Ft8Sharp/Encode/Ft8SymbolEncoder.cs`: `SymbolCount` 79 at `:58`,
  `DataSymbolCount` 58 at `:61`, `SyncBlockLength` 7 at `:64`, `SyncBlockCount` 3 at
  `:67`, `SyncBlockOffset` 36 at `:79`, `BitsPerSymbol` 3 at `:82`, `ToneCount` 8 at
  `:88`, `SyncBlockStart` at `:96`, `IsSyncSymbol` at `:110`.
- `src/Ft8Sharp/Encode/Ft8Waveform.cs:94` `SamplesPerSymbol(sampleRate)`.
- `src/Ft8Sharp/Dsp/Ft8SlotDecoder.cs`: `Decode(ReadOnlySpan<float> samples)` at
  `:133`, `Decode(Ft8Waterfall)` at `:139`.

The sibling as unit 247 left it - `src/Ft8Sharp.Deep/`:

- `Ft8DeepSlotDecoder.cs`: `Port` at `:133`, `Osd` at `:139` **null by default and
  null meaning do exactly what the port does**, `LastOsd` at `:158`,
  `RemembersHearings` at `:164`, `LastHearings` at `:185`, `Decode(ReadOnlySpan<float>
  samples)` at `:208` **which is `Decode(new Ft8Monitor(Geometry).Analyse(samples))`
  and keeps no samples**, `Decode(Ft8Waterfall)` at `:226`.
- `Ft8DeepOsdSettings.Default` order 2 at `:86`. `Ft8DeepCombineSettings` at `:50`
  with `SubmissionsPerSlot` at `:184` and **`ExpectedFalseAccepts(submissions) =
  submissions / 16384.0` at `:198`**. `Ft8DeepRepeatDecoder` at `:48`,
  `Ft8DeepSoftCombiner`, `Ft8DeepHearing(Ft8Candidate, float[] Ratios)` at `:31`,
  `Ft8DeepOrderedStatistics`, the counts types, `porting-notes.md`, `LICENSE`,
  `NOTICE`.
- `tests/Ft8Sharp.Deep.Tests/Ft8DeepSlotDecoderTests.cs:180`
  `TheSiblingHoldsExactlyTheseTypesAndTheListIsAssertedWhole` asserts the sibling
  assembly's **whole type list**. **That is a tripwire units 245, 246 and 247 each
  left for the next one. Changing it deliberately is this unit's job; discovering it
  afterwards is not.**

The instrument:

- `tests/Ft8Sharp.Tests/Dsp/Ft8LadderHarness.cs`: `DefaultSeed` 221001 at `:61`,
  `DefaultFrequencyHz` 1000.0 at `:64`, `DefaultOffsetSamples` at `:69`, **`Decoder(string
  Name, Func<float[], Ft8SlotResult> Decode)` at `:74` - the seat takes raw samples,
  so a decoder that needs the audio fits it with no change to the harness**;
  `Result` at `:91`; `Header` at `:170`; `Available()` at `:194` returning two
  entries; `Run(rung, trials, seed, decoders?, frequencyHz?, offsetSamples?, log?)`
  at `:244`; `RunRepeats` at `:425`; `RepeatsReport` at `:669`.
- **`Run` at `:244` and `RunRepeats` at `:425` are not to be modified.** Every row
  this phase has recorded was taken through one of them. Both already accept
  `frequencyHz` and `offsetSamples`, which is all task 1 needs.
- `tests/Ft8Sharp.Tests/Dsp/SearchFixture.cs`: `Truth(string Label, double
  BaseFrequencyHz, int OffsetSamples)` at `:38`, `Place` at `:55`, **`OneSignal`
  at `:84` returning `(float[] Slot, Truth Where)` - the exact placement, which is
  the oracle position task 2 needs**, `AddNoise` at `:152`, `TransmissionPower` at
  `:174`.
- `tests/Ft8Sharp.Tests/Dsp/Ft8Step6Ladder.cs`: `Rungs` at `:88`, `Seeds` at `:110`,
  `CollapseBottomDecibels` -24.0 at `:119`, `Population()` at `:160` - **51 scoreable
  messages**, `TrialsFor` at `:179`, `Wilson` at `:255`.
- `docs/unit246-osd.md` carries the OSD ceiling and, at its "How much of the decibel
  that is" table, **the -19.54 and -19.81 crossings and the interpolation they came
  from**. `docs/unit247-combining.md` carries the combining distances and tables.
  **Read both before task 1 and do not re-measure what they already measured.**

Versions and bookkeeping:

- Root version `1.12.50` at `Directory.Build.props:145`. `Ft8Sharp` **`0.10.7`** at
  `src/Ft8Sharp/Directory.Build.props:396`. `Ft8Sharp.Deep` `0.3.0` at
  `src/Ft8Sharp.Deep/Directory.Build.props:46`.
- The highest issue id in `OPEN_ISSUES.md` is `HM-OPEN-075`.
- `PHASE_STATUS.md` and `PHASE_OUTCOME.md` head `STEP: 0` **`partial`** while the
  last `## UNIT 2 - STEP 0` entry reads `done`. **Do not re-audit step 0, do not
  reconcile the header, and do not report the disagreement a sixth time.**
- `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `RUN_LEDGER.md` were modified and
  uncommitted at the root when this was authored. **They are the loop's own
  bookkeeping - commit them with your first task's commit and say you did.**
  `.run-unit/` is the launcher's; leave it.

**Going-in test baselines, from unit 247:** `Ft8Sharp.Tests` **593 passed / 0 failed
/ 1 skipped / 8 m 12 s**; `Ft8Sharp.Deep.Tests` **51 passed / 0 failed / 0
skipped**. **A different baseline is itself a finding.**

**Expected to fail, and not this unit's:**
`CwAdjudicationTests.ASpeedChangeInRealisticAudio` and the 51 inherited CW reds in
`docs/unit239-failing-set.txt`. **None of those is in `Ft8Sharp.Tests`.** The one
expected skip is `Ft8TableGenerationTests.RewriteTheCheckedInTablesFile`. If
`Ft8Sharp.Tests` shows red outside that set, or a second skip appears, that is a
finding and it goes in section 1.

---

## Rulings in force for this phase

Transcribed from `PHASE_PLAN.md`. **Not to be re-argued by this unit.**

**The seam is split.** `Ft8Sharp` stays a faithful MIT port of `ft8_lib`,
byte-identical in behaviour, and **nothing in this phase changes a line of it.**
Improvements live in `Ft8Sharp.Deep`. The port's value now is that it cannot drift:
every measurement in this phase is taken against something known-identical to
upstream, so a regression in the sibling is always visible.

**`Ft8Sharp.Deep` is GPL-3.0**, carrying its own `LICENSE` and a `NOTICE` citing the
published sources it implements. Ruled by Tim, 2026-09-04. **No unit raises the
licence and no step is held by it.**

**No algorithm comes from WSJT-X's source or `ft4_ft8_public/`.** Published
description only - Fossorier and Lin 1995 for ordered statistics, and the QEX paper
(Franke K9AN, Somerville G4WJS, Taylor K1JT, "The FT4 and FT8 Communication
Protocols," QEX, July/August 2020) - cited at the point of use. **WSJT-X is a
measuring instrument in this phase and never a source.** This is the second of the
three things the arbiter may not reason past, and it binds this unit hard: **WSJT-X
downconverts and re-syncs candidates too. Do not go and look at how.** Baseband
mixing, low-pass filtering, decimation and Costas correlation are textbook DSP and
are cited as such; the FT8 frame they are applied to comes from the QEX paper.

**Transmit.** `CLAUDE.md` §0.2 is untouched. **Nothing in this phase keys the
radio.** This step synthesises signals as test oracles and they never reach a
transmitter. This is the first of the three.

**What Hamlet asserts to Tim.** §12.1 and §0.0. **A decode this phase produces that
nobody sent is worse than a decode it misses.** If an approach produces a wrong
decode, **that approach is rejected and another is taken** - the step does not close
and does not stop. This is the third of the three, and step 4's own third exit is
where it is enforced.

**There is no WSJT-X on the development machine and no unit may assume one.** Step 4
has **no deferred exit and needs no fixture**: all four of its must-pass exits close
on the ladder, which knows what it transmitted.

**Nothing is claimed without the scoreboard.** No unit in steps 1 to 6 may report an
improvement except as a number on step 0's instrument. A decode rate quoted without
it is not evidence.

**A wrong decode is counted separately from a missed one, everywhere, in every
report.**

**The steps are a hypothesis, not a contract.** `PHASE_PLAN.md` grants leave to
reorder, replace, retire and add steps and to split or re-scope criteria, with the
record in `PHASE_OUTCOME.md` as the only constraint. **A step whose number does not
move is evidence, not a halt.**

### Five things the arbiter decided, so this unit does not spend a night on them

1. **Step 4 is taken now, ahead of steps 3 and 5, and steps 2 and 6 are not
   re-opened.** Licensed by `PHASE_PLAN.md`'s leave to reorder steps 2, 3, 4 and 5
   on measured evidence from the scoreboard. The evidence is above: unit 222's
   oracle was a grid oracle, `AlignmentSweep.Point` carries integers only, and
   `HM-OPEN-074` and `HM-OPEN-075` are two measured arguments that the sub-grid cell
   costs decodes. **Step 3 is not taken because the ladder synthesises one
   transmission a slot** (`SearchFixture.OneSignal`), so subtraction has nothing to
   subtract on the instrument this phase's number is measured on. **Step 5 is not
   taken because it moves no decibel** - it is a report-only measurement and a
   column. **Do not re-argue the ordering and do not open step 2, 3, 5 or 6.**

2. **Step 4's entry criterion is satisfied.** It reads *step 1 complete*, and step 1
   is `done` at four of four with `STATE_AFTER: done` from a separate session.
   Nothing else gates it. **Do not re-audit step 0 or step 1.**

3. **The extraction seam is the port's own `ExtractSymbol`, not a new one.** The
   sibling computes the **eight tone magnitudes in decibels, in symbol-value order
   through `Ft8Tables.Ft8GrayMap`**, at the re-synced position from its own baseband
   samples, and hands them to `Ft8SoftSymbols.ExtractSymbol` for every non-sync
   symbol, in `Ft8SymbolEncoder`'s layout. **The Gray map, the bit partition and the
   ratio arithmetic are not re-implemented in `Ft8Sharp.Deep`.** The one named
   change is *where the magnitudes are measured from* - which is the whole of step 4
   and is what makes the gain attributable.

4. **Fine sync runs where the coarse decode fails the port's gates, and it never
   replaces a decode the port already made.** Same shape as unit 246's OSD: the
   candidate goes through the port's path first, and only a candidate the port
   refuses is re-synced and re-submitted. **At most one extra submission to
   `Ft8CodewordDecoder.Decode` per coarse candidate.** This bounds the false-accept
   arithmetic at about twice today's and it makes the superset property assertable.
   **If the measurement says re-syncing every candidate is worth more, that is a
   finding to report, not a change to make tonight.**

5. **The sibling gets its own samples-carrying entry point; the waterfall-only one
   is untouched.** `Ft8DeepSlotDecoder.Decode(ReadOnlySpan<float>)` at `:208`
   discards the samples the moment it has a waterfall, and a waterfall has no phase
   and no samples in it. Keep the samples alongside. **`Decode(Ft8Waterfall)` with
   fine sync configured performs no re-sync and says so through a count** - it does
   not throw, and it does not silently pretend to have re-synced. The identity test
   with everything off is not weakened.

---

## Status cadence

After each task, before starting the next, update `PROJECT_STATUS.md` per
`CLAUDE.md` - `STATE`, `TASK: n of m`, `BALL`, `UPDATED` **read from `date` and
never composed** (unit 247 got this wrong and corrected it; do not repeat it), and
`NOTE` saying what is moving inside the task. The same every ten minutes while a
task is running. **Use the file-editing tools if the shell refuses.**

**A long measurement is a status update, not a silence.** Tasks 1 and 5 each walk
many whole blocks of 51 and 306 trials; say so in `NOTE` before you start them, with
the number of trials and the estimated wall clock.

---

## Tasks

Seven tasks. **Task 1 is a trace and comes first**, because this unit must measure
what the coarse grid costs before it writes a line of re-sync. **Task 7 is the named
drop candidate.**

**Start the `Ft8Sharp.Tests` baseline run early** - it was 8 m 12 s for unit 247 and
it can run while task 1 is being written.

### Task 1 - the trace: what does the grid cost, measured with nothing new

**Measuring only. Not a line of production code in this task.** Every number here
comes out of `Ft8LadderHarness.Run`, which already takes `frequencyHz` and
`offsetSamples`. **Say what you find, not what this instruction expects.**

1. **Confirm the grid arithmetic against the tree** and report it: `BlockSize`,
   `SubblockSize`, `TransformBinSpacingHz`, `ToneSpacingHz`, and whether
   `DefaultFrequencyHz` and `DefaultOffsetSamples` land exactly on a bin centre and
   exactly on a sub-block boundary. **If the ladder's default placement is not
   on-grid in both axes, that is a finding and the rest of this task changes
   meaning.**

2. **The placement sweep.** At **-20 dB** - the rung nearest the crossing, where the
   rate is most sensitive - walk `Run` over a grid of placements: frequency offsets
   of **0, 0.78, 1.56 and 2.34 Hz** added to `DefaultFrequencyHz` (quarter steps of
   one 3.125 Hz bin) crossed with time offsets of **0, 240, 480 and 720 samples**
   added to `DefaultOffsetSamples` (quarter steps of one 960-sample sub-block).
   **51 trials per placement is enough for the shape**; use one whole block so the
   population is whole. Report the port's and the Deep-OSD-on rate at each of the 16
   placements, with the three counts.

3. **The corners at full weight.** Take the best and the worst placement the sweep
   found and re-run each at **306 trials at -20 dB**, and the same two placements at
   **-21 dB, 306 trials**. **This is the size of the prize**: the difference between
   the on-grid rate the whole phase has quoted and the worst-cell rate is what
   perfect alignment could recover, and it is measured rather than argued.

4. **Say plainly what it means.** If the spread across the cell is small - a couple
   of points of rate - **then step 4 has little to recover on this instrument and
   this unit's own scoreboard is likely to say the same thing.** Report that
   before you build, size the rest of the night accordingly, and **still build
   it**: exit 1 is the re-sync existing and exit 2 is the crossing measured, and a
   measured null is what closes the question the phase has been reasoning around
   since unit 222. If the spread is large, say how large.

5. **The placement-averaged rate, stated once.** Give the mean rate over the 16
   placements beside the on-grid rate at -20 dB. **Real air is uniform over the
   cell and the ladder's default is one corner of it.** If those two numbers differ
   materially, record it as a finding about what the phase's own baseline means -
   **do not change any target in `PHASE_PLAN.md` and do not restate the baseline.**
   That is the arbiter's to do with the number you hand it.

### Task 2 - the baseband extractor, at a position it is told

`src/Ft8Sharp.Deep/`, a new type. **No search in this task. It is told where to
look.** This is the half of step 4's first exit that is *mixed to baseband,
filtered, extracted*; task 3 is *re-synced*.

- Given the slot's samples, the sample rate, a **base tone frequency in hertz** and
  a **start time in seconds** - both continuous, neither a grid index - produce the
  **174 ratios** in the port's convention.
- **Mix to complex baseband** at the base tone frequency, **low-pass filter** to the
  eight-tone occupancy and **decimate**. State the filter, its length, its cutoff
  and the decimated rate, and why each is what it is - the eight tones span
  8 × 6.25 = 50 Hz, so the choice has an arithmetic behind it rather than a taste.
- For each of the 79 symbols, compute the **eight tone powers** over that symbol's
  window at the commanded start time, convert to decibels, order them **by symbol
  value through `Ft8Tables.Ft8GrayMap`**, and call
  **`Ft8SoftSymbols.ExtractSymbol`** for each non-sync symbol per ruling 3. A symbol
  whose window falls outside the slot gets **three zero ratios**, which is the
  port's own rule at `Ft8SoftSymbols.cs:174` and means *no opinion*, not *refuse*.
- **State the symbol window shape and what it buys.** The port applies
  `Ft8Monitor.HannSquaredSine` to 1920-sample blocks. A rectangular window at the
  exact symbol boundary is a different measurement, not automatically a better one.
  **Pick one, measure the other once, and report both numbers** - do not sweep.

**The control, and it is the most important measurement in this task.** Run the new
extractor **at each coarse candidate's own grid position** - the position the port
would have read - and compare its decode rate over one 51-trial block at -21 dB
against the port's. **If the sibling's extractor is materially worse than the port's
at the same position, the extractor is the problem and nothing the fine search does
afterwards can be credited or blamed.** Report that comparison before any fine
search number, and if it is worse, say by how much and what you think is
responsible.

**The oracle ceiling, which is the second half of the trace.** `SearchFixture.OneSignal`
returns a `Truth(Label, BaseFrequencyHz, OffsetSamples)`. For one 51-trial block at
**-21 dB** and one at **-20 dB**, at the **worst placement task 1 found**, measure
the hard-decision distance to the transmitted codeword - the same
`Ft8Payload.Create` / `LdpcEncoder.Encode` chain units 246 and 247 used - from:

- the closest candidate's ratios through the **port's** `Extract` (this should land
  near unit 246's median of **31 of 174**, and if it does not that is a finding
  about the harness first);
- the new extractor at that **same grid position**;
- the new extractor at the **oracle position** - `Truth`'s exact frequency and exact
  offset.

**The code's iterative recovery reaches zero at about 17.** How far the oracle row
falls below the grid row is the ceiling on everything task 3 can win. **Report it
whichever way it comes out.**

Tests in `tests/Ft8Sharp.Deep.Tests`, on synthesised audio rather than on the
ladder: a clean loud transmission extracted at its true position decodes to the text
that was sent; the same one extracted a whole tone away does not; the extractor
never throws on silence, on a slot shorter than a frame, or on a base frequency near
the passband edges.

### Task 3 - the fine search: finding the position instead of being told it

- For a coarse candidate, search a **stated grid of time and frequency offsets
  around it** and return the offset that maximises the **Costas sync correlation**
  over the 21 sync symbols - `Ft8Tables.Ft8CostasPattern`, three groups of seven at
  symbols 0, 36 and 72, per `Ft8SymbolEncoder.SyncBlockStart`.
- **The extent must cover the whole cell the coarse grid leaves undetermined:** at
  least **±0.04 s** in time and **±1.5625 Hz** in frequency. **State the step in each
  axis and what it buys**, measured on task 2's distance instrument rather than
  chosen - a step finer than the measurement can distinguish is tuning.
- **Report the edge-hit rate**: how often the winning offset sat on the boundary of
  the search grid. A high edge rate means the extent is too small and the search is
  reporting the edge rather than a peak. **Say the number; do not quietly widen the
  grid to hide it.**
- **Report what one candidate's re-sync costs in milliseconds**, before task 5, so
  the scoreboard can be sized. The port sits at about 64 ms a slot and OSD at 72.
- Cite the QEX paper in `src/Ft8Sharp.Deep/porting-notes.md` and in XML remarks at
  the point of use for the FT8 frame, the Costas arrays and the CRC-14 this depends
  on. **The mixing, filtering and correlation are standard DSP and are cited as
  such.** No WSJT-X source, and no looking for anybody's downconverter.

Tests: on synthesised audio with a **known** sub-grid displacement planted, the
search recovers the planted time offset to within its own step and the planted
frequency offset to within its own step, over a spread of displacements covering
the whole cell; on pure noise it returns something and does not throw; and it is
**deterministic** - the same samples give the same offset twice.

### Task 4 - the re-sync in the loop, without disturbing anything below it

- A settings type in `Ft8Sharp.Deep` in the shape of `Ft8DeepOsdSettings` and
  `Ft8DeepCombineSettings` - **null by default, and null meaning do exactly what the
  sibling did at unit 247**, which with `Osd` also null is exactly what the port
  does.
- A samples-carrying entry point per ruling 5. **`Decode(Ft8Waterfall)` with fine
  sync configured performs no re-sync and reports that through a count.**
- **Fine sync runs only where the port's gates refused the coarse candidate** (ruling
  4), and the re-synced ratios go to **`Ft8CodewordDecoder.Decode` - the port's
  parity gate and CRC-14 gate are the only acceptance.** Nothing in `Ft8Sharp.Deep`
  decides a message is real. The codeword comes back through the same route unit 246
  used, so no `Ft8CodewordResult` is constructed outside the port.
- **The result with fine sync on must be a superset of the result with it off.**
  Every message the ordinary path returned is still there, in order, unchanged.
  **Re-syncing only ever adds. Assert it.**
- **Count its own outcomes separately** - candidates offered, candidates re-synced,
  re-synced codewords the port then accepted, and the mean and worst offset the
  search moved a candidate by in both axes. **A rate that moved with no visible
  re-sync activity behind it is not evidence**, and the offset distribution is what
  says the search is doing what it claims.
- **State the submission arithmetic in the report, in full.** Every codeword put to
  the CRC-14 is an independent chance of a false accept at about **one in 16,384**;
  `Ft8DeepCombineSettings.ExpectedFalseAccepts` at `:198` is that arithmetic already
  written down. Report submissions per slot and across the whole measurement, and
  the expected wrong count that implies, beside the wrong count actually observed.
- **The OSD-off, fine-sync-off, combine-off whole-`Ft8SlotResult` identity test
  against `Ft8SlotDecoder` is not optional and is not to be weakened.** It is what
  keeps the instrument an instrument.
- **Change `Ft8DeepSlotDecoderTests.cs:180`'s type-list assertion deliberately** to
  whatever the sibling now holds, and say in the report that you changed a tripwire
  unit 247 left rather than that a test broke.

### Task 5 - the scoreboard, whole, and the crossing

**Nothing is claimed without this.** Step 4's exits 2 and 4 are this task.

- **Three rungs - -19, -20 and -21 dB - at 306 trials each**, through
  `Ft8LadderHarness.Run` unmodified, with **three columns**: the port; the sibling
  with **fine sync on and OSD and combining OFF**; and the sibling with **OSD on and
  fine sync off**, which is the step 2 regression column. **Three counts on every
  row, never two.** Quote the tables whole with delivered SNR.
- **The 50 per cent crossing, interpolated the same way unit 246 interpolated it** -
  linearly between the -19 and -20 rungs, which is the arithmetic `HM-OPEN-067`'s
  "near -19.5" was read off, and quoted as an interpolation rather than as a
  measured crossing. **The figures to beat are -19.54 dB for the port and -19.81 dB
  with OSD.**
- **Exit 2 is judged at the ladder's default on-grid placement**, because that is
  the placement every figure this phase has recorded was taken at and a crossing
  compared across two placements is not a comparison. **Then run the same three
  rungs and three columns at the worst placement task 1 found**, and quote that
  pair too. If the crossing moves at the off-grid placement and not on-grid, **say
  exactly that** - it is a true and useful result, and it is not exit 2 being met.
- **The gain quoted separately from steps 2 and 3** - step 4's fourth exit. The fine
  sync column has OSD off and combining off, so the difference between column one
  and column two is one named change. **Do not run fine sync and OSD stacked in this
  task** and do not report a combined figure as step 4's.
- **Zero wrong decodes across every rung, every column and both placements**, or the
  approach is rejected and the report says which rung produced it, with the message
  sent beside the message returned. `WrongReturn` already prints that line.
- **The step 2 regression check must still read 10.8 per cent (33 of 306), 0 wrong
  at -21 dB.** If it does not, something this unit did moved a number underneath it
  and that is the first thing section 3 says.
- **Worst-case time per slot with the margin stated against 15 seconds.** The worst
  single slot observed, not the mean, with its candidate count and how many of them
  were re-synced. **If fine sync costs so much that the worst slot passes 1.5
  seconds - a tenfold margin - say so and cut the search grid down, stating what you
  cut and what it cost in rate.**

### Task 6 - the write-up and the record

- `docs/unit248-baseband-resync.md`: task 1's placement sweep, task 2's oracle
  ceiling and control, task 3's step and edge-hit rate, and task 5's tables, so the
  next unit on step 4 does not re-measure them.
- **Open an `OPEN_ISSUES.md` entry at the next free id** (`HM-OPEN-076` unless
  something took it) **only if the numbers earned one**. Candidates the measurements
  might raise: an oracle ceiling that says the grid costs little; an edge-hit rate
  that says the search extent is wrong; a placement-averaged rate that differs
  materially from the on-grid figure the phase quotes. **If the numbers said nothing
  of the kind, open nothing and say why** - an empty issue is worse than none.
- Close or annotate **`HM-OPEN-074`** and **`HM-OPEN-075`** with what this unit
  measured about each, since both were opened naming step 4 as what they were owed
  by. **Do not close either on an argument - only on a number.**

### Task 7 - fine sync underneath combining. THIS IS THE DROP CANDIDATE

**If the night runs short, this is what is shed, and the report says it was.**

`HM-OPEN-075` says placement jitter took unit 247's combined column from 200 of 306
to 55 of 306, and named this step as the work that would recover it. **Test that
claim, once:** `Ft8LadderHarness.RunRepeats` at `:425` unmodified, at **-21 dB, 306
trials, with the same 2.00 Hz and 480-sample jitter unit 247 used**, with fine sync
**on** underneath combining. Report the trials only the combination decoded, against
unit 247's 55, with the wrong count on the same line.

**Dropping this costs the phase one answer about an open issue, not a criterion.**
No step 4 exit needs it, step 6 is already `done`, and tasks 1 to 5 are the whole of
step 4. **If it is dropped, say so in section 1 and say why.**

### Both suites, every unit

`PHASE_PLAN.md`: `dotnet test tests/Ft8Sharp.Tests` and `dotnet test
tests/Ft8Sharp.Deep.Tests`, **whole, one project at a time and never
concurrently.** Baseline before your first code change and totals after. Unit 247
left them at **593 passed / 0 failed / 1 skipped / 8 m 12 s** and **51 passed / 0
failed / 0 skipped**; a different baseline is itself a finding. **Do not run
`Hamlet.App.Tests` or `Hamlet.RadioEngine.Tests`** - nothing here touches either.

---

## Parked - do not touch, do not raise

- **Ordered statistics decoding.** Step 2, `partial` and open on its own second
  exit. **Do not change `Ft8DeepOsdSettings.Default`, do not sweep orders, and do
  not re-measure unit 246's ceiling.** OSD appears as one regression column in task
  5 and is otherwise left exactly as it is.
- **Combining.** Step 6, `done`. Not a line of `Ft8DeepSoftCombiner`,
  `Ft8DeepCombineSettings` or `Ft8DeepRepeatDecoder` changes tonight. Task 7 *uses*
  them through the existing entry point and changes neither.
- **Subtraction and per-message SNR.** Steps 3 and 5. Not one line. If a measurement
  says the decibel is somewhere those steps live, **that is a finding to report, not
  a step to start.**
- **Widening the candidate search.** `HM-OPEN-074`'s trials with no candidate near
  the signal at all cannot be helped by refining a candidate that does not exist.
  **Say that where it bears on a number; do not go and change `Ft8SyncSearch`, which
  is in the port and untouchable anyway.**
- **`Ft8Sharp.Deep`'s licence.** Ruled GPL-3.0. Do not raise it.
- **Step 0's `partial` header** and `PHASE_PLAN.md`'s stale step-0 wording. Decided
  five times already. Do not report either.
- **The `RULES_AT` mismatch** between `PROJECT_STATUS.md` and `CLAUDE.md` §1.
  Reported once by unit 246, as instructed. **Do not report it again and do not
  reconcile it** - `CLAUDE.md` is the owner's file.
- **The shell permission fault and `allowed.txt`.** Banked and not blocking; the
  working spellings are at the top of this file. Do not probe it.
- **`HM-OPEN-071`'s missing per-message SNR**, owed by step 5. **`HM-OPEN-073`, the
  real capture fixture**, Tim's and deferred. Both gate nothing here.
- **The CW decoder**, the 419 dropped chunks, the 51 inherited failing cases, the
  engine project's missing total, the waterfall's late first row.

---

## What not to do

- **Do not touch `src/Ft8Sharp/`** - not a line of code and not `porting-notes.md`.
  The port is the instrument. **If `Ft8Sharp`'s version moves off `0.10.7`,
  something changed and that is a finding, not a bump.**
- **Do not change `Ft8LadderHarness.Run` or `RunRepeats`.** Both already take the
  placement arguments this unit needs. Every row this phase has recorded came
  through them.
- **Do not read WSJT-X source or `ft4_ft8_public/`**, and do not go looking for
  anyone's downconverter, resampler or fine-sync implementation. The second of the
  three things the arbiter may not reason past, and this is the step where the
  temptation is real.
- **Do not re-implement the Gray map, the bit partition or the ratio arithmetic.**
  Ruling 3. `Ft8SoftSymbols.ExtractSymbol` is public and pinned against upstream;
  a second copy in the sibling is a second thing to be wrong.
- **Do not let `Ft8Sharp.Deep` decide a message is real.** The port's two gates,
  always. A checksum re-implemented in the sibling is the worst line this unit could
  write.
- **Do not tell the decode path where the signal is.** `SearchFixture.Truth` is used
  in task 2's oracle measurement and **nowhere else** - not in task 3's search, not
  in task 5's scoreboard, not in any column that is scored. An oracle number is
  reported as an oracle number and is added to no total.
- **Do not trade a wrong decode for a rate.** Whatever the rate. If an approach
  produces one, reject that approach and take another, and report both.
- **Do not credit fine sync with a gain measured at a different placement from the
  baseline it is compared against.** Task 5's pairing rule exists for exactly this.
- **Do not claim an improvement that is not on the scoreboard.** No rate without its
  trial count, its Wilson interval and its wrong count. No crossing without saying
  it is an interpolation over which rungs.
- **Do not report a rate that moved without saying what it cost in milliseconds.**
- **Do not skip the everything-off identity test to save time.** It is unit 246's
  ruling 4 carried forward twice and it is the whole value of the seam.
- **Do not compose a timestamp.** Read the clock. Unit 247 recorded getting this
  wrong.
- **Do not stop because the shell refused something.** Record it, switch tools,
  continue.
- **Do not stop because the crossing did not move.** A measured null on this step,
  with task 1's spread and task 2's oracle ceiling behind it, is this unit's
  finding and the phase carries it. **The one thing that would waste this night is
  not measuring.**

---

## Committing and pushing

Commit and push each task before starting the next, on `main`, which is trunk.
**Commit `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `RUN_LEDGER.md` with your first
commit.** Root version `1.12.50` to **`1.12.51`** if anything was committed;
`Ft8Sharp.Deep` `0.3.0` to **`0.4.0`**, because it grows a capability.
**`Ft8Sharp` stays `0.10.7`.** If nothing could be committed, do not bump and say
why.

Append this unit's entry to `PHASE_OUTCOME.md` through `dotnet build
tools/arbiter/outcome-append.proj`, with `-p:EntryProps=` an **absolute path with
forward slashes**. **Use the tool rather than writing the entry by hand** - it
updates the header's step state in the same call. **No apostrophes in any field.**
If it refuses, write the entry in exactly the format the existing entries use,
update the `STEP: 4` header line yourself, and say in the report that you did.

Validate `output.md` through `dotnet build tools/arbiter/validate-output.proj`
before you finish, and report the rule count and the exit code.

---

## Reporting

`output.md` at the repository root, overwritten, four sections per `CLAUDE_CODE.md`
§8.

**First, above everything, the ordering block. `validate-output.bat` refuses a
report without it.** Three parts, every line specific to this unit:

- **A - THE PHASE GOAL**, and the state of all seven steps as this unit leaves them.
  The phase is the 1.5 dB between -19.5 and -21 and then past it, and **step 4 is
  the step whose exit is the crossing itself**. Say where steps 0 and 1 stand; that
  step 2 is `partial` at **10.8 per cent (33 of 306), 0 wrong** and was cut down
  rather than closed; that step 6 is `done` at 22.2 per cent jittered; that steps 3
  and 5 are open and untouched; and **what this unit did to the number step 4 is
  judged on** - the 50 per cent crossing, before and after, with the rungs the
  interpolation used, and the wrong count on the same line.
- **B - THIS STEP AND ITS EXIT CRITERIA.** Step 4's four must-pass exits, **one by
  one, with met or not met against each**: coarse candidates mixed to baseband,
  filtered and re-synced at sub-symbol time and sub-hertz frequency before
  extraction; the ladder's 50 per cent crossing moving down, measured, with the
  figure and its trial count; zero wrong decodes; and the gain quoted separately
  from steps 2 and 3 on the scoreboard. **Step 4 has no deferred exit and needs no
  fixture.** If an exit is not met, say which and what is needed - not a summary of
  effort.
- **C - THIS REPORT**, weighed against A and B: what it found that bears on the goal
  and the criteria - **task 1's placement sweep and task 2's oracle ceiling are the
  things here**, because between them they say how much of the missing 1.2 dB is
  sitting in the coarse grid at all, and they were both measured before the search
  was written - **how many items section 4 raises**, and **whether any of them stands
  in the way of an exit criterion in B.** An item that asks for no ruling is logged
  there as logged.

Then the six-line header: `UNIT`, `PHASE GOAL`, `UNIT GOAL`, `ADVANCED`, `NUMBER`,
`DRIFT`. **`NUMBER` for this unit is the 50 per cent crossing in decibels for each
column at the on-grid placement, and the -21 dB rate out of 306 with its wrong
count** - plus both suites' totals. **A rate without its wrong count is not a number
this project prints.**

**Section 3 leads with five things, in this order:**

1. **Task 1's placement sweep** - the 16-placement table at -20 dB, the two corners
   at 306 trials, and the placement-averaged rate beside the on-grid one. **State
   plainly how much rate the coarse grid's cell costs.**
2. **Task 2's control and oracle ceiling** - the new extractor against the port's at
   the same grid position first, then the three distance rows, against the code's
   recovery threshold of about 17. **The control comes first because nothing after
   it means anything if the extractor is worse.**
3. **Task 5's tables, whole** - three rungs, three columns, both placements, three
   counts each, with the interpolated crossings and what moved.
4. **Task 3's search behaviour and task 4's submission arithmetic** - the step in
   each axis, the edge-hit rate, the distribution of offsets the search actually
   applied, candidates re-synced, submissions made, expected false accepts against
   observed wrong decodes, and the worst-case slot time with its margin against
   15 seconds.
5. **Both suites' totals**, and whether any red is outside the expected set.

**Section 4 says, in one line, whether step 4 is closed, and if it is not, what the
measurements say is in the way** - which is what decides whether the next unit takes
another approach at step 4 or the arbiter moves to step 3 or step 5. **If task 7 was
dropped, say so there too.**

Write `output.md`, then stop. Do not start the next unit.

---

```
ARBITER-DECISION
STEP: 4
APPROACH: per-candidate baseband re-sync in Ft8Sharp.Deep - mix each coarse candidate to complex baseband, low-pass filter and decimate, search sub-symbol time and sub-hertz frequency by Costas correlation, and extract the soft values there through the port own Ft8SoftSymbols.ExtractSymbol before submitting to the port own parity and CRC-14 gates
MOVE: continue
WHY: step 4 is the only unattempted step whose own must-pass exit is the 50 per cent crossing, which is the number this phase is named for, and its entry is satisfied because step 1 is done. The argument unit 247 used to set it aside does not survive the tree - AlignmentSweep.Point carries integer grid indices only, so unit 222 oracle alignment was oracle selection on the coarse grid and no measurement in this repository has ever extracted a symbol below that grid. The loop test returns NOT FOUND and the five approaches on record are a shell probe, a fixture format, a delegating sibling, ordered statistics decoding and soft combining, none of which touches synchronisation.
STATE: not started
DECIDED: five. First, that step 4 is taken ahead of steps 3 and 5 and that steps 2 and 6 stay closed, licensed by the plan leave to reorder steps 2 3 4 and 5 on measured evidence; step 3 is not taken because the ladder synthesises one transmission a slot so subtraction has nothing to subtract on the instrument this phase measures on, and step 5 is not taken because it moves no decibel. Second, that step 4 entry is satisfied on step 1 being done at four of four with a separate session verdict, and that steps 0 and 1 are not re-audited. Third, that the sibling computes eight tone magnitudes at the re-synced position and hands them to the port own Ft8SoftSymbols.ExtractSymbol rather than re-implementing the Gray map and the bit partition, so the one named change is where the magnitudes are measured from. Fourth, that fine sync runs only where the port gates refused the coarse candidate and adds at most one submission per candidate, which bounds the false-accept arithmetic at about twice today and makes the superset property assertable. Fifth, that the sibling gets a samples-carrying entry point while the waterfall-only entry does no re-sync and reports that through a count rather than throwing or pretending.
LICENCE: PHASE_PLAN.md step 4 and its four must-pass exits - coarse candidates mixed to baseband, filtered and re-synced at sub-symbol time and sub-hertz frequency before extraction; the ladder 50 per cent crossing moving down, measured, with the figure and its trial count; zero wrong decodes; and the gain quoted separately from steps 2 and 3 so three changes do not credit each other. Step 4 entry note that it depends on step 1 only and is independent of steps 2 and 3. The plan section that the steps are a hypothesis and not a contract, and its leave to reorder steps on measured evidence with the record as the constraint. The phase ruling that improvements live in Ft8Sharp.Deep and nothing changes a line of the port. The second of the three things the arbiter may not reason past, which is why the re-sync is textbook DSP cited as such and no downconverter is read. The third of the three, which is where the one-submission-per-candidate bound comes from.
ACCOMPLISHED: Hamlet will read each candidate at the time and frequency the signal is actually at rather than at the nearest corner of a 0.08 second by 3.125 hertz grid - or it will have measured, on its own instrument and against the message the ladder knows it transmitted, exactly how much of the missing 1.2 dB is sitting in that grid and how much is not. Either way the phase stops reasoning from unit 222 oracle alignment, which the tree shows was an oracle over grid indices and never asked the sub-grid question at all. Every codeword still passes the port own parity and CRC-14 gates under a stated bound of one extra submission per candidate, and the port stays byte-identical to upstream underneath it.
ADVANCES: step 4, and the criteria it moves are the first - coarse candidates mixed to baseband, filtered and re-synced at sub-symbol time and sub-hertz frequency before extraction - and the second, the ladder 50 per cent crossing measured with its trial count against the -19.54 dB the port reads today, with zero wrong decodes riding on both and the gain quoted in its own column so it is not credited to step 2 or step 6. It also carries evidence back to two open issues that name step 4 as what they are owed by, HM-OPEN-074 and HM-OPEN-075, and it settles whether the ladder default placement sitting exactly on a bin centre and exactly on a sub-block boundary has been flattering every figure this phase has recorded.
END-ARBITER-DECISION
```
