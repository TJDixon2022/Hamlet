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

**What the tree showed, before anything changed: the terminal already read the run reader.**
- Since unit 490 the tab has given the decoder the detector's marks. With `ReadsRuns` on, the only
  letters that reach `Transcript.Settle`, or the scroll through the same event, are
  `CwRunReader`'s. The timing-only path's settles and leading edge return early.
- **So the `E`s in the owner's terminal were the run reader's.**
- Measured on three minutes of loud noise:
  - The run reader printed 53 to 81 letters, nearly all `E`. The timing-only path printed none;
    its emission gate refuses noise.
  - The mechanism: a noise "sender" made its two qualifying runs by chance and was printed. Noise
    keeps arriving at its pitch, so it was never silent for a second and was never released. It
    printed every noise mark there, mostly as `E`.
  - The scroll draws a letter only over its blocks, so it showed none of them. That is why the two
    surfaces disagreed.
- If the owner's build predates unit 490, his `E`s came from the old path instead. The fix below
  covers both.

**The changes, file by file** (all in `d4fbcdf7`):
- **`src/Hamlet.RadioEngine/Cw/CwMark.cs`:** a mark carries `Keyed` - whether the detector was
  keying at its peak, or within two bins of it, when the mark was called. Keying means bars paired
  and clear of their gaps' wander in the last second: the same test that decides the blocks the
  scroll draws. A station's second mark on is keyed; noise never is.
- **`src/Hamlet.RadioEngine/Cw/CwEnvelopeDetector.cs`:** sets `Keyed` when it calls a mark. Nothing
  else changed: the marks, the tolerances, the flatness rules, the pairing, and unit 492's three
  rules and three-window bound are all as they were.
- **`src/Hamlet.RadioEngine/Cw/CwRunReader.cs`:**
  - **A sender's two qualifying runs must each hold a keyed mark.** So a letter in the terminal is
    a letter over the blocks.
  - `StationPitchHz`: the printed sender's pitch, NaN when none.
- **`src/Hamlet.RadioEngine/Cw/CwDecoder.cs`:**
  - `PrintingHz`: the printed sender's pitch while runs are read.
  - The run reader's letters also raise `CharacterDecoded`. The app times its quiet offer and its
    evidence that the operator is working Morse (HM-DEC-149) from that event, and it had not fired
    since unit 490.
- **`src/Hamlet.App/ViewModels/MainWindowViewModel.cs`:** the scope tick passes `PrintingHz`, not
  the retired path's mixing pitch.
- **`src/Hamlet.App/ViewModels/CwHearingViewModel.cs`:** the empty case reads **"no station"**, and
  the hover says the number is the pitch of the station whose letters are shown.
- **Tests.**
  - New in the engine: `ThreeMinutesOfNoiseReadNothing`.
  - New in the app: `OneDecoderOneTruthTests`, the five cases.
  - Two tests had their pinned "not mixing" changed to "no station".
- **Records.**
  - R106 is in both `PHASE_PLAN.md` copies, and `DECISIONS.md` has HM-DEC-198. It records what the
    instruction named and what the tree showed.
  - Both outcome and status copies name 493.
  - Version 1.13.179 → 1.13.180.

**How the pitch crosses threads.** The reader writes the printed sender's pitch into one `double`
with `Volatile.Write` on the audio thread, once per batch of marks. The panel's scope tick reads it
with `Volatile.Read` through `CwDecoder.PrintingHz`. That is the pattern the decoder already uses
for `Heard`, which is read with `Interlocked.Read`. The scope's blocks cross differently, as a copy
taken under the detector's lock, but one number needs no lock.

**The old path and unit 489's switches.**
- The timing-only path reaches no surface: not the terminal, the leading edge, the scope, or the
  capture sheet's transcript, which is the terminal's.
- It stays in the tree behind `ReadsRuns`, and nothing was deleted.
- Unit 489's three switches gate nothing on the screen's path now. They were left as they are.

**Watched failing first:** three minutes of loud noise, in the radio's 500 Hz CW passband and
across the whole band.
- **Before:** the run reader printed 53 and 81 letters, all but a handful `E`.
- **After:** it prints none.

**Verification.**
- The build: 0 warnings, 0 errors.
- **The app carry-forward line: 278 of 278.**
- **The app scope, layout, voice and one-truth types: 91 of 93.**
  - A `TheTopRowTests` name failed in the long run and passes alone, 15 of 15.
  - The British spelling red is from 2026-09-26, not this unit.
- **The engine run, gate and detector types: 31 of 34.** The reds are unchanged:
  `ThatPitchIsTheStationsOwn` at 1024, and `AMarkIsTheEnvelopeOverAThresholdTests` at 70 and 208.

## 2. What the owner should expect

1. Rebuild.
2. On noise, the scroll is empty and **so is the terminal**.
3. On a station, the letters in the terminal are the same letters that sit over the blocks, at the
   same moment. Both come from one decoder, and a station has to be keying, bars paired where the
   blocks are drawn, before anything prints.
4. The panel shows the pitch of the station it is printing, or **no station**.
5. **If a real station reads nothing, the scroll will be empty too.** That is the diagnosis, and it
   is the one thing to report back.

## 3. What you should see

The terminal's letters beside the scroll's, on the five cases. **They are equal in each.**

| case | terminal | terminal letters | scroll letters | equal |
|---|---|---|---|---|
| 1. the clean call | `CQ CQ DE N0CALL N0CALL K` | `CQCQDEN0CALLN0CALLK` | `CQCQDEN0CALLN0CALLK` | yes |
| 2. the call with bursts in every gap | `CQ CQ DE N0CALL N0CALL K` | `CQCQDEN0CALLN0CALLK` | `CQCQDEN0CALLN0CALLK` | yes |
| 3. loud noise, no station | empty | empty | empty | yes |
| 4. a lone dit; a lone dah | empty; empty | empty; empty | empty; empty | yes |
| 5. the call plus `TEST DE W1AW K` 200 Hz away | `CQ CQ DE N0CALL N0CALL K` | `CQCQDEN0CALLN0CALLK` | `CQCQDEN0CALLN0CALLK` | yes |

- The scroll has no word gaps, so the comparison is letter for letter.
- **Three minutes of noise:** 53 to 81 letters before, none after (engine test).

## 4. What's blocking us

Nothing blocks. What is left, a line each:
- **A station must be keying to print.** Its bars must pair and clear their gaps' wander within
  two bins of its peak, and its first two multi-element letters must each hold such a mark. A very
  weak station the detector never pairs will print nothing, and the scroll will show why.
- **Whether the owner's screenshot came from a build before unit 490** isn't known here. Either
  way, the terminal now prints only what the scroll draws.
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
  revised, at the cost of seconds of lag. On the run path, which is now the only path to the
  screen, the terminal shows only settled text. The ask stands only for the timing-only path,
  which no longer reaches the screen; no change for it sits in the tree.
