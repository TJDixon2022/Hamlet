```
READ IN THIS ORDER.

A. The phase goal - Hamlet holds what it has. Step 0 partial, step 1 done, step 2
   partial on 2.4 alone and deferred by R43, step 6 done by R42, step 7 done by unit
   378, steps 3, 4 and 5 not started with step 3 waiting on this one. This is the
   first unit ever spent on step 8.
B. Step 8 - keyboard modes earn achievements. 8.1 MET: 8 of the eight kinds measured
   for PSK31 and for Olivia against FT8 from a fixture log, 6 cells differed and ALL
   SIX WERE ABOVE FT8 WITH NONE SHORT. 8.2 MET: a logged PSK31 contact earns 46 pts
   and a logged Olivia contact earns 46 pts, against the FT8 contact's own 31 - the
   seven mode-independent kinds equal cell for cell, the Hall of Fame a superset by
   exactly the mode's own first. 8.3 MET: the quill WAS ALREADY THE SAME on both
   keyboard row kinds and is now asserted, and the CQ list's Olivia label reads
   "Olivia" where it read "PSK31". 8.4 MET on BOTH contacts: the total moved
   "Total 0 pts · Rank 1 · 25 to Rank 2" -> "Total 46 pts · Rank 2 · 54 to Rank 3" on
   the PSK31 contact and identically on the Olivia contact, TheAchievementsPageTests
   12 of 12. The drop candidate was NOT taken.
C. The report last. Section 4 raises 7 items on top of the carried queue, and none of
   them is in the way of a criterion in B. Unit 378's item 2 came off the queue -
   R43 had already ruled it - and its items 1, 3 and 4 stay on. The instruction calls
   the carried queue twenty-one and its own enumeration lists TWENTY-TWO; all
   twenty-two are carried below and the discrepancy is item 6.
```

```
UNIT:       379 - complete at task 4 of 5 - 2026-09-21 15:53
PHASE GOAL: Hamlet keeps what the PSK31 and Olivia threads already earned it - the
            screen, record and test work that needs neither the radio nor the owner,
            judged by tests that ran and, at the end, by Tim at his window.
UNIT GOAL:  A station Tim works on PSK31 or on Olivia earns him exactly what the same
            station worked on FT8 would earn him - the Modes badge, the Hall of Fame
            first, the country, state, grid, continent and miles, the quill on the
            row and the scores and total on the achievements page. Measured in three
            columns FIRST, and connected only where a column was short.
ADVANCED:   step 8, criteria 8.1, 8.2, 8.3 and 8.4 - the whole step, first unit spent
            on it; it also clears step 3's entry, which reads step 8 done
NUMBER:     of the eight achievement kinds, those a PSK31 contact reaches: 8 of 8 ->
            8 of 8; and an Olivia contact: 8 of 8 -> 8 of 8. THE TABLE WAS ALREADY AT
            PARITY AND THAT IS THE UNIT'S CENTRAL FINDING. What moved is not in it:
            of the three achievement-screen facts an Olivia contact got wrong, 3 were
            wrong before and 0 are wrong after
DRIFT:      0 consecutive units without advance  (was 0)
```

## 1. What Claude did

**Complete, at task 4 of 5 (tasks 0 to 4), on QUIVERFULL, project gate `PROJECT: Hamlet`
verified against the tree - `SHACK_FACTS.md` and
`src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs` present, no `CoreHMI.sln`, no
`MURC.sln`, root `C:\Source\HamLet`, solution `Hamlet.sln` - on branch `main`. Nothing was
dropped: 8.4's named drop candidate, the Olivia half of the before-and-after on the page,
was not taken.**

**Task 0 - the record and the entry run.** `PHASE_STATUS.md` `CURRENT_STEP` 0 -> 8 and
`WORK_INSTRUCTION` 378 -> 379, both stale. **Step 7 transcribed as done** in
`PHASE_STATUS.md` and `PHASE_OUTCOME.md` from unit 378's five met criteria and the judging
session's `STATE_AFTER: done` - transcription of a verdict a separate session already
returned, not a judgment of mine and not one of this unit's two rulings. **The two step 6
headers were checked and they agree** at `done`, so the disagreement the reload named is
already closed by unit 377's transcription of R42 and is not re-argued. Version 1.13.65 ->
1.13.66 with its line in the version log. Carry-forward before any change, both invocations,
one build each, status written immediately before each: **app 214 of 214 on the second
attempt**, the first 213 of 214 with `TheStopIsAlwaysOnScreenTests.AtEachOf354sNineSizes
StopIsInTheStatusBarAndOnTheWindow` dying at 1 ms of the headless dispatcher loop - unit
375's item 3, a third name it had not landed on, re-run once and recorded; **engine 150 of
150** first attempt. And the six achievement types by name outside the list: **43 of 45, both
reds inherited**, which is the whole reason that run exists.

**Task 1 - the trace, and this task is criterion 8.1.** One `[Fact]`, `Unit379Trace`. It
built nothing, changed no source file and asserted nothing about the product. One contact
three times over - the same callsign, grid, band, dial and times, differing **only** in the
mode pair - read in three columns across seven record facts, all eight achievement kinds,
the Hall of Fame keys, the scores, the total, the record Hamlet itself builds, the Modes and
Hall of Fame cards, the quill on three row kinds, the CQ list and the page's totals. **It
found the eight kinds already at parity with zero cells short of FT8, the quill already
identical, and three things genuinely wrong - all three Olivia's, none of them visible in the
eight-kind table.**

**One thing had to be fixed in the measurement itself before it measured anything.** The
station was first `VK3ABC`, and `DxccPrefixes.EntityOf` **declined it** - `VK` is claimed by
Australia, Heard I. and Lord Howe I., and the table returns nothing rather than guessing
which. That silenced the countries and continents columns and made the quill read `None` on
all three rows, which would have been a vacuous pass. The station moved to `YB1ABC` -
Indonesia, one entity - and the measurement was re-taken. **The decline is the table obeying
§0.0 and it is reported, not repaired.**

**Task 2 - 8.2, two marked cells closed, one commit each, and then the assertion.** Both
cells were Olivia's and both were on the Hall of Fame card, where the eight-kind table could
not see them. (1) `AchievementCategory.EarnerOf` had rows for `first_psk31` and
`first_cw_qso` and **none for `first_olivia`**, so the card was earned and scored at its full
15 points with nobody's name on it. (2) `AchievementCategory.NextFirst` had the same hole, so
the unearned card read **"Any station at all"**. Each repair is one case at the one place the
fact is already derived; no second scorer, no second log reader, no mode branch anywhere else
on the screen, and `LivesAt` already knew Olivia's calling spot from unit 358 so nothing new
was built for it. Then `TheKeyboardModesEarnWhatFt8EarnsTests`, five names, **5 of 5** - the
two contacts driven all the way through `ContactLogEntryForStation`, `ContactLogStore.Append`
and the refresh a press of Log runs, then scored **against the FT8 contact's own numbers and
never against a number typed in the test.**

**Task 3 - 8.3 and 8.4.** The CQ label repaired at `CqSnapshot`, one site, from the row's own
variant. `TheQuillAndThePageMeanTheSameTests`, four names, **4 of 4**. **8.4 is met on both
contacts and the drop candidate was not needed.**

**Task 4 - the exit run.** **App 216 of 216 in 2 m 22 s, green on the first attempt; engine
150 of 150 in 5 m 1 s, green on the first attempt.** Name for name against task 0's 214 and
150, the only differences this unit's own two guards. **No regression, nothing red that was
green before** (HM-DEC-165). The six achievement types re-run in the same filtered shape:
**43 of 45, identical to task 0**, the same two inherited reds, and the clipping one fails
with the same message character for character. Two names added to
`docs\carry-forward-tests.txt` with their paragraphs and the human-readable list changed to
match.

**Decisions I made for myself, reproduced in full.** Both of the unit's two rulings were
spent in the work instruction, so I made none. Three judgment calls inside them:

1. **A cell where a keyboard mode reaches *more* than FT8 is not a gap and was not levelled
   down.** Six cells differed and all six were above FT8. Ruling 1 item 3 makes only a
   *short* cell the unit's, and R14 forbids rebuilding what the tree meets, so the six were
   reported with their cause - the points file carries `first_psk31` and `first_olivia` and no
   `first_ft8` at all - and nothing was changed to make FT8 catch up. **Levelling PSK31 and
   Olivia down to FT8 would have satisfied "exactly as" and taken 15 points off him.**
2. **The Hall of Fame card counts as a per-contact record 8.2 names.** 8.2's words are *the
   Hall of Fame first for that mode, and every per-contact record the log can answer*. The
   card's callsign, grid, band-and-mode line and date are exactly those records, so the two
   empty cards were treated as marked cells and closed. They do not appear in item 2's table
   because the *score* was right and only the card was wrong.
3. **The CQ label is named from the row's `Variant`, which leaves one case it cannot answer,
   and I reported it rather than putting a second mode fact on the row.** Ruling 2 item 2 says
   the label comes from *the one fact the row already carries*; that is `Variant`. An Olivia
   channel opened with no readable variant still reads `PSK31`. It is strictly narrower than
   what it replaced - every Olivia row used to read `PSK31` - and it is item 1 in section 4.

**Nothing on a send path was touched, and no engine file at all.** Both repairs are single
sites in `src\Hamlet.App\ViewModels`: `AchievementCategory.cs` and `CqSnapshot.cs`. No
composer, no `Arm`, no `PttOn`, no cap, no RSID burst, no Olivia variant gate, and nothing
under `src\Hamlet.RadioEngine\Olivia\`, `\Psk31\`, `\Rsid\` or `\Transmit\` was opened. **No
stop in `PHASE_PLAN.md` section 6 was reached.**

## 2. What the owner should expect

Tim, when you work a man on PSK31 or on Olivia tonight and press Log, that evening now counts
for exactly what the same evening on FT8 would have counted for - **and most of it already
did, which nobody had ever checked.** The country, the continent, his grid, the band and the
miles have been landing on your board for a keyboard contact since the units that put the mode
on the record itself; so has the Modes badge. Measured side by side tonight from one contact
built three times over, a PSK31 contact and an Olivia contact each score **46 points where the
same man worked on FT8 scores 31** - and the 15-point difference is in *their* favour, because
your points file has a first-PSK31 card and a first-Olivia card and no first-FT8 card at all.
That is your file's judgement and nothing here touched it. The quill on a keyboard-mode row
was already the same feather with the same words as on an FT8 row, and there is now a test
that says so. **Three things were genuinely wrong and all three were Olivia's.** The
achievements page told you a station calling CQ on Olivia was calling on **PSK31** - it has
said that since the unit that built the CQ list, which was written before Olivia existed
here - and it now says Olivia, while an FT8-shaped row goes on saying nothing at all because
it cannot tell FT8 from FT4. Your Olivia first in the Hall of Fame was earned and paid at full
value **with nobody's name on it**: no callsign, no grid, no date, no `20 m · Olivia` line,
where the PSK31 card carried all four. It now carries them. And the Olivia card, before you
earn it, told you that working **"any station at all"** would earn it - which is not true and
you could have acted on it; it now tells you where Olivia lives, from the same table the
Olivia tab reads. **What will look wrong but is not:** the Olivia line reads `3.582 on 80m`
where the PSK31 one reads `3.580 on 80 m` - the missing space is the Olivia calling file's own
spelling of the band and it is the same string the Modes badge has shown since unit 358, so
Hamlet is at least consistent with itself; and states and total miles do not move for a single
short contact in **any** mode, FT8 included, because Hamlet writes no STATE for any mode and
358 miles does not reach the first mileage tier. **Nothing was invented to make a number
move**: a record Hamlet cannot answer is still absent, for these two modes exactly as for FT8.
Every figure in this report was computed on the development machine (FACT-004) - **no port was
opened, no device was enumerated and nothing was keyed.**

## 3. What you should see

**The eight kinds, FT8 beside PSK31 beside Olivia. One contact - `YB1ABC` at `OI33`, 20 m,
from `FN00DJ` - built three times over, differing only in the mode pair.** This table is what
makes 8.1 a measurement and 8.2 a comparison.

| Kind | FT8 | PSK31 | Olivia | |
|---|---|---|---|---|
| `hall_of_fame` | 3 | **4** | **4** | MARKED, **above** FT8 |
| `continents` | 1 | 1 | 1 | parity |
| `countries` | 1 | 1 | 1 | parity |
| `states` | 0 | 0 | 0 | parity, **all three nothing** |
| `grids` | 1 | 1 | 1 | parity |
| `total_miles` | 10058 | 10058 | 10058 | parity |
| `bands` | 1 | 1 | 1 | parity |
| `modes` | 1 | 1 | 1 | parity |
| **score** `hall_of_fame` | 135 | **150** | **150** | MARKED, **above** |
| **score** others | 50/5/0/1/0/5/5 | same | same | parity |
| **TOTAL** | 201 | **216** | **216** | MARKED, **above** |

**Six cells marked, all six above FT8, none short - so nothing in this table was task 2's to
close.** Before and after are the same table: it was already at parity and it still is. The
Hall of Fame keys: FT8 earns `first_contact`, `first_over_5000_miles`,
`first_over_10000_miles`; PSK31 those three plus `first_psk31`; Olivia those three plus
`first_olivia`. **The asymmetry is the owner's file and not the scorer**, measured key by key:
`first_psk31` 15 pts, `first_olivia` 15 pts, and **no such key in the file** for `first_ft8`,
`first_ft4`, `first_cw`, `first_wspr` or `first_voice`.

**The record Hamlet itself builds**, all sixteen `AdifContact` fields, the same textbook
transcript replayed down the PSK31 panel and the Olivia panel and an FT8 exchange with the
same station. **PSK31 and Olivia are identical field for field.**

| Field | FT8 | PSK31 | Olivia |
|---|---|---|---|
| `Call` / `StationCallsign` | W1AW / KC3QIS | same | same |
| `Band` | `20m` | `20m` | `20m` |
| `Mode` | `FT8` | `PSK` | `OLIVIA` |
| `Submode` | **null** | `PSK31` | `OLIVIA 16/500` |
| `ReportSent` | null | null | null |
| `ReportReceived` | `-12` | **null** | **null** |
| `RstSent` | **null** | `599` | `599` |
| `RstReceived` | **null** | `599` | `599` |
| `GridSquare` | `FN31` | `FN31` | `FN31` |
| `MyGridSquare` | `FN00DJ` | `FN00DJ` | `FN00DJ` |
| `State` | **null** | **null** | **null** |
| `Comment` | null | null | null |

The four differences against FT8 are **what each mode exchanges, not a gap**: FT8 exchanges
decibels and a keyboard mode exchanges RST, and only FT8 has no submode. **`State` is null in
all three columns**, which proves section 6 ruling 1 item 2 by measurement rather than taking
it from the paragraph: an FT8 contact Hamlet logged earns no state record either, so a
keyboard mode earning none is parity and not a gap.

**The Modes and Hall of Fame cards, titles verbatim, before and after.** `ModesToWork` reads
`six modes to work` and `WorkableModes` is 6, counted from the table and nowhere typed.

| | Before | After |
|---|---|---|
| Modes / FT8 | `"FT8"` earned, `1 contact` | unchanged |
| Modes / PSK31 | `"PSK31"` earned, `1 contact` | unchanged |
| Modes / Olivia | `"Olivia"` earned, `1 contact` | unchanged |
| HoF / PSK31 | `"A PSK31 contact"` earned, figure `1 contact`, callsign `YB1ABC`, `YB1ABC · Indonesia`, `20 m · PSK31`, `Sep 19, 2026` | unchanged |
| HoF / Olivia | `"An Olivia contact"` earned, figure `""`, callsign `""`, callGrid `""`, bandMode `""`, date `""` | figure `1 contact`, callsign `YB1ABC`, `YB1ABC · Indonesia`, **`20 m · Olivia`**, `Sep 19, 2026` |
| HoF / Olivia, unearned | `"An Olivia contact"` wants **`Any station at all`** | wants **`Olivia lives at 3.582 on 80m`** |
| HoF / PSK31, unearned | `"A PSK31 contact"` wants `PSK31 lives at 3.580 on 80 m` | unchanged |

**The quill, row kind by row kind, for one station on an empty log.** Section 5 said both row
kinds reach `MarkIfItOpensSomething`; it is proved.

| | FT8 row | PSK31 row | Olivia row |
|---|---|---|---|
| `Nudge` | `Door` | `Door` | `Door` |
| `NudgeTip` | `new area · would open something you have not seen yet` | identical | identical |
| `NudgeReasonLine` | `A first contact in a new area · working him opens a set of cards you have not seen yet` | identical | identical |
| `NudgeEarnsLine` | `A QSO first. The card comes with it.` | identical | identical |
| `NudgePreview` | `HasPreview=True IsDoor=True` | identical | identical |
| `IsTextOnly` / `HasVariant` | False / False | True / False | True / **True** |

**Not one difference. 8.3's quill half was already met and the work was the assertion (R14).**

**The CQ list the achievements page is handed, calls verbatim with their `Mode` field.**

| Callsign | Grid | HeardUtc | Mode **before** | Mode **after** |
|---|---|---|---|---|
| `IK4LZH` (FT8 row) | `JN54` | `214130` | `""` | `""` |
| `G4XYZ` (PSK31 row) | `""` | `193556` | `"PSK31"` | `"PSK31"` |
| `W1AW` (Olivia row, variant `16/500`) | `""` | `193556` | **`"PSK31"`** | **`"Olivia"`** |

**Section 5's prediction was right and is confirmed.** The FT8-shaped row still claims
nothing, which is asserted, because it does not know whether it was FT8 or FT4.

**The page's eight scores and the total, before the contact and after it, built the way
`OpenAchievements` builds it. Identical for both modes.**

| Badge | Before | After (PSK31) | After (Olivia) |
|---|---|---|---|
| `hall_of_fame` | 0 | **25** | **25** |
| `continents` | 0 | **5** | **5** |
| `countries` | 0 | **5** | **5** |
| `states` | 0 | 0 | 0 |
| `grids` | 0 | **1** | **1** |
| `total_miles` | 0 | 0 | 0 |
| `bands` | 0 | **5** | **5** |
| `modes` | 0 | **5** | **5** |
| **total line** | `Total 0 pts · Rank 1 · 25 to Rank 2` | `Total 46 pts · Rank 2 · 54 to Rank 3` | `Total 46 pts · Rank 2 · 54 to Rank 3` |

**Six of the eight badges moved on each contact**, asserted as a difference and not as a
figure, with nothing allowed to go backwards and the Modes badge and Hall of Fame asserted to
be among what moved.

**The carry-forward counts, and the six achievement types.**

| | Task 0 | Task 4 |
|---|---|---|
| App carry-forward | **214 of 214** (2 m 18 s, second attempt) | **216 of 216** (2 m 22 s, first attempt) |
| Engine carry-forward | **150 of 150** (4 m 54 s) | **150 of 150** (5 m 1 s) |
| The six achievement types | **43 of 45** | **43 of 45** |
| `TheAchievementsPageTests` (8.4 names it) | green | green |

The app's two additions are this unit's own guards and are the whole of the difference. **The
six achievement types are identical before and after**, the same two inherited reds. This
unit's own types: `TheKeyboardModesEarnWhatFt8EarnsTests` 5 of 5,
`TheQuillAndThePageMeanTheSameTests` 4 of 4, `Unit379Trace` 1 of 1.

## 4. What's blocking us

**Nothing is blocking a criterion in B.** All four are met and 8.4's drop candidate was not
taken.

**1. The CQ list still cannot tell an Olivia channel with no readable variant from a PSK31
row, and labels it `PSK31`. A finding, and it names a choice that is overrulable in one
line.** `CqSnapshot` now names the mode from the row's own `Variant`, which is the one fact
the row carries about which mode it is and the one section 6 ruling 2 item 2 points at. But
the row keeps the variant **string**, while `Psk31ContactWith` answers the same question off
**dictionary membership** - `_oliviaVariants.ContainsKey(channelId)` - precisely so that a
channel with no readable variant cannot be called PSK31 in the log. So the log gets this right
and the CQ list does not. It is strictly narrower than what it replaced, where **every** Olivia
row read `PSK31`, and closing it means putting a mode fact on `DigitalDecodeRow` at the one
builder that makes a text row, which is a change ruling 2 item 2 does not license. **A later
unit's, or one line from Tim if he wants it now.**

**2. `ViewTestsActThroughControlsTests.NoViewTestWritesAPropertyAControlOwns` is red,
inherited, and reported for the fourth unit running. A finding, not a ruling request.** It
reports `TheStopIsAlwaysOnScreenTests.cs:102 writes OperatingMode`. Unit 375 wrote that line;
unit 378 ruled it belongs to whichever unit next touches that file. **No task here touched
it**, so it is reported and not repaired, exactly as unit 378's own rule directs.

**3. The unearned Olivia card spells the band `80m` where PSK31's spells it `80 m`. A
finding.** Olivia's calling spot comes from the Olivia calling table's own `Band` column and
PSK31's from `DigitalCallingFrequencies`. It is **the same string the Modes badge has shown
since unit 358**, so Hamlet is consistent with itself and this unit introduced nothing; the
spelling belongs to `data/bands/olivia-calling.json`, which §0 says is the source of truth and
which is not this unit's to edit. Reported, not repaired.

**4. The carry-forward line for task 2's guard landed in task 3's commit, one commit late. A
disclosure, not a ruling request.** Section 11 asks that a carry-forward line edit go in the
same commit as the test it names. `TheKeyboardModesEarnWhatFt8EarnsTests.AnOliviaContact
EarnsWhatTheSameFt8ContactEarns` was committed at task 2 and its line added at task 3. The
list and the test are both in the tree and both green; only the commit boundary is wrong.

**5. A scratch file of mine, `probe379.py`, was committed by accident at task 3 and has been
emptied and commented rather than deleted. A disclosure, not a ruling request.** It was a
throwaway written at task 1 to print the shape of `data/callsigns/dxcc-prefixes.json` while I
worked out why `VK3ABC` resolved to no entity. **It was never run** - the shell refused
`python probe379.py` - and the question was answered by reading the file with `grep` instead.
It had been left staged by a `git add -A` at task 1, and task 3's commit carried no pathspec,
so it went in with that task's real files. `PHASE_PLAN.md` section 6 says *empty it, comment
it, list it* rather than delete, so it is now a comment saying what it was, that it was never
run, how it got in and that nothing reads it. **Listing it here is the third part of that
rule.**

**6. This instruction calls the carried queue twenty-one and its own enumeration lists
twenty-two. A mismatch against the instruction, reported per section 5; a finding.** Section 3
enumerates *unit 378's items 1, 3 and 4* (3); *unit 377's item 4* (1); *unit 376's items 3, 4
and 5* (3); *unit 375's items 3 and 4* (2); *unit 374's item 3* (1); *unit 373's item 2* (1);
*unit 372's items 4 and 7* (2); *unit 371's five* (5); *unit 369's four* (4) - **22**, and
says "twenty-one" twice, as does section 12's ordering block. Unit 378's report carried
nineteen and this unit adds three. **All twenty-two are carried below**; dropping one to reach
the stated count would have lost an item.

**7. `validate-output.bat` still refuses, in the exact shape section 2 prescribes, and that
is a finding about the harness.** Section 2 says to report the exact command and the exact
refusal rather than hand-check silently. Three shapes were tried:

```
./tools/arbiter/validate-output.bat output.md
  -> This command requires approval

sh tools/arbiter/validate-output.bat output.md
  -> tools/arbiter/validate-output.bat: line 1: @echo: command not found
     ... line 68: syntax error near unexpected token `('

powershell -NoProfile -Command "<the script's own rule 1 check, verbatim>"
  -> This command requires approval
```

**The refusal is not the shape this time - it is the permission mode.** Section 2's diagnosis
was that unit 378's five failures were shape, and it was right that `cd X && Y` and `cmd /c`
are refused; my first attempt carried a `cd` prefix and was refused for that reason, and
removing it changed the refusal from a shape error to **`This command requires approval`**,
which is the sandbox and not the syntax. This session is non-interactive, so no approval can
be given. `sh` cannot run a `.bat` at all - the second refusal above is `cmd` syntax reaching
a POSIX shell - and the script's own checks run through `powershell`, which is refused on the
same ground as the first. **So the shape section 2 found is correct and the tool is still
unreachable from a non-interactive session.**

**The six rules were checked by hand against the script's own source instead, and all six
pass**: rule 1, a parseable `UNIT:` line at line 27, above section 1 at line 46 and inside the
60-line window the script reads; rules 2 and 3, exactly four `## ` headings - `1. What Claude
did`, `2. What the owner should expect`, `3. What you should see`, `4. What's blocking us` - in
that order with no fifth; rule 4, section 4 present at line 295; rule 5, section 3 non-empty,
lines 172 to 294; rule 6, the ordering block above the `UNIT:` line with `READ IN THIS ORDER`,
an `A.`, a `B.`, a `C.` and a committed count of section 4 items. **That is a hand-check and it
is named as one** - it is not the script's own verdict, and the next unit should not read it as
one.

**One more thing worth the next author's time, and it is a finding about the trace itself.**
`Unit379Trace`'s first station, `VK3ABC`, resolved to **no DXCC entity**, because `VK` is
claimed by Australia, Heard I. and Lord Howe I. and `DxccPrefixes` declines a shared prefix
rather than guessing. With no entity there is no country, no continent and **no quill**, so
the first run of the trace reported `Nudge=None` on all three rows and its own
same-kind-same-words check passed **vacuously**. The station was moved to `YB1ABC` and the
measurement re-taken, and the assertion type now checks that the FT8 row's quill says
something **before** comparing the other two against it. The decline is the table obeying
§0.0 and wants no repair; the lesson is that an equality test over three empty things passes.

**No change on a send path was needed to reach any criterion in this step**, and that is
stated here because the instruction asks for it as the first item if one had been found. R40
licensed reading a log record and moving a number on a page, and that is all this unit did: no
composer, no `Arm`, no `PttOn`, no cap, no RSID burst, no Olivia variant gate, and **no engine
file at all.**

---

**The carried queue, verbatim per HM-DEC-139. Twenty-two, and this unit answers none of
them.** **Unit 378's item 2 came off it because R43 had already ruled it** - the five canned
line-and-variant pairs refused by the 30-second fallback - and no timing row was written.

1. **Unit 378's item 1** - `ViewTestsActThroughControlsTests.NoViewTestWritesAProperty
   AControlOwns` is red, inherited, and reports `TheStopIsAlwaysOnScreenTests.cs:102 writes
   OperatingMode`. Unit 375 wrote that line. It belongs to whichever unit next touches that
   file; no task here touched it. **Also section 4 item 2 above.**
2. **Unit 378's item 3** - the canned file is read once per session. An author's choice,
   stated and overrulable, and nothing in this unit touches it.
3. **Unit 378's item 4** - `Unit378Trace` now prints the after, not the before. A note about
   a trace. **`Unit379Trace` has the same property and that is correct**: the before is
   preserved in this report, in `PHASE_OUTCOME.md` and in task 1's own commit message.
4. **Unit 377's item 4** - `TheOliviaMoveUpTests.ItIsNotOfferedOnAGuessedYourTurn` is red,
   inherited, measured against the old embedded file and red there too, and not on the
   carry-forward list. A finding. This unit did not repair it and did not chase it.
5. **Unit 376's item 3.**
6. **Unit 376's item 4.**
7. **Unit 376's item 5.**
8. **Unit 375's item 3** - Avalonia's headless `InvalidProgramException: You have caused
   dispatcher loop`, which kills an app invocation at about 1 ms before any assertion and
   moves between names. **It showed once in this unit**, at task 0, on
   `TheStopIsAlwaysOnScreenTests.AtEachOf354sNineSizesStopIsInTheStatusBarAndOnTheWindow` - a
   third name it had not landed on, after unit 378's two - and the single re-run was clean.
   Recorded, not chased. **The engine invocation has still never failed since unit 375.**
9. **Unit 375's item 4** -
   `TheStopIsAlwaysOnScreenTests.WithNothingKeyedItSaysStopAndIsStillPressable`. It did not
   show in this unit's three app invocations.
10. **Unit 374's item 3** - `tools\run-carry-forward.sh`. The two command lines were run from
    `docs\carry-forward-tests.txt` itself, as the instruction directs.
11. **Unit 373's item 2** - `ApplyBestBet`'s stale `BestBetLabel`.
12. **Unit 372's item 4.**
13. **Unit 372's item 7** - `PHASE_PLAN.md`'s criterion checkboxes. Not ticked, not raised.
14. **Unit 371's first.**
15. **Unit 371's second.**
16. **Unit 371's third.**
17. **Unit 371's fourth.**
18. **Unit 371's fifth.**
19. **Unit 369's first.**
20. **Unit 369's second.**
21. **Unit 369's third** - `LearnedAlcReference.Ago()`.
22. **Unit 369's fourth.**

**And the `RULES_AT` id-scheme split, carried and reported again, not repaired:**
`PROJECT_STATUS.md` `RULES_AT` reads `HM-DEC-165 (2026-09-19)` while `CLAUDE.md` section 1
holds `CPS-DEC-0165`. The reload names it first among the disagreements. `tools/status.sh`
writes the field as a literal, so repairing it means editing that script, and `tools\` is not
this unit's.

**Step 3's entry reads *step 8 done*.** Step 8's four criteria are met on measured evidence
and the judging session decides. Step 3's visibility telemetry was not started and no
visibility event was added to any row.
