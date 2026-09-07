# Work instruction 265 - the level Tim is asked to set, and the control to set it with

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

**All four were checked against the tree at `HEAD 2a47955` while this instruction
was written.** `SHACK_FACTS.md` and
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` are both present; `ls` on
`CoreHMI.sln` and on `MURC.sln` both returned *No such file or directory*; the
only solution at the root is `Hamlet.sln`. **Check them anyway.**

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
258 through 264 wrote `PROJECT_STATUS.md` repeatedly and lost nothing. **The
status write is part of the work, not part of the reporting.**

`dotnet build` is allowed, foregrounded, with a timeout.

**This unit opens no serial port and keys nothing.** There is one task that plays
sound into this machine's own render endpoint - task 3's decode margin - and it
is bounded to that. Everything else is proved against the `FakePort` and the
transmit fakes units 253 to 264 left in the tree.

---

## THE TOOL RULE

`tools\arbiter\outcome-append.bat` has been refused for **twelve consecutive
units**, and `tools\arbiter\validate-output.bat` in twenty-one forms across ten.
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
an apostrophe in the approach text is a parse error. Recorded at units 262, 263
and 264, **not this unit's to repair.** *(The arbiter's own loop test ran clean
tonight with an apostrophe-free approach line.)*

---

## Why this unit exists

**This is work instruction 265. Twelve units have been spent on this phase, 253
through 264**, and `PHASE_OUTCOME.md` carries twenty-four entries because every
unit since 253 has been recorded twice.

```
PHASE GOAL: Hamlet works stations on the air.
UNIT GOAL:  Hamlet stops transmitting at full scale. The operator gets a
            transmit drive level with a conservative default, applied once
            where the samples are built, and he can see the level that
            actually went out.
ADVANCES:   Step 3, criterion 1 - "Audio plays to the radio's USB input at the
            right device, rate and level." The device half was met by unit 256
            and the rate half by unit 262. This unit takes the level half as
            far as this machine can take it and names the residue. It also
            advances step 2's criterion 4, "level and clipping stated", which
            is the same figure read from the other end.
```

**Why this and not something else, and why it reverses last night's park.**

Unit 264's instruction parked the level with these words: *"Step 2's criterion 4
and step 3's criterion 1's level half are Tim's to read off the radio under
FACT-004. Do not measure it, do not infer it, do not build a control for it
tonight."* **That park was right about the figure and wrong about the control,
and this instruction reverses it deliberately.**

FACT-004 defers **what the IC-7300's USB modulation input expects**. It says
nothing about **whether Hamlet has any way to set a level at all**. Four
consecutive units - 254, 256, 262 and 263 - have written *the level is deferred
to Tim and what he must do is named*. **Nobody checked whether he has anything to
do it with.** Measured cold at `HEAD 2a47955` while this instruction was written:

| Measured | Where | What it says |
|---|---|---|
| `signal[k] = MathF.Sin(phase)` | `src/Ft8Sharp/Encode/Ft8Waveform.cs:199` | the waveform is built at **peak 1.0, full scale** |
| `ComposeSignal(string?, int, float)` | `src/Hamlet.RadioEngine/Transmit/Ft8Composer.cs:259` | **there is no amplitude parameter** on any compose route |
| `ComposeSignal(wanted, _transmitSampleRate)` | `src/Hamlet.App/ViewModels/MainWindowViewModel.cs:8184` | the application's only send composes with the two arguments there are |
| `Clamp(...)`, `PeakWritten`, `ClippedSamples` | `src/Hamlet.RadioEngine/Audio/WasapiTransmitSink.cs:407-408` | the sink **measures** the peak and clamps; it applies **no gain** |
| grep for `PeakWritten`, `ClippedSamples` outside their own file | `src/`, `tests/` | **every reader is a test.** Nothing in `src/` reads either. The operator is never told the level he transmitted at |

**So: Hamlet transmits at 0 dBFS, there is no control anywhere, and the operator
is shown no number afterwards.** Step 3's criterion 1 asks for the right *level*
and step 2's criterion 4 asks for *level and clipping stated*. Both have been
deferred to Tim four times. **A deferral to an operator who has no control and no
readout is not a deferral, it is a criterion that cannot be closed by anyone.**
This unit gives it a control and a number, so that the residue - the figure his
radio wants - becomes something he can close in one evening at the rig.

**And the base rate says look.** Unit 260 reported that what stood between step 6
and being attempted was no longer code. Unit 261 then found there was no stop
button. Unit 262 then found the application could transmit through none of this
machine's four render endpoints. Unit 263 then found the stop left 8,345 ms of
audio going out after the operator pressed it. Unit 264 walked the whole contact
and found it green - **one clean night, which is the first, and it is not four.**

**What this is not.** It is not a claim that overdriving a transceiver is unsafe
in some way that needs a ruling - it does not, it is ordinary operating practice
and the plan already rules that Tim transmits on a live antenna. It is the plain
observation that **a first FT8 transmission at full scale is a wide, distorted
signal over other people's band, and Hamlet currently offers no way to make it
anything else.**

---

## Verify this instruction against the tree

**Every line number, quotation and claim above and below was read from
`HEAD 2a47955`. Check them.** Where the tree disagrees with this instruction,
**the tree wins**: report the mismatch in section 4 and continue with what the
tree says. **Do not repair the instruction and do not repair `PHASE_OUTCOME.md`.**

**Do not edit any existing `PHASE_OUTCOME.md` entry.** Unit 258's ruling stands
and units 261, 263 and 264 all honoured it: a unit rewriting an entry that is not
its own is worse than a record that disagrees with itself in public. Append yours
and put any correction there and in section 4.

**Failures you should expect and must not chase:**

- `CwAdjudicationTests.ASpeedChangeInRealisticAudio`.
- The 51 CW cases in `docs/unit239-failing-set.txt`.
- The `Ft8Sharp.Deep.Tests` whole-type-list tripwire.

These are inherited reds named in `PHASE_PLAN.md` and are **never chased**.

**Four things this instruction expects to be told it got wrong. Say so plainly if
so; none is a halt, and each is a real answer:**

1. **That scaling the composed samples down still decodes.** It is the whole
   premise and it is measured in task 1 and again in task 3, not assumed. **If a
   conservative default costs decode margin the loopback cannot afford, that is
   the finding** - report the figures, take the highest level that holds with
   margin, and say what the margin was. Do not quietly keep full scale and do not
   quietly ship a level that does not decode.
2. **That `AppSettings.cs:210-213` is stale.** Its remark on `AudioOutputDeviceId`
   says *"A SETTINGS SCREEN FOR IT IS NOT BUILT (work instruction 259, task 4)"*,
   but unit 260's outcome entry says it shipped the picker. One of them is wrong.
   Say which, from the tree. Do not rewrite the remark unless task 5 puts the
   drive control beside that picker, in which case the remark is in your way and
   correcting it is part of your change.
3. **That `Ft8Composer` is the right place for the scale.** This instruction says
   it is, and says why below. If the tree shows a reason it is not - a caller that
   would be missed, a route that bypasses it - **report it and put the scale where
   the tree says**, naming every `ComposeSignal` and `Compose` caller you found.
4. **That nothing in `src/` reads `PeakWritten`.** Measured by grep tonight. If
   task 1 finds a reader, name it, and task 4 becomes smaller rather than
   different.

**Reported from the reload, for you to judge and not for me to repair**
(`ARBITER.md` §5):

- `PROJECT_STATUS.md` carries `RULES_AT: HM-DEC-157 (2026-09-06)` while
  `CLAUDE.md` §1's highest is `CPS-DEC-0152`. **Reported, not chased.**
- `PHASE_OUTCOME.md`'s header says step 1 is `done` while the last entry for it
  says `partial`. **The entries win, and this is expected**: unit 264 set the
  header to `done` on work instruction 264's authority, and the entry beneath it
  was written before that. **Do not reconcile it and do not edit the entry.**
- Five files are modified or deleted and uncommitted at the root -
  `PHASE_OUTCOME.md`, `PHASE_STATUS.md`, `RUN_LEDGER.md`, `SESSION.lock` deleted,
  and the whole of `.run-unit/` - with `.run-unit/watched.rc` untracked. **They
  are the previous unit's and the launcher's bookkeeping. Commit what is the
  phase's record before you start task 2** so that a watchdog kill costs a diff
  and not a loss, and say in section 4 what you committed and what you left.

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
it.** `Ft8Sharp.Deep` is GPL-3.0. **This ruling decides where tonight's scale
goes**: `Ft8Waveform.cs:199` builds at full scale and **you may not touch it**.
The scale is applied in `Hamlet.RadioEngine` after the port returns.

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
transmit - **never message content and never a callsign.** If task 4 puts a level
anywhere near telemetry, this rule governs it: **a number is not a callsign, and
you add no string.**

### One ruling made tonight by the arbiter, and it is not yours to re-argue

**The level split.** `SHACK_FACTS.md` FACT-004 defers **the figure the IC-7300's
USB modulation input expects**. It does not defer **whether Hamlet has a transmit
level control**. Those are two different things and four units have run them
together. **Tonight the control, the default and the readout are built here and
measured on this machine; the radio's own figure stays Tim's and is not claimed,
not inferred, and not written down as though it were known.**

**What you do about it:** build the control, and in every place you state a level,
say which machine the number came from and say plainly that the figure the radio
wants is not in this repository. **Do not close step 3 criterion 1 and do not
close step 2 criterion 4.** They stay `partial` with the residue named.

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

Write `docs/unit265-the-level-trace.md`. **Every answer carries a file, a line
and a quotation.** No product code in this task, no new test in this task.

1. **What peak leaves the composer today?** Follow
   `Ft8Composer.ComposeSignal` at `:259` into the port and back. Quote
   `Ft8Waveform.cs:199`. Then say what `Ft8Transmission.PeakSample` reports for a
   composed message - the property is measured off the array every time it is
   asked for, so you can state it from the existing
   `TheSeamTurnsWordsIntoASlotOfAudioTests.cs:100` output rather than writing
   anything. **Give the number, not the adjective.**
2. **Is there any gain anywhere between the composer and the endpoint?** Walk
   `Ft8Composer` -> `OperatorSend` -> `Ft8ArmedSend` -> `Ft8TransmitSequence:287`
   -> `ITransmitAudioSink.PlayAsync` -> `WasapiTransmitSink`. **Name every place a
   sample is multiplied by anything.** Then answer in one sentence: **by any
   route, including editing the settings file by hand, can the operator change
   the level Hamlet transmits at today?**
3. **What does the sink measure and who reads it?** Quote
   `WasapiTransmitSink.cs:407-408`. Then grep `src/` for `PeakWritten` and
   `ClippedSamples` and say how many readers there are outside tests. **This
   instruction measured zero. Confirm or correct it.**
4. **Does `PeakWritten` describe what leaves the machine?** From the code alone,
   say whether the Windows endpoint's own session volume is applied before or
   after the samples this property measures - and **if it cannot be told from
   here, say that plainly** rather than guessing. It changes what task 4's number
   means and the report must say which it is.
5. **What would a scale break?** Name every existing test that asserts a peak, a
   decode, an SNR or a sample value on the transmit path - start with
   `TheSeamTurnsWordsIntoASlotOfAudioTests`,
   `WhatTheTransmissionLooksLikeAsAudioTests`,
   `TheSinkPlaysToANamedEndpointTests` (`:207` asserts `ClippedSamples == 0`),
   `TheLoopbackProvesTheWholeChainTests` and
   `TheLoopbackThroughTheApplicationsSendPathTests`. **This list is task 3's
   re-run list. A test you do not name here is one you will not run.**
6. **Where must the scale go so no send path misses it?** List **every** caller of
   `Ft8Composer.ComposeSignal` and `Ft8Composer.Compose` in `src/` and in
   `tests/`. Confirm that the application's right-click send and its CQ button
   both arrive at `MainWindowViewModel.cs:8184` and that there is no second
   compose site. **If there is a second one, that is the finding and this
   instruction was wrong.**
7. **What decode margin is there to spend?** From the existing loopback test
   output, state the reported SNR or decode result at full scale on **this
   machine's** endpoint. This is the budget task 2's default is chosen out of.

**Commit the trace as its own commit before task 2 starts.** Units 263 and 264
both did this and it is why their findings survived.

### Task 2 - the drive, watched failing first

**The breakage it would have caught**, which `PHASE_PLAN.md` requires you to
name: **Tim's first transmission goes into the radio's USB input at 0 dBFS, a
heavily overdriven FT8 signal goes out over other people's band, and there is no
control in Hamlet to turn it down.**

1. **Add the setting.** A transmit drive level on `AppSettings`, expressed as a
   peak amplitude with a documented dBFS equivalent, defaulting to a value
   **chosen from task 1 question 7's margin and at least 6 dB below full scale**.
   Write the choice at the site **with its arithmetic**, in the register the
   surrounding remarks use, and **state in that remark that it is a starting
   point the operator adjusts against his own radio's ALC, not a figure this
   repository knows.** This is the same treatment unit 255 gave the 0.5 s slot
   offset: *a choice recorded with its arithmetic, not quoted as a
   specification.*
2. **Apply it in one place**, in `Ft8Composer`, as a new optional parameter on
   the compose route, defaulting to the level above so that no caller can get
   full scale by forgetting an argument. **Why there and not in the sink:**
   `Ft8Transmission.PeakSample` is measured off the array, so a transmission
   scaled at compose time *carries* the peak it will actually play at, and a
   gain applied later would mean the object handed to `Ft8TransmitSequence`
   claims a peak it does not have - the same class of lie unit 256 caught in
   `SamplesPlayed`. **Do not touch `Ft8Sharp`.**
3. **Watch it red first.** Write the assertion that the composed peak equals the
   configured level, run it against the tree as it stands, and **quote the red** -
   it should report a peak of about 1.0. Then make it green and quote that
   beside it. **A test that was never red proves nothing about where the scale
   went.**
4. **Assert these, each by name:**
   - the default is at least 6 dB below full scale, asserted as a number and not
     as a comment;
   - `PeakSample` on a composed transmission equals the asked-for level within a
     stated tolerance, at more than one sample rate;
   - a level of zero or below, or above 1.0, is **refused with words** through
     the existing `Ft8ComposeResult` refusal shape - add a refusal value beside
     the four at `Ft8Composer.cs:14-34` rather than throwing, because every other
     refusal on this path comes back as a sentence the operator reads;
   - **the message still reads back as itself** through the existing round trip
     at the new default. Reuse the round trip; do not write a second decoder.
5. **Wire the application to it** at `MainWindowViewModel.cs:8184`, reading the
   setting the same way `_transmitSampleRate` is read - **one line, one place**,
   and assert that the CQ button and the right-click send both get the same
   level.

### Task 3 - the neighbours, and the decode margin on a real endpoint

1. **Re-run, filtered by exact name, every test named in task 1 question 5**, and
   quote the counts. **A neighbour you did not run is a neighbour you did not
   check** - say which you ran and which you did not, and why.
2. Where an existing test asserted a full-scale figure that is now correctly
   different, **change the expectation and write the reason at the site.** Where
   one fails for a reason that is *not* the scale, **stop, report it in section 4,
   and do not chase it.**
3. **The one sound this unit makes.** Run the existing application-side loopback
   once at the new default into this machine's own render endpoint, and quote:
   the level asked, `PeakWritten`, `ClippedSamples`, and whether the message
   decoded back as itself. **Say which machine every figure came from.**
   FACT-004: this says nothing whatever about the IC-7300 and you must write that
   sentence beside the numbers. If the shell refuses to enumerate endpoints, use
   unit 262's recorded fallback and say you did.

### Task 4 - the operator can see what went out

**The breakage it would have caught:** Tim turns the drive down, transmits, and
has no way to tell what level actually reached the card - so he is setting his
radio's ALC against a number Hamlet knows and never shows him, and the criterion
he is asked to close stays unclosable.

After a send completes, the Send area line beneath the waterfall says **the level
it went out at, in dBFS, and the clip count**. Prefer the sink's own
`PeakWritten` and `ClippedSamples` if task 1 question 3 found a route to them
from the result the sequence already returns; if there is no such route,
**use the transmission's own `PeakSample` and say in the line's own remark that
it is the level composed rather than the level written, and what the difference
is.** Do not invent a plumbing route through the sequence for it if that means
touching the keying path - see *what not to do* item 6.

Test it: one assertion on the string an operator would read, driven through the
application's own send path on the fake port and the substituted sink factory.
**Open no device in this test.**

### Task 5 - the Settings control - **THE NAMED DROP CANDIDATE**

**This is the task to drop if the night runs short**, and it is named as the drop
candidate deliberately. **The conservative default and the applied scale are what
protect the band, and they land in task 2**; a control in the Settings window is
the convenience on top. The setting is honoured from the settings file with or
without it, so dropping this costs Tim a JSON edit and costs the band nothing.
**If you drop it, say so in section 3, say what he must edit instead and name the
file, and leave it named for the next unit.**

Put the drive beside the transmit endpoint picker unit 260 shipped. It shows the
current value in dBFS, it will not accept a value the composer would refuse, and
**it says on the screen that the number is a starting point to be set against the
radio's ALC.** If putting it there makes `AppSettings.cs:210-213`'s remark wrong,
correct that remark as part of this change and say so.

---

## Parked - do not touch, do not raise

- **The figure the IC-7300's USB modulation input expects.** FACT-004. It is not
  in this repository, it may not be inferred here, and tonight's work does not
  claim it. **Build the control; do not guess the number.**
- **The transmit audio base frequency.** `ComposeSignal`'s third parameter is
  `DefaultBaseFrequencyHz` and the operator never chooses it, so every Hamlet
  transmission goes out at the same audio offset. **The arbiter measured this
  tonight and logs it. It is legal, it is decodable, it is not a criterion, and
  it is not tonight's. Do not build a control for it.**
- **Step 4's criterion 6** - the synthesized corpus rather than a WSJT-X capture.
  Recorded, parked, not reopened.
- **Step 1's fifth criterion and its `blocked`/`partial`/`done` flapping.**
  Settled by work instruction 264 and set to `done`. **Do not revisit it, do not
  re-argue it, and do not touch either header line for step 1.**
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

1. **Do not change a line of `Ft8Sharp` or `Ft8Sharp.Deep`.** Ruled, and it is
   the ruling that decides tonight's design: `Ft8Waveform.cs:199` builds at full
   scale and stays that way. **The scale is applied in `Hamlet.RadioEngine`.**
2. **Do not open a serial port and do not key anything.** The only sound this
   unit makes is task 3 item 3, into this machine's own endpoint.
3. **Do not infer the radio's expected level from anything measured here.**
   FACT-004. Every level you state carries the machine it came from and the
   sentence that it says nothing about the IC-7300.
4. **Do not repair what you find; report it.** `ARBITER.md` §5. The licensed
   changes tonight are the five tasks and the expectation edits task 3 item 2
   names.
5. **Do not edit `PHASE_OUTCOME.md`'s existing entries**, and do not touch any
   step's word in either header line. **No step closes tonight.** Append your own
   entry.
6. **Do not touch the keying path to carry a number.** `Ft8TransmitSequence`'s
   key at `:283`, the sink call at `:287`, the `finally`, the abort and the stop
   are units 255, 261 and 263's and they are proved. If task 4's readout appears
   to want a change in there, **take the composed peak instead and say why in the
   report.**
7. **Do not touch `TransmitGuard.Check` or any existing caller**, do not write a
   second copy of the licence rule, and **do not make the drive level a condition
   on transmitting.** It is a level, not a gate. A drive of zero is refused at
   compose time with words; nothing else about the send path learns a new reason
   to refuse.
8. **Do not make the abort or the stop conditional on anything you add.**
9. **Do not add a second `ComposeSignal` call site** and do not book a send
   anywhere but `MainWindowViewModel.cs:8279`, which is `RecordSent`'s one caller.
10. **Do not run an unfiltered `dotnet test`**, do not background a command and
    poll for it, and do not add a test without naming the breakage it would have
    caught.
11. **Do not close step 6, step 3 or step 2.** Tonight advances a criterion; it
    closes none. Say the difference plainly.

---

## Committing and pushing

**Commit at the end of each task**, in `CLAUDE_CODE.md`'s message form, with the
task's own evidence in the body. Push when the last task lands. **A unit killed
by the watchdog with work uncommitted loses it** - unit 257 lost three files that
way. Commit the trace as its own commit before task 2 starts, and **commit task
2's assertion at the red before you make it green**, saying the colour in the
message.

---

## Reporting

**`output.md`. The ordering block comes first, before the header.**
`validate-output.bat` refuses a report without it, so a report that omits it is
rejected whatever else it contains.

```
READ IN THIS ORDER

A. THE PHASE GOAL IS "Hamlet works stations on the air", and every step's
   state: step 0 done; step 1 done; steps 2 to 5 partial; step 6 not started.
   Say whether anything this unit measured changed any of those - and say
   plainly that NO STEP CLOSED TONIGHT and that none was meant to. Then state
   the fact that shaped this unit: what has been recorded open in steps 2 and 3
   for four units is THE LEVEL, deferred to Tim under FACT-004 - and say
   whether, before tonight, Hamlet had any control or readout with which he
   could have closed it.

B. THIS UNIT AIMS AT STEP 3, CRITERION 1 - "audio plays to the radio's USB
   input at the right device, rate and level." Say which of its three halves
   were already met and by which unit: the device by 256, the rate by 262.
   Then answer the question this unit exists for:

   AT WHAT PEAK DID HAMLET TRANSMIT BEFORE THIS UNIT, AT WHAT PEAK DOES IT
   TRANSMIT AFTER IT, AND CAN THE OPERATOR NOW CHANGE IT AND SEE IT?

   Give both numbers in dBFS, name the machine they were measured on, and say
   in one sentence what is still unmet in criterion 1 and whose it is. Say
   whether task 5 was dropped and, if it was, what Tim must edit instead.

C. THIS REPORT'S OWN FINDINGS, weighed against A and B. Name how many items
   section 4 raises and, for each, say whether it stands in the way of anything
   named in B. Four are anticipated and none is blocking if it lands as this
   instruction predicts: whether the scaled signal still decodes and with what
   margin; whether AppSettings.cs:210-213's remark is stale; whether
   Ft8Composer was the right place for the scale; and whether anything in src/
   reads PeakWritten. If section 4 raises nothing, say so in a sentence -
   CLAUDE_CODE.md section 8 makes that a real answer.
```

Then the six-line header:

```
UNIT: 265 - <state> at task n of 5 - <timestamp>
PHASE GOAL: Hamlet works stations on the air
UNIT GOAL:  Hamlet stops transmitting at full scale and the operator can set
            and see the level.
ADVANCED:   <step 3 criterion 1's level half, with the figures - or none, and
            why. Do not write "closed"; it is not closed.>
NUMBER:     <the peak before, in dBFS, and the peak after, on this machine>
DRIFT:      <five consecutive units - 260 to 264 - recorded no criterion
            advance. Say whether this unit ends that count or continues it,
            honestly, and say which criterion if it ends it.>
```

**Section 3 must lead with task 2's red**, quoted verbatim - the assertion that
the composed peak equals the configured level, run against the untouched tree,
showing what Hamlet actually transmits at today. That one block is the state of
the phase's oldest deferral in a single number, and everything after it is the
repair. **If that assertion is green against the untouched tree, this instruction
was wrong about the tree** - lead with that instead, quote it, and say so
plainly.

**Section 4 is for what stands in the way.** A note recorded for the record is
not a ruling request; say which yours are.

---

```
ARBITER-DECISION
STEP: 3
APPROACH: add an operator transmit drive level applied to the composed samples before the sink, so the deferred level criterion has a control to set
MOVE: work around
WHY: Step 3 criterion 1 names the right device, the right rate and the right level; the device half was met by unit 256 and the rate half by unit 262, and the level half has been recorded "deferred to Tim under FACT-004" by four consecutive units without anyone checking whether he has a control to set it with. I measured at HEAD 2a47955 that he does not: Ft8Waveform.cs:199 builds at peak 1.0, ComposeSignal at Ft8Composer.cs:259 takes no amplitude, the application composes with the two arguments there are at MainWindowViewModel.cs:8184, WasapiTransmitSink measures the peak at :407-408 and applies no gain, and every reader of PeakWritten and ClippedSamples in the repository is a test - so Hamlet transmits at 0 dBFS, nothing can change it, and the operator is never shown the number afterwards. FACT-004 defers the figure the IC-7300 expects; it does not defer whether the control exists. This is the approach that turns a permanently unclosable criterion into one Tim can close in one evening at the rig.
STATE: partial
DECIDED: Three on my own authority. First, that work instruction 264's park of the level is reversed - it read "do not build a control for it tonight", and the distinction it missed is that FACT-004 defers a figure and not a feature; the figure stays deferred and unclaimed tonight and only the control, the default and the readout are built, all measured on this machine with the residue named. Second, that the scale is applied in Ft8Composer rather than in the sink, because Ft8Transmission.PeakSample is measured off the array every time it is asked for, so a transmission scaled at compose time carries the peak it will actually play at, whereas a gain applied downstream would hand Ft8TransmitSequence an object claiming a peak it does not have - the same class of lie unit 256 caught in SamplesPlayed - and because Ft8Sharp may not be touched, which puts Ft8Waveform.cs:199 out of reach anyway. Third, that the default level is not specified by me but chosen by the unit out of the decode margin it measures in task 1, bounded only at "at least 6 dB below full scale", and recorded at the site with its arithmetic as a starting point rather than as a specification - the treatment unit 255 gave the 0.5 s slot offset - because a number invented in an instruction and then quoted back by a report is exactly the plausible-rather-than-true answer, and this one would be quoted at a radio.
LICENCE: PHASE_PLAN.md step 3 criterion 1 in its own words - audio plays to the radio's USB input at the right device, rate and level - and step 2 criterion 4, level and clipping stated, which is the same figure from the other end. "The steps are a hypothesis, not a contract" licenses taking unattempted ground inside a partial step and moving a target found to have been measured wrong, recording the evidence. The named alternatives to stopping license the rest: the tree wins, report the mismatch and continue, and a target not reached is closed with the figure reached and what was tried. SHACK_FACTS.md FACT-004 licenses measuring this machine's own endpoint while forbidding any inference about the IC-7300, and it is also what keeps the radio-side figure deferred and unclaimed tonight. The ruling that Ft8Sharp is a faithful port and nothing in this phase changes a line of it decides where the scale goes.
ACCOMPLISHED: Tonight Hamlet transmits at full scale and there is no way to change it. Tim's first transmission on a live antenna would have been a wide, overdriven FT8 signal over other people's band, and the only thing he could have reached for was the Windows volume slider. After this unit Hamlet sends at a level well below full scale by default, he can set that level and see in dBFS what actually went out, and the one number nobody in this repository can know - what his own radio's USB input wants - becomes something he closes in one evening with the control in front of him instead of something four unit reports have promised to defer to him.
ADVANCES: Step 3, criterion 1 - "audio plays to the radio's USB input at the right device, rate and level." The device half is met (unit 256) and the rate half is met (unit 262); this unit moves the level half from unreachable to reachable and does not close the criterion, because the radio-side figure is Tim's under FACT-004. It advances step 2's criterion 4, "level and clipping stated", by the same measurement. And it clears the blocker under step 6 that four units created between them: a criterion deferred to an operator who had no control and no readout could not have been closed by him either.
END-ARBITER-DECISION
```
