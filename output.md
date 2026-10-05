## 1. What Claude did

- **Session:** development computer, Claude Code. The prompt claimed `PROJECT: Hamlet`.
- **Gate:** checked in `C:\Source\HamLet`.
  - Present: `SHACK_FACTS.md`, `docs\cw-scoreboard.md`, `docs\cw-keying-map.md`.
  - Absent: `CoreHMI.sln`, `MURC.sln`.
- **Nothing in this report is evidence about the radio.**
- **Work instruction 542, run by hand on `main`:**
  - SESSION.lock was taken and released.
  - Nothing was written to RUN_LEDGER.md, nothing under `tools\arbiter\` was touched, and nothing in PHASE_PLAN.md was ticked.
  - Nothing keys or transmits. The only radio write in the new code is the frequency.
  - No scan catch was read.
- **R88:** lifted for the owner's twelve recordings only. They were read only by the scoreboard.
- **Version:** 1.13.226 → 1.13.227. **Ruling:** HM-DEC-246, *One wiring for every listener; the scan lands by ear*.
- **The scoreboard read 191 after every task.** Its test stays red only on the two hard limits red at HEAD: the carrier at seed 5195, and one letter in a silence.

**Task 1: one wiring for every listener** (commit `7858b92d`).
- **What was found:** three wiring differences, listed in section 3.
- **What changed:** `CwChain` (`src/Hamlet.RadioEngine/Cw/CwChain.cs`) builds the decoder and the detector and wires them exactly as the app did.
  - The app, the scan's ear, the scoreboard and the bench helpers in `ThePatternIsTheGateTests` and `TheOwnersRecordingReadsTests` all use it.
  - The app's own wiring block is now one call to `CwChain.Wire`.

**Task 2: a peak is what the scope really shows** (commit `8f180ed4`).
- **What changed:** `ScopeWatch` replaces the level-only peak rule. A peak must repeat, be narrow, and stand over a floor the radio clips to nought. The figures are in section 3.

**Task 3: the scan lands by ear** (commit `534812bd`).
- **How it lands:**
  - At each peak the scan listens two seconds for the strongest narrow tone in the passband.
  - It retunes so that tone sits at the CW pitch.
  - If no tone is heard, it tries half a filter width either side.
  - A tone already within 10 Hz of the pitch is not retuned, and the catch keeps what the probe heard.
- **The new `empty` kind:** a stop with no tone after all three tries is `empty`, left as `NothingHeard`, and left at once.
- **Records:** a catch now records the scope's frequency and the tone heard.
- **The pitch and filter the scan listens through** are the radio's own in CW. Where the radio's are unread or read as nought, it uses the ear's.
- **One scan test was changed:** `APositiveThatStopsIsLeftAfterItsSilence` now counts the silence from the end of the call, not from the start of the catch.
  - A scan that probes and retunes begins its catch after the call has started.
  - In that test the scope's centroid put the dial 80 Hz off. The scan heard the call at 680 Hz and retuned onto it.

**Task 4: the carrier and the noise** (commit `f8d9efae`).
- **What was done:** measured through the shared wiring at five seeds.
- **Result:** neither the carrier nor the noise reaches green. Both hold amber for a while. The figures are in section 3.
- **No cause was plain, so nothing was changed.**

**Records:** DECISIONS.md HM-DEC-246, the CLAUDE.md §1 row, PHASE_OUTCOME and PHASE_STATUS in both copies, and the version.

## 2. What the owner should expect

- **Rebuild**, then run a scan as before.
- **What was wrong with the scan's ear:** it was wired its own way. It listened through the pitch and filter Hamlet was given, not the radio's own.
  - Now the ear, the app, the scoreboard and the bench all share one wiring, and it is the app's.
- **That was not the whole fault:**
  - On a synthetic 20 WPM call through the filter, the old ear and the app already found the same 70 marks and reached green.
  - So this work does not show that your three real stations would now be positives. Only the radio can tell us that.
  - What has changed for them is the landing: the scan now puts the tone it hears at your CW pitch, where before it trusted the scope to within a few hundred hertz.
- **How the scan picks signals:** it keeps only a place on the scope that comes back sweep after sweep, is narrow, and stands above the floor. One-sweep noise blips at 6 or 7 are no longer stops.
- **How it lands:** it listens two seconds and retunes onto the tone it hears. If it hears none, it looks half a filter either side.
- **The new `empty` kind:** a stop with nothing to hear is marked `empty` and left after about 7 seconds. Before, it sat out the 30-second negative stay.
- **How much faster a pass is:**
  - In your first scan, 45 of 55 stops held nothing. At 30 s each that was about 22 minutes.
  - At 7 s each it is about 5 minutes, before counting the blips the new peak rule no longer stops for.
- **Green is still the line for a positive.** Amber still comes on over a steady carrier and over noise, and is not counted.

## 3. What you should see

**The wiring differences:**
- **The passband.**
  - The ear heard through the pitch and filter it was given, fixed at the start.
  - The app hears through the radio's own CW pitch and filter, read live, and only in CW or CW-R.
  - The ear now does the same.
- **The order.**
  - The app's decoder takes each chunk before its detector does, so the decoder reads the detector's marks one chunk behind.
  - The scoreboard and the bench helpers ran the detector first. They now run the decoder first.
- **The waiting pitch.** The scoreboard and the bench helpers never wired the detector's waiting pitch (a sender qualified but not yet printed). They do now.
- **The scoreboard:** 191 before and after, 208 of 244 right, 15 wrong, 2 invented.

**The synthetic call** (20 WPM, 12 dB, a 1 dB AGC overshoot, through the 500 Hz filter):

| | marks | shape | green | text |
|---|---|---|---|---|
| app, before | 70 | 0.494 | yes | `CQ CQ CQ DE W1AW W1AW W1AW K` |
| ear, before | 70 | 0.494 | yes | `CQ CQ CQ DE W1AW W1AW W1AW K` |
| app, after | 70 | 0.494 | yes | `CQ CQ CQ DE W1AW W1AW W1AW K` |
| ear, after | 70 | 0.494 | yes | `CQ CQ CQ DE W1AW W1AW W1AW K` |

**The scope rule** (`APeakIsWhatTheScopeShowsTests`, 11 pass):
- **The figures:**
  - Where the median is above nought, a peak stands six of the floor's spreads over it. Where the median is nought, any value above nought counts.
  - It must stand in a quarter of the sweeps watched, and three at least, within a bin of the same place.
  - It may be three bins wide at most. Peaks within 250 Hz are one peak.
- **The test sweeps:** 20 sweeps with a floor clipped to nought, two blips a sweep at 6 or 7, a keyed station at 6 up in half the sweeps, and a crash thirty bins wide in two sweeps.
- **With the station:** exactly one peak at each of five seeds, 22 to 79 Hz from the station, standing in 8 to 14 of 20 sweeps.
- **Without it:** no peak at any seed.
- **An unclipped floor** keeps its six-spread margin.

**The landing test** (`TheScanLandsByEar`):
- The scope peak was put 205 Hz above the call.
- The scan heard the tone at 395 Hz, retuned, and landed **0 Hz** from the call. It read the call as a positive.
- The empty stop was left in **7.0 s**.
- All 9 catch-scan tests pass, and both never-transmits tests pass.

**The carrier and the noise** (`TheEarHearsAsTheAppDoesTests`, 26 s each, five seeds):

| | green | amber | best shape | printed |
|---|---|---|---|---|
| steady carrier at 750 Hz | never | 0.1 to 12.2 s | 0.167 | nothing |
| band noise | never | 0 to 5.0 s | 0.000 | nothing |

**Build:** no warnings. **App carry-forward:** 277 of 278. The one loss, `ThreeChipsCostTheTopBandNothing`, is the dispatcher loop, and it passes alone.

## 4. What's blocking us

- **Whether the three real stations now read green** is unknown until a scan on the air. The synthetic call already read green through the old ear, so the field miss is not explained.
- **Amber on a carrier and on noise** holds up to 12.2 s and 5.0 s. Green is unaffected, so nothing was changed.
- **The silence limit stays red at one letter,** as at HEAD.
- **The carrier limit stays red at seed 5195,** as at HEAD.
- **The low-confidence 22:18:51 stretch** still prints junk the score cannot see, as reported by unit 541.

### Asks still outstanding

- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.

Unit 540's ask, whether amber counts as a positive, is answered by work instruction 542 (*green stays the line for a positive*) and is dropped.
