READ IN THIS ORDER.

A. Phase goal: Hamlet meets the CW requirements. Steps by the plan -
   0 done, 1 done, 2 done (closed partial), 3 4 of 6, 4 5 of 7,
   5 1 of 6, 6 3 of 6, 7 4 of 5, 8 0 of 6, 9 7 of 8; 9.4 ended as a
   loop, and this unit worked step 3 against R86's words, reported as
   a mismatch.
B. Step 3 (HM-REQ-011): the invented letters placed against the sent
   text - largest unattacked group split, 0 real, 6 synthetic;
   the change kept; real MET-INVENTED 33/473 -> 25/473, synthetic
   14/252 -> 7/252; 3.6 ticked, every commit green on the five; 3.5's
   count 0 of 3; 3.4 recording absent.
C. The findings weighed against A and B: section 4 raises 9 items.
   None is in the way of 3.6, which is ticked, or of 3.5, whose count
   the kept change holds at 0. Item 2 is in the way of 3.4: no unit can
   meet it until the 7.052 recording and its cases row are in the tree.
   The R86 mismatch in one line: R86 bars steps 2 to 8 until 9.4 has a
   kept change, 9.4 has no route left, and this unit worked step 3
   anyway as the launcher named it.

UNIT:       472 - complete at task 4 of 4, decode-time line dropped - 2026-09-27 20:10
PHASE GOAL: Hamlet's CW decoder meets every requirement in CW_REQUIREMENTS.md at the conditions CW_SPEC.md names, each proved by a test that names it.
UNIT GOAL:  Find where each letter Hamlet prints with confidence but was never sent sits against what was sent. Remove the largest kind no earlier attempt touched, and only if R78 holds. Keep every commit green on the floors and both carry-forward lines, which is what 3.6 asks.
ADVANCED:   yes - step 3 criterion 6, ticked: every commit of the unit (600c6da4, 62bb097f, 45e4c50a and the task 4 commit) exits green on build, both carry-forward lines and the three floor tests, the app line's dispatcher losses handled under DECIDED (7)
NUMBER:     MET-INVENTED real 33/473 -> 25/473, synthetic 14/252 -> 7/252; coverage real 403 -> 403; change kept
DRIFT:      step 3 0; step 4 1; step 5 1; step 6 0; step 7 0; step 9 0 (carried from 471's report; step 3 reset on this unit's advance)

## 1. What Claude did

**Complete at task 4 of 4.** I dropped one line, task 2's decode time. It is the instruction's
first drop candidate. Measuring it needs a parity run before and after, and the before needs HEAD's
`src` restored and rebuilt. Nothing that is never shed was dropped. Provenance: Claude Code on
QUIVERFULL, project Hamlet, branch `main`. The gate passed: the four MUST EXIST are present and
`CoreHMI.sln` and `MURC.sln` are absent. HEAD at entry was `80df7cd5`, and the version went from
1.13.158 to 1.13.159.

**The tree against section 3.** It matched except as follows:
- **Plans:** both `PHASE_PLAN.md` copies are identical. Every line of step 2 is ticked; 3.1 to 3.3
  are ticked and 3.4 to 3.6 open; 9.4 is step 9's only open line; R86 is at line 342.
- **Patches and the port:** the 470 and 471 patches exist and were not applied. The port diff
  against `19109b51` is empty.
- **7.052 traffic net: absent.** `GRAY KC` and `LIVER VIA` appear only as quotations, in both plans,
  the instruction, `.run-unit/arbiter-prompt.txt`, `.run-unit/reload.txt` and an earlier report. No
  audio file is dated 2026-09-25 or later, and no tracked file from those dates holds a recording.
- **Mismatch:** `.run-unit\watched.rc` did not exist. `watched.cpu` was the runner's deleted file at
  launch; the runner rewrote it during the session, and it was committed as it stood at each commit.
- **Mismatch, arising during the unit:** an empty `STOP` file appeared at the repository root at
  19:30, after the task 2 commit. `tools/arbiter/run-phase.bat` reads it as the owner's brake at
  the top of its next iteration: the running unit finishes and is judged, and then the run ends.
  I finished the unit and began nothing after it. The file was not staged, moved or deleted.
- **Not staged:** `SESSION.lock`, `.run-unit\fldigi\` and `STOP`. Nothing was fetched.

**Task 0.** I wrote the record in both `PHASE_OUTCOME.md` copies, set both `PHASE_STATUS.md` copies
to 472 and step 3, bumped the version, and committed the runner's writes as they were. Every entry
figure matched 471's exit. The app line lost 2 to the dispatcher loop, then passed 278 of 278 on the
one rerun. The texts with classes are byte-identical to `unit471-texts-entry.txt` (`600c6da4`).

**Task 1, the trace.** I read `CwMetrics` first. Its alignment is `CwScorer`'s own Levenshtein on
symbols, with spaces taken out of both sides. A sure letter with no key is added, and a sure letter
whose key differs is substituted.

`WhereTheInventedLettersSitFact` drives the same harness and the same `Covered`, `Symbols` and
`Align` calls as `TheRequirementsAreMeasuredTests`. Its totals cross-check exactly: real 33 invented
(5 added, 28 substituted), synthetic 14 (6 added, 8 substituted).

It places each letter two ways:
- **Alignment reading, both sets:**
  - a split is a run of steps that spells one key letter;
  - a merge is a substituted letter spelling its key letter and a deleted neighbour's;
  - an added letter sits at a word boundary or the stretch's end, or else in a character gap.
- **Timing reading, synthetic set only:** the sent marks are rebuilt from `CwFixtureGenerator.KeyEdges`.
  Each letter's span is shifted by the right letters' median lag (start 17-20 ms, end 8-12 ms), and
  the letter is placed by the sent characters it covers.

The table uses the timing reading where the key is exact. The two readings disagree on 12 of the
synthetic 14 (section 4, item 3).

The largest group no earlier route acted on is **split: 6 synthetic, 0 real**, all on
`cq-18wpm-5db-char5`:
- **What they are:** a dit lost in the noise between two element gaps leaves 3.2 to 3.6 units of
  key-up. This sender's character gap is 5 units, and the stream holds 357 ms. So `D` (-..) printed
  sure as `T` then `E`.
- **No single column separates them.** Every measured quantity puts them inside the right letters'
  range.
- **The ratio does.** The quantity is the shorter key-up the path read as a character gap, over the
  character gap in force. Five of the six sit at 0.588 to 0.60. The lowest sure right letter on
  either set is at 0.632 (`032113` `R` and `S`), and 2 of 403 real and 0 of 159 synthetic right
  letters lie at or under the group's top.

I registered the rule in `.run-unit/unit472-trace.txt` before any after-figure. I committed it with
the fact, green on the five (`62bb097f`).

**Task 2, the change.** `TheInventedLettersAreNotPrintedSureTests` (HM-REQ-011) was watched red at
HEAD: `T` and `E` sure over the sent `D` at 8.200 to 8.667 s (`.run-unit/unit472-red.txt`). I built
the rule as registered, in one form. `CwProbabilisticStream.Character` now emits `Low` where
`NearGapFitsTheCharacterGap` finds that key-up under `NearGapShare` = 0.62 of the held character gap,
or of three units where none is held. It acts the same on settled letters and the leading edge. The
test then passed.

Every line of 3.2 held. It was committed with its test and `metrics.md`'s Unit 472 section as
`45e4c50a`, with task 3's line folded in: step 3's count 0 of 3.

**Task 4, the exit round.** Every figure is as under the change, and the texts are byte-identical
to the change's. The app line lost 1 type to the dispatcher loop and then 3 others on the one rerun.
None was lost twice. All four were run alone and passed under DECIDED (7):
- `TheWindowHoldsBelowItsMinimumTests` 3/3;
- `Unit376TheTopBandTests` 5/5;
- `TheFavoritesAreUnderTheGreenZoneTests` 3/3;
- `ThePsk31ConversationCardTests` 8/8.

`TheSpeedFollowsTheSendersMarkPairsTests` is red, as expected, now at 26 of 29 sure wrong or added
against 471's 28 of 31. Sure and right is unchanged at 3 of 21. I ticked 3.6 in both plans and
nothing else.

Decisions I made for myself:
- **Which neighbours the rule sees.** In the stream it reads the letters in the same read, where the
  trace read the settled letters. The rule was registered naming that risk, and R78 measured it.
- **The four lost app types.** I ran all four alone, not only a type lost twice, since none was.

## 2. What the owner should expect

**What changes.** When a weak station's dit drops into the noise in the middle of a letter, Hamlet
no longer prints the two halves as two certain letters. The pieces now come up dim. The screen shows
them as a guess, and they no longer count as invented. On the bulletin
`032050`, `IULLETIN CAN BE FOTAND IN TELEWRITTER` now reads `IULLETIN CAN BE FO[T][A]ND IN
TELEWRITTER` (brackets are dim): the U of FOUND lost a dit and split. On `004234`, `T HANTT TK■ET
FOR` now reads `T HAN[T][T] TK■[E][T] FOR`: the K of THANK lost its middle dit. On the synthetic
five-unit sender at 5 dB, `C Q TEE T ■KTDUUEUEN` now reads `C Q [T][E]E T ■[K]TDU[U][E][U][E]N`.

**What does not change.** No letter changes, none is added or removed, and no right letter in the
keyed stretches went dim. Coverage is exactly where it was.

**What will look wrong but is not.** 23 letters outside the keyed stretches also went dim:
- 9 in the `013347` opening noise run (`E EI I` -> `E [E][I] I`);
- `NT` on `134712`;
- the `E5` at the head of `004507`;
- `TM` at the head of `031948`;
- 8 more on keyed recordings, outside their scored stretches (`003758` 1, `031838` 2,
  `032129` 3, `004234` 2).

No key says whether those were right. They sit beside the same short key-ups.

## 3. What you should see

**The commits, with the five at each exit** (3.6 is judged on this):

| commit | what | build | engine line | app line | named floors | captures | adjudicated |
|---|---|---|---|---|---|---|---|
| `600c6da4` | task 0, record and entry round | 0 errors | 178/178 | 278/278 on the one rerun (2 lost to the dispatcher loop first) | 13/13 | 51/51 | 13/13 |
| `62bb097f` | task 1, fact and trace, rule registered | 0 errors | 178/178 | 278/278 on the one rerun (3 lost first) | 13/13 | 51/51 | 13/13 |
| `45e4c50a` | task 2, the kept change, its test, `metrics.md` | 0 errors | 178/178 | 278/278 first run | 13/13 | 51/51 | 13/13 |
| task 4 commit | exit printouts, `output.md`, the 3.6 tick; nothing under `src` or `tests` | takes `45e4c50a`'s; re-measured: 0 errors | 178/178 | 277/278, then 275/278 on the rerun; the 4 lost types each pass alone (DECIDED (7)) | 13/13 | 51/51 | 13/13 |

**1. The R78 table**, before (HEAD, `src` as at `80df7cd5`) -> after (`45e4c50a`):

| part of R78 | before | after | verdict |
|---|---|---|---|
| MET-INVENTED, real, inferred | 33 over 473 (5 added, 28 wrong) | 25 over 473 (4 added, 21 wrong) | falls |
| MET-INVENTED, synthetic, exact | 14 over 252 (6 added, 8 wrong) | 7 over 252 (4 added, 3 wrong) | falls |
| MET-CER-SURE, real, inferred | 33 of 436, 0.0757 | 25 of 428, 0.0584 | falls |
| MET-CER-SURE, synthetic, exact | 14 of 173, 0.0809 | 7 of 166, 0.0422 | falls |
| coverage, real, inferred | 403 over 473, 0.8520 | 403 over 473 | holds |
| coverage, synthetic, exact | 159 over 252, 0.6310 | 159 over 252 | holds |
| MET-WBE, real / synthetic | 37 over 113 / 44 over 84 | the same | unchanged |
| dim precision (HM-REQ-014), scored stretches | no dim letter on either set | real 0 right of 8 dim; synthetic 0 right of 7 dim | reported |
| adjudicated readings | 13 of 13 | 13 of 13 | hold |
| V-11, 35 recordings | - | 0 worse; 5 rows fewer wrong or added, sure-and-right held on every row | holds |
| floors, both carry-forward lines, `TheArbitrationEarnsItsPlaceTests` (5, 1, 2) | green | green | hold |

Per condition, before -> after, key's kind beside each:
- **Real, sender not stated (inferred):**
  - MET-INVENTED 33 over 410 (5 added, 28 wrong) -> 25 over 410 (4 added, 21 wrong);
  - MET-CER-SURE 33 of 374 -> 25 of 366;
  - coverage 0.8317 -> 0.8317;
  - MET-WBE 32 of 97 -> 32 of 97.
- **Real, TX-FARNS, TX-ITU and TX-TIGHT (inferred):** 0 invented before and after; every figure
  unchanged.
- **Synthetic, character gap 5 at 5 dB (exact):**
  - MET-INVENTED 11 over 21 (4 added, 7 wrong) -> 4 over 21 (2 added, 2 wrong);
  - MET-CER-SURE 11 of 25 -> 4 of 18;
  - coverage 0.6667 -> 0.6667;
  - MET-WBE 8 of 7 -> 8 of 7.
- **Every other synthetic condition (exact):** unchanged. TX-ITU 5 dB and 15 dB 1 added each;
  character gap 5 at 15 dB 1 wrong; both 0 dB no sure letter.

The full tables are in `docs/phase-requirements/metrics.md` under `## Unit 472`.

**2. Task 1's position table** (timing reading on the exact set, alignment reading on the inferred;
"no route" = no earlier route acted on it):

| position | real invented (no route) | synthetic invented (no route) | earlier routes beside it |
|---|---|---|---|
| word gap | 0 | 0 | - |
| character gap | 5 (0) | 0 | all 5 acquiring (470, 471); 1 overlaps a neighbour (443) |
| split | 0 | 11 (6) | 2 are the 25 WPM `KK` double read (443); 3 under 1 nat (442), 1 of them acquiring; fldigi (E), (W) refused |
| merge | 0 | 0 | 445's inner-gap edge (kept); fldigi (E) |
| stand-in | 28 (4) | 3 (2) | 24 real acquiring (470, 471), 4 of them under 1 nat (442), 2 overlap (443); fldigi (A), (B), (C), (D) refused |

**3. The right letters at risk** for the split group. On the quantity, the group lies at 0.588 to
0.658. Right letters at or under 0.658: real 2 of 403 (`032113` 11.285 s `R` and 11.760 s `S`, both
0.632); synthetic 0 of 159. No right letter lies under 0.632 on either set. Under the change, no
scored right letter was dimmed: coverage held on every condition.

**4. The rule as registered** (`62bb097f`, `.run-unit/unit472-trace.txt` part 5):
> In `CwProbabilisticStream.Character`, a known letter that `GapsFitTheUnit` would emit High is
> emitted Low where the shorter key-up between it and the named letter before or after it in the
> same read is shorter than 0.62 of the character gap in force. The key-up is taken span to span
> from the path's own characters, and only on a side where the read places no word space that the
> relabel keeps. The character gap in force is the stream's held length where structure is held,
> otherwise three units at the read's speed. The edge is not from `CW_SPEC.md`'s timing, which gives
> no line between one and five units that parts a lost dit from a five-unit character gap. It is a
> line no sure right letter crosses on either set: the lowest is at 0.632, and five of the group's
> six are under it at 0.588 to 0.60. The sixth, at 0.658, is not reached and the edge is not moved
> to reach it.

**5. Every recording whose text or classes changed** (dim in brackets; entry -> exit). No letter
changed, only classes. The port's texts are byte-identical.
- `cq-18wpm-5db-char5`: `C QC Q C Q TEE T ■KTDUUEUEN 0CAL L K` -> `C QC Q C Q [T][E]E T ■[K]TDU[U][E][U][E]N 0CAL L K`
- `013347`: `E EI I HIAEIHEEEA E EEE HEEIEE ...` -> `E [E][I] I HIAEIH[E][E][E]A E E[E][E] [H][E]EIEE ...` (all outside the keyed `VA3VRR`, which is unchanged)
- `134712`: `E ■ NT N4LQ K` -> `E ■ [N][T] N4LQ K`
- `004507`: `E5 I E A T ARRL ...` -> `[E][5] I E A T ARRL ...`
- `003758`: `... A ET EEEETMP/4 QNIKK ...` -> `... A ET E[E][E][E][T]MP/4 QNIKK ...`
- `031838`: `A 3, AT3 , 2TT 2, AND ■ WIAHA MEAN OF 2 TT` -> `A 3, [A][T]3 , 2TT 2, AND ■ WIAHA MEAN OF 2 T[T]`
- `031948`: `TM 150 110, AND 110 ...` -> `[T][M] 150 110, AND 110 ...`
- `032050`: `... BE FOTAND IN TELEWRITTER ...` -> `... BE FO[T][A]ND IN TELEWRITTER ...`
- `032129`: `■ MTMTJ26 PGOPAGATION ...` -> `■ [M][T][M]TJ26 PGOPAGATION ...`
- `004234`: `... T HANTT TK■ET FOR ...` -> `... T HAN[T][T] TK■[E][T] FOR ...`

**6. The 7.052 traffic net:** searched once. It is absent (section 1), so no stretch is printed.

**Exit diffs:**
- `git diff 80df7cd5 -- src` prints only the change in `CwProbabilisticStream.cs`.
- The port diff against `19109b51` and the transmit diff against `7e209cb4` (eleven files, all
  present) print nothing.
- `git status` shows `.run-unit/fldigi/`, `SESSION.lock` and `STOP` untracked.

## 4. What's blocking us

1. **R86 mismatch.** R86 (`PHASE_PLAN.md` section 6) reads that steps 2 to 8 are not authorable
   until 9.4 has a kept change. 9.4 has no route left (ARBITER.md section 4, (A) to (E) and (W)
   refused). This unit worked step 3 as the launcher named it, as units 468 to 471 did. This is a
   plain reading, not a claim that R86 is overruled.
2. **3.4.** The 7.052 traffic-net recording holding `EETTTEETTTTTTTTETTETETKTETEE` is not in the
   tree. No unit can meet 3.4 until that recording and its `cases-*.txt` row are added.
3. **DECIDED (4), the reading of where a letter sits, is the arbiter's and overrulable.**
   - **Where the tree forced a reading (R85):** a real key is text without timing, so on the real
     set the position comes from the alignment and the Morse patterns alone.
   - **The two readings disagree on the exact set, 12 of 14.** The alignment files most splits as an
     added letter in a word gap or as a stand-in. So "0 real splits" is the alignment's reading and
     likely understates them. `032050`'s `FOTAND` and `004234`'s `HANTT`, dimmed by this change, look
     like splits.
4. **3.5's count after this unit: 0 of 3.** Unit 472 kept a change. It was 0 after unit 445, and no
   unit worked step 3 between them.
5. **An owner's `STOP` file** appeared at the root at 19:30, empty, after the task 2 commit. I read
   it as `run-phase.bat` does: finish the running unit, then the run ends. I finished this unit,
   started nothing after it, and left the file in place for the runner.
6. **The edge is narrow**, 0.62 against the lowest right letter at 0.632, as unit 445's was.
   Beside the 15 scored letters it dimmed, all wrong, it dimmed 23 letters outside the keyed
   stretches, whose rightness no key states (section 2).
7. **A second form, one line:** the group's sixth letter (12.005 s `T`, 0.658) and the real
   stand-ins under 0.632 suggest the same quantity with an edge from each sender's own measured
   gaps. It was not tried.
8. **`TheSpeedFollowsTheSendersMarkPairsTests`** moved with the change, from 28 of 31 to 26 of 29
   sure wrong or added. It is red as before and on neither carry-forward line.
9. **Mismatch:** `.run-unit\watched.rc`, which the instruction names, did not exist.
