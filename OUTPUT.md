READ IN THIS ORDER — A the phase, B the step and its exit criteria, C this report against both.

A. PHASE — *Hamlet hears FT8 off the radio and displays the decoded text on screen.* Seven steps.
Steps 1 and 2 **closed**. Step 3 closed on its four must-pass criteria with its nice-to-pass one
recorded as `HM-OPEN-065`. Step 4 closed by unit 214. **Step 5 is this unit's and its fifth**: it
entered at **2 of its 3 subject criteria, with criterion 3 partial at 760 of 1298 against a
representable ceiling of 1157**, and it **leaves at exactly that — 2 of 3, criterion 3 still partial
at 760 of 1298, extras still 23.** Steps 6 and 7 have not started and cannot, every step of this plan
depending on the one before it. **Step 5 was the only step this phase could move tonight.**
**Nothing in this unit is a step 6 measurement and none of it may be counted toward one** — step 6
still wants a reproducible curve, a comparison with the published figure *as a verdict*, and graceful
degradation, and this unit measured **presence per signal**, not sensitivity.

B. STEP 5 — *a found signal becomes a message.* Five exit criteria, by number.
**3 first, because it is this unit's subject.** `ft8_lib`'s reference WAVs decode against its expected
lists: **entering at 760 of 1298 and leaving at 760 of 1298 — PARTIAL, and the count did not move.**
Reproduced column for column in task 1: 7803 candidates, 2733 parity, 2733 checksum, 2263 text, 783
unique, 538 missed, 23 extra, 60 files, 0 skipped. **And the outcome split the night bought whether
or not the count moved: of the 78 strong-SNR misses, 5 are present and recoverable, 35 are present
and not recoverable, and 38 are not present at all — and all five of the recoverable ones are
expected lines their own list carries twice.**
**1.** A corrupted codeword within the code's correcting power is recovered and one beyond it fails
honestly — **met** by unit 215, given a decibel value by unit 218. **Carried by the suite tonight and
not re-argued.**
**2.** A candidate failing CRC is never returned as a decode — **met**, and taken again harder: task
2's ten quiet-frequency sweeps ran 600 alignment points each with real true codewords and **returned
0 messages and recovered 0 codewords.**
**4.** `Ft8Sharp` tests green — **entry 496 total, 495 passed, 0 failed, 1 skipped; exit 502, 501, 0,
1.** The one skip at both ends is `Ft8TableGenerationTests.RewriteTheCheckedInTablesFile`, the table
write gate, and it is correct.
**5.** Attribution clean and channels green — **180 paths at entry and 183 at exit** from `2828ab6`,
the three added being this unit's own test files, and the `src/Hamlet.`/`tests/Hamlet.` filter
**returns 0** at both ends. Channels **55** and **9**, both re-run after the version bumps with
`VersionTests` among them. Filter strings in section 3.

C. THIS REPORT — **the split, first: of the 78, 5 present and recoverable, 35 present and not
recoverable, 38 not present at all.** **And the reading that decides what the 5 mean: every one of
them is a repeated expected line**, whose message the untold path returned and de-duplicated by
upstream's own payload rule, with the search keeping the decoding point in all five. **Outcome-A
lines that are not a repeated expected line: zero — in the 78 and in the further 91 alike.**
**The control group:** 12 of 12 already-matched lines found a decoding alignment at mean agreement
170.2 of 174, every one at bin offset zero; 10 quiet-frequency sweeps returned nothing; 12 of 12
agreement figures equalled the existing instrument's. **No fix was licensed. Condition 2 failed
outright** — the only change the evidence points at is not de-duplicating, which is a divergence
*from* the pin and would put the same message on the screen twice; condition 1 is half-met, naming a
target and no change. **What the split licenses next: a measured bar, not a defect to hunt.** Across
169 matchable missed lines at −5 dB or better, **96 are not present as far as this receiver can see,
68 are present and beyond this code's correcting power, and 0 are recoverable and thrown away.**
**Section 4 raises 2 items, neither is a ruling request, and one bears directly on criterion 3 in B.**
**Task 5's drop condition: the drop branch did NOT apply and the task ran** — the largest single
outcome is 38 of 78, **48.7 per cent against a bar of 66.7**.

UNIT: 219
DATE: 2026-09-02
STATE: COMPLETED
TASKS: 7 of 7
DROPPED: none — task 5 was the named drop candidate and its measured condition forbade dropping it
ADVANCED: no — the sweep answered its question and criterion 3's count did not move; criterion 3 is still partial at 760 of 1298

## 1. What Claude did

**Hamlet confirmed.** All four identity checks hold: `SHACK_FACTS.md` present,
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` present, `ls *.sln` returns `Hamlet.sln` and
nothing else, and `CoreHMI.sln` and `MURC.sln` are both absent.

**Seven tasks of seven, nothing dropped.**

1. **The trace and the ground re-measured.** The suite, the build, attribution, the channels, the
   tree facts — and then the two runs everything tonight is joined to, **both of which reproduce
   exactly.**
2. **The instrument proved before it was trusted.** A sweep told the answer can be written to find
   it, so three checks ran before a single miss was swept: it finds what is there, it refuses what is
   not, and it agrees with the instrument this phase already had. **The neighbourhood and the decode
   rule were fixed and printed here, before task 3 ran, and were not widened afterwards.**
3. **The sweep.** All 78 strong-SNR misses, 600 alignment points each, every one placed in exactly
   one of three outcomes named before the run.
4. **The four readings that came free** — the score at the true point against the search's own
   threshold, the message limit, the passband distribution, and the one recording that returns
   nothing.
5. **The wider 91 lines between −5 and 0 dB**, swept the same way and reported separately. **Not
   dropped**, because its own measured condition forbade it.
6. **The fix conditions weighed. No fix is licensed, and that is an acceptable outcome and it is not
   a failure**, in those words.
7. **The record** — `HM-OPEN-066` extended with its routes re-ranked, `porting-notes.md`'s unit 219
   section, both versions bumped, everything re-run, and this report.

**Committed:** `PROJECT_STATUS.md`, `OPEN_ISSUES.md`, `src/Ft8Sharp/porting-notes.md`, both
`Directory.Build.props` files, and three test files under `tests/Ft8Sharp.Tests/Dsp/` —
`AlignmentSweep.cs`, `Ft8AlignmentSweepControlTests.cs`, `Ft8StrongMissSweepTests.cs` — plus a
six-line internal accessor added to the existing `Ft8MissAccountingTests.cs`. **Left alone:** every
one of the 33 other lines `git status --short` prints, including the eight `.obj`, everything under
`tools\`, `PHASE_STATUS.md` beyond its one directed line, `PHASE_OUTCOME.md`, and the four untracked
probe files.

**One slip, caught and corrected before any push.** Task 3's commit was staged with
`git add tests/Ft8Sharp.Tests/Dsp/`, which swept in three pre-existing untracked probe files —
`Unit216Probe.cs`, `Unit217Probe.cs`, `UpstreamSyncSearchProbe.cs`. They were removed from the index
and the commit amended within the same minute; they are untracked again and `git status --short`
returned to its prior count. **No push had happened, and nothing else was touched.**

**Nothing was tuned and no library file changed.** `DefaultCandidateLimit`, `DefaultMinimumScore`,
`DefaultMessageLimit`, the iteration count and every tolerance stand exactly where the pin put them.
The only library change in the whole unit is the version number and the comment above it.

## 2. What the owner should expect

**The number on the screen has not changed, and the argument behind it has ended.**

For three nights this project has been unable to say whether the five hundred messages it misses on
Karlis Goba's recordings are whispers too faint for any receiver or transmissions it is throwing
away. Last night's unit made that worse rather than better: it proved this receiver hears down to
eighteen decibels below the noise, which is a real FT8 receiver — and then found seventy-eight
messages it missed that those recordings' own lists call *strong*, two of them at plus nineteen
decibels, thirteen of them the same stations missed in file after file at the same frequency.

**Tonight each one of those seventy-eight was picked up individually and asked whether the
transmission is in the recording at all.** The library built the exact bits that station would have
sent, then looked for them at six hundred different alignments around where the signal should be.
**Five of the seventy-eight are there and recoverable. Thirty-five are there and too damaged for the
error correction. Thirty-eight are not there at all as far as this receiver can see.**

**And the five are not what they look like.** Every one of them is a line the recording's own list
prints *twice*, and Hamlet returned that message once and correctly refused to say it a second time —
which is exactly what upstream's own decoder does. **Not one of the seventy-eight is a message this
library could have read and threw away.** The wider set of ninety-one weaker misses says the same
thing with an even flatter answer: none recoverable at all.

**So there is no defect to hunt here.** The missing messages are a mixture of transmissions that are
not on those recordings in any form this receiver can reach, and transmissions that are there and too
faint. That is a real answer and it is the one the project needed before it could decide what to do
about the number.

**What it is not.** It is not a verdict on whether the *reference* decoder — the C program these
recordings came with — would find them either. **That program is still not built on this machine, and
tonight's result promotes building it from third place to first among the things that would settle
this.** It is also not a sensitivity result: step 6 still owes a proper curve and a proper comparison
with the published FT8 threshold, and this unit claims none of it.

**Nothing keyed a radio, nothing wrote audio anywhere, and nothing reached a screen.** The library
version moves to `0.10.3` and Hamlet's to `1.12.26`, both patches, because the library gained evidence
rather than a capability.

## 3. What you should see

### The outcome table — the counts first, before any interpretation

```
THE 78 - MISSED MATCHABLE EXPECTED LINES AT 0 dB OR BETTER

  lines swept                                    : 78
  A  PRESENT AND RECOVERABLE                     : 5
  B  present and not recoverable                 : 35
  C  not present as far as this receiver can see : 38

  points swept per line                          : 600
  belief propagations run in all                 : 1712

  lines whose text the untold path DID return    : 5
  of those, in outcome A                         : 5
  OUTCOME A LINES THAT ARE NOT A REPEATED LINE   : 0

  Of the outcome A lines, whether the search kept the decoding point:
    the search KEPT it, so the loss is after the search : 5
    the search kept NO candidate there                  : 0

  best agreement over the population, mean       : 133.2 of 174
  highest                                        : 174
  lowest                                         : 101
```

**The headline in one sentence with three numbers in it:** *of the 78 strong-SNR misses, 5 are present
and recoverable, 35 are present and not recoverable, and 38 are not present at all.*

### The distribution of that agreement, so the bound can be seen rather than believed

```
agreement     lines
100-109           5
110-119          16
120-129          17
130-139          10   <- the bound is at the foot of this band
140-149          14
150-159           8
160-169           3
170-179           5

  HIGHEST agreement among the C lines : 129
  LOWEST agreement among the B lines  : 132
```

**The bound of 130 was fixed in task 2, before task 3 ran, and it fell in a two-point gap rather than
through a cluster.**

### All 78 rows

`agree` is the best hard-decision agreement out of 174 found anywhere in the neighbourhood and `at`
is where; `score` is the best sync score found and where; `rank` is the best rank the search itself
gave any point in the neighbourhood, `-` meaning it kept none; `dec` says whether anything decoded to
the expected text exactly; `rpt` marks a line whose text the untold path did return for that file.

```
file                    listHz snr agree at                     score at                     rank   dec      rpt O  text
20m_busy/test_14.wav      2046  19   120 blk  9 t1 bin 295 f0     29 blk  9 t0 bin 295 f1       6   no           C  RA9UJP 9A9A RR73
20m_busy/test_28.wav      2046  19   120 blk  9 t0 bin 295 f1     29 blk  9 t1 bin 295 f1       5   no           C  LY3BES 9A9A RR73
20m_busy/test_18.wav      2046  17   120 blk  9 t1 bin 295 f0     27 blk  9 t0 bin 295 f1      10   no           C  LU5HA 9A9A RR73
20m_busy/test_22.wav      2046  17   122 blk  9 t1 bin 295 f0     27 blk  9 t1 bin 295 f1      12   no           C  LU5HA 9A9A RR73
191111_110615.wav          906  15   174 blk  9 t1 bin 113 f0     38 blk  9 t1 bin 113 f0       1   DECODED  yes A  PA3EPP SP8NFO KN09
20m_busy/test_21.wav      2378  13   111 blk  3 t1 bin 349 f0     25 blk 14 t1 bin 350 f0      19   no           C  CQ SP9LKP JO90
20m_busy/test_29.wav      2388  13   120 blk 14 t1 bin 350 f0     24 blk 14 t1 bin 350 f0      22   no           C  RV6ARS E75C RR73
20m_busy/test_01.wav      2378  12   113 blk -3 t0 bin 348 f1     25 blk -3 t0 bin 348 f1      19   no           C  R1CBP SP9LKP RR73
20m_busy/test_07.wav      1088  12   135 blk  9 t1 bin 142 f0     25 blk  9 t1 bin 142 f0      23   no           B  R3FO R7NO -16
20m_busy/test_15.wav      2389  12   119 blk 14 t1 bin 350 f0     23 blk 14 t1 bin 350 f0      33   no           C  PA3GAE E75C RR73
20m_busy/test_07.wav      2378  11   111 blk -3 t1 bin 348 f1     26 blk 15 t0 bin 350 f1      19   no           C  F4VTS SP9LKP -20
20m_busy/test_19.wav      1561  11   132 blk  0 t0 bin 218 f0     13 blk -1 t1 bin 218 f0     109   no           B  7Z1AL RA3TPE LO25
20m_busy/test_26.wav      2518  11   162 blk 12 t0 bin 371 f0     19 blk 12 t0 bin 371 f0      38   no           B  RU0LL F5CCX -10
20m_busy/test_06.wav       448  10   145 blk  9 t0 bin  39 f1     22 blk  9 t0 bin  39 f1      30   no           B  CQ DG0OFT JO50
20m_busy/test_08.wav       763  10   120 blk 10 t0 bin  90 f0     28 blk 10 t0 bin  90 f0       7   no           C  UR7HN HB9BIN RR73
20m_busy/test_15.wav      1124  10   154 blk  9 t1 bin 148 f0     29 blk  9 t1 bin 148 f0      10   no           B  DG1BQC HB9CUZ RRR
20m_busy/test_36.wav      2200  10   145 blk  0 t0 bin 320 f0     28 blk  0 t0 bin 320 f0       9   no           B  BD8NBG RA3TPE 73
websdr_test7.wav           570  10   163 blk  5 t0 bin  59 f1     19 blk  5 t1 bin  59 f1      41   no           B  RA6FSD SP2EWQ -07
191111_110645.wav          906   9   174 blk  8 t1 bin 113 f0     38 blk  8 t1 bin 113 f0       1   DECODED  yes A  PA3EPP SP8NFO R+01
20m_busy/test_05.wav       987   9   142 blk -1 t1 bin 126 f0     18 blk -1 t1 bin 126 f0      62   no           B  TA1NGE RA3TPE LO25
20m_busy/test_01.wav      2390   8   155 blk 14 t1 bin 350 f0     25 blk -3 t0 bin 348 f1      19   no           B  CQ E75C JN93
20m_busy/test_13.wav      1087   8   147 blk  9 t1 bin 142 f0     23 blk  9 t1 bin 142 f0      26   no           B  CQ R7NO KN98
websdr_test9.wav          2229   8   170 blk-10 t0 bin 324 f1     25 blk-10 t0 bin 324 f1       6   DECODED  yes A  K4VBM HA8EK -15
20m_busy/test_01.wav      2279   7   119 blk 11 t0 bin 332 f1     21 blk 11 t1 bin 332 f1      42   no           C  PY2DPM ON6UF RR73
20m_busy/test_05.wav      2378   7   116 blk -3 t1 bin 348 f1     24 blk 14 t1 bin 350 f1      25   no           C  CQ SP9LKP JO90
20m_busy/test_06.wav      2519   7   111 blk -1 t0 bin 371 f1     23 blk 12 t0 bin 371 f0      29   no           C  SP4TXI F5CCX RR73
20m_busy/test_26.wav       517   7   148 blk 10 t1 bin  51 f0     22 blk 10 t1 bin  51 f0      19   no           B  BD8NBG S51SG JN76
websdr_test11.wav         2230   7   117 blk-10 t0 bin 324 f1     29 blk-10 t0 bin 325 f0       5   no           C  K4VBM HA8EK RR73
191111_110115.wav         1234   6   148 blk 10 t0 bin 165 f1     24 blk 10 t0 bin 165 f1       1   no           B  GJ0KYZ RK9AX MO05
20m_busy/test_17.wav      1560   6   151 blk -1 t1 bin 218 f0     21 blk  8 t0 bin 215 f0      36   no           B  7Z1AL RA3TPE LO25
20m_busy/test_23.wav      2089   6   164 blk 10 t0 bin 302 f0     22 blk  9 t1 bin 302 f0      29   no           B  <ZY50Y> IV3KVC JN65
websdr_test4.wav          1716   6   174 blk  5 t1 bin 242 f1     17 blk  6 t0 bin 242 f1      61   DECODED  yes A  SM2EKA UT7IS KN98
websdr_test6.wav          1716   6   174 blk  6 t0 bin 242 f1     33 blk  6 t0 bin 242 f1       4   DECODED  yes A  SM2EKA UT7IS -06
20m_busy/test_09.wav      1087   5   138 blk  9 t1 bin 142 f0     22 blk  9 t1 bin 142 f0      27   no           B  R3FO R7NO -16
20m_busy/test_21.wav      1124   5   153 blk  9 t1 bin 148 f0     31 blk  9 t1 bin 148 f0       6   no           B  DG1BQC HB9CUZ RRR
20m_busy/test_30.wav      2202   5   115 blk -1 t1 bin 320 f0     27 blk  0 t0 bin 320 f0      10   no           C  BD8NBG DJ2BW -15
20m_busy/test_35.wav      2520   5   142 blk 19 t0 bin 371 f1     18 blk 19 t1 bin 371 f1      63   no           B  F5CCX F4AGZ JN38
websdr_test9.wav          2199   5   141 blk 13 t1 bin 319 f1     18 blk 13 t1 bin 319 f1      40   no           B  F4DFQ F5LOW IN95
20m_busy/test_01.wav      1158   4   147 blk  9 t0 bin 153 f1     20 blk  9 t1 bin 153 f0      45   no           B  CQ HA1BF JN86
20m_busy/test_03.wav      1088   4   125 blk  9 t1 bin 142 f0     22 blk  9 t1 bin 142 f0      26   no           C  EA2DIC R7NO -25
20m_busy/test_05.wav      1088   4   128 blk  9 t1 bin 142 f0     25 blk  9 t1 bin 142 f0      18   no           C  EA2DIC R7NO -25
20m_busy/test_11.wav      1087   4   137 blk  9 t1 bin 142 f0     23 blk  9 t1 bin 142 f0      24   no           B  CQ R7NO KN98
20m_busy/test_25.wav      2378   4   113 blk -3 t1 bin 348 f1     24 blk 14 t1 bin 350 f0      24   no           C  CQ SP9LKP JO90
20m_busy/test_32.wav      2201   4   118 blk -1 t1 bin 320 f0     31 blk  0 t0 bin 320 f0       3   no           C  BD8NBG DJ2BW -15
websdr_test11.wav         2198   4   157 blk 13 t1 bin 319 f1     25 blk 13 t1 bin 319 f1      13   no           B  F4DFQ F5LOW IN95
20m_busy/test_05.wav      1158   3   150 blk  9 t1 bin 153 f1     17 blk  9 t1 bin 153 f0      78   no           B  CQ HA1BF JN86
20m_busy/test_06.wav       493   3   141 blk 10 t0 bin  47 f0     28 blk  9 t1 bin  47 f0       8   no           B  CQ 2E0LDW IO70
20m_busy/test_07.wav      1561   3   141 blk 16 t0 bin 218 f0     25 blk  3 t1 bin 218 f1      20   no           B  7Z1AL OK2BV JN89
20m_busy/test_36.wav      2201   3   126 blk 10 t0 bin 320 f0     28 blk  0 t0 bin 320 f0       9   no           C  BD8NBG DJ2BW -15
websdr_test13.wav         2218   3   126 blk  2 t0 bin 323 f0     21 blk  2 t1 bin 323 f0      14   no           C  K3ZK IK2ZDT RR73
websdr_test5.wav          1766   3   122 blk  5 t1 bin 250 f1     20 blk  5 t1 bin 250 f1      40   no           C  ON6OM DL8FBD 73
20m_busy/test_05.wav       955   2   132 blk  8 t0 bin 120 f1     29 blk  8 t0 bin 121 f0       3   no           B  CQ IU8DMZ JN70
20m_busy/test_07.wav       955   2   110 blk  8 t0 bin 120 f1     17 blk  8 t0 bin 121 f0      67   no           C  CQ IU8DMZ JN70
20m_busy/test_07.wav      1158   2   136 blk  9 t1 bin 153 f0     18 blk  9 t1 bin 153 f1      63   no           B  CQ HA1BF JN86
20m_busy/test_27.wav      1826   2   114 blk -8 t1 bin 261 f0     27 blk  5 t0 bin 260 f0      12   no           C  R4WZ ON6UF JO10
20m_busy/test_28.wav      1740   2   154 blk  9 t1 bin 246 f1     18 blk  9 t1 bin 246 f0      50   no           B  BD8NBG DL4SBF JN48
20m_busy/test_29.wav      1560   2   134 blk  3 t1 bin 217 f1     15 blk  3 t1 bin 217 f1      76   no           B  7Z1AL DF2FE JO51
websdr_test4.wav          2256   2   121 blk  7 t1 bin 329 f0     30 blk  3 t1 bin 327 f0       4   no           C  CQ DO8OL JO33
websdr_test5.wav          2746   2   147 blk  5 t1 bin 407 f1     26 blk  5 t1 bin 407 f1      10   no           B  SP2EWQ DL8TG R+07
websdr_test5.wav          1874   2   121 blk -7 t0 bin 267 f1     26 blk -7 t1 bin 268 f0       9   no           C  OH1WR RA4UDC RR73
websdr_test8.wav          1219   2   119 blk  9 t0 bin 163 f0     37 blk  9 t0 bin 163 f0       1   no           C  SM5NAS OZ0JD RR73
20m_busy/test_05.wav      1561   1   136 blk 16 t0 bin 218 f0     26 blk  3 t1 bin 218 f1      13   no           B  7Z1AL OK2BV JN89
20m_busy/test_14.wav       456   1   120 blk 10 t1 bin  41 f0     34 blk 10 t1 bin  41 f0       2   no           C  SP4TXI ON2RK RR73
20m_busy/test_19.wav      1089   1   155 blk 10 t0 bin 142 f0     22 blk 10 t0 bin 142 f0      32   no           B  CQ R7NO KN98
20m_busy/test_21.wav      1652   1   132 blk  7 t1 bin 232 f0     24 blk  7 t1 bin 232 f1      20   no           B  CQ RX6DA KN85
20m_busy/test_23.wav      2378   1   111 blk -2 t0 bin 348 f0     25 blk 14 t1 bin 350 f0      20   no           C  CQ SP9LKP JO90
20m_busy/test_25.wav       793   1   129 blk 11 t0 bin  95 f0     20 blk 11 t0 bin  95 f0      46   no           C  JA1FWS F8BBL R-15
20m_busy/test_31.wav      2378   1   105 blk  3 t0 bin 349 f1     27 blk 14 t1 bin 350 f0      13   no           C  ES1KK SP9LKP -13
20m_busy/test_35.wav       955   1   122 blk  8 t1 bin 120 f1     23 blk  8 t1 bin 120 f1      26   no           C  CQ IU8DMZ JN70
websdr_test13.wav         1618   1   110 blk 15 t1 bin 229 f1     33 blk -1 t0 bin 227 f0       2   no           C  HNY 2019 73
websdr_test4.wav          1992   1   120 blk  6 t0 bin 286 f1     26 blk  6 t0 bin 287 f0      14   no           C  RW6FY OM7ZM RR73
20m_busy/test_05.wav      1862   0   143 blk  9 t0 bin 266 f0     25 blk  9 t0 bin 266 f0      16   no           B  CQ IZ5ILK JN63
20m_busy/test_11.wav      1285   0   144 blk  5 t0 bin 173 f1     24 blk  5 t0 bin 173 f1      19   no           B  CQ 4U1A JN88
20m_busy/test_21.wav       990   0   108 blk 15 t1 bin 124 f0     26 blk  9 t0 bin 126 f1      13   no           C  YC6RMT IZ7NLM -22
20m_busy/test_29.wav       955   0   138 blk  8 t1 bin 120 f1     22 blk  8 t1 bin 120 f1      29   no           B  CQ IU8DMZ JN70
20m_busy/test_29.wav      2378   0   105 blk -3 t0 bin 347 f0     24 blk 14 t1 bin 350 f0      22   no           C  ES1KK SP9LKP -13
websdr_test5.wav          1822   0   103 blk -3 t1 bin 261 f1     17 blk  5 t0 bin 259 f1      54   no           C  DB4BU DK5OK RR73
websdr_test7.wav          1765   0   101 blk  5 t1 bin 250 f0     15 blk  5 t0 bin 250 f1      72   no           C  CQ DL8FBD JO40
```

### The thirteen recurring texts as a group

```
 times    snr range     outcomes  text
     4    0 to   2         BBCC  CQ IU8DMZ JN70
     4    1 to  13         CCCC  CQ SP9LKP JO90
     3    2 to   4          BBB  CQ HA1BF JN86
     3    1 to   8          BBB  CQ R7NO KN98
     3    3 to   5          CCC  BD8NBG DJ2BW -15
     2    4 to   4           CC  EA2DIC R7NO -25
     2    1 to   3           BB  7Z1AL OK2BV JN89
     2    5 to  12           BB  R3FO R7NO -16
     2    5 to  10           BB  DG1BQC HB9CUZ RRR
     2    6 to  11           BB  7Z1AL RA3TPE LO25
     2   17 to  17           CC  LU5HA 9A9A RR73
     2    0 to   1           CC  ES1KK SP9LKP -13
     2    4 to   5           BB  F4DFQ F5LOW IN95

  texts appearing more than once                 : 13
  lines belonging to one                         : 33
  of those texts, ALL their lines in one outcome : 12
  scattered across outcomes                      : 1
```

**Twelve of thirteen fall entirely within one outcome.** A station missed in file after file at the
same frequency is a property with a cause, and the cause is the same one every time — which is what a
signal-level explanation predicts and what a random-draw explanation does not.

### Divergence 22

```
  lines that met it : 0
```

**No line's neighbourhood met the passband refusal**, in either population — 169 lines and 101 400
alignment points. The highest frequency any of these lines names is 2746 Hz and the passband reaches
3000, so the eighth tone never leaves it. **Reported, not fixed.**

### The control group, in its own block

**A reader must be able to see the instrument answer correctly on a known answer before he is asked
to believe it on an unknown one.** All three checks ran before a single miss was swept.

**Check one — it finds what is there.** 12 expected lines the untold path already matched, swept
identically:

```
  lines swept                       : 12
  THE SWEEP FOUND A DECODING POINT  : 12
  found none                        : 0
  mean best agreement of 174        : 170.2
  lowest best agreement             : 156
  mean best sync score              : 24.6

  WHERE THE DECODING POINT SAT, relative to the centre the list's frequency gave:
    bin offset from centre  count
                         0     12
```

```
file                    list Hz   snr  best agree               decoded at             text
191111_110130.wav           683    -6  170 blk  8 t1 bin  77 f0  blk  8 t1 bin  77 f0  CQ TA6CQ KN70
191111_110130.wav           989   -16  162 blk 10 t0 bin 126 f1  blk 10 t0 bin 126 f1  OH3NIV ZS6S -03
191111_110130.wav          1291    -6  174 blk  9 t1 bin 174 f1  blk 10 t0 bin 174 f1  CQ R7IW LN35
191111_110145.wav          1234     7  174 blk 10 t1 bin 165 f1  blk 10 t1 bin 165 f1  GJ0KYZ RK9AX MO05
191111_110200.wav           683    -4  174 blk  8 t1 bin  77 f1  blk  8 t1 bin  77 f1  CQ TA6CQ KN70
191111_110200.wav          1031   -17  156 blk  8 t0 bin 133 f0  blk  8 t0 bin 133 f0  CQ LZ1JZ KN22
191111_110200.wav          1292   -12  172 blk 10 t0 bin 174 f1  blk 10 t0 bin 174 f1  CQ R7IW LN35
191111_110215.wav           996   -12  173 blk  5 t0 bin 127 f1  blk  5 t0 bin 127 f1  GJ0KYZ UA6HI -15
191111_110215.wav          1235     2  174 blk  9 t1 bin 165 f1  blk  9 t1 bin 165 f1  GJ0KYZ RK9AX MO05
191111_110615.wav           431    -2  174 blk 10 t0 bin  37 f0  blk 10 t0 bin  37 f0  VK4BLE OH8JK R-17
191111_110615.wav           539   -14  172 blk 10 t0 bin  54 f0  blk 10 t0 bin  54 f0  RK6AH JH1AJT -05
191111_110615.wav           656   -18  168 blk  9 t1 bin  73 f0  blk  9 t1 bin  73 f0  PA3EPP SP8NFO KN09
```

**Check two — it refuses what is not there, and this bears on criterion 2.** Ten quiet
neighbourhoods, the *same* true codewords, at frequencies the expected list places nothing within
30 Hz of:

```
  quiet neighbourhoods swept        : 10
  MESSAGES RETURNED                 : 0   <- criterion 2, and it must be zero
  true codewords recovered          : 0
  points swept in each              : 600

  mean best agreement               : 110.1 of 174
  HIGHEST best agreement anywhere   : 115 of 174
  lowest best agreement             : 106 of 174
  mean best sync score              : 9.9

file                    quiet Hz  best agree  best score  decoded  codeword asked for
191111_110130.wav            400  109         8           no       CQ TA6CQ KN70
191111_110130.wav            537  107         9           no       OH3NIV ZS6S -03
191111_110145.wav            400  106         8           no       GJ0KYZ RK9AX MO05
191111_110145.wav            537  112         7           no       GJ0KYZ RK9AX MO05
191111_110200.wav            400  112         8           no       CQ TA6CQ KN70
191111_110200.wav            537  109        10           no       CQ LZ1JZ KN22
191111_110215.wav            400  111         8           no       GJ0KYZ UA6HI -15
191111_110215.wav            537  112         9           no       GJ0KYZ RK9AX MO05
191111_110615.wav            400  115         8           no       VK4BLE OH8JK R-17
191111_110615.wav            948  108        24           no       RK6AH JH1AJT -05
```

**This is the null the night actually needed, and it is not unit 218's 84.8.** That figure is a
*one-point* chance measurement. **The best of six hundred correlated points is a different and higher
statistic**, and on empty air it runs **106 to 115, mean 110.1**. Every reading in the outcome table
is against that. **The B bound of 130 sits 15 above the highest the null ever reached.**

**Check three — it agrees with the instrument this phase already had.** At the nearest kept candidate,
the one place both instruments read:

```
  lines compared : 12
  EQUAL          : 12
  differing      : 0

file                   list Hz theirs   mine verdict  text
191111_110115.wav         1234    148    148   equal  GJ0KYZ RK9AX MO05
191111_110130.wav         2479    149    149   equal  TK4LS YC1MRF 73
191111_110200.wav          990    113    113   equal  OH3NIV ZS6S RR73
191111_110215.wav         2059    148    148   equal  CQ DX Z33Z KN11
191111_110615.wav          906    174    174   equal  PA3EPP SP8NFO KN09
191111_110615.wav          906    174    174   equal  PA3EPP SP8NFO KN09
191111_110615.wav         1049    105    105   equal  CQ UB3AQS KO85
191111_110630.wav          840     90     90   equal  CQ OR18RSX
191111_110630.wav         1114    143    143   equal  CQ JR5MJS PM74
191111_110645.wav          906    174    174   equal  PA3EPP SP8NFO R+01
191111_110645.wav         1201     93     93   equal  G1XJM HA7JIV JN97
191111_110645.wav         2092    107    107   equal  WB2QJ ES3AT KO18
```

`Ft8MissAccountingTests` has its own private extraction and its own private nearest-candidate rule;
`AlignmentSweep.AgreementAt` is this unit's, written separately. **Equality is two implementations
agreeing rather than one implementation agreeing with itself.** This check is also what exposed the
repeated-line reading that decides what an outcome A means: `191111_110615.wav` at 906 Hz agrees at
**174 of 174** and is *still* counted a miss.

### Task 4's four readings

**Reading 1 — the score at the true point, against the search's own threshold.**

```
  outcome A and B lines                     : 40
  mean best sync score in neighbourhood     : 23.4
  lowest                                    : 13
  BELOW DefaultMinimumScore of 10           : 0
  the search kept NO point in the sweep     : 0
```

The kept-candidate score distributions in those same slots run from **10 or 11 at the bottom** to
**31 to 38 at the top**, and the sweep kept **three to twelve** neighbourhood points per line at best
ranks from **1 to 109**. **Not one recoverable or present transmission is scoring below what the
search keeps. The search is not the stage, and no threshold argument survives that.**
`DefaultMinimumScore` and `DefaultCandidateLimit` were **read and not changed, not swept, and not
proposed as a fix** — the park is narrowed for reading only.

**Reading 2 — the message limit, and nobody in this phase had ever read it.**

```
  recordings                                : 60
  RECORDINGS THAT RETURNED EXACTLY 50       : 0
  most messages any recording returned      : 20
  most expected lines any list carries      : 34
```

**`DefaultMessageLimit` of 50 is nowhere near being reached.** The limit that *is* saturated is
`DefaultCandidateLimit`, at **140 in 52 of the 60 recordings** — and unit 216 already swept that at
140, 280, 560 and 1120 and it bought nothing, so it is reported and left parked. The busiest files by
expected count are `20m_busy/test_21.wav` at 34 expected and 15 returned, and
`20m_busy/test_05.wav` and `test_35.wav` at 32 expected and 19 and 13 returned.

**Reading 3 — where the 78 sit in the passband, against the 752 matched lines.**

```
      band, Hz  the misses  the matched  miss share %
        0- 500           3           91           3.2
      500-1000          12          146           7.6
     1000-1500          15          169           8.2
     1500-2000          17          140          10.8
     2000-2500          27          151          15.2
     2500-3000           4           55           6.8
         TOTAL          78          752
```

**They do not cluster at one end.** The share rises from 3.2 per cent at the bottom of the passband to
15.2 per cent in 2000–2500 Hz and falls again to 6.8 at the top — a mild concentration around the
2000–2500 Hz band, which is where `9A9A` at 2046, `SP9LKP` at 2378 and `E75C` near 2389 all live, and
not a passband-edge effect.

**Reading 4 — the one recording that produced nothing, and it is a much smaller thing than it
sounded.**

```
  expected lines it carries    : 1
  candidates the search found  : 24
  BEST SYNC SCORE ANYWHERE     : 24
  candidates reaching parity   : 0
  messages returned            : 0
  best score in the other 59   : 34.9 mean, 26 lowest
```

**`191111_110115.wav` carries exactly one expected line.** A whole file returning nothing is therefore
one missed message and not a file-wide failure, and the phrase has been carried for three units
sounding larger than it is. Its best sync score of **24 is below the lowest best score of any other
recording (26)**, so it is measurably the weakest file in the set. Its single expected line,
`GJ0KYZ RK9AX MO05` at 1234 Hz and +6 dB, is one of the 78 and **the sweep put it in outcome B at
agreement 148 of 174, at the point the search ranked first.** The signal is there and the ratios are
too damaged to correct. **That file is now fully accounted for.**

### Task 5's split, kept separate from the 78's

```
THE ADDITIONAL LINES FROM -5.0 dB UP TO BUT NOT INCLUDING 0.0 dB

  lines swept                                    : 91
  A  PRESENT AND RECOVERABLE                     : 0
  B  present and not recoverable                 : 33
  C  not present as far as this receiver can see : 58

  points swept per line                          : 600
  belief propagations run in all                 : 1991

  lines whose text the untold path DID return    : 0
  OUTCOME A LINES THAT ARE NOT A REPEATED LINE   : 0

  best agreement over the population, mean       : 127.1 of 174
  highest                                        : 161
  lowest                                         : 103

agreement     lines
100-109          12
110-119          24
120-129          22
130-139          10
140-149          14
150-159           8
160-169           1
170-179           0

  HIGHEST agreement among the C lines : 129
  LOWEST agreement among the B lines  : 130
```

**78 plus 91 is 169, which is unit 218's figure exactly**, so the two populations reconcile against
the number they came from. The mean best agreement of **127.1 against the 78's 133.2**, and the
highest of **161 against 174**, are the same shape one rung weaker — which is what a genuinely weaker
population should look like and is itself a check on the instrument. **Here the bound is adjacent
rather than in a gap** — highest C 129, lowest B 130 — **and that is stated rather than hidden**: for
this population the B/C boundary is a cut through a continuum, and its two counts should be read as
approximate where the 78's should not.

The recurring texts in this population behave differently and that is worth noting: **only 3 of 11
repeated texts fall entirely in one outcome and 8 scatter**, against 12 of 13 in one outcome for the
78. That is what a population sitting on the boundary looks like.

```
 times    snr range     outcomes  text
     7   -5 to  -2      BBBCCCC  CQ OE8GMQ JN66
     6   -5 to  -1       BBBBBC  CQ HA1BF JN86
     3   -3 to  -1          BCC  CQ IU8DMZ JN70
     2   -4 to  -4           BC  CQ RX6DA KN85
     2   -4 to  -1           BC  JA1FWS OK2BV R-13
     2   -4 to  -4           CC  YC6RMT IZ7NLM -22
     2   -2 to  -2           BC  SP5QAC F5UOU -11
     2   -2 to  -1           CC  ZL2OK DL1KDA R-24
     2   -3 to  -2           CC  9A9A SP9LKP JO90
     2   -4 to  -3           BC  CQ E74BYZ JN84
     2   -5 to  -4           BC  SM2EKA SV9FBN KM25
```

**Which branch of task 5's drop condition applied, with the counts that decided it.** The drop is
licensed **only** where the 78 land at least two thirds in a single outcome. They land **5, 35 and
38**, so **the largest single outcome is 38 of 78, 48.7 per cent, against a bar of 66.7 per cent.**
**The drop branch did not apply and the task ran in full.**

**Across both populations together: 169 lines — 5 A, every one a repeated expected line; 68 B; 96 C.**

### The neighbourhood, fixed in task 2 and not widened

```
  block offsets      : -10 to 19 inclusive, the SEARCH'S OWN range - 30 values
  time sub-offsets   : both, 0 and 1
  bins either side   : 2, which is 2 whole FT8 tone spacings - 5 bins
  frequency sub-offs : both, 0 and 1, so the frequency step is half a tone
  POINTS PER LINE    : 600
```

**Why that span.** The block range is the search's own, so the sweep reaches every alignment the
search could have proposed and no more. Two bins either side with both sub-offsets reaches about
**15.6 Hz** either way, against the **four hertz** every previous unit tested at — a signal the list
has placed two whole tones away is still inside the sweep.

**The decode rule, stated before the run.** Score and agreement at all 600 points; belief propagation
at the **20 best-agreeing points plus every point in the neighbourhood the search itself kept**, so
the sweep can never miss a decode the untold path could have had. **1712 propagations over the 78 and
1991 over the 91.** **It was not widened at any point**, and nothing in the neighbourhood or the rule
changed between the control group and either sweep.

### Whether a fix was licensed

**No fix was licensed, and that is an acceptable outcome and it is not a failure.**

**Which condition failed, and it failed on the sharpest evidence this phase has produced.**
**Condition 2 fails outright.** A fix must be a **fidelity** fix restoring what the pinned `ft8_lib`
does. The only route the night opened is outcome A — and all five outcome-A lines are expected lines
their own list carries twice, whose message the untold path **returned** and then de-duplicated by
**upstream's own payload rule**, which unit 216 proved this library keeps. To turn those five into
matches, the library would have to **stop de-duplicating**, which is a divergence *from* the pin
rather than toward it and would **put the same message on Tim's screen twice**. **Condition 1 is
half-met at best**: the night names a target and names no change. Conditions 3 and 4 do not arise.

**Cross-checked independently.** `Ft8MissAccountingTests.TheSevenMessagesBetweenUnit216sTwoNumbersAreRepeatedExpectedLines`
names the same groups without knowing anything about tonight's sweep:

```
    191111_110615.wav      x3  RETURNED         PA3EPP SP8NFO KN09
    191111_110645.wav      x2  RETURNED         PA3EPP SP8NFO R+01
    websdr_test11.wav      x3  never came back  K4VBM HA8EK RR73
    websdr_test4.wav       x2  RETURNED         SM2EKA UT7IS KN98
    websdr_test6.wav       x2  RETURNED         SM2EKA UT7IS -06
    websdr_test9.wav       x3  RETURNED         K4VBM HA8EK -15
```

The five that RETURNED are exactly the five outcome-A lines. The one that never came back —
`websdr_test11.wav`, `K4VBM HA8EK RR73` at 2230 Hz — is **outcome C at agreement 117**, which is the
consistent answer.

**Criterion 3 was therefore not re-taken after a change, because there was no change.** It was
measured in task 1 through `TheReferenceRecordingsDecodeAgainstUpstreamsOwnExpectedLists` unchanged
and stands at **760 matched of 1298, 538 missed, 23 extra — the same 23.**

### The prohibition, stated in these terms

**Nothing from the sweep was added to any total.** The sweep is told the file, the frequency and the
text, and that is what makes it able to answer the question. **No expected text, frequency, count or
list reached `Ft8SlotDecoder`, `Ft8SoftSymbols`, `Ft8SyncSearch` or `Ft8CodewordDecoder` on any path
whose output is counted.** In every sweep the untold path ran first and its candidate list and its
returned texts were **read, not filtered, re-ordered or re-scored**; the expected text built a
comparison codeword and the expected frequency chose where to centre the sweep, both after the fact.
**A point at which the sweep recovers the expected text is evidence that the transmission is present
and it is nothing else** — not a decode, not a match, and no part of criterion 3's 760, which is
re-taken only through the untold path unchanged.

### `HM-OPEN-066`, extended and its routes re-ranked

**Extended, not duplicated** — no second id was created. It gains a unit-219 section carrying the
split, the control group, what the sweep found at the true alignments, and its four routes re-ranked:

1. **`decode_ft8.exe`** — **from third to first.** Tonight established that 96 of 169 strong misses
   are not present as far as *this* receiver can see. **The one remaining question is whether they
   are present as far as the pin can see**, and nothing else can answer it.
2. **The 82 placeholder messages** — unchanged at second. Reserved to the owner under `CLAUDE.md`
   §12.1 and in front of him from unit 217.
3. **The 2 dB** — from fourth to third. Step 6's to make as a verdict; tonight's 68 outcome-B lines
   are the population two rungs of sensitivity would act on.
4. **The 78** — **discharged, and marked so rather than deleted**, so the next reader can see the
   route was taken rather than dropped.

### Divergences and notes added

**No divergence added, and none was expected** — nothing was ported and no library file changed. The
count stands at **twenty-five**. `src/Ft8Sharp/porting-notes.md` gains its unit-219 section: the
instrument, the fixed neighbourhood and decode rule, the three-check control table, the outcome table,
the four free readings, **what this measurement is evidence about**, and **five things it is not, with
step 6 named first.**

### Versions, tests and the tree

**Versions.** `src/Ft8Sharp/Directory.Build.props` **0.10.2 → 0.10.3** under HM-DEC-152 — measurement
only, the library gains evidence rather than a capability, and the props file itself now carries the
line that **nothing in 0.10.3 is a step 6 result**. Root `Directory.Build.props` **1.12.25 → 1.12.26**
under HM-DEC-150.

**Re-run after both bumps.** `Ft8Sharp` **502 total, 501 passed, 0 failed, 1 skipped** in 1 m 39 s —
**six tests added** against entry's 496, and still the one correct skip,
`Ft8TableGenerationTests.RewriteTheCheckedInTablesFile`, the table write gate. **The clone is present
on this machine, so no reference test skipped and every measurement above is real rather than
absent.** Library **0 warnings, 0 errors**.

**Channels, with the filter strings so the next unit inherits a filter rather than a number:**

```
RadioEngine, 55 green:
  --filter "FullyQualifiedName~AudioSeamTests|FullyQualifiedName~PrivilegeTests"
App, 9 green, VersionTests among them:
  --filter "FullyQualifiedName~DecisionLogOrderTests|FullyQualifiedName~VersionTests|
            FullyQualifiedName~EveryResourceKeyResolvesTests|FullyQualifiedName~ViewTestsActThroughControlsTests"
```

**Attribution.** **180** paths from `2828ab6` at entry and **183** at exit, the three added being this
unit's own test files, and the `src/Hamlet.`/`tests/Hamlet.` filter **returns 0** at both ends.

**The tree.** **8 `.obj` at the repository root**, counted at the end and untouched — not committed,
not deleted, not read, not linked, not run and not reasoned from. **`tools\build-ft8-oracle.bat` is
present, untracked, and was not run**; nothing tonight needed a compiler. The known probe and scratch
files are all still on disk and no attempt was made to remove any of them —
`tests/Ft8Sharp.Tests/TempEncoderProbe.cs` tracked, and untracked `UpstreamSyncSearchProbe.cs`,
`UpstreamLdpcProbe.cs`, `Unit216Probe.cs`, `Unit217Probe.cs`, `unit215-section.md`,
`unit216-section.md` and `unit217-status.py`. **`PHASE_STATUS.md` is stale and `PHASE_OUTCOME.md`'s
header disagrees with its own entries; both are the loop's, neither was hand-edited beyond the one
`WORK_INSTRUCTION:` line the session prompt directed, and neither was committed.**
`git status --short` printed **34 at entry**; the loop's own uncommitted files among them are
`tools/arbiter/run-phase.bat` and `run-unit.bat` modified, and untracked `ARBITER.md`, `MANIFEST.txt`,
`PHASE_PLAN.md`, `RUN_LEDGER.md`, `VERIFY_PASS.md`, `SCRUB_SELFTEST.bat`, `.run-unit/`,
`docs/phase-uplift/`, `tools/unit217/` and `SESSION.lock`. **None was committed.**

### The validator

`tools\arbiter\validate-output.bat` was attempted in **all five** spellings
`tools\arbiter\run-unit-tools.txt` lists and **none of them validated**. Three —
`tools\arbiter\validate-output.bat`, `cmd //c …` and `cmd.exe //c …` — died with
`toolsarbitervalidate-output.bat: command not found`, because Git Bash strips the backslashes out of
the path before the command is ever run. The other two — `cmd /c …` and `cmd.exe /c …` — were
permitted and **opened an interactive `cmd` shell instead**, for the same reason: with the
backslashes gone there is no batch file left to run. **This is the ninth consecutive unit.** It is
reported as a refusal in section 4 and **it was not routed around** — no alternative interpreter, no
copy of the script under a different name, and no reimplementation of its checks.

**The six rules were therefore checked by hand against the script's own source**, which was read and
not edited. Its rules are: (1) a parseable `UNIT:` line above section 1; (2) and (3) the four
top-level `##` sections in order with exact names and no fifth; (4) `## 4. What's blocking us`
present even when empty; (5) section 3 non-empty; (6) the ordering block above the `UNIT:` line, in
the first 60 lines, carrying a `READ IN THIS ORDER` header, an `A.`, a `B.` and a `C.` each at the
start of a line, and a phrase matching `raises \d+ item`. **Measured against this file:** `UNIT: 219`
at line 53 and `## 1.` at line 60; the four sections at lines 60, 105, 143 and 739 with no fifth `##`
anywhere; section 3 running to just under six hundred lines; and the ordering block at lines 1, 3, 14
and 36 with
*Section 4 raises 2 items* at line 49 — **every one of them above the `UNIT:` line and inside the
first 60. All six pass.**

### Mismatches against the instruction, reported and not repaired

1. **`git status --short` printed 34, not 33.** The instruction records 33 from the arbiter's own
   measurement and separately notes that unit 218 reported 34 including `SESSION.lock`. **34 is what
   this session measured at entry**, and `SESSION.lock` is present because a session is running.
   **Reported, not repaired.**
2. **`191111_110115.wav` carries one expected line, not many.** The instruction calls it *the one
   recording that produced nothing* and asks for its expected line count — the count is **1**, so the
   phrase, carried through units 216 and 218, describes one missed message rather than a file-wide
   failure. **Reported, not repaired**, and the file is now accounted for in reading 4.
3. **The instruction's `9A9A` example is a frequency grouping, not a text grouping.** It says *`9A9A`
   at 2046 Hz missed in four files at +17 and +19 dB*, which is true at the station level — four
   lines at 2046 Hz — but the four are three distinct texts (`RA9UJP 9A9A RR73`, `LY3BES 9A9A RR73`
   and `LU5HA 9A9A RR73` twice), so only one of them appears in the thirteen recurring **texts**.
   **Reported, not repaired.**
4. **Everything else the instruction asserted about the tree checked out.** `HEAD` `3177e41`;
   `Directory.Build.props` at 1.12.25 and `src/Ft8Sharp/` at 0.10.2 without importing the root; 8
   `.obj`; 25 divergences; nine files in `src/Ft8Sharp/Dsp/` and four in `Ldpc/`; `ScoreAt` public
   with the stated signature; `Ft8Candidate` a record struct of those five fields; `Extract` taking a
   waterfall, a candidate and 174 ratios; `DefaultFirstBlockOffset` −10, `DefaultLastBlockOffset` 19,
   `DefaultMinimumScore` 10, `DefaultCandidateLimit` 140, `DefaultMessageLimit` 50; the geometry's
   200–3000 Hz passband, `MinBin` 32, `MaxBin` 481, `BinCount` 449 and both oversampling factors 2;
   `Ft8MissAccountingTests` already re-encoding through `LdpcEncoder.Encode` and parsing `fields[1]`
   and `fields[3]`; unit 218's six test files present with `Ft8OnAirSnrJoinTests` producing the 78;
   and unit 218's exit figures of 496/495/0/1, 180 attribution paths, and channels 55 and 9, **all
   re-measured and all reproduced.**

## 4. What's blocking us

**Two items. Neither is a ruling request, and one bears directly on criterion 3 in section B.**

**1. The 78 are not overwhelmingly outcome C, but the thing that matters is that none of them is
outcome A in the sense that would have been a defect — and that is the evidence criterion 3 would be
cut down against a measured bar.** The instruction asked for this to be said in section 4 in one
paragraph with the count, so that the arbiter does not have to infer it from a table. **Of the 78
strong-SNR misses, 38 are not present as far as this receiver can see and 35 are present and beyond
this code's correcting power; the remaining 5 are expected lines their own list carries twice, whose
message this library returned once and correctly de-duplicated. Over the wider 169, the figures are
96, 68 and 0.** **Not one of the 169 is a transmission this library could have recovered and threw
away**, and the search kept a candidate in every one of the 40 neighbourhoods where a transmission
was present. **There is no defect with an address in this port at the strong end of criterion 3's
residue.** The one qualification, and it is stated rather than buried: an outcome-C line could in
principle be one whose text this library packs to *different bits* than were on the air, which unit
217 measured at 76 of 2263 — 3.4 per cent — on messages this library itself decoded, so of the order
of three lines in 78 and not thirty-eight. **This is not blocking anything and it is not a decision
request; it changes what the rest of this phase is about, and the next unit should not have to find
it in a table.**

**2. `tools\arbiter\validate-output.bat` did not run, for the ninth consecutive unit.** All five
spellings `tools\arbiter\run-unit-tools.txt` lists were attempted: three failed with
`toolsarbitervalidate-output.bat: command not found` and two were permitted but opened an interactive
`cmd` shell — **in every case because Git Bash strips the backslashes out of the path before the
command runs**, which is the same cause units 211 through 218 recorded. **It is reported as a refusal
and it was not routed around** — no alternative interpreter, no copy of the script, no
reimplementation of its checks. **The six rules were checked by hand against the script's own source
instead and all six pass**, with the line numbers named in section 3. **This is an environment fact
rather than a finding, it does not need re-arguing at length, and it is the owner's to look at if he
wishes.**
