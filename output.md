## 1. What Claude did

Development computer, project gate `PROJECT: Hamlet` checked against `PROJECT_CARD.md`, the order's own five checks and `Hamlet.sln`: Hamlet confirmed. Nothing here is evidence about the radio. Branch `main`. Run by hand: `SESSION.lock` taken at 10:36:42 and released at the end, nothing written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` touched, no box ticked in `PHASE_PLAN.md`. Version 1.13.230 to 1.13.231. Ruling HM-DEC-250, the number the order gave. Nothing was recorded under §12.1.

**Task 1, a dah that breaks in two** (`84d40dc2`): traced, and the rule did not ship.
- **Where the break happens:** in the signal, read through the sender's own window. Each of the nine dahs has a real dip in the middle, 8 to 20 dB deep and 8 to 24 ms long. The detector stands two marks around it, and the gate prints two dits.
- **The order's rule:** a gap under half the sender's own gap inside a letter is not a gap. It meets the break in the gate, where gaps are judged.
  - Applied from the sender's first gaps, it scored 154.
  - Applied only once the sender had shown its two kinds, its letter gaps and ten inside gaps, it scored 186.
  - At best it joined 2 of the 9 dahs (32.65 and 49.17 s), and it joined real dits elsewhere: `GIT` read `GTT` and `H` read `U`.
- Reverted. The trace test stays.

**Task 2, Farnsworth** (`1afcec1b`): found and fixed, but not shipped on the spaces bar.
- **From cold**, W1AW's form at 18 WPM, spaced for 5 and 7.5 WPM, already reads as words through the app's chain.
- **After ordinary 18 WPM sending** it prints letter by letter, as on the air: `X I S F R O M O C O B R 2 0 2 4`.
- **The cause:** the sender's old letter gaps (about 200 ms) and word gaps (about 470 ms) stay in its window. The word line falls between them, far under the new 1,574 ms letter gaps, until the old gaps age out.
- **The fix tried:** three gaps in a row, each past the word line by √(7/3), are a new spacing, and the sender's gaps are taken from the first of them. Three one-letter words in a row is what the gate already refuses as sending. The slow section then reads as words.
  - It scored 191, with 55 of 77 spaces and 2 added. Spaces right did not rise, so it fails this task's bar, and it was reverted.
  - The change is kept as `.run-unit\unit546-spacing.patch` (not committed) and described in section 4.
- **A slip, measured and not shipped:** my first version also turned the speed-now filter's `>` into `>=`. That keeps the one gap that ends where the newer-speed marks begin. Alone, it reads 197 (211 right, 12 wrong) but 54 spaces.

**Task 3, the spaces that slipped** (`1416164a`): found; nothing shipped.
- Spaces right is 55 whether the neighbour judgement of gaps is on or off. The three spaces were lost to **three lone letters dropped**.
- The rule drops the opening `E` of 221745 and of 221805, and with the letter gone the space beside it is not counted.
- With that rule off: 191 (210 right, 17 wrong), 58 of 77 spaces, 4 added. Spaces added rises, so it fails the bar. Task 2's change does not bring the spaces back.

**Task 4, the weak fast station** (`24c6925e`): traced, not fixed. It was the drop candidate.
- The chain loses this station in the detector.
- At 25 WPM and 10 dB, on the recording and on a synthetic call, it keeps about 5 dahs of about 40.
- With the edge test off, the 10 dB call keeps 34 dahs and reads its second half. So the flat-top test most likely breaks each dah into pieces, and the edge test then refuses each piece.
- Nothing plain to fix: the scoreboard needs the edge test (89 without it).

**Task 5, tidy** (`0c74643d`):
- `neighbour` in the rule's name and `centre` in the scan's advancing line are now American, and `VoiceTests` passes 5 of 5.
- `MostNoiseBarsHaveNoEdges` is retired with the ten rules (HM-DEC-245).
- The old CW read guards are not touched.

**Records:**
- `docs\cw-scoreboard.md`: one row per task.
- `PHASE_OUTCOME.md`, both copies: `## UNIT 546 - STEP 12`.
- `PHASE_STATUS.md`, both copies: names 546.
- `Directory.Build.props`: 1.13.231.
- `CLAUDE.md` §1: a row above HM-DEC-249.
- `DECISIONS.md`: HM-DEC-250, *Broken dahs, Farnsworth gaps, and the spaces*.

**Build** `-warnaserror`: no warnings, no errors.

**App carry-forward:** 277 of 278. The loss is `TheRecordNamesTheSubModePressedTests.TheCqPressWritesTheLabelTheOperatorPressed("Olivia")`, which passes in its own class (12 of 12), and this unit changed nothing in the app.

## 2. What the owner should expect

Rebuild and run as usual. **Nothing you read changes**: no task met its bar, so the decoder reads exactly as before. The score stays at 191 (208 letters right, 15 wrong, 2 invented), with 55 of 77 spaces right and 3 added. Both hard limits are red, as before.

- **The broken dahs.** The strong 860 Hz station's dahs really do dip in the middle, deep and brief, perhaps its keying or a flutter on the path. The detector hears that as two short marks. Joining them across a short gap also joined real dits on your recordings, so it was put back. Those dahs will still read as two dits.
- **Farnsworth.** W1AW's slow code reads as words when Hamlet starts listening at the slow section. It reads letter by letter when Hamlet was already listening to ordinary sending, which is what you saw. The fix was found, and it reads the slow section as words without costing anything on your recordings. It did not ship only because none of your recordings holds a Farnsworth sender, so spaces right could not rise. Whether to ship it is your call, in section 4.
- **The three spaces.** They were never lost to the neighbour judgement. The rule that drops three lone letters in a row drops a real `E` at the start of two transmissions, and the space beside it goes too. Turning that rule off gets them back but adds a space, so it stays on.
- **The weak fast station** (26 WPM, about 11 dB) is still junk. Its dahs break into pieces in the detector, and fixing that is detector work for another unit.
- **Tidied:** two British spellings, and one stale test.

## 3. What you should see

**The scoreboard after each task:**

| unit | right | wrong | invented | score | printed in silence | spaces | what changed |
|---|---|---|---|---|---|---|---|
| HEAD | 208 of 244 | 15 | 2 | **191** | 1 | 55 of 77, 3 added | |
| 546 task 1 | 208 of 244 | 15 | 2 | **191** | 1 | 55 of 77, 3 added | the join was measured (154; 186 once known) and not shipped |
| 546 task 2 | 208 of 244 | 15 | 2 | **191** | 1 | 55 of 77, 3 added | the new-spacing rule was measured (191, 55 of 77, 2 added) and not shipped |
| 546 task 3 | 208 of 244 | 15 | 2 | **191** | 1 | 55 of 77, 3 added | three lone letters dropped off was measured (191, 58 of 77, 4 added) and not shipped |
| 546 task 4 | 208 of 244 | 15 | 2 | **191** | 1 | 55 of 77, 3 added | traced only |
| 546 task 5 | 208 of 244 | 15 | 2 | **191** | 1 | 55 of 77, 3 added | spellings and a retired test |

**The nine dahs** (the plain read's dah, the marks the detector stood, and the dip on the sender's own window):

| at | plain dah | stood | gap | dip | joined by the rule (sender known) |
|---|---|---|---|---|---|
| 3.54 s | 174 ms | 50 + 85 ms | 15 ms | 8.5 dB | no: sender not yet known |
| 31.62 s | 150 ms | 67 + 50 ms | 24 ms | 17.6 dB | no |
| 32.65 s | 160 ms | 61 + 81 ms | 15 ms | 19.9 dB | **yes**, 157 ms dah |
| 40.69 s | 162 ms | 65 + 66 ms | 22 ms | 10.8 dB | no |
| 49.17 s | 155 ms | 68 + 56 ms | 19 ms | 13.8 dB | **yes**, 142 ms dah |
| 52.14 s | 166 ms | 69 + 69 ms | 20 ms | 10.3 dB | no |
| 53.63 s | 152 ms | 23 + 107 ms | 8 ms | 8.8 dB | no |
| 69.54 s | 167 ms | 72 + 75 ms | 13 ms | 8.2 dB | no |
| 72.11 s | 165 ms | 71 + 65 ms | 19 ms | 9.3 dB | no |

The main catch read 109 of 152 against its pending reference at HEAD, and 106 with the join on.

**The Farnsworth test** (`AFarnsworthBulletinReadsAsWordsTests`):

| case | HEAD reads | with the new-spacing rule |
|---|---|---|
| 18/5, from cold | `TEXT IS FROM OCTOBER 2024 QST PAGE 50 5 WPM TEXT FOLLOWS` | same |
| 18/7.5, from cold | the bulletin whole | same |
| after `VVV VVV QST DE W1AW` at the same spacing | whole | whole |
| after 18 WPM ordinary sending | `... CODE X I S F R O M O C O B R 2 0 2 4 Q S P A G 5 0 5 W P M XT FOLLOWS` | `... CODE XT IS FROM OCTOBER 2024 QST PAGE 50 5 WPM TEXT FOLLOWS` |

- The existing Farnsworth cases (the call at 5, 10 and 13 WPM, and the answer at 5) read whole, as at HEAD.
- `TheLoneLettersInsideWordsStillPrint("DE DE")` was already red; it failed before any source change in this unit.

**The three spaces** (with three lone letters dropped off and then on):

| stretch | reference | rule off | rule on |
|---|---|---|---|
| 221745 at 502 Hz | `E E DAND ON 40M TONITE` | `E IAND TMN 4MEMM` (9 of 11 spaces) | `IAND TMN 4MEMM` (8 of 11) |
| 221805 at 601 Hz | `ET ON 40T S THESE DAYS` | `E 40T U THESE DAYS` (7 of 10) | `40T U THESE DAYS` (5 of 10) |

**The weak fast station's trace:**

| case | plain read | detector marks | plain marks it missed | dahs | gate | prints |
|---|---|---|---|---|---|---|
| `143906` | 102 marks, dit 47, dah 154 ms | 68 | 43 | ~7 | shape 0.124, dit read 24 ms | `I II E NI ST TAEEKEEIEEII IMESE` |
| synthetic 25 WPM, 12 dB | 110 marks | 98 | 12 | ~45 | shape 0.466, dit 32-42 ms | `SSDE ■EEI EEINKEEWEAW K QSY QSY DE W1AW W1AW K` |
| synthetic 25 WPM, 10 dB | 110 marks | 70 | 43 | ~5 | shape 0.245, nothing printed | `` |
| 10 dB, edge test off | | 98 | | 34 | | `G EE I IE I E V SSY QSY DE W1AW W1AW K` |

The other mark tests, switched off one at a time (shape, narrowness, settle, own window), leave the 10 dB call at 70 marks or so, with 4 or 5 dahs, printing nothing.

## 4. What's blocking us

- **Whether to ship the Farnsworth fix despite the spaces bar.**
  - Ruling: three gaps in a row, each past the sender's word line by √(7/3), are a new spacing, and the sender's gaps are taken from the first of them.
  - Reasoning: it reads W1AW's slow sections as words after ordinary sending, and costs nothing on your recordings (191, 55 of 77, 2 added). Spaces right cannot rise on recordings that hold no Farnsworth sender.
  - Rejected: holding it until a slow-section recording is in the tree, which leaves the air fault in place meanwhile.
  - The change is in `.run-unit\unit546-spacing.patch`, uncommitted.
- **The old CW read guards are not touched**, as the order said. `TheAdjudicatedReadingsKeepReadingTests`, the 08-25 cases of `TheCapturesThatDecodeKeepDecodingTests` and `CwFixtureTests.TheCleanRecordingsDecodeExactly` read the corpus R88 bans, and were pinned to the removed decoder. You have not yet ruled on replacing them with the scoreboard.
- **The weak fast station** needs detector work: a dah's top at low contrast, so the flat-top test stops breaking it into pieces.
- **The broken dahs** need a rule that sees a dip inside one keyed tone without joining two dits. The gap alone does not separate them on these fists.
- **The speed-now filter's `>=`** read 197 at 54 spaces. Recorded as found, not proposed.
- **`TheLoneLettersInsideWordsStillPrint("DE DE")`** was red before this unit.
- **The silence limit stays red at one letter,** and **the carrier limit at seed 5195,** as at HEAD.

### Asks still outstanding

- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
- **Unit 546, 2026-10-06:** whether the Farnsworth new-spacing rule ships despite the spaces bar. It waits on the owner, and the change sits in `.run-unit\unit546-spacing.patch`, uncommitted.
