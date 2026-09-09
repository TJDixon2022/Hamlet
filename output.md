READ IN THIS ORDER

A. THE PHASE GOAL: FT4 works exactly the way FT8 does. Step 0, where FT4 lives - done, unit 289, untouched tonight. Step 1, FT4 decodes a signal Hamlet made - partial, unit 289, untouched tonight. Step 2, the slot machinery is FT4's - partial, units 290 and 292; tonight's re-census removed two of its on-screen fifteen-second assumptions and names the one that is left, and the step does not close. Step 3, the log can say FT4 - done, unit 291, and re-evidenced tonight by the report leg's own log write: MODE=MFSK, SUBMODE=FT4, RST_SENT=-08. STEP 4, THE FT4 BUTTON WORKS, IS THE ONE AT ISSUE - partial at the start of tonight, and this unit's evidence takes it to done: the tooltips close criterion 2's last untouched surface and the report-shape exchange takes criterion 4 off carry-forward. Step 5, Tim hears FT4, and step 6, Tim works a station on FT4 - not started, both Tim's at the shack, and neither was widened to.

   DOES THIS UNIT'S EVIDENCE TAKE CRITERION 4 OFF CARRY-FORWARD? YES, IN THOSE TERMS. It rested on unit 293's RRR proof, whose stated reason in the test file was that every FT4 row's ratio is null. Unit 294 falsified that and did not re-run the proof. Tonight the whole exchange runs on the REPORT shape, from two right clicks, on a real render endpoint with a real loopback capture, and the number the row showed is asserted equal to the number that came back off the air. The criterion now rests on a capture rather than on a sentence that is no longer true.

B. STEP 4 AND ITS FOUR EXIT CRITERIA, each marked met or not, with who met it.

   1. MET - pressing FT4 tunes to the band's FT4 frequency and decodes. Unit 292. Not re-run tonight and not claimed as tonight's.
   2. MET, AND ALL SEVEN SURFACES ARE NOW ANSWERED - the log (unit 291), the button and the tune (unit 292), the send path and the composer (unit 293), the ledger and the right-click menu (unit 294), AND THE DECODE-COLUMN TOOLTIPS (this unit), which were the last of the seven never touched. Nothing in criterion 2 is now carried as parked.
   3. MET - one click, one transmission, through the same abort. Unit 293, AND RE-ASSERTED HERE ON THE REPORT LEG: UnkeyRoute.OrdinaryUnkey on both legs, exactly four CI-V frames for two clicks, NothingArmed at the FT4 boundary nobody clicked, and Ft8ArmedSend.Arm counted at exactly one caller in src/ by scanning every .cs file under it.
   4. MET - a whole exchange runs from one right click at the bench. Unit 293 on RRR, AND THIS UNIT ON THE REPORT SHAPE. THE CAPTURE WAS A REAL LOOPBACK AND NOT A FAKE: the audio went out of S34J55x (3- HD Audio Driver for Display Audio), one of 4 active WASAPI render endpoints on this machine, chosen deliberately and not the default, and was captured on a real WasapiLoopbackCapture - 241920 samples at 48000 Hz out, 261120 samples captured peaking at -12.0 dBFS. THE REPORT LEG FAILED NOWHERE. The fake-sink path exists in the same file, is labelled the fake it is, and was not the evidence.

C. WHAT THIS REPORT ADDS, WEIGHED AGAINST A AND B. THE NUMBER THAT MATTERS, AND IT AGREES: the snr cell showed -8, the menu item carried "W1ABC KC3QIS -08", and the FT4 decoder read "W1ABC KC3QIS -08" off the loopback capture. Those three are asserted as ONE EQUALITY on a tuple, not three prints a reader compares by eye. The ratio delivered into the receiver was -8.00 dB in a 2500 Hz reference bandwidth, and no row was seeded - the number came off unit 294's estimator through the tab's own decode path.

   Section 4 raises 4 items and NONE is in the way of a criterion in B, item by item. (1) The FT8 whole-chain control is 1 of 3 green and the red is inherited: it sits beside criterion 2's FT8-unchanged reading rather than in its way, because the failure is in a test row finder before any chain runs, and it was reproduced with this unit's only src change reverted. (2) Ft8Reader.NoWholeSlot still says "fifteen-second slot" on screen: that belongs to step 2, which stays partial, and is in the way of no step 4 criterion. (3) Five untracked files the harness will not let a session remove: working-tree hygiene, in the way of nothing. (4) PHASE_STATUS.md's CURRENT_STEP and the reload's RULES_AT disagreement: both the launcher's, reported and not repaired, in the way of nothing.

UNIT:       295 - complete at task 6 of 6 - 2026-09-09 13:52
PHASE GOAL: FT4 works exactly the way FT8 does.
UNIT GOAL:  A measured FT4 signal report survives the whole chain from one right
            click - cell, menu, composer, arm, wire, loopback, decoder - and
            comes back the number the row showed; and the decode columns tell the
            operator what the mode he is running actually does, rather than what
            FT8 does.
ADVANCED:   yes - step 4 exit criterion 4, taken off carry-forward and onto a real
            loopback capture on the report shape; and exit criterion 2, the
            tooltips, the last of its seven surfaces never touched.
NUMBER:     the row showed -8 -> the loopback gave back "W1ABC KC3QIS -08". They
            agree, asserted as one equality.
DRIFT:      0 consecutive units without advance  (was 0)

## 1. What Claude did

**Exit state: complete, at task 6 of 6. Nothing was dropped, including task 5, the
named drop candidate.**

Machine `C:\Source\HamLet`, project claimed `Hamlet` and confirmed against the tree by
the instruction's own four checks — `SHACK_FACTS.md` present,
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` present, `CoreHMI.sln` absent,
`MURC.sln` absent — branch `main`.

### Task 1 - the trace, and the report leg does not stop

Nothing under `src/` changed. A station calling the operator **with a grid** — so the
conventional next thing to send is a report and not an `RRR` — was synthesized as FT4 at
a commanded −8.00 dB in 2500 Hz and put through the tab's own `ShowDecodes` in FT4. Then
one real right click on the realized row, the `report` item, and the whole path to the
wire and back. Every stage is in section 3.

The tooltips were found by key at `App.axaml:49`, `:50` and `:59` — the instruction's
line numbers all hold — and **`HmDecodeContactHelp` is bound nowhere**, which the
instruction doubted and told me to measure rather than take on its word. It was right to
doubt it and right to doubt itself.

### Task 2 - the decode columns say what the running mode does

**The decision I made for myself, reproduced in full.** The instruction gave me two
honest shapes and left the choice to me: one sentence covering both modes explicitly, or
text that follows the running mode. **I took both-modes-explicitly.** Three reasons. A
string naming both modes is true whichever mode the tab is running, so it cannot go
stale the way a mode-following string does the day a third mode arrives. The operator
who switches between FT8 and FT4 wants to know what the *other* mode does, and a
mode-following string hides exactly that from him. And the rejected shape needs a
view-model property and a new binding on a live column header — more surface than a
wording fix should buy, and `BindingHealthTests` exists because unresolved bindings here
are silent.

### Task 3 - the whole FT4 exchange on the report shape

The second decision I made for myself: **I struck unit 293's remark rather than editing
it.** `TheWholeFt4ChainRunsFromOneRightClickTests.cs:40-43` said the seeded row carried
no ratio *because every FT4 row's ratio is null*. Unit 294 made that false. Editing the
sentence would leave criterion 4 resting on a repaired excuse; striking it and putting
the report-shape proof beside it makes the criterion rest on a capture. The `RRR` leg is
kept — nothing about it broke and it is a good proof of the acknowledgement shape.

### Task 4 - the FT8 control

Run and reported faithfully rather than claimed. Details in section 3.

### Task 5 - the re-census, not dropped

Whole, not partial. Details in section 3.

### Task 6 - bookkeeping

`PHASE_OUTCOME.md`'s unit 295 entry written with the file-editing tools in the format
`outcome-entry.py` produces, saying on its own face that it was written by hand and why;
the arguments the script would have been given are committed at
`tools/arbiter/unit295-append.bat`. Units 288 to 294's entries untouched, including the
two `STATE_AFTER` verdicts each carries. `PHASE_STATUS.md`'s `WORK_INSTRUCTION:` set and
nothing else.

**Version.** The root read **`1.12.245`** when I started — I read it rather than assuming
either number, and the instruction's figure was right. Bumped once per commit: `.246`,
`.247`, `.248`, `.249`, `.250`, `.251`. **`Ft8Sharp.Deep` did not move** — no public type
changed; this unit consumed unit 294's estimator and built no DSP. **`Ft8Sharp` did not
move at all**, and no decode-path file under it was touched.

**What I ran, in full, and nothing else.** No unfiltered `dotnet test` on any project. No
suite. Every run filtered by exact test name or exact class name, foregrounded.

| Filter | Result | Time |
|---|---|---|
| `Unit295ReportLegTraceTests` (2 tests) | pass | 13 s, 37 ms |
| `Unit295TheColumnsSayWhatTheRunningModeDoesTests` (4 tests) | pass | 2.3 s |
| `TheWholeFt4ChainRunsFromOneRightClickTests` (5 tests) | pass | 21 s |
| `TheWholeChainRunsFromOneRightClickTests` (3 tests) | **1 pass, 2 fail — inherited** | 23 s |

`Ft8Unit251SnrAgreementTests` **was not run**, and that is safe rather than a judgement:
`tests/Ft8Sharp.Tests/Ft8Sharp.Tests.csproj` carries two `ProjectReference`s, `Ft8Sharp`
and `Ft8Sharp.Deep`. It does not reference `Hamlet.App`, which is the only project this
unit changed a line of. It cannot see anything I touched. Running its 2 m 27 s would have
been looking thorough rather than being it.

## 2. What the owner should expect

**What is now true.**

- Hovering the `snr` column on an FT4 row tells you FT4's measured precision — 0.58 dB on
  average and 1.41 dB at the 95th percentile over 970 messages — instead of FT8's. It
  also tells you FT4's estimator reads about 1.2 dB low on a very strong signal, plainly,
  because you should know that before you put the number on the air.
- Hovering `utc` tells you slots are fifteen seconds on FT8 and 7.5 seconds on FT4,
  instead of asserting fifteen whatever you are running.
- The signal report you send on FT4 is proved to arrive as the number your own screen
  showed you — off a real sound card, off a real loopback, decoded back as itself — and
  it reaches the log as `RST_SENT` too.

**What will look wrong but is not.**

- **The `snr` tooltip is longer and mentions FT8 while you are on FT4.** That is the shape
  I chose and the reason is in section 1. It is deliberate, not leftover text.
- **The contact-column tooltip was corrected and you will never see it.** It is bound to
  nothing. I fixed the sentence because a false sentence in the tree is false whether or
  not a screen shows it, and I did **not** add a binding, because putting a new tooltip on
  screen is a different decision and is yours.
- **Two FT8 tests are red, and they were red before tonight.** Measured, not assumed —
  section 3 says how.
- **The `snr` tooltip says "about one and a half" for FT4 and 1.41 dB in the same
  sentence.** Both are the same figure: the number is the measurement and the phrase is
  the reading guidance beside it.

## 3. What you should see

### The trace, first, because it cannot be recovered afterwards

Task 1a, driven at HEAD `efa0732`, before anything moved. **It does not stop.**

```
mode the tab runs: Ft4 on 7.50 s slots
dial             : 14080000 Hz
synthesized      : "KC3QIS W1ABC FN31" at 1240 Hz
delivered ratio  : -8.00 dB in 2500 Hz
decoded back     : "KC3QIS W1ABC FN31"

1. THE snr CELL  : "-8"
   the row reads : 142237 | -8 | 1.3 | 1240 | KC3QIS W1ABC FN31

2. THE MENU      : 6 clickable messages
     W1ABC KC3QIS FN00   grid
     W1ABC KC3QIS -08    report - the one that comes next
     W1ABC KC3QIS R-08   roger and report
     W1ABC KC3QIS RRR    acknowledge
     W1ABC KC3QIS 73     73
     Log this contact...
   HE CLICKED    : W1ABC KC3QIS -08   report - the one that comes next
   WHICH CARRIES : "W1ABC KC3QIS -08" (16 characters)

3. ARMED         : Sending to W1ABC, "W1ABC KC3QIS -08" in the slot at 17:26:15 UTC.
   COMPOSER GOT  : "W1ABC KC3QIS -08"
   grid on send  : 7.50 s slots, named FT4
   composed at   : 48000 Hz, 241920 samples

4. KEYED         : True
   came out of tx: OrdinaryUnkey
   frames        : FE FE 94 E0 1C 00 01 FD | FE FE 94 E0 1C 00 00 FD
   samples played: 241920

5. CAPTURED      : 261120 samples at 48000 Hz, peak -12.0 dBFS

6. DECODED BACK  : "W1ABC KC3QIS -08"
   WHICH HALF    : A REAL WASAPI RENDER ENDPOINT AND A REAL LOOPBACK CAPTURE
```

**The finding is that there is nothing to report as a defect, and that is a real result.**
Unit 294 built the report path carefully and it holds at every stage. What the night buys
is that criterion 4 stops resting on a test whose stated reason is false.

### The three tooltips, before and after, quoted verbatim

**`HmDecodeUtcHelp`**, `App.axaml:49` before, `:72` after. Bound at
`MainWindow.axaml:3840`.

> *before:* "When the slot started, as hhmmss. **FT8 runs on a strict fifteen-second
> grid**, so every message decoded from one slot carries the same time."

The false clause on an FT4 row is **"FT8 runs on a strict fifteen-second grid"** — wrong
in the mode and wrong in the number, in one breath.

> *after:* "When the slot started, as hhmmss. Slots run on a strict grid - fifteen seconds
> on FT8, 7.5 seconds on FT4 - so every message decoded from one slot carries the same
> time."

**`HmDecodeSnrHelp`**, `App.axaml:50` before, `:73` after. Bound at
`MainWindow.axaml:3845`.

> *before:* "...It is measured for each message from the power in the tone that was sent
> **against the seven tones that were not**. **Over 510 synthesized messages it agreed
> with the ratio actually delivered to 0.26 dB on average and 0.62 dB at the 95th
> percentile**, so it is shown in whole decibels and should not be read closer than about
> one..."

Two false clauses on an FT4 row. **"against the seven tones that were not"** — FT4 has
four tones, so three. **"Over 510 synthesized messages ... 0.26 dB ... 0.62 dB"** — that
is FT8's measurement, quoted at an FT4 operator as the precision of what he is looking at.

> *after:* "Signal-to-noise in decibels in a 2500 Hz reference bandwidth, which is how
> these modes are normally quoted. It is measured for each message from the power in the
> tone that was sent against the tones that were not - seven of them on FT8, three on FT4.
> Over 510 synthesized FT8 messages it agreed with the ratio actually delivered to 0.26 dB
> on average and 0.62 dB at the 95th percentile; over 970 FT4 messages, to 0.58 dB on
> average and 1.41 dB at the 95th percentile, and on FT4 it reads about 1.2 dB low on a
> very strong signal. It is shown in whole decibels and should not be read closer than
> about one on FT8 or about one and a half on FT4. A dash means this message's ratio could
> not be measured."

**`HmDecodeContactHelp`**, `App.axaml:59` before, `:96` after. **Bound nowhere** — the
instruction could not find it bound and told me to measure it, and the measurement agrees:
the only two files under `src/` naming any of the three keys are `App.axaml` and
`MainWindow.axaml`, and `MainWindow.axaml` names only the other two.

> *before, the string:* "Where the contact with the sender stands, **and how many slots
> ago**..."
>
> *before, the code comment above it:* "...the number beside them is a count of slots, **a
> slot being fifteen seconds.**"

The string never says how long a slot is, so it inherits whatever the reader assumes; the
comment says fifteen seconds flat, and unit 294 had already made the ledger count FT4's
slots on FT4's grid.

> *after:* "Where the contact with the sender stands, and how many slots ago - a slot
> being fifteen seconds on FT8 and 7.5 seconds on FT4. Waiting on him, your move,
> complete, or gone quiet..."

The comment above it is rewritten too, and says both grids and where they come from.

**The figures, read from the tree and not from the instruction (task 1c).** FT8's 0.26 dB
/ 0.62 dB over 510 is recorded in **`docs/unit251-snr-trace.md` §6**, in the table's own
`BOTH all` summary row, produced by **`Ft8Unit251SnrAgreementTests`**. FT4's 0.58 dB /
1.41 dB over 970 is recorded at **`src/Hamlet.RadioEngine/Audio/Ft8Reception.cs:666`**, in
`ReadFt4`'s remarks — *"the mean absolute error against the delivered ratio is **0.58 dB
and the 95th percentile 1.41 dB**"* — produced by **`Ft4Unit294SnrAgreementTests`**. Both
agree with the instruction. The tone counts are read too: `Ft4SymbolEncoder.BitsPerSymbol`
is 2, so four tones and three that were not sent; `Ft8SymbolEncoder.BitsPerSymbol` is 3,
so eight and seven.

**The pin, and the breakage it catches.**
`Unit295TheColumnsSayWhatTheRunningModeDoesTests` catches **a tooltip asserting a
measurement taken on a different mode** — which is what was in the tree tonight and what
nothing caught for three units. It reads every figure out of the file that recorded it
rather than carrying its own copies, because a test with six literals in it passes forever
while the screen and the measurement drift apart. It also asserts that neither mode's
clause carries the other's numbers, which is the same fault wearing new figures, and that
no rewritten string carries a transmission length.

### The exchange capture

`TheWholeExchangeOnTheReportShapeRunsFromTwoRightClicks`, on the same real endpoint and
real loopback.

```
---- he calls, with a grid ----
synthesized      : "KC3QIS W1ABC FN31"    delivered -8.00 dB    THE snr CELL: "-8"

---- the first right click ----
HE CLICKED       : W1ABC KC3QIS -08   report - the one that comes next
COMPOSER GOT     : "W1ABC KC3QIS -08"     grid FT4     241920 samples at 48000 Hz
came out of tx   : OrdinaryUnkey
THE WIRE         : FE FE 94 E0 1C 00 01 FD | FE FE 94 E0 1C 00 00 FD
DECODED BACK     : "W1ABC KC3QIS -08"

---- THE NUMBER ----
delivered to the receiver : -8.00 dB
the snr cell showed       : -8
the menu item carried     : -8
the decoder read back     : -8

---- the log write on that leg ----
<CALL:5>W1ABC  <STATION_CALLSIGN:6>KC3QIS  <BAND:3>20m
<MODE:4>MFSK   <SUBMODE:3>FT4            <RST_SENT:3>-08

---- he answers, and the operator clicks again ----
he sent          : "KC3QIS W1ABC R-12"
HE CLICKED       : W1ABC KC3QIS RRR   acknowledge - the one that comes next
THE WIRE         : ...01 FD | ...00 FD | ...01 FD | ...00 FD

---- the FT4 boundary nobody clicked ----
outcome          : NothingArmed     frames on wire: 4 (unchanged from 4)
```

**Two legs, two clicks, and nothing between them.** His answer only puts a row on the
table; the acknowledgement transmits because the operator right-clicked it. Nothing
composes a reply because a decode arrived. `ArmHasExactlyOneCallerInSrc` scans every `.cs`
under `src/` and finds one caller of `.Arm(`, at
`src/Hamlet.App/ViewModels/MainWindowViewModel.cs:10650`.

### The FT8 control

`TheWholeChainRunsFromOneRightClickTests`: **3 tests, 1 passed, 2 failed.**

- **Passed** — `OneClickInAHeadlessWindowMakesARealSoundOnARealCard`. The FT8
  sound-on-a-real-card control: 625920 samples off a real loopback, peak −12.0 dBFS,
  rms −15.2 dBFS.
- **Failed** — `OneRightClickDrivesTheWholeChainAndTheAudioDecodesBackAsTheClickedText`
  and `TheOperatorsStopButtonTakesARealTransmissionOffTheCardMidSlot`. Both die in the row
  finder, **before the chain begins**, with the same message: *"no realized row from W1ABC
  addressed to KC3QIS. Left rows: 0; mine rows: 1; realized grids with a row DataContext:
  0"*.

**It is inherited, and that is measured rather than argued.** This unit's only change
under `src/` is three `x:String` values in `App.axaml`. I edited those three back to
exactly what HEAD `efa0732` held, rebuilt the test project and re-ran the class: **the
identical two failures with the identical message.** I then restored them, and `git diff`
shows `App.axaml` clean against the commit.

**Where it comes from.** The finder searches the right lists — commit `4d91bfe` fixed that
— but takes `OfType<Grid>()` for the row root. Unit 280 made the *For you* side a
conversation of bubbles whose row root is a `StackPanel`, so the row is on the table
(`mine rows: 1`) and no `Grid` carries it. Unit 293 hit exactly this in the FT4 sibling and
used `OfType<Panel>()`, saying so in the file. The FT8 class never got the same fix. **It
is one identifier and I did not change it** — a decision ask is in section 4 instead.

**The FT8 tooltip control is green.** Rewriting the shared string did not cost FT8 the
precision it measured: the pin parses the FT8 clause out of the live tooltip and checks it
against `docs/unit251-snr-trace.md`'s own `BOTH all` row — 510 messages, 0.26 dB, 0.62 dB
— and separately asserts the FT8 clause carries none of FT4's figures.

### The re-census, task 5, not dropped

**Scope, narrower than unit 290's whole survey on purpose:** the operator's own path only
— what is on screen or reaches a screen. Comments are unit 290's *prose* population and
arithmetic is its *first*; counting either here would make the number incomparable with
the 15 it is being read against. Unit 290's own pattern, unchanged.

**Unit 290 found 15 on screen. Four sites stand today.**

| | Site | Verdict |
|---|---|---|
| ✓ | `src/Hamlet.App/App.axaml:72` `HmDecodeUtcHelp` | **true** - names both grids. Rewritten tonight |
| ✓ | `src/Hamlet.App/App.axaml:96` `HmDecodeContactHelp` | **true** - names both grids. Rewritten tonight |
| ✓ | `src/Hamlet.RadioEngine/Explore/ModeGuide.cs:55` | **true** - the field guide's FT8 row, "15-second warbles", describes FT8 by name; the FT4 row beside it says "half-length slots" |
| ✗ | `src/Hamlet.RadioEngine/Audio/Ft8Reception.cs:405` `Ft8Reader.NoWholeSlot` | **FALSE on FT4** |

**The one that is false, traced to the screen rather than assumed to reach one.** It says
*"there is not a whole fifteen-second slot in what was kept, so there was nothing to
decode"*. It is returned as `Ft8Reception.Refusal` at `Ft8Reception.cs:458`, assigned to
`_digitalRefusal` and `_digitalDecodeNote` at `MainWindowViewModel.cs:9170-9171`, and read
by `DigitalModeStripLine` at `:2764` and `DigitalDecodedSummary` at `:2783`. The cut above
it has been `Ft8SlotCutter.Cut(..., mode.Grid())` since unit 292 — so on FT4 it is 7.5 s
slots that were not found, and fifteen seconds that the operator is told about. Unit 290
task 7 fixed the cutter's own two sentences and missed the one in the file above them.
**Named, not repaired** — task 5 is a count, and step 2's criterion 4 closes by naming.

**The widening unit 290's pattern would have missed**, run as well: any literal carrying a
fifteen figure, because the capture sheet once wrote `15.00 s slots`. It finds nothing new
— the sheet has formatted the length off the grid since unit 292.

Three commands committed at `tools/unit295-census15.sh`. This session's shell refused to
execute the file; each was run individually and the file records that on its own face.

### Mismatches against the instruction, and the validator

- **The root version read `1.12.245`** when I started, which is the figure the instruction
  gave. Read, not assumed.
- **The three tooltip line numbers `:49`, `:50` and `:59` all held.**
- **`HmDecodeContactHelp` is bound nowhere**, as the instruction suspected. Rewritten
  anyway; no binding added.
- **`PHASE_STATUS.md`'s `CURRENT_STEP:` reads `1` while the phase is on step 4.** Reported
  for the **sixth** time and not repaired. It is the launcher's.
- **`RULES_AT`**: the reload calls `HM-DEC-160` against `CPS-DEC-0160`. Not re-measured for
  a seventh time — units 289 to 294 each measured the three files and found them agreeing.
  **It is the reload's own** and there is nothing here to repair.
- **The untracked files are still there, and there are now five.** Section 4, item 3.

**The validator.** `dotnet build tools/arbiter/validate-output.proj`. Its first run
against this report **exited 1**, naming two failures — *rule 1, no `UNIT:` line above
section 1*, and *rule 6, no ordering block above the `UNIT:` line* — because the ordering
block had been written with `A —`, `B —`, `C —` headings and the `UNIT:` block had been
placed after it in a fenced code block, past the sixty-line window both rules read. The
block was rewritten to the shape the script requires and it was re-run. **The seven rules
it checks, all of which it reports on:** 1 a `UNIT:` line above section 1, parseable; 2 the
four top-level sections, in order, with exact names; 3 no fifth top-level section; 4
section 4 present even when empty; 5 section 3 non-empty; 6 the ordering block above the
`UNIT:` line, with an `A.`, a `B.`, a `C.`, and `C` naming how many items section 4 raises;
7 no placeholder token in the header block. **Final result: `validate-output exit 0`, all
seven rules ok.**

## 4. What's blocking us

**Four items. None is in the way of a step 4 exit criterion, and the ordering block says
so item by item.** Two want a ruling; two are recorded for the record and are labelled as
such rather than dressed as ruling requests.

### 1. May a unit fix the FT8 whole-chain test's row finder? — wants a ruling

**Ruling wanted:** whether `TheWholeChainRunsFromOneRightClickTests`'s `OfType<Grid>()` row
finder may be changed to `OfType<Panel>()`, and whether these two reds are among the four
inherited reds that are with you.

**Reasoning.** The FT8 whole-chain proof is the control this phase reads *FT4 unchanged
FT8* against, and two of its three tests cannot reach the chain at all: they fail in the
row finder because unit 280 made the *For you* row root a `StackPanel` and the finder still
looks for a `Grid`. Unit 293 made exactly this fix in the FT4 sibling and wrote the reason
into the file. It is one identifier. I did not make it because this unit was told not to
widen and not to chase inherited reds, and because I cannot tell from here whether these
two are among the four you already hold.

**What I rejected and why.** Making the change quietly: it would have turned a red I
inherited into a green I appeared to have earned, in the one test that exists to keep this
phase honest about FT8. Leaving it unreported: the next unit would meet the same wall and
spend the same measurement.

### 2. May the on-screen refusal sentence be corrected? — wants a ruling

**Ruling wanted:** whether `Ft8Reader.NoWholeSlot` at
`src/Hamlet.RadioEngine/Audio/Ft8Reception.cs:405` may be reworded so it stops telling an
FT4 operator that no whole **fifteen-second** slot was found.

**Reasoning.** It is a §0.0 fault on the operator's own path — the sentence reaches
`DigitalModeStripLine` and `DigitalDecodedSummary` — and it is the same class of fault as
the three tooltips this unit fixed. It is also a `public const` in the engine rather than a
wording-only change, and task 5 was a census rather than a repair, so I named it with its
line number instead of editing it. Unit 290 fixed the two sentences below it in the cutter
and missed this one; five units have passed over it since.

**What I rejected and why.** Fixing it under task 5's cover: a census that quietly repairs
what it counts is no longer a census, and the count is what step 2's criterion 4 asked for.

### 3. Five untracked files, and a seventh session has not been able to remove them — for the record, not a ruling request

`.unit290-commit.txt`, `SESSION.lock`, `tests/Ft8Sharp.Tests/Unit289SourceProbe.cs`,
`tools/census15.sh`, `tools/unit294-thread-grid.py` — and `.unit295-msg.txt`, which this
session created to carry commit messages because the shell will not take a quoted heredoc
containing an apostrophe. **No attempt was made to remove any of the carried four**, per
the instruction; the one incidental measurement is that `rm` on this session's own file
inside the working directory was refused by the harness, which is the same refusal units
289 to 294 recorded. The working tree still is not what a fresh clone builds. Recorded so
the count is accurate; it is not a ruling request.

### 4. `CURRENT_STEP:` and `RULES_AT` — for the record, not a ruling request

`PHASE_STATUS.md`'s `CURRENT_STEP:` reads `1` while the phase is on step 4; this is the
sixth report of it and it was not repaired. The reload's `HM-DEC-160` against
`CPS-DEC-0160` disagreement was not re-measured for a seventh time — units 289 to 294 each
found the three files in the tree agreeing. Both belong to the launcher. Recorded, not
asked.
