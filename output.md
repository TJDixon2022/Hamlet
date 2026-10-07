## 1. What Claude did

Development computer, project gate `PROJECT: Hamlet` checked against `PROJECT_CARD.md`, the order's five checks and `Hamlet.sln`: Hamlet confirmed. Nothing here is evidence about the radio. Branch `main`. Run by hand: `SESSION.lock` taken at 14:44:04 and released at the end, nothing written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` touched, no box ticked in `PHASE_PLAN.md`. Version 1.13.236 to 1.13.237. Ruling HM-DEC-256, the number the order gave. Nothing was recorded under §12.1. R88 was lifted only as the order named. Every test run was one filtered invocation, the longest under three minutes.

**Task 1, a carrier keys down too much to be Morse** (`6c092449`), **shipped**:
- **Measured:** each sender's key-down share is its marks over its marks and the gaps between them up to a word gap (seven dits), over the same recent marks the score already reads, with pauses left out. It was measured on unit 550's four classes (section 3).
- **The term, `KeyDown`:**
  - one up to Morse's ceiling, **15 of 22**: the letter `0` over and over, five dahs and their gaps, then a letter gap;
  - falling straight to nought at a share of one, a key that never comes up;
  - the form and the window are the author's.
- **Result:**
  - The noise burst falls from 0.69 to **0.26**.
  - Junk's 75th percentile falls from 0.40 to 0.29, its 90th from 0.67 to 0.61, its 95th from 0.69 to 0.65.
  - Real hands keep their median of 0.76; their 5th percentile moves 0.46 to 0.44.
  - **The carrier catch stays at 0.68:** by the gate's marks it is keyed down 0.64 of its sending, under Morse's ceiling.
  - The scoreboard reads exactly as at HEAD.

**Task 2, a station tuned in cold** (`c57b4c70`), **traced, not shipped:**
- **The premise doesn't hold here.** `AStationTunedInColdTests` prints every letter of W1AW's opening with the sender's split and letter line at the moment it printed. They are 117 ms and 113 ms, against 119 and 111 at the end of the recording, and every letter's labels are what the end's lines give. The backlog is already read with what is known at the pick: printing re-splits a sender's unprinted runs at its current lines every time.
- **The cause is marks the detector never produced.** Dumping every mark it called in the first 4.5 s:
  - **P's first dah** (about 0.42 to 0.62 s) was never called, while the detector had no gap level yet; its contrast reads NaN until 1.3 s.
  - **N's dah in AND** was called as 60 ms of its 205.
  - Every mark the detector did call was printed.
- **No code changed.** Re-reading the backlog cannot bring back a mark that was never called. The test stays as the trace and asserts nothing.

**Task 3, the digital watering holes keep their names** (`834dcd0b`), **shipped:**
- On the CW tab a shared block the band data names for a digital mode keeps its name in CW's colour: RTTY, FT4, PSK31, FT8, JS8, `auto` and `Data`. Open ground is named CW.
- The Digital and Voice tabs are unchanged.
- The 40 m test is updated, and the app test still shows a tab change writing only its own mode.

**Task 4, the scoreboard is the CW guard** (`42a61639`), **shipped, at your yes:**
- **Retired, naming HM-DEC-256:** `TheAdjudicatedReadingsKeepReadingTests` and `CwFixtureTests.TheCleanRecordingsDecodeExactly` carry a Skip; the thirteen `cw-2026-08-25` rows are gone from `TheCapturesThatDecodeKeepDecodingTests`' floors.
- **The guard,** `TheScoreboardGuardsCwTests.TheScoreboardFallsNowhere`, runs the scoreboard and holds it to the **last unit row** of `docs\cw-scoreboard.md`:
  - the score not below it, spaces right not below it, spaces added not above it, no more letters in a silence;
  - the random carrier at no more seeds than a new guard line in that file records (1 of 20, seed 5195);
  - the first recording whole, and loud noise silent.
- **A second test** checks that the guard reads the last unit row and not the scan table's rows after it.
- **The carry-forward line lives in `docs\carry-forward-tests.txt`**, on its engine line (line 9). Section 3 shows it.
- I did not run the retired guards: they read the corpus R88 bars.

**Records:**
- `docs\cw-scoreboard.md`: a row per task, and the guard line.
- `docs\carry-forward-tests.txt`: the engine line, the guards table, and a dated note.
- `PHASE_OUTCOME.md`, both copies: `## UNIT 552 - STEP 12`.
- `PHASE_STATUS.md`, both copies: names 552, that line only.
- `Directory.Build.props`: 1.13.237.
- `CLAUDE.md` §1: a row.
- `DECISIONS.md`: HM-DEC-256.

**Build** `-warnaserror`: no warnings.

**App carry-forward:** 277, 276 and 277 of 278 over three runs. Every failure was `You've caused dispatcher loop`, thrown by Avalonia's headless session, and it landed on a different test each run:
- `TheCarrierHoldsTheButtonsTests`
- `ThePowerIsOfferedTests` (twice)
- `BindingHealthTests`

Each passes alone (11 of 11 together). It is the dispatcher-loop failure units 544 and 548 named.

## 2. What the owner should expect

Rebuild and run as usual.

- **The noise burst now scores 0.26, from 0.69,** because its key was down 88% of the time, and Morse never keeps the key down more than 68%: a run of zeros is the most any text can do. Junk as a whole sits lower.
- **The fading carrier the scan caught still scores 0.68.** By the marks Hamlet sees, its key is down only 64% of the time, which Morse can do, so this test can't tell it from a station. It is the one bit of junk still scoring like a real hand.
- **A station tuned in cold still loses its first letter or two.** W1AW read from its first second still prints `E NE II AEED` for `PE II AND`.
  - The idea was to re-read those first letters once Hamlet knows the sender, and it would change nothing: they are already read with everything Hamlet knows by then.
  - The real fault is earlier. For the first second or so, before Hamlet has measured the band's quiet level at that pitch, it misses whole dahs, or catches only the start of one. That is the next thing to fix, in the detector rather than the decoder.
- **On the CW tab the map is amber across the shared stretch as before, but the digital spots keep their names:** FT8, FT4, JS8, PSK31, RTTY and the rest, so you can still see where the digital crowd sits. The Digital and Voice tabs haven't changed.
- **The scoreboard now guards every unit.** The old CW checks, which read recordings that are off limits and tested a decoder that no longer exists, are retired. In their place a test reruns the scoreboard and fails if the score, the spaces, or any of the hard limits read worse than the last recorded row.
- **The scoreboard:** 227 before, 227 after every task, everything else exactly as before.
- **What will look wrong but is not:** the app's test line shows one failure per run, a different test each time, the "dispatcher loop" fault the test framework throws. Each of those tests passes on its own.
- Pushed to `main`.

## 3. What you should see

**The scoreboard rows:**

| unit | right | wrong | invented | score | printed in silence | spaces |
|---|---|---|---|---|---|---|
| HEAD | 248 of 288 | 19 | 2 | **227** | 1 | 65 of 87, 2 added |
| 552 task 1 | 248 of 288 | 19 | 2 | **227** | 1, as at HEAD | 65 of 87, 2 added |
| 552 task 2 | 248 of 288 | 19 | 2 | **227** | 1, as at HEAD | 65 of 87, 2 added |
| 552 task 3 | 248 of 288 | 19 | 2 | **227** | 1, as at HEAD | 65 of 87, 2 added |
| 552 task 4 | 248 of 288 | 19 | 2 | **227** | 1, as at HEAD | 65 of 87, 2 added |

Each task's whole table was identical to HEAD's, line for line. The random carrier prints at seed 5195 only, as at HEAD.

**The key-down share by class** (the gate's senders, a sample each second of audio):

| class | samples | min | p5 | p25 | median | p75 | p95 | max |
|---|---|---|---|---|---|---|---|---|
| W1AW | 25 | 0.349 | 0.380 | 0.422 | 0.479 | 0.517 | 0.536 | 0.537 |
| real hands | 263 | 0.340 | 0.438 | 0.499 | 0.535 | 0.574 | 0.621 | 0.819 |
| synthetic clean | 232 | 0.552 | 0.566 | 0.585 | 0.598 | 0.618 | 0.648 | 0.662 |
| junk | 309 | 0.363 | 0.387 | 0.437 | 0.479 | 0.619 | 0.870 | 0.928 |

**Per source, the two the order named:**

| source | key-down share | score before | score after |
|---|---|---|---|
| the carrier catch `catch-154614-7047190` | 0.64 (0.61 to 0.70) | 0.68 | 0.68 |
| noise 180 s, seed 5370 | 0.88 (0.84 to 0.89) | 0.69 | **0.26** |

The one real hand over Morse's ceiling is the station catch `catch-153810-7033367`, at 0.82 in one sample. Its median score moves 0.68 to 0.66.

**The score's percentiles, before and after:**

| class | p5 before | p5 after | median before | median after | p75 before | p75 after | p95 before | p95 after |
|---|---|---|---|---|---|---|---|---|
| W1AW | 0.868 | 0.868 | 0.937 | 0.937 | 0.940 | 0.940 | 0.942 | 0.942 |
| real hands | 0.459 | 0.442 | 0.757 | 0.757 | 0.879 | 0.879 | 0.937 | 0.937 |
| synthetic clean | 0.892 | 0.892 | 0.949 | 0.949 | 0.950 | 0.950 | 0.951 | 0.951 |
| junk | 0.011 | 0.011 | 0.106 | 0.101 | 0.403 | **0.289** | 0.691 | **0.653** |

**W1AW read cold, before and after:** `E NE II AEED TYPE IV RADIO EMISSIONS HOWEVER, THIS NME IS` both, since task 2 changed nothing. The trace:

```
at the end: split 119 ms, letter line 111 ms
0.30-0.36 s printed at 4.12 s `E`: marks 60 ms; split then 117, letter line then 113; labels at the end .
0.70-1.02 s printed at 4.12 s `N`: marks 205 55 ms, gaps 65 ms; split then 117, letter line then 113; labels at the end -.
...
3.39-3.45 s printed at 4.30 s `E`: marks 60 ms; ...; labels at the end .
3.63-3.70 s printed at 4.30 s `E`: marks 70 ms; ...; labels at the end .
mark 0.300-0.360 s, 60 ms at 600 Hz, -17.6 dB, contrast NaN, keyed, printed
mark 0.695-0.900 s, 205 ms at 600 Hz, -17.7 dB, contrast NaN, keyed, printed
mark 3.385-3.445 s, 60 ms at 600 Hz, -17.4 dB, contrast 9.4, keyed, printed
mark 3.630-3.700 s, 70 ms at 600 Hz, -17.9 dB, contrast 9.8, keyed, printed
```

No mark between 0.36 and 0.695 s, where P's first dah was. N's dah ends at 3.445 s, 60 ms long.

**The guard's line**, in `docs\carry-forward-tests.txt`, line 9, the engine invocation. Its last name is the guard:

```
timeout 480 dotnet test tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj --filter "FullyQualifiedName~TheUnslottedSendTests|...|FullyQualifiedName~TheOliviaModulatorTests.EachMacroAndATypedLineComeBackIdentical|FullyQualifiedName~TheScoreboardGuardsCwTests"
```

The guards table in the same file now reads:

```
CW     read  TheScoreboardGuardsCwTests, the scoreboard held to its last recorded row   engine   unit 552, HM-DEC-256
```

The guard run, against the row then last, 552 task 3:

```
recorded, 552 task 3: score 227, spaces 65 right and 2 added, 1 in a silence, the carrier at 1 seeds
now: score 227, spaces 65 right and 2 added, 1 in a silence, the carrier at 1 seeds; the first recording reads `FER C HAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA`
```

`TheLastUnitRowIsWhatTheGuardReads` passes.

**Other tests:**
- The 40 m test and the app's tab test pass.
- `TheShapeScoreTakenApartTests` passes.
- The app carry-forward results are in section 1.

## 4. What's blocking us

- **The carrier catch still scores 0.68.** Its key-down share by the gate's marks is 0.64, under Morse's ceiling. The scan's own frame-by-frame key-up share calls it a carrier, but the gate's marks do not see that way, and no CW term measured so far separates it from the roughest hands.
- **The cold start is in the detector.** Before it has a gap level at a pitch it misses whole dahs (contrast NaN for the first 1.3 s) or cuts them short. Re-reading the opening audio through the sender's own window once that opens is the likely repair, a detector change.
- **The dispatcher-loop failure** in the app's test run hits one test per run and is not this unit's. The order asked for the line green, and it is green only test by test.
- **The engine carry-forward line was not run whole**, to keep runs short. Its new CW guard was run alone and passes.

### Asks still outstanding

- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
- **Unit 550, 2026-10-07:** the handover margin, 0.16, and the challenger's 15 s are the author's figures from measurement, and no recording yet shows the margin acting. It waits on the owner, and the change sits in `src/Hamlet.RadioEngine/Cw/CwSenderGate.cs`.
- **Unit 552, 2026-10-07:** the key-down term's form, one to Morse's 15 of 22 and straight to nought at a key never up, is the author's. It waits on the owner, and the change sits in `src/Hamlet.RadioEngine/Cw/CwSequenceShape.cs`.
