READ IN THIS ORDER

A. The phase goal is **FT4 works exactly the way FT8 does.** Step 0 done; step 1
   done; **step 2 done**; steps 3, 4, 5 and 6 not started. Nothing this unit found
   changes the state of another step, and two things step 3 and step 4 must know
   are in section 2: the census names five on-screen sentences and one field-guide
   row that still assume fifteen seconds and need a mode threaded to them, and the
   transmit guard at `Ft8TransmitSequence.cs:497-530` is one of the places that
   needs it.

B. Step 2 and its four exit criteria:
   1. slots cut on 7.5-second boundaries from corrected UTC, and a capture's
      sidecar says which grid it used                          must-pass **MET**
   2. the turn ring counts down 7.5 seconds, and whose turn it is derives from
      what the other station sent on that grid                 must-pass **MET**
   3. a slot the operator transmitted in says so and reports no search result,
      as unit 282 built for FT8                                must-pass **MET**
   4. nothing assumes fifteen seconds anywhere, and the report names what it
      found that did                                           must-pass **MET**
      (closed by task 1's census, which is section 3's first item)

C. This report's own findings. **Section 4 raises 5 items and none of them is in
   the way of a criterion in B.** Three are the questions already with Tim and
   carried unchanged - the 4.48 against 5.04 timing, the version scheme, and unit
   289's widened candidate sweep - and step 2 was built so none of the three can
   block it. The fourth is four inherited red tests found while running the
   controls, none about the grid and none chased. The fifth is the
   `PHASE_STATUS.md` ownership mismatch, reported and not repaired.
   **Task 7, the named drop candidate, was taken rather than dropped**, so
   criterion 4 is met twice over: by the census naming what assumes fifteen, and
   by the two sentences unit 288 found no longer saying it.

UNIT:       290 - complete at task 7 of 7, none dropped - 2026-09-09 09:53
PHASE GOAL: FT4 works exactly the way FT8 does.
UNIT GOAL:  Slots are cut, watched, counted and counted down on a 7.5-second grid
            from corrected UTC, and every fifteen-second assumption left in the
            tree is named, whether or not it was changed.
ADVANCED:   yes - step 2, all four exit criteria, from not started to done
NUMBER:     the census, measured at 2edfb6a before any arithmetic moved: **47
            arithmetic**, **72 prose** and **15 on screen** in `Hamlet.RadioEngine`
            and `Hamlet.App`. The on-screen count separately: 11 of the 15 wrote
            fifteen as a literal and 4 formatted it from the constant. Six of
            those 11 now follow the grid or name no length; 5 stand. The port
            carries 4 more arithmetic sites and 12 more prose lines, named and
            left alone.
DRIFT:      0 consecutive units without advance (was 0)

## 1. What Claude did

**Complete at task 7 of 7. Nothing was dropped, including the named drop
candidate.** This machine, `C:\Source\HamLet`, project gate `PROJECT: Hamlet`
verified against the tree - `SHACK_FACTS.md` present,
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` present, `CoreHMI.sln`
absent, `MURC.sln` absent - branch `main`, seven commits pushed.

### The order of the work, and why task 7 came before task 6

Tasks 1 to 5 ran in order. **Task 7 was then done before task 6**, which is the
one sequencing decision this session made for itself. Task 6 is bookkeeping and
its `PHASE_OUTCOME.md` entry records what the unit ended as; running it last
meant the entry could say *seven of seven, none dropped* as a fact rather than as
a forecast. Task 6's own instruction - *do this even if task 7 is dropped* -
survives the swap intact, because both were done.

### What was verified against the instruction, and what did not match

Everything the instruction stated about the tree held. Root version `1.12.216`
at `Directory.Build.props:267` and `Ft8Sharp` at `0.11.0` at
`src/Ft8Sharp/Directory.Build.props:438`, both read. `Ft8Slots.SlotSeconds` at
`:126` and `TransmissionSeconds` at `:135`, and the five functions at `:162`,
`:174`, `:182`, `:195` and `:209`, all exactly where it said. **103 references to
`Ft8Slots.` - 49 in `src`, 54 in `tests`, across the 8 named files - counted and
identical.** `Ft8Turn`'s truncation at `:77` and its countdown at `:210-211`,
`Ft4Timing.SlotSeconds = 7.5f` at `:51` and `OccupancySeconds` at `:63`, the
`ProjectReference` at `Hamlet.RadioEngine.csproj:33`, the sidecar's `slotGrid`
key at `:282` and its counts at `:292` and `:357`, the two on-screen sentences,
and `_transmittedSlots` at `:1651` with its three call sites: all confirmed.

**One thing the instruction named that turned out to be understated**, and it is
a finding rather than a mismatch. It said the tree holds three independent copies
of 15 - `Ft8Slots.SlotSeconds`, `Ft8WaterfallGeometry.SlotSeconds` and
`Ft8Waveform.SlotSeconds`. **There are five.** The training path carries two more:
`SignalSynthesizer.Ft8Period` (`:20`, `TimeSpan.FromSeconds(15)`) and
`ModeAudio.cs:53` (`TrainingMode.Ft8 => TimeSpan.FromSeconds(15)`). And 12.64 has
**two** independent copies - `Ft8Slots.TransmissionSeconds` and
`SignalSynthesizer.Ft8Transmission` - beside the port's, which derives it as
79 x 0.16 rather than storing it. **None of the five or the two was consolidated**;
the port's are not this unit's, and the training path's are step 4's or later.

### The decisions this session made for itself, in full

**One: what the ring shows on a slot that is not a whole number of seconds.** The
instruction named this as arithmetic meeting a type and asked what was done about
it. The ring shows **one decimal where the slot is fractional and whole seconds
where it is not**, and the rule keys off the slot rather than the moment - which
is what keeps FT8's countdown reading whole seconds in every state including the
one where its 12.64-second transmission is running. Rounding up in whole seconds
would have opened an FT4 slot at **8**, and 8 is a slot length nothing in this
application is cutting on: a countdown asserting a grid the cutter is not cutting,
on the one screen the operator times his transmission by. `TurnRingSweep` was
already drawn as a fraction and its own remark said that is what lets two lengths
share one shape; four lengths share it now and only the denominator moved.
`Ft8Turn.SecondsLeft` keeps its `int` and its ceiling, and nothing on screen reads
it any more.

**Two: the sidecar and the two sentences name the slot LENGTH rather than the
mode name.** The length is what the application measured and the name is a label;
and while the 4.48 against 5.04 question is open, a sheet or a screen reading
`FT4` would be asserting an answer to it. This is the same reasoning in three
places, applied once.

**Three: the sequencing swap above.**

**Nothing else was decided.** The 4.48 against 5.04 figure, the widened candidate
sweep and the version scheme all stay with Tim, untouched, and step 2 was built so
that none of them can block it - the slot is 7.5 seconds on either timing answer,
and every criterion in this step is about the grid rather than the occupancy.
`5.04` and `7.5` are typed nowhere in `Hamlet.RadioEngine` or `Hamlet.App`: both
arrive from `Ft8Sharp.Ft4Timing` through `SlotGrid.Ft4`, so a ruling still costs
one edit in one file. **That was checked by grep at the end and by an assertion in
the tests, not assumed.**

### What was built

`SlotGrid` is a `readonly record struct` carrying the slot length and the
transmission length together, with `SlotStart`, `IntoSlot`, `BoundariesBetween`,
`TransmissionFits`, `SlotsPerMinute` and `Describe` on it. `SlotGrid.Ft8` hands in
`Ft8Slots`'s own constants and `SlotGrid.Ft4` reads both numbers from the port.
`Ft8Slots` keeps its constants and its five functions and is now a forwarder onto
`SlotGrid.Ft8` - unit 289's own device for `Ft8WaterfallGeometry` - so there is
one implementation and not two that could drift apart.

The arithmetic is in **ticks, floored from the top of the minute**. Whole seconds
could not express a 7.5-second boundary at all. The minute is the anchor because
it is the largest unit both lengths divide exactly - four FT8 slots, eight FT4
slots - so nothing accumulates across an hour or a day, and `BoundariesBetween`
steps in ticks rather than by `AddSeconds` so a boundary list cannot drift off its
own grid.

`Ft8SlotCutter.Cut`, `DigitalCaptureSheet.Compose` and `Ft8Turn.Read` take an
optional `SlotGrid`; `Ft8SlotWatch` carries one as an init-only property. **All
four default to FT8**, so every caller written before this unit does exactly what
it did. The watch's grid is init-only deliberately: a watch that changed grid
mid-flight would hold `_lastSeenSlotStart` on one grid and compare it against
`current` on another, and that comparison is what decides whether a slot closed -
so the fault would surface as a skipped or duplicated slot rather than as an
error. A mode change builds a new watch, which arms afresh.

### Tests, all foregrounded and filtered by exact name. No suite was run.

| Test | Count | What it is |
|---|---|---|
| `TheGridIsNotAConstantTests` | 5 of 5 | new - both grids, and FT8 unchanged |
| `SlotsAreCutOnTheGridTheyWereGivenTests` | 5 of 5 | new - criterion 1 |
| `TheRingCountsDownSevenAndAHalfTests` | 5 of 5 | new - criterion 2 |
| `ASlotHeTransmittedInOnFt4sGridSaysSoTests` | 4 of 4 | new - criterion 3 |
| `TheScreenSaysWhichGridIsRunningTests` | 4 of 4 | new - task 7 |
| `TheClockIsMeasuredNotCorrectedTests` + `TheCutterAndTheSidecarAgreeTests` | 19 of 19 | FT8 control, unchanged |
| `TheBeatIsDerivedNotGuessedTests` | 13 of 13 | FT8 control, unchanged |
| `TheTurnIsARingTests` | 9 of 9 | FT8 control, unchanged |
| `ASlotHeTransmittedInSaysSoTests` | 3 of 3 | unit 282's control, unchanged |
| `TheSlotCutterTests`, `TheSlotWatchTests`, `TheDigitalTabDecodesWhatItKeptTests`, `ACapturedFileDiagnosesItselfTests` and the new cutter test | 36 of 36 | FT8 controls, unchanged |
| `TheTabSaysWhyNothingIsDecodingTests`, `TheDecodedTableIsRealTests`, `TheTabHearsEverySlotTests`, `EverySlotLeavesALineTests` | 45 of 47 | 2 inherited reds, section 4 |
| `TheSheetSaysWhichAudioPathItRanOnTests` | 8 of 10 | 2 inherited reds, section 4 |

**No FT8 test's expected value was changed.** One call was updated - the const
`DigitalIdleText.ModeStrip` became `ModeStripFor(SlotGrid)` - which the
instruction names as an updated call rather than a changed expectation, and the
value it is compared against is still FT8's grid.

### Task 6, and what the shell would not do

`tools\arbiter\outcome-append.bat` was tried once, in three invocation forms - a
relative path with a redirection, `cmd //c`, and `./tools/arbiter/...` - and all
three were refused by the sandbox. **This is a non-interactive session, so there
was nobody to approve them.** This is the refusal unit 289 measured and the
instruction anticipated, and no more time was spent on it. The entry was written
with the file-editing tools in the format `outcome-entry.py` produces, it says so
on its own face under `APPENDED BY HAND:`, and the arguments the script would have
been given are committed at `tools/arbiter/unit290-append.bat` so it can be
replayed rather than reconstructed. **Unit 289's two entries were not touched.**
`tools\arbiter\validate-output.bat` met the same refusal; what was done instead is
in section 3.

### Three pieces of litter, named because none could be swept up

**`.commit-msg.txt` is a tracked file and this session overwrote it seven times**,
once per commit, using it to pass multi-line messages to `git commit -F` because
this shell will not carry a heredoc containing an apostrophe. It should not have
been the file used: it was already in the repository, holding unit 272's commit
message. **It has been restored to its committed content and the working tree
shows no diff on it.** Unit 269 untracked it once (`9510527`) and it came back, so
it is worth someone deciding whether it belongs in the repository at all.

**`tools/census15.sh` is an untracked leftover.** It was written early in task 1 to
run the prose census as a script, was never run - the shell refuses scripts too -
and the census was done with plain `grep` instead. **The sandbox refused every
attempt to delete it**, so it is named here rather than silently left. It is not
staged, not committed and reads nothing but `src`.

**`.unit290-commit.txt` is an untracked leftover of the same kind**, written to
carry task 6's own commit message once `.commit-msg.txt` had been restored. The
sandbox refused to delete it too. Not staged, not committed.

## 2. What the owner should expect

**The slot machinery can now run FT4.** Everything that cuts a slot, watches for
one closing, counts them into a capture sidecar, counts down inside one, or says
whose it is, will run on a 7.5-second grid the moment something hands it one. All
of it is proved on FT4's real numbers rather than on a stand-in.

**Nothing has changed on the screen for FT8**, and that is deliberate. The
waterfall summary still reads `15 s slots` character for character. The turn ring
still counts 15, 14, 13 down to 1. Every slot boundary is identical to the tick.

**What will look wrong but is not.** The idle line on the Digital tab changed its
wording: it used to say *FT8 runs in fifteen second slots* and now says *Slots
here run 15 seconds*. It stopped naming a mode because the tab has no way yet to
know which mode it is running, and a sentence naming FT8 on a 7.5-second grid
would be wrong twice over.

**The FT4 button still does nothing.** This unit built the seam and nothing that
presses it. `MainWindowViewModel.DigitalGrid` is get-only, always FT8's, and no
code in the application sets it.

### What step 4 inherits

Step 4 has to thread a mode from the button to these places. They are named here
so it is a list rather than a search:

- **`MainWindowViewModel.DigitalGrid`** - one property, and the two on-screen
  sentences already read it. This is the intended entry point.
- **`MainWindowViewModel.DriveTheArmedSend`** (`:10437-10438`) and the next-
  boundary arithmetic at `:10381` - **both still call `Ft8Slots.SlotStart`
  directly.** This matters more than it looks: the send path books a slot key that
  `_transmittedSlots` is read against, so on FT4 the send side would key at `:15`
  while the watch reported a slot at `:07.5`, and the `HashSet<DateTime>` would
  miss silently. Task 5 proved the key holds **when both sides are on the same
  grid**; making both sides be on the same grid is step 4's, and it is the single
  highest-value thing on this list.
- **`Ft8TransmitSequence.cs:497-530`** - **yes, this is one of them.** It guards a
  send against `Ft8Slots.SlotSeconds` at `:497` and `:505` and against
  `Ft8Slots.TransmissionFits` at `:507`, and its refusal sentences format the FT8
  constants at `:501`, `:512` and `:530`. Parked by this instruction and untouched.
  On FT4 it would refuse or admit a send against the wrong slot length.
- **The construction of `Ft8SlotWatch`** - it takes its grid at construction, so
  a mode change must build a new watch rather than mutate one.
- **`Ft8Composer`** - its padded slot comes from `Ft8Waveform.SlotSeconds` in the
  port. `Ft4Waveform` already has its own, reading `Ft4Timing`, so this is a call
  to switch rather than arithmetic to write.
- **`SignalSynthesizer.Ft8Period` and `ModeAudio.cs:53`** - the training radio's
  own copies of 15 and 12.64. Not on the receive path.

### What step 3 should know

**`ModeGuide.cs:55` describes FT8 as `15-second warbles` and there is no FT4 row
at all.** That is correct about FT8 and is a gap rather than an error - it wants a
row beside it, not an edit. Step 3 owns `SUBMODE`, the ADIF log and the
achievements FT4 row, and the field guide belongs in the same conversation.

## 3. What you should see

### 1. The census - criterion 4, which no other task closes

Measured over the whole tree at commit `2edfb6a`, **before any arithmetic moved**,
which is why it runs first: a census written from a diff is a list of its author's
own edits wearing a survey's clothes.

**`Hamlet.RadioEngine` and `Hamlet.App`: 47 arithmetic, 72 prose, 15 on screen.**

**The arithmetic list in full**, file and line, at `2edfb6a`:

| File | Lines | What it computes |
|---|---|---|
| `Audio/Ft8Slots.cs` | 126, 135 | the two constants themselves |
| `Audio/Ft8Slots.cs` | 163 | `TransmissionFits` against 12.64 |
| `Audio/Ft8Slots.cs` | 184, 186 | `SlotStart` - the `(int)` cast and the whole-second `DateTime` |
| `Audio/Ft8Slots.cs` | 196 | `IntoSlot`, via `SlotStart` |
| `Audio/Ft8Slots.cs` | 222, 228 | `BoundariesBetween`, stepping by `AddSeconds(15)` |
| `Audio/Ft8SlotCutter.cs` | 115, 117 | samples per slot and per transmission |
| `Audio/Ft8SlotCutter.cs` | 129, 153 | the boundary list and the fit test |
| `Audio/Ft8SlotWatch.cs` | 262 | the current slot |
| `Audio/Ft8SlotWatch.cs` | 285 | how many slots closed between two looks |
| `Audio/Ft8SlotWatch.cs` | 295 | samples per slot |
| `Audio/Ft8SlotWatch.cs` | 360, 380 | the arrival window and the slot handed over |
| `Audio/Ft8Turn.cs` | 77 | `private const int Slot = (int)Ft8Slots.SlotSeconds` |
| `Audio/Ft8Turn.cs` | 163, 165, 169 | the transmitting countdown against 12.64 |
| `Audio/Ft8Turn.cs` | 180 | the current slot for parity |
| `Audio/Ft8Turn.cs` | 185, 191 | `TheirSlotSecond` as parity x 15 |
| `Audio/Ft8Turn.cs` | 198 | `ParityOf`, `(second / 15) % 2` |
| `Audio/Ft8Turn.cs` | 211, 213 | the countdown and its clamp |
| `Audio/DigitalCaptureSheet.cs` | 287, 292, 357 | the boundary list and the two whole-transmission counts |
| `Contacts/Ft8ContactLedger.cs` | 96 | slots between two moments |
| `Contacts/Ft8ContactState.cs` | 102 | `GoneQuietAfterSlots * SlotSeconds` |
| `ViewModels/MainWindowViewModel.cs` | 1688, 1689 | the ring's denominator |
| `ViewModels/MainWindowViewModel.cs` | 1870 | has the slot moved past a stop |
| `ViewModels/MainWindowViewModel.cs` | 2390 | slots since the last heard message |
| `ViewModels/MainWindowViewModel.cs` | 2711 | a slot as a `TimeSpan` |
| `ViewModels/MainWindowViewModel.cs` | 10370 | the next boundary |
| `ViewModels/MainWindowViewModel.cs` | 10427 | the boundary the armed send is driven on |
| `Transmit/Ft8TransmitSequence.cs` | 497, 505, 507 | **PARKED, step 4** - the send guard |
| `Training/SignalSynthesizer.cs` | 20, 23, 258 | the training radio's own 15 and 12.64 |
| `Training/ModeAudio.cs` | 53, 159 | the training cycle |

**Prose: 72 lines across 27 files.** `Ft8SlotWatch.cs` carries 12, `Ft8Slots.cs`
9, `MainWindowViewModel.cs` 16, `Ft8SlotCutter.cs` 5, `Ft8Composer.cs` 4,
`Ft8Reception.cs` 3, `AudioSpectrumSource.cs` 3, `AudioHandoff.cs` 2,
`Ft8ArmedSend.cs` 2, `SignalSynthesizer.cs` 2, `ModeAudio.cs` 2,
`Ft8TransmitSequence.cs` 2, and one each in `AudioArrival.cs`, `AudioTap.cs`,
`DigitalCaptureSheet.cs`, `Ft8Resample.cs`, `Ft8Turn.cs`, `ReusableWindow.cs`,
`Ft8ContactState.cs`, `SyntheticSignal.cs`, `FollowingScroll.cs` and
`ContactMilestones.cs`. **Wrong documentation, not wrong behaviour, and none of it
was rewritten.** Excluded as not about the slot grid, and named so the count can be
checked: eleven CW lines, `WasapiTransmitSink.cs:81` (fifteen *milliseconds*),
`SourceHealth.cs:70` and `MainWindowViewModel.cs:11721` (fifteen *minutes*), and
`ScanStop.cs:194`.

**On screen: 15, of which 11 wrote fifteen as a literal.** These are the §0.0
breaches, because they are what the operator reads:

| Where | What it said | Now |
|---|---|---|
| `DigitalIdleText.cs:24` | "FT8 runs in fifteen second slots" | **follows the grid** |
| `MainWindowViewModel.cs:939` | "15 s slots" | **follows the grid** |
| `DigitalReadiness.cs:92` | "the fifteen second boundaries fall is not known" | **names no length** |
| `Ft8SlotCutter.cs:80` (`NoOffset`) | "where the fifteen-second boundaries fall" | **names no length** |
| `DigitalCaptureSheet.cs:282` | sidecar, clock unread | **names the grid** |
| `DigitalCaptureSheet.cs:338` | "no fifteen-second boundary falls inside this window" | **names the grid** |
| `MainWindowViewModel.cs:2759` | "of the last fifteen seconds, so this slot is fragments" | **stands** |
| `Ft8SlotWatch.cs:145` (`AudioShort`) | "delivered {0} of the last fifteen seconds" | **stands** |
| `Ft8Reception.cs:399` | "there is not a whole fifteen-second slot in what was kept" | **stands** |
| `DigitalCaptureSheet.cs:311` | "over the last fifteen seconds" | **stands** |
| `Explore/ModeGuide.cs:55` | FT8's row: "15-second warbles" | **stands - true of FT8** |

The four that formatted fifteen from the constant rather than writing it -
`Ft8TransmitSequence.cs:501`, `:512`, `:530` and `DigitalCaptureSheet.cs:349` -
follow whatever grid the constant carries and are not literals. **Three of the
four that stand describe an arrival window whose length happens to be the slot**;
they need the grid threaded to them and that is step 4's work, not a wording fix.

**`Ft8Sharp` and `Ft8Sharp.Deep`, marked separately and changed in no way:** 4
arithmetic sites - `Ft8WaterfallGeometry.cs:52` and `:106`, `Ft8Waveform.cs:69`
and `:128` - and 12 prose lines. **FT4 already has its own beside every one of
them**: `Ft4WaterfallGeometry.cs:58` and `Ft4Waveform.cs:72` and `:130`, all
reading `Ft4Timing.SlotSeconds`. The port is faithful and it is not this unit's.

**How many independent copies of each number the tree holds: five of 15 and two of
12.64.** Named in section 1. None consolidated.

### 2. The boundaries, both grids, side by side

`SlotStart` at a handful of moments past the top of a minute. The FT4 column
includes what the arithmetic this unit replaced would have produced if its
constant had simply been changed to 7.5 - which is what a session reaching for a
one-line fix would have shipped.

| Moment | FT8 grid | FT4 grid | what a changed constant gave |
|---|---|---|---|
| `:00.000` | `:00` | `:00.0` | `:00` - right, by coincidence |
| `:07.000` | `:00` | `:00.0` | `:07` |
| `:07.500` | `:00` | `:07.5` | `:07` |
| `:08.000` | `:00` | `:07.5` | `:07` |
| `:14.900` | `:00` | `:07.5` | `:14` |
| `:15.000` | `:15` | `:15.0` | `:14` |
| `:22.500` | `:15` | `:22.5` | `:21` |
| `:29.999` | `:15` | `:22.5` | `:28` |
| `:37.500` | `:30` | `:37.5` | `:35` |
| `:45.000` | `:45` | `:45.0` | `:42` |
| `:52.500` | `:45` | `:52.5` | `:49` |
| `:59.999` | `:45` | `:52.5` | `:56` |

`(int)7.5` is `7`. The old arithmetic agreed with the real 7.5-second grid at
exactly one of those eleven boundaries, `:00`, and by three and a half seconds at
`:52.5`. **And it could not have expressed four of the eight boundaries anyway**:
its `DateTime` constructor took whole seconds and had no field for the half.

**The evidence that FT8's column is identical to what it was before this unit.**
`Ft8sBoundariesAreTickIdenticalToTheWholeSecondArithmetic` keeps a transcription
of the replaced arithmetic as a control that nothing in the application calls, and
compares the shipping code against it over **3,888 moments** - every sixteenth of
a second across four minutes, which crosses three minute boundaries and all four
slots in each, plus a spread of awkward tick offsets including one tick either
side of every boundary. **Every one is identical to the tick.** Not a rounding
comparison: `Ticks` against `Ticks`.

A minute of silence at 12 kHz, cut:

```
FT8 (15.00 s slots, 12.64 s transmission): 4 slots
  14:22:00.000  sample      0  180000 samples, padded 0.00 s
  14:22:15.000  sample 180000  180000 samples, padded 0.00 s
  14:22:30.000  sample 360000  180000 samples, padded 0.00 s
  14:22:45.000  sample 540000  180000 samples, padded 0.00 s
FT4 (7.50 s slots, 5.04 s transmission): 8 slots
  14:22:00.000  sample      0   90000 samples, padded 0.00 s
  14:22:07.500  sample  90000   90000 samples, padded 0.00 s
  14:22:15.000  sample 180000   90000 samples, padded 0.00 s
  14:22:22.500  sample 270000   90000 samples, padded 0.00 s
  14:22:30.000  sample 360000   90000 samples, padded 0.00 s
  14:22:37.500  sample 450000   90000 samples, padded 0.00 s
  14:22:45.000  sample 540000   90000 samples, padded 0.00 s
  14:22:52.500  sample 630000   90000 samples, padded 0.00 s
```

And the watch, stepped a quarter-second at a time through the same minute:

```
FT8 boundaries seen: 15.0, 30.0, 45.0, 60.0
FT4 boundaries seen: 7.5, 15.0, 22.5, 30.0, 37.5, 45.0, 52.5, 60.0
```

**With an unmeasured offset both grids cut nothing and say so.** A mode parameter
decides which boundaries exist; the measured offset decides where they fall in
wall-clock time, and without one the answer stays unknown. That is HM-DEC-009 and
it is asserted on both grids.

### 3. The turn

**What the ring shows on a 7.5-second slot, tenth by tenth:**

```
7.5 7.4 7.3 7.2 7.1 7.0 6.9 6.8 6.7 6.6 6.5 6.4 6.3 6.2 6.1 6.0 5.9 5.8 5.7 5.6
5.5 5.4 5.3 5.2 5.1 5.0 4.9 4.8 4.7 4.6 4.5 4.4 4.3 4.2 4.1 4.0 3.9 3.8 3.7 3.6
3.5 3.4 3.3 3.2 3.1 3.0 2.9 2.8 2.7 2.6 2.5 2.4 2.3 2.2 2.1 2.0 1.9 1.8 1.7 1.6
1.5 1.4 1.3 1.2 1.1 1.0 0.9 0.8 0.7 0.6 0.5 0.4 0.3 0.2 0.1
```

and at `:07.5` the next slot opens at `7.5`. **It never reads 8 and it never reads
0.** FT8's control column beside it, unchanged: `15 15 14 14 13 13 12 ... 2 2 1 1`.

**How the whole-second countdown was resolved against a half-second slot.** The
measurement and the display were separated. `Ft8Turn` now carries
`SecondsLeftExact` as a `double?` beside the existing `int? SecondsLeft`, and a
`CountText` property renders it - whole seconds where the slot is a whole number
of seconds, one decimal otherwise. **The rule keys off the slot rather than the
moment**, which is what keeps FT8 reading whole seconds even while its 12.64-second
transmission is counting down. `SecondsLeft` keeps its `int`, its ceiling and its
clamp, and nothing on screen reads it any longer. One decimal rather than two
because the tick that drives the ring runs four times a second, so a hundredth
would be a digit the reading cannot support. It rounds up to its own last digit
and never to zero, for the reason it never did on FT8: a zero says the slot is
over while a station is still transmitting in it.

**Parity, and unit 277's rule.** Unchanged in shape and unchanged in force. What
moved is only how a boundary becomes an index: it counts whole slots from the top
of the minute instead of dividing the second-of-minute by fifteen, because four of
FT4's eight boundaries are not on a whole second at all.

```
parity across the minute: 0 1 0 1 0 1 0 1
```

and the same eight an hour later, a day later and a minute later. **It does not
shift at a minute or an hour** because eight slots to the minute is even, exactly
as four was. A station heard on the `:07.5` half makes the `:07.5` slots theirs
and the `:00` slots his, derived from its transmission and not from a default.
`NoStationYet` is still a real state and still refuses to pick a side, and an
unmeasured clock still produces no count at all.

One thing to record: `Ft8Turn.TheirSlotSecond` is an `int?` and on FT4 it rounds
to `7` where the half is `7.5`. **`TheirSlotSecondExact` carries the measurement**
and nothing on screen reads either today; anything that starts to must read the
exact one.

**And reaching zero sends nothing.** The countdown is run to its last tenth and
across the boundary. Because a side effect would not show up in a return value,
the test also asserts the type's whole public surface - printed in the test output
- contains no member whose name reads like an action, and that `Ft8Turn.cs`
mentions no `ICivLink`, no `Ptt`, no `Ft8ArmedSend` and no `SendAsync`. **The
transmitting countdown's denominator is `Ft8Sharp.Ft4Timing.OccupancySeconds`,
asserted against the port rather than pinned to a typed 5.04.**

### 4. What the sidecar now says about its grid

Verbatim, known case:

```
slotGrid   5 boundaries, corrected to UTC, on 15.00 s slots, 12.64 s transmission
wholeSlots 4  (of 5 boundaries, this many are followed by the whole 12.64 s transmission inside the audio)

slotGrid   9 boundaries, corrected to UTC, on 7.50 s slots, 5.04 s transmission
wholeSlots 8  (of 9 boundaries, this many are followed by the whole 5.04 s transmission inside the audio)
```

Verbatim, unknown case - **it still says unknown**:

```
slotGrid   unknown (not read)  (the clock offset has not been measured, so where the boundaries of the 15.00 s slots, 12.64 s transmission fall is not known)
slotGrid   unknown (not read)  (the clock offset has not been measured, so where the boundaries of the 7.50 s slots, 5.04 s transmission fall is not known)
```

It names which grid it *would* have used, which is a fact about the capture, and
withholds where the boundaries fall, which is the thing nobody measured. There is
a third case - a window with no boundary in it at all - and it names the grid too,
because that is the capture where the spacing cannot be inferred from the list and
so is the one that most needs telling.

**`TransmissionFits` is still one function.** It was made one because two answers
disagreed in consecutive lines of one sidecar on `ft8-2026-09-03-210644`. The grid
travels into it; the arithmetic did not fork.

### The transmitted-slot key, and the two sentences

**300 of 300 moments across four FT4 slots resolved the key correctly, 0 missed**,
and exactly the 75 tenths inside the keyed slot report it as his. The census stamp
gained a tenth of a second and only where the boundary carries one:

```
15:16:07.5 UTC was yours - Hamlet was transmitting and did not listen
15:16:45 UTC was yours - Hamlet was transmitting and did not listen
```

Every FT8 stamp takes the whole-second branch and is byte-identical.

The two sentences, on both grids:

```
15   idle : nothing on this frequency yet. Slots here run 15 seconds, so give it a slot or two before deciding the band is empty.
15   wfall: 200-3000 Hz - 15 s slots
7.5  idle : nothing on this frequency yet. Slots here run 7.5 seconds, so give it a slot or two before deciding the band is empty.
7.5  wfall: 200-3000 Hz - 7.5 s slots
```

**The waterfall summary's FT8 rendering is byte-identical.** Both read one
`DigitalGrid` property, and that they cannot come to disagree is asserted rather
than arranged.

### The report validator, checked by hand

`tools\arbiter\validate-output.bat` met the same shell refusal as
`outcome-append.bat`. **A hand check is not the same thing as the script exiting
`0`**, and this is what a hand check of its seven rules against this file found:

| Rule | Result |
|---|---|
| 1 - a `UNIT:` line above section 1, parseable | pass - line 33, within the 60-line window |
| 2 - the four top-level sections, in order, exact names | pass - and no other `##` anywhere |
| 3 - no fifth top-level section | pass |
| 4 - section 4 present even when empty | pass - present and not empty |
| 5 - section 3 non-empty | pass |
| 6 - the ordering block above `UNIT:`, with `A.`, `B.`, `C.` and C naming a count | pass - `READ IN THIS ORDER` at line 1, and C says section 4 raises 5 items |
| 7 - no placeholder token in the header block | pass - no `_PENDING`, `PENDING_`, `TBD`, `TODO`, `FIXME`, `XXX`, `<FILL`, `FILL IN>` or `PLACEHOLDER` before `## 1.` |

## 4. What's blocking us

**Five items. None is in the way of a step 2 criterion.** Three are questions
already with Tim, carried unchanged and not re-argued.

### 1. FT4's transmission: 4.48 seconds or 5.04? Still with Tim.

Raised by unit 288, carried by 289, carried again here and **not settled, not from
memory and not from a model's knowledge of FT4**. `PHASE_PLAN.md` step 1 says 4.48
s; upstream's `FT4_SYMBOL_PERIOD` of `0.048f` puts 105 symbols at 5.04 s and the
string `4.48` appears nowhere in the pinned clone. **Step 2 did not depend on the
answer** - the slot is 7.5 seconds on either figure - and the constant is still in
exactly one file, `src/Ft8Sharp/Ft4Timing.cs`, so a ruling costs one edit. What
changed is that more now reads from it: the ring's transmitting denominator, the
sidecar's whole-transmission count and `SlotGrid.Ft4` all take it from there, and
`5.04` is typed nowhere in `Hamlet.RadioEngine` or `Hamlet.App`. **Most blocking of
the five, because it is the only one that will move a number the application
displays.**

### 2. The version scheme. With Tim since unit 288.

HM-DEC-150 against what `Directory.Build.props` has been doing. Bumped as
instructed, one patch per committed task: `1.12.216` to `1.12.222`. Not resolved,
not reasoned about.

### 3. Unit 289's widened FT4 candidate sweep, -10 to 51 blocks. With Tim.

Its own decision, on the record with its reasoning, and named as an item for
confirmation rather than a blocker. **This unit did not touch it and had no reason
to** - it moves nothing about where an FT4 signal sits in its slot.

### 4. Four inherited red tests, none about the grid and none chased.

Found while running FT8 controls. None is on the known-reds list
(`CwAdjudicationTests.ASpeedChangeInRealisticAudio`, the 51 CW cases in
`docs/unit239-failing-set.txt`, the `Ft8Sharp.Deep.Tests` type-list tripwire,
`HM-OPEN-088`'s ten), so the ruling wanted is whether they are fixed or added to
it.

| Test | Why it fails | Since |
|---|---|---|
| `TheSheetSaysWhichAudioPathItRanOnTests.TheCensusNamesTheStageEachSlotReached` | asserts `DoesNotContain("snr")`; the sheet grew an `snr` line per census slot | `8f53e79`, 2026-09-05 |
| `TheSheetSaysWhichAudioPathItRanOnTests.TheSheetSaysHowLoudTheAudioInEachSlotWas` | same assertion, same cause | `8f53e79`, 2026-09-05 |
| `TheDecodedTableIsRealTests.NoInventedDecodeIsLeftInTheMarkup` | looks for `{Binding DigitalDecodes}`, which the markup no longer has | markup last changed `dab335a`, 2026-09-08 |
| `TheTabHearsEverySlotTests.AFullTableStillSaysNothingAboutWhatAMessageMeans` | forbids translating a message; translation was later permitted by ruling | test last touched 2026-09-05 |

All four tests were last touched on 2026-09-05 and all four causes landed on or
after that date. **They were failing before this unit started and this unit did
not touch any of the code involved.**

### 5. `PHASE_STATUS.md` disagrees with the record. Reported, not repaired.

`STEP: 1 | partial` and `CURRENT_STEP: 1`, while `PHASE_OUTCOME.md` carries both
`done` and `partial` for unit 289 and this instruction's arbiter reads step 1 as
`done`. **Those lines and `HEARTBEAT:` belong to the launcher and were not
written.** Unit 289 was told to write them by its instruction and forbidden by its
prompt, and the conflict cost an edit made and reverted; this instruction resolves
it the same way and so does this unit. Only `WORK_INSTRUCTION:` was set, to
`290 - the slot machinery is FT4's`.

**Not an ask, recorded so nobody chases it:** `RULES_AT` needed no repair.
`.run-unit/reload.txt` reports *CLAUDE.md section 1 holds CPS-DEC-0160*, and
**`CPS-DEC-` appears nowhere in `CLAUDE.md`, `DECISIONS.md` or
`PROJECT_STATUS.md`.** `CLAUDE.md:360`, `DECISIONS.md:7` and `PROJECT_STATUS.md`
all say `HM-DEC-160`, dated 2026-09-08, and agree with each other. **The reload is
the launcher's file** and its reading is stale.
