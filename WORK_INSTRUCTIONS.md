# Work instruction 247 - the same transmission heard twice, combined before the decoder sees it

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

Carried forward from units 244, 245 and 246 unchanged, because it is measured and
it still holds. **Two different faults behave oppositely and have been conflated
before.**

| | What it is | How it announces itself | The answer |
|---|---|---|---|
| **A - the allow-list** | a fixed set of permitted command prefixes, matched against the command **as typed** | `This command requires approval` | **there is usually a permitted spelling that works.** Find it before concluding anything |
| **B - the sandbox** | the shell may not create files or directories | `... was blocked. For security, Claude Code may only write to files in the allowed working directories for this session` | **there is no shell spelling that works.** Use `Write`/`Edit` |

`docs/shell-probe-243.md` has all eight probes verbatim. What runs: `dotnet build
<path>`, `dotnet test <path>`, `git` throughout, `ls`, `grep`, `sed`, `head`,
`tail`, `find`, `wc`, `date`. What never runs: `mkdir`, `cp`, `mv`, `git mv`,
`dotnet sln add`, `dotnet run`, `git clean`, any redirect write, and any compound
line where one part is refused. `dotnet --version` is refused and **is not
evidence about the toolchain.**

Four spellings earlier units paid for, banked here so this unit does not pay
again: **`dotnet sln add` is refused** - edit `Hamlet.sln` with the file tools;
**`git clean` is refused** - a one-shot `.proj` deletes itself with an MSBuild
`<Delete>` task; **`-p:EntryProps=` needs an ABSOLUTE path with FORWARD slashes**,
e.g. `C:/Source/HamLet/.run-unit/scratch/unit247-outcome.props`, because a
relative path resolves against `tools/arbiter/` and Git Bash eats backslashes; and
**`git commit -F -` with a heredoc is refused by the static analyser** - write the
message to a file under `.run-unit/scratch/` and use `git commit -F <file>`.

**`tools\arbiter\*.bat` is a closed loop.** Go through `dotnet build
tools/arbiter/outcome-append.proj` and `dotnet build
tools/arbiter/validate-output.proj`. **No apostrophes in any `PHASE_OUTCOME.md`
field** - one breaks the tool's PowerShell parse.

**A refused shell call is a signal to reach for the other tool, not to stop.**
Nothing in this unit halts the loop.

---

## Why this unit exists

**This is unit 247. It is the fifth unit of this phase, and the first aimed at
step 6 - the step that is the phase's second half.**

Steps 0 and 1 are closed. Step 2 is `partial` and stays open: unit 246 built an
ordered statistics decoder in `Ft8Sharp.Deep` and moved the -21 dB rate on the
306-trial ladder from **4.2 per cent (13 of 306), 0 wrong** to **10.8 per cent (33
of 306), 0 wrong**, Wilson 7.8 to 14.8, at 72.5 ms a trial against the port's
64.1 - five of step 2's six must-pass exits met, and the 50 per cent crossing
moved from -19.54 dB to -19.81 dB. **About 0.27 dB of the 1.5 is closed. About
1.2 dB is not.**

### The arbiter's move on step 2, and the reason it is not this unit's subject

**Step 2 is cut down, not declared unachievable, and it is not re-attempted
tonight.** Unit 246's task 1.3 measured the ceiling **before** a line of OSD was
written, against the codeword the ladder knows it transmitted, over one whole
51-trial block at -21 dB: the closest candidate carries a median **31 of 174**
hard-decision errors, of which a median **6** fall inside the 91 most reliable
positions. That distribution admits at most **13.7 per cent** of trials at order 2,
**19.6 per cent** at order 3 and **31.4 per cent** at order 4 - and order 4 is
about two and a half million re-encodings a candidate. **No tractable order reaches
step 2's 40 per cent**, and both caveats on that count run in the direction of
worse. Taking another order tonight is tuning a measured ceiling, not a new
approach.

**Step 2 keeps its `partial` state on the authority of its own second exit** - the
step stays open while the number is moving, and 4.2 to 10.8 per cent is a move
outside its own interval. It is not closed `unachievable`, because the thing that
would move it again is better evidence per bit, and **that is what this unit
produces.**

### Why step 6 and not step 4 or step 3, recorded as `PHASE_PLAN.md` requires

`PHASE_PLAN.md` step 6 says, in the plan's own words: *Best taken after steps 2 and
4, and **not gated on them** - if they stall, this is the step to try instead.*
Step 2 has stalled against a measured ceiling. This is that case, named in advance.

- **Not step 4.** The plan sizes it *worth a fraction of a decibel on its own*, and
  the phase description records that unit 222 already tried **oracle alignment** -
  telling the decoder exactly where the signal was - and got a result **inside the
  as-is 95 per cent interval**, along with unquantised magnitudes and four times the
  iteration bound. Step 4 is not retired and `HM-OPEN-074` keeps its argument alive
  - 2 of 51 trials had no candidate within 60 of the truth at all - but two trials
  cannot account for a decibel and the plan's own estimate of the step is smaller
  than what is missing.
- **Not step 3.** The ladder synthesises **one** transmission a slot
  (`SearchFixture.OneSignal`). Subtraction has nothing to subtract on the
  instrument this phase's number is measured on; its value is crowding rather than
  sensitivity, and its real-air criterion needs a fixture nobody has.
- **Step 6 is worth more than the whole remaining shortfall.** Two repeats
  combined is 3 dB of processing gain and four is 6, against 1.2 dB still out
  there. **It closes entirely on synthesized signals** - the ladder knows what it
  transmitted - so it needs no radio, no WSJT-X and nothing from Tim. And its
  gain and OSD's stack: a combined ratio vector is what OSD gets handed next.

**It is also the step the plan calls most likely to fail**, which is an argument
for starting it on a full night rather than on the last one.

```
PHASE GOAL:   Hamlet reads FT8 as well as the best decoder there is, and then
              reads it further.
UNIT GOAL:    Ft8Sharp.Deep identifies the same transmission repeated in a later
              slot at the same frequency, adds the two slots' log-likelihood
              ratios before anything decodes them, and recovers a message that
              neither slot could give up alone - with every combined codeword
              accepted or refused by the port's own parity and CRC-14 gates, and
              a stated, bounded number of submissions to those gates.
ADVANCES:     step 6. The identification and combination of a repeat before
              decoding; a message decoded that no single slot could decode alone,
              at a stated SNR below the single-slot crossing; zero wrong decodes;
              the gain measured on the ladder with its trial count; and every
              combined decode verified against the ladder's own ground truth.
```

**This unit is not required to reach any particular rate.** What it is required to
do is measure, on step 0's instrument, whether soft combining reaches messages a
single slot cannot - and if it does not, to say so with the distribution that says
why. **Task 1 is that measurement and it comes before any code.**

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Every line below was read from it while this
instruction was written. **Check each one and report mismatches in section 1; do
not repair them and do not repair this instruction.**

The port's surface, all `public`:

- `src/Ft8Sharp/Dsp/Ft8SoftSymbols.cs`: `RatioCount` at `:73`, `Extract(Ft8Waterfall,
  Ft8Candidate, Span<float>)` at `:117` writing **174 ratios in codeword bit
  order, positive meaning the bit is more likely 1**; `Normalise(Span<float>)` at
  `:287` returning the variance; `Variance` at `:323`; `HardDecision` at `:351`.
- `src/Ft8Sharp/Ldpc/Ft8CodewordDecoder.cs`: `Decode(ratios, cache,
  maxIterations)` at `:70`, **GATE 1 parity at `:80` and GATE 2 the checksum at
  `:96`**, both commented as such in the file.
- `src/Ft8Sharp/Dsp/Ft8Candidate.cs:48`: `Ft8Candidate(int Score, int BlockOffset,
  int TimeSubOffset, int BinOffset, int FrequencySubOffset)`, with
  `FrequencyHz(geometry)` at `:93` and `TimeSeconds(geometry)` at `:103`.
- `src/Ft8Sharp/Dsp/Ft8Waterfall.cs`: **magnitudes only, quantised to bytes at
  0.5 dB** (`DecibelsFor` at `:65`). There is no phase and there are no samples in
  it. `src/Ft8Sharp/Dsp/Ft8Monitor.cs:211` `Analyse(ReadOnlySpan<float> samples)`
  is how audio becomes one.
- `src/Ft8Sharp/Dsp/Ft8SlotDecoder.cs`: `Decode(ReadOnlySpan<float> samples)` at
  `:133`, `Decode(Ft8Waterfall)` at `:139`, `DefaultMessageLimit` 50 at `:63`.

The sibling as unit 246 left it:

- `src/Ft8Sharp.Deep/` holds `Ft8DeepSlotDecoder.cs`,
  `Ft8DeepOrderedStatistics.cs`, `Ft8DeepOsdCounts.cs`, `Ft8DeepOsdSettings.cs`,
  `porting-notes.md`, `LICENSE`, `NOTICE`.
- `Ft8DeepSlotDecoder.cs`: `Decode(ReadOnlySpan<float> samples)` at `:171`, which
  is `Decode(new Ft8Monitor(Geometry).Analyse(samples))`; `Decode(Ft8Waterfall)`
  at `:189`, the port's per-candidate loop reproduced through public members;
  `Osd` at `:130`, **null by default and null meaning do exactly what the port
  does**; `LastOsd` at `:149`; `Port` at `:124`.
- `Ft8DeepOsdSettings.Default` at `:86` is **order 2**; `MaximumOrder` 4 at `:44`.
- `tests/Ft8Sharp.Deep.Tests/Ft8DeepSlotDecoderTests.cs:181` asserts the sibling
  assembly's **whole type list** - `Ft8DeepOrderedStatistics`, `Ft8DeepOsdCounts`,
  `Ft8DeepOsdResult`, `Ft8DeepOsdSettings`, `Ft8DeepSlotDecoder`. **That is a
  tripwire unit 246 left in the same shape unit 245 left one for it. Changing it
  deliberately is this unit's job; discovering it afterwards is not.**

The instrument:

- `tests/Ft8Sharp.Tests/Dsp/Ft8LadderHarness.cs`: `Decoder(string Name,
  Func<float[], Ft8SlotResult> Decode)` at `:74`; `Result` at `:91` carrying the
  three counts, `Wilson` interval, `DeliveredMean`, `WorstDeliveryError` and
  `MillisecondsPerTrial`; `Header` at `:170`; `Available()` at `:194` returning
  two entries; `Run(rung, trials, seed, decoders?, frequencyHz?, offsetSamples?,
  log?)` at `:244`. `DefaultSeed` 221001 at `:61`, `DefaultFrequencyHz` 1000.0 at
  `:64`, `DefaultOffsetSamples` three symbols at `:69`. **Block `s` draws its noise
  from `seed + s + round(rung * 10)`.**
- **`Decoder` takes one slot and returns one result, so it cannot express a
  multi-slot decode.** A second entry point is needed; `Run` is the instrument this
  phase's number is measured on and is **not** to be changed.
- `tests/Ft8Sharp.Tests/Dsp/SearchFixture.cs`: `OneSignal(rate, entry,
  baseFrequencyHz, offsetSamples)` at `:84`, `AddNoise` at `:152`,
  `TransmissionPower` at `:174`. `GaussianNoise(seed)` and
  `SignalToNoise.NoiseAmplitudeFor` / `DecibelsFor` are beside them in the same
  folder.
- `tests/Ft8Sharp.Tests/Dsp/Ft8Step6Ladder.cs`: `Population()` at `:160` - **51
  scoreable messages**; `CollapseBottomDecibels` **-24.0** at `:119`; `Wilson` at
  `:255`. **The type is named for the previous phase's step 6 and has nothing to do
  with this phase's step 6.**
- `tests/Ft8Sharp.Tests/Encode/EncodeCorpus.cs:57`: `Entry(Label, Kind, byte[]
  Message, ...)`, where `Message` is **the 77 bits that went on the wire**.
- `docs/unit246-osd.md` carries the ceiling distribution and the order table.
  **Read it before task 1 and do not re-measure what it already measured.**

Versions and bookkeeping:

- Root version `1.12.49` at `Directory.Build.props:145`. `Ft8Sharp` **`0.10.7`** at
  `src/Ft8Sharp/Directory.Build.props:396`. `Ft8Sharp.Deep` `0.2.0` at
  `src/Ft8Sharp.Deep/Directory.Build.props:46`.
- The highest issue id in `OPEN_ISSUES.md` is `HM-OPEN-074`.
- `PHASE_STATUS.md` and `PHASE_OUTCOME.md` head `STEP: 0` **`partial`** while the
  last `## UNIT 2 - STEP 0` entry reads `done`. **Do not re-audit step 0, do not
  reconcile the header, and do not report the disagreement a fifth time.**
- `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `RUN_LEDGER.md` were modified and
  uncommitted at the root when this was authored. **They are the loop's own
  bookkeeping - commit them with your first task's commit and say you did.**
  `.run-unit/` is the launcher's; leave it.

**Going-in test baselines, from unit 246:** `Ft8Sharp.Tests` **586 passed / 0
failed / 1 skipped / 5 m 23 s**; `Ft8Sharp.Deep.Tests` **35 passed / 0 failed / 0
skipped / 997 ms`. **A different baseline is itself a finding.**

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
Improvements live in `Ft8Sharp.Deep`. The port's value now is that it cannot
drift: every measurement in this phase is taken against something
known-identical to upstream, so a regression in the sibling is always visible.

**`Ft8Sharp.Deep` is GPL-3.0**, carrying its own `LICENSE` and a `NOTICE` citing
the published sources it implements. Ruled by Tim, 2026-09-04. **No unit raises
the licence and no step is held by it.**

**No algorithm comes from WSJT-X's source or `ft4_ft8_public/`.** Published
description only - Fossorier and Lin 1995 for ordered statistics, and the QEX
paper (Franke K9AN, Somerville G4WJS, Taylor K1JT, "The FT4 and FT8 Communication
Protocols," QEX, July/August 2020) - cited at the point of use. **WSJT-X is a
measuring instrument in this phase and never a source.** This is the second of the
three things the arbiter may not reason past.

**Transmit.** `CLAUDE.md` §0.2 is untouched. **Nothing in this phase keys the
radio.** This step synthesises signals as test oracles and they never reach a
transmitter. This is the first of the three.

**What Hamlet asserts to Tim.** §12.1 and §0.0. **A decode this phase produces
that nobody sent is worse than a decode it misses.** If an approach produces a
wrong decode, **that approach is rejected and another is taken** - the step does
not close and does not stop. This is the third of the three.

**There is no WSJT-X on the development machine and no unit may assume one.** A
unit that cannot close without a real-air comparison says so; it does not
substitute `decode_ft8.exe`, which is `ft8_lib` and therefore the thing being
improved on. **Step 6's sixth exit - decodes WSJT-X did not return on a real
capture - is `deferred` by the plan itself and gates nothing tonight.**

**Nothing is claimed without the scoreboard.** No unit in steps 1 to 6 may report
an improvement except as a number on step 0's instrument. A decode rate quoted
without it is not evidence.

**A wrong decode is counted separately from a missed one, everywhere, in every
report.**

**The steps are a hypothesis, not a contract.** `PHASE_PLAN.md` grants leave to
reorder, replace, retire and add steps and to split or re-scope criteria, with the
record in `PHASE_OUTCOME.md` as the only constraint. **A step whose number does not
move is evidence, not a halt.**

### Four things the arbiter decided, so this unit does not spend a night on them

1. **Step 6 is taken now, ahead of steps 3, 4 and 5, and step 2 is not
   re-attempted.** Licensed by `PHASE_PLAN.md`'s leave to reorder steps 2, 3, 4 and
   5 on measured evidence from the scoreboard, and by step 6's own entry note that
   it is the step to try when 2 and 4 stall. The evidence is unit 246's ceiling
   distribution. **Do not re-argue the ordering and do not open step 2, 3, 4 or 5.**

2. **Step 6's entry criterion is satisfied.** It reads *step 1 complete*, and step 1
   is `done` at four of four with `STATE_AFTER: done` from a separate session.
   Nothing else gates it. **Do not re-audit step 0 or step 1.**

3. **Soft combining, not coherent combining.** The plan says *coherent or soft*.
   Take soft: the waterfall carries **quantised magnitudes and no phase**, so
   coherent combining across slots would require a second path down to the samples
   that this unit has no time to build and no measurement to justify. Adding
   log-likelihood ratios for the same codeword bit across independent observations
   is the whole of the 3 dB. **If task 1 measures that soft combining does not
   reach, say so with the distribution - do not reach for coherent combining
   instead.**

4. **The combined vector is re-normalised through the port's own
   `Ft8SoftSymbols.Normalise` before it is submitted.** That file records that belief
   propagation is **not scale-free** - `fast_tanh` has a hard clamp - so a summed
   vector sitting at a larger scale than upstream's is a different experiment, not a
   better one. Sum, then normalise, then submit. Say in the report what the summed
   variance was before normalisation.

---

## Status cadence

After each task, before starting the next, update `PROJECT_STATUS.md` per
`CLAUDE.md` - `STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, and
`NOTE` saying what is moving inside the task. The same every ten minutes while a
task is running. **Use the file-editing tools if the shell refuses.**

**A long measurement is a status update, not a silence.** Tasks 1 and 6 each walk
whole 51-trial and 306-trial blocks with two slots a trial; say so in `NOTE`
before you start them.

---

## Tasks

Seven tasks. **Task 1 is a trace and comes first**, because this unit must measure
whether combining can reach anything before it writes a line of combining. **Task 7
is the named drop candidate.**

**Start the `Ft8Sharp.Tests` baseline run early** - it was 5 m 23 s for unit 246
and it can run while task 1 is being written.

### Task 1 - the trace: does adding two slots' ratios reach where one cannot

**Reading and measuring only. Nothing is combined in production code in this
task.** Report each with file and line, and **say what you find, not what this
instruction expects.**

1. **Two independent hearings of one transmission.** For each of the 51 population
   entries at **-21 dB**, synthesise the slot **twice** - the same `OneSignal`
   clean audio at `DefaultFrequencyHz` and `DefaultOffsetSamples`, and **two
   different noise draws** from two different `GaussianNoise` seeds. Report the
   delivered SNR of each, so the pair is known to be the same rung and not two
   rungs.

2. **The distance, three ways.** The ladder knows what it transmitted:
   `EncodeCorpus.Entry.Message` through `Ft8Payload.Create` and
   `LdpcEncoder.Encode` gives **the true 174-bit codeword** - the chain unit 246
   used at its task 1.1 and verified against 500 random payloads. For each of the
   51 trials, take the **closest candidate in each slot** by hard-decision distance
   to the truth, and report the distribution over the 51 trials of:
   - **slot A's closest distance**, and **slot B's closest distance** - these should
     land near unit 246's median of **31 of 174**, and if they do not, that is a
     finding about the harness before it is a finding about combining;
   - **the distance of the summed vector** - slot A's normalised ratios plus slot
     B's normalised ratios, re-normalised, hard-decided against the same truth.

   **This is the number that decides the night.** The code's iterative recovery
   reaches zero at about **17**. If the summed distance falls from a median of 31
   to below 17 on a useful fraction of trials, belief propagation will converge on
   the combination and step 6 works. **If it does not fall, report the distribution
   and say so plainly - that finding is worth more to this phase than a combiner
   that decodes nothing**, and the arbiter will re-order the plan on it.

3. **The same measurement at -24 dB**, `Ft8Step6Ladder.CollapseBottomDecibels`,
   which is **below the single-slot crossing of -19.81 dB** by a margin that makes
   *no single slot could decode alone* unambiguous. Report both the single-slot
   distances and the combined distance at that rung.

4. **The pairing, measured before it is designed.** Over those 51 trials at -21 dB,
   report how far apart the two slots' closest candidates are in
   `FrequencyHz(geometry)` and `TimeSeconds(geometry)`, and **how often the closest
   candidate is not the highest-scoring one**. A pairing rule that only works on the
   best-scoring candidate in each slot is a different rule from one that works on
   the closest, and this unit must know which it is building.

### Task 2 - the combiner core, on its own, with tests on synthesized ratios

`src/Ft8Sharp.Deep/`, a new type. Not wired into any loop yet.

- Given two or more 174-ratio vectors in the port's convention - **positive means
  more likely 1** - and the pairing already decided, produce one combined vector:
  **normalise each input through `Ft8SoftSymbols.Normalise`, add them position by
  position, and normalise the result** (ruling 4).
- **State and justify the weighting.** Equal weight is optimal when both slots
  carry the same SNR, which is what the ladder delivers; on real air it is not.
  Report whether you weighted by each vector's pre-normalisation
  `Ft8SoftSymbols.Variance` or took equal weight, and **which one the measurement
  chose** - not which one reads better.
- Cite the QEX paper in `src/Ft8Sharp.Deep/porting-notes.md` and in XML remarks at
  the point of use for the FT8 frame and CRC-14 facts this depends on. **The
  combining arithmetic itself is standard soft-decision theory and is cited as
  such** - it does not come from anyone's source.

Tests, in `tests/Ft8Sharp.Deep.Tests`, on synthesized ratios rather than on audio:

- two copies of one codeword with **independent planted errors** combine to a
  vector whose hard decision has strictly fewer errors than either input, over a
  spread of error counts;
- **two vectors from different codewords do not**: the combination is no closer to
  either than its inputs were. This is the wrong-pairing case and it is the one the
  gate has to catch;
- combining a vector with **itself** doubles nothing after re-normalisation - the
  result's hard decision is identical to the input's. **A combiner that reports a
  gain from hearing the same slot twice is measuring its own arithmetic**;
- it never throws on degenerate input - all zero, all equal, all infinite, all
  not-a-number - because it will be called on noise.

### Task 3 - the pairing rule and the submission budget. THIS IS WHERE THE STEP FAILS QUIETLY

**This is §0.0 and it is the criterion this step cannot trade.**

- **A combined codeword is submitted to `Ft8CodewordDecoder.Decode` as the
  candidate's ratios and the port's parity gate and CRC-14 gate are the only
  acceptance.** Nothing in `Ft8Sharp.Deep` decides that a message is real.
- **State the pairing rule and bound it.** Which candidate in the earlier slot is
  paired with which in the later one, and on what tolerance in frequency and time.
  A transmitter repeating a message does not move by a tone between slots; task
  1.4 measured how far it does move.
- **State the submission arithmetic, in the report, in full.** Every codeword put
  to the CRC-14 is an independent chance of a false accept at about **one in
  16,384**. Pairing every candidate with every candidate is up to **140 × 140 =
  19,600 submissions a slot pair**, which is about **1.2 expected wrong decodes per
  trial** and would put wrong messages in front of Tim within one rung. Unit 246
  spent 11,451 submissions across the whole 918-trial ladder for **0 wrong**, where
  the naive arithmetic predicts 0.70 - **so the naive figure is an upper bound
  because GATE 1 must converge first, and an upper bound is what a budget is set
  from.** Report **submissions per slot pair and submissions across the whole
  measurement**, and set the rule so the expected count is well under one.
- **The dedup key for a combined decode is the codeword the combination produced** -
  its first 77 bits - not a re-run over either slot's original ratios.
- A test that hands the gate a **deliberately wrongly-paired** combination and
  watches the port refuse it, **in the port's own words, quoted in the report.**

### Task 4 - the combiner in the loop, without disturbing the single-slot path

- A public type in `Ft8Sharp.Deep` that is fed slots **in order**, returns the
  ordinary single-slot `Ft8SlotResult` for each, and then attempts combinations
  against a **bounded history of previous slots**. **State how many slots back it
  keeps and what that costs in memory and time.**
- **The result of a slot with combining on must be a superset of the result with
  combining off**: every message the single-slot path returned is still there, in
  order, unchanged. **Combining only ever adds.** Assert it.
- **Combining is off by default**, and off means the sibling does exactly what it
  did at unit 246 - which, with `Osd` also null, is exactly what the port does.
  **The OSD-off, combine-off whole-`Ft8SlotResult` identity test against
  `Ft8SlotDecoder` is not optional and is not to be weakened.** It is what keeps
  the instrument an instrument.
- Count the combiner's own outcomes separately - **pairs offered, combinations
  submitted, codewords the port then accepted** - and print them beside the counts
  the port and OSD already return. **A rate that moved with no visible combining
  activity behind it is not evidence.**
- **Change `Ft8DeepSlotDecoderTests.cs:181`'s type-list assertion deliberately** to
  whatever the sibling now holds, and say in the report that you changed a tripwire
  unit 246 left rather than that a test broke.

### Task 5 - the repeats ladder: a second entry point, not a changed one

- A new entry point beside `Ft8LadderHarness.Run` that walks a rung with **R slots
  per trial** carrying the same message at the same frequency, each with its own
  noise draw, and scores three columns: **single slot** (the first slot alone),
  **single slot + OSD**, and **combined across R slots**. Reuse `Result`, `Header`
  and `Wilson` so the scoreboard stays one instrument and prints one shape.
- **`Run` at `:244` is not to be modified.** It is what this phase's -21 dB number
  is measured on and a change to it invalidates every row already recorded.
- **The seeds must differ between the R slots and be deterministic**, so a fresh
  process draws the same noise. Say what the arithmetic is, in the same shape as
  `Run`'s `seed + block + round(rung * 10)`.
- **A harder variant, and it is not optional: the repeats must not be
  bit-identical in placement.** Give the later slot a different `offsetSamples`
  and a small frequency offset from the earlier one, as a real station's clock and
  oscillator would. **Report the gain both ways.** A combiner that only works when
  the two slots sit on the same sample is not a decoder, and finding that out here
  is worth more than a larger number.

### Task 6 - the scoreboard, whole, and the time budget

**Nothing is claimed without this.**

- **The repeats ladder at -21 dB and at -24 dB, 306 trials each**, three columns,
  **three counts on every row, never two.** Quote both tables whole, with delivered
  SNR and its worst delivery error.
- **The number this step is judged on: how many trials neither single slot could
  decode alone and the combination did**, counted per trial and stated with its
  trial count. Report the converse too - **any trial the single slot decoded and
  the combination did not** - because ruling task 4's superset property makes that
  count zero by construction and a non-zero one is a defect.
- **Zero wrong decodes across every rung and every column**, or the approach is
  rejected and the report says which rung produced it, with the message sent beside
  the message returned. `WrongReturn` already prints that line.
- **Every combined decode verified against the ladder's own ground truth** - the
  message that went in - which is step 6's fifth must-pass exit. Say how many
  combined decodes there were and that every one was checked.
- **The regression check, cheap:** the OSD-on, combine-off column at **-21 dB over
  306 trials** must still read **10.8 per cent (33 of 306), 0 wrong**. One column at
  about 72 ms a trial is roughly 22 seconds and it proves nothing tonight moved
  step 2's number underneath the new one.
- **Worst-case time per slot with the margin stated against 15 seconds.** Take the
  worst single slot observed, not the mean, and say the candidate count and the
  number of combinations it carried. The port sits at about 64 ms and OSD at about
  72.

### Task 7 - the write-up and the record. THIS IS THE DROP CANDIDATE

**If the night runs short, this is what is shed, and the report says it was.**

- `docs/unit247-combining.md`: task 1's distance distributions and task 6's tables
  written up, so the next unit on step 6 does not re-measure them.
- If task 1 found that summed distance does not fall below the code's recovery
  threshold, or that the pairing tolerance the ladder needs is wider than a real
  station's drift, **open an `OPEN_ISSUES.md` entry at the next free id**
  (`HM-OPEN-075` unless something took it) naming what it means for step 6. **If
  the numbers said nothing of the kind, open nothing and say why** - an empty issue
  is worse than none.

**Dropping this costs the phase a document, not a criterion. Tasks 2 to 6 are step
6's must-pass exits, and task 1's numbers still go in section 3 even if this task
is dropped** - what is shed is the write-up, not the measurement.

### Both suites, every unit

`PHASE_PLAN.md`: `dotnet test tests/Ft8Sharp.Tests` and `dotnet test
tests/Ft8Sharp.Deep.Tests`, **whole, one project at a time and never
concurrently.** Baseline before your first code change and totals after. Unit 246
left them at **586 passed / 0 failed / 1 skipped / 5 m 23 s** and **35 passed / 0
failed / 0 skipped / 997 ms**; a different baseline is itself a finding. **Do not
run `Hamlet.App.Tests` or `Hamlet.RadioEngine.Tests`** - nothing here touches
either.

---

## Parked - do not touch, do not raise

- **Ordered statistics decoding.** Step 2, cut down to `partial` and open on its
  own second exit. **Do not change `Ft8DeepOsdSettings.Default`, do not sweep
  orders, and do not re-measure unit 246's ceiling.** OSD is switched on as a
  column in task 6 and is otherwise left exactly as it is.
- **Subtraction, baseband re-sync, per-message SNR.** Steps 3, 4 and 5. Not one
  line tonight. If task 1 says the decibel is somewhere those steps live, **that is
  a measurement to report, not a step to start.**
- **Coherent combining.** Ruled out for tonight by ruling 3, with the reason. Do
  not re-argue it.
- **`Ft8Sharp.Deep`'s licence.** Ruled GPL-3.0. Do not raise it.
- **Step 0's `partial` header** and `PHASE_PLAN.md`'s stale step-0 wording. Decided
  four times already. Do not report either.
- **The `RULES_AT` mismatch** between `PROJECT_STATUS.md` and `CLAUDE.md` §1.
  **Reported once by unit 246, as instructed. Do not report it again and do not
  reconcile it** - `CLAUDE.md` is the owner's file.
- **The shell permission fault and `allowed.txt`.** Banked and not blocking; the
  working spellings are at the top of this file. Do not probe it.
- **`HM-OPEN-071`'s missing per-message SNR**, owed by step 5. **`HM-OPEN-073`, the
  real capture fixture**, Tim's and deferred. **`HM-OPEN-074`**, the two trials with
  no candidate near the signal, owed by step 4. All three gate nothing here.
- **The CW decoder**, the 419 dropped chunks, the 51 inherited failing cases, the
  engine project's missing total, the waterfall's late first row.

---

## What not to do

- **Do not touch `src/Ft8Sharp/`** - not a line of code and not `porting-notes.md`.
  The port is the instrument. **If `Ft8Sharp`'s version moves off `0.10.7`,
  something changed and that is a finding, not a bump.**
- **Do not change `Ft8LadderHarness.Run`.** Add beside it. Every row this phase has
  recorded was taken through it.
- **Do not read WSJT-X source or `ft4_ft8_public/`**, and do not go looking for
  anyone's combining implementation. The second of the three things the arbiter may
  not reason past.
- **Do not let `Ft8Sharp.Deep` decide a message is real.** The port's two gates,
  always. A checksum re-implemented in the sibling is the worst line this unit could
  write.
- **Do not submit combinations without a bounded budget.** Task 3's arithmetic
  first, then the code. This is the specific way this step fails, and it fails
  quietly.
- **Do not trade a wrong decode for a rate.** Whatever the rate. If an approach
  produces one, reject that approach and take another, and report both.
- **Do not tell the decode path what was transmitted.** The truth is used once,
  after the code has answered, to compare the text - which is the ladder's own rule.
  A combiner that pairs slots by knowing they carry the same message is measuring
  nothing.
- **Do not claim an improvement that is not on the scoreboard.** No rate without
  its trial count, its Wilson interval and its wrong count.
- **Do not report a rate that moved without saying what it cost in milliseconds.**
- **Do not skip the OSD-off, combine-off identity test to save time.** It is ruling
  4 of unit 246, carried forward, and it is the whole value of the seam.
- **Do not stop because the shell refused something.** Record it, switch tools,
  continue.
- **Do not stop because the gain fell short.** A measured distribution that says
  combining does not reach is this unit's finding and the phase carries it.

---

## Committing and pushing

Commit and push each task before starting the next, on `main`, which is trunk.
**Commit `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `RUN_LEDGER.md` with your first
commit.** Root version `1.12.49` to **`1.12.50`** if anything was committed;
`Ft8Sharp.Deep` `0.2.0` to **`0.3.0`**, because it grows a capability.
**`Ft8Sharp` stays `0.10.7`.** If nothing could be committed, do not bump and say
why.

Append this unit's entry to `PHASE_OUTCOME.md` through `dotnet build
tools/arbiter/outcome-append.proj`, with `-p:EntryProps=` an **absolute path with
forward slashes**. **Use the tool rather than writing the entry by hand** - it
updates the header's step state in the same call. **No apostrophes in any field.**
If it refuses, write the entry in exactly the format the existing entries use,
update the `STEP: 6` header line yourself, and say in the report that you did.

Validate `output.md` through `dotnet build tools/arbiter/validate-output.proj`
before you finish, and report the rule count and the exit code.

---

## Reporting

`output.md` at the repository root, overwritten, four sections per
`CLAUDE_CODE.md` §8.

**First, above everything, the ordering block. `validate-output.bat` refuses a
report without it.** Three parts, every line specific to this unit:

- **A - THE PHASE GOAL**, and the state of all seven steps as this unit leaves
  them. The phase is the 1.5 dB between -19.5 and -21 **and then past it**, and
  **step 6 is the half that goes past**. Say where steps 0 and 1 stand; that step 2
  is `partial` at **10.8 per cent (33 of 306), 0 wrong** and was cut down rather
  than closed; that steps 3, 4 and 5 are open and untouched; and **what this unit
  did to the number that step 6 is judged on** - how many trials neither single slot
  could decode alone and the combination did, with the wrong count in the same line.
- **B - THIS STEP AND ITS EXIT CRITERIA.** Step 6's five must-pass exits and its one
  deferred, **one by one, with met or not met against each**: a repeat identified
  and its soft values combined before decoding; a message decoded that no single
  slot could decode alone at a stated SNR below the single-slot crossing; zero wrong
  decodes with every combined decode passing the same CRC-14; the gain measured on
  the ladder and quoted with its trial count; every combined decode verified against
  the ladder's own ground truth; and the real-capture exit, which needs a fixture
  nobody has and is deferred by the plan. **If an exit is not met, say which and
  what is needed - not a summary of effort.**
- **C - THIS REPORT**, weighed against A and B: what it found that bears on the goal
  and the criteria - **task 1's summed-distance distribution is the thing here**,
  because it says whether soft combining can reach a codeword at all before any
  ladder is walked - **how many items section 4 raises**, and **whether any of them
  stands in the way of an exit criterion in B.** An item that asks for no ruling is
  logged there as logged.

Then the six-line header: `UNIT`, `PHASE GOAL`, `UNIT GOAL`, `ADVANCED`, `NUMBER`,
`DRIFT`. **`NUMBER` for this unit is the count of trials that only the combination
decoded, out of 306, at both -21 and -24 dB, with the wrong count** - plus both
suites' totals. **A rate without its wrong count is not a number this project
prints.**

**Section 3 leads with four things, in this order:**

1. **Task 1's distances** - the distribution of the closest candidate's
   hard-decision error count in each of the two slots, and the distribution for the
   summed vector, at -21 and -24 dB, against the code's recovery threshold of about
   17. **State plainly whether soft combining reaches, and by how much.**
2. **The repeats ladder tables, whole** - single slot, single slot + OSD, and
   combined, at -21 and -24 dB over 306 trials, three counts each, both with and
   without the placement jitter of task 5. **The trials only the combination
   decoded, and the trials the single slot decoded and the combination did not,
   which should be zero.**
3. **The submission arithmetic** from task 3 - pairs offered, combinations
   submitted per slot pair and across the whole measurement, codewords the port
   accepted, and the expected false-accept count that budget implies - **and the
   worst-case slot time with its margin against 15 seconds.**
4. **Both suites' totals**, and whether any red is outside the expected set.

**Section 4 says, in one line, whether step 6 is closed, and if it is not, what the
distribution says is in the way** - which is what decides whether the next unit
takes another approach at step 6 or the arbiter moves to step 3, 4 or 5.

Write `output.md`, then stop. Do not start the next unit.

---

```
ARBITER-DECISION
STEP: 6
APPROACH: soft combining of a transmission repeated in a later slot at the same frequency - add the two slots normalised log-likelihood ratios, re-normalise, and submit the combination to the port own parity and CRC-14 gates under a bounded pairing budget
MOVE: continue
WHY: step 2 is cut down to partial rather than closed - unit 246 moved the -21 dB rate from 4.2 to 10.8 per cent with zero wrong and met five of six exits, and its ceiling measured before a line of OSD was written says no tractable order reaches 40 per cent, so another order tonight would be tuning a measured ceiling rather than a new approach. PHASE_PLAN.md step 6 says in advance that it is not gated on steps 2 and 4 and is the step to try instead when they stall. Two repeats is 3 dB against 1.2 dB still missing, it closes entirely on synthesized signals with no radio and no WSJT-X, and step 4 was sized by the plan at a fraction of a decibel and already had oracle alignment land inside the as-is interval at unit 222. The loop test returns NOT FOUND and the only approaches on record are a shell probe, a fixture format, a delegating sibling and ordered statistics decoding.
STATE: not started
DECIDED: four. First, that step 6 is taken ahead of steps 3, 4 and 5 and that step 2 is cut down to partial and not re-attempted, licensed by the plan leave to reorder steps on measured evidence from the scoreboard and by step 6 own entry note; the evidence is unit 246 ceiling distribution of a median 31 hard-decision errors with a median 6 inside the 91 most reliable positions, admitting at most 19.6 per cent at order 3. Second, that step 6 entry is satisfied because it reads step 1 complete and step 1 is done at four of four with a separate session STATE_AFTER of done. Third, that combining is soft rather than coherent, because Ft8Waterfall carries quantised magnitudes and no phase and a second path down to the samples is not justified by any measurement this phase holds; if soft combining does not reach, the unit reports the distribution rather than reaching for coherent combining. Fourth, that the combined vector is re-normalised through the port own Ft8SoftSymbols.Normalise before submission, because that file records belief propagation as not scale-free with a hard clamp in fast_tanh, so a summed vector on a larger scale is a different experiment rather than a better one.
LICENCE: PHASE_PLAN.md step 6 and its five must-pass exits - a transmission repeated in a later slot at the same frequency identified and its soft values combined before decoding; a message decoded that no single slot could decode alone at a stated SNR below the single-slot crossing; zero wrong decodes with every combined decode passing the same CRC-14; the gain measured on the ladder with its trial count; and every combined decode verified against the ladder own ground truth. Step 6 entry note that it is not gated on steps 2 and 4 and is the step to try when they stall. The plan section that the steps are a hypothesis and not a contract, and its leave to reorder steps 2, 3, 4 and 5 on measured evidence with the record as the constraint. Step 2 second must-pass exit, which keeps that step open at partial while the number is moving. The phase ruling that improvements live in Ft8Sharp.Deep and nothing changes a line of the port. The three things the arbiter may not reason past, of which the third is what Hamlet asserts to Tim and is where the submission budget comes from.
ACCOMPLISHED: Hamlet will hear a call it already missed once, by adding what it heard the first time to what it heard the second, and reading the sum - or it will have measured, on its own instrument and against the message it knows it transmitted, exactly how far two hearings fall short and why. Nothing in WSJT-X combines repeats, so this is the first of this phase steps that aims past the best decoder there is rather than at it, and it closes entirely on synthesized signals without a radio, without WSJT-X and without Tim. Every combined codeword passes the port own parity and CRC-14 gates under a stated and bounded number of submissions, so the one thing this step could do wrong - putting a message in front of Tim that nobody sent - has an arithmetic in the report rather than a hope.
ADVANCES: step 6, and the criteria it moves are the identification and soft combination of a repeat before decoding, a message decoded that no single slot could decode alone at a stated SNR below the -19.81 dB single-slot crossing, the gain measured on the 306-trial ladder with its trial count, and every combined decode verified against the ladder own ground truth. Zero wrong decodes rides on every one of them. It also carries a blocker off step 2 without opening it: step 2 stalls because the closest candidate carries more hard-decision errors than the code can recover from, and a combined ratio vector is the only thing this phase has that reduces that count rather than searching harder around it.
END-ARBITER-DECISION
```
