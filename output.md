## 1. What Claude did

Development computer, project gate `PROJECT: Hamlet` checked against `PROJECT_CARD.md`, the order's five checks and `Hamlet.sln`: Hamlet confirmed. Nothing here is evidence about the radio. Branch `main`. Run by hand: `SESSION.lock` taken at 18:30:34 and released at the end, nothing written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` touched, no box ticked in `PHASE_PLAN.md`. Version 1.13.232 to 1.13.233. Ruling HM-DEC-252, the number the order gave. Nothing was recorded under §12.1.

**Task 1, the live fault on the bench** (`78f87f33`):
- **How live audio reaches the chain:**
  - `WasapiAudioSource` opens the device with a 100 ms buffer. NAudio's shared-mode capture reads it about every 50 ms and raises one chunk of about 2,400 samples at 48 kHz on WASAPI's own thread.
  - The decoder and the detector were both subscribed straight to that event, so the whole chain ran inside the capture callback.
  - The CW decode has run there since its hand-off queue was taken out on 2026-09-23.
- **The reproduction:** W1AW's recording fed through the chain twice, the second pass with the first as history.
  - Clean feeds, in any chunk size, read whole.
  - Lost audio prints the junk's kind (the table is in section 3).
  - A late chunk is the same chunk to the chain, which counts samples and reads no clock.
- **The cause shown:** audio lost before it reaches the chain. That the shack machine lost audio that evening is not shown, since its telemetry is not on this computer.
- **Fixed, the first place:** `CwLiveFeed` takes the chain off the capture thread.
  - The callback feeds the tap, as before, and copies the chunk into a bounded queue (`AudioHandoff`, the queue the waterfall already uses).
  - The chain drains the queue on its own thread.
  - The longest callback fell from about 18 ms (the chain's worst 50 ms chunk) to 0.08 ms.
  - W1AW through the queue reads exactly what the chain reads fed directly (a test).
- **The second place, not built:** telling the gate where audio went missing, so that no letter measured across a hole prints.
  - Built twice and measured; in its best form it ignores holes shorter than half the sender's dit and drops only the letter a hole lies in.
  - It read better on stalls and worse on frequent short losses (section 3), so it was taken out.
  - The feed still counts every hole.
- **The cost** (section 3): 4.7% to 9.0% of real time on this machine, the worst 50 ms chunk 15 to 22 ms against the 100 ms buffer.
- The scoreboard reads 227, unchanged.

**Task 2, audio continuity counted** (`450b379e`):
- **The sheet:** a capture sheet line, `audio      lost 0 ms, longest stall 0 ms, queue peak 1`, with a note saying what each means.
- **Every verdict row:** `audioLostMs`, `audioLostLastMinuteMs`, `audioStallMs` and `audioQueuePeak`. The two tests that pin the row's fields gained the four.
- **The story line:** when audio went missing in the last minute it adds *"Some audio never reached the decoder, about 340 milliseconds of it in the last minute, so letters around then may be missing or wrong."*
- **The test** (`TheLiveFeedIsCountedTests.TheCountersReadWhatWasDropped`): a fake capture on a fake clock. It counts:
  - the loss after one 100 ms buffer of jitter;
  - the 400 ms stall;
  - the hole;
  - the chunks.

**Task 3, Record keeps longer** (`fa0f40d1`):
- A setting, `Record keeps, seconds`, 30 to 300 in steps of 30. It is the fifth row in the scan's settings popover (the `⋯` beside Scan), saved the moment it changes, in `settings.json` as `RecordSeconds`.
- The tap is sized when Hamlet starts listening, so a new length takes effect the next time it does.
- The Record button's hover says the length in words, *"Records the last 5 minutes"*, and the sheet's `seconds` line already says how long each recording is.
- **The cost:** the tap holds 32-bit floats, not 16-bit samples, so five minutes at 48 kHz is about **58 MB**, not 29 MB. A press copies it once more while it writes.

**Task 4, what the senders were** (`8cb3913f`): W1AW's recording after five minutes of band noise at its own floor, and a station 10 dB weaker at 650 Hz keying for the first four.
- The second station printed and was let go when it stopped.
- W1AW then took the terminal and read whole.
- No sender held from before took it, and there was nothing to fix.

**Records:**
- `docs\cw-scoreboard.md`: a row per task.
- `PHASE_OUTCOME.md`, both copies: `## UNIT 548 - STEP 12`.
- `PHASE_STATUS.md`, both copies: names 548.
- `Directory.Build.props`: 1.13.233.
- `CLAUDE.md` §1: a row above HM-DEC-251.
- `DECISIONS.md`: HM-DEC-252.

**Build** `-warnaserror`: no warnings, no errors.

**App carry-forward:** 277 of 278. The loss is `TheStopIsAlwaysOnScreenTests.AtEachOf354sNineSizesStopIsInTheStatusBarAndOnTheWindow`, the dispatcher-loop test unit 544 named, which passes in its own class (5 of 5).

## 2. What the owner should expect

Rebuild and run as usual.

- **The live W1AW junk was reproduced on the bench, by losing audio.** When pieces of audio go missing before the decoder sees them, its dahs come out as dits and its letters run together, which is exactly the `E I S A N` kind of junk you saw. The same recording fed whole reads clean every time.
  - Hamlet was doing all its CW decoding inside the sound card's own delivery call. A slow moment there costs audio the sound card throws away, and nothing told the decoder.
  - The decoding now runs behind a queue, on its own thread, so a slow moment costs nothing unless the decoder falls well behind, and then it is counted. Whether your shack machine was losing audio that evening I cannot see from here. The new audio line will say.
- **The new line on the capture sheet** reads like `audio      lost 0 ms, longest stall 12 ms, queue peak 3`. Look at **lost** first: anything over nought means the decoder missed audio. A **longest stall** near or over 100 ms means the sound card's thread waited long enough to lose audio. A **queue peak** that climbs into the twenties means the decoder is falling behind. The story line under the terminal also says so whenever audio went missing in the last minute.
- **To set Record longer:** press the small `⋯` beside **Scan**, set **Record keeps, seconds** to 300, then stop and start listening so it takes. Five minutes holds about 58 MB while Hamlet listens.
- **At the next W1AW session:**
  1. Set Record to five minutes.
  2. Start listening before W1AW starts.
  3. Press Record the moment junk appears.
  4. Send the capture and its sheet.

  The sheet's audio line will say whether audio was lost, and the five minutes will show where the extra senders came from.
- **The extra senders at 550 and 650 Hz:** in a bench case with a weaker station that stops before W1AW, nothing held over took the terminal. What they were on the air is still to be seen in a long capture.
- **The score:** 227 with W1AW's bulletin, 191 on your twelve recordings, the same before and after.

## 3. What you should see

**The live-feed table** (W1AW's recording, fed twice; the second pass, scored against `PE II AND TYPE IV RADIO EMISSIONS HOWEVER, THIS CME IS`):

| way of feeding | lost | the second pass prints | right, wrong of 44 |
|---|---|---|---|
| 10 ms chunks, nothing lost (the scoreboard's) | 0 | `EPE II AND TYPE IV RADIO EMISSIONS HOWEVER, THIS NME IS` | 43, 1 |
| 50 ms chunks, nothing lost (the live capture's) | 0 | the same | 43, 1 |
| 100 ms chunks, nothing lost | 0 | `S EPE II AND TYPE IV ...` | 43, 1 |
| 10 ms chunks, one in a hundred lost | 600 ms | the same as clean | 43, 1 |
| 50 ms chunks, one a second lost | 3,000 ms | `IS IME IS EEGE II AND TYAEE IEA RADIO EMII EEIOTS HOWEVER, TH` | 28, 9 |
| 50 ms chunks, a 200 ms stall every 5 s | 2,400 ms | `ND IS EPE II IND TYPE ■ RADIO EAISSIONS HOWEVER■ THIS` | 34, 4 |
| 50 ms chunks, a 200 ms stall every 2 s | 6,000 ms | `E■ II IND ■■ IVRADEO EAISSEONI HWWEUERT■ TSIS IME 5` | 25, 13 |
| late but not lost | 0 | the same as clean: the chain reads no clock | 43, 1 |

With the gate told where audio went missing (measured, not built):

| way of feeding | right, wrong |
|---|---|
| one a second lost | 20, 11 |
| a 200 ms stall every 5 s | 37, 3 |
| a 200 ms stall every 2 s | 29, 10 |
| one in a hundred lost | 43, 1 |

**The cost table** (this machine; 48 kHz in 50 ms chunks; W1AW's recording with synthetic stations added):

| stations | senders held at the end | per second of audio | of real time | worst 50 ms chunk | against the buffer |
|---|---|---|---|---|---|
| W1AW alone | 1 | 89.8 ms | 9.0% (with the first run's warm-up) | 20.0 ms | 100 ms |
| W1AW and 2 more | 3 | 57.0 ms | 5.7% | 21.8 ms | 100 ms |
| W1AW and 5 more | 3 (six did not stand) | 47.0 ms | 4.7% | 15.4 ms | 100 ms |

The margin here is about five times the worst chunk. Through the queue, the callback's longest is 0.08 ms.

**The counters' test** (a fake capture, a fake clock, 50 ms chunks at 8 kHz):

| moment | lost | longest stall | queue peak | holes | chunks |
|---|---|---|---|---|---|
| after 20 chunks on time | 0 ms | 0 ms | 19 | 0 | 20 |
| after 400 ms of nothing, then a chunk whose place jumps a second | 250 ms (350 lost, less the 100 ms of jitter allowed for) | 400 ms | 21 | 1 | 22 |

The queue peak of 19 is the test delivering faster than its worker reads. Live, chunks come every 50 ms.

## 4. What's blocking us

- **Whether the shack machine lost audio** on 2026-10-06 is not shown. The next W1AW session's audio line answers it.
- **The 550 and 650 Hz senders on the live sheet** wait on a five-minute capture from the start of listening.
- **The scan's ear** runs a second chain on the capture thread while a scan is on. It was not moved behind a queue in this unit.
- **A hole in the audio is not told to the gate.** The measured rule was mixed, and a better one would need to know which letter a hole really cut.
- **The old CW read guards** are still untouched, and still wait on your ruling about replacing them with the scoreboard.
- **`TheLoneLettersInsideWordsStillPrint("DE DE")`** is red, as before this unit.
- **The silence limit stays red at one letter,** and **the carrier limit at seed 5195,** as at HEAD.

### Asks still outstanding

- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
