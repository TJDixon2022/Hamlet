## 1. What Claude did

- **Session:** development computer, Claude Code. The prompt claimed `PROJECT: Hamlet`.
- **Gate:** checked in `C:\Source\HamLet`.
  - Present: `SHACK_FACTS.md`, `docs\cw-scoreboard.md`, `CW_REQUIREMENTS.md`.
  - Absent: `CoreHMI.sln`, `MURC.sln`.
- **Nothing in this report is evidence about the radio.** The scan ran only against a fake radio.
- **Work instruction 540, run by hand on `main`:**
  - SESSION.lock was taken and released.
  - Nothing was written to RUN_LEDGER.md, nothing under `tools\arbiter\` was touched, and nothing in PHASE_PLAN.md was ticked.
  - **Nothing was keyed or transmitted, and nothing was written to a radio:** no radio was connected.
- **Version:** 1.13.224 → 1.13.225. **Ruling:** HM-DEC-244.

**Tasks 1 and 2: the scan, and what each catch keeps** (commit `2f343e7b`). They are one commit, because the scan writes each catch's files as it goes.

- **The engine** (`Hamlet.RadioEngine.Scan`):
  - `CwCatchScan`, the loop.
  - `CwCatchEar`, its own detector and gate on the same audio, fresh for every catch. What the CW terminal reads is not touched.
  - `ScopePeaks`, which finds signals in the scope sweep.
  - `ListenOnlyLock`, the lock described under task 3.
- **The app:**
  - A **`Scan`** button beside `Record`, `Copy` and `Clear`. It reads **`Stop`** while a scan runs, and its hover says what it does and that it never transmits.
  - A **`⋯`** settings popover: length, positive stay, negative stay.
  - **The line under the header**, in the hint's own cell, so nothing moves. It reads `scanning 7.012`, `listening on 7.031 · shape found · 0:42`, `negative on 7.044 · 0:18`, `scan done · 14 catches`, `scan aborted · the radio's link dropped · 3 catches`.
- **The band:** whatever band the radio is on. Its CW segment comes from `HfBands`, the band data the map already uses. A band with no CW segment refuses to start. A hand-tune into another band stops the scan at once.
- **The scope margin is six of the floor's own spreads.**
  - The floor is the sweep's median bin. Its spread is 1.4826 × the median deviation, which equals a standard deviation for noise, and never under one scope unit.
  - Why: the scope's amplitude scale is the radio's own and the manual does not state it in decibels, so the margin is read from the sweep itself. Band noise stands six spreads over its floor about once in a billion bins, so 475 bins of noise show no peak.
  - **Hold:** each place is held for 2 s at its highest value, because a keyed station shows nothing in a single sweep taken between its marks.
  - **One peak per 250 Hz**, half the CW filter.
  - **Centroid:** each peak sits at the centroid of its bins, since a bin is about 280 Hz wide at a wide span.
- **Landing:** the dial is tuned to the peak. In CW, the dial reads the frequency of a signal heard at the CW pitch, so the signal lands at the pitch, in the middle of the filter.
- **The silence time is 10 s.** A positive is left once its station has been silent that long.
  - It is longer than any pause inside one operator's sending: two word gaps at 5 WPM Farnsworth are under 4 s.
  - It is long enough for the other operator to start a reply on the same frequency, so a contact in progress keeps the scan there up to the 90 s stay.
- **Ending:** at its length; on `Stop`, which saves the catch under way marked *stopped*; on leaving the CW tab; on a band change; when the link drops; or if the radio transmits.
  - **A dial moved by hand:** the scan carries on from the new frequency.
- **What it writes to the radio:** the frequency, to land on each peak and to put the dial back where it was when the scan ends.
  - The starting frequency is noted on disk before the first tune, using the old scanner's crash-safe note, so a crash mid-scan is put right on the next connect.
  - The dial is not put back if the link is down or the band changed.
  - If the segment is wider than the scope's span, it also tunes across the segment a span at a time to survey it.
  - **It writes no scope span or mode, no filter and no mode.** The scan never needed a scope write.
- **What each catch keeps,** in `%AppData%\Hamlet\scans\scan-<date>-<time>\`, next to `telemetry\`:
  - `catch-<time>-<frequency>.wav`: the whole stay, at the audio's own rate, which is 48 kHz from the IC-7300's USB codec.
  - `catch-<time>-<frequency>.json`: the start and end in UTC, the dial, the signal's frequency and level, and the scope floor; positive or negative, and why it left; every sender the gate held; the text with each letter's time; the light's states over the stay; and the radio's state at landing.
  - `scan.json`: rewritten after every catch, so a scan that dies still has its record.

**Task 3: listen only, proven** (commit `f5c0b724`).

- **The lock:** while a scan runs it holds one `ListenOnlyLock`, and every path that keys the radio asks it immediately before keying:
  - The CW door, `CwTransmitter.Check`, refuses with a new readiness state, `ScanRunning`, token `scan_running`. The CW send buttons therefore go grey with the reason beside them.
  - The radio's keyer, `KeyerCwSender`, asks before every piece.
  - The auto-caller's keyer shares the same lock.
  - The push-to-talk sequence that every audio mode keys through, `Ft8TransmitSequence`, refuses with `RefusedWhileScanning` before the key goes down and before any audio reaches the sound card.
- **The digital CQ button** is also bound off while a scan runs. The digital send row only shows on the Digital tab, and leaving the CW tab stops a scan.
- **The tests:** a whole scan on a fake radio, with both keying paths asked twice, keys nothing, writes no frame and plays nothing. Once the scan ends, the same keyer keys, which proves the lock was what held it. In the window, every transmit control pressed during a scan does nothing.

**Recorded, at the owner's order:** HM-DEC-244, *The scan: catch CW unattended, positives and negatives, listen only*. It quotes the owner and records all thirteen of his answers. It supersedes HM-DEC-107 on two points for this scan only, both by his answers: a dial moved by hand is carried on from, and the fence is the band's cited CW segment rather than a file he edits. The rest of §0.2.1 holds.

## 2. What the owner should expect

- **Rebuild.**
- **To start a scan:**
  1. Set the band on the radio.
  2. Open the CW tab and press **Listen**.
  3. Press **Scan** in the CW terminal's header, beside Record.
- **While it runs,** the button reads **Stop**, and the line under the header says what it is doing:
  - `scanning 7.012` while it looks at the scope;
  - `listening on 7.031 · shape found · 0:42` on a station it can read;
  - `negative on 7.044 · 0:18` where the scope shows something but no shape forms;
  - `scan done · 14 catches` when it finishes;
  - `scan aborted · the radio's link dropped` if the cable goes.
- **The `⋯` beside Scan** sets its length (30 minutes), how long it stays on a station it can read (90 s), and how long it listens where it cannot (30 s).
- **It needs the radio's scope stream reaching Hamlet**, the same stream the waterfall draws. If the waterfall is empty, the scan refuses and says the scope is not reaching Hamlet.
- **Where the catches land:** `%AppData%\Hamlet\scans\scan-<date>-<time>\`, beside the telemetry folder. **Tools > Open data folder** gets you there, so you can zip a scan folder and send it.
  - Each catch is a WAV and a JSON, and the folder has one `scan.json` listing them.
  - **A 30-minute scan is about 173 MB:** 48 kHz, 16-bit, mono. The test scans at 8 kHz were 3.8 MB for 4 minutes, which is the same arithmetic.
- **Nothing can transmit while it runs.** The CW send buttons go grey and say "a scan is running, and a scan listens only". Behind them, the keyer and push-to-talk refuse even if something gets pressed.
- **The only thing it changes on the radio is the frequency,** and it puts it back where you left it when it ends. It never changes band.
  - If you turn the dial yourself, it carries on from there.
  - If you change band, or leave the CW tab, it stops.
- **One difference from the order:** a station counts as a "positive" when the light goes **green** (shape found, or reading), not amber. In the tests, amber came on within seconds on plain band noise and on a steady carrier. See section 4.
- **Build:** clean.
- **App carry-forward:** 278 of 278.
- **The scoreboard reads exactly as at HEAD:** 206 right, 18 wrong, 2 invented, score 186. It is red for the same two reasons as before: the carrier at seed 5195, and one letter printed in a silence.
- **Pushed:** to `main`.
- **Untouched and uncommitted, from before this unit:** `PARKED.md`, `RUN_LEDGER.md`, `WORK_INSTRUCTIONS.md`, `.run-unit\denials.txt`, the four `.run-unit\reports` files, and `tests\fixtures\cw\captured\cases-2026-10-0{2,3}.txt`.

## 3. What you should see

**The tests:** 12 of 12 pass. The engine tests run on a fake rig, scope and audio with virtual time; the app test runs headless.

| # | test | result |
|---|---|---|
| 1 | `ThreePeaksAreVisitedAndCaught`: a call, noise and a carrier on the scope | visits 7.010, 7.030, 7.050 in order. Positive, negative, negative. WAV, JSON and scan.json written. Dial back home. **Pass** |
| 2 | `APositiveThatKeepsSendingIsLeftAtNinetySeconds` | left `stay-ran-out` at 90.0 s. **Pass** |
| 2 | `APositiveThatStopsIsLeftAfterItsSilence` | a 21.8 s call, left `read-out` at 30.0 s. **Pass** |
| 3 | (in 1) a negative | left at 30 s, its WAV 30 s long. **Pass** |
| 4 | `TheScanStopsOnItsOwnAtItsLength` (40 s) | `scan done · 1 catch`, the catch `scan-ended`. **Pass** |
| 5 | `StopEndsItAtOnceAndSavesTheCatch` | ended at 20 s, catch saved `stopped`, dial home. **Pass** |
| 6 | `ADialMovedByHandIsCarriedOnFrom` | catch `dial-moved`, next catch at 7.050. **Pass** |
| 6 | `ABandChangeStopsIt` | `scan stopped · the band changed`, dial left on 14.030. **Pass** |
| 7 | `ALinkThatDropsAbortsIt` | `scan aborted · the radio's link dropped`, catch `link-dropped`, scan.json says so. **Pass** |
| 8 | `NothingTransmitsWhileItRuns` | keyer refused: *"A scan is running, and a scan listens only: nothing goes out until it stops."* Push-to-talk `RefusedWhileScanning`, twice. 0 keying commands, 0 frames, 0 audio. Keys after the scan. **Pass** |
| 8 | `TheCwDoorRefusesWhileTheLockIsHeld` | readiness `ScanRunning`, token `scan_running`. **Pass** |
| 8 | `EveryTransmitControlDoesNothingWhileAScanRuns` (app) | CW send, auto-call arm and start, digital CQ, message, PSK31 answer and typed line pressed. Nothing keyed, framed or played; the CW buttons show the scan's reason. **Pass** |

Also run, with no change and nothing failing: 62 engine transmit, readiness and scan tests; `BindingHealthTests`; and `DecisionLogOrderTests`.

**Disk:** the test scan in `ThreePeaksAreVisitedAndCaught` wrote 3.8 MB over 4.0 minutes at 8 kHz.

**`scan.json`** from that test scan, with three of its six catches shown:

```json
{
  "startUtc": "2026-10-04T23:00:00Z",
  "endUtc": "2026-10-04T23:04:00Z",
  "band": "40 m",
  "segmentLowHz": 7000000,
  "segmentHighHz": 7125000,
  "homeHz": 7031000,
  "lengthMinutes": 4,
  "positiveStaySeconds": 90,
  "negativeStaySeconds": 30,
  "silentSeconds": 10,
  "marginSpreads": 6,
  "catches": [
    { "startUtc": "2026-10-04T23:00:02.75Z", "signalHz": 7009921, "kind": "positive", "left": "stay-ran-out",
      "text": "RQ CQ CQ DE W1AW W1AW W1AW K CQ CQ CQ DE W1AW W1AW W1AW K CQ CQ CQ DE W1AW W1AW W1AW K CQ CQ CQ DE W1AW W1AW W1AW K CQ N",
      "wav": "catch-230002-7009921.wav", "json": "catch-230002-7009921.json" },
    { "startUtc": "2026-10-04T23:01:33.25Z", "signalHz": 7030100, "kind": "negative", "left": "stay-ran-out",
      "text": "", "wav": "catch-230133-7030100.wav", "json": "catch-230133-7030100.json" },
    { "startUtc": "2026-10-04T23:02:03.75Z", "signalHz": 7049994, "kind": "negative", "left": "stay-ran-out",
      "text": "", "wav": "catch-230203-7049994.wav", "json": "catch-230203-7049994.json" }
  ],
  "ended": "length-reached",
  "sentence": "scan done · 6 catches",
  "scopeFollowsDial": null
}
```

**A catch's JSON**, the positive above, shortened where it lists 120 letters, 60 light changes and the radio's fields:

```json
{
  "startUtc": "2026-10-04T23:00:02.75Z",
  "endUtc": "2026-10-04T23:01:32.75Z",
  "dialHz": 7009921,
  "signalHz": 7009921,
  "signalLevel": 90,
  "scopeFloor": 23,
  "kind": "positive",
  "left": "stay-ran-out",
  "stations": [ { "pitchHz": 600.04, "shapeScore": 0.464, "marks": 289, "printed": true } ],
  "text": "RQ CQ CQ DE W1AW W1AW W1AW K CQ CQ CQ DE W1AW ... CQ N",
  "letters": [ { "seconds": 0.73, "text": "R" }, { "seconds": 1.79, "text": "Q" }, { "seconds": 3, "text": "C" }, "..." ],
  "lights": [ { "seconds": 0, "light": "listening" }, "...", { "seconds": 87.47, "light": "shape found" }, { "seconds": 87.79, "light": "reading" } ],
  "radio": [ { "field": "Frequency", "value": "7.010 MHz", "known": true }, { "field": "Mode", "value": "CW", "known": true }, "..." ],
  "wav": "catch-230002-7009921.wav",
  "sampleRate": 8000,
  "seconds": 90
}
```

The first letter reads `R` where the call opens with `C`. The ear joined the call 0.7 s in, part way through the first C. The rest of the call reads whole.

## 4. What's blocking us

- **Your call: does amber count as a positive?** The order said a positive is the light's amber, *shape forming*, or better.
  - In the scan's own tests, amber came on within seconds on plain band noise and on a steady carrier. By that rule every catch was a positive, and the negatives were never caught.
  - So the scan counts a positive only on **green**, a shape found or reading.
  - Amber is still kept in every catch's light history, so the JSON shows which negatives went amber.
  - Industry practice: count on the stronger signal, as built.
  - The alternative is amber held for some seconds.
- **Never run on the radio.** Things a fake can't show:
  - whether the IC-7300's scope follows the dial in its centre mode when the segment is wider than the span (`scopeFollowsDial` in scan.json will say);
  - how the six-spread margin behaves on a real band;
  - whether CPU holds with a second detector running.
- **The scope must already be streaming.** The scan writes no scope setting, so with the scope's wave output off, it refuses and says the scope isn't reaching Hamlet.
- **The order named three "why it left" reasons that needed definitions:**
  - *read out*: a positive that printed and then went silent;
  - *went silent*: one that went silent without printing;
  - Added beside them: *dial moved*, *band changed*, *scan ended*, and *scan stopped* (tab left or the radio transmitted).
- **The scan's settings are not remembered across restarts.** They reset to 30 min, 90 s and 30 s.

### Asks still outstanding

- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
- **Unit 539, 2026-10-04:** ship rule set A, rule set B, or neither (above). Waiting on the owner. Both are switch lists in `CwRules` and neither is on.
- **Unit 540, 2026-10-04:** does amber count as a positive (above)? Waiting on the owner. The scan counts green; the change is one line in `CwCatchEar`.
