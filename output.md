## 1. What Claude did

Development computer, project gate `PROJECT: Hamlet` checked against `PROJECT_CARD.md`, the order's five checks (`SHACK_FACTS.md`, `CwSenderLane.cs`, `cw-2026-10-03-143906.wav` present; no `CoreHMI.sln`, no `MURC.sln`) and `Hamlet.sln`: Hamlet confirmed. Nothing here is evidence about the radio. Branch `main`. Run by hand: `SESSION.lock` taken at 08:22:51 through `tools\arbiter\lock.bat take` and released at the end, nothing written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` touched, no box ticked in `PHASE_PLAN.md`. **HEAD was tagged `before-candidate-windows` and pushed before any engine change.** R88 lifted only as the order named, and no recording added. Version 1.13.242 to **1.13.243**. Ruling **HM-DEC-262**, the number the order gave. Nothing was recorded under §12.1. Every test run was one filtered invocation; scripts went in `.run-unit\unit558-*.sh`, not committed.

**Task 1, a candidate gets a window before it stands** (`faa32330`): built, measured, **in the tree switched off**.
- **What was built:**
  - `CwPatternGate.Candidates` lists every sequence, standing or not, holding 3 marks or more with two lengths and some shape.
  - The detector opens a provisional `Window` on each, the station's own window: mixed at its pitch, filtered to its dit, the last two seconds read through it.
  - Each window reads marks by level as a standing station's does, at the 0.4 up and 0.6 down lines, trimmed, at least half a dit, and its marks go to the pattern gate.
  - The grid gives way within two bins of a window that reads.
  - At most 4 windows at once, the loudest first, two bins clear of the station's window and of each other. A window is lost after 4.4 s without a mark.
  - The station's own window takes over unchanged once the candidate is printed or waiting.
- **The figures and their reasons** are in HM-DEC-262 and the code.
- **Measured** (section 3):
  - As first built, the windows sat on noise and on a station's shadows: 147 on the twelve and 1314 on w1aw.
  - The two-bin rule, two lengths and some shape brought it to **234 and 2111**, under 237 and 2112.
  - **Under the bar, so it is off.** `HAMLET_CANDIDATE_WINDOWS=1` turns it on for the next unit.
- **The five cases** (section 3):
  - The synthetic 13 dB POTA call reads whole on and off, so the bench does not reproduce KM3STU's failure.
  - `143906` reads a little more of its call.
  - The replies and W1AW cold read as at HEAD.
- **Not done:** the cold re-read was not tried again.
- **Tests:** `ACandidateGetsAWindowTests` (cases 1 and 2), and `TheWeakFastStationTracedTests` now traces four recordings with the windows open each second.

**Task 2, the light and the scan:** dropped. With the windows off, nothing reaches the light or the scan's ear.

**Records:**
- `docs\cw-scoreboard.md`: a row in both tables.
- `PHASE_OUTCOME.md`, both copies: `## UNIT 558 - STEP 12`.
- `PHASE_STATUS.md`, both copies: names 558.
- `Directory.Build.props`: 1.13.243.
- `CLAUDE.md` §1: a row.
- `DECISIONS.md`: HM-DEC-262.

**Build** `Hamlet.sln -warnaserror`: 0 warnings, 0 errors.

**Both guards:** 4 of 4. **The decision-log and voice tests:** 7 of 7.

**App carry-forward:** 276 of 278.
- `TheWindowHoldsBelowItsMinimumTests.TheWorkingPanelsScrollInsideThemselvesRatherThanCollapsing` failed with `You've caused dispatcher loop`.
- `TheStopIsAlwaysOnScreenTests.AtEachOf354sNineSizesStopIsInTheStatusBarAndOnTheWindow` failed with `You've caused dispatcher loop`.
- Both pass alone (2 of 2).

## 2. What the owner should expect

Rebuild and run as usual. **Nothing about reading Morse changed this time**: the new piece is built and switched off.

- **What I built:** your idea, giving a weak station its own listening window before it has proved itself. Hamlet hears a station much better once it has its own window, tuned to its tone and its speed. Until now that window opened only after the station had stood, and a weak station like KM3STU's never stood on the few marks the coarse search found.
- **What it did:** turned on, it read a little more of the weak fast station (`… QIKEEIEWB2FU …` where it had `… T T AEEYDEWB2FU`). But it cost 3 letters on your twelve recordings and 1 on W1AW, so under the rule that neither scoreboard may fall it's off.
- **Why it cost letters:** at first the windows sat on noise and on a station's own shadow a bin or two away, and read the same station twice at two pitches. Keeping them away from each other and requiring a dot and a dash first fixed most of that. On one recording a station's first two seconds, read through its early window, still shifted its timing enough to turn an O into M and T.
- **KM3STU's case:** I couldn't reproduce it on the bench. A made-up 22 WPM POTA call at 13 dB with its tone 200 Hz off reads whole with or without the windows. Whatever made your real one go unread for 30 seconds is something the bench signal doesn't have. Your recording of it isn't in the tree.
- **The first word of a reply:** reads exactly as before.
- **W1AW cold:** reads as before. It still starts `E NE II AND`, without the P.
- **The cost:** at most four extra windows at once, each one small filter per sample. A 30-second recording read in the same second either way.
- **Both scoreboards:** 237 and 2112.
- Pushed to `main`.

## 3. What you should see

**Both tables' rows:**

| unit | right | wrong | invented | score | printed in silence | spaces |
|---|---|---|---|---|---|---|
| HEAD | 254 of 288 | 17 | 0 | **237** | 0 | 67 of 87, 3 added |
| 558 task 1 (windows off) | 254 of 288 | 17 | 0 | **237** | 0 | 67 of 87, 3 added |

| w1aw | score | right | wrong | invented | spaces |
|---|---|---|---|---|---|
| HEAD | **2112** | 2131 of 2151 | 19 | 0 | 438 of 440, 11 added |
| 558 task 1 (windows off) | **2112** | 2131 of 2151 | 19 | 0 | 438 of 440, 11 added |

**The windows on, version by version:**

| version | the twelve | right | wrong | invented | in silence | spaces | w1aw |
|---|---|---|---|---|---|---|---|
| any sequence of 3 marks | 147 | 182 | 27 | 8 | 0 | 59 of 87, 12 added | 1314 |
| and 2 bins clear of the station and of each other | 221 | 239 | 14 | 4 | 4 | 64 of 87, 5 added | 2072 |
| and two lengths shown | 234 | 255 | 18 | 3 | 0 | 70 of 87, 5 added | 2111 |
| and some shape (as kept, off) | 234 | 254 | 19 | 1 | 0 | 69 of 87, 3 added | 2111 |

With the windows on as kept, the stretches that move:

| stretch | HEAD | windows on |
|---|---|---|
| `221530` 492 Hz | `6 CHA MPION`, 9 of 11 | `6 C HA MPIMTN`, 7 of 11, 2 wrong |
| `221828` | `… ESBEST SV A MU`, 37 of 41, 10 of 14 spaces | `… ESBEST SV <AR>WU`, 39 of 41, 12 of 14 spaces |
| `221548` low | `I M IEE IEE E USINGA V …`, 18 of 26 | `I M EEEARED CW USINGA V …`, 22 of 26 |
| `221851` low | nothing | `SESE E IE EIEA2LCQ DE NA8SBN NA EI`, 12 of 20, 1 invented |
| `143951` low | `O WAE EEIEWRK T AGN ES`, 11 of 16 | `O WAE IIEURK T AGN ES`, 14 of 16 |

**The five cases**, windows off (HEAD) and on (as kept):

| case | off | on |
|---|---|---|
| 1. `143906` | `IEE I I E NIEE IE T T AEEYDEWB2FU` | `IEE I I E NUVEE T QIKEEIEWB2FU AEE` |
| 2. 22 WPM POTA call, 13 dB, 800 Hz | `CQ POTA DE KM3STU KM3STU K`, first letter at 3.60 s | the same |
| 3. synthetic QSO reply | `TAW DE K3ZZ K3ZZ K` | the same |
| 3. `144020` reply | `WX IN NETAGIT IAN TEMP` | the same |
| 4. W1AW cold | `E NE II AND TYPE IV RADIO EMISSIONS HOWEVER, THIS NME IS` | the same |
| 5. noise, carrier, first recording | noise nothing; seed 5195 only; reads whole | the same |

**The window cost:**
- At most 4 windows, each one mixer and one fourth-order low-pass on every sample, and the level reading of a few hops each hop.
- Reading a 30 s recording took 1 s with windows on and off.
- The scoreboard and w1aw runs took the same time.

## 4. What's blocking us

- **KM3STU's failure doesn't reproduce on the bench.** A synthetic 13 dB call 200 Hz off the pitch reads whole at HEAD, so the window can't be tested against the case it was built for. The live recording isn't in the tree (R88).
- **The candidate windows** read the twelve at 234 and w1aw at 2111. What remains is a station's first marks read through an early window shifting its timing lines (`221530`'s O), and one invented letter on `221851`.

### Asks still outstanding

- **Unit 557, 2026-10-08:** whether Hamlet's fastest readable speed is 48 WPM, as `CwDecoder.FastestPlausibleWpm` has it (the radio keyer's fastest), or 40 WPM, as work instruction 557 assumed. It waits on the owner. No change for it sits in the tree; the floor measured from either number is reverted.
- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
