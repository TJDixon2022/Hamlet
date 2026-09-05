# Work instruction 253 - strong signals are subtracted and the slot is read again

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

*The four checks were verified against the tree at authoring: `SHACK_FACTS.md` and
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` both present, `CoreHMI.sln`
and `MURC.sln` both absent, and the only solution file at the root is
`Hamlet.sln`.*

---

## THE TWO RULES THAT KILLED THREE SESSIONS ON 2026-09-05

**Tim's rulings, now HM-DEC-155, recorded in `docs/gate-set.md`. Not this unit's
to weigh.**

**1. A unit runs no test suite.** Tim runs them, once, at the end of the phase.
**A unit may run only the unit test it constructs in that work instruction**,
filtered by exact name, in the foreground, with a stated timeout. Not the project
it sits in. Not the channel. **An unfiltered `dotnet test` on any project is
forbidden.**

**2. Never background a command and poll for it.** `RUN_LEDGER.md` records the
three kills at `01:32→02:48`, `12:02→12:35` and `13:09→13:47`, every one sitting in

```
until grep -q "exited with code" .../tasks/xxx.output; do sleep 15; done
```

with a `900000` ms timeout. **The watchdog fires after twelve minutes with no
status write.** The suite was incidental; the poll was fatal.

**This unit is a measurement, so it constructs several tests and may run them -
every one of them a test THIS instruction constructs, and never two at once.**
That reading of HM-DEC-155 was made by the arbiter for unit 252, carried out
eight times that night without incident, and is carried forward unchanged: *the
unit test it constructs* is read in the plural where the instruction constructs
more than one, and the binding words are the other four - **by exact name, alone,
foregrounded, with a stated timeout.**

- **One test per invocation, filtered by its exact full method name.** Never a
  class filter, never a `~` that catches a sibling.
- **A status line written immediately before you start it and immediately after
  it returns.**
- **A stated timeout on every run.** 480 s is tonight's ceiling, chosen so no
  single foreground call can approach twelve minutes.
- **If a measurement will not fit one foregrounded call under that ceiling, split
  it into more test methods.** Task 4 is already split that way. **Do not solve it
  by backgrounding.**

`dotnet build` is allowed, foregrounded, with a stated timeout.

**Watched-failing-first holds** (`docs/gate-set.md` rule 6) for the assertion in
task 3. That task says what the red must say.

---

## THE TOOL RULE

**This session's shell may refuse calls.** Unit 251 had six, unit 252 had seven,
every one worked around. **The file-editing tools have been unaffected
throughout.**

- **A refused shell call is a signal to reach for the other tool, not to stop.**
- **Record every refusal verbatim.**
- **Nothing in this unit halts the loop.**

Measured while this instruction was authored, and it cost two calls: **a `grep`
whose pattern contains `\|` is refused as *this Bash command contains multiple
operations*** - the permission check reads the pipe inside the quotes. Spell an
alternation as two greps rather than arguing with it.

**`dotnet` and `git` were refused in no spelling on units 251 or 252** - `dotnet
build` ran six times and `dotnet test` eight times on 252 alone - so if `dotnet`
is refused tonight, say which spelling. `G4` in `docs/breakage-record.md` is a
whole night lost to writing off a working toolchain on one refused probe.

If `tools\arbiter\outcome-append.bat` is refused, units 251 and 252 both took the
same route and it worked: append with the file-editing tools in the format the
entries use, update the header's `STEP:` line in the same edit, and say so in the
entry. If `tools/arbiter/validate-output.bat` is refused, unit 252's working
spelling was `"tools\arbiter\validate-output.bat" output.md`, which returned exit
0; unit 251's fallback was `dotnet build tools/arbiter/validate-output.proj
-p:Report=output.md`.

---

## Why this unit exists

**The count today.** Step 0 done, one unit spent. Step 1 done, three units spent.
Step 2 done, two units spent. Step 3 done, two units spent. **Steps 4, 5 and 6 not
started, zero units spent between them.** The operator has seen two changes this
phase - Hamlet decodes through `Ft8Sharp.Deep`, and the `snr` column carries a
number good to 0.26 dB - and the decoder has been measured to the end of what
ordered statistics buys it: 33 of 306 at -21 dB against the port's 13, zero wrong.

**Every one of those figures was taken on a slot containing exactly one station.**
`Ft8LadderHarness.Run` places one transmission and adds noise, and so sensitivity
is the only thing this project has ever measured. On 14.074 the thing that costs
the operator a message is more often occupancy than noise: Tim's 21:58 capture
returned **80 candidates and 7 distinct messages from one slot**, and
`PHASE_PLAN.md` step 4 says in terms what that costs - *a station at -5 dB sitting
on one at -18 hides it completely on the first pass.* **No amount of ordered
statistics recovers a signal whose bins are full of somebody else.**

**Most of what a subtraction needs already exists, built for other reasons.**

- `Ft8DeepMessageSymbols.TryEncode` recovers the 79 channel symbols behind a
  decoded message, guarded by a round trip through the message layer - unit 251
  measured **0 refusals in 510** and every recovered sequence byte-for-byte the
  transmitted one.
- `Ft8DeepSignalToNoise.Estimate` refines a message's place before measuring at
  it, by a coordinate search in time and frequency, and reports how far it moved.
  Unit 251 measured that **not** refining reads 3.50 dB out, so the refinement is
  already known to find the signal rather than the analysis cell.
- `Ft8Waveform.Synthesize` in the port renders 79 symbols to samples, GFSK pulse
  and all, from the published description the `NOTICE` cites.

**What is missing is the amplitude, the phase, the subtraction and the second
pass.** That is this unit.

**And the honest framing, because this project has a precedent for it.**
`tests/Ft8Sharp.Tests/Dsp/Ft8SecondPassMeasurementTests.cs` opens with *it is
measured first because the honest outcome might be zero, and a zero here removes a
hypothesis permanently and is worth as much as a fix.* **Task 2 of this unit is
that measurement**: how much a loud neighbour actually costs a quiet station, and
what the ceiling on recovery is, taken **before the subtractor exists**. If the
ceiling is near zero the step closes on that figure and it is a result.

```
PHASE GOAL:   Everything this project has built reaches the operator's screen,
              and the decoder is taken as far as it will go.
UNIT GOAL:    A decoded message re-synthesised at its measured place, fitted for
              amplitude and phase, subtracted, and the residual decoded again -
              with what that buys and what it costs measured on a two-signal
              ladder where one station masks another.
ADVANCES:     step 4 - its first, second and fourth exit criteria: each decoded
              message re-synthesised at its measured frequency, time and
              amplitude and subtracted with the residual decoded again; the
              ladder showing more decodes from the same audio than a single
              pass, at a stated SNR with its trial count; and the pass count and
              stopping rule stated with what each pass buys. Its third (zero
              wrong decodes introduced by any pass) and fifth (time inside
              budget) are asserted on every row rather than reported at the end.
```

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim below and **report
mismatches in section 3. Do not repair the instruction.**

Unit 251 found four mismatches, unit 252 found one material one it had inherited
from a measurement document. **Assume the same rate tonight.**

Measured at authoring, 2026-09-05, `HEAD 3bd4c51`:

- Root version `1.12.54` (`Directory.Build.props:191`). `Ft8Sharp` `0.10.7`
  (`src/Ft8Sharp/Directory.Build.props:396`). `Ft8Sharp.Deep` `0.6.0`
  (`src/Ft8Sharp.Deep/Directory.Build.props:109`).
- **`Ft8DeepMessageSymbols.TryEncode(in Ft8DecodeResult, Span<byte>)` at
  `src/Ft8Sharp.Deep/Ft8DeepMessageSymbols.cs:60`**, allocating overload at `:107`
  returning `byte[]?`. It packs the text back to 77 bits through a fresh
  `Ft8CallsignCache`, decodes them again, compares ordinally, and **only then**
  writes `Ft8SymbolEncoder.SymbolCount` tones. **A refusal is a correct answer**
  and the caller gets nothing rather than half a frame.
- **`Ft8DeepSignalToNoise.Estimate(samples, sampleRate, baseFrequencyHz,
  startSeconds, symbols, settings, refine)` at
  `src/Ft8Sharp.Deep/Ft8DeepSignalToNoise.cs:165`**, and the baseband overload
  `Estimate(baseband, startSeconds, frequencyOffsetHz, symbols, refine)` at
  `:225`. It returns `Ft8DeepSnrEstimate(Decibels, Symbols,
  TimeAdjustmentSeconds, FrequencyAdjustmentHz)`. **It reports where the
  refinement moved to and it does NOT report an amplitude or a phase.** That gap
  is task 1's to price and task 3's to fill.
- **`Ft8DeepSlotDecoder.CandidateTimeBiasSeconds` at
  `src/Ft8Sharp.Deep/Ft8DeepSlotDecoder.cs:182`** is exactly minus one symbol
  period, measured by unit 248. **The place a decoded message reports is the
  coarse candidate**, quantised to a 0.080 s by 3.125 Hz cell, even where fine
  sync moved it - unit 251's finding, and the reason its estimator refines.
- **`Decode(ReadOnlySpan<float> samples)` at `:307`** analyses and then decodes;
  **`Decode(Ft8Waterfall)` at `:325` hands the private body an empty span.**
  **A pass over a residual is only possible where the samples are**, which is
  unit 249's finding about fine sync arriving again in a different place. What a
  waterfall-only call does when asked to subtract is task 1's to state.
- `Ft8SlotResult(CandidateCount, ParitySatisfiedCount, ChecksumPassedCount,
  BecameTextCount, DuplicateCount, Messages)` and `Ft8SlotMessage(Candidate,
  Result)` with `Text`, `FrequencyHz(geometry)` and `TimeSeconds(geometry)`, in
  `src/Ft8Sharp/Dsp/Ft8SlotDecoder.cs` around `:230-260`. **The duplicate
  suppression is per `Decode` call** - the `seen` list is local to it - so
  **nothing in the tree stops a second pass returning a message the first pass
  already returned.** That is task 1's duplicate rule to write down.
- **`Ft8Waveform.Synthesize(symbols, sampleRate = 12000, baseFrequency = 1000f)`
  at `src/Ft8Sharp/Encode/Ft8Waveform.cs:142`**: 79 × `SamplesPerSymbol` floats,
  no padding, GFSK pulse spanning three symbol periods, phase accumulated across
  every boundary, a raised-cosine ramp on the first and last eighth of a symbol.
  `SynthesizeSlot` at `:220`, `SlotSampleCount` at `:132`. **There is no amplitude
  parameter and no phase parameter**: it renders at unit amplitude with the
  carrier starting at zero phase. **This file is the port and does not move.**
- `tests/Ft8Sharp.Tests/Dsp/SearchFixture.cs`: `Place(slot, sampleRate, entry,
  baseFrequencyHz, offsetSamples)` at `:55` **sums** into the slot and has **no
  amplitude parameter**; `OneSignal` at `:84`; `ManySignals` at `:110`;
  `AddNoise` at `:152`, which draws, measures and reports the noise power it
  actually delivered; `TransmissionPower` at `:174`.
- `tests/Ft8Sharp.Tests/Dsp/Ft8LadderHarness.cs`: **`Decoder(string Name,
  Func<float[], Ft8SlotResult> Decode)` at `:74` - a column is a delegate over
  samples, so a multi-pass column needs no change to anything.** `Run` at `:244`
  places **one** signal a trial through `SearchFixture.OneSignal`. `RunRepeats` at
  `:425` is **unit 247's second entry point, added rather than changing `Run`**,
  and the comment above it at about `:311-318` says why in terms: *a change to it
  would invalidate all of them.* **That is the precedent a masked ladder
  follows.** `DefaultSeed = 221001` at `:61`, `DefaultFrequencyHz = 1000.0` at
  `:64`, `DefaultOffsetSamples` = three symbol periods at `:69`.
- `Ft8LadderHarness.Result` at `:91` carries `Decoded`, `Missed`, `Wrong`,
  `WrongReturns`, `Elapsed`, and `Interval => Ft8Step6Ladder.Wilson(...)`.
  **It records totals and not per-trial outcomes.** That is `HM-OPEN-078`, and
  see the ruling below - tonight it is in the way of an exit criterion.
- `Ft8Step6Ladder.Wilson(successes, trials)` at
  `tests/Ft8Sharp.Tests/Dsp/Ft8Step6Ladder.cs:255`; `Population()` at `:160` is
  the 51-message scoreable population every 306-trial figure in this phase was
  taken over.
- **`tests/Ft8Sharp.Deep.Tests/Ft8DeepSlotDecoderTests.cs:189`,
  `TheSiblingHoldsExactlyTheseTypesAndTheListIsAssertedWhole`, asserts an
  exhaustive list of 18 types.** `Ft8DeepSignalToNoise`, `Ft8DeepSnrEstimate` and
  `Ft8DeepMessageSymbols` are **not** in that list and were added by unit 251, so
  **this test is red in the tree before you touch anything.** The file's own
  remarks rule that *the list is rewritten by the unit that changed the assembly*.
  **Task 3 adds types and therefore rewrites it; it cannot make its own additions
  green without carrying unit 251's three, so it carries them and names them as
  unit 251's in the report.** Confirm the count and the three names before you
  write the list.
- `tests/Ft8Sharp.Deep.Tests/Ft8DeepBoundaryTests.cs` asserts the sibling
  references the port, the port does not reference the sibling, and **no Hamlet
  assembly arrives in either**. That is what already guards a waveform built
  inside `Ft8Sharp.Deep` from having any route to an audio device.
- **`tests/Ft8Sharp.Tests/Dsp/Ft8SecondPassMeasurementTests.cs` is NOT this.** Its
  *second pass* is a message-layer re-offer of payloads refused for an unresolved
  callsign hash; it touches no DSP. **Do not extend it, do not rename around it,
  and do not let its name confuse a reader of your report.** Its opening remark -
  measure first, because the honest outcome might be zero - is the discipline
  task 2 borrows and the only thing taken from it.
- `src/Hamlet.RadioEngine/Audio/Ft8Reception.cs:460` builds
  `new Ft8DeepSlotDecoder(osd: Ft8DeepOsdSettings.Default, fineSync:
  Ft8DeepFineSyncSettings.Default)`, and `Ft8DecoderIdentity` a few lines below
  carries the decoder's name and **two** stage flags. **Not yours tonight** - see
  the ruling on shipping.
- The record this unit's `before` must be comparable with: at -21 dB, 306 trials,
  fine sync off - port 13 of 306, Deep OSD off 13 of 306, Deep order 2 full basis
  33 of 306 (7.8-14.8), zero wrong on all of them; worst observed slot 330.4 ms in
  the shipping configuration against a 15,000 ms budget, a 45× margin.
- `docs/gate-set.md` runs to **entry 10**; `docs/breakage-record.md` to **B15**
  and **G7**; `OPEN_ISSUES.md` holds `HM-OPEN-078` (the paired statistic) and
  `HM-OPEN-077` (the `CLAUDE.md` §1 ruling-table gap).
- Six of the loop's own files are modified and uncommitted at the root -
  `OUTPUT.md`, `PHASE_OUTCOME.md`, `PHASE_PLAN.md`, `PHASE_STATUS.md`,
  `RUN_LEDGER.md`, `WORK_INSTRUCTIONS.md` - together with everything under
  `.run-unit/`. **They are the harness's. Do not commit them as part of a task
  commit and do not tidy them.**

**Failures you should expect, so you do not chase them:** the whole-type-list
tripwire above, red before you start and red again the moment you add a type;
`CwAdjudicationTests.ASpeedChangeInRealisticAudio`; the 51 CW cases in
`docs/unit239-failing-set.txt`;
`Scan.ScannerEndToEndTests.ADwellReachesTheDecoderAndTheVerdictCarriesItsConfidence`.

---

## Rulings in force

Transcribed from `PHASE_PLAN.md` and `docs/gate-set.md`. **Not to be re-argued.**

**A unit runs no test suite; only the tests it constructs in this instruction,
each filtered by exact name, alone, foregrounded, with a stated timeout. Never
background and poll.** Tim, 2026-09-05, HM-DEC-155.

**No test is added, to the tree or to the gate set, without naming the breakage it
would have caught.**

**`Ft8Sharp` is a faithful MIT port and nothing in this phase changes a line of
it.** Its value is that it cannot drift. **Calling `Ft8Waveform.Synthesize` is not
changing the port, and adding a test under `tests/Ft8Sharp.Tests/` is not changing
it either** - that project already references the sibling.

**`Ft8Sharp.Deep` is GPL-3.0.** Settled 2026-09-04. All algorithm work goes here.

**Hamlet decodes through Deep, not the port**, and **both of the port's parity and
CRC-14 gates stay in the path**. Nothing in `Ft8Sharp.Deep` decides that a message
is real, and **that holds for a message recovered from a residual exactly as it
holds for one recovered on the first pass.**

**A wrong decode is counted separately from a missed one, everywhere.** Every
column measured in this project reads zero wrong and no unit may be the one that
stops checking. Step 4 names its own hazard: **zero wrong decodes introduced by
any pass. Subtraction leaves residue, and residue that decodes is this step's
specific hazard.**

**Targets are waypoints, not gates. A step closes on the figure it reached.**

**The licensing boundary.** No route to an algorithm goes through WSJT-X's source
or `ft4_ft8_public/`. Published description only - **Franke, Somerville and
Taylor, *The FT4 and FT8 Communication Protocols*, QEX, July/August 2020**, which
is the paper the port's own `NOTICE` cites for the waveform, and Fossorier and Lin
1995 for ordered statistics - **cited at the point of use.** A least-squares fit of
a known waveform to a buffer is textbook and needs no source but arithmetic; write
the arithmetic down and cite the waveform.

**Transmit.** `CLAUDE.md` §0.2. Nothing keys the radio.

**No unit assumes WSJT-X on this machine**, and `decode_ft8.exe` is never
substituted for it.

**A licence, naming or scope question is already ruled and is not to be raised.**

### Three readings the arbiter has made, so this unit does not stop to ask

**1. A reference waveform built to be subtracted is not a transmit path.** §0.2 and
the plan's first unreasonable-past rule govern *keying a transmitter*. A waveform
synthesised into a `float[]`, subtracted from a copy of the received slot and
dropped is a decode-path internal; `Ft8DeepBoundaryTests` already asserts that no
Hamlet assembly - and so no audio device - is reachable from `Ft8Sharp.Deep`.
**Assert it rather than assume it** (task 3), say so in the type's own remarks, and
**do not raise it as a question.**

**2. Subtraction ships OFF by default this unit, and `Ft8Reception.cs` is not
touched.** Turning it on changes what `Ft8DecoderIdentity` must record - step 0's
must-pass is that a capture says *which decoder read the slot and which stages were
on* - and that is a surface change across the census, the telemetry line and the
sidecar which step 4 does not ask for. **The unit measures, states what turning it
on would buy and cost, and lists the surfaces that must change first.** That
decision belongs to the closing measurement with the figures in front of it. **Not
re-argued, and not worked around by a "small" wiring change at the end of a long
night.**

**3. `HM-OPEN-078` is in scope, and this is the one item from the last report that
is chased rather than logged.** Unit 252 could not say whether 41 of 306 beat 33 of
306 because `Ft8LadderHarness.Run` is a **paired** design and
`Ft8Step6Ladder.Wilson` is an **independent-sample** interval. **Step 4's second
exit is *the ladder shows more decodes from the same audio than a single pass*** -
the same paired claim, on the same instrument, and it will fail to be sayable for
the same reason. So this unit records the per-trial outcomes it already computes
and reports **the discordant counts** between columns. **`Run` does not change, and
`Result.AsRow`'s existing columns do not move**; the discordance prints on its own
line. This is not the last report setting the subject - it is the smallest thing
that makes tonight's own criterion assertable.

---

## Status cadence

After each task, before starting the next, update `PROJECT_STATUS.md` per
`CLAUDE.md` - `STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, and `NOTE`
saying what is moving inside the task. The same every ten minutes while a task is
running. **Use the file-editing tools if the shell refuses.**

**Tonight the cadence is load-bearing and not paperwork.** Tasks 2 and 4 run
several multi-minute foreground calls. **Write a status line immediately before
each `dotnet test` and immediately after it returns**, naming which test and which
cell. The watchdog fires at twelve minutes of silence and every kill this project
has taken came from a session that went quiet while something ran.

---

## Tasks

### Task 1 - the trace, and it decides how the night is spent

**Reading and at most one `dotnet build`. No test is run in this task.**

Write **`docs/unit253-subtraction.md`**. Answer these with file and line
references, because each one changes what tasks 2 to 5 can be:

1. **Where a second pass can happen at all, and what it costs.** Which entry point
   has the samples; **what `Decode(Ft8Waterfall)` must do when a caller asks for
   subtraction and there are no samples** - unit 249 found fine sync silently
   refusing 42 of 42 candidates for exactly this reason and it must not happen a
   second time, so say whether it refuses loudly, skips and counts, or is
   unreachable by construction. Then price one extra pass from the record: a
   re-analysis is `Ft8Monitor.Analyse` plus `Ft8SyncSearch.Find` plus the decode
   loop, and the measured slot costs in the tree are the port at about 64 ms,
   OSD on at about 72 ms in isolation and 330.4 ms in the shipping configuration,
   against 15,000 ms.
2. **The place, the amplitude and the phase.** What is known about a decoded
   message's position: the coarse candidate quantised to 0.080 s by 3.125 Hz, plus
   `CandidateTimeBiasSeconds`, plus what `Ft8DeepSignalToNoise.Estimate`'s
   coordinate search returns. **Then write out, in one paragraph of arithmetic, how
   the amplitude and the carrier phase are obtained**, since nothing in the tree
   returns either. State plainly whether the reference is built at the full sample
   rate or in the complex baseband `Ft8DeepBaseband` already builds, **what the fit
   minimises**, and why a real scale factor alone is not enough. Cite the QEX paper
   for the waveform at the point of use.
3. **THE SAFETY QUESTION, ANSWERED HERE OR THE UNIT DOES NOT PROCEED. What can a
   second pass return that a first pass could not?** Every pass puts codewords to
   the port's parity and CRC-14, and unit 246 §5 item 2 records each submission as
   an independent false accept at about one in 16,384. **Write the arithmetic:**
   submissions a slot on one pass, on two, on three, and the expected wrong
   messages a slot for each. Then name the hazard that is this step's own - **a
   subtraction that leaves a shaped remnant where a transmission was is the one
   thing this stage can invent that no earlier stage could.** State, before either
   is written, **the stopping rule and the duplicate rule**: what makes a pass the
   last one, and what happens when a later pass returns a message an earlier pass
   already returned. **If any change you are contemplating would submit more than
   one codeword per candidate per pass, do not make it.**
4. **The masking prediction, tabulated before anything is measured.** A
   transmission spans eight tones at 6.25 Hz, so 50 Hz wide, against a 3.125 Hz
   analysis bin. Predict which frequency separations can overlap at all, and at
   what level difference a loud neighbour should start costing a quiet station its
   decode. **Say which (separation, level difference) cells task 2 walks and why
   those.** Task 2 measures whether the prediction held.
5. **What this unit's `before` and its ceiling are.** Name the exact call the new
   harness entry point will make - population, blocks, seeds, frequencies,
   offsets - and confirm that the single-signal control column will be
   **bit-identical audio** to what `Run` draws, which is what makes it comparable
   with 33 of 306. Then define the **ceiling column**: the same audio with the loud
   transmission absent and the noise draw unchanged. **The gap between the single
   pass and the ceiling is the whole of what subtraction could ever recover**, and
   a report that quotes a gain without it is quoting a number with no scale.
6. **What would have to change for subtraction to ship**, listed and not done:
   `Ft8Reception.cs:460`, `Ft8DecoderIdentity`'s stage flags, the five-count
   census, the telemetry line, the capture sidecar. **This list is what step 6
   reads.**

**Commit this before you write a line of task 2.** Unit 249 found the whole of
step 0 in a task like this one and unit 251 found a 3.4 dB placement error in one.

### Task 2 - what a loud neighbour actually costs, measured before anything is built

**One test, `tests/Ft8Sharp.Tests/Dsp/`, run alone by exact full method name,
foregrounded, 480 s timeout, status line either side.**

**This is the task that can end the step honestly and cheaply.** It builds no
subtractor. It asks one question: over a whole block of the 51-message population,
how many quiet messages does a single pass lose to a loud neighbour, and how many
of those come back when the neighbour is simply not there?

- **Two transmissions summed into one slot**, the quiet one at the frequency and
  offset `Run` uses and the loud one at a separation and a level difference from
  the grid task 1 chose; noise added to a commanded ratio for the **quiet** signal,
  which is the one scored. Both messages drawn deterministically from
  `Ft8Step6Ladder.Population()` so a fresh process draws the same slot.
- **`SearchFixture.Place` has no amplitude.** Add one as an **optional parameter
  defaulting to 1.0** so every existing call site compiles unchanged, and **assert
  in this test that unit amplitude places bit-identical samples to today** - the
  passband and slot-decoder tests that call it are how several recorded figures
  were taken.
- **Per cell report:** quiet messages returned by the single pass; **the ceiling** -
  the same trial with the loud transmission absent and the identical noise draw;
  the difference; and **wrong**, which is anything returned that neither station
  sent. **Assert zero wrong on every row.**
- **The output is a decision, and state it as one: which cell the ladder in task 4
  walks**, chosen because the single pass loses the quiet message there **and** the
  ceiling says it was recoverable. A cell where the ceiling is also zero is a cell
  where the signal is not there to be found, and it is not the cell.

**If no cell shows a gap - if the single pass gets essentially everything the
ceiling gets - stop building and say so.** Write the table, state that subtraction
has nothing to recover on this instrument at these separations, close the step on
that figure with what was tried, and spend the rest of the night on tasks 5 and 6.
**That is `PHASE_PLAN.md`'s own named alternative to stopping and it is a closed
step, not a failure.**

### Task 3 - the subtractor, and the two things it must not do

**`src/Ft8Sharp.Deep/`. Not `src/Ft8Sharp/` - the port does not move.**

Build the smallest thing that takes a slot, a message's symbols and its place, and
returns the residual together with what it fitted - the gain, the phase, the place
it settled on, the energy it removed, and how many symbols of the frame lay inside
the slot. Settings for the pass count and the stopping rule live beside it.

- **Refuse loudly rather than clamp**, in the voice `Ft8DeepOsdSettings` already
  uses. **A message whose symbols `Ft8DeepMessageSymbols.TryEncode` will not give
  up is not subtracted, and that is counted rather than hidden** - a silent skip is
  how a stage comes to report a pass it did not make.
- **Say in the file what this is and what it is not.** It is the multi-pass
  subtraction the QEX paper describes, cited at the point of use. It is **not** a
  gate, a threshold or an acceptance rule, and **it does not change how many
  codewords are put to the port's parity and CRC-14 per candidate per pass.**
- **The whole-type-list tripwire will go red.** Rewrite
  `TheSiblingHoldsExactlyTheseTypesAndTheListIsAssertedWhole` as the file's own
  remarks say the unit that changes the assembly must. **You will find three types
  unit 251 added and did not list - name them as unit 251's in the list's remarks
  and in your report**, so the record shows who added what.

**THE WATCHED FAILURE, AND IT IS THIS ONE.** On a synthesised slot carrying one
known transmission and **no noise**: after subtracting that message at its measured
place, **the slot no longer decodes it**, and the energy remaining in its band is
reported as a measurement in decibels beside the assertion. **Run it once and watch
it fail before the fit exists** - with unit gain and zero phase the message will
still decode out of the "residual", which is exactly the shape of the bug this
whole task is about - **and quote the failure verbatim in the report.** Then make
it pass. **Report the decibels removed; do not turn them into a gate**, because a
threshold picked tonight would be a target written before the work started.

**AND THE IDENTITY THAT PROTECTS EVERY RECORDED FIGURE: with subtraction off,
`Ft8DeepSlotDecoder` returns bit-for-bit what it returns today.** Whole-result
identity on a slot, asserted in the same test file. Every row of units 246, 248,
251 and 252 is invalidated if the default path moved, and **off must be the
default** for the same reason ordered statistics is off unless it is asked for.

`Ft8Sharp.Deep` `0.6.0` → `0.7.0`.

### Task 4 - the masked ladder, and what each pass buys

**A new entry point on `Ft8LadderHarness`, added beside `Run` and `RunRepeats` and
never changing `Run`** - unit 247's comment above `RunRepeats` is the precedent and
says why. A column is a `Decoder(name, Func<float[], Ft8SlotResult>)`, so a
multi-pass column is a delegate and needs nothing else.

**Record the per-trial outcome per column on `Result`** so discordant counts can be
taken between any two columns. **`AsRow`'s existing columns do not move**; the
discordance prints on its own line. See ruling 3.

**Three test methods, each runnable alone by its exact full method name**, split
because one method carrying all of this would approach the watchdog and **splitting
is the licensed answer; backgrounding is not.**

**4a - the pass-count sweep, one block of 51 trials, on task 2's chosen cell.**
Columns: one pass, two, three, four. **Per row:** quiet messages decoded, **wrong**,
ms a trial, worst observed slot ms, messages subtracted, messages refused for want
of symbols, and passes actually run. **This is where the stopping rule is read off**,
and it is cheap enough that the answer exists even if the night ends early.

**4b - the scoreboard, 306 trials.** Six whole blocks of the population, the count
every recorded figure in this phase was taken at. Columns, in this order: **single
pass - the before**; **two passes with subtraction - the after**; **the ceiling -
the same audio with the loud station absent**. **Fine sync off and ordered
statistics off on every column**, so the difference is subtraction's alone and
nothing else. **Per row:** requested and delivered decibels, trials, decoded,
missed, **wrong**, rate, the 95 per cent Wilson interval, wall seconds, ms a trial,
worst observed slot ms. **Then the discordant counts** between the single pass and
each other column: trials only the one decoded, trials only the other decoded.
- **Assert zero wrong on every row**, with every wrong return printed sent beside
  returned. **A message returned that neither station sent is this step's specific
  hazard and it is reported, never averaged over.**
- **Assert nothing about the rate.** No bound, no target. A column that returns
  nothing is a measurement.

**4c - the control, on the single-signal ladder, and it is not droppable.** At
-20 dB through `Run` itself - the same audio every recorded row was taken on -
subtraction on against subtraction off. **It must take nothing away and add no
wrong decode.** Name the figure. This is the row that says the stage is safe to
have in the path at all, and it is the strong candidate for the gate set.

**Nice-to-pass, and only if it fits: one shipping-configuration column** - fine
sync on and ordered statistics on, with two passes - so the report can say what the
operator would actually get rather than what the isolation says. **Label it as not
part of the isolation.** If it does not fit a 480 s call, drop it and say so.

**DROP CANDIDATE - and it is this one, named in advance: the three-pass column at
306 trials in 4b.** It is the most expensive optional thing in the night and the
step's criteria are met without it: 4a already prices what a third pass buys over a
second at 51 trials, and 4b's before-and-after is the pair the exit criterion asks
for. **If the night runs long, drop it, quote 4a's figure instead, and say in the
report that a third pass at 306 trials remains unmeasured and why.** That is a
result.

**Not droppable, in order of what to protect:** 4a's sweep; 4b's single-pass,
two-pass and ceiling columns at 306 trials; the zero-wrong assertion on every row;
4c's control; and task 1's trace. **Do not silently shrink the trial count to save
time.** Cutting a column is licensed; cutting the measurement is not.

### Task 5 - the verdict

**The figures, and then decisions taken rather than left open.**

- **What subtraction bought**, as *n* of 306 before and after, with both Wilson
  intervals, **the discordant counts**, and **the ceiling beside them** - so the
  report says what fraction of what was there to recover was recovered, not merely
  that a number went up.
- **The pass count and the stopping rule stated, with what each pass buys** -
  pass 2 over pass 1, pass 3 over pass 2, pass 4 over pass 3, in decodes and in
  milliseconds, from 4a. **Write the stopping rule down as a rule**, not as a
  setting that happened to be used.
- **Worst observed slot for the configuration you would recommend, and its margin
  against the 15,000 ms budget, as a multiple**, the way unit 252 stated 45×.
- **The control's figure**, in one sentence: what subtraction cost the single-signal
  ladder, and what it took away.
- **Then the shipping question, answered and not left open.** Subtraction stays off
  by default (ruling 2). Say **what turning it on would buy the operator, what it
  would cost him a slot, and which surfaces must change first** - the list from task
  1 §6. If the measurement showed no gain, say that in the same plain words. **The
  step closes on the figure reached either way.**

### Task 6 - bookkeeping

**File edits, and the tools.**

- **`docs/unit253-subtraction.md`** completed with every table from tasks 2, 4 and
  5, in the shape of `docs/unit252-osd-window.md`. **This is the document step 6
  will read**; `output.md` is overwritten every unit and cannot carry it.
- **`docs/breakage-record.md` gains `B16`** for what this unit's tests would have
  caught. **Write the breakage before the gate-set entry that cites it** - unit 251
  found the last instruction claiming a breakage was already recorded when it was
  not.
- **`docs/gate-set.md` gains entry 11**, in the format already there: full name,
  the property it guards, the breakage it would have caught, with the unit number.
  **The strong candidate is task 4c's control or task 3's subtraction-off identity**
  - whichever names a breakage a reader can point at. **An entry that cannot name a
  breakage does not go in**, and you say so instead. **At most one entry**; the
  file's own rule is *do not pad it.*
- **Add that test's name to `tools/arbiter/gate-set.bat`** if and only if it went
  into the document, so the two stay in step. **Do not run that script.**
- **`OPEN_ISSUES.md`**: log anything dropped, with the figure that was reached and
  the price of settling it, so step 6 finds it rather than infers it.
- Append this unit's entry to `PHASE_OUTCOME.md` through
  `tools\arbiter\outcome-append.bat`. **If the shell refuses, append with the
  file-editing tools in the format the existing entries use and update the header's
  `STEP: 4` line in the same edit**, which is what the script does in one call. Say
  so in the entry.

---

## Parked - do not touch, do not raise

- **Step 5, cross-slot combining.** `Ft8DeepSoftCombiner`, `Ft8DeepRepeatDecoder`,
  `Ft8DeepCombineSettings`, `Ft8DeepCombineCounts`, `Ft8DeepHearing` and
  `Ft8LadderHarness.RunRepeats` all exist and are **not yours tonight**, however
  natural combining looks once a residual is in your hand.
- **Step 6, the closing measurement.** It reads what you leave behind. Do not start
  writing it.
- **`Ft8DeepOsdSettings.Default`.** Unit 252 settled it at order 2 over the full
  basis with the table on the property's own remarks. **Do not move it, do not
  re-argue it, and do not tune the window.**
- **The SNR estimator.** Use `Ft8DeepSignalToNoise` and `Ft8DeepMessageSymbols`;
  **do not tune either.** Step 2 closed at 0.26 dB and its constants are derived,
  not fitted.
- **Fine sync and the baseband.** `Ft8DeepFineSync`, `Ft8DeepBaseband` and unit
  248's placement arithmetic are settled. Held **off** in task 4's isolation and
  **on** only in the one nice-to-pass row, and neither is tuned.
- **`Ft8Reception.cs` and everything downstream of it** - the census, the telemetry
  line, the sidecar, `Ft8DecoderIdentity`. Ruling 2. **Listed in task 1 §6, changed
  by nobody tonight.**
- **`src/Ft8Sharp/`.** Not one line. It is the instrument. `Ft8Waveform` is called,
  not edited.
- **`docs/unit246-osd.md` §5 item 4's wall-clock figure**, wrong by about
  fifteen-fold and reported by unit 252. **Logged, not repaired, not yours** - step
  6 reads that document and the correction belongs with whoever owns it.
- **Whether the capture sidecar's per-message lines carry the decoded text.** Unit
  251 raised it and it is **Tim's**.
- **`HM-OPEN-077`**, `CLAUDE.md` §1's ruling table stopping at `CPS-DEC-0152`.
  Standing gap, logged, not chased.
- **The two column definitions for one table** that unit 251 found in
  `MainWindow.axaml` and `App.axaml`. Reported, not repaired, not yours.
- **The gate set as a whole**, beyond the one entry task 6 may add. **Never run it.**
- **The panel.** No columns, no colours, no sorting.
- **The CW decoder**, the 419 dropped chunks, the 51 inherited reds,
  `ReusableWindow`, `ProcessDelayForTests`, the tap's owner, unit 237's Extensible
  conclusion, work instruction 231's four tree items,
  `validate-output.bat`'s permitted-spellings bug, the 101.33 ms pulse above 6 kHz.
- **A real-air or WSJT-X comparison.** Step 4 defers it in terms. The ladder knows
  what it transmitted and that is the whole instrument.

---

## What not to do

- **Do not run a test suite.** One test per invocation, by exact full method name,
  and only tests this instruction constructs.
- **Do not background a command and poll for it.** HM-DEC-155. If a run will not
  fit a foreground call, **split the test**; do not detach it.
- **Do not go quiet while something runs.** A status line before and after every
  `dotnet test`.
- **Do not run `tools/arbiter/gate-set.bat`.**
- **Do not change `src/Ft8Sharp/`**, and do not change `Ft8LadderHarness.Run`.
- **DO NOT SUBMIT MORE THAN ONE CODEWORD PER CANDIDATE PER PASS TO THE CRC-14.**
  The arithmetic is task 1 §3 and unit 246 §5 item 2. **A wrong decode is worse
  than a missed one**, and a second pass over a residual is the exact place this
  phase could produce one.
- **Do not let a message reach a result without both of the port's gates**, whatever
  pass recovered it.
- **Do not return the same message twice** because a later pass found it again.
  Task 1 §3 writes the rule before task 3 implements it.
- **Do not turn subtraction on by default and do not touch `Ft8Reception.cs`.**
  Ruling 2.
- **Do not report a rate without its trial count and its interval**, and do not
  report a gain without the ceiling beside it.
- **Do not turn the decibels removed by the fit into a pass/fail threshold.**
- **Do not assert a bound on the decode rate.** Targets are waypoints.
- **Do not stop checking wrong decodes**, on any row, at any pass, ever.
- **Do not extend or rename `Ft8SecondPassMeasurementTests`** - it is a different
  second pass and the collision is in the name only.
- **Do not add a test without naming the breakage it would have caught.**
- **Do not ship a placeholder token in a reported number.** `validate-output.bat`
  rule 7 catches it and unit 248 is why the rule exists.
- **Do not stop because the shell refused something.**

---

## Committing and pushing

Commit and push each task before starting the next. Root `1.12.54` → `1.12.55`.
`Ft8Sharp.Deep` `0.6.0` → `0.7.0`. **`Ft8Sharp` stays at `0.10.7` and does not
move.** If `git` is refused, say so and carry on.

---

## Reporting

`output.md` at the repository root, overwritten, **four sections** per
`CLAUDE_CODE.md` §8.

**First, above everything, the ordering block. `validate-output.bat` rule 6 refuses
a report without it, and rule 7 reads the header underneath it.** Every line of it
specific to this unit:

```
READ IN THIS ORDER

A. THE PHASE GOAL, and where every step of it stands - the goal in your own
   words, then the state of steps 0 to 6 as you found them, with steps 0, 1, 2
   and 3 closed and this unit the first spent on step 4.

B. THIS STEP AND ITS EXIT CRITERIA. Step 4, strong signals are subtracted and the
   slot is read again. Its five must-pass exits - each decoded message
   re-synthesised at its measured frequency, time and amplitude and subtracted
   with the residual decoded again; the ladder showing more decodes from the same
   audio than a single pass, at a stated SNR with its trial count; zero wrong
   decodes introduced by any pass; the pass count and stopping rule stated with
   what each pass buys; and time inside the 15-second budget with the margin
   stated. Say which were met, which were not, and which were met partially, one
   line each. The WSJT-X comparison on a real capture is deferred by the step
   itself and is not a criterion you owe.

C. THIS REPORT, weighed against A and B. Lead with the quiet-message decode count
   single pass against two passes, both with their trial count and Wilson
   interval, THE CEILING BESIDE THEM, and the discordant counts. Say what
   subtraction bought, what it cost a slot, and whether any pass returned a
   message neither station sent. Then name HOW MANY ITEMS SECTION 4 RAISES and
   whether any of them stands in the way of a criterion in B.
```

Then the header block, in this shape:

```
UNIT:       253 - <state> at task n of 6 - <clock>
PHASE GOAL: <the phase goal>
UNIT GOAL:  <this unit's goal>
ADVANCED:   <yes or no, and which exit criteria of step 4>
NUMBER:     <quiet messages decoded of the trial count, single pass and two
            passes, with both 95 per cent Wilson intervals, the ceiling, and the
            discordant counts - the numbers this unit exists to produce>
TESTS:      <every test you ran, by exact name, each with its wall clock and its
            timeout, which was watched failing first and what its red said, and
            that no suite was run and nothing was backgrounded>
VERSION:    <root, Ft8Sharp.Deep, and Ft8Sharp untouched>
DENIALS:    <count, and whether each was worked around>
DRIFT:      <consecutive units without advance>
```

**Section 3 leads with these five, in this order:**

1. **The masking survey from task 2, whole** - every cell, with the single pass,
   the ceiling and the gap between them - and the sentence saying **which cell the
   ladder walked and why that one.**
2. **The masked ladder, whole**, in the harness's own columns: trials, decoded,
   missed, **wrong**, rate, Wilson interval, ms a trial and worst slot ms for the
   single pass, two passes, the ceiling, and the third pass if it was not dropped.
   **Then the discordant counts** between the single pass and each other column.
3. **The pass-count sweep and the stopping rule** - what each pass bought and what
   each cost - stated as a rule and not as a setting.
4. **The time budget**: worst observed slot for the configuration you recommend and
   its margin against 15,000 ms, as a multiple; and the control's figure on the
   single-signal ladder.
5. **The shipping verdict** - subtraction stays off, what turning it on would buy
   and cost, and the surfaces that must change first. **If the drop candidate was
   taken, say so here rather than in section 4.**

Then **the watched failure, verbatim**, then the refused shell calls verbatim, then
the mismatches you found between this instruction and the tree, in a table,
**reported and not repaired**.

Write `output.md`, then stop. Do not start the next unit.

---

```
ARBITER-DECISION
STEP: 4
APPROACH: subtract each decoded message by re-synthesising its symbols at the measured place and decode the residual, measured on a masked two-signal ladder
MOVE: continue
WHY: Step 4 is not started with zero units spent, its entry criterion - step 0 complete - is met, and the loop test finds no approach resembling this one in any entry. Most of what a subtraction needs is already in the tree and was built for other reasons - Ft8DeepMessageSymbols recovers the symbols behind a decoded message with a round-trip guard, Ft8DeepSignalToNoise already refines a message's place, and the port's Ft8Waveform renders symbols to samples - so what this unit builds is the amplitude and phase fit, the subtraction and the second pass, and what it spends the night on is the measurement.
STATE: not started
DECIDED: four. First, that a reference waveform built in memory to be subtracted from a buffer is a decode-path internal and not a transmit path - CLAUDE.md 0.2 governs keying a transmitter, Ft8DeepBoundaryTests already asserts no Hamlet assembly and so no audio device is reachable from Ft8Sharp.Deep, and the unit asserts that rather than raising it, because a unit that stopped here would lose the night to a question the plan has already answered. Second, that subtraction ships OFF by default and Ft8Reception.cs is not touched, because turning it on changes what Ft8DecoderIdentity must record under step 0's must-pass that a capture says which stages were on, and that surface change is not what step 4 asks for - the unit measures, states what shipping would buy and cost, and lists the surfaces that must change first. Third, that HM-OPEN-078 - the paired statistic unit 252 raised - is chased rather than logged, uniquely among that report's items, because step 4's second exit is a paired claim on the same instrument that stopped unit 252 moving a default, and the smallest fix is to record the per-trial outcomes the harness already computes and report the discordant counts, without changing Run or the printed columns. Fourth, that the named drop candidate is the three-pass column at 306 trials, because task 4a already prices what a third pass buys at 51 trials and the exit criterion is met by the before-and-after pair, so a long night sheds the most expensive optional comparison and never the measurement.
LICENCE: PHASE_PLAN.md step 4 and its five must-pass exits, its deferral of the real-capture comparison, and its rulings that targets are waypoints, that a step closes on the figure it reached, and that an approach returning one wrong decode is rejected and another taken. The plan's ruling that Ft8Sharp is a faithful port nothing in this phase changes and that Ft8Sharp.Deep is GPL-3.0. The licensing boundary - published description only, Franke, Somerville and Taylor, QEX July/August 2020 for the waveform and the multi-pass description, cited at the point of use, and no route through WSJT-X's source or ft4_ft8_public/. CLAUDE.md 0.2 on transmit, and 0.0 and 12.1 on what Hamlet asserts to the operator, which is what the zero-wrong assertion on every pass exists to protect. HM-DEC-155 on what a unit may run. docs/gate-set.md, which rules that the ladder is a measurement and not a test and that no test is added without naming the breakage it would have caught. Step 0's must-pass that a capture records which decoder read the slot and which stages were on, which is what keeps subtraction off by default tonight.
ACCOMPLISHED: the project can say, in decodes with trial counts and intervals and against a ceiling that says what was there to be recovered, whether pulling a loud station out of a slot and reading it again gets the operator messages he would otherwise never see - and what it costs him per slot, with zero wrong decodes on every pass. If the answer is that a loud neighbour costs him little on this instrument, that is a hypothesis removed permanently, measured before anything was built, and the step closes on it.
ADVANCES: step 4, and specifically its first, second and fourth exit criteria - each decoded message re-synthesised at its measured frequency, time and amplitude and subtracted with the residual decoded again; the ladder showing more decodes from the same audio than a single pass, at a stated SNR with its trial count; and the pass count and stopping rule stated with what each pass buys. The third criterion, zero wrong decodes introduced by any pass, and the fifth, time inside the 15-second budget with the margin stated, are asserted on every row of every measurement rather than reported at the end.
END-ARBITER-DECISION
```
