READ IN THIS ORDER — A, then B, then C.

A. PHASE — Hamlet hears FT8 off the radio and displays the decoded text on screen. Seven steps.
Step 1 (the library exists and its tables are proven) and step 2 (messages round-trip through 77
bits) are CLOSED. Step 3 (a valid FT8 signal can be produced) is CLOSED on its four must-pass
criteria; its nice-to-pass criterion is NOT MET and is recorded by name in OPEN_ISSUES.md as
HM-OPEN-065, the reference decoder that is not built on this machine. Step 4 (signals are found in
noise) is THIS UNIT'S and this is its SECOND unit. It ENTERED at 0 of its three subject criteria —
unit 213's own report says so in those words — and it LEAVES AT 3 OF 3. Steps 5 (a found signal
becomes a message), 6 (sensitivity meets the published threshold) and 7 (Hamlet displays decoded
FT8) are NOT STARTED. Step 4 was the only step this phase could move, because every step depends on
the one before it and steps 1 to 3 are done while 5, 6 and 7 all need a list of places to point a
decoder at, which did not exist this morning.
B. STEP 4 — signals are found in noise. FIVE exit criteria, ALL FIVE MUST-PASS, ALL FIVE MET.
(1) A synthesized signal at a known offset and time is found — MET. 56 OF 56 messages of the corpus
found, AT RANK 1 IN 56 OF THEM, on clean slots at frequencies rotating over a bin centre, a quarter
bin off and EXACTLY HALFWAY BETWEEN TWO BINS, at offsets on the block grid, the sub-block grid and
neither. Frequency error at worst 1.5625224 Hz, tolerance asserted AFTER the print at half a bin
plus 0.001 Hz for the geometry's own single-precision symbol period. Mean signed time error
+0.158936 s = 0.993 blocks, a constant bias; residual at worst 0.0156 s against a tolerance of half
a block. In noise at a DELIVERED -10.001 dB, measured not assumed: 56 of 56, rank 1 in 56. THE
SEARCH WAS GIVEN THE SAMPLES AND THE GEOMETRY AND NOTHING ELSE.
(2) Twenty simultaneous synthesized signals across the passband are found — MET. 20 OF 20, on a
clean slot and again with noise over the whole passband at a delivered -10.009 dB per transmission.
The list must be read 22 DEEP to cover all twenty. None missed, so none is named.
(3) Candidate ranking is stable across runs — MET. Compared ON THE VALUES AT EVERY POSITION and
never on the count: two independent runs (680 and 75 field comparisons, all equal); a fresh monitor
against one reused after Reset(); one search instance used six times alternating between two slots;
and the one that catches an unstable sort — all 53040 hypotheses re-enumerated in REVERSED order and
in a SEEDED SHUFFLE, giving the search's own list element for element. Not the FFT's or the
waterfall's determinism, which unit 213 measured and called the foundation.
(4) Ft8Sharp tests green — ENTRY 348 total, 347 passed, 0 failed, 1 skipped in 11 s; EXIT 394 total,
393 passed, 0 failed, 1 skipped in 19 s, re-run after both version bumps. 46 tests added. The one
skip at entry and at exit is Ft8TableGenerationTests.RewriteTheCheckedInTablesFile, the table write
gate, which is meant to skip. No new skip: every clone-reading test found the clone.
(5) Attribution clean from 2828ab6 and the channel tests green — 147 paths, NOT ONE under
src/Hamlet.App/, src/Hamlet.RadioEngine/, tests/Hamlet.App.Tests/ or tests/Hamlet.RadioEngine.Tests/;
AudioSeamTests and PrivilegeTests green at 55; DecisionLogOrderTests, VersionTests,
DecisionEmissionTests and VoiceTests green at 13; both re-run after the bumps.
C. THIS REPORT — THE SEARCH FOUND SIGNALS IT WAS NOT TOLD ABOUT, AND IT FOUND THEM AT RANK 1: 56 of
56 messages, first in the list every time, out of a slot of audio handed over with no frequency, no
offset and no alignment, through a signature that has no parameter one could be passed through — a
fact asserted by reflection rather than by inspection. Twenty simultaneous transmissions across the
passband: 20 of 20, twice, with the list 22 deep. The ranking is stable, and it is stable in the
strong sense: the same list comes back when the hypotheses are generated in reversed or shuffled
order, which is the comparison upstream's own heapsort could not pass, because ITS ORDER IS NOT A
TOTAL ORDER — every comparison in its heap is on the score alone, and this port measured 2976 tied
adjacent pairs in a list of 3000. That is divergence 19. Task 7 was NOT DROPPED although the FIRST
branch of its condition licensed it — tasks 4, 5 and 6 all ran and produced their measurements — and
it was run anyway because it cost 14 seconds and it is what step 6 will start from.
Section 4 raises 1 item, it is NOT a ruling request, and it is NOT in the way of a criterion in B.

UNIT:       214 — complete at task 8 of 8 — 2026-09-01 23:47
DATE:       2026-09-01
STATE:      COMPLETE
TASKS:      8 of 8
DROPPED:    none — task 7 was the named candidate, its first branch licensed dropping it, and it
            was run anyway; see section 3
PHASE GOAL: Hamlet listens to the radio, finds FT8 transmissions in the audio, and puts the words
            they carry on the screen.
UNIT GOAL:  Build the Costas sync correlation and the candidate search over the waterfall, with a
            deterministic ranking, and use it to find signals whose frequency and time were never
            handed to it.
ADVANCED:   yes — criteria 1, 2 and 3 of step 4, all three subject criteria, all met and all
            measured: 56 of 56 at rank 1, 20 of 20 across the passband, and a ranking that survives
            being generated in a different order. Criteria 4 and 5 re-taken.
NUMBER:     step 4 subject criteria met: 0 -> 3 of 3. Corpus found out of a slot with no hint:
            none -> 56 of 56 at rank 1, and 20 of 20 simultaneous.
DRIFT:      0 consecutive units without advance  (was 1 — unit 213 built the substrate and advanced
            no criterion, and said so)

## 1. What Claude did

**COMPLETE, at task 8 of 8.** This machine, `C:\Source\HamLet`, branch `main`, gated on
`PROJECT: Hamlet` and verified against the tree before the work instruction was read: `SHACK_FACTS.md`
and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` both present, `CoreHMI.sln` and `MURC.sln`
both absent, `Hamlet.sln` the only solution.

**Nothing was dropped and nothing was left unreachable.** Task 7 was the named drop candidate; the
first branch of its condition licensed dropping it, and it was run anyway. That is recorded here
rather than presented as diligence: it was a sizing decision, and the reason it was safe is that the
whole sweep cost 14 seconds.

### What was traced, built and measured

**Task 1 — the ground, measured before anything was changed.** `Ft8Sharp.Tests` at entry 348 total,
347 passed, 0 failed, 1 skipped in 11 s, which is exactly what unit 213 reported. The one skip is
`Ft8TableGenerationTests.RewriteTheCheckedInTablesFile`, the table write gate, and it is meant to
skip. The library builds at 0 warnings and 0 errors; `Ft8Sharp.csproj` still reads `net8.0`, nullable
enabled, warnings as errors, and has no `PackageReference` and no `ProjectReference`. 137 paths from
`2828ab6`, and the filter for `src/Hamlet.` and `tests/Hamlet.` returned nothing. Channels 55 and 13.
`HEAD` `58c1563`, `git status --short` 25 lines, 8 `.obj` at the root, versions 1.12.20 and 0.7.0, 18
divergences in `porting-notes.md`. The four substrate classes green at 113, and the geometry
re-derived from `Ft8WaterfallGeometry` rather than taken from the prior report: block 1920, sub-block
960, transform 3840, 93 blocks, bins 32 to 481 which is 449, stride 1796, 167028 magnitudes, 3.125 Hz.

**Task 2 — upstream's search, read through the test process and pinned by ten tests.** The sandbox
refused the session `C:\Source\ft8_lib` directly, exactly as it refused the arbiter, so every line of
it was read by `dotnet test`. The result is `UpstreamSyncSearchInventoryTests`, which skips when the
clone is absent.

**Task 3 — the correlator and the search.** `src/Ft8Sharp/Dsp/Ft8SyncSearch.cs` and
`Ft8Candidate.cs`, new. The scoring is upstream's `ft8_sync_score` term for term and guard for guard;
the sweep is upstream's; the selection and the ordering are not, and both are recorded as
divergences. 17 tests, including seven refusals watched refusing.

**Task 4 — the target.** `Ft8SearchRecoveryTests`, 7 tests. The measurement is in section 3.

**Task 5 — twenty at once.** `Ft8SearchPassbandTests`, 3 tests, including the depth-against-limit
table.

**Task 6 — the same input, the same ranking.** `Ft8SearchStabilityTests`, 8 tests, every comparison
on values and never on counts.

**Task 7 — how far down it still hears.** `Ft8SearchSensitivityTests`, one sweep over nine ratios.

**Task 8 — the record.** `porting-notes.md` gains its unit-214 section; `OPEN_ISSUES.md` is
unchanged; `Ft8Sharp` 0.7.0 → 0.8.0 and the root 1.12.20 → 1.12.21; and this report.

### Decisions this session made for itself, reproduced in full

**One, and it is a test-design decision rather than a ruling.** The task 2 inventory test prints the
demo application's minimum score and candidate limit rather than asserting them as literals, and the
assertion that binds them to the port's own defaults lives in a separate class,
`Ft8SyncSearchProvenanceTests`. The reason: those two numbers are upstream's application's to own, so
the claim worth asserting is not *upstream says 10* but *this port's default is still whatever
upstream's application says*. It also lets the record of the read compile and answer on its own,
before the port it describes exists. Reproduced here because it changes where a reader looks.

## 2. What the owner should expect

**`Ft8Sharp` can now be handed fifteen seconds of audio and asked what is in it.** It answers with a
ranked list of places — a frequency, a time and a strength for each — and the list is the same list
every time for the same audio.

**Nothing decodes, and nothing will until step 5.** A candidate is a place, not a message. There is
no demodulator in this tree, no soft symbol, no belief propagation and no CRC check on a received
signal, so no text comes out of a radio yet and none will tonight.

**Nothing about Hamlet itself changed.** No screen is different, no panel is new, nothing behaves
differently when the application runs. The attribution filter from `2828ab6` returns nothing under
`src/Hamlet.*` or `tests/Hamlet.*` for the fifteenth unit running, and that is deliberate.

### What will look wrong but is not

**The reported start time of every signal is one block late, by 0.16 s, every time.** That is
measured, named and reported as a constant bias rather than corrected, because correcting it would be
a guess about what upstream's own block index means in samples — the one thing task 2 could not read.
Step 5 is what will settle it, against a decode that either works or does not.

**The candidate list is full of near-duplicates and that is expected.** A transmission is a couple of
bins wide and half a block long in this geometry, so several neighbouring hypotheses score well on
the same energy. Twenty signals produced a list where 20 to 27 entries were duplicates of a signal
already covered. It is not a defect; it is what the depth number in section 3 is for.

**`src/Hamlet.RadioEngine/Audio/Ft8Sync.cs` still exists and now has a rival.** This repository
carries 289 lines of Hamlet's own Costas sync search from work instruction 042. It was not read for
structure, not copied, not referenced and not edited, and the reason is in `porting-notes.md`. There
are now two of these in the tree. **What becomes of Hamlet's own copy is step 7's question**, and
raising it earlier would put a Hamlet path into this phase's attribution filter.

**The library version and the application version disagree, still.** 0.8.0 and 1.12.21. That is
HM-DEC-152 working as intended.

## 3. What you should see

**The task 4 measurement, before any prose.**

```
56 OF 56 MESSAGES FOUND, AT RANK 1 IN 56 OF THEM     (clean slots, whole corpus)
  ranks:  worst 1, mean 1.00
  scores: worst 31, mean 35.0

  base frequencies, rotating over the corpus:
    1000.000000 Hz   on a bin centre
    1001.562500 Hz   EXACTLY HALFWAY BETWEEN TWO BINS   (unit 213's 4.5 dB case)
    1000.781250 Hz   a quarter of a bin off
  slot offsets, rotating over the corpus:
    0        on the block grid
    5760     3 whole blocks
    4800     5 sub-blocks - off the block grid
    5000     off both grids
    12345    off both grids

  frequency error     worst |e|  1.562522 Hz   mean |e| 0.781257 Hz   MEAN SIGNED  0.223237 Hz
  time error          worst |e|  0.171250 s    mean |e| 0.158936 s    MEAN SIGNED  0.158936 s
  time error          worst |e|  2055 samples  mean |e| 1907 samples  MEAN SIGNED  1907 samples
  time residual       worst |e|  0.015603 s    mean |e| 0.006130 s    MEAN SIGNED  0.000000 s

  THE MEAN SIGNED TIME ERROR IS +0.158936 s, WHICH IS 0.993 BLOCKS.

  TOLERANCES ASSERTED, AFTER THE NUMBERS THEY WERE SET FROM:
    frequency  half a bin, 1.5625000 Hz, plus 0.001 Hz for the geometry's own
               single-precision symbol period = 1.5635000 Hz.  Worst measured 1.5625224 Hz.
    time       within half a block, 0.080 s, of the constant bias.  Worst measured 0.015603 s.

IN NOISE, DELIVERED RATIO MEASURED RATHER THAN ASSUMED:
  requested -10.0 dB in a 2500 Hz reference bandwidth
  DELIVERED -10.001 dB   (worst -10.033, best -9.974)
  56 OF 56 FOUND, AT RANK 1 IN 56.  scores: worst 24, mean 28.7

NOISE ALONE, 20 SLOTS, NO SIGNAL ANYWHERE IN THEM, SAME NOISE LEVEL:
  top score over noise alone     worst 14, mean 11.7
  score at a true signal          31
  a number beside a number:       31 against 14, a margin of 17
  FALSE ALARMS AT THE TRUE SIGNAL'S OWN STRENGTH: 0 of 20

THE SEARCH WAS GIVEN THE SAMPLES AND THE GEOMETRY AND NOTHING ELSE.
```

That sentence is in the test file, in `porting-notes.md` and here, in those words. It is the whole
difference between this unit and the last one, and it is not left to be believed: `Find` has two
overloads and neither has a parameter whose name contains *freq*, *hertz*, *time*, *offset*,
*expect*, *hint* or *truth*, which is asserted by reflection over the signature.
`ToneRecovery.AlignmentFor` — the helper that computes the truth from a known offset — appears in
none of the search's test files.

### The dedicated sweeps, because an average can hide the hard case

```
FREQUENCY, four fractions of a bin at six base frequencies from 300 to 2500 Hz:
  on centre       (+0.00000 Hz)  6 of 6, rank 1 in 6, worst score 38, worst |dF| 0.0001 Hz
  quarter bin     (+0.78125 Hz)  6 of 6, rank 1 in 6, worst score 38, worst |dF| 0.7812 Hz
  HALF BIN        (+1.56250 Hz)  6 of 6, rank 1 in 6, worst score 33, worst |dF| 1.5626 Hz
  three quarters  (+2.34375 Hz)  6 of 6, rank 1 in 6, worst score 37, worst |dF| 0.7813 Hz
  24 of 24 across the whole sweep

TIME, seven offsets:
  0      on the block grid                 rank 1, score 38
  5760   3 whole blocks                    rank 1, score 38
  4800   5 sub-blocks, off the block grid  rank 1, score 38
  3880   off both grids by 40 samples      rank 1, score 37
  5000   off both grids                    rank 1, score 35
  12345  off both grids                    rank 1, score 37
  27913  off both grids                    rank 1, score 37
```

The half-bin case is the one worth looking at. Unit 213 measured the tone-recovery margin falling
from 13.5 dB to 4.5 dB there and carried it forward as the number tonight's thresholds should be set
against. **It cost three points of sync score — 33 against 38 — and nothing else.** All six were
found, all at rank 1.

### Task 5 — twenty at once, and how deep the list has to be read

```
  twenty transmissions, 300.0000 Hz to 2772.3438 Hz, across a 200..3000 Hz passband
  every one at a different fraction of a bin; five start offsets, three of them off both grids
  closest pair 127.6562 Hz apart = 20.4 tone spacings = 40.9 bins - NO TWO OF THEM OVERLAP

  CLEAN SLOT                    20 OF 20 FOUND.  list 140 long, 22 DEEP, 27 duplicates
  NOISE AT -10.009 dB DELIVERED 20 OF 20 FOUND.  list 133 long, 22 DEEP, 20 duplicates
    ranks: worst 22, mean 10.7      scores: worst 25, mean 29.2
    worst |dF| 1.56246 Hz           worst |dt - bias| 0.01158 s

  DEPTH AGAINST THE CANDIDATE LIMIT, which is the demo application's number and not FT8's:
     limit  returned  covered  depth  duplicates
        20        20       19     19           0
        40        40       20     21           1
        80        80       20     21          12
       140       130       20     21          23
       400       130       20     21          23
```

**Nothing was tuned to reach twenty.** *Found* means a candidate within task 4's tolerance exists
somewhere in the list — not that the top twenty candidates are the twenty signals. The depth is the
number step 5 pays for: it is how many decode attempts have to be made before the last of the twenty
is reached. At the default limit of 140 the limit is not the binding constraint; at 20 it is, and one
signal is lost to it.

**No two of the twenty are close enough in frequency to be confused**, and that is stated rather than
left to be assumed — the closest pair is 20.4 tone spacings apart. A fixture with overlapping signals
would be a different and harder measurement, and it is not one step 4 asks for.

### Task 6 — stability, on the values and never on the count

```
  two independent runs, twenty signals in noise   136 candidates,  680 field comparisons, all equal
  two independent runs, one signal                 15 candidates,   75 field comparisons, all equal
  fresh monitor vs one reused after Reset()        both slots, all equal
  one search instance used 6 times, alternating    both slots, all equal every round
  REVERSED generation order, both slots            all equal, element for element
  SEEDED SHUFFLE of all 53040 hypotheses           all equal, element for element
  shortening the list at limits 1, 5, 20, 60, 140  truncates rather than reorders
```

The last two are the ones that mean something. **Two runs of the same code over the same data agree
even when the sort is unstable**, because the generation order is the same both times. Re-enumerating
the whole hypothesis space in a different order and requiring the same answer is the comparison that
does not let that through, and it is exactly the comparison upstream's own search would fail.

### Task 2 — the shapes, and the anchoring split

**STRONG — a macro, a typedef or a header declaration, which cannot be misread (6):** the candidate
record of five fields with an integer score; the search entry point and its four parameters; the
integer accessor that reads a stored magnitude as a count and never as decibels; the waterfall axis
order and block stride; the three-groups-of-seven-thirty-six-apart sync geometry; the declaration of
the seven-tone Costas array.

**WEAK — an expression inside a static function body (6):** the four neighbour difference terms and
their guards; the asymmetry by which a block before the slot is skipped and a block past its end
abandons the group; the integer division by the number of terms actually taken; the block offset
range; the frequency offset bound; the min-heap and the heapsort.

**WEAKEST, named because they are the two numbers that bound the answer:** **the minimum score and
the candidate limit are not in the library at all.** They are file-scope constants in
`demo/decode_ft8.c`, and that neither name appears anywhere in `ft8/` is asserted rather than
assumed. They are one application's judgement about how much sensitivity to trade for how much work,
so `Ft8SyncSearch` exposes both as constructor parameters with the demo's values as defaults.

**Three things named as UNREAD rather than guessed:** what upstream's decoder actually returns for a
slot (the binary is not on this machine, HM-OPEN-065, and a unit may not build one); the exact
alignment between a block index and a sample offset, which reading does not settle and which was
measured instead; and whether upstream's heap order for tied scores is reproducible across compilers,
which is not readable and is not needed, because this port replaces that order rather than
reproducing it.

### The tie-break, and whether upstream's order is total

**It is not.** Every comparison in both of upstream's heap helpers is on `score` and on nothing else,
its sort is a heapsort, and heapsort is not stable. Scores are small integers over tens of thousands
of hypotheses: **2976 of 3000 adjacent pairs in one list tied on score alone.** So where two
candidates tie, upstream's returned order is whatever its heap's swaps left — fixed for one build
over one input, and not a function of the input.

`Ft8Candidate` therefore compares on **score descending, then block offset, then time sub-offset,
then bin offset, then frequency sub-offset, all ascending.** No two distinct hypotheses share all
four position fields, so **no two distinct candidates ever compare equal.** Nothing about that
sequence is claimed to be better than another; what is claimed is that it is fixed, that it exhausts
every field, and that step 5 can therefore rely on the order it reads.

### Every refusal, watched refusing, and by how much it missed

```
  a minimum score no hypothesis reaches   best score on the slot was 38; a minimum of 39
                                          returns 0 candidates. Empty, not an exception,
                                          not a partly filled list.
  a candidate limit of zero               returns 0, does not throw
  more candidates than the slot supplies  asking for 1,000,000 returns exactly the 34
                                          hypotheses that reach the minimum and stops
  no candidate below the minimum, ever    at minimums 0, 10, 25 and 50 the weakest returned
                                          candidate was 0, 10, 25 and 0-of-none
  a negative candidate limit              refused, with the reason
  an inverted block offset sweep          refused: 5 to 4 is empty
  a null waterfall                        refused
  a time sub-offset of 2 (of 2)           refused
  a frequency sub-offset of 2 (of 2)      refused
  a bin offset of 442 (of 0..441)         refused, and 441 SCORES rather than refusing,
                                          so the bound is where it says it is
  a hypothesis outside the analysed blocks  scores 0 rather than throwing
```

### Task 7 — how far down it still hears

```
  20 messages and 5 noise-only slots at each ratio. Delivered ratio measured at every point.

    asked  delivered    found   rate  worst true  mean true  best false  mean false  cands
     -4.0     -4.000    20/20  100.0%         28       32.2          13        11.6     10
     -8.0     -8.000    20/20  100.0%         27       30.3          11        10.8     12
    -11.0    -10.999    20/20  100.0%         24       27.8          12        11.4     10
    -13.0    -12.995    20/20  100.0%         21       25.4          12        11.4     11
    -15.0    -15.001    20/20  100.0%         19       22.8          12        11.6     13
    -17.0    -16.997    20/20  100.0%         16       20.4          12        11.2     12
    -19.0    -18.999    20/20  100.0%         14       16.6          13        12.2     13
    -21.0    -21.001    19/20   95.0%          9       12.7          13        11.8     11
    -24.0    -24.002    11/20   55.0%          6        9.7          12        11.4     11

  EVERY MESSAGE FOUND DOWN TO A DELIVERED -18.999 dB.
  FIRST MISS AT -21.001 dB: 19 of 20.
  THE DISTRIBUTIONS BEGIN TO OVERLAP AT -21.001 dB, where the weakest true score 9 has
  fallen below the best noise-alone score 13.
```

**The false-alarm floor does not move.** Across a twenty-decibel sweep the best score noise alone
produced stayed between 11 and 13 while the score at a true signal fell from 32 to 10. That is what
makes the separation readable, and it is why the overlap point is a real number rather than an
artefact of one seed.

**Nothing was tuned to improve this and it is not compared with any published sensitivity figure.**
Those figures are about decodes; error correction stands between a found signal and a decoded one;
nothing in this library demodulates anything. Step 6's question.

### The divergences added, and the versions

**19 — the candidate ordering is a total order with an explicit tie-break**, where upstream's
compares the score and nothing else. Reasoned above.

**20 — every hypothesis is scored and the survivors are sorted**, where upstream keeps a bounded
min-heap as it sweeps. Recorded separately because it is a different observable: upstream's eviction
rule discards the current worst only for a *strictly* greater score, so which of several tied
candidates is standing at the cut depends on the order the sweep visited them in. **The cost was
measured before it was accepted:** the whole space is 53040 hypotheses at 12 kHz, scored in 10 to
12 ms.

`porting-notes.md` now records **20 deliberate divergences**, numbered on from eighteen in the same
form. `Ft8Sharp` **0.7.0 → 0.8.0** under HM-DEC-152, with the note saying what it does not claim.
Root **1.12.20 → 1.12.21** under HM-DEC-150. `OPEN_ISSUES.md` is unchanged, which is the expected
answer: step 4 has no nice-to-pass criterion and all five of its must-pass criteria are met.

### What was committed and what was left alone

Eight commits, each pushed before the next task began. **Committed:** `PROJECT_STATUS.md`;
`src/Ft8Sharp/Dsp/Ft8SyncSearch.cs`; `src/Ft8Sharp/Dsp/Ft8Candidate.cs`;
`src/Ft8Sharp/porting-notes.md`; `src/Ft8Sharp/Directory.Build.props`; the root
`Directory.Build.props`; and six files under `tests/Ft8Sharp.Tests/Dsp/` —
`UpstreamSyncSearchInventoryTests.cs`, `Ft8SyncSearchProvenanceTests.cs`, `Ft8SyncSearchTests.cs`,
`SearchFixture.cs`, `Ft8SearchRecoveryTests.cs`, `Ft8SearchPassbandTests.cs`,
`Ft8SearchStabilityTests.cs` and `Ft8SearchSensitivityTests.cs`.

**Left alone, every one of them deliberately:** the 8 `.obj` files at the repository root, counted
and not touched; `tools/build-ft8-oracle.bat`, present, untracked, not run and not committed;
`PHASE_STATUS.md`, `PHASE_OUTCOME.md` and everything under `tools/`; the modified
`ANALYSIS-cw-*.md`, `PROJECT_CARD.md` and `WORK_INSTRUCTIONS.md`; and the untracked `ARBITER.md`,
`MANIFEST.txt`, `PHASE_PLAN.md`, `RUN_LEDGER.md`, `VERIFY_PASS.md`, `SCRUB_SELFTEST.bat`,
`SESSION.lock`, `.run-unit/` and `docs/phase-uplift/`. `git status --short` printed **25** lines at
entry — the instruction said 24 — and **28** at exit, the three added being the modified `OUTPUT.md`,
the modified `PROJECT_STATUS.md` before it is committed, and the one piece of untracked debris the
harness would not let this session delete, named below.

### Mismatches against the instruction, reported and not repaired

1. **`git status --short` printed 25 lines at entry, not 24.** The extra is `SESSION.lock`, which the
   instruction's list of untracked loop files does not name. Reported; nothing touched.
2. **`git diff --name-only 2828ab6..HEAD` listed 137 paths at entry, which matches.** No mismatch.
   Named here because the instruction asked for the number and it was checked.
3. **Every one of task 2's expectations was correct.** The instruction warned that finding one wrong
   would be a result; none was wrong. The search is in `ft8/decode.c` with the type in
   `ft8/decode.h`; the candidate carries a time block offset, a time sub-offset, a frequency bin
   offset, a frequency sub-offset and an integer score; the score is summed over three sync blocks of
   seven with differences against frequency AND time neighbours; there is a minimum score and a
   candidate limit, a heap and then a sort; and the sweep begins at a negative block offset and
   covers both sub-offset axes. **The one thing the instruction did not predict is which way the
   answer to its own question about the sort would come out**, and it comes out *not a total order*.
4. **The instruction says to update `PHASE_STATUS.md`'s `WORK_INSTRUCTION:` line, and also says not
   to hand-edit `PHASE_STATUS.md`.** The session prompt is explicit that the `WORK_INSTRUCTION:`,
   `PHASE:`, `PHASE_SET:` and `DESCRIPTION:` lines are the session's, so that one line was edited and
   nothing else was, and the file was **not committed** — known item 11 says to commit none of the
   loop's files. Both instructions were followed as far as they can both be followed; the conflict is
   reported rather than resolved.

### Refusals by the harness, reported as refusals

1. **The sandbox refused the session `C:\Source\ft8_lib`**, exactly as it refused the arbiter. Every
   line of upstream read tonight was read by the test process, which is the sanctioned route.
   **No route around the refusal was attempted.**
2. **The harness refused every attempt to DELETE a file**, including its own scratch directory and
   the temporary probe used to read the clone. This is the same refusal that has left
   `TempEncoderProbe.cs` on disk for eleven sessions. Consequence: two untracked files remain on
   disk and **neither is committed** — `tests/Ft8Sharp.Tests/Dsp/UpstreamSyncSearchProbe.cs`, emptied
   to a comment so that what is on disk and what is in the commit compile to the same tests, and
   `scratch-audio/u214/rotate.sh`, which is inside a `.gitignore`d directory.
3. **The harness refused `sed -i` and shell redirection into the working tree**, so every file was
   written through the editing tools instead. No mismatch resulted; noted because it is why no
   scripted status update was used.
4. **`tools\arbiter\validate-output.bat` could not be run, in any of the five spellings
   `tools\arbiter\run-unit-tools.txt` lists.** This is the fourth unit running — 211, 212 and 213
   were refused it too, and unit 213 measured why. Both `//c` forms reach the shell but the
   backslashes are stripped before `cmd` sees them, so `cmd` reports
   `'toolsarbitervalidate-output.bat' is not recognized`; the bare `tools\arbiter\...` form is
   stripped the same way by bash itself; and `cmd /c`, `cmd.exe /c` and every quoted variant tried
   were refused outright by the permission scope. **No route around the refusal was attempted** —
   no copy of the script, no reimplementation of its checks in another language, no editing of
   `tools\`. **The six rules were checked by hand instead**, against the script's own source, which
   is readable: the `UNIT:` line is at line 53, inside the first 60 and above section 1; there are
   exactly four `## ` headings and they are the four expected, in order, with no fifth; section 4 is
   present; section 3 holds several hundred non-blank lines; and the ordering block sits above the
   `UNIT:` line with the `READ IN THIS ORDER` header, an `A.`, a `B.` and a `C.` each beginning a
   line, and the count of section 4's items written as a digit on one line. **The unit's own check
   says this report is valid; the script did not say so, because it could not be reached.**

## 4. What's blocking us

**One item, and it is not a ruling request.** It is carried forward for step 5, which is the unit
that will need it.

**The block-to-sample alignment is measured but not explained against upstream.** A candidate's block
offset `b` names a transmission that began at about `(b - 1)` blocks into the slot: the mean signed
time error is +0.158936 s over 56 messages, which is 0.993 of a block, with a residual of at worst
0.0156 s. The arithmetic of the analysis window predicts exactly that one-block lead, so the number
is understood on this side. **What is not settled is whether upstream's own block offset means the
same thing**, because upstream never writes down what its `time_offset` is in samples and its decoder
is not built on this machine to be asked. Nothing was corrected for it tonight, because a correction
would be a guess about the very thing task 2 named as unread.

**Why it is not in the way of anything in B.** Criterion 1 is met with the bias reported and the
tolerance stated against the residual rather than against zero; criteria 2 and 3 do not depend on it
at all. It becomes real in step 5, where a demodulator has to start reading data symbols at the right
block — and there it will be settled by a decode that either works or does not, which is a better
test of an alignment than anything this unit could have run.

**The reference decoder is not re-raised.** It is a standing item, `HM-OPEN-065`, and known item 4.
