## 1. What Claude did

Development computer, project gate `PROJECT: Hamlet` checked against `PROJECT_CARD.md`, the order's five checks and `Hamlet.sln`: Hamlet confirmed. Nothing here is evidence about the radio. Branch `main`. Run by hand: `SESSION.lock` taken at 11:48:56 and released at the end, nothing written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` touched, no box ticked in `PHASE_PLAN.md`. Version 1.13.233 to 1.13.234. Ruling HM-DEC-253, the number the order gave. Nothing was recorded under §12.1. No recording was read for the work. The scoreboard, which reads the owner's recordings, was run after every task as the order requires.

**Task 1, every W1AW session captured whole** (`2f266409`):
- **When:** Hamlet is listening, the radio reads CW or CW-R, no scan is running, and the dial is on a frequency in `w1aw-morse.json`. The window runs from **a minute before** a scheduled run to **two minutes after** its scheduled end (the order's figures).
- **On the frequency means within half the radio's filter, or 250 Hz where the filter is unread.** The order said "the tolerance the W1AW button already uses", but the button has none: it tunes to the frequency exactly. Half the filter is the point where W1AW's tone leaves the passband. Handed back in section 4.
- **Back-to-back sessions** (15:00 practice, then the 16:00 bulletin) overlap by three minutes once widened. The later one takes over when its window opens, so each capture holds one schedule entry.
- **Written as it arrives, never from a ring.** The sound card's thread copies each chunk into a three-second queue, and a thread of its own writes it to disk. A chunk straddling a piece boundary is split, so each piece starts on the very next sample. A gap in the audio clock is written on the sheet as a `hole` line, never filled with silence.
- **Pieces** are 5 minutes each, 48 kHz 16-bit, `piece-01.wav` with `piece-01.txt`, about 28.8 MB a piece. They go in `captures\auto\w1aw-<UTC date>-<time>\`.
- **Each sheet** carries:
  - the schedule entry (kind, speed word, scheduled UTC and Central times, the schedule's own speeds line);
  - the frequency matched and the window;
  - the piece number, the time it started and closed, and its sample span;
  - missing samples and any notes;
  - the app's own lines: frequency and band from the radio, pitch, speed, senders, the audio line, and every rig field.
- **It ends** when the dial leaves, the radio leaves CW, listening stops, the window closes, or the next session begins. The last sheet says which.
- **The line** sits in the hint's cell under the terminal header, so nothing moves: `capturing W1AW · code practice · piece 3 · 12:40`, the time being how long it has run. Its hover says where the files go. A scan's own line wins that cell.

**Task 2, trouble captured where it happens** (`d9b4d929`). This applies while listening in CW and not scanning. Hamlet keeps its own **5-minute ring**, whatever Record is set to. When a trigger fires it saves what the ring holds (5 minutes once it has heard 5) to `captures\auto\trouble-<date>-<time>-<reason>\capture.wav` with `capture.txt`. The triggers:
- **`senders`:** four or more senders held for 10 seconds.
  - Why four: the order's telemetry had W1AW at 4 to 9 senders against 0 to 2 at other hours.
  - Why 10 seconds: longer than noise takes to form a sender and be forgotten.
- **`audio-lost`:** any increase in the live path's lost count. One lost 50 ms chunk a second reproduced the junk (HM-DEC-252).
- **`stray-letters`:** six or more letters of one or two elements (E T I A N M), **each printed alone as a word**, within 10 seconds.
  - **The order's figure fires on clean copy, measured** (`WhatJunkLooksLikeTests`, text only):
    - These letters are 52% of plain English (the Gettysburg Address).
    - At 18 WPM, plain English prints up to **15** of them in any 10 s, a bulletin's text **19**, and a Q-code exchange **13**. At 35 WPM the figures are 23, 32 and 17.
    - Six of them in a row occurs 3 times in the English, 5 in the bulletin and 2 in the QSO. The bench junk `TYAEE IEA RADIO EMII EEIOTS` has it twice, so a run does not separate junk from clean text either.
  - **What clean text never does is print them alone.** The only one-letter English words are A and I. English, bulletin and Q-code text reach **at most 2** lone ones in any 10 s at 18, 25 or 35 WPM. Your word was *stray* letters, the `E I S A N` kind, so I took six alone, three times the clean ceiling.
  - **What it misses:** the bench junk above has no lone letters, so this trigger does not fire on it (the audio-lost trigger does). Handed back in section 4.
- **Cooldown:** each trigger waits **10 minutes** after it fires.
- **During a W1AW capture,** a trigger is written on that capture's sheet as `trigger    13:10:16 UTC ... (noted here instead of saving again)`.
- **During a scan,** nothing builds up: the senders' clock restarts, a loss during the scan is taken as seen, and stray letters are forgotten.

**Task 3, the telemetry** (`1bd9d975`):
- **`cw_listen`, every 10 s while listening:**
  - `audioLostMs`, `audioLostLast10sMs`, `audioLongestStallMs`, `audioQueuePeak`;
  - `sendersHeld`, and `senders`, each with `pitchHz`, `shape`, `marks`, `qualified` and `printed`;
  - `printedPitchHz`, `lettersPrinted10s`, `light`;
  - `capture`, the folder's name alone, no path (HM-DEC-018).
- **`qualified`** is the gate's first test (two keyed runs, two kinds, its own letter gaps). That test was moved into one method in `CwSenderGate`, unchanged, and the gate uses that same method.
- **`decode_quality` counts are re-sourced from the shape side.**
  - `charactersEmitted` now comes from the letters printed, and `charactersUnsure` from the letters printed as the placeholder.
  - **`elementsResolved` is dropped.** `marksStood` replaces it under its own name, because it is a different count.
  - `toneHz` already came from the shape side.
  - The row now carries `countsFrom: "shape side"`.
  - Why the old counters stood at 576 and 1402 all day was **not found**. Both looked live in the code here, and your telemetry is not on this computer.
- **The privacy walk** covers `cw_listen`, and its event count went from 82 to 83.

**Task 4, the disk** (`d686b172`):
- **Size limit:** automatic captures use **10 GB** at most, counting a GB as 2^30 bytes, the unit Explorer shows.
- **Making room:** before each piece or trouble capture is written, the oldest automatic capture goes first, judged by the UTC time in its folder name, until the new one fits. The capture being written is never deleted.
- **Free-space floor:** under **20 GB free**, nothing automatic is written. The line says `auto capture paused · disk under 20 GB free`. A drive that cannot be read counts as paused.
- **What may be deleted:** only folders directly under `captures\auto`, each checked by its full path first.
- **Found while testing:** after a capture ended for lack of room, the next tick began a fresh capture of the same session, which was then free to delete the piece just written. **A session the disk refused now stays stopped.** The line reads `auto capture stopped · no room under the 10 GB kept for automatic captures`.

**Records:**
- `PHASE_OUTCOME.md`, both copies: `## UNIT 549 - STEP 12`.
- `PHASE_STATUS.md`, both copies: names 549, and only that line changed.
- `Directory.Build.props`: 1.13.234.
- `CLAUDE.md` §1: a row above HM-DEC-252.
- `DECISIONS.md`: HM-DEC-253.

**Build** with `-warnaserror`: no warnings, no errors.

**Scoreboard:** 227 after every task, and its whole printed table is **identical to HEAD's**, line for line, all 45 lines.

**App carry-forward:** 278 of 278.

## 2. What the owner should expect

Rebuild and run as usual.

- **Leave the radio on W1AW's frequency, in CW, with Hamlet listening, and every scheduled session is recorded.**
  - Recording starts a minute before the session and stops two minutes after it.
  - It is kept in five-minute pieces, each a .wav with a .txt beside it saying what was scheduled and what the radio was doing.
  - Being near the frequency is enough: within half your filter, so within 250 Hz on a 500 Hz filter.
  - Moving the dial, changing mode, starting a scan, or stopping listening ends the recording, and its last sheet says why.
- **The line under the CW terminal header** reads `capturing W1AW · code practice · piece 3 · 12:40` while it records. Hover over it to see the folder. Otherwise the usual hint is there.
- **Trouble captures** happen on their own anywhere else while Hamlet listens in CW and no scan is running. Each keeps the last five minutes when one of these happens:
  - four or more senders are held for ten seconds;
  - any audio is lost before the decoder;
  - six stray letters (E, T, I, A, N or M, each printed on its own) come within ten seconds.

  Each kind waits ten minutes before it saves again. During a W1AW recording they are written on its sheet instead.
- **Where it lands:** `%AppData%\Hamlet\captures\auto\`, one folder per capture: `w1aw-2026-10-07-205900` for a session, and `trouble-2026-10-07-181500-senders` for a trouble capture.
  - **To zip one for the web session:** open that folder in Explorer, right-click the capture's folder, and choose **Send to > Compressed (zipped) folder**. Add the day's telemetry file from `%AppData%\Hamlet\telemetry\`.
  - A whole 45-minute session is about 260 MB and zips to less.
- **The disk:**
  - The `auto` folder never holds more than 10 GB, and the oldest captures go first to make room.
  - With under 20 GB free nothing automatic is recorded, and the line says `auto capture paused · disk under 20 GB free`.
  - Your own Record presses, the scan's catches and the telemetry are never deleted.
- **What will look wrong but is not:**
  - The first `cw_listen` row comes one second after listening starts, then every ten seconds.
  - `decode_quality` rows now say `countsFrom: shape side`, and their old `elementsResolved` field has become `marksStood`.
  - The scoreboard test itself is red at HEAD and still red: the random carrier prints at seed 5195, as recorded since unit 547. The score is 227.
- Pushed to `main`.

## 3. What you should see

**The tests**, all green:
- `EveryW1awSessionIsCapturedTests` (5): the start begins it, with three pieces continuous sample for sample across every boundary; the schedule entry is on every sheet; the dial leaving ends it; the window closing ends it; listening stopping ends it; no session, scanning or not in CW means no capture; the threaded path loses nothing; the tolerance is half the filter.
- `TroubleIsCapturedWhereItHappensTests` (6): four senders for ten seconds fires once, and again only after ten minutes; audio lost fires once and waits out its cooldown; six stray letters fire, while ANTENNA MEANTIME and their like do not; the saved audio is the 5 minutes before the trigger, sample for sample; nothing during a scan; during a W1AW capture the trigger is noted on its sheet.
- `TheDiskIsKeptTests` (4): at the cap the oldest automatic capture goes and nothing else; a manual capture is never touched, even when refusing; under the floor nothing is written and the line says paused; a W1AW capture makes room piece by piece and stops rather than eat itself.
- `WhatJunkLooksLikeTests` (3): the measurements quoted in section 1.
- `TheListenRowTests.AMinuteOfRows` (app): the rows below.
- `CallsignPrivacyTests` (count 83), `BindingHealthTests`, the app carry-forward line (278 of 278).

**A minute of `cw_listen` rows.** The audio is synthetic: a station at 600 Hz, 18 WPM, a second at 760 Hz from 20 s, noise over both. It runs on a fake clock, with half a second the capture never delivered at 34 s. The light word is fixed by the test, and the queue peak of 20 comes from the test feeding audio faster than real time.

```
{"audioLostMs":0,"audioLostLast10sMs":0,"audioLongestStallMs":0,"audioQueuePeak":19,"sendersHeld":0,"senders":[],"printedPitchHz":null,"lettersPrinted10s":0,"light":"reading","capture":null}
{"audioLostMs":0,"audioLostLast10sMs":0,"audioLongestStallMs":0,"audioQueuePeak":20,"sendersHeld":1,"senders":[{"pitchHz":600,"shape":0.72,"marks":38,"qualified":true,"printed":true}],"printedPitchHz":600,"lettersPrinted10s":11,"light":"reading","capture":null}
{"audioLostMs":0,"audioLostLast10sMs":0,"audioLongestStallMs":0,"audioQueuePeak":20,"sendersHeld":2,"senders":[{"pitchHz":600,"shape":0.74,"marks":75,"qualified":true,"printed":true},{"pitchHz":760,"shape":0.126,"marks":5,"qualified":false,"printed":false}],"printedPitchHz":600,"lettersPrinted10s":9,"light":"reading","capture":null}
{"audioLostMs":0,"audioLostLast10sMs":0,"audioLongestStallMs":0,"audioQueuePeak":20,"sendersHeld":2,"senders":[{"pitchHz":600,"shape":0.751,"marks":113,"qualified":true,"printed":true},{"pitchHz":760,"shape":0.4,"marks":46,"qualified":true,"printed":false}],"printedPitchHz":600,"lettersPrinted10s":12,"light":"reading","capture":"w1aw-2026-10-07-205900"}
{"audioLostMs":350,"audioLostLast10sMs":350,"audioLongestStallMs":500,"audioQueuePeak":20,"sendersHeld":2,"senders":[{"pitchHz":600,"shape":0.712,"marks":145,"qualified":true,"printed":true},{"pitchHz":761,"shape":0.448,"marks":87,"qualified":true,"printed":false}],"printedPitchHz":600,"lettersPrinted10s":10,"light":"reading","capture":"w1aw-2026-10-07-205900"}
{"audioLostMs":350,"audioLostLast10sMs":0,"audioLongestStallMs":500,"audioQueuePeak":20,"sendersHeld":2,"senders":[{"pitchHz":600,"shape":0.736,"marks":183,"qualified":true,"printed":true},{"pitchHz":760,"shape":0.451,"marks":127,"qualified":true,"printed":false}],"printedPitchHz":600,"lettersPrinted10s":10,"light":"reading","capture":"w1aw-2026-10-07-205900"}
```

**An example W1AW sheet**, piece 2 of the test's capture. The test runs at 100 samples a second, where the app runs at 48,000. Its app lines are a fake rig's single line; the app writes the full block.

```
captured   automatically, a W1AW session (work instruction 549)
session    code practice · slow · scheduled 2026-10-07 13:00 to 14:00 UTC (08:00 to 09:00 America/Chicago)
speeds     code practice at 5 to 15 words a minute (slow) or 10 to 35 (fast); CW bulletins at 18  (the schedule's own words)
w1aw       7047500 Hz on 40 m  (the dial within 250 Hz of it: half the radio's filter, or 250 Hz where unread)
window     12:59:00 to 14:02:00 UTC  (a minute before the scheduled start to two minutes after the scheduled end)
piece      2
started    2026-10-07 13:03:59 UTC
closed     2026-10-07 13:08:59 UTC
seconds    300.0
sampleRate 100
samples    30900 to 60900 on the audio clock  (the next piece begins at 60900, so nothing falls between them)
missing    0 samples  (audio the capture never received inside this piece; not filled)
continues  piece-03.wav

frequency  7047500 Hz  (the fake rig)
```

In the app, the block after the blank line holds the same lines a Record sheet writes: `frequency`, `band`, `pitch`, `speed`, `senders` and `audio`. Then comes a `composed` line giving when the app last composed them, then every rig field with its provenance. That block was not run against a radio here.

The last piece's sheet ends `ended      the dial left W1AW's frequency`, or whichever of the five reasons applied.

## 4. What's blocking us

- **The stray-letter trigger is narrowed from the order's figure.**
  - **Ruling:** a burst of junk is six or more letters of one or two elements, **each printed alone**, within 10 seconds.
  - **Why:** the order's six-in-ten-seconds fires on clean copy at every speed. Measured on text, plain English at 18 WPM reaches 15 such letters in ten seconds and a bulletin's text 19. Six in a row occurs in English and Q-code text as often as in the bench junk. Letters standing alone never exceed 2 in ten seconds in clean text.
  - **Rejected:** the literal figure, which would save a trouble capture every ten minutes of ordinary copy; and six in a row, which does not separate junk from clean text.
  - **The cost:** the lone-letter rule does not fire on the bench junk `TYAEE IEA RADIO EMII EEIOTS`, whose letters run together. The audio-lost trigger catches that kind.
- **The W1AW tolerance.**
  - **Ruling:** the dial is on W1AW's frequency within half the radio's filter width, or 250 Hz where the width is unread.
  - **Why:** the order named the W1AW button's tolerance, and the button has none. Half the filter is where W1AW's tone stays audible.
  - **Rejected:** an exact match, which a dial nudged to centre the tone would break; a fixed wide figure, which would record a neighbour on a wide filter.
- **Why `decode_quality` counts stood still** on the shack machine was not found. The row now reads the shape side, and the first `cw_listen` rows from the shack will show whether the shape side's letters move.
- **Memory:** the automatic capture's own five-minute ring adds about 58 MB while listening, on top of Record's tap.
- **Not done by this unit:** the scan's ear still runs a second chain on the capture thread while a scan is on (as before), and the old CW read guards are untouched.
- **The silence limit stays red at one letter,** and **the carrier limit at seed 5195,** as at HEAD.

### Asks still outstanding

- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
- **Unit 549, 2026-10-07:** the stray-letter narrowing above. It waits on the owner, and the change sits in `src/Hamlet.RadioEngine/Capture/TroubleWatch.cs`.
- **Unit 549, 2026-10-07:** the W1AW tolerance above. It waits on the owner, and the change sits in `src/Hamlet.RadioEngine/Capture/W1awSessionWindow.cs`.
