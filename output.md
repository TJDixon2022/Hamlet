## 1. What Claude did

Development computer, project gate `PROJECT: Hamlet` checked against `PROJECT_CARD.md`, the order's five checks and `Hamlet.sln`: Hamlet confirmed. Nothing here is evidence about the radio. Branch `main`. Run by hand: `SESSION.lock` taken at 17:31:22 and released at the end, nothing written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` touched, no box ticked in `PHASE_PLAN.md`. Version 1.13.231 to 1.13.232. Ruling HM-DEC-251, the number the order gave. Nothing was recorded under §12.1.

**Task 1, a station's shadow** (`e342d2be`): the hypothesis was tested and not borne out. W1AW's stretch joins the board.
- **The recording, through the app's chain at the radio's own pitch and filter:**
  - read cold, the gate holds **one** sender, at 600 Hz, and prints nothing at another pitch;
  - heard three and six times end to end, which is minutes of the same station as the live session had, it still holds one sender with no handover.
- **The detector's candidates beside W1AW:**
  - 4 at 550 Hz and 7 at 650 Hz in 30 s, at about -25 dB, the noise level of every other pitch;
  - **none begins and ends in step with a W1AW mark.**
- **A strong call keyed hard**, with no shaping, holds one sender and reads whole (a test).
  - Steady tones 50 Hz either side, keyed with it, are not a key's sidebands: they beat with the call as 50 Hz amplitude modulation and the call prints nothing. That model was set aside.
- **What follows:** the 550 and 650 Hz senders on the live sheet were built in the two and a half minutes before the 30 s the recording holds. No change in the tree could be named as letting skirts stand, because none was shown. No shadow rule was built.
- **The board:** `cw-2026-10-06-212015` joins as its thirteenth stretch, high confidence, with the reference the order gave.
  - It reads 40 of 44 and 10 of 10 spaces. The 4 wrong are the cold-start opening, `E NE II AEED` for `PE II AND`; with history in front the same audio reads `PE II AND`.
  - **The total with it is 227** (248 of 288 right, 19 wrong, 2 invented, 65 of 87 spaces, 3 added). **Without it, 191**, as at HEAD.
- The recording and its sheet were already committed (`af1277e8`).

**Task 2, Farnsworth ships** (`f8135413`):
- The rule was applied from `.run-unit\unit546-spacing.patch` with my trace hooks removed, and without the `>=`.
- 227, 65 of 87 spaces, added 3 to 2. The only change on the board is one added space that goes, on 221745.
- `AFarnsworthBulletinReadsAsWordsTests` now asserts the slow section reads as words after ordinary sending, and passes 6 of 6.

**Task 3, the six points** (`226cfbdb`): explained and not shipped.
- With the gap kept, the board reads 233 (251 right, 16 wrong) but 64 of 87 spaces, 3 added.
- The whole gain is on 221745 at 502 Hz: the gap is a 685 ms pause before the sender's newer-speed marks, which becomes its word cluster and demotes its real 390 ms word gaps to letter gaps. The full chain is in section 3.
- **Not from what a sender does**: a pause is neither speed's spacing. Reverted.

**Task 4, a dip inside one tone** (`e776af07`): measured; the rule did not ship.
- The depths separate in part (section 3).
- Joining across a dip that stays under 0.6 of the contrast, held in the sender's window, scored 226, with an invented `E` on 221745, and joined none of the nine. Most likely the joined stretch then fails the mark's shape test, the dip still in its top.
- The main catch rose from 109 to 112 of 152. Reverted.

**Task 5, a weak dah's top** (`827a7862`, the drop candidate): found, and the fix did not ship.
- A weak dah's top does break the flat-top tolerance (section 3).
- A tolerance taking the noise's extreme over the run's looks read 230 and 68 of 87 spaces, 4 added. But it **broke the first recording's hard limit** (`FEN` for `FER`), and the 10 dB call still kept 5 dahs. Reverted.

**Records:**
- `docs\cw-scoreboard.md`: one row per task.
- `PHASE_OUTCOME.md`, both copies: `## UNIT 547 - STEP 12`.
- `PHASE_STATUS.md`, both copies: names 547.
- `Directory.Build.props`: 1.13.232.
- `CLAUDE.md` §1: a row above HM-DEC-250.
- `DECISIONS.md`: HM-DEC-251.

**Build** `-warnaserror`: no warnings, no errors. **App carry-forward:** 278 of 278.

## 2. What the owner should expect

Rebuild and run as usual.

- **The W1AW junk was not its own shadows, as far as the recording can show.** The 30 seconds you kept hold one station and nothing keyed in step beside it. Read through Hamlet as it is now, the bulletin reads `PE II AND TYPE IV RADIO EMISSIONS HOWEVER, THIS NME IS`, the `NME` where the signal weakens at the end, with history in front of it. Read cold from the file's first second, it starts `E NE II AEED`, because Hamlet waits a word before it picks a station.
  - The two extra senders on your sheet grew in the minutes before the recording. What they were is not in this file. A longer capture of W1AW, from the moment Hamlet starts listening, would show it.
- **W1AW's slow sections now read as words**, even when Hamlet was already listening to ordinary-speed sending before them. The first letter or two of the slow section can still be lost, since it takes three gaps to see the spacing has changed.
- **The six points were one long pause** on your 22:17:45 recording, which pulled that sender's word line up. It happened to keep `40M` whole and cost a real space. It did not ship.
- **The broken dahs still read as two dits.** Your real gaps always fall much deeper than most of those dips, so depth can tell them apart. But the joined dah is then refused by the detector's own shape test. That is the next place to look.
- **The weak fast station is unchanged.** Its dahs' tops wobble more than the detector allows at that strength. Widening the allowance helped elsewhere but broke the first recording, so it did not ship.
- **The score:** 191 on your twelve recordings before and after. With W1AW's bulletin added to the board, 227, and spaces 65 of 87 with added falling from 3 to 2.

## 3. What you should see

**The scoreboard after each task** (with W1AW's stretch from task 1 on; at HEAD 191, 208 of 244, 15 wrong, 55 of 77 spaces, 3 added):

| unit | right | wrong | invented | score | printed in silence | spaces | what changed |
|---|---|---|---|---|---|---|---|
| 547 task 1 | 248 of 288 (208 of 244) | 19 (15) | 2 | **227** (191) | 1 | 65 of 87, 3 added (55 of 77, 3) | W1AW joins the board; no shadow rule |
| 547 task 2 | 248 of 288 | 19 | 2 | **227** | 1 | 65 of 87, **2 added** | Farnsworth ships |
| 547 task 3 | 248 of 288 | 19 | 2 | **227** | 1 | 65 of 87, 2 added | the gap measured at 233, 64 spaces; not shipped |
| 547 task 4 | 248 of 288 | 19 | 2 | **227** | 1 | 65 of 87, 2 added | the dip join measured at 226; not shipped |
| 547 task 5 | 248 of 288 | 19 | 2 | **227** | 1 | 65 of 87, 2 added | the long-top tolerance measured at 230, first recording `FEN`; not shipped |

**W1AW, the senders and handovers** (before and after are the same, since no rule changed what the gate holds):

| read | senders at the end | handovers | printed away from 600 Hz |
|---|---|---|---|
| once, cold | 600 Hz, shape 0.59, 110 marks, printed | 4.13 s on; released 18.36-18.74 s and 27.29-28.23 s | nothing |
| three times | 600 Hz, 334 marks | the same in each pass | nothing |
| six times | 600 Hz, 670 marks | the same in each pass | nothing |
| live sheet (not reproducible) | 600 Hz 0.59 printed; 650 Hz 0.71, 71 marks; 550 Hz 0.59, 37 marks | — | — |

| candidates by pitch, one pass | count | level | in step with a 600 Hz mark |
|---|---|---|---|
| 600 Hz | 118 | -18.0 dB | — |
| 550 Hz | 4 | -24.7 dB | 0 |
| 650 Hz | 7 | -25.3 dB | 0 |
| every other pitch, 375-850 Hz | 1 to 13 each | -24 to -33 dB | 0 |

**Task 3, letter by letter on 221745 at 502 Hz:**

| at | gap kept out (as shipped) | gap kept in |
|---|---|---|
| lines | letter line 87 ms, word line 298 ms; letter cluster 3 at 195 ms, word 1 at 390 ms | letter line 95-113 ms, word line 283-456 ms; letter cluster 3 at 248-299 ms, word 1 at 401-685 ms |
| 6.37-7.38 s | `M`, then `N` with no space before | `M`, then ` N` (gap 283 ms, at the word line 283 ms): **a space added** |
| 8.13 s | ` 4` (gap 405 ms past 298 ms): right | `4` (gap 401 ms under 456 ms): **a space lost** |
| 9.08-10.35 s | `M E M` (the `0` cut into three) | `0`: **three letters right** |

- **The stretch:** `TMN 4MEMM` (18 right, 9 wrong) becomes `TM N40M` (21 right, 6 wrong).
- **On 143951** (low, not counted): the larger lines remove spaces before `K` and `E`.
- **On 221851** (low): letters regroup with no gain.

**The dips against key-up**, on the sender's own window, each as the share of the sender's contrast it falls through:

| | count | min | 5th percentile | median | max |
|---|---|---|---|---|---|
| the nine dips | 9 | 0.47 | 0.47 | 0.59 | 1.24 |
| real gaps inside a letter, all stretches and the catch | 966 | 0.65 | 0.83 | 1.51 | 3.46 |

| under this share of the contrast | dips | real gaps |
|---|---|---|
| 0.5 | 2 of 9 | 0 of 966 |
| 0.6 | 5 of 9 | 0 of 966 |
| 0.7 | 6 of 9 | 6 of 966 |
| 0.8 | 6 of 9 | 31 of 966 |

**The nine dahs**, as shipped (unchanged by this unit) and with the dip join that did not ship:

| at | depth | as shipped | with the join |
|---|---|---|---|
| 3.54 s | 0.47 | `. .` | `. .` |
| 31.62 s | 1.06 | `. . .` | `. . .` |
| 32.65 s | 1.24 | `. .` | `.` then `.` in the next letter |
| 40.69 s | 0.66 | `. .` | `. .` |
| 49.17 s | 0.83 | `. . .` | `. . .` |
| 52.14 s | 0.59 | `. .` | `. .` |
| 53.63 s | 0.54 | `. . .` | `. . .` |
| 69.54 s | 0.49 | `. . .` | `.` |
| 72.11 s | 0.50 | `. . .` | `. . .` |

The main catch read 109 of 152 against its pending reference at HEAD, and 112 with the join.

**The weak dah's top**, rebuilt offline on the bin's own 10 ms Hann window at the tone every 5 ms, for a 25 WPM call:

| tone over noise | bin contrast | tolerance | dahs holding their top | top spread, median / 95th / max |
|---|---|---|---|---|
| 20 dB | 27.2 dB | 1.50 dB | 65 of 66 | 0.70 / 1.04 / 1.66 dB |
| 12 dB | 18.6 dB | 1.50 dB | 14 of 66 | 1.83 / 2.64 / 4.75 dB |
| 10 dB | 16.7 dB | 1.50 dB | 3 of 66 | 2.40 / 3.09 / 4.15 dB |

With the long-top tolerance, the 10 dB call still stands 70 marks, 5 of them dahs, and prints nothing. 143906 also prints nothing, where it printed junk.

## 4. What's blocking us

- **The live W1AW senders at 550 and 650 Hz cannot be traced from this recording.** A capture that starts when Hamlet starts listening to W1AW would.
- **The broken dahs** need the mark's own shape test to see a dip inside one tone. The depth rule alone joins none, though the depths do separate.
- **The weak fast station** needs a second flatness check found and measured. The run test is not the only one holding a weak top back, and widening it broke `FER`.
- **The old CW read guards** (`TheAdjudicatedReadingsKeepReadingTests`, the 08-25 cases of `TheCapturesThatDecodeKeepDecodingTests`, `CwFixtureTests.TheCleanRecordingsDecodeExactly`) are still not touched, and still wait on your ruling about replacing them with the scoreboard.
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
