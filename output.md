## 1. What Claude did

**Surface and gate.** Claude Code on the development computer at `C:\Source\HamLet`, branch
`main`. The prompt carries `PROJECT: Hamlet`, and all five of section 0's checks hold. Hamlet
confirmed. Nothing in this report is evidence about the radio.

**Run by hand, outside the loop.**
- `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released the same way.
- Nothing was written to `RUN_LEDGER.md`, and nothing under `tools\arbiter\` was touched.
- No box was ticked; `PHASE_PLAN.md` has 69 before and after.
- No recording, fixture, floor or telemetry was read.
- Nothing under `.run-unit\` was committed.

**The two reds are still red, and the cause is not the one section 2 named.**

- **Unit 490's finding did not reproduce.** On unit 490's own detector, the call with bursts in its
  gaps has **every one of its 65 marks called**, the same as the clean call. This is measured by
  matching each of the clean call's marks to an overlapping mark in the case.
- **Where the letters go wrong: marks arrive late.** The L's third dit (10.520 to 10.565 s) was
  handed out at 11.000 s, 435 ms after it ended. By then the run reader had ended the L's run, so
  the L split into `E` and `I`.
- **Why they arrive late:** the check that paired bars stand clear of their gaps' wander is
  measured over the bin's last second. A burst anywhere in that second holds pairs back until it
  leaves the window.

**The check section 3 asked me to remove stays, because it does real work.**
- It lives in `src/Hamlet.RadioEngine/Cw/CwEnvelopeDetector.cs`, in `Evaluate`:
  `if (!dropped || lower - waves.Level <= waves.Wander)`, at line 963 now.
- It is there to refuse noise's chance flat runs, which stand no clearer of their gaps than those
  gaps wander.
- **Removed:** thirty seconds of loud noise alone called 61 marks and printed `NETMITT`, and the
  clean call misread (`N0DEALL`).
- **Narrowed to each pair's own gap** instead of the last second: the clean call still misread,
  and the second station put more marks at the call's pitch (11, where the unchanged detector puts
  3 or 4).
- **Both were reverted.** The instruction said that if the guard does real work, the report says
  so rather than forcing the other two green.

**What changed** (in `016131ae`):
- **`src/Hamlet.RadioEngine/Cw/CwEnvelopeDetector.cs` - a bar pairs with the nearest bar at its own
  level.**
  - Counters on the pairing, at the call's bin, showed the bursts multiplying one rejection tenfold:
    bars refused for disagreeing on level went from 2,391 to 23,901.
  - The cause: a burst makes a short bar of its own in the call's bin. A bar used to be compared
    only with the bar just before it, so each of the call's bars was compared with the burst and
    refused.
  - Now a bar looks back to the nearest bar at its own level, within the same second as before. A
    bar at another level between them is part of their gap.
  - **Unchanged:** the agreement asked of two bars (within 1.5 dB, each at least a dit long), the
    check that their gap dropped below both, the wander check, the flatness tolerance and the
    shortest bar.
- **`tests/Hamlet.RadioEngine.Tests/Cw/ACharacterIsARunOfMarksThatAgreeTests.cs`:**
  - `NoiseAloneReadsNothing`, new: loud noise with no station prints nothing.
  - `HowManyOfTheCallsMarksAreCalled`, new: prints how many of the clean call's marks each case
    calls. It asserts nothing.
  - The two reds' remarks now give the measured cause.
- **Records.**
  - R104 is in both `PHASE_PLAN.md` copies, in the owner's words.
  - `DECISIONS.md` has HM-DEC-196 under the instruction's headline. It records that unit 490's
    finding did not reproduce, and what was measured instead.
  - Both outcome and status copies name 491.
  - Version 1.13.177 → 1.13.178.
- **Section 4, the panel's pitch, was not done.** The reader runs on the audio thread, and reading
  its printed sender from the screen's thread cleanly needs locking. That is more than "clean and
  quick".

**Verification.**
- The build: 0 warnings, 0 errors.
- **The app carry-forward line: 275 of 278.** The three failures ran in 1 ms each, the
  dispatcher-loop loss: `TheRstIsYoursToCorrectTests` and two in `TheFavoritesAreUnderTheGreenZoneTests`.
  All pass alone (4 of 4 and 3 of 3).
- **The app scope, layout and voice types: 84 of 87.**
  - Two `TheTopRowTests` names failed in the long run and pass alone, 15 of 15.
  - The British spelling red is from 2026-09-26, not this unit.
- **The engine run, gate and detector types: 25 of 30.** The reds:
  - `TheCallReadsWholeThroughTheBlips` and `TheStationPrintedReadsWhole`.
  - `ThatPitchIsTheStationsOwn`.
  - `AMarkIsTheEnvelopeOverAThresholdTests`: 70 and 208, where it was 40 and 208 (see section 2).

## 2. What the owner should expect

1. Rebuild.
2. **A station whose gaps have noise in them reads better than yesterday, but not yet whole.**
   Where a burst of noise sits in a gap, the letters beside it are right more often. A letter can
   still split in two when the noise delays one of its dots.
3. Noise with no station still prints nothing.
4. Two stations at once still read as one.
5. **One cost.** On a weak station, about 15 dB over the noise, the scope can now switch to a
   neighboring frequency and miss a dash there. On the test tone, 70 hops of key-down were missed
   where 40 were before. This is the scope's picture, not the terminal's letters.

## 3. What you should see

| case | sent | read before (unit 490) | read now |
|---|---|---|---|
| clean call | `CQ CQ DE N0CALL N0CALL K` | `CQ CQ DE N0CALL N0CALL K` | `CQ CQ DE N0CALL N0CALL K` |
| the call with bursts in every gap | `CQ CQ DE N0CALL N0CALL K` | `CQ CT A K EE N0CAAEI D N0CALL K` | `CQ CQ DE N0CAAEI D N0CALAI K` |
| the call plus `TEST DE W1AW K` 200 Hz away | the call, one station | `CGE CEN K EE N0 FALL N0CALL K` | `C RE CENT DE N0 FALL N0CALL K` |
| loud noise, no station | nothing | nothing | nothing |

**The call's marks called, of the clean call's 65:**

| case | before | after |
|---|---|---|
| clean call | 65 | 65 |
| the call with bursts | 65 | 65 |
| two stations | 62 (and 3 others at the call's pitch and level) | 61 (and 4 others) |

In the two-station case, every mark the printed letters were read from is the call's, before and
after.

## 4. What's blocking us

Nothing blocks. What is left, a line each:
- **The run reader ends a run before all its marks have arrived.**
  - The detector can call a mark up to about a second late, because a pair waits on the wander
    check and on its partner.
  - The reader waits only a character gap plus its longest mark. It should hold a run open until
    the detector can no longer add a mark to it, and put a late mark in its place.
  - That is in the reader, which this unit was told not to touch. It is what turns
    `TheCallReadsWholeThroughTheBlips` green.
- **In the two-station case, 4 of the call's marks are not called at all**, where the answer keys
  inside the call's lobe.
- **The weak-tone cost:** on the 15 dB case, 70 key-down hops missed on the scope against 40,
  because a second bin now counts as keying.
- **Section 4 is not done:** "decoding at N Hz" is still the old path's mixing pitch, and needs the
  printed sender's pitch read across threads.
- **Pre-existing reds, not this unit's:**
  - `VoiceTests`' British spelling.
  - `HowMuchTheApplicationSaysTests`.
  - `ModeFollowsTheMapAgainTests.NothingButTheModeIsEverWritten`.
  - `ThatPitchIsTheStationsOwn`.
  - `AMarkIsTheEnvelopeOverAThresholdTests`' two cases.

### Asks still outstanding

- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25, waiting on
  the owner; no change sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8
  record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, so nothing on it is ever
  revised, at the cost of seconds of lag. On the run path the terminal already shows only settled
  text. The ask is still the owner's for the timing-only path, and no change for it sits in the
  tree.
- **Unit 491, new, 2026-09-28:** whether to keep the change to how bars pair. It reads the call
  with bursts better and costs the 15 dB tone's scope picture 30 missed hops. The change sits in
  `CwEnvelopeDetector.Evaluate` in `016131ae`. Reverting it restores yesterday's detector exactly.
