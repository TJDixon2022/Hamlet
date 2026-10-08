## 1. What Claude did

Development computer, project gate `PROJECT: Hamlet` checked against `PROJECT_CARD.md`, the order's five checks and `Hamlet.sln`: Hamlet confirmed. Nothing here is evidence about the radio. Branch `main`. Run by hand: `SESSION.lock` taken at 20:01:07 through `tools\arbiter\lock.bat take` and released at the end, nothing written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` touched, no box ticked in `PHASE_PLAN.md`. R88 lifted only as the order named; the folder holds the four pieces of the 19:59 session and nothing else, and no recording was added. Version 1.13.239 to **1.13.240**. Ruling **HM-DEC-259**, the number the order gave. Nothing was recorded under §12.1. Every test run was one filtered invocation; the longest was the flicker test, 4 minutes. Scripts went in `.run-unit\unit555-*.sh`, not committed. Both scoreboards ran after every task.

**Task 1, lone letters are noise only as words** (`0660b744`), **shipped at the owner's ruling:**
- **The rule:** three or more one-mark letters in a row are dropped only when each of them is printed alone as a word. That means a word gap before the row (or nothing before it), between each two of its letters, and after it. A row touching any bigger letter, or with any gap inside it short of a word gap, is text. This replaces HM-DEC-258's inside-a-word test.
- **Seen failing first:** at HEAD the four new lines read `ADVOCA ILL HE WENT`, `FOR THE LAS N YEARS`, `IT IS LEGITIMA O INFER` and `AT TH ST`. All eleven lines now read whole, and `E T E T E` still reads nothing.
- **The twelve read 237**, one under 238. That is the cost the owner accepted, and the guard's row records it under his ruling.
- **The w1aw table** rises from 2097 to **2112**.

**Task 2, a passband that flickers does not make senders** (`085667c5`), **shipped:**
- **What the detector did:** the app sets the passband from the rig state on every scope tick, twenty a second. Where the radio's CW pitch or filter went unread for a tick, `CwChain.Passband` gave nulls and the detector summed the whole band. Each change rebuilt the bins, shut the sender's window, reset the hop count and cleared the reading, so one unread tick cost two rebuilds.
- **Driven on a scripted rig state** (`AFlickeringPassbandTests`): W1AW's four pieces through the app's chain, the passband set every 50 ms of audio. The pitch read was dropped for one tick every 20 s and for three ticks every 20 s, ten seconds apart.
  - **Before:** the text differed from the steady replay in **101 places**. Stray senders stood at 375 Hz (8 marks) and 475 Hz (5 marks), and W1AW took 5630 marks against 5772.
  - **After:** **0 places**, the same four senders as steady, one rebuild.
  - **No 550 Hz sender either way**, so the flicker is not the ghost.
- **The rule** (`CwPassbandHold`, in the chain and the app's scope tick): a passband already known is held until a new reading replaces it, and an unread tick changes nothing.
  - **What changes it:** a different pitch or filter read gives a new passband. A mode read as anything but CW or CW-R gives the whole band and lets the hold go. So does a rig state with nothing known.
  - **How long:** for as long as the radio stays connected and in CW, however many ticks go unread. A missed poll says nothing about the radio, whose pitch and filter change only when it reports a different value.
  - **Both or neither:** a pitch without a width is the whole band.
- **The record:**
  - `PassbandRebuilds` on the detector.
  - The capture sheet gains `at start` (the senders held when the capture began), `version` and `rebuilds`.
  - The 10-second `cw_listen` row gains `appVersion` and `passbandRebuilds`.
- **Tests:** `APassbandIsHeldTests` (2) and `TheListenRowTests` now checks the two new fields.

**Task 3, `H` at 35 WPM** (`ae8e886a`), traced, not changed:
- `TheHAt35WpmTests` traces every H in piece 2's 35 WPM text.
- **5 of 6 read whole.** The sixth, LAY THE at 257.8 s, opens with a dit broken into pieces 6 to 10 dB under the key-down level. The offline read loses it too, and the gate makes `I`.
- **The synthetic line** `THE HELD HABITS OF HIS SMITH` at 35 WPM reads whole, steady and with a 6 dB fade.
- **No plain decoder cause, so no fix.** The qualifying run's recording, where the fault was common, is not in the tree.

**Records:**
- `docs\cw-scoreboard.md`: a row per task in both tables.
- `PHASE_OUTCOME.md`, both copies: `## UNIT 555 - STEP 12`.
- `PHASE_STATUS.md`, both copies: names 555.
- `Directory.Build.props`: 1.13.240.
- `CLAUDE.md` §1: a row.
- `DECISIONS.md`: HM-DEC-259, superseding HM-DEC-258 on which lone letters are dropped.

**Build** `Hamlet.sln -warnaserror`: 0 warnings, 0 errors.

**Both guards:** 4 of 4 pass, with the twelve at 237 and the w1aw table at 2112.

**The decision-log and voice tests:** 7 of 7.

**App carry-forward:** 276 of 278.
- `TheRstIsYoursToCorrectTests.OnTheWindowTheTwoReportsAreBoxesWithTheirMarks` failed with `You've caused dispatcher loop`.
- `Unit376TheTopBandTests.TheTopBandIsOneShortRowAndThePanelsAreTallerByTheDifference` failed with `You've caused dispatcher loop`.
- Both pass alone (2 of 2).

## 2. What the owner should expect

Rebuild and run as usual.

- **ADVOCATE TILL and LAST TEN read whole.** Hamlet now throws away a string of one-letter E's and T's only when every one of them stands alone as its own word, the way noise prints. So AT THE, THE TEST, ATE TO and the rest of English come through even where the string crosses a word gap.
  - As you ruled, this costs one letter on the twelve recordings: 237 where it was 238.
  - W1AW's table goes up, from 2097 to 2112.
- **When the radio's pitch goes unread for a moment, nothing happens now.** Hamlet asks the radio twenty times a second, and a reading that times out used to make it listen to the whole band for a moment and then start over. Each blip cost a few letters, and on W1AW played back with a blip every ten seconds about a hundred words came out different. Now it keeps the pitch and filter it last read until the radio reports something new.
- **It didn't make the 550 Hz station**, so that ghost is still unexplained.
- **What the next session's sheets will tell:** each capture sheet now names the build that heard it, how many times the detector rebuilt its passband since listening started, and which senders it was already holding when the capture began. The 10-second telemetry rows carry the build and the rebuild count too. If the 550 Hz sender shows up again, the next sheets will say whether it was there before W1AW started, whether the passband moved, and which version was running.
- **The H at 35 WPM:** in the 35 WPM text in the tree, five of six H's read whole. The one that doesn't has its first dot arrive weak and broken in the recording itself; even the offline reading loses it. A made-up line full of H's at 35 WPM reads perfectly, steady or fading, so there was nothing plain to fix. The qualifying run where you saw it often isn't in the tree.
- **What will look wrong but is not:** the app's test line shows "dispatcher loop" failures, a different test each run, and each passes on its own.
- Pushed to `main`.

## 3. What you should see

**Both tables' rows:**

| unit | right | wrong | invented | score | printed in silence | spaces |
|---|---|---|---|---|---|---|
| HEAD | 252 of 288 | 14 | 0 | **238** | 0 | 65 of 87, 2 added |
| 555 task 1 | 254 of 288 | 17 | 0 | **237** | 0 | 67 of 87, 3 added |
| 555 task 2 | 254 of 288 | 17 | 0 | **237** | 0 | 67 of 87, 3 added |
| 555 task 3 | 254 of 288 | 17 | 0 | **237** | 0 | 67 of 87, 3 added |

| w1aw | score | right | wrong | invented | spaces |
|---|---|---|---|---|---|
| HEAD | **2097** | 2116 of 2151 | 19 | 0 | 438 of 440, 11 added |
| 555 task 1 | **2112** | 2131 of 2151 | 19 | 0 | 438 of 440, 11 added |
| 555 task 2 | **2112** | 2131 of 2151 | 19 | 0 | 438 of 440, 11 added |
| 555 task 3 | **2112** | 2131 of 2151 | 19 | 0 | 438 of 440, 11 added |

The random carrier prints at seed 5195 only, as at HEAD, and the loud-noise runs print nothing.

Task 1 moves these stretches of the twelve (HEAD then after):

| stretch | HEAD | after |
|---|---|---|
| `221805`, 5-30 s | `40T U THESE DAYS. …`, 27 right, 1 wrong | `E 40T U THESE DAYS. …`, 28 right, 1 wrong |
| `221745`, 0-28 s | `IAND TMN40M …`, 20 right, 6 wrong | `TEIAND TMN40M …`, 21 right, 7 wrong |
| `221548`, 18-30 s | `E WRHEE MYSCO MASTEN`, 1 wrong | `E WRHEE MYSCOET T MASTEN`, 3 wrong |
| `221548`, 0-18.5 s (low) | `I M I I USINGA …`, 16 right | `I M IEE IEE E USINGA …`, 18 right |
| `143951`, 14.5-30 s (low) | `O WA IEWRK …`, 3 wrong | `O WAE EEIEWRK …`, 5 wrong |
| `143906` (none) | `I I I E NI I AEEYDEWB2FU` | `IEE I I E NIEE IE T T AEEYDEWB2FU` |

**Task 1's lines**, 30 WPM, through the filter with the bench's AGC:

| sent | HEAD | after |
|---|---|---|
| `ADVOCATE TILL HE WENT` | `ADVOCA ILL HE WENT` | whole |
| `FOR THE LAST TEN YEARS` | `FOR THE LAS N YEARS` | whole |
| `IT IS LEGITIMATE TO INFER` | `IT IS LEGITIMA O INFER` | whole |
| `AT THE TEST` | `AT TH ST` | whole |
| `DETECT THE RADIATED SIGNAL`, `PERFORMANCE IS DETERMINED BY`, `HUNDREDS OF KILOMETERS`, `INSPIRED TO ATTEMPT A TRANSVERTER`, `A BETTER LETTER` | whole | whole |
| `E T E T E`, five lone words | nothing | nothing |

**The scripted rig's senders, before and after** (session seconds; pitch read dropped one tick every 20 s and three ticks every 20 s):

| | pitch | seen | marks | shape | printed |
|---|---|---|---|---|---|
| steady | 600 Hz | 63 to 1182 s | 5772 | 0.95 | 1102 s |
| steady | 675 / 700 / 800 Hz | 521-580 / 315-318 / 205-211 s | 6 / 5 / 5 | 0.54 / 0.61 / 0.63 | never |
| flickering, before | 600 Hz | 63 to 1182 s | 5630 | 0.95 | 1099 s |
| flickering, before | 375 Hz | 299 to 364 s | 8 | 0.00 | never |
| flickering, before | 475 Hz | 29 to 50 s | 5 | 0.52 | never |
| flickering, before | 700 Hz | 318 to 322 s | 6 | 0.41 | never |
| flickering, after | as steady, every row | | | | |

- **Text against the steady replay:** 101 places before, 0 after. Before, it read `KST QST QST IE W1AW …`, `20■ 6`, `3SE 30 25 2M`.
- **Passband rebuilds:** steady 1, flickering after 1.

**The `H` trace**, piece 2's 35 WPM text. In the window, each hop is 5 ms: `#` is within 6 dB of the key-down level, `+` within 10 dB, `.` under that.

| H of | at | offline marks | window | the gate made |
|---|---|---|---|---|
| THEN | 255.264 s | 45 39 39 41 ms | `....+########......########......#######......+#######` | `H` 41 35 34 37 ms |
| THE (read `TSE` offline) | 257.805 s | 34 41 40 ms | `....+#+.+##.+.....########......########` | `I` 40 37 ms |
| THE | 262.809 s | 42 40 40 39 ms | `....+#######+.....+#######+.....########......########` | `H` 38 36 37 35 ms |
| THE | 266.926 s | 39 40 41 38 ms | `....#######+.....+#######+.....########......#######+` | `H` 35 37 36 35 ms |
| THE | 293.820 s | 44 38 41 39 ms | `....+########......#######+......########.....+#######+` | `H` 41 35 40 37 ms |
| THE | 297.944 s | 40 41 40 38 ms | `....+#######......########......########......#######` | `H` 36 37 37 34 ms |

The synthetic line `THE HELD HABITS OF HIS SMITH`, 35 WPM, reads whole steady and with a 6 dB fade.

## 4. What's blocking us

- **The 550 Hz ghost is still not found.** Nothing in the audio, the chunk size, the builds or a flickering passband makes it. The next live session's sheets now say which build ran, how often the passband was rebuilt, and which senders stood when the capture began.
- **The twelve read 237** under the owner's ruling. Spaces added went from 2 to 3; the guard holds the new row.
- **The H fault at 30 and 35 WPM** is seen on the qualifying run, which is not in the tree. On what is in the tree it is one H in six, and the fault is in the audio.
- **The dispatcher-loop failures** in the app's test line hit a different test each run and are not this unit's.

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
- **Unit 553, 2026-10-07:** these figures are the author's from measurement, and wait on the owner:
  - the hysteresis, 0.4 up and 0.6 down;
  - the half-dit floor on the dit the window opened on;
  - opening the window on the waiting sender.

  The change sits in `src/Hamlet.RadioEngine/Cw/CwEnvelopeDetector.cs`.
- **Unit 554, 2026-10-07:** these are the author's and wait on the owner (the ask on task 1's rule is answered by HM-DEC-259 and dropped):
  - the trim's two hops and 3 dB;
  - the w1aw reference's corrections and omissions.

  The changes sit in `src/Hamlet.RadioEngine/Cw/CwEnvelopeDetector.cs` and `tests/Hamlet.RadioEngine.Tests/Cw/TheW1awTableTests.cs`.
- **Unit 555, 2026-10-07:** these are the author's and wait on the owner:
  - what lets the held passband go: another mode read, or a rig state with nothing known; an unknown mode alone holds;
  - the senders `at start` are noted on the first once-a-second tick a capture is under way.

  The changes sit in `src/Hamlet.RadioEngine/Cw/CwPassbandHold.cs` and `src/Hamlet.App/ViewModels/MainWindowViewModel.AutoCapture.cs`.
