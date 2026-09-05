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

