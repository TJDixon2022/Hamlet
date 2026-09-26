READ IN THIS ORDER.

A. Hamlet meets the CW requirements: step 6 at 1 of 6, step 5 at 1 of 6, step 4 at 5 of 7,
   step 2 at 3 of 5, step 3 at 3 of 6 by the plan's checkboxes, steps 0 and 1 done, 7 and 8 not
   started.
B. Step 6, criterion 6.3 (HM-REQ-080, 081): MET-WBE real 46 over 113 (39 inserted, 7
   deleted), synthetic 48 over 84, per condition in section 3; HM-REQ-080 not met on 17:37 (7 over
   6, inferred) and on the character-gap-5 15 dB passband case, met on the TX-ITU 15 dB passband
   case; HM-REQ-081 not met on both 0 dB passband cases; conditions not measurable here 20; the
   change kept, MET-WBE real 46 to 37, 17:37 7 to 7; 6.3 ticked; 6.1, 6.2, 6.4 to 6.6 open.
C. This report adds MET-WBE per condition with inserted and deleted boundaries apart, a trace of
   all 94 wrong boundaries in seven groups, and one kept change: where nothing is measured or held,
   the relabel reads the sender's letter spacing from the gaps the path already placed between
   letters. No letter moved. Section 4 raises 3 items; none is in the way of a criterion in B.
   6.6 stays red on 443's DECIDED (3) and the same three named floors.

UNIT:       452 - complete at task 3 of 3, none dropped - 2026-09-26 11:33
PHASE GOAL: Every must-tier CW requirement in CW_REQUIREMENTS.md is met and shown met by a test naming it, step by step, ending with Tim reading real CW on the air.
UNIT GOAL:  Say, per condition and with the key's kind, how often the decoder breaks a word or runs two together, state HM-REQ-080 and 081 against that, trace every wrong boundary to its cause, and fix the largest movable cause without making any recording's boundaries or letters worse.
ADVANCED:   yes - PHASE_PLAN.md 6.3 flipped from [ ] to [x] in both copies, on MET-WBE per condition with inserted and deleted apart, 080 and 081 stated per condition, and the figure in metrics.md
NUMBER:     MET-WBE real 46 -> 37 over 113; synthetic 48 -> 44 over 84; 17:37 7 -> 7; 080 not met; 081 not met
DRIFT:      step 2 0; step 3 0; step 4 1; step 5 1; step 6 0

## 1. What Claude did

**Complete, at task 3 of 3. Nothing was dropped.** That includes task 1's drop candidate, the
per-boundary rows of the synthetic 0 dB cases, which are kept in the printout.

**Provenance.** Claude Code on the Windows machine. Project Hamlet was confirmed by the gate:
SHACK_FACTS.md, CwProbabilisticDecoder.cs, CW_REQUIREMENTS.md and CW_SPEC.md are present, and
there is no CoreHMI.sln or MURC.sln. Branch `main`.

**Commits**, all pushed to `origin/main`:
- `a523b39e`, task 0: the entry, with the runner's writes committed as they were.
- `ad0d78d1`, task 1: the measurement, the tick, and the trace printer. **This departs from the
  instruction**, which asked for the measurement in a commit of its own. My first commit, meant for
  the printer, stopped when `git add` refused an ignored `.tmp` file. The printer was already
  staged, so it went in with the measurement.
- `50069134`, the change on its own.
- `97d08f0a`, task 2, the change kept. **Its message says 30 recordings' text changed. The diff
  shows 27.** That count is corrected here and in the exit commit.
- The exit commit, which carries this report.

**Quoted from the documents, as section 1 of the instruction asks:**
- **HM-REQ-080:** "On every must-tier sender profile at 15 dB reference on CH-AWGN, the decoder
  shall place every word boundary where the sender placed it (MET-WBE = 0)." Rationale: "Part of
  "decoded whole". `DEW B 6 RE D` is not whole." Verification row 080: "T | each must TX at 15 dB |
  MET-WBE | 0 | — | synthetic + WB6RED key (inferred)". Matches the instruction.
- **HM-REQ-081:** "On every must-tier condition at the sensitivity floor, the decoder shall keep
  MET-WBE at or below 5 % of words." Row 081: "T | each must condition at floor | MET-WBE | ≤ 5 % |
  — | synthetic". Matches.
- **HM-REQ-082:** "The decoder shall score word-boundary errors separately from character
  errors." Row 082: "I, A | scoring code | MET-WBE separate from MET-CER | yes". Matches.
- **MET-WBE** (`CW_SPEC.md` 11): "Word gaps inserted or deleted relative to truth / words sent.
  Scored on boundaries alone, separately from MET-CER."
- **The sensitivity floor** (8.3): "−7 dB reference at 20 WPM on CH-AWGN with TX-ITU. It scales
  at 3 dB per doubling of speed."
- **Must-tier senders** (10): TX-ITU, TX-KEYER-W, TX-FARNS and TX-TIGHT. TX-BUG and TX-STRAIGHT are
  should-tier, and TX-SLOPPY is later. **Must-tier channels** (9.2): CH-LM, CH-MM and CH-HM, each
  with an allowance against the CH-AWGN floor.

**Mismatches between the instruction and the tree** (reported, not repaired):
1. **The tree has no condition that is HM-REQ-080's own.** The instruction lists "TX-ITU at 15, 5
   and 0 dB". The synthetic set's SNR is measured inside the 350 to 870 Hz passband, and its
   noise is a shaped band that has not been shown to be CH-AWGN. By arithmetic alone, 15 dB in 520
   Hz is about 8.2 dB in the 2500 Hz reference, and 0 dB is about −6.8. So the "15 dB" case is not
   080's 15 dB, and the 0 dB case is only near the floor. I state the verdicts on those cases
   because the instruction asks for them. The requirements' own 20 conditions are listed as not
   measurable here.
2. **The character-gap-5 row is not a full TX-FARNS profile.** Its character speed equals its
   overall speed. The tree labels it "inside TX-FARNS's 3 to 7", and so does this report.
3. **The scorer.** MET-WBE is computed by `CwMetrics.WordBoundaries`. It splits inserted from
   deleted in `CwBoundaryErrors`, and `TheRequirementsAreMeasuredTests` prints both per recording.
   Its per-condition row prints only the combined "boundaries wrong". The per-condition split in
   this report comes from the new printer, which agrees with the metric's count on every condition.
   The scorer is unchanged.
4. **What the instruction says that holds:** HEAD was `6d40cc90`. In both copies of the plan, 5.5
   was ticked, no step 6 line was ticked, 2.1 to 2.3 were ticked, and 2.4 and 2.5 were not. 17:37's
   key is `CQ CQ CQ DE WB6RED WB6RED`, inferred, 7 wrong over 6 words.
5. **The known items.** Each was confirmed and none was edited:
   - `PHASE_OUTCOME.md`'s header still carries the old titles for steps 2, 3 and 8.
   - `CW_SPEC.md` 11 still defines MET-COVERAGE as sure over sent.
   - `PARKED.md`'s header says a session never writes it.
   - `TheQuietestBinNoLongerWinsTests` and `ThePitchControlsAreOffThePanelTests` are excluded by
     `Compile Remove`.
   - The RULES_AT item does not hold as stated: `CLAUDE.md` 376 carries HM-DEC-165, which matches
     `PROJECT_STATUS.md`, and CPS-DEC-0183 does not appear in it. Units 450 and 451 reported the
     same.
6. **Expected failures, as listed.**
   - The three named floors were red at entry, under the change and at exit, with the same
     readings: 17:37 38 of 46, `032113` 43 of 45, `032129` 42 of 64.
   - `TheFiveToEightDecibelPlateauHolds` and `AHeldPitchDoesNotOutliveItsEvidenceTests` are on
     neither line. Neither was touched or run.
   - The app line lost 2 at entry, `TheTestsStayOffTheNetworkTests`, to the dispatcher loop. Run
     alone, that type first lost 1 of 5 to a 2-pixel layout re-read, then passed 5 of 5 on the one
     rerun. At exit the line lost 2, `TheFavoritesAreUnderTheGreenZoneTests`, to the dispatcher loop,
     and that type passed 3 of 3 alone. There was no hang.

**Task 0.** The build had 0 errors. The engine line passed 178 of 178 in 375 s and the app line
276 of 278 in 163 s. Captures were 51 of 51 in 129 s, adjudicated 13 of 13 in 32 s, and named 10
of 13 in 68 s. The four metrics were as unit 451 left them. 9 files were more than 25 Hz off
(264 s). 63 recordings' text was saved, identical to 451's.

**Task 1, the measurement and the trace.** The printer is `WhereTheWordBoundariesGoWrongTests`,
and it asserts nothing. Each recording is decoded as the metrics decode it, and the stream's state
is taken by reflection at each settle. That state covers:
- the unit, and whether it was measured;
- the speed's proof state;
- the held gaps and the read that set them;
- this read's character gap, and why the estimator refused one;
- the word-from.

The printer re-runs the relabel on the same inputs to find the spaces it took out. For each wrong
boundary it prints:
- the gap in ms and in units;
- what the path and the relabel did;
- the marks and key-ups of the letters either side;
- the key and the decode around it.

Its counts agree with `CwMetrics.WordBoundaries` on all 10 conditions. The printout is
`.run-unit/unit452-wbe-trace.txt`. I grouped the 94 wrong boundaries by the mechanism that set the
boundary in force. One grouping was tried and corrected before the groups were fixed: the marks the
envelope shows inside a gap proved unreliable, because the path's letter spans sit late against the
marks. Whether letters were lost is therefore read from the alignment.

**Task 2, the change.** **The group chosen was G1**, the largest: 21 real and 18 synthetic. Its
cause is in the audio. Each kept gap is a letter space of 4.6 to 7 units, shorter than that
sender's own word spaces, and the two do not overlap. G1 exists because the envelope's three heaps
found no character heap:
- in 8 reads, a 15 to 20 ms key-up took the shortest heap;
- in 29 reads, there was no trough between the first two heaps;
- 2 reads had under 12 gaps.

**The groups not attacked:**
- G2, 9 real: the measured character gap itself is wrong there, not the fallback.
- G3 and G7, 7 real on 17:37: its spacing overlaps, recorded no by unit 444.
- G4, 5 synthetic: the relabel's 1.53 share against the character-gap-5 sender's ratio of 1.4.
- G5 and G6, 9 real and 24 synthetic at 0 dB: letters not read, not a spacing.

**The rule, set before any number was run** (`.run-unit/unit452-rule.md`): where a read measures no
character gap and the stream holds no gaps of the sender's own, the relabel takes the median of
that read's own gaps between letters, when it has at least 12, as the sender's character gap. It
then takes out a word gap shorter than that times the root of seven thirds, and it never lowers the
boundary below the path's textbook word-from.
- It uses durations only, under R72.
- **How it differs from 444's rule:** 444 removed key-ups from the envelope's clustering in both
  estimators, which moved the held structure and the letters. This rule touches neither estimator,
  the path, the held gaps nor the speed. It only raises the relabel's fallback boundary, so it can
  only take a space out.
- 444's three recordings do not get worse: 17:37 goes 7 to 7, `031905` 1 to 1, and the
  character-gap-5 case at 5 dB 10 to 8.

It was tried once and kept. The trace printer now re-runs the relabel with the new boundary where
the tree has it.

**Task 3, the exit round.** Each result, with its wall time:
- **Build:** 0 errors, 16 s.
- **Engine line:** 178 of 178, 387 s. It was also 178 of 178 while judging the change, in 382 s.
- **App line:** 276 of 278, 165 s. The losses are as in item 6 above.
- **Captures:** 51 of 51, 132 s.
- **Adjudicated:** 13 of 13, 32 s.
- **Named:** 10 of 13, 67 s, the same three at the same counts.
- **The four metrics:** as under the change, 64 s.
- **MET-PITCH-ERR:** 9 files more than 25 Hz off, 259 s.
- **The one test type touched,** `WhereTheWordBoundariesGoWrongTests`: green, 65 s.
- **Text:** identical to the change round's, 140 s.

**What changed in `src`, file by file.** Only `src/Hamlet.RadioEngine/Cw/CwProbabilisticStream.cs`,
44 lines: `LetterSpaceBoundary`, `LetterSpacesToMeasure`, and four lines in `Read` that raise the
relabel's `wordFrom`. In `tests`, one new printer. **None of it keys or transmits.**

**Ticks:** 6.3 only, under the instruction's §3 (a). No other line was ticked. DRIFT follows §3 (e):
step 6 is at 0 because the change was kept.

## 2. What the owner should expect

**What the operator sees.** Today, on the 23 keyed recordings, the operator meets a word broken
in two, or two words run together, at about one boundary in three words: 37 over 113, down from
46. The inferred keys cannot prove any single boundary wrong (V-13).
- **It is worst on 17:37**, where `CQ CQ CQ DEWB6 RE D W B 7E E I` is still 7 wrong over 6 words.
  That sender's letter and word spaces overlap, so no rule on durations gives them back.
- **It is also bad on the 7.052 traffic net:** 5 over 11.
- **The change mended** `AR R L` to `ARRL` on the traffic net, `INT ERNE T` to `INTERNET` on the
  W1AW bulletin, and `FOTA ND` to `FOTAND` (sent as `FOUND`). On unkeyed recordings it mended
  `BOXE S F OR` to `BOXES FOR`, `Q SO WITH WB 4 ET` to `QSO WITH WB4ET`, `MY SELF O CC UPI ED` to
  `MYSELF OCCUPIED`, and `BR EE Z E` to `BREEZE`, one of HM-REQ-084's named spans. No letter changed
  anywhere.
- **What will look wrong but is not a regression the metrics missed:** on `031838`, `W IAH A MEAN`
  became `WIAHA MEAN`. One inserted space went and one true boundary was lost, so the count stays
  1.
- **What may be a real cost the metrics cannot see:** on unkeyed recordings the same rule also ran
  words together. `MONTHS OR S O` became `MONTHSORSO`, and `SAY73 ES T KS T K` became
  `SAY73ESTKSTK`. There is no key to count these, and section 4 asks whether they should weigh.
- **What the synthetic cases do not prove** (12.5): they are clean 1:3:7 or 1:5:7 senders in a
  shaped band, not CH-AWGN, not a fading channel and not a hand fist. That TX-ITU at 15 and 5 dB
  reads with no boundary wrong says nothing about a real TX-ITU sender through a CH-* channel.

## 3. What you should see

**The answer: MET-WBE falls from 46 to 37 over 113 words on the real set, and from 48 to 44 over 84
on the synthetic set. HM-REQ-080 and 081 are not met wherever the tree can measure them.**

| condition | key | words | inserted | deleted | MET-WBE at HEAD | after the change | HM-REQ-080 | HM-REQ-081 |
|---|---|---|---|---|---|---|---|---|
| real, sender not stated (20 recordings) | inferred | 97 | 32 -> 24 | 7 -> 8 | 0.4021 | 0.3299 | not a CH-* condition (7.4) | not a CH-* condition (7.4) |
| real, TX-FARNS, traffic net `004507` | inferred | 11 | 7 -> 5 | 0 -> 0 | 0.6364 | 0.4545 | not a CH-* condition | not a CH-* condition |
| real, TX-ITU, KD0UN `012403` | inferred | 4 | 0 | 0 | 0.0000 | 0.0000 | not a CH-* condition | not a CH-* condition |
| real, TX-TIGHT, `013347` | inferred | 1 | 0 | 0 | 0.0000 | 0.0000 | not a CH-* condition | not a CH-* condition |
| real, 17:37 alone | inferred | 6 | 6 -> 6 | 1 -> 1 | 1.1667 | 1.1667 | **not met**, 7 over 6 | - |
| synthetic TX-ITU, 15 dB in the passband | exact | 21 | 0 | 0 | 0.0000 | 0.0000 | met on this case | - |
| synthetic TX-ITU, 5 dB | exact | 21 | 0 | 0 | 0.0000 | 0.0000 | - | - |
| synthetic TX-ITU, 0 dB | exact | 21 | 0 | 18 | 0.8571 | 0.8571 | - | **not met**, 0.857 |
| synthetic character gap 5, 15 dB | exact | 7 | 10 -> 7 | 4 -> 5 | 2.0000 | 1.7143 | **not met**, 12 over 7 | - |
| synthetic character gap 5, 5 dB | exact | 7 | 9 -> 6 | 1 -> 2 | 1.4286 | 1.1429 | - | - |
| synthetic character gap 5, 0 dB | exact | 7 | 0 | 6 | 0.8571 | 0.8571 | - | **not met**, 0.857 |

**Not measurable here: 20 conditions.**
- HM-REQ-080 on TX-ITU, TX-KEYER-W, TX-FARNS and TX-TIGHT, each at 15 dB reference on CH-AWGN.
- HM-REQ-081 on CH-AWGN, CH-LM, CH-MM and CH-HM at their floors, each with the four must-tier
  senders.

Both are built by 7.1 and 7.2.

**The groups of wrong boundaries at HEAD** (real / synthetic above 0 dB / synthetic at 0 dB), one
example of each, with the key beside it. A bar marks the boundary.

| group | at HEAD | after | example: key / decode |
|---|---|---|---|
| G1 inserted: no character gap measured, so the textbook 4.58 u is under the letter space | 21 / 18 / 0 | 12 / 12 / 0 | `004507` 5.42 s: key `AT_AR|RL_DOT` / decode `AR_|R_L` |
| G2 inserted: this read's character gap x 1.53 is under the gap | 9 / 0 / 0 | 9 / 0 / 0 | `004507` 7.75 s: key `ARRL_D|OT_NET` / decode `D_|O_T` |
| G3 inserted: the held gaps' word-from is under the gap | 6 / 1 / 0 | 6 / 1 / 0 | 17:37 24.82 s: key `DE_WB6|RED_WB6RED` / decode `DEWB6_|RE_D` |
| G4 deleted: the relabel took out the word gap the path read | 0 / 5 / 0 | 1 / 7 / 0 | `cq-18wpm-15db-char5` 4.15 s: key `CQ_|CQ_CQ` / decode `C_Q|CQCQDEN0_C` |
| G5 key letters lost between the two decoded letters | 5 / 0 / 0 | 4 / 0 / 0 | `032050` 2.88 s: key `THIS_|BULLETIN_CAN` / decode `I|ULLETIN_CAN` |
| G6 no decoded letter on one side | 4 / 0 / 24 | 4 / 0 / 24 | `032012` 1.77 s: key `N_|OF_117.` / decode `|T_F` |
| G7 deleted: the path read a letter gap | 1 / 0 / 0 | 1 / 0 / 0 | 17:37 22.05 s: key `DE_|WB6RED_WB6RED` / decode `CQ_DE|WB6_RE` |

**17:37 as the operator reads it**, against the key `CQ CQ CQ DE WB6RED WB6RED`:
- at HEAD: `CQ CQ CQ DEWB6 RE D W B 7E E I`
- after the change: `CQ CQ CQ DEWB6 RE D W B 7E E I`, unchanged. It is not at 5 or fewer, so
  nothing arises under §3 (c).

**Every recording whose text changed, before -> after.** There are 27, and only spaces changed.
Leading and trailing blanks are trimmed here.
1. `cq-18wpm-15db-char5` (key `CQ CQ CQ DE N0CALL N0CALL K`): `C QCQCQDEN0 C A E L N 0 C A L L K` -> `C QCQCQDEN0 C A E LN 0CAL L K`
2. `cq-18wpm-5db-char5`: `C QC Q C Q TEE T ■KTDUUEUE N 0 C A L L K` -> `C QC Q C Q TEE T ■KTDUUEUEN 0CAL L K`
3. `cw-2026-08-17-013347`: `E EI I HIAEIHEEEA E EEE HEEIEE IEE E T E I E E IEEI TEEI T E HA E WVRR VA3VRRR` -> `E EI I HIAEIHEEEA E EEE HEEIEE IEEETE I EE IEEI TEEITEHAEWVRRVA3VRRR`
4. `cw-2026-08-17-013622`: `E ISIIHE II 5EIEIE EEETE TE ESE E IE U EEE TSET TEEE A E ET EEE II I` -> `E ISIIHE II 5EIEIEEEETETE ESE E IE UEEETSET TEEE A E ET EEE II I`
5. `cw-2026-08-17-134712`: `E     ■   NT    N4LQ K` -> `E ■   NT    N4LQ K`
6. `cw-2026-08-18-004507`: `E5 I E A T AR R L D O T N E T <BT>  E ACH STATION HANDLING THIS MESSAGE PE` -> `E5 I E A T ARRL D O T N E T <BT>  E ACH STATION HANDLING THIS MESSAGE PE`
7. `003758`: `■R L T U   I AN EAND E A ET EEEETMP/4 QNIKK   EAN EANQNIK        EAN E` -> `■R L T U   IAN EANDE A ET EEEETMP/4 QNIKK   EAN EANQNIK        EAN E`
8. `031838`: `A 3, AT3 , 2TT 2, AND  ■ W IAH A MEAN OF 2 TT` -> `A 3, AT3 , 2TT 2, AND ■ WIAHA MEAN OF 2 TT`
9. `032050`: `IULLETIN CAN BE FOTA ND IN TELEWRITTER, PA■ K E    H IE S T S SI I` -> `IULLETIN CAN BE FOTAND IN TELEWRITTER, PA■ KE    H IE S TSSI I`
10. `032113`: `A KET■ A N O INT ERNE T ■ E RSIONS OF  200J6   I I I TT E E I I I WE  T I` -> `A KET■ A N O INTERNET ■ERSIONS OF  200J6   I I ITT EEIIIWE TI`
11. `001831`: `E IE E U  KT■TQ   O Q <AR> SGEQ K5QQ 5NNDELA RR SNN TTTO  TUKV` -> `E IE E U  KT■TQ  OQ <AR> SGEQ K5QQ 5NNDELA RR SNN TTTO  TUKV`
12. `001952`: `HET E  E ■HIN WEFU  EENE MCON TA K8T     I  ■ E I II II EE   MN S    B0` -> `HET E  E ■HINWEFU  EENEMCON TA K8T     I  ■ E I IIIIEE   MNS    B0`
13. `002016`: `H■ S   B■   I E E      O NA■T SIT E■      EE  I I   E      K TIE0NH VNN JEENG` -> `H■ S   B■ IEE   ONA■T SIT E■      EE  I I   E     KTIE0NH VNN JEENG`
14. `012823`: `E S SE EE  TTN T K E M TE E O IN U T     E` -> `E S SE EE  TTNT KEMTEEO IN U T  E`
15. `012922`: `I W W M     E  TTTTT II  WA T T S WI QJ SAY73 ES T KS T K S73 D NDH` -> `I W W M   E TTTTT II  WA T T S WI QJ SAY73ESTKSTK S73 D NDH`
16. `013303`: `G ALL BOXE S F OR FOC EESSS    HPE YOU INJOY UR LONG WAI T 5 EE ■` -> `G ALL BOXES FOR FOC EESSS    HPE YOU INJOY UR LONG WAI T 5 EE ■`
17. `013402`: `W■E IN Q SO WITH WB 4 ET ES S S CAME IN TO J OIN? NOT SURE - BUT ANY WAY VY NICE` -> `W■E IN QSO WITH WB4ET ES S S CAME IN TO J OIN? NOT SURE - BUT ANY WAY VY NICE`
18. `013520`: `MONTHS OR S O I GUESS BUT ALL GUD ES  CAN KE E G KI KE EP MY SELF O CC UPI ED A` -> `MONTHSORSO I GUESS BUT ALL GUD ES  CAN KE E G KI KE EP MYSELF OCCUPIED A`
19. `013637`: `TE MP NEVEN T REV■R G OT AB OVE 7 5 F ES CLEAR S KY LI TE BR EE Z E ALL DAY JUST AWE SO` -> `TE MP NEVEN T REV■R G OT AB OVE 7 5 F ES CLEAR S KY LI TE BR EEZE ALL DAY JUST AWE SO`
20. `021410`: `A    T O MTT T  Y M TT  O AO IHI DT RIGHR IS  FLENT 66OAM` -> `A    T O MTTT  YMTT  O AOIHI DT RIGHR IS  FLENT 66OAM`
21. `021825`: `I E   E I ETO U E NT  K OC 1 UEK K` -> `IE E IETOUENT KOC1UEK K`
22. `003901`: `EII E T NHHK` -> `EII ETNHHK`
23. `004027`: `QNE K  E E  2 G ■ 2G KA2GJV QNK CAT WI2TN HRK 5 I HRRHCE Q` -> `QNE K  E E 2G■ 2GKA2GJV QNK CAT WI2TN HRK 5 I HRRHCE Q`
24. `004108`: `HR NR 2 0 R H X G K E 8 W G K 2 6 STMW OH SEP19 RIC` -> `HR NR 2 0 R H X G K E8 W G K2 6 STMW OH SEP19 RIC`
25. `004133`: `1 9 R I C H ARD D J GEL ■ 0 D T  UT 5 0 0 5 H ■ 8` -> `1 9 R I C H ARD D J GEL ■ 0 DT UT 50 0 5 H ■ 8`
26. `004427`: `■ FOORDINATOR <AR>N QSL TN6TBRE C E DEWI2 G K88NET TU EAT` -> `■ FOORDINATOR <AR>N QSL TN6TBRE C E DEWI2G K88NET TU EAT`
27. `004535`: `AA3SB SE IFOK DN5 DN■ EEEDN5EIN5 3RN 1 TH N B O TEI NANX K A3HT` -> `AA3SB SE IFOK DN5 DN■ EEEDN5EIN5 3RN1 THNBOTEINANX K A3HT`

The adjudicated readings on 013347 (`VA3VRR`), 134712 (`N4L`) and 003758 (`MP/4 QNIK`) still read,
13 of 13.

**The §3 (b) table:**

| part of R78 | before (`a523b39e`) | under the change | verdict |
|---|---|---|---|
| MET-WBE, real, inferred | 46 (39 ins, 7 del) over 113, 0.4071 | 37 (29 ins, 8 del), 0.3274 | falls |
| MET-WBE, synthetic, exact | 48 (19, 29) over 84, 0.5714 | 44 (13, 31), 0.5238 | does not rise |
| MET-CER-SURE, real / synthetic | 33 of 436 / 14 of 173 | 33 of 436 / 14 of 173 | rise on neither |
| MET-INVENTED, real / synthetic | 33 over 473 / 14 over 252 | same | rise on neither |
| sure-and-right coverage, real / synthetic | 403 over 473 / 159 over 252 | same | falls on neither |
| adjudicated readings | 13 of 13 | 13 of 13 | hold |
| **V-11**, 35 recordings: boundaries wrong and sure-wrong letters per recording | - | 0 worse. Boundaries: `004507` 7->5, `032050` 4->2, `032113` 7->3, `004108` 2->1, char-gap-5 15 dB 14->12, 5 dB 10->8, 17:37 7->7, `031905` 1->1, `031838` 1->1; every other recording unchanged. Sure-wrong letters unchanged on every recording | holds |
| capture rows, named floors | 51 of 51; 10 of 13 | 51 of 51; 10 of 13, the same three at the same counts (`032113`'s edits 10 -> 6) | as at entry |

The full per-recording V-11 column is in `.run-unit/unit452-v11-change.txt`, and the running figure
is in `docs/phase-requirements/metrics.md` under unit 452.

## 4. What's blocking us

None of these is in the way of 6.3. The first bears on HM-REQ-054 and 080 for TX-FARNS.

1. **The relabel's share against a Farnsworth sender's ratio.** The relabel treats a word gap as at
   least the root of seven thirds (1.53) times the character gap. TX-FARNS allows a character gap
   up to 7 units, whose word gap may be only a little longer. On the character-gap-5 case (1:5:7,
   ratio 1.4) the relabel took out 5 true word gaps at HEAD and 7 under the change: group G4. A
   ruling is asked on whether HM-REQ-054's TX-FARNS spacing binds the relabel's share, and so which
   of 1.53 and the sender's own ratio should govern.
   - Rejected: changing the share here. It is a second value of an existing rule, and it would move
     G2 as well.
2. **V-11 per recording, read net or boundary by boundary.** On `031838` the change removed one
   inserted space and lost one true boundary (`WIAHA MEAN` for `WITH A MEAN`). The recording stays
   at 1 wrong. I read V-11 as the per-recording count, which does not rise. A ruling is asked on
   whether a recording that trades one wrong boundary for another fails V-11.
   - Rejected: refusing the change on this reading. The instruction's column is a count per
     recording.
3. **Unkeyed recordings.** The keep rule judges only the 35 keyed recordings. Of the 27 whose text
   changed, most are unkeyed. There, the change mended words (`BOXES FOR`, `QSO WITH WB4ET`,
   `MYSELF OCCUPIED`, `BREEZE`) and also ran some together (`MONTHSORSO`, `SAY73ESTKSTK`). A ruling
   is asked on whether an unkeyed recording's text should weigh in R78, for example by keying more
   of these recordings, since today nothing can count either kind of change on them.
   - Rejected: judging them by eye here. That would be word reasoning on an inferred reading (R72,
     V-13).
