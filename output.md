## 1. What Claude did

Development computer, project gate `PROJECT: Hamlet` checked against `PROJECT_CARD.md`, the order's five checks (`SHACK_FACTS.md`, `docs\cw-scoreboard.md`, `docs\cw-survey-2026-10-09.md` present; no `CoreHMI.sln`, no `MURC.sln`) and `Hamlet.sln`: Hamlet confirmed. Nothing here is evidence about the radio. Branch `main`. Run by hand: `SESSION.lock` taken at 10:31:31 through `tools\arbiter\lock.bat take` and released at the end, nothing written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` touched, no box ticked in `PHASE_PLAN.md`. Nothing keys, transmits or writes to the radio; no recording added, removed or promoted. Version 1.13.251 to **1.13.252**. Ruling **HM-DEC-272**; HM-DEC-269 stays held. Nothing was recorded under §12.1.

**Task 1, the trace** (`2c226567`, the label corrected in `6df31ffc`), shipped as a report. `TheWeakEndTests.WhereEachWeakFileDies` traces each of the eighteen weak files, a row each:
- **The signal as sent,** from the generator's own recipe (`SyntheticCq.All`, `CwFixtureCatalogue.All`). Its SNR is the tone's RMS over the noise's RMS **inside the receiver's 350 to 870 Hz passband**, 520 Hz wide (`CwFixtureGenerator`, `amplitude = √2 · noiseRms · 10^(snr/20)`). All eighteen sit at 615 Hz, drifting 3 Hz; the working fixtures carry 12 dB of QSB.
- **At the wide detector:** one grid bin mirrored offline, a 10 ms Hann window every 5 ms at the pitch, against the marks as keyed (`CwFixtureGenerator.KeyEdges`). Beside it, what the chain offered, stood and gathered into sequences at the pitch.
- **At the gate:** when a candidate window opened, and the sender's shape, whether it qualified and whether it printed.
- **In the sender's own window opened by hand** (`CwSenderLane`, at the true pitch and dit): its contrast, the marks the level rule finds (up at 0.6 of the contrast, down at 0.4, half a dit at least), their shape, and what they read at the recipe's own timing. That last is **the ceiling**: what is possible once the station is found.

**Which stage they die at:**

| stage | files |
|---|---|
| candidacy | 10 |
| window (a window opened, no sender formed) | 3 |
| reader (printing junk) | 3 |
| marks | 1 |
| under Sure | 1 |

**The finding.** The belief is half right.
- **The window reads them once opened.** At 5 dB it shows 15 to 17 dB of contrast, finds every mark, and all four 5 dB CQs read **21 of 21**. Fifteen of the eighteen read half their letters or better; the three that don't are `cq-25wpm-0db`, `fast-edge` and `fast-working`.
- **But the gate that kills them is not a level line.** It is the grid:
  - a 5 dB station stands only about 11 dB over a bin's noise, which swings about 5.5 dB hop to hop;
  - so every mark reaches the pattern gate as pieces of **20 to 35 ms, all one length**;
  - the sequence gathers 30 to 74 of them at one pitch (shape up to 0.71) but never shows **two lengths**;
  - so it never stands (`Stood` 0 on all eight CQs and all five edge files) and never earns a candidate window, which needs two lengths too.
- **The line, in the order's terms:** two lengths with a ratio of two or more. Strong stations' marks reach it; the weak ones' pieces reach a ratio of about 1.75 at most (20 to 35 ms).

**Task 2, the gate moved** (`6df31ffc`): built, measured, **not shipped**.
- **The gate:** a sequence of twelve or more pieces at one steady pitch, never splitting in two, gets a candidate window, opened at a 20 WPM dit rather than the pieces' 20 ms.
- **What else it took:** with that alone, windows opened in 3 to 10 s on all but two files, but senders printed junk. That window's marks were judged against the grid's level for the pieces, a few dB above the true key-down. So a second change was measured with it: such a window judges its marks against **its own key-down**, the top of its last three seconds.
- **Result, both together:**
  - two of the four 5 dB CQs still printed junk: `cq-12wpm-5db` `I H E EE IOEE EES` and `cq-18wpm-5db` `AT E0CA I N`;
  - `cq-18wpm-5db-char5` and `cq-25wpm-5db` read partly;
  - the twelve-plus-three read **299** with invented letters rising from 1 to 2;
  - W1AW read **2109**.
- **Verdict:** it fails the bar on the CQs, on invented and on W1AW. The engine is as at HEAD, and the diff is kept outside the tree.

**Records:**
- `docs\cw-scoreboard.md`: a 568 row in each table, at HEAD's figures.
- `PHASE_OUTCOME.md`, both copies: `## UNIT 568 - STEP 12`.
- `PHASE_STATUS.md`, both copies: names 568.
- `Directory.Build.props`: 1.13.252.
- `CLAUDE.md` §1: a row.
- `DECISIONS.md`: HM-DEC-272.

**Build** `Hamlet.sln -warnaserror`: 0 warnings, 0 errors. **Guards:** 4 of 4. **Decision-log and voice tests:** 7 of 7. **App carry-forward:** 278 of 278. **The survey line:** reran in 126 s, its doc rewritten byte for byte the same.

## 2. What the owner should expect

**Weak stations do not read yet; nothing changed.** Rebuild if you like.

| measure | before | after |
|---|---|---|
| your recordings | 297 | 297 |
| W1AW | 2111 | 2111 |
| synthetic CQs at 5 dB and 0 dB | 0 of 84 each | 0 of 84 each |
| edge fixtures | 0 of 81 | 0 of 81 |
| working fixtures | 5 of 81 | 5 of 81 |

**Where the weak stations were dying:**
- **Hamlet's first look at the band is too coarse for them.** It splits the audio into slices about 150 Hz wide, 5 ms at a time. A station at 5 dB stands only a little above the hiss in such a slice, and the hiss jumps around enough to cut every dot and dash into 20 to 35 ms pieces, all the same length.
- **Hamlet only takes a closer look at a station whose marks come in two lengths,** dots and dashes. These never do, so the closer look never happens.
- **The closer look would read them.** Opened by hand on the right pitch, it hears a 5 dB CQ 15 to 17 dB clear of the hiss and reads all four 5 dB CQs perfectly. Fifteen of the eighteen weak files would read at least half right.

**What I tried:** giving a station that arrives in same-length pieces its closer look anyway. It opened in seconds, but two of the four 5 dB CQs still printed junk, your recordings gained an invented letter, and W1AW lost two letters. So it isn't in. Making weak stations read means changing how the closer look takes over from the coarse one, more than one gate, and that's yours to decide.

**At 0 dB** the closer look reads the slower CQs about half right and the 25 WPM one hardly at all. Nothing was asked of 0 dB, and nothing changed there.

Pushed to `main`.

## 3. What you should see

**The eighteen** (at HEAD):
- **sent:** the generator's SNR in its 520 Hz passband, pitch, WPM, marks sent;
- **wide bin:** the contrast of one grid bin against the true keying, and its key-up spread;
- **chain:** marks offered at the pitch (their own contrast), marks that stood, the longest sequence at the pitch and its best shape;
- **own window:** opened by hand at the true pitch.

| file | sent | wide bin: contrast, key-up spread | chain: offered (contrast), stood, sequence marks, shape | window opened, sender | own window: contrast median / p10 | level marks found of sent, whole | their shape | dies at | chain read | window read, right of sent |
|---|---|---|---|---|---|---|---|---|---|---|
| `cq-12wpm-0db` | 0 dB, 615 Hz, 12 WPM, 73 | 6.3 dB, 5.7 dB | 75 (3.8 dB), 0, 56, 0.68 | no window, no sender | 12.2 / 10.9 dB | 77 of 73, 70 whole | 0.06 | **candidate** | `` | `CQ CQ C■ DE S0CA■ N0CALL K`, 17 of 21 |
| `cq-12wpm-5db` | 5 dB, 615 Hz, 12 WPM, 73 | 10.8 dB, 5.4 dB | 89 (2.3 dB), 0, 74, 0.71 | at 18.5 s, no sender | 16.9 / 16.2 dB | 73 of 73, 73 whole | 0.96 | **window** | `` | `CQ CQ CQ DE N0CALL N0CALL K`, 21 of 21 |
| `cq-18wpm-0db` | 0 dB, 615 Hz, 18 WPM, 73 | 6.3 dB, 5.5 dB | 62 (3.4 dB), 0, 47, 0.69 | no window, no sender | 10.4 / 8.9 dB | 76 of 73, 66 whole | 0.06 | **candidate** | `` | `CQ CQ TN■ DE N■I■ALL N,K SLL K`, 12 of 21 |
| `cq-18wpm-0db-char5` | 0 dB, 615 Hz, 18 WPM, 73 | 6.0 dB, 5.5 dB | 78 (3.4 dB), 0, 42, 0.74 | at 12.5 s, no sender | 10.0 / 8.5 dB | 82 of 73, 62 whole | 0.00 | **window** | `` | `■Q C■ K ■ DE S■6A■L N■CALL K`, 12 of 21 |
| `cq-18wpm-5db` | 5 dB, 615 Hz, 18 WPM, 73 | 10.7 dB, 5.7 dB | 70 (2.8 dB), 0, 60, 0.71 | at 12.3 s, no sender | 15.7 / 14.9 dB | 73 of 73, 73 whole | 0.94 | **window** | `` | `CQ CQ CQ DE N0CALL N0CALL K`, 21 of 21 |
| `cq-18wpm-5db-char5` | 5 dB, 615 Hz, 18 WPM, 73 | 10.6 dB, 5.2 dB | 59 (3.2 dB), 0, 51, 0.00 | no window, no sender | 15.3 / 14.5 dB | 73 of 73, 73 whole | 0.93 | **candidate** | `` | `CQ CQ CQ DE N0CALL N0CALL K`, 21 of 21 |
| `cq-25wpm-0db` | 0 dB, 615 Hz, 25 WPM, 73 | 6.2 dB, 5.5 dB | 45 (3.6 dB), 0, 31, 0.00 | no window, no sender | 9.4 / 7.7 dB | 90 of 73, 58 whole | 0.00 | **candidate** | `` | `CHS ■Q /■ DE NT■<AR>A■HE N■ERAL5 B E`, 5 of 21 |
| `cq-25wpm-5db` | 5 dB, 615 Hz, 25 WPM, 73 | 10.8 dB, 5.6 dB | 54 (2.9 dB), 0, 43, 0.46 | no window, no sender | 13.9 / 13.2 dB | 73 of 73, 73 whole | 0.92 | **candidate** | `` | `CQ CQ CQ DE N0CALL N0CALL K`, 21 of 21 |
| `coverage-edge` | 0 dB, 615 Hz, 12 WPM, 97 | 6.3 dB, 5.6 dB | 89 (3.3 dB), 0, 61, 0.72 | no window, no sender | 11.9 / 10.7 dB | 98 of 97, 94 whole | 0.44 | **candidate** | `` | `■23■56Z 890 QRZ? DE/N0CALL`, 20 of 23 |
| `coverage-working` | 5 dB, 615 Hz, 12 WPM, 97 | 11.1 dB, 5.4 dB | 119 (6.5 dB), 101, 77, 0.68 | at 3.3 s, sender 0.68 qualified printed | 16.7 / 11.2 dB | 97 of 97, 93 whole | 0.57 | **reader** | `ITTEIIIT` | `12EW456■890 QRZ? TEE/S0CALL`, 17 of 23 |
| `exchange-edge` | 0 dB, 615 Hz, 12 WPM, 65 | 6.3 dB, 5.7 dB | 71 (2.4 dB), 0, 49, 0.00 | no window, no sender | 12.4 / 10.6 dB | 68 of 65, 62 whole | 0.12 | **candidate** | `` | `CQ CQ DE N0CAL5 N■CALL K`, 17 of 19 |
| `exchange-working` | 5 dB, 615 Hz, 12 WPM, 65 | 11.0 dB, 5.5 dB | 86 (3.3 dB), 75, 51, 0.63 | at 7.3 s, sender 0.70 qualified printed | 17.1 / 11.0 dB | 68 of 65, 60 whole | 0.14 | **reader** | `I N T II E` | `CQ CQ LE N■CA5AI N0■AR L K`, 12 of 19 |
| `fast-edge` | 0 dB, 615 Hz, 25 WPM, 65 | 6.0 dB, 5.6 dB | 32 (3.2 dB), 0, 22, 0.00 | no window, no sender | 9.2 / 7.1 dB | 78 of 65, 52 whole | 0.00 | **candidate** | `` | `■- ■T■ DIN■■AR R N■6 TED5 ■ E E`, 4 of 19 |
| `fast-working` | 5 dB, 615 Hz, 25 WPM, 65 | 11.5 dB, 5.3 dB | 49 (12.3 dB), 46, 33, 0.88 | at 4.5 s, sender 0.84 qualified printed | 14.8 / 9.1 dB | 65 of 65, 51 whole | 0.04 | **reader** | `EOXDOTAN` | `CDI C■ DE N■T■AEEEL T 1NT ALE I K`, 5 of 19 |
| `prosigns-edge` | 0 dB, 615 Hz, 12 WPM, 37 | 6.4 dB, 5.7 dB | 36 (3.3 dB), 0, 28, 0.62 | no window, no sender | 12.2 / 11.1 dB | 38 of 37, 37 whole | 0.80 | **candidate** | `` | `<BT> N0CALL <AR> <SK> E`, 9 of 9 |
| `prosigns-working` | 5 dB, 615 Hz, 12 WPM, 37 | 11.5 dB, 5.6 dB | 40 (2.3 dB), 30, 30, 0.55 | at 3.5 s, sender 0.63 qualified | 16.9 / 11.4 dB | 38 of 37, 36 whole | 0.71 | **marks** | `` | `■ NOMCALL <AR> <SK>`, 6 of 9 |
| `tightfist-edge` | 0 dB, 615 Hz, 13 WPM, 19 | 6.6 dB, 5.7 dB | 18 (3.5 dB), 0, 14, 0.62 | no window, no sender | 12.5 / 11.2 dB | 20 of 19, 18 whole | 0.04 | **candidate** | `` | `TEST DE TEST X`, 10 of 11 |
| `tightfist-working` | 5 dB, 615 Hz, 13 WPM, 19 | 11.7 dB, 5.8 dB | 19 (2.3 dB), 15, 15, 0.63 | at 4.3 s, sender 0.00 | 16.9 / 10.3 dB | 19 of 19, 19 whole | 0.79 | **sure** | `` | `TEST DE TEST K`, 11 of 11 |

A typical weak sequence, `cq-18wpm-5db` at 12 s: `614 Hz 31 marks [60 20 20 20 25 25 75 20 30 25 20 25 20 20 25 20] one length shape 0.71`. Its pieces are 20 to 35 ms with no jump of two between them, so it never stands.

**The windows for a station in pieces, the five 5 dB CQs and working files they printed on** (not shipped):

| file | chain read with them on |
|---|---|
| `cq-12wpm-5db` | `I H E EE IOEE EES` |
| `cq-18wpm-5db` | `AT E0CA I N` |
| `cq-18wpm-5db-char5` | `5 E II FMTDE N1DEALLN0CALL K` |
| `cq-25wpm-5db` | `S E EK CQ■E N0CALL N0CALL K` |
| `fast-working` | `O NA DOTAN` |

**The survey's summary, before and after:** unchanged, since nothing shipped. The survey reran and wrote the same document. Its groups as they stand:

| group | right of sent |
|---|---|
| synthetic fixtures | 46 of 67 |
| receiver easy | 74 of 90 |
| receiver working | 5 of 81 |
| receiver edge | 0 of 81 |
| receiver ungraded | 49 of 98 |
| CQs at 15 dB | 73 of 84 |
| CQs at 5 dB | 0 of 84 |
| CQs at 0 dB | 0 of 84 |
| twelve-plus-three | 318 of 353, 297 |
| W1AW | 2131 of 2151, 2111 |

**Both scoreboards' rows:**

| unit | right | wrong | invented | score | printed in silence | spaces |
|---|---|---|---|---|---|---|
| HEAD | 318 of 353 | 20 | 1 | **297** | 0 | 83 of 110, 3 added |
| windows for a station in pieces (not shipped) | 319 of 353 | 18 | 2 | 299 | 0 | 86 of 110, 3 added |
| 568 (engine as HEAD) | 318 of 353 | 20 | 1 | **297** (234 without) | 0 | 83 of 110, 3 added |

| w1aw | score | right | wrong | invented | spaces |
|---|---|---|---|---|---|
| HEAD | **2111** | 2131 of 2151 | 20 | 0 | 438 of 440, 11 added |
| windows for a station in pieces (not shipped) | 2109 | 2130 of 2151 | 21 | 0 | 438 of 440, 11 added |
| 568 (engine as HEAD) | **2111** | 2131 of 2151 | 20 | 0 | 438 of 440, 11 added |

## 4. What's blocking us

- **Weak stations need more than one gate moved.** A candidate window for a station in pieces, and how a window's marks take over from the grid's pieces for the same station: the owner's ruling on the direction.
- **At 0 dB** the hand-opened window reads the 12 and 18 WPM CQs 12 to 17 of 21 and the 25 WPM one 5 of 21. It is not required and nothing ships for it.
- **`fast-working` reads only 5 of 19 even through the hand-opened window,** where 12 dB of QSB at 25 WPM takes marks under the level rule.
- **The trace's shape column** scores all of a file's level marks as one sequence, so a whole message scores low where its own window reads it whole (`cq-12wpm-0db` 0.06 at 17 of 21). It is a figure for this table only.
- **`TheRsidDetectorTests.TheDetectorKeepsUpWithRealTime`** is red when the engine line runs whole, and the two reply tests (`TAW`, `IAN`) are red at HEAD.

### Asks still outstanding

- **Unit 568, 2026-10-09:** how a weak station gets its own window. The grid breaks it into pieces of one length, so it never stands nor earns a window. A window for such a station, judged on its own key-down, read 2 of 4 CQs and cost an invented letter and two W1AW letters. Waiting on the owner. No change for it sits in the tree.
- **Unit 562, 2026-10-08:** what next for the junk before a weak call, given the 0.4 start costs the start of every real call. Waiting on the owner. No change for it sits in the tree.
- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
