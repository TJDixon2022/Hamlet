READ IN THIS ORDER.

A. The phase goal, Hamlet reads a CQ call correctly, and what this unit changes under it: what
   the preamp is set to on each band now, and every sentence the app says about it, before and
   after. Section 3 leads with both.
B. Step 7 and its exit criteria: 7.8 clause by clause, and 7.6 at exit. Section 1, under
   "Criterion 7.8, clause by clause" and "Exit round".
C. What this report adds beyond that: the survey work instruction 424 section 5 asked for, the
   author's decisions, and section 4. Section 4 raises 5 items, none blocking. One of them, the
   overload case read only at the tune-in, bears on 7.8's second clause and is why 7.8 is left
   unticked; none bears on A.

UNIT:       424 - complete at task 4 of 4, none dropped - 2026-09-24 18:53
PHASE GOAL: Hamlet reads Morse CQ calls as they were sent, scored against a key, and Tim confirms it at the radio.
UNIT GOAL:  The CW preamp is set to what the IC-7300 manual says for the band and the front end, and nothing on screen tells him to change what Hamlet set.
ADVANCED:   yes - the preamp on 80 and 40 m goes from off to preamp 1 as the manual says, and sentences contradicting what Hamlet set fall from 28 to 0, measured by one trace before and after
NUMBER:     sentences contradicting what Hamlet set: 28 -> 0
DRIFT:      0

## 1. What Claude did

**Complete, 4 of 4 tasks, nothing dropped.** Machine QUIVERFULL, project Hamlet confirmed by the
gate, branch main, entry e4085d43, exit c2b0cdf9 plus this report's commit. Every commit pushed,
`push rc 0` each time.

**Task 0, the record (a97a5bd2).** DECISIONS.md and the CLAUDE.md section 1 row carry the ruling,
PHASE_OUTCOME.md has `## UNIT 424 - STEP 7`, PHASE_STATUS.md names unit 424 and
`CURRENT_STEP: 7`, and the version went from 1.13.110 to 1.13.111. **Mismatch: the instruction
names the ruling HM-DEC-176, but DECISIONS.md already holds HM-DEC-176 for unit 421's floor-bar
ruling.** DecisionLogOrderTests refuses a repeated id, so I recorded the ruling as **HM-DEC-177**,
said so inside the record, and kept the instruction's own words on the PHASE_OUTCOME LICENCE line.

**Task 1, the trace (88d7bd90).** `WhatIsSaidAboutThePreampTests` is a fact that asserts
nothing. It drives the real `ReceiverSetup` against `ScriptedRadio` with each block's own
conditions, as `EstablishReceiveConditionsAsync` does, at 1.810, 3.530, 7.030, 10.110, 14.050,
18.080, 21.050, 24.900, 28.050 and 50.100 MHz, quiet and then overloading. For each it prints,
word for word, what six things say about the preamp: the setup's own sentence, Receive Help's
advice, the diagnostics observations, the obstructions line, the panel's front-end chip and the
panel's overload sentence. It does that right after the tune-in and again once he has moved into
a block that does not state the preamp (no field owned, then FT8's fields owned). It also counts
the contradicting sentences. `ScriptedRadio.cs` and `ModeEntryBench.cs` are linked into the app
tests rather than copied.

**Task 2, the condition (23b1b843).** Red first: `ThePreampIsWhatTheManualSaysTests` failed 10 of
17. At 7.030 the message was *Collections differ, Expected: [1]*, with nothing written, because
the old row left a radio at off. Then:
- The CW preamp row now carries `bands`: 1,800,000 to 29,999,999 Hz wants 1, and 50,000,000 to
  54,000,000 Hz wants 2, each with its source. It also carries `whenOverloading: 0`.
- `wantedText` reads *preamp 1 from 1.8 to 29.999 MHz, preamp 2 at 50 MHz, and off while the
  front end reads overloading (IC-7300 manual IC-7300_ENG_FM_12b, page 4-3)*. `because` cites
  the page as well.
- `ReceiverSetup.ResolveAsync`'s `band` rule, unit 419's mechanism, now reads the overload flag
  first and then the row's stretches, in place of the old `hz > 10_000_000 ? 1 : 0`. The QRP and
  CW DX blocks get the same row through their `sameAs` lines, as unit 420 left them.

After the change it is 17 of 17. Three Rig facts pinned the superseded *off at 40 m*, and each
now reads the row instead:
- `EveryMorseBlockSetsWhatCwSetsTests.At7030ThePreampEndsOff` is now `...EndsAtTheRowsValue`.
- `ThePreampFollowsItsOwnTextTests.At7030...` now expects preamp 1.
- `TheOperatorsHandCrossesTheMorseBlocksTests` on 40 m now has his hand at off, against the row's 1.

`ModeEntryBench.AlreadyRightForCw` reads the row too. The Rig round went 339 of 342 before those
three moved, then 45 types all green.

**Task 3, one voice (c2b0cdf9).** Red first: `OneVoiceOnThePreampTests` failed 15 of 15. At 14.050
the message was *Assert.Empty() Failure, Collection: ["off", "preamp 2"]*: the setup wrote preamp
1, then said *I set the preamp to preamp 1 from 1.8 to 29.999 MHz, preamp 2 at 50 MHz, and off
while ...*. Before task 2, the same clause at 14.050 said *...off at 40 m and below because below
40 m the noise arrives with the signal and gain adds both*. **That clause is the voice unit 419
missed**, and it is the one that tells him the preamp should be off. Three fixes, none of which
silences a voice; each one now says what is true:
- `ReceiverSetupVoice.Did` says a conditional row as the value the radio read back: *I set the
  preamp to preamp 1 because that is what the radio's own manual gives for this band and this
  front end.*
- `ReceiveAdvice.Preamp` no longer proposes *Switch the preamp on* while the front end reads
  overloading, because the manual has it off then. It now says *The preamp is off, and the radio
  says its front end is overloading, which is when the radio's manual has it off.*
- `MainWindowViewModel.OverflowAdviceFor` takes the attenuator's state and no longer asks him to
  hold P.AMP/ATT for an attenuator that is already in. `AttenuatorIsIn` is wired from the rig
  state.

After the fixes it is 16 of 16, including a direct fact for the attenuator sentence.

**Decisions I made for myself, all overrulable:**
- **How the value is carried.** It uses unit 419's `band` rule, with the stretches and the
  overload value moved into the data file (§0: data with sources, not a literal in the resolver).
- **Overload.** The reading used is `RigField.Overflow`, `CivReads.Overflow`, `15 07`, page 19-3.
  It is polled Live and printed on the capture sheet as `Overflow`. It is read at the tune-in,
  exactly as the attenuator row already reads it. An unread flag writes nothing, and a frequency
  outside every stretch writes nothing.
- **The 54 MHz top of the 6 m stretch** comes from 47 CFR 97.301 as I know it. No file in the
  tree carries it. It is marked as uncited in the stretch's own `source`, and parked as P23.
- **The count's rule was narrowed in task 3.** A P.AMP/ATT sentence counts only when it asks him
  to move the preamp or the attenuator away from where the setup left it. Four of task 1's 32
  were *Hold P.AMP/ATT ... to bring it in* at 3.530 and 7.030 while overloading. Those ask for the
  attenuator the row wanted and the radio refused (P14), so they are not contradictions. Read
  from task 1's committed output, the before figure under the narrowed rule is 28.
- **7.8 is not ticked.** See the clauses below.

**The survey section 5 asked for.**
- **The manual is not in the tree.** No `IC-7300_ENG_FM_12b` file is in the repository, so I took
  section 4's quotations as given and put the citation in the row either way.
- **The CW preamp condition at entry:** `"wanted": 1`, `wantedText` *preamp 1 above 40 m, off at
  40 m and below*, `confirmed: true`, `"condition": "band"`. The band rule lived in
  `ReceiverSetup.ResolveAsync` as a 10 MHz literal; the file could not state a range.
- **Components that name the preamp.**
  - Writes on a tune-in: `ReceiverSetup`, through `CivWrites.Preamp`.
  - Writes when he presses Receive Help: `ReceiveAdvice`.
  - Speak: `ReceiverSetupVoice`, the setup's own sentence, which was not scoped by 419 and was
    wrong. Also `ReceiveAdvice` (Receive Help), `RigObservations.AttenuatorAndPreampTogether`
    (diagnostics), and `MainWindowViewModel.OverflowAdviceFor` and `FrontEndTextFor` (the
    panel); 419 scoped those three.
  - Show the value only: `RigReadout`, `DigitalCaptureSheet`, `OwnedSettings`, `CivDecode`.
  - `ReceiveObstructions` names the preamp only in a comment.
- **Overload indicator:** yes, `RigField.Overflow`, as above.
- **Can the file state a value that depends on frequency?** Not before this unit. Now it can,
  through `bands` and `whenOverloading`.

**Criterion 7.8, clause by clause.**
- *The CW conditions carry preamp 1 for 1.8 to 29.999 MHz and preamp 2 at 50 MHz, with the page
  cited in the condition's own text:* **met.** It applies to every CW-family block.
- *Off when the receiver reports overloading rather than by band:* **met at the tune-in; not
  followed after it.** If the band starts overloading after he has tuned in, nothing changes the
  preamp. Following the flag live would write outside a tune-in, against R67 and HM-DEC-056, so I
  parked it as P22.
- *No component asks him to change the preamp, or any other field a condition states, after
  Hamlet has set it:* **met for the preamp, 28 to 0.** The same holds for the attenuator in the
  overload sentence. Other fields were covered inside the block by unit 419, but this unit did
  not re-survey them once he has moved on.
- *Watched failing at 7.030 and 14.050:* **met.** The red messages are quoted above.
- *The report prints a frequency on each HF band and at 50 MHz:* **met.** It is in section 3.
- **Not ticked.** The live overload case and the other fields once he has moved on are the two
  open edges. Whether they are enough to hold 7.8 open is the arbiter's call.

**Exit round (7.6 holds).**
- Hamlet.sln builds non-incremental with warnings as errors: 0 warnings, 0 errors.
- Engine carry-forward 178 of 178. App carry-forward 278 of 278.
- Captures 51 of 51. Adjudicated 13 of 13. Keyed floors 13 of 13.
- Every Rig type: 45 types, 342 of 342. At entry it was 44 types, 325 of 325; the new type
  is `ThePreampIsWhatTheManualSaysTests`.
- Sheet tests: `TheSidecarIsReReadTests` 2 of 2 (411), `TheTonePeakIsAboutThisRecordingTests`
  3 of 3 (417), `TheRestOfTheSheetIsTrueTests` 10 of 10 and `TheKeyingCaptionNamesTheSweepItRanTests`
  2 of 2 (418).
- Touched app types all green: `WhatIsSaidAboutThePreampTests`, `OneVoiceOnThePreampTests` 16,
  `TheOverloadSentenceLeavesTheModesFieldsTests` 4, `TheFrontEndIsOnThePanelTests` 6,
  `HowMuchTheApplicationSaysTests` 5, `TheStatusBarStopsLecturingTests` 4,
  `WhichPathsPutNarrationOnTheBarTests` 6, `WhatElseIsComposedAtRuntimeTests` 1,
  `RigDiagnosticsTests` 10, `BindingHealthTests` 1.
- `DecisionLogOrderTests.EveryRulingAppearsOnceAndTheGapsAreTheKnownOnes` is **red at entry and
  at exit, the same way**: *Expected [105, 136], Actual [105, 136, 166]*. The CLAUDE.md index has
  no HM-DEC-166 row. Reported, not repaired (P25).
- `src/Hamlet.RadioEngine/Cw` prints nothing against entry. The transmit files print nothing
  against 7e209cb4.
- Nothing is red at exit that was green at entry.
- `HowMuchTheApplicationSaysTests.AddingASentenceToACappedSurfaceTurnsItRed` failed twice
  mid-task (*Expected 1779, Actual 1843*). It then passed on the task 2 tree and on the same
  source that had failed, and passed at exit. Something time-dependent appears on the Digital
  tab; I parked it as P26.

## 2. What the owner should expect

When you tune into Morse on 40 m, Hamlet sets the preamp to preamp 1. On 20 m it sets preamp 1.
That covers every HF band from 1.8 to 29.999 MHz, which is where Icom's manual (page 4-3) and its
published sensitivity figures put preamp 1. On 6 m the row says preamp 2, but Hamlet has no 6 m
on its band map yet, so tuning to 50.100 sets nothing at all until 6 m is added. If the radio
says its front end is overloading at the moment you tune in, Hamlet turns the preamp off, which
is the manual's case for off. If the band only starts overloading after you have tuned in,
Hamlet does not change it by itself yet; that is waiting on a ruling. Nothing on screen asks you
to change the preamp Hamlet set. The only sentence about it is Hamlet's own: *I set the preamp to
preamp 1 because that is what the radio's own manual gives for this band and this front end.*

**What will look wrong but is not.** On your first Morse tune-in on 80 or 40 m after this build,
Hamlet will switch the preamp from off to preamp 1. That is the fix, not a regression. And while
the radio reads overloading with the preamp already off, the panel may still say *hold P.AMP/ATT
to bring the attenuator in*. That sentence is true: Hamlet asked for the attenuator and the radio
refused the byte Hamlet sent (P14, parked since unit 419).

## 3. What you should see

**Sentences contradicting what Hamlet set: 28 -> 0.** That is 10 -> 0 right after the tune-in,
and 18 -> 0 once he has moved into a block that does not state the preamp. The count covers the
seven frequencies on the map, quiet and overloading. Task 1's first count, under a stricter
rule, was 32; section 1 explains why 4 of those were not contradictions.

**What the preamp is set to, before and after.** Each tune-in starts from a radio left at preamp
off. The table is task 1's trace as committed at 88d7bd90, beside the same trace at exit.

| Frequency | Block the app finds | Quiet, before | Quiet, after | Overloading, before | Overloading, after |
|---|---|---|---|---|---|
| 1.810, 160 m | none, the app writes nothing | off | preamp 1 | off | off |
| 3.530, 80 m | CW main street | off | preamp 1 | off | off |
| 7.030, 40 m | QRP watering hole | off | preamp 1 | off | off |
| 10.110, 30 m | QRP watering hole | preamp 1 | preamp 1 | preamp 1 | off |
| 14.050, 20 m | CW main street | preamp 1 | preamp 1 | preamp 1 | off |
| 18.080, 17 m | CW main street | preamp 1 | preamp 1 | preamp 1 | off |
| 21.050, 15 m | CW main street | preamp 1 | preamp 1 | preamp 1 | off |
| 24.900, 12 m | none, the app writes nothing | preamp 1 | preamp 1 | preamp 1 | off |
| 28.050, 10 m | CW main street | preamp 1 | preamp 1 | preamp 1 | off |
| 50.100, 6 m | none, the app writes nothing | preamp 1 | preamp 2 | preamp 1 | off |

Rows with no block are the CW row driven directly. They show what entering CW there would do;
the app does nothing there today (P23).

**What is said, word for word.**
- **Before**, at 14.050 quiet, right after the tune-in, the setup said: *I set the preamp to
  preamp 1 above 40 m, off at 40 m and below because below 40 m the noise arrives with the signal
  and gain adds both.* The same clause appeared at every frequency above 10 MHz.
- **Before**, at 7.030, once he had moved into an FT8 block or a block that states nothing,
  Receive Help said: *Switch the preamp on. It is more gain at the front end, which is what a
  faint signal needs.* That was after Hamlet had left the preamp off.
- **Before**, at 14.050 overloading, once he had moved on, the panel said: *... Press P.AMP/ATT on
  the front of the radio until the preamp reads off.* That was after Hamlet had set preamp 1.
- **After**, at every frequency on the map, the setup says: *I set the preamp to preamp 1 because
  that is what the radio's own manual gives for this band and this front end.* At 50.100, driven
  directly, it says preamp 2.
- **After**, overloading, the setup says nothing because the preamp was already off. Inside the
  block, the panel says: *The radio says its front end is overloading, ... The preamp and the
  attenuator are set by this mode when you tune in, so Hamlet is not asking you to change them
  here.* Once he has moved on, Receive Help says: *The preamp is off, and the radio says its front
  end is overloading, which is when the radio's manual has it off.*
- The diagnostics observations and the obstructions line say nothing about the preamp, before or
  after.

The full before and after traces are in `.run-unit/unit424-trace-before.txt` and
`.run-unit/unit424-type-exit-WhatIsSaidAboutThePreampTests.txt`.

## 4. What's blocking us

Nothing blocks the phase. Five items, most relevant first.

**The preamp follows the overload flag between tune-ins, or only at them (P22).** Proposed
ruling: the preamp follows the flag only at the tune-in, as the attenuator does, and the in-block
overload sentence says that the band has begun overloading since Hamlet set the front end.
Reasoning: R67 and HM-DEC-056 make a tune-in the one moment Hamlet writes the receive side, and
his hand wins after it. A live rule would write while he is listening and would fight his own
hand. Rejected: turning the preamp off live, which is a new writer outside a tune-in and needs
Tim's word; leaving the in-block sentence as it is, which says Hamlet is not asking him to change
a setting the manual would have off.

**6 m, 160 m and 12 m have no blocks on the map, and 54 MHz is not in the tree (P23).** Proposed
ruling: a band joins the map only with its edges carried in `data/privileges` from 47 CFR 97.301,
and until then the preamp row's 6 m stretch stays stated and unreached. Reasoning: §0.2.1 forbids
frequencies from a model's memory, and `HfBands` names the map's bands as a scope decision.
Rejected: adding 6 m to the map inside this unit.

**The decision log has two numbering faults (P25).** Proposed ruling: add the missing HM-DEC-166
row to the CLAUDE.md index from DECISIONS.md, and confirm HM-DEC-177 as this unit's id.
Reasoning: DecisionLogOrderTests has been red on the missing row since before this unit, and the
instruction's HM-DEC-176 was already taken. Rejected: renumbering unit 421's ruling.

**The attenuator's sentence will give the quiet band's reason when a 20 dB write lands (P24,
behind P14).** Proposed ruling: the attenuator's `says` is rewritten to be true of both values
when P14's byte is fixed, cited to the manual's page. Reasoning: the setup now says the value the
radio read back, so a reason that fits only the off case becomes a contradiction once 20 dB can
be set. Rejected: changing it now, which is another receive condition and outside this unit.

**A ceiling test fails at some wall-clock times (P26).** Proposed ruling: trace which Digital-tab
text appears between its two measurements, and pin it as unit 302 pinned the card's clock.
Reasoning: it failed twice and then passed twice on the same source, 64 characters apart.
Rejected: widening the tolerance.
