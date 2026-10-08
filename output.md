## 1. What Claude did

Development computer, project gate `PROJECT: Hamlet` checked against `PROJECT_CARD.md`, the order's five checks (`SHACK_FACTS.md`, `CwPatternGate.cs`, `cw-2026-10-03-143906.wav` present; no `CoreHMI.sln`, no `MURC.sln`) and `Hamlet.sln`: Hamlet confirmed. Nothing here is evidence about the radio. Branch `main`. Run by hand: `SESSION.lock` taken at 08:00:43 through `tools\arbiter\lock.bat take` and released at the end, nothing written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` touched, no box ticked in `PHASE_PLAN.md`. R88 lifted only as the order named, and no recording added. Version 1.13.241 to **1.13.242**. Ruling **HM-DEC-261**, the number the order gave. Nothing was recorded under §12.1. Every test run was one filtered invocation; scripts went in `.run-unit\unit557-*.sh`, not committed. Both scoreboards ran after every variant and after each task.

**Task 1, a floor under the fastest dit: measured, not shipped** (`1f4d210a`, the scoreboard rows; the engine is as at HEAD).
- **Where the ceiling lives:** there is no 40 WPM ceiling in the tree.
  - The fastest speed it names is **48 WPM**, `CwDecoder.FastestPlausibleWpm`, "the fastest the radio's own keyer sends". It bounds the printed speed.
  - A dit at 48 WPM is 25 ms, and the detector's shortest bar, `CwEnvelopeDetector.ShortestBarMs`, is the same 25 ms.
- **The floor was built from that number,** `1.2 / FastestPlausibleWpm`, in the gate where a mark joins a sender. 40 WPM was measured beside it through an environment variable, for this report only. Four variants were measured (section 3).
- **Every variant lowered the twelve:** 235, 230, 230 and 227. Some invented letters, and none made `143906` read better. Under the order's bar, that neither table may fall, nothing shipped and the change was reverted.
- **Not built:** the synthetic cases the order named (a 25 WPM call at 11 dB with blips before it, and a 40 WPM call). The real recordings had already decided the task.

**Task 2, the carrier and noise:** answered by the same runs, since each board run carries the hard limits.
- The random carrier printed at seed 5195 only under every floor, as at HEAD.
- The four loud-noise runs printed nothing.
- The guard line is unchanged.

**Records:**
- `docs\cw-scoreboard.md`: rows for both tasks in both tables.
- `PHASE_OUTCOME.md`, both copies: `## UNIT 557 - STEP 12`.
- `PHASE_STATUS.md`, both copies: names 557.
- `Directory.Build.props`: 1.13.242.
- `CLAUDE.md` §1: a row.
- `DECISIONS.md`: HM-DEC-261.

**Build** `Hamlet.sln -warnaserror`: 0 warnings, 0 errors.

**Both guards:** 4 of 4. **The decision-log and voice tests:** 7 of 7. **App carry-forward:** 278 of 278.

## 2. What the owner should expect

Rebuild and run as usual. **Nothing about reading Morse changed this time.**

- **The weak fast station reads as before:** junk at the start, then `DE WB2FU` once Hamlet has hold of it.
- **Hamlet still builds a station partly out of noise blips** before it starts sending properly.
- **Why the floor didn't work:** throwing away every mark shorter than the fastest Morse dot cost real letters elsewhere, because a real dash that dips in the middle leaves short pieces too. Doing it only for stations not yet confirmed let a junk sender look cleaner, and it printed letters that were never sent. Every version lost letters on your twelve recordings, so none went in.
- **The speed ceiling:** the work order said Hamlet reads up to 40 WPM. The code's ceiling is 48 WPM, the fastest your radio's keyer sends. Even at 40 the floor did worse. Whether the ceiling should be 40 is yours to say (section 4).
- **The carrier and noise:** the random-carrier test still prints at the same one seed out of twenty, and the loud noise prints nothing, with or without any floor.
- **Both scoreboards:** 237 and 2112, unchanged.
- Pushed to `main`.

## 3. What you should see

**Both tables' rows:**

| unit | right | wrong | invented | score | printed in silence | spaces |
|---|---|---|---|---|---|---|
| HEAD | 254 of 288 | 17 | 0 | **237** | 0 | 67 of 87, 3 added |
| 557 tasks 1 and 2 | 254 of 288 | 17 | 0 | **237** | 0 | 67 of 87, 3 added |

| w1aw | score | right | wrong | invented | spaces |
|---|---|---|---|---|---|
| HEAD | **2112** | 2131 of 2151 | 19 | 0 | 438 of 440, 11 added |
| 557 tasks 1 and 2 | **2112** | 2131 of 2151 | 19 | 0 | 438 of 440, 11 added |

**The floor, measured and reverted:**

| floor | applied to | the twelve | invented | w1aw | `143906` reads |
|---|---|---|---|---|---|
| none (HEAD) | | 237 | 0 | 2112 | `IEE I I E NIEE IE T T AEEYDEWB2FU` |
| 48 WPM, 25 ms | every sender | 235 | 0 | 2112 | `I I E NEEE E ITTKIWDEWB2FU` |
| 40 WPM, 30 ms | every sender | 230 | 2 | 2108 | nothing |
| 48 WPM, 25 ms | senders not yet qualified | 230 | 5 | 2112 | `I I E NIEE IE T T AEEYDEWB2FU` |
| 40 WPM, 30 ms | senders not yet qualified | 227 | 5 | 2112 | nothing |

**What moved on the twelve with the floor on every sender, 48 WPM:**

| stretch | HEAD | with the floor |
|---|---|---|
| `221805` | `E 40T U THESE DAYS. …`, 28 right | `40T U THESE DAYS. …`, 27 right |
| `221745` | `TEIAND TMN40M …`, 21 right | `T IAND TMN40M …`, 20 right |
| `221548`, low | `I M IEE IEE E USINGA …`, 5 wrong | `I M RW USINGA …`, 1 wrong |
| `221851`, low | nothing | `SES EIEAET EI G IENAAEIBK`, 6 right, 14 wrong |

With the floor on senders not yet qualified, `221851` prints `… XETESTA RF EE■ EISNR` with **5 letters invented**.

**The synthetic cases:** not built (section 1).

**The carrier seeds:**
- HEAD and both 48 WPM variants: 1 of 20 prints, seed 5195, `NTTTTNTE TEAETOTTNEAIT TAN`.
- Both 40 WPM variants: 1 of 20, seed 5195, `TGTE TEAETOTTNEAIT TAN`.
- Loud noise, 30 s at seeds 5130 and 5220 and 180 s at 5280 and 5370: nothing in every run.

## 4. What's blocking us

- **The weak fast station** still stands on noise blips before it keys. A floor at the fastest dit can't separate them without costing real letters, and no other rule has been measured yet.
- **The ceiling:** the work order puts Hamlet's ceiling at 40 WPM; the tree's is 48, the radio keyer's fastest. The floor was measured at both, and both cost letters.

### Asks still outstanding

- **Unit 557, 2026-10-08:** whether Hamlet's fastest readable speed is 48 WPM, as `CwDecoder.FastestPlausibleWpm` has it (the radio keyer's fastest), or 40 WPM, as work instruction 557 assumed. It waits on the owner. No change for it sits in the tree; the floor measured from either number is reverted.
- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
