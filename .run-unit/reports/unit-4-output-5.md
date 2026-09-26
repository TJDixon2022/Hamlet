READ IN THIS ORDER.

A. Hamlet meets the CW requirements: step 2 at 3 of 5 (2.2 parked), step 3 at 1 of 6 by the
   plan's checkboxes, steps 0 and 1 done, 4 to 8 not started.
B. Step 3, HM-REQ-011 with HM-REQ-010 as the guard: 3.1 ticked; the change not kept, because
   MET-INVENTED rose from 47 to 48, sure-and-right coverage fell from 374 to 372 and V-11 failed on
   032050 and 032129. Added 13 at 1fb0bad6 -> 4 at entry -> 4 at exit; MET-INVENTED 47 over 473 ->
   47 over 473 (48 under the change); MET-CER-SURE 0.1116 -> 0.1116 (0.1143 under the change).
C. Section 4 raises 2 items; item 1 (the traffic-net recording is not in the tree) is in the way of 3.4.
   This report adds the first trace of the sure added letters. It shows that 441's two kept changes
   removed 9 of 439's 13, which four remain, and that the only cause more than one of them shares is
   the same marks read twice. It also reports a first rule against that cause, which failed R78.

UNIT:       443 - complete at task 3 of 3, none dropped - 2026-09-25 22:58
PHASE GOAL: The CW decoder meets CW_REQUIREMENTS.md, measured by requirement id, and at the end Tim at the radio says it read.
UNIT GOAL:  Trace every letter the decoder prints as sure where nothing was sent, at 439's baseline and at HEAD, then build one change against the biggest shared cause without making a sent letter wrong.
ADVANCED:   yes - 3.1 flipped in both copies of PHASE_PLAN.md; 3.2 and 3.3 did not, because the change was not kept.
NUMBER:     added 13 -> 4 (4 at entry, 4 at exit); MET-INVENTED 47 -> 47 over 473; MET-CER-SURE 0.1116 -> 0.1116
DRIFT:      1 consecutive unit on step 3 without a kept change (was 0)

## 1. What Claude did

**Exit state: complete, at task 3 of 3, none dropped.** This ran on QUIVERFULL in Claude Code. The
project was Hamlet, confirmed by the six-item gate. Branch `main`: every commit was pushed to
`origin/main`, and nothing is left unpushed.

Commits: `d8b9bd17` (task 0), `07f826d2` (task 1), `c3978a89` (task 2), and a closing commit with
this report.

**Task 0, the entry.** `PHASE_OUTCOME.md` gained `## UNIT 443 - STEP 3` in both copies, taken from
the decision block, plus an `ENTRY:` line. `PHASE_STATUS.md` now names 443 with `CURRENT_STEP: 3` in
both copies. The version went from 1.13.129 to 1.13.130. The runner's uncommitted `PHASE_OUTCOME.md`,
`PHASE_STATUS.md`, `RUN_LEDGER.md`, `PARKED.md` (root and docs copy) and `WORK_INSTRUCTIONS.md` were
committed as the runner wrote them. They went in one commit together with the entry, as "one commit
per task" asks. My only edits to those files were the 443 lines.

Entry round, one type per invocation:
- Build: 0 errors.
- Engine carry-forward: 178 of 178.
- App carry-forward: 278 of 278.
- Captures: 51 of 51.
- Adjudicated: 13 of 13.
- Named: 12 of 13. 17:37 is red: banked 46, reads 38.

Metrics, real recordings, inferred keys:
- MET-INVENTED: 47 over 473, 0.0994, of which 4 added and 43 substituted. All 47 are on the
  condition whose sender CW_SPEC.md does not state (20 recordings, inferred keys). TX-FARNS, TX-ITU
  and TX-TIGHT have 0 each (inferred keys).
- MET-CER-SURE: 47 of 421, 0.1116.
- Sure-and-right coverage: 374 over 473, 0.7907.
- MET-WBE: 52 over 113, 0.4602.
- Synthetic set, exact keys: MET-INVENTED 14 over 252, 6 added and 8 substituted.

**Task 1, the trace (3.1).** I added a new printer, `WhereTheSureAddedLettersComeFromTests.EverySureAddedCharacterAndWhatItShares`,
beside the sure-wrong one, which is unchanged. For every sure letter MET-INVENTED counts as added, it
prints:
- the recording and time;
- the key's text and the decoder's text either side;
- the letter and its pattern;
- its span and its neighbours' spans, and the gaps to them;
- the speed and the unit;
- the pitch at the hop and the sender's pitch;
- the envelope's marks and gaps in ms and in units;
- whether it sits beside an inserted or a missed word space;
- its energy against the noise.

**The decoder does not carry an energy-over-noise number at the hop.** `SignalToNoiseDb` is NaN on
the probabilistic path, and the noise scale stays internal to `LogLikelihoods`. So the printout shows
the span's ratio against silence as the decoder's own figure. Beside it is a level over noise that
the printer computes itself, using the decoder's quarter-point identity, and it is labelled as the
printer's.

I ran it three times. For the two older runs I checked out the decoder's `src` and later restored
it, as 442 did. `WhereTheSureWrongLettersComeFromTests` was taken from f14b2453 for those runs,
because the HEAD copy reads `MarkUnit`, which 441 added.

| decoder at | real added (inferred) | synthetic added (exact) |
|---|---|---|
| 1fb0bad6 (439's baseline) | 13 | 13 |
| 42d5dbb9 (G1 alone) | 5 | 6 |
| HEAD | 4 | 6 |

Matched on recording, text and time within 0.3 s:
- **G1 (42d5dbb9) removed 8**, all on 17:37: five around `CQ DE WB6RED` (22.54 to 23.74 s) and three
  at 26.25 to 26.82 s.
- **The marks' speed (14f515bd) removed 1**: `031838` at 21.355 s. 441's committed printouts agree:
  5 added under G1, 4 under the speed change.
- **The 4 that remain are old, and none are new:**
  - `032012` `.` at 7.425 s and `1` at 8.640 s. The key has `117. LINKS`; the decoder reads
    `117.1. LINKS`.
  - `032050` `T` at 11.195 s inside `FOUND`. It is a single dah with the recording's lowest span, 74.
  - `004347` `O` at 19.280 s, where `LOTW` is read as `L OOTW`.

**Grouping at HEAD.** The real 4 share no cause: every group is a single. The one cause shared by
more than one added letter, real or synthetic, is **the same marks read twice**. Each of these
letters has a span that begins 8.9 to 10.6 units inside the letter before it: `004347`'s `O`, and the
closing `K` printed `KK` on both 25 WPM synthetic cases. That is 3 of 10. `031838`'s letter at the
baseline overlapped its neighbour the same way. I added the overlap feature to the grouping after the
first HEAD run and before any change was built. It changes the grouping, not a rule.

**The 7.052 traffic net (3.4).** `EETTTEETTTTTTTTETTETETKTETEE` is in no fixture or test. It appears
only in the plan, the reports and the runner's prompt files. `GRAY KC` and `LIVER VIA` appear nowhere
in the tree. The text first appears in seed commit `9db61107` as "the 7.052 traffic net, 2026-09-25,
nine minutes, 664 characters". The tree's 7.052 recordings are twelve short ones from 2026-09-24
00:39 to 00:45 UTC. Only two of them, `004535` and `004550`, have a row in `cases-2026-09-23.txt`,
and none holds that text. **Not found, and not reconstructed.** The text at HEAD therefore cannot be
printed. Whether that recording has a key cannot be said. If it has none, MET-INVENTED does not count
its letters.

**Task 2, one change (3.2).** The real 4 share no cause, and 3.4's litter is not in the tree, so the
instruction's fallback was not available. **I aimed at the double read, the only shared cause the
trace shows.** `CwProbabilisticStream.Read` announces a re-read letter when its *end* is past the
settled mark, so a later window that cuts the same marks differently prints them again. The rule,
taken from the trace and not moved afterwards: a letter whose first mark begins before the last
settled letter's last mark ended is not announced. It is dropped, not dimmed.

| part of R78 | before | under the change | verdict |
|---|---|---|---|
| MET-INVENTED, real, inferred | 47 over 473 (4 added, 43 wrong) | 48 (3 added, 45 wrong) | **rises** |
| MET-CER-SURE, real, inferred | 47 of 421, 0.1116 | 48 of 420, 0.1143 | **rises** |
| sure-and-right coverage, real | 374 over 473 | 372 over 473 | **falls** |
| adjudicated readings | 13 of 13 | 13 of 13 | hold |
| V-11, 35 recordings | - | `032050`, `032129` each lose one right | **fails** |
| MET-WBE, real | 52 over 113 | 53 over 113 | reported |
| MET-INVENTED, synthetic, exact | 14 over 252 | 12 over 252 | falls |
| capture rows | 51 of 51 | 31 of 51 | reported |

**Not kept, because MET-INVENTED rises on the real set.** The diff is committed as
`.run-unit/unit443-added-notkept.diff`, and `src` carries none of it. It is recorded in
`docs/phase-requirements/metrics.md`, per condition with the key kind. I also added
`EveryRecordingAsTheOperatorReadsIt` to the same class so the text before and after could be printed.

**Task 3, the exit round.**
- Build: 0 errors.
- Engine carry-forward: 178 of 178.
- App carry-forward: 274 of 278. All 4 losses are `TheStopIsAlwaysOnScreenTests`, which passes 5 of
  5 alone. This is the recorded dispatcher-loop loss.
- Captures: 51 of 51.
- Adjudicated: 13 of 13.
- Named: 12 of 13. 17:37 is red at 38, as recorded.
- Metrics are the same as at entry.
- `WhereTheSureAddedLettersComeFromTests`: 2 of 2.
- `TheFiveToEightDecibelPlateauHolds` is in neither line and was not run.

**`src` at exit against entry `1308f688`: no file changed.** Nothing that keys or transmits was
touched. The one tree change outside the record is `tests/.../WhereTheSureAddedLettersComeFromTests.cs`,
a new printer. **3.1 is ticked** in both copies of `PHASE_PLAN.md`. Evidence:
`.run-unit/unit443-added-1fb0bad6.txt` and `unit443-added-head.txt` print every added letter with
every field (13 and 4 real, 13 and 6 synthetic), and `unit443-match.txt` matches the two lists and
groups them. **3.2 and 3.3 are not ticked**, because the change was not kept. **3.4 is not ticked**,
and neither is **3.6**, because 17:37 is red. DRIFT for step 3 is 1.

**Mismatches with the instruction, reported and not repaired:**
- **HM-REQ-011** reads: "On every condition at or above the sensitivity floor, the decoder shall keep
  MET-INVENTED at zero." It says every condition, not only the must-tier ones as §1 has it.
- **HM-REQ-010** reads: "On every must-tier condition at or above the sensitivity floor, the decoder
  shall keep MET-CER-SURE below 1 %." This matches.
- **HM-REQ-012** reads: "On every must-tier condition at the sensitivity floor, the decoder shall emit
  at least 90 % of sent characters as sure (MET-COVERAGE ≥ 0.90)."
- **HM-REQ-014** reads: "Over the corpus of each must-tier condition, characters the decoder emits as
  dim shall be correct at least 70 % of the time."
- **HM-REQ-015** reads: "The decoder shall make each character's confidence class available at the
  moment the character is emitted."
- None of the five says a letter that was not sent "may go to unknown or dim". That sentence is the
  instruction's reading, and it is consistent with them.
- The traffic-net recording is not in the tree (above).
- Checks that matched the instruction: all four commits are in HEAD's ancestry. `unit440-trace.txt`
  prints 54 wrong letters and no added ones.
- Known and not mine, reported once: `PHASE_OUTCOME.md`'s header still has the old titles for steps
  2, 3 and 8. `CW_SPEC.md` §11 still defines MET-COVERAGE as "Sure characters emitted / characters
  sent".

**Decisions I made myself:**
1. I traced the synthetic set's added letters as well as the real ones, because MET-INVENTED counts
   both.
2. I ran a third trace at 42d5dbb9, so the attribution to 441's two commits is measured rather than
   inferred.
3. I checked out the whole `src/Hamlet.RadioEngine/Cw` folder at the older commits, not a file list.
4. I chose the double-read group as the target, since the instruction's fallback (3.4's litter) was
   not in the tree.
5. The change dropped the re-read letter rather than dimming it. A dimmed duplicate still takes a
   place in the alignment, and the sure copy could still be the one scored as added.
6. I added a second printer fact to print every recording's text.

## 2. What the owner should expect

No: the operator sees the same letters as before. Nothing the decoder prints has changed, because the
one change built was not kept. What is new is knowledge. Of the 13 sure letters 439 found where
nothing was sent, G1 removed 8 and the marks' speed removed 1. The 4 left are three unrelated cases
and one double read. The evidence is inferred keys only (V-13): `032012`'s `117.1.` could be what was
actually sent. The rejected change showed that the double read is common and visible, well beyond
the 4 the metric counts. It fixed `VA3VRRR`, `NNOTT`, `QNIKK`, `W1AW/88` and `OOTW`, which is
exactly the kind of junk the operator reads. But the rule was too blunt: it also dropped letters that
shared a first mark with the letter before and carried new marks after it. **What will look wrong but
is not:** the app line reads 274 of 278 at exit against 278 at entry. Those four are the recorded
dispatcher-loop losses, and that type passes 5 of 5 alone. The named floor for 17:37 is red at 38,
as recorded, and was not re-banked.

## 3. What you should see

**The 7.052 traffic net from `GRAY KC` to `LIVER VIA` cannot be shown.** The recording is not in the
tree, so there is no entry or exit text to give. The tree records the plan's
`EETTTEETTTTTTTTETTETETKTETEE` only as text from seed commit `9db61107`, and no commit ever read it
from a recording.

**No recording's text changed at exit.** The one change was not kept. Here is what it did while
built: each line shows the text as the operator reads it now, then under the change. Added letters
the change removed are in brackets. Every other difference is a letter the change dropped that the
key may call sent.

| recording | now | under the change |
|---|---|---|
| `cq-25wpm-15db`, `cq-25wpm-5db` | `N0CALL KK` | `N0CALL K` [K] |
| `004347` | `ARRL L OOTW <BT> ANTHONY` | `ARRL L OTW <BT> ANTHONY` [O] |
| `013347` | `WVRR VA3VRRR` | `WVRR VA3VRR` |
| `003126` | `WHY NNOTT` | `WHY NOT` |
| `003758` | `TMP/4 QNIKK` | `TMP/4 QNIK` |
| `004405` | `W1AW/88 ■OORDINAT` | `W1AW/8 ■OORDINAT` |
| `004234` | `ON HYPP N AIR` | `ON HYP N AIR` |
| `004510` | `1U RLTT` | `1U RLT` |
| `013303` | `FOC EESSS` | `FOC ES` |
| `004027` | `HRRHCE` | `HRHCE` |
| `032113` | `200J6` ... `TT E EI  EEEEI` | `20J6` ... `T E EI  EEEE` |
| `032129` | `MTMTJ26` ... `IEEEE` | `MTMT6` ... `IEEE` |
| `032050` | `H IE S T S` | `H I S T S` |
| `031838` | `A 3, AT3 ,` | `A 3, AT ,` |
| `001952` | `MCON TA` | `MON TA` |
| `004427` | `TN6TBRE` | `TN6TRE` |
| `001831` | `TTTO` | `TTO` |
| `013010` | `T■L ES` | `TL ES` |
| `013637` | `REV■R` | `REVR` |
| `013622`, `012823`, `002016`, `021410`, `021629`, `021825` | one letter or placeholder fewer each | see `.run-unit/unit443-text-diff.txt` |

The full before and after for every changed recording is in `.run-unit/unit443-text-diff.txt`.

## 4. What's blocking us

1. **The nine-minute 7.052 traffic-net recording of 2026-09-25 should be added to
   `tests/fixtures/cw/captured/unadjudicated/` with its `cases-*.txt` row, and with a key if one can
   be inferred.** Reasoning: 3.4 asks for its text between `GRAY KC` and `LIVER VIA` before and after
   a kept change. The tree does not hold it, and the instruction forbids reconstructing it. Without
   a key, its litter is not counted by MET-INVENTED at all. Rejected: printing the nearest 7.052
   recordings instead, because they are twelve different, shorter recordings from a day earlier.
   Until it is added, 3.4 cannot be met by any unit.
2. **Listen to `unadjudicated/cw-2026-08-22-032012` from 7.4 to 8.7 s: was `117.` or `117.1.`
   sent?** Reasoning: two of the 4 added letters left at HEAD are the `.` and `1` there. Their
   patterns are clean, and their spans (1164 and 730) are among the strongest on the recording. If
   the sender sent them, the key is short, and MET-INVENTED counts 2 letters that are right (V-13).
   Rejected: editing the key in this unit, because a key is changed by ruling, not by a session.
