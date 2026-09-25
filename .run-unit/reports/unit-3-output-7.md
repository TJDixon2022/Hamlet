READ IN THIS ORDER.

A. Phase goal: Hamlet reads a CQ call correctly. Steps 0, 1, 2 done; 3, 6 and 7 partial; 4 and
   5 not started. This unit moved step 7. The preamp is now turned off on an overload that
   starts after the tune-in, not only on one read at it. Nothing in the decoder moved.
B. Step 7's criterion 7.8, clause by clause:
   - **The manual's rows with the page:** still true. The CW row is unchanged: preamp 1 from
     1.8 to 29.999 MHz, preamp 2 from 50 to 54 MHz, off while overloading, citing
     `IC-7300_ENG_FM_12b` page 4-3.
   - **Off when overloading:** met at the tune-in and now after it. At every one of the six
     traced frequencies, one write of off follows an overload that holds; the band's value goes
     back once it clears; nothing is written after his hand, while transmitting, or in a block
     that does not state the preamp.
   - **No voice asking him to change a field Hamlet set:** 0 at the tune-in and after it, for
     all nine fields, where the tuned block still owns the field. Once he has moved on,
     sentences asking him to change a field the CW tune-in left went from 60 to 12. The 12 are
     one true sentence about the attenuator (section 4, item 1).
   - **The table:** printed in section 3 for 160 m to 10 m and 50.100 MHz. 160 m, 12 m and 6 m
     find no block on the map (P23).
   - **Can 7.8 be ticked?** Only if clause three is read as the fields the tuned block owns.
     Read strictly, the 12 attenuator sentences after moving on keep it open. I have not ticked
     it.
   - **7.6 at exit:** holds. All three floors and both carry-forward lines are green, and
     nothing green at entry is red.
C. The rest. Section 4 raises 9 items: 3 new ones and 6 carried. Item 1 bears on ticking 7.8;
   none stands in the way of the preamp work.

```
UNIT:       426 - complete at task 4 of 4, none dropped - 2026-09-24 21:04
PHASE GOAL: Hamlet prints a CQ call as it was sent, and the receiver is set right for the mode and stays right while he listens
UNIT GOAL:  When the band starts overloading after he has tuned in to Morse, Hamlet turns the preamp off itself and back when it clears, leaves it alone once he has touched it, and nothing on screen asks him to change any of the nine fields the CW row sets
ADVANCED:   yes - 7.8's second clause now holds after the tune-in, with one preamp-off write at each of six frequencies, 0 before; sentences asking him to change a field Hamlet set fell 60 to 12
NUMBER:     sentences asking him to change a field Hamlet set, all nine fields, after the tune-in: 60 -> 12, and 0 -> 0 where the tuned block still owns the field; preamp writes on a live overload at 7.030: 0 -> 1
DRIFT:      0 consecutive units without advance, was 1 in unit 425's report and 0 in the work instruction
```

## 1. What Claude did

**Complete, at task 4 of 4, none dropped.** The drop candidate, writing the band's value back
when the overload clears, is built. The run was on QUIVERFULL, in C:\Source\HamLet, on branch
`main`. The gate confirmed Hamlet. All five commits are pushed.

- **Task 0, `c14c8089`.** HM-DEC-179 is in `DECISIONS.md` with the five limits, marked as the
  arbiter's reading and overrulable by Tim, with the two rejected options. It has a row at the
  top of `CLAUDE.md` §1 dated 2026-09-24. P22 is marked taken up. `PHASE_OUTCOME.md` has
  UNIT 426 - STEP 7 with the entry round. `PHASE_STATUS.md` names unit 426 at CURRENT_STEP 7.
  The version went from 1.13.112 to 1.13.113. Entry round, one type per invocation:
  - build: 0 errors
  - engine carry-forward: 178 of 178
  - app carry-forward: 275 of 278. The 3 misses were the dispatcher loop: `ThePowerIsOfferedTests`
    twice and `TheRecordNamesTheSubModePressedTests` Olivia. Alone they pass, 3 of 3 and 12 of 12.
  - floors: captures 51 of 51, adjudicated 13 of 13, keyed floors 13 of 13
  - every Rig type: 45 types, 342 of 342
  - `OneVoiceOnThePreampTests` 16 of 16, `WhatIsSaidAboutThePreampTests` 1 of 1,
    `TheOverloadSentenceLeavesTheModesFieldsTests` 4 of 4
  - `DecisionLogOrderTests`: red on the missing HM-DEC-166 row (P25), as expected.
- **Task 1, `5200a490`.** I added `WhatHappensWhenTheBandOverloadsTests.PrintWhatHappens`, a fact
  that asserts nothing, at 3.530, 7.030, 14.050, 21.050, 28.050 and 50.100 MHz. It runs four
  points: a quiet tune-in; `Overflow` overloading for 12 polls; clear for 40; then his hand sets
  the preamp and the overload comes back. At each point it prints what the preamp is asked for,
  every write, the read-back, and every sentence the setup's voice, Receive Help
  (`ReceiveAdvice`), `RigObservations`, `ReceiveObstructions`, the front-end chip and the
  overload sentence say about each of the nine fields. It does this in the block, moved into a
  block that states nothing, and moved into FT8's block, and counts per field and point.
  - **The line that would have to act:** `MainWindowViewModel.ApplyRigState`, at
    `MainWindowViewModel.cs:10983-10987` at entry. It read the flag into
    `FrontEndIsOverloading` and nothing acted on it.
  - **The sentences that contradicted what Hamlet set:**
    - `ReceiveAdvice.Gain` (`ReceiveAdvice.cs:456-486`), once he has moved on: *"Open the
      receive gain all the way. It is at about 39 percent"*, with the gain at 100 percent. This
      is false as well as a request to change a field the CW row set.
    - The unowned overload sentence (`MainWindowViewModel.cs:9094-9101`): *"Press P.AMP/ATT ...
      until the preamp reads off"*, while the preamp Hamlet left was on through an overload.
  - **A second fact, `MeasureTheLivePollCadence`,** times the live pass: 7 fields, then the
    plan's 250 ms. `ScriptedRadio` now answers the transmit flag, and the live meters when asked.
- **Task 2, `507b14da`, red by itself.** The tests and their messages are in section 3. I moved
  the tune-in and poll into `LivePollBench`, shared by the trace and the engine tests and linked
  into the app tests. Its `OnPollAsync` is what the app does with a poll, and at that commit it
  wrote nothing.
- **Task 3, `40691542`.** I built `ReceiverSetup.FollowOverloadAsync`, in the one component that
  owns the preamp:
  - **Off:** 4 readings of overloading in a row write off once.
  - **Back on:** 20 readings of quiet write the band's own value back once.
  - **Relapse:** if the overload comes back and holds within 20 readings of the preamp going
    back on, it goes off and stays off until the next tune-in.
  - **Limits:** it writes only while the last tune-in's block states the preamp with an overload
    rule. It never writes while the radio is transmitting or while the transmit flag is unread.
  - **Read before and after:** it reads the preamp before and after writing, as the setup does.
    A hand found at write time stops it, and he is told in the setup's own words.
  - **One list for every voice:** it replaces the preamp's result in the tune-in's list, so every
    voice that reads the list reads what the radio now holds.

  `ApplyRigState` hands it each fresh `Overflow` reading, once. The app never runs it while a
  tune-in or another follow write is on the bus, and drops its result if a newer tune-in has
  overtaken it. After a write it re-reads the preamp at once, rather than at the 30 s sweep.
  `ReceiveAdvice.Gain` now compares on the read's scale. The owned overload sentence says where
  the preamp is and why. Task 2's tests went green, 7 of 7.
- **Task 4, this commit.** I added `PrintTheTable` for the 7.8 table. The replaced result now
  carries the follow's reason instead of the row's band reason. The exit round follows.

**Section 5 checks. Nothing was repaired.**
- `RigField.Overflow => RigPollRate.Live` is at `RigPollPlan.cs:145`, and `LiveInterval` is
  250 ms at `:64`. **Not in the instruction:** the preamp itself is not in the plan, so it falls
  to the default `RigPollRate.Session`, 30 s (`:177`). That is why task 3 re-reads it after a
  write.
- `ReceiverSetup` read `Overflow` only in `ResolveAsync`, at the tune-in: lines 349-352 for the
  attenuator and 372-375 for the preamp. The instruction's "around 350 to 375" matches.
- The other readers of `Overflow` in `src`:
  - `ReceiveAdvice.cs:380`
  - `MainWindowViewModel.cs:10983` (`ApplyRigState`)
  - `DigitalCaptureSheet.cs:202` and `:664`
  - `CivDecode.cs:83`
  - `CivReads.cs:202`
  - `RigReadout.cs:138`
- The CW preamp row carries 1800000 to 29999999 at 1 and 50000000 to 54000000 at 2, with
  `whenOverloading` 0, citing `IC-7300_ENG_FM_12b` page 4-3. It matches.
- The operator's hand is recorded by `ReceiverSetupMemory.MovedByHandSince`, at
  `ReceiverSetup.cs:101` at entry. The record is at `:79`.
- Transmitting is `RigState.IsTransmitting` (`RigState.cs:186`), read from `TransmitStatus`,
  which is polled live (`RigPollPlan.cs:124`).
- `DecisionLogOrderTests` is red at entry and at exit on the same assertion (P25).
- `EveryElementCarriesItsOwnPitchTests` and `NoSenderIsSplitInTwoTests` are still
  `Compile Remove`d. They were not on this unit's list.
- **No ruling forbids every write outside a tune-in in its own words.** The closest is
  HM-DEC-174's *"Every condition a mode states is written once when the radio is not already at
  it and not written when it is"*. That is about not rewriting a value already right, not about
  when a write may happen. The "once per tune-in, then hands off" wording is a code comment in
  `ReceiverSetup`, not a ruling. So the full build went ahead.

**Exit round (7.6), run twice.** It ran once, then again after the task 4 change to the
replaced result's reason. The figures below are the second run, on the final source.
- `Hamlet.sln` builds non-incremental with warnings as errors: 0 warnings, 0 errors.
- Engine carry-forward: 178 of 178. App carry-forward: 278 of 278, both runs.
- Captures: 51 of 51 in 119 s. Adjudicated 13 of 13. Keyed floors 13 of 13.
- Every Rig type: 47 types, 351 of 351. That is the entry's 342 plus the 9 new tests.
- Touched app types:
  - `WhatHappensWhenTheBandOverloadsTests` 3 of 3
  - `TheOverloadSentenceSaysWhereThePreampIsTests` 2 of 2
  - `OneVoiceOnThePreampTests` 16 of 16
  - `WhatIsSaidAboutThePreampTests` 1 of 1
  - `TheOverloadSentenceLeavesTheModesFieldsTests` 4 of 4
  - `TheFrontEndIsOnThePanelTests` 6 of 6
  - `HowMuchTheApplicationSaysTests` 5 of 5
  - `BindingHealthTests` 1 of 1
  - `WhichPathsPutNarrationOnTheBarTests` 6 of 6
  - `TheStatusBarStopsLecturingTests` 4 of 4
  - `WhatElseIsComposedAtRuntimeTests` 1 of 1
  - `RigDiagnosticsTests` 10 of 10
  - `ReceiveHelpViewModelTests` matches no test, as at unit 424.
- `DecisionLogOrderTests`: red on HM-DEC-166, as at entry. **Nothing green at entry is red.**
- `src/Hamlet.RadioEngine/Cw` prints nothing against entry `972510e6`. The transmit files print
  nothing against `7e209cb4`. `data` prints nothing against entry. `src` changed only in
  `ReceiverSetup.cs`, `ReceiveAdvice.cs` and `MainWindowViewModel.cs`.

**Decisions I made myself.**
- **The hold, from the measured cadence.** The live pass gives a fresh `Overflow` reading every
  252.9 ms median (249.9 min, 265.8 max) against the scripted radio. On the IC-7300 add about
  14 ms of wire a pass, by `PollBudgetTests`' 2 ms a read. I chose:
  - `OverloadHoldReadings = 4`, about 1 s.
  - `ClearHoldReadings = 20`, about 5 s. This is also the relapse window.

  No radio is on this machine (FACT-006), so the wire figure is an estimate, not a measurement.
- **The hold counts readings, not wall time.** The app hands over each distinct `Overflow`
  reading once, keyed on its timestamp.
- **An unread transmit flag counts as not safe.** HM-DEC-179 says never while transmitting; I
  also refuse to write when the radio will not say.
- **One write, not a retry loop.** A follow write the radio does not confirm is said as
  unconfirmed, and the follow stops until the next tune-in.
- **The follow acts only on a preamp the tune-in left right, found or set.** A preamp the tune-in
  found in his hand, could not read, or could not confirm is not followed.
- **The RF gain repair.** Task 1 found the RF gain contradicted once he has moved on, so task 2
  gave it a test and task 3 fixed the comparison. `RigWriteTests` had four RF gain readings
  written as raw 107 and 255 beside the texts "42%" and "100%". The real decode is the percent,
  so I moved those four numbers to 42 and 100, the values their own texts state. The assertions
  are unchanged, and the type is green, 351 of 351 with the rest.
- **What the trace counts.** It counts a sentence as asking him to change a field Hamlet set
  and still owns when the field holds the value Hamlet last left, and the tuned block's tune-in
  states that field. Requests on a field Hamlet left but no longer owns, once he has moved on,
  are counted apart and marked. Both columns are in section 3.
- **`ScriptedRadio` changes, test code only.** It answers `1C 00` when a test sets
  `Transmitting`, and the live meters when `AnswersMeters` is on. Both are off by default, so
  older tests see the radio as before.

## 2. What the owner should expect

Say you are listening to Morse in a CW block and the band starts overloading the radio. Once it
has held for about a second, Hamlet turns the preamp off itself, as the IC-7300 manual says for
strong signals. It tells you so behind the mark on the status line. The overload sentence on the
panel now says the preamp is off and why, instead of saying the mode only sets it when you tune
in. When the overload has been gone for about five seconds, Hamlet puts the preamp back to the
band's setting: preamp 1 on HF, preamp 2 on 6 m. If the overload comes straight back, it leaves
the preamp off until you next tune in. If you set the preamp yourself at any point, Hamlet leaves
it exactly where you put it until you next tune in, and says so. Hamlet never touches it while
you are transmitting. Nothing else on the radio is followed this way.

What will look wrong but is not: the radio's preamp may change without you touching it, a second
or so after an overload starts. Receive Help also stops telling you to open a receive gain that
is already fully open. It used to call full "about 39 percent".

## 3. What you should see

**1. Task 2's red messages, quoted, at `507b14da`.**
- `ThePreampFollowsTheOverloadTests`, 0 of 3:
  - `AtSevenThirtyAnOverloadThatHoldsTurnsThePreampOffOnce`: *Assert.Equal() Failure:
    Collections differ. Expected: int[] [0]. Actual: List<int> []*
  - `AtFourteenFiftyHisHandWhileItOverloadsStopsTheFollowing`: the same, on its first
    assertion. There was no off write for his hand to follow.
  - `NothingIsWrittenWhileTheRadioTransmits`: the same, on its second half. The rise during
    transmit wrote nothing at HEAD too, so the test also asks for the one write once receiving.
- `TheReceiveGainIsReadOnItsOwnScaleTests`, 0 of 2:
  - `AGainAtFullIsAlreadyOpenOnceHeHasMovedOn`: *Open the receive gain all the way. It is at
    about 39 percent, and a gain control turned down is the one thing that quietly undoes
    everything else.*
  - `AGainTurnedDownIsSaidAsThePercentTheRadioReads`: *Assert.Contains() Failure: Sub-string not
    found ... Not found: "about 50 percent"*
- `TheOverloadSentenceSaysWhereThePreampIsTests`, 0 of 2:
  - `WithThePreampOffItSaysItIsOffAsTheManualHasIt`: *Not found: "The preamp is off"*
  - `WithThePreampOnItSaysHamletTurnsItOffUnlessItIsHis`: *Not found: "turns it off"*

All 7 are green at exit. Task 3 added 4 more: the hold, the relapse, FT8's block, and an unread
transmit flag.

**2. The live-overload script at 7.030 and 14.050, before and after.** Both frequencies trace
identically. 7.030 is the QRP block and 14.050 is CW main street; both state all nine conditions.
- **Point 1, quiet tune-in, both runs:** written noise blanker 0, AGC 1, preamp 1; read back
  preamp 1. The setup says *"I set the preamp to preamp 1 because that is what the radio's own
  manual gives for this band and this front end."*
- **Point 2, overloading for 12 polls:**
  - *Before:* written nothing; read back preamp 1. The chip reads *overloading · preamp 1*. The
    overload sentence says *"... The preamp and the attenuator are set by this mode when you tune
    in, so Hamlet is not asking you to change them here."*
  - *After:* written preamp 0; read back off. Narrated: *"I turned the preamp off because the
    radio says its front end is overloading, and the radio's manual has the preamp off with
    strong signals."* The chip reads *overloading · preamp off*. The overload sentence says
    *"... The preamp is off, which is where the radio's manual has it while the front end is
    overloading. This mode sets the attenuator when you tune in, so Hamlet is not asking you to
    change either of them here."* Receive Help: *"The preamp is covered by what this mode states
    when you tune in, so Hamlet is leaving it out of these suggestions."*
- **Point 3, clear for 40 polls:**
  - *Before:* written nothing; read back preamp 1.
  - *After:* written preamp 1; read back preamp 1. Narrated: *"I set the preamp back to preamp 1
    because the radio's front end has stopped overloading, and that is what the radio's manual
    gives for this band."*
- **Point 4, he sets preamp 2, then overload for 12 and clear for 40:**
  - *Before and after:* written nothing; read back preamp 2 throughout.
  - *After:* narrated *"Your preamp is preamp 2 and I have left it there, because you moved it
    after I last set it."*
- **Moved on, at point 2:**
  - *Before:* Receive Help says the receive gain is at *"about 39 percent"*. The overload
    sentence says *"Press P.AMP/ATT on the front of the radio until the preamp reads off."*
  - *After:* Receive Help says *"The receive gain is already open all the way"* and *"The preamp
    is off, and the radio says its front end is overloading, which is when the radio's manual
    has it off."* The overload sentence says *"... the preamp is already off, so the next thing
    to try is the attenuator. Hold P.AMP/ATT for a moment to bring it in."* (Section 4, item 1.)
- **The 14.050 red test's own script,** a hand on the preamp while it is still overloading: one
  off write, then nothing through a 12-poll overload, a 40-poll clear and a 12-poll return.

**3. Per field, contradicting sentences before and after.** Six frequencies; in the block, moved
into a block that states nothing, and moved into FT8.

| field | Hamlet set and still owns: points 1 / 2 / 3 / 4 | left and no longer owns, moved on: 1 / 2 / 3 / 4 |
|---|---|---|
| auto notch, manual notch, noise blanker, noise reduction, AGC, squelch | 0 / 0 / 0 / 0 -> 0 / 0 / 0 / 0 | 0 / 0 / 0 / 0 -> 0 / 0 / 0 / 0 |
| RF gain | 0 / 0 / 0 / 0 -> 0 / 0 / 0 / 0 | 12 / 12 / 12 / 12 -> 0 / 0 / 0 / 0 |
| attenuator | 0 / 0 / 0 / 0 -> 0 / 0 / 0 / 0 | 0 / 0 / 0 / 0 -> 0 / 12 / 0 / 0 |
| preamp | 0 / 0 / 0 / 0 -> 0 / 0 / 0 / 0 | 0 / 12 / 0 / 0 -> 0 / 0 / 0 / 0 |
| **all nine** | **0 -> 0** | **60 -> 12** |

**4. The 7.8 table, at exit, in the block.** For all ten rows, overloading after the tune-in
reads the same: asked preamp 0; written 0; read back off. The follow's narration and the
owned overload sentence are the quoted ones in item 2.

| frequency | block | quiet: asked, written, read back | Receive Help, quiet and overloading | chip, overloading |
|---|---|---|---|---|
| 1.810, 160 m | **none on the map (P23)**: the app writes nothing; CW row driven directly | 1, 1, preamp 1 | covered by the mode (row driven directly) | overloading · preamp off |
| 3.530, 80 m | CW main street | 1, 1, preamp 1 | covered by the mode | overloading · preamp off |
| 7.030, 40 m | QRP watering hole | 1, 1, preamp 1 | covered by the mode | overloading · preamp off |
| 10.110, 30 m | QRP watering hole | 1, 1, preamp 1 | covered by the mode | overloading · preamp off |
| 14.050, 20 m | CW main street | 1, 1, preamp 1 | covered by the mode | overloading · preamp off |
| 18.080, 17 m | CW main street | 1, 1, preamp 1 | covered by the mode | overloading · preamp off |
| 21.050, 15 m | CW main street | 1, 1, preamp 1 | covered by the mode | overloading · preamp off |
| 24.900, 12 m | **none on the map (P23)**: the app writes nothing; CW row driven directly | 1, 1, preamp 1 | covered by the mode (row driven directly) | overloading · preamp off |
| 28.050, 10 m | CW main street | 1, 1, preamp 1 | covered by the mode | overloading · preamp off |
| 50.100, 6 m | **none on the map (P23)**: the app writes nothing; CW row driven directly | 2, 2, preamp 2 | covered by the mode (row driven directly) | overloading · preamp off |

- **Quiet, every row:** the setup's clause is *"I set the preamp to preamp N because that is
  what the radio's own manual gives for this band and this front end."* No observation mentions
  the preamp, and the overload sentence says nothing.
- **Overloading, every row:** the setup's clause becomes *"I turned the preamp off because the
  radio says its front end is overloading, and the radio's manual has the preamp off with strong
  signals."*
- **Full text:** every sentence is in
  `.run-unit/unit426-type-exit2-WhatHappensWhenTheBandOverloadsTests.txt`. Task 1's before trace
  is `.run-unit/unit426-type-t1-a-WhatHappensWhenTheBandOverloadsTests.txt`.

## 4. What's blocking us

1. **Once he has moved on, the overload sentence asks for the attenuator the CW tune-in left
   off. This bears on ticking 7.8. Parked as P27.**
   - **What happens:** with the preamp now off on an overload, the sentence outside an owned
     block reads *"the next thing to try is the attenuator. Hold P.AMP/ATT for a moment to bring
     it in"*. It appears 12 times in the trace, and it is true. Before this unit the same view
     asked him to turn the preamp off instead.
   - **Proposed ruling, the owner's:** 7.8's third clause covers the fields the tuned block owns.
     Once he is in a block that does not state a field, a true voice about it may speak.
   - **Reasoning:** HM-DEC-179 lets the preamp follow only while the block owns it, and forbids
     following the attenuator at all. The attenuator's 20 dB write is P14. The only way to
     remove the sentence without a ruling would be to silence a true voice, which R74 forbids.
   - **Rejected:**
     - following the attenuator live, which HM-DEC-179 forbids;
     - rewording the sentence to stop naming the attenuator, which would be silencing it.
2. **A band that starts overloading after he has moved into a block that does not state the
   preamp is not followed.** This is HM-DEC-179's third limit, as ruled.
   - **What happens:** the unowned overload sentence then asks him to press P.AMP/ATT until the
     preamp reads off. That is true and it stands.
   - **Proposed:** leave it as ruled unless Tim widens the licence.
   - Not blocking.
3. **Receive Help's USB level has the RF gain's scale mismatch. Parked as P28.**
   - **What happens:** `ReceiveAdvice.UsbLevel` compares a percent to 77 on the 0-255 scale, so
     50 percent, where the radio ships, would be asked to turn up.
   - It is not one of the nine fields, and it was not changed.
   - **Proposed:** a step 6 unit repairs it as this unit repaired the RF gain.
   - Not blocking.

**Carried per HM-DEC-139, verbatim. None of these is this unit's to answer:**

4. **P23 - 6 m has no block on the map, and its top edge is not in the tree.** "The CW row states
   preamp 2 from 50.000 to 54.000 MHz (HM-DEC-177), and `ReceiverSetup` writes 2 when driven there
   directly, but `HfBands.Names` is 80 to 10 m and `data/bands/us-neighborhoods.json` has no 6 m
   rows, so tuning to 50.100 in the app finds no block and writes nothing. 160 m and 12 m are the
   same (1.810 and 24.900 find no block). The 54 MHz top edge is 47 CFR 97.301's and no file in
   the tree carries it: `data/privileges/us-part97-privileges.json` has no 6 m row. Adding bands
   to the map is the scope decision `HfBands` names. Not blocking."
5. **P24 - the attenuator's sentence gives the quiet band's reason when it writes 20 dB.** "Since
   unit 424 the setup says a conditional row as the value the radio read back rather than as the
   rule. For the attenuator that will read *I set the attenuator to 20 dB because twenty decibels
   thrown away on a signal that had none to spare* once a 20 dB write lands, which gives the off
   case's reason for the on case. It cannot be heard today, because the 20 dB write is refused
   (P14). The row's `says` is the attenuator's, which is another receive condition and not this
   unit's; its page in `IC-7300_ENG_FM_12b` was not checked, because the manual is not in the
   tree. Not blocking."
6. **P25 - the decision log's index has no HM-DEC-166 row.**
   "`DecisionLogOrderTests.EveryRulingAppearsOnceAndTheGapsAreTheKnownOnes` is red at entry
   `e4085d43` and after: *Expected [105, 136], Actual [105, 136, 166]*. `DECISIONS.md` holds
   HM-DEC-166 and the `CLAUDE.md` §1 table has no row for it. Unit 424 did not repair it (report,
   repair nothing)." Still red at unit 426's entry and exit, and not repaired.
7. **P26 - a ceiling test's added paragraph comes back 64 characters longer at some times.**
   "`HowMuchTheApplicationSaysTests.AddingASentenceToACappedSurfaceTurnsItRed` failed twice ...
   with *Expected 1779, Actual 1843* ... Something on the Digital tab adds 64 characters between
   the two measurements at some wall-clock times. Not traced by unit 424. Not blocking." It was
   5 of 5 in both of unit 426's rounds.
8. **Step 3's join question, from unit 425.** "3.6 clause two may need a join, and a join moves
   the unkeyed floors ... A ruling would be needed on whether that counts as a floor lowered on
   an unkeyed row." It is step 3's.
9. **P19** was answered by HM-DEC-178 at unit 425. It is step 3's and is listed only so the
   carry is complete.
