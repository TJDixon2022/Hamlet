## 1. What Claude did

- **Session:** development computer, Claude Code. The prompt claimed `PROJECT: Hamlet`.
- **Gate:** checked in `C:\Source\HamLet`.
  - Present: `SHACK_FACTS.md`, `src\Hamlet.RadioEngine\Cw\CwChain.cs`, `docs\cw-scoreboard.md`.
  - Absent: `CoreHMI.sln`, `MURC.sln`.
- **Nothing in this report is evidence about the radio.**
- **Work instruction 543, run by hand on `main`:**
  - SESSION.lock was taken and released.
  - Nothing was written to RUN_LEDGER.md, nothing under `tools\arbiter\` was touched, and nothing in PHASE_PLAN.md was ticked.
  - No recording or scan catch was read.
  - Nothing keys or transmits. The only radio write is the frequency.
- **Version:** 1.13.227 → 1.13.228. **Ruling:** HM-DEC-247, *The scan watches a span, visits what it saw, then moves on*.
- **The scoreboard read 191 after every task.** Its test stays red only on the two hard limits red at HEAD: the carrier at seed 5195, and one letter in a silence.

**Task 1: why clear stations are skipped** (commit `f42108f0`, before any change).
- **The fake radio:**
  - A centre-mode scope that follows the dial, ±10 kHz, 475 bins of about 42 Hz.
  - The floor clipped to nought, and two noise blips a sweep at 6 or 7.
- **The sweep rate:** about 4.5 sweeps a second. That is the rate measured off the owner's radio and recorded in `ScopeFlow.QuietAfter`. The older fake scope sent ten a second, more than twice the real rate.
- **Three stations in the first span,** each keyed down in 55% of sweeps, drawn as a core with skirts:
  - strong at 7.0043 MHz, 120 at its core, 11 bins over nought at its base;
  - moderate at 7.0091 MHz, 30 at its core, 5 bins at its base;
  - moderate at 7.0150 MHz, 24 at its core, 5 bins at its base.
  - The skirt shapes are mine, from the scope's resolution being wider than a bin at that span.
- **The cause: every clear station was refused as too wide.**
  - The scan did wait: it watched the span for 9 sweeps, and the rule needed 3. So the first suspected cause, not waiting, does not hold.
  - The radio clips its floor to nought, so the line sat at 1, and a station's width was counted at the foot of its skirts.
  - Every sweep in which a station was up measured 11 or 5 bins, against a limit of 3, so none ever qualified.

**Task 2: watch, visit, advance** (commit `cc3f0d1a`).
- **One span at a time.** The scan advances one scope span across the CW segment and wraps at its end.
  - Where the scope shows the whole segment, or does not move with the dial, there is one span: what the scope shows.
- **Watch.** It watches for the **survey time**, 3 s by default, about 13 sweeps.
  - The survey time is a setting in the popover, 1 to 30 s, kept with the other three scan settings.
- **What gets listed.** Every top, not one per run of bins, that does both of these:
  - **stands in a quarter of the sweeps watched, and three at least**, as before;
  - **is 250 Hz or narrower at half its own height over the floor.** Where three bins are wider than 250 Hz, the limit is three bins.
- **Why 250 Hz:**
  - A keyed CW signal occupies about four times its speed in hertz, 160 Hz at 40 WPM, and the scope adds its own resolution.
  - A phone signal is about 2.4 kHz, and a static crash covers many bins.
  - Half its own height is where a signal's width is its own and not its strength's.
- **Neighbours:** tops within 250 Hz are one station, as before.
- **Visit, then advance.**
  - It visits each listed station in frequency order and lands by ear, as before.
  - Then it advances. A span with nothing listed advances as soon as its survey ends.
- **A hand on the dial** carries on from there:
  - With a moving scope, the next span is centred where the hand left it.
  - With a whole-segment scope, the visits begin at the station nearest the hand.
- **Watched fail first.** The three span tests were run against the task 1 commit, and all three failed there. Section 3 has the failures.

**Task 3: the scan shows its reasoning** (commit `5e999774`).
- **The line** reads `watching 7.000–7.020 · 0:03 · 3 stations`, then `visiting 1 of 3 · 7.0043 · landing`, then the light, `negative`, or `empty` with its clock, then `advancing to 7.020–7.040`.
- **`scan.json` keeps every survey:**
  - its span, the sweeps watched, its floor, line and bin width;
  - every peak considered, with its level, width at half height, sweeps stood narrow and sweeps shown in;
  - a verdict for each (`listed`, `not-repeating`, `too-wide` or `merged`) and why, in words.
  - Each catch names the survey it came from.
- **Two faults found by this task's test, and fixed:**
  - A weaker station 150 Hz from a stronger one was measured across its neighbour's slope and called too wide. A top's width now stops where the bins rise again, so the two are merged as one station.
  - A skipped wide signal was placed at its left edge. It is now placed at the centroid of every sweep it showed in.
- **One departure from the order's numbering:** the survey record and the new line were built in task 2's restructure of the loop, since the new loop is what produces them. Task 3 added their tests and the two fixes.

**Records:** DECISIONS.md HM-DEC-247, the CLAUDE.md §1 row, PHASE_OUTCOME and PHASE_STATUS in both copies, and the version.

## 2. What the owner should expect

- **Rebuild**, then run a scan as before.
- **Why clear stations were skipped:**
  - The scan judged a station's width at the very bottom of its trace on the waterfall, where the radio has cut its noise to nothing.
  - A strong, clear station spreads wide down there, so it was thrown out as too wide to be Morse. The clearer the station, the surer it was to be skipped.
  - The scan was waiting long enough; it just refused what it saw.
- **What the scan does now, span by span:**
  - It moves the dial by one waterfall's width, as you saw before.
  - It watches for three seconds.
  - It lists every narrow signal that keeps coming back, judged by its width halfway up rather than at its foot.
  - It visits each one in turn, low to high, landing by ear.
  - Then it moves to the next span. A span with nothing in it is left as soon as the three seconds are up.
  - The three seconds is a new setting, beside the others under the `⋯` button.
- **What the line under the header says:**
  - `watching 7.000–7.020 · 0:02 · 3 stations` while it looks;
  - `visiting 2 of 3 · 7.0091 · shape found · 0:31`, or `· negative · 0:12`, or `· empty`, while it listens;
  - `advancing to 7.020–7.040` as it moves on.
- **Where the reasons are kept:** in `scan.json` in the scan's folder (`%AppData%\Hamlet\scans\scan-…`), under `surveys`.
  - Every survey lists every peak it considered and why it was visited or skipped.
  - A station you saw on the waterfall and the scan passed by has its reason written there: shown in too few sweeps, too wide for Morse, or kept as one with a stronger neighbour.
- **Measured on fake sweeps only.** Nothing here was tried on your radio. The widths a real station shows on your scope have not been measured.

## 3. What you should see

**Task 1, the trace at the task 1 commit, before any change.** Sweep by sweep, level/width in bins over the line, `-` = keyed up:
```
span 7.000-7.020 MHz, tuned at 0.25 s, held 0.75 to 2.75 s; 9 sweeps watched; next tune at 2.75 s to 7.0280 MHz
floor 0 (median of every bin), line 1; a peak must stand in 3 of 9 sweeps and be 3 bins wide or less
strong 7.0043 MHz: 120/11 - 120/11 - 120/11 120/11 120/11 - 120/11 -> refused: too wide - 6 sweeps up, 0 of them 3 bins or narrower at the line
moderate 7.0091 MHz: 30/5 - 30/5 - 30/5 - - 30/5 30/5 -> refused: too wide - 5 sweeps up, 0 of them 3 bins or narrower at the line
moderate 7.0150 MHz: 24/5 - - - 24/5 24/5 - - - -> refused: too wide - 3 sweeps up, 0 of them 3 bins or narrower at the line
noise blips: 15 over 15 places, the most any place came back 1 -> refused: not repeating
the rule's own peaks over these sweeps: none
catches in 25 s of scanning: 0; tunes: 7.010@0.3, 7.028@2.7, 7.046@5.2, 7.064@7.7, 7.082@10.2, 7.100@12.7, 7.118@15.2, 7.010@19.8, ... 7.031@39.3
```
- The scan at the task 1 commit also ran past its 25 s length to 39 s, because its survey of the whole segment never looked at the clock.

**The same span now** (13 sweeps in 3 s): the old rule still refuses all three, and the new rule lists all three.
```
7.0043 MHz level 120, 126 Hz at half height, listed: stood narrow in 8 of 13 sweeps
7.0091 MHz level 30, 126 Hz at half height, listed: stood narrow in 7 of 13 sweeps
7.0150 MHz level 24, 126 Hz at half height, listed: stood narrow in 6 of 13 sweeps
tunes: 7.010@0.3, 7.004@3.7, 7.004@6.2, 7.005@8.7, 7.009@11.2, 7.009@13.7, 7.009@16.2, 7.015@18.8, 7.015@21.3, 7.015@23.8, ...
```

**The tests:**
- `TheScanWatchesASpanTests`, all 3 pass. At the task 1 commit, all 3 fail:
  - **All three clear stations are visited:** expected 3, got 0.
  - **A span of blips is left after its survey:**
    - Now: nothing visited, and the next span is tuned 3.5 s after the first.
    - At the task 1 commit: the next tune went to 7.028 rather than one span on, 7.030.
  - **A station keyed half the time is visited, and one keyed in one sweep of fifteen is not.**
    - At the task 1 commit, nothing was visited.
    - At 40% keying and this seed, the station showed in 3 of 13 sweeps and was skipped as not repeating. That is the rule as written, and its reason is recorded.
- `TheScanShowsItsReasoningTests`, all 3 pass:
  - the watching, visiting and advancing lines;
  - `visiting 1 of 3 · 7.0100 · reading · 0:05` on the call, and `visiting 3 of 3 · 7.0500 · negative · 0:12` on the carrier;
  - `scan.json`'s survey.
- **Other tests:**
  - `WhyClearStationsAreSkippedTests` passes.
  - All 93 scan tests pass, including every catch-scan and never-transmits test.
  - The app's scan-settings tests pass, now with the survey time.

**An example survey from `scan.json`:** a moderate station, a weaker one 150 Hz above it, and a phone-wide signal. The 22 one-sweep blips, each `not-repeating`, are left out here.
```json
{
  "index": 1, "lowHz": 7000000, "highHz": 7020000, "sweeps": 13, "listed": 1,
  "considered": [
    { "frequencyHz": 7004021, "level": 30, "widthHz": 126.3, "stood": 10, "seen": 10,
      "verdict": "listed", "why": "stood narrow in 10 of 13 sweeps" },
    { "frequencyHz": 7004147, "level": 20, "widthHz": 126.3, "stood": 10, "seen": 10,
      "verdict": "merged", "why": "within 250 Hz of the station at 7.0040 MHz, kept as that one" },
    { "frequencyHz": 7012021, "level": 40, "widthHz": 2484.2, "stood": 0, "seen": 13,
      "verdict": "too-wide", "why": "showed in 13 of 13 sweeps, 2484 Hz wide at half its height, over the 250 Hz a keyed CW signal fills, narrow in only 0" }
  ]
}
```

**Build:** no warnings. **App carry-forward:** 277 of 278. The one loss, `TheOfferIsOneButtonAndItIsTheOneTheEngineNamed`, is the dispatcher loop, and it passes alone.

## 4. What's blocking us

- **The 250 Hz limit and the station shapes are the author's, not measured off your scope.** A survey's `widthHz` on the air will show whether real stations sit under it.
- **A scope in its fixed mode, narrower than the segment, is not tested.** The scan treats what it shows as the one span.
- **A visit's landing does not watch the scan's length.** A scan can end up to one landing, about 7 s, after its length.
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
