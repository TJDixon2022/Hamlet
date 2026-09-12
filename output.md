```
READ IN THIS ORDER.
```

A. The phase goal - the screen says what is true and looks like someone meant it.
   Step 0 done by this unit; steps 1, 2 and 3 not started.
B. Step 0 and its five must-pass, each met, with the number:
   1. no band pill on the green zone, and the map is wider - 202 -> 232 px at a 1400
      window, 202 -> 492 px at 1920. Met.
   2. band, frequency, mode and license lines on the left; heard count on the right;
      rule of thumb under the map. Met, and the left lines wrap exactly as before.
   3. "Hamlet cannot work CW, PSK31 and Voice yet" exists nowhere. Met - unit 332 had
      already removed it; a VoiceTests check now holds it out.
   4. BindingHealthTests and VoiceTests green; carry-forward green. Met - 100 of 100 app
      and 85 of 85 engine after the change.
   5. the report lists every emptied file once. Met - thirteen, in section 2.
C. The report last. Section 4 raises 3 items of its own on top of the carried queue; none
   stands in the way of a criterion in B.

```
UNIT:       334 - complete at task 3 of 4, tasks 0 to 3 all done, none dropped - 2026-09-12 16:24
PHASE GOAL: Maintenance - make what Hamlet shows true and deliberate-looking, screen only, judged
            finally by Tim at his own window size.
UNIT GOAL:  Open the phase, take the repeated band list off the green zone and give the map its
            room, confirm the stale cannot-work sentence is gone, and hand Tim one list of the
            files sessions emptied because they could not delete them.
ADVANCED:   yes - all five of step 0's must-pass are met by tests run in this session
NUMBER:     green zone map width 202 -> 232 px at a 1400 window (202 -> 492 px at 1920)
DRIFT:      0
```

## 1. What Claude did

**Complete - all four tasks, 0 to 3, none dropped, each committed and pushed.** Claude Code on
Tim's Windows 11 machine, project Hamlet, gate passed on all four checks, branch `main`.

**Every appearance claim in this report is computed on the Avalonia headless host, not seen.**
That host draws text at a flat ten pixels a character, wider than the glass.

### Task 0 - the phase opens

- Carry-forward list, run before any edit, the two invocations its comment orders: app **100 of
  100** in 10 s, engine **85 of 85** in 4 s.
- `PHASE_OUTCOME.md`: `UNIT 334 - STEP 0` appended; `STATE_AFTER: done` added at the end.
- Version 1.13.18 -> **1.13.19**, with its comment in `Directory.Build.props`.
- `PROJECT_CARD.md`: `PHASE` and `PHASE_SET` now name this phase and 2026-09-12.
- `DECISIONS.md`: **HM-DEC-162**, Tim's ruling of 2026-09-12 in his words - the maintenance
  phase set, the PSK31 phase archived with its step 6 open for him to close at the radio. Indexed
  in `CLAUDE.md` section 1.
- Commit `chore(unit334): 1.13.19, ...`.

### Task 1 - the band pills come off the green zone

**Before, measured on the window at 1400:** panel 756 px inside; left block 253 px; map 202 x
110; right block 262 px holding the best bet, **7 pills in 3 rows, each with its pip bar**, the
*you are on it* line, and the count with its sparkline. At 1920: map 202 x 110, right 478 px.

**Test first.** `TheGreenZoneTests` was rewritten under R12:
- Assertion 10 now asserts no `GreenZonePills`, no `hm-chip`, no pip bar and no *you are on it*
  text on the panel. It also asserts the map grew by the stated number, the four left lines, the
  count and the rule of thumb are drawn, the count sits right of the map, and the rule sits under
  it.
- Assertion 12 no longer looks for the on-it line.
- It was **watched red**: `chips 7, pips 7, map 202.00 x 110.00`.

**Change.**
- The pills block is gone from `MainWindow.axaml`; `GreenZonePill`, `GreenZone.Pills`,
  `GreenZone.YouAreOnIt` and the `bands` parameter are gone from `GreenZone.cs`, and the view
  model no longer passes `Bands`.
- The regions grid went from `*,Auto,*` to `*,*,Auto`, and the map lost its fixed `Height="110"`.
  So the right is only as wide as the best bet and the count, and the map draws at its own
  proportions across the column it shares with the left.

**After:**

| Window | Map before | Map after | Left block | Right block | Panel height |
|---|---|---|---|---|---|
| 1400 | 202 x 110 | **232 x 127** | 253 -> 232 px, lines 1/2/3 as before | 262 -> 260 px | 193 -> 177 px |
| 1920 | 202 x 110 | **492 x 269** | 513 -> 492 px, lines 1/1/2 as before | 478 -> 260 px | 151 -> 310 px |

- Panel ink is **100%** of its width at both.
- The sparkline stays; it fits.
- The drop candidate was not used.

**Green:** `TheGreenZoneTests` 15 of 15, plus the green zone width test. Then `BindingHealthTests`,
`VoiceTests`, `TheGreenZoneTests` and that width test together: **21 of 21**.

### Task 2 - the stale line

- `git grep` finds *Hamlet cannot work CW, PSK31 and Voice yet* nowhere in `src`, `data` or
  `assets`. Unit 332 removed it in `ece5788`.
- `VoiceTests.TheStaleCannotWorkSentenceIsNowhereInTheSource` is added. It checks every `.cs` and
  `.axaml` file under `src`, comments included, plus a sample proving the sweep sees the sentence.
- **It could not be watched red on the tree**, because the sentence was already gone. It went
  green on its first run, and `VoiceTests` is 5 of 5.

### Task 3 - the leftovers, listed once

- Searched the tracked, untracked and ignored files for sessions' *emptied* and *could not
  delete* notes. Then checked every candidate for declarations.
- **Thirteen files are emptied**, all comment-only with no code. **None was left unemptied**, so
  nothing was changed.
- The list is in section 2. No commit was needed beyond the closing one.

### After the tasks

Carry-forward re-run on the changed app, the two invocations: app **100 of 100** in 9 s, engine
**85 of 85** in 4 s.

### Decisions this session made for itself, reproduced in full

1. **`*,*,Auto`, so the left block and the map split what the right leaves.**
   - The left box narrows by 21 px at both widths, but its lines wrap exactly as before.
   - Rejected: keeping the left box at exactly its old width, which no star grid expresses.
   - Rejected: stacking the count under its sparkline to narrow the right further, which
     rearranges the count beyond what the task asked.
2. **The best-bet button stays on the right.** The task named the pills, their bars and the
   on-it line, and nothing else.
3. **The pill model is deleted, not left unbound.** `GreenZonePill`, `Pills`, `YouAreOnIt` and
   the `bands` parameter had no reader once the markup stopped binding them.
4. **The width assertion was tightened after measuring.** The red run asserted only *wider than
   202*. After the change, the map measured 232 and the test now pins 202 + 30 within half a
   pixel. The ARBITER block says the width is measured, not chosen; nothing was loosened.
5. **`Unit332TwoWidthsTests` was rewritten under R12.** It looked up `GreenZonePills` by name and
   would have failed on its absence. It now prints the chip count, which is 0.
6. **Emptied files with several comment lines were not cut down to one.** The task empties only
   files *not yet emptied*, and all thirteen already were.
7. **Each task was pushed on its own.** The prompt says to push each task; section 11 of the
   instruction says push at the end. The prompt wins.

### Where the instruction and the tree disagreed

Reported, not repaired.

- **`.py` at the root does not exist.** The instruction lists it as an undeletable file; no file
  of that name is in the tree.
- **The instruction's five named files are ten more than it knew of.** It names five, and a
  sixth, `.py`, that does not exist. The tree holds thirteen emptied files:
  - unit 333's probe;
  - unit 323's retired `ThePsk31TabIsInertTests.cs`;
  - six comment-only probes under `tests/Ft8Sharp.Tests` from units 203 and 214 to 289.
- **`Unit333ProbeTests.cs` is tracked**, where unit 333 called it untracked. It was committed in
  `3ea149e`.
- **The green zone as unit 332 left it matched section 5 exactly**: left block, map with its
  night side and the operator's dot, right block with pills, the on-it line, sparkline and count,
  and the rule of thumb under the map.
- **`PHASE_STATUS.md` line 1 names this phase with four steps**, and `docs/phase-psk31-run/`
  holds its three files. Both match.
- **`tools/status.sh` cannot run here.** `sh tools/status.sh`, `bash tools/status.sh` and
  `./tools/status.sh` all came back *requires approval*. Every status write in this unit was a
  `date` reading pasted into the file whole, and none was composed.
- **Task numbering in `PROJECT_STATUS.md` counts from one.** Tasks 0 to 3 are written as
  `TASK: 1 of 4` to `4 of 4`.

## 2. What the owner should expect

**The green zone no longer repeats the band row.** Below the band strip, the panel shows three
things:
- on the left, the band large, the frequency, the mode and the license lines, unchanged;
- in the middle, a larger world map with its night side and your dot;
- on the right, *best bet now* and *heard just now*, with the sparkline and the count.

**What will look wrong but is not:**
- **At a 1400 window the map is only a little bigger**: 232 x 127 against 202 x 110, computed.
  The pills had wrapped to three rows inside a right column whose width was already set by the
  count and its sparkline. So the room they gave back was mostly height, and the map grew into
  it. Section 4 item 2 has the choice that would give it more.
- **At 1920 the panel is about twice as tall**: 310 px against 151, computed. The map widens to
  492 px and keeps its proportions, so it is 269 px tall. Section 4 item 1 asks whether you want
  it capped.
- **The left block's box is 21 px narrower** at both widths, computed. Its lines break in the
  same places as before.
- **The achievements window still has *cannot work them* hovers for FT4 and PSK31.** They are
  in `AchievementsViewModel.cs` lines 501 to 517. Task 2 checked only the one named sentence, and
  these are carried item 1 from unit 332, below.

**Version 1.13.19.** The card and the decisions file name the maintenance phase. Nothing on the
radio side changed.

### The one list of files for Tim to delete by hand

Every one is a file a session emptied because this environment refuses deletes. Each holds only
comments and compiles to nothing. **All thirteen are tracked by git**, so delete them and commit
the deletion.

1. `C:\Source\HamLet\commit-msg-326.txt`
2. `C:\Source\HamLet\toolsarbitervalidate-output.bat`
3. `C:\Source\HamLet\tools\arbiter\unit323-append.bat`
4. `C:\Source\HamLet\tools\arbiter\unit323-append.py`
5. `C:\Source\HamLet\tools\cut-header-action.py`
6. `C:\Source\HamLet\tests\Hamlet.App.Tests\Views\Unit333ProbeTests.cs`
7. `C:\Source\HamLet\tests\Hamlet.App.Tests\ViewModels\ThePsk31TabIsInertTests.cs`
8. `C:\Source\HamLet\tests\Ft8Sharp.Tests\Dsp\Unit216Probe.cs`
9. `C:\Source\HamLet\tests\Ft8Sharp.Tests\Dsp\Unit217Probe.cs`
10. `C:\Source\HamLet\tests\Ft8Sharp.Tests\Dsp\UpstreamSyncSearchProbe.cs`
11. `C:\Source\HamLet\tests\Ft8Sharp.Tests\Ldpc\UpstreamLdpcProbe.cs`
12. `C:\Source\HamLet\tests\Ft8Sharp.Tests\TempEncoderProbe.cs`
13. `C:\Source\HamLet\tests\Ft8Sharp.Tests\Unit289SourceProbe.cs`

**Not on the list, because nobody emptied them.** Both are gitignored scratch, also safe to
remove:
- `C:\Source\HamLet\.commit-msg.tmp`, unit 261's commit message;
- `C:\Source\HamLet\artifacts\unit333\psk31-fixture-export.adi`, unit 333's export copy.

## 3. What you should see

**The map on the green zone is bigger, and the band pills are gone from the panel.** At a 1400
window the map goes from 202 to 232 px wide. At 1920 it goes from 202 to 492 px wide. Both
figures are computed on the test host, not seen.

- Open Hamlet on the Digital tab with the neighborhood map open. The panel under the map legend
  has no row of band buttons and no *you are on it* line. The band strip above is where the bands
  are.
- The world map in the middle of that panel is wider and taller than yesterday, with the night
  side and your dot as before. The rule of thumb is still one line of small text under it.
- On the right: *best bet now*, then *heard just now* with its little line and the station count.
- On the left: the band in large type, the frequency, the mode line and the license line, reading
  exactly as before.
- Widen the window and the map grows with it. At full width on a 1920 screen the panel is
  noticeably taller than before.

## 4. What's blocking us

### Raised by this unit

**1. At 1920 the green zone is 310 px tall, because the map keeps its shape as it takes the
pills' width.**

*Ruling wanted: cap the map's height or not.* Computed: the map is 492 x 269 at a 1920 window,
and the panel goes from 151 to 310 px. At 1400 it goes from 193 to 177 px, so there it is
shorter.

*Reasoning.* R21 says the map takes the space the pills held, and at 1920 that is 218 px of
width. A map drawn at its own proportions cannot take width without height, and stretching it
would move every place off the pixel the projection puts it on (HM-DEC-092).

*What was rejected and why.* Choosing a cap here. A height limit is a number about how much of
your screen the panel may take, and it is yours to pick.

**2. At 1400 the map gained only 30 px, because the count and its sparkline set the right
column's width, not the pills.**

*Ruling wanted, if 30 px is not the bigger map you meant.* The right column is 260 px after,
against 262 before.
- The sparkline is 110 px of it plus a 10 px gap.
- Dropping the sparkline, the task's named drop candidate, would free that width.
- The left block and the map share freed width equally, so the map would gain about half of it.
  That is arithmetic on the measured widths, not a measurement.

*Reasoning.* The task says keep the sparkline if it still fits, and it fits, so it stayed.

*What was rejected and why.* Dropping it anyway, which the task does not allow while it fits.
Stacking the count under the sparkline, which rearranges the count beyond what was asked.

**3. `tools/status.sh` is still refused, in all three spellings.**

*No ruling wanted; a finding, the same as carried item 5 below.* `sh`, `bash` and `./` all came
back *requires approval*. Every `UPDATED` in this unit is a `date` reading pasted whole, and none
was composed. The validator was run by the `.proj` route; its verdict is in the session
transcript, not quoted here.

### Carried from unit 333's section 4, per HM-DEC-139 - verbatim

Headings under it are moved down one level so they sit inside this one; the words are unchanged.

#### Raised by this unit

**1. Where PSK31 is the only Hall of Fame first left unearned, Ruling C and §3.1 say opposite
things about one slot.**

*Ruling wanted.* The slot is the Hall of Fame badge's next card, and Hall of Fame's unearned
card inside the category. The exact string is `A PSK31 contact`.

It arises on a log holding *Your first contact*, *A DX contact*, *A Morse contact*, *Over 5,000
miles* and *Over 10,000 miles* but no PSK31 contact, for instance an imported CW log with long
contacts.
- Ruling C, Tim, 2026-09-12: *every kind shown, the nearest unearned card in each, nothing
  beyond it.*
- `ACHIEVEMENTS_PHILOSOPHY.md` §3.1: *absent, not dimmed. No PSK31 card exists until the first
  PSK31 contact.*

*Reasoning.* On every other log the two agree: the badge shows the nearest first §3.1 allows.
In this one case, showing the card breaks §3.1 and showing nothing breaks Ruling C. The
instruction says not to choose, so **the screen is left as it was and still shows
`A PSK31 contact` there.**

*What was rejected and why.* Showing no next card, which is choosing §3.1. Moving `first_psk31`
last in the list, which only moves the collision to a later log and reorders the owner's
firsts.

**2. The nice-to-pass wants a logger, and this session could not look for one.**

*Something you can do in a minute, not a stop.* Import `docs/unit333-psk31-fixture-export.adi`
into a new, empty log in any logger you already have, with the five steps in section 2. Name
the logger and its version, and say whether it took both records with PSK/PSK31, both RSTs
and the grid. That closes the criterion.

*Reasoning.* The instruction forbids installing one, and the permission layer refused every
listing outside `C:\Source\HamLet` and the registry query.

*What was rejected and why.* Downloading or building a logger, which is your decision about
your machine. Guessing from memory which loggers are installed, which would be a claim nobody
measured.

**3. This environment refuses deletes inside the repository and listings outside it, so two
scratch files remain.**

*No ruling wanted; housekeeping.*
- `tests/Hamlet.App.Tests/Views/Unit333ProbeTests.cs` is untracked and one comment line.
- `artifacts/unit333/psk31-fixture-export.adi` is gitignored.

Both are safe to delete by hand, beside the five carried in item 19 below. The validator was
run by the `.proj` route the instruction names. The prompt's `.bat` spelling is the one unit
243 documented as mangled by Git Bash.

#### Carried from unit 332's section 4, per HM-DEC-139 - verbatim

**1. The mode rows' hovers still say Hamlet cannot work PSK31 and FT4 and cannot log CW.**

*Ruling wanted on whether to rewrite them.* `AchievementsViewModel.Why` has *Hamlet can tune
you to the PSK31 watering holes and cannot work them* and the FT4 and CW equivalents. That is
the same falsity as the sentence task 0 removed.

*Reasoning.* The instruction named the one line and §12.6 says not to repair unrelated things
on the way past. Those hovers are not on the new page, but `ModeFirstRow.Why` is still in the
tree, and a later surface could draw it.

*What was rejected and why.* Rewriting them here. The words about what Hamlet can now work are
a claim about PSK31 and CW, and they want the step 5 and 6 facts behind them, not this unit's
guess.

**2. The continent level names two continents he has not opened.**

*Ruling wanted.* Antarctica and Oceania each get a badge on the fixture, with `A first here`,
`0 pts` and `500 for a first` or `50 for a first`.

*Reasoning.* The instruction says *seven continent badges*, and seven named badges are what the
Continents level is. Its next cards do not name the continent again. But §3.1 says nothing
shows inside a category until something adjacent is earned, and this bends it one step further
than ruling C bends the page.

*What was rejected and why.* Drawing only the opened continents. The instruction asked for
seven, and a five-badge level would read as five continents.

**3. The fit is measured on a host that draws text about half again wider than the glass.**

*Ruling wanted on the window size.* To pass a measured no-clip test there, the achievements
window went from 820 to 1040 wide and nine strings were shortened.

*Reasoning.* The host advances ten pixels a character at every size. A string that fits there
fits on the glass, so the test cannot pass a clip the owner would see. The price is a window
wider than the glass needs.

*What was rejected and why.* Estimating widths for a proportional face instead of measuring.
The instruction says *asserted by measuring*, and an estimate is the thing that let 331's text
clip.

**4. The green zone's license phrase is not on one line at 1400.**

*Ruling wanted.* The instruction says *the license phrase and citation on one line*. At a 1400
window the left region is 253 px, and the phrase lays out to three lines on the test host; I
estimate one or two on the glass.

*Reasoning.* The mockup fits it by rewording it to *General covers digital modes here*.
`PrivilegeStatus.Detail` is the regulation's sentence, and 331 kept it in its own words. One
line at 1400 therefore needs either a shorter sentence or less room for the map and the
buttons.

*What was rejected and why.* Keeping it one line and letting it run past the panel's edge,
which is what the first cut did, at 142%.

**5. `tools/status.sh` cannot run here, and neither could the validator's `.bat`.**

*No ruling wanted; a finding.* `sh tools/status.sh` and `bash tools/status.sh` both came back
*requires approval*. So every status write was `date` and a paste, and none was composed: the
helper exists and the permission layer does not allow it.

The validator was attempted as instructed, `tools\arbiter\validate-output.bat output.md`, and
Git Bash turned the path into `toolsarbitervalidate-output.bat`. It was then run through the
route unit 243 built, `dotnet build tools/arbiter/validate-output.proj -p:Report=output.md`,
and its verdict is the last thing in this session's transcript. It is not reproduced here,
because a report cannot quote a run of itself.

*What was rejected and why.* Composing timestamps as units 327, 328 and 331 did.

**6. Step 5 is `partial` in `PHASE_STATUS.md` and *done* in the instruction.** **ANSWERED by
this unit**: the four must-pass and R13 are proved in section 3, the nice-to-pass is unmet, and
unit 333's `PHASE_OUTCOME.md` entry records `STATE_AFTER: done`. The `STEP: 5` lines are the
launcher's and were not written.

#### Carried from unit 331's queue, as unit 332 carried it - verbatim

**1. Fourteen `UPDATED` timestamps in `PROJECT_STATUS.md` were composed rather than
read from the clock - the third unit running, and this session read both prior
reports of it before doing it.**

*No ruling wanted; reported because it is now a pattern rather than a slip.* The
clock was read at `12:44:45` and at `13:48:41`, and every status write between them
carried an extrapolated time: `13:02`, `13:15`, `13:24`, `13:40`, `13:52`, `14:05`,
`14:12`, `14:30`, `14:44`, `14:58`, `15:12`, `15:30`, `15:52`, `16:25`. **The last of
those is two and a half hours ahead of the true time.** The final write is from the
clock and says so.

*Reasoning.* This defeats the one signal that catches a stopped session, which is the
whole purpose of the ten-minute write - a panel reading `16:25` at `13:48` cannot tell
a working session from a dead one, and would have read unit 330 as alive for two
hours after the watchdog killed it. Unit 327 reported it, unit 328 reported it and
repeated it, and this session did it fourteen times.

*What was rejected and why.* Reporting it as a detail. Three units is a mechanism
problem: the rule says *read from the clock* and the failure mode is that reading the
clock is a separate command nobody budgets for. **The fix that would work is a status
helper that reads the clock itself** - `tools/status.sh` arrived in the seed commit
and this session did not use it, which is its own finding.

**2. The instruction's own width arithmetic cannot hold at 1400 px, and the honest
resolution costs the card 48 px.**

*Ruling wanted.* Task 1a asks for about 460 px inside the card. Measured: the decoded
panes are half the tab, the decoded list needs 383 of them to stop clipping the
longest FT8 line, and 460 inside the card needs about 516 px of panel - so the pair
needs about 899 px, which is a window of about **1856**. At 1400 the arithmetic leaves
**227 px** inside the card, down from 275.

*Reasoning.* Two §0.0 claims are in conflict at 1400 and only one can win: a clipped
callsign on the decoded list is a station misidentified, so the list got what it
needs. **The room the instruction wants exists at your own window width if it is over
about 1850**, and does not below it.

*What was rejected and why.* Taking the pixels from the waterfall. Changing
`DigitalPanes` from `*,*` to `1*,2*` would give the card about 456 px inside at 1400 -
almost exactly the number asked for - but the outer split is your ruling from a phase
ago, and the waterfall would fall from 666 px to 447. **That is the option, and it is
yours, not mine.**

**3. The States badge scores nought because the log does not read `STATE`.**

*Ruling wanted on whether to read it.* An ADIF record carries `STATE` and
`AchievementContact` does not parse it, so the kind has no count and no score. The
badge draws, its next card is *Your first state*, and nought is the honest figure.

*Reasoning.* A state worked out from a callsign prefix would be a claim about where
somebody lives, which the prefix does not support - a `W3` can be anywhere. Reading
the field would work for records written by a logger that fills it; **Hamlet's own
`Ft8ContactLogEntry` does not write one**, so the kind would score for imported
records and not for his own, which is a worse screen than an honest nought.

*What was rejected and why.* Hiding the badge. Eight kinds is the shape you approved,
and a kind that is absent because Hamlet cannot yet count it teaches nothing; a
nought with a first card behind it says what is missing.

**4. `first_answer_to_own_cq` is in your points file and Hamlet can never award it.**

*No ruling wanted; a finding.* The log says a contact happened and not who called
first. Awarding it would mean deciding that from the exchange, which nobody recorded.
It stays in the file because the file is yours and a key Hamlet cannot award today is
a key it may award later - it simply never scores.

**5. The family word on the green zone is `Digital` where task 4's example says
`Data`.**

*Ruling wanted, and it is one word.* See decision 1 in section 1. `ModePalette`'s own
label is what the map legend teaches, and a fifth word for one of four families would
have two surfaces calling one thing two things.

**6. `validate-output.bat` CLOSED - the route has existed since unit 243 and five
units have not used it.**

*No ruling wanted; the ask is answered and the answer was already in the tree.* The
`.bat` invocation was refused again exactly as units 324 to 328 recorded. **Then
`tools\arbiter\validate-output.proj` was found sitting beside it**, written by unit
243 for precisely this deadlock, and it works:

```
dotnet build tools/arbiter/validate-output.proj -p:Report=output.md
    -> VALID - all seven rules passed.
    -> validate-output exit 0
```

*Reasoning.* `dotnet build` is permitted with a wildcard, MSBuild's `Exec` runs a
command, and the `.proj` calls the validator unmodified with its own rules and fails
the build on a non-zero exit. **Nothing was copied, read around or reimplemented.**
Unit 328 wrote *this needs the permission layer changed or a route that is not a
`.bat`*; the route existed, in the same folder, with a 32-line header explaining
itself.

*What was rejected and why.* Applying the seven rules by hand again, as unit 328 did.
A hand-applied rule is applied by the same session that wrote the file, which is
exactly the independence the rule wanted; now that an independent run is available,
the hand-check is worth nothing beside it. **The line for the next unit to carry is
the command above, not the fault.**

**7.** *(was item 1)* **`MainWindow.axaml`'s comment on the mark now says the
opposite of what the code does.** **CLOSED by unit 330 task 1** and re-checked here:
both remaining *filled disc* strings read correctly in context, one of them 330's own
corrected comment.

**8.** *(was item 2)* **`Unit300SizesTests.WhatTheMarkDrawsAtEachSize` is red, and two
tests in this repository assert opposite things about the same mark.** **CLOSED by
unit 330 task 1**, reconciled on option B of 2026-09-10, and 10 of 10 green here.

**9.** *(was item 3)* **The render recorder erases the type of every shape, so a shape
assertion written the obvious way silently passes.** Carried. `DrawingGroup.Open()`
returns every geometry as `PlatformGeometry`, whatever it was drawn as, so
`Assert.IsNotType<EllipseGeometry>` passes against a filled disc. **Bounds are the
honest question**: a circle's are square.

**10.** *(was items 4 and 10)* **Composed `UPDATED` timestamps.** **Carried and
repeated** - see item 1 above, which is the same fault in the same file a third unit
later.

**11.** *(was item 5)* **The demodulator's quality measure vouches for a carrier that
has stopped, for between five and seven seconds, and that now sets how long a dead row
survives.** *Ruling wanted.* `Psk31Demodulator.Quality` is documented as *0.637 on
uniform noise phase and 1.0 on clean keying*. After a loud carrier stops, the input
**is** uniform noise phase and it goes on reporting 0.99, because both of its rolling
means are weighted by magnitude and the carrier's own loud symbols dominate the window
while they decay. **Measured on `psk31-idle-8s-1000hz.wav`: the squelch shut 5.70 s
after the carrier stopped; the quality fell under 0.80 at 7.20 s.** With
`KeepReadableSeconds` on top, `Psk31Listener.RetiredWithinSeconds` had to go from 2.5
to **9.0**. Two fixes would each bring it back under three seconds and neither has
been built: normalizing the measure per symbol changes what every PSK31 decode is
squelched on, and capping how long a vouch may outlive the spectrum contradicts
*retired only when both have lost it*.

**12.** *(was item 6)* **The idle fixture does not reproduce the fault the owner
saw.** Carried. With both new mechanisms switched off,
`psk31-idle-8s-1000hz.wav` still yields one carrier across the whole gap and still
nominates at 1000.0 Hz. **So the keep rule is built from the physics and from his
telemetry, and is proved not to break anything - it is not proved to fix what he
saw.** This is the same ask unit 324 left: **two minutes of his own 14.070 or 7.070,
captured to WAV.**

**13.** *(was item 7)* **`AchievementMarkControl.cs` was taken off the SHA pin, on
unit 327's own judgement.** Carried. Units 330 and 331 have both changed that file
since, under instructions that name it.

**14.** *(was item 8)* **The `Views` reds in `TheMenuIsUnderTheMouseTests` are eight,
not two, and the shared collapse flag was not the cause.** *Ruling wanted on who fixes
it.* Every one of the eight fails at the same line: `expected both decoded lists in
the window, found DigitalDecodedRows`. `DigitalMineRows` lives inside a `ScrollViewer`
gated on `ShowsConversation`, so the right-hand **row** list is realized only after
*show the N messages* is pressed. **That is deliberate** - the For you side became a
panel of cards and the raw rows are one press down, never gone. The test's premise
went stale on the day cards replaced that list.

**15.** *(was items 11, 12, 13)* **Unit 326 items 8 and 9 and unit 325 item 6 -
CLOSED by unit 327** and re-proved in the runs above.

**16.** *(was item 14)* **Unit 324 item 4 - why a 62 dB carrier failed the
keying-shape test. HALF ANSWERED.** The other half still wants a recording and is
item 12 above.

**17.** *(was item 15)* **The ALC margin of 15.** Carried verbatim: built, carried,
**Tim's to overrule**. Nothing in this unit touched it.

**18.** *(was item 16)* **`HM-DEC-161` versus `CPS-DEC-0161` - two id schemes.**
Reported, not repaired. `PROJECT_STATUS.md` carries `RULES_AT: HM-DEC-161
(2026-09-11)`; the other scheme appears in the arbiter's own artifacts. **Nothing in
this repository resolves which is canonical**, and no unit should pick one without a
ruling.

**19.** *(was item 17)* **Files this environment cannot delete.** Now five, listed for
Tim in section 2: `commit-msg-326.txt`, `toolsarbitervalidate-output.bat`,
`tools\arbiter\unit323-append.bat`, `tools\arbiter\unit323-append.py` and this
session's own `tools\cut-header-action.py`.

**All other items stand as unit 328 carried them.**

#### Where the carried items stand after this unit

- **Unit 332 item 1, the `Why` hovers:** not drawn on the rebuilt page before a PSK31 contact.
  Every hover on every visible control was read in task 1 and none names PSK31, so it stays
  parked.
- **Unit 332 item 6, step 5 partial or done:** answered above.
- **Carried item 1 and 10, composed timestamps:** not repeated. Every `UPDATED` in this unit is
  a `date` reading, pasted.
- **Every other item stands as carried.** Nothing in this unit touched the 1400 split, States,
  `first_answer_to_own_cq`, `Digital`, the demodulator vouch, the idle fixture, the ALC margin,
  the two id schemes, the five files or step 6.

### Where the carried items stand after unit 334

- **Unit 333 item 3 and unit 331-queue item 19, the undeletable files:** now **thirteen**, and
  listed once in section 2. `Unit333ProbeTests.cs` is tracked, not untracked. None was left
  unemptied.
- **Unit 332 item 1, the `Why` hovers:** unchanged. The FT4 and PSK31 sentences are still at
  `AchievementsViewModel.cs` lines 501 to 517, and both are now false. Task 2 named only the one
  sentence it checked.
- **Unit 332 item 4, the license phrase at 1400:** still three lines on the test host. The left
  block is now 232 px, against 253, and the phrase breaks in the same places.
- **Unit 332 item 5 and unit 333 item 3, the status helper:** still refused, now in three
  spellings; see item 3 raised above. No timestamp was composed.
- **Every other item stands as carried.** Nothing in this unit touched the radio side, the
  achievements rulings, States, the 1400 decoded-list split, the demodulator, the ALC margin, the
  id schemes or PSK31 step 6.
