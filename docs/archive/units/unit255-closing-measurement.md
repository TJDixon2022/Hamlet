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

> ## AMENDED BY UNIT 256, 2026-09-05
>
> **Four things changed and nothing was rewritten silently.**
>
> 1. **§4.1 is replaced whole.** Every crossing now carries **a band** computed from the two
>    rungs it was interpolated between, and every row that read `not bracketed` at the cell
>    centre now carries a crossing measured tonight at 306 trials. **The earlier §4.1 gave
>    eight bare point values and marked four cell-centre rows `not bracketed`.** The point
>    values themselves did not move — all eight reproduce to the hundredth of a decibel when
>    the arithmetic is executed rather than done by hand.
> 2. **§4.1's prose said *three of the six are not straddled* and its own table marked
>    four.** The table was right. The replacement says **four**, and says what the earlier
>    text said.
> 3. **A new §5.4 gives combining an on-and-off panel of its own**, on the repeats ladder,
>    with its own 50 per cent crossing — **-21.48 dB**, which no unit in this project had
>    ever quoted. §5.2's citation from unit 254 stays exactly as it was.
> 4. **§6.1 item 4 carries the two operator crossings' bands.** Nothing else in §6.1 moved
>    and §6.4 gains a note; the checking is recorded at the foot of §6.1.
>
> **§3's thirty-six cells are untouched.** Not re-run, not re-tabulated, not re-worded.
> §4.2, §4.3, §5.0 to §5.3, §6.0, §6.2 and §6.3 are untouched. **And §6.1 item 7's sentence
> about zero wrong decodes now carries a qualification**, because unit 256 measured one — at
> -23 dB, on the repeats ladder, two decibels below anything in this document. It is
> `HM-OPEN-082` and it changes no figure printed here.
>
> Unit 256's working record — the trace, the price, the predictions and what they turned out
> to be — is `docs/unit256-crossings-and-combining.md`. Its artefacts are
> `docs/unit256-runs/`.

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

### 4.1 The 50 per cent crossing, all six columns, both placements, each with its band

> **REPLACED WHOLE BY UNIT 256, 2026-09-05.** The earlier text gave eight bare point values
> and marked four cell-centre rows `not bracketed`; **its prose said three of the six were
> not straddled while its own table marked four**, and the table was right. The point values
> are unchanged — all eight reproduce to the hundredth of a decibel when the arithmetic is
> executed rather than done in prose (`docs/unit256-runs/crossing-bands.txt`). **What is new
> is the band on every one of them, and a crossing for every row that had none.**

**The crossing is interpolated linearly between the two rungs that straddle 50 per cent, and
is quoted as an interpolation.** Nothing is extrapolated and no crossing is quoted from two
rungs on the same side of 50 per cent.

#### What the band is, and what it is not

**THE BAND IS NOT A CONFIDENCE INTERVAL ON THE CROSSING AND THIS DOCUMENT DOES NOT CALL IT
ONE.** It is obtained by pushing each rung's **95 per cent Wilson interval** through the
**same linear interpolation** the point crossing uses, under the assumption this section
already makes — **that the decode rate moves linearly in decibels between two rungs one
decibel apart.**

- Join the two rungs' **upper** Wilson bounds: that is the optimistic curve, and it reaches
  50 per cent at the **lower (better)** ratio.
- Join the two **lower** bounds: that is the pessimistic curve, and it reaches 50 per cent at
  the **higher** ratio.
- **The pair is the band.**

**Where a bound curve does not reach 50 per cent inside the bracket, that side of the band is
OPEN and is written as open.** It is never extrapolated. Ruling 1 of unit 256, and unit 255's
ruling 3 before it.

**Why it is worth having at all.** A crossing published as a bare number invites the
comparison `CLAUDE.md` §0.0 forbids: someone sets -19.90 beside another decoder's -19.7 and
declares a 0.2 dB win. **The narrowest band in either table below is 0.126 dB wide and one is
open**, so a 0.2 dB difference is inside this project's own bracket at 306 trials and cannot
be called a win.

**Computed, not hand-derived.** `Ft8Unit256CrossingBand` and
`Ft8Unit256CrossingIntervalTests.TheCrossingBandBracketsThePointAndIsBuiltFromTheRungsOwnIntervals`,
watched failing first on an inverted bound pairing
(`docs/unit256-runs/task2-watched-failure.txt`).

#### On the grid — every column straddled by -19 and -20, 306 trials a rung

| column | -19 dB rate (Wilson 95 %) | -20 dB rate (Wilson 95 %) | crossing | **band** |
|---|---|---|---|---|
| `Ft8Sharp` | 81.05 % (76.28 – 85.04) | 23.86 % (19.42 – 28.94) | **-19.54 dB** | **-19.62 to -19.46 dB** (0.162 wide) |
| `Deep all off` | 81.05 % (76.28 – 85.04) | 23.86 % (19.42 – 28.94) | **-19.54 dB** | **-19.62 to -19.46 dB** (0.162 wide) |
| `fine sync only` | 87.58 % (83.41 – 90.82) | 31.05 % (26.12 – 36.44) | **-19.66 dB** | **-19.75 to -19.58 dB** (0.167 wide) |
| `OSD only` | 90.20 % (86.35 – 93.05) | 40.85 % (35.49 – 46.44) | **-19.81 dB** | **-19.92 to -19.71 dB** (0.209 wide) |
| **`SHIPPING`** | **92.48 % (88.97 – 94.94)** | **45.10 % (39.62 – 50.70)** | **-19.90 dB** | **OPEN beyond -20 dB, to -19.79 dB** |
| `subtraction only` | 81.05 % (76.28 – 85.04) | 23.86 % (19.42 – 28.94) | **-19.54 dB** | **-19.62 to -19.46 dB** (0.162 wide) |

> **`SHIPPING`'s BAND IS OPEN ON ITS OPTIMISTIC SIDE AND THE CELL SAYS SO.** Its -20 dB
> rung's Wilson upper bound is **50.700 per cent — still above 50** — so the optimistic curve
> never crosses inside `[-20, -19]` at all. The honest statement is *at least as good as
> -19.79 dB, and this ladder cannot put a floor under how much better.* **It is open by 0.70
> percentage points**, which is another way of saying that at 306 trials the -20 dB rung is
> one decode away from straddling on its own.

#### At the cell centre — 306 trials a rung, and the four unbracketed rows are bracketed

**FOUR of the six were not straddled by -19, -20 and -21**, not three: `Ft8Sharp`,
`Deep all off`, `OSD only` and `subtraction only`. **All four are bracketed below by rungs
unit 256 measured at -17 and -18 dB**, at 306 trials each, at this same placement.

| column | upper rung (Wilson 95 %) | lower rung (Wilson 95 %) | crossing | **band** |
|---|---|---|---|---|
| **`Ft8Sharp`** | **-17 dB: 235 of 306, 76.80 %** (71.75 – 81.18) | **-18 dB: 77 of 306, 25.16 %** (20.63 – 30.31) | **-17.52 dB** | **-17.61 to -17.43 dB** (0.187 wide) |
| `Deep all off` | -17 dB: 235 of 306, 76.80 % (71.75 – 81.18) | -18 dB: 77 of 306, 25.16 % (20.63 – 30.31) | **-17.52 dB** | **-17.61 to -17.43 dB** (0.187 wide) |
| `fine sync only` | -19 dB: 90.52 % (86.72 – 93.32) | -20 dB: 23.86 % (19.42 – 28.94) | **-19.61 dB** | **-19.67 to -19.55 dB** (0.127 wide) |
| `OSD only` | **-18 dB: 172 of 306, 56.21 %** (50.61 – 61.66) | **-19 dB: 33 of 306, 10.78 %** (7.78 – 14.76) | **-18.14 dB** | **-18.25 to -18.01 dB** (0.234 wide) |
| **`SHIPPING`** | **-19 dB: 90.85 %** (87.09 – 93.59) | **-20 dB: 23.86 %** (19.42 – 28.94) | **-19.61 dB** | **-19.67 to -19.55 dB** (0.126 wide) |
| `subtraction only` | -17 dB: 235 of 306, 76.80 % (71.75 – 81.18) | -18 dB: 77 of 306, 25.16 % (20.63 – 30.31) | **-17.52 dB** | **-17.61 to -17.43 dB** (0.187 wide) |

**Not one cell in either table above is `not bracketed`, and no side of any band at the cell
centre is open.** `docs/unit256-runs/cell-centre-minus17.txt`,
`docs/unit256-runs/cell-centre-minus18.txt`, and — for `OSD only`'s lower rung —
`docs/unit255-runs/minus19-cell-centre.txt`, which is this document's own §3.2 row, measured
at the same placement, the same seed and the same `Ft8Sharp.Deep` 0.8.0.

**Three columns straddle at (-18, -17) and one at (-19, -18), and that is a measurement.**
`OSD only` reads 172 of 306 at -18 dB — already above 50 — so its crossing lies a decibel
deeper than the other three. **No column is interpolated from a 51-trial rung.** Unit 256's
coarse search at 51 trials (`docs/unit256-runs/cell-centre-coarse.txt`) localised the
crossings and is quoted nowhere as a crossing; it stopped at its very first rung, -17 dB, and
**the -9 dB ceiling was never approached.**

#### What the numbers say, now that all twelve are stated

**1. THE BARE PORT NEEDS -17.52 dB, BAND -17.61 TO -17.43, TO HEAR HALF OF WHAT IS SENT AT
THE CENTRE OF A COARSE WATERFALL CELL.** No unit in this project had been able to state that
number. Unit 248 marked it `not bracketed` and this document reproduced the mark until unit
256 measured the two rungs.

**2. The off-grid penalty on the port is 2.02 dB**, -19.54 on the grid against -17.52 at the
cell centre. Both bands are closed, 0.162 dB and 0.187 dB wide, against a 2.02 dB gap — **the
first statement of that penalty as a ratio rather than as a decode count.**

**3. The shipping configuration crosses at -19.90 dB on the grid and -19.61 dB at the cell
centre**, both interpolated between -19 and -20 at 306 trials.

- **-19.90 dB is 0.36 dB better than the port's -19.54**, and 0.09 dB better than ordered
  statistics alone at -19.81. It is the best crossing anywhere in this project on the grid.
  **But its band is open on the optimistic side and `OSD only`'s band, -19.92 to -19.71,
  overlaps it**: at 306 trials this ladder does not separate the shipping stack from ordered
  statistics alone **on the grid**. That is §3.4's finding in the crossing's own terms and
  the band is what makes it sayable.
- **-19.61 dB off the grid is better than the port's own on-grid -19.54**, and the two bands
  — -19.67 to -19.55 against -19.62 to -19.46 — **overlap by 0.07 dB**, so that particular
  comparison is at the edge of what 306 trials support and is stated as such.
- **Off the grid the shipping stack is 2.09 dB better than the bare port at the same
  placement**, -19.61 against -17.52, and **the two bands are 1.94 dB apart at their nearest
  edges.** That comparison is not close and no trial count in this project's reach would
  change it. **It is the phase's headline number and it is now stated with its uncertainty.**
- **Off the grid the shipping crossing and fine sync alone's crossing are the same number to
  two decimals** (-19.610 against -19.608 before rounding), and **ordered statistics alone
  gets only to -18.14 dB there** — 1.47 dB short, with bands nowhere near each other.
  Ordered statistics contributes essentially nothing to the crossing off the grid and fine
  sync contributes nearly all of it.

**4. Subtraction alone crosses exactly where the port does, at both placements** — -19.54 on
the grid and -17.52 at the cell centre, band for band. That is §5.0's *the stopping rule
correctly finding nothing to remove* said in the crossing's own terms for the first time.

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

> **AMENDED IN PLACE BY UNIT 257, 2026-09-05. THE RULING ABOVE STANDS AND IS NARROWED TO WHAT
> IT ACTUALLY ESTABLISHES.**
>
> **Still true, and not reopened: a §5 row cannot sit in §3's table.** §3 gives each trial
> **one** slot and `RunRepeats` gives each trial **four**, so a four-slot row set beside a
> one-slot row is a false comparison whatever else is held equal. Nothing in §3 was re-run,
> amended or reopened by unit 257.
>
> **What did not follow, and unit 256's report drew it anyway: that combining therefore cannot
> be measured at the coordinates §3's table is quoted at.** The rungs **-19, -20 and -21 dB**,
> the two **placements** — on grid and at the cell centre — the **306 trials a cell** and the
> **wrong count on every row** are properties of a *measurement*, not of that table, and every
> one of them is reachable on the ladder that can carry combining. **§5.5 supplies them.**
>
> **What made the placement split honest is dropping the jitter, and that is the only thing
> unit 257 changed.** `Ft8LadderHarness.cs:573-574` makes `frequencyHz` and `offsetSamples`
> the origin of hearing `r = 0`, with every later hearing stepping from it by `r` times the
> jitter; **with both jitters zero every hearing of every trial sits at exactly the stated
> placement**, so *on grid* and *at the cell centre* mean in §5.5 precisely what they mean in
> §3.1 and §3.2. A jittered panel cannot say that about itself, which is why §5.4 measured one
> placement and §5.5 measures two.
>
> **§5.4 STANDS UNCHANGED, IS NOT RE-RUN AND IS NOT CORRECTED, AND BOTH PANELS ARE KEPT.**
> They are not two attempts at one number; they are two different questions. §5.4 is the
> **conservative** reading — a station whose oscillator and clock drift 2.00 Hz and 480 samples
> between hearings — and §5.5 is the **aligned-repeats** reading — a station whose four
> transmissions land in the same place, which is what a stable oscillator does over the two
> minutes four slots take. Unit 247 §4 measured that difference directly at two hearings, 217
> of 306 aligned against 68 of 306 jittered, and it is `HM-OPEN-075`. **Between them they
> bracket what combining is worth to a real station; either alone would overstate or understate
> it.** So the rule is now: **no row of §5.5 is comparable with a row of §3, and no row of §5.5
> is comparable with a row of §5.4 either** — the jitter differs and it is the largest single
> term in the result.

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

### 5.4 Combining on and off, on its own ladder — added by unit 256, 2026-09-05

> **ADDED BY UNIT 256.** Step 6's first exit asks for the port and Deep **with each stage on
> and off**, and §3's table has six columns of which combining is not one. Until this section
> combining appeared in this document only as §5.2's citation from unit 254 and §5.3's single
> cell. **This is combining turned on and off against itself, at three rungs, with its own 50
> per cent crossing.**

**LADDER: `Ft8LadderHarness.RunRepeats`.** Four slots a trial carrying the same message,
**jittered 2.00 Hz and 480 samples between hearings** as a real station's oscillator and clock
would drift. **306 trials a rung. `Ft8Sharp.Deep` 0.8.0.** Every row on this panel sees the
identical audio, so the comparison is paired.

> **THIS LADDER IS NOT §3's AND NO ROW HERE IS COMPARABLE WITH ONE THERE.** §5.0 rules it and
> it is repeated on this table's own face: §3 gives each trial **one** slot and this panel
> gives each trial **four**. A row from here set beside a row from there is a false comparison.

**ONE PLACEMENT, AND THAT IS DELIBERATE.** The first slot starts on grid and every later
hearing is jittered from the one before it, **so the panel is already a mixed-placement
instrument** and a second column labelled *cell centre* would not mean what that phrase means
in §3. It is measured at the same placement as §5.3 so that section's -21 dB row can be cited
rather than re-run.

**THREE ROWS AND NOT FOUR**, and the third is labelled `summed x4` and not `combined x4`
because the accumulation depth is 3 and the label is taken from the depth of the sum rather
than from the repeat count (`Ft8LadderHarness.cs:514`). That distinction is `B17`.

#### The panel

| row | what it is | -21 dB *(cited, §5.3)* | **-22 dB** | **-23 dB** | wrong |
|---|---|---:|---:|---:|---:|
| `single slot` | **combining OFF** — the port on the first slot alone | 13 of 306, 4.2 % (2.5 – 7.1) | **0 of 306**, 0.0 % (0.0 – 1.2) | **0 of 306**, 0.0 % (0.0 – 1.2) | **0** |
| `single + OSD` | **combining OFF** — the sibling with ordered statistics, same first slot | 33 of 306, 10.8 % (7.8 – 14.8) | **1 of 306**, 0.3 % (0.1 – 1.8) | **0 of 306**, 0.0 % (0.0 – 1.2) | **0** |
| **`summed x4`** | **combining ON** — four hearings accumulated three deep, stacked with the shipping stages | **254 of 306, 83.0 %** (78.4 – 86.8) | **43 of 306, 14.1 %** (10.6 – 18.4) | **1 of 306**, 0.3 % (0.1 – 1.8) | **1 at -23 dB** |
| | **`OnlyCombined`** — trials no single slot decoded alone and the combination did | **206 of 306** | **41 of 306** | **1 of 306** | |
| | `LostByCombining` | **0** | **0** | **0** | |
| | combinations submitted / accepted | 2 232 / 736 | **1 335 / 67** | **632 / 1** | |
| | combined decodes / verified | 458 / **458** | **53 / 53** | **1 / 1** | |
| | worst single slot | 109.6 ms, 137× | **76.2 ms, 197×** | **71.7 ms, 209×** | |

**Rungs -22 and -23 are unit 256's** — `docs/unit256-runs/combining-panel-minus22.txt` and
`combining-panel-minus23.txt`. **Rung -21 is §5.3's, cited and not re-run**, because the
tree's `Ft8Sharp.Deep` is still 0.8.0 and the call's arguments were checked against the tree
one by one before it was cited.

#### The crossing, and no unit had ever quoted one

| row | ladder | rungs | crossing | **band** |
|---|---|---|---|---|
| `single slot` | repeats ×4, 306 trials | -21 / -22 | **not bracketed — above -21 dB** | — |
| `single + OSD` | repeats ×4, 306 trials | -21 / -22 | **not bracketed — above -21 dB** | — |
| **`summed x4`** | **repeats ×4, 306 trials** | **-21 / -22** | **-21.48 dB** | **-21.54 to -21.42 dB** (0.119 wide) |

> **COMBINING CROSSES 50 PER CENT AT -21.48 dB, BAND -21.54 TO -21.42, ON THE REPEATS LADDER
> AT FOUR HEARINGS.** Interpolated between 254 of 306 at -21 dB and 43 of 306 at -22 dB, both
> at 306 trials, both on this ladder. **The band is the same construction §4.1 uses and it is
> not a confidence interval on the crossing.**

**The two combining-off rows have no crossing on this ladder and the ceiling is named:
above -21 dB.** They are already below 50 per cent at the panel's highest rung. **The panel
was walked downward from -21 dB because that is where combining's crossing is**, and no rung
above -21 was walked on this ladder — so `not bracketed - above -21 dB` is the licensed
answer and nothing is extrapolated. Their -21 dB rates, **13 and 33 of 306, are exactly
§3.1's on-grid `Ft8Sharp` and `OSD only` figures at the same rung**, which is the consistency
check available without walking more of this ladder, and it holds to the decode.

**The -24 dB rung was licensed and was not spent.** It was conditional on -23 dB still
reading above 50 per cent; -23 dB reads 1 of 306. Unit 247 §1's floor is untouched.

#### THE CAVEAT THAT TRAVELS WITH THE CROSSING, AND IT IS UNIT 254'S OWN

**`RunRepeats` scores the combined column on the union over the trial's slots, so a
four-repeat column gets four single-slot attempts as well as deeper sums.** 13 → 254 at
-21 dB is **not** the gain from accumulation; it conflates more hearings with more chances.
`OnlyCombined` is the honest statement of what combining added, and at the two rungs the
crossing came from it reads **206 of 306** and **41 of 306**.

**So -21.48 dB is what *a station heard four times, with the combiner accumulating and the
shipping stages stacked on it,* crosses at.** It is **not** the gain from accumulation in
isolation, which unit 254 §4a puts at **+4 of 51** at four hearings.

#### One wrong decode, at -23 dB, and it is reported rather than buried

**At -23 dB the `summed x4` row returned one message nobody sent.**

```
trial    29  seed 220771  SENT "CQ PY2ABC GG66"  RETURNED "WN8ESU/P JG5HKE/P R AH58"
```

**It reproduces on the same trial and the same seed on a second run — deterministic, not a
flake.** It is the **first wrong decode measured anywhere in this phase**, against zero in
all thirty-six cells of §3 and zero in every table of §5.1 to §5.3.

**IT DID NOT COME FROM A COMBINATION.** At that rung the port took exactly **one** combination
past both its gates, `CombinedDecodes` is 1 and `CombinedDecodesVerified` is 1 — **the one
combination accepted was the message that was sent.** The wrong return came from the combined
column's **inner** decoder acting on a single slot: the shipping stack, run on all four slots
of every trial and therefore at **four times the exposure** of the `single + OSD` row, which
reads zero wrong at the same rung.

**It changes no figure in this document.** -23 dB is two decibels below the deepest rung in
§3 and 1.5 dB below combining's own crossing. **It is `HM-OPEN-082`**, and the test that
found it, `Ft8Unit256CombiningPanelTests.TheCombiningPanelAtMinus23`, **is left red in the
tree with its assertion unweakened.**

**And nothing here changes what ships.** Combining is off by default, this panel is a
measurement handed to Tim, and §6.2 lists the surfaces that would have to move before any of
it reached a radio.

### 5.5 Combining on and off at the closing table's rungs and both placements, zero jitter — added by unit 257, 2026-09-05

> **ADDED BY UNIT 257.** §5.4 answered *combining on and off* at -21, -22 and -23 dB in **one**
> placement, on a jittered ladder. The session that judged unit 256's report against step 6's
> exits returned `partial` on exactly that: exit 1 asks for each stage on and off **at -19, -20
> and -21 dB, on grid and at cell centre, 306 trials a cell, with wrong counts**, and combining
> had never been measured at -19 or -20 dB anywhere in this project, nor off the analysis grid
> at all. **This is that measurement.**

**LADDER: `Ft8LadderHarness.RunRepeats`.** Four slots a trial carrying the same message.
**JITTER: ZERO on both axes** — `frequencyJitterHz: 0.0`, `offsetJitterSamples: 0` — so by
`Ft8LadderHarness.cs:573-574` **every hearing of every trial sits at exactly the placement
named on its column**. **PLACEMENTS: two.** On grid is `DefaultFrequencyHz` 1000.0 Hz and
`DefaultOffsetSamples`; the cell centre is **+1.56 Hz and +480 samples**, unit 248's two
constants from `Ft8Unit255ClosingLadderTests:62` and `:64` — the same two §3.2 uses, so the
placement is the placement §3.2 means. **306 trials a cell. `Ft8Sharp.Deep` 0.8.0.** Every row
in a cell sees identical audio, so the comparison within a cell is paired.

> **NO ROW HERE IS COMPARABLE WITH A ROW IN §3**, which gives each trial **one** slot where
> this gives each trial **four**, **NOR WITH A ROW IN §5.4**, which is the jittered panel and
> where the jitter is the largest single term in the result. §5.0, as amended, rules both.
> **§5.4 stands unchanged and is not re-run.**

**THREE ROWS AND NOT FOUR**, and the third is `summed x4` rather than `combined x4` because the
accumulation depth is 3 and the label is taken from the depth of the sum (`Ft8LadderHarness.cs:514`).
That distinction is `B17`. `DeepestHearings` is **4 in every cell below**, so the label is
earned everywhere on this panel.

#### The panel, on the grid — 1000.0 Hz, three whole symbol periods in

| row | what it is | **-19 dB** | **-20 dB** | **-21 dB** | wrong |
|---|---|---:|---:|---:|---:|
| `single slot` | **combining OFF** — the port on the first slot alone | 248 of 306, 81.0 % (76.3 – 85.0) | 73 of 306, 23.9 % (19.4 – 28.9) | 13 of 306, 4.2 % (2.5 – 7.1) | **0** |
| `single + OSD` | **combining OFF** — ordered statistics, same first slot | 276 of 306, 90.2 % (86.3 – 93.0) | 125 of 306, 40.8 % (35.5 – 46.4) | 33 of 306, 10.8 % (7.8 – 14.8) | **0** |
| **`summed x4`** | **combining ON** — four hearings accumulated three deep, shipping stages stacked | **306 of 306, 100.0 %** (98.8 – 100.0) | **306 of 306, 100.0 %** (98.8 – 100.0) | **306 of 306, 100.0 %** (98.8 – 100.0) | **0** |
| | **`OnlyCombined`** — trials no single slot decoded alone and the combination did | 1 of 306 | 39 of 306 | **200 of 306** | |
| | `LostByCombining` | **0** | **0** | **0** | |
| | combinations submitted / accepted | 2 856 / 2 415 | 2 517 / 2 027 | 2 110 / 1 510 | |
| | combined decodes / **verified** | 98 / **98** | 491 / **491** | 713 / **713** | |
| | worst single slot | 121.6 ms, **123×** | 93.5 ms, **160×** | 92.3 ms, **162×** | |

#### The panel, at the cell centre — +1.56 Hz, +480 samples

| row | what it is | **-19 dB** | **-20 dB** | **-21 dB** | wrong |
|---|---|---:|---:|---:|---:|
| `single slot` | **combining OFF** — the port on the first slot alone | 6 of 306, 2.0 % (0.9 – 4.2) | 0 of 306, 0.0 % (0.0 – 1.2) | 0 of 306, 0.0 % (0.0 – 1.2) | **0** |
| `single + OSD` | **combining OFF** — ordered statistics, same first slot | 33 of 306, 10.8 % (7.8 – 14.8) | 1 of 306, 0.3 % (0.1 – 1.8) | 0 of 306, 0.0 % (0.0 – 1.2) | **0** |
| **`summed x4`** | **combining ON** — four hearings accumulated three deep, shipping stages stacked | **306 of 306, 100.0 %** (98.8 – 100.0) | **270 of 306, 88.2 %** (84.1 – 91.4) | **75 of 306, 24.5 %** (20.0 – 29.6) | **0** |
| | **`OnlyCombined`** — trials no single slot decoded alone and the combination did | 196 of 306 | **262 of 306** | **75 of 306** | |
| | `LostByCombining` | **0** | **0** | **0** | |
| | combinations submitted / accepted | 6 223 / 4 173 | 4 467 / 1 515 | 2 480 / 140 | |
| | combined decodes / **verified** | 689 / **689** | 483 / **483** | 87 / **87** | |
| | worst single slot | 96.9 ms, **155×** | 125.4 ms, **120×** | 114.4 ms, **131×** | |

**Every figure above is transcribed from a committed artefact under `docs/unit257-runs/`** —
`placement-panel-on-grid-minus19.txt`, `-minus20.txt`, `-minus21.txt`,
`placement-panel-cell-centre-minus19.txt`, `-minus20.txt`, `-minus21.txt` — **and none from a
console buffer.**

#### The consistency check, and it is exact in all twelve figures

The two combining-OFF rows see only slot 0, which at zero jitter is exactly the audio §3
walked. **They reproduce §3.1 and §3.2 to the decode in all six cells:**

| rung-placement | §5.5 `single slot` | §3's `Ft8Sharp` | §5.5 `single + OSD` | §3's `OSD only` |
|---|---:|---:|---:|---:|
| -19 on grid | **248** | 248 | **276** | 276 |
| -20 on grid | **73** | 73 | **125** | 125 |
| -21 on grid | **13** | 13 | **33** | 33 |
| -19 cell centre | **6** | 6 | **33** | 33 |
| -20 cell centre | **0** | 0 | **1** | 1 |
| -21 cell centre | **0** | 0 | **0** | 0 |

**Twelve figures, twelve exact matches.** That is what establishes that §5.5's placements are
§3's placements — the claim the whole section rests on — and it is measured rather than
asserted. At -21 dB on grid the same pair also matches §5.3's cited row and
`docs/unit247-combining.md` §4: **four independent records agreeing on 13 and 33 of 306.**

#### The crossings

| row | placement | ladder | rungs | crossing | **band** | width |
|---|---|---|---|---|---|---:|
| `single slot` | on grid | repeats ×4, zero jitter, 306 trials | -19 / -20 | **-19.54 dB** | **-19.62 to -19.46 dB** | 0.162 |
| `single + OSD` | on grid | repeats ×4, zero jitter, 306 trials | -19 / -20 | **-19.81 dB** | **-19.92 to -19.71 dB** | 0.209 |
| **`summed x4`** | **on grid** | repeats ×4, zero jitter, 306 trials | **-22 / -23** | **-22.41 dB** | **-22.47 to -22.35 dB** | 0.120 |
| `single slot` | cell centre | repeats ×4, zero jitter, 306 trials | — | **not bracketed — above -19 dB** | — | — |
| `single + OSD` | cell centre | repeats ×4, zero jitter, 306 trials | — | **not bracketed — above -19 dB** | — | — |
| **`summed x4`** | **cell centre** | repeats ×4, zero jitter, 306 trials | **-20 / -21** | **-20.60 dB** | **-20.67 to -20.53 dB** | 0.138 |

**The band is not a confidence interval on the crossing.** It is the two rungs' own 95 per cent
Wilson bounds pushed through the same linear interpolation the point crossing uses, under the
stated assumption that the decode rate moves linearly in decibels between two rungs one decibel
apart — the construction §4.1 uses and the arithmetic `Ft8Unit256CrossingBand` executes, which
is gate-set entry 13 and was watched failing once as `B18`. **Every band above contains its own
point crossing**, checked. Artefact: `docs/unit257-runs/crossing-bands.txt`.

**The two unbracketed columns are unbracketed *upward*, and the ceiling is named.** At the cell
centre `single slot` reads 6 of 306 and `single + OSD` 33 of 306 at **-19 dB, the shallowest
rung this panel walked** — already below 50 per cent at the top of the bracket. **A downward
extension rung cannot reach them and walking up the ladder was not licensed, so -19 dB is the
stated ceiling and nothing is extrapolated.** Unit 255's ruling 3.

**`summed x4` on the grid needed two rungs below the table's**, because it reads 306 of 306 at
-19, -20 **and** -21 dB. -22.0 dB (239 of 306) and -23.0 dB (29 of 306) bracket it. **The search
was capped at -23 dB and did not need the cap.**

#### What this panel says

1. **On the grid, at every ratio the closing table is quoted at, a station heard four times is
   decoded every time.** 306 of 306 at -19, -20 and -21 dB. **That is saturation and it is a
   result rather than a fault** — no bound is asserted on any rate, and it was not chased down
   the ladder inside the measurement. Combining's on-grid crossing is **1.4 dB below the
   deepest rung the closing table quotes**, at -22.41 dB.
2. **Off the grid, the two combining-off rows collapse and the combined row does not.** At -20
   dB the port reads **0 of 306** and combining reads **270 of 306**. At -21 dB the port and
   ordered statistics **both read 0 of 306**, `AnySlotAlone` is **0**, and combining reads
   **75 of 306** — **every one of them a trial nothing else could reach.**
3. **The cost of landing off the grid, for combining, is 1.81 dB**: -22.41 dB on grid against
   **-20.60 dB** at the cell centre, and the two bands do not come close to touching.
4. **`LostByCombining` is 0 in all six cells.** Combining is a strict superset of the single
   slot on this audio at every rung and both placements; it never took a decode away.
5. **Zero wrong in all eighteen rows, and 2 561 combined decodes with 2 561 verified**, against
   naive false-accept expectations of 0.129 to 0.380 across the six cells. **No wrong decode
   appeared at any rung or either placement inside this panel.**
6. **The worst single slot anywhere in the panel is 125.4 ms — a 120× margin** against FT8's
   15 000 ms, at -20 dB at the cell centre.

#### Beside §5.4, and the two are a pair of bounds rather than a disagreement

| | **§5.4 — jittered** | **§5.5 — aligned** |
|---|---|---|
| the station it models | oscillator and clock drift 2.00 Hz and 480 samples between hearings | four transmissions land in the same place |
| `summed x4` at -21 dB, on grid | **254 of 306, 83.0 %** | **306 of 306, 100.0 %** |
| combining's 50 % crossing, on grid | **-21.48 dB** (band -21.54 to -21.42) | **-22.41 dB** (band -22.47 to -22.35) |
| placements measured | one, and mixed by construction | **two, and each exact** |

**0.93 dB separates the two crossings, and that gap is the cost of drift** — the same term unit
247 §4 measured at two hearings as 217 of 306 aligned against 68 of 306 jittered. It is
`HM-OPEN-075` and it is not this section's to close. **Neither panel is the answer on its own:
§5.4 understates what a stable station gets and §5.5 overstates what a drifting one gets, and a
real station is somewhere between them.**

#### The wrong decode at -23 dB reproduces at zero jitter, and it is the same one

**The downward extension rung at -23 dB on the grid returned one message nobody sent**, in the
`summed x4` row:

```
trial    29  seed 220771  SENT "CQ PY2ABC GG66"  RETURNED "WN8ESU/P JG5HKE/P R AH58"
```

**That is byte for byte the same trial, the same seed, the same message sent and the same
message returned as §5.4's**, which unit 256 found at -23 dB on the **jittered** panel. **So it
reproduces across a change of placement configuration as well as across runs**, which narrows
it: it is not a property of the jitter and not a property of the pairing geometry. As at §5.4,
**it did not come from a combination** — `CombinedDecodes` is 32 and `CombinedDecodesVerified`
is 32 at that rung, so every message the combining stage added was the message that was sent.
It came from the combined column's **inner** decoder acting on a single slot: the shipping
stack, run on all four slots of every trial, at four times the exposure of the `single + OSD`
row, which reads zero wrong at the same rung.

**It changes no figure in this document** — -23 dB is two decibels below the deepest rung in §3
and 1.4 dB below combining's own on-grid crossing. **It is `HM-OPEN-082`**, it is a second dated
observation against that issue and not a new one, and the test that found it,
`Ft8Unit257PlacementPanelTests.TheDownwardExtensionOnGridAtMinus23`, **is left red in the tree
with its assertion unweakened**, as `Ft8Unit256CombiningPanelTests.TheCombiningPanelAtMinus23`
is.

**And nothing here changes what ships.** Combining is off by default, this panel is a
measurement handed to Tim, and §6.2 lists the surfaces that would have to move before any of it
reached a radio.

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

> **AMENDED BY UNIT 256: both crossings now carry a band, and one of them is open.**
>
> | | crossing | **band** |
> |---|---|---|
> | on the grid | **-19.90 dB** | **open beyond -20 dB, to -19.79 dB** |
> | at the cell centre | **-19.61 dB** | **-19.67 to -19.55 dB** |
>
> **The band is not a confidence interval on the crossing.** It is the two rungs' own 95 per
> cent Wilson bounds pushed through the same interpolation, under the assumption the rate
> moves linearly in decibels between them — §4.1 says it in full. **The open side is open
> because the -20 dB rung's upper bound is 50.700 per cent, still above 50**, so this ladder
> cannot put a floor under how good the on-grid crossing might be.
>
> **AND THE COMPARISON IN THIS ITEM CHANGES.** *The off-grid crossing is better than the bare
> port's own on-grid -19.54 dB* is still true of the point values, **but the two bands overlap
> by 0.07 dB** — -19.67 to -19.55 against -19.62 to -19.46 — **so that particular sentence is
> at the edge of what 306 trials support and must not be read as a clean win.**
>
> **The comparison that is not close, and it is the better one to quote:** off the grid, the
> shipping stack crosses at **-19.61 dB** against the bare port's **-17.52 dB** at the same
> placement — **2.09 dB**, with **1.94 dB between the nearest edges of the two bands.**
> The port's own cell-centre crossing was `not bracketed` until unit 256 measured it.

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

> **QUALIFIED BY UNIT 256, AND THIS IS THE ONE CLAIM IN §6.1 THAT DOES NOT SURVIVE
> UNCHANGED.** Every figure above still reads zero wrong, and unit 256 added **2 448 more
> scored slot decodes at the cell centre at -17 and -18 dB with zero wrong** and **918 at
> -22 dB on the repeats ladder with zero wrong.** But at **-23 dB on the repeats ladder** —
> two decibels below the deepest rung anywhere in this document — **the `summed x4` row
> returned one message nobody sent**, reproducibly (§5.4). **The sentence "every column
> measured in this project reads zero wrong" is no longer true without a rung qualifier**, and
> it is `HM-OPEN-082`.
>
> **What it does not do.** It does not touch a figure in this document. It did not come from a
> combination — the one combination the port accepted at that rung was correct — and both of
> the port's gates were in the path when it happened, which is the point: **parity and CRC-14
> are a filter and not a proof**, and at 632 submissions the naive expectation of a message
> nobody sent was 0.039. **Nothing in `Ft8Sharp.Deep` decided that message was real; the
> port's own gates let it through.**

---

**§6.1 RE-READ AGAINST UNIT 256'S NUMBERS, ITEM BY ITEM, 2026-09-05.** Item 4's two crossings
gained bands and one of its comparisons was qualified, above. Item 7 was qualified, above.
**Items 1, 2, 3, 5 and 6 were each checked against tonight's measurements and none of them
moved**: item 1's `Ft8Reception.cs:460` was not touched; items 2 and 3 quote §3's
thirty-six cells, which were not re-run; item 5's 336.8 ms and 44.5× are `SHIPPING`'s and
`SHIPPING` was not re-run — **the worst slot anywhere in unit 256's own walks was 154.4 ms**,
a 97× margin, and nothing tonight came near the budget; item 6's SNR figures are step 2's and
were not touched. **The checking was done and this is the record of it.**

---

**8. ADDED BY UNIT 257 — what hearing the same station four times is worth, at the ratios this
table is quoted at and at the place a real station lands.** **This is a claim about a stage
that is OFF by default; read item 8 with §6.2 in front of you.** §5.5, 306 trials a cell, zero
jitter, four hearings a trial, `Ft8Sharp.Deep` 0.8.0. **Not comparable with items 2 and 3
above, which are one-slot rows** — this is four slots a trial and the two ladders may not be
read side by side.

| ratio | placement | **combining OFF** (`single slot`) | **combining ON** (`summed x4`) | trials no single slot reached |
|---|---|---|---|---|
| -19 dB | on grid | 248 of 306 — 81.0 % | **306 of 306 — 100.0 %** | 1 of 306 |
| -20 dB | on grid | 73 of 306 — 23.9 % | **306 of 306 — 100.0 %** | 39 of 306 |
| -21 dB | on grid | 13 of 306 — 4.2 % | **306 of 306 — 100.0 %** | **200 of 306** |
| -19 dB | cell centre | 6 of 306 — 2.0 % | **306 of 306 — 100.0 %** | 196 of 306 |
| -20 dB | cell centre | 0 of 306 — 0.0 % | **270 of 306 — 88.2 %** | **262 of 306** |
| -21 dB | cell centre | 0 of 306 — 0.0 % | **75 of 306 — 24.5 %** | **75 of 306** |

**In plain words: at -21 dB, half a bin off Hamlet's grid, a station heard once is a station
Hamlet never hears — 0 of 306, and ordered statistics does not help it, also 0 of 306. Heard
four times in the same place, it is decoded 75 times in 306, and every one of those 75 is a
trial nothing else could reach.** **Zero wrong across all eighteen rows**, and **`LostByCombining`
is 0 in all six cells** — combining never took a decode away. Worst slot in the panel
**125.4 ms, a 120× margin.**

**Combining's own 50 per cent crossing, which no unit could state at either placement before:**
**-22.41 dB on the grid** (band -22.47 to -22.35) and **-20.60 dB at the cell centre** (band
-20.67 to -20.53). **The cost of landing off the grid is 1.81 dB.** §5.4's jittered panel puts
the on-grid crossing at **-21.48 dB** instead, and **the 0.93 dB between them is the cost of a
station whose oscillator drifts between overs** — the two panels bracket a real station rather
than disagreeing.

**THE CAVEAT THAT TRAVELS WITH EVERY FIGURE IN THIS ITEM, AND IT IS UNIT 254'S OWN.**
`RunRepeats` scores the combined column on the union over the trial's slots, **so a four-repeat
column gets four single-slot attempts as well as deeper sums.** 13 → 306 at -21 dB on grid is
**not** the gain from accumulation; it conflates more hearings with more chances. **The
`trials no single slot reached` column is the honest statement of what combining added**, and
it is printed above for every cell.

---

**§6.1 RE-READ AGAINST UNIT 257'S NUMBERS, ITEM BY ITEM, 2026-09-05.**

- **Item 1 — did not move.** `Ft8Reception.cs:460` was not touched. Nothing under `src/` moved
  tonight and no production line changed.
- **Item 2 — did not move.** It quotes §3's on-grid one-slot cells, which were not re-run. **A
  §5.5 row may not be put in its table**, §5.0 as amended. What §5.5 does add is a second
  reading of item 2's *the port at -19, -20 and -21 dB on grid* — 248, 73 and 13 of 306 — which
  §5.5's `single slot` row reproduces **to the decode**, so item 2's left-hand column is now
  confirmed by an independently walked ladder.
- **Item 3 — did not move, and it gained its off-grid counterpart for a stage it did not
  cover.** Its own figures are §3.2's and were not re-run; §5.5 reproduces its bare-port
  cell-centre column — 6, 0 and 0 of 306 — to the decode. **Item 3's claim is about fine sync
  and ordered statistics; item 8 is the same question asked of combining**, and it is a
  different ladder and a separate item for that reason.
- **Item 4 — did not move, and was not reopened.** Its two crossings are `SHIPPING`'s on §3's
  one-slot ladder. **Combining's crossings are new numbers on a different ladder and are stated
  in item 8, not folded into item 4.**
- **Item 5 — did not move, and tonight's worst slot is quoted beside it.** 336.8 ms and 44.5×
  are `SHIPPING`'s on §3's ladder and `SHIPPING` was not re-run. **The worst single slot
  anywhere in unit 257's eight walks was 129.3 ms — a 116× margin** — at -22 dB on grid.
  **Nothing tonight came within two orders of the budget**, and the four-slot panel's worst slot
  is still a third of §3's worst.
- **Item 6 — did not move.** Step 2's SNR figures were not touched.
- **Item 7 — QUALIFIED FURTHER, AND THE QUALIFIER IS NARROWED.** Unit 257 added **5 508 more
  scored slot decodes** — six cells of 918 at the closing table's rungs and both placements —
  **with zero wrong in every one of the eighteen rows**, and **918 more at -22 dB on grid with
  zero wrong**. At **-23 dB on grid at zero jitter** the `summed x4` row returned **the same
  wrong message as §5.4's, on the same trial and the same seed**. **So the rung qualifier unit
  256 added stands and is now better founded: the wrong decode is a property of the rung and of
  the shipping stack's exposure, and not of the jitter or the placement** — it survived a change
  of both. It remains `HM-OPEN-082`, it remains outside every rung this document quotes, and it
  came from the port's own gates rather than from anything in `Ft8Sharp.Deep`.
- **Item 8 — new tonight**, above.

**The checking was done, item by item, and this is the record of it.**

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

> **AMENDED BY UNIT 256, THE SECOND UNIT SPENT ON STEP 6.** It carried the two shortfalls the
> session that judged this document named: **every crossing now carries a band** (§4.1, twelve
> of them, and the four cell-centre rows that read `not bracketed` are bracketed at 306
> trials), and **combining has an on-and-off panel of its own with its own crossing at
> -21.48 dB** (§5.4). The one figure it produced that nobody could state before: **the bare
> port needs -17.52 dB, band -17.61 to -17.43, to hear half of what is sent at the centre of a
> coarse waterfall cell.** Its own working record is
> `docs/unit256-crossings-and-combining.md`.
>
> **This is still not a declaration that the phase is closed.** That reading is the next
> arbiter's.

