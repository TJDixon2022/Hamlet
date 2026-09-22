# Work instruction 389 - step 9 proved at HEAD so it can be graded, and the sun map to the band's left edge

**Two things tonight, and the first matters more.** **Step 9 is five criteria about the contact
Tim actually had with KC3FL.** He keyed on top of a station that was still sending, and he had
no way to log the contact. Unit 385 built all five fixes and claimed all five met at `e5e4bee0`.
**No judging session has ever read that claim**, because the launcher graded the wrong report
(the correction is appended at `PHASE_OUTCOME.md:492`). So the plan still says `not started` on
work that landed a night ago. **Tonight's first job is to measure each of 9.1 to 9.5 against
HEAD, character for character against the criterion's own words, and report what is true.**
That includes a shortfall, and one is already visible from the tree (section 5).

**The second job is 10.3's last 36 px.** Unit 388 measured the one layout that reaches them:
the map at the band's left edge. It declined to build it because moving the map felt like Tim's
call. **`PHASE_PLAN.md` section 6 makes a layout the arbiter's, and the judging session said so
in as many words** (`PHASE_OUTCOME.md:597`). I have decided it, as the author, and Tim can
overrule it. **This task is the named drop candidate.**

**The step you were handed was step 0. This unit does not touch it, and section 4 explains why
in one table.**

**Status.** `sh tools/status.sh`, real clock, after every commit and every task, and immediately
before every test invocation. **The `sh` is part of the command. Read section 2 before you type
it.**

---

## 0. The project gate

```
SHACK_FACTS.md                                          must exist
src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs     must exist
CoreHMI.sln                                             must not exist
MURC.sln                                                must not exist
root                                                    C:\Source\HamLet
```

**If any of the four is wrong, stop and say so in `output.md` section 4. Write nothing
else.** The refusal text: *This is not Hamlet. Nothing was changed.*

*All four were checked against the tree at authoring time and all four hold:
`SHACK_FACTS.md` and `src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs` are present,
`CoreHMI.sln` and `MURC.sln` do not exist at the root, and the root is `C:\Source\HamLet`.*

---

## 1. The rules that killed sessions

**HM-DEC-155.** Never run the whole suite. Run only `docs\carry-forward-tests.txt`, the way its
own top comment says: **two invocations, one build each**, with a status line written
immediately before each. Never run in the background and poll. **Unit 323 ran one `dotnet test`
per name, which was forty-one builds. It wrote one status line, and the watchdog killed it at
twelve minutes of silence.**

**The app invocation is the flaky one, and you should expect it to fail that way.** The headless
`InvalidProgramException: You've caused dispatcher loop` kills a run at about **1 ms, before any
assertion**, and it hits a different name each time. **It has never once touched the engine
invocation.** **It is not a red. It is a lost invocation. Re-run it once, record both attempts,
and do not chase it.** A failure at about 1 ms, before any assertion, is the session fault.
**Anything that ran and disagreed is a red, and it is yours.** Unit 388's exit also saw one
**test-host crash with no cause found**, and a **file-lock red** on
`TheOliviaRowsTests.WithRowsPresentNothingIsComposedUntilAPressAndEachPressCarriesItsRowsVariant`
at entry. If you see either one again, record it and do not chase it.

**Put any loop in a script file**, because `;` is refused in a compound command. `Bash(sh:*)` is
granted, so `sh .run-unit/unit389-<name>.sh` works. **Write the script with the editor.**

---

## 2. The tool facts

- **Type the status line as `sh tools/status.sh ...`, with the `sh`.** A permission rule is a
  literal prefix match over the whole command string. `tools/status.sh ...` and
  `./tools/status.sh ...` are both refused.
- **Run `git status` with no `-C`.** You are already standing in the root.
- **`git log`, `git log -S`, `git show`, `git diff` and `git rev-parse` are granted.**
  **`git stash`, `git worktree add`, `git checkout <ref> -- <paths>`, `git mv` and `rm` are
  refused.** You cannot run the pre-unit tree, and a revert is done by hand.
- **Shell output redirection (`>`) is refused for every path.** Write files with the editor.
  Read a test run's output from the console, or pipe it to `grep` in the same command.
- **A compound command with `;`, `&&` or a second operation is refused**, and so is
  `cd X && Y`.
- **Apostrophes inside quoted heredocs break them, and doubled backslashes collapse to one.
  For a multi-line commit message, pass `-m` more than once.**
- **Write this repository's files as UTF-8. `PHASE_OUTCOME.md` is BOM+CRLF. Edit it with the
  editor and do not normalise it.**
- **`validate-output.bat` is at `tools/arbiter/validate-output.bat`.** Run it as
  `./tools/arbiter/validate-output.bat output.md`: forward slashes, a leading `./`, one command,
  no `cd`, no `cmd /c`. **Run it before you say the report is written.** If it answers *This
  command requires approval*, that is the permission mode. Hand-check the report against the
  script's own header, say in section 4 that it was a hand-check and not a run, and move on.
  Nine units in a row have hit this.

---

## 3. Asks still outstanding

These are carried per HM-DEC-139, **verbatim in section 4**.

**The queue is unit 388's report, `output.md` at the root at `3d980458`. Read it; do not
reconstruct the queue from memory.** You answer none of the asks. Four of them touch tonight,
and each gets one line in your section 4:

- **Unit 388's item 1: the map at 1400 needs the pills row to stand elsewhere.** *This is
  answered tonight by the arbiter's decision in section 6 ruling 2, as author and overrulable.*
  Say so in one line, and give task 3's numbers.
- **Unit 388's item 2: `CardFloor = 400` and the caption's place.** These are still unit 388's
  choices. Leave the floor as it is. If ruling 2 moves the caption, report where it went.
- **Unit 388's item 5: `TheTopRowTests` is not on carry-forward line 7, although 10.5's ticked
  text says it is.** Count it again and report the number. Do not repair it.
- **Unit 387's item 1: *make a card anyway* is a note on a row that names nobody.** This is
  still Tim's. Tonight's step 9 measurement reads cards that name someone, so it does not touch
  that note.

**The reload's disagreements.** Each gets one line, and none of them is yours to repair:

- **The `RULES_AT` split, now the twelfth unit running.** `PROJECT_STATUS.md` reads `HM-DEC-165
  (2026-09-19)` and `CLAUDE.md` section 1 holds `CPS-DEC-0165`. The value is written as a
  literal by `tools\status.sh`, and **you may not edit `tools\`**.
- ***The PHASE_OUTCOME header says step 6 is done; the last entry for it says partial.*** That
  is R42: Tim closed step 6 by ruling after the judging session. The header is right, and the
  entry is history.
- **`PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `RUN_LEDGER.md` are uncommitted.** They carry the
  launcher's and the judging session's writes about unit 388 (`PHASE_OUTCOME.md:584-599` is
  the judged `## UNIT 1 - STEP 10`). **Read `git diff` on each of the three.** Where a diff only
  appends entries or rewrites header and ledger lines, commit it **unaltered** with task 0.
  Where it does anything else, report it and leave that file out of the commit. **Do not edit a
  judging session's line.** Leave the `.run-unit/` files alone. They are not yours.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet holds what it has - the hardening phase: screen, record and test
            work banked in the PSK31 and Olivia threads that needs neither the radio
            nor the owner, judged by tests that ran and, at the end, by Tim at his
            window.
UNIT GOAL:  Put step 9 in front of a judging session for the first time, with
            each of its five criteria measured against HEAD in the criterion's own
            words; close the one shortfall that sits off the send path; and give
            the sun map the band's full 214 px by standing it at the band's left
            edge, where the pills keep one row.
ADVANCES:   step 9, criteria 9.1 to 9.5; and step 10, criterion 10.3 - task 3,
            the named drop candidate.
DRIFT:      none.
```

**Today's count** comes from `PHASE_PLAN.md` at HEAD `3d980458`: **37 ticked and 10 unticked**.
The unticked ten are 0.1, 4.3, 5.1, 7.2, 9.1 to 9.5 and 10.3.

| Step | State | Units spent | Where it stands |
|---|---|---|---|
| 0 | `partial` | 369, 371's carried repair | 0.2 to 0.5 met. **0.1 is cut down and closed to units by instruction 386 ruling 1** |
| 1, 2, 3, 6, 8 | **`done`** | | Judging session `done`, or R42 |
| 4 | `partial` | 382, 383 | 4.3 is cut down by instruction 384 and logged to the owner |
| 5 | `not started` | 0 | **Tim's own, and it ends the run** |
| 7 | `partial` | 378, 383 | 7.2 is met on unit 383's own account and **unjudged** |
| 9 | `not started` in the header | **385** | **The work landed at `e5e4bee0` and was never graded. This unit** |
| 10 | `partial` | 387, 388 | 10.1, 10.2, 10.4, 10.5 and 10.6 met. **10.3 is 178 of 214 px. This unit, task 3** |

### Why not step 0, which was the step handed to me

| # | What was tried at 0.1 | What it hit |
|---|---|---|
| A | Unit 369 searched the 119 commits of `681d45c8..ec4b466e` over the four settings paths | **Zero** commits. The search was complete and the answer was negative |
| B | Instruction 372 ruled that negative as meeting 0.1 | The judging session returned `partial`, because the report *"names none"* |
| C | Instruction 386 re-measured wider, over every path containing `settings` | **Zero** again. **Cut down, closed to units, and the remedy logged to Tim** |

**Instruction 386 ruling 1 says no unit in this phase searches that window again. No arbiter
may overrule an earlier arbiter's ruling. Only Tim can, and he has not.** I measured one thing
before I let this go. **Unit 369's own section 4 item 2 records that the refusal Tim saw was
`transmit_device_would_not_open`, which means the saved device id survived.** In that same
window, `MainWindowViewModel.cs` changed by 1,452 lines, and `UnslottedTransmission.cs` and
`Ft8TransmitSequence.cs` changed too. **The drop may not be in the settings at all.** It may be
where a saved id is resolved against what the OS offers, or where the device is opened.
Instructions 387 and 388 logged that same shape to Tim. **It stays logged to him, and I add
this line to the log.** It is a search of the window that ruling 1 closes, and the path it
would read ends at the device that keys. That makes it doubly not a unit's to open.

### Why step 9 is reachable tonight, and was not ruled closed

Instructions 386 and 387 each said that 9.1 to 9.5 *"are not re-proved ... and are not ticked
**by this unit**"*. **Both clauses are scoped to their own unit, so neither is a standing
ruling, and working step 9 tonight overrules no one.** Step 9's entry reads *step 8 done*, and
step 8 is done.

### Why this is not unit 385's approach again

Unit 385 **built** the four fixes. **Tonight builds nothing on the send path.** It measures what
unit 385 built, in the criterion's own words, and writes the report a judging session reads.
Where the words and the tree disagree off the send path, the unit closes the gap. Where they
disagree on the send path, it reports the gap. The loop test on *re-measure step 9 criteria at
HEAD from the KC3FL replays* returned **NOT FOUND**.

### Why 10.3 this way is not unit 388's approach again

Unit 388 stood the map **between the card and the rig face** and ran it up beside the pills.
At 1400 that left the pills 343 px of the 675 they need, so they wrapped and the band went to
252. **Tonight the map stands at the band's left edge**, so the pills keep the whole width to
its right. **Unit 388's task 1 measured 961 px there at 1400**, and the break-even is a row of
about 1082 px, which is a window of about 1114. The loop test on *sun map at the band's left
edge spanning the pills row* returned **NOT FOUND**. I judge it a different place for the map,
not a rewording of unit 388's approach.

### What this unit is worth, in Tim's terms

**He answered KC3FL, sent his report on top of KC3FL's live carrier, watched the second
hand-back not move the card, and could not log the contact.** A unit fixed all four, and
nobody has checked the claim. Tonight someone checks it, and tells him plainly which of the
four holds. **And his sun map fills the band he gave it.**

---

## 5. Verify this instruction against the tree

Every line below was read at authoring time. **Report every mismatch in sections 1 and 4 of
your report. Repair nothing except what is this unit's.** Line numbers move under an edit, so
the name is the thing that matters. **Where your measurement disagrees with mine, yours wins
and it is a finding.**

### The record

`Directory.Build.props` line 1221 reads `<Version>1.13.75</Version>`. `PHASE_STATUS.md` reads
`CURRENT_STEP: 0` and `WORK_INSTRUCTION: 388 - ...`, and both are stale. `HEAD` is `3d980458`.
`docs/carry-forward-tests.txt` is 866 lines. **Unit 388's exit was app 265 of 265 and engine 150
of 150, with line 7 at 62 filter terms and line 9 at 25.** Use the lines unedited, except where
a section 6 ruling edits a name in the same commit as the test it names.

### For step 9: what unit 385 left, and the shortfall I can already see

| Where | What is there |
|---|---|
| `e5e4bee0` `output.md` | Unit 385's claim that all five are met. **Read it. It is the claim you are checking, not evidence** |
| `ad3b70ab` | The four step 9 types went onto line 7. `TheFourAreOnOliviaCardsTooTests` is one of them. **Find the other three by reading that commit's diff to the line** |
| `tests/Hamlet.App.Tests/ViewModels/` | `TheCarrierHoldsTheButtonsTests.cs`, `ThePsk31ConversationCardTests.cs`, `ThePsk31ReadsTheConversationTests.cs`, `Unit385Trace.cs` |
| `src` | **`card_dismissed` occurs 0 times.** Unit 385 wrote the press as *"the existing `card_cleared` and not a second spelling"*. **9.1 names `card_dismissed`**, so section 6 ruling 1 item 3 applies |
| `src/.../MainWindowViewModel.cs` | `his_carrier_live`, at the mode gate above the only composer |
| Unit 385's 9.2 claim | *"3 of the four controls held with one sentence and the fourth already withheld by R1"*. **9.2 names four controls, each greyed with *he is still sending*. Measure each of the four and say which way each one is held** |

### For 10.3: the band and the map

| Where | What is there |
|---|---|
| `MainWindow.axaml:3024` | `TopRow`, `Grid.Row="1"` |
| `MainWindow.axaml:3042` | Unit 388's `ctl:BandGovernsTheMapPanel x:Name="BandRow"`, with the map at `:3070` `GreenZoneGrayLine` between the card and the rig face |
| `MainWindow.axaml:3404` | The band-pills `ItemsControl`, `Grid.Row="0"`, a **full-width row above** `TopRow` and a non-wrapping horizontal `StackPanel` |
| `Unit376TheTopBandTests` | `BandReachedWithThePills = 214`, the ratchet. `TopBand` runs from the top of the pills to the lowest of card, rig and `RigDriveAndPower` |
| Unit 388's trace, `PHASE_OUTCOME.md:576` | Pills need 675 px. The row is 1888 px at 1920 and 1368 at 1400. A 214-tall map is 392.9 px wide at aspect 1.8358. **With the map at the left edge the pills have 961 px at 1400. Break-even is a row of about 1082** |
| Unit 388's stage A, `:578` | The map is 327 x 178 at 1920 and 1400. The card is 987 and 467 wide. The caption is in the card's Favorites row. The nine sizes read `483, 71, 92, 163, 171, 267, 483, 460, 860` |

### What will be red before you start

The inherited reds are unit 383's three and unit 387's eight `TheMenuIsUnderTheMouseTests` names
(0 of 8 at unit 388's entry and exit), plus the known-red block in
`docs/carry-forward-tests.txt`. **None of them is on the list.**

---

## 6. Rulings in force

**These are transcribed in full. Do not re-argue any of them.** Two are mine, as author, and are
overrulable under R31, which allows two a unit. The rest belong to the plan and the project.

### The first is mine: what re-proving step 9 may and may not change

**Author's and overrulable.** This is licensed by `PHASE_PLAN.md` step 9, R44, and section 6's
*"anything would change what goes on the air, or what keys: `MOVE: stop`"*.

1. **Each criterion is measured in its own words, clause by clause.** Report each clause as
   **met**, **met differently** (with what the tree does instead), or **not met**, and give the
   number or quoted line behind it. **A clause that unit 385 met "by identity" or "already by
   R1" is reported that way, word for word.** You may not round it up to met.
2. **Nothing on a send path changes tonight.** That covers the hold, `his_carrier_live`, the
   gate, the composer, `Arm`, `PttOn`, every command that sends, and whether a control is
   enabled. **A shortfall there is reported with its number and left alone.** It is not
   repaired and it is not argued. That is the stop's safe side, and a judging session can grade
   a partial.
3. **Off the send path, a shortfall that is small and inside the criterion's words is
   closed.** The one I can see is **9.1's `card_dismissed`**:
   - The dismiss press writes `card_dismissed`, the token 9.1 names, through the writer that
     writes `card_cleared` today.
   - **Measure first where `card_cleared` is written.** If the dismiss press is its only
     writer, rename it in place so there is one spelling, the plan's. If anything else writes
     it, leave it for those other writers and give the press the plan's token.
   - Either way, change `CallsignPrivacyTests`' walk and every test that reads the token **in
     the same commit**, and write nothing personal (HM-DEC-018 section 2.1).
   - The same rule covers a word on a card, the Log fields and the turn word: close it if it is
     small and off the send path. **Anything bigger is reported, not built.**
4. **Replays come from the record's own moments, as the criteria name them**: 17:45:40 to
   17:45:44 for 9.2 and 17:48:33 for 9.4. **Use the fixtures unit 385 built, if they are those
   moments. If they are not, say what they are instead.**
5. **Tick in `PHASE_PLAN.md` only the criteria whose every clause is met**, each with tonight's
   numbers beside it in the plan's own text, as unit 388 did for 10.6. **Leave the header's step
   9 state word alone.** Grading is a judging session's job.

### The second is mine: the sun map stands at the band's left edge

**Author's and overrulable.** This is licensed by `PHASE_PLAN.md` section 6: *"a layout, a
number ... decide, mark author's, continue."* The judging session's reading at
`PHASE_OUTCOME.md:597` is that the 36 px *"wait only on a layout question about where the pills
sit, which is not one of the three stops"*.

1. **The map moves from between the card and the rig face to the band's left edge.** It runs
   from the top of the pills row to the bottom of `TopRow`, **the band's full 214 at 1920 and
   at 1400**, at its own proportions (HM-DEC-092).
2. **The pills keep one row and stay in their own order.** They sit to the map's right, above
   the card and the rig face. **No pill is hidden, clipped or reordered.** The card sits between
   the map and the rig face. **The rig column's width and place are untouched.** The map's width
   comes from the neighborhood card, which is what 10.3 asks for.
3. **Below the break-even width, the map falls back to unit 388's stage A shape**: as tall as
   `TopRow`, beside the card, with the pills row full-width above. **This fallback is what keeps
   the pills in one row at 1100 x 780.** The switch is decided by measured width, never by a
   magic number. **Report the window width at which it switches.**
4. **10.3 is met when `GreenZoneGrayLine`'s drawn height equals the band's `WithPills` height
   within the band's own outer margins**, with each margin reported by name and in px, at 1920
   and at 1400, **with the band still 214**. The dot and the caption are kept, and the caption's
   words are unchanged, **wholly drawn and on the window**. Where the caption goes is your
   decision, and you report it.
5. **`TheSunMapIsTheSizeItWasAndStillCarriesHisGrid` is rewritten under R12 a third time** to
   assert this: *the map's height is the band's at 1920 and 1400, the band is still 214, the
   pills are one row, and the fallback holds at 1100 x 780*. **Keep the 246 x 134 floor and
   every assertion about the grid marker and the caption.** Tests that assert the map sits
   *between the card and the rig face* (unit 388 rewrote `TheTopRowTests.TheWorldClockIsAtThe
   CardsRightEndWithOneMarker` and `TheGreenZoneTests.NoBandPillIsOnTheGreenZoneAndTheMapTook
   TheirWidth` to that) are rewritten under R12 to assert the new place. **Name every test you
   rewrite.**
6. **If the pills wrap at 1920 or 1400, or the band grows by one pixel, revert task 3 in the
   same task by hand**, leave stage A standing, and report the numbers. **A band at 215 is not a
   permitted outcome. 10.3 staying partial is.**

### The plan's own, which tonight is built on

**9.1** *Every conversation card and every receipt has a dismiss X; dismissing removes it and
writes `card_dismissed`.* **9.2** *While a station's carrier is on the air his row and his card
carry a color and the word* sending*; Report, Confirm, the canned lines and the typed line are
held - greyed with* he is still sending *- until his carrier drops, and a send attempted during
it is refused with that sentence and `send_refused reason his_carrier_live`; replayed from the
17:45:40-17:45:44 record as a fixture, Tim's Report would have been held.* **9.3** *Log is on
every conversation card from the moment it exists, with the RST fields editable and defaulted to
what was exchanged if anything was; a logged contact with no certain 73 logs what is known and
nothing invented.* **9.4** *Every parsed line addressed to the operator with a hand-back moves
the card's turn to* your turn *- marked as a guess when uncertain - not only the first answer;
replayed from the 17:48:33 record, the card reads* your turn?*.* **9.5** *The four are on PSK31
and Olivia cards alike, asserted.*

**10.3, rev7, verbatim:** *The sun map grows to the top band's existing height at 1920 and at
1400 - the band itself does not grow, 214 px stands - keeping its aspect; the width it needs
comes from the neighborhood strip beside it; its dot and caption kept. (Tim, 2026-09-22: vertical
space is the concern, not horizontal; unit 387's reading that the map governs the band is
superseded - the band governs the map.)*

**Instruction 386 section 6 ruling 1, in one sentence:** *0.1 is unachievable as written and
step 0 is cut down at partial; 0.1 stays unticked and is never ticked by a unit of this phase; no
unit in this phase searches that window again; the remedy - rewording 0.1 so a completed negative
satisfies it - is the owner's and is logged to him.* **Instruction 384 section 6 ruling 2 item 1:**
*step 4 is cut down at partial and no unit in this phase works 4.3 again.*

**R44** is the KC3FL contact. **R46(c)** says the sun map takes the band's full height. **R42**
says 214 stands. **R34** says the send area never leaves the window. **HM-DEC-092** keeps the
map's proportions. **R1** is the certainty gate. **R11** means nothing at the radio. **R12** says
a session rewrites its own tests. **R13** requires telemetry on every stage. **R14** allows
nothing beyond the criterion. **R19** means American. **R31** covers unattended running: criteria
by id, a done step closed, the owner's step ends the run, and **two rulings a unit**.
**HM-DEC-018 section 2.1** keeps anything personal out of an event. **Section 0.0** says a
sentence on the screen is a claim.

**`PHASE_PLAN.md` section 6:** *Three stops only: keying, transmit or the radio's safety; money
past the budget; a fact the product states to the operator about the radio, a contact or a
send. A hint, a label, a number, a layout, a test's shape, a mechanism arithmetic will not
allow: decide, mark author's, continue. The later ruling wins. A done step is closed. A
must-pass missed by a little: ship, report, `partial`, move on. **Never loosen a test.***
**Tonight stays clear of the stops because ruling 1 item 2 keeps it off the send path.**

---

## 7. Status cadence

Run `sh tools/status.sh` on the real clock **after every commit, after every task, and
immediately before every test invocation**. Name the task and what is running. **The watchdog
kills at twelve minutes of silence, and the app invocation alone runs about 2 m 40 s.**

---

## 8. The tasks

**There are five tasks. The trace is task 1, and it builds nothing under `src`. The named drop
candidate is task 3**, 10.3's left-edge map. If the night runs short, drop it in one line and
go to task 4. **Step 9's measurement is not the drop candidate. It is the reason this unit
exists.**

### Task 0: the record and the entry run

- Bump the version **1.13.75 -> 1.13.76**, with its line in the version log.
- Set `PHASE_STATUS.md` to **step 9 and unit 389**.
- Commit the three launcher files under section 3's rule.
- **`PHASE_PLAN.md` ticks nothing at this task.**
- Append `## UNIT 389 - STEP 9` to `PHASE_OUTCOME.md` in the house shape. It carries this
  instruction's `STEP`, `ADVANCED`, `APPROACH`, `MOVE`, `WHY`, `DECIDED`, `LICENCE`, `COST` and
  `ACCOMPLISHED`, and an `ENTRY:` line with the entry round's real numbers.

**Then run the entry round:** both command lines, unedited, one build each, with a status line
before each. **Expect app 265 of 265 and engine 150 of 150.** If the app attempt is lost, re-run
it once. **A carry-forward name that is red on an assertion here is inherited. Name it. It does
not stop the night.**

**Commit.**

### Task 1: the trace, where each step 9 criterion stands today

**No file under `src` changes.** Every *before* number comes from here.

1. **Find step 9's guards on line 7**, by name, from `ad3b70ab`'s diff. **Run those four types
   once, together, in one build**, and record each name's result.
2. **Make a clause-by-clause table for 9.1 to 9.5** (section 6 ruling 1 item 1). For each
   clause, give the test and the assertion line that proves it, or write *no assertion*. For
   9.2, **the four controls, one row each**: held or not, greyed or not, and the words on each.
3. **The token:** every writer of `card_cleared`, with file and line, and whether the dismiss
   press is the only one (ruling 1 item 3).
4. **The replays:** what fixture 9.2 and 9.4 use, and whether it is the 17:45:40 to 17:45:44 and
   17:48:33 record the criteria name.
5. **For 10.3: the width a left-edge map leaves the pills, at 1920, 1400 and 1100 x 780**, and
   the break-even window, measured on today's tree, not copied from unit 388.

**Commit** the trace's record only.

### Task 2: step 9, measured and closed where ruling 1 allows

- **9.1's token**, under ruling 1 item 3, with its tests and the privacy walk in the same
  commit.
- **Any other off-send-path shortfall** that task 1 found, closed under the same rule. Each one
  is named.
- **Every clause measured again after the change.** The table from task 1 item 2 is filled in
  as *before* and *after*.
- **A send-path shortfall is left exactly as it stands**, and reported with its number (ruling
  1 item 2).

**Commit.**

### Task 3: 10.3, the map at the band's left edge. THE DROP CANDIDATE

This is section 6 ruling 2, all of it. **Measure the map, the band, the pills' row count, the
card's width and the caption at 1920, 1400 and 1100 x 780, on FT8, PSK31 and Olivia.** Measure
**unit 354's nine sizes** against `483, 71, 92, 163, 171, 267, 483, 460, 860`. **10.6's
drop-down still has to be in the card and still has to tune.** If the pills wrap or the band
grows at 1920 or 1400, revert by hand in the same task and report the numbers.

**Commit.**

### Task 4: the exit run, the record and the report

1. Run the exit round of both invocations, one build each, with a status line before each.
   **Compare it name for name against task 0.**
2. Run `TheMenuIsUnderTheMouseTests` and `ViewTestsActThroughControlsTests` once each, and
   report both counts.
3. Update `PHASE_PLAN.md`: **tick only what tonight earned**, each with tonight's numbers beside
   it. That means **9.k only where every clause is met, and 10.3 only if task 3 held.** Tick
   nothing in steps 0, 4 or 7.
4. Write the `STEP: 9` and `STEP: 10` lines in `PHASE_STATUS.md`.
5. Write `output.md`.
6. Run `validate-output.bat`.
7. Push.

**Leave the header state words alone.** Closing a step is a judging session's job.

---

## 9. Parked: do not touch, do not raise

- **Criterion 0.1, the 1.13.30-to-1.13.48 window, and the device resolve and open path.** They
  are covered by instruction 386 ruling 1 and logged to Tim (section 4).
- **4.3 and `docs/RADIO_SHEET.md`, including every string it quotes.** If a string you would
  change is quoted there, do not change it. Raise it as a section 4 item.
- **7.2.** It is met on unit 383's own account and waits for a judging session.
- **10.1, 10.2, 10.4, 10.5 and 10.6.** They are met and ticked.
- **The cause of the dispatcher loop, and unit 388's test-host crash.** Record every occurrence
  and chase none.
- **`tools\`, including the `RULES_AT` split.**
- **The two false outcome rows** (the Olivia phase's `## UNIT 2 - STEP 1` and this phase's
  `## UNIT 7 - STEP 2`). Leave both exactly as they are.
- **Renaming `TheFavoritesAreBackOnTheRigDisplayTests.cs`** (unit 388's item 7). `git mv` is
  refused.

---

## 10. What not to do

- **Do not touch a send path, a hold, a gate, a modulator, an `Arm` site, a `PttOn` site, or
  whether a send control is enabled** (ruling 1 item 2). Tonight's step 9 work only measures
  these. It does not change them.
- **Do not round a step 9 clause up.** *Met by identity* and *already withheld by R1* are
  reported in those words.
- **Do not tick a 9.k with an open clause.**
- **Do not let the band grow by one pixel past 214** at 1920 or 1400. **Do not let the pills
  wrap there, and do not hide, clip or reorder a pill.**
- **Do not take the map's width from the rig column**, and do not shrink or move the rig face.
- **Do not stretch or crop the map** (HM-DEC-092).
- **Do not delete or skip a carry-forward name.** Rewrites and renames go in the same commit as
  the line edit, and the line's count may not fall.
- **Do not write a property a control owns from a view test.** Act through the control with the
  headless pointer, and move it before you press.
- **Do not put anything personal in an event** (HM-DEC-018 section 2.1), and do not open Tim's
  own settings file.
- **Do not run a suite** (HM-DEC-155).
- **Do not call a red environmental because you would like it to be.**

---

## 11. Committing and pushing

**Make one commit per task.** **A carry-forward line edit or a privacy-walk change goes in the
same commit as the name or writer it is about.** A task 3 revert is its own commit, stating what
it reverted and why. **Push once, at the end.**

---

## 12. Reporting

Write `output.md` at the root. **The canonical headings are** `## 1. What Claude did`, `## 2. What
the owner should expect`, `## 3. What you should see` and `## 4. What's blocking us`.

**The ordering block goes first. `validate-output.bat` refuses a report without it.** Fill every
line from what you measured.

```
READ IN THIS ORDER.

A. The phase goal - Hamlet holds what it has. Steps 1, 2, 3, 6 and 8 done; step 0
   partial with 0.1 closed to units by instruction 386; step 4 partial with 4.3
   logged to the owner; step 7 partial on an unjudged 7.2; step 5 Tim's own and it
   ends the run. Step 9's work landed at e5e4bee0 and had never been graded; step 10
   partial on 10.3 alone, the map 178 of the band's 214.
B. Step 9 - each of 9.1 to 9.5 <met | met differently: how | not met: which clause>,
   the four 9.2 controls <n of 4 greyed with "he is still sending">, card_dismissed
   <0 -> n writers>, the 17:45:40 replay <held | not>, the 17:48:33 replay reading
   <words>. Step 10 - 10.3 <met | partial | dropped>: the map <w> x <h> in a band of
   <h>, the pills <1 | n> rows at 1920 and 1400, the fallback switching at <w> px.
C. The report last. Section 4 raises <N> items on top of the carried queue, and
   <none of them is | item <k> is> in the way of a criterion in B. Say in one line
   whether any step 9 shortfall sits on the send path and was therefore left.
```

```
UNIT:       389 - <complete|stopped> at task N of 5 - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   step 9, criteria <ids>; step 10, criteria <ids or none>
NUMBER:     step 9 clauses met at HEAD: <unit 385's claim, as a count> -> <measured
            count> of <total>; the sun map's share of its band: 83.2% -> <n>%
DRIFT:      none
```

**Section 3 leads with step 9's clause table, before and after**, one row per clause, with the
test and assertion line that proves it or *no assertion*. Then come 9.2's four controls, the
`card_cleared` writers, and the two replays, quoted. **Then the band table at 1920, 1400 and
1100 x 780**: pills and their row count, the map, the card, the caption, `WithPills` and
`WithoutPills`. Then the nine sizes against unit 381's. Then the entry and exit rounds, name for
name, and the carry-forward line's count before and after.

**Section 2 tells Tim in one paragraph, in his terms, which of the four things from the KC3FL
contact now hold on his screen and which do not, and why any that does not was left.** Add a
line on the map. Every claim is computed, not seen (FACT-004). Include one line saying that no
port was opened, nothing was enumerated and nothing was keyed.

---

```
ARBITER-DECISION
STEP: 9
APPROACH: re-measure step 9's five criteria at HEAD clause by clause from the KC3FL replays, close only off-send-path shortfalls such as 9.1's card_dismissed token, and stand the sun map at the band's left edge spanning the pills row so it reaches the band's full 214
MOVE: work around
WHY: The step handed over was 0. Its only open criterion, 0.1, is closed to units by instruction 386 ruling 1, which no arbiter may overrule. Step 9's five criteria (PHASE_PLAN.md step 9, R44) landed at e5e4bee0 and have never been graded because the launcher graded the wrong report, so measuring them is the largest block a unit can move. 10.3's last 36 px wait only on a layout decision, which PHASE_PLAN.md section 6 makes the arbiter's.
STATE: not started
DECIDED: Author's and overrulable. Two rulings, transcribed in work instruction 389 section 6. (1) Re-proving step 9: every criterion is measured clause by clause in its own words, and a clause met by identity or by R1 is reported that way, never rounded up. Nothing on a send path changes: the hold, his_carrier_live, the gate, the composer, Arm, PttOn and whether a send control is enabled all stay as they are, and a shortfall there is reported, not repaired. A small shortfall off the send path is closed. The first is 9.1's card_dismissed, which appears 0 times in src: the dismiss press writes the plan's token, and card_cleared is renamed in place if the press is its only writer, with the privacy walk and every reader in the same commit. A 9.k is ticked only when every clause is met, and the header state word is left for a judging session. (2) The sun map stands at the band's left edge, from the pills' top to TopRow's bottom, 214 at 1920 and 1400. The pills keep one row in their own order to its right, and the rig column is untouched. Below the measured break-even width the map falls back to unit 388's stage A shape. TheSunMapIsTheSizeItWasAndStillCarriesHisGrid and the tests unit 388 moved to the between-card-and-rig place are rewritten under R12. Task 3 is reverted by hand if the pills wrap or the band grows. Also decided, and not a ruling: step 9 and 10.3 over the handed step 0, and the device resolve-and-open shape of 0.1 stays logged to Tim, because it searches the window ruling 1 closes and ends at the device that keys.
LICENCE: PHASE_PLAN.md step 9 and R44; step 10 rev7 criterion 10.3 and R46(c); R42, 214 stands; R34 and step 1's rules; HM-DEC-092; PHASE_PLAN.md section 6 - a layout is the arbiter's, anything that changes what keys is a stop, never loosen a test; the judging session's STATE_WHY at PHASE_OUTCOME.md:597; ARBITER.md section 3's work around, section 4 whose readings on both approach halves returned NOT FOUND, section 6, and section 8's rule that STATE_AFTER comes from a judging session; work instruction 386 section 6 ruling 1 and 384 section 6 ruling 2 item 1, not reopened; instructions 386 and 387 on step 9, both scoped to their own unit; R1, R11, R12, R13, R14, R19, R31 two rulings a unit; HM-DEC-018 section 2.1, HM-DEC-139, HM-DEC-155, HM-DEC-165; FACT-004
ACCOMPLISHED: Tim learns, from a measurement rather than a unit's own word, which of the four things that went wrong in his KC3FL contact now hold on his screen: the X, the hold while a station is still sending, Log from the start, and the turn moving on every hand-back. The one record word that did not match his plan is made to match, and anything on the send side that falls short is named rather than touched. His sun map fills the whole band he gave it, with the pills still in one row.
ADVANCES: step 9 criterion 1, step 9 criterion 2, step 9 criterion 3, step 9 criterion 4, step 9 criterion 5; and step 10 criterion 3, with task 3 (10.3's left-edge map) the named drop candidate.
END-ARBITER-DECISION
```
