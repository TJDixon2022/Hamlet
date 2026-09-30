## 1. What Claude did

**Surface and gate.** Claude Code on the development computer at `C:\Source\HamLet`, branch
`main`. The prompt carries `PROJECT: Hamlet`, and all five of section 0's checks hold. Hamlet
confirmed. Nothing in this report is evidence about the radio.

**Which unit this is.** The order is headed 502, but 502's shape score (its section 3a), R110 and
HM-DEC-206 were already committed in `6f1d3d0c`, and 503 came after it. The file on disk had been
edited at 09:51 to add three new tasks: 3, 3b and 3c. I asked; the owner ruled that I run the new
work as **unit 504**, leave 3a as done, and duplicate no record. The order carries no `ISSUED:`
line, no status instruction, no phase count and no `Asks still outstanding` (§9.6). The status was
written regardless, and the queue below is carried from unit 503's report.

**Run by hand, outside the loop.**
- `SESSION.lock` was taken through `tools\arbiter\lock.bat take` and released the same way. The
  script wrote it to `C:\Source\ClaudeProjectStatus\SESSION.lock`, its own default root.
- Nothing was written to `RUN_LEDGER.md`, and nothing under `tools\arbiter\` was touched.
- No box was ticked and nothing was added to `PHASE_PLAN.md`.
- No recording, fixture, floor or telemetry was read.
- Nothing under `.run-unit\` was committed, and nothing keys or transmits.

**Version.** 1.13.190 to 1.13.191.

**The changes, file by file** (all in `fa66b24f`):
- **`src/Hamlet.RadioEngine/Cw/CwEnvelopeDetector.cs`:**
  - The walk back to where a mark's tone rose now stops at hop zero (task 3).
  - Three switches for work instruction 492's delivery rules: `MarksNeedPromptness`,
    `MarksNeedKeyUp` and `MarksNeedOneCall`. All are on by default, so each gate can be turned off
    in turn (task 3b).
- **`src/Hamlet.RadioEngine/Cw/CwRunReader.cs`:** the letter gap is now the lowest cluster of at
  least three gaps between runs. A smaller cluster below it is read against it (task 3c).
- **`tests/.../FarnsworthAndLoneLettersTests.cs`:** `LoneMarksAfterACallPrintNothing` is back on
  seed 5035. It threw `IndexOutOfRangeException` before the fix and reads the call alone after.
- **`tests/.../WhichGateTurnsAwayW1awTests.cs`, new:** the gate table and the on-air differences.
- **`tests/.../TheLetterGapHoldsTests.cs`, new:** the owner's hesitation case, a stretched gap, a
  dropped dit, and a hand fist scattered a fifth each way.
- **Records:** both `PHASE_OUTCOME.md` copies (`## UNIT 504 - STEP 12`), both `PHASE_STATUS.md`
  copies, `Directory.Build.props`, `DECISIONS.md`, and `WORK_INSTRUCTIONS.md` as it arrived.

**The letter-gap rule and its numbers, the author's.**
- **Three gaps make a sustained change.** That is `MeasuredRunGaps`, the count a sender already
  shows before its letter gap is used at all. A sender who really speeds up or slows down makes a
  new cluster of three within three letters. One odd gap never does.
- **The line between an odd gap and a new one stays at √(7/3), about 1.53.** A hand's gaps scatter
  by tens of percent, which is well inside a factor of 1.53.
- **The 10-to-20 WPM case still follows the change.** Its test is green.

**Recorded: HM-DEC-208**, in full:

> **One odd gap does not move a sender's letter gap.** Tim, in the work instruction, 2026-09-30:
> *"once a sender's letter gap has been measured on real letters, one gap does not move it. A
> sustained change over several letters does."* His screen that day, a hand-sent QSO on 40 m, read
> twenty letters of clean English and then every letter as a word of its own.
>
> **What it ends.** A hesitation inside a letter splits it, and the gap between the halves can sit
> far enough under the sender's letter gaps to be a cluster of its own. The run reader took the
> lowest cluster of a sender's gaps between runs as the letter gap, so that one gap became the
> letter gap, the word boundary fell under every real letter gap, and every letter printed as a word
> for the forty gaps the sender remembers.
>
> **What is built.** The letter gaps are the lowest cluster holding at least three gaps between
> runs, the count a sender already shows before its letter gap is used at all; a smaller cluster
> under it is read against it rather than measuring it. Where no cluster holds three, the lowest
> stands as before. Two clusters are still told apart by the square root of seven thirds, past the
> tens of percent a hand's gaps scatter by.
>
> **Whose words are whose.** The ruling is Tim's; three as the count of a sustained change, and
> keeping the cluster ratio as the line between an odd gap and a new one, are the author's under
> work instruction 504, and overrulable.

**Build and tests.**
- **Build:** `Hamlet.sln` with warnings as errors, 0 warnings, 0 errors.
- **Reader and detector tests: 49 of 52.** This covers the reader, detector, shape, speed,
  Farnsworth, gate and letter-gap tests. The three reds were also red at HEAD, checked with this
  unit's source changes stashed:
  - `AMarkIsTheEnvelopeOverAThresholdTests`' two cases.
  - `BurstsBetweenLettersDoNotSetTheSpeed`.
- **App carry-forward: 277 of 278.** The one loss, `Unit376TheTopBandTests`, took 1 ms and is
  green alone.

## 2. What the owner should expect

- **Rebuild.**
- **The CW tab no longer crashes the detector** in the first seconds of audio.
- **A sender who hesitates once no longer turns into one letter per word.** Before, one odd gap
  meant every letter after it printed alone, for about forty letters. Now only the letter he
  hesitated in reads wrong, usually as two letters, and the rest reads normally.
- **The W1AW zero is not fixed. The measurement says the fault is not in the mark gates.**
  - A synthetic W1AW at 725 Hz in your 500 Hz filter reads whole with every gate on.
  - No single gate turned off changes that.
  - What differs on the air is still to be found. The best lead is that the `marks` count on the
    verdict row only counts the one bin the detector is watching. If that bin is off the station,
    the row says keying with 0 marks.
- **Very ragged hand sending, with gaps about a third off, still misreads.** That fault is in where
  letters split, not in the spaces, and this unit didn't change it.

## 3. What you should see

**Task 3b's gate table.** 18 WPM call at 725 Hz, 65 marks sent. "Own marks" means marks within a
bin of 725 Hz and within 6 dB of the loudest there. "Most 4 s" is the highest `MarksLast4s` read
on any hop.

| Passband | Gate off | Own marks | Most 4 s | Reads |
|---|---|---|---|---|
| whole band, 100 to 3000 Hz | none | 66 | 18 | `CQ CQ DE N0CALL N0CALL K` |
| whole band | 492 promptness | 66 | 18 | whole |
| whole band | 492 key-up | 66 | 18 | whole |
| whole band | 492 one-call | 652 | 18 | placeholders only |
| whole band | 497 edges | 66 | 18 | whole |
| whole band | 498 narrowness | 66 | 18 | whole |
| whole band | 502 shape | 66 | 18 | whole |
| whole band | all | 686 | 18 | placeholders and one E |
| 600/500, bins 350 to 850 Hz | none | 66 | 18 | whole |
| 600/500 | each of promptness, key-up, edges, narrowness, shape | 66 | 18 | whole |
| 600/500 | 492 one-call | 645 | 18 | placeholders only |
| 600/500 | all | 662 | 18 | placeholders only |
| 600/500, scope pointed at 725 | none, or any one but one-call | 66 | 18 | whole |
| 600/500, scope pointed at 600 | none | 66 | 15 | whole |
| 600/500 at 48 kHz | none | 65 | 18 | whole |

- **The 66th mark** is one mark split in two, and the reader joins it back.
- **Narrowness on a 725 Hz tone in a 500 Hz filter centred on 600** reads one side, 425 Hz. The
  1025 Hz side is off the bins. Every mark scores the full narrowness of 1.00.
- **No gate turns away the synthetic marks.** One-call off makes things worse, not better, because
  it lets every key-edge fragment through as a mark.

**Task 3c's cases, before and after.**
- **The owner's screen, rebuilt.** A hand sender at 20 WPM, four dits between letters and nine
  between words, hesitating two and a half dits inside the Y:
  - Before: `NICEL TW I N T O M K T O Y O T A P R I U S B O T H W I T H A N D W I T H O U T T H E HYBRID ENGINE RUNNING`.
  - After: `NICELTW INTO MK TOYOTA PRIUS BOTH WITH AND WITHOUT THE HYBRID ENGINE RUNNING`.
- **The order's own two cases were already green at HEAD, not red.** Both read the same before and
  after:
  - One gap of a word gap and a half after INTO: `NICELY INTO MK TOYOTA PRIUS BOTH WITH`.
  - A dit dropped from the L: `NICEAEY INTO MK TOYOTA PRIUS BOTH WITH`.
  - The stretched gap stays in its own cluster above the letter gaps. The dropped dit leaves a gap
    three dits long, which is just a letter gap.
- **A hand fist scattered a fifth each way** reads the whole QSO, with or without the stretched gap.
- **At three tenths scatter it misreads**, for example `NICELY FTO ■TOYOK PRIUSBOTH■...`, and it does
  so at HEAD too. That fault is in the letter boundaries and is not in this unit's tests.

**Task 3.** Seed 5035: `the call then T E T T E` reads `CQ CQ DE N0CALL N0CALL K`.

**Every existing case** reads as at HEAD. That covers the calls at 5, 10, 18 and 35 WPM, the
Farnsworth cases, the speed change, the two stations, `TEST DE W1AW K`, `DE DE`, the lone and stray
marks, the shape cases and the noise tests.

## 4. What's blocking us

Nothing blocks. What is left, a line each:
- **The W1AW zero on the air is still unexplained.** Next step: put the watched bin's pitch and the
  pointed pitch on the verdict row beside `marks`, so the next W1AW press shows whether the watched
  bin is on the station.
- **`MarksLast4s` counts the watched bin's paired bars, not the marks handed to the reader.** Its
  name suggests the second. Whether the row should count the marks handed out is a choice for the
  owner.
- **Hand sending scattered by about a third misreads on letter boundaries.** The character boundary
  comes from the dit and the gap smear, not from the sender's measured gaps.
- **Two worktree folders from unit 503, `C:\Source\HamLet-wt503` and `C:\Source\HamLet-wt503b`,**
  are still safe to delete by hand.
- **Pre-existing reds, not this unit's:**
  - `AMarkIsTheEnvelopeOverAThresholdTests`' two cases.
  - `BurstsBetweenLettersDoNotSetTheSpeed`. This one is red at HEAD and was not named by unit 503.
  - The app reds unit 503 listed, which were not re-run here.

### Asks still outstanding

- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25, waiting on
  the owner; no change sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8
  record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, so nothing on it is ever
  revised, at the cost of seconds of lag. On the run path, now the only path to the screen, the
  terminal shows only settled text. The ask stands only for the timing-only path, which no longer
  reaches the screen; no change for it sits in the tree.
