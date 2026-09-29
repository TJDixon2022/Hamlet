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

**The changes, file by file** (all in `f73dce1b`):

- **`src/Hamlet.RadioEngine/Cw/CwEnvelopeDetector.cs` - a mark is handed out when it ends.**
  - Every completed bar in every bin, paired or not, is a mark one envelope window (10 ms) after
    it ends. It used to wait to be paired, and pairing waited on a second of gap history.
  - Pairing, the wander check and unit 491's nearest-bar-at-its-own-level change are untouched.
    They still drive the keying verdict, the light and the scope, whose test counts did not move.
  - Handing out bars unpaired exposed three things the pairing had been hiding. Each is now a rule,
    stated in the code:
    - **A mark ends where its tone ends.** One window after the bar, the peak of its lobe must have
      dropped below it by more than the flatness floor (1.5 dB). Without this, a shoulder bin's
      piece of a dah that stops while the tone goes on was handed out first, and the whole dah was
      refused as already called. The clean call's first dah came out as two 20 ms marks.
    - **A mark begins where its tone rose.** The key-down edge can join the front of a dah's top
      as a run of its own and split it, so the bar that ends with the tone may be only its back
      half. The mark reaches back over the peak's hops at its level, within the flatness floor.
      Traced on Q's first dah: its rising-edge hop at −11.0 dB split the top at hop 750.
    - **A mark already called stands in for a new one only at the same level**, within twice the
      flatness floor. An edge fragment 20 dB under a dah is not the dah.
  - **A bar is handed out no later than three windows after it ends.** A bar found later, when a
    bin's history is re-read at a newly measured contrast, was not a bar when it ended. Without
    this bound, marks arrived up to 4.9 s late.
  - **Not touched:** the flatness tolerance, the shortest bar, the pitch-peak walk and the pairing
    agreement. The flatness floor is used as a threshold; its value is unchanged.
- **`src/Hamlet.RadioEngine/Cw/CwRunReader.cs` - the noise guard moves to where letters are made.**
  - A sender is printed only after two runs of **two marks or more**, with dits and dahs among
    them (at least 2 to 1).
  - A run of one mark never counts toward a sender.
  - A sender that never gets there is forgotten after a second of silence. Before that, loud noise
    alone would pile up senders forever.
  - **Once a sender is printed, its one-mark letters print with it.** I read "a run of one mark
    makes no letter" that way because the literal reading would print every `DE` as `D`, and case
    1 requires the call to read whole.
- **`tests/Hamlet.RadioEngine.Tests/Cw/ACharacterIsARunOfMarksThatAgreeTests.cs`:**
  - `ALoneDitOrDahPrintsNothing`, new.
  - `HowLateAMarkIsHandedOut`, new; a printer that asserts nothing.
  - The call's reference marks are now picked out of all the marks, within a bin of 625 Hz and
    6 dB of the loudest. The noise now hands out marks too, and the old reference, the median of
    all marks, landed at the noise level. That is a test-reference fix, not a tolerance change.
- **`PARKED.md`:** the owner's answer to unit 491's ask - **keep the pairing change** - is one
  `RESOLVED` line.
  - The file's header says sessions never write to it. The order asked for it, so I followed the
    order.
  - Only that line was staged. The loop's own uncommitted line (11.6) was left as it was.
- **Records.**
  - R105 is in both `PHASE_PLAN.md` copies, and `DECISIONS.md` has HM-DEC-197.
  - Both outcome and status copies name 492.
  - Version 1.13.178 → 1.13.179.

**Watched failing first.** On the detector as unit 491 left it:
- `TheCallReadsWholeThroughTheBlips` and `TheStationPrintedReadsWhole` were red.
- Noise alone and the lone dit and dah were green.
- Worst delays: 85, 445 and 710 ms.

The intermediate versions and what each did are in section 3.

**Verification.**
- The build: 0 warnings, 0 errors.
- **The app carry-forward line: 276 of 278.** The two failures ran in 1 ms each, both in
  `ThePowerIsOfferedTests`, and pass alone (3 of 3).
- **The app scope, layout and voice types: 86 of 87.** The red is the British spelling from
  2026-09-26, not this unit.
- **The engine run, gate and detector types: 30 of 33.** The reds are unchanged:
  - `ThatPitchIsTheStationsOwn` at 1024.
  - `AMarkIsTheEnvelopeOverAThresholdTests` at 70 and 208.

## 2. What the owner should expect

1. Rebuild.
2. Letters no longer split because a dot arrived late. The blocks and the letters should appear
   within a fraction of a second of the sending, and a letter prints once the gap after it has
   passed.
3. Noise with no station still prints nothing.
4. A single lone dit or dah prints nothing, on purpose. A station has to send at least two
   letters of two or more elements, dits and dahs both, before anything prints. Its first letters
   then appear together.
5. Two stations at once still read as one.
6. **Unchanged:** the light, the scope, the layout, the preamp, and text that stays printed.

## 3. What you should see

| case | sent | unit 491 | now |
|---|---|---|---|
| 1. the call with bursts in every gap | `CQ CQ DE N0CALL N0CALL K` | `CQ CQ DE N0CAAEI D N0CALAI K` | `CQ CQ DE N0CALL N0CALL K` |
| 2. loud noise, no station | nothing | nothing | nothing (1,457 marks handed out, none printed) |
| 3. the call plus `TEST DE W1AW K` 200 Hz away | the call | `C RE CENT DE N0 FALL N0CALL K` | `CQ CQ DE N0CALL N0CALL K` |
| 4. a lone dit; a lone dah | nothing | nothing | nothing |
| clean call | `CQ CQ DE N0CALL N0CALL K` | the same | the same |

**Worst delivery delay, from a mark's end to when it is handed out:**

| case | unit 491 | now |
|---|---|---|
| clean call | 85 ms | 15 ms |
| the call with bursts | 445 ms | 15 ms |
| two stations | 710 ms | 15 ms |
| noise alone | none handed out | 15 ms |

**The call's own marks called, of 65:** 65, 65 and 65 on the three call cases (unit 491: 65, 65
and 61). Every mark the printed letters were read from is the call's.

**The intermediate versions, in order:**
1. **Every bar handed out, nothing else changed:** noise alone printed 52 letters, and marks
   arrived up to 4.9 s late.
2. **Plus the reader's guard and the three-window bound:** noise printed nothing, but the clean
   call read `EEETE THE TETE…` because shoulder pieces stood in for dahs.
3. **Peak bars only:** the whole dahs from the shoulder bins were lost.
4. **The key-up check instead:** `…N0CALL NE`, the last K split at a dah's rising edge.
5. **Plus the reach-back start:** the version committed.

## 4. What's blocking us

Nothing blocks. What is left, a line each:
- **A station now needs two multi-element letters with dits and dahs both before anything
  prints.** A call like `EEEE`, or one letter alone, will not print.
- **The detector now hands out many noise marks:** about 50 a second on loud noise across the
  whole band. The reader forgets them after a second, but on a slow machine the cost is worth
  watching.
- **The panel's "decoding at N Hz"** is still the old path's mixing pitch.
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

Unit 491's ask - keep the pairing change - is answered: keep it (`PARKED.md`).
