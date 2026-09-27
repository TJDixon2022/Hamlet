READ IN THIS ORDER.

A. Phase goal: Hamlet meets the CW requirements. Steps by the plan -
   0 done, 1 done, 2 4 of 5, 3 3 of 6, 4 5 of 7, 5 1 of 6, 6 3 of 6,
   7 2 of 5, 8 0 of 6, 9 7 of 8; steps 2 to 8 still barred by R86,
   and 9.4 open with every mechanism 9.3 named refused.
B. Step 9, criterion 9.7 (HM-REQ-128): condition rows 10, summary
   rows synthetic, all, live row real HF, all; re-watch red on real
   TX-FARNS, synthetic TX-ITU 0 dB, char-gap-5 0 dB and char-gap-5
   5 dB against an empty table; green on every condition row yes;
   summary synthetic, all emitted against ours alone on 012 158 vs 159
   of 252 and 081 48 vs 44 of 84; src unchanged yes; the port
   byte-identical yes; emitted text byte-identical to entry yes;
   9.7 ticked; 9.8 ticked.
C. The findings weighed against A and B: section 4 raises 7 items.
   None is in the way of 9.7 or 9.8. Item 1 is the reading that 9.7
   rests on: which rows are conditions is the arbiter's reading of
   CW_SPEC.md section 4, overrulable, and 9.7 reopens if union rows
   are ruled conditions. Item 4 is the one that matters for the
   phase: with 9.7 and 9.8 ticked, step 9's only open line is 9.4,
   which has no authorable route, and R86 holds steps 2 to 8 behind
   it. So R86 with 9.4's refusals leaves no step 9 line, and no other
   step, authorable. Nothing touches transmit or the operator's screen.

UNIT:       467 - complete at task 2 of 2, drop candidate dropped (the live-path reprint of the synthetic condition rows) - 2026-09-27 10:35
PHASE GOAL: Hamlet's CW decoding meets every requirement in CW_REQUIREMENTS.md at the conditions CW_SPEC.md names, each proved by a test that names it. While no technique from fldigi is kept in ours, R86 routes every unit through step 9.
UNIT GOAL:  Judge HM-REQ-128 on the rows CW_SPEC.md section 4 calls conditions (one named profile at one level), plus the live product's own row on the live path. Print union rows as summaries without asserting them. Re-watch the test red on the losing condition rows against an empty switch table, run it green, and tick 9.7 and 9.8, with no switch, rule or src line changed.
ADVANCED:   yes - 9.7 and 9.8 ticked. HM-REQ-128's test now asserts the 10 condition rows and the live row, was watched red on exactly the 4 losing rows, and is green. The exit round was green on all five checks at first run.
NUMBER:     HM-REQ-128 met on 10 of 10 condition rows and the live row; re-watch red on 4 rows; real HF, all MET-CER-SURE arb/ours/port 33/436 / 33/436 / 64/221 live
DRIFT:      step 2 1; step 3 0; step 4 1; step 5 1; step 6 0; step 7 0; step 9 0 (was 1)

## 1. What Claude did

**Complete at task 2 of 2. I dropped the named drop candidate, the live-path reprint of the synthetic condition rows, and nothing else.** 9.7 and 9.8 are ticked. Provenance: Claude Code on QUIVERFULL, project Hamlet, gate passed (`SHACK_FACTS.md`, `CwProbabilisticDecoder.cs`, `CW_REQUIREMENTS.md` and `CW_SPEC.md` present; `CoreHMI.sln` and `MURC.sln` absent), branch `main`, HEAD at entry `d34a0563`.

**The tree against section 3.** Everything matched, with two exceptions:
- `.run-unit\watched.rc` does not exist. At session start the runner had deleted `watched.cpu`. It rewrote the file before task 0's commit, which took it as it was. The runner has modified it again since, and I left that unstaged as the runner's.
- `SESSION.lock` (PID 35488) is the launcher's and was not staged.

`.run-unit\fldigi\` was not staged, and nothing was fetched. Both `PHASE_PLAN.md` copies were identical at entry: 9.1, 9.2, 9.3, 9.5 and 9.6 ticked, 9.4, 9.7 and 9.8 open, and R86 in section 6.

**Task 0.**
- Wrote the record: `## UNIT 467 - STEP 9` in both `PHASE_OUTCOME.md` copies, and both `PHASE_STATUS.md` copies at 467, step 9.
- Moved the version from 1.13.153 to 1.13.154.
- Committed the runner's writes as they were.
- Every entry figure equals 466's exit except decode time: 65.50 s over 690 s against 64.27 s (section 3, item 5).
- Saved the emitted transcript with class and p: harness 1458 lines (`unit467-emitted-before.txt`) and live 1467 lines (`unit467-emitted-live-before.txt`). Both are byte-identical to 466's exit saves.
- Commit `000ee77c`.

**Task 1.**
- **Classified all 12 rows:** 10 conditions, the live row and 1 summary (section 3 leads with them).
- **The classification lives in one list,** `TheArbitrationEarnsItsPlaceTests.Decided2And3Rows`, which quotes `CW_SPEC.md` section 4. `TheArbitrationEarnsItsPlaceFact`'s header carries it as paragraph 4. The fact prints it into the new section 8 of `arbitration.md`, together with the union rows' three figures and the emitted output.
- **Narrowed part (b):** the harness fact asserts the condition rows and the live fact asserts real HF, all. Part (a) is untouched.
- **Re-watch against an empty table:** red on exactly the 4 losing condition rows. With the table restored it is green.
- **Byte-identical checks:** the emitted transcripts, harness and live, and the port all match.
- **At the commit:** floors 51/13/13; engine line 178/178; app line 277/278, then 278/278 on the one rerun (DECIDED (6)).
- Commits: `1b51e222`, then the 9.7 tick `3fadb6f6`.

**Task 2.** Ran the exit round. Every figure is as at entry, and all five checks were green at first run (section 3, item 5). Ticked 9.8 in its own commit, `e27fd7fa`. This report and the exit printouts are the task 2 commit.

**Decisions I made for myself:**
- **The re-watch empties the test's table, not `src`'s.** Part (b) now reads its switch table from one field in the test, `Table`, set to `CwSwitchTable.Rows`. For the re-watch that field alone was set to an empty dictionary, rebuilt and run, then restored. An empty table is `CwSwitchTable.For`'s default everywhere: every row arbitrates, the same as 466's `Rows` emptied. This kept task 1's rule that nothing under `src` changes, and CLAUDE.md 12.5's rule that the re-watch does not use the table under test. The live fact reads the same field in place of `CwSwitchTable.Live`, and the two are identical when the field holds `Rows`.
- **Section 8 of `arbitration.md` is generated by the fact, not hand-written.** `TheThreeWayTable` rewrites `arbitration.md` whole, so a hand-written section would vanish on the next run. The fact now also scores the output emitted under the switch in the harness, for section 8. It still asserts only that all 35 were read. Sections 1 to 7 came out byte-identical to 466's, apart from the first line's credit to this unit.
- **The test checks the list against the rows.** Part (b) fails if a computed row is missing from `Decided2And3Rows`, or if a listed row of the class it asserts was not computed. A renamed row therefore cannot silently drop out of the assertion.
- **The harness fact now prints real HF, all**, marked "Live, printed, not asserted". Before, it filtered that row out. The row is asserted only in the live fact, as before.
- **Real, sender not stated is classed a condition** (section 4, item 2).

## 2. What the owner should expect

Nothing on the screen changes, and nothing Hamlet prints changes. Both transcripts, the test harness's and the live product's, are character for character what they were at the start of this unit. "Every condition" in HM-REQ-128 was read the way `CW_SPEC.md` section 4 defines a condition: one named kind of signal (a sender, a channel, an interference or an impairment profile) at one signal-to-noise level, never a mix. The work instruction's ruling applied that definition, and it is overrulable. On that reading there are 10 condition rows in the tree, plus the everyday real-HF row the product actually runs under. On every one of them, the transcript with two readers is no worse than the better reader alone. So no condition is worse for having two readers. The mixed synthetic test set, taken as a whole, is not a condition: it blends two kinds of sender at three signal levels. On that set, switching to fldigi's reader on one kind of signal (5 dB, 5-unit letter gaps) costs one right letter (158 against 159 of 252) and four word spaces (48 against 44 errors in 84 words), while cutting wrong confident letters from 14 to 6. That set is printed in full, not hidden, and the test does not judge it. If you rule that mixed sets count as conditions, 9.7 reopens.

**What will look wrong but is not:**
- HM-REQ-128's test no longer goes red on `synthetic, all`. It is not asserted there, by the reading above. It is still printed.
- `arbitration.md` has a new section 8 after "What the table does not prove", so section 7 is no longer the last section.

## 3. What you should see

**The row classification** (`arbitration.md` section 8; `Decided2And3Rows`). `CW_SPEC.md` section 4: *"Every condition is a named profile from this file (`CH-*`, `TX-*`, `INT-*`, `IMP-*`) and an SNR in the reference bandwidth (§8), never prose."* The verification table, row 010: *"one row per condition profile"*.

| row | class | SNR | reason | asserted by HM-REQ-128's test |
|---|---|---|---|---|
| real HF, all | **live** | not measured | the row the live product runs under; the union of the 23 real recordings, judged on the live path only (DECIDED (3)) | yes, live path |
| real TX-FARNS | condition | not measured | one named sender profile, one recording | yes, harness |
| real TX-ITU (KD0UN) | condition | not measured | one named sender profile, one recording | yes, harness |
| real TX-TIGHT | condition | not measured | one named sender profile, one recording | yes, harness |
| real, sender not stated | condition | not measured | the real per-sender row for the 20 recordings section 10 names no sender for; a condition by DECIDED (3), though it names no profile (section 4, item 2) | yes, harness |
| **synthetic, all** | **summary** | mixed: 0, 5 and 15 dB | the union of six rows, two senders at three levels: not one profile at one SNR | no, printed only |
| syn TX-ITU, 0 dB | condition | 0 dB in the passband | one sender at one level | yes, harness |
| syn TX-ITU, 5 dB | condition | 5 dB in the passband | one sender at one level | yes, harness |
| syn TX-ITU, 15 dB | condition | 15 dB in the passband | one sender at one level | yes, harness |
| syn gap 5, 0 dB | condition | 0 dB in the passband | one sender (character gap 5, inside TX-FARNS) at one level | yes, harness |
| syn gap 5, 5 dB | condition | 5 dB in the passband | the same sender at one level | yes, harness |
| syn gap 5, 15 dB | condition | 15 dB in the passband | the same sender at one level | yes, harness |

Synthetic levels are in-passband and not restated in the 2500 Hz reference, as every earlier unit tabled them.

**The three-way table, harness, as unit 466 printed it, with a class column.** Each cell is arbitrated · ours alone · port alone. The last column is the output emitted under `CwSwitchTable`. `n.d.` means not defined. None of the figures has moved since 466: `arbitration.md` sections 3 and 4 regenerated byte-identical.

| row | class | 011 MET-INVENTED | 010 MET-CER-SURE | 012 coverage | 081 MET-WBE | loses? | switch | emitted (011, 010, 012, 081) | emitted no worse than the better alone? |
|---|---|---|---|---|---|---|---|---|---|
| **real HF, all** (harness; judged live) | live | 33/473 · 33/473 · 62/473 | 33/436 · 33/436 · 62/239 | 403/473 · 403/473 · 177/473 | 37/113 · 37/113 · 86/113 | no | arbitrate | 33/473, 33/436, 403/473, 37/113 | printed; asserted live, below |
| real TX-FARNS | condition | 0/44 · 0/44 · 3/44 | 0/43 · 0/43 · 3/33 | 43/44 · 43/44 · 30/44 | 5/11 · 5/11 · 4/11 | yes, 081 | ours alone | 0/44, 0/43, 43/44, 5/11 | yes (better alone: ours, at 011) |
| real TX-ITU | condition | 0/13 · 0/13 · 1/13 | 0/13 · 0/13 · 1/6 | 13/13 · 13/13 · 5/13 | 0/4 · 0/4 · 3/4 | no | arbitrate | 0/13, 0/13, 13/13, 0/4 | yes |
| real TX-TIGHT | condition | 0/6 · 0/6 · 2/6 | 0/6 · 0/6 · 2/3 | 6/6 · 6/6 · 1/6 | 0/1 · 0/1 · 0/1 | no | arbitrate | 0/6, 0/6, 6/6, 0/1 | yes |
| real, sender not stated | condition | 33/410 · 33/410 · 56/410 | 33/374 · 33/374 · 56/197 | 341/410 · 341/410 · 141/410 | 32/97 · 32/97 · 79/97 | no | arbitrate | 33/410, 33/374, 341/410, 32/97 | yes |
| **synthetic, all** | **summary** | 14/252 · 14/252 · 34/252 | 14/173 · 14/173 · 34/168 | 159/252 · 159/252 · 134/252 | 44/84 · 44/84 · 56/84 | no | arbitrate (no recording runs under it) | 6/252, 6/164, **158/252**, **48/84** | **no: worse than ours alone on 012 and 081; not asserted** |
| syn TX-ITU, 0 dB | condition | 0/63 · 0/63 · 21/63 | n.d. · n.d. · 21/27 | 0/63 · 0/63 · 6/63 | 18/21 · 18/21 · 15/21 | yes, 012, 081 | ours alone | 0/63, n.d., 0/63, 18/21 | yes (better alone: ours, at 011) |
| syn TX-ITU, 5 dB | condition | 1/63 · 1/63 · 5/63 | 1/63 · 1/63 · 5/47 | 62/63 · 62/63 · 42/63 | 0/21 · 0/21 · 6/21 | no | arbitrate | 1/63, 1/63, 62/63, 0/21 | yes |
| syn TX-ITU, 15 dB | condition | 1/63 · 1/63 · 2/63 | 1/64 · 1/64 · 2/56 | 63/63 · 63/63 · 54/63 | 0/21 · 0/21 · 3/21 | no | arbitrate | 1/63, 1/64, 63/63, 0/21 (013 0/63) | yes |
| syn gap 5, 0 dB | condition | 0/21 · 0/21 · 2/21 | n.d. · n.d. · 2/3 | 0/21 · 0/21 · 1/21 | 6/7 · 6/7 · 6/7 | yes, 012 | ours alone | 0/21, n.d., 0/21, 6/7 | yes (better alone: ours, at 011) |
| syn gap 5, 5 dB | condition | 11/21 · 11/21 · 3/21 | 11/25 · 11/25 · 3/16 | 14/21 · 14/21 · 13/21 | 8/7 · 8/7 · 12/7 | yes, 011, 010 | **port alone** | 3/21, 3/16, 13/21, 12/7 | yes (better alone: the port, at 011) |
| syn gap 5, 15 dB | condition | 1/21 · 1/21 · 1/21 | 1/21 · 1/21 · 1/19 | 20/21 · 20/21 · 18/21 | 12/7 · 12/7 · 14/7 | no | arbitrate | 1/21, 1/21, 20/21, 12/7 (013 1/21) | yes |

**The live row, on the live path**, as the test asserts it:
- **real HF, all**: 011 33/473 · 33/473 · 64/453; 010 33/436 · 33/436 · 64/221; 012 403/473 · 403/473 · 157/453; 081 37/113 · 37/113 · 78/107; 083 10/331 · 10/331; 084 0/3 · 0/3 · 0/3.
- **Verdict:** it does not lose, and ours is better by dominance. The switch is arbitrate, and the emitted output equals the arbitrated output.
- **The other live rows** are 466's live table, `arbitration.md` section 4, unchanged. This was the drop candidate.

**The summary row, stated plainly** (`arbitration.md` section 8):
- **What the switch costs:** `synthetic, all` emits worse than ours alone on 012 (158 against 159 of 252) and on 081 (48 against 44 of 84).
- **Where the cost comes from:** the char-gap-5 5 dB row, switched to the port alone. There the emitted output has 13/21 right against ours' 14/21, and 12/7 word-boundary errors against ours' 8/7.
- **What it gains:** on 011 and 010 the same set is better than ours alone (6/252 against 14/252, and 6/164 against 14/173).
- Real HF, all emits no worse than ours alone in the harness on any metric.

**1. The red and green printouts of part (b)** (`.run-unit/unit467-watch-red.txt` and `.run-unit/unit467-green.txt`).
- **Red, with the test's table empty:** `EveryHarnessRowIsNoWorseThanTheBetterDecoderAlone` failed on exactly these rows, 5 failure lines over 4 rows:
  - real TX-FARNS: the table says Arbitrate, the rule says OursAlone.
  - syn TX-ITU 0 dB: the table says Arbitrate, the rule says OursAlone.
  - syn gap 5 0 dB: the table says Arbitrate, the rule says OursAlone.
  - syn gap 5 5 dB: the table says Arbitrate, the rule says PortAlone; and it emitted worse than the better decoder alone (the port) on 011 and 010.

  Every other condition row was green. `synthetic, all` was printed as "Summary, printed, not asserted" and raised nothing. The live fact was green, 2 of 2: its row does not lose, so an empty table and the real table agree there.
- **Green, with `CwSwitchTable.Rows`:** the harness fact passed, with all 10 condition rows at "emitted worse than the better alone on nothing". The live facts passed, 2 of 2. The same results held again at exit.

**2. The byte-identical checks.**
- Emitted harness transcript (1458 lines) after task 1: `cmp` identical to task 0's save and to 466's exit save.
- Emitted live transcript (1467 lines) after task 1: `cmp` identical to task 0's save and to 466's exit save.
- `git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/`: nothing.
- `git diff d34a0563 -- src`: nothing.
- `git diff 7e209cb4` over the eleven transmit files `PARKED.md` names: nothing, and all eleven are present.
- `git status` at exit: `.run-unit/fldigi/` and `SESSION.lock` are untracked; `.run-unit/watched.cpu` is modified by the runner.

**3. The commit table**, with the five checks at each commit:

| commit | what | build | engine line | app line | captures | adjudicated | named |
|---|---|---|---|---|---|---|---|
| `000ee77c` | task 0 | 0 errors | 178/178 | 276/278, then 278/278 on the one rerun (2 lost, 1 ms, dispatcher loop) | 51/51 | 13/13 | 13/13 |
| `1b51e222` | task 1 | 0 errors | 178/178 | 277/278, then 278/278 on the one rerun (1 lost, 1 ms, dispatcher loop) | 51/51 | 13/13 | 13/13 |
| `3fadb6f6` | 9.7 tick | 0 errors | 178/178 | 278/278 | 51/51 | 13/13 | 13/13 |
| `e27fd7fa` | 9.8 tick | 0 errors | 178/178 | 278/278 | 51/51 | 13/13 | 13/13 |
| task 2 | this report and the exit printouts | 0 errors | 178/178 | 278/278 | 51/51 | 13/13 | 13/13 |

The exit round ran on the tree of `3fadb6f6`. Nothing under `src` or `tests` changed after it: `e27fd7fa` touches the plan, and task 2 touches the report and `.run-unit` only.

**4. Unit 466's figures carried**, unchanged: loses on 4 of 12 rows; switch arbitrate on 8, ours alone on 3 and the port alone on 1.

**5. The exit round beside entry.**

| check | entry | exit |
|---|---|---|
| build | 0 errors | 0 errors |
| engine line | 178 of 178 | 178 of 178 |
| app line | 276 of 278, then 278 of 278 on the one rerun | 278 of 278 |
| captures | 51 of 51 | 51 of 51 |
| adjudicated | 13 of 13 | 13 of 13 |
| named | 13 of 13 | 13 of 13 |
| real, inferred | MET-CER-SURE 33 of 436, MET-INVENTED 33/473, coverage 403/473, MET-WBE 37/113 | same; no recording's row moved (V-11, 35 of 35) |
| synthetic, exact | 14 of 173, 14/252, 159/252, 44/84 | same |
| `BothDecodersAreScoredAlikeTests` | 5 of 5; the port real 62 of 239, coverage 177; synthetic 34 of 168, coverage 134 | same; `parity.md` restored after each run |
| the port's own tests | 8 of 8 | 8 of 8 |
| 465's three | `TheOperatorSeesOneTranscriptTests` 3/3, `BothDecodersReadTheSameSamplesTests` 3/3, `TheHigherCalibratedReadingWinsTests` 8/8 | same |
| `EveryCharacterCarriesAConfidenceTests` | 2 of 2 | 2 of 2 |
| `TheArbitrationEarnsItsPlaceTests` | (a) 5 of 5; (b) live 2 of 2; (b) harness **red on synthetic, all** | (a) 5 of 5; (b) live 2 of 2; (b) harness **green** |
| `TheSpeedFollowsTheSendersMarkPairsTests` | - | red, 28 of 31 sure wrong or added (as at 466's exit; not required) |
| decode time, live path, 690 s real audio | 65.50 s (ours alone 52.73 s) | 64.97 s after task 1 (ours alone 52.54 s); 466's exit was 64.27 s |

## 4. What's blocking us

Nothing blocks this unit. Every item below is a plain reading or a log entry, not a request for a ruling.

1. **Which rows are conditions is the arbiter's reading of `CW_SPEC.md` section 4, overrulable.**
   - **The reading:** a condition is one named profile at one level. `synthetic, all`, a union across two senders and three levels, is a summary: printed, not asserted.
   - **Rejected:** counting union rows as conditions. The document defines a condition as one profile at one SNR, and no per-row switch table can satisfy both a union and its sub-row once the order rule picks the port on the sub-row (466's section 4, item 1).
   - **If the owner rules union rows are conditions, 9.7 reopens.**
2. **Real, sender not stated is classed a condition.**
   - **Why:** DECIDED (3) calls the real per-sender rows conditions "as unit 466 tabled them", and asserting the row cannot loosen HM-REQ-128's test.
   - **The tension:** read strictly, the row names no profile, so `CW_SPEC.md` section 4 would not make it one. It groups 20 recordings of unnamed senders at unmeasured levels.
   - **Recorded under R85 and carried on.** It is green either way (it does not lose).
3. **HM-REQ-127's margin is 0.05, the requirement's recommended value, held provisionally.** It is unchanged here.
4. **With 9.7 and 9.8 ticked, step 9's only open line is 9.4, which has no authorable route, and R86 holds steps 2 to 8 behind it.** All five mechanisms 9.3 named are recorded as refused, and so the next arbiter has no step R86 permits. This is logged for the owner, not a stop, because it touches neither transmit nor what the product promises the operator.
5. **Tree mismatches from section 3.**
   - `.run-unit\watched.rc` is absent. The runner deleted and then rewrote `watched.cpu`: task 0 committed it as it was, and the later rewrites are left unstaged.
   - The reload's `RULES_AT` disagreement (HM-DEC-165 against CPS-DEC-0183) is logged and is not this unit's.
   - `outcome-read`'s step titles for steps 2, 3 and 8 differ from `PHASE_PLAN.md`'s. Logged, not this unit's.
6. **The headless dispatcher loop took app tests twice:** 2 at entry and 1 before task 1's commit, each a 1 ms loss. DECIDED (6) was applied: one rerun each time, both 278 of 278, so no type needed running alone. The exit run was clean.
7. **Decode time moved between runs with no code change:** 65.50 s at entry against 64.27 s at 466's exit, then 64.97 s after task 1. The emitted text was byte-identical throughout, so this is run-to-run timing on this machine, not a change in the decoder.
