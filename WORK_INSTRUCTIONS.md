# Work instruction 264 - the whole contact, end to end, through the application

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project - nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

**All four were checked against the tree at `HEAD d8a92a6` while this instruction
was written.** `SHACK_FACTS.md` and
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` are present; neither
`CoreHMI.sln` nor `MURC.sln` exists; the only solution at the root is
`Hamlet.sln`. Check them anyway.

---

## THE THREE RULES THAT KILLED FOUR SESSIONS

**Tim's rulings of 2026-09-05. Not this unit's to weigh.**

**1. A unit runs no test suite.** **A unit may run only the unit test it
constructs in that work instruction**, filtered by exact name, in the foreground,
with a stated timeout of a few minutes. **An unfiltered `dotnet test` on any
project is forbidden.**

**2. Never background a command and poll for it.** Three sessions were killed by
the watchdog on 2026-09-05, at 33 to 38 minutes, each sitting in
`until grep -q "exited with code" ...; do sleep 15; done` with a `900000` ms
timeout.

**3. THE WATCHDOG FIRES AFTER TWELVE MINUTES WITH NO STATUS WRITE.** It killed
unit 257 at fourteen minutes with three files written and none committed. Units
258 through 263 wrote `PROJECT_STATUS.md` repeatedly and lost nothing. **The
status write is part of the work, not part of the reporting.**

`dotnet build` is allowed, foregrounded, with a timeout.

**This unit opens no serial port and keys nothing, and it plays no sound.**
Everything here is proved against the `FakePort` and the transmit fakes that
units 253 to 263 left in the tree. There is no real-endpoint task tonight and
none is wanted: units 256, 262 and 263 have each already played into this
machine's own card, and nothing further is learned by doing it a fourth time.

---

## THE TOOL RULE

`tools\arbiter\outcome-append.bat` has been refused for **eleven consecutive
units**, and `tools\arbiter\validate-output.bat` in twenty-one forms across nine.
**Try each once, verbatim, and record the refusal text.** Then do the work with
the file-editing tools in the exact format the script writes - twelve fields,
same order, ASCII, existing entries untouched. **Do not spend a second call on a
refused form and do not treat a refusal as a halt.** `PHASE_PLAN.md`: *if the
shell refuses a call, use the file-editing tools.*

Unit 263 recorded that `validate-output.bat` prints its own seven rules in its
header and does not read them from `CLAUDE_CODE.md` at run time, so where the
script cannot be run **the rules can be checked by hand against that header** -
and doing so caught two real failures in unit 263's own report. Do the same, and
say plainly that it is a hand check standing in for a run.

`outcome-read.bat --approach` embeds its argument in PowerShell single quotes, so
an apostrophe in the approach text is a parse error. Recorded at units 262 and
263, confirmed again tonight, **not this unit's to repair.**

---

## Why this unit exists

**This is work instruction 264. Eleven units have been spent on this phase, 253
through 263**, and `PHASE_OUTCOME.md` carries twenty-two entries because every
unit since 253 has been recorded twice.

```
PHASE GOAL: Hamlet works stations on the air.
UNIT GOAL:  A whole contact walks through the application - two clicks, two
            transmissions, the ledger, the row and the telemetry file - and
            the row ends up reading "complete".
ADVANCES:   Step 6, criterion 2 - "the transmitted slots appear in telemetry
            and the row reads complete." Only Tim's contact can close that
            criterion. This unit proves its mechanism, so that when he makes
            the contact the evidence exists rather than being discovered
            missing afterwards, when the contact cannot be repeated.
```

**Why this and not something else.** Unit 263's report says, correctly, that no
exit criterion across steps 1 to 5 remains that a unit on this machine can
advance - what is left there is the level, which is Tim's to read off the radio
under FACT-004. That is true, and it is not the same as the work being finished.
**Step 6 has two halves: the contact, which is Tim's, and the evidence the
contact must leave behind, which is the tree's.** The second half has never been
walked.

Every part exists and each was proved alone. **Nothing has ever joined them.**
Measured at `HEAD d8a92a6` while this instruction was written:

| The part | Where | Proved by |
|---|---|---|
| the menu and the click | `MainWindowViewModel.cs` | unit 260, against a real control tree |
| arming, one click one message | `Ft8ArmedSend` | units 259, 260 |
| the boundary fires in the live app | `MainWindowViewModel.cs:7618` -> `:8220` -> `:8237` | **no test drives the tick** |
| key, play, unkey | `Ft8TransmitSequence` | units 255, 256, 262 |
| the ledger books what went out | `MainWindowViewModel.cs:8279` -> `Ft8ContactLedger.cs:225` | **one call site, never driven twice for one station** |
| "complete" | `Ft8ContactState.cs:186` | unit 258, against a hand-fed ledger |
| the row's contact cell | `MainWindowViewModel.cs:7823`, `:7884`, `:7914` | unit 258, on rows built from decodes |
| the transmit record reaches a file | `Ft8TransmitSequence.cs:404` | unit 255, **through the engine's own telemetry, not the application's** |

**Two seams in that table have never had a test through them**, and both are
inside step 6's criterion 2. They are named in task 1 and this instruction does
not tell you what you will find there - it tells you where to look and what to
quote.

**And the base rate says look.** Unit 260 reported that what stood between step 6
and being attempted was no longer code. Unit 261 then found there was no stop
button at all. Unit 262 then found the application could transmit through none of
this machine's four render endpoints - every real click keyed the radio, threw at
the sink, and put nothing on the air. Unit 263 then found the stop left 8,345 ms
of audio going out after the operator pressed it. **Three units in a row, each
after an honest report saying the code was done, each finding something that
would have wrecked or endangered the first live transmission.** That is not a
reason to keep going forever. It is a reason to walk the one path nobody has
walked before handing a licensed operator an antenna.

---

## Verify this instruction against the tree

**Every line number, quotation and claim above and below was read from
`HEAD d8a92a6`. Check them.** Where the tree disagrees with this instruction,
**the tree wins**: report the mismatch in section 4 and continue with what the
tree says. **Do not repair the instruction and do not repair `PHASE_OUTCOME.md`.**

**Do not edit any existing `PHASE_OUTCOME.md` entry.** Unit 258's ruling stands
and units 261 and 263 both honoured it: a unit rewriting an entry that is not its
own is worse than a record that disagrees with itself in public. Append yours and
put any correction there and in section 4.

**Failures you should expect and must not chase:**

- `CwAdjudicationTests.ASpeedChangeInRealisticAudio`.
- The 51 CW cases in `docs/unit239-failing-set.txt`.
- The `Ft8Sharp.Deep.Tests` whole-type-list tripwire.

These are inherited reds named in `PHASE_PLAN.md` and are **never chased**.

**Three things this instruction expects to be told it got wrong. Say so plainly
if so; none is a halt, and each is a real answer:**

1. **That task 2's walk fails.** It is written to be run against the tree as it
   stands and to be quoted either way. **If it passes whole on the first run,
   that is the finding**, and you say so, quote it, and go to task 4 - do not
   manufacture a red to have one. Unit 263's red was real; a decorative one is
   worse than none.
2. **That the contact cell is computed as of the row's own slot** rather than as
   of now - `MainWindowViewModel.cs:7914` reads
   `Ft8ContactStates.Read(record, row.SlotStartUtc)`. Whether that means the row
   does not move after a send is a question for task 1's measurement, not for
   this instruction's assertion.
3. **That `Transmit` telemetry survives the application's own enabled-category
   predicate.** `App.axaml.cs:42` passes
   `category => _settings.IsTelemetryEnabled(category)`, and `AppSettings.cs:395`
   says unknown categories are on, which suggests it does. Measure it through the
   application's writer rather than reasoning from those two lines.

**Reported from the reload, for you to judge and not for me to repair**
(`ARBITER.md` §5):

- `PROJECT_STATUS.md` carries `RULES_AT: HM-DEC-157` while `CLAUDE.md` §1's
  highest is `CPS-DEC-0152`. Reported, not chased.
- Four files are modified and uncommitted at the root - `PHASE_OUTCOME.md`,
  `PHASE_STATUS.md`, `RUN_LEDGER.md` - and `SESSION.lock` is deleted, with
  `.run-unit/watched.rc` untracked. **They are the previous unit's and the
  launcher's bookkeeping. Commit what is the phase's record before you start
  task 2** so that a watchdog kill costs a diff and not a loss, and say in
  section 4 what you committed and what you left.

---

## Rulings in force

**Transcribed from `PHASE_PLAN.md` and `SHACK_FACTS.md`. Not this unit's to
re-argue. Do not re-argue them.**

**The dummy load is withdrawn.** HM-DEC-008 and HM-DEC-098 superseded,
2026-09-06. **Do not reference it, do not propose it, do not treat its absence as
a risk.** Tim operates a licensed station on an antenna and Hamlet transmits on
the air.

**One click, one message.** Ruled 2026-09-06. Hamlet transmits because the
operator clicked. **Never on a timer, never on a decode, never to continue a
contact.**

**Right-click sends immediately, in the next slot, with no confirmation.** Ruled
2026-09-06.

**Nothing is forbidden in the menu.** The expected next message is highlighted;
everything valid stays clickable; a repeat is correct behaviour and shows its
count.

**A contact is never closed by the app.** Complete is shown when the exchange has
what a QSO needs. `73` is politeness, not a requirement, and **its absence never
withholds complete.**

**`Ft8Sharp` is a faithful MIT port and nothing in this phase changes a line of
it.** `Ft8Sharp.Deep` is GPL-3.0.

**The engine is not told that tabs exist** (§0.1). **Nothing interprets a
message** (§12.1) - a row's state is derived from which messages passed between
two callsigns, which is bookkeeping, not meaning.

**The three things no unit may reason past:**

1. **The abort.** Every path that keys the transmitter has a same-thread,
   no-await abort - CI-V `0x17` with `0xFF`, PTT off as the fallback. No unit
   ships a keying path before its abort is watched to fire.
2. **One click, one transmission.** A transmission he did not ask for is this
   phase's one unrecoverable fault.
3. **Licence privileges.** Hamlet never transmits outside them. The Settings gate
   is not bypassable from any send path.

**`SHACK_FACTS.md` FACT-004.** There are two computers and only one has a radio
on it. **No radio has ever been attached to this machine.** No measurement of
this machine's audio endpoints says anything about the IC-7300's USB codec, and
**which machine a piece of evidence came from is part of the evidence** - say so
in every measurement you report.

**HM-DEC-018 and `TransmitRecord`'s own rule:** the transmit telemetry record
carries when a slot went out, where, how long for and how the radio came out of
transmit - **never message content and never a callsign.** Task 4 asserts this
rather than trusting it.

### One ruling made tonight by the arbiter, and it is not yours to re-argue

**Step 1's fifth criterion - "no transmitting code exists yet when this step
closes" - is a sequencing gate and it was passed at unit 253's close.** It is not
a standing invariant. The word is *yet*; the criterion's stated purpose is that
no unit ships a keying path before its abort is watched to fire; and unit 253
measured it met by a grep that returned `TransmitAbort`'s own declaration and
nothing else. Read as a permanent invariant it makes step 1 unclosable unless
steps 3 and 5 are deleted, which contradicts the plan's own ordering that step 3
*depends on* step 1.

This has flapped three times - `blocked` at one judging session, back to
`partial` at unit 262's arbiter, and `partial` again with the same objection
recorded at unit 263's. **It is settled here under `PHASE_PLAN.md`'s own licence
that the steps are a hypothesis and the arbiter may move a target found to have
been measured wrong, recording the evidence.** Step 1's five criteria are met.

**What you do about it: in task 5, set step 1 to `done` in the header lines of
both `PHASE_STATUS.md` and `PHASE_OUTCOME.md`, and say in your report that you
did it on this instruction's authority and not on your own judgment.** Change
nothing else about step 1 and write no new step 1 test.

---

## Status cadence

**Write `PROJECT_STATUS.md` before you start each task and again when it lands**,
and never let eight minutes pass without a write. The watchdog fires at twelve
minutes with no status write and it has already cost this phase one whole unit.
`TASK: n of 5`, and `NOTE:` carrying what actually happened, not what is planned.

**Read the clock for `UPDATED:`.** Unit 263 wrote composed increments that ran
half an hour ahead of the real clock and corrected them at the end. A timestamp
written into the future defeats the one signal that catches a stopped session.

---

## Tasks

### Task 1 - the trace, and measure before you build (no product code)

Write `docs/unit264-whole-contact-trace.md`. **Every answer carries a file, a
line and a quotation.** No product code in this task, no test in this task.

1. **Where is `_contacts` built and with what callsign?** `MainWindowViewModel.cs:1129`
   declares it and `:7904` constructs it. Quote the construction, say what `mine`
   is and what happens when it is empty or absent, and confirm whether the
   instance `ContactTextFor` reads at `:7914` is the same instance
   `AtSlotBoundaryAsync` writes to at `:8279`.
2. **What recomputes a row's contact cell, and when?** `:7823` assigns
   `row with { Contact = ContactTextFor(row) }`. Find every caller of that line's
   enclosing method and say what triggers each. **Then answer the question that
   matters: after `AtSlotBoundaryAsync` books a send at `:8279`, is there
   anything that makes the row recompute, or does the cell keep the value it was
   built with?**
3. **As of when is it computed?** `:7914` reads
   `Ft8ContactStates.Read(record, row.SlotStartUtc)`. Say whether the state is
   evaluated at the row's own slot or at the current slot, and what that means
   for a row whose last decode was three slots before the operator's final
   transmission.
4. **Can `complete` be reached at all through the application?**
   `Ft8ContactState.cs:186` requires `record.Sent.Count > 0`,
   `ours.Any(IsGridOrReport)` and `ours.Any(IsAcknowledgement)`. Working from
   `Ft8ContactLedger.RecordSent` at `:225`, list the exact sequence of operator
   messages that would satisfy it, and confirm that `RecordSent` populates
   `Fields` - `IsComplete` discards any message where `Fields` is null.
5. **Does the application's own telemetry write a `Transmit` line to disk?**
   Follow `App.axaml.cs:39-42` -> `AppSettings.cs:395` ->
   `Ft8TransmitSequence.cs:404-405`. Name the folder, the file, the event name
   and the category. **Then say whether any existing test reads that line back
   through the application's writer** -
   `WhereTheTransmissionStartsAndWhatTheRecordSaysTests` constructs a real
   `JsonlTelemetry`; say whether it does so on the application's path or the
   engine's.
6. **The dark tripwire.**
   `tests/Hamlet.RadioEngine.Tests/Transmit/TheUnkeyHappensWhateverGoesWrongTests.cs:404`,
   `ExactlyOneFileInTheShippedTreeCallsTheSequence`. Quote both of its
   assertions. Say which one fails, why it fails, and confirm by direct grep
   whether the safety property the second assertion carries - that
   `_sequence.RunAsync` has exactly one caller in `src/` - still holds.
7. **Is there any existing test that drives two successful sends through
   `AtSlotBoundaryAsync` for the same station and then reads the row's contact
   cell?** Name it, or write "none" and say what the nearest one does instead.

**Commit the trace as its own commit before task 2 starts.** Unit 263 did this
and it is why its finding survived.

### Task 2 - walk the whole contact, and quote what happens

One new test file,
`tests/Hamlet.App.Tests/ViewModels/TheWholeContactWalksThroughTheApplicationTests.cs`.

**The breakage it would have caught**, which `PHASE_PLAN.md` requires you to
name: Tim completes a QSO on the air and the row never says `complete`, or the
slots he transmitted are not in the telemetry file - so step 6's second criterion
fails after the one event in this phase that cannot be repeated.

Drive a whole exchange through the application's own path, on the fake port and
the substituted sink factory that units 260 and 262 left in the tree. **Open no
device, open no port, key nothing.** The exchange:

| Slot | Who | Message |
|---|---|---|
| 0 | W1ABC, heard | `CQ W1ABC EM12` |
| 1 | the operator, **clicked** | `W1ABC KC3QIS FN00` |
| 2 | W1ABC, heard | `KC3QIS W1ABC -09` |
| 3 | the operator, **clicked** | `W1ABC KC3QIS R-11` |
| 4 | W1ABC, heard | `KC3QIS W1ABC RR73` |

The operator's two transmissions go **through the menu and the arming**, not by
calling `RecordSent` by hand - the point of this test is the join, and a test
that books the send itself proves nothing about the application. Drive each
boundary through `AtSlotBoundaryAsync` the way the existing app tests at
`TheSendPathReachesARealRadioTests.cs:234-235` do.

Then assert, in this order:

1. Both boundaries ran and both reported the transmission sent.
2. The ledger holds two sent messages against `W1ABC` and three heard.
3. **The row for `W1ABC` reads `complete`** - the string an operator would see,
   read off the row the way the UI reads it, not off the ledger.
4. Both transmitted slots appear in telemetry, with the slot times they went out
   in.

**Run it against the tree exactly as it stands and quote the result verbatim,
whatever it is.** Filtered by exact name, foregrounded, with a stated timeout.
Commit it at that result, red or green, and say which in the commit message.

**If it is green whole, say so plainly and go to task 4** - the walk existing and
passing is a real and reportable answer, and task 3 is then correctly empty.
Do not invent a failure to fill it.

### Task 3 - fix what task 2 found, at the seam it is at

Only if task 2 is red. **Fix the seam, not the test.**

- **Write the reason at the site**, in the register the surrounding code uses.
- **Change nothing about how the ledger decides `complete`.** `Ft8ContactState`
  was proved by unit 258 and it is not what task 2 is testing.
- **Change nothing about arming, the sequence, the abort or the stop.** If the
  fix appears to want any of those, stop, report it in section 4, and leave it.
- **Re-run task 2's test and quote the green beside the red.**
- Then re-run, filtered by exact name, whichever of these your change could
  touch, and quote the counts: `TheOperatorCanStopItTests`,
  `OneClickSendsExactlyOneMessageTests`, `TheMenuIsUnderTheMouseTests`,
  `TheLedgerHoldsWhatPassedEachWayTests`. **A neighbour you did not run is a
  neighbour you did not check** - say which you ran and which you did not.

### Task 4 - the transmitted slot, read back out of the application's own file

One test, in the same file or beside it. **The breakage it would have caught:**
the transmit record is written to a telemetry instance the application never
gives the sequence, or to a category the operator's settings switch off, so the
file Tim sends back after his contact has nothing in it.

Construct `JsonlTelemetry` **the way `App.axaml.cs:39-42` does**, including the
enabled-category predicate from a real `AppSettings`, into a temporary folder.
Drive one send. Then:

1. **Read the file back off disk** and find the transmit line by its event name.
2. Assert the slot time and the duration are there and are the ones that went
   out.
3. **Assert the line contains no callsign and no message content** - search the
   raw text for `KC3QIS`, `W1ABC` and the message text and assert each is absent.
   HM-DEC-018 and `TransmitRecord`'s own rule, checked rather than trusted.
4. Say which machine every figure came from. FACT-004.

### Task 5 - the dark tripwire, and the header line - **THE NAMED DROP CANDIDATE**

**This is the task to drop if the night runs short**, and it is named as the drop
candidate deliberately: the safety property underneath it still holds and unit
263 checked it directly, so what is broken here is the guard and not the thing
guarded - whereas tasks 2 to 4 are step 6's criterion 2 itself. **If you drop it,
say so in section 3 and leave it named for the next unit.**

1. `ExactlyOneFileInTheShippedTreeCallsTheSequence` has been red since unit 260,
   fifteen commits, and it is **not** on `PHASE_PLAN.md`'s list of inherited
   reds - so it is a real red and not one to leave alone. From task 1 question 6
   you know why it fails. **The substantive question, which unit 263 named and
   left: should the assertion distinguish constructing an `Ft8TransmitSequence`
   from reaching `RunAsync` through one?** Decide it, make the test say what it
   means, and **keep the assertion that carries the safety property intact** -
   that `_sequence.RunAsync` has exactly one caller in `src/`. Quote it green.
2. Set **step 1 to `done`** in the header lines of `PHASE_STATUS.md` and
   `PHASE_OUTCOME.md`, per the arbiter's ruling above. Say in your report that it
   was done on this instruction's authority. Touch no other step's line.

---

## Parked - do not touch, do not raise

- **The level.** Step 2's criterion 4 and step 3's criterion 1's level half are
  Tim's to read off the radio under FACT-004. Do not measure it, do not infer it,
  do not build a control for it tonight.
- **Step 4's criterion 6** - the synthesized corpus rather than a WSJT-X capture.
  Recorded, parked, and not reopened tonight.
- **Unit 263's section 4 items 1 and 2** - unit 261's superseded reasoning about
  WASAPI registrations, and the two line numbers. Both are settled and neither is
  in this unit's way. **Logged, not chased.**
- **`Ic7300Rig.AbortCw`, `SendCwAsync`, `KeyerCwSender`, `CwTransmitter`,
  `AutoCaller`, `CivWrites.TuneNow`.** CW send and band scan are out of this
  phase.
- **The licence-gate wording question banked at unit 253**, and the other callers
  of `TransmitGuard.Check`.
- **The `RULES_AT` disagreement** between `PROJECT_STATUS.md` and `CLAUDE.md` §1.
  Report it if you touch it; do not go and reconcile the ruling files.
- Automatic sequencing, logging, FT4, PSK31, WSPR, the OSD re-encoding count,
  `ReusableWindow`, `ProcessDelayForTests`, the tap's owner, the waterfall's
  first row, `validate-output.bat`'s permitted-spellings bug, the 101.33 ms pulse
  above 6 kHz, the CW decoder and its inherited reds.

---

## What not to do

1. **Do not open a serial port, do not key anything, and do not play sound.**
   `PHASE_PLAN.md`, and FACT-004 makes it meaningless here anyway. Unlike unit
   263 there is no real-endpoint task tonight and none is wanted.
2. **Do not repair what you find; report it.** `ARBITER.md` §5 - the arbiter
   reports tree faults in the instruction and lets the unit decide, and the unit
   reports what the instruction got wrong rather than rewriting the record.
   Task 3 is the one licensed repair and it is bounded to the seam task 2 finds.
3. **Do not edit `PHASE_OUTCOME.md`'s existing entries.** Append your own. The
   only edit licensed tonight is step 1's word in the two header lines, task 5.
4. **Do not change a line of `Ft8Sharp` or `Ft8Sharp.Deep`.** Ruled.
5. **Do not run an unfiltered `dotnet test`**, do not background a command and
   poll for it, and do not add a test without naming the breakage it would have
   caught. `PHASE_PLAN.md`, *What a unit runs*.
6. **Do not book a send in the ledger anywhere but where it is booked now.**
   `MainWindowViewModel.cs:8279` is the one call site of `RecordSent` in the
   tree and it fires only where the run says the whole transmission went. A
   message booked at the moment of arming would put a transmission in the ledger
   that a licence refusal, a cancel or a missed boundary meant never happened.
   **If task 3's fix appears to want a second call site, it is the wrong fix.**
7. **Do not make the row's state depend on anything but which messages passed.**
   §12.1. If the cell needs to recompute at a different moment, change *when* it
   is computed, not *what* it decides.
8. **Do not touch `TransmitGuard.Check` or any existing caller of it**, and do
   not write a second copy of the licence rule.
9. **Do not make the abort or the stop conditional on anything you add**, and do
   not change the transmission's placement in the slot, the composer's rate
   handling, or anything units 262 and 263 landed.
10. **Do not close step 6.** It is Tim's, it needs a radio and an antenna, and no
    unit can perform it. Proving its mechanism is not performing it - say the
    difference plainly in your report.

---

## Committing and pushing

**Commit at the end of each task**, in `CLAUDE_CODE.md`'s message form, with the
task's own evidence in the body. Push when the last task lands. **A unit killed
by the watchdog with work uncommitted loses it** - unit 257 lost three files that
way. Commit the trace document as its own commit before task 2 starts, and commit
task 2's test at whatever colour it comes out, saying the colour in the message.

---

## Reporting

**`output.md`. The ordering block comes first, before the header.**
`validate-output.bat` refuses a report without it, so a report that omits it is
rejected whatever else it contains.

```
READ IN THIS ORDER

A. THE PHASE GOAL IS "Hamlet works stations on the air", and every step's
   state: step 0 done; step 1 done as of tonight, on work instruction 264's
   authority and not on this unit's judgment; steps 2 to 5 partial; step 6 not
   started. Say whether any of those changed by anything this unit measured.
   Repeat the fact that shaped this unit: what remains open in steps 2 to 5 is
   the level, which is Tim's to read off the radio under FACT-004 - and say
   that step 6 has two halves, the contact which is his and the evidence which
   is the tree's, and which half this unit worked on.

B. THIS UNIT AIMS AT STEP 6 AND CLAIMS THE MECHANISM OF ITS SECOND CRITERION,
   NOT THE CRITERION. Step 6's three criteria are: Tim answers a CQ on 14.074
   or 7.074 and completes an exchange; the transmitted slots appear in
   telemetry and the row reads complete; and what he saw, in his words, is
   recorded. State all three, say which of them a unit can touch at all, and
   then answer the question this unit exists for:

   DOES A WHOLE CONTACT WALKED THROUGH THE APPLICATION END WITH THE ROW READING
   "COMPLETE" AND BOTH TRANSMITTED SLOTS IN THE TELEMETRY FILE - YES OR NO,
   BEFORE THIS UNIT AND AFTER IT?

   Give both answers. If it was already yes before this unit, say so plainly -
   that is a real finding and not a failure. If it was no, say at which of the
   four assertions it failed and what the seam was. Say whether task 5 was
   dropped.

C. THIS REPORT'S OWN FINDINGS, weighed against A and B. Name how many items
   section 4 raises and, for each, say whether it stands in the way of anything
   named in B. Three are expected there and none is blocking if it lands as
   this instruction predicts: whether task 2 was red or green as it stands,
   whether the contact cell is computed as of the row's slot or as of now, and
   whether Transmit telemetry survives the application's own category
   predicate. If section 4 raises nothing, say so in a sentence - CLAUDE_CODE.md
   section 8 makes that a real answer.
```

Then the six-line header:

```
UNIT: 264 - <state> at task n of 5 - <timestamp>
PHASE GOAL: Hamlet works stations on the air
UNIT GOAL:  A whole contact walks through the application and the row reads
            complete.
ADVANCED:   <step 6 criterion 2's mechanism, with what you proved - or none,
            and why>
NUMBER:     <of the four assertions in task 2, how many passed against the tree
            as it stood, and how many after>
DRIFT:      <consecutive units without a criterion advance, and say whether
            this unit ends that count or continues it, honestly>
```

**Section 3 must lead with task 2's first run against the untouched tree**,
quoted verbatim - the four assertions and which of them held. That is the state
of the join in one block, and everything after it is either a repair or a
confirmation. **If it was green, lead with the green and say that the walk
existed and held**; if it was red, lead with the red and the green comes after.

**Section 4 is for what stands in the way.** A note recorded for the record is
not a ruling request; say which yours are.

---

```
ARBITER-DECISION
STEP: 6
APPROACH: walk a whole QSO through the application - right-click, send, ledger, row reads complete, and the transmitted slots read back out of the telemetry file on disk
MOVE: work around
WHY: No exit criterion in steps 1 to 5 is reachable by a unit on this machine - what is left there is the level, which FACT-004 defers to Tim - so the reachable work is step 6's second half. Step 6 has two: the contact, which only Tim can make, and the evidence that contact must leave behind, which is criterion 2's "the transmitted slots appear in telemetry and the row reads complete" and is entirely in the tree. Every part of that chain exists and each was proved alone, but I measured at HEAD d8a92a6 that two seams in it have never had a test through them - nothing drives the live slot tick, and RecordSent's single call site has never been driven twice for one station and then read off the row - so whether a completed exchange actually reads complete is unknown rather than proved. The loop test returned NOT FOUND against all twenty-two entries, and I judge it not a loop: the nearest neighbours are units 259 and 260 wiring the menu to the sequence and unit 258 building the ledger, and each of those built a part and proved it in isolation, which is what makes the join between them untried rather than a repeat.
STATE: not started
DECIDED: Three on my own authority. First, that a unit is authored rather than the phase handed to Tim tonight - the plan is not satisfied, step 6 is not started rather than unachievable, and ARBITER.md section 8 licenses stopping only on a satisfied plan; the base rate also argues it, since unit 260 said what stood between step 6 and being attempted was no longer code and the three units after it each found something that would have wrecked the first live transmission. Second, that step 1's fifth criterion is settled as a sequencing gate passed at unit 253's close rather than a standing invariant, because read as an invariant it makes step 1 unclosable unless steps 3 and 5 are deleted, which contradicts the plan's own ordering that step 3 depends on step 1 - the state has flapped between blocked and partial across three judgments and the instruction closes it done and tells the unit it is acting on my authority, not its own. Third, that this unit claims a criterion's mechanism and says explicitly that it does not claim the criterion, because four consecutive units have recorded no advance and a fifth "none - clears a blocker" would be true but would also hide that the remaining work has a criterion attached to it; overclaiming step 6 as advanced would be the plausible-rather-than-true answer ARBITER.md section 7 warns about, and both halves are written into ADVANCES rather than one.
LICENCE: PHASE_PLAN.md's named alternatives to stopping - the tree wins, report the mismatch and continue, and a target not reached is closed with the figure reached and what was tried - together with "the steps are a hypothesis, not a contract", which licenses both taking unattempted ground inside a step and moving a target found to have been measured wrong, recording the evidence, which is what step 1's fifth criterion is. Step 6's own criterion 2 licenses the subject in its own words: the transmitted slots appear in telemetry and the row reads complete. PHASE_PLAN.md's ruling that a contact is never closed by the app, and that complete means the exchange has what a QSO needs with 73 never withholding it, governs what task 2 may assert. SHACK_FACTS.md FACT-004 licenses the fake port and the substituted sink factory, forbids any inference about the IC-7300, and keeps the level deferred and unclaimed.
ACCOMPLISHED: When Tim finishes his contact, Hamlet will have the record of it. The row he worked will say "complete" and the two slots he transmitted will be in the telemetry file with the times they went out - which is the evidence step 6 asks for, and it is evidence that can only be gathered once, because a contact cannot be made again. Before this unit every part of that chain existed and had been proved on its own, and no test had ever walked one contact from the right-click to the word on the row.
ADVANCES: Step 6, criterion 2 - "the transmitted slots appear in telemetry and the row reads complete." This unit moves that criterion's mechanism from unproven to proven and does not close the criterion, which only Tim's contact can do. It also clears the last two untested seams under step 6: nothing drives the live slot tick in any test, and RecordSent has never been driven twice for one station and the row then read.
END-ARBITER-DECISION
```
