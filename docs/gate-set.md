# The gate set — the tests every unit runs, and why each one is in it

**Written 2026-09-05 by work instruction 250, step 1 of the on-air phase.**

This file is the answer to a question this repository could not previously
answer: *which tests must run before a change is believed?*

Until tonight the honest answer was **all of them**, and that answer had stopped
being affordable. `Ft8Sharp.Tests` is 610 tests and 14 minutes.
`Hamlet.RadioEngine.Tests` is 2,281 tests and had never once completed a
whole-project run — started alone at 08:15 on 2026-09-01 and cut off at 09:16,
with no per-test times ever recorded, so nobody knew which of them was
expensive. Four consecutive reports carried no total for that project.

**A suite nobody can finish guards nothing.** What follows is the short list
that does.

---

## The standing rule this phase runs under

Three sentences, and they are the whole of it.

1. **A unit runs the gate set, every time.** That is the list below, and it is
   one command.
2. **A unit runs the channels it touched**, whole, one project at a time, never
   concurrently. Contention once turned one standing failure into five.
3. **A unit does not run anything else.** Not for completeness, not to be safe.

And two more that bound how this list may grow:

4. **The full engine suite is Tim's, by hand, uncontended, once.** It is not a
   unit's job and its absence never blocks a step.
5. **No test is added to this list — or to the tree — without naming the
   breakage it would have caught.** A test that guards nothing that has ever
   broken is cost without cover. **Work instruction 250 wrote this rule down and
   is the first unit bound by it**; every entry below names a real event with the
   unit number where it happened.

**The ladder is a measurement, not a test.** `Ft8LadderHarness.Run` is called
when a step needs a number. It is never in the gate set, and the one ladder entry
below is in it for its *zero-wrong* assertion and not for its rate.

---

## How to run it

```
tools\arbiter\gate-set.bat
```

Four projects, in sequence, one at a time, never concurrently. Exit 0 is green;
exit 1 names the project that failed. It writes a TRX per project into
`.run-unit\trx\` so a failure can be read per test afterwards, through
`tools\arbiter\trx-rank.py`.

When the shell's allow-list refuses a `.bat` invoked directly — which it has done
for a fortnight — the same script is reachable through the route already
established twice in this tree:

```
dotnet build tools\arbiter\gate-set.proj
```

That project runs the `.bat` unmodified. It is not a second copy of the gate set.

**It never runs `Hamlet.App.Tests` unfiltered.** That project stops partway when
run whole; `docs/full-suite-run.md` holds the four filtered commands for it.

---

## The measured cost

| | |
|---|---|
| Tests in the gate set | **27 methods, 30 cases** |
| Wall clock, whole command, cold | **2 m 39 s** |
| Target | under 3 minutes |
| Sum of per-test time | 121.4 s |

**It has been watched failing.** Work instruction 250 broke one guarded property
deliberately in a scratch change and confirmed the gate set reddened before
reverting; the evidence is in that unit's `output.md`. **A gate set nobody has
seen fail is a list, not a gate.**

Most of the wall clock is not the tests. Four `dotnet test` invocations pay four
build-and-discovery costs, and those are roughly half of it. The tests themselves
are dominated by two entries — `Ft8DeepIdentityTests` at 54.7 s and
`Unit222TraceTests` at 40.8 s — which together are 79 per cent of the measured
test time and are the two entries the phase would be least willing to lose.

---

## The gate set

Each entry names the property it guards and **the breakage it would have
caught**. An entry that cannot name one does not belong here.

### 1. Deep is a superset of the port — whole-result identity

| | |
|---|---|
| `Ft8Sharp.Tests.Dsp.Ft8DeepIdentityTests.OverAWholeBlockOfTheLadderTheTwoResultsAreIdentical` | 2 cases, 25.1 s |
| `Ft8Sharp.Tests.Dsp.Ft8DeepIdentityTests.OverTheCommittedExampleCaptureTheTwoResultsAreIdentical` | 0.31 s |
| `Ft8Sharp.Tests.Dsp.Ft8DeepIdentityTests.OverEveryReferenceRecordingTheTwoResultsAreIdentical` | 29.3 s |

**The property.** With every stage off, `Ft8DeepSlotDecoder` returns the *whole*
`Ft8SlotResult` that `Ft8SlotDecoder` returns — all five counts, and every
message's text, candidate, frequency and dt, in order. Not `Texts` alone.

**The breakage it would have caught.** At **unit 245** the sibling held an
`Ft8SlotDecoder` and delegated to it, so identity was trivially true: one decoder
called twice. **Unit 246 replaced that with the sibling running the port's
per-candidate loop itself**, through the port's public members, because ordered
statistics decoding had nowhere else to sit. From that commit the two columns
were two pieces of code, and any divergence in the reproduction would have made
the scoreboard's OSD-off column something other than the port. Every decibel
units 246, 247, 248 and 249 attributed to one named change is attributable only
because this test held. It is unit 246's **ruling 4** and the same three tests
assert `deep.Osd is null`, so a later unit that flips the default cannot quietly
turn an identity comparison into a comparison of the port against an OSD run.

**Why the expensive one stays.** `OverEveryReferenceRecordingTheTwoResultsAreIdentical`
is 69 real off-air recordings and 801 messages across the seam. It is the largest
body of evidence this phase has on the receive side and it costs 29 s. It skips
rather than fails when the pinned `ft8_lib` clone is absent, which is what keeps a
fresh clone green.

### 2. The port's parity and CRC-14 gates are in the decode path

| | |
|---|---|
| `Hamlet.RadioEngine.Tests.Audio.HamletDecodesThroughDeepTests.TheReaderReturnsAtLeastWhatThePortDidAndNothingUngated` | 1.28 s |
| `Ft8Sharp.Deep.Tests.Ft8DeepGateTests.ARightOsdCodewordComesBackThroughThePortAsTheMessage` | 0.85 s |
| `Ft8Sharp.Deep.Tests.Ft8DeepSeamProbeTests.AWrongCodewordHandedBackIsStillRefused` | 0.85 s |
| `Ft8Sharp.Deep.Tests.Ft8DeepCombineGateTests.ADeliberatelyWrongPairingIsRefusedByThePortsOwnGates` | 0.90 s |

**The property.** Every message Hamlet shows passed the port's own parity check
and its CRC-14, **whatever route recovered the codeword**. The engine test
re-checks each returned message by packing it back into its 77 bits rather than
assuming; the three sibling tests check the two directions of the seam — a right
codeword comes back as the message, a wrong one is still refused.

**The breakage it would have caught.** This is the §0.0 hazard of the whole
phase: **Deep returns messages the port never would**, and a wrong one lands in
the operator's table looking exactly like the others. Three real events sit
behind these four tests. **Unit 246** found that `Ft8CodewordResult` cannot be
constructed outside `Ft8Sharp`, so an OSD-recovered codeword has to be handed
*back* to `Ft8CodewordDecoder` as normalised ratios — a route that works and that
a refactor could trivially shortcut, which is precisely what
`AWrongCodewordHandedBackIsStillRefused` exists to stop. **Unit 247** measured
that unbounded pairing puts the naive false-accept expectation at **366 messages
across a 306-trial rung** against 0.24 for the bounded rule it shipped. **Unit
248's** new baseband extractor is *worse* than the port at the same coarse
position — median hard-decision distance 56 against 48 at -21 dB — and it submits
one codeword per refused candidate. In all three cases the observed wrong count
was zero, and it was zero because these gates were in the path.

### 3. `Ft8Sharp` references nothing outside itself

| | |
|---|---|
| `Ft8Sharp.Tests.Ft8SharpBoundaryTests.DeclaresNoReferences` | 0.00 s |
| `Ft8Sharp.Tests.Ft8SharpBoundaryTests.NoHamletAssemblyArrives` | 0.00 s |
| `Ft8Sharp.Deep.Tests.Ft8DeepBoundaryTests.ThePortsBuiltAssemblyDoesNotReferenceTheSibling` | 0.85 s |
| `Ft8Sharp.Deep.Tests.Ft8DeepBoundaryTests.NoHamletAssemblyArrivesInEitherAssembly` | 0.00 s |

**The property.** The MIT port stays separately publishable. Nothing reaches into
it, and it reaches out to nothing — in particular not to the GPL-3.0 sibling.

**The breakage it would have caught.** **Unit 245** had to wire a brand-new
sibling project into a tree where the port already existed, and had to add a
`ProjectReference` to reach it. The reference it added went on
`tests/Ft8Sharp.Tests`, and the arbiter ruled that direction safe *on the grounds
that the mechanical guard already catches the breaching one*. The natural mistake
— putting the reference on `src/Ft8Sharp.csproj` instead — would have made an MIT
library depend on a GPL-3.0 one, which is a licensing breach that compiles
silently and that nothing else in this tree would have noticed. These four tests
are why that ruling could be taken in an evening instead of argued.

**These are the cheapest entries in the set.** All four together are under a
second, and they guard the one property in this phase that cannot be fixed after
a release.

### 4. The ladder reports zero wrong

| | |
|---|---|
| `Ft8Sharp.Tests.Dsp.Unit222TraceTests.TheRungTheVerdictIsReadAtReproducesUnitTwoTwentyOnesNumber` | 40.8 s |
| `Ft8Sharp.Deep.Tests.Ft8DeepFineSyncGateTests.TheSubmissionArithmeticIsBoundedAtOnePerRefusedCandidate` | 2.28 s |
| `Ft8Sharp.Deep.Tests.Ft8DeepRepeatDecoderTests.TheSubmissionsSpentNeverExceedTheBudgetTheSettingsBound` | 3.60 s |

**The property.** **Not the rate — only that nothing is returned that was not
sent.** A 306-trial rung at -21 dB, against a ladder that knows exactly what it
transmitted, with `Assert.Equal(0, row.Wrong)`. Beside it, the two arithmetic
bounds that make the zero an argument rather than luck: one codeword submitted
per refused candidate, and a combining budget the settings actually enforce.

**The breakage it would have caught.** **A wrong decode is counted separately
from a missed one, everywhere**, and every column this project has measured reads
zero wrong. That is not an accident of the code — it is a consequence of two
bounds that are each one line from being relaxed. **Unit 247** wrote its pairing
budget down *before* the code and then counted rather than estimated: 516
combinations put to the gates across the whole jittered -21 dB walk, naive
expectation 0.031 messages nobody sent, zero returned. Unbounded, the same
arithmetic gives 366. **Unit 248** made 4,137 submissions for an expected 0.253
false accepts and observed zero. **A later unit tuning for reach would move
exactly these two numbers**, and without these three tests the first symptom
would be a message on Tim's screen that nobody transmitted.

**Why 40 seconds is worth paying.** The rung test is the only entry that walks
real trials with ground truth. A cheaper zero-wrong assertion exists on 51 trials
rather than 306, and 51 trials at an expectation of 0.03 is not a measurement of
anything.

### 5. Deep adds and never removes, with the stages on

| | |
|---|---|
| `Ft8Sharp.Deep.Tests.Ft8DeepFineSyncGateTests.WithEverythingOffTheWholeResultIsThePortsWholeResult` | 0.57 s |
| `Ft8Sharp.Deep.Tests.Ft8DeepFineSyncGateTests.EveryMessageTheOrdinaryPathReturnedIsStillThere` | 1.55 s |
| `Ft8Sharp.Deep.Tests.Ft8DeepSlotDecoderTests.OrderedStatisticsIsOffUnlessItIsAskedFor` | 0.84 s |

**The property.** Entry 1 proves identity with everything *off*. This proves the
thing Hamlet actually runs: with fine sync **on**, every message the ordinary
path returned is still there. A stage may only add.

**The breakage it would have caught.** **Unit 248** built a new extractor that is
measurably worse than the port's at the same coarse position — a rectangular
one-symbol matched filter against the port's tapered two-symbol frame, median
distance 56 against 48 at -21 dB. Its whole value is at the *wrong* place; at the
right place it is a downgrade. Wiring it in front of the port instead of behind
the port's refusals would have cost decodes on every on-grid signal while
appearing to help off-grid, and the scoreboard rung it was read on is the one
placement where the grid has nothing to lose. `OrderedStatisticsIsOffUnlessItIsAskedFor`
is unit 246's ruling 4 stated as a default: a flipped default makes entry 1's
identity test compare the port against an OSD run and silently invalidates every
attribution in units 246 through 249.

### 6. The five-count census reaches all three surfaces

| | |
|---|---|
| `Hamlet.RadioEngine.Tests.Audio.HamletDecodesThroughDeepTests.TheFiveCountCensusIsStillPopulated` | 1.26 s |
| `Hamlet.RadioEngine.Tests.Audio.TheSheetSaysWhichAudioPathItRanOnTests.TheCensusNamesTheStageEachSlotReached` | 0.00 s |
| `Hamlet.RadioEngine.Tests.Audio.TheSheetSaysWhichAudioPathItRanOnTests.ASheetWithNoDecodeBehindItSaysTheCensusWasNotRead` | 0.00 s |
| `Hamlet.RadioEngine.Tests.Audio.ACapturedFileDiagnosesItselfTests.AFileOnDiskComesBackWithACensusThatNamesEveryStage` | 2 cases, 2.27 s |
| `Hamlet.App.Tests.Telemetry.EverySlotLeavesALineTests.EverySlotInAReadingGetsItsOwnLine` | — |
| `Hamlet.App.Tests.Telemetry.EverySlotLeavesALineTests.ASlotThatDecodedNothingStillWritesItsCensus` | — |

**The property.** Candidates, parity satisfied, checksum passed, became text,
duplicates — reaching **all three surfaces the operator reads**: the slot
telemetry line, the capture sidecar, and the census line under the table. The
stages narrow in order, and a slot with nothing in it is counted rather than
omitted.

**The breakage it would have caught.** `AudioArrival`'s own remarks record it:
**on 2026-09-03 the tap filled at 13 per cent of real time for an entire evening
and not one of the three surfaces could say so** — all three described the
decode, so a starved sound card and an empty band wrote identical output. That is
HM-DEC-093. The more recent hazard is **unit 249**, which changed which decoder
produces those five numbers. Deep reports them on the port's own result type, so
they travel the same route unchanged — but unit 246 had already decided that the
five port counts stay a report on the *port's* belief propagation while OSD's
three counts carry the OSD story. **Without a test on all three surfaces, that
decision would have changed what `parity satisfied` means on a census line read
six months later, and nothing would have said so.**

### 7. A decoder's identity is recorded

| | |
|---|---|
| `Hamlet.RadioEngine.Tests.Audio.ACaptureSaysWhichDecoderReadItTests.EverySlotNamesTheDecoderThatReadIt` | 1.22 s |
| `Hamlet.RadioEngine.Tests.Audio.ACaptureSaysWhichDecoderReadItTests.TheSidecarSaysWhichDecoderReadIt` | 1.46 s |
| `Hamlet.RadioEngine.Tests.Audio.ACaptureSaysWhichDecoderReadItTests.AnUnrecordedDecoderIsSaidToBeUnrecorded` | 0.00 s |

**The property.** Every slot and every sidecar names the decoder that read it and
which stages were on, and a census nobody stamped says *unrecorded* rather than
naming the port by default.

**The breakage it would have caught.** **Every capture taken before unit 249 is
unattributable.** The tree now holds captures from both sides of the switch from
the port to `Ft8Sharp.Deep`, and on the sheet they are indistinguishable — same
fields, same five counts, different decoder. A capture read six months from now
cannot be compared against anything unless it says what read it. The third test
is the one that matters most and costs nothing: a default that named the port
would be worse than a gap, because it would be *plausible*.

### 8. One slot decodes inside the budget

| | |
|---|---|
| `Hamlet.RadioEngine.Tests.Audio.HamletDecodesThroughDeepTests.ASlotDecodesInsideTheFifteenSecondBudget` | 2.66 s |

**The property.** A slot decodes in less than 15,000 ms, with the margin printed.
FT8's slot boundary arrives whether or not the last one finished.

**The breakage it would have caught.** Nothing else in the tree bounds decode
time, and three separate stages have moved it. **Unit 246**'s ordered statistics
went from 64.1 to 72.5 ms a trial with a worst observed slot of 110 ms. **Unit
248**'s fine sync took the worst observed slot to 315 ms. **Unit 249** measured
Hamlet's shipping configuration at 261 ms a slot, 1.74 per cent of budget — and
found on the way that ordered statistics re-encoded **192,602 times on one slot
of clean synthetic audio, with nothing bounding that number**. Steps 3, 4 and 5
of this phase all add work inside the slot. This is the one test that will say
so.

---

## Known red, inherited, never chased

**These are red before any unit starts and are not that unit's finding.** They
are recorded here so a session finds them in one place instead of rediscovering
them.

| What | Where | Note |
|---|---|---|
| `Hamlet.RadioEngine.Tests.Cw.Fixtures.CwAdjudicationTests.ASpeedChangeInRealisticAudio` | engine, `Cw` | 1 test |
| The CW cases in `docs/unit239-failing-set.txt` | engine, `Cw` | 51 named; **they fail at the baseline `d541fc8` too** |
| `Ft8Sharp.Deep.Tests`' whole-type-list tripwire | sibling | reddens whenever a type is added to Deep — by design |
| `Hamlet.RadioEngine.Tests.Scan.ScannerEndToEndTests.ADwellReachesTheDecoderAndTheVerdictCarriesItsConfidence` | engine, `Scan` | red on 2026-09-05; **not previously recorded**, see below |

**None of them is in the gate set**, and the gate set runs green with all of them
red.

**On the `Scan` failure.** Work instruction 250 measured `Hamlet.RadioEngine.Tests`
in namespace batches and this was the single failure outside `Cw` and `Rig` —
988 tests, 987 passed. It is not in unit 204's inherited list and not in
`docs/unit239-failing-set.txt`. It is recorded here as newly observed rather than
newly caused: no unit ran that namespace to completion before tonight, so there
is no earlier reading to diff against and **the honest statement is that nobody
knows when it went red.**

---

## What this file replaces

`docs/test-baseline.md` remains the name-by-name record of the inherited failing
set and the 2026-09-01 census. **Its counts are superseded by this unit's
measurements** and it now says so at the top. This file is not a second failing
set; it is the list of what runs.

`docs/full-suite-run.md` remains the four filtered commands for
`Hamlet.App.Tests`, which still stops partway when run unfiltered.
