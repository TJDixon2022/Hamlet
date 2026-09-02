READ IN THIS ORDER

A. PHASE — Hamlet hears FT8 off the radio and displays the decoded text on screen. Seven steps.
Steps 1 and 2 are closed. Step 3 is closed on its four must-pass criteria, its nice-to-pass one
recorded as HM-OPEN-065. Step 4 was closed by unit 214 on all five must-pass criteria, at 56 of 56
messages found at rank 1 and 20 of 20 across the passband. STEP 5 IS THIS UNIT'S AND THIS IS ITS
FIRST UNIT: it entered at 0 of its three subject criteria and LEAVES AT 2 OF 3. Steps 6 and 7 have
not started and cannot, because each step of this plan depends on the one before it — which is why
step 5 was the only step this phase could move.

B. STEP 5 — a found signal becomes a message. Five exit criteria, one at a time. (1) A corrupted
codeword within the code's correcting power is recovered and one beyond it fails honestly — MET.
6 is the largest k at which every one of 400 trials recovered; recovery reaches zero at k = 17; and
across 37 952 trials of the whole night in which a wrong message could have come back, 0 DID.
(2) A candidate failing CRC is never returned — MET AT THE CODEWORD ENTRY POINT, and that reading is
stated rather than glossed: 5 096 valid codewords with a deliberately wrong checksum returned nothing,
0 of 5 000 uniform and 0 of 5 000 Gaussian noise arrays returned a message, and 56 of 56 clean
codewords decoded to their own message. (3) ft8_lib's reference WAVs decode against its expected
decode lists — NOT AIMED AT TONIGHT, by the arbiter's decision to split step 5; it waits on soft
symbol extraction, which does not exist in this library. (4) Ft8Sharp tests green — 429 total, 428
passed, 0 failed, 1 skipped at exit, the one skip being the table write gate. (5) Attribution clean
and the channels green — 158 paths from 2828ab6 with nothing under src/Hamlet. or tests/Hamlet.,
channels 55 and 13 re-run after both version bumps.

C. THIS REPORT — the findings weighed against A and B. THE LARGEST NUMBER OF BIT ERRORS FROM WHICH
EVERY TRIAL RECOVERED IS 6, over 400 trials at each of 45 error counts; AND ACROSS THE WHOLE NIGHT
0 WRONG MESSAGES CAME BACK OUT OF 37 952 TRIALS. The CRC gate's four counts are 56 of 56, 0 of 5 096,
0 of 5 000 and 0 of 5 000. The inverted sign convention RETURNED NOTHING, 56 of 56, so the decoder is
not convention-blind and that finding does not go in section 4. Section 4 raises 2 items; neither is
a ruling request and NEITHER IS IN THE WAY OF A CRITERION IN B. Task 7 was NOT dropped, though its
first branch licensed it: tasks 4, 5 and 6 all produced their measurements, so it was step 6
provisioning rather than diagnosis, and it cost 2.3 seconds.

UNIT:       215 — complete at task 8 of 8 — 2026-09-02 09:16
PHASE GOAL: Hamlet hears FT8 off the radio and displays the decoded text on screen.
UNIT GOAL:  Build the LDPC belief-propagation decoder and the gate behind it, so a damaged codeword
            is recovered where the code can recover it and nothing failing parity or CRC is returned.
ADVANCED:   yes — step 5 criteria 1 and 2, both must-pass, both at 0 this morning, both met and
            measured. Criterion 3 was not aimed at and is not counted against this unit.
NUMBER:     step 5 at 0 of its 3 subject criteria -> 2 of 3
DRIFT:      0 consecutive units without advance  (was 0)

## 1. What Claude did

**Exit state: complete, at task 8 of 8. Nothing was dropped.** Machine `C:\Source\HamLet`, project
claimed and confirmed as Hamlet by all four gate checks — `SHACK_FACTS.md` and
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` both tracked, `Hamlet.sln` the only solution,
`CoreHMI.sln` and `MURC.sln` both absent. Branch `main`. Every task was committed and pushed before
the next began; eight commits, `14f4305` to `3ba2b50`, all pushed and accepted.

### What was traced, built and measured

**Task 1 — the ground, re-measured rather than inherited.** Ft8Sharp at entry 394 total, 393 passed,
0 failed, 1 skipped in 18 s, which is exactly what unit 214 reported. The library built at 0 warnings
and 0 errors, `net8.0`, nullable enabled, warnings as errors, no `PackageReference` and no
`ProjectReference`. 147 paths from `2828ab6` with nothing under any Hamlet project. Channels 55 and
13. `HEAD` `14f4305`, 8 `.obj` at the root, versions 1.12.21 and 0.8.0, 20 divergences. **The code's
shape re-derived from `Ft8Tables` by a new test rather than read out of the generated file** — and
the number that mattered was the sum of `LdpcNumRows`, 522, agreeing exactly with 174 × 3 counted
from the other table. That is the Tanner graph's edge count and the decoder's message-passing loop
bound.

**Task 2 — upstream's decoder, read through the test process** because the sandbox refuses the
session direct access to `C:\Source\ft8_lib`, as it refused units 209 to 214. Ten tests green over
the pin. The findings are in section 3.

**Task 3 — the decoder.** `src/Ft8Sharp/Ldpc/` held one file this morning and it was the encoder. It
now holds `LdpcDecoder` and `LdpcDecodeResult`: upstream's `bp_decode` ported term for term, with 14
refusals watched refusing.

**Task 4 — the correcting power, swept.** 45 error counts, 400 trials each, table printed before any
bound was asserted.

**Task 5 — the gate.** `Ft8CodewordDecoder`, composed from the decoder, `Ft8Payload.TryRead` and
`Ft8MessageDecoder`, re-implementing none of them.

**Task 6 — determinism, the sign convention and every refusal in one table.**

**Task 7 — the soft sweep**, not dropped.

**Task 8 — the record, the versions and this report.**

### Decisions I made for myself, reproduced in full

1. **I doubled task 4's trial count from 200 to 400 after seeing the first table**, because the number
   the unit leads with is a *count of wrong messages* and a count means nothing without the total it
   is out of. **This changed the headline answer**: at 200 trials the largest k at which every trial
   recovered read 7, and the extra 200 found a single failure at k = 7 that the first run had missed.
   The reported answer is therefore 6. I am naming this because the same arithmetic applies to every
   rate in the report, and because a session that had stopped at the first table would have published
   a slightly generous number in good faith.
2. **I used the test project's own composition of the two proven pieces for task 4's verdict rather
   than waiting for task 5's library gate**, so the table did not depend on code the instruction
   ordered built afterwards. Task 5 then holds the library gate against that composition over the
   corpus and they agree 56 of 56, so the two did not drift.
3. **I fixed a defect in step 2's `Ft8DecodeResult`.** Its `Text` property returned `null` on a
   default-constructed result, against a summary line that has promised *the empty string on a
   refusal* since step 2. My own gate found it by dereferencing it — the gate carries a default
   result whenever parity or the checksum refuses. I judged this in scope because my new code caused
   the exposure and because a display reading it would have thrown at exactly the moment nothing
   decoded, which is the moment that has to be uneventful. It is recorded in `porting-notes.md` as a
   corrected defect and explicitly **not** as a divergence.
4. **I chose the confident ratio magnitude as the square root of 24** rather than a round number,
   because upstream rescales its ratios to a variance of 24 before its decoder sees them and this
   decoder is not scale-free. A magnitude picked for tidiness would have measured a decoder nobody
   will run. The reasoning is in the test file.

### One defect of my own, found and corrected rather than left

My first ordering assertion in the upstream inventory matched the `toc` **array declaration** at the
top of `bp_decode` instead of the first message actually sent, and went red against a pin that is
entirely correct. It now matches an assignment to the array, and the reason is written into the test
so the next reader does not repeat it.

## 2. What the owner should expect

**`Ft8Sharp` can now repair a damaged codeword and refuse one it cannot.** That is a real capability
and it is why the library moved 0.8.0 to 0.9.0.

**What will look like progress and is not.** Nothing you can run has changed. There is no screen, no
audio path and no decode of anything off a radio. **No message has come off the air, tonight or in
any unit before tonight.** Every ratio this decoder has ever seen was constructed in the test project
from a codeword the library encoded itself.

**What will look wrong and is not:**

- **Step 5 is not closed and this report says so.** It is at 2 of its 3 subject criteria. The third —
  upstream's reference WAVs decoding — was deliberately not aimed at, by the arbiter's decision to
  split step 5 into two units. It is the next unit's whole night.
- **`Ft8Sharp` gained 35 tests and the total went from 394 to 429**, while the run time barely moved,
  from 18 s to 19 s. The two sweeps are the slow part and they cost about 12 seconds between them.
- **There is still exactly one skip and it is correct** — the table write gate, which only runs when
  asked with an environment variable.
- **Two files I could not delete are still on disk**, untracked and emptied to comments:
  `tests/Ft8Sharp.Tests/Ldpc/UpstreamLdpcProbe.cs`, my reading window onto the clone, and
  `unit215-section.md` at the root, a scratch buffer I needed because the shell refused to append to
  `porting-notes.md`. The harness refused both deletions, as it has refused twelve sessions asked to
  be rid of `TempEncoderProbe.cs`. Neither is committed and neither changes what compiles.
- **The library's version and Hamlet's version disagree, deliberately** — 0.9.0 and 1.12.22, under
  HM-DEC-152.

## 3. What you should see

**No visible change in the application. Nothing an operator could see is different.** This unit built
the half of step 5 that can be proved with no audio in it at all, so what it produces is numbers.

Here are the numbers, and the table comes before the prose.

```
THE CORRECTING POWER — 45 error counts, 400 trials at each, 56 corpus messages,
seed 21501 + k, maxIterations 25.  PRINTED BEFORE ANY BOUND WAS ASSERTED.

   k | trials | recovered  rate% | wrongMsg | crcRejected | noDecode | iters mean  worst
-----+--------+------------------+----------+-------------+----------+------------------
   0 |    400 |       400  100.0 |        0 |           0 |        0 |       1.00      1
   1 |    400 |       400  100.0 |        0 |           0 |        0 |       2.00      2
   2 |    400 |       400  100.0 |        0 |           0 |        0 |       2.09      3
   3 |    400 |       400  100.0 |        0 |           0 |        0 |       2.29      4
   4 |    400 |       400  100.0 |        0 |           0 |        0 |       2.65      5
   5 |    400 |       400  100.0 |        0 |           0 |        0 |       3.04      6
   6 |    400 |       400  100.0 |        0 |           0 |        0 |       3.65     11
   7 |    400 |       399   99.8 |        0 |           0 |        1 |       4.29     25
   8 |    400 |       396   99.0 |        0 |           0 |        4 |       5.33     25
   9 |    400 |       382   95.5 |        0 |           0 |       18 |       7.18     25
  10 |    400 |       370   92.5 |        0 |           0 |       30 |       9.03     25
  11 |    400 |       346   86.5 |        0 |           0 |       54 |      11.57     25
  12 |    400 |       265   66.2 |        0 |           0 |      135 |      16.10     25
  13 |    400 |       190   47.5 |        0 |           0 |      210 |      19.41     25
  14 |    400 |       109   27.2 |        0 |           0 |      291 |      22.14     25
  15 |    400 |        37    9.2 |        0 |           0 |      363 |      24.23     25
  16 |    400 |        15    3.8 |        0 |           0 |      385 |      24.67     25
  17 |    400 |         0    0.0 |        0 |           0 |      400 |      25.00     25
  18 |    400 |         0    0.0 |        0 |           0 |      400 |      25.00     25
  19 |    400 |         0    0.0 |        0 |           0 |      400 |      25.00     25
  20 |    400 |         0    0.0 |        0 |           0 |      400 |      25.00     25
  21 |    400 |         0    0.0 |        0 |           1 |      399 |      24.98     25
  22 |    400 |         0    0.0 |        0 |           0 |      400 |      25.00     25
  23 |    400 |         0    0.0 |        0 |           0 |      400 |      25.00     25
  24 |    400 |         0    0.0 |        0 |           0 |      400 |      25.00     25
  25 |    400 |         0    0.0 |        0 |           0 |      400 |      25.00     25
  26 |    400 |         0    0.0 |        0 |           1 |      399 |      24.99     25
  27 |    400 |         0    0.0 |        0 |           1 |      399 |      24.97     25
  28 |    400 |         0    0.0 |        0 |           0 |      400 |      25.00     25
  29 |    400 |         0    0.0 |        0 |           0 |      400 |      25.00     25
  30 |    400 |         0    0.0 |        0 |           0 |      400 |      25.00     25
  31 |    400 |         0    0.0 |        0 |           0 |      400 |      25.00     25
  32 |    400 |         0    0.0 |        0 |           0 |      400 |      25.00     25
  33 |    400 |         0    0.0 |        0 |           0 |      400 |      25.00     25
  34 |    400 |         0    0.0 |        0 |           0 |      400 |      25.00     25
  35 |    400 |         0    0.0 |        0 |           0 |      400 |      25.00     25
  36 |    400 |         0    0.0 |        0 |           0 |      400 |      25.00     25
  37 |    400 |         0    0.0 |        0 |           0 |      400 |      25.00     25
  38 |    400 |         0    0.0 |        0 |           0 |      400 |      25.00     25
  39 |    400 |         0    0.0 |        0 |           0 |      400 |      25.00     25
  40 |    400 |         0    0.0 |        0 |           0 |      400 |      25.00     25
  41 |    400 |         0    0.0 |        0 |           0 |      400 |      25.00     25
  42 |    400 |         0    0.0 |        0 |           0 |      400 |      25.00     25
  43 |    400 |         0    0.0 |        0 |           0 |      400 |      25.00     25
  44 |    400 |         0    0.0 |        0 |           0 |      400 |      25.00     25
```

**The three numbers, in words.**

- **The largest k at which EVERY trial recovered is 6**, out of 400 trials at that k.
- **Recovery reaches zero at k = 17**, and stays there through k = 44.
- **0 wrong messages were returned out of 18 000 trials in this table, and 0 out of 37 952 trials
  across the whole night.**

**And the arithmetic beside that zero, because the instruction was right that the honest answer might
not have been zero.** Only 3 of the 18 000 trials ever converged to a *different* valid codeword at
all — at k = 21, 26 and 27 — and CRC-14 caught all three. Its undetected-error floor is one in
2^14 = 16 384, so the expected number of escapes from 3 such trials is 0.0002. **Zero is what the
arithmetic predicts, and it is reported with the arithmetic rather than asserted.** Nothing was tuned
toward it.

**The honest-failure half, which is the criterion's second clause.** At k = 44, where recovery is
zero: 400 trials returned nothing, 0 returned a wrong message, and the sum is the trial count.

**Cost.** 18 000 decodes in 9 070 ms, **0.504 ms each** — well under the few milliseconds that the
instruction said would have been a finding about the port.

### Task 5 — the CRC gate's four counts, and the reading of *candidate* I stood on

1. **A clean codeword decodes to its own message: 56 of 56** on the 77 bits exactly, and the gate's
   whole verdict equals what step 2 makes of the same message **56 of 56**. 51 became text; the 5
   that did not are the hashed-callsign entries, sound past both gates and unreadable without a cache
   that has heard the call. **They are counted by agreeing about the refusal rather than excused from
   the count.**
2. **The tempting case: 5 096 codewords whose checksum was altered *before* the parity was computed**,
   so every one is a genuine member of the code that belief propagation finds in one iteration with
   zero unsatisfied checks. 784 checksum-bit alterations and 4 312 message-bit ones.
   **Parity was fully satisfied on all 5 096. All 5 096 were refused at the checksum gate. 0 returned
   anything at all.**
3. **Random ratios, uniform, seed 21552: 0 of 5 000 returned a message.** All 5 000 stopped at parity;
   the closest any came was 2 of 83 checks still unsatisfied.
4. **Ratios from noise alone with no codeword under them, Gaussian, seed 21554: 0 of 5 000.** All
   5 000 stopped at parity, and **the closest came within one check of 83** and still returned
   nothing.

**Which reading of the criterion I stood on, in the instruction's own words.** The criterion says
*a candidate*, and there are no candidates in this path tonight — extraction does not exist.
**The gate is proven at the codeword entry point and it is re-taken end to end when the next unit
connects a candidate to it.** That sentence is in the test file as well as here. I am not claiming
the criterion in the candidate sense.

### Task 6 — determinism, and what the inverted convention did

**Determinism, asserted on values and never on a count.** 56 messages, each damaged by 10 bit flips,
decoded in four orderings: forward, again, **reversed**, and a **seeded shuffle**. Every one of 174
bit positions plus the iteration count plus the unsatisfied-check count was compared per message per
ordering — **29 568 value comparisons, all equal.** The damage was drawn once and held so the
reversed run is the same experiment rather than a different one. It bites because the answers are not
alike: 55 of 56 recover, the iteration counts spread over eleven distinct values from 4 to 25, and the
unsatisfied counts are 0 and 4. `LdpcDecoder` is static and allocates both message arrays per call, so
there is no instance to reuse and the ordering comparisons stand in its place — said rather than
skipped.

**THE INVERTED SIGN CONVENTION RETURNED NOTHING, 56 OF 56.** 0 correct messages, 0 wrong messages.
**So the decoder is not convention-blind, and this does not go in section 4** — the instruction said
it belonged there only if it had decoded.

**And the reason is predicted from the code's own structure before it is measured, which is what makes
it a result rather than luck.** Negating every ratio complements the hard decision, and the complement
of a codeword is that codeword exclusive-ored with the all-ones word. The all-ones word satisfies a
parity check exactly when that check has **even** degree. **This code is not regular**: 59 of its 83
checks cover six variables and 24 cover seven. So an inverted codeword is predicted to fail precisely
the **24 odd-degree checks** — and the measurement at the first hard decision is **24**, for every one
of the 56 messages. Belief propagation then works it down from 24 to 12 over 25 iterations and never
reaches zero. **Had this code been regular, an inverted codeword would have been a perfect codeword
and the decoder would have been blind to the convention.**

### Task 2 — the shapes read, the anchoring split, and the second decoder

**The split is 6 strong, 9 weak, 1 weakest.** Strong is a macro, a typedef or a header declaration:
the code's three dimensions, both check tables' row widths, both decoders' declarations, that
`ftx_decode_candidate` takes the iteration count as a parameter, the decode status structure's three
fields, and the fixed variable-node degree. Weak is an expression inside a function body: where the
hard decision is taken, what the convergence test is, both break conditions, the running minimum,
that the check loop is bounded by `Num_rows` and not the row width, the two exclusion rules in the
message updates, the gate order, and the clamped rational hyperbolics.

**The weakest, named as the instruction asked: the maximum iteration count.** It is a file-scope
constant in `demo/decode_ft8.c` and it appears in **none** of `ft8/ldpc.c`, `ft8/ldpc.h`,
`ft8/decode.c`, `ft8/decode.h` or `ft8/constants.h` — measured, not assumed. It is the *application's*
choice, so the port exposes it as a parameter with upstream's value as its default rather than burying
it in a loop. **Nothing was left unread**; every question this unit asked of the pin was answered.

**There IS a second decoder and it was read and deliberately not ported.** `ft8/ldpc.h` declares both
`bp_decode` and `ldpc_decode` with identical signatures. **`ftx_decode_candidate` calls `bp_decode`,
with the call to `ldpc_decode` commented out on the line below it.** They are the same sum-product
algorithm: `ldpc_decode` carries two dense 83-by-174 float matrices — upstream's own comment says
~60 kB of each — where `bp_decode` carries only the 522 edges the graph has. Porting the one upstream
does not run would be porting something nothing has ever exercised.

**The sign convention, and which leg the claim stands on.** **A positive ratio means the bit is more
likely 1.** It stands on **three independent readings of upstream's source**, not on a round trip:
extraction in `ft8/decode.c` states `log(p(1) / p(0))` in its own comment and computes
`max_one - max_zero`; both decoders decide `(l > 0) ? 1 : 0`; and the check-node update
`-2·atanh(∏ tanh(-T/2))` is the exact sum-product rule *only* in that convention — substituting
`L = -λ` into `+2 atanh(∏ tanh(L/2))` gives upstream's expression, negations and all. **The two extra
minus signs are the convention, not decoration.** A self-consistent round trip through this project's
own helpers is explicitly *not* counted as evidence, and the report says so.

### Mismatches against the instruction, reported and not repaired

**Three, and the first is the most useful thing in this report after the two headline numbers.**

1. **`ft8/ldpc.c`'s own opening comment states the OPPOSITE sign convention to the one its code
   uses.** It calls the input the *log-likelihood of zero* and writes `codeword[i] = log(P(x=0) /
   P(x=1))`. That contradicts all three readings above. **The code was followed and the comment was
   not**, and the inventory test asserts the wrong comment is *still present*, so a re-pin that
   corrects it goes red beside the port rather than silently removing the trap. The instruction did
   not anticipate this and could not have; it is upstream's, not the instruction's, but it is exactly
   the trap the instruction's most important paragraph was written about.
2. **Two more stale comments in the same pair of files.** `ft8/ldpc.h` says `ok == 87 means success`,
   which is wrong twice over — success is zero, and 87 is not the number of checks either;
   `ft8/ldpc.c` says the *last* 87 bits are the systematic plain text, where `ftx_decode_candidate`
   packs the *first* 91.
3. **`git status --short` printed 27 lines at entry where the instruction says 26**, and 29 at exit.
   Reported, not repaired, and nothing of the loop's was committed. **Known item 9 confirmed:** the
   report file really is tracked as `OUTPUT.md`, upper case, and writing `output.md` landed on it. I
   did not rename it.

**And two of the arbiter's expectations that were RIGHT and are worth confirming**, since the
instruction invited the check: the decoder does live in `ft8/ldpc.c` declared in `ft8/ldpc.h`; there
*are* more than one of them; the signature *is* 174 floats, a maximum iteration count, the recovered
bits and a residual error measure; and the CRC *does* sit after the LDPC decode with a codeword that
corrects cleanly but fails CRC being refused.

### Every refusal watched refusing, and by how much it missed

```
what was handed in                             | what happened
-----------------------------------------------+---------------------------------------
ratios: 173 long                               | refused, and the message names 174 and 173
ratios: 175 long                               | refused, and the message names 174 and 175
ratios: empty                                  | refused, and the message names 174 and 0
output buffer: 173 long                        | refused, and the message names 174 and 173
maxIterations: -1                              | refused rather than treated as zero
gate: ratios 173 long                          | refused on the same terms
all ratios exactly zero                        | ParityNeverSatisfied, missed by 83 of 83
every bit confidently 0                        | ParityNeverSatisfied, missed by 83 of 83
every ratio negated (convention inverted)      | ParityNeverSatisfied, missed by 12 of 83
a clean codeword with 44 bits flipped          | ParityNeverSatisfied, missed by 11 of 83
a clean codeword, unaltered                    | Decoded, missed by 0 of 83
```

**The all-zero case is the one that matters** and its test proves the refusal is a *decision* rather
than an arithmetic accident: it computes the all-zero word's true syndrome independently through
`LdpcCheck` and prints it as **0 checks failing**. The all-zero word satisfies every check of any
linear code, so a decoder that only counted checks would report a perfect decode on a signal that was
never there. **The last row is there so the table is not a list of a decoder that always says no.**

### Task 7 — the soft sweep, not dropped

**Which branch licensed dropping it: the first — tasks 4, 5 and 6 all ran and produced their
measurements.** I ran it anyway, because it is what step 6 starts from and it cost 2.3 seconds.

```
 sigma/A | trials | decoded   rate% | wrongMsg | noDecode | iters mean | variance
---------+--------+-----------------+----------+----------+------------+---------
    0.00 |    400 |     400   100.0 |        0 |        0 |       1.00 |     23.4
    0.25 |    400 |     400   100.0 |        0 |        0 |       1.01 |     24.9
    0.50 |    400 |     400   100.0 |        0 |        0 |       2.23 |     29.4
    0.75 |    400 |     356    89.0 |        0 |       44 |       8.78 |     36.8
    1.00 |    400 |      18     4.5 |        0 |      382 |      24.36 |     47.5
    1.25 |    400 |       0     0.0 |        0 |      400 |      25.00 |     61.2
    1.50 |    400 |       0     0.0 |        0 |      400 |      25.00 |     77.0
    1.75 |    400 |       0     0.0 |        0 |      400 |      25.00 |     97.1
    2.00 |    400 |       0     0.0 |        0 |      400 |      25.00 |    119.2
    2.50 |    400 |       0     0.0 |        0 |      400 |      25.00 |    171.8
    3.00 |    400 |       0     0.0 |        0 |      400 |      25.00 |    238.2
    4.00 |    400 |       0     0.0 |        0 |      400 |      25.00 |    405.6
```

**The cliff is sharp, which is what an LDPC code looks like**: 100 per cent to σ/A = 0.50, 89.0 at
0.75, 4.5 at 1.00, zero from 1.25. Iterations track it exactly. **0 wrong messages out of 4 800.**

**The variance column is a caveat and not a result, and it is the most useful thing in this table.**
Upstream rescales all 174 ratios to a variance of 24 before its decoder ever sees them, and **that
normalisation is extraction's and was deliberately not ported**, so these arrays leave upstream's
scale as the noise grows. Where the column leaves 24, this sweep and upstream's path have parted
company, and the tail should be read accordingly.

**Said plainly, in the test file as well as here: this is NOT a decode rate off a channel, the axis is
NOT a signal-to-noise ratio, and it is NOT comparable with the published sensitivity figure.** There
is no audio, no search and no demodulation anywhere in it. Nothing was tuned.

### The record, the versions, and what was committed

- **`porting-notes.md` gains its unit-215 section**, in the form units 209 to 214 used.
- **One divergence added and numbered 21**: the output bits are cleared before the iteration begins,
  where upstream leaves them untouched — which in C is stack garbage, and there is no faithful port of
  undefined content. Observable only where a caller asks for zero iterations, which upstream never
  does. **The divergence count goes 20 to 21.**
- **Two things named as NOT divergences** so they are not mistaken for any: the iteration count on the
  result type is an *addition* that changes no decision, and the null `Text` was a *corrected defect*.
- **`OPEN_ISSUES.md` is unchanged, and that is the expected answer** — step 5 carries no nice-to-pass
  criterion, so nothing is owed there by the plan's own ruling. I did not reorganise or tidy it.
- **Versions**: `Ft8Sharp` 0.8.0 → **0.9.0** under HM-DEC-152, with a note saying what it does not
  claim; root 1.12.21 → **1.12.22** under HM-DEC-150.
- **Re-run after both bumps**: Ft8Sharp **429 total, 428 passed, 0 failed, 1 skipped in 19 s** — 35
  tests added and still the one correct skip, `Ft8TableGenerationTests.RewriteTheCheckedInTablesFile`,
  the table write gate, which skips with a message naming the environment variable that would run it.
  Channels **55** and **13**, `VersionTests` among them. Library 0 warnings, 0 errors.
- **Attribution**: **158 paths** from `2828ab6`, and the filter for `src/Hamlet.` and `tests/Hamlet.`
  **returns nothing**.
- **No new shared artifact**, so **no new channel** — which is what the instruction expected.
- **8 `.obj` at the root**, counted and untouched. `tools\build-ft8-oracle.bat` is present, untracked,
  and was neither run nor edited.
- **What I committed**: only files under `src/Ft8Sharp/`, `tests/Ft8Sharp.Tests/`, the two
  `Directory.Build.props`, `PROJECT_STATUS.md` and `PHASE_STATUS.md`'s `WORK_INSTRUCTION:` line.
  **What I left alone**: every `.obj`, everything under `tools/`, `PHASE_OUTCOME.md`,
  `PROJECT_CARD.md`, `WORK_INSTRUCTIONS.md`, the two `ANALYSIS-cw-*` files, and all eight of the
  loop's untracked files. Nothing from the clone was committed.

### The validator was refused, for the fifth unit running, and the refusal is reported as a refusal

**All five spellings `tools\arbiter\run-unit-tools.txt` lists were tried and none of them ran the
script.** The two `//c` forms strip the backslashes and cmd reports
`'toolsarbitervalidate-output.bat' is not recognized`; the bare form fails the same way through bash;
and the two `/c` forms open an interactive cmd that prints its banner and the prompt path instead of
running anything. This reproduces units 211 to 214 exactly. **I did not route around it** — no
sixth spelling was invented.

**So I read the script's own source and checked its six rules by hand**, which is what unit 214 did:

1. **A `UNIT:` line above section 1** — present at line 34, inside the first 60 lines the script
   reads, and above section 1 at line 43.
2. **The four top-level sections, in order, exact names** — `## 1. What Claude did` at 43,
   `## 2. What the owner should expect` at 115, `## 3. What you should see` at 142,
   `## 4. What's blocking us` at 441.
3. **No fifth top-level section** — those four are every `## ` line in the file.
4. **Section 4 present** — matched at the start of a line with a straight apostrophe, which is what
   the script's own `%WANT%` string uses.
5. **Section 3 non-empty** — 257 non-blank lines between the two headings.
6. **The ordering block above the `UNIT:` line** — `READ IN THIS ORDER` at line 1, `A.` at 3, `B.` at
   11, `C.` at 25, and `raises 2 items` at 29. All five are inside the first 60 lines, all are above
   line 34, and **the count in C is written as a digit**.

## 4. What's blocking us

**Section 4 raises 2 items. Neither is a ruling request and neither is in the way of a criterion in
B.** I am not re-raising the reference decoder — it is a standing item and `HM-OPEN-065`.

### Carried forward 1 — the decoder is not scale-free, and the thing that puts ratios on its scale is not ported

`fast_tanh` and its clamp at ±4.97 are not homogeneous, so multiplying every ratio by a constant
changes the answer. Upstream rescales the 174 ratios to a fixed variance in `ftx_normalize_logl`
between extraction and `bp_decode`. **That routine sits on the extraction side of tonight's line and
was deliberately not ported.**

**What the next unit needs to know:** extraction must deliver ratios on upstream's *scale*, not merely
with upstream's *signs*. Getting the signs right and the magnitudes wrong will look like a decoder
that half works, and it will be the harder failure to localise of the two. This is recorded in
`porting-notes.md`, in `LdpcDecoder`'s own remarks and in the inventory test, and it is not a decision
anybody has to make now.

### Carried forward 2 — the inventory test goes red on purpose if upstream corrects its own wrong comments

`UpstreamLdpcDecoderInventoryTests` asserts that `ft8/ldpc.c` **still carries** the comment stating the
wrong sign convention, and that `ft8/ldpc.h` **still carries** `ok == 87 means success`. That is
deliberate: if the pin is ever moved to a commit where those were fixed, the reading has to be
re-taken rather than assumed unaffected, and a test that quietly kept passing would be the worst
outcome. **It means a future re-pin will produce two red tests that are not defects.** They carry
messages saying so. No decision is needed now; this is a note to whoever moves the pin.

### And explicitly not in this section

**The inverted sign convention decoded nothing, 56 of 56**, so it does not belong here — the
instruction placed it in section 4 only if the decoder had turned out to be convention-blind. It is
not. That result is in section 3 with the reasoning that predicts it.
