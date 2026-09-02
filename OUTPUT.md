READ IN THIS ORDER — A the phase, B the step and its exit criteria, C this report against both.

A. PHASE — Hamlet hears FT8 off the radio and displays the decoded text on screen. Seven steps.
Steps 1 and 2 closed. Step 3 closed on its four must-pass criteria, its nice-to-pass one recorded as
HM-OPEN-065. Step 4 closed by unit 214. **Step 5 is this unit's and its fourth**, entered at 2 of its
3 subject criteria with criterion 3 partial at 760 of 1298 against a ceiling of 1157, and **leaves at
760 of 1298, still partial.** Steps 6 and 7 not started and cannot start, because every step of this
plan depends on the one before it — the plan's own named deviation. **Step 5 was the only step this
phase could move.** **This unit borrowed step 6's instrument as step 5's diagnostic and did not take
step 6's criteria**, which is stated plainly here and again in section 2 because a report that leaves
step 6 looking done would do the phase harm.

B. STEP 5 — a found signal becomes a message. Five exit criteria.
1. Corrupted codeword recovered, one beyond the power failing honestly — met by unit 215. **Tonight
   gives it a decibel value for the first time**: at -21 dB the mean hard-decision agreement at the
   right candidate is 140.4 of 174, thirty-four bit errors, against a code whose recovery unit 215
   measured reaching zero at seventeen. The code is behaving exactly as measured.
2. A candidate failing CRC is never returned — met by unit 216 at four counts of zero. **Tonight is
   the strongest test of it ever taken: 18 slots of pure noise returned 0 messages**, from 183
   candidates, at three amplitudes and six seeds. **0 wrong messages in 2416 further trials.**
3. `ft8_lib`'s reference WAVs decode against its expected decode lists — **entered at 760 of 1298,
   leaves at 760 of 1298. PARTIAL, not met.** **The count did not move and no fix was licensed**, which
   is the outcome the instruction named as expected. It moved from *unexplained* to *measured against
   physics*, and the measurement did not come back one-sided — see C.
4. `Ft8Sharp` green — entry **485/484/0/1**, exit **496/495/0/1**, the one skip the table write gate.
5. Attribution clean and channels green — **180** paths from `2828ab6`, **0** under `src/Hamlet.` or
   `tests/Hamlet.`, channels **55** and **9** re-run after both version bumps. Filter strings in
   section 3.

C. THIS REPORT — 9 findings, and the lead is the collapse rung. **This path returns every message
down to -18.0 dB and stops between -18.0 and -21.0 dB** — 100 per cent at -18.0, **25.0 per cent at
-20.0**, **3.8 per cent at -21.0**, 0.0 at -22.0 and below, 50 per cent crossing near -19.
**WRONG MESSAGES RETURNED: 0 out of 520 on the aligned ladder, 0 out of 1070 on the impaired ones, 0
out of 306 in the anatomy — and 0 messages out of 18 slots of pure noise.** **MISSED EXPECTED LINES
AT 0 dB OR BETTER BY THE LIST'S OWN COLUMN: 123, of which 45 are hashed, so 78 A RECEIVER COULD HAVE
MATCHED — two of them at +19 dB — and 169 at -5 dB or better.** **Which outcome the measurements
chose: NONE CLEANLY, and the report says ambiguous rather than picking one.** Outcome 2, a deaf
receiver, is ruled out; outcome 3's three tested impairments are ruled out at under a rung each;
outcome 1 is supported by the agreement calibration and contradicted by the join. **Task 5c's drop
condition: the FIRST branch applied — 5a and 5b agree with the aligned ladder within about one rung,
so 5c was droppable — and it was run anyway** because it costs 30 slot decodes against their 1040.
**Section 4 raises 2 items and NEITHER is a ruling request; one bears on criterion 3.**

UNIT:       218 — complete at task 7 of 7 — 2026-09-02 12:53
PHASE GOAL: Hamlet hears FT8 off the radio and shows the decoded text on screen.
UNIT GOAL:  Measure the signal-to-noise ratio at which this whole path stops returning messages, on
            signals it makes itself where the truth is exact, and join that to the SNR column
            upstream's own expected lists carry on all 1298 of their lines.
ADVANCED:   no — the ladder is measured and the count did not move, which is what the instruction
            named as the expected answer. No criterion newly met. Criterion 3 stays partial at 760
            of 1298. Criteria 1 and 2 gained their strongest evidence yet without changing status.
NUMBER:     760 of 1298 -> 760 of 1298 — unchanged, and unchanged on purpose: no library file was
            touched, nothing was tuned, and no fix met all four of task 7's conditions.
DRIFT:      1 consecutive unit without advance (was 0) — and it is drift by design rather than
            thrash: the instruction said in terms that the count was not expected to move and that
            the unit was not being judged on whether it did.

## 1. What Claude did

**Complete, at task 7 of 7. Nothing dropped.** Task 5c was the named drop candidate, its drop
condition was satisfied, and it was run anyway — the numbers that decided it are in section 3.
Machine `C:\Source\HamLet`, project **Hamlet confirmed** against all four checks in the instruction's
gate, branch `main`, HEAD `bbb8ae1` at exit.

**No library file changed tonight.** `src/Ft8Sharp/` gained a version bump and a `porting-notes.md`
section and nothing else. Every number below came out of tests added under `tests/Ft8Sharp.Tests/`.

### The gate

`git ls-files --error-unmatch` returned both `SHACK_FACTS.md` and
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs`; `ls *.sln` returned `Hamlet.sln` and nothing
else; `CoreHMI.sln` and `MURC.sln` both absent. All four hold.

### Task 1 — the ground, and the two runs everything tonight is joined to

`Ft8Sharp` at entry **485 total, 484 passed, 0 failed, 1 skipped** in 38 s. The skip is
`Ft8TableGenerationTests.RewriteTheCheckedInTablesFile`, **the table write gate**, whose reason names
the environment variable that would run it — it is meant to skip. **The pinned clone is present on
this machine, so no reference test skipped for its absence** and every measurement below is real
rather than absent. Library builds **0 warnings, 0 errors**.

**`git diff --name-only 2828ab6..HEAD` returned 174 paths at entry** and the
`src/Hamlet.`/`tests/Hamlet.` filter returned **nothing**. Channels: **55** RadioEngine and **9** App,
both green, under the filters unit 217 reconstructed and this unit inherited rather than re-invented
— they are printed in section 3 so the next unit inherits a filter rather than a number.

`HEAD` **`15c9364`** on `main` at entry. **`git status --short` printed 34**, not the 33 the
instruction states — reported and not repaired. Versions **1.12.24** and **0.10.1**. **8 `.obj`** at
the root and **25 numbered divergences** in `porting-notes.md`.

**Then the two runs.** `TheReferenceRecordingsDecodeAgainstUpstreamsOwnExpectedLists`, re-run
unchanged, **reproduced every total column for column**: 60 files measured, 0 skipped for a rate,
**7803 candidates, 2733 parity, 2733 checksum, 2263 text, 783 unique, 1298 expected, 760 MATCHED, 538
missed, 23 EXTRA** — and the same single file producing nothing, `191111_110115.wav`, candidates found
and none reaching parity. `TheCorpusComesBackInSeededNoiseAtAMeasuredRatio`, re-run unchanged, still
returns **51 of 51 with 0 wrong**, delivered **worst -10.028 dB and best -9.961 dB** against a
requested -10.0. **Nothing differed, so nothing had to stop.**

### Task 2 — the instrument proved before it was trusted

Three new tests. The convention was read out of `SignalToNoise` rather than remembered; requested was
proved equal to delivered **at every rung of tonight's ladder at both seeds, over one slot's draw**
rather than unit 214's twenty slots; the noise was shown to be noise on its mean, its variance and
its seed; and **pure noise was decoded and returned nothing.** All four numbers are in section 3.

### Task 3 — the ladder, the centre of the night

**520 slot decodes** in 45 s, 10 rungs, 26 messages, 2 seeds. **The budget was stated in
`PROJECT_STATUS.md` before the run**, as the instruction directs. The table was printed before
anything was asserted about it, and the only assertions in the test are **zero wrong messages at every
rung** and **the top rung still returning everything** — nothing in it can throw on a poor result.

**One decision made for myself, reported in full.** The messages offered at every rung are the corpus
filtered to `!CarriesHashedCallsign`, exactly as the existing -10 dB test filters it, **and then
thinned by taking every second entry rather than the first 26**. The corpus is written in blocks by
kind — CQ, standard, free text, telemetry, non-standard — so the first 26 would have measured one
kind's sensitivity rather than the path's. 26 clears the instruction's floor of 20.

**One error of my own, found and fixed before any number was read.** The true codeword was being built
from the corpus entry's 77 bits handed straight to `LdpcEncoder`, which wants the 91 bits that come
out of the CRC step; it refused 10 bytes where it wants 12 and the test went red on its first run. The
fixture now takes the same three steps the encoder takes — 77 bits, then the checksum, then the parity
— which is the codeword that was actually on the air.

### Task 4 — what the collapse is made of

Two tests. The first walks the whole 51-message corpus at the two rungs task 3 bracketed the collapse
with, plus -21 beside them, **306 decodes**, and reports what a rate cannot: how many trials had **no
kept candidate within four hertz** of where the fixture actually put the signal. The second takes an
agreement-only sweep at all ten rungs — no decoding at all, because agreement is measured before error
correction is asked to do anything, which is exactly where unit 217 measured it — and reads unit 217's
three on-air figures off the measured curve by interpolation.

### Task 5 — the three things real air has that the fixture does not

Three tests, **1070 slot decodes** in 85 s. Off the block grid, off the frequency bin, and twenty
transmissions sharing one slot, **each on its own** so the cost of each is its own number.

**Drift is named and not tested, rather than quietly omitted.** A drifting transmitter needs a
synthesizer that can make one and step 3's proven encoder cannot; building one tonight would have been
new DSP nobody has bounded. It is the fourth thing real air has and this unit did not measure it.

### Task 6 — the join

Two tests. The first splits all 1298 expected lines by the SNR their own list gives them and joins the
column to matched-or-missed **for the first time in this phase**. The second locates the strong-SNR
misses and counts them for repeats.

**A second decision made for myself, reported in full.** The instruction's task 6 asks for the table,
the strong-SNR list, the comparison with the ladder and the verdict. **I added one test beyond that**
— the strong-SNR misses located against the candidate list and grouped by text — because *78 strong
misses, unexplained* and *78 strong misses, and here is where they die and here is that thirteen of
them recur across files* are very different things to hand the next unit, and it cost 7 seconds
extending unit 217's own instrument rather than rebuilding it.

### Task 7 — no fix, then the record

**NO FIX IS LICENSED, AND THAT IS THE EXPECTED OUTCOME AND IT IS NOT A FAILURE.** The four conditions
were taken one at a time. **Condition 1 is half met at best**: the 78 strong-SNR misses name a
*target*, but no measurement tonight names a *change*. **Condition 2 fails outright**: nothing found
tonight is a departure from the pinned `ft8_lib`, so any change would be a tuned constant, a widened
tolerance or a lowered threshold — unit 216 was offered tuning and refused it in writing, unit 217
measured two fixes and built neither, and that is the standard. Conditions 3 and 4 were never reached.
**So no library file was touched, criterion 3 was not re-taken after a change because there was no
change, and the extras did not rise — still 23, the same 23.**

`OPEN_ISSUES.md` gains **`HM-OPEN-066`**; `src/Ft8Sharp/porting-notes.md` gains its unit-218 section;
both versions bumped; everything re-run after the bumps. Details in section 3.

## 2. What the owner should expect

**Hamlet's behaviour has not changed.** No library file was edited tonight. The decoder returns
exactly what it returned yesterday, on the reference recordings and on anything else.

**What is now true that was not this morning: this receiver has a sensitivity, in decibels, and it is
a fact about this tree that owes nothing to any list, any binary or anybody's opinion.** It returns
every message down to **-18.0 dB** in calibrated noise on signals it makes itself, holds 25 per cent
at -20.0, and stops by -22.0. It has never manufactured a message out of noise — not once in 18 slots
of pure noise, and not once in 1896 trials with a signal in them.

**WHAT WILL LOOK WRONG BUT IS NOT — and this is the most important paragraph on the page.**

- **"The FT8 threshold is -21 dB and this thing quits at -19. It is broken."** It is not, and the
  comparison is not available tonight. That figure is quoted for a whole system under a stated
  convention, and **step 6 is the unit that measures this library against it as a verdict.** Unit 218
  took a diagnostic ladder in one session and is forbidden — by its own instruction — to claim it.
  What is fair to say is that the ladder sits near -19 and that step 6 will have a two-decibel
  question to answer.
- **"Step 6 looks done."** It is not, and nothing in this unit may be counted toward it. **Step 6
  still requires all three of its criteria**: a curve *generated across a range of SNR and shown to be
  reproducible*; a decode rate at -21 dB *compared with the published figure as a verdict*; and
  *behaviour below the threshold degrading rather than producing wrong decodes, recorded as a
  criterion*. This unit produced early evidence for the third and claimed none of the three.
- **"Rungs that returned nothing mean tests failed."** They do not. The ladder was deliberately
  written so that a rung returning nothing is a **measurement**; a test that threw at -22 dB would
  have destroyed the thing it exists to take. Every test in this unit is green.
- **"The count is still 760, so the night was wasted."** The instruction said in terms that the count
  was not expected to move and that a report saying so with a measured ladder underneath it is a
  complete success. What changed is that criterion 3's residue now has an id, a measured ladder, a
  named verdict and four ranked routes out of it.
- **"58.6 per cent sounds bad."** It is also 65.7 per cent of the 1157 lines any receiver could
  reach, because 141 of those lines lost their callsign in the list itself. **Both readings are given
  and neither stands alone**, tonight as in unit 217.

**One thing to expect from the next unit rather than from this one.** The night produced two
measurements that disagree, and neither is weak. That disagreement is written up as `HM-OPEN-066` with
the evidence for each side rather than resolved by preference.

## 3. What you should see

### THE LADDER, THE ANSWER THIS UNIT WAS COMMISSIONED TO GET

Aligned, on a bin centre, alone in the passband. 10 rungs, 26 corpus messages filtered to
`!CarriesHashedCallsign`, 2 seeds — **520 slot decodes**. Stage columns are means per slot.
**Binned by the DELIVERED ratio; the requested column is what was asked for.**

```
requested  delivered  offered  returned    rate   WRONG   cand     par     crc     txt
    -10.0     -9.999       52        52   100.0       0    18.2     1.6     1.6     1.6
    -12.0    -12.001       52        52   100.0       0    17.5     1.2     1.2     1.2
    -14.0    -14.001       52        52   100.0       0    16.0     1.0     1.0     1.0
    -16.0    -16.000       52        52   100.0       0    14.4     1.0     1.0     1.0
    -18.0    -17.998       52        52   100.0       0    13.6     1.0     1.0     1.0
    -20.0    -19.998       52        13    25.0       0    11.9     0.2     0.2     0.2
    -21.0    -20.998       52         2     3.8       0    12.5     0.0     0.0     0.0
    -22.0    -22.000       52         0     0.0       0    13.3     0.0     0.0     0.0
    -24.0    -23.998       52         0     0.0       0    12.4     0.0     0.0     0.0
    -26.0    -25.999       52         0     0.0       0    11.4     0.0     0.0     0.0
NOISE ONLY, NO SIGNAL AT ALL, 18 slots at 3 amplitudes and 6 seeds:
                                 —         0     —         0    10.2     0.0     0.0     0.0
```

**THE COLLAPSE, IN ONE SENTENCE WITH A NUMBER IN IT: this path returns every message down to
-18.0 dB and stops between -18.0 dB and -21.0 dB, with the 50 per cent crossing near -19 dB.**

**WRONG MESSAGES RETURNED: 0 out of 520.** The ladder brackets the collapse rather than stopping
above it — the bottom three rungs return nothing, so no rungs had to be added.

**The collapse is not the search, and the stage columns say so without a second run.** Candidates per
slot barely move across the whole ladder — 18.2 down to 11.4 — while parity falls off a cliff from 1.0
per slot at -18 to 0.2 at -20 to 0.0 at -21. **The signal is still being found at every rung; it stops
being recovered.**

### TASK 4 — THE AGREEMENT COLUMN, AND UNIT 217'S HISTOGRAM GETS A DECIBEL VALUE

Hard-decision agreement with the true codeword out of 174, read at the candidate nearest where the
fixture actually put the signal, on signals whose codeword is **exact** because this fixture generated
it.

```
delivered   trials  no cand  mean agree   worst   best
   -9.999       52        0       174.0     174    174
  -12.001       52        0       173.9     171    174
  -14.001       52        0       173.7     171    174
  -16.000       52        0       171.3     166    174
  -17.998       52        0       164.1     157    170
  -19.998       51        1       148.5      81    162
  -20.998       50        2       139.9      87    157
  -22.000       45        7       135.6     121    152
  -23.998       21       31       120.8      84    138
  -25.999        4       48        93.5      79    111
```

**UNIT 217'S THREE ON-AIR FIGURES READ OFF THAT CURVE:**

| unit 217's on-air figure | of 174 | reads, on this ladder's axis |
|---|---|---|
| matched, mean agreement | 167.7 | **about -17.0 dB** (between -18.0 and -16.0) |
| **MISSED, mean agreement** | **122.8** | **about -23.7 dB** (between -24.0 and -22.0) |
| chance, measured | 84.8 | below -26.0 dB, off the bottom of this ladder |

**So on this receiver's own axis the on-air misses look like signals three to five decibels below
where this path stops answering** — which is the reading that says the receiver is sound and the
shortfall is the benchmark's reach.

**And the anatomy of the bracket, over the whole 51-message corpus, 306 decodes:**

```
delivered  offered   back   rate %  WRONG  no cand    cand     par  agree back  agree missed
  -17.997      102    101     99.0      0        0    13.8     1.0       164.8         159.0
  -20.000      102     32     31.4      0        1    11.7     0.3       156.3         148.3
  -21.000      102      0      0.0      0        3    13.3     0.0           -         140.4
```

**At -21.0 dB, where the rate is 0.0 per cent, 99 of 102 trials still had a kept candidate within four
hertz**, and the mean agreement there is **140.4 of 174 — thirty-four bit errors** — against a code
whose recovery unit 215 measured **reaching zero at seventeen.** The signal is found and the ratios
are too damaged to correct. **That is what a channel does, not what a defect does.** The histogram is
a **slope and not a cliff**, and at -20 the returns and the misses overlap between 150 and 159.

### TASK 5 — THE IMPAIRED LADDERS, EACH AGAINST TASK 3'S ALIGNED COLUMN

```
rung     aligned   off the block grid   off the frequency bin   twenty sharing the slot
-10.0      100.0                100.0                   100.0                     100.0
-12.0      100.0                100.0                   100.0                     100.0
-14.0      100.0                100.0                   100.0                     100.0
-16.0      100.0                100.0                   100.0                     100.0
-18.0      100.0                 94.2                    86.5                      93.3
-20.0       25.0                 19.2                    11.5                      10.0
-21.0        3.8                  0.0                     0.0                       0.0
-22.0        0.0                  0.0                     0.0                       0.0
-24.0        0.0                  0.0                     0.0                       0.0
-26.0        0.0                  0.0                     0.0                       0.0
WRONG:         0                    0                       0                         0
EXTRA:         —                    —                       —                         0
```

**5a, off-grid in time.** Six offsets, none a multiple of the 1920-sample block or the 960-sample
sub-block: 5761 (misses a boundary by one sample), 7013, 3701, 961, 1439, 12345. Frequency left
exactly on a bin centre, **so the offset is the whole of the impairment**.

**5b, off-bin in frequency.** Six places across the passband — 300, 700, 1200, 1800, 2400, 2700 Hz,
every one exactly on a bin centre — plus **a quarter bin and a half bin**. Offset left on the block
grid, **so the fraction is the whole of the impairment**. **The transform bin is 3.1250 Hz, not the
6.25 Hz tone spacing**, because the geometry oversamples frequency by two, so a half bin here is
1.5625 Hz and is genuinely equidistant from two bins — this is harsher than it reads.

**5c, a populated passband, AND THIS IS THE DROP CANDIDATE.** **The FIRST branch of its drop condition
applied and the numbers that decided it are these: 5a and 5b agree with the aligned ladder within
about one rung** — at -18 they are 94.2 and 86.5 against 100, at -20 they are 19.2 and 11.5 against
25.0, and all three go to zero within one rung of each other. **So 5c was droppable. It was run
anyway**, because it costs 30 slot decodes against 5a's and 5b's 1040 and four seconds against their
78, and it earned them: **twenty simultaneous transmissions with 137 candidates per slot at -10 dB
returned not one message nobody sent, at any rung, down to -26 dB.**

**So the third of the three named outcomes — that the loss is something real air has and the fixture
does not — is NOT supported by the three things this unit could test.** Timing, frequency placement
and a crowded band together cost about one decibel. Drift was not tested and is named above.

### TASK 6 — THE JOIN, AND THE NUMBER THAT NEEDS NO CALIBRATION

**The totals reproduce through the join** — 60 recordings, **1298 lines parsed, 0 with no SNR field**,
**760 matched, 538 missed, 23 extra**, column range **-24.0 to +20.0 dB**. **This is task 1's untold
run read a second way, not a new or filtered one.**

**THE MOST IMPORTANT FINDING IN THIS REPORT, and it goes first because it needs no calibration at
all:**

| floor, by the list's own column | expected lines | matched | MISSED | of those, hashed | **MISSED AND MATCHABLE** |
|---|---|---|---|---|---|
| **0 dB or better** | 384 | 261 | 123 | 45 | **78** |
| **-5 dB or better** | 631 | 395 | 236 | 67 | **169** |

**Seventy-eight is not a handful.** The strongest of them, in full:

```
20m_busy/test_14.wav     19.0   2046.0  cand within 4 Hz: yes   RA9UJP 9A9A RR73
20m_busy/test_28.wav     19.0   2046.0  cand within 4 Hz: yes   LY3BES 9A9A RR73
20m_busy/test_18.wav     17.0   2046.0  cand within 4 Hz: yes   LU5HA 9A9A RR73
20m_busy/test_22.wav     17.0   2046.0  cand within 4 Hz: yes   LU5HA 9A9A RR73
191111_110615.wav        15.0    906.0  cand within 4 Hz: yes   PA3EPP SP8NFO KN09
20m_busy/test_21.wav     13.0   2378.0  cand within 4 Hz: yes   CQ SP9LKP JO90
20m_busy/test_29.wav     13.0   2388.0  cand within 4 Hz: yes   RV6ARS E75C RR73
20m_busy/test_01.wav     12.0   2378.0  cand within 4 Hz: yes   R1CBP SP9LKP RR73
20m_busy/test_07.wav     12.0   1088.0  cand within 4 Hz: yes   R3FO R7NO -16
20m_busy/test_15.wav     12.0   2389.0  cand within 4 Hz: yes   PA3GAE E75C RR73
20m_busy/test_07.wav     11.0   2378.0  cand within 4 Hz: NO    F4VTS SP9LKP -20
20m_busy/test_19.wav     11.0   1561.0  cand within 4 Hz: yes   7Z1AL RA3TPE LO25
20m_busy/test_26.wav     11.0   2518.0  cand within 4 Hz: yes   RU0LL F5CCX -10
```

**Where they die: 75 of the 78 had a kept candidate within four hertz — 96.2 per cent, against unit
216's 95.9 per cent over ALL 531 misses.** They die exactly where the weak ones die, so **the list's
SNR column is not separating two faults.**

**And they are not random.** 58 distinct texts among the 78; **13 of them missed in more than one
recording**, covering 33 lines:

```
times   snr range  text
    4    0 to   2  CQ IU8DMZ JN70
    4    1 to  13  CQ SP9LKP JO90
    3    2 to   4  CQ HA1BF JN86
    3    1 to   8  CQ R7NO KN98
    3    3 to   5  BD8NBG DJ2BW -15
    2    4 to   4  EA2DIC R7NO -25
    2    1 to   3  7Z1AL OK2BV JN89
    2    5 to  12  R3FO R7NO -16
    2    5 to  10  DG1BQC HB9CUZ RRR
    2    6 to  11  7Z1AL RA3TPE LO25
    2   17 to  17  LU5HA 9A9A RR73
    2    0 to   1  ES1KK SP9LKP -13
    2    4 to   5  F4DFQ F5LOW IN95
```

`9A9A` at **2046 Hz** is missed in four files at +17 and +19 dB; `SP9LKP` at 2378 Hz in four; `R7NO`
around 1087 Hz in three. **A station missed in file after file at the same frequency is a property of
that signal, not a draw against a noise floor.**

**THE SNR-BINNED TABLE, ALL 1298 LINES, WITH THE COUNT IN EVERY BIN:**

```
 snr bin, dB   lines  matched  missed   rate %
  18 to  21      17       12       5     70.6
  15 to  18      23       20       3     87.0
  12 to  15      38       30       8     78.9
   9 to  12      61       43      18     70.5
   6 to   9      55       32      23     58.2
   3 to   6      91       64      27     70.3
   0 to   3      99       60      39     60.6
  -3 to   0     138       83      55     60.1
  -6 to  -3     166       84      82     50.6
  -9 to  -6     159       76      83     47.8
 -12 to  -9     136       82      54     60.3
 -15 to -12      87       52      35     59.8
 -18 to -15      79       44      35     55.7
 -21 to -18      60       32      28     53.3
 -24 to -21      89       46      43     51.7
       TOTAL    1298      760     538     58.6
```

**THE SAME TABLE WITH THE 141 HASHED LINES EXCLUDED. Both are given and neither stands alone:**

```
 snr bin, dB   lines  matched  missed   rate %
  18 to  21      14       12       2     85.7
  15 to  18      23       20       3     87.0
  12 to  15      35       30       5     85.7
   9 to  12      53       43      10     81.1
   6 to   9      45       32      13     71.1
   3 to   6      82       64      18     78.0
   0 to   3      87       60      27     69.0
  -3 to   0     128       83      45     64.8
  -6 to  -3     150       84      66     56.0
  -9 to  -6     135       76      59     56.3
 -12 to  -9     122       82      40     67.2
 -15 to -12      80       52      28     65.0
 -18 to -15      67       44      23     65.7
 -21 to -18      50       32      18     64.0
 -24 to -21      86       46      40     53.5
       TOTAL    1157      760     397     65.7
```

**THE ON-AIR CURVE BESIDE THE SYNTHETIC LADDER**, and it is the comparison the verdict turns on:

| dB, by each scale | synthetic ladder, rate % | on-air, all 1298 | on-air, 1157 representable |
|---|---|---|---|
| about -10 | 100.0 | 60.3 (-12 to -9) | 67.2 |
| about -16 | 100.0 | 55.7 (-18 to -15) | 65.7 |
| about -18 | 100.0 | 53.3 (-21 to -18) | 64.0 |
| about -21 | 3.8 | 51.7 (-24 to -21) | 53.5 |
| about -24 and below | 0.0 | — the column stops at -24 | — |
| **+18 and above** | **off the top of the ladder** | **70.6** | **85.7** |

**THE TWO dB SCALES ARE NOT PROVEN TO BE THE SAME SCALE, AND NO CONCLUSION THAT DEPENDS ON THEIR
BEING IDENTICAL IS DRAWN.** The list's column is a third party's estimate under a convention this
project did not choose — and unit 216 proved from the lists themselves that they are not even the
pinned decoder's output, since `decode_ft8` prints `score * 0.5f` and refuses a score below 10, so
+5.0 is the lowest it can print. **The comparison above is of shape and ordering only.**

**And the shape does not match.** The synthetic ladder falls from 100 per cent to 0 across four
decibels. The on-air rate falls from **85.7 per cent to 53.5 per cent across forty-five decibels**,
which is not the shape a sensitivity limit makes.

### THE THREE-OUTCOME VERDICT, AGAINST THE TABLE FIXED BEFORE TASK 3 RAN

| outcome named before the run | verdict | the numbers that decided it |
|---|---|---|
| **1 — the path holds to about -20 and the on-air misses sit below where it collapses; the receiver is sound and the shortfall is the benchmark's reach** | **SUPPORTED BY ONE MEASUREMENT AND CONTRADICTED BY ANOTHER** | *for*: unit 217's on-air miss mean of 122.8 of 174 reads about **-23.7 dB** on this ladder's own agreement curve, three to five dB below the collapse. *against*: **78 matchable expected lines at 0 dB or better were missed, two at +19 dB**, and the on-air rate curve is nearly flat against the list's own column |
| **2 — the path collapses well above -20, say at -13 or -15; the receiver is deaf and the port has a defect nobody has found** | **RULED OUT** | 100 per cent returned at **-18.0 dB**, 25.0 at -20.0. A decoder returning every message at -18 dB is not deaf at -13 or -15 |
| **3 — the loss is something real air has that the fixture does not; timing, frequency offset, drift, neighbours** | **THE THREE THINGS TESTED ARE RULED OUT; THE FORM SURVIVES** | off-grid timing, off-bin frequency and twenty in one slot each cost **well under one rung**. But **drift was not tested**, and the 78 recurring by station and frequency is a per-signal property that noise alone does not produce |

**SO THE MEASUREMENTS ARE AMBIGUOUS, AND SAYING SO IS THE HONEST ANSWER.** Outcome 2 is dead.
Outcome 1 and outcome 3 each hold half the evidence, and **one of the two readings is wrong**:
either unit 217's agreement figure is misleading because it is taken at the nearest candidate, which
for a miss may be sitting on a different transmission, or the lists' SNR column is not measuring what
its sign suggests. **This unit cannot say which**, because settling it needs either `decode_ft8.exe`
or a reason to trust that column, and neither is available here.

**WHAT THE NEXT UNIT SHOULD DO, in the order the evidence points.** Take the **78**, and specifically
the **13 that recur** — `9A9A` at 2046 Hz across four recordings at +17 and +19 dB is the sharpest
single target this phase has ever had. **75 of 78 had a candidate at the right frequency**, so the
search delivered and something after it did not. Read the agreement at those specific candidates and
ask why a transmission a third party calls +19 dB is found and not recovered. **That is a defect with
an address if it is one, and it is the cheapest thing left on the board.**

### TASK 2 — THE CONVENTION AND THE CALIBRATION

**The convention, read out of `SignalToNoise` rather than remembered:**

```
SNR(dB) = 10 log10( signal power / noise power in a 2500 Hz reference bandwidth )
sigma   = sqrt( signalPower * (12000/2) / (2500 * 10^(snr/10)) )

reference bandwidth : 2500 Hz — the amateur weak-signal convention the published figure uses
sampled bandwidth   : 6000 Hz one-sided, real samples at 12000 Hz
noise in reference  : sigma^2 * 2500 / 6000 = sigma^2 * 0.41667
signal mean square  : 0.499008 — MEASURED from the samples, not assumed to be 0.5 for a sine
slot                : 180000 samples, 15.00 s
```

**Requested against delivered, over ONE SLOT'S DRAW** — the draw every trial of every ladder actually
gets, where unit 214 proved it over twenty slots' worth. **20 points, all ten rungs, both seeds:
worst |requested − delivered| 0.0211 dB, mean 0.0097 dB**, printed before the bound of 0.05 dB was
asserted. **This is why every ladder in this unit is binned by delivered and never by requested.**

**The noise is noise.** Variance / sigma² = **1.00355** at sigma 1.0, 0.1 and 0.0125 alike; mean below
2 per cent of sigma at all three. **The seed decides it: 180000 of 180000 samples identical on a
repeated seed, 180000 of 180000 different on a new one** — asserted on the samples and never on a
count of them.

**And the floor.** **18 slots of pure noise, no signal in them at all**, at the amplitudes the top,
middle and bottom of the ladder deliver, six seeds each:

```
slots of pure noise decoded : 18
candidates FOUND            : 183
reached parity              : 0
passed the checksum         : 0
became text                 : 0
MESSAGES RETURNED           : 0
```

**Candidates found and no text is the path behaving correctly** — the search is permissive by design
and the parity and CRC gates are what refuse. **A fact nobody had written down fell out of it: the
candidate counts are identical at all three amplitudes for the same seed** — 14, 7, 13, 10, 9, 8 at
-10, at -18 and at -26 alike. **The search is scale-invariant**, which is the waterfall's
normalisation doing its job.

### NOTHING WAS TOLD TO THE DECODE PATH, AND IT DID NOT HAVE TO BE

The synthetic ladders know their own truth **because they generated it**: the frequency and offset are
chosen in the fixture and handed to the *synthesizer*, and the truth is used twice, both times after
the code has answered — to compare the text, and to choose which candidate the agreement is read at.
**Task 6's join reads the results of task 1's untold run and does not re-run the path with anything
told to it**; the expected lines are read afterwards. In the one place a frequency from the list is
used at all — choosing which candidate to inspect for the 78 — **the search's own list was taken whole
and was never filtered by it.** No expected text, frequency, count or list reached `Ft8SlotDecoder`,
`Ft8SoftSymbols`, `Ft8SyncSearch` or `Ft8CodewordDecoder`.

### THE FIX, THE RECORD, THE VERSIONS AND THE TREE

**NO FIX WAS LICENSED AND THAT IS THE EXPECTED OUTCOME AND IT IS NOT A FAILURE.** Condition 1 half
met — the 78 name a target, not a change. **Condition 2 failed outright** — nothing tonight is a
departure from the pin, so any change would be tuning. Conditions 3 and 4 never reached. **Criterion 3
therefore needed no re-take after a change, because there was no change; it was nevertheless
reproduced four times tonight — task 1's untold run, task 6's join, task 6's location pass and the
full suite — and the extras did not rise. Still 23, the same 23.**

**`OPEN_ISSUES.md` gains `HM-OPEN-066`** — *owner `claude`, severity `slows`, blocks step 5
criterion 3*. It carries the number, the four buckets, the ceiling, tonight's ladder, which of the
three outcomes applied, and four ranked things it would take to close it with **the 78 first**,
`decode_ft8.exe` third and the two-decibel question fourth. It also says what is **not** wanted:
tuning.

**`src/Ft8Sharp/porting-notes.md` gains its unit-218 section** — the convention with its arithmetic,
the calibration, the noise floor, the ladder table, the impaired table, the agreement curve, and, said
as plainly as unit 212 said its single-precision finding, **five things this measured sensitivity is
NOT evidence about**, with step 6 named first.

**No divergence was added and none was expected.** Nothing was ported tonight and no library file
changed. **The count stands at 25.**

**Versions.** `src/Ft8Sharp/Directory.Build.props` **0.10.1 → 0.10.2** under HM-DEC-152 — measurement
only, the library gains evidence rather than a capability, unit 211's precedent and unit 217's. The
line *that -19 is not a step 6 result* is written into the props file itself, not only into this
report. Root `Directory.Build.props` **1.12.24 → 1.12.25** under HM-DEC-150.

**Re-run after both bumps.** `Ft8Sharp` **496 total, 495 passed, 0 failed, 1 skipped** in 97 s —
**eleven tests added** and still the one correct skip, `Ft8TableGenerationTests.RewriteTheCheckedInTablesFile`,
the table write gate. Library **0 warnings, 0 errors**.

**Channels, with the filter strings so the next unit inherits a filter rather than a number**
(known item 17):

```
RadioEngine, 55 green:
  --filter "FullyQualifiedName~AudioSeamTests|FullyQualifiedName~PrivilegeTests"
App, 9 green, VersionTests among them:
  --filter "FullyQualifiedName~DecisionLogOrderTests|FullyQualifiedName~VersionTests|
            FullyQualifiedName~EveryResourceKeyResolvesTests|FullyQualifiedName~ViewTestsActThroughControlsTests"
```

These are unit 217's reconstruction, inherited rather than re-invented, and neither reds.

**Attribution.** **180** paths from `2828ab6` at exit against 174 at entry — the six added are this
unit's own test files — and the `src/Hamlet.`/`tests/Hamlet.` filter **returns 0**.

**The tree.** **8 `.obj` at the repository root**, counted at the end and untouched — not committed,
not deleted, not read, not linked, not run and not reasoned from. **`tools\build-ft8-oracle.bat` is
present, untracked, and was not run**; nothing tonight needed a compiler. `git status --short` printed
**34 at entry** and prints **35** as this is written, the extra line being `OUTPUT.md` awaiting its
commit — **34 again once it is committed, the same count the unit entered at.** **This unit added
nothing to the working tree that it did not commit.**

**Committed:** `PROJECT_STATUS.md`, `OPEN_ISSUES.md`, both `Directory.Build.props`,
`src/Ft8Sharp/porting-notes.md`, and six new test files under `tests/Ft8Sharp.Tests/Dsp/` —
`SensitivityLadder.cs`, `Ft8LadderCalibrationTests.cs`, `Ft8SensitivityLadderTests.cs`,
`Ft8CollapseAnatomyTests.cs`, `Ft8ImpairedLadderTests.cs` and `Ft8OnAirSnrJoinTests.cs`. **Seven
commits, one per task plus this report, each pushed before the next task started; every push accepted
first time and no refusals from GitHub.**

**Left alone:** the 8 `.obj`; `tools/build-ft8-oracle.bat`; `PHASE_OUTCOME.md`;
`src/Ft8Sharp/Tables/Ft8Tables.g.cs`, read for declarations and not edited; everything under
`tools/`; the loop's uncommitted files — modified `tools/arbiter/run-phase.bat` and `run-unit.bat`,
`ANALYSIS-cw-emit-decision-2026-08-24.md`, `ANALYSIS-cw-two-stations-2026-08-23.md`,
`PROJECT_CARD.md`, `WORK_INSTRUCTIONS.md`, and untracked `ARBITER.md`, `MANIFEST.txt`,
`PHASE_PLAN.md`, `RUN_LEDGER.md`, `VERIFY_PASS.md`, `SCRUB_SELFTEST.bat`, `SESSION.lock`,
`.run-unit/`, `docs/phase-uplift/` and `tools/unit217/`; the eight probe and scratch files known item
10 names; and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` and
`src/Hamlet.RadioEngine/Audio/Ft8Sync.cs`, neither read for structure, ported, copied, referenced nor
edited.

**No probe file of my own was left behind.** Everything this unit wrote is a checked-in test that runs
in the suite, so what is on disk and what is committed compile to the same tests.

**`PHASE_STATUS.md` was edited on one line only** — `WORK_INSTRUCTION:` set to
`218 - how deaf is this receiver, in decibels, measured against physics rather than against a list`,
as the session prompt directs. It was **not committed**, `HEARTBEAT:`, `CURRENT_STEP:` and the `STEP:`
lines were not touched, and nothing below the `---` was changed. **The work instruction's known item 6
and its rule 13 say not to hand-edit that file at all**; the two directions conflict and the session
followed the prompt's explicit instruction while keeping the edit to the single line the prompt claims
and leaving it out of every commit. Reported rather than resolved.

### MISMATCHES AGAINST THE INSTRUCTION, REPORTED AND NOT REPAIRED

1. **`git status --short` printed 34 at entry, not the 33 the instruction states.** The extra line is
   `SESSION.lock`, which unit 217 reported the same way.
2. **Known item 10 says "seven probe and scratch files" and then lists eight.** `TempEncoderProbe.cs`,
   `UpstreamSyncSearchProbe.cs`, `UpstreamLdpcProbe.cs`, `Unit216Probe.cs`, `Unit217Probe.cs`,
   `unit215-section.md`, `unit216-section.md` and `unit217-status.py` are all present on disk. The
   count in the prose is one short of its own list. **No fifteenth attempt was made to delete any of
   them.**
3. **Attribution is 174 at entry, matching the instruction's citation of unit 217's report exactly,
   and 180 at exit** — the six added being this unit's own files. Not a mismatch, recorded because the
   instruction asked for it to be re-measured rather than inherited.
4. **The SNR column's range is -24.0 to +20.0 dB**, which is what unit 216 reported and what the
   instruction cites as "+? to -24" from a report rather than from the arbiter's own reading. **The
   arbiter's caveat is discharged: the clone is present, the column was read here, and it is as
   unit 216 described it.**
5. **`Ft8SlotDecoderTests.TheRateHoldsAtEveryOffsetOnAndOffTheGrid` exists as stated**, and
   `Ft8SlotDecoderPassbandTests.TwentyOverlappingTransmissionsSurviveSeededNoise` exists at -10.0 dB
   as stated. Task 5 walked both shapes down the ladder rather than inventing them. No mismatch.
6. **The instruction says `Ft8WaterfallGeometry` declares oversampling of 2 in both axes and it does**
   — which is why **the transform bin is 3.1250 Hz and not the 6.25 Hz tone spacing.** Not a mismatch,
   but it changes what "a half bin" means in task 5b and the report says so rather than letting the
   figure read as 3.125 Hz of error when it is 1.5625.

### THE VALIDATOR

Reported below in section 4 alongside its outcome, as known item 15 requires.

## 4. What's blocking us

**Two items. Neither is a ruling request, and neither is being manufactured to fill this section.**
One bears on criterion 3. **The reference decoder is not re-raised** — known item 4 — and **the
placeholder question is not re-raised**, being already in front of the owner from unit 217 and not
improved by being asked twice.

### 1. The two halves of the night disagree, and it changes what the rest of this phase is about

**Not a ruling request and not blocking anything.** Stated here in one place because the next arbiter
reads reports and this must not be missed.

**This receiver is not deaf.** It returns **every message down to -18.0 dB** and **25 per cent at
-20.0 dB** in calibrated noise, with **zero wrong messages in 1896 trials** and **zero messages out of
18 slots of pure noise**. The outcome the instruction feared — a collapse at -13 or -15 — **did not
happen**, and the phase can stop worrying about it.

**But the join does not agree that the on-air residue is weakness.** The decode rate against the
lists' own SNR column runs **85.7 per cent in the +18 dB bin to 53.5 per cent in the -24 dB bin** over
the 1157 representable lines — thirty points across forty-five decibels — and **78 expected lines a
receiver could have matched, at 0 dB or better by that column and two of them at +19 dB, were
missed.** **75 of the 78 had a candidate at the right frequency**, and **13 texts recur across
recordings** at fixed frequencies. Against that, unit 217's on-air miss agreement of 122.8 of 174
reads **about -23.7 dB** on this ladder's own curve, which says they are very weak indeed.

**One of those two readings is wrong.** Either the agreement figure is misleading — it is taken at the
nearest candidate, which for a miss may be sitting on a different transmission entirely — or the
lists' SNR column does not mean what its sign suggests. **This unit measured both and cannot choose
between them**, and it did not chase it, as the instruction directs. It is recorded as
**`HM-OPEN-066`** with the evidence for each side and four ranked routes out. **The 78 are the
cheapest of the four and the sharpest target this phase has had.**

**The two-decibel question, stated once and not claimed.** This ladder's 50 per cent crossing sits
near **-19 dB**. The published FT8 figure step 6 will judge against is near **-21**. **That comparison
is step 6's to make as a verdict and this unit did not make it** — but if it holds under step 6's own
measurement it is worth about two rungs of weak signals, and it belongs in whatever the next arbiter
plans.

### 2. The validator was refused again, for the eighth unit running

`tools\arbiter\validate-output.bat` was attempted in **all five spellings
`tools\arbiter\run-unit-tools.txt` lists**. **Four were denied outright** — `cmd //c`, `cmd /c`,
`cmd.exe //c` and `cmd.exe /c` — and **the fifth is permitted but unusable**, because Git Bash strips
the backslashes out of the path and the shell reported
`toolsarbitervalidate-output.bat: command not found`. **The refusal is reported as a refusal and was
not routed around**, per known item 15.

**The script's own six rules were then checked by hand against its source and against this file, and
all six pass:**

1. **Rule 1 — a parseable `UNIT:` line above section 1.** Present at **line 44**, inside the 60 the
   script reads.
2. **Rule 2 — the four top-level sections, in order, exact names.** `## 1. What Claude did`,
   `## 2. What the owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`, at
   lines 58, 170, 209 and 643, with a plain ASCII apostrophe so `findstr /b /c:` matches.
3. **Rule 3 — no fifth top-level section.** There are exactly four `##` lines in the file. Everything
   else is `###` or deeper, which the script states it ignores.
4. **Rule 4 — section 4 present even when empty.** Present, and not empty.
5. **Rule 5 — section 3 non-empty.** Several hundred non-blank lines between `## 3.` and `## 4.`.
6. **Rule 6 — the ordering block above `UNIT:`, with `C` naming a count.** `READ IN THIS ORDER` at
   line 1; `A.` at line 3, `B.` at line 13, `C.` at line 30, all with no indentation, all inside the
   first 60 and all above line 44; and the phrase the script matches on — `raises 2 items` — at
   **line 42**.

**This is the eighth consecutive unit to report this refusal.** It is not blocking — the rules are
checkable by hand and were checked — but a self-validation step that no unit since 210 has been able
to run is a gate that exists only on paper, and the owner may want to know that the count is now
eight rather than one.
