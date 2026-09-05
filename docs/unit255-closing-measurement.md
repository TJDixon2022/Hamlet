# Unit 255 — the closing measurement, and what the operator actually gets

**Step 6 of the phase *everything this project has built reaches the operator's screen,
and the decoder is taken as far as it will go*. The last step; steps 0 to 5 are all
`done`.** Root `1.12.56` going in, `Ft8Sharp` `0.10.7`, `Ft8Sharp.Deep` `0.8.0`.
`HEAD 82d7bc2` when the tree was read. **No line under `src/` is changed by this unit
and no type is added to either library.**

§1 to §2 are the trace and the price, written before any test ran. §3 onward is measured,
and every table carries its ladder, its rung, its placement and its trial count on its own
face.

---

## 1. The trace — what the tree says, at file and line

### 1.1 The six columns, defined against the constructor that builds them

`Ft8DeepSlotDecoder`'s first constructor is at `src/Ft8Sharp.Deep/Ft8DeepSlotDecoder.cs:76`
and its signature is, verbatim:

```csharp
public Ft8DeepSlotDecoder(
    Ft8WaterfallGeometry? geometry = null,
    Ft8SyncSearch? search = null,
    int messageLimit = Ft8SlotDecoder.DefaultMessageLimit,
    int maxIterations = LdpcDecoder.DefaultMaxIterations,
    Ft8DeepOsdSettings? osd = null,
    bool rememberHearings = false,
    Ft8DeepFineSyncSettings? fineSync = null,
    Ft8DeepBasebandSettings? baseband = null,
    Ft8DeepSubtractionSettings? subtraction = null)
```

**Every stage parameter is nullable and defaults to off**, so every column below is one
call to this constructor and **no new type is needed for any of them.** That was checked
rather than assumed.

| # | column | the exact call | what it is |
|---|---|---|---|
| 1 | `Ft8Sharp` | `new Ft8SlotDecoder()` | the port. The reference every figure in this project is quoted against |
| 2 | `Deep all off` | `new Ft8DeepSlotDecoder()` | **the attribution column.** Every stage null. It must equal column 1 trial for trial or nothing to its right is attributable |
| 3 | `fine sync only` | `new Ft8DeepSlotDecoder(fineSync: Ft8DeepFineSyncSettings.Default)` | baseband re-synchronisation alone |
| 4 | `OSD only` | `new Ft8DeepSlotDecoder(osd: Ft8DeepOsdSettings.Default)` | ordered statistics alone |
| 5 | **`SHIPPING`** | `new Ft8DeepSlotDecoder(osd: Ft8DeepOsdSettings.Default, fineSync: Ft8DeepFineSyncSettings.Default)` | **transcribed from `Ft8Reception.cs:460`, not assumed** |
| 6 | `subtraction only` | `new Ft8DeepSlotDecoder(subtraction: Ft8DeepSubtractionSettings.Default)` | the strong-signal subtraction pass alone |

**Column 5 is transcribed and it matches what the instruction predicted.**
`src/Hamlet.RadioEngine/Audio/Ft8Reception.cs:460` reads, verbatim:

```csharp
decoder ??= new Ft8DeepSlotDecoder(
    osd: Ft8DeepOsdSettings.Default,
    fineSync: Ft8DeepFineSyncSettings.Default);
```

So **the shipping configuration is ordered statistics plus fine sync, and nothing else.**
Subtraction is not passed. Combining is not passed — `rememberHearings` is left `false`.
There is no surprise here to report at the top of section 3, and that is itself the
finding: what the operator runs is exactly the two stages the phase claimed for him.

The defaults those two names resolve to, checked at their own lines:

| name | line | value |
|---|---|---|
| `Ft8DeepOsdSettings.Default` | `src/Ft8Sharp.Deep/Ft8DeepOsdSettings.cs:150` | `new(2)` — order 2 over the full basis |
| `Ft8DeepFineSyncSettings.Default` | `src/Ft8Sharp.Deep/Ft8DeepFineSyncSettings.cs:52` | the default grid |
| `Ft8DeepSubtractionSettings.Default` | `src/Ft8Sharp.Deep/Ft8DeepSubtractionSettings.cs:179` | `new()`, whose constructor at `:118` defaults `maxPasses` |
| `Ft8DeepCombineSettings(int historyDepth = 1, …, int accumulationDepth = 1)` | `src/Ft8Sharp.Deep/Ft8DeepCombineSettings.cs:108` | both depths default to 1 |

**`Ft8Reception.cs:460` is read and transcribed and nothing downstream of it is touched.**
The identity record at `:475` reads its stage flags off the decoder that ran rather than
off this method's default, which is why a capture can say what decoded it — step 0's
must-pass, and the reason ruling 2 keeps this file closed tonight.

### 1.2 The harness, and the two placements

`Ft8LadderHarness.Run` is at `tests/Ft8Sharp.Tests/Dsp/Ft8LadderHarness.cs:270`:

```csharp
internal static IReadOnlyList<Result> Run(
    double rungDecibels,
    int trials,
    int seed = DefaultSeed,
    IReadOnlyList<Decoder>? decoders = null,
    double frequencyHz = DefaultFrequencyHz,
    int? offsetSamples = null,
    Action<string>? log = null)
```

**The design is paired.** `:304` synthesises the audio once per trial —
`SearchFixture.OneSignal`, then `AddNoise` — and hands the identical array to every
decoder. That is what makes `Discordance` (`:871`) meaningful and what makes two
overlapping Wilson intervals the wrong question, which is `HM-OPEN-078` and the note at
`:140`.

`Result` is at `:91` with `Decoded` (`:106`), `Missed` (`:109`), `Wrong` (`:115`),
`WrongReturns` (`:118`), `Rate`, `Interval` — `Ft8Step6Ladder.Wilson(Decoded, Trials)` —
`MillisecondsPerTrial`, `Outcomes` and `AsRow()`. `Header` is at `:106` of the report
block. All present as the instruction said.

**The two placements, written out as the exact call arguments so tasks 2 and 3 copy
rather than re-derive:**

```csharp
// ON GRID — Ft8LadderHarness.DefaultFrequencyHz = 1000.0 (:64) and
// DefaultOffsetSamples = Ft8Waveform.SamplesPerSymbol(Rate) * 3 (:69).
Ft8LadderHarness.Run(rung, 306, decoders: columns);

// CELL CENTRE — unit 248's WorstFrequencyOffsetHz = 1.56 and WorstOffsetSamples = 480,
// at tests/Ft8Sharp.Tests/Dsp/Ft8Unit248ScoreboardTests.cs:44 and :46.
Ft8LadderHarness.Run(
    rung, 306, decoders: columns,
    frequencyHz: Ft8LadderHarness.DefaultFrequencyHz + 1.56,
    offsetSamples: Ft8LadderHarness.DefaultOffsetSamples + 480);
```

**Both constants were opened and read at their own lines.** Using unit 248's two and no
others is what makes tonight's cell-centre rows comparable with its own.

**The rungs are `-19.0`, `-20.0` and `-21.0`**, one test method each, both placements —
six methods. **306 trials every cell**, which is six whole blocks of
`Ft8Step6Ladder.Population()` (`tests/Ft8Sharp.Tests/Dsp/Ft8Step6Ladder.cs:160`); the
population is `EncodeCorpus.Build()` filtered by `CanBeScored`, **51 messages**, and the
filter is a fixed predicate over a fixed order so trial *i* is the same message in every
process.

`Ft8LadderHarness.RunRepeats` is at `:472` and does take `combinedOsd` (`:483`) and
`combinedFineSync` (`:484`), as unit 254 left it. **It produces three columns, not four**
(`:456`): the port on the first slot alone, the sibling with ordered statistics on the
same first slot, and the combined column fed all R slots and scored on the last. Unit
254's four-row tables are two runs printed together.

### 1.3 The fixtures, and what does not exist

| claim | line | state |
|---|---|---|
| `Ft8CaptureFixtures` | `tests/Ft8Sharp.Tests/Fixtures/Ft8CaptureFixture.cs:505` | present |
| `CapturedFolder` = `captured` | `:508` | present |
| `ProvenanceWsjtx` = `wsjtx` | `:107` | present |
| `RequireCapture()` | `:335` | present |
| `RequireScorable(what)` | `:369` | present |
| `Ft8LadderHarness.ScoreFixture` | `Ft8LadderHarness.cs:1117` | present |
| `Compare` | `:1151` | present |
| `FixtureHeader` | `:1104` | present — **the instruction said `:1103`** |
| `tests/fixtures/ft8/captured/` | — | holds `README.md` and nothing else |
| `tools/Ft8FixtureMaker/` | — | present: `Program.cs`, `Ft8FixtureMaker.csproj`, `make-fixture.proj`, `README.md` |

**No committed command calls `ScoreFixture` over the captured folder.** The only two
callers in the tree are `Ft8FixtureGeneratorTests.cs:278`, which scores a fixture it has
just written, and `Ft8FixtureScoringTests.cs:140`, which asserts that `ScoreFixture`
*refuses* the committed example while `Compare` does not. **Neither iterates the folder.**
That is §6's finding and it is reported rather than filled — see §6.2 for why writing a
test there would breach gate-set rule 5.

### 1.4 What has never been measured — the three claims, checked

All three hold, so all three tasks stay runs rather than becoming citations.

1. **The shipping stack has been measured at -21 dB on grid only.** `docs/unit252-osd-window.md`
   is the only document in the tree with a stacked row, and it is one row: `ship today`,
   `-21.0`, 306 trials, on grid, **35 of 306**, 11.4 per cent, Wilson 8.3–15.5, zero
   wrong, worst slot **330.4 ms, 45×**. Unit 248 measured fine sync alone and ordered
   statistics alone at both placements and all three rungs and **states in terms** at
   §4's preamble that *fine sync and OSD are never stacked here*.
2. **No 50 per cent crossing exists for the shipping stack.** Unit 252's crossing table
   quotes `-19.54` for the port and `-19.81` for order 2 full basis, both with **fine
   sync off**. Unit 248's cell-centre table quotes `-19.61` for fine sync alone. **There
   is no crossing anywhere in the tree for the two stacked**, at either placement.
3. **No run has ever combined accumulation with the shipping stages.** `HM-OPEN-081`
   (`OPEN_ISSUES.md:7`) says so, and the tree agrees: unit 254 §4b's **252 of 306** is
   accumulation at four hearings with the combined column's inner decoder *unstacked*,
   and §4c's **79 of 306** is the stack at **two** hearings with combining at the default
   depth of one. **The cell where both are on has never been run.**

### 1.5 Provenance of every figure this document cites from elsewhere

**Nothing is in this table that was not read out of the document named.** Every one of
these files was opened.

| figure | unit | document, section | `Ft8Sharp.Deep` at | ladder | rung | placement | trials |
|---|---|---|---|---|---|---|---|
| port 248 / Deep-off 248 / o2 full 276 | 252 | `unit252-osd-window.md` §(isolation scoreboard) | **0.5.0** | single-signal `Run` | -19 | on grid | 306 |
| port 73 / Deep-off 73 / o2 full 125 | 252 | same | 0.5.0 | single-signal `Run` | -20 | on grid | 306 |
| port 13 / Deep-off 13 / o2 full 33 | 252 | same | 0.5.0 | single-signal `Run` | -21 | on grid | 306 |
| crossings -19.54 (port), -19.81 (o2 full) | 252 | same, crossing table | 0.5.0 | single-signal `Run` | -19/-20 interp. | on grid | 306 |
| **ship today 35 of 306, 11.4 %, 8.3–15.5, worst slot 330.4 ms 45×** | 252 | same, shipping section | 0.5.0 | single-signal `Run` | -21 | on grid | 306 |
| port 248 / fine sync 268 / OSD 276 | 248 | `unit248-baseband-resync.md` §4.1 | **0.4.0** | single-signal `Run` | -19 | on grid | 306 |
| port 73 / fine sync 95 / OSD 125 | 248 | §4.1 | 0.4.0 | single-signal `Run` | -20 | on grid | 306 |
| port 13 / fine sync 18 / OSD 33 | 248 | §4.1 | 0.4.0 | single-signal `Run` | -21 | on grid | 306 |
| **port 6 / fine sync 277 / OSD 33** | 248 | **§4.2** | 0.4.0 | single-signal `Run` | -19 | **cell centre** | 306 |
| **port 0 / fine sync 73 / OSD 1** | 248 | §4.2 | 0.4.0 | single-signal `Run` | -20 | cell centre | 306 |
| **port 0 / fine sync 3 / OSD 0** | 248 | §4.2 | 0.4.0 | single-signal `Run` | -21 | cell centre | 306 |
| crossings: port not bracketed, fine sync -19.61, OSD not bracketed | 248 | §4.2 | 0.4.0 | single-signal `Run` | -19/-20 | cell centre | 306 |
| worst slot 315.5 ms, 48× | 248 | §4 cost block | 0.4.0 | single-signal `Run` | -21 | cell centre | 306 |
| masked: 1 pass 0, 2 passes 153, 3 passes 153, ceiling 304 | 253 | `unit253-subtraction.md` §8 | **0.6.0** | **masked two-signal** | -18.0 req. | co-frequency, +6 dB loud | 306 |
| sub off 73 / sub on 73, identical trial for trial, +30.0 ms a slot | 253 | **§8.4** | 0.6.0 | single-signal `Run` | -20 | on grid | 306 |
| port 13 / OSD 33 / combined x2 68 / summed x4 252 | 254 | `unit254-combining-depth.md` §4b | **0.7.0→0.8.0** | **repeats**, jittered 2.00 Hz / 480 samples | -21 | on grid + jitter | 306 |
| combined x2 stacked 79, worst slot 99.6 ms | 254 | §4c | 0.7.0→0.8.0 | repeats, jittered | -21 | on grid + jitter | 306 |
| x4 accumulated 41 of 51, x4 pairwise 37 of 51 | 254 | §4a | 0.7.0→0.8.0 | repeats, jittered | -21 | on grid + jitter | **51** |

**The cell-centre figures in the instruction are unit 248's, taken at `Ft8Sharp.Deep`
`0.4.0`, and this document says so on the face of every row that quotes them.** The
sibling is at `0.8.0` tonight — four minor versions on. Unit 254 §2.3 reproduced unit
247's 0.3.0 scoreboard to the decode at 0.7.0 and recorded that only wall-clock numbers
differed, which is the standing evidence that the instrument has not moved across those
versions; **tonight's tasks 2 and 3 re-measure the port, `Deep all off`, fine sync and
OSD at 0.8.0 and so check that claim again rather than resting on it.**

### 1.6 The price, computed rather than copied

**Why this is a task and not an aside.** `docs/unit246-osd.md` §5 item 4 asserted that
*306 trials at order 3 is about 25 minutes of wall clock*; unit 252 measured **91.2 s**
and recorded the premise as wrong by about fifteen-fold. The whole shape of that night
had been built on it.

**Per-trial costs, each read out of the document it was recorded in:**

| column | ms a trial | where |
|---|---:|---|
| 1 `Ft8Sharp` | 63.9 – 66.3 | 252 (63.9–65.5, grid), 248 §4.2 (64.3–66.3, cell centre) |
| 2 `Deep all off` | 63.7 – 64.7 | 252 |
| 3 `fine sync only` | 192.3 – 208.0 | 248 §4.1 (192.3–194.7), §4.2 (194.3–208.0) |
| 4 `OSD only` | 71.7 – 75.8 | 252 (71.7–73.2), 248 §4.2 (72.8–75.8) |
| 5 `SHIPPING` | 200.3 | 252, `ship today` row |
| 6 `subtraction only` | **93.8** | 253 §8.4 — the row reads 93.8 against `sub off`'s 63.8; **the 30.0 ms the instruction quotes is the marginal cost, not the column cost** |

Taking the **top** of each range, so the prediction is conservative:

```
66.3 + 64.7 + 208.0 + 75.8 + 200.3 + 93.8  =  709 ms a trial across all six columns
709 ms x 306 trials                        =  217 s per rung-placement
```

**217 s per rung-placement, and the ceiling is 480 s.** That is a 2.2× margin and it is
**below the 300 s line at which the instruction requires the six columns split into two
methods of three, so they stay in one method per rung-placement.** The synthesis is shared
across the columns rather than per column — `Run` builds the audio once per trial at `:304`
— so it does not multiply, and unit 252's own tables show the rung wall clock tracking the
sum of the column clocks closely.

**Six rung-placements at 217 s is about 22 minutes of foreground `dotnet test` for tasks 2
and 3 together**, in six calls, none of which approaches the twelve-minute watchdog.

**Task 5's cell, priced separately.** `RunRepeats` at `repeats: 4` runs three columns: port
on slot one (≈ 64 ms), OSD on slot one (≈ 72 ms), and the combined column over four slots.
Unit 254 §4b measured `summed x4` unstacked at **259.6 ms a trial**; §4c measured the stack
adding **146.8 − 128.6 = 18.2 ms a trial over two slots**, so about 9 ms a slot, which at
four slots is **+36 ms**. That gives ≈ 296 ms a trial for the stacked accumulated column
and:

```
(64 + 72 + 296) ms x 306 trials  =  132 s, plus four-slot synthesis
```

Call it **150–200 s against a 480 s ceiling.** The instruction's *about five minutes* is
the right order and slightly pessimistic. **The named drop candidate is therefore not
taken: task 5 item 2 runs at the full 306 trials**, and that decision is made here, at the
start, as the instruction requires.

### 1.7 The full cross-product, priced — and what was not run, and why

Step 6's first exit reads *the port, and Deep with each stage on and off*. **The literal
reading is four stages — fine sync, ordered statistics, subtraction, combining — hence 16
configurations, at 3 rungs and 2 placements and 306 trials a cell.** Priced from the same
per-trial figures:

**The eight configurations without combining.** Base 64 ms; the increments are fine sync
**+130**, ordered statistics **+9**, subtraction **+30**. Each increment is present in four
of the eight, so the mean is `64 + (130 + 9 + 30) / 2 = 148.5 ms` a trial.

```
8 configs x 3 rungs x 2 placements x 306 trials  =  14 688 trials
14 688 x 148.5 ms                                =  2 181 s  =  36 minutes
```

**The eight with combining.** Combining is not a stage on `Run` at all — it needs
`RunRepeats`, a different ladder, and at `repeats: 4` each trial costs roughly four slot
decodes plus the pairing work:

```
4 x 148.5 + ~20 ms pairing            =  ~615 ms a trial
14 688 trials x 615 ms                =  9 033 s  =  2.5 hours
```

**Total: about 11 200 s — 3.1 hours of pure decode**, in at least **24 back-to-back
foreground calls** at the 480 s ceiling, against a watchdog that fires at twelve minutes of
silence. **And the slot-decode count is not 29 376 but 73 440**: 14 688 single-slot trials
plus 14 688 four-repeat trials at four slots each. The instruction's 29 376 is the trial
count, and it under-counts the work by two and a half times.

**Ten of the sixteen would also be arithmetically uninformative.** Unit 253 §8.4 measured
subtraction on and off on the single-signal ladder at **73 of 306 and 73 of 306, identical
trial for trial**, with **0 trials only-off and 0 only-on** — there is nothing in an
unmasked slot to subtract, so every configuration differing only in subtraction duplicates
its neighbour on this ladder.

**So what is not run, stated plainly:** the twelve configurations that pair subtraction or
combining with the `Run` ladder's other stages. **What is run instead:** six columns —
every stage measured against the same all-off baseline on the ladder that can show it, plus
the shipping stack — at three rungs and both placements; subtraction's own masked ladder and
combining's own repeats ladder cited from units 253 and 254 with their ladders on the face
of every row; and the one stacked-accumulation cell nobody has run, measured tonight.
**That narrowing is the arbiter's reading and is not re-argued here; it is written down so
it is visible rather than silent.**

---

## 2. How the walks were run

**`tests/Ft8Sharp.Tests/Dsp/Ft8Unit255ClosingLadderTests.cs`, six methods, one rung-placement
each.** Every one was run **alone, by its exact full method name, in the foreground, with a
480 s stated timeout**, with a status line written into `PROJECT_STATUS.md` immediately
before the call and immediately after it returned. **No suite was run. Nothing was
backgrounded and nothing was polled.**

**None of these methods was watched failing first, and that is correct rather than
skipped.** `docs/gate-set.md` rules the ladder a measurement and not a test and never a
gate-set entry, and rule 5 forbids adding a test without naming the breakage it would have
caught. **A closing measurement has no defect to watch fail.** No red was manufactured to
satisfy a rule that does not bind, none of tonight's methods enters the gate set, and none
earns a breakage-record entry.

**One thing had to be fixed before any number survived.** The first run of
`TheClosingLadderAtMinus19OnGrid` **passed in 3 m 55 s and printed nothing**: VSTest does
not surface `ITestOutputHelper` for a test that passes. The walk was re-run with a file
sink added, and every table below is transcribed from a committed artefact under
`docs/unit255-runs/` rather than from a console buffer. **That cost one 4-minute call and
it is recorded rather than hidden.**

**The price held.** §1.6 predicted **217 s** a rung-placement from the recorded per-trial
costs. The measured rung-placement wall clocks are in §4.2's cost table, taken from the run
logs. Every one is comfortably inside the 480 s ceiling and nowhere near the twelve-minute
watchdog, and unit 246 §5 item 4's fifteen-fold error is not repeated.

---

## 3. The closing table, whole

**Six columns, three rungs, both placements, 306 trials every cell.** The ladder is
`Ft8LadderHarness.Run` — one signal, no neighbour — and it is **paired**: the audio is
synthesised once per trial and every column is handed the same array.

**`WRONG` reads 0 in all thirty-six cells.** That is 36 × 306 = **11 016 scored slot
decodes with not one message returned that nobody sent.**

### 3.1 On the grid — 1000.0 Hz, three whole symbol periods in

```
decoder      requested  delivered  trials  DECODED  MISSED  WRONG    rate   lo 95   hi 95    wall s    ms/tr
Ft8Sharp         -19.0    -19.001     306      248      58      0    81.0    76.3    85.0     19.6     64.0
Deep all off     -19.0    -19.001     306      248      58      0    81.0    76.3    85.0     19.6     64.2
fine sync only   -19.0    -19.001     306      268      38      0    87.6    83.4    90.8     58.1    189.9
OSD only         -19.0    -19.001     306      276      30      0    90.2    86.3    93.0     22.4     73.1
SHIPPING         -19.0    -19.001     306      283      23      0    92.5    89.0    94.9     59.9    195.9
subtraction only -19.0    -19.001     306      248      58      0    81.0    76.3    85.0     50.9    166.4

Ft8Sharp         -20.0    -20.000     306       73     233      0    23.9    19.4    28.9     19.8     64.6
Deep all off     -20.0    -20.000     306       73     233      0    23.9    19.4    28.9     19.6     64.1
fine sync only   -20.0    -20.000     306       95     211      0    31.0    26.1    36.4     59.8    195.3
OSD only         -20.0    -20.000     306      125     181      0    40.8    35.5    46.4     22.4     73.3
SHIPPING         -20.0    -20.000     306      138     168      0    45.1    39.6    50.7     61.8    201.8
subtraction only -20.0    -20.000     306       73     233      0    23.9    19.4    28.9     29.1     95.1

Ft8Sharp         -21.0    -21.001     306       13     293      0     4.2     2.5     7.1     19.9     64.9
Deep all off     -21.0    -21.001     306       13     293      0     4.2     2.5     7.1     19.8     64.8
fine sync only   -21.0    -21.001     306       18     288      0     5.9     3.8     9.1     59.9    195.6
OSD only         -21.0    -21.001     306       33     273      0    10.8     7.8    14.8     22.5     73.4
SHIPPING         -21.0    -21.001     306       35     271      0    11.4     8.3    15.5     62.4    203.8
subtraction only -21.0    -21.001     306       13     293      0     4.2     2.5     7.1     21.7     70.8
```

**The attribution column equals the port at all three rungs**, decoded for decoded, missed
for missed, wrong for wrong — asserted per rung. Everything to its right is attributable to
the stage that names it.

**Subtraction alone equals the port at all three rungs too**, which is unit 253 §8.4's
result reproduced at three rungs instead of one: **there is nothing in an unmasked slot to
subtract.** It is the empirical justification for §1.7's narrowing, re-measured tonight
rather than cited.

**The discordant counts for SHIPPING, on identical audio:**

| rung | only the port | only SHIPPING | only `Deep all off` | only SHIPPING |
|---|---:|---:|---:|---:|
| -19.0 | **0** | **35** | **0** | **35** |
| -20.0 | **0** | **65** | **0** | **65** |
| -21.0 | **0** | **22** | **0** | **22** |

**SHIPPING is a strict superset of the port on this audio at every rung.** It takes 35, 65
and 22 trials the port did not and **loses none at any rung** — which is the claim two
overlapping Wilson intervals could never have supported, and it is what the paired design
is for.

### 3.2 At the cell centre — +1.56 Hz, +480 samples

*Unit 248's `WorstFrequencyOffsetHz` and `WorstOffsetSamples` and no others, so these rows
are comparable with its §4.2.*

```
decoder      requested  delivered  trials  DECODED  MISSED  WRONG    rate   lo 95   hi 95    wall s    ms/tr
Ft8Sharp         -19.0    -19.001     306        6     300      0     2.0     0.9     4.2     20.1     65.6
Deep all off     -19.0    -19.001     306        6     300      0     2.0     0.9     4.2     20.1     65.7
fine sync only   -19.0    -19.001     306      277      29      0    90.5    86.7    93.3     63.1    206.1
OSD only         -19.0    -19.001     306       33     273      0    10.8     7.8    14.8     23.1     75.5
SHIPPING         -19.0    -19.001     306      278      28      0    90.8    87.1    93.6     66.1    216.1
subtraction only -19.0    -19.001     306        6     300      0     2.0     0.9     4.2     20.9     68.1

Ft8Sharp         -20.0    -20.000     306        0     306      0     0.0     0.0     1.2     19.9     65.0
Deep all off     -20.0    -20.000     306        0     306      0     0.0     0.0     1.2     19.8     64.8
fine sync only   -20.0    -20.000     306       73     233      0    23.9    19.4    28.9     61.8    201.9
OSD only         -20.0    -20.000     306        1     305      0     0.3     0.1     1.8     22.7     74.1
SHIPPING         -20.0    -20.000     306       73     233      0    23.9    19.4    28.9     64.9    212.1
subtraction only -20.0    -20.000     306        0     306      0     0.0     0.0     1.2     19.9     65.1

Ft8Sharp         -21.0    -21.001     306        0     306      0     0.0     0.0     1.2     19.7     64.5
Deep all off     -21.0    -21.001     306        0     306      0     0.0     0.0     1.2     19.7     64.3
fine sync only   -21.0    -21.001     306        3     303      0     1.0     0.3     2.8     59.3    193.9
OSD only         -21.0    -21.001     306        0     306      0     0.0     0.0     1.2     22.3     72.9
SHIPPING         -21.0    -21.001     306        3     303      0     1.0     0.3     2.8     62.3    203.6
subtraction only -21.0    -21.001     306        0     306      0     0.0     0.0     1.2     19.8     64.7
```

**The attribution column equals the port at all three rungs here too**, which is what unit
248's own rows said it did, and it is asserted rather than assumed. **Subtraction alone
again equals the port at all three rungs.**

**The discordant counts for SHIPPING at the cell centre:**

| rung | only the port | only SHIPPING | only `Deep all off` | only SHIPPING |
|---|---:|---:|---:|---:|
| -19.0 | **0** | **272** | **0** | **272** |
| -20.0 | **0** | **73** | **0** | **73** |
| -21.0 | **0** | **3** | **0** | **3** |

**At -19 dB off the grid the shipping stack reads 272 trials of 306 that the port did not,
and loses none.** That is the largest single figure in this document and §3.4 is what it
means.

### 3.3 The same table read across the two placements

**This is the pair the phase goal is about**, and it is the reason the cell centre had to
be walked rather than cited.

| rung | column | on grid | cell centre | change |
|---|---|---:|---:|---:|
| -19.0 | `Ft8Sharp` | **248** of 306 (81.0 %) | **6** of 306 (2.0 %) | **−242** |
| -19.0 | **`SHIPPING`** | **283** of 306 (92.5 %) | **278** of 306 (90.8 %) | **−5** |
| -20.0 | `Ft8Sharp` | 73 (23.9 %) | 0 (0.0 %) | −73 |
| -20.0 | **`SHIPPING`** | **138** (45.1 %) | **73** (23.9 %) | −65 |
| -21.0 | `Ft8Sharp` | 13 (4.2 %) | 0 (0.0 %) | −13 |
| -21.0 | **`SHIPPING`** | **35** (11.4 %) | **3** (1.0 %) | −32 |

**Read the -19 dB pair.** A sender who moves **one and a half hertz and an eightieth of a
second** — a distance no operator could control or would notice, and one that nothing on
14.074 avoids, because real stations do not arrange themselves on Hamlet's analysis grid —
costs the bare port **242 of its 248 decodes**. It costs what Hamlet actually ships **five
of its 283.** The port falls from 81.0 per cent to 2.0; the shipping stack falls from 92.5
to 90.8.

**That sentence is the phase goal, measured.** It is also the first time in this project
that the configuration on the operator's screen has been read at the placement a real
station lands in.

**At -20 and -21 dB the picture is different and the document says so rather than
generalising from the best rung.** Off the grid at -20 dB the stack keeps 73 of the 138 it
had on the grid, and at -21 dB it keeps 3 of 35. **Off-grid immunity is close to complete
at -19 dB, partial at -20 and slight at -21** — the stack does not make placement free, it
buys back most of it where there is enough signal for fine sync to lock.

### 3.4 Which stage is doing the work, and it is not the same one at each placement

| placement | rung | port | fine sync alone | OSD alone | SHIPPING |
|---|---|---:|---:|---:|---:|
| on grid | -19 | 248 | 268 | **276** | 283 |
| on grid | -20 | 73 | 95 | **125** | 138 |
| on grid | -21 | 13 | 18 | **33** | 35 |
| cell centre | -19 | 6 | **277** | 33 | 278 |
| cell centre | -20 | 0 | **73** | 1 | 73 |
| cell centre | -21 | 0 | **3** | 0 | 3 |

**On the grid ordered statistics carries the column and fine sync adds a little.** Off the
grid **it reverses completely**: fine sync carries the column and ordered statistics adds
essentially nothing — at -20 and -21 the shipping stack equals fine sync alone, decode for
decode.

**Neither stage alone is the answer, and that is the argument for shipping both.** On the
grid, dropping fine sync would cost 7, 13 and 2 decodes; off the grid, dropping fine sync
would cost 245, 72 and 3. **The two stages are not redundant — they cover different
failures**, and the configuration `Ft8Reception.cs:460` builds is the one that covers both.

---

## 4. The crossings and the cost

**Everything in this section comes from §3 and from nothing else.** No figure here is
copied from another unit; §4.3 is the only place other units appear and it is a comparison,
not a source.

### 4.1 The 50 per cent crossing, all six columns, both placements

**Interpolated linearly between the two rungs that straddle 50 per cent, and quoted as an
interpolation.** A column not straddled by -19, -20 and -21 reads **`not bracketed`** with
its direction said. **Nothing is extrapolated** and no crossing is quoted from two rungs on
the same side of 50 per cent.

**On the grid**, every column is straddled by -19 and -20:

| column | -19 dB rate (Wilson 95 %) | -20 dB rate (Wilson 95 %) | crossing |
|---|---|---|---|
| `Ft8Sharp` | 81.0 % (76.3 – 85.0) | 23.9 % (19.4 – 28.9) | **-19.54 dB** (interpolated) |
| `Deep all off` | 81.0 % (76.3 – 85.0) | 23.9 % (19.4 – 28.9) | **-19.54 dB** (interpolated) |
| `fine sync only` | 87.6 % (83.4 – 90.8) | 31.0 % (26.1 – 36.4) | **-19.66 dB** (interpolated) |
| `OSD only` | 90.2 % (86.3 – 93.0) | 40.8 % (35.5 – 46.4) | **-19.81 dB** (interpolated) |
| **`SHIPPING`** | **92.5 % (89.0 – 94.9)** | **45.1 % (39.6 – 50.7)** | **-19.90 dB** (interpolated) |
| `subtraction only` | 81.0 % (76.3 – 85.0) | 23.9 % (19.4 – 28.9) | **-19.54 dB** (interpolated) |

**At the cell centre**, three of the six are not straddled:

| column | -19 dB rate (Wilson 95 %) | -20 dB rate (Wilson 95 %) | crossing |
|---|---|---|---|
| `Ft8Sharp` | 2.0 % (0.9 – 4.2) | 0.0 % (0.0 – 1.2) | **not bracketed** — below 50 % at all three rungs; the crossing lies **above -19 dB** |
| `Deep all off` | 2.0 % (0.9 – 4.2) | 0.0 % (0.0 – 1.2) | **not bracketed** — above -19 dB |
| `fine sync only` | 90.5 % (86.7 – 93.3) | 23.9 % (19.4 – 28.9) | **-19.61 dB** (interpolated) |
| `OSD only` | 10.8 % (7.8 – 14.8) | 0.3 % (0.1 – 1.8) | **not bracketed** — above -19 dB |
| **`SHIPPING`** | **90.8 % (87.1 – 93.6)** | **23.9 % (19.4 – 28.9)** | **-19.61 dB** (interpolated) |
| `subtraction only` | 2.0 % (0.9 – 4.2) | 0.0 % (0.0 – 1.2) | **not bracketed** — above -19 dB |

**`not bracketed` is a result and it is the point.** Three columns never reach 50 per cent
at any rung this ladder measured off the grid, and saying so is the licensed answer — unit
248 wrote the same sentence for two of its own columns. **To quote a number for them would
require extrapolating from two rungs on the same side of 50 per cent, which ruling 3
forbids.**

**The shipping configuration has a 50 per cent crossing for the first time in this project:
-19.90 dB on the grid and -19.61 dB at the cell centre.** Both are interpolations between
-19 and -20; both rungs and both Wilson intervals are printed above.

**Two things about that pair are worth reading twice.**

- **-19.90 dB is 0.36 dB better than the port's -19.54**, and 0.09 dB better than ordered
  statistics alone at -19.81. It is the best crossing anywhere in this project on the grid.
- **-19.61 dB off the grid is better than the port's own on-grid -19.54.** Hamlet's
  shipping decoder, at the worst placement in a coarse cell, crosses 50 per cent at a lower
  ratio than the bare port does at the best placement. **The stack does not merely mitigate
  the off-grid penalty at this rung; it more than erases it relative to the port's best
  case.**
- **Off the grid the shipping crossing and fine sync alone's crossing are the same number
  to two decimals** (-19.610 against -19.608 before rounding). Ordered statistics
  contributes essentially nothing to the crossing off the grid, which is §3.4's finding in
  the crossing's own terms.

### 4.2 The cost — step 6's third exit

**Taken from tonight's worst observed slot across all six rung-placements, not copied from
unit 252.**

| rung-placement | SHIPPING worst slot | its candidates | margin vs 15 000 ms | SHIPPING ms/trial | walk wall clock |
|---|---:|---:|---:|---:|---:|
| -19.0, on grid | 310.3 ms | 23 | 48× | 195.9 | 233.6 s |
| -20.0, on grid | 333.1 ms | 21 | 45× | 201.8 | 215.7 s |
| -21.0, on grid | 333.3 ms | 24 | 45× | 203.8 | 209.5 s |
| **-19.0, cell centre** | **336.8 ms** | **26** | **44.5×** | 216.1 | 216.8 s |
| -20.0, cell centre | 335.5 ms | 25 | 45× | 212.1 | 212.5 s |
| -21.0, cell centre | 331.8 ms | 24 | 45× | 203.6 | 206.6 s |

> **THE ANSWER TO EXIT 3.** The shipping configuration's **worst observed single slot
> tonight is 336.8 ms**, at -19 dB at the cell centre, on a slot carrying 26 candidates.
> **FT8's budget is 15 000 ms, so the margin is 44.5×.** Its **mean cost is 205.6 ms a
> slot** across all six rung-placements and 1 836 scored slots.

**The decoder uses about 2.2 per cent of the slot it has to keep up with, in the worst
single slot observed anywhere tonight.** Unit 252 recorded 330.4 ms and 45× at one rung and
one placement; tonight's figure is 336.8 ms over six rung-placements, **1.9 per cent
higher, at the same margin to the nearest integer.** The cost claim did not depend on the
one cell it had been measured in.

**Per-trial means for every column**, over all six rung-placements:

| column | mean ms/trial | range | worst single slot anywhere |
|---|---:|---|---:|
| `Ft8Sharp` | **64.8** | 64.0 – 65.6 | 106.1 ms (141×) |
| `Deep all off` | **64.7** | 64.1 – 65.7 | 106.0 ms (141×) |
| `fine sync only` | **197.1** | 189.9 – 206.1 | 317.5 ms (47×) |
| `OSD only` | **73.7** | 72.9 – 75.5 | 118.2 ms (127×) |
| **`SHIPPING`** | **205.6** | 195.9 – 216.1 | **336.8 ms (44.5×)** |
| `subtraction only` | **88.4** | 64.7 – 166.4 | 238.7 ms (63×) |

**`Deep all off` costs 0.1 ms a slot less than the port**, which is nothing, and is the
cost evidence for the attribution claim: the sibling with every stage null is the port.

**Subtraction's cost is the one that varies**, from 64.7 ms where nothing decodes to
166.4 ms at -19 dB on the grid where 248 slots decode and each buys a second pass. **That
range is a finding**: unit 253 quoted 30.0 ms as the marginal cost from the -20 dB rung
alone, where 73 slots decode; at -19 dB on the grid the marginal cost is **101.6 ms a
slot**, three times as much, because the stopping rule runs a second pass on every slot
that returned something. **§1.6's prediction used the -20 dB figure and was therefore low
on this one column**; it did not matter because fine sync came in cheaper than predicted,
and the six walks averaged **215.8 s against a predicted 217 s.**

### 4.3 Against the record, column by column

**A difference is a finding to report, not a defect to chase.** The verdict per row:

**On the grid:**

| column | rung | tonight | the record | verdict |
|---|---|---:|---:|---|
| `Ft8Sharp` | -19 / -20 / -21 | 248 / 73 / 13 | 248 / 73 / 13 (units 248 §4.1, 252) | **reproduced, to the decode** |
| `Deep all off` | -19 / -20 / -21 | 248 / 73 / 13 | 248 / 73 / 13 (unit 252) | **reproduced, to the decode** |
| `fine sync only` | -19 / -20 / -21 | 268 / 95 / 18 | 268 / 95 / 18 (unit 248 §4.1) | **reproduced, to the decode** |
| `OSD only` | -19 / -20 / -21 | 276 / 125 / 33 | 276 / 125 / 33 (units 248 §4.1, 252) | **reproduced, to the decode** |
| **`SHIPPING`** | **-21** | **35** | **35** (unit 252) | **reproduced, to the decode** |
| **`SHIPPING`** | **-19 / -20** | **283 / 138** | *no record* | **new — never measured before tonight** |
| `subtraction only` | -20 | 73 | 73 (unit 253 §8.4) | **reproduced, to the decode** |
| `subtraction only` | -19 / -21 | 248 / 13 | *no record on this ladder* | **new**, and equal to the port as §8.4 predicts |

**At the cell centre:**

| column | rung | tonight | the record | verdict |
|---|---|---:|---:|---|
| `Ft8Sharp` | -19 / -20 / -21 | 6 / 0 / 0 | 6 / 0 / 0 (unit 248 §4.2) | **reproduced, to the decode** |
| `fine sync only` | -19 / -20 / -21 | 277 / 73 / 3 | 277 / 73 / 3 (unit 248 §4.2) | **reproduced, to the decode** |
| `OSD only` | -19 / -20 / -21 | 33 / 1 / 0 | 33 / 1 / 0 (unit 248 §4.2) | **reproduced, to the decode** |
| `Deep all off` | all three | 6 / 0 / 0 | *no record* | **new**, and equal to the port, which unit 248 asserted but did not tabulate |
| **`SHIPPING`** | **all three** | **278 / 73 / 3** | *no record* | **new — never measured before tonight** |
| `subtraction only` | all three | 6 / 0 / 0 | *no record* | **new**, and equal to the port |

**Crossings against the record:**

| column, placement | tonight | the record | verdict |
|---|---|---|---|
| `Ft8Sharp`, grid | -19.54 dB | -19.54 dB (units 246, 252) | **reproduced** |
| `fine sync only`, grid | -19.66 dB | -19.66 dB (unit 248 §4.1) | **reproduced** |
| `OSD only`, grid | -19.81 dB | -19.81 dB (units 246, 252) | **reproduced** |
| `Ft8Sharp`, cell centre | not bracketed | not bracketed (unit 248 §4.2) | **reproduced** |
| `fine sync only`, cell centre | -19.61 dB | -19.61 dB (unit 248 §4.2) | **reproduced** |
| `OSD only`, cell centre | not bracketed | not bracketed (unit 248 §4.2) | **reproduced** |
| **`SHIPPING`, both** | **-19.90 / -19.61 dB** | *no record* | **new** |

> **NOT ONE CONTROL FIGURE MOVED.** Twenty-one recorded decode counts and six recorded
> crossings, taken at `Ft8Sharp.Deep` **0.4.0**, **0.5.0** and **0.6.0**, all reproduce
> tonight at **0.8.0**, at both placements, to the decode and to the hundredth of a
> decibel. **That is what says the instrument did not move underneath the three columns
> that are new**, and it is why 283, 278, 138, 73 and 35 can be read as measurements of the
> decoder rather than of the harness. Unit 252's reproduction of unit 246 is the precedent;
> this is the same check run across four minor versions and two placements at once.

---

## 5. The two stages that need their own ladder, and the cell nobody had run

### 5.0 Why these are not in §3's table, said plainly

**Subtraction and combining cannot be shown on the closing table's ladder, and putting them
there anyway would be a false comparison.**

- **Subtraction needs a second signal to subtract.** §3 measured `subtraction only` on the
  single-signal ladder at all six rung-placements and it **equalled the port in every one**.
  That is not subtraction failing; it is the stopping rule correctly finding nothing to
  remove. **Its ladder is unit 253's masked two-signal ladder** and its figures are §5.1's.
- **Combining needs the same station heard more than once.** The closing table gives each
  trial one slot. **Its ladder is `RunRepeats`**, four slots a trial with jitter between
  them, and its figures are §5.2's and §5.3's.

**No row in §5 is comparable with any row in §3**, and the two must not be read side by
side. Every row below carries its ladder, its rung, its placement, its trial count and the
`Ft8Sharp.Deep` version it was taken at on its own face, for exactly that reason.

### 5.1 Subtraction — cited from unit 253, not re-run

**Ladder: the masked two-signal ladder.** Two stations at the same frequency and the same
sample, the loud one 6 dB up. **Rung -18.0 dB requested, 306 trials, `Ft8Sharp.Deep` 0.6.0.**
Source: `docs/unit253-subtraction.md` §8.

| configuration | ladder | rung | placement | trials | Deep | decoded | wrong |
|---|---|---|---|---:|---|---:|---:|
| single pass | masked two-signal | -18.0 | co-frequency, +6 dB loud | 306 | 0.6.0 | **0 of 306** (0.0 – 1.2) | **0** |
| two passes | masked two-signal | -18.0 | co-frequency, +6 dB loud | 306 | 0.6.0 | **153 of 306** (44.4 – 55.6) | **0** |
| three passes | masked two-signal | -18.0 | co-frequency, +6 dB loud | 306 | 0.6.0 | **153 of 306** | **0** |
| ceiling — loud station absent, identical noise draw | masked two-signal | -18.0 | co-frequency | 306 | 0.6.0 | **304 of 306** (97.6 – 99.8) | **0** |

Discordance against the single pass: **0 and 153**. **Zero wrong across 3 468 slot
decodes.** And on the **single-signal** ladder at -20 dB, subtraction on and off read
**73 of 306 and 73 of 306, identical trial for trial** — which §3 reproduced tonight at all
three rungs and both placements.

**What it is worth, in one line: subtraction recovers 153 of the 304 messages that were
there to recover under a co-frequency 6 dB neighbour, and nothing at all when there is no
neighbour.** The 151 it does not recover are `HM-OPEN-079` and are outside step 6.

### 5.2 Combining — cited from unit 254, not re-run

**Ladder: `RunRepeats`, four slots a trial, jittered 2.00 Hz and 480 samples between
hearings.** **Rung -21.0 dB, 306 trials, `Ft8Sharp.Deep` 0.7.0 → 0.8.0.** Source:
`docs/unit254-combining-depth.md` §4b and §4c.

| configuration | ladder | rung | trials | Deep | decoded | wrong |
|---|---|---|---:|---|---:|---:|
| the port, one slot | repeats, jittered | -21.0 | 306 | 0.8.0 | **13 of 306** (2.5 – 7.1) | **0** |
| single slot + ordered statistics | repeats, jittered | -21.0 | 306 | 0.8.0 | **33 of 306** (7.8 – 14.8) | **0** |
| combined ×2 | repeats, jittered | -21.0 | 306 | 0.8.0 | **68 of 306** (17.9 – 27.2) | **0** |
| combined ×2, **stacked** with fine sync and OSD | repeats, jittered | -21.0 | 306 | 0.8.0 | **79 of 306** (21.2 – 31.0) | **0** |
| **four hearings accumulated**, unstacked | repeats, jittered | -21.0 | 306 | 0.8.0 | **252 of 306** (77.7 – 86.2) | **0** |

**236 of 306** trials had no single slot decode alone while the combination did. **470 of
470** combined decodes verified against the message that went in, **0 wrong across 5 777
submissions**, worst slot **85.4 ms accumulated and 99.6 ms stacked**.

**THE CAVEAT THAT TRAVELS WITH 252 OF 306, AND IT IS UNIT 254'S OWN.** `RunRepeats` scores
the combined column on the union over the trial's slots, so **a four-repeat column gets four
single-slot attempts as well as deeper sums.** 68 → 252 is **not** the gain from
accumulation; it conflates more hearings with more chances. **Unit 254 §4a is the isolation
and it says accumulation is worth +4 of 51 at four hearings.** The honest reading is: *a
station heard four times, with the combiner accumulating, is read 252 times in 306 against
13 for one hearing through the port.*

### 5.3 The cell nobody had run — measured tonight

**`HM-OPEN-081`.** Accumulation stacked with the stages Hamlet ships. One call, run at the
**full 306 trials — the named drop candidate was NOT taken**, and that was decided at the
start of the task rather than at the end: §1.6 priced it at 150–200 s against a 480 s
ceiling and the six closing walks had already shown the pricing accurate to under one per
cent. **It ran in 145.3 s.**

```csharp
Ft8LadderHarness.RunRepeats(
    -21.0, 306, repeats: 4,
    frequencyJitterHz: 2.0, offsetJitterSamples: 480,
    combining: new Ft8DeepCombineSettings(historyDepth: 3, accumulationDepth: 3),
    combinedOsd: Ft8DeepOsdSettings.Default,
    combinedFineSync: Ft8DeepFineSyncSettings.Default)
```

```
decoder      requested  delivered  trials  DECODED  MISSED  WRONG    rate   lo 95   hi 95    wall s    ms/tr
single slot      -21.0    -21.001     306       13     293      0     4.2     2.5     7.1     19.5     63.8
single + OSD     -21.0    -21.001     306       33     273      0    10.8     7.8    14.8     22.2     72.5
summed x4        -21.0    -21.000     306      254      52      0    83.0    78.4    86.8     90.4    295.5
```

> **THE CELL: 254 of 306, 83.0 per cent, Wilson 78.4 – 86.8, zero wrong.**
> Ladder `RunRepeats`, four slots a trial, jittered 2.00 Hz and 480 samples, -21.0 dB,
> 306 trials, `Ft8Sharp.Deep` 0.8.0.

| | this cell (stacked) | unit 254 §4b (unstacked) |
|---|---:|---:|
| decoded | **254 of 306** | 252 of 306 |
| trials no single slot decoded alone and the combination did | **206 of 306** | 236 of 306 |
| trials some slot decoded alone | **48 of 306** | 16 of 306 |
| **lost by combining** | **0** | 0 |
| candidate pairs the rule looked at | **299 908** | 299 908 |
| **combinations submitted to the port** | **2 232** | **2 232** |
| the port took past both gates | **736** | 736 |
| hearings in the deepest combination | **4** (6.02 dB if independent) | 4 |
| messages the combining stage added | **458** | 470 |
| **of those, the message that was sent** | **458** | 470 |
| **of those, a message that was NOT sent** | **0** | **0** |
| naive expected messages nobody sent | 0.136 | 0.136 |
| worst single slot | **109.6 ms, 137×** | 85.4 ms, 176× |

**Three things this settles.**

1. **Stacking the shipping stages onto accumulation buys +2 of 306, and costs nothing in
   submissions.** 2 232 combinations submitted in both, 736 accepted in both, 299 908 pairs
   offered in both — **identical budgets to the unit.** That is the same result unit 254
   §4c found at two hearings, where the stack also submitted exactly 516 in both: **ordered
   statistics and fine sync change which candidates decode, not how many combinations are
   attempted, so the false-accept exposure does not move.**
2. **The gain is small because the two overlap.** With the stack on, **48 trials of 306
   decode from some single slot alone against 16 without it** — ordered statistics is
   reading slots the combination would otherwise have had to rescue, so only-combined falls
   from 236 to 206 while the total rises. **The two mechanisms compete for the same trials
   at four hearings**, which is why +11 of 306 at two hearings (79 against 68) becomes +2 at
   four. **Accumulation has already taken most of what there is to take.**
3. **Zero wrong, and 458 of 458 combined decodes verified**, against 0.136 naively expected
   false accepts over 2 232 submissions. **Nothing here manufactures a message.**

**`HM-OPEN-081` is answered and is closed by this row.** The recommended configuration was
run; it reads 254 of 306; it costs 109.6 ms in the worst slot, a **137× margin**.

**And it changes nothing about what ships.** Ruling 2 stands: combining is off by default,
this figure is a measurement handed to Tim, and §6.2 lists the surfaces that would have to
move before any of it reached a radio.

---

## 6. What the operator should now see, and what he does not

### 6.0 The sentence that goes before all the others

> **Every figure in this document came off a synthesizer.** The ladder builds the audio
> itself, so it knows exactly what it transmitted and can score what came back against it.
> **Nothing in this phase has run on air.** No number here was measured against a real
> signal, a real band or a real station, and none of them is a prediction of what a
> particular evening on 14.074 will give.

That is said first and plainly rather than buried, because everything below is worth less if
it is read as an on-air result.

### 6.1 What he gets

**Each claim carries its figure and its trial count.**

**1. Hamlet decodes through `Ft8Sharp.Deep`, with fine sync and ordered statistics both on.**
`src/Hamlet.RadioEngine/Audio/Ft8Reception.cs:460` builds
`new Ft8DeepSlotDecoder(osd: Ft8DeepOsdSettings.Default, fineSync: Ft8DeepFineSyncSettings.Default)`
for every slot. That is step 0, and it is what every figure in §3's `SHIPPING` column was
measured on.

**2. On a station that lands on the analysis grid, it reads more than the port at every
ratio measured.** At 306 trials a rung:

| ratio | the bare port | **what Hamlet runs** |
|---|---|---|
| -19 dB | 248 of 306 — 81.0 % (76.3 – 85.0) | **283 of 306 — 92.5 % (89.0 – 94.9)** |
| -20 dB | 73 of 306 — 23.9 % (19.4 – 28.9) | **138 of 306 — 45.1 % (39.6 – 50.7)** |
| -21 dB | 13 of 306 — 4.2 % (2.5 – 7.1) | **35 of 306 — 11.4 % (8.3 – 15.5)** |

**It never reads fewer.** On identical audio it took 35, 65 and 22 trials the port did not
and **lost none at any rung.**

**3. On a station that does not land on the grid — which is every real station — the
difference is the whole decoder.** Real signals do not arrange themselves on Hamlet's
analysis grid; at the centre of one coarse cell, 1.56 Hz and 480 samples off:

| ratio | the bare port | **what Hamlet runs** |
|---|---|---|
| -19 dB | **6 of 306 — 2.0 %** (0.9 – 4.2) | **278 of 306 — 90.8 %** (87.1 – 93.6) |
| -20 dB | 0 of 306 — 0.0 % (0.0 – 1.2) | **73 of 306 — 23.9 %** (19.4 – 28.9) |
| -21 dB | 0 of 306 — 0.0 % (0.0 – 1.2) | **3 of 306 — 1.0 %** (0.3 – 2.8) |

**In plain words: a station a hertz and a half off Hamlet's grid at -19 dB is one the bare
port almost never hears — six times in 306 — and one Hamlet hears nearly always, 278 times
in 306.** The stack takes 272 of those trials the port did not, and loses none. **That is
the single largest thing this phase changed for the operator**, and it is the reason both
stages ship rather than either one: on the grid ordered statistics does the work, off the
grid fine sync does (§3.4).

**4. It crosses 50 per cent at -19.90 dB on the grid and -19.61 dB off it**, both
interpolated between the -19 and -20 dB rungs at 306 trials each (§4.1). **The off-grid
crossing is better than the bare port's own on-grid -19.54 dB.**

**5. It keeps up with the air with room to spare.** The worst single slot observed anywhere
tonight — over six rung-placements and 1 836 scored slots — took **336.8 ms of FT8's
15 000 ms**, a **44.5× margin**, and the mean was **205.6 ms a slot, 1.4 per cent of the
budget** (§4.2).

**6. The `snr` column carries a real ratio.** That is step 2. Measured against the ladder's
commanded ratio over **510 messages**, at both placements: **mean absolute error 0.26 dB,
95th percentile 0.62 dB** (`docs/unit251-snr-trace.md`). 510 trials, 510 decoded, 510
measured — no message was skipped to improve the figure.

**7. Nothing it shows him was invented.** **Zero wrong decodes in all thirty-six cells of
§3's table — 11 016 scored slot decodes, not one message returned that nobody sent** — and
zero in every cited table in §5. Both of the port's gates, parity and CRC-14, stay in the
path for every message however it was recovered; nothing in `Ft8Sharp.Deep` decides that a
message is real.

### 6.2 What he does NOT get

> **Subtraction and combining are OFF by default. No radio does either. Nobody's Hamlet has
> ever done either.**

This has to be said on the face of the closing table, because two of the four stages this
phase measured produce the largest numbers in it and **none of those numbers is on anyone's
screen.**

| stage | what it reads in this document | what a radio does today |
|---|---|---|
| **subtraction** | 153 of 306 recovered under a co-frequency neighbour 6 dB up, against a ceiling of 304 (§5.1) | **nothing — off by default** |
| **combining, four hearings accumulated** | 252 of 306 unstacked, **254 of 306 stacked**, against the port's 13 (§5.2, §5.3) | **nothing — off by default** |

**254 of 306 against the port's 13 is the most impressive figure in this document and it is
the one furthest from the operator.** If this table printed it without this paragraph, this
project would have told its owner he has something he does not have.

**What would have to change first**, from `docs/unit253-subtraction.md` **§6** and
`docs/unit254-combining-depth.md` **§1.7** — *note that unit 253's list is at §6, not §1.7
as this unit's instruction had it*:

**For subtraction, five surfaces:**

| # | surface | what must change |
|---|---|---|
| 1 | `Ft8Reception.cs:460` | the construction gains a subtraction settings argument, and the pass budget must be reconciled with the 15 000 ms slot budget for the **shipping** configuration, not the isolation |
| 2 | `Ft8DecoderIdentity` | carries two stage flags today; subtraction is a third. A capture written by a subtracting decoder and read back as a two-flag identity says a pass ran that the reader cannot see |
| 3 | the five-count census | `Ft8SlotResult`'s five counts are per `Decode` call; under multi-pass they are per **pass** |
| 4 | the telemetry line | must say how many passes ran and how many messages were subtracted — `CLAUDE.md` §0.0.1 |
| 5 | the capture sidecar | same as 2 and 4, on disk. A sidecar that cannot say whether subtraction was on is breakage `B13`'s shape exactly |

**For combining, seven — a larger surface still:**

| surface | what a cross-slot combiner needs |
|---|---|
| `Ft8Reception.cs:460` | an `Ft8DeepRepeatDecoder` **held across slots** rather than constructed per slot, with `Reset()` on band change, frequency change or a gap in the slot sequence — a slot heard after a five-minute silence must not be paired with the slot before it |
| `Ft8DecoderIdentity` | a third flag, plus the depth and the partner count |
| the five-count census | the four of `Ft8DeepCombineCounts` beside the port's five, plus hearings-per-combination |
| the telemetry line | must distinguish *this slot decoded it* from *this slot plus the previous two decoded it*, or an operator cannot tell a fresh decode from a recovered one |
| the capture sidecar | a combined message belongs to more than one slot; the per-message rows need which slots the sum drew on |
| **the memory** | none today. At most 140 hearings × 174 floats — about **97 kB a slot**, under a megabyte at the maximum depth of eight |
| the time | plus one `Normalise` and one `Decode` per submission; tonight's stacked accumulation measured a worst slot of **109.6 ms, 137×** (§5.3) |

**Every one of those is a change to what a capture records about itself, which is step 0's
must-pass.** That is why nothing was turned on tonight and why the decision is Tim's, with
these figures in front of him, rather than a session's at the end of a long night.

### 6.3 The two deferred criteria, and the artefact that settles both

**`PHASE_PLAN.md` marks two criteria *deferred*:** step 2's *agreement with WSJT-X on a real
capture* (`PHASE_PLAN.md:256`) and step 4's *decodes per slot against WSJT-X on a real
capture* (`:305`). **Both are settled by the same artefact, and neither can be attempted
here: there is no WSJT-X on the development machine and no unit may assume one.**
`decode_ft8.exe` is never substituted for it.

**What is needed — one pair of files, same stem, committed together:**

```
tests/fixtures/ft8/captured/<stem>.wav           the audio, exactly as it was recorded
tests/fixtures/ft8/captured/<stem>.fixture.txt   what WSJT-X returned for it, message by message
```

- **Format:** `docs/ft8-capture-fixture-format.md`.
- **Provenance must be `wsjtx`**, which is `Ft8CaptureFixtures.ProvenanceWsjtx` at
  `tests/Ft8Sharp.Tests/Fixtures/Ft8CaptureFixture.cs:107`. `RequireScorable` (`:369`)
  refuses to let a claim about WSJT-X be made from a worked example, and
  `Ft8FixtureScoringTests.ScoreFixtureRefusesTheExampleWhileCompareDoesNot` is the test that
  holds it to that. `RequireCapture` (`:335`) makes a fixture that names a `.wav` which is
  not there a hard failure rather than a silent pass.

> **THE COMMAND TIM RUNS AT THE SHACK**, from the folder's own README:
>
> ```
> dotnet run --project tools/Ft8FixtureMaker -- <capture.wav>
> ```
>
> `tools/Ft8FixtureMaker/` exists — `Program.cs`, `Ft8FixtureMaker.csproj`,
> `make-fixture.proj` and a README.

**`tests/fixtures/ft8/captured/` holds a `README.md` and nothing else**, and that is the
correct state here: the radio lives on a different computer (`SHACK_FACTS.md` FACT-004).
**Zero real fixtures passes cleanly and is not a defect.**

**And the honest state of the scoring side, which is the part a finished-looking night would
skip.** `Ft8LadderHarness.ScoreFixture` exists at `Ft8LadderHarness.cs:1117` and `Compare`
at `:1151`. **No committed command calls `ScoreFixture` over the captured folder.** The only
two callers in the tree are `Ft8FixtureGeneratorTests.cs:278`, which scores a fixture it has
just written, and `Ft8FixtureScoringTests.cs:140`, which asserts that `ScoreFixture` refuses
the committed example. **Neither iterates the folder. No test in the tree does.**

**That gap is named here and deliberately not filled.** Gate-set rule 5 forbids adding a
test without naming the breakage it would have caught, and **a test guarding a folder that
has never held a file guards nothing** — it would pass vacuously today and go on passing
vacuously. **Naming the gap is this unit's deliverable; filling it is the first job of
whoever holds the first fixture**, and it should be written against that fixture, watched
failing first, in the unit that adds it.

### 6.4 The closing position of the phase

**This is where the phase stands. It is not a declaration that the phase is closed** —
that reading is the next arbiter's from `PHASE_OUTCOME.md`, and `PHASE_CONTROL.md` §6
forbids a phase being reopened, which is exactly why nobody closes one in passing.

| step | state | units | the one figure it produced |
|---|---|---:|---|
| 0 — Hamlet decodes through `Ft8Sharp.Deep` | done | 1 | Deep carries **27** candidates through to text where the port carries **9**, at **261 ms** of a 15 000 ms slot |
| 1 — the gate set exists, slow tests named | done | 3 | **12** gate-set entries; the ladder is ruled a measurement and never one of them |
| 2 — the `snr` column shows a number | done | 2 | **0.26 dB** mean absolute error, 95th percentile **0.62 dB**, over **510** messages |
| 3 — ordered statistics, as far as it goes | done | 2 | **33 of 306** at -21 dB on grid, crossing **-19.81 dB** |
| 4 — strong signals subtracted, slot read again | done | 2 | **153 of 306** recovered under a co-frequency neighbour 6 dB up, against a ceiling of **304** |
| 5 — repeated transmissions combined | done | 2 | **252 of 306** at -21 dB from four hearings, against the port's **13** |
| **6 — the closing measurement** | **this unit, the first spent on it** | **1** | **the shipping stack at 283 / 138 / 35 of 306 on grid and 278 / 73 / 3 at the cell centre, crossing at -19.90 and -19.61 dB, worst slot 336.8 ms at 44.5×, zero wrong in 11 016 slot decodes** |

**Step 6 is the last step of the phase**, and every other step was closed before this unit
began.

