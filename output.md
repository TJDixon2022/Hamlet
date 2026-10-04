## 1. What Claude did

- **Session:** development computer, Claude Code. The prompt claimed `PROJECT: Hamlet`, and the gate was verified against
  `PROJECT_CARD.md`, `CLAUDE.md` and `Hamlet.sln`. Nothing in this report is evidence about the radio.
- **Work instruction 537**, run by hand on `main`:
  - SESSION.lock was taken and released.
  - Nothing was written to RUN_LEDGER.md, nothing under `tools\arbiter\` was touched, and no box was ticked.
  - Nothing was keyed or transmitted, and `tests\fixtures\cw\captured\` was not touched.
- **Tag:** HEAD was tagged `before-cleanup-2026-10-04` and pushed before anything was deleted.
- **Version:** 1.13.221 → 1.13.222.

**Task 1: the repo is clean** (commit `98e93cde`).
- **Deleted:**
  - the `TestResults` folders, the `.unit*` scratch folders and `artifacts\`;
  - the four `tools\arbiter.bak-*` copies;
  - at the root: commit stubs, one-off scripts, `.trx`, `.obj` and `.bak` files;
  - `.run-unit\`, except its `reports\`, `allowed.txt` and `denials.txt`.
- **Archived:**
  - 31 root documents moved to `docs\archive\`;
  - 67 unit files and folders moved from `docs\` to `docs\archive\units\`.
- **`.gitignore`:** gains `.unit*/` and `.run-unit/*`, with those three exceptions.
- **`.run-unit\fldigi`:** this gitlink was removed from the index.
- **Arbiter state files:** 18 under `.run-unit\` had uncommitted changes the tag does not hold. They were copied to the
  session scratchpad before removal.
- No rm or git rm was refused.

**Task 2: the capture sheet reads the shape side** (commit `0beaf955`).
- **The new reading:** `CwShapeSideReading` is built in `CwSenderGate` and exposed as `CwDecoder.ShapeSide`. It holds:
  - the printed sender's pitch and dit;
  - letters printed and unreadable;
  - marks stood;
  - the senders held, each with pitch, shape and marks;
  - the last 120 s of letter and mark times.
- **What reads it:** the sheet and the roster take the reading at the press.
- **Retired tests:**
  - six test files of the dropped lines;
  - nine methods across three other files.
- **New tests, all passing:**
  - `TheSheetReadsTheShapeSideTests` (2 tests);
  - one engine test, `TheShapeSideReadingCountsWhatThePrinterPrinted`.

**Task 3: three long-standing reds pass** (commit `5e9830af`).
- **British spelling (VoiceTests):** the two `centre`s were in the old `toneHz` caption, which went with task 2.
- **Two scope tone-line tests (TheScopeShowsTheMarksTests):**
  - They said `no keying` because they stopped at 1.73 s, before five marks stood.
  - They are re-pinned to 2.33 s, Q's last dah.
- **The decision log (DecisionLogOrderTests):**
  - 22 rulings gain their `CLAUDE.md` index rows: HM-DEC-166, 189 to 208, and 210.
  - The six ids with no ruling are named in the test as known gaps: 105, 136, 182, 216, 220 and 222.

**Task 4 was dropped, as the order allows.** A list of every red needs whole-suite runs. HM-DEC-155 rules them out, and
the engine suite has never completed one.

**Recorded:**
- HM-DEC-241, *The cleanup: the repo, the capture sheet and the red tests*, in `DECISIONS.md` and the `CLAUDE.md` §1
  index;
- PHASE_OUTCOME `## UNIT 537 - STEP 12` and PHASE_STATUS, both copies;
- a 537 row in `docs/cw-scoreboard.md`.

## 2. What the owner should expect

- **The scoreboard read 205 of 244 after every task.** Nothing that reads was changed. Its one red is still the carrier
  limit on seed 5195, as at HEAD.
- **Build:** 0 warnings, 0 errors.
- **App carry-forward:** 276 of 278. The two losses pass when run alone, and they are the known dispatcher-loop flake:
  - `TheRstIsYoursToCorrectTests`;
  - `TheStopIsAlwaysOnScreenTests`.
- **A capture now writes a shorter sheet.** Pitch, speed, tone peak and duty name the printed sender. A new `senders`
  line lists who the gate held. With nobody printed, those lines say `nobody printed`.
- **The roster:** a capture's case row carries the printed pitch and speed, and `shape 0.xx` where the fit used to be.
- **The working tree** is 8,355 files and 4.7 GB. Almost all of it is ignored build output under `src\` and `tests\`.
- **Recovering a removed file:** `git checkout before-cleanup-2026-10-04 -- <path>` brings it back.

## 3. What you should see

**The counts:**

| | before | after |
|---|---|---|
| tracked files | 11,056 | 2,499 |
| working tree | 19,673 files, 6.9 GB | 8,355 files, 4.7 GB |
| `.git` | 608 MB | 608 MB (history kept) |
| scoreboard | 205 of 244 | 205 of 244 |

**The sheet's fields:**

| before | after |
|---|---|
| `toneHz` (old tracker's tone) | dropped; `pitch` is the printed sender's, from its own marks |
| `heldPeak` | dropped |
| `tonePeak` (old tracker's bin) | at the printed sender's pitch |
| `unkeyed`, `elements`, `characters` | dropped; `inThis` gives letters, unreadable and marks inside the recording |
| `decoderWpm`, `speed` (old estimator) | `speed`, from the printed sender's dit |
| `spanLlr`, `arbiter`, `competing`, `reading`, `elementHz` | dropped |
| (none) | `senders`: every sender held, with pitch, shape and marks, and which one printed |
| `duty` (old tracker's bin) | at the printed pitch |
| (none) | `sinceLast`: letters and marks since the previous capture |
| keying line | unchanged |

**The red tests this unit touched:**

| test | was | now | why |
|---|---|---|---|
| `VoiceTests` British spelling | red | green | the `centre`s went with `toneHz` |
| `TheToneLineReadsThePitchWhileAMarkIsUp` | red | green | re-pinned past the fifth mark |
| `TheScopeIsOnTheCwTabBesideTheButtonsAndPaints` | red | green | same |
| `DecisionLogOrderTests` gaps | red | green | 22 rows added, 6 gaps named |
| `EveryKeyedMarkIsABarWithItsLength` | red | red | dah-to-dit width 4.0 against 2.4 to 3.6; see section 4 |
| scoreboard carrier limit, seed 5195 | red | red | unchanged by design |
| the old sheet's tests (6 files, 9 methods) | mixed | retired | their lines are gone |

## 4. What's blocking us

- **`EveryKeyedMarkIsABarWithItsLength` is still red.** The detector reads the synthetic dits short, so the bars' dah to
  dit ratio is 4.0. That is a reading question, so it was left.
- **The old decoder still reaches the app:**
  - `DecodeReport.Level` drives the level meter, `DecoderStory` and `KeyingAdviceIsUseful`;
  - `Report.Competitor`;
  - `WordsPerMinute`/`SpeedProof` feed `DetectedWpm` and the transmit speed;
  - `mixing.PrintingHz` drives the scope.
- **The red-test census (task 4)** waits on a way to run a whole suite that HM-DEC-155 allows.
- **Not named by the order and left at the root:**
  - `.claude\`, `.vs\` and `__pycache__\`;
  - `assets\`, `data\`, `docs\`, `graphify-out\`, `scratch-audio\`, `src\`, `tests\` and `tools\`;
  - `.unit290-commit.txt` and `.unit362-carry.tmp`.
- **Uncommitted local files from before this unit are untouched:**
  - `PARKED.md`, `RUN_LEDGER.md`, `WORK_INSTRUCTIONS.md` and `.run-unit\denials.txt`;
  - the four `.run-unit\reports` files;
  - `tests\fixtures\cw\captured\cases-2026-10-0{2,3}.txt`.

### Asks still outstanding

- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
