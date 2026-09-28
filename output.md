## 1. What Claude did

**Surface and gate.** Claude Code on the development computer at `C:\Source\HamLet`, branch
`main`. The prompt and `WORK_INSTRUCTIONS.md` both carry `PROJECT: Hamlet`, and all five of
section 0's checks hold. Hamlet confirmed. Nothing in this report is evidence about the radio.

**Run by hand, outside the loop.**
- `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released the same way.
- Nothing was written to `RUN_LEDGER.md`, and nothing under `tools\arbiter\` was touched.
- No box was ticked; the checkbox count is 42 before and after.
- No recording, fixture, floor or telemetry was read.

**The changes, file by file** (all in `fec08bba`):
- **`src/Hamlet.RadioEngine/Cw/CwDecoder.cs` - the screen tells the truth.** The gate now judges
  each character by the detector at the moment it would reach a surface. It must be keying now,
  and the character must have been heard in the stretch that is keying now. When keying goes
  false, the terminal's provisional tip is cleared in the same moment. Unit 485 judged by when
  the audio was heard, which is what let letters land under a panel saying no keying.
- **`CwDecoder.cs` - the decoder listens where the detector hears.** A new `DetectorPitch` is the
  second rung of the mixing pitch, after the operator's lock and before the tracker. That is unit
  477's task 2, never built. The lattice, the unit estimator and the emission gate are untouched.
- **`src/Hamlet.App/ViewModels/MainWindowViewModel.cs`** sets `DetectorPitch` to the detector's
  watched pitch while it says keying, and NaN otherwise. The unit 485 hold keeps that pitch
  through a station's gaps.
- **`data/bands/mode-receiver-conditions.json` - R98.** The CW preamp row is now off (0) on every
  band, confirmed, with no band rule and no overload rule. Its text cites R98 and HM-DEC-191 and
  keeps the manual's page 4-3 reasoning as history. The data rows are unchanged.
- **`src/Hamlet.RadioEngine/Rig/ReceiverSetup.cs` - his hand first.** The tune-in now asks whether
  he moved the setting by hand before it accepts the setting as already right.
- **`src/Hamlet.RadioEngine/Rig/ReceiveAdvice.cs`.** With the preamp off, the advice list said
  "Switch the preamp on" in any block that does not state the preamp. It now says the preamp is
  off, which is how he runs it, and proposes nothing.
- **Tests.**
  - `NoDetectionNoLettersTests` gained two synthetic tests, both watched failing first. A `K`
    reached the screen after the detector let go. The decoder stayed at 536 Hz with the detector
    at 675.
  - Twelve engine test files, their shared bench helper and two app test files encoded the
    superseded preamp 1, the old advice, or the Q of a short call. They were moved to R98 and
    R97, and no file lost assertions without gaining more.
- **Records.**
  - R98 is in both copies of `PHASE_PLAN.md`, and `DECISIONS.md` has HM-DEC-191.
  - Both status and outcome copies name 486.
  - The version went from 1.13.172 to 1.13.173.
- The commit also carries this unit's scratch logs and two copies of the view model made for a
  comparison, under `.run-unit/`. They are harmless, and `rm` is refused here.

**The three preamp faults, named.**
1. **Coming back to CW did not restore off because CW asked for preamp 1.** HM-DEC-177's rule
   was preamp 1 from 1.8 to 29.999 MHz. The data rows have never stated the preamp, so the 1 he
   saw in data was the CW tune-in's 1, left standing; the radio keeps the preamp per band. So
   there was nothing to restore. Under R98, CW asks for off, and returning to CW writes it.
2. **His hand lost in `ReceiverSetup.ApplyAsync`.** The "found already right" branch ran before
   the hand check and recorded the reading as Hamlet's own. Suppose he set the preamp off, and a
   tune-in on an overloading band wanted off too. His off was adopted as Hamlet's, and the next
   tune-in that wanted 1 saw no hand and wrote 1. The overload follow already asked in the right
   order.
3. **R98** is the CW row, above.

**What each row asks and writes now.**

| mode | asked | written | his own change survives a later tune-in |
|---|---|---|---|
| CW, CW DX, QRP (every band, 50 MHz too) | off | `FE FE 94 E0 16 02 00 FD` (CI-V `16 02`, value `00`, page 19-3) when the radio reads preamp 1 or 2; nothing when it reads off | yes, until a band change re-arms it (HM-DEC-056, unchanged) |
| FT8, FT4 | nothing: the rows do not state the preamp | nothing | yes, nothing touches it |

No row follows the overload flag any more, because the CW row was the only one with an overload
rule. The follow code is unchanged, and its tests now run on a condition built in the test.

**Mismatch with the instruction.** It names the overridden ruling HM-DEC-176; in `DECISIONS.md`
that is the floors' span bar, and the preamp ruling is HM-DEC-177. HM-DEC-191 says so.

**Verification.**
- The build: 0 warnings, 0 errors.
- The app carry-forward line: 276 of 278. The two failures are the dispatcher-loop loss,
  `TheWindowHoldsBelowItsMinimumTests` and `TheChipSaysTheChosenModeTests`, and both pass alone,
  3 of 3 and 6 of 6.
- Engine preamp, setup, advice and gate types: 180 of 180.
- App preamp, setup and scope types: 86 of 89. The three reds were already red before unit 484,
  checked on that commit in a separate worktree (see section 4).

## 2. What the owner should expect

1. Rebuild.
2. On a quiet band the panel and the terminal are empty and stay empty.
3. When a station keys, the blocks, the letters over them and the terminal fill together, and
   when it stops they stop together.
4. The *tone* and *mixing* numbers should now be the same number.
5. Tune into CW and the preamp goes off. Turn it on or off yourself, in CW or in data, and it
   stays where you put it; a band change is the only thing that re-arms it.

**The cost you ruled, and it is bigger than it sounds.** The letters the decoder settles after
the detector lets go are dropped. On a long over that is the last letter or two. On a short call
it is a whole letter every time: the training radio's `CQ` now shows the `C` and never the `Q`.
If it proves too much at the radio, the hold is the figure to lengthen, since it decides how long
the screen stays open after the last mark.

## 3. What you should see

- **Quiet band:** empty scope, empty terminal, and `no keying` on the panel.
- **A station keying:** the panel says `tone 700 Hz · mixing 700 Hz`, or whatever the pitch is,
  and the two agree. Blocks and letters appear together. The last letter of a short over may not
  appear.
- **The preamp:** off after a CW tune-in, and not put back when you change it.

## 4. What's blocking us

Nothing blocks. What is left, a line each:
- **Two app tests were red before unit 484 and are not this unit's.**
  - `HowMuchTheApplicationSaysTests`: the CW tab holds 585 characters against a ceiling of 550,
    the same at units 483, 484 and 485.
  - `ModeFollowsTheMapAgainTests.NothingButTheModeIsEverWritten`: its sweep region is 6,115
    characters against a 6,000 bound, the same since unit 440 at least.
  - Neither is on a carry-forward line.
- **`AMarkIsTheEnvelopeOverAThresholdTests` has two cases red at HEAD** (40 and 208), unchanged
  from unit 485.
- **The strayed-frequency question from unit 484** (options A to C) is still the owner's.
- **"Screen 1" at wide windows is still the owner's to rule**, from unit 485.

### Asks still outstanding

- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25, waiting on
  the owner; no change sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8
  record work under R80.
- **Unit 484's strayed-line question** (options A to C), 2026-09-28, waiting on the owner; no
  change sits in the tree.
- **Unit 485's screen 1 question at wide windows**, 2026-09-28, waiting on the owner; no change
  sits in the tree.
