READ IN THIS ORDER.

A. Phase goal: Hamlet meets the CW requirements. Steps by the plan -
   0 done, 1 done, 2 3 of 5, 3 3 of 6, 4 5 of 7, 5 1 of 6, 6 3 of 6,
   7 0 of 5, 8 0 of 6, 9 3 of 8.
B. Step 9, criterion 9.4: HM-REQ-129 - the technique taken from cw.cxx
   524-535 and 831-843 (fldigi's dot-dash pair speed tracking, (D)),
   against task 1's largest group (d), tied with (a) at 5 of 18
   departures, with (D) behind 6 of 18; with it in, real (inferred)
   MET-CER-SURE 33 of 436 to 25 of 431 sure, MET-INVENTED 33 to 25 over
   473, coverage 403 to 406, MET-WBE 37 to 37 (29/8 to 28/9 inserted/
   deleted); synthetic (exact) the same on all four at 14 of 173, 14,
   159 and 44; adjudicated 12 of 13 (031948 broke); V-11 3 of 23 keyed
   recordings worse; refused and reverted; 9.4 open; the port
   byte-identical yes; 9.5 to 9.8 open as they stand, 9.8 held red by
   the three named floors (443 DECIDED (3)).
C. The findings weighed against A and B: section 4 raises 5 items. Item
   1 (the task 2 test left red in the tree) and item 2 (what 9.4 tries
   next) bear on 9.4; neither blocks 9.5. Item 3 bears on step 2's
   MET-CER-SURE: the refused change took the real figure from 33 to 25
   and was refused on three recordings and one adjudicated comma, so
   the next author knows exactly what stood in the way.

UNIT:       459 - complete at task 4 of 4, task 3 dropped - 2026-09-26 18:59
PHASE GOAL: Hamlet's CW decoder meets every requirement in CW_REQUIREMENTS.md, each proven by a test that names it; section M runs fldigi's receiver beside ours as a second decoder, scored, then calibrated, then voting.
UNIT GOAL:  Find, from our decoder's own evidence, the elements it loses where the fldigi port reads the letter right, take the one port mechanism behind the most of them into our decoder as its own change, and keep it only if R78 says the numbers and every recording are no worse.
ADVANCED:   no - 9.4 not ticked: the one technique taken (fldigi's pair speed tracking) was refused under R78 on the adjudicated 031948 reading and three recordings made worse (V-11), and was reverted; the trace and the red test stand
NUMBER:     HM-REQ-129 not met; HM-REQ-010 real MET-CER-SURE 33/436 to 33/436 (inferred), synthetic 14 to 14 (exact); coverage real 403 to 403; MET-INVENTED real 33 to 33; the port byte-identical yes
DRIFT:      step 2 0; step 3 0; step 4 1; step 5 1; step 6 0; step 9 1 (was 0)

## 1. What Claude did

**Complete through the exit round; task 3, the drop candidate, was shed.** Tasks 0, 1, 2 and 4
are done. Development machine, Claude Code, `PROJECT: Hamlet` claimed and confirmed by the gate
(the four files present, no `CoreHMI.sln` or `MURC.sln`). Branch `main`, pushed after every
commit. The session found `SESSION.lock` already present (PID 16540, 17:33:59); it is the
launcher's and was not taken or released.

**Commits:** `124c87b1` (task 0), `31dd7773` (task 1), `1fcb0376` (task 2, the test, watched
red), `6a0b65a1` (task 2, the change), `796f9af4` (task 2, refused and reverted), and the exit
commit that carries this report.

**Task 0.** 459 block added to both copies of `PHASE_OUTCOME.md`; `PHASE_STATUS.md` names 459
with `CURRENT_STEP: 9` in both copies; version 1.13.145 to 1.13.146. The runner's writes committed
as they were; `.run-unit/fldigi/` not staged. Entry round, every figure as 458 left it: build 0
errors; engine line 178 of 178; app line 278 of 278 with no dispatcher-loop loss; adjudicated 13 of
13; named 10 of 13 as recorded; the four metrics as in the table in section 3; the port 8 of 8;
`BothDecodersAreScoredAlikeTests` 5 of 5. Our 126 text lines saved to
`.run-unit/unit459-text-before.txt`, the port's 74 to `.run-unit/unit459-port-before.txt`.

**Task 1, the trace** (`WhereOursLosesWhatThePortKeepsTests`, `.run-unit/unit459-trace.txt`).
Every key letter ours prints sure and wrong where the port prints the same key letter sure and
right, in the harness's own alignments: **18 departures**, 11 real (inferred keys), 7 synthetic
(exact). For each, our decoder was driven again exactly as the harness drives it and the read
behind the letter kept; a copy of `DecodeAt` re-spelled the winning path segment by segment and
**matched the decoder on 18 of 18 reads**, so every dit, dah and gap printed is the decoder's own.
Each carries its marks and gaps in ms, samples and units, the speed, pitch and thresholds in force,
each segment's level in noise sigmas, where the element went, the port's representation, speed
and `two_dots`, and our own lattice re-run on the same window at the port's speed. Groups:
(a) never keyed 5, (d) ended by a gap rule or a speed too high 5, (b) keyed and thrown away 4,
(c) classed as the other element 2, and 2 where ours added an element. **The technique chosen is
(D), fldigi's speed tracking:** (a) and (d) tie, and (D) is behind 6 departures, 5 real, including
032113's (c), where the speed classed 70 ms dits as dahs; on four of them our own lattice at the
port's speed reads the key letter from marks it already keyed. **Why not (A):** 17:37's missing
dahs of R and D stand no higher than noise in our envelope (median 1.0 sigma, peak 2.3, against a
keyed level of 3.9), so no detection rule on our envelope keeps them.

A second printer (`WhatSetTheSpeedAtEachDeparture`, `.run-unit/unit459-speedset.txt`) found what
set the wrong speed: at every speed-caused departure the window's short mark cluster sat at 20 ms
(bursts) or its short gap cluster at 15 to 20 ms (dropouts), pulling `CwUnitEstimator.Measure` to
28 to 40 WPM; the port's pair rule over the same window's marks holds 17 to 22 WPM, the port 17 to
26. Where ours had the speed right (17:37, 004234, the synthetic cases), the pair speed agrees with
ours within 1 to 2 WPM.

**Task 2.** The test, `TheSpeedFollowsTheSendersMarkPairsTests` (HM-REQ-010, HM-REQ-129), is
032050's window made exact: dit 80, dah 190, element gap 50 ms, gaps of three and seven dits,
15 dB, and a 20 ms burst at the sender's pitch and level in the middle of every letter and word
gap. It asserts no sure character wrong or added and coverage of at least 0.90. **Red at HEAD: read
at 32 to 37 WPM, 28 of 31 sure wrong or added, 3 of 21 right**, committed red (`1fcb0376`).

The change (`6a0b65a1`): `CwUnitEstimator.PairUnit`, our code following `cw.cxx` at `61b97f41` - a
mark under half the tracked dit is a spike (515, 818-822); a mark more than two and less than four
times the one before, or the reverse, is a pair (831-843); the pair's mean goes into a 16-long
average that fills with its first value (524-535; Cmovavg, `filters.cxx:273`). In
`CwProbabilisticStream.Read`, after the marks' overrule, where the pairs' unit times the read's
speed exceeds the existing `MarksOverruleRatio` of 1.25, the window is read again at the pairs'
speed inside the unchanged 8 to 40 WPM bounds. Nothing under `Cw/Second/` changed. With it in, the
test read `CQ CQ CQ DEN0CALL N0CA<AS>LL K` at 17 to 19 WPM - 21 of 21 right, 1 of 22 added (a burst
read as a fifth element of L) - still red.

**Judged under R78, refused:** the real set's three numbers moved the right way and MET-WBE held,
and the synthetic set was unchanged, but the adjudicated reading on 031948 broke (`110, AND 110
WITH A MEAN OF 117` read `11■ AND 110 ...`, not its own adjudicated text, R66), V-11 found three
recordings worse (below), and three named floors fell. Reverted in its own commit (`796f9af4`);
`git diff 1fcb0376 -- src` is empty. The test stays in the tree red, as written (section 4, item 1).

**Task 3 was dropped.** It is the drop candidate the instruction names. At the refusal the session
was 65 minutes in, past the unit's window, and task 1's own trace argues against the next group's
mechanism: the three real (a) departures are two dahs with nothing above noise in our envelope and
one letter under a burst-inflated keyed level. The next unit inherits 9.4 open, the trace, the
speed evidence and the red test.

**Task 4, the exit round:** every figure as at entry (section 3); the task 2 test red at 28 of 31
as committed; `git diff 7e209cb4` over the eleven transmit files prints nothing; `git diff
19109b51 -- src/Hamlet.RadioEngine/Cw/Second/` prints nothing, and neither does `git diff
19109b51 -- src`; the port's texts byte-identical to task 0's save; our texts identical to task
0's save; `.run-unit/fldigi/` still untracked.

**Decisions the session made for itself:** (1) the (a)-(d) tie was broken by the mechanism behind
the departures and by the re-read at the port's speed, both printed; (2) fldigi's seed speed, its
operator setting of 18 WPM, was not taken into ours - the average starts at its first pair, as
Cmovavg does when empty; (3) the overrule reuses the existing 1.25 edge and bounds rather than a new
number; (4) the refused change was not narrowed and re-tried after its numbers were seen; (5) the
task 2 test was left red rather than skipped or removed; (6) task 3 was shed as the named drop
candidate.

**Mismatches with the instruction, the tree taken as the fact:** the port reads 003758's first A
as E too, so only its second A and the 4 are departures; the port misses 004234's U, so only its O
is; there is no `.run-unit/watched.rc` in the tree. The instruction carried no `Asks still
outstanding` queue; none was reconstructed beyond section 4 below.

## 2. What the owner should expect

Nothing changes on the screen: the one technique taken from fldigi, holding the sender's speed
from pairs of his own dits and dahs, was refused and taken back out, so every recording reads
exactly as it did yesterday, letter for letter. With it in, the bulletins read better in most
places - `AA4MP/4` where Hamlet printed `EETMP/4`, `FOUND IN TELEWRITTER, PACKE` where it printed
`FOTAND ... PA■ KE`, `AND INTERNET VERSIONS` where it printed `A N O INTERNET ■ERSIONS`, `WILL SAY`,
`A TUBE XMTR` - but it turned the adjudicated `110,` into `11■`, `EACH` on the ARRL bulletin into
`I ACH`, and `PREDICTED` into `PLDICTED`, and the rules do not trade one reading for another. What
will look wrong but is not: a new test, `TheSpeedFollowsTheSendersMarkPairsTests`, fails when run
on its own; it is the evidence for the open requirement, it is outside both carry-forward lines,
and it was left red on purpose.

## 3. What you should see

**Every departure's stretch: key, ours before, ours with the refused change, the port** (the
stretch each decoder's text was scored on; ours at exit equals ours before).

```
17:37 (inferred)       key         CQ CQ CQ DE WB6RED WB6RED
                       ours before CQ CQ CQ DEWB6 RE D W B 7E E I
                       ours after  CQ CQ CQ DEWB6 RE D W B 7E E I
                       port        CQDW SRED
003758 (inferred)      key         AA4MP/4 QNIK
                       ours before EETMP/4 QNIK
                       ours after  AA4MP/4 QNIK
                       port        EA4M■ /4_NIK
031838 (inferred)      key         2, 2, AND 2 WITH A MEAN OF 2.9. PRE
                       ours before TT 2, AND ■ WIAHA MEAN OF 2 TT
                       ours after  2 2, AND  ■ WIAH A MEAN OF 2
                       port        2, 2, AN■TWV2F 2P
032012 (inferred)      key         N OF 117. LINKS TO ARTICLES OR OTHER WEBSITES MENTI
                       ours before T F 117.1. LINKS TO ARTICLES OR OTHER WEBSITES MENTI
                       ours after  T F 117.1. LINKS TO ARTICLES OR OTHER WEBSITES MENTI
                       port        O7■. LOÈICLOROTIDI
032050 (inferred)      key         THIS BULLETIN CAN BE FOUND IN TELEPRINTER, PACKET, AND INTE
                       ours before     IULLETIN CAN BE FOTAND IN TELEWRITTER, PA■ KE    H IE
                       ours after      IULLETIN CAN BE FOUND IN TELEWRITTER, PACKE   H E SE
                       port        EN<AR> OUÉTERAW■CTY I
032113 (inferred)      key         ACKET, AND INTERNET VERSIONS
                       ours before A KET■ A N O INTERNET ■ERSIONS
                       ours after  A KET■ AND INTERNET VERSIONS
                       port        T TNDINE OJ
004234 (inferred)      key         OT COM <BT> THANK YOU
                       ours before O M <BT> T HANTT
                       ours after  O M <BT> T HANTT
                       port        T T AN■U O
cq-18wpm-15db-char5    key         CQ CQ CQ DE N0CALL N0CALL K
(exact)                ours before C QCQCQDEN0 C A E LN 0CAL L K
                       ours after  C QCQCQDEN0 C A E LN 0CAL L K
                       port        E Q C Q D E N 0 C A L L N 0 C A L L K
cq-18wpm-5db-char5     key         CQ CQ CQ DE N0CALL N0CALL K
(exact)                ours before C QC Q C Q TEE T ■KTDUUEUEN 0CAL L K
                       ours after  C QC Q C Q TEE T ■KTDUUEUEN 0CAL L K
                       port        E Q D N 0 C T L L 0C A L L K E
and the two that refused it:
004507 (inferred)      key         AT ARRL DOT NET <BT> EACH STATION HANDLING THIS MESSAGE P
                       ours before  T ARRL D O T N E T <BT>  E ACH STATION HANDLING THIS MESSAGE P
                       ours after   T ARRL D O T N E T <BT> I ACH STATION HANDLING THIS MESSAGE P
031948 (adjudicated    key         110, 110, AND 110 WITH A MEAN OF 117
 110, AND 110 ...)     ours before 150 110, AND 110 WITH A MEAN OF 117
                       ours after  150 11■ AND 110 WITH A MEAN OF 117
```

**1. Task 1's grouping**, 18 departures, 11 real, `cw.cxx` at `61b97f41`:

| group | departures | real | port mechanism behind it |
|---|---|---|---|
| (a) never keyed as a mark | 5 | 3 (17:37 R, D; 032012 O) | (A) detection, 610-623, 640-641, 649-656 |
| (d) ended by a gap rule or a speed too high | 5 | 4 (003758 A, 4; 031838 `,`; 032050 U) | (D) speed tracking, 524-535, 831-843, 502; (E) 880-888 |
| (b) keyed and thrown away | 4 | 1 (004234 O) | (B) spike rule, 515, 818-822 |
| (c) classed as the other element | 2 | 2 (031838 `2`; 032113 D) | (C) classing, 846-855; 032113's by the speed, so (D) |
| an element added | 2 | 1 (031838 T) | none of (A) to (E) |

The fifth (d), synthetic L on cq-18wpm-5db-char5, qualifies by the split rule only: ours `E` and
the next letter `D` spell `.-..` across a 475 ms gap. It is counted as the rule gives it and
flagged in the trace as a coincidence of patterns.

**2. The four metrics per condition**, entry, with the refused change in, and exit:

| condition | key | metric | entry | change in | exit |
|---|---|---|---|---|---|
| real HF, 23 recordings | inferred | MET-CER-SURE | 33 of 436 | 25 of 431 | 33 of 436 |
| | | MET-INVENTED | 33 / 473 | 25 / 473 | 33 / 473 |
| | | coverage | 403 / 473 | 406 / 473 | 403 / 473 |
| | | MET-WBE | 37 (29 ins, 8 del) / 113 | 37 (28 ins, 9 del) / 113 | 37 (29 ins, 8 del) / 113 |
| synthetic, 12 cases | exact | MET-CER-SURE | 14 of 173 | 14 of 173 | 14 of 173 |
| | | MET-INVENTED | 14 / 252 | 14 / 252 | 14 / 252 |
| | | coverage | 159 / 252 | 159 / 252 | 159 / 252 |
| | | MET-WBE | 44 (13, 31) / 84 | 44 (13, 31) / 84 | 44 (13, 31) / 84 |

Decode time, ours over 690 s of real audio: 52.0 s at entry, 53.5 s with the change in, 53.4 s at
exit with no change; the difference is the machine, not the change. The port's row in `parity.md`
did not move; the port is byte-identical at every run. Adjudicated readings: 13 of 13 at entry and
exit, 12 of 13 with the change in.

**3. V-11, every recording that moved with the change in** (all 35 read; the rest identical):

| recording | key | sure wrong | sure added | sure right | word boundaries wrong | verdict |
|---|---|---|---|---|---|---|
| 003758 | inferred | 3 to 0 | 0 to 0 | 8 to 11 of 11 | 0 to 0 | better |
| 031838 | inferred | 5 to 1 | 0 to 0 | 16 to 16 of 26 | 1 to 1 | better (4 fewer sure emitted) |
| 032113 | inferred | 1 to 0 | 0 to 0 | 21 to 23 of 25 | 3 to 1 | better |
| 004507 | inferred | 0 to 1 | 0 to 0 | 43 to 42 of 44 | 5 to 5 | **worse** |
| 031948 | inferred | 1 to 1 | 0 to 0 | 26 to 24 of 28 | 0 to 0 | **worse** (coverage) |
| 032050 | inferred | 4 to 4 | 1 to 0 | 35 to 36 of 50 | 2 to 4 | **worse** on MET-WBE to improve the others |

**4. The named floors:** at entry and exit, 17:37 38 of 46, 032113 43 of 45, 032129 42 of 64,
held red as recorded, the other ten green. With the change in, those three unchanged and three
more fell below: 003758 43 to 38 of 43, 031838 29 to 23 of 29, 031948 31 to 29 of 31. None is
re-banked.

**5. Task 3** was not run.

## 4. What's blocking us

1. **The task 2 test is left in the tree red.** Ruling taken by the session, overrulable:
   `TheSpeedFollowsTheSendersMarkPairsTests` stays as written, failing at 28 of 31 sure wrong at
   HEAD, as the standing evidence for HM-REQ-010 under bursts between letters. Reasoning: it was
   built from 032050's measured window before the change, the change it was written for was
   refused, and it is the test the next 9.4 attempt should turn green. It is outside both
   carry-forward lines, so the lines stay green. Rejected: deleting it, which throws away an exact
   case of the failure; marking it skipped, which hides a red requirement; loosening its assertion
   to the change's 1 of 22, which is V-14's fixture fitting. Needs the owner only if a red test in
   the tree is unwanted between units.
2. **What 9.4 tries next.** Ruling for the next author, not taken here: the refused pair speed
   fixed the speed overshoots it was aimed at, and it broke three reads it was not aimed at
   (031948's `110,`, 004507's `EACH`, 031905's `PREDICTED`). The next unit should trace those
   three reads - which windows moved speed, and whether the pairs there were few or mixed - before
   any second form is built. Rejected here: narrowing the rule (a larger edge, a full 16-pair
   average) after seeing R78's numbers, which is tuning a constant to the fixture that refused it;
   and taking (A) as task 3, which the trace says cannot recover 17:37's dahs.
3. **Step 2's MET-CER-SURE has a measured lever.** Recorded, not ruled: the refused change took the
   real set from 33 of 436 to 25 of 431 with coverage up by 3 and MET-WBE level. No step 2 line is
   ticked or moved (443 DECIDED (3)).
4. **The instruction's departure list disagreed with the tree twice** (003758's first A, 004234's
   U, where the port is wrong too). Taken as the tree has it; logged, not repaired.
5. **CW reading taken from fldigi's source (R85), one line:** fldigi seeds its tracker at the
   operator's speed setting (`cw.cxx:272-273`, `progdefaults.CWspeed` 18); ours took no seed and
   started at the first pair, as `Cmovavg::run` does when empty (`filters.cxx:273`), because
   Hamlet's receiver has no operator speed and a default would be a prior on the sender.
