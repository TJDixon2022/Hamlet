```
READ IN THIS ORDER.

A. The phase goal - Hamlet holds what it has. Step 0 done (0.1 ruled met on the
   negative, work instruction 372 section 6, author's). Step 1 partial. Steps 2,
   3, 4, 5 not started.
B. Step 1 - Hamlet opens whole. 1.1 met - already in the tree, confirmed by
   measurement at all nine sizes. 1.2 met - by this unit, and no source file
   changed. 1.3 partial - measured, the panels draw 0 px at 900x620 and the
   repair is a canvas redesign. 1.4 not met - on an inherited red that is not
   this unit's. All four must-pass.
C. The report last. Section 4 raises 7 items on top of the carried nine (eleven
   less the two section 6 answered), and items 1 and 2 are in the way of a
   criterion in B - item 1 is 1.3, item 2 is 1.4. The other five are findings.
```

```
UNIT:       372 - complete at task 4 of 5 - 2026-09-20 20:21
PHASE GOAL: Bank what Hamlet already has. Five steps of screen, record and test
            work that needs neither the radio nor the owner, run unattended.
UNIT GOAL:  Prove by measuring, not by describing, that Hamlet opens whole and
            stays whole as the window shrinks - the working panels give up
            height first, then the top row, and the send area never leaves.
ADVANCED:   step 1, criteria 1.1 and 1.2 met; 1.3 measured to partial; 1.4
            measured to not met on a red this unit did not cause
NUMBER:     the heights at which Hamlet was measured: nine sizes -> nine sizes
            plus a sweep from 780 to 500, on two windows
DRIFT:      none
```

## 1. What Claude did

**Complete, at task 4 of 5** - tasks 0 through 4, each committed on its own, on `main`, at
`C:\Source\HamLet`. The project gate passed against the tree: `SHACK_FACTS.md` and
`src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs` both present, no `CoreHMI.sln`, no
`MURC.sln`. **Nothing was dropped.** The drop candidate task 4 named - task 1's three repeat runs
of `TheStopIsAlwaysOnScreenTests` - was run and measured. The drop candidate task 3 named -
**that task's repair, not its measurement** - was taken, deliberately and with numbers, and that is
item 1 of section 4.

**Task 0.** Version 1.13.58 -> 1.13.59 with its line in the log; `UNIT 372 - STEP 1` appended to
`PHASE_OUTCOME.md`. The carry-forward list ran before anything changed, both invocations, one build
each, status written immediately before each: **app 206 of 206, engine 146 of 146, both green, no
red to name.**

**Task 1 - the trace, which built nothing.** `Unit372TraceTests.Unit372TraceTheWindowAsItShrinks`
prints unit 354's nine sizes and then a descending sweep at width 1100 - 780, 740, 700, 660, 620,
580, 540, 500 - on two windows: the pinned-facts window and the one that has content in its panels.
It asserts nothing about new behavior. Four things came out of it, all in section 3's table.

**Task 2 - criterion 1.2.** `TheWindowGivesUpHeightInOneOrderTests`, four names, **4 of 4 green**,
and **no source file changed**. The rule was already in the tree and in unit 356's comment above
`TopRow`; what was missing was anything measuring it, which is precisely the state R34 was ruled
about. *The instruction asked for the test to be watched failing in a worktree at task 0's commit.*
**This session was not permitted to create a worktree**, so the same thing was established the way a
worktree would have: `git diff d14badb1 -- src/` is **empty**, so those four names ran against task
0's application byte for byte. They were green on it. Nothing was watched failing because there was
nothing failing to watch.

**Task 3 - criterion 1.3.** `TheWindowHoldsBelowItsMinimumTests`, three names, watched the same way
and **red on one of them then and now**. Two pass: the send area stays put and keeps its 22 px at
every small size, and the header and the status bar are pinned with no scrolling ancestor at all
(HM-DEC-051). One fails, and it is the criterion: **all three working panels draw 0 px** at
900 x 620 and at every height from 700 down, with their content still present and unreachable.
**The test was kept red and was not loosened** (`PHASE_PLAN.md` §6). The repair was measured and not
made - section 4 item 1 has the numbers and the reason.

**Task 4 - criterion 1.4 and the list.** `BindingHealthTests` 1 of 1; `TheWorkingPanelsTests` 8 of 8;
`TheStopIsAlwaysOnScreenTests` 5 of 5; **`TheTopRowTests` 14 of 15**. Carry-forward after the last
change: **app 206 of 206, engine 146 of 146** - name for name identical to task 0's run. **No
regression.** Nothing was added to `docs\carry-forward-tests.txt`, which is the default that task 4
asks be stated: this unit's three types are measurements of one step's criteria and not guards on a
rule the whole product depends on, the app invocation already runs about two and a half minutes, and
one of the three is knowingly red, which that file must never carry.

**Mismatches found against §5's reading of the tree, reported and not repaired.** Every line in §5
held except one: `x:Name="TopRow"` is at **line 2956**, not 2957. Lines 12, 13, 2658, 2896, 3385,
3394, 3464, 3480, 3745, 3818, 3104 and 6640 were each read and each correct, and **no `ScrollViewer`
stands on root row 1** - confirmed, and 1.3 does turn on it. `TheStopIsAlwaysOnScreenTests` is 499
lines with the three names named. **The two things §5 said were not mine to repair**: `RULES_AT`
reads `HM-DEC-165 (2026-09-19)` while `CLAUDE.md` §1 holds `CPS-DEC-0165`, carried and untouched;
and `WORK_INSTRUCTIONS.md` and `output.md` were indeed deleted and `RUN_LEDGER.md` modified in the
working tree against `723142cb` when this session opened. **And one red that is not this unit's**:
`TheTopRowTests.TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow`, which is why 1.4 is not
met - section 4 item 2.

## 2. What the owner should expect

When you drag the Hamlet window smaller, the **CQ button, the mode tabs, the area they sit in, the
drive note and Stop stay exactly where they are and exactly the size they are** - the send area is
22 px tall at every height from 780 down to 620 and every one of those five controls is inside the
window at every one of them. What gives way instead is the three working panels - waterfall, Decoded
text and For you - which shrink first while the top strip with the radio's face keeps its full
height. **That much is now asserted rather than described**, and it is the fault of 2026-09-14
closed: CQ can no longer end up drawn below the bottom of the window. **What will look wrong, and
is**: drag the window down to about 700 px tall or open it at its smallest size of 900 x 620 and
**the three panels disappear completely** - not shrunk, not scrolled, gone, with the decoded rows
and the conversation cards still in there and no way to reach them. That is unchanged from before
this unit; what is new is that it is measured, and that a test in the tree now fails until it is
fixed. Hamlet will not let itself be made shorter than 620, so 620 is as bad as it gets.
**Every appearance claim here is computed, not seen** (FACT-004): these are layout rectangles read
off a headless window, nothing was painted and nobody looked at a screen.

## 3. What you should see

**The descending sweep at width 1100**, one row per height. `asked` is the height the window was
told to be and `drawn` is the height it took; `panelRow` is the height the three side-by-side
working panels share and `x3` is them added up; `sendArea` is `DigitalSendReserved`. The five
controls are CQ, the mode tabs, the reserved send area, the drive note and Stop.

The pinned-facts window:

| asked | drawn | TopRow | panelRow | x3 | sendArea | the five controls |
|---|---|---|---|---|---|---|
| 780 | 780 | 300 | 71 | 213 | 22 | all five whole |
| 740 | 740 | 300 | 31 | 93 | 22 | all five whole |
| 700 | 700 | 300 | 0 | 0 | 22 | all five whole |
| 660 | 660 | 300 | 0 | 0 | 22 | all five whole |
| 620 | 620 | 300 | 0 | 0 | 22 | all five whole |
| 580 | **620** | 300 | 0 | 0 | 22 | all five whole |
| 540 | **620** | 300 | 0 | 0 | 22 | all five whole |
| 500 | **620** | 300 | 0 | 0 | 22 | all five whole |

The window with content in its panels:

| asked | drawn | TopRow | panelRow | x3 | sendArea | the five controls |
|---|---|---|---|---|---|---|
| 780 | 780 | 300 | 73 | 219 | 22 | all five whole |
| 740 | 740 | 300 | 33 | 99 | 22 | all five whole |
| 700 | 700 | 300 | 0 | 0 | 22 | all five whole |
| 660 | 660 | 300 | 0 | 0 | 22 | all five whole |
| 620 | 620 | 300 | 0 | 0 | 22 | all five whole |
| 580 | **620** | 300 | 0 | 0 | 22 | all five whole |
| 540 | **620** | 300 | 0 | 0 | 22 | all five whole |
| 500 | **620** | 300 | 0 | 0 | 22 | all five whole |

**Read four things off that table.** The `sendArea` column never moves - 22 px at every height on
both windows, which is what *never the thing that leaves* is as a number. The `TopRow` column never
moves either - 300 px, unit 356's cap, at every height, and it never passes it. The `panelRow`
column gives up everything, and reaches **zero at 700** and stays there. And the `drawn` column is
not the `asked` column below 620: **`MinHeight` binds even on a headless window**, so 580, 540 and
500 are one size - 1100 x 620 - reached three ways.

**At the nine sizes, which is criterion 1.1.** All five controls whole on the window at every one of
1920x1040, 900x620, 1100x780, 1280x720, 1366x728, 1536x824, 1400x1040, 1920x1017 and 2560x1400.
`TopRow` drew 198, 293, 300, 278, 250, 204, 224, 198 and 198 - under or at its cap everywhere. **The
panel row drew 450, 0, 71, 50, 86, 228, 424, 427 and 827**, and the zero is 900 x 620, the smallest
size Hamlet opens at.

**What the panels are hiding when they are at zero**, at 1100 x 620: the Decoded text scroller reads
**extent 36 in a viewport of 0**, and For you **extent 360 in a viewport of 0**. The content is
there. Nothing can reach it. That is hiding information rather than detail, which §0.5 forbids.

**The test counts.**

| type | count | |
|---|---|---|
| `Unit372TraceTests` | 1 of 1 | task 1, asserts nothing, not on the list |
| `TheWindowGivesUpHeightInOneOrderTests` | **4 of 4** | criterion 1.2, green on task 0's tree too |
| `TheWindowHoldsBelowItsMinimumTests` | **2 of 3** | criterion 1.3, red on the criterion, kept red |
| `BindingHealthTests` | **1 of 1** | criterion 1.4 |
| `TheWorkingPanelsTests` | **8 of 8** | criterion 1.4 |
| `TheStopIsAlwaysOnScreenTests` | **5 of 5** | criterion 1.4 |
| `TheTopRowTests` | **14 of 15** | criterion 1.4 - one inherited red, section 4 item 2 |

**The carry-forward counts, before and after.**

| | app | engine |
|---|---|---|
| before any change (task 0) | **206 of 206** | **146 of 146** |
| after the last change (task 4) | **206 of 206** | **146 of 146** |

**Name for name identical. No red after that was green before, so no regression** (HM-DEC-165).
Both runs were two invocations with one build each, foregrounded, with the status file written
immediately before each (HM-DEC-155).

## 4. What's blocking us

**Nothing blocks the phase.** Seven items from this unit, most-blocking first; two of them are in
the way of a criterion. Then the carried queue.

### Raised by this unit

**1. Criterion 1.3 cannot be met without redesigning how root row 1 allocates height, and that is
past a layout number.**

*Ruling wanted. This is the one thing standing between step 1 and done.* The measurement, at
1100 x 620, the worst size that can be reached: the workspace region is given **51 px** and needs
**138 px** before the panels get their first pixel - 26 px of the boundary's border and padding, and
112 px of the mode strip and the row beneath it. The deficit is **87 px**. At width 1100 the panel
row's height is exactly *window height minus 707*, which reads 33 at 740, 0 at 700 and 0 at 620.
**The only pool of height above the panels is `TopRow`'s 300 px**, and the radio's own face inside it
measures only **110 px**, so some of it genuinely could be taken without clipping the picture.
*What makes this more than a number*: taking the 87 px of deficit plus any usable viewport means
taking about **147 px**, and the grid would have to start taking it at **767 px of window height and
below - which includes 1280 x 720 and 1366 x 728, two of unit 354's nine sizes where the layout is
sound today** and where unit 356 measured its cap as inert. There is no way to say *keep the panels
at a floor and let the top row pay for it* in this grid, because the rest of the canvas varies with
width as well as height. **So the repair changes the screen at sizes that are right today, and the
number it needs is not available without one.** *Rejected*: capping `TopRow` off a window-height
converter, which buys 102 px at 1100 - not enough for a viewport - and misfires at other widths;
and putting a `ScrollViewer` on root row 1, which would put the send area inside it and is the exact
thing R34 forbids. **Nothing was changed on the send area's row and nothing was loosened.**

**2. Criterion 1.4 is not met on a red this unit did not cause and did not touch.**

*A finding, and it is in the way of 1.4.*
`TheTopRowTests.TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow` fails - `Assert.Single()
Failure: The collection was empty`, `TheTopRowTests.cs` line 867, printing *at 1920.0 best bet 40 m:
pills wearing the badge []*. It fails run after run and it fails alone, so it is not interference
from this unit's new files. **`git diff 723142cb -- src/ tests/.../TheTopRowTests.cs` is empty**:
the application and that test file are byte-identical to unit 371's tip. It is about the best-bet
pill and has nothing to do with height. It is **not** on the carry-forward list, which is why the
list is green while 1.4 is not. *Not repaired* - §5 says repair nothing but this unit's.

**3. `MinHeight` binds on a headless window, so three of task 3's four sizes are one size.**

*A finding, and the instruction asked to be told either way.* Asked for 1100 x 580, 1100 x 540 and
1100 x 500, the window drew **1100 x 620** every time. So the three sizes below the minimum are one
size reached three ways, and `TheWindowHoldsBelowItsMinimumTests` says so rather than reporting four
readings. **The consequence is good news**: the worst case Tim can reach by dragging is 620, and it
is the case that was measured.

**4. Step 2's flake is measured, named, and not chased.**

*A finding, recorded for the unit that does step 2.*
`TheStopIsAlwaysOnScreenTests.KeyedAtTheOpeningSizeAClickOnTheBarFiresTheAbortWhileItRuns` was red
in **three of seven** runs of its type and **two of seven** runs of the name alone. It is the only
one of the type's five names that was ever red, and it was green in both carry-forward runs. It
always fails the same way: the abort pair is on the wire **twice** - expected key-on, abort, PTT off
and got those plus a second abort and a second PTT off. **The numbers are in
`docs\unit372-flake-measurement.md`** rather than only here, because this file is overwritten and
step 2 would otherwise inherit a rumor. Which of the click's own `StopNow` and the sequence's unkey
wrote the second pair **was not determined**, because the instruction said record and do not chase,
and choosing between them would be a guess presented as a finding (§R14).

**5. A worktree could not be created in this session, and two tasks asked for one.**

*A mismatch with the tool facts, reported for the next order.* `git worktree add` was refused both
outside and inside the repository root. Tasks 2 and 3 both say *watched failing first ... copy it
into a worktree there*. What was done instead, and it is equivalent: `git diff d14badb1 -- src/` is
**empty**, so both types ran against task 0's application byte for byte, and everything this unit
had added was a test. **Task 3's red was watched on that tree; task 2's four names were green on
it.** Worth knowing before the next order writes a worktree into a task.

**6. Two tool facts corrected, and §5 has one line number off by one.**

*Findings.* `x:Name="TopRow"` is at **line 2956** of `src\Hamlet.App\Views\MainWindow.axaml`, not
2957; every other line §5 named was correct. And two things this session could not do that the order
did not warn about: **shell output redirection (`>`) is refused to every path**, including inside the
repository and the scratchpad, so files are written with the editor rather than a heredoc; and a
compound command with `;` or a second operation is refused, so loops go into a script file first.
*Python was not needed and was not used.*

**7. The live `PHASE_PLAN.md`'s criterion boxes are never ticked, by any unit.**

*A finding, reported and not repaired.* All four of step 1's boxes are still `- [ ]`, and so are all
five of step 0's, which unit 369 met four of. The state that is actually maintained is in
`PHASE_STATUS.md` and `PHASE_OUTCOME.md`, and this unit followed that convention rather than
starting a second record. **It is the same drift unit 369 raised about the archived Olivia plan**,
one phase earlier. *Ruling wanted only if the boxes are meant to be the record* - if they are, a
unit should be told to tick them.

### Asks still outstanding - the carried queue, per HM-DEC-139

**Two came off the queue and are not reproduced below**: unit 369's item 1 (whether a fully searched
negative satisfies criterion 0.1 - **yes**, and step 0 is done) and unit 371's item 1 (whether the CQ
filter should hide a PSK31 row addressed to somebody else - **R9 stands, it should not**). Both were
answered in work instruction 372 §6, author's and overrulable. **The remaining nine are verbatim
below and this unit answers none of them.**

#### Carried from unit 371's section 4

**2. A guessed answer with no speaker still opens no card.**

*A finding.* The card is keyed by station, so a parse Hamlet cannot put a name to opens nothing -
its row is on his side and marked a guess, which is where it was before. In the record, the parser
named the speaker; had it not, the card would still not appear.

**3. The turn indicator keeps its words rather than a question mark.**

*A finding, the author's call under the decision block.* The card says *Your turn, a guess*; the
order suggested *his turn?*. In grayscale a word survives and a question mark is easy to miss
(§0.6), and the word was already in the tree from unit 319.

**4. Six tests asserted the shut door, two of them on the carry-forward list.**

*A finding about coverage, not a defect.* The door was guarded in six places, which is why the
middle carry-forward run was 204 of 206. All six now guard the rule, each in a §R12 commit.

**5. `psk31_answer_taken` fires once per station per session, not per line.**

*A finding.* A station who answers, goes, and answers again writes one line. The card's own
history is what carries the rest, and `psk31_line_parsed` already writes every line.

**6. Python runs here, contrary to the order's tool facts.**

*A mismatch, reported for the next order.* Scripts written to the scratchpad and run as
`python file.py` worked throughout, as in units 361 and 362.

#### Carried from unit 369's section 4

**2. The `no_transmit_device` refusal was never wrong about the device — the
refusal Tim actually saw was `transmit_device_would_not_open`, and the split above
assumes his device id was still in the file at that moment.** If instead the file
had been reset to defaults, he would have seen `no_transmit_device`, and unit 362's
report says he saw the other. *That means the device id survived and the device
genuinely would not open* — which is a different fault from the settings loss, and
this unit repaired both without proving which one he hit. *Ruling wanted:* whether
that matters enough to chase. His telemetry from 2026-09-14 to 09-19 would settle
it in one read; nothing in this repository has it.

**3. `LearnedAlcReference.Ago()` counts only in seconds and whole minutes.** Now
that the reference survives a restart, a legitimate value is *4320 minutes ago*.
Honest but poor. *Ruling wanted:* whether to extend it to hours and days. *Not done
here* because `TheAlcSentenceTests` asserts the current forms and §10 said not to
touch wording beyond the refusal's.

**4. The archived Olivia plan's checkboxes say 25 of 40, not 38 of 40.**
`docs/phase-olivia-run/PHASE_PLAN.md` has 25 boxes ticked and 15 open, including
several the unit reports say were met (1.5, 4.1–4.8, 5.1–5.3). HM-DEC-166 records
38 of 40 as ruled, which comes from the reports. *Reported, not repaired* — §5 says
repair nothing but this unit's, and an archived phase's plan is not this unit's.

**5. No settings file in the tree was ever reconstructed before this unit, and the
three fixtures are mine.** They carry the right key set, but their *values* are
invented — nobody's real 1.13.30 file was available. They prove the shape loads,
not that Tim's particular file does. *Raised once, not a blocker.*
