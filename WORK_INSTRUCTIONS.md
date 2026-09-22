# Work instruction 385 - the contact Tim had: an X, a carrier that holds the buttons, Log from the start, and a turn that moves every time

**Step 9 of the hardening phase, and the whole step: R44.** Six tasks, one more than the
usual five, because the step carries five criteria and **no unit has ever been spent on
it**. Task 1 builds nothing and repairs nothing: three of these five criteria are claims
about things the tree may already do - the card already has a *He is still sending* word,
the row already knows whether its carrier is live, the log already knows how to write a
half exchange - and **what task 1 measures decides what tasks 2, 3 and 4 are allowed to
touch** (R14).

**This is the step Tim wrote from his own contact**, at 17:44-17:48 UTC on 2026-09-21,
four hours before this instruction. He answered a station. He sent the Report on top of
the man's live carrier. The man's 363-character reply came back garbled. His second
hand-back did not move the card. And there was no way to log him. **Those are the four
things, and they are the whole step.**

**Status.** `sh tools/status.sh`, real clock, after every commit and every task. **Read
section 2 first - unit 384 lost three calls to the wrong spelling of that command.**

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

*All four were checked against the tree at authoring time and all four hold: `SHACK_FACTS.md`
and `src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs` are present, neither `CoreHMI.sln`
nor `MURC.sln` exists at the root, and the root is `C:\Source\HamLet`.*

---

## 1. The rules that killed sessions

**HM-DEC-155.** No suite; only this unit's names and `docs\carry-forward-tests.txt` as its
top comment says - **two invocations, one build each**, status written immediately before
each. Never background and poll.

**This unit's own runs are filtered and foregrounded too.** Task 1's measurement is one
`[Fact]` trace in the app test project, run by name. Tasks 2, 3 and 4 run their own named
types and the app types they turn red. That is not a suite and it is not a poll.

**This is app work, and the app invocation is the flaky one.** The headless
`InvalidProgramException: You have caused dispatcher loop` has now been seen **sixteen
times across five units**, always at about 1 ms inside
`HeadlessUnitTestSession.EnsureApplication`, always before any assertion, and never twice
on the same name. **Unit 384's entry round lost both app attempts to it** - attempt 1 read
225 of 226 with one occurrence, attempt 2 read 223 of 226 with three - and **not one of
the four was an assertion failure**. The engine invocation has not failed since unit 375
and read 150 of 150 that same night. **Expect it, re-run once, record both attempts, do
not chase it.** It is unit 375's item 3 and it is nobody's criterion tonight.

**A loop goes in a script file** (`;` is refused in a compound command).

---

## 2. The tool facts

**Two of these are new tonight and both cost unit 384 real calls. Read them before you
type a command.**

- **The status line is `sh tools/status.sh ...`, with the `sh`.** Unit 384 was refused
  three times running - twice as `tools/status.sh ...` and once as `./tools/status.sh ...`
  - because the granted rule is `Bash(sh tools/status.sh:*)` (`.run-unit\allowed.txt` line
  16) and **a permission rule is a literal prefix match over the whole command string**.
  Every instruction before this one wrote the command without its `sh`; that is the bug and
  this line is the fix.
- **`git status` with no `-C`.** Unit 384's fourth denial was
  `git -C /c/Source/HamLet status --short -- ...`. The granted rule is `Bash(git status:*)`.
  You are already standing in the root.
- **Shell output redirection (`>`) is refused to every path.** Write files with the editor.
  **A test run's output is read from the console**, or piped to `grep` in the same command.
- **A compound command with `;`, `&&` or a second operation is refused**, and so is
  `cd X && Y`. **Python runs here - as a file you wrote with the editor, run by name.**
- **Apostrophes in quoted heredocs break; doubled backslashes collapse; `rm` is refused;
  `-m` more than once for a multi-line commit.** Append to a file with the editor, not with
  `cat >> FILE <<'EOF'`. Patch a file with the editor, not with `python - <<'EOF'`.
- **Write this repository's files as UTF-8.** A PowerShell `>` redirect writes UTF-16 and
  the launcher cannot read it.
- **`validate-output.bat` is still at `tools/arbiter/validate-output.bat`** after the layer
  was replaced tonight (commit `74f640fa`), and the shape is forward slashes, a leading
  `./`, one command, no `cd` in front and no `cmd /c` around it:

  ```
  ./tools/arbiter/validate-output.bat output.md
  ```

  **Run it before you say the report is written.** Six units running it has answered *This
  command requires approval*, which is the permission mode and not the syntax. **If it
  refuses again, hand-check the six rules against the script's own header as unit 383 did,
  say in section 4 that what you did was a hand-check and not a run, and move on.**

---

## 3. Asks still outstanding

Carried per HM-DEC-139, **verbatim in section 4**.

**The queue stands at thirty-nine and you answer none of them.** It is the twenty-nine
unit 383 carried - unit 382's item 5; unit 381's item 1; unit 380's items 1 and 4; unit
379's items 1, 3 and 7; unit 378's items 1, 3 and 4; unit 377's item 4; unit 376's items 3,
4 and 5; unit 375's items 3 and 4; unit 374's item 3; unit 373's item 2; unit 372's items 4
and 7; unit 371's five; unit 369's four - **plus unit 383's own ten**.

**And here is a fact about the queue you must state rather than paper over: work
instruction 384 was deleted from the tree before this instruction was written** (commit
`6c6232cc`), so **its section 3 is not recoverable and no item is recorded as answered by
it.** Unit 383's report was deleted in the same commit and **is recoverable** - `git show
6c6232cc^:output.md` - and its section 4 is the list above. Read it; do not reconstruct the
queue from memory.

Where this unit stands on the three of unit 383's ten that its own work touches:

- **Unit 383's item 1 - 4.3 and the radio sheet.** **Cut down and closed to this phase** by
  work instruction 384 section 6 ruling 2 item 1. Two judging sessions have now returned
  `partial` on it and the only remedies left are deleting a quoted refusal, which 4.2's
  backward direction forbids, or changing a sentence Hamlet says, which is the owner's.
  **It stays on the queue as a question logged to the owner. No task here opens
  `docs/RADIO_SHEET.md`, `TheRadioSheetQuotesTheScreenTests` or any operator-facing string
  the sheet quotes.**
- **Unit 383's item 5 - the two inherited reds**,
  `ViewTestsActThroughControlsTests.NoViewTestWritesAPropertyAControlOwns` and
  `TheOliviaMoveUpTests.ItIsNotOfferedOnAGuessedYourTurn`. Neither is on the carry-forward
  list and neither is anybody's yet. **The second one is about a guessed *your turn*, and
  criterion 9.4 is exactly about guessed your-turns.** Under unit 378's own rule, **if task
  4 ends up editing `TheOliviaMoveUpTests.cs`, that red becomes yours and you repair it in
  that commit and say so.** If you do not touch the file, report it again and do not repair
  it.
- **Unit 383's item 4 - `TheOliviaSendTests.StopMidPlayAbortsAnOliviaSend`**, red under
  parallel load with two different failures and green on its own in 273 ms. **Not
  quarantined, not chased, and not yours.** Report it.

**And the `RULES_AT` id-scheme split, for the sixth unit running.** `PROJECT_STATUS.md`
reads `HM-DEC-165 (2026-09-19)`; `CLAUDE.md` section 1 holds `CPS-DEC-0165`. The reload
names it first among the disagreements. `tools/status.sh` writes that field as a literal
and **you may not edit `tools\`**. Report it. Do not repair it.

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet holds what it has - the hardening phase, screen, record and test
            work banked in the PSK31 and Olivia threads that needs neither the radio
            nor the owner, judged by tests that ran and, at the end, by Tim at his
            window.
UNIT GOAL:  The four things Tim watched go wrong in one four-minute contact are made
            right on the conversation card: every card and every receipt can be got
            rid of with an X; while a station's carrier is up his row and his card
            say so and the four send controls are held, so Hamlet will not let him
            key on top of a man who is still sending; Log is on the card from the
            moment the card exists rather than only when the exchange finished; and
            every line addressed to him that hands the turn back moves the card,
            not only the first one.
ADVANCES: step 9 criterion 1 - and 9.2, 9.3, 9.4 and 9.5 in the same unit; the launcher form names one, the report names all five
            zero units spent and whose entry step 8 opened.
DRIFT:      none.
```

**The count today**, from `PHASE_OUTCOME.md`, `PHASE_PLAN.md` at `6c6232cc` and the reload
of 2026-09-21 21:10.

| Step | State | Units spent | Where it stands |
|---|---|---|---|
| 0 | `partial` | 369, and 371's carried repair | **0.2 to 0.5 certified met** by the judging session at `PHASE_OUTCOME.md:41`; 0.1's answer is a completed negative, ruled author's in instruction 372 |
| 1 | **`done`** | 372, 373, 374 | closed by 374 |
| 2 | `partial` | 375, 377, and 384 for one task | 2.1, 2.2, 2.3 met. **2.4 and 2.6 open** |
| 3 | **`done`** | 380, 381 | closed by 381, judging session `done` |
| 4 | `partial` | 382, 383 | 4.1 and 4.2 met. **4.3 cut down** by instruction 384 |
| 5 | `not started` | 0 | Tim's own, and it ends the run |
| 6 | **`done`** | 376 | closed by R42 at the measured floor |
| 7 | `partial` | 378, 383 | 7.1, 7.3, 7.4, 7.5 met. 7.2 met on unit 383's own account, **unjudged** |
| 8 | **`done`** | 379 | all four criteria met, judging session `done` |
| 9 | `not started` | **0** | **this unit** |

**Step 9's entry, checked:** *step 8 done.* Unit 379 met 8.1 to 8.4 and the separate
judging session returned `STATE_AFTER: done` at `PHASE_OUTCOME.md:322`. **The entry is
open.**

**Why step 9 and not 2.4, which is where the last unit was aimed.** Four reasons, and the
fourth is the one that decides it.

1. **Step 9 is the owner's newest ruling and it was added to the plan tonight.** R44 is
   dated 2026-09-21 and the plan's own revision record says *step 9 added from the KC3FL
   contact (R44)*. It is five unmet criteria, its entry is open, and nothing waits behind
   it.
2. **Everything else open is either answered, the owner's, or bookkeeping.** 0.1 is a
   completed negative - 119 commits searched and an empty diff over every settings path -
   and re-searching it is the loop shape. 4.3's remedy is a change to a sentence Hamlet
   says, which is the owner's, and two judging sessions have said so. 5.1 is Tim at his
   window. 7.2 needs a judging session and not a unit.
3. **2.4 is a measurement of an environment, not of Hamlet.** Across twenty-one full app
   invocations in units 375 and 384, every break but two was the ~1 ms dispatcher loop
   before any assertion. Unit 384's single completed round lost both app attempts to it.
   A night spent there buys a count of dice rolls.
4. **And 2.4 counts only on a tree that does not move.** Work instruction 384's own ruling
   defines the criterion as five consecutive rounds **with no file under `src`, `tests`,
   `assets` or `data` changing between the first counted round and the last.** Step 9 is
   the last block of code this phase has left. **Soaking the list tonight and then landing
   step 9 tomorrow would invalidate the soak by 2.4's own definition.** 2.4 belongs at the
   end, after the last change, and **2.6 goes with it** - one record unit closes step 2
   when nothing is moving. That is the ordering, and it is why this unit leaves 2.4 alone.

**What happened to unit 384, stated plainly, because you will find its fingerprints.** Its
run **did not finish**: `.run-unit\last-run.json` records `terminal_reason: api_error`,
`api_error_status: 529`, at 72 turns and $4.72. It committed task 0 (`9832f972`) and never
wrote `output.md`. **The launcher then graded unit 383's leftover report as unit 384's** -
the `## UNIT 7 - STEP 2` entry now in `PHASE_OUTCOME.md` carries `FATE: executed` for a run
that died, and a `STATE_WHY` that describes a unit that *worked only on steps 4 and 7*,
which is unit 383. **That is the same launcher fault criterion 2.6 exists to record about
the Olivia phase, happening a second time.** Task 0 appends the correction. **The false row
is not edited** - the record is append-only.

**What this unit is worth, in the owner's terms.** Tim worked a man on PSK31 this afternoon
for the first time in weeks. Hamlet let him transmit while the other station's carrier was
still on the air, which is the one thing a keyboard operator must never do; it showed him no
way to log the contact until the exchange had finished in the exact shape Hamlet expects;
it did not move the card when the man handed back a second time; and it gave him no way to
clear a card off his screen. **After tonight Hamlet holds his hand off the key while the
other man is sending, tells him why in a sentence, offers Log from the moment there is
somebody to log, follows the conversation past its first turn, and lets him tidy his own
screen.** None of it needs the radio and none of it needs him.

---

## 5. Verify this instruction against the tree

Every line below was read at authoring time, at the file and line named, with `grep` and
`sed` over the working tree. **Report every mismatch in section 4 and in section 1 of your
report; repair nothing but this unit's.** Line numbers move under an edit - if one is off by
a few, the name is the thing that matters; say so and go on.

### The card already knows he is still sending, and it already has the words

`src\Hamlet.App\ViewModels\Ft8ContactCard.cs`:

- **Line 432, `TurnWord`** - `Psk31TurnState.HeIsSending => "He is still sending"` at line
  **437**, beside `"Your turn"` / `"Your turn, a guess"` at **435** and `"His turn"` /
  `"His turn, a guess"` at **436**. **The guess wording 9.4 asks for already exists.**
- **Line 442, `TurnIsGuess`** - true where the turn is not certain.
- **Line 466, `TurnSentence()`**, and **476, `TurnState()`** - one sentence per turn state,
  including `Psk31TurnState.HeIsSending => $"Text is still arriving on the frequency
  {Callsign} called you on."` at **480**.
- **Line 734, `ShowsLogLink`** - and this is criterion 9.3 in one expression:

  ```
  public bool ShowsLogLink
      => IsPsk31
          ? _psk31Complete
          : !IsCallToAnyone && ActionKind != Ft8CardActionKind.Log;
  ```

  **On a PSK31 card Log appears only when the exchange is finished.** Its own remarks at
  **731** say so: *on a PSK31 card it appears when the exchange is finished ... Hamlet has
  sent the confirmation and he has certainly said goodbye. Before that it is not there.*
  **That is exactly what Tim hit.**
- **Line 739, `LogLabel = "Log this contact"`; line 748, `LogTip`** - which already says
  the honest thing 9.3's second half asks for: *whatever passed between you and {Callsign},
  however far it got. Anything Hamlet did not observe is shown as not recorded and is left
  out of the record entirely, so a half exchange never looks like a whole one.* **The
  sentence for a partial log is already written. 9.3 may be one expression and an
  assertion.**
- **Lines 1024, 1032 and 1035** - `ReceiptWord = "Calling"`, `ReceiptSentence = "Your call
  went out to anyone listening."`, `IsCallToAnyone`. **A receipt is the same type as a card
  with `IsCallToAnyone` true**, so 9.1's *every card and every receipt* is one control and
  not two.
- **Line 724 carries a ruling you must not undo:** *it is false on a receipt, which is a
  different reason* (Tim, 2026-09-11: *"CQ should not have log option, that is
  self-gratification."*). **Log never goes on a receipt.** Section 6 ruling 2 is where this
  binds.

### The turn states, and what the engine means by them

`src\Hamlet.RadioEngine\Psk31\Psk31Turn.cs` line **6**:

```
public enum Psk31TurnState { Unknown, YourTurn, HisTurn, HeIsSending }
```

`HeIsSending` is documented at **17** as *characters are still arriving after the last
complete message*, and the rule string at **53** says *characters arriving after any message
make it he is still sending*. **Read that carefully: it is a fact about decoded text, not
about a carrier.** `src\Hamlet.RadioEngine\Psk31\Psk31Offer.cs:36` says a macro is offered
only on a *certain your turn* and that *a guessed your turn, his turn, he is still sending
and unknown alike* offer nothing. **So the card already withholds its own macro while he is
sending - and the four controls 9.2 names are not that macro.** Measure the difference in
task 1; section 6 ruling 1 is what it means.

### The carrier itself, which is a different fact and is the one 9.2 names

`src\Hamlet.App\ViewModels\MainWindowViewModel.cs`, `EndOrRemovePsk31Row` at **2685**:

- **Line 2707, `row.Ended = true`**, and **line 2712, `row.StoppedUtc`**, written *here and
  nowhere else, in the one place `Ended` is set, so the fact and the moment cannot
  disagree* (its own comment, **2709**).
- So **a station's carrier is on the air exactly while his row is on the list and `Ended`
  is false**, and that is a fact Hamlet already holds per row. `psk31_carrier_appeared` is
  written at `src\Hamlet.App\Telemetry\Psk31Events.cs:184` and again at **651** with what
  Olivia measures.
- `Psk31StationOn(row)` at **17780** is how a row is turned into a station - the question
  unit 378 used for the canned menu, and the same question a card is keyed by.

### The one door that composes, which is where a refusal belongs

`src\Hamlet.App\ViewModels\MainWindowViewModel.cs` **16380-16410**: the mode gate, whose own
comment says *this is the only call site in `src/` that composes a signal, so a refusal here
covers the CQ button, the right-click answer, and anything a later unit adds - there is no
second route to guard and none to forget.*

- **16388** `if (!CanTransmitIn(ChosenDigitalMode))`, **16390**
  `AppEvents.OperatorAction(_telemetry, "send_refused", OperatingMode, ...)`, **16399**
  `Psk31Events.SendRefused(_telemetry, "mode", kind: null, "gate")`, **16402**
  `DigitalSendLine = ...`.
- `send_refused` is written through `AppEvents` at `src\Hamlet.App\Telemetry\AppEvents.cs:1951`
  in `TelemetryCategory.Transmit` at `TelemetryLevel.Warn`; `psk31_send_refused` is at
  `Psk31Events.cs:831` and its reason tokens are documented at `Psk31Macro.cs:10`. **Both
  writers exist. R13 forbids a new event type where an existing writer will carry it.**
- **This is the same shape unit 377 used for `variant_not_proved`**: a gate above the only
  composer, with the gate's line number asserted to be below the composer's declaration.
  Do that again.

### The dismissal, half of which already exists

- **`src\Hamlet.App\Telemetry\OnScreen.cs:77`** - `public const string Dismissed =
  "dismissed";`, unit 380's work.
- **`MainWindowViewModel.cs:6447`** - `NoteCardOnScreen(live[at], OnScreenState.Removed,
  OnScreenBy.Dismissed)`, inside the reconcile. **A card leaving the screen is already
  recorded with a `dismissed` reason.**
- **And there is no `card_dismissed` anywhere under `src`** - measured at authoring time,
  a `grep` for `card_dismissed` and `CardDismissed` over every `.cs` file under `src`
  returned nothing. **So 9.1's token names a press that does not exist yet, and the
  screen-side fact already has a writer.** Section 6 ruling 2 item 3 says which is which.
- The only dismiss affordances in the markup today are `IsLightDismissEnabled` on popups
  and `DismissReceiveOfferCommand` at `Views\MainWindow.axaml:1802`, which dismisses the
  receive *offer* and is not a card.

### Log, where it lives today

- `MainWindowViewModel.cs:15965` `CanLogRow(row)`; **15989** the log command's guard;
  **16129** the reason line for a row that cannot be logged.
- `Views\MainWindow.axaml:5863` `IsVisible="{Binding ShowsLogLink}"` - the card's link.
- `Views\MainWindow.axaml.cs:575-591` - the row's own Log menu item, gated on
  `vm.CanLogRow(row)` and bound to `vm.LogContactCommand`.
- `ContactLogEntryForStation` is the one place a log record is built (instruction 379
  section 5 measured it at **15475**, with the RST and the grid taken from the mode's own
  parser at **15536** and **15551**). **9.3's *defaulted to what was exchanged if anything
  was* may already be true there. Measure it before you write anything.**

### Where the record of Tim's contact is

`docs\RADIO_SHEET.md:159` - the record is `%AppData%\Hamlet\telemetry\yyyy-MM-dd.jsonl`,
one file a day, in UTC. **The contact 9.2 and 9.4 name is in `2026-09-21.jsonl`, between
17:44 and 17:48 UTC**, if this machine still has it.

**Two things about that file, and both matter.** It holds **nothing personal** (HM-DEC-018
section 2.1) - **no callsign is in it**, so `KC3FL` comes off Tim's screen and not out of
the record. The record gives you **offsets, times and stages**. So a replay fixture takes
its *timings* from the record and its *callsign* from the test. **Never put a real callsign
in a fixture, a test name or the report.**

### The record and the versions

`Directory.Build.props` line **1174** reads `<Version>1.13.71</Version>` - unit 384 bumped
it at its task 0. `PHASE_STATUS.md` `CURRENT_STEP` reads **2** and `WORK_INSTRUCTION` reads
**384**, both stale. `PHASE_OUTCOME.md` and `PHASE_STATUS.md` are **modified and
uncommitted at the root** - that is the launcher's own append after unit 384 died, not a
session's edit. Task 0 commits them as they stand.

**`PHASE_PLAN.md` does not tick 0.2, 0.3, 0.4 or 0.5, and the judging session certified
them.** Unit 384's ruling 2 item 2 brought nineteen criteria up to the record and missed
these four; `PHASE_OUTCOME.md:41` reads, verbatim: *Criteria 0.2 to 0.5 are met with quoted
sentences, counts and green unedited suites.* **Task 0 ticks those four, citing that line.
That is transcription of a verdict another session returned - it is not a judgment of
yours, it does not count as one of your two rulings, and it does not tick 0.1.**

### What will be red before you start

Nothing in the tree asserts any of step 9's five criteria today, so **the entry run should
be exactly unit 383's exit: app 226 of 226 and engine 150 of 150.** Outside the
carry-forward list, expect the two inherited reds of section 3. **Anything else red at task
0 is not yours - name it and go on. Anything red after task 2 is yours.**

---

## 6. Rulings in force

Transcribed in full. **Do not re-argue any of these.**

### The owner's ruling that licenses this unit, in full

**R44 - Tim, 2026-09-21, from the KC3FL contact.** *Every card has an X; a live carrier is
visible and gates the buttons; Log on every card from the start; every hand-back moves the
turn. Step 9.*

**R11 - nothing at the radio. R12 - a session rewrites its own tests**, and a rewrite
asserts more than it replaced, in its own commit, after the change it is about. **R13 -
telemetry on every stage, and no new event type where an existing writer will carry it.
R14 - nothing beyond the criterion: a criterion the tree already meets is asserted, not
rebuilt. R19 - American. R31 - two rulings a unit.**

**PSK31 plan R1 - the certainty gate**, and **R8** - the log belongs to the card and never
to the receipt. **HM-DEC-018 section 2.1** - nothing personal in an event. **HM-DEC-165** -
nothing red that was green before. **FACT-004** - nothing in this unit is evidence about
the radio. **CLAUDE.md section 0.0** - a sentence on the screen is a claim, and a refusal
says the true reason. **CLAUDE.md section 0.2** - nothing goes out until he clicks.

### The stop, and why 9.2 is not one

`PHASE_PLAN.md` section 6: *anything that would change what goes on the air, or what keys -
`MOVE: stop`.* And the plan's section 1 says this phase touches *nothing that keys, except
step 0's refusal text at the arm stage.*

**9.2 is a second refusal at that same arm stage, and it was ruled by the owner four hours
before this instruction was written.** The plan's section 1 was written on 2026-09-20; R44
is dated 2026-09-21 and section 6's own rule is *the later ruling wins*. **So building the
hold is licensed and you do not stop for it.**

**What is still the stop, and it is one inch away.** The gate may **refuse** a send. It may
never **cause** one, delay one that is already going out, or alter one byte of what does.
**If reaching any criterion here appears to need a change at a composer, an `Arm`, a
`PttOn`, a cap, an RSID burst, the Olivia variant gate, a modulator or a demodulator - stop,
record it, hand it back.** Nothing under `src\Hamlet.RadioEngine\Olivia\`, `\Psk31\`,
`\Rsid\` or `\Transmit\` is opened in this unit. A card is a screen. It is never a reason to
change what goes out.

### The first is mine: what *his carrier is on the air* is, where the hold is built, and that it must let go

**Author's, overrulable.**

1. **The fact is the carrier, not the parser.** Section 5 measures two candidates:
   `Psk31TurnState.HeIsSending`, which is *characters still arriving after the last complete
   message*, and `row.Ended == false`, which is *his carrier has not stopped*. **9.2's words
   are *while a station's carrier is on the air*, so the row's own liveness is the fact and
   the turn state is not.** Print both in task 1 over the same fixture and say where they
   disagree. **Where they disagree, the carrier wins**, because a man whose carrier is up
   between two messages is still transmitting and keying on him is the harm R44 names.
2. **The hold is three things and they are separate.** (a) **His row and his card say it**
   - a color and the word *sending*, which is a display change; (b) **the four controls are
   held** - Report, Confirm, the canned lines and the typed line, greyed with *he is still
   sending*; and (c) **a press that gets through anyway is refused** at the single door that
   composes, beside the mode gate at `MainWindowViewModel.cs:16388` and **above** the
   composer, writing `send_refused` with `reason his_carrier_live` through
   `AppEvents.OperatorAction` and `Psk31Events.SendRefused`, **no new event type, no new
   telemetry category, no new stage** (R13). **Assert that the gate's line is above the
   composer's call, as unit 377 asserted it for `variant_not_proved`.** (a) and (b) are what
   Tim sees; (c) is what makes it true.
3. **The sentence is one sentence and it is the same one in all three places** - *he is
   still sending* - so the grey button, the refusal line and the record cannot drift apart
   (§0.0).
4. **THE HOLD MUST LET GO, AND THIS IS THE SAFETY ITEM, NOT THE GATE.** A gate that sticks
   would leave Hamlet unable to transmit at all, which is worse than the fault it fixes.
   **The hold is a reading of a live fact and never a latch**: it is false the moment his
   row ends, and it is false when there is no row, no station and no card. **Prove the
   release in a test of its own** - carrier up, controls held, carrier drops, controls live
   again, one refusal written and exactly one - and **prove that a press with nobody on the
   frequency is not refused**, so the guard cannot silence him on an empty band.
5. **Olivia is held by the same fact or the criterion is not met.** An Olivia channel's row
   has the same `Ended`. If the two row kinds turn out to answer differently, that is a
   finding and it goes in section 4 with the measurement.

### The second is mine: Log from the start, the X, and what is never written

**Author's, overrulable.**

1. **Log goes on every conversation card from the moment the card exists**, which on a
   PSK31 or Olivia card means `ShowsLogLink` stops being `_psk31Complete`. **It does not go
   on a receipt and it does not go on a call-to-anyone** - Tim, 2026-09-11: *"CQ should not
   have log option, that is self-gratification"*, and R8. 9.3's words are *every
   conversation card*, and a receipt is not one. **The existing `IsCallToAnyone` clause is
   the guard and it stays.**
2. **Nothing is invented to fill a field.** A logged contact with no certain 73 logs what is
   known: what Hamlet observed goes in, what it did not observe is shown as *not recorded*
   and is left out of the record entirely. `LogTip` at `Ft8ContactCard.cs:748` already says
   this in the product's own words; **the unit's job is to make that sentence true earlier,
   not to write a new one** (§0.0). **No RST invented, no grid guessed, no time composed. If
   the defaults are already taken from the mode's own parser, say so and assert it rather
   than rebuilding it** (R14).
3. **The X is a press, and the press and the disappearance are recorded by the writers that
   already exist.** `OnScreenBy.Dismissed` and `NoteCardOnScreen` are unit 380's and already
   record a card leaving the screen; **the operator's own press is written through
   `AppEvents.OperatorAction`, and that is where 9.1's token `card_dismissed` goes** - a
   press event name, not a new telemetry category and not a second card list (R13). **Both
   halves in the same commit as the control.**
4. **Dismissing removes a card from the screen and nothing else.** Not from the log, not
   from the contact record, not from the decoded list, not from the ledger. `PHASE_PLAN.md`
   section 6's spirit - *a file must be deleted: empty it, comment it, list it* - is the same
   rule one control along. **If a dismissed station transmits again, Hamlet may show him
   again; that is the author's choice and it is yours to make and state.**
5. **Measure before you move, and say which half was already true.** Three of these five
   criteria are claims about things the tree may already do. **A report that cannot say what
   the before was has not earned any of them, whatever else it earned.**

---

## 7. Status cadence

`sh tools/status.sh`, real clock, **after every task and every commit**, and **immediately
before each carry-forward invocation** - task 0's two and task 5's two. **Task 1's
measurement is one task**: status before it and after it, not between runs. Never compose a
timestamp.

---

## 8. The tasks

### Task 0 - the record and the entry run

Append `UNIT 385 - STEP 9` to `PHASE_OUTCOME.md` with `ADVANCED: step 9`, **written with
the editor and not with a shell heredoc** (section 2). Patch-bump **1.13.71 -> 1.13.72** in
`Directory.Build.props` with its line in the version log. **Set `CURRENT_STEP: 9` and
`WORK_INSTRUCTION: 385 - ...` in `PHASE_STATUS.md`**, both stale at 2 and 384.

**Three pieces of record-keeping, all of them transcription and none of them a ruling:**

1. **Tick 0.2, 0.3, 0.4 and 0.5 in `PHASE_PLAN.md`**, citing `PHASE_OUTCOME.md:41`
   verbatim. **0.1 stays unticked and step 0 stays `partial`.**
2. **Append the correction about unit 384**, in the entry you are already writing: its run
   ended on an API 529 after task 0, it never wrote `output.md`, and the `## UNIT 7 - STEP
   2` entry's `FATE: executed` and `STATE_AFTER` were graded from unit 383's leftover
   report - whose `STATE_WHY` describes a unit that *worked only on steps 4 and 7*, which is
   unit 383's night and not unit 384's. Name the evidence: `.run-unit\last-run.json`'s
   `terminal_reason: api_error` and `api_error_status: 529`, and the absence of any commit
   after `9832f972`. **Do not edit the false row.** This is the same fault criterion 2.6
   records about the Olivia phase, happening a second time, **and saying so is not working
   2.6** - 2.6 is an append to the archived Olivia file and no task here opens it.
3. **Leave step 7's line exactly where unit 384 left it.** 7.2 is unjudged. Do not re-run
   it, re-prove it or tick it.

**Run the carry-forward list, both invocations, before anything changes**, status written
immediately before each, and put the two counts in the outcome entry's `ENTRY:` line. Unit
383's exit left it at **app 226 of 226, engine 150 of 150**; unit 384 changed no file under
`src` or `tests`, so that is what you should see. A red here is not yours; name it and go
on. **If either invocation dies of the dispatcher loop before any assertion, re-run it once
and record both attempts** (section 1).

**Also run, by name and outside the carry-forward list, the types this unit will touch or
borrow**, in one filtered invocation, and record the count: `ThePsk31ReadsTheConversationTests`,
`TheCannedListIsOfferedTests`, `TheOliviaMoveUpTests`, `TheOliviaSendTests`,
`TheRecordSaysWhatWasOnScreenTests`, `TheRecordDiagnosesTheEveningTests`,
`ThePsk31CarrierLivesTests`, `CallsignPrivacyTests`, `ThePsk31SeamTests`,
`TheSendReachesTheAirTests`. **Task 0 is the only moment you can learn whether a red you
meet later was yours.**

**Drop candidate:** none.

### Task 1 - the trace: what the card, the row and the log already do

**This task builds nothing, changes no source file, repairs nothing and asserts nothing
about the product.** One `[Fact]` named `Unit385Trace` in `tests\Hamlet.App.Tests\ViewModels\`,
run by name. **Everything in it is composed on the development machine - computed, not seen**
(FACT-004). The report's section 3 leads with its tables.

Print, in this order:

1. **The contact itself, out of the record if it is there.** Look for
   `%AppData%\Hamlet\telemetry\2026-09-21.jsonl` and, if it exists, print what it says
   between 17:44 and 17:48 UTC: the carrier appear and stop lines with their offsets, the
   send stages, the parse lines, and **the two moments R44 names - 17:45:40 to 17:45:44, and
   17:48:33.** **If the file is not on this machine, say so in one line and go on**: the
   fixture is then built to the timings R44 states and is **named in the test and in the
   report as a constructed fixture and never as the owner's record.** Either way **no
   callsign from that file or from R44 goes into a test, a fixture or the report** - use a
   test callsign (section 5).
2. **The two candidate facts for *his carrier is on the air*, side by side.** Over a PSK31
   fixture with a station that sends, pauses between messages, and then stops: print, sample
   by sample of the fixture's clock, `row.Ended`, `row.StoppedUtc`, the card's `TurnWord`,
   the card's `_turn.State` and `IsCertain`. **Name every moment where `HeIsSending` and
   `Ended == false` disagree.** Say which one is up during the pause between his two
   messages. **This is what ruling 1 item 1 rests on, and if the measurement disagrees with
   me, the measurement wins and you say so in section 4.**
3. **The four controls, and whether anything holds them today.** For Report, Confirm, a
   canned line and the typed line: print, while his carrier is up, whether each is enabled,
   what it would send, and what the card offers (`OfferNote`, `WaitingToBeSure`,
   `Psk31Offer`). **Then press one and print what happens** - whether it composes, and how
   many calls reach the sound card. **This is the before for 9.2 and it is the exact moment
   Tim hit.**
4. **Log, before.** On a card that exists but is not finished: `ShowsLogLink`, `LogLabel`,
   `LogTip`, `CanLogRow` for the same station's row, and **every field of the `AdifContact`
   `ContactLogEntryForStation` would build for it right now** - naming which are filled,
   which are null, and where each filled one came from. **Then the same on a finished card.**
   **9.3's second half is a question about those fields; answer it with the fields.**
5. **The turn, and why the second hand-back did not move it.** Drive a conversation with
   **two** lines addressed to the operator that hand back - one certain, one that does not
   read cleanly - and print after each: the card's `TurnWord`, `TurnIsGuess`, the parser's
   kind and certainty, and whether the card moved. **Name the site that decides, and say
   whether the second line is not parsed, parsed and not applied, or applied and not shown.**
   Those are three different repairs.
6. **The X, before.** Count the cards and receipts on screen in a fixture with both kinds,
   and print for each whether any control removes it. Print what `NoteCardOnScreen` writes
   today when the reconcile drops a card, verbatim from the serialised line.

**Drop candidate:** none. **Nothing in tasks 2 to 4 may be written before this task's
numbers are on the page.**

### Task 2 - 9.2: the carrier holds the buttons

Ruling 1 is the whole of it. **Build it in the order (c), (a), (b)** - the refusal first,
then the display, then the greying - **because the refusal is the safety and the greying is
the convenience, and a night that runs out should run out with the safety in.**

- The refusal at the one door, above the composer, with `reason his_carrier_live`, through
  the writers that exist.
- The word *sending* and a color on his row and his card.
- Report, Confirm, the canned lines and the typed line greyed with *he is still sending*.
- **The release, proved** - ruling 1 item 4. This is its own test name and it is not
  optional.
- **The replay**: from task 1's record or its constructed fixture, **Tim's Report at
  17:45:40-17:45:44 would have been held**, asserted as a refusal that was written and a
  composer that was not reached.

**One commit for the gate, one for the display, one for the greying, one for the replay.**
Any test that goes red on the way is an R12 rewrite in its own commit after the change it is
about, asserting more than it replaced.

**Drop candidate:** none in this task.

### Task 3 - 9.3 and 9.1: Log from the start, and an X on every card

Ruling 2. Two commits, and **9.3 first**, because it is Tim's *there was no way to log him*.

- `ShowsLogLink` on a PSK31 or Olivia card stops waiting for the exchange to finish; the
  receipt and the call-to-anyone guards stay exactly as they are.
- The RST fields editable and defaulted to what was exchanged **if anything was**, and
  **nothing invented** where nothing was.
- The X on every conversation card **and every receipt** - one control on the one type.
- `card_dismissed` on the press through `AppEvents.OperatorAction`; the card leaving the
  screen through unit 380's `NoteCardOnScreen` with `OnScreenBy.Dismissed`.
- **Then assert the privacy scan is still green over both** - `CallsignPrivacyTests`, and
  its `ExpectedEventMethodCount` moved in the same commit as any writer you add, exactly as
  units 380 and 381 did.

**Drop candidate:** none in this task.

### Task 4 - 9.4: every hand-back moves the turn

Whatever task 1 item 5 named as the site. **Every parsed line addressed to the operator that
hands back moves the card's turn to *your turn*, marked as a guess where the parser is not
certain - not only the first.**

- The guess word already exists (`"Your turn, a guess"`, `Ft8ContactCard.cs:435`). **Do not
  write a second one.**
- **R1's certainty gate is untouched**: a guessed turn still offers no macro
  (`Psk31Offer.cs:36`). **Moving the card is not offering a send.** Say that in the test's
  own remarks, because it is the distinction the whole criterion turns on.
- **The replay**: from the 17:48:33 line, **the card reads *your turn?*** - or whatever the
  guessed wording in the tree actually is, quoted character for character rather than
  paraphrased.
- If this task edits `TheOliviaMoveUpTests.cs`, the inherited red in it becomes yours
  (section 3).

### Task 5 - 9.5, the exit run, the record and the report

- **9.5: the four are on PSK31 and Olivia cards alike, asserted.** Drive each of the four
  down an Olivia channel at a variant as well as a PSK31 conversation, and assert by
  identity where they are the same object - the same card type, the same row type, the same
  gate site - rather than by writing a second set of tests (unit 378's 7.4 is the
  precedent).
- **The exit carry-forward, both invocations, one build each, status before each**, name for
  name against task 0's counts, with your own new guards named as the only differences.
- **Your new carry-forward names go on `docs\carry-forward-tests.txt` in the same commit as
  the test they name.** 9.2's refusal, 9.2's release and 9.3's Log-from-the-start are the
  three that belong there permanently; say why in the line.
- The report, section 12's shape.

**THE NAMED DROP CANDIDATE, AND IT IS THIS ONE:** **the second replay - driving 9.2's hold
and 9.4's turn a second time down an Olivia channel from an Olivia fixture.** If the night
runs short, 9.5 rests on the identity assertion above, and **the report says plainly which of
the four were proved on Olivia by replay and which by construction, with the count.** Unit
382 took the same drop on the radio sheet's Olivia walk and kept every fact; do it the same
way. **Nothing else in this unit is a drop candidate** - in particular the refusal, the
release and Log-from-the-start are not, and a night that cannot reach 9.4 reports 9.4 not met
rather than reaching it thinly.

---

## 9. Parked - do not touch, do not raise

- **4.3 and `docs\RADIO_SHEET.md`.** Cut down by instruction 384 section 6 ruling 2 item 1.
  Two judging sessions agree the conflict is not one of the three stops. **No task opens the
  sheet, its test, or any operator-facing string it quotes.** The question is the owner's and
  it is already logged.
- **2.4 and 2.6.** Both belong to the record unit that closes step 2 **after this phase's
  last code change**, for the reason in section 4. **Do not run the carry-forward list more
  than task 0's round and task 5's round**, and do not open
  `docs\phase-olivia-run\PHASE_OUTCOME.md`.
- **0.1.** A completed negative: 119 commits searched, an empty diff over every settings
  path, the mechanism named and repaired. **Do not search that window again.**
- **7.2.** Met on unit 383's own account and unjudged. Do not re-run it, re-prove it, or tick
  it.
- **Step 5.** Tim's own, and it ends the run.
- **The `RULES_AT` id split.** Report; `tools\` is not yours.
- **The dispatcher loop.** Record both attempts; do not chase it.

---

## 10. What not to do

- **Do not touch a send path's content.** Not a composer, not an `Arm`, not a `PttOn`, not a
  cap, not an RSID burst, not the Olivia variant gate, not a modulator, not a demodulator.
  Section 6's stop paragraph says what to do instead. **You may add a refusal above the
  composer. You may not change what the composer does.**
- **Do not make the hold a latch** (ruling 1 item 4). A guard that can stick is worse than
  the fault it fixes.
- **Do not put Log on a receipt or on a call-to-anyone card** (ruling 2 item 1). That is
  Tim's own ruling of 2026-09-11 and R8.
- **Do not invent a fact to fill a log field** (§0.0). Absent, not guessed.
- **Do not delete anything when a card is dismissed.** The card leaves the screen; the record
  stays.
- **Do not add a telemetry category, an event type or a stage where a writer exists** (R13).
- **Do not put a callsign, a grid or a word of decoded text in an event** (HM-DEC-018 2.1),
  and **do not put a real callsign in a fixture, a test name or the report**.
- **Do not loosen a test to make a criterion pass.** `PHASE_PLAN.md` section 6: *a must-pass
  missed by a little - ship, report, `partial`, move on. Never loosen a test.*
- **Do not rebuild what the tree already does** (R14). The likeliest wasted night here is a
  second turn tracker beside `Psk31Turn`, or a second card list beside the reconcile.
- **Do not run the suite** (HM-DEC-155).
- **Do not repair anything this instruction got wrong about the tree** - report it (section
  5).
- **Do not edit the false `## UNIT 7 - STEP 2` row.** Append.
- **Do not spend more than two rulings** (R31). Both are spent in section 6.

---

## 11. Committing and pushing

**One commit per task**, with these exceptions: **task 2 takes one commit per piece it lands**
- the gate, the display, the greying, the replay - **task 3 takes one for 9.3 and one for
9.1**, and **each R12 rewrite is its own commit after the change it is about.** A
carry-forward line edit goes in the same commit as the test it names. **Push once, at the
end**, after task 5's runs are green or their reds are named.

---

## 12. Reporting

`output.md` at the root. **Canonical headings:** `## 1. What Claude did`, `## 2. What the
owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

**The ordering block first. `validate-output.bat` refuses a report without it.** Fill every
line from what you measured - a line that is the same every unit is furniture.

```
READ IN THIS ORDER.

A. The phase goal - Hamlet holds what it has. Step 0 partial with 0.2 to 0.5 now
   ticked and 0.1 a ruled negative, steps 1, 3, 6 and 8 done, step 2 partial on 2.4
   and 2.6 which are parked for the end-of-phase record unit, step 4 partial on 4.3
   which is cut down and logged to the owner, step 7 partial on an unjudged 7.2,
   step 5 Tim's own. This is the first unit ever spent on step 9.
B. Step 9 - the contact Tim had. 9.1 <met|partial|not>: an X on <n> cards and <n>
   receipts, card_dismissed written <where>. 9.2 <met|partial|not>: the hold reads
   off <which fact>, <n> of the four controls held, the refusal his_carrier_live
   written at <site> above the composer, the release proved <how>, and Tim's Report
   at 17:45:40 <would|would not> have been held. 9.3 <met|partial|not>: Log on a
   card from <when> where it was <when>, RST defaulted from <where>, <n> fields left
   absent rather than guessed. 9.4 <met|partial|not>: <n> of <n> hand-backs moved
   the turn where <n> did, the 17:48:33 replay reads <verbatim>. 9.5
   <met|partial|not>: <n> of the four proved on Olivia by replay, <n> by
   construction.
C. The report last. Section 4 raises <N> items on top of the carried thirty-nine,
   and <none of them is | item <k> is> in the way of a criterion in B. Say in one
   line whether the measurement in task 1 item 2 agreed with section 6 ruling 1
   item 1, because if it did not, the measurement won.
```

```
UNIT:       385 - <complete|stopped> at task N of 6 - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   step 9, criteria <the ids you actually moved>
NUMBER:     sends attempted on top of a live carrier, before and after: <n> of <n>
            went out -> <n> of <n>; and conversation cards offering Log at the
            moment they appear: <before> -> <after>
DRIFT:      none
```

**Section 3 must lead with task 1's before-tables** - the two candidate carrier facts side
by side with every moment they disagree, then the four controls with what each did while his
carrier was up, then the log fields filled and null on an unfinished card, then the two
hand-backs and which moved the card. **That is what makes 9.2 and 9.4 measurements rather
than adjectives**, and it is the only place the next unit can read the before. Then the
after for each, in the same shape. Then the refusal line verbatim out of the written record.
Then the carry-forward counts before and after, and the named-type count before and after.

**Section 2 tells Tim in one paragraph what is different**, in his terms: not that a gate
was added, but that **Hamlet will not let him transmit while the other man's carrier is up,
and says why; that Log is on the card the moment there is somebody to log, and logs only what
actually passed; that the card follows the conversation past its first turn; and that he can
clear a card off his screen with an X.** Say plainly which half was already true. Every
claim computed, not seen (FACT-004), and say in one line that no port was opened, nothing
was enumerated and nothing was keyed.

**Section 4:** your own items first, most-blocking first, each saying plainly whether it
wants a ruling or is a finding - **a note is not a ruling request.**

- **If task 1 item 2's measurement disagreed with ruling 1 item 1, that is your first item**
  and it is a finding, not a ruling request - the measurement wins and section 6 says so.
- **If you found that reaching a criterion needs a change on a send path, that is your first
  item and it wants a ruling.** It is `PHASE_PLAN.md` section 6's first stop and nothing in
  this unit licenses it.
- **If the telemetry file for 2026-09-21 was not on this machine, say so with one line**
  naming what you built instead. That is a finding about evidence, not a blocker.
- **Say in one line what you did about the `## UNIT 7 - STEP 2` row** and that you appended
  rather than edited.

Then the carried queue verbatim per HM-DEC-139: **the twenty-nine unit 383 carried, plus
unit 383's own ten - thirty-nine**, with one line saying that work instruction 384's section
3 was deleted from the tree and so nothing is recorded as answered by it, and one line on
each of the three items of section 3 that this unit's own work touched.

---

```
ARBITER-DECISION
STEP: 9
APPROACH: the conversation card gains a dismiss X, a live-carrier hold on the send buttons, Log from the moment it exists and a turn that moves on every hand-back
MOVE: continue
WHY: PHASE_PLAN.md step 9's entry - step 8 done - is open on unit 379's judging-session verdict at PHASE_OUTCOME.md:322, and step 9 is five unmet criteria set by the owner's own newest ruling R44 and added to the plan tonight at commit 6c6232cc; every other open thing is answered, the owner's, or bookkeeping - 0.1 is a completed negative, 4.3's remedy is a sentence Hamlet says and two judging sessions have called it not a stop, 5.1 is Tim at his window, 7.2 needs a judging session rather than a unit - and 2.4 is left deliberately because work instruction 384's own definition counts it only over five rounds with no file under src, tests, assets or data changing, so soaking the list before step 9 lands would invalidate it by its own terms; 2.4 and 2.6 therefore go together to a record unit after this phase's last code change.
STATE: not started
DECIDED: author's, overrulable, two rulings and one choice of step, both rulings transcribed in full in work instruction 385 section 6. (1) What his carrier is on the air is, where the hold is built, and that it must let go: the fact is the row's own liveness - row.Ended false at MainWindowViewModel.cs:2707, the one place Ended is set - and not Psk31TurnState.HeIsSending, which the engine documents as characters still arriving after the last complete message, because 9.2's words are while a station's carrier is on the air and a man whose carrier is up between two messages is still transmitting; task 1 prints both side by side over the same fixture and where they disagree the measurement wins; the hold is three separable things built in the order refusal, display, greying - a refusal at the single door that composes, beside the mode gate at :16388 and asserted to be above the composer, written as send_refused reason his_carrier_live through AppEvents.OperatorAction and Psk31Events.SendRefused with no new event type, no new category and no new stage, then the word sending and a color on his row and his card, then Report, Confirm, the canned lines and the typed line greyed with the one sentence he is still sending, which is the same sentence in all three places; and THE HOLD MUST LET GO - it is a reading of a live fact and never a latch, false the moment his row ends and false where there is no station at all, with the release and the empty-band case each proved in a test of their own, because a gate that sticks would leave Hamlet unable to transmit and that is worse than the fault it fixes. (2) Log from the start, the X, and what is never written: ShowsLogLink on a PSK31 or Olivia card stops being _psk31Complete so Log is there from the moment the card exists, while the receipt and call-to-anyone guards stay exactly as they are because Tim ruled on 2026-09-11 that a CQ has no log option and R8 keeps the log on the card; nothing is invented to fill a field - what Hamlet observed goes in and what it did not is shown as not recorded and left out, which is what LogTip at Ft8ContactCard.cs:748 already promises, so the unit's job is to make that sentence true earlier rather than to write a new one; the X is a press written through AppEvents.OperatorAction as card_dismissed with the card's leaving recorded through unit 380's existing NoteCardOnScreen and OnScreenBy.Dismissed, no new category and no second card list; and dismissing removes a card from the screen and never from the log, the record or the ledger. (0) The choice of step, which PHASE_PLAN.md section 6 makes the arbiter's and which is not one of R31's two rulings: step 9 and not 2.4, for the reason in WHY. Also decided and not a ruling: task 0 ticks 0.2 to 0.5 in PHASE_PLAN.md citing the judging session's own words at PHASE_OUTCOME.md:41, which unit 384's tick-up missed, and appends - never edits - a correction recording that unit 384's run ended on an API 529 after task 0 and that its ## UNIT 7 - STEP 2 entry was graded from unit 383's leftover report, which is the same launcher fault criterion 2.6 records about the Olivia phase happening a second time; it is logged to the owner, not chased, and no task here opens the archived Olivia file.
LICENCE: PHASE_PLAN.md R44 - Tim, 2026-09-21, from the KC3FL contact: every card has an X, a live carrier is visible and gates the buttons, Log on every card from the start, every hand-back moves the turn - which is this unit's whole subject and which, being later than the plan's section 1, licenses a second refusal at the arm stage under section 6's the later ruling wins; step 9's entry opened by unit 379's judging-session done; PHASE_PLAN.md section 6 - a hint, a label, a number, a layout, a test's shape and a mechanism arithmetic will not allow are the arbiter's, and a must-pass missed by a little ships partial; ARBITER.md section 8, which makes a separate judging session's STATE_AFTER the evidence and is why 0.2 to 0.5 are ticked and 7.2 is not; ARBITER.md section 4, whose reading returned NOT FOUND on tonight's approach line; PSK31 plan R1 and R8, R11, R12, R13, R14, R19, R31; HM-DEC-018 section 2.1; HM-DEC-139, HM-DEC-155, HM-DEC-165; CLAUDE.md 0.0, 0.2; FACT-004
ACCOMPLISHED: The four things Tim watched go wrong in one four-minute contact this afternoon stop being possible. Hamlet will not let him put a signal on top of a man who is still sending - his row and his card say the other station is sending, the four ways to transmit are held with a sentence saying why, and a press that gets through anyway is refused at the one door that composes, with the record naming the reason; and because a gate that sticks would be worse than the fault it fixes, the hold lets go the moment the other carrier drops and never holds him on an empty band. Log is on the conversation card from the moment there is somebody to log, and it writes only what actually passed - a half exchange never looks like a whole one and nothing is invented to fill a field. The card follows the conversation past its first turn, so a second hand-back moves it and says so as a guess where the reading was not clean. And he can clear a card or a receipt off his own screen with an X, which removes it from the screen and from nothing else.
ADVANCES: step 9, criteria 9.1, 9.2, 9.3, 9.4 and 9.5 - the whole step, which has zero units spent. It advances no other step: 2.4 and 2.6 are deliberately left to a record unit after this phase's last code change, and 4.3 is cut down.
END-ARBITER-DECISION
```
