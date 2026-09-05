# The gate set — the short list of tests that guard this phase, and why each one is in it

**Written 2026-09-05 by work instruction 250, step 1 of the on-air phase.
Rewritten the same day under Tim's ruling that a unit runs no test suite.**

This file is the answer to a question this repository could not previously
answer: *which tests must run before a change is believed?*

Until now the honest answer was **all of them**, and that answer had stopped
being affordable. `Ft8Sharp.Tests` is **610 tests and about fourteen minutes**.
`Hamlet.RadioEngine.Tests` is **2,281 discovered** and **has never once completed
a whole-project run** — started alone at 08:15 on 2026-09-01 and cut off at
09:16. **Four consecutive reports carried no total for that project.**

**A suite nobody can finish guards nothing.** What follows is the short list that
does. **Tim runs it, at the end of the phase. No unit runs it.**

**The evidence for every entry is `docs/breakage-record.md`**, which lists what
has actually broken in this project, with the unit number and whether a test
would have caught it. Each entry below cites it by number — `B1`, `B7`, and so
on. **An entry that cannot cite one does not belong here.**

---

## The standing rules of this phase

**Tim's rulings of 2026-09-05. They are not a unit's to weigh, and they are
recorded here because this is where the next session will look.**

1. **A unit runs no test suite.** Not filtered, not unfiltered, not "just the
   fast ones". **Tim runs them, once, at the end of the phase.**
2. **A unit may run only the unit test it constructs in that work instruction**,
   filtered by exact name, in the foreground, with a stated timeout of a few
   minutes. Not the project it sits in. Not the channel. **An unfiltered
   `dotnet test` on any project is forbidden.**
3. **Never background a command and poll for it.** **Three sessions were killed
   by the watchdog on 2026-09-05** — `RUN_LEDGER.md` records them at
   `01:32→02:48`, `12:02→12:35` and `13:09→13:47` — every one of them sitting in
   `until grep -q "exited with code" ...; do sleep 15; done` with a 900,000 ms
   timeout. **The watchdog fires after twelve minutes with no status write.** The
   suite was incidental; **the poll was fatal**. If a command cannot finish in
   the foreground inside a stated timeout, **it does not belong in a unit**.
4. **`dotnet build` is allowed**, foregrounded, with a stated timeout.
5. **No test is added — to this list or to the tree — without naming the breakage
   it would have caught.** A test that guards nothing that has ever broken is
   cost without cover. **This rule is what stops the list growing back into the
   suite it replaces**, and work instruction 250 is the first unit bound by it.
6. **Watched-failing-first still holds** for the test a unit writes. That is what
   the allowance in rule 2 is for, and `B11` in the breakage record is why: the
   check written to catch a placeholder token **could not see the token it was
   written for** until it was watched failing against the real file.

Rules 1 and 2 **supersede** the sentence in HM-DEC-154 that a unit runs the gate
set and the channels it touched. A ruling is never edited; the later one wins,
and it is recorded as HM-DEC-155.

**The ladder is a measurement, not a test.** `Ft8LadderHarness.Run` is called when
a step needs a number. It is never in the gate set, and the one ladder entry
below is in it for its *zero-wrong* assertion and not for its rate.

---

## How Tim runs it

```
tools\arbiter\gate-set.bat
```

Four projects, in sequence, **one at a time, never concurrently** — `G3` in the
breakage record is the day contention turned one standing failure into five. Exit
0 is green; exit 1 names the project that failed. It writes a TRX per project
into `.run-unit\trx\` so a failure can be read per test afterwards, through
`tools\arbiter\trx-rank.py`.

When the shell's allow-list refuses a `.bat` invoked directly — which it has done
for a fortnight — the same script is reachable through the route already
established twice in this tree:

```
dotnet build tools\arbiter\gate-set.proj
```

That project runs the `.bat` unmodified. **It is not a second copy of the gate
set.**

**It never runs `Hamlet.App.Tests` unfiltered.** That project stops partway when
run whole; `docs/full-suite-run.md` holds the four filtered commands for it.

**Five of the twenty-one filters name a class rather than a method** —
`Ft8SharpBoundaryTests`, `Ft8DeepIdentityTests`, `Unit222TraceTests`,
`HamletDecodesThroughDeepTests`, `ACaptureSaysWhichDecoderReadItTests`. Each of
those classes today holds **exactly** the methods listed below and nothing else,
counted method by method on 2026-09-05. **A method added to one of those classes
later joins the gate set silently**, which is intended for these five and is the
reason the other ten are named one by one.

---

## What it costs

**This unit ran nothing.** Everything below is either read out of TRX files that
an earlier attempt at this same work instruction left in `.run-unit\trx\` at
12:03 to 12:31 on 2026-09-05, or is an **estimate** and is labelled as one.

### Measured, by an earlier session, from the TRX

**27 of the 29 methods have a measured duration in the tree.** The two
`Hamlet.App.Tests` entries do not — that project was not re-measured.

| Project | Gate-set tests | Measured per-test time |
|---|---|---|
| `Ft8Sharp.Tests` | 7 methods, 8 cases | **239.7 s** |
| `Ft8Sharp.Deep.Tests` | 11 methods, 26 cases | **14.3 s** |
| `Hamlet.RadioEngine.Tests` | 9 methods, 10 cases | **10.1 s** |
| `Hamlet.App.Tests` | 2 methods, 2 cases | **not measured** |
| | **29 methods, 46 cases** | **264.1 s over 44 of the 46 cases** |

**Unit 252 added entry 10** and measured it on this machine, alone by exact name:
**2 s for its sixteen theory cases**, green. That is the second figure in this
table taken by the unit that added the entry, and it is the cheapest entry in the
set.

**Three entries are 91 per cent of that.** `Ft8Unit251SnrAgreementTests` is
144.2 s, `Ft8DeepIdentityTests` is 54.7 s and `Unit222TraceTests` is 40.8 s, and
they are the three entries the phase would be least willing to lose.
**Unit 251's is measured, on this machine, twice** — red at 2 m 25 s and green
at 2 m 24 s — and is the only figure in this table taken by the unit that added
the entry.

### Estimated, not measured

**The whole command has never been run.** Four `dotnet test` invocations pay four
build-and-discovery costs on top of the 262 s of test time, and this tree has no
figure for a single invocation's build. **The estimate is five to six minutes
cold**, and it is an estimate — the script prints its own wall clock every run,
and the first person to run it should write the real figure in here.

**THE THREE-MINUTE WAYPOINT IS NOW PAST**, and it was passed deliberately by unit
251 rather than drifted past. It is a waypoint and not a gate, and the rule that
put it there says so: *a slow gate set that guards the right things beats a fast
one that does not, and no entry was dropped to reach a number*. Entry 9 measures
the one number in this project that an operator reads before he reads the
message, and it caught a 10 dB error on its own first run.

**Against the counts in `docs/test-baseline.md`, corrected below**: the gate set
is **30 cases** against **2,960 discovered across the three projects re-measured
plus whatever `Hamlet.App.Tests` now holds**. The alternative it replaces is
**856 s for `Ft8Sharp.Tests` alone** and an engine project that **has never
finished**. That is the trade, and it is why the target of *under three minutes*
is **a waypoint and not a gate** — no entry was dropped to reach a number.

### What this unit could not confirm

An earlier attempt at this work instruction wrote into this file that the whole
command had been measured at **2 m 39 s** and that the gate set had been
**watched failing** against a deliberately broken property. **Neither claim can
be checked from the tree**: there are no `gate-*.trx` files in `.run-unit\trx\`,
that session was killed by the watchdog at 13:47 and its report was never
committed, and `PROJECT_STATUS.md` at 13:35 said in its own words that the wall
clock *cannot be measured* yet. **Both claims have been removed rather than
carried forward.** A gate set nobody has seen fail is a list and not a gate, and
this one has not yet been seen fail.

---

## The gate set

Each entry names the property it guards and **the breakage it would have
caught**, with the unit number and the `docs/breakage-record.md` entry.
**An entry that cannot name one does not belong here.**

### 1. Deep is a superset of the port — whole-result identity

| | |
|---|---|
| `Ft8Sharp.Tests.Dsp.Ft8DeepIdentityTests.OverAWholeBlockOfTheLadderTheTwoResultsAreIdentical` | 2 cases, 25.1 s |
| `Ft8Sharp.Tests.Dsp.Ft8DeepIdentityTests.OverTheCommittedExampleCaptureTheTwoResultsAreIdentical` | 0.31 s |
| `Ft8Sharp.Tests.Dsp.Ft8DeepIdentityTests.OverEveryReferenceRecordingTheTwoResultsAreIdentical` | 29.3 s |

**The property.** With every stage off, `Ft8DeepSlotDecoder` returns the *whole*
`Ft8SlotResult` that `Ft8SlotDecoder` returns — all five counts, and every
message's text, candidate, frequency and dt, in order. Not `Texts` alone.

**The breakage it would have caught — `B2`, units 245 and 246.** At unit 245 the
sibling held an `Ft8SlotDecoder` and delegated to it, so identity was trivially
true: one decoder called twice. **Unit 246 replaced that with the sibling running
the port's per-candidate loop itself**, through the port's public members,
because ordered statistics decoding had nowhere else to sit. From that commit the
two columns were two pieces of code, and any divergence in the reproduction would
have made the scoreboard's OSD-off column something other than the port. **Every
decibel units 246, 247, 248 and 249 attributed to one named change is
attributable only because this test held.** It is unit 246's **ruling 4**, and
the same three tests assert `deep.Osd is null`, so a later unit that flips the
default cannot quietly turn an identity comparison into a comparison of the port
against an OSD run.

**Why the expensive one stays.** `OverEveryReferenceRecordingTheTwoResultsAreIdentical`
is 69 real off-air recordings and 801 messages across the seam. It is the largest
body of evidence this phase has on the receive side and it costs 29 s. It **skips
rather than fails** when the pinned `ft8_lib` clone is absent, which is what keeps
a fresh clone green.

### 2. The port's parity and CRC-14 gates are in the decode path

| | |
|---|---|
| `Hamlet.RadioEngine.Tests.Audio.HamletDecodesThroughDeepTests.TheReaderReturnsAtLeastWhatThePortDidAndNothingUngated` | 1.28 s |
| `Ft8Sharp.Deep.Tests.Ft8DeepGateTests.ARightOsdCodewordComesBackThroughThePortAsTheMessage` | 0.85 s |
| `Ft8Sharp.Deep.Tests.Ft8DeepSeamProbeTests.AWrongCodewordHandedBackIsStillRefused` | 0.85 s |
| `Ft8Sharp.Deep.Tests.Ft8DeepCombineGateTests.ADeliberatelyWrongPairingIsRefusedByThePortsOwnGates` | 0.90 s |

**The property.** Every message Hamlet shows passed the port's own parity check
and its CRC-14, **whatever route recovered the codeword**. The engine test
re-checks each returned message by packing it back into its 77 bits **rather than
assuming**; the three sibling tests check the two directions of the seam — a
right codeword comes back as the message, a wrong one is still refused.

**The breakages it would have caught — `B12`, `B3` and `B1`, units 245 to 249.**
This is the §0.0 hazard of the whole phase: **Deep returns messages the port
never would**, and a wrong one lands in the operator's table looking exactly like
the others. `B12`: every unit of the previous phase could say the *sibling* put
codewords to the gates, and none of them could say what *Hamlet* displayed had
passed them, because Hamlet was calling the port until unit 249. `B3`: unit 246
found that `Ft8CodewordResult` cannot be constructed outside `Ft8Sharp`, so an
OSD-recovered codeword has to be handed *back* to `Ft8CodewordDecoder` as
normalised ratios — a route that works and that a refactor could trivially
shortcut, which is precisely what `AWrongCodewordHandedBackIsStillRefused` exists
to stop. `B1`: unit 249 found `Ft8Reader` on the overload that hands Deep an
empty span, and this is the test that would have said so.

The arithmetic behind the zeros is real: **unit 247** measured that unbounded
pairing puts the naive false-accept expectation at **366 messages across a
306-trial rung** against 0.24 for the bounded rule it shipped, and **unit 248**
made 4,137 submissions for an expected 0.253 false accepts. **In every case the
observed wrong count was zero, and it was zero because these gates were in the
path.**

### 3. `Ft8Sharp` references nothing outside itself

| | |
|---|---|
| `Ft8Sharp.Tests.Ft8SharpBoundaryTests.DeclaresNoReferences` | 0.00 s |
| `Ft8Sharp.Tests.Ft8SharpBoundaryTests.NoHamletAssemblyArrives` | 0.00 s |
| `Ft8Sharp.Deep.Tests.Ft8DeepBoundaryTests.ThePortsBuiltAssemblyDoesNotReferenceTheSibling` | 0.00 s |
| `Ft8Sharp.Deep.Tests.Ft8DeepBoundaryTests.NoHamletAssemblyArrivesInEitherAssembly` | 0.84 s |

**The property.** The MIT port stays separately publishable. Nothing reaches into
it, and it reaches out to nothing — in particular not to the GPL-3.0 sibling.

**The breakage it would have caught — `B4`, unit 245.** That unit had to wire a
brand-new sibling into a tree where the port already existed, and had to add a
`ProjectReference` to reach it. The reference it added went on
`tests/Ft8Sharp.Tests`, and the arbiter ruled that direction safe *on the grounds
that the mechanical guard already catches the breaching one*. The natural mistake
— putting the reference on `src/Ft8Sharp.csproj` instead — **would have made an
MIT library depend on a GPL-3.0 one, which is a licensing breach that compiles
silently** and that nothing else in this tree would have noticed. These four
tests are why that ruling could be taken in an evening instead of argued.

**These are the cheapest entries in the set.** All four together are under a
second, and they guard the one property in this phase **that cannot be fixed
after a release**.

### 4. The ladder returns nothing that was not sent

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

**The breakage it would have caught — `B5`, unit 247, with unit 248's arithmetic
beside it.** **A wrong decode is counted separately from a missed one,
everywhere**, and every column this project has measured reads zero wrong. That
is not an accident of the code — it is a consequence of **two bounds that are each
one line from being relaxed**. Unit 247 wrote its pairing budget down *before* the
code and then counted rather than estimated: 516 combinations put to the gates
across the whole jittered -21 dB walk, naive expectation 0.031, zero returned;
unbounded, the same arithmetic gives **366**. **A later unit tuning for reach
would move exactly these two numbers**, and without these three tests the first
symptom would be a message on Tim's screen that nobody transmitted.

**Why 40 seconds is worth paying.** The rung test is the only entry that walks
real trials with ground truth. A cheaper zero-wrong assertion exists on 51 trials
rather than 306, and **51 trials at an expectation of 0.03 is not a measurement of
anything**.

### 5. Deep adds and never removes, with the stages on

| | |
|---|---|
| `Ft8Sharp.Deep.Tests.Ft8DeepFineSyncGateTests.WithEverythingOffTheWholeResultIsThePortsWholeResult` | 0.57 s |
| `Ft8Sharp.Deep.Tests.Ft8DeepFineSyncGateTests.EveryMessageTheOrdinaryPathReturnedIsStillThere` | 1.55 s |
| `Ft8Sharp.Deep.Tests.Ft8DeepSlotDecoderTests.OrderedStatisticsIsOffUnlessItIsAskedFor` | 0.84 s |

**The property.** Entry 1 proves identity with everything *off*. This proves the
thing Hamlet actually runs: **with fine sync on, every message the ordinary path
returned is still there.** A stage may only add.

**The breakage it would have caught — `B6`, unit 248.** That unit built a new
extractor that is **measurably worse than the port's at the same coarse
position** — a rectangular one-symbol matched filter against the port's tapered
two-symbol frame, median hard-decision distance 56 against 48 at -21 dB. Its
whole value is at the *wrong* place; at the right place it is a downgrade.
**Wiring it in front of the port instead of behind the port's refusals would have
cost decodes on every on-grid signal while appearing to help off-grid**, and the
scoreboard rung it was read on is the one placement where the grid has nothing to
lose. `OrderedStatisticsIsOffUnlessItIsAskedFor` is unit 246's ruling 4 stated as
a default: **a flipped default makes entry 1's identity test compare the port
against an OSD run** and silently invalidates every attribution in units 246
through 249.

### 6. The five-count census reaches all three surfaces

| | |
|---|---|
| `Hamlet.RadioEngine.Tests.Audio.HamletDecodesThroughDeepTests.TheFiveCountCensusIsStillPopulated` | 1.26 s |
| `Hamlet.RadioEngine.Tests.Audio.TheSheetSaysWhichAudioPathItRanOnTests.TheCensusNamesTheStageEachSlotReached` | 0.00 s |
| `Hamlet.RadioEngine.Tests.Audio.TheSheetSaysWhichAudioPathItRanOnTests.ASheetWithNoDecodeBehindItSaysTheCensusWasNotRead` | 0.00 s |
| `Hamlet.RadioEngine.Tests.Audio.ACapturedFileDiagnosesItselfTests.AFileOnDiskComesBackWithACensusThatNamesEveryStage` | 2 cases, 2.26 s |
| `Hamlet.App.Tests.Telemetry.EverySlotLeavesALineTests.EverySlotInAReadingGetsItsOwnLine` | not measured |
| `Hamlet.App.Tests.Telemetry.EverySlotLeavesALineTests.ASlotThatDecodedNothingStillWritesItsCensus` | not measured |

**The property.** Candidates, parity satisfied, checksum passed, became text,
duplicates — reaching **all three surfaces the operator reads**: the slot
telemetry line, the capture sidecar, and the census line under the table. The
stages narrow in order, and **a slot with nothing in it is counted rather than
omitted**.

**The breakage it would have caught — `B7`, 2026-09-03, HM-DEC-093.**
`AudioArrival`'s own remarks record it: **the tap filled at 13 per cent of real
time for an entire evening and not one of the three surfaces could say so** — all
three described the decode, so **a starved sound card and an empty band wrote
identical output**. The more recent hazard is unit 249, which changed *which
decoder* produces those five numbers. Deep reports them on the port's own result
type, so they travel the same route unchanged — but unit 246 had already decided
that the five port counts stay a report on the *port's* belief propagation while
OSD's three counts carry the OSD story. **Without a test on all three surfaces,
that decision would have changed what `parity satisfied` means on a census line
read six months later, and nothing would have said so.**

### 7. A decoder's identity is recorded

| | |
|---|---|
| `Hamlet.RadioEngine.Tests.Audio.ACaptureSaysWhichDecoderReadItTests.EverySlotNamesTheDecoderThatReadIt` | 1.22 s |
| `Hamlet.RadioEngine.Tests.Audio.ACaptureSaysWhichDecoderReadItTests.TheSidecarSaysWhichDecoderReadIt` | 1.46 s |
| `Hamlet.RadioEngine.Tests.Audio.ACaptureSaysWhichDecoderReadItTests.AnUnrecordedDecoderIsSaidToBeUnrecorded` | 0.00 s |

**The property.** Every slot and every sidecar names the decoder that read it and
which stages were on, and **a census nobody stamped says *unrecorded*** rather
than naming the port by default.

**The breakage it would have caught — `B13`, unit 249.** **Every capture taken
before that unit is unattributable.** The tree now holds captures from both sides
of the switch from the port to `Ft8Sharp.Deep`, and on the sheet they are
indistinguishable — same fields, same five counts, different decoder. A capture
read six months from now cannot be compared against anything unless it says what
read it. **The third test is the one that matters most and costs nothing**: a
default that named the port would be worse than a gap, because it would be
*plausible*.

### 8. One slot decodes inside the budget

| | |
|---|---|
| `Hamlet.RadioEngine.Tests.Audio.HamletDecodesThroughDeepTests.ASlotDecodesInsideTheFifteenSecondBudget` | 2.66 s |

**The property.** A slot decodes in less than 15,000 ms, with the margin printed.
**FT8's slot boundary arrives whether or not the last one finished.**

**The breakage it would have caught — `B1`, unit 249, and the three stages that
have moved the figure.** Nothing else in the tree bounds decode time. Unit 246's
ordered statistics went from 64.1 to 72.5 ms a trial with a worst observed slot
of 110 ms. Unit 248's fine sync took the worst observed slot to 315 ms. Unit 249
measured Hamlet's shipping configuration at **261 ms a slot, 1.74 per cent of
budget** — and found on the way that ordered statistics **re-encoded 192,602
times on one slot of clean synthetic audio, with nothing bounding that number**.
**Steps 3, 4 and 5 of this phase all add work inside the slot. This is the one
test that will say so.**

### 9. The `snr` column carries a ratio and not something else

| | |
|---|---|
| `Ft8Sharp.Tests.Dsp.Ft8Unit251SnrAgreementTests.TheEstimateAgreesWithTheCommandedRatioOverTwoHundredSynthesizedMessages` | 2 m 24 s |

**The property.** The number under the `snr` heading is a **signal-to-noise ratio
in a 2500 Hz reference bandwidth**, and it agrees with the ratio actually
delivered to a **mean absolute error inside 1 dB** over **510 synthesized
messages** across five rungs and two placements, taken **at the place the decoder
itself reports** and against a symbol sequence **packed back out of the decoded
text**, through the decoder Hamlet actually runs. Beside it: every recovered
symbol sequence is byte for byte the transmitted one, and **the whole
`Ft8SlotResult` is identical when the slot is decoded again after the estimate
has been taken** — which is step 2's report-only criterion stated as an assertion
rather than as a claim.

**The breakage it would have caught — `B14`, work instruction 037 to unit 251.**
The column was committed on the assumption that a signal-to-noise ratio is what a
decoder produces. It is not — `Ft8Sharp` returns a Costas sync score, which is
carried on `Ft8Decode.SyncScore` and has sat **one formatting call away from the
cell** for two hundred units. Prose in `DigitalDecodeRow` and `Ft8Reception` is
all that held the line, and prose cannot stop the next edit.

**And it caught something on its own first run.** Watched failing first, the
estimate taken at the decoder's reported place *without* alignment read **3.50 dB
out over 510 messages** — 10.57 dB at the strongest rung, all of it at the cell
centre, and **worsening as the signal got stronger** because the noise estimate
becomes the signal's own leakage into the neighbouring bins. **A number 10 dB out
under the heading an operator reads before the message**, one commit from
shipping. The bound is **1.00 dB against a measured 0.26**, deliberately tighter
than `PHASE_PLAN.md`'s 2 dB display gate, because the two single-edit regressions
this also guards — averaging the grid's decibels rather than inverting its floor,
and dropping `CandidateTimeBiasSeconds` — are **2.5 dB each** and would slip
through 2 dB.

**Why 2 m 24 s is worth paying, and what it buys that is not the rate.** It is
the second-largest entry in the set after `Ft8DeepIdentityTests`, and it is the
only one that measures a number the operator reads directly. It is **not a
sensitivity measurement**: 510 of 510 trials decoded at these rungs, and nothing
in it is claimed at −21 dB, where the rate is about 11 per cent and an agreement
figure would be taken on the trials whose noise happened to be kind.

### 10. The ordered-statistics search costs what it says, and the full basis has not moved

| | |
|---|---|
| `Ft8Sharp.Deep.Tests.Ft8DeepOrderedStatisticsTests.TheCostOfAnOrderInAWindowIsTheNumberOfSubsetsOfTheWindow` | 2 s |

**The property.** Two halves, and the second is the one that protects every figure
this phase has recorded. **First:** the re-encoding count an `(order, window)` cell
spends is `1 + sum over i of C(window, i)`, pinned against **sixteen written-out
triples** from `(0, 91, 1)` to `(4, 30, 31931)` — so a window that is stored and
reported but not honoured by the enumeration is red. **Second, on every one of those
sixteen rows:** the **three-argument call**, which is the one
`Ft8DeepSlotDecoder` and `Ft8DeepOsdSettings.Default` make, spends the pinned
full-basis count and returns **the same 174 bits and the same soft distance** as an
explicit `window: 91`.

**The breakage it would have caught — `B15`, work instruction 252.** A search knob is
one integer, and both of its failure modes leave the decoder returning correct
messages. A window not honoured makes every price in the unit's tables a price nobody
paid and closes step 3's third exit on a fiction. A default path moved by one
re-encoding — an off-by-one in `BasisBits - window`, a default of 90 — invalidates
`docs/unit246-osd.md`'s whole scoreboard, `HM-OPEN-067`'s rows, the -19.81 dB
crossing and the 33 of 306 that unit 252 used as its own `before`, **all together and
all silently**, because they are measurements of that path and are comparable only
while it is byte for byte what it was.

**And it caught the first half on its own first run.** Watched failing with the
window plumbed through and the enumeration untouched: **11 of 16 rows red**,
`(order: 3, window: 40, expected: 10701)` reading `Actual: 125672`, five full-basis
rows green. Right answers at the wrong price, which is the shape of the whole
failure.

**Why 2 s is the whole cost.** It is synthesised ratios and no audio: sixteen
theory rows, the dearest of them order 4 over the full basis at 2 798 342
re-encodings. **It is the cheapest entry in this set and it guards the comparability
of every sensitivity figure the project quotes.**

---

## Known red, inherited, never chased

**These are red before any unit starts and are not that unit's finding.** They are
recorded here so a session finds them in one place instead of rediscovering them.

| What | Where | Note |
|---|---|---|
| `Hamlet.RadioEngine.Tests.Cw.Fixtures.CwAdjudicationTests.ASpeedChangeInRealisticAudio` | engine, `Cw` | 1 test |
| The CW cases in `docs/unit239-failing-set.txt` | engine, `Cw` | 51 named; **they fail at the baseline `d541fc8` too** |
| `Ft8Sharp.Deep.Tests`' whole-type-list tripwire | sibling | reddens whenever a type is added to Deep — **by design** |
| `Hamlet.RadioEngine.Tests.Scan.ScannerEndToEndTests.ADwellReachesTheDecoderAndTheVerdictCarriesItsConfidence` | engine, `Scan` | red on 2026-09-05; **not previously recorded**, see below |

**None of them is in the gate set**, and the gate set is expected to run green
with all of them red.

**On the `Scan` failure.** An earlier attempt at work instruction 250 ran
`Hamlet.RadioEngine.Tests` in namespace batches and this was the single failure
outside `Cw` and `Rig` — 988 tests, 987 passed. The evidence is still in the tree
at `.run-unit\trx\engine-a-not-cw-not-rig.trx`, `outcome="Failed"` at 12:19:21 on
2026-09-05. It is not in unit 204's inherited list and not in
`docs/unit239-failing-set.txt`. It is recorded here as **newly observed rather
than newly caused**: no unit ran that namespace to completion before that run, so
there is no earlier reading to diff against and **the honest statement is that
nobody knows when it went red**.

---

## What this file replaces, and what it corrects

`docs/test-baseline.md` remains the name-by-name record of the inherited failing
set and the 2026-09-01 census. **Three of its counts are false and its top box now
says so**, from TRX files in `.run-unit\trx\`:

| | `test-baseline.md`, 2026-09-01 | measured 2026-09-05 |
|---|---|---|
| `Ft8Sharp.Tests` | 38 discovered, 42 ms | **610 discovered**, 856 s |
| `Ft8Sharp.Deep.Tests` | did not exist | **69 discovered**, 10.4 s |
| `Hamlet.RadioEngine.Tests` | 2,157, never completed, no per-test time | **2,281 discovered**, 1,541 run in four namespace batches with per-test durations |

**The 38 was never wrong — it has grown.** Units 216 through 249 added the ladder,
the scoreboard, the placement traces and the identity comparison to that project.

**`PHASE_PLAN.md` and work instruction 250 both quote 609 tests in about seven
minutes forty-four for `Ft8Sharp.Tests`, and 2,157 for the engine project.** The
TRX in the tree says **610** and **2,281**. The figures in the plan are the
2026-09-01 ones carried forward; **they are stale rather than wrong**, and
nothing in this phase turns on the difference.

**`docs/full-suite-run.md`** remains the four filtered commands for
`Hamlet.App.Tests`, which still stops partway when run unfiltered.

**This file is not a second failing set. It is the list of what runs.**
