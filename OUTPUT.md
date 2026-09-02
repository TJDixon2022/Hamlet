READ IN THIS ORDER

A. PHASE — Hamlet hears FT8 off the radio and displays the decoded text on screen. Seven steps.
Steps 1 and 2 are closed. Step 3 is closed on its four must-pass criteria, its nice-to-pass one
recorded as HM-OPEN-065. Step 4 was closed by unit 214 on all five must-pass criteria. STEP 5 IS
THIS UNIT'S AND THIS IS ITS SECOND UNIT: it entered at 2 of its 3 subject criteria and LEAVES AT
2 OF 3 — criterion 2 was re-taken and strengthened, criterion 3 was attempted for the first time and
is PARTIAL, not met. Steps 6 and 7 have not started and cannot, because every step of this plan
depends on the one before it — the plan's own named deviation, and why step 5 was the only step this
phase could move.

B. STEP 5 — a found signal becomes a message. Five exit criteria, one at a time, and criterion 3
leads because it is the unit's target. (3) ft8_lib's reference WAVs decode against its expected
decode lists — PARTIAL, on RUNG 1 of the instruction's ladder. 760 OF 1298 EXPECTED MESSAGES MATCHED
ACROSS 60 RECORDINGS, 538 missed, and 23 RETURNED THAT ARE NOT ON ANY LIST out of 783 returned. No
file skipped for its rate; one file produced nothing and its stage is named. (2) A candidate failing
CRC is never returned — MET, AND NOW CLAIMED IN THE CANDIDATE SENSE, which unit 215 explicitly did
not claim: 0 messages from an empty slot, 0 from 239 candidates found in noise over 20 slots, 0 of 51
genuine transmissions carrying a wrong checksum whose candidates reached parity with ZERO unsatisfied
checks, and 0 wrong text from 51 transmissions at −30 dB. (1) A corrupted codeword within the code's
correcting power is recovered and one beyond it fails honestly — MET BY UNIT 215 at k = 6 over 400
trials with 0 wrong messages in 37 952; tonight bears on it only in that the ratios reaching that
decoder now come off real air and are on upstream's own scale. (4) Ft8Sharp tests green — 429/428/0/1
at entry, 470/469/0/1 at exit, the one skip being the table write gate at both ends. (5) Attribution
clean and the channels green — 168 paths from 2828ab6 with 0 under src/Hamlet. or tests/Hamlet.,
channels 55 and 13 re-run after both version bumps.

C. THIS REPORT — the findings weighed against A and B. 760 OF 1298 OF UPSTREAM'S OWN EXPECTED
DECODES CAME BACK, ACROSS 60 OF ITS OFF-AIR RECORDINGS, AND 23 MESSAGES WERE RETURNED THAT ARE NOT ON
ANY EXPECTED LIST. On audio this library synthesized, 51 of 56 corpus messages came back as
themselves through the whole path — the 5 that did not are the hashed-callsign entries and step 2's
own decoder refuses the same 77 bits — with 288 of 288 across the offset sweep and 0 wrong messages
anywhere. The measured ratio variance is 24.0000 after normalisation from inputs whose variances were
0.0595, 0.9523, 22.8555, 380.9222 and 9523.05, against the figure 24 read out of the pin in task 2.
Section 4 raises 3 items and ONE OF THEM IS IN THE WAY OF A CRITERION IN B — the shortfall on
criterion 3 and what would settle its cause. Task 7 was NOT dropped, though the branch that licenses
it applied: task 5 produced a real comparison on rung 1, and it was run anyway because a synthetic
twenty is the one instrument that separates overlap from real-world impairment.

UNIT:       216 — complete at task 8 of 8 — 2026-09-02 10:35
PHASE GOAL: Hamlet hears FT8 off the radio and displays the decoded text on screen.
UNIT GOAL:  Build soft symbol extraction and the whole path from samples to text, and decode
            ft8_lib's own reference recordings against its expected decode lists.
STATE:      complete
TASKS:      8 of 8
DROPPED:    none
ADVANCED:   partly — criterion 2 is now met IN THE CANDIDATE SENSE, which unit 215 could not claim,
            and criteria 4 and 5 are re-taken. CRITERION 3 IS NOT MET. It was attempted for the
            first time and it is PARTIAL at 760 of 1298, so step 5 leaves at 2 of its 3 subject
            criteria, the same count it entered at, and STEP 5 DOES NOT CLOSE.
NUMBER:     step 5 at 2 of its 3 subject criteria -> 2 of 3 (criterion 3 partial, 760 of 1298)
DRIFT:      1 consecutive unit without advance  (was 0)

## 1. What Claude did

**Exit state: complete, at task 8 of 8. Nothing was dropped.** Machine `C:\Source\HamLet`, project
claimed and confirmed as Hamlet by all four gate checks — `SHACK_FACTS.md` and
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` both tracked, `Hamlet.sln` the only solution,
`CoreHMI.sln` and `MURC.sln` both absent. Branch `main`. Every task was committed before the next
began; eight commits, `897183b` to `29e82cf`.

**THE LAST PUSH WAS REFUSED BY THE REMOTE AND THE REFUSAL IS REPORTED RATHER THAN WORKED AROUND.**
The first seven commits were pushed and accepted. Task 8's commit was refused twice with
`! [remote rejected] main -> main (Internal Server Error)` — a server-side fault at GitHub, not a
rejection of the content. Nothing was rebased, force-pushed or otherwise coerced. `main` is one
commit ahead of `origin/main` and the eighth commit is on disk.

### What was traced, built and measured

**Task 1 — the ground, re-measured rather than inherited.** Ft8Sharp at entry 429 total, 428 passed,
0 failed, 1 skipped in 19 s, which is exactly what unit 215 reported. Library built at 0 warnings and
0 errors, `net8.0`, nullable enabled, warnings as errors, no `PackageReference` and no
`ProjectReference`. 158 paths from `2828ab6` with 0 under any Hamlet project. Channels 55 and 13.
`HEAD` `760eae2`, 8 `.obj` at the root, versions 1.12.22 and 0.9.0, 21 divergences. Both halves this
unit joins were taken from existing green tests and not re-swept:
`Ft8SearchRecoveryTests.EveryMessageOfTheCorpusIsFoundAtAPlaceTheSearchWasNeverTold` for the search,
`Ft8LdpcCodewordGateTests.ACleanCodewordDecodesToItsOwnMessageOverTheWholeCorpus` for the decoder.

**Task 2 — upstream's extraction and its recordings, read through the test process** because the
sandbox refuses the session direct access to `C:\Source\ft8_lib`, exactly as it refused units 209 to
215 and this unit's arbiter. Twelve inventory tests green over the pin. Findings in section 3.

**Task 3 — extraction and normalisation.** `src/Ft8Sharp/Dsp/Ft8SoftSymbols.cs`: a waterfall and a
candidate in, 174 log-likelihood ratios out, plus upstream's normalisation as a separate callable
step. Nine tests. **56 of 56 corpus messages extract at 174 of 174 hard decisions** before any
correction is involved.

**Task 4 — the whole path.** `src/Ft8Sharp/Dsp/Ft8SlotDecoder.cs`: samples in, messages out. Seven
tests. **51 of 56, 288 of 288, 51 of 51 in noise, 0 wrong.**

**Task 5 — the unit's target.** All 60 reference recordings that carry an expected list, in 9
seconds. Five tests. The table leads section 3.

**Task 6 — criterion 2 in the candidate sense.** Five tests, four counts, all zero.

**Task 7 — twenty at once.** Two tests. **20 of 20, twice, 0 extra.**

**Task 8 — the record, the versions and this report.**

### Decisions made for myself, reproduced in full

**One. `WavFile` was taught to walk to the `data` chunk, rather than nine of the sixty recordings
being skipped.** The instruction says *this is how a WAV is read, do not write a second reader*. The
reader required `data` to be the second chunk; nine of the sixty reference recordings put a 158-byte
chunk in between, which is why they are 360 202 bytes where the rest are 360 044. The choice was
between measuring criterion 3 on 51 files for a reason that has nothing to do with the audio, or
extending the one reader by fifteen lines. **I extended it, and nothing else was relaxed** — the
RIFF and WAVE tags, the fmt chunk's position and length, the format, the channel count, the bit width
and the truncation check are all exactly as they were, and every one of them is still watched by
`WavFileTests`. The refusal that changed is *the second chunk is not data*, which became *there is no
data chunk anywhere*, and the refusal message names every chunk it did find so the existing test
that replaces `data` with `fact` still sees `fact` in the message.

**Two. The de-duplicator recovers the codeword by re-running the decoder.** Upstream keys duplicates
on the packed payload. `Ft8CodewordDecoder` does not hand the codeword back and is closed evidence
this unit may not change, so `Ft8SlotDecoder` re-runs `LdpcDecoder.Decode` over the same ratios —
**only for candidates that already passed the gate**, so it costs one belief propagation per
successful decode and none per refusal. It is not a second CRC check. A later unit that wants to tidy
this would have the gate return the payload.

**Three. Task 7 was run rather than dropped, and the branch that licensed dropping it applied.** Task
5 produced a real comparison on rung 1, which is exactly the branch the instruction names as making
task 7 droppable. It was run anyway, for two seconds, because task 5 came back at 58.6 per cent and a
synthetic twenty is the one instrument that has overlap and none of the fading, interference or
timing error a real recording carries. Twenty of twenty says the shortfall is not about having more
than one signal in a slot. **This was a decision to do more than the instruction required, not less.**

**Four. The candidate limit was swept as a measurement and not adopted.** The obvious move on a 58.6
per cent match rate is to raise a threshold until the number improves. That is tuning and it is
forbidden. It was swept at 140, 280, 560 and 1120, printed, and shown to change nothing; the default
remains upstream's 140. **The minimum sync score and the iteration count were not swept at all**,
because they are upstream's, they already match, and sweeping them is the temptation the instruction
names by name.

## 2. What the owner should expect

**Ft8Sharp can now be handed fifteen seconds of audio and hand back the messages that were in it.**
That is the thing the whole phase is for, and until tonight no unit had ever done it: unit 215's own
report says in its own words that no message had come off the air.

**And it has been judged by somebody other than itself for the first time.** Every receive-side
number this phase had produced was measured against a signal the library synthesized. Tonight it
decoded Karlis Goba's own off-air recordings against the lists checked in beside them, and **760 of
1298 came back.**

**What will look wrong but is not, and there are four.**

**One. 58.6 per cent looks like a broken port and the evidence says it is not that simple.** The
expected decode lists in the clone **were not written by the decoder this library was ported from**,
and that is provable from the lists rather than guessed: `decode_ft8` computes its SNR column as
`score * 0.5` and refuses a score below 10, so the lowest figure it can print is +5.0 — and 1078 of
the 1298 lines are below it, down to −24.0. They are a *stronger* reference than `ft8_lib` itself.
What this port has not been held against is `ft8_lib` running, and it cannot be, because
`decode_ft8.exe` is not built on this machine.

**Two. 23 messages came back that are not on any expected list, and that is the number this project
refuses.** It is reported as an extra, in full, every one of them. It is **not proven** that any of
them is a false decode: nothing in the clone says the lists are complete, so some may be messages the
list-writer missed. Every one of them passed all 83 parity checks and CRC-14. It is counted as an
extra anyway, because that is the safe way round.

**Three. `ADVANCED` says *partly* and the criterion count did not move.** Step 5 entered at 2 of 3
and leaves at 2 of 3. Criterion 2 got materially stronger — it is now claimed with real candidates
from real audio rather than at the codeword entry point — but a strengthening is not a new criterion,
and criterion 3 is partial. **Step 5 does not close and step 6 does not open.**

**Four. The library version went to 0.10.0 and it is still not a 1.x.** No audio comes from a radio,
nothing is scheduled to a UTC slot, nothing reaches a screen, and the sensitivity is unmeasured.

## 3. What you should see

### The task 5 table — upstream's own recordings, one row per file

Columns: candidates, parity satisfied, checksum passed, became text, unique after de-duplication,
expected, matched, missed, and **returned but not on the list**. Rate is 12 000 Hz, duration 15.00 s
and sample count 180 000 for every row, so those three are stated once here rather than repeated.

```
file                    cand  par  crc  txt uniq  exp  match miss  extra
191111_110115.wav         24    0    0    0    0    1      0    1      0
191111_110130.wav         40   10   10    8    4    5      4    1      0
191111_110145.wav         32    5    5    3    1    2      1    1      0
191111_110200.wav         37   11   11   11    4    5      4    1      0
191111_110215.wav         44    9    9    6    2    4      2    2      0
191111_110615.wav        140   54   54   52   16   22     16    6      0
191111_110630.wav        140   39   39   39   12   15     11    4      1
191111_110645.wav        140   48   48   44   15   20     15    5      0
191111_110700.wav        140   46   46   44   13   16     13    3      0
20m_busy/test_01.wav     140   50   50   39   15   24     13   11      2
20m_busy/test_02.wav     140   56   56   36   13   24     13   11      0
20m_busy/test_03.wav     140   33   33   33   12   19     12    7      0
20m_busy/test_04.wav     140   49   49   45   14   20     13    7      1
20m_busy/test_05.wav     140   54   54   52   19   32     19   13      0
20m_busy/test_06.wav     140   49   49   41   17   27     17   10      0
20m_busy/test_07.wav     140   56   56   43   15   31     15   16      0
20m_busy/test_08.wav     140   53   53   45   15   19     14    5      1
20m_busy/test_09.wav     140   49   49   42   16   27     16   11      0
20m_busy/test_10.wav     140   54   54   48   16   20     14    6      2
20m_busy/test_11.wav     140   52   52   45   16   31     16   15      0
20m_busy/test_12.wav     140   47   47   41   12   18     12    6      0
20m_busy/test_13.wav     140   56   56   44   16   26     16   10      0
20m_busy/test_14.wav     140   44   44   27   10   17     10    7      0
20m_busy/test_15.wav     140   63   63   50   16   28     16   12      0
20m_busy/test_16.wav     140   44   44   41   15   16     14    2      1
20m_busy/test_17.wav     140   53   53   44   15   26     15   11      0
20m_busy/test_18.wav     140   47   47   31   11   20     11    9      0
20m_busy/test_19.wav     140   64   64   49   17   30     17   13      0
20m_busy/test_20.wav     140   51   51   35   12   20     11    9      1
20m_busy/test_21.wav     140   52   52   40   15   34     15   19      0
20m_busy/test_22.wav     140   58   58   36   12   23     12   11      0
20m_busy/test_23.wav     140   51   51   35   13   26     11   15      2
20m_busy/test_24.wav     140   52   52   33   12   22     11   11      1
20m_busy/test_25.wav     140   55   55   47   17   28     17   11      0
20m_busy/test_26.wav     140   49   49   34   12   23     12   11      0
20m_busy/test_27.wav     140   53   53   42   15   29     15   14      0
20m_busy/test_28.wav     140   43   43   28   11   25     11   14      0
20m_busy/test_29.wav     140   50   50   38   14   23     12   11      2
20m_busy/test_30.wav     140   56   56   47   15   27     15   12      0
20m_busy/test_31.wav     140   53   53   40   14   24     12   12      2
20m_busy/test_32.wav     140   57   57   49   19   25     17    8      2
20m_busy/test_33.wav     140   56   56   45   14   28     14   14      0
20m_busy/test_34.wav     140   48   48   34   12   25     12   13      0
20m_busy/test_35.wav     140   60   60   43   13   32     13   19      0
20m_busy/test_36.wav     140   45   45   32   11   24     11   13      0
20m_busy/test_37.wav     140   54   54   47   13   24     13   11      0
20m_busy/test_38.wav     140   40   40   35   11   19     11    8      0
websdr_test1.wav         140   35   35   35   13   18     13    5      0
websdr_test10.wav        113   30   30   30   12   15     12    3      0
websdr_test11.wav        140   44   44   32   10   23     10   13      0
websdr_test12.wav         99   12   12   12    7   14      6    8      1
websdr_test13.wav        140   36   36   35   12   13     10    3      2
websdr_test2.wav         140   50   50   46   18   21     18    3      0
websdr_test3.wav         134   28   28   27    8   11      8    3      0
websdr_test4.wav         140   61   61   55   19   23     18    5      1
websdr_test5.wav         140   49   49   41   15   27     15   12      0
websdr_test6.wav         140   57   57   57   20   30     20   10      0
websdr_test7.wav         140   49   49   47   17   27     16   11      1
websdr_test8.wav         140   55   55   53   17   26     17    9      0
websdr_test9.wav         140   50   50   49   13   24     13   11      0
TOTAL                   7803 2733 2733 2263  783 1298    760  538     23
```

### The three numbers, in words

**MATCHED OUT OF EXPECTED: 760 of 1298**, which is 58.6 per cent, across 60 files. **538 missed.**

**RETURNED BUT NOT ON ANY LIST: 23**, out of 783 returned. Every one is printed in full by the test:
`JH1AJT W4FGA EM83`, `OE3MLC G3ZQQ 73`, `JO1COV PA0CAH JO21`, `CQ MM0IMC IO75`, `RW6PA UA3NFG 73`,
`CQ LZ365BM`, `SP9LKP F4VTS 73`, `UR7HN UA3NFG LO28`, `CQ 2E0LDW IO70`, `YC6RMT IZ7NLM -22`,
`7Z1AL DF2FE JO51`, `CQ G0OSK IO91`, `DM2DLG UR7HN -13`, `<LZ365BM> US5IQI KN87`, `DM2DLG UR7HN -13`,
`RA3TPE BD8NBG -17`, `DH1NAS UA3NFG LO28`, `E75C RA9UJP NO25`, `CT7AIX WG5D EM62`, `CQ 2E0PKK IO90`,
`CQ N2BJ EN61`, `UT7IS SV8EUB -12`, `SQ7MRR ON7AN JO20`.

**FILES THAT PRODUCED NOTHING: 1**, and the stage it died at is named — `191111_110115.wav`, **24
candidates found and none of them reached parity.** Its expected list holds one message.
**No file was skipped for its sample rate.**

### Criterion 3: PARTIAL, on rung 1, and that is said in those words

It is not met. Messages came back off real air and matched a list this project did not write, which
is more than this phase has ever had, and 58.6 per cent is not *matching its expected decode lists*.
**A partial is not a pass and this report does not let it read as one.**

### Where the misses die, and where they do not — three checked-in diagnostics

**One. The lists were not written by the pinned decoder.** 1078 of the 1298 expected lines carry an
SNR column below +5.0; the column runs −24.0 to +20.0. `decode_ft8` computes `snr = score * 0.5f` and
`ftx_find_candidates` refuses a score below `kMin_score` = 10, **so +5.0 is the lowest it can print.**
Some lines also carry a trailing country annotation its `printf` does not emit. They are a stronger
reference than the code being ported, and a shortfall against them is not by itself evidence that
this port is worse than `ft8_lib`.

**Two. Eight times the candidate list buys nothing.** Over a sixth of the recordings, limits of 140,
280, 560 and 1120 all return **117 matched of 183 with 6 extras**; candidates rise from 1257 to 1748
and stop. The cap is not what costs the match rate. The default remains upstream's 140.

**Three, and it is what the next unit needs. 509 of the 531 misses — 95.9 per cent — had a kept
candidate within 4 Hz of the frequency the expected list gives for them.** 22 had none. The search
found the place and the message was not recovered from it, which points at extraction fidelity or at
the code's correcting power at real signal levels, and away from unit 214's search. The candidate
limit bound on 52 of the 60 files.

### Task 4 — the whole path on audio this library made

**The corpus, one transmission per slot.** 56 messages. **51 came back as themselves.** The 5 that
did not are the hashed-callsign entries, and they are counted **by agreeing about the refusal** —
step 2's own decoder, given the same 77 bits with no cache, refuses them too. 1341 candidates over
the 56 slots, 159 reached parity, 159 passed the checksum, 143 became text, 51 unique.
**WRONG MESSAGES: 0 out of 56.**

**The offset sweep.** Four frequencies — a bin centre, a quarter bin up, exactly half a bin up, three
quarters of a bin up — crossed with six offsets — on the block grid, three whole blocks, five
sub-blocks, half a symbol, 5000 samples and 12345 samples, the last four off the block grid and three
of them off both. **288 of 288 at every one of the 24 combinations. WRONG MESSAGES: 0 out of 288.**

**In seeded noise**, seed 216 004, at a **delivered −9.961 to −10.028 dB measured rather than
requested**: **51 of 51. WRONG MESSAGES: 0 out of 51.** The ratio is stated and is not compared with
any published sensitivity figure.

**De-duplication watched working:** one strong transmission gives 34 candidates, 3 decodes, 2
duplicates suppressed and **exactly one message**.

**Determinism, on the messages and their order and never on a count:** five runs over a six-signal
slot — a fresh decoder, one reused twice, and one handed the waterfall instead of the samples —
equal on the text *and* the candidate at every position and on all five stage counts.

### Task 3 — the 174-of-174 hard-decision check, and the alignment

**56 of 56 corpus messages extract at 174 of 174**, at four frequencies and four offsets, taken at
the candidate the search ranked first and **before a single bit of correction is involved.** Worst
agreement 174, mean 174.00.

**The alignment unit 214 carried forward is settled by measurement.** At the candidate: 174 of 174.
One block earlier 103, one block later 105. Two blocks earlier 100, two later 98. The wrong time
sub-offset 139. One bin low 113, one bin high 131. The wrong frequency sub-offset 149. **Chance is
87.** There is exactly one place it works and the search puts the candidate there. Reading upstream
adds the structural half: `ft8_sync_score` and `ft8_extract_likelihood` both enter through
`get_cand_mag`, so the two cannot disagree, and the port keeps that by sending both through
`Ft8Waterfall.IndexOf`.

### The variance before and after normalisation

Against **24**, the figure task 2 read out of `ft8/decode.c` and
`Ft8SoftSymbolsProvenanceTests` binds the port's constant to.

```
input magnitude   variance before   variance after   mean before   mean after
          0.250            0.0595          24.0000       -0.0546      -1.0964
          1.000            0.9523          24.0000       -0.2184      -1.0964
          4.899           22.8555          24.0000       -1.0699      -1.0964
         20.000          380.9222          24.0000       -4.3678      -1.0964
        100.000         9523.0547          24.0000      -21.8391      -1.0964
```

Every sign is untouched, so the hard decision is unchanged by the rescale, and **the mean is scaled
rather than removed**, which is what upstream does. Off the air the raw ratios are differences of
decibels tens of dB apart, so their variance before normalisation is far above 24 and the rescale is
doing real work rather than a rounding.

### Task 6 — criterion 2's four gate counts, in the candidate sense

Every input below went in **as audio** and was found by the search without being told where it is.

1. **An empty slot:** 0 candidates, **0 messages.**
2. **Seeded Gaussian noise alone, 20 slots, seeds 216 601 to 216 620, rms 0.02:** **239 candidates
   found**, best sync score 15, 0 reached parity, 0 became text, **0 messages returned.**
3. **51 genuine transmissions whose checksum was made wrong before the parity bits were computed**,
   synthesized at five frequencies: 893 candidates, **114 reached parity with ZERO unsatisfied checks
   out of 83 — some in a single iteration — so every one is a genuine member of the code the parity
   gate has nothing to object to.** 0 passed the checksum. 0 became text. **0 of 51 returned anything
   at all.** The fixture that builds them is checked against the library's own encoder, 4424 symbol
   comparisons over 56 messages all equal, so the only thing wrong with one of them is its checksum.
4. **51 transmissions at a delivered −30.043 to −29.972 dB:** 603 candidates, 0 reached parity,
   **0 wrong text out of 51.**

**Criterion 2 is now claimed in the candidate sense, and what changed since unit 215 is that a
candidate exists:** that unit met it at the codeword entry point because extraction did not exist and
nothing that had been near a radio could reach the gate; tonight every one of these went in as audio.

### Task 2 — the shapes, the anchoring split, the inventory and the rung

**Anchoring: 7 strong, 8 weak, 3 weakest.** Strong — the extraction and decode entry points, the
waterfall struct and its documented axis order, the two magnitude macros, the data and channel symbol
counts, the codeword length, the Gray map declaration. Weak, every one an expression inside a static
function body — the candidate's fold into an offset, the `k + (k<29 ? 7 : 14)` step, the zero-fill for
an out-of-range block, the value-order gather, the three bit partitions, the variance formula, the
extract-normalise-decode order, the payload comparison. Weakest — the normalisation's target
variance, whose own comment calls it an *experimentally found coefficient*, and the four application
constants.

**The shapes, as shapes.** A candidate indexes the store in the store's own axis order. The sync
blocks are **stepped over, not through**. A symbol whose block falls outside the waterfall gives
**three zero ratios** — no opinion, not a refusal. The magnitudes are read as **decibels** where the
scorer reads the raw byte. Each ratio is a maximum over the four values whose bit is one less a
maximum over the four whose bit is zero. The normalisation takes the population variance of all 174
with the mean removed *from the variance*. **One attempt per candidate** — no retry at neighbouring
offsets. Duplicates are decided on the whole packed payload, and the text is produced only after.

**`ft8_decode_multi_symbols` is read and deliberately not ported.** It is declared once, defined once
and **never called**; the inventory test asserts the mention count is exactly two, so a re-pin that
starts calling it goes red.

**The WAV and expected-list inventory.** `test/wav` and `test/wav/20m_busy` hold **69 recordings**.
**60 carry a `.txt` expected decode file named for the recording**, holding **1298 expected messages**
in the format `decode_ft8` prints. All 60 are mono, 16-bit, **12 000 Hz**, 15.00 s, 180 000 samples.
The 9 without a list are `websdr_test14` through `websdr_test20` at **6400 Hz** and two 12 kHz
re-samples of two of them. **141 of the 1298 expected lines name a station by an unresolved hash**,
printed `<...>`.

**RUNG 1 of the ladder** — a checked-in expected-decode file beside the recordings. Nothing was
invented and no lower rung was needed.

**Hashed callsigns: compared like any other line, not excused.** Upstream's own hash table produced
`<...>` from the same recording, so both sides are in the same position.

**The text normalisation, applied identically to both sides, stated exactly:** the message is
everything after the first tilde, trimmed, up to a run of **two or more spaces**. **Nothing else.** No
brackets stripped, no case folded, `RR73` and `RRR` stay different messages. The two-space rule exists
because 85 lines carry a trailing country annotation and an FT8 message is single-space separated
between tokens.

### Upstream's four decode constants, and whether this library matched

```
constant                where                      upstream   this library                match
kMin_score              demo\decode_ft8.c                10   Ft8SyncSearch.DefaultMinimumScore     yes
kMax_candidates         demo\decode_ft8.c               140   Ft8SyncSearch.DefaultCandidateLimit   yes
kLDPC_iterations        demo\decode_ft8.c                25   LdpcDecoder.DefaultMaxIterations      yes
kMax_decoded_messages   demo\decode_ft8.c                50   Ft8SlotDecoder.DefaultMessageLimit    yes (new)
```

**All four match. None of the four appears anywhere in `ft8/`** — all are the application's choices.
**Nothing was tuned and nothing needed correcting.** The fourth had no counterpart in this library
before tonight because nothing here returned a *list* of messages.

### Every refusal watched refusing

**Extraction, 16 refusals**, each with both numbers in the message: a null waterfall; output spans of
0, 173, 175 and 348; bin offsets of −1, 442 and 449, the middle one being the case whose eighth tone
is one bin past the end; a time sub-offset of 2 where there are 2 subdivisions; a frequency
sub-offset of −1; normalisations over 0, 173 and 175 ratios; seven tone magnitudes; four bits from
one symbol; a hard decision into a span of a different length.

**And the ones that must not refuse:** bin offset **441**, the highest legal one, extracts — so the
bound is exactly where the message says it is; and **a block offset of −10 extracts**, with 78 of 174
ratios zero, because those symbols fall before the slot and the search sweeps there on purpose.

**The path, 4 refusals:** a negative message limit, a negative iteration count, a null waterfall,
audio shorter than one block. **And the one that must not refuse:** a message limit of **0** runs the
whole path, decodes 3 candidates, and returns 0 messages.

### Task 7 — twenty at once, not dropped

```
                    cand  par  crc  txt  dupes  unique  themselves  missed  extra
clean                140   51   51   51     31      20       20/20       0      0
noise -10.020 dB     140   45   45   45     25      20       20/20       0      0
```

Unit 214's own fixture — twenty different messages from 300 Hz to 2772 Hz, every one at a different
fraction of a bin, at five different start offsets, summed into one buffer.
**Twenty become twenty, twice, with nothing extra.** The branch that licensed dropping it applied and
it was run anyway; two seconds bought the finding that the reference-recording shortfall is not about
having more than one signal in a slot.

### Divergences added, versions, and the tree

**Three divergences, numbered on from twenty-one.** **22** — a candidate whose eighth tone falls
outside the kept bins is refused, where upstream reads past the end of its array. **23** — 174
identical ratios are left alone, where upstream computes `sqrtf(24/0)` and multiplies the array by an
infinity. **24** — a full message list stops accepting, where upstream's own duplicate probe never
terminates. Plus one **addition that is not a divergence** — `Ft8SlotResult`'s five stage counts,
which change no decision — and one **test-project change that is not one either**, `WavFile` walking
to the `data` chunk.

**Versions.** `src/Ft8Sharp/Directory.Build.props` **0.9.0 → 0.10.0** under HM-DEC-152, with the note
saying what it does not claim. Root `Directory.Build.props` **1.12.22 → 1.12.23** under HM-DEC-150.
**Re-run after both bumps: Ft8Sharp 470 total, 469 passed, 0 failed, 1 skipped in 35 s**, 41 tests
added and still the one correct skip — `Ft8TableGenerationTests.RewriteTheCheckedInTablesFile`, the
table write gate, whose reason names the environment variable that would run it. Channels **55** and
**13**, `VersionTests` among them. Library 0 warnings, 0 errors. **168 paths from `2828ab6` and the
`src/Hamlet.`/`tests/Hamlet.` filter returns 0.** **8 `.obj` at the root, untouched.**
`git status --short` prints **33**.

**No new shared artifact was added, so no new channel was needed.**

### What was committed, and what was left alone

**Committed:** `PROJECT_STATUS.md`, `PHASE_STATUS.md`'s `WORK_INSTRUCTION:` line, the two
`Directory.Build.props`, `src/Ft8Sharp/porting-notes.md`, two new files under `src/Ft8Sharp/Dsp/`,
seven new files under `tests/Ft8Sharp.Tests/Dsp/`, and one modified file under
`tests/Ft8Sharp.Tests/Encode/`. **Nothing else.**

**Left alone, every one of them deliberately:** the 8 `.obj` at the root; `tools/build-ft8-oracle.bat`
and everything under `tools/`; `ARBITER.md`, `MANIFEST.txt`, `PHASE_PLAN.md`, `RUN_LEDGER.md`,
`VERIFY_PASS.md`, `SCRUB_SELFTEST.bat`, `SESSION.lock`, `.run-unit/`, `docs/phase-uplift/`;
`PHASE_OUTCOME.md` and the two `ANALYSIS-cw-*` files and `PROJECT_CARD.md` and `WORK_INSTRUCTIONS.md`;
`OPEN_ISSUES.md`, unchanged and correctly so; every WAV and every byte of the clone; and every file
under `src/Hamlet.*` and `tests/Hamlet.*`.

**OPEN_ISSUES.md is unchanged and that is the expected answer.** Step 5 carries no nice-to-pass
criterion, so nothing is owed there. The one exception the instruction names — the clone carrying no
expected decode list — does not apply, because task 2 found 60 of them.

### Mismatches against this instruction, reported and not repaired

**One. Extraction does not need an inverse Gray map, and the instruction says it does.** It says
*extraction needs the inverse map, tone → value; derive it in code from `Ft8GrayMap`*. Upstream
indexes its eight magnitudes **by symbol value through the forward map** — `s2[j] = mag[GrayMap[j]]` —
so the array is already in value order and the three bit tests read straight off it. **No inverse map
exists anywhere in upstream's decoder and none was built.** The forward map is used from
`Ft8Tables.Ft8GrayMap` in code, as required, and nothing was transcribed.

**Two. `WavFile` could not read 9 of the 60 recordings.** The instruction says *this is how a WAV is
read, do not write a second reader*, and the reader as it stood refused a file whose `data` chunk is
not second. Reported, and the repair chosen is described in section 1 as a decision made for myself.

**Three. `git status --short` printed 29 at entry where the instruction says 28**, and 33 at exit.
The extra untracked file at entry is `SESSION.lock`.

**Four, and it is not a mismatch but is worth stating:** every one of the sixteen known items held.
The 8 `.obj` are still there and still untouched. `tools\build-ft8-oracle.bat` is present, untracked
and was not run. `PHASE_STATUS.md` still reads `CURRENT_STEP: 4` against an outcome file recording
step 4 done and step 5 partial, and was not hand-edited beyond the one line that is mine.
`Ft8Sharp.Tests` still carries exactly one skip and it is the table write gate. `TempEncoderProbe.cs`,
`UpstreamSyncSearchProbe.cs`, `UpstreamLdpcProbe.cs` and `unit215-section.md` are all still on disk; a
thirteenth attempt to delete them was not made. **This unit leaves two more of the same kind** —
`tests/Ft8Sharp.Tests/Dsp/Unit216Probe.cs`, emptied to a comment so that what is on disk and what is
committed compile to the same tests, and `unit216-section.md` at the root, the scratch copy of the
porting-notes section. Both are untracked.

### The validator was refused for the sixth unit running, and the refusal is reported as a refusal

**All five spellings `tools\arbiter\run-unit-tools.txt` lists were tried and none of them ran the
script.** `tools\arbiter\validate-output.bat output.md`, `cmd //c tools\...` and
`cmd.exe //c tools\...` all failed with *`toolsarbitervalidate-output.bat` is not recognized* — **the
shell strips the backslashes before `cmd` ever sees the path.** `cmd /c tools\...` and
`cmd.exe /c tools\...` printed the Windows banner and the contents of `.run-unit\prompt.txt` instead,
because without the `//` escape the `/c` is consumed as a path. Two further spellings with forward
slashes were denied by the permission layer. **No route around was attempted** and nothing was copied,
renamed or re-pathed to make it run. This is the same refusal units 211 to 215 reported, and unit 213
measured why.

**So the six rules were checked by hand against the script's own source, which was read**, and all
six pass. **Rule 1** — a parseable `UNIT:` line at file line 40, inside the first 60 the script reads
and above `## 1.` at line 54. **Rules 2 and 3** — exactly four `## ` headings at lines 54, 135, 170
and 535, in order, with the exact names the script's `WANT` string holds, and no fifth; `###` and
deeper are ignored by the script's own stated reading. **Rule 4** — `## 4. What's blocking us` is
present, with a straight ASCII apostrophe, which is what its `findstr /b /c:` needs. **Rule 5** —
section 3 runs from line 170 to line 534 and is very far from empty. **Rule 6** — `READ IN THIS ORDER`
at line 1, `A.` at line 3, `B.` at line 12, `C.` at line 28, and *Section 4 raises 3 items* at line
35: all five inside the first 60 lines, all above the `UNIT:` line, each of `A.`, `B.` and `C.`
beginning its line with no indentation, and **the count in `C` written as a digit.**

## 4. What's blocking us

**Three items. One is in the way of a criterion in B; the other two are carried forward, and none is
a ruling request.**

**1. Criterion 3 is partial at 760 of 1298, and the cause is narrowed but not settled.** This is the
one in the way. What is known: the search is not where the misses die — **509 of the 531 misses had a
kept candidate within 4 Hz of the listed frequency**; the candidate limit is not the cost — eight
times the list changes nothing; overlap is not the cause — twenty clean overlapping transmissions all
decode; and the reference itself is stronger than the code being ported — 1078 of 1298 expected lines
carry an SNR the pinned decoder cannot print. What is not known is **whether the remaining gap is
this port falling short of `ft8_lib`, or `ft8_lib` itself falling short of whatever wrote those
lists.** Step 6 depends on the answer, because a sensitivity measurement against the published
threshold is meaningless if the path is already leaving decodes on the table for a reason nobody has
named. **The single thing that would separate the two is `decode_ft8.exe` run over the same
recordings** — which is `HM-OPEN-065`, a standing item and known item 4, raised here not as a new
finding but because this is the first unit whose result actually turns on it.

**2. What wrote the expected decode lists is unread and unreadable from the clone.** They are in
`decode_ft8`'s print format, they carry an SNR column it cannot produce, and some carry a country
annotation it does not emit. Nothing in the clone says which program, which version or which machine
wrote them. They are treated as upstream's claim about its own recordings, which is what criterion 3
asks for, and **not** as ground truth about what was transmitted. **This is why the 23 messages
returned that are not on any list are reported as extras but are not proven to be false decodes.**

**3. The push of task 8's commit was refused by the remote, twice, with an Internal Server Error.**
The first seven commits are on `origin/main`. The eighth — `29e82cf`, the porting notes, both version
bumps and the final status — is local only. It was not worked around. `git push` on `main` should
land it when GitHub recovers.
