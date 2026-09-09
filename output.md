READ IN THIS ORDER: A the phase goal, B this step and its exit criteria, C what this
report adds and whether any of it bears on A or B.

A. **The phase goal is that FT4 works exactly the way FT8 does**, and where every
   step stands after tonight: step 0 done; step 1 partial, its remainder the 4.48
   against 5.04 question with Tim; step 2 partial, its remainder discharged by unit
   292 as far as its threading reached; step 3 done; **step 4 done**; steps 5 and 6
   not started and Tim's at his own radio. **Every bench step in the phase is now
   answered.** Nothing after step 4 is a unit's to take.

B. **Step 4 and its four exit criteria:**
   1. pressing FT4 tunes to the band's FT4 frequency and decodes, through the same
      path the FT8 button uses                                   **met** - by 292
   2. the panel, conversation, ring, filters, tooltips, ledger and right-click menu
      all work unchanged, and the report names anything that did not
                                                                 **met** - by 292
   3. one click, one transmission, through the same abort        **met** - this unit
   4. a whole exchange runs from one right click at the bench, with the transmit
      endpoint on a loopback                                     **met** - this unit
   **For criterion 4 the evidence is a real render endpoint.** This machine has four
   active ones; a display-audio endpoint was chosen deliberately, a real
   `WasapiTransmitSink` was opened on it, 241 920 samples of FT4 tones were rendered
   at 48 kHz, and a real WASAPI loopback capture read them back at -12.0 dBFS. **It
   is not a fake sink.** A `FakeSink` run sits beside it and is reported as the fake
   it is.

C. **What this report adds, weighed against A and B.** Section 4 raises 8 items:
   **one** is beside a criterion in B and none is in the way of one, and the
   other seven are standing questions and bookkeeping. Task 1 found **20 places
   between the right click and a keyed transmission that were FT8 by construction**;
   **17 now follow the mode** and **3 are still standing**, of which two stand by
   design and are named. **Nothing found tonight makes step 5 or step 6 harder than
   the plan assumes**, and one thing found tonight would have made step 6 impossible
   and is fixed: the tick would not have recognised four of FT4's eight boundaries a
   minute. **Task 8 was dropped whole**, as the named drop candidate; no criterion in
   B depended on it, and both ledger gaps - `Ft8ContactLedger.SlotsAgo` and
   `Ft8ContactStates.GoneQuietAfterSeconds` - are named and left standing.

```
UNIT:       293 - complete at task 7 of 8, task 8 dropped whole - 2026-09-09 12:04
PHASE GOAL: FT4 works exactly the way FT8 does.
UNIT GOAL:  One right click on an FT4 row composes an FT4 transmission, arms it for
            the next 7.5 s boundary, keys, plays and unkeys through the same abort
            the FT8 path uses - and the contact it produces is logged as the mode it
            was made in.
ADVANCED:   yes - step 4's criteria 3 and 4, which were its last two unmet criteria.
            Step 4 goes partial to done, and every bench step in the phase is
            answered.
NUMBER:     20 places between the right click and a keyed transmission were FT8 by
            construction; 17 now follow the mode and 3 still stand. The right-click
            menu offers 3 message shapes on an FT4 row against 5 on an FT8 row -
            both numbers stated, and they are not equal.
DRIFT:      0 consecutive units without advance  (was 0)
```

## 1. What Claude did

**Exit state: complete at task 7 of 8.** Task 8 was dropped whole; it is the drop
candidate work instruction 293 named, so this is not a sizing decision made for
myself. Why it was dropped: the unit had run about forty minutes by the time task 7
closed, the instruction's own words are *if this unit is running long, drop this task
whole and say it was dropped*, and it adds *criteria 3 and 4 closed with the log
writing the right mode is a good night*. Both halves of task 8 are named in section 4
and neither is in the way of anything. **What the next unit inherits is those two
ledger gaps and nothing else from this unit.**

Provenance: Windows 11 on `C:\Source\HamLet`, branch `main`, project gate `PROJECT:
Hamlet` verified against the tree - `SHACK_FACTS.md` present,
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` present, `CoreHMI.sln` absent,
`MURC.sln` absent. Hamlet confirmed. Root version read from
`Directory.Build.props:488` as `1.12.234` and bumped to `1.12.239`; `Ft8Sharp` read
from `src/Ft8Sharp/Directory.Build.props:438` as `0.11.0` and **not moved** - nothing
in either port assembly was changed.

### What was built

**Task 1 - the trace.** Measured, not described. See section 3.

**Task 2 - `Ft4Composer`.** Built beside `Ft8Composer`, not inside it. Both are now
faces onto a shared `DigitalComposer` holding the packing, the three-pass hash
ordering, the round trip through `Ft8MessageDecoder`, the five refusals and the
drive; what each face supplies is a `ComposeGeometry` - four constants and three port
calls, read off `Ft8Sharp` and typed nowhere in this assembly. 7 tests, all green.

**Task 3 - the fit guard and the arm.** `OperatorSend` gained `Grid`; `Sendable`
measures against it in all four places and both refusal sentences read their numbers
and the mode's name off it; `SendMessage` composes through the mode's own composer and
arms on `DigitalGrid`; `DriveTheArmedSend` computes the boundary on the armed send's
own grid. 9 tests, all green.

**Task 4 - the log.** `ContactModes.Named("FT8")` became `_digitalMode.Contact()`.
4 tests, all green.

**Task 5 - criterion 3.** 7 tests, all green.

**Task 6 - criterion 4.** 3 tests, all green, one of them on a real sound card.

**Task 7 - bookkeeping.** Below.

**Test discipline.** **No test suite was run.** Every run was filtered by exact
class name, foregrounded, with a `timeout 600` on the shell call - 600 000 ms, the
tool's maximum. **Every `dotnet build` was foregrounded with the same timeout** - I did
not keep an exact count of them and am not going to invent one. **Nothing was
backgrounded and nothing was polled for.**

**Counts.** 30 tests constructed or rewritten by this unit, all green. 195 control
tests run by exact name across both projects, all green: 53 FT8 composer, 73 engine
transmit and abort, 41 log and achievements, 37 ADIF, plus unit 290's grid pin and
unit 292's `AnFt8PressLeavesEveryBoundaryAndSentenceWhereItWas`. **No inherited red
was chased and none was re-measured.**

### Decisions made for myself, reproduced in full

**One. The FT4 composer is `Ft8Composer`'s sibling and not its branch.** The two
honest alternatives were one implementation taking the modulation as a value, or two
copies of a file. **The copy is what the port's whole argument is against** - a second
packer beside the first is a second thing to drift, and the drift would be a message
that says one thing on FT8 and another on FT4. So the shared half moved down into
`DigitalComposer` and `Ft8Composer` kept every public member it had, delegating.
`Ft8Composer`'s own remark already said its two routes *differ in one call and in
nothing else*; FT4's difference is the same size, and `ComposeGeometry` is what names
it.

**Two. The grid rides on `OperatorSend` and not on `Ft8TransmitSequence`.** The
sequence is built once, when the radio connects, and outlives any number of presses of
the mode strip; a grid held there would be a second place the operator's choice lives,
able to disagree with the audio in the very record it was being asked about. **The
grid is part of what he asked for** - this message, at this frequency, in this slot, on
this grid - and the whole reason that record exists is that his intent travels as one
value. It is an `init` property defaulting to `SlotGrid.Ft8`, which is the status quo
written down rather than a guess: every caller that existed before tonight got fifteen
seconds and still gets them, the same rule `DigitalModes.Grid()` already states.

**Three. I fixed one screen this unit's own change would otherwise have broken.**
`SendingLine` formatted `HH:mm:ss`, which is every FT8 boundary exactly and **four of
FT4's eight wrong by half a second** - `:07.5` read as `07`, `:22.5` as `22`. It only
became reachable tonight, when the arm started following FT4's grid. A sentence naming
a moment the transmission does not start at is §0.0 in the one line telling him what is
about to go out on the band. **It is asked of the boundary rather than of the mode**,
so FT8's sentence is unchanged character for character.

**What I did not settle.** Not the 4.48-against-5.04 figure - both refusal sentences
read the occupancy off the grid and no transmission length was typed into a new place
anywhere. Not the version scheme. Not unit 289's widened candidate sweep - it was
called, not retuned. Not the four inherited reds. **And no FT4 signal report was
invented, and no constant was substituted for one.**

### One mistake, corrected

Task 5's commit staged `tests/` wholesale and swept in
`tests/Ft8Sharp.Tests/Unit289SourceProbe.cs`, which the instruction says plainly is
not to be committed. **It was untracked in commit `ffb09cb` immediately afterwards**,
with the reason on the commit's face. It remains on disk as an untracked leftover,
because this shell refuses every form of deletion available here - see section 4.

Also: **task 2's commit carried no version bump.** The patch was bumped for tasks 1,
3, 4, 5 and 6, so the root went `1.12.234` to `1.12.239` across six commits rather
than seven. Nothing depends on it and it is recorded rather than back-filled.

### Task 7, in full

- **`PHASE_OUTCOME.md`.** Unit 293's entry appended **with the file-editing tools, in
  the format `outcome-entry.py` produces, and saying so on its own face** in an
  `APPENDED BY HAND:` line. **No `.bat` under `tools/arbiter/` was attempted at all** -
  four consecutive units measured that refusal and the instruction says a fifth
  measurement is worth nothing. The arguments the script would have been given are
  committed at `tools/arbiter/unit293-append.bat`. **Units 289's to 292's entries were
  not touched**, including the two `STATE_AFTER` verdicts each carries.
- **`PHASE_STATUS.md`.** `WORK_INSTRUCTION:` set to `293 - one click, one FT4
  transmission, through the same abort` and nothing else. `HEARTBEAT:`,
  `CURRENT_STEP:` and the `STEP:` lines were not written. `CURRENT_STEP:` still reads
  `1` while the phase is on step 4; **reported again, not repaired.**
- **`RULES_AT`.** Re-checked and there is nothing to repair: `CPS-DEC-` appears zero
  times in `CLAUDE.md`, `DECISIONS.md` and `PROJECT_STATUS.md`; `HM-DEC-160` appears
  once in each; the three agree. `.run-unit/reload.txt:9` still says `CLAUDE.md
  section 1 holds CPS-DEC-0160`. **That is the launcher's file and not this unit's.**
- **The validator.** `dotnet build tools/arbiter/validate-output.proj`, which runs the
  real validator unmodified. Result quoted in section 3.
- **The working tree.** `.commit-msg.txt` is tracked and carried each commit message,
  because this shell will not take a quoted heredoc with an apostrophe. The three
  untracked leftovers could not be removed - section 4.

## 2. What the owner should expect

**Hamlet can work a station on FT4.** That is the sentence this phase was built
toward and tonight is when it became true. Right-click a station that has called you
on FT4, take what the menu offers, and Hamlet composes FT4's own tones from FT4's own
encoder, waits for the next 7.5-second boundary, keys the radio, plays the
transmission, unkeys, and writes the contact into the log as `MODE=MFSK` with
`SUBMODE=FT4` - the spelling every other logger reads.

**Every bench step in this phase is answered.** Steps 0, 3 and 4 are done; steps 1 and
2 are partial with their remainders named and with Tim; **steps 5 and 6 are yours, at
your own radio, and no unit can perform them for you.** There is nothing left at the
bench standing between you and an FT4 contact.

### What to expect the first time you try it

**You have half the time.** An FT4 slot is 7.5 seconds where FT8's is 15. The
transmission itself is 5.04 seconds, so from a decode appearing on the table to the
next boundary you have **roughly two and a half seconds to read a row and click a
menu item** - and if you miss it, Hamlet does not send late. It discards the send and
says so; a message put out in a slot you did not choose is a transmission you did not
ask for, and that rule is not relaxed for FT4. Expect to miss turns at first. **That
is Hamlet refusing to guess, not Hamlet failing.**

**Nothing found tonight makes step 5 or step 6 harder than the plan assumes.** One
thing found tonight would have made step 6 impossible and is fixed: the slot tick
computed a quarter-minute boundary whatever mode was running, so **four of FT4's eight
boundaries a minute - `:07.5`, `:22.5`, `:37.5`, `:52.5` - were boundaries it could
not recognise.** A send armed for one of those would have been found "not due" at the
quarter-minute before it and "too late" at the one after: discarded every time, with
you having clicked and nothing ever going out, and no sentence on screen able to say
why. Half your clicks. It now reads the boundary off the send's own grid.

### What will look wrong but is not

- **The FT4 right-click menu is shorter than the FT8 one - three messages instead of
  five.** The `report` and `roger and report` entries are missing because **Hamlet has
  no way to measure a signal-to-noise ratio on FT4 yet**: the estimator it has is
  welded to FT8's symbol count and tone count, and there is no FT4 equivalent in this
  tree. The menu says so out loud rather than going quiet. **It has not invented a
  report and it has not substituted a constant**, which is the alternative and is the
  worse one - a number on the air that nothing measured. Building an FT4 estimator is
  a unit of its own.
- **The send line now sometimes shows a half second** - `in the slot at 15:45:22.5
  UTC`. That is correct: four of FT4's eight boundaries a minute fall on a half
  second, and the line used to round them down.
- **`PHASE_STATUS.md` says `CURRENT_STEP: 1` while the phase is on step 4.** That
  line is the launcher's; three units have now reported it and none has repaired it.
- **The ledger still counts FT4 slots as though they were FT8's.** *Heard 2 slots ago*
  on FT4 is half the true count, and a station goes quiet twice as late as it should.
  Named, standing, and section 4 has it.

## 3. What you should see

### 1. The trace: what the transmit path did with FT4 chosen, before anything changed

**In the words of what Tim's radio would have done:** with FT4 chosen and the dial on
14.080 MHz - FT4's own 20 m frequency - Hamlet **composed 12.64 seconds of FT8 tones,
armed them for a fifteen-second boundary, keyed the radio, played them, unkeyed and
told the operator it had sent.** Nothing between the click and the keying frame
noticed that the operator had asked for FT4. Measured, not deduced:

```
mode chosen      : Ft4
grid the tab runs: 7.50 s slots, 5.04 s transmission
dial             : 14080000 Hz (FT4's 20 m row)
composed samples : 151680 at 12000 Hz   =  12.64 s of tones
FT8 synthesises  : 79 symbols, 8 tones at 6.25 Hz
FT4 synthesises  : 105 symbols, 4 tones at 20.8333 Hz
armed for        : 2026-09-09T15:30:00.0000000Z   (0 s into the minute)
run outcome      : Sent, keyed True, came out of tx OrdinaryUnkey
```

**That is the number this unit exists to produce**, and no later measurement could
have recovered it. Unit 292's arbiter split step 4 on the reading that the transmit
half "has no FT4 in it at all". **That is true of the code and it is not the same as
saying nothing happens.**

**The census: 20 places that were FT8 by construction between the right click and a
keyed transmission**, each marked *constant*, *type* or *default*, in the same three
categories unit 292 used so the two censuses read together. Line numbers are as they
were at HEAD `9a81d4d`.

| # | Place | Kind | Now |
|---|---|---|---|
| 1 | `MainWindowViewModel.cs:10520` `Ft8Composer.ComposeSignal` - the only composer | type | follows the mode |
| 2 | `:10521` `Ft8Composer.DefaultBaseFrequencyHz` | constant | follows the mode |
| 3 | `Ft8Composer.cs:383` `Ft8SymbolEncoder.Encode` | type | follows the mode |
| 4 | `:385-386` `Ft8Waveform.Synthesize` / `SynthesizeSlot` | type | follows the mode |
| 5 | `:431-432` `Ft8Waveform.SampleCount` / `SymbolCount` / `SamplesPerSymbol` | constant | follows the mode |
| 6 | `:510` `Ft8Waveform.ToneCount` / `ToneSpacingHz` | constant | follows the mode |
| 7 | `MainWindowViewModel.cs:10553` `Ft8Slots.SlotStart` | type | follows the mode |
| 8 | `:10554` `.AddSeconds(Ft8Slots.SlotSeconds)` | constant | follows the mode |
| 9 | `Ft8TransmitSequence.cs:92` `OperatorSend` carries no grid | type | follows the mode |
| 10 | `:497` `send.StartSecondsIntoSlot >= Ft8Slots.SlotSeconds` | constant | follows the mode |
| 11 | `:501` `{Ft8Slots.SlotSeconds:0} s slot` in the sentence | constant | follows the mode |
| 12 | `:505` `left = Ft8Slots.SlotSeconds - ...` | constant | follows the mode |
| 13 | `:507` `Ft8Slots.TransmissionFits(left)` | type | follows the mode |
| 14 | `:512` *an FT8 transmission needs 12.64 s* | constant | follows the mode |
| 15 | `:530` the same, in the padded-slot sentence | constant | follows the mode |
| 16 | `MainWindowViewModel.cs:10611` `Ft8Slots.SlotStart` in the tick | type | follows the mode |
| 17 | `:10425` `ContactModes.Named("FT8")`, unconditional | default | follows the mode |
| 18 | `:9533` `Ft8Composer.RateIsUsable` at connect, and its sentence | type + default | **still standing** |
| 19 | `:10594` `StartSecondsIntoSlot = 0.5`, with FT8's arithmetic in its remark | constant | **standing by design** |
| 20 | `Ft8ReadBack`, `Ft8Transmission`, `Ft8ComposeResult`, `DefaultDrivePeak` - FT8-shaped names over genuinely shared code | type + constant | **standing by design** |

**17 of the 20 now follow the mode. 3 are still standing**, one of them a real item
(18, in section 4) and two of them standing on purpose: **19** needed no change because
0.5 s leaves 7.0 s of a 7.5 s slot, which holds FT4's occupancy **on either answer to
the open question**, and its remark now says so; **20** is the 77-bit message layer and
one drive level, which are shared deliberately and whose names are the only FT8 thing
about them.

**Unit 292's thirteen gaps, by number, inside this unit's boundary.** The arbiter's
reading was 1, 5, 12 and 13. **I agree with 1 and disagree about the rest, and here is
where it differs**: gap 1 (the log's unconditional FT8) is inside and is closed by task
4. Gaps 6 and 7 (the ledger) were task 8's and are dropped. **Gaps 5, 12 and 13 are
receive-side or naming items that no part of tasks 2 to 6 touches**, and I did not fix
them - the instruction is explicit that naming is what criterion 2 asked for and unit
292 already met it.

**What fires the abort, and what about it is protocol-dependent.** `TransmitAbort.Fire`
at `src/Hamlet.RadioEngine/Civ/TransmitAbort.cs:57`, fired from
`Ft8TransmitSequence.cs:359` - inside the `finally`, whenever the ordinary unkey was
not taken - and from `Ft8ArmedSend.cs:348` - the operator's stop. **The arbiter's
reading was that nothing about it is protocol-dependent, and that reading is correct.**
It is asserted rather than agreed with: the file is read and contains no `await`, no
`async`, no `Task`, no `lock`, no `WriteAsync`, and **no `Ft4`, no `Ft8` and no
`Slot`**. It is CI-V `0x17` with `0xFF` and a PTT fallback. Nothing in it changed.

**The menu on an FT4 row: 3 message shapes**, against **5 on an FT8 row**.

```
FT4 row snr      : "—"
FT4 menu offers  : 3        FT8 menu offers  : 5
    grid : W1ABC KC3QIS FN00     grid : W1ABC KC3QIS FN00
    acknowledge : ... RRR        report : W1ABC KC3QIS -14
    73 : W1ABC KC3QIS 73         roger and report : ... R-14
                                 acknowledge : ... RRR
    ABSENT: no signal report      73 : W1ABC KC3QIS 73
    has been measured for this
    station, so the messages
    that carry one are not offered
```

Cause: every FT4 row's ratio is null, because `Ft8DeepSignalToNoise.Estimate` is welded
to `Ft8SymbolEncoder.SymbolCount` and `ToneCount` and there is no FT4 equivalent in this
tree; `MeasuredReport` returns null and `Ft8SendOptions.For` returns null text for the
`Report` and `RogerAndReport` shapes. **The two missing entries are named with their
cause and nothing was built for them.** Counted as clickable items the menu shows 4 and
6, the extra being *Log this contact...* on both.

**One mismatch found against the instruction, and it is in the instruction's favour.**
The instruction says unit 292's report gives the log write as `:10299` and the tree says
`:10425` today, and asks which is right. **`:10425` is right** - `ContactModes.Named("FT8")`
was on that line at HEAD `9a81d4d`. Every other line and figure in the *Verify this
instruction against the tree* section checked out against the tree: `Directory.Build.props:488`
`1.12.234`, `src/Ft8Sharp/Directory.Build.props:438` `0.11.0`, `Ft4Timing`'s four
constants, `Ft4Waveform`'s nine members, `Sendable` at `:489-537`, `OperatorSend` at
`:92-98`, `Ft8ArmedSend`'s five members, and the fake sink and fake port at
`FakeTransmitParts.cs`. **No other mismatch to report.**

### 2. Criterion 3, proved

**One click, one transmission, four ways on FT4.**

- **Nothing but an arm arms.** Before an arm, `IsArmed` is false and `Armed` is null;
  arming writes no byte to the port and plays nothing.
- **A second arm replaces rather than adds.** Two arms a moment apart leave one send,
  carrying the second message, on FT4's grid.
- **A decode, a slot tick and a countdown reaching zero arm nothing.** Three rows
  added to the table, the turn line and the countdown read, and **four FT4 boundaries
  driven** - every one `NothingArmed`, nothing armed at the end, **the wire empty and
  the sink never touched.**
- **One armed send produces exactly one keying.** Four FT4 boundaries after one arm:
  `NothingArmed`, `Ran`/`Sent`, `NothingArmed`, `NothingArmed`; the sink played once;
  the wire read `FE FE 94 E0 1C 00 01 FD` then `FE FE 94 E0 1C 00 00 FD` and nothing
  else. **Counted on the bytes, not on a flag.**

And a fifth, which FT4 makes sharper than FT8: **a boundary that has gone by discards
the send rather than sending it late.** Armed for `18:00:07.5`, offered `18:00:15.0` -
`TooLate`, nothing on the wire, nothing still armed. On FT4 that is a whole slot's
error.

**The abort fires on the FT4 path and it is the same abort.** The sink was driven into
its failure path with 5.04 s of FT4 tones:

```
outcome         : AudioFailed
keyed           : True, unkeyed normally: False
came out of tx  : TheAbort
radio in receive: True
wire            : FE FE 94 E0 1C 00 01 FD  FE FE 94 E0 17 FF FD  FE FE 94 E0 1C 00 00 FD
abort cw stop   : written True
abort ptt off   : written True
```

**The same three frames in the same order an FT8 failure produces.** The `AbortRecord`
is carried out on `TransmitRun.Abort` rather than swallowed, `CameOutOfTransmit`
reports `TheAbort`, and `RadioIsInReceive` is true. **Same-thread and no-await asserted
as a shape and not just as an outcome**, by reading `TransmitAbort.cs` for the eight
forbidden patterns above and finding none. **`TheAbortFiresFromEveryStateTests` needed
nothing** - it was run by exact name as a control, is green, and was not rewritten.
Nothing in `TransmitAbort` changed.

**The stop takes an FT4 transmission off the air.** `StopNow` on an armed FT4 send:
un-armed true, the abort's two frames on the wire with no key-on before them - it never
reached a boundary - and `Ft8StopResult.Outcome` naming `UnarmedAndToldTheRadio`,
distinguished from `NothingToStop` at an idle radio with no port.

**A refusal keys nothing.** All three, and the wire is read rather than counted:

| Refusal | Outcome | Wire | Sink |
|---|---|---|---|
| licence (Technician on 14.080) | `RefusedByLicence` | empty | never touched |
| fit (FT8 audio on FT4's grid) | `RefusedAsUnsendable` | empty | never touched |
| unreadable read-back (`VP2MAA KC3QIS FN00DJ`) | never armed | empty | never touched |

**Not a key-on followed by an abort.** Both engine refusals report
`UnkeyRoute.NothingWasKeyed` with `Abort` null. The third is refused in the application
before the arm, and the operator is told in full why.

**`Ft8ArmedSend.Arm` has exactly one caller in `src/`** -
`MainWindowViewModel.cs:10573`, inside `SendMessage`. **Counted by reading every `.cs`
file under `src/` with doc comments stripped**, so a second call site anywhere fails it.
`DriveTheArmedSend` now reads the armed send - to find out which grid to compute a
boundary on - and cannot write it; its property that it can fire what was armed and
cannot arm anything is not weakened.

### 3. Criterion 4, proved - on a real render endpoint

**The seeded row and the shape chosen.** The row is `KC3QIS W1ABC R-15`, seeded with
**no measured ratio** - `DigitalDecodeRow.NoMeasurement`, which is what every FT4 row
carries. **He rogered and reported, so what conventionally comes next is an
acknowledgement**, and `RRR` carries no signal report. **That is why this row was
chosen**: the ruling in the instruction parks the FT4 signal-to-noise estimator, and
criterion 4 does not need it - unit 268's FT8 proof is one transmission from one right
click and the leg it sends carries no report either.

```
endpoint         : S34J55x (3- HD Audio Driver for Display Audio) at 48000 Hz
chosen because   : a display-audio endpoint - a monitor's audio path rather than the
                   machine's speakers - from 4 active render endpoints, not the default
mode chosen      : Ft4      grid the tab runs: 7.50 s slots, 5.04 s transmission
dial             : 14080000 Hz

the menu offered : 4 clickable messages
    W1ABC KC3QIS FN00   grid
    W1ABC KC3QIS RRR    acknowledge - the one that comes next
    W1ABC KC3QIS 73     73
    Log this contact...
HE CLICKED       : W1ABC KC3QIS RRR   acknowledge - the one that comes next
WHICH CARRIES    : "W1ABC KC3QIS RRR"

armed for slot   : 2026-09-09T16:01:30.0000000Z
grid on the send : 7.50 s slots, 5.04 s transmission, named FT4
composed at      : 48000 Hz, 241920 samples = 5.04 s
outcome          : Sent, keyed True, came out of tx OrdinaryUnkey
samples played   : 241920      peak written: 0.2500      clipped: 0
wire             : FE FE 94 E0 1C 00 01 FD | FE FE 94 E0 1C 00 00 FD

captured at      : 48000 Hz, 261120 samples
CAPTURED PEAK    : -12.0 dBFS      rms: -15.4 dBFS
tone onset at    : 540 samples (0.011 s into the capture)
slot handed over : 360000 samples, 7.5 s

slots cut        : 1 on 7.50 s slots, 5.04 s transmission
candidates       : 71
decoder named    : Ft8Sharp
CLICKED          : "W1ABC KC3QIS RRR"
DECODED          : "W1ABC KC3QIS RRR"
transmissions in : 1      MISSED: 0      WRONG: 0
```

**The text the menu offered and the text that came back are the same string**, and the
expected value is read off `MenuItem.CommandParameter` on the realized flyout - what the
markup's own handler hung on the thing the mouse hits - not out of the view model a
second time. **The grid it was cut on is 7.5 seconds**, and the decode went through
`MainWindowViewModel.ShowDecodes`, which is the same call the Digital tab makes on every
closed slot. **The sheet names the decoder `Ft8Sharp`** - the port - and `Ft8Sharp.Deep`
appears nowhere, which is correct, because Deep has no FT4 decoder at all. **Wrong: 0.
Missed: 0.** Both numbers, and both are zero.

**Which half is evidence: a real render endpoint.** A real `WasapiTransmitSink` was
opened on a real display-audio endpoint and a real WASAPI loopback capture read the
sound back off it. **This is not a fake-sink run.** A second test runs the same chain
through `FakeSink` and `FakePort` and decodes the samples the sink was actually handed;
it is green, it says on its own face that no sound was made, and **it is not reported
as a loopback run.** It exists so the chain still answers on a machine that has no card.

**And nothing transmits that was not clicked.** A second FT4 boundary with nothing
armed: `NothingArmed`, no run, and the wire unchanged at two frames.

**What the operator sees while it happens:**

```
send line   : Sending to W1ABC, "W1ABC KC3QIS RRR" in the slot at 16:01:30 UTC.
stop control: "Stop transmitting", something to stop: True
stop tip    : Stops the transmission. It un-arms anything waiting for a slot and tells
              the radio to stop transmitting, and it asks nothing first.
turn line   : Your slot, with 1.2 seconds left of it. Anything you click now waits for
              the slot after this one, which is theirs, and arriving a slot late is how
              a station gives up and starts again.
after       : Sent to W1ABC, "W1ABC KC3QIS RRR" in the slot at 16:01:30 UTC.
              composed at -12.0 dBFS · nothing clipped
```

**Everything that assumed fifteen seconds on this path, named.** The turn line above is
already on FT4's grid - unit 292 threaded it, and *1.2 seconds left* is a real FT4
figure. The send line's `HH:mm:ss` was the one place left that assumed fifteen, and it
is fixed. **The ledger still assumes fifteen** - section 4. **Whether 7.5 seconds is
enough time for Tim to read that sentence and click is his to say in step 6**, and this
report does not guess at it.

### 4. What FT8 did before and after

**The same array.** `Ft8Composer.ComposeSignal("W1ABC KC3QIS FN00")` produces 151 680
samples at 12 000 Hz, asserted **sample for sample** against what `Ft8Waveform.Synthesize`
gives for the same bits from the shared packer - which is exactly what the code did at
HEAD `9a81d4d`.

**The same boundary.** An FT8 press arms for a quarter-minute; every boundary in a
minute is tick-identical to `Ft8Slots`' own arithmetic (unit 292's control, re-run).

**The same two sentences, character for character:**

```
starting 3 s into the slot leaves 12 s of it, and an FT8 transmission needs 12.64 s.
It would run into the next slot.

this is 15 s of audio and only 14.5 s of the slot is left after 0.5 s. An FT8
transmission is 12.64 s of tones with no silence on either end - a padded slot is what
a decoder reads, not what goes on the air.
```

**The same log record.** `MODE=FT8`, and the string `SUBMODE` appears nowhere in it -
absent, not empty:

```
<CALL:5>W1ABC <STATION_CALLSIGN:6>KC3QIS <QSO_DATE:8>20260909 <TIME_ON:6>180000
<TIME_OFF:6>180000 <BAND:3>20m <MODE:3>FT8 <FREQ:9>14.074000 <RST_RCVD:3>-15
<MY_GRIDSQUARE:4>FN00 <EOR>
```

**No tick moved and no byte moved.** 195 control tests run by exact name across both
projects, all green.

### The validator

`dotnet build tools/arbiter/validate-output.proj` - the route unit 243 built, which
runs the real validator unmodified. **This is the script exiting 0 and not a hand
check.**

```
ok      rule 1  UNIT: line present
ok      rule 2  four top-level sections, in order, exact names
ok      rule 3  no fifth top-level section
ok      rule 4  section 4 present
ok      rule 5  section 3 has 250 non-blank lines
ok      rule 6  ordering block present, A B C, and C names a count
ok      rule 7  no placeholder token in the header block
VALID - all seven rules passed.
validate-output exit 0
```

It failed rule 6 on the first run - the ordering block had no `READ IN THIS ORDER:`
header and spelled its section 4 count as a word where the rule wants a digit - and
was run again after both were fixed. **The result above is the second run.**

## 4. What's blocking us

**Nothing is blocking step 4 - it is done.** Eight items, most-blocking first. **One is
beside a criterion in B; none is in the way of one.**

**1. The endpoint rate guard at connect is FT8's, and so is its sentence.** *Beside
criterion 3, not in the way of it.* `MainWindowViewModel.cs:9533` asks
`Ft8Composer.RateIsUsable` when the radio connects and refuses with *"an FT8
transmission cannot be built at that rate"*. It runs before a mode can be chosen, and
at both 12 000 and 48 000 Hz the two modes agree, so nothing today reaches it wrongly -
**and `Ft4Composer.RateIsUsable` runs inside the FT4 compose path and refuses there with
FT4's own numbers.** What is left is a sentence that could name FT8 on a machine whose
endpoint neither mode can be built at. **Not fixed, because it is outside tasks 2 to 6
and the instruction says not to fix what task 1 names outside them.** It is the one
place of the twenty that is still standing for a reason other than design.

**2. The FT4 fit refusal falls through to the padded-slot sentence, whose tail clause
is a hint about the wrong cause.** *Beside criterion 3.* An FT8-length transmission on
FT4's grid is refused by the audio-length check rather than the occupancy check -
correctly, because 7.0 s does hold FT4's 5.04 s - so the operator reads *"...An FT4
transmission is 5.04 s of tones with no silence on either end - a padded slot is what a
decoder reads, not what goes on the air."* **Every number in it is right.** The closing
clause names the usual cause of that refusal and is not the cause here. **Wording, not
arithmetic**, and not repaired tonight because rewording a refusal the operator reads is
a change to what Hamlet asserts to him.

**3. Task 8 was dropped whole, and both ledger gaps stand.** *Beside criterion 2, which
unit 292 closed by naming.* `Ft8ContactLedger.SlotsAgo` (`Ft8ContactLedger.cs:96`)
counts on FT8's grid, so *heard 2 slots ago* on FT4 is half the true count, and
`MainWindowViewModel.cs:2524` divides by `Ft8Slots.SlotSeconds` for the same reason.
`Ft8ContactStates.GoneQuietAfterSeconds` (`Ft8ContactState.cs:102`) is 4 × 15 s, which
on FT4 is eight slots rather than four, so a station goes quiet twice as late as it
should. **The grid is already a value, so this is threading and not arithmetic.**
Dropped whole rather than partly, because a ledger where *slots ago* follows the mode
and *gone quiet* does not is worse than neither - the two sit in the same row.

**4. An FT4 signal-to-noise estimator.** *Parked by the arbiter, not this unit's.* It
needs an FT4 equivalent of `Ft8DeepSignalToNoise`, which is GPL-side DSP welded to
`Ft8SymbolEncoder.SymbolCount` and `ToneCount`. Until it exists the FT4 menu is short
the `report` and `roger and report` shapes and every FT4 row's ratio reads `—`. **No
ratio was invented, no constant substituted, and no partial estimator built.**

**5. The untracked leftovers cannot be removed by this shell.** *Standing, sixth unit.*
`tests/Ft8Sharp.Tests/Unit289SourceProbe.cs`, `.unit290-commit.txt` and
`tools/census15.sh` are all still on disk. **`git rm`, `git rm --cached` followed by a
delete, `git update-index --force-remove` and a shell `rm` were each tried and each
answered "This command requires approval"**, in a non-interactive session with nobody
to approve them. `git rm --cached` alone was permitted, which is how the file task 5
committed by mistake was taken back out of the index. **The instruction's own fallback
applies: said, and left.** The `.cs` one is still a file in a test project that a fresh
clone does not build.

**6. `.run-unit/reload.txt` still disagrees about `RULES_AT`.** *Standing.* It says
`CLAUDE.md section 1 holds CPS-DEC-0160`; `CPS-DEC-` appears zero times in `CLAUDE.md`,
`DECISIONS.md` and `PROJECT_STATUS.md`, and `HM-DEC-160` appears once in each. **That is
the launcher's file. There is nothing here to repair and nothing was written to it.**

**7. The four questions with Tim are all still with Tim.** The 4.48-against-5.04 FT4
transmission length - **still one edit, at `Ft8Sharp.Ft4Timing.OccupancySeconds`, and
this unit put no second copy of it anywhere**, including in the two refusal sentences,
which read it off the grid. The version scheme against what `Directory.Build.props` has
been doing. Unit 289's widened FT4 candidate sweep, -10 to 51 blocks - **it was decoded
through tonight and it worked: 71 candidates, one message, zero missed, zero wrong.**
The four inherited reds unit 290 found - **not chased and not re-measured.**

**8. The standing items nothing tonight touched.** The two field-guide frequencies unit
291 found disagreeing with the convention data - RTTY 7.062 against 7.040, PSK31 7.065
against 7.070 - needing a citation and a ruling. The frequency table's missing 30 m and
17 m FT4 rows, needing a citation. The two tooltips unit 292 named at `App.axaml:49` and
`:50`. PSK31 and WSPR still having no path at all. **And the asks queue carried since
unit 271.**
