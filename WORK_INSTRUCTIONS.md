# Work instruction 263 - the stop stops the audio too

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

**All four were checked against the tree at `HEAD 5186820` while this instruction
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
258 through 262 wrote `PROJECT_STATUS.md` repeatedly and lost nothing. **The
status write is part of the work, not part of the reporting.**

`dotnet build` is allowed, foregrounded, with a timeout.

**This unit opens no serial port and keys nothing.** It may *enumerate* audio
endpoints and, in task 5 only, *play into one* - and task 5 is the named drop
candidate. Everything else is proved against the `FakePort` and the transmit
fakes that units 253 to 262 left in the tree.

---

## THE TOOL RULE

`tools\arbiter\outcome-append.bat` has been refused for **ten consecutive
units**, and `tools\arbiter\validate-output.bat` in seventeen forms across eight.
**Try each once, verbatim, and record the refusal text.** Then do the work with
the file-editing tools in the exact format the script writes - twelve fields,
same order, ASCII, existing entries untouched. **Do not spend a second call on a
refused form and do not treat a refusal as a halt.** `PHASE_PLAN.md`: *if the
shell refuses a call, use the file-editing tools.*

Unit 262 recorded that `outcome-read.bat --approach` embeds its argument in
PowerShell single quotes, so an apostrophe in the approach text is a parse error.
**It was confirmed again while this instruction was written** - the loop test was
run twice, the first call lost to `sink's`. It is not this unit's to repair, and
it is recorded here so a third session does not pay for it.

---

## Why this unit exists

**This is work instruction 263. Ten units have been spent on this phase, 253
through 262.** `PHASE_OUTCOME.md` carries twenty entries because every unit since
253 is recorded twice - once from the arbiter's decision block and once from the
session that judged the report. Step 0 is `done`; steps 1 through 5 are
`partial`; step 6 has not started.

```
PHASE GOAL: Hamlet works stations on the air
UNIT GOAL:  The operator's stop stops the audio as well as the carrier. Today
            it takes PTT off and Hamlet goes on feeding the rest of a 12.64 s
            FT8 transmission into the radio. Where the unkey frames do not
            land - which is the exact case the abort exists for - the whole
            transmission still goes out over other people's band after the
            operator pressed stop.
ADVANCES:   None. This unit clears a blocker under step 6. It clears the one
            open, named, unrepaired finding on the phase's first
            non-negotiable, recorded by unit 261 in PHASE_OUTCOME.md: "The
            abort takes the carrier off but does not stop the sink, which keeps
            playing into an unkeyed radio for the rest of the slot."
```

**Read the next paragraph before you form an opinion about the size of the job.**

Every remaining exit criterion across steps 1 to 5 is either recorded met or is
a radio-side figure `SHACK_FACTS.md` FACT-004 puts beyond this machine. **There
is no criterion left that a unit on the development machine can advance.** What
is left before step 6 is not a criterion - it is whether Hamlet is fit to be
pointed at an antenna. Unit 260's report said *what stands between him and
trying is no longer code*; unit 261 found the missing stop button an hour later,
and unit 262 found that the send path could not transmit through a single one of
this machine's four render endpoints. **This is the third thing, and unit 261
named it in its own entry and deliberately did not repair it.**

Here is what was measured from the tree at `HEAD 5186820` while this was written:

| Measured | Where |
|---|---|
| `PlayAsync` **already takes a `CancellationToken`**, and its documented contract is *"Stops the playing"* | `ITransmitAudioSink.cs`, the `cancellationToken` param |
| The real sink **polls that token** - `while (written < total && !cancellationToken.IsCancellationRequested)`, and again in the drain loop | `WasapiTransmitSink.cs:339`, `:372` |
| The sink's `finally` calls `_client.Stop()` and `_client.Reset()` under its own lock | `WasapiTransmitSink.cs:398-402` |
| **There is not one `Register(` call on the whole transmit path** - not in `WasapiTransmitSink.cs`, not in `Ft8ArmedSend.cs`, `Ft8Composer.cs`, `Ft8TransmitSequence.cs` or `ITransmitAudioSink.cs`. `grep -rn "Register("` over those files returns nothing | measured, `HEAD 5186820` |
| `Ft8TransmitSequence.RunAsync` **passes its token straight through** to `PlayAsync` | `Ft8TransmitSequence.cs:288` |
| `Ft8ArmedSend.AtBoundaryAsync` **passes its token straight through** to `RunAsync` | `Ft8ArmedSend.cs:296` |
| **The application calls it with no token at all**, so the whole chain runs on `CancellationToken.None` and nothing can ever cancel it | `MainWindowViewModel.cs:8258` - `await _armedSend.AtBoundaryAsync(boundaryUtc)` |
| `StopNow` un-arms and fires `TransmitAbort`. **It touches no audio and holds nothing that could.** | `Ft8ArmedSend.cs:235-251` |
| The engine's fake **ignores the token entirely** and returns `Task.FromResult` immediately | `tests/.../FakeTransmitAudioSink.cs:71-95` |

**So the rope is already strung from the operator's thumb to the sound card, and
nobody tied the last knot.** The cancellation path exists, is polled rather than
registered, and stops the endpoint cleanly - and the one thing missing is a
cancellation source in `Ft8ArmedSend` that `StopNow` can cancel.

**Unit 261 recorded a reason not to do this**, and it is the first thing task 1
must measure rather than accept:

> *"Threading a cancellation source from `Ft8ArmedSend` into `RunAsync` would run
> WASAPI's registrations synchronously on the operator's UI thread, which is the
> one property the abort may not have, so it is reported as a finding rather than
> repaired here."*

**The measurement above says there are no registrations.** `Cancel()` on a source
with no registered callbacks sets a flag and returns. If task 1 confirms that, the
stated reason does not hold and the work is small; **if task 1 contradicts the
measurement above, the measurement above is wrong and you report that instead** -
see the next section. Either way, **the abort's same-thread, no-await, never-waits
property is the thing this unit must not damage**, and task 3 asserts it rather
than assuming it.

---

## Verify this instruction against the tree

**Every line number, quotation and claim in the table above was read from
`HEAD 5186820`. Check them.** Where the tree disagrees with this instruction,
**the tree wins**: report the mismatch in section 4 and continue with what the
tree says. **Do not repair the instruction, do not repair `PHASE_OUTCOME.md`, and
do not edit unit 261's entry** - a unit rewriting an entry that is not its own is
worse than a record that disagrees with itself in public, which is unit 258's
ruling and it stands.

**Failures you should expect and must not chase:**

- `CwAdjudicationTests.ASpeedChangeInRealisticAudio`.
- The 51 CW cases in `docs/unit239-failing-set.txt`.
- The `Ft8Sharp.Deep.Tests` whole-type-list tripwire.

These are inherited reds named in `PHASE_PLAN.md` and are **never chased**.

**Two things this instruction expects to be told it got wrong.** Say so plainly
if so; neither is a halt:

1. That `Cancel()` on the source you create can reach no callback and therefore
   cannot block. If you find a registration anywhere on the path you build, the
   design changes and you say how.
2. That the buffer already inside the endpoint at the moment of cancellation is
   discarded by `_client.Reset()` rather than played out. Measure it; do not
   reason about it.

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

**A contact is never closed by the app.** `73` is politeness, not a requirement.

**`Ft8Sharp` is a faithful MIT port and nothing in this phase changes a line of
it.** `Ft8Sharp.Deep` is GPL-3.0.

**The engine is not told that tabs exist** (§0.1). **Nothing interprets a
message** (§12.1).

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

**Step 1's own criterion:** the abort **cannot be disabled, deferred, or made
conditional.**

---

## Status cadence

**Write `PROJECT_STATUS.md` before you start each task and again when it lands**,
and never let eight minutes pass without a write. The watchdog fires at twelve
minutes with no status write and it has already cost this phase one whole unit.
`TASK: n of 5`, and `NOTE:` carrying what actually happened, not what is planned.

---

## Tasks

### Task 1 - trace, and measure before you build (no product code)

**Write `docs/unit263-stop-audio-trace.md`.** Answer each question with a file,
a line number and a quotation. **Say for every measurement which machine it came
from.** No code outside this document.

- **Q1.** What cancellation token does the application hand to
  `Ft8ArmedSend.AtBoundaryAsync`? Quote the call site and its line. Follow the
  token from there to `PlayAsync` and state every hop.
- **Q2.** Does `WasapiTransmitSink.PlayAsync` **poll** the token or **register**
  a callback on it? Quote both loops and the `finally`. Run
  `grep -rn "Register("` over the transmit path and report the count.
- **Q3.** What do `_client.Stop()` and `_client.Reset()` do to samples already
  handed to the endpoint but not yet played? **How much audio can still leave the
  card after the token is cancelled** - in samples and in milliseconds - given
  this sink's `BufferFrames` and `WaitMilliseconds`? State the arithmetic.
- **Q4.** Do `FakeTransmitAudioSink` (`tests/Hamlet.RadioEngine.Tests/Transmit/`)
  and `FakeSink` (`tests/Hamlet.App.Tests/FakeTransmitParts.cs`) read the token
  at all? What does each do today if cancelled mid-play? **Unit 262 found the app
  fake was the worse of the two and that the app's send path runs against that
  one** - check which fake each test actually gets.
- **Q5.** **Unit 261 recorded that threading a cancellation source into `RunAsync`
  would run WASAPI's registrations synchronously on the operator's UI thread.**
  Measure whether any such registration exists. **If it does not, say so plainly
  with the evidence** - that is a correction to the phase's memory and it belongs
  in your report, not in an edit to unit 261's entry.
- **Q6.** What does `CancellationTokenSource.Cancel()` do when nothing is
  registered on it, and what can it throw? State what you are relying on and where
  you read it. If it can throw at all, task 3 must survive that.
- **Q7.** List every test in `TheOperatorsStopFiresFromEveryStateTests` by name,
  and say **which of them would still pass today if the audio never stopped**.
  That set is the measure of what the current green does not cover.

### Task 2 - make the fakes faithful, and watch the red

**A fake more permissive than the real thing hides the defect** - unit 262's
lesson, and it cost that unit a task to learn.

Give **both** fakes the one behaviour they lack: a play that **takes time and
honours the token**, returning how much actually went out. Keep the instant
behaviour available for the tests that rely on it; do not rewrite existing
assertions to suit the new shape.

**Then watch the red, and quote it.** With the tree as it stands: arm a send,
let a boundary run it, call `StopNow` mid-transmission, and show

- the wire carrying `17 FF` and `1C 00 00` - the carrier is off, and that part
  already works;
- **and the sink playing on to the end regardless**, `SamplesPlayed` equal to the
  full slot, with the milliseconds of audio that went out after the stop.

**That red is the unit's evidence and it must appear in the report.** A green
that was never watched red proves the test, not the fix.

### Task 3 - the stop cancels the transmission

Give `Ft8ArmedSend` **one** cancellation source beside `_armed`, created at the
boundary under the same lock, **linked to the caller's token**, and cleared in a
`finally`. `StopNow` cancels it on the calling thread.

**Design constraints, and each is asserted rather than argued:**

- **The abort is not put behind anything new.** Whatever order you choose for
  un-arm, cancel and `TransmitAbort.Fire`, **the frames must still reach the wire
  if the cancel throws or the source is already disposed.** Assert it by making
  the cancel path fail and reading the wire. Say which order you chose and why.
- **Nothing on the stop path is awaited.** Extend
  `NothingOnTheStopPathWaitsForAnything` to cover every new line, and say what it
  reads.
- **`StopNow` returns inside a stated bound** - name it, in the shape unit 253
  used - **while a full 12.64 s transmission is in flight.** A stop that waits on
  the transmission it is stopping is the one property it may not have.
- **The abort still fires from all six states in unit 261's table**, bytes quoted
  for each. Nothing there regresses.
- **One click still sends exactly one message.**
  `OneClickSendsExactlyOneMessageTests` runs green with no edit to its
  assertions, and the arming grep returns the same single call site before and
  after. Quote both.
- **No second way to un-arm and no second route to a keying frame.** One field,
  one lock, one line that clears it - `Ft8ArmedSend.cs`'s own remark, and it
  stays true.

Then the application: `AtSlotBoundaryAsync` must hand the boundary run a token
the stop can reach. **Do not add a second stop entry point in the view model** -
`MainWindowViewModel.cs:8426` already calls `StopNow` and stays the only one.

### Task 4 - the operator is told what was stopped

The stop's result records **three facts now, not two**: whether something was
un-armed, what the abort's frames did, and **whether the audio was told to
stop**. Extend `Ft8StopResult` and `Ft8StopOutcome` rather than adding a parallel
record beside them, and keep unit 261's rule that a keyed radio with neither
route out taken reads as `NothingReachedTheRadio` rather than as safe.

The Send area sentence says, in the operator's words, what happened to both -
the carrier and the sound. **A sentence that says "stopped" when only half of it
stopped is the failure here.**

### Task 5 - the same thing on a real endpoint - **THE NAMED DROP CANDIDATE**

**This is the drop candidate. Drop this and nothing else.**

Play a full transmission into a real render endpoint through the real
`WasapiTransmitSink`, cancel roughly a third of the way in, and measure:

- `SamplesPlayed` short of the total, by how much;
- how long after the cancel the card actually went quiet;
- `StopNow` returning inside the bound task 3 stated.

**No serial port is opened and nothing is keyed** - this is the sound card only,
on the development machine, and FACT-004 forbids inferring anything about the
radio from it.

**If you drop it, say so in section 3, name what stays unproved because of it,
and say that tasks 2 to 4 carry the assertion on the shape where the fault would
be.** Do not silently shorten it into something cheaper.

---

## Parked - do not touch, do not raise

- **Unit 262's two section 4 notes**: the refusal sentence carrying the engine's
  developer-facing clause in its middle, and that branch being unreachable on
  ordinary hardware. **Logged, not chased.** Both are recorded as notes rather
  than ruling requests and neither is in this unit's way.
- **The level.** Step 2's criterion 4 and step 3's criterion 1's level half are
  Tim's to read off the radio under FACT-004. Do not measure it, do not infer it,
  do not build a control for it tonight.
- **Step 4's criterion 6** - the synthesized corpus rather than a WSJT-X capture.
- **`Ic7300Rig.AbortCw`, `SendCwAsync`, `KeyerCwSender`, `CwTransmitter`,
  `AutoCaller`, `CivWrites.TuneNow`.** CW send and band scan are out of this
  phase.
- **The licence-gate wording question banked at unit 253**, and the other
  callers of `TransmitGuard.Check`.
- Automatic sequencing, logging, FT4, PSK31, WSPR, the OSD re-encoding count,
  `ReusableWindow`, `ProcessDelayForTests`, the tap's owner, the waterfall's
  first row, `validate-output.bat`'s permitted-spellings bug, the 101.33 ms pulse
  above 6 kHz, the CW decoder and its inherited reds.

---

## What not to do

1. **Do not open a serial port and do not key anything.** `PHASE_PLAN.md`, and
   FACT-004 makes it meaningless here anyway.
2. **Do not repair what you find; report it.** `ARBITER.md` §5 - the arbiter
   reports tree faults in the instruction and lets the unit decide, and the unit
   reports what the instruction got wrong rather than rewriting the record.
3. **Do not edit `PHASE_OUTCOME.md`'s existing entries**, unit 261's above all.
   Append your own and correct the record there.
4. **Do not change a line of `Ft8Sharp` or `Ft8Sharp.Deep`.** Ruled.
5. **Do not run an unfiltered `dotnet test`**, do not background a command and
   poll for it, and do not add a test without naming the breakage it would have
   caught. `PHASE_PLAN.md`, *What a unit runs*.
6. **Do not make the abort conditional on anything you add.** Not on a source
   being non-null, not on the application believing it is transmitting, not on a
   token's state. Unit 261 watched a stop conditional on the armed field do
   nothing at exactly the moment it was needed.
7. **Do not stop the audio by disposing the sink, by killing a thread, or by any
   path that runs a WASAPI call on the caller's thread.** The token is polled;
   use it.
8. **Do not touch `TransmitGuard.Check` or any existing caller of it**, and do
   not write a second copy of the licence rule.
9. **Do not change the transmission's placement in the slot**, the composer's
   rate handling or anything unit 262 landed.

---

## Committing and pushing

**Commit at the end of each task**, in `CLAUDE_CODE.md`'s message form, with the
task's own evidence in the body. Push when the last task lands. **A unit killed
by the watchdog with work uncommitted loses it** - unit 257 lost three files that
way. Commit the trace document as its own commit before task 2 starts.

---

## Reporting

**`output.md`. The ordering block comes first, before the header.**
`validate-output.bat` refuses a report without it, so a report that omits it is
rejected whatever else it contains.

```
READ IN THIS ORDER

A. THE PHASE GOAL IS "Hamlet works stations on the air", and every step's state:
   step 0 done; steps 1 to 5 partial; step 6 not started. Say whether any of
   those changed tonight and by what evidence. Say plainly, because it is the
   fact that shaped this unit: no exit criterion across steps 1 to 5 remains
   that a unit on the development machine can advance - what is left is
   radio-side, deferred to Tim under FACT-004, or step 6 itself.

B. THIS UNIT AIMS AT STEP 1 AND CLAIMS NO CRITERION OF IT. Step 1's five
   criteria were recorded met at unit 253's close and unit 261's; this unit
   clears the blocker under step 6 that unit 261 named and did not repair - the
   abort takes the carrier off and leaves Hamlet feeding audio into the radio.
   State the exit criteria of step 1 and mark each met-and-unchanged, and then
   answer the question this unit exists for: HOW MANY MILLISECONDS OF AUDIO
   STILL LEAVE THE MACHINE AFTER THE OPERATOR PRESSES STOP, MID-TRANSMISSION -
   before tonight and after. Give both numbers, say how each was measured and on
   which machine, and say whether task 5 was dropped.

C. THIS REPORT'S OWN FINDINGS, weighed against A and B. Name how many items
   section 4 raises and, for each, say whether it stands in the way of anything
   named in B. Two are expected there and neither is blocking if it lands as
   this instruction predicts: whether unit 261's stated reason for not doing
   this work held up under measurement, and whatever task 1 found that this
   instruction got wrong. If section 4 raises nothing, say so in a sentence -
   CLAUDE_CODE.md section 8 makes that a real answer.
```

Then the six-line header:

```
UNIT: 263 - <state> at task n of 5 - <timestamp>
PHASE GOAL: Hamlet works stations on the air
UNIT GOAL:  The operator's stop stops the audio as well as the carrier.
ADVANCED:   <none - blocker cleared, and which | or what you claim, with the
            criterion named>
NUMBER:     <ms of audio out after the stop, before -> after>
DRIFT:      <consecutive units without a criterion advance, and say that this
            unit was authored not to claim one>
```

**Section 3 must lead with the red**: the wire showing the carrier off and the
sink playing on regardless, quoted, with the milliseconds that went out after
the operator pressed stop. That is the fault in one paragraph, and the green
that follows means nothing without it.

**Section 4 is for what stands in the way.** A note recorded for the record is
not a ruling request; say which yours are.

---

```
ARBITER-DECISION
STEP: 1
APPROACH: stop the audio as well as the carrier - thread a cancellation source from Ft8ArmedSend.StopNow into the polling loop of the transmit sink
MOVE: work around
WHY: No exit criterion across steps 1 to 5 is still reachable by a unit on this machine - every one left is a radio-side figure FACT-004 defers to Tim - so the reachable work is the last open blocker under step 6, which is unit 261's own named and unrepaired finding that the stop unkeys the radio and leaves Hamlet feeding it audio for the rest of the slot. The loop test returned NOT FOUND against all twenty entries; the nearest neighbour is unit 261's "give the operator a route to the abort", and I judge it not a loop because that unit built the route and recorded the audio as deliberately left running, so this is the complement it named rather than a repeat of it, by a different mechanism - a polled token through the sink, not a CI-V frame.
STATE: partial
DECIDED: Three on my own authority. First, that a unit is authored at all rather than the phase being handed to Tim tonight - every remaining criterion is deferred or his, and the comfortable answer at three in the morning is that the code is finished, which is what unit 260 said an hour before unit 261 found there was no stop button and two before unit 262 found the send path could transmit through none of this machine's endpoints. Second, that this unit claims no step 1 criterion and says so in ADVANCES, because all five were recorded met at unit 253's and 261's closes and filling the field with one I know to be met is the plausible-rather-than-true answer ARBITER.md section 7 warns about. Third, that unit 261's recorded reason for not doing this work - that a cancellation source would run WASAPI registrations on the operator's UI thread - is handed to the unit as task 1's measured question rather than accepted or overruled here, because I measured zero Register( calls on the whole transmit path at HEAD 5186820 and a correction to the phase's memory should be made by the session that can watch it, not by me.
LICENCE: PHASE_PLAN.md's first thing the arbiter may not reason past - every path that keys the transmitter has a same-thread, no-await abort - together with step 1's own criterion that it cannot be disabled, deferred, or made conditional, which an abort that stops half of what is going out does not fully satisfy. The steps are a hypothesis not a contract licenses taking a partial step again on unattempted ground. PHASE_PLAN.md's named alternatives to stopping license the rest: the tree wins, report the mismatch and continue, and where the radio is wanted the step is closed on what can be proved here with what Tim must do named. SHACK_FACTS.md FACT-004 licenses playing into this machine's own endpoint in task 5 while forbidding any inference about the IC-7300, and keeps the level half deferred and unclaimed.
ACCOMPLISHED: When Tim presses stop, everything stops. Tonight the button takes the carrier off the antenna and Hamlet goes on playing the rest of a twelve-second transmission into the radio - which is harmless if the radio heard the unkey and is the whole transmission going out anyway if it did not, and the radio not hearing the unkey is precisely what the stop is for. After this unit the sound stops too, inside a measured number of milliseconds, on the calling thread, without waiting for the transmission it is stopping, and the abort still fires from every state including when the cancel itself fails. And Hamlet tells him which of the two it managed rather than saying "stopped" when only half of it did.
ADVANCES: none - this unit clears a blocker. It clears the last open finding standing between the tree and step 6, "Tim works a station": unit 261's recorded and deliberately unrepaired result that the operator's abort unkeys the radio but does not stop the audio, leaving a licensed operator with a stop button that stops half of what is on the air.
END-ARBITER-DECISION
```
