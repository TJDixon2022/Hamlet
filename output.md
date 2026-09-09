READ IN THIS ORDER

A. The phase goal is **FT4 works exactly the way FT8 does.** Step 0 done. Step 1
   partial, its remainder the 4.48-against-5.04 transmission figure, which is with
   Tim and which no unit may settle. Step 2 partial before tonight; **this unit
   discharged the whole of the remainder it could reach and named what it could
   not** - the five on-screen sentences booked here are now three fixed and two
   left, the two left being one that is unreachable today and one that belongs to
   the transmit half; the transmit guard and the two training-path copies are
   untouched by design. Step 3 done. **Step 4 is now partial**: criteria 1 and 2
   met, criteria 3 and 4 booked to unit 293 by the arbiter before this unit ran.
   Steps 5 and 6 not started, and both are Tim's at his radio.

B. Step 4 and its four exit criteria.
   1. pressing FT4 tunes to the band's FT4 frequency and decodes, through the same
      path the FT8 button uses                            must-pass - **MET**
   2. the panel, conversation, ring, filters, tooltips, ledger and right-click menu
      all work unchanged, and the report names anything that did not
                                                          must-pass - **MET**
   3. one click, one transmission, through the same abort  must-pass - **BOOKED TO
      UNIT 293** by the arbiter before this unit ran, not a failure of this one
   4. a whole exchange from one right click at the bench, transmit endpoint on a
      loopback                                             must-pass - **BOOKED TO
      UNIT 293**, same booking, same reason

C. This report raises 11 items in section 4 and **none of them is in the way of
   criterion 1 or criterion 2**; four are with Tim, five are the launcher's or the
   sandbox's, and two need a citation. Task 4 named **13 gaps**. One of them -
   the log's write path still handing in `ContactModes.Named("FT8")` - is in the
   way of unit 293 and must land with it; three more are unit 293's own; the other
   nine are beside both criteria. **Task 6 was not dropped.** It was taken whole,
   both ledger surfaces together, and the two ledger gaps unit 291 named are now
   closed rather than standing.

UNIT:       292 - complete at task 6 of 6, nothing dropped - 2026-09-09 11:12
PHASE GOAL: FT4 works exactly the way FT8 does.
UNIT GOAL:  Pressing FT4 on the Digital tab tunes to the band's FT4 frequency and
            decodes FT4 off the air on a 7.5 s grid, through the same path the FT8
            button uses, and everything around it either still works or is named.
ADVANCED:   yes - step 4 criteria 1 and 2, and step 2's booked remainder as far as
            this unit's threading reaches it.
NUMBER:     17 places between the press and a decoded row were FT8 by construction
            at HEAD 9449d02 - 6 by constant, 6 by type, 5 by default. Task 4 named
            13 gaps.
DRIFT:      0 consecutive units without advance

---

## 1. What Claude did

Six tasks of six. **Nothing was dropped, and the named drop candidate was taken
whole.** Every task was committed and pushed before the next began.

### The shape of it

| Task | What it was | Commit |
|---|---|---|
| 1 | The trace, and one test measuring the starting position | `3668643` |
| 2 | The grid follows the chip he pressed | `f8b4869` |
| 3 | The decode runs FT4's decoder, and the sheet says so | `de9f250` |
| 4 | Criterion 2's seven surfaces, each one measured | `a5cf9b4` |
| 5 | Bookkeeping | `4369aac` |
| 6 | The ledger shows the submode | `5422b6d` |

Root version `1.12.229` → `1.12.234`, five patches. **`Ft8Sharp` did not move and is
still `0.11.0`** - unit 289 built the FT4 decoder and this unit calls it. Nothing in
`src/Ft8Sharp/` or `src/Ft8Sharp.Deep/` was touched.

### What was built

**One fact where there were going to be two.** `DigitalMode` (`Ft8` or `Ft4`) is a
new type in `Hamlet.RadioEngine/Audio/DigitalMode.cs`. The grid, the watch, the
cutter, the sidecar, the arrival window and the decoder are all derived from it.
Nothing carries a grid and a decoder as separate arguments, because two arguments
that must agree is exactly how a reader comes to cut 7.5-second slots and hand them
to FT8's decoder.

**The route unit 290 cut and left unpressed now exists.** `_digitalGrid` is gone;
`DigitalGrid` is derived from `_digitalMode`, which follows `ChosenDigitalMode` -
both on a press and at start-up, because the field is assigned rather than set in the
constructor and an operator who left the app on FT4 would otherwise have opened it on
FT4's chip and FT8's grid.

**`Ft8Reader.Read` gained one `DigitalMode` parameter, defaulting to FT8**, and
branches to a new `ReadFt4` which builds `Ft4SlotDecoder`, `Ft4SyncSearch` and
`Ft4WaterfallGeometry`. A branch beside FT8's loop rather than a protocol flag inside
it: that loop has been byte-identical since unit 249.

**Four sentences stopped saying fifteen where the tab is not cutting fifteen** - the
census line, the sidecar's arrival line, `Ft8SlotWatch.AudioShort`, and the arrival
window itself, which was `Ft8Slots.SlotSeconds` flat.

**Both ledger surfaces carry the submode**, and the FT8 half is asserted as hard as
the FT4 half.

### What was not done, and why

**Nothing in this unit transmits, arms a transmission, or touches a keying path.**
`Ft8TransmitSequence.cs:497-530` was read, is named below, and is unchanged.
`TheWholeChainRunsFromOneRightClickTests` and
`TheLoopbackThroughTheApplicationsSendPathTests` were not run and not extended.

**None of the four questions with Tim was settled.** The grid still reads both FT4
timing numbers from `Ft8Sharp.Ft4Timing`, so a ruling on 4.48 against 5.04 still
costs one edit. No sentence on screen states a transmission length, on either grid,
and that is asserted.

**No test suite was run.** Every test was filtered by exact name, foregrounded, with
a stated timeout. Nine `dotnet build` invocations, foregrounded.

---

## 2. What the owner should expect

**Press FT4 on the Digital tab and the radio goes to the band's FT4 frequency, the
tab starts cutting 7.5-second slots, and FT4 decodes appear on the table.** That is
the first time in this phase that the button has done anything at all.

The five bands with a cited FT4 row are **80 m (3.575), 40 m (7.047), 20 m (14.080),
15 m (21.140) and 10 m (28.180)**, all from `data/bands/us-neighborhoods.json`. On
30 m and 17 m there is no FT4 row and the dial does not move: the screen says *There
is no FT4 on 30 m, so the dial has not moved. 80 m, 40 m, 20 m, 15 m and 10 m have
one.* That is the correct behaviour and not a fault - §0.2.1 forbids writing a
frequency from memory. **The tab still runs FT4 there**, because he pressed FT4.

**Step 5's first criterion is *Tim hears FT4*, and after tonight that is reachable
at the bench.** Receiving needed the grid and the decoder and it now has both.
Whether real off-air FT4 decodes is a measurement only the radio can take, and this
unit could not take it - what is proved here is Hamlet's own FT4 signal through
Hamlet's own application path, four transmissions in and four rows out.

**Step 6 - working a station on FT4 - is still waiting on unit 293.** Hamlet cannot
transmit FT4 at all. The achievements card's FT4 row still reads *waiting on Hamlet*
and this unit did not light it.

### What unit 293 inherits, including two things the arbiter did not know

The arbiter split step 4 knowing the transmit half had no FT4 in it. Two findings
sharpen that:

1. **The log's write path is worse than a transmit gap - it is a permanent-record
   gap, and it becomes live the moment unit 293 lands.**
   `MainWindowViewModel.cs:10299` still hands in `ContactModes.Named("FT8")`
   unconditionally. `LogContactAsync` is reachable from the right-click menu with no
   transmission at all - `CanLogRow` only asks whether the message is addressed to
   the operator. **It is unreachable tonight** only because nothing can address him
   on FT4 until Hamlet can transmit on it. Unit 293 must change that line in the same
   unit it opens the transmit path, or the first FT4 contact goes into the log as
   FT8, in the file `PHASE_PLAN.md` says outlives everything else. The mode wiring it
   needs now exists and it is one line.

2. **The right-click menu will not offer a signal report on FT4**, because
   `MeasuredReport` (`MainWindowViewModel.cs:10469`) parses `row.Snr`, and every FT4
   row's ratio is *not measured* - see the SNR gap below. So an FT4 exchange composed
   from the menu is missing the report leg unless unit 293 either measures FT4 ratios
   or takes the report from somewhere else. That is a decode-side consequence the
   transmit split did not anticipate.

Beyond those, unit 293 inherits what was expected: no `Ft4Composer`; the guard at
`Ft8TransmitSequence.cs:497-530` measuring against `Ft8Slots` literals; the arm
computing FT8's next boundary at `MainWindowViewModel.cs:10553-10554`; and the abort
to be re-proved through whatever it builds.

---

## 3. What you should see

### 3.1 The trace - which fact drives the mode, and where FT8 was welded in

**`IsChosen` drives the grid and the decoder. `IsLit` may not.**

`DigitalModeChip.cs:70-78` keeps them apart on purpose: `IsLit` is *the dial is
inside this mode's block, and the map is what answers*; `IsChosen` is *this is the
chip he pressed*. Which mode Hamlet is **trying to decode** is the operator's
instruction, not a reading of anything, so it can only come from `IsChosen`. Driving
it from `IsLit` would change the decoder under him when he turned the dial and would
let the map override a button he pressed - and `IsLit` is a lookup in
`us-neighborhoods.json`, which describes what other people do at a frequency, not
what Hamlet should read.

**In the `IsChosenElsewhere` case the tab runs the mode he chose, and adds no second
voice.** He pressed FT4 and the dial is in the FT8 block or nowhere the map knows:
the tab cuts 7.5 s and reads FT4. Nothing about the band is asserted by that - the
grid is a fact about what Hamlet is cutting. The disagreement is **already drawn
twice**: the chip has its own appearance for it (`DigitalModeChip.cs:40`) and
`DigitalTuneLine` says either that the tune did not take or which bands have a row.
Adding a third statement would be the noise §0.0 warns about, not more honesty.

`ABandWithNoFt4RowMovesNothingAndTheTabStillRunsFt4` asserts exactly this case: on
30 m the dial does not move, `DigitalTuneFailed` is true, and `DigitalGrid` is FT4's.

**The press path**, with file and line:

`ChooseDigitalModeAsync` (`MainWindowViewModel.cs:1039`) → `DigitalModeChip.Canonical`
(`:1041`, which drops a fifth label) → `ChosenDigitalMode = picked` (`:1052`,
recorded whether or not the tune takes) → **new this unit:**
`OnChosenDigitalModeChanged` → `FollowTheChosenMode(DigitalModeFor(value))` →
`TuneToDigitalModeAsync` (`:1059`) → `DigitalCallingFrequencies.Find(bandName, picked)`
(`:1062`) → set over CI-V, **read back over CI-V 03**, display moves on the read-back
and never on the command (`:1107-1128`).

Bands with an FT4 row, counted in the tree rather than assumed - five `FT4 sprint`
rows at `data/bands/us-neighborhoods.json:155, 291, 578, 865, 1004`: **80 m, 40 m,
20 m, 15 m, 10 m**. The tree carries seven bands; **30 m and 17 m have no FT4 row**,
which confirms the comment at `MainWindowViewModel.cs:1067`.

**The decode path**, with file and line:

`OnSlotTick` (`:8928`, returns early off the tab and re-arms the watch) →
`_slotWatch.Look(tap, DateTime.UtcNow, offset)` (`Ft8SlotWatch.cs:249`) →
`DecodeTheSlotAsync` (`:8979`) → `Ft8Reader.Read` (`Ft8Reception.cs:421`) →
`Ft8SlotCutter.Cut` (`Ft8SlotCutter.cs:122`) → `Ft8Resample.ToFt8Rate` → the decoder →
`NoteSlot` → `AddDecodeRow`. The press-capture path is `ShowDecodes` (`:8896`), which
reaches the same reader.

**The 17 places that were FT8 by construction at HEAD `9449d02`**, each marked
constant, type or default:

| # | Place | Which |
|---|---|---|
| 1 | `MainWindowViewModel.cs:190` `_slotWatch = new()`; `Ft8SlotWatch.Grid` is `init` defaulting to `SlotGrid.Ft8` | default |
| 2 | `MainWindowViewModel.cs:1661` `_digitalGrid = SlotGrid.Ft8`, `UseGridForTests` its only writer | constant |
| 3 | `Ft8Reader.Read`'s signature had no way to say FT4 at all | type |
| 4 | `Ft8Reception.cs:430` `Ft8SlotCutter.Cut(...)` with the grid argument omitted | default |
| 5 | `Ft8Reception.cs:460` `decoder ??= new Ft8DeepSlotDecoder(...)` | default |
| 6 | `Ft8Reception.cs:425` the parameter's type is `Ft8DeepSlotDecoder?`, so no FT4 decoder could be passed | type |
| 7 | `Ft8Reception.cs:468` `new Ft8SyncSearch(...)` - FT8's Costas search | type |
| 8 | `Ft8Reception.cs:469` `new Ft8Monitor(decoder.Geometry)`, an `Ft8WaterfallGeometry` | type |
| 9 | `Ft8Reception.cs:475` the identity literal `"Ft8Sharp.Deep"` | constant |
| 10 | `Ft8Reception.cs:540` `Measure(...)`, which packs through `Ft8DeepMessageSymbols` | type |
| 11 | `Ft8Reception.cs:483` `new Ft8SlotDecoder()` for the port comparison | type |
| 12 | `MainWindowViewModel.cs:2845` `MeasureArrival`'s `TimeSpan.FromSeconds(Ft8Slots.SlotSeconds)` | constant |
| 13 | `Ft8SlotWatch.cs:159` `AudioShort` naming *fifteen seconds* | constant |
| 14 | `Ft8Reception.cs:398` `NoWholeSlot` naming *a whole fifteen-second slot* | constant |
| 15 | `MainWindowViewModel.cs:2806` `ArrivalSuffix` naming *the last fifteen seconds* | constant |
| 16 | `MainWindowViewModel.cs:8886` `DigitalCaptureSheet.Compose(...)` with the grid omitted | default |
| 17 | `MainWindowViewModel.cs:1923` `Ft8Turn.Read(...)` with the grid omitted | default |

**6 constant, 6 type, 5 default.** The boundary drawn is *the press to a row, and the
surfaces that row arrives with* - the census line, the strip sentence, the waterfall
caption and the sidecar, because a row that arrives under a caption naming the wrong
grid is a row the operator cannot check.

**Places 1 to 8, 12, 15, 16 and 17 now follow the mode.** 9, 10, 11, 13 and 14 are
covered below.

**What is left of unit 290's fifteen-second census.** Measured again rather than
inherited as a list. Five sentences reach the operator; the field-guide row was
filled by unit 291.

| Sentence | Where | State |
|---|---|---|
| `HmDecodeUtcHelp` - *FT8 runs on a strict fifteen-second grid* | `App.axaml:49` | **left standing.** Reached by this unit's threading? **No** - it is a static markup string on the decode table's UTC column, which now carries FT4 rows. Gap 8. |
| The census line's arrival sentence | `MainWindowViewModel.cs:2806` | **fixed.** Names the grid's own length. |
| `Ft8SlotWatch.AudioShort` | `Ft8SlotWatch.cs:158` | **fixed.** Gained a second format argument; its one test moved with it. |
| The sidecar's arrival line | `DigitalCaptureSheet.cs:332` | **fixed.** Names the grid the capture was cut on. |
| `Ft8Reader.NoWholeSlot` | `Ft8Reception.cs:398` | **left standing, and unreachable.** Every zero-slot cut carries its own reason, so the fallback never renders. Gap 11. |

Still standing beside those, all deliberately: **the transmit guard**
`Ft8TransmitSequence.cs:497-530`, which tests `send.StartSecondsIntoSlot >=
Ft8Slots.SlotSeconds` and computes `left = Ft8Slots.SlotSeconds -
send.StartSecondsIntoSlot` - read, named, **changed in no way**, and unit 293's; and
**the two training-path copies**, `Training/ModeAudio.cs:53` and
`Training/SignalSynthesizer.cs:20`, both `TimeSpan.FromSeconds(15)` and both correct,
because the training radio synthesizes FT8.

**What a slot decoded as FT4 must say about which decoder read it.** `Ft8Sharp` and
never `Ft8Sharp.Deep`. `Ft8DecoderIdentity` (`Ft8Reception.cs:292-305`) carries `Port`
= `"Ft8Sharp"` at `:301`; **`Ft8Sharp.Deep` has no FT4 decoder of any kind**, so a
sheet naming it would be naming a decoder that does not exist. Asserted, below.

### 3.2 An FT4 slot decoded through the application

Driven through `MainWindowViewModel.ShowDecodes`, **not** through `Ft4SlotDecoder`.
A test calling the decoder directly would prove unit 289's work again and this unit's
not at all.

The recording: thirty seconds at 12 kHz, four FT4 transmissions at four places in the
passband, each `Ft4Waveform.SynthesizeSlot` from `Ft4SymbolEncoder.Encode` of a packed
standard message.

```
  grid     : 7.50 s slots, 5.04 s transmission
  slots    : 4
  decoder  : Ft8Sharp
  row      : 11:59:30.0   1000.0 Hz  score  35  snr     -  CQ K1ABC FN42
  row      : 11:59:37.5   1250.0 Hz  score  38  snr     -  K1ABC W9XYZ EM12
  row      : 11:59:45.0   1500.0 Hz  score  35  snr     -  W9XYZ K1ABC -11
  row      : 11:59:52.5   1750.0 Hz  score  39  snr     -  K1ABC W9XYZ RR73
  read 4 of 4, 0 missed, 0 wrong
```

- **Text in equals text out**, message for message.
- **Cut on 7.5 s.** The four boundaries are `:30.0`, `:37.5`, `:45.0`, `:52.5` -
  including the two half-second ones no whole-second return type could express.
- **The sheet names `Ft8Sharp`**, asserted as `Ft8DecoderIdentity.Port` on every slot.
- **0 wrong decodes, 4 of 4 read, 0 missed.** Both numbers, as the standing ruling
  requires, and the wrong count stated even though it is zero.

**The same recording at HEAD `9449d02`, with FT4 chosen**, which is what task 1
measured before anything was threaded:

```
  grid     : 15.00 s slots, 12.64 s transmission
  slots    : 2
  decoder  : Ft8Sharp.Deep
  messages : 0
```

**`compareWithThePort` turned on, in FT4.** `WithTheComparisonOnAnFt4SlotRecordsNo
ComparisonRatherThanAnInventedOne`: four slots, four decodes, **zero comparisons
recorded**. The flag is read and answered rather than silently dropped - the
comparison decodes each slot through the faithful port beside Deep, and on FT4 **the
port is what ran**. Running the same decoder twice and printing the agreement would
be a measurement of nothing wearing evidence's clothes. `PortComparison` is null,
which is this tree's word for *nobody took one*.

**No signal-to-noise ratio, and none invented.** Every FT4 row's `SignalToNoiseDb` is
null and every slot's `SignalToNoise.Measured` is 0, asserted. The estimator packs
the decoded text back to FT8's 79 symbols through `Ft8DeepMessageSymbols`; there is no
FT4 equivalent in this tree. A plausible number in a column headed `snr` derived from
measuring FT4 tones against FT8's symbol layout is exactly the fault §0.0 names. It
is gap 2 and it has a consequence - gap 4.

### 3.3 Criterion 2 - the seven, one row each, no gaps

| # | Surface | Verdict |
|---|---|---|
| 1 | **The panel** | **Worked, with a change this unit made.** The mode strip line and the waterfall summary follow `DigitalGrid` (`MainWindowViewModel.cs:2672`, `:943`) and read `7.5 s slots` on FT4; the census line's arrival sentence and the arrival window itself now follow the grid (`:2845`, `:2806`). The decode table, the readiness line and the waterfall itself needed nothing: the table is text and slot stamps, the readiness line names no grid since unit 290, and the waterfall draws no boundary rules. **The UTC column's tooltip did not follow** - gap 8. |
| 2 | **The conversation** | **Worked unchanged.** `RebuildConversation` (`:2379`) folds repeats by message text walking forward in time and orders by `SlotStartUtc`. Nothing in it reads a mode, a grid or a decoder, and an FT4 row is an ordinary row to it. |
| 3 | **The ring** | **Worked, with a change this unit made.** `RefreshTurn` now hands `Ft8Turn.Read` the FT4 grid, so `TurnRingSweep` divides by `_turn.On.SlotSeconds` (`:1810-1812`) and `TurnRingCount` renders `CountText`, which unit 290 built to read 7.5 down to 0.1 and never 8. **On FT8 the record is byte-identical**: null is passed rather than `SlotGrid.Ft8`, deliberately, so not a byte of the `Ft8Turn` record moved. |
| 4 | **The filters** | **Worked unchanged.** `WantsRow` (`:1543`) is `DecodedFilterRule.Wants(ShowsCqOnly, row.Addressee)` and `IsForHim` - message text only. **No FT4 row is hidden by a filter that does not know the mode**, and the hidden count still answers *what did the band do that I cannot see*. |
| 5 | **The tooltips** | **Did not work.** Two are wrong on FT4. `App.axaml:49` `HmDecodeUtcHelp` tells the operator *FT8 runs on a strict fifteen-second grid* on a column now carrying 7.5-second stamps (gap 8). `App.axaml:50` `HmDecodeSnrHelp` describes a measurement that never runs on FT4 and does not say the column is always a dash there (gap 9). `HmDecodeContactHelp` (`App.axaml:56`) says *how many slots ago*, which is counted on FT8's grid (gap 6). **Named, not fixed.** |
| 6 | **The ledger** | **Did not work, and this unit fixed the two the instruction listed and named the rest.** Fixed in task 6: `ContactLogRow` (`ContactLogViewModel.cs:32`) showed an FT4 contact as `MFSK`, and `LogContactViewModel.Fields` (`:78`) had no submode row. **Still broken and named:** the one write path hands in `ContactModes.Named("FT8")` (`MainWindowViewModel.cs:10299`, gap 1); `Ft8ContactLedger.SlotsAgo` counts on FT8's grid (`Ft8ContactLedger.cs:96`, gap 6); `Ft8ContactStates.GoneQuietAfterSeconds` is 4 × 15 s (`Ft8ContactState.cs:102`, gap 7). |
| 7 | **The right-click menu** | **Worked up to the point where it would key, which is where it stops being this unit's.** `SendMenuFor` (`:9928`) builds from `Ft8SendOptions.For` over the ledger record and the operator's own callsign and grid - all mode-independent, so the menu appears on an FT4 row with its options in order and nothing greyed. **What it composes cannot be sent**: `Ft8Composer` is FT8-only and `SendMessage` arms at FT8's next boundary (`:10553-10554`), gap 5. **The report reply is not offered at all**, because `MeasuredReport` (`:10469`) parses a ratio that is always absent on FT4, gap 4. Named against what it does today, as instructed; nothing was built. |

**The 13 gaps, in one list.** None was fixed except where a task above already
required it.

1. `MainWindowViewModel.cs:10299` - the log's one write path is unconditionally FT8.
   **In the way of unit 293** and must land with it.
2. `Ft8Reception.cs:627` `Measure` - no FT4 signal-to-noise ratio exists.
3. `Ft8Reception.cs` `ReadFt4` - no port comparison is possible on FT4.
4. `MainWindowViewModel.cs:10469` - no signal report offered on an FT4 row.
   **Unit 293's**, and it follows from gap 2.
5. `Ft8Composer` / `MainWindowViewModel.cs:10553` - the menu composes a reply Hamlet
   cannot send on FT4. **Unit 293's.**
6. `Ft8ContactLedger.cs:96` - *slots ago* counted on FT8's grid; half the true count
   on FT4.
7. `Ft8ContactState.cs:102` - *gone quiet* after 60 s, which is eight FT4 slots
   rather than four.
8. `App.axaml:49` - the UTC tooltip asserts a fifteen-second grid.
9. `App.axaml:50` - the SNR tooltip describes a measurement FT4 never takes.
10. `MainWindowViewModel.cs:1815` `DigitalModeFor` - PSK31 and WSPR run FT8's grid
    and FT8's decoder, because neither has a path anywhere in this tree. Parked, and
    now measurable rather than assumed.
11. `Ft8Reception.cs:398` `NoWholeSlot` - names fifteen seconds; unreachable today.
12. `Ft8TransmitSequence.cs:497-530` - the guard measures against `Ft8Slots`
    literals. **Unit 293's.** Read and unchanged.
13. `MainWindowViewModel.cs:1917` - the stopped-send sentence uses
    `Ft8Slots.SlotStart`; unreachable on FT4 because nothing can transmit.

### 3.4 What FT8 did before and after

**Not a boundary, not a tick, not a byte of a record.**

- `AnFt8ReadIsWhatItWasBeforeThisUnit` - the same thirty-second recording, with FT8
  chosen **and** with nothing chosen: `15.00 s slots, 12.64 s transmission`, **2
  slots**, **`Ft8Sharp.Deep` with fine sync and ordered statistics both on**, 0
  messages. Identical to what task 1 measured at HEAD `9449d02`.
- `AnFt8PressLeavesEveryBoundaryAndSentenceWhereItWas` - after a round trip FT8 → FT4
  → FT8, all five boundaries in a minute are **tick-identical** to
  `Ft8Slots.BoundariesBetween`, and both screen sentences are byte-identical to a
  panel that never left FT8.
- `Ft8Turn` still records a **null** grid on FT8, so the record itself is unchanged.
- Unit 290's own pin,
  `TheGridIsNotAConstantTests.Ft8sBoundariesAreTickIdenticalToTheWholeSecondArithmetic`
  - **1 of 1**, unchanged.
- FT8's real-audio route through `Ft8Reader.Read`: `HamletDecodesThroughDeepTests`,
  `ACaptureSaysWhichDecoderReadItTests`, `ThePortComparisonIsEvidenceTests` -
  **9 of 9**.
- The ledger's FT8 cell is byte-identical (`FT8`, no separator) and the FT8 dialog
  still says *Hamlet heard all 11 of these*.

### 3.5 Test counts, every one filtered by exact name and foregrounded

| Filter | Result | Timeout |
|---|---|---|
| `PressingFt4TunesAndDecodesFt4Tests` (7 cases, new) | **7 / 7** | 400 s |
| `TheLedgerShowsTheSubmodeTests` (4 cases, new) | **4 / 4** | 400 s |
| `TheScreenSaysWhichGridIsRunningTests` (rewritten to the new seam) | **4 / 4** | 400 s |
| `TheSlotSaysHowMuchAudioArrivedTests` (one case rewritten), `TheGridIsNotAConstantTests`, `ACapturedFileDiagnosesItselfTests` | **15 / 15** | 400 s |
| `TheAppOpensWhereItWasLeftTests`, `PressTheModeLandOnTheFrequencyTests` (controls) | **12 / 12** | 400 s |
| `HamletDecodesThroughDeepTests`, `ACaptureSaysWhichDecoderReadItTests`, `ThePortComparisonIsEvidenceTests` (FT8 controls) | **9 / 9** | 500 s |
| `TheLogHasAWindowTests`, `TheLogShowsBothGridsTests`, `TheLogDoesNotClipItsColumnsTests` (ledger controls) | **13 / 13** | 400 s |
| `AContactRemembersItsOwnDialTests`, `TheLogDialogAndTheWorkedMarkTests`, `HowMuchTheApplicationSaysTests` (ledger controls) | **23 / 23** | 400 s |
| `TheSheetSaysWhichAudioPathItRanOnTests` (inherited, re-measured only) | 9 / 11 - **the 2 inherited reds unit 290 named, unchanged in number** | 400 s |
| `TheSpanRatioReachesTheSidecarTests` | **5 / 5** | 400 s |

**Every new test names the breakage it would have caught**, in its own remarks:
`PressingFt4TunesAndDecodesFt4Tests` catches the FT4 button tuning correctly and then
decoding nothing all evening with no sentence saying why - which is what the tab did
at HEAD `9449d02`, measured rather than described. `TheLedgerShowsTheSubmodeTests`
catches a log showing FT4, JS8 and MSK144 as one word, and its mirror image, an FT8
contact made to look as though it were missing a field it cannot have.

### 3.6 Mismatches between the instruction and the tree

**None in the instruction.** Every claim checked held: root version `1.12.229`,
`Ft8Sharp` `0.11.0`, `_digitalGrid` at `:1661` with `UseGridForTests` as its only
writer, `SlotGrid` at `Ft8Slots.cs:137` with `Ft4` at `:153` reading both numbers from
`Ft8Sharp.Ft4Timing`, `Ft8SlotWatch.Grid` an `init` at `:98`, the press path at
`:1039`/`:1052`/`:1059`/`:1062`, five `FT4 sprint` rows at the five lines named,
`OnSlotTick` at `:8928`, `DecodeTheSlotAsync` at `:8979` calling `Ft8Reader.Read` at
`:8986`, `Ft8Reader.Read` at `Ft8Reception.cs:421` defaulting to Deep at `:460`,
`Ft4SlotDecoder` at `:41` with its four published bounds at `:96-105`,
`Ft8DecoderIdentity` at `:292` with `Port` at `:301`, the guard at
`Ft8TransmitSequence.cs:497-530`, `DigitalModeChip.Labels` at `:60` and
`IsChosenElsewhere` at `:40`, and `ContactModes.Named("FT8")` at `:10299`.

**Two small things worth recording rather than repairing.**

1. `Ft8Reader.NoWholeSlot` is described as the refusal for audio holding no whole
   slot. It is **unreachable**: `Ft8SlotCutter.Cut` returns a non-empty `Reason` on
   every path that yields no slots, so the `cut.Reason.Length > 0 ? ... : NoWholeSlot`
   fallback never renders. It is gap 11 and it was left alone.
2. `Ft4WaterfallGeometry` **derives from `Ft8WaterfallGeometry`**, which is what let
   `Ft8Monitor` and `Ft8SlotMessage.TimeSeconds`/`FrequencyHz` be shared by the FT4
   branch without touching `Ft8Sharp`. Worth knowing before unit 293 looks for a
   reason to change the port.

---

## 4. What's blocking us

**11 items. None blocks criterion 1 or criterion 2, and none blocked this unit.**
Numbered 1 to 10 with a `6b`, because item 6b arrived after the ordering block was
written and renumbering the rest would break the cross-references in section 2.

### Four with Tim, all four carried and none settled here

1. **FT4's transmission: 4.48 seconds or 5.04?** Raised by 288, carried by 289, 290,
   291 and now 292. `SlotGrid.Ft4` reads both numbers from `Ft8Sharp.Ft4Timing` and
   the constant lives in one file, so a ruling still costs one edit. **No sentence on
   screen states a transmission length on either grid**, asserted by
   `TheSentencesFollowTheChosenModeAndNameNoTransmissionLength`. Beside both criteria.
2. **The version scheme**, HM-DEC-150 against what `Directory.Build.props` has been
   doing. With Tim since 288. Bumped as instructed - five patches,
   `1.12.229` → `1.12.234`. Beside both criteria.
3. **Unit 289's widened FT4 candidate sweep, blocks -10 to 51.** This unit **called
   the decoder that has it and did not retune it** - `ReadFt4` builds its search
   mirror from `decoder.FirstBlockOffset` and `decoder.LastBlockOffset` so the two
   cannot come apart. Nothing was widened and no decode was missed. Beside both
   criteria.
4. **The four inherited reds unit 290 found.** Not chased and not added to. Two of
   them - in `TheSheetSaysWhichAudioPathItRanOnTests` - were re-measured after this
   unit changed that sheet's arrival line, and the count is **unchanged at 2 of 11**.
   The other two were not run. Beside both criteria.

### Five that are the launcher's or the sandbox's

5. **Every `.bat` in `tools/arbiter/` is refused, for the fourth consecutive unit.**
   Work instruction 292 said the authoring session had run `outcome-read.bat`
   successfully again and told this unit to try the one form once and stop the moment
   it was refused. `./tools/arbiter/outcome-append.bat` was tried exactly once, in
   exactly that form - forward slashes, leading `./`, no `cd`, no redirection - and
   the sandbox answered *This command requires approval*. This session is
   non-interactive, so there was nobody to approve it. **No second call was spent.**
   289, 290, 291 and 292 have now all measured this. The entry was written with the
   file-editing tools in `outcome-entry.py`'s format, says so on its own face, and
   its arguments are committed at `tools/arbiter/unit292-append.bat`.
6. **The three untracked leftovers are still in the tree, and this is the fourth unit
   that could not remove them.** `.unit290-commit.txt`, `tools/census15.sh` and
   `tests/Ft8Sharp.Tests/Unit289SourceProbe.cs`. **Both routes were refused**: `rm`
   answered *may only remove files from the allowed working directories* about a file
   inside the working directory, and `git clean -n` required approval. They were left
   rather than committed. **The third is the one that matters**: it is a `.cs` file in
   a test project, so the tree this session tested is not the tree a fresh clone
   builds - four units old now, and still a standing item.
6b. **`.commit-msg.txt` is left modified in the working tree**, and this is the same
    refusal one notch along. It is a **tracked** scratch file - its history shows unit
    269 untracking it and a later unit re-tracking it - and this session used it to
    carry each task's commit message, because this shell will not take a quoted
    heredoc containing an apostrophe. **Nothing was committed from it**; every commit
    used `-F` and staged only named paths. Restoring it with `git checkout --` was
    refused for approval, so it is left holding the last commit message. **A next
    session will see it modified in `git status` and can discard it freely.**

7. **`PHASE_STATUS.md`'s `CURRENT_STEP:` still reads `1`** while the phase is on step
   4. It is the launcher's line and was not written. Reported again, as unit 291 was
   told to do; repairing it is not this unit's.
8. **`.run-unit/reload.txt:9` says `CLAUDE.md section 1 holds CPS-DEC-0160`.** It does
   not. `CPS-DEC-` appears **zero times** in `CLAUDE.md`, `DECISIONS.md` and
   `PROJECT_STATUS.md`; `HM-DEC-160` appears in all three. **The three files already
   agree and there is nothing to repair.** The reload is the launcher's file. Fourth
   unit reporting this.

### Two that need a citation

9. **Two field-guide frequencies disagree with the convention data**, found by unit
   291 and untouched: RTTY at 7.062 MHz against the data's 7.040, PSK31 at 7.065
   against 7.070. Needs a citation and a ruling on which source is right. Not
   adjudicated from memory. Beside both criteria.
10. **The frequency table has no FT4 row on 30 m or 17 m.** Counted again this unit -
    five rows, on 80, 40, 20, 15 and 10 m. **A band with no row moves nothing and says
    so, and that is the correct behaviour**, not a defect; it needs a citation before
    a row could be added. Beside both criteria.

### One thing that is not blocking and is worth saying plainly

**Gap 1 - the log's write path - is the only finding in this report that could become
a §0.0 fault in the permanent record.** It is not blocking tonight, because
`CanLogRow` needs a station to have addressed the operator and nothing can address him
on FT4 until Hamlet can transmit on it. It is in section 2 rather than here because it
is unit 293's to land, not a blocker to be cleared before unit 293 starts.
