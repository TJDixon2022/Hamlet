READ IN THIS ORDER.

A. Phase goal: Hamlet meets the CW requirements. Steps by the plan -
   0 done, 1 done, 2 4 of 5, 3 3 of 6, 4 5 of 7, 5 1 of 6, 6 3 of 6,
   7 2 of 5, 8 0 of 6, 9 5 of 8; steps 2 to 8 still barred by R86,
   and 9.4 open with every mechanism 9.3 named refused.
B. Step 9, criterion 9.7 (HM-REQ-128): metrics compared 011
   MET-INVENTED, 010 MET-CER-SURE, 013 (15 dB rows), 012 MET-COVERAGE,
   014 dim accuracy (ours and arbitrated only), 081/080 MET-WBE, 083
   live against settled boundaries (live, ours and arbitrated only),
   084 named spans (3 of 6 measurable); the arbitration loses on 4 of
   12 rows - real TX-FARNS to the port on 081, synthetic TX-ITU 0 dB to
   the port on 012 and 081, char-gap-5 0 dB to the port on 012,
   char-gap-5 5 dB to the port on 011 and 010; the better decoder alone
   there is ours by order (at 011) on the first three and the port by
   order (at 011) on the fourth; switch table TX-FARNS: ours alone,
   TX-ITU 0 dB: ours alone, char-gap-5 0 dB: ours alone, char-gap-5
   5 dB: port alone, the other 8: arbitrate; live row real HF, all ->
   arbitrate; emitted output after the switch no worse than the better
   decoder alone on every row NO - yes on all 4 losing rows and on the
   live row, but the aggregate row synthetic, all becomes worse than
   ours alone on 012 (158 vs 159 of 252) and 081 (48 vs 44 of 84),
   because its char-gap-5 5 dB sub-row now emits the port; HM-REQ-128's
   test watched failing first - (a) red 4 of 5 against a Choose stubbed
   to answer arbitrate, (b) red on exactly the 4 losing rows with the
   table empty; (a) then green 5 of 5, (b) live green, (b) harness
   still red on synthetic, all; text changed on cq-18wpm-5db-char5
   only (harness), none live; decode time 64.71 s to 64.27 s; the port
   byte-identical yes; 9.7 open; 9.8 open.
C. The findings weighed against A and B: section 4 raises 9 items.
   Item 1 is in the way of 9.7. Condition rows are nested, and no
   per-row switch table can make both synthetic, all and its
   char-gap-5 5 dB sub-row pass once the order rule picks the port on
   the sub-row. Whether an aggregate row is a "condition" for HM-REQ-128
   is the owner's ruling. 9.8 follows 9.7 and is open with it, though
   all five exit checks are green. Item 3 is the standing reading:
   R86, with 9.4's refusals, holds steps 2 to 8. Nothing touches
   transmit, and nothing on the operator's screen changes.

UNIT:       466 - complete at task 4 of 4, none dropped - 2026-09-27 08:49
PHASE GOAL: Hamlet's CW decoding meets every requirement in CW_REQUIREMENTS.md at the condition each names, each proved by a test that names it. While no technique from fldigi is kept in ours, R86 routes every unit through step 9.
UNIT GOAL:  Score the arbitrated transcript, ours alone and the port alone side by side on every metric of sections B and I, per condition row, on the harness and live. Where the arbitration loses to either decoder alone, switch it off on that row and emit the better decoder alone, from a table fixed from that measurement. Prove it with a test naming HM-REQ-128, watched failing first.
ADVANCED:   no - 9.7 is left open. The switch is built and meets HM-REQ-128 on all 4 losing rows and on the live row. But HM-REQ-128's test stays red on the aggregate row synthetic, all, which the port-alone switch on one of its sub-rows makes worse than ours alone on coverage and MET-WBE. No per-row table meets both rows (section 4 item 1).
NUMBER:     HM-REQ-128 not met; arbitration loses on 4 of 12 rows; switch arbitrate 8, ours 3, port 1; real HF, all MET-CER-SURE arb/ours/port 33/436 / 33/436 / 64/221 (0.076 / 0.076 / 0.290, live, inferred keys); decode time 64.71 s to 64.27 s over 690 s
DRIFT:      step 2 1; step 3 0; step 4 1; step 5 1; step 6 0; step 7 0; step 9 1 (was 0)

## 1. What Claude did

**Complete at task 4 of 4, nothing dropped. 9.7 and 9.8 are left open.** Provenance: Claude Code on QUIVERFULL, project Hamlet, gate passed (`SHACK_FACTS.md`, `CwProbabilisticDecoder.cs`, `CW_REQUIREMENTS.md` and `CW_SPEC.md` present; `CoreHMI.sln` and `MURC.sln` absent), branch `main`, HEAD at entry `9cc2c74b`.

**The tree against section 3.** Everything matched, with two exceptions. `.run-unit\watched.rc` does not exist; the runner modified `.run-unit\watched.cpu` instead, and it was committed as it was. `SESSION.lock` is untracked; it is the launcher's (PID 51844) and was not staged. `.run-unit\fldigi\` was not staged and nothing was fetched. Both `PHASE_PLAN.md` copies are identical, with 9.1, 9.2, 9.3, 9.5 and 9.6 ticked and 9.4, 9.7 and 9.8 open.

**Task 0.** Wrote the record (`## UNIT 466 - STEP 9` in both `PHASE_OUTCOME.md` copies; both `PHASE_STATUS.md` copies at 466, step 9). Moved the version from 1.13.152 to 1.13.153. Committed the runner's writes as they were. Every entry figure equals 465's exit (section 3, item 7). Saved four texts, each with class and p: ours alone (1467 lines), the port alone (818), and the arbitrated transcript in the harness (1467) and live (1467). Both arbitrated saves are byte-identical to ours. Entry decode time on the live path was 64.71 s over 690 s of real audio (ours alone 51.83 s). Commit `0442ebfc`.

**Task 1.** Fixed the metric list and the loss, better-decoder and path rules in `TheArbitrationEarnsItsPlaceFact`'s header, and named where the switch sits. This was committed alone as `9eef6850`, before any three-way figure existed (V-14). The rules are copied into section 3, item 1.

**Task 2.** Built the three-way table (`TheArbitrationEarnsItsPlaceFact.TheThreeWayTable`, printout `.run-unit\unit466-threeway.txt`, and the new `docs/phase-requirements/arbitration.md`). The arbitration loses on 4 of 12 rows, all to the port. Ours' and the port's harness figures equal `parity.md`'s exactly. The port alone, rebuilt from its harvested readings, gives parity's symbols on all 35. Commit `a37a0d82`.

**Task 3.** Built the switch: `CwSwitchTable` beside `CwVoteTable`, the arbiter and the live decoder emitting under it, and the sheet marking it. Wrote `TheArbitrationEarnsItsPlaceTests`, naming HM-REQ-128, and watched both parts fail first. Part (b) stays red on `synthetic, all`. Commit `4a5d3f86`.

**Task 4.** Ran the exit round (section 3, item 7). Did not tick 9.7. Did not tick 9.8, which follows 9.7.

**Decisions I made for myself:**
- **Three read-only seams were added to `src` at task 2, not task 3.** The live port is rebuilt up to five times per recording when our pitch moves, so the last port alone cannot give its whole live reading. `CwDecoder.SecondRead` raises each reading as it is harvested. The harvester already read the port's word spaces and dropped them; it now marks them on the next reading (`CwSecondReading.WordGapBefore`). The arbiter reads neither. All four saves stayed byte-identical after this.
- **Task 1's rule went into `src` at task 2, as the pure function `CwSwitchTable.Choose`**, so the table, the fact and the test all read one copy. Watching part (a) fail first was done by stubbing `Choose` to answer arbitrate, then removing the stub.
- **Part (b) asserts two things per row.** First, the table's switch is the rule's switch on the path that decides the row. Second, the output emitted under it is no worse than the better decoder alone. The second assertion alone could not go red on the three ours-alone rows, because there the arbitrated output already is ours. So the first assertion is what made (b) red on every losing row.
- **Task 1's instrument choices.** 083 is measured as the share of distinct consecutive pairs the leading edge showed whose live boundary differs from the settled one. 084 is placed on the rows `real HF, all` and `sender not stated`, because its recordings name no sender. 013's verdict is met at no sent character missed and MET-CER-SURE zero, as its rationale reads it.
- **The table stays as the rule gives it, and part (b) stays red** on `synthetic, all` (section 4, item 1). The alternatives would change a rule after a figure was seen, which V-14 forbids: moving the char-gap-5 5 dB row off the port, or leaving the aggregate row out of the test. DECIDED (7) holds a row at arbitrate only when a floor test reddens, and none did.

## 2. What the owner should expect

**Nothing on the CW tab changes, on any kind of signal.** The product runs under real HF, all, and the switch there is `arbitrate`. On that row the second reader's vote does not earn a place yet, but it costs nothing either. The port is advisory everywhere under today's calibration, so the arbitrated text is exactly ours, character for character, class for class. Measured live, ours alone is better than the port alone on every metric.

On four kinds of signal Hamlet would now simply use one reader instead of arbitrating. Three of them use ours alone: the traffic-net sender (TX-FARNS), and the two 0 dB synthetic cases. There the text is the same as today's, because the port never voted. On one synthetic case (5 dB, 5-unit character gaps) Hamlet would use fldigi's reader alone. It asserts fewer wrong letters (3 of 16 sure against ours' 11 of 25), but it misplaces more word spaces and reads one fewer letter right. That swap makes the synthetic set as a whole slightly worse than ours on those two counts. This is why 9.7 is not ticked, and it needs your reading of what "every condition" means.

**What will look wrong but is not:**
- HM-REQ-128's test (harness part) is committed red on one row. That red is the finding.
- The capture sheet now adds `/switch ours-alone` or `/switch port-alone` after a character only where the arbitration is switched off. Under the live row it never appears.

## 3. What you should see

**The three-way table, harness, each recording under its own row.** In each metric cell the three figures are arbitrated · ours alone · port alone. `n.d.` means not defined: nothing to divide by. 014 is not defined on any row for any output, because neither ours nor the arbitrated output emits a dim character, and the port emits none by its mapping. 083 does not exist in the harness. Row names are shortened: "real <sender>" is `real HF, no CH-* profile, SNR_2500 not measured, sender <sender>`; "syn <gap>, <level>" is the synthetic row with that character gap and in-passband level.

| row | key | 011 MET-INVENTED | 010 MET-CER-SURE | 013 | 012 coverage | 081 MET-WBE | 084 | loses? | better alone | switch | emitted after the switch |
|---|---|---|---|---|---|---|---|---|---|---|---|
| **real HF, all** (set from the live path) | inferred | 33/473 · 33/473 · 62/473 | 33/436 · 33/436 · 62/239 | - | 403/473 · 403/473 · 177/473 | 37/113 · 37/113 · 86/113 | 0/3 · 0/3 · 0/3 | no | ours, dominance | arbitrate | as arbitrated |
| real TX-FARNS | inferred | 0/44 · 0/44 · 3/44 | 0/43 · 0/43 · 3/33 | - | 43/44 · 43/44 · 30/44 | 5/11 · 5/11 · **4/11** | - | **yes: 081 to the port** | ours, order at 011 | **ours alone** | ours; text unchanged |
| real TX-ITU (KD0UN) | inferred | 0/13 · 0/13 · 1/13 | 0/13 · 0/13 · 1/6 | - | 13/13 · 13/13 · 5/13 | 0/4 · 0/4 · 3/4 | - | no | ours, dominance | arbitrate | unchanged |
| real TX-TIGHT | inferred | 0/6 · 0/6 · 2/6 | 0/6 · 0/6 · 2/3 | - | 6/6 · 6/6 · 1/6 | 0/1 · 0/1 · 0/1 | - | no | ours, dominance | arbitrate | unchanged |
| real, sender not stated | inferred | 33/410 · 33/410 · 56/410 | 33/374 · 33/374 · 56/197 | - | 341/410 · 341/410 · 141/410 | 32/97 · 32/97 · 79/97 | 0/3 · 0/3 · 0/3 | no | ours, dominance | arbitrate | unchanged |
| **synthetic, all** | exact | 14/252 · 14/252 · 34/252 | 14/173 · 14/173 · 34/168 | - | 159/252 · 159/252 · 134/252 | 44/84 · 44/84 · 56/84 | - | no | ours, dominance | arbitrate (no recording runs under it) | **6/252, 6/164, 158/252, 48/84: worse than ours alone on 012 and 081** |
| syn TX-ITU, 0 dB | exact | 0/63 · 0/63 · 21/63 | n.d. · n.d. · 21/27 | - | 0/63 · 0/63 · **6/63** | 18/21 · 18/21 · **15/21** | - | **yes: 012, 081 to the port** | ours, order at 011 | **ours alone** | ours; text unchanged |
| syn TX-ITU, 15 dB | exact | 1/63 · 1/63 · 2/63 | 1/64 · 1/64 · 2/56 | 0/63 · 0/63 · 9/63 | 63/63 · 63/63 · 54/63 | 0/21 · 0/21 · 3/21 | - | no | ours, dominance | arbitrate | unchanged |
| syn TX-ITU, 5 dB | exact | 1/63 · 1/63 · 5/63 | 1/63 · 1/63 · 5/47 | - | 62/63 · 62/63 · 42/63 | 0/21 · 0/21 · 6/21 | - | no | ours, dominance | arbitrate | unchanged |
| syn gap 5, 0 dB | exact | 0/21 · 0/21 · 2/21 | n.d. · n.d. · 2/3 | - | 0/21 · 0/21 · **1/21** | 6/7 · 6/7 · 6/7 | - | **yes: 012 to the port** | ours, order at 011 | **ours alone** | ours; text unchanged |
| syn gap 5, 15 dB | exact | 1/21 · 1/21 · 1/21 | 1/21 · 1/21 · 1/19 | 1/21 · 1/21 · 3/21 | 20/21 · 20/21 · 18/21 | 12/7 · 12/7 · 14/7 | - | no | ours, dominance | arbitrate | unchanged |
| syn gap 5, 5 dB | exact | 11/21 · 11/21 · **3/21** | 11/25 · 11/25 · **3/16** | - | 14/21 · 14/21 · 13/21 | 8/7 · 8/7 · 12/7 | - | **yes: 011, 010 to the port** | **port, order at 011** | **port alone** | 3/21, 3/16, 13/21, 12/7: the port's |

**The live path**: `CwDecoder` with the second reader, as the product runs it, grouped by the same rows. The port alone here is its live reading, rebuilt at our pitch.

| row | key | 011 | 010 | 012 | 081 | 083 (arbitrated · ours) | 084 | loses? | switch on this path |
|---|---|---|---|---|---|---|---|---|---|
| **real HF, all** (decides this row) | inferred | 33/473 · 33/473 · 64/453 | 33/436 · 33/436 · 64/221 | 403/473 · 403/473 · 157/453 | 37/113 · 37/113 · 78/107 | 10/331 · 10/331 | 0/3 · 0/3 · 0/3 | no; ours by dominance | **arbitrate**; emitted byte-identical to task 0's save |
| real TX-FARNS | inferred | 0/44 · 0/44 · 5/44 | 0/43 · 0/43 · 5/26 | 43/44 · 43/44 · 21/44 | 5/11 · 5/11 · 7/11 | 0/22 · 0/22 | - | no | arbitrate |
| real TX-ITU | inferred | 0/13 · 0/13 · 1/13 | 0/13 · 0/13 · 1/5 | 13/13 · 13/13 · 4/13 | 0/4 · 0/4 · 3/4 | 0/2 · 0/2 | - | no | arbitrate |
| real TX-TIGHT | inferred | 0/6 · 0/6 · 2/6 | 0/6 · 0/6 · 2/4 | 6/6 · 6/6 · 2/6 | 0/1 · 0/1 · 0/1 | 1/31 · 1/31 | - | no | arbitrate |
| real, sender not stated | inferred | 33/410 · 33/410 · 56/390 | 33/374 · 33/374 · 56/186 | 341/410 · 341/410 · 130/390 | 32/97 · 32/97 · 68/91 | 9/276 · 9/276 | 0/3 · 0/3 · 0/3 | no | arbitrate |
| synthetic, all | exact | 14/252 · 14/252 · 29/252 | 14/173 · 14/173 · 29/154 | 159/252 · 159/252 · 125/252 | 44/84 · 44/84 · 52/84 | 1/40 · 1/40 | - | no | arbitrate |
| syn TX-ITU, 0 dB | exact | 0/63 · 0/63 · 13/63 | n.d. · n.d. · 13/18 | 0/63 · 0/63 · 5/63 | 18/21 · 18/21 · 17/21 | n.d. | - | 012, 081 to the port | ours alone |
| syn TX-ITU, 15 dB | exact | 1/63 · 1/63 · 2/63 | 1/64 · 1/64 · 2/56 | 63/63 · 63/63 · 54/63 | 0/21 · 0/21 · 3/21 | 0/15 · 0/15 | - | no | arbitrate |
| syn TX-ITU, 5 dB | exact | 1/63 · 1/63 · 5/63 | 1/63 · 1/63 · 5/47 | 62/63 · 62/63 · 42/63 | 0/21 · 0/21 · 6/21 | 0/13 · 0/13 | - | no | arbitrate |
| syn gap 5, 0 dB | exact | 0/21 · 0/21 · 4/21 | n.d. · n.d. · 4/5 | 0/21 · 0/21 · 1/21 | 6/7 · 6/7 · 7/7 | n.d. | - | 012 to the port | ours alone |
| syn gap 5, 15 dB | exact | 1/21 · 1/21 · 3/21 | 1/21 · 1/21 · 3/13 | 20/21 · 20/21 · 10/21 | 12/7 · 12/7 · 8/7 | 0/4 · 0/4 | - | 081 to the port | ours alone (the harness sets this row: arbitrate) |
| syn gap 5, 5 dB | exact | 11/21 · 11/21 · 2/21 | 11/25 · 11/25 · 2/15 | 14/21 · 14/21 · 13/21 | 8/7 · 8/7 · 11/7 | 1/8 · 1/8 | - | 011, 010 to the port | port alone |

On the live path the port's "sent" can fall below the key's: 453 of 473 on real HF, all. Each stretch is located in the port's own text by the scorer's unchanged rule, and some stretches do not fit.

**1. The metric list and the three rules, as task 1 fixed them (`9eef6850`).**
- **Metrics:**
  - 011 MET-INVENTED, 010 MET-CER-SURE, 012 MET-COVERAGE and 081/080 MET-WBE apply to all three outputs.
  - 013, sent characters not emitted sure and correct over sent, applies only on the 15 dB rows.
  - 014, dim right over dim emitted, applies to ours and the arbitrated output only; the port emits no dim.
  - 083 applies live only, and to ours and the arbitrated output only; the port has one rendering.
  - 084 is the named spans read exactly, over the 3 of 6 in the tree.
  - A rate is compared as a rate. A rate whose denominator is zero is not defined.
- **Properties:** 015 holds for all three: each character carries its class when it is emitted (`EveryCharacterCarriesAConfidenceTests` 2 of 2; the harvester's reading; `CwArbiter.Decide`). 082 holds for all three: `CwMetrics.WordBoundaries` scores boundaries apart from characters, through one scorer.
- **Loss rule:** the arbitration is strictly worse than either decoder alone on any metric defined for both.
- **Better-decoder rule:** the decoder no worse on every metric defined for both. If neither is, the first difference in the order 011, 010, 013, 012, 014, 081, 083, 084 decides. Ours wins a full tie.
- **Path rule:** real HF, all is set from the live path; every other row from the harness.

**2. Where the switch sits.**
- **The table:** `src/Hamlet.RadioEngine/Cw/CwSwitchTable.cs` holds `Rows`, transcribed from `arbitration.md` section 2 at `a37a0d82`, with `For`, `Live`, `Choose`, `Compare` and `Order`. `CwVoteTable` is untouched.
- **The arbiter (`CwArbiter`):**
  - `Arbitrate(ours, second, vote, switch)`, `Decide(..., switch)` and `DecideAlone(..., switch)` take the switch; `Alone` and `GapBefore` emit the port alone.
  - The record carries the switch in `CwArbitration.Switch`.
- **The live decoder (`CwDecoder`):**
  - `Switch` defaults to `CwSwitchTable.Live`.
  - It is read in `Arbitrated`, `ArbitratedEdge`, `OneSided` and `Flush`, through the same `Settle` and `CharacterSettled` seam the tab reads.
- **The sheet:** `MainWindowViewModel.ArbitrationLine` adds `/switch ours-alone` or `/switch port-alone`, and nothing when the switch is arbitrate.
- **The seams added in task 2:** `CwSecondReading.WordGapBefore` (set by `CwSecondHarvester.Take`) and `CwDecoder.SecondRead`.
- `git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/` prints nothing.

**3. How each part of `TheArbitrationEarnsItsPlaceTests` was watched failing first.**
- **(a)** Five hand-built cases: beats both, loses to ours, loses to the port, a split decided for ours at 011, and a split decided for the port at 011. The injected stream is ours `AB` and the port `AC` plus `E` after a space, with only the port voting. So arbitrating, ours alone and the port alone emit `ACE`, `AB` and `AC E`. Against `Choose` stubbed to always answer arbitrate: **red 4 of 5**. The beats-both case passed, correctly (`.run-unit/unit466-a-red.txt`). With the rule: 5 of 5, and again at exit.
- **(b) harness.** With `CwSwitchTable.Rows` empty: **red on exactly the four losing rows**, each "the table says Arbitrate, the rule says OursAlone / PortAlone". The char-gap-5 5 dB row was also red as "emitted worse than the better decoder alone (the port) on 011, 010" (`unit466-b-harness-red.txt`). With the table: green on all four losing rows and on every other sub-row, but **red on `synthetic, all`: emitted worse than ours alone on 012, 081**. It is still red at exit (`unit466-b-harness-exit.txt`).
- **(b) live** (real HF, all): green. It was not watched red, because the rule and the table agree on arbitrate there, and task 2 found no loss on that row.
- `ThePortAloneOnTheLivePathEmitsThePortsOwnReading`: under the port alone, `CwDecoder` emits exactly the port's live reading, `E Q D N 0 C T L L 0C A L L K`. This is a check of the live path, not watched red.

**4. HM-REQ-084's six spans.** `WEEKEND`, `THINKING` and `USED TO USE A FIRM` are not measurable in the tree (`WhatTheNamedWordsReadTests.NotHere`). None of the three measurable spans is read exactly by any output on either path:

| span | arbitrated (harness and live) | ours alone | port alone, harness | port alone, live |
|---|---|---|---|---|
| ABOVE (013637) | ` AB OVE ` | ` AB OVE ` | ` JE` | `E` |
| BREEZE (013637) | ` BR EEZE ` | ` BR EEZE ` | `BREEZ` | `BREEZ` |
| FLEX (021410) | ` FLENT ` | ` FLENT ` | `ZA ` | `L ` |

HM-REQ-013 and 080 on the 15 dB rows, both paths:
- On syn TX-ITU 15 dB, the arbitrated output and ours both fail 013 (1 sure added of 64, every sent character right) and meet 080. The port fails both.
- On syn gap 5, 15 dB, all three fail both.

HM-REQ-014: no dim character is emitted by ours or the arbitrated output on any row, and it is not defined for the port.

**5. The one row whose text changed: cq-18wpm-5db-char5**, in the harness only (the port alone).
- R78's four metrics, before → after:

  | metric | before | after |
  |---|---|---|
  | MET-CER-SURE | 11/25 (0.440) | 3/16 (0.188) |
  | MET-INVENTED | 11/21 | 3/21 |
  | coverage | 14/21 | 13/21 |
  | MET-WBE | 8/7 | 12/7 |

- Text before: ` C Q C Q C Q TEE T ■KTDUUEUEN 0CAL L K`. After: `E Q D N 0 C T L L 0C A L L K E`.
- The other 34 recordings are byte-identical to task 0's arbitrated save. The live transcript is byte-identical to task 0's live save.
- **V-11:** ours' per-recording metrics did not move on any of 35. Floors are 51 of 51, 13 of 13 and 13 of 13. No floor runs through the harness switch.

**6. Decode time** on the live path over 690 s of real audio, ours plus the port plus the arbiter:
- entry 64.71 s (ours alone 51.83 s);
- after task 3, 64.19 s;
- exit 64.27 s (ours alone 51.99 s).

**7. Exit round beside entry, and the commit table.**

| check | entry | exit |
|---|---|---|
| build | 0 errors | 0 errors |
| engine line | 178 of 178 | 178 of 178 |
| app line | 278 of 278 | 277 of 278, then 273 of 278 on the one rerun; every loss 1 ms with "dispatcher loop". The types lost on the rerun pass alone: `BindingHealthTests` 1 of 1, `TheRstIsYoursToCorrectTests` 4 of 4, `TheFavoritesAreChipsTests` 4 of 4 (DECIDED (9)) |
| captures | 51 of 51 | 51 of 51 |
| adjudicated | 13 of 13 | 13 of 13 |
| named | 13 of 13 | 13 of 13 |
| real, inferred | 33 of 436, 33/473, 403/473, 37/113 | same |
| synthetic, exact | 14 of 173, 14/252, 159/252, 44/84 | same |
| the port (`parity.md`) | real 62 of 239, coverage 177; synthetic 34 of 168, coverage 134 | same; `parity.md` restored after each run |
| `BothDecodersAreScoredAlikeTests` | 5 of 5 | 5 of 5 |
| `EveryCharacterCarriesAConfidenceTests` | 2 of 2 | 2 of 2 |
| the port's own tests | 8 of 8 | 8 of 8 |
| 465's three | 8 of 8, 3 of 3, 3 of 3 | 8 of 8, 3 of 3, 3 of 3 |
| `TheArbitrationEarnsItsPlaceTests` | - | (a) 5 of 5; (b) live 1 of 1; port-alone check 1 of 1; **(b) harness red on synthetic, all** |
| `TheSpeedFollowsTheSendersMarkPairsTests` | red, 28 of 31 | red, 28 of 31 (not required) |

Other exit prints:
- `git diff 19109b51 -- Cw/Second/`: nothing.
- `git diff 7e209cb4` over the eleven transmit files: nothing.
- `git status`: `.run-unit/fldigi/` and `SESSION.lock` untracked.
- Emitted transcript against task 0's arbitrated saves: harness identical except cq-18wpm-5db-char5 (row syn gap 5, 5 dB; switch port alone); live identical.

Every commit this unit made, with its five exit checks:

| commit | build | engine line | app line | captures | adjudicated | named |
|---|---|---|---|---|---|---|
| `0442ebfc` task 0 | 0 errors | 178/178 | 278/278 | 51/51 | 13/13 | 13/13 |
| `9eef6850` task 1 | 0 errors | 178/178 | 278/278 | 51/51 | 13/13 | 13/13 |
| `a37a0d82` task 2 | 0 errors | 178/178 | 278/278 | 51/51 | 13/13 | 13/13 |
| `4a5d3f86` task 3 | 0 errors | 178/178 | 278/278 on the one rerun (275 first, dispatcher loop) | 51/51 | 13/13 | 13/13 |
| task 4 (this report) | 0 errors | 178/178 | green by DECIDED (9), as above | 51/51 | 13/13 | 13/13 |

## 4. What's blocking us

1. **Is an aggregate row a "condition" for HM-REQ-128?** This blocks 9.7.
   - **What the tree forces.** Condition rows are nested: `synthetic, all` is the union of its six sub-rows (parity.md section 3, 458 DECIDED (4)). The rule fixed at task 1 picks the port alone on the char-gap-5 5 dB sub-row, because 011 comes first. That makes `synthetic, all` worse than ours alone on 012 and 081 while better on 011 and 010.
   - **No per-row table meets both rows.** Holding the sub-row at ours fails the sub-row (worse than the port on 011 and 010).
   - **Reading A.** HM-REQ-128 is judged on the rows a recording is emitted under: each harness sub-row, and real HF, all live. Then 9.7 is met as built, and part (b) drops its assertion on `synthetic, all` and `real HF, all` in the harness.
   - **Reading B.** Every tabled row, the aggregates included, is a condition. Then the order rule, or a no-port-alone-where-a-parent-row-loses rule, has to change.
   - **Why the owner decides.** Changing either now, after the figure is seen, is what V-14 forbids a session to do.
   - It touches neither transmit nor the screen: the live product arbitrates.
2. **HM-REQ-127's margin is 0.05, the requirement's recommended value, held provisionally.** Unchanged here.
3. **9.4 has no authorable route.** Its words and the five mechanisms 9.3 named are all recorded as refused, and R86 holds steps 2 to 8 behind 9.4. This is logged for the owner, not a stop. It touches neither transmit nor what the product promises the operator.
4. **The port alone is picked on one row.** On synthetic, 5 dB, 5-unit character gaps, the switch table picks the port alone. That row's operator would read fldigi's text at its all-sure mapping: `E Q D N 0 C T L L 0C A L L K E`, every letter sure. That is 3 of 16 sure letters wrong (ours: 11 of 25), 13 of 21 sent letters right (ours: 14), and 12 word-space errors in 7 words (ours: 8). DECIDED (6) licenses this. The live product never runs under that row.
5. **Three of the four switches change nothing today.** The port votes nowhere, so the arbitrated output is ours on every row. "Ours alone" on TX-FARNS, TX-ITU 0 dB and char-gap-5 0 dB emits the same text as arbitrating. These switches start to matter only once a recalibration lets the port vote there.
6. **The live path and the harness disagree on one row.** Live, syn gap 5 at 15 dB loses to the port on 081 (8/7 against ours' 12/7). In the harness it does not. By the path rule the harness sets that row (arbitrate). This matters only if a synthetic row is ever run live.
7. **Unpaired characters are not on the live sheet under the port alone.** Live, a character only ours read is not emitted, so it never reaches the sheet. The harness records it (emitted: none). No live row runs under the port alone.
8. **The headless dispatcher loop took app tests at two runs**: task 3's first run lost 3, and the exit round lost 1 and then 5. DECIDED (9) was applied both times, and every type lost on a rerun passed alone.
9. **Tree mismatches from section 3.** `.run-unit\watched.rc` is absent (the runner modified `watched.cpu`). The reload's `RULES_AT` disagreement and `outcome-read`'s step titles are logged and are not this unit's.
