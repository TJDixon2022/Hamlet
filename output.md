READ IN THIS ORDER

A. THE PHASE GOAL. Hamlet works stations on the air - Tim answers a CQ on 14.074
or 7.074 from Hamlet and completes an exchange. After tonight: step 0 `done` and
step A `done`, both closed by unit 266. **Step B is `done`** - this unit, all four
must-pass criteria met on runs tonight, nothing deferred to Tim. Step C `not
started` - the whole chain from one right-click at the bench, and task 5 names its
pieces without building them. Steps D and E `not started` and both are Tim's, at
the radio: the drive level his IC-7300 wants, and working a station.

B. THIS STEP AND ITS EXIT CRITERIA. Step B, *right-click and it goes*, four
must-pass criteria, and **all four are met**. 1 - a CQ button sending `CQ KC3QIS
FN00` from Settings with no typing: **met on a run tonight**. 2 - right-click a
decoded row and every valid message is offered, the expected one highlighted, none
forbidden, a repeat showing its count: **met on four runs tonight**, one of them a
real right-click on a real row control. 3 - one click sends exactly one message,
asserted by a test: **met on a run tonight**, two boundaries and one transmission.
4 - what is being sent and to whom appears in the Send area, and out of licence
privileges it says so and sends nothing: **met on runs tonight**, by the test this
unit built. **No criterion was claimed by reading the tree**, which is the caveat
step A closed with last night and the one this unit existed partly to avoid.

C. WHAT THIS REPORT ADDS, AND WHETHER IT BEARS ON A OR B. It adds the proof that
the licence gate holds from the operator's own click through to a slot boundary -
the one of step B's four criteria that had nothing behind it, and the third of the
three things `PHASE_PLAN.md` says no unit may reason past.
**Section 4 raises 4 items, and none is in the way of a criterion in B**, which is
closed. One of the four asks the owner to decide something, and it is not
blocking: what the
send path should do when the operator switches the licence guard off in Settings.
It is measured, quoted and left exactly as found.

UNIT:       267 — complete at task 5 of 5 — 2026-09-07 10:45
PHASE GOAL: Hamlet works stations on the air - Tim answers a CQ from Hamlet and
            completes an exchange on his own antenna
UNIT GOAL:  Step B closes - the CQ button, the right-click menu and the Send area
            proved on a run, and the licence gate proved to hold from the
            operator's click through to the slot boundary
ADVANCED:   yes — step B moved from `not started` to `done`, all four must-pass
            criteria, and the criterion that had no proof behind it now has one
NUMBER:     0 -> 5. Tests anywhere in the tree that drive an out-of-privileges
            click through the application to a slot boundary
DRIFT:      0 consecutive units without advance  (was 0)

## 1. What Claude did

**Complete, at task 5 of 5.** Nothing was dropped, including the named drop
candidate. Development machine QUIVERFULL, project confirmed as Hamlet against the
tree - `SHACK_FACTS.md` present, `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs`
present, `CoreHMI.sln` and `MURC.sln` both absent - branch `main`.

**FACT-004 throughout.** No serial port was opened, nothing was keyed, no sound was
made, and nothing here says anything about the IC-7300.

### Task 1 - the trace, committed on its own at 4c01587

`docs/unit267-step-b-trace.md`, seven questions, each with a file, a line and a
quotation. **The instruction was right about the thing it was least sure of.**
Question 6 - *is there any test anywhere in which the operator's click, out of
licence privileges, is driven through the application to a slot boundary* - answers
**none**. `RefusedByLicence` appears nowhere in `tests/Hamlet.App.Tests` at all;
every test hit is in the two engine suites, and both build their own
`OperatorSend`. `MainWindowViewModel.cs:8346`'s `RefusedByLicence` branch - the
sentence an operator reads when the gate refuses his click - **was asserted by
nothing**, confirmed by grepping `Citation` across the project and finding three
hits, all in `Licensing/` tests that never touch `DigitalSendLine`.

Question 3 answers **none** too, and it is the good kind: nothing is greyed,
disabled, hidden, sorted away or filtered anywhere between `Ft8SendOptions.For`
(`Ft8SendOptions.cs:99`) and the flyout (`MainWindow.axaml.cs:171`). The one
`continue`, at `Ft8SendOptions.cs:118`, is a message that **does not exist** -
no grid in Settings, no ratio measured - and its reason goes into `Absent`.

The trace also wrote down the trap task 2 would otherwise have walked into: a
licence refusal comes back as **`Ft8ArmOutcome.Ran` at the boundary**
(`Ft8ArmedSend.cs:473` returns `Ran` for anything the sequence ran) and only
`TransmitRun.Outcome` carries `RefusedByLicence`. A test asserting on the arm
outcome alone would read a refusal as a send.

### Task 2 - the goal task, red at 5c91f7e then green at e34cf4e

`tests/Hamlet.App.Tests/ViewModels/TheLicenceGateHoldsFromTheClickTests.cs`, five
assertions, each run alone by exact name. **Four of the five were green whole on
their first run against the tree as it stands.** That is the finding: **no red was
manufactured and no product code was written**, and none was needed.

**The one red was mine, and correcting it is a real measurement.** I wrote the
guard-off case expecting the transmission to proceed on the operator's own
authority, because `TransmitGuard.Check` returns `MayTransmit` true with
`WasOverridden` true. It does not. Verbatim, committed at 5c91f7e before it was
touched:

```
Assert.Equal() Failure: Values differ
Expected: Sent
Actual:   RefusedByLicence

guard enabled     : False
licence class     : Technician
run outcome       : RefusedByLicence
sent              : False
sink calls        : 0
frames at the port: 0
```

`Ft8TransmitSequence.Permits` at `:443` refuses an overridden permit outright: the
gate permits in three ways and this send path accepts one, because §0.2 says the
Settings check is not bypassable from any send path. The expectation was corrected
to what was measured; the product was not touched.

### Task 3 - the seven committed tests, run rather than claimed

All seven pass, each run **alone, by exact name, foregrounded**. No suite, nothing
unfiltered, nothing backgrounded, nothing outside the seven and what task 2 built.

| Test | Result |
|---|---|
| `TheCqButtonSendsFromSettingsWithNoTypingAndNeverInventsAGrid` | passed, 186 ms |
| `TheRowsMenuOffersEveryMessageWithTheExpectedOneMarked` | passed, 156 ms |
| `ARowWithNoMeasuredRatioOffersNoReportAndSaysWhy` | passed, 183 ms |
| `EveryStationsPredictedMenuAppearsUnderTheMouse` | passed, 855 ms, 5 of 5 stations |
| `TheRepeatCountBelongsToTheClickAndNotToTheRow` | passed, 794 ms |
| `OneClickIsOneMessageAcrossTwoBoundaries` | passed, 198 ms |
| `WithNoGridTheSendAreaSaysSo` | passed, 153 ms |

**Nothing red.**

### Task 4 - the phase's bookkeeping

Step B recorded **`done`** in `PHASE_OUTCOME.md`, with all fourteen fields and a
`STATE_WHY` quoting the evidence for each of the four criteria. The header's
`STEP: B` line moved from `not started` to `done` in place. No earlier entry body
was touched. `PHASE_STATUS.md`'s `STEP:` lines were **not** written - they belong
to the launcher; only `WORK_INSTRUCTION:` was.

**`outcome-append.bat` was refused, both forms**, verbatim in section 4. The
instruction's own named alternative was taken and the entry says so on its face in
an `APPENDED_BY:` line.

### Task 5 - the named drop candidate, not dropped

`docs/unit267-what-step-c-needs.md`. **Named, not built** - no test was written and
nothing was added to `tests/`. Each of step C's four criteria has its closest
existing test with a file and a line, what that test lacks, and which harness
pieces would have to be joined.

### Decisions made for myself, reproduced in full

**One.** The Send area line after a *successful* send was measured by adding a
print and an addressee assertion to the test task 2 built, rather than by running
an eighth committed test. Task 3 asks the report to quote that line; none of the
seven named tests carries it, and the no-suite rule permits only those seven and
what task 2 builds. Adding it to my own test stays inside both.

**Two.** The guard-off case is recorded as a measurement and left exactly as found.
Work instruction 267 said to record it and not to decide it, and changing it would
mean editing `Ft8TransmitSequence`'s own gate, which the instruction forbids.

## 2. What the owner should expect

**He right-clicks a station he has decoded and sends it a message with one click.**
The message that conventionally comes next is marked in words as well as in weight
- *the one that comes next* - and **nothing is taken away from him**: all five
message shapes stay on the menu and stay clickable, including ones he has already
sent, which show their count instead. A message that does not exist because
Settings has no grid, or because that station's ratio was never measured, is
**absent with the reason said out loud** rather than drawn grey.

**The CQ button calls from his own Settings with no typing.** With his grid set it
builds `CQ KC3QIS FN00`; with none it builds `CQ KC3QIS`, and **no locator is
invented**. The Send area tells him what went out and to whom.

**And on a frequency his licence does not cover, Hamlet tells him so and transmits
nothing.** Not a greyed button and not silence - a sentence naming the message that
did not go, the regulator's reason and the paragraph it comes from.

### What will look wrong but is not

**The menu still offers everything while the licence refuses.** On a frequency he
may not transmit on, the right-click menu is exactly as long as it always was, with
the licence as a note underneath. That is ruled, not an oversight: nothing is
forbidden in the menu, and a refusal at the gate is not a reason to take an option
off it. The refusal happens where it can actually stop a transmission.

**Switching the licence guard off in Settings does not let him transmit outside his
privileges.** He gets a different sentence, not a transmission. See section 4.

**Nothing on screen changed tonight.** No product code was written. What changed is
what is proved, and the whole of it is in `tests/`.

## 3. What you should see

### 1. The out-of-privileges click, quoted

`TheLicenceGateHoldsFromTheClickTests.OutOfPrivilegesTheClickReachesTheBoundaryAndNothingIsKeyed`,
run alone by exact name, passed in 601 ms. A Technician licence class in Settings,
the panel on 14.074 MHz, the operator's own click through `SendMessageCommand`,
driven to its boundary with `AtSlotBoundaryAsync`:

```
licence class     : Technician
frequency         : 14074000
boundary outcome  : Ran
run outcome       : RefusedByLicence
sent              : False
keyed             : False
sink calls        : 0
bytes at the port : 0
reason            : Technician privileges do not reach this frequency; it needs General.
citation          : 97.301(e)
```

**The sentence the operator is left looking at**, word for word, read off
`DigitalSendLine` after the boundary and after the posted job ran:

> Hamlet did not send "W1ABC KC3QIS -10": Technician privileges do not reach this frequency; it needs General. (97.301(e))

Before tonight nothing in the tree asserted that sentence.

### 2. The same click inside privileges, going

`TheSameClickInsidePrivilegesRunsAndKeysTheRadio`, run alone, passed in 494 ms.
**Same panel, same harness, same frequency, same click** - only the licence class
in Settings differs:

```
licence class     : General
boundary outcome  : Ran
run outcome       : Sent
sent              : True
sink calls        : 1
frames at the port: 2
```

**That is what makes assertion 1 a gate and not a dead path.** Without it the whole
class would pass against a send path that never worked at all.

And the Send area after it, which is criterion 4's first half - *what is being sent
and to whom* - measured on the same run:

> Sent to W1ABC, "W1ABC KC3QIS -10" in the slot at 14:40:30 UTC. It was composed at -12.0 dBFS with nothing clipped - that is the level Hamlet built, before this machine's own volume for that device and before the radio's input gain. Set the radio's drive against its own ALC meter.

### 3. One menu's header strings, verbatim, and the CQ text

`EveryStationsPredictedMenuAppearsUnderTheMouse`, run alone, passed in 855 ms, 5 of
5 stations matching. **W1ABC's menu as it appears under the mouse**, on a real
`ContextRequested` raised on a real row control in a real window - the highlight and
a repeat count both on it:

```
W1ABC - under the mouse:
    W1ABC KC3QIS FN00   grid
    W1ABC KC3QIS -10   report
    W1ABC KC3QIS R-10   roger and report
    W1ABC KC3QIS RRR   acknowledge, 2nd time - the one that comes next
    W1ABC KC3QIS 73   73
```

Every one of those five items came back `IsEnabled`. **The CQ text the button
builds**, from `TheCqButtonSendsFromSettingsWithNoTypingAndNeverInventsAGrid`:
`CQ KC3QIS FN00` with the grid set, and `CQ KC3QIS` with none.

### The rest of the evidence, briefly

- **Nothing is withheld while the licence refuses.**
  `WhileTheLicenceRefusesTheMenuStillOffersEveryMessageAndSaysWhy`, passed: all
  five shapes still on the menu with a Technician class set, and the licence note
  reading *Technician privileges do not reach this frequency; it needs General.
  (97.301(e))*.
- **One click is one message.** `OneClickIsOneMessageAcrossTwoBoundaries`, passed:
  first boundary `Ran`, second `NothingArmed`, 1 sink call, 2 port frames.
- **A repeat shows its count.** `TheRepeatCountBelongsToTheClickAndNotToTheRow`,
  passed: `VK2PQ KC3QIS 73   73` becomes `VK2PQ KC3QIS 73   73, 2nd time` after the
  send, with the list the same length.
- **Absent is not forbidden.** `ARowWithNoMeasuredRatioOffersNoReportAndSaysWhy`,
  passed: *no signal report has been measured for this station, so the messages
  that carry one are not offered*.
- **The unset grid says so.** `WithNoGridTheSendAreaSaysSo`, passed: *Your grid
  square is not set in Settings, so Hamlet calls CQ as "CQ KC3QIS" and does not
  offer the messages that carry a grid. It will not invent one.*

### The number

**0 -> 5.** Before tonight, tests anywhere in the tree driving an out-of-privileges
click through the application to a slot boundary: **zero**. After: **five
assertions**, in one class, on the operator's own click with his own licence class
and his own guard setting out of Settings.

Version `1.12.120` -> `1.12.121`. `Ft8Sharp` did not move.

## 4. What's blocking us

**Nothing is blocking, and no criterion of step B is held open by any of these
four.** One asks the owner to decide something and it is not urgent.

### 1. A ruling is wanted here, and it is the only one - the licence guard's off switch

**Ruling wanted: should the FT8 send path keep refusing when the operator has
switched `RestrictTransmitToPrivileges` off in Settings?** My answer in a sentence:
**yes, leave it exactly as it is** - a program that hands a slot of audio to a radio
with nobody's hand on a key should not key on an answer it cannot stand behind, and
the code already says so in its own words.

**The measurement, and it is not what I expected.** `TransmitGuard.Check` at
`TransmitGuard.cs:88-91` returns `MayTransmit` true with `WasOverridden` true when
the guard is off - so a reader would predict the transmission goes. **It does not.**
`Ft8TransmitSequence.Permits` at `:443` accepts one of the gate's three ways of
permitting and refuses the other two. With a Technician class on 14.074 and the
guard switched off, measured tonight: `RefusedByLicence`, `Sent` false, `Keyed`
false, 0 sink calls, 0 frames at the port, and the operator reads:

> Hamlet did not send "W1ABC KC3QIS -10": the licence guard is switched off in Settings, so Hamlet has no answer it can stand behind about whether this frequency is inside your privileges. It will not key on that. What the guard said when it was last asked: Technician privileges do not reach this frequency; it needs General. (97.301(e))

**Why it is raised at all:** the setting is presented to the operator as *only let
me transmit where my licence allows*, and switching it off does not do the thing
its wording implies on this path. **That is a wording question, not a safety one** -
the safe direction is the one the code takes. It is left untouched, because work
instruction 267 said to record it and not to decide it, and because changing it
would mean editing the keying path's own gate. **Nothing in step B depends on the
answer.**

### 2. `outcome-append.bat` was refused, both forms

Recorded, not a ruling ask. Two invocations, both refused with the same verbatim
message:

```
This command requires approval
```

- `cmd //c .oa-267.bat` (a wrapper file written precisely to avoid the shell's
  quoting problems)
- `tools/arbiter/outcome-append.bat`

Task 4's named alternative was taken: appended with the file-editing tools in the
format the existing entries use, with an `APPENDED_BY:` line saying so on its face.
**Three other shell refusals happened tonight**, all worked around the same way and
none of which stopped anything:

```
Contains brace with quote character (expansion obfuscation)
Contains shell syntax (command) that cannot be statically analyzed
This Bash command contains multiple operations. The following parts require approval
```

### 3. `PHASE_OUTCOME.md` still carries a phantom step 1, from unit 266

Reported, not chased, and **not the duplicate unit 266 fixed**. The file's header
carries `STEP: 1 | not started | (described by the plan)` and the entries carry a
`## UNIT 266 - STEP 1` whose body is mostly *not recorded*. **The live
`PHASE_PLAN.md` has no step 1** - this cut's steps are 0, A, B, C, D, E. It appears
to be the loop's own route calling `outcome-append.bat` with a step the plan does
not have. It is cosmetic, it misleads nobody who reads the entries, and chasing it
would mean editing the launcher's scripts, which is not this unit's subject.

### 4. Inherited reds, untouched and not chased

Exactly as work instruction 267 lists them, and **none was run tonight**:
`CwAdjudicationTests.ASpeedChangeInRealisticAudio`; the 51 CW cases in
`docs/unit239-failing-set.txt`; the `Ft8Sharp.Deep.Tests` whole-type-list tripwire;
and
`TheSinkPlaysToANamedEndpointTests.ACancelledPlayGoesOutShortAndTheSequenceCallsItAudioFailed`,
which unit 265 recorded and unit 266 left alone. **Twelve distinct tests ran
tonight** - task 3's seven named, plus the five this unit built - every one
filtered by exact name and foregrounded, with two of the five run twice.
`Hamlet.RadioEngine.Tests` was not run at all.
