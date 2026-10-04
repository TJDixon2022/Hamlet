```
UNIT: 536 - both tasks done - 2026-10-04
UNIT GOAL: the carrier rule ships, and the last two seeds
NUMBER: random carriers alone print at 1 of 20 seeds, from 9; scoreboard holds at 205 of 244
```

## 1. What Claude did

Claude Code on the development machine, branch `main`. The prompt claimed `PROJECT: Hamlet`, and the order's gate held: `SHACK_FACTS.md`, `docs\cw-scoreboard.md` and `cw-2026-10-03-221828.wav` exist, there is no `CoreHMI.sln` or `MURC.sln`, and the root is `C:\Source\HamLet`. Hamlet confirmed. Nothing in this report is evidence about the radio beyond the owner's twelve recordings. HM-DEC-240 was free.

**How the session ran:**
- It took SESSION.lock and released it at the end.
  - It took the lock a second time to correct this report: the first closing commit (`44c178d7`) went out with the previous unit's `output.md`, because the write failed.
- It wrote nothing to `RUN_LEDGER.md`, touched nothing under `tools\arbiter\`, and ticked no box.
- R88 was lifted for the twelve recordings and no other. Nothing keys, transmits or writes to the radio.
- The scoreboard was run after each task, and both rows are appended.

**Task 1: the carrier rule ships.** Commit `96599aca`.
- **The rule.** A sender qualifies to print only where its last ten marks split into two length kinds with a clean 2:1 jump, neither side wider than a hand makes, and each kind recurring, two marks or more. It is switchable as `CwRules.KindsHeld`.
- **Re-measured at HEAD it holds the total, 205 of 244**, so it ships, with the carrier limit still asserted and red.
- **Random carriers alone print at 2 of 20 seeds, down from 9.** Beside a clean sender at 100 Hz, nothing prints at all.
- **It moves letters between stretches, not out of the total:**

| stretch | before | after |
|---|---|---|
| 143951 | 4 | 11 |
| 221502 | 27 | 30 |
| 221745 | 17 | 19 |
| 221530 at 491.5 Hz | 8 | 6 |
| 221548 at 597.7 Hz | 22 | 18 |
| 221851 | 13 | 5 |

  The lower stretches are mostly low confidence, so they don't count toward the total.

**Task 2: the last two seeds.** Commit `57be07e0`.
- **Where the letters go.** Last unit the gap condition cost 4 letters, measured before the heavy-keying fix. At HEAD, a clean 2:1 jump among the nine gaps costs **13 letters, all on 22:15:48 at 498 Hz** (`YRHEE MY SCOUT MASTER`), which drops from 13 of 18 to nothing printed.
- **Why its gaps fail.** At the moment it would qualify, its gaps are `60 60 65 65 65 75 140 270 460` ms. Inside a letter it leaves 60 to 75 ms and between letters 140 to 270 ms, so the jump is 75 to 140, a ratio of **1.87**, short of 2.
- **The condition from what CW is.** Morse's own line between a gap inside a letter (one unit) and one between letters (three) is their midpoint, √3 (1.73), the line the gate already uses for those gaps. The 2:1 jump was the marks' rule borrowed for gaps.
- **With a √3 jump, at least two gaps either side, it ships:**
  - the scoreboard holds at 205 of 244;
  - seed 5206 goes quiet, and **1 of 20 still prints (5195)**;
  - switchable as `CwRules.GapKinds`.

**Records:**
- HM-DEC-240 in `DECISIONS.md`, and the `CLAUDE.md` row.
- `PHASE_OUTCOME` (both copies) has `## UNIT 536 - STEP 12`.
- `PHASE_STATUS` (both copies) names 536.
- Version 1.13.220 to 1.13.221.

**Build and app line:** build 0 warnings, 0 errors. App carry-forward 276 of 278. The two losses, `TheCqPressWritesTheLabelTheOperatorPressed("Olivia")` and `OnTheWindowTheTwoReportsAreBoxesWithTheirMarks`, are at 1 ms ("You've caused dispatcher loop") and pass alone.

## 2. What the owner should expect

- **Rebuild.**
- **Random carriers: 1 of 20 still prints, from 9.**
  - Hamlet now prints a sender only after it has shown dits and dahs again and again over its last ten marks, with gaps that fall into Morse's own kinds.
  - A carrier keyed at random mostly fails that. Beside a real station 100 Hz away it prints nothing.
  - The hard limit stays red on the one seed that still gets through.
- **Your recordings: 205 of 244, unchanged.** Some stretches read better and some worse, but the total held. 14:39:51 now reads `EWAE IIEWRK I AGN ES`, close to its reference.
- **One cost you may notice.** A station that sends fewer than ten marks and stops, like a bare `DE DE`, now prints nothing. A real over is far longer than ten marks, and its letters print from its first once it qualifies.

## 3. What you should see

**The scoreboard rows:**

| row | total | hard limits |
|---|---|---|
| 535 task 4 (HEAD) | 205 of 244 | carrier limit red: 2 of 5 print |
| 536 task 1 | 205 of 244 | carrier limit red: 1 of 5 (5195) |
| 536 task 2 | 205 of 244 | carrier limit red: 1 of 5 (5195) |

**The scoreboard after both tasks:**

| recording | pitch | printed | right |
|---|---|---|---|
| 200157 | 662.8 | `FERCHAT<BT>BEST7V73<SK>KC4ZGPDEWA` | 27/27 |
| 143906 | 514.2 | `IEEIIENIEEEIETTAEEKEEIEEIIIMESEITT` | - |
| 143951 | 499.5 | `EWAEIIEWRKIAGNES` | 12/16 |
| 144020 | 499.5 | `ESOKONPA<BT>` | 9/9 |
| 144020 | 599.9 | `WXINNETAGITIENTEMP` | 17/19 |
| 144045 | 599.9 | `NEMP5757<BT>BTUBOBDEKG8VK` | 22/23 |
| 221502 | 491.5 | `EDOFITSOWNHEEBKBKWHATBUGAEIEUIE■■` | 29/42 |
| 221530 | 491.5 | `HCHAMPIMTN` | 6/11 |
| 221530 | 598.4 | `IHESMHENI■SAGE12ILEAREDCWUSINGAV` | 27/34 |
| 221548 | 597.7 | `IMIEEIEEEUSINGAVBPLXZEPS` | 18/26 |
| 221548 | 498.0 | `EWRHEEMYSCOATQSTEN` | 13/18 |
| 221745 | 501.7 | `EIANDTMN4MEMMTONITE.EUROWEEHIRDTOO` | 19/29 |
| 221805 | 601.3 | `E40TUTHESEDAYS.TNXFERANOTHERF` | 28/33 |
| 221828 | 601.3 | `FERANOTHERFBQSOESHOPEUHAVEAGNEDESBESTEVAMU` | 37/41 |
| 221851 | 601.3 | `SESEIEEIEAEIGEEIENAFEIBK` | 5/20 |
| **total** | | | **205/244** |

**The carrier seeds, alone:**

| seeds | HEAD | after task 1 | after task 2 |
|---|---|---|---|
| 5195 | prints | prints `NTTTTNTE TEAETOTTNEAIT TANTTTTK` | prints, the same |
| 5206 | prints | prints `TTMET ETOT TTATAUTTAT TOMOTEG` | quiet |
| 5197, 5198, 5202, 5204, 5205, 5210, 5211 | print | quiet | quiet |
| the other eleven | quiet | quiet | quiet |
| **printing** | **9 of 20** | **2 of 20** | **1 of 20** |

**Beside a clean sender:**

| carrier | HEAD | now |
|---|---|---|
| 100 Hz away | `TTNOAM0TT ETMTTYTMTT` (red) | nothing at all |
| 150 Hz away | nothing at the carrier; the clean sender garbled | the same |
| 200 Hz away | nothing at the carrier; the clean sender garbled | the same |
| 400 Hz away | the clean sender whole | the same |

**Task 2's letters:** the 2:1 gap condition cost 13 letters at HEAD (last unit's 4 were measured before the heavy-keying fix). All 13 are on 22:15:48 at 498 Hz, which printed nothing with it. Its gaps were `60 60 65 65 65 75 140 270 460` ms, a jump of 1.87 from inside-letter to letter gaps. At √3 it prints `EWRHEEMYSCOATQSTEN`, 13 of 18, as before.

**Synthetic sets, against unit 535:**
- **b1:** identical.
- **a:** one new red, `TheLoneLettersInsideWordsStillPrint("DE DE")`, which reads nothing: eight marks never reach the ten the rule needs. Two "gates off" diagnostic rows changed their garbage.
- **b4:** the carrier lines changed as above. With shape picking off, the carrier beside a sender no longer prints.
- **b5:** the same failing set, and every reading line identical except candidate counts and the two untotalled recordings.

## 4. What's blocking us

1. **A transmission under ten marks never prints.** `DE DE` alone reads nothing.

   | option | for | against |
   |---|---|---|
   | A. accept it | a real over is far longer; random carriers stay closed | a lone short call or `K` after a long gap may not show |
   | B. let a sender already printed recently qualify again on fewer marks | short replies from a known station print | a second rule to measure |

   The usual practice is A until a recording shows a short transmission lost.
2. **One random carrier still prints** (seed 5195). Its last ten marks show both kinds with a 2:1 jump, and its gaps a √3 jump, by chance.
3. **Pre-existing app reds** outside the line are untouched.

### Asks still outstanding

- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
