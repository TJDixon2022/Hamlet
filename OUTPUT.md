READ IN THIS ORDER

A. The phase goal. Hamlet hears FT8 off the radio and displays the decoded
   text on screen. Steps 1 to 5 are done; tonight is the third unit on
   step 6 of 7 - sensitivity meets the published threshold - and step 7 is
   not started.
B. This step and its exit criteria. Step 6 has five must-pass criteria.
   Four are met and were re-measured tonight: the reproducible fourteen-rung
   curve stands as units 221 and 222 measured it, wrong messages are zero
   everywhere including all 5202 substituted slot decodes taken tonight,
   Ft8Sharp is green at 512/511/0/1, and attribution is clean at 192 paths
   with 0 under Hamlet. CRITERION 2, the decode rate at -21 dB against the
   published figure, has been NOT MET at 13 of 306 through two units and it
   is NOT MET tonight at the same 13 of 306, re-measured in a fourth
   process. It was the only criterion this unit aimed at.
C. What this report adds, and it bears on B. This unit is in OUTCOME TWO:
   the information is not in the ratios, now measured rather than inferred.
   Exact tanh and atanh at upstream's 25 iterations give 15 of 306 (4.9 %,
   3.0 to 7.9) and at 100 iterations 17 of 306; an independently written
   soft decoder over the identical ratios gives 17 of 306 (5.6 %, 3.5 to
   8.7); the as-is row is 13 of 306 (4.2 %, 2.5 to 7.1). All three land
   INSIDE the as-is interval, which is the reading fixed before the run.
   The normalisation audit found the port faithful - ten of twelve terms
   identical to the pin, both differences already recorded. No fidelity fix
   was licensed and none landed; nothing under src/Ft8Sharp/ changed except
   porting-notes.md and the version. The -21 dB rate at the end of the night
   is 13 of 306, 4.2 per cent, NOT MET against the unchanged band. Section 4
   raises 2 items, neither is a ruling request from me, and the first is
   criterion 2 itself so it bears directly on B.

UNIT:       223 - complete at task 7 of 7
PHASE GOAL: Hamlet hears FT8 off the radio and puts the decoded text on screen.
UNIT GOAL:  Price upstream's fast_tanh and fast_atanh against exact arithmetic at -21 dB, and find out by measurement whether the true codeword is reachable from the same extracted ratios by any soft decoder.
ADVANCED:   no - criterion 2 did not move and no other criterion changed state. What moved is that criterion 2's last unit-reachable route is now closed by measurement instead of by an untested inference.
NUMBER:     exact arithmetic: 15 of 306 | independent decoder: 17 of 306 | as-is: 13 of 306 | verdict: NOT MET
DRIFT:      2 consecutive units without advance

## 1. What Claude did

### The outcome, first, because the instruction fixed both before the run

**This unit is in OUTCOME TWO: the information is not in the ratios, now measured
rather than inferred.** Exact arithmetic, exact arithmetic with four times the
iterations, and an independently written soft decoder all land inside the as-is
row's own 95 per cent interval of 2.5 to 7.1 per cent.

**And the row that decided it is not a rate.** It is the score census. At the oracle
alignment, over the 292 trials the library's decoder failed and the 288 the
independent decoder failed, scoring each word by `sum of ratio times (2*bit - 1)` on
the normalised ratios:

**the true codeword scores higher than the word the decoder settled on in 0.0 per
cent of them.** Not a minority. **Zero of 292 and zero of 288.** Mean gap -86.7 and
-86.4, and the least negative gap anywhere is -7.2.

**The ratios themselves prefer the wrong answer.** No decoder that searches these
ratios for the most likely codeword can recover the message, however patient or
however exact. That is a fact about the ratios and not about any decoder, and it
means **criterion 2 is not reachable by any change to the correction stage.**

Seven tasks, nothing dropped. Task 5 was the named drop candidate and it was **run**,
because the night was not long.

### Task 1 - the ground, and the before-number

Every figure re-measured rather than inherited. **The before-number reproduces
exactly: 13 of 306, 4.2 per cent, at a delivered -21.001 dB, 0 wrong, worst delivery
error 0.0406 dB** - unit 221's figure to the numerator and the **fourth process** to
produce it.

**The four untracked probe sources are answered rather than repaired.** All four
**compile** - the build succeeds with them present - and all four are
**comment-only**, six to eight comment lines each and not one declaration, emptied
by units 214 to 217 when their reads were committed into the inventory tests. **The
count is 512 with them and 512 without.** There is no reproducibility gap and they
are left where they are.

**One figure disagrees with the instruction and it is a budgeting fact rather than a
defect.** One slot decode cost **75.5 ms** tonight against unit 222's 26.1, the
Ft8Sharp suite 4 m 57 s against its 1 m 42, and the RadioEngine channel set 13 m 36 s
against its 7 m 38. The machine is about three times slower. **Rates are
deterministic in the seeds and were unaffected**; only wall time was.

### Task 2 - the stage unit 222's own rule skipped

Unit 222's audit rule picked the stage its largest budget row named. **No budget row
could ever have named this one**: the normalisation sits between extraction and
correction, every row passed through it, and it cancels out of every delta a budget
can form. It is the only stage of the receive path no unit had read against upstream.

`ftx_normalize_logl` against `Ft8SoftSymbols.Normalise` and `Variance`, term by term,
through a checked-in test that reports skipped when the clone is absent. **Twelve
terms, ten SAME, two DIFFERING and both already recorded.** The port is faithful here
too. Details in section 3.

**One mistake of mine the instrument caught before it reached a verdict.** My first
slice of the region between the two calls started at the open bracket rather than
after the semicolon, so the call's own argument sat inside the region and the row read
DIFFERING at 1 of 2 statements touching the array. Fixed, with the reason written into
the file, and it reads 0 of 1.

### Task 3 - the row unit 222 named and could not take

A copy of `LdpcDecoder.Decode` in the test project with exactly two terms moved, and a
**transcription control** - the same apparatus with upstream's own arithmetic in it -
which reproduces the as-is row exactly. Without that control, rows F and G would be
unreadable.

**And the approximation is materially in play**, which is what made the row worth
taking even though it bought little: `fast_atanh` caps every check-to-variable message
at **4.567** against exact arithmetic's 37.43, and **5.58 per cent of 50,410,062
`fast_tanh` calls** land on the ±4.97 clamp over 306 real slots.

**One thing neither the instruction nor unit 222 expected**, found only because the
largest message came back above the ceiling `fast_atanh` allows: **`fast_tanh`
overshoots one just below its own clamp**, at 1.007218, so the product of tangents
leaves [-1, 1] and `fast_atanh`'s denominator falls from 120 to 86.8 toward its own
root. The pole is not reached. Reported, not fixed.

### Task 4 - the claim the whole phase's world-two finding rested on

Unit 222 concluded *the information is not in the ratios* by comparing about 31
soft-decoded errors against a correcting power of 17 measured over **hard bit flips**.
Those are not the same kind of thing, and a soft decoder is not bound by its
hard-decision limit. So a **second decoder** was written from the parity tables in this
tree and the sum-product algorithm as public literature - the log domain through
Gallager's phi rather than a product of tangents, the opposite sign convention
internally, prefix and suffix sums rather than re-multiplication, the Tanner graph
built from `LdpcNm` alone with the variable side counted rather than read from
`LdpcMn`, double precision, 100 iterations.

**Watched refusing before it was believed, and one control refused on its first run.**
Control 3 was written expecting zero and read 1 of 500 passing parity on pure noise. It
was **widened to 5000 rather than loosened**, and at 5000 the independent decoder and
the library's own read **identically** at 2 of 5000, with 0 becoming a message - which
is the gate row H actually counts at.

**One bound was widened after a result was seen and it is declared.** The assertion
that phi is its own inverse was written at 1e-9 and read 1.6e-3 at x = 35. The reason
is the **type** and not the algorithm - phi(35) is 1.3e-15 - so it is scoped to x <= 20
where phi is still 4.1e-9. **No measurement moved and the verdict band was not
touched.**

### Task 5 - run, not dropped, and it vindicates upstream's constant

Twelve targets cost 63.8 s. **24 sits on a broad plateau** - everything from 18 to 36
reads 13 or 14 of 306 - and the best row in the sweep is one decode better and inside
the 24 row's own interval. **The value in `src/Ft8Sharp/` did not move.**

### Task 6 - no fix was licensed, and tonight the refusal cost something

**Both conditions failed.** Condition 1: neither differing term is a place where this
port's arithmetic differs from the pin - one is divergence 23, which can only fire on
174 identical ratios, the other a return type. Condition 2 fails outright: no row
attributed a decode to any difference.

**This is the first night a substituted row plainly decoded better and the answer is
still no.** Four numbers were measured to be worth decodes and **not one of them
moved** - exact tanh and atanh at +2, exact arithmetic at 100 iterations at +4, an
independent decoder at +4, and a normalisation target of 18 at +1.

### Task 7 - the record

`porting-notes.md` gains its unit-223 section. `HM-OPEN-067` is **updated in place and
not duplicated** - still open, still severity *blocks*, still blocking step 6 criterion
2 by name - with route 2 now marked **COMPLETE** rather than merely audited, route 1
gaining the normalisation audit and the sweep, and `HM-OPEN-065` still first and still
the only route left, cited and not re-raised. Versions bumped **both patches** because
no library file changed. **The curve was not re-run to fill a section.**

## 2. What the owner should expect

**Step 6 did not close.** Four of its five must-pass criteria are met; criterion 2 is
not, and it is the one the step is named after.

**Nothing an operator could see moved, and the version bump says so by being a patch.**
The screen shows exactly what it showed under 0.10.6. A station at -21 dB is still
decoded about four times in a hundred rather than about half the time, and the 50 per
cent crossing is still near -19.5 dB - about **1.5 dB** short of the published figure.
**No fix landed, so there is no number of decibels weaker a station Tim can now hear.**

**What has changed is what Tim now knows, and it is the thing that makes his next
decision decidable.** Three instruments have now been pointed at criterion 2. Unit 221
measured the rate. Unit 222 measured the axis and substituted four stages above the
arithmetic. This unit went inside the arithmetic itself and then asked whether the
message was ever recoverable at all - and the answer is that **it was not**. The true
codeword is not the most likely word these ratios describe, in **every single failing
trial**. A better decoder cannot find what is not there.

**So the two routes left are both his**, and this unit has taken neither:

- **`HM-OPEN-065`, the reference decoder.** Building `decode_ft8.exe` is a compiler run
  and is owner-class. It is the only instrument that could say whether `ft8_lib` itself
  hears these same samples at -21 dB - which would turn *this port is 1.5 dB short* into
  either *short of upstream* or *upstream is short too and the figure is quoted
  differently*.
- **The deliberate-divergence ruling** unit 222 put in front of him. **This unit adds a
  fourth priced number to it** - exact `tanh` and `atanh`, worth two decodes in 306 -
  beside the byte-quantised waterfall, the 25-iteration bound and the lowest-order
  `fast_tanh`. Every one of them is small. **None of them was taken.**

**What he should not expect is a fourth measurement of this criterion from a unit.**
Everything a unit can reach has now been read against the pin or substituted and
measured, and the budget is flat everywhere.

## 3. What you should see

### The rate table - as-is first, then F, G and H

```
row                                    n     of    rate   lo 95   hi 95    delta  WRONG
A.  as-is, the library's own path     13    306     4.2     2.5     7.1      0.0      0
A'. as-is, TRANSCRIBED (the control)  13    306     4.2     2.5     7.1      0.0      0
F.  exact tanh/atanh, 25 iterations   15    306     4.9     3.0     7.9     +0.7      0
G.  exact tanh/atanh, 100 iterations  17    306     5.6     3.5     8.7     +1.3      0
H.  independent soft decoder          17    306     5.6     3.5     8.7     +1.3      0
```

**All three substituted rows lie inside row A's own 95 per cent interval.** Row A' is
the control: the same apparatus as F and G with upstream's own arithmetic in it,
reproducing row A exactly. On the ladder, the largest of these is worth about **+0.07
dB** against a shortfall of 1.5.

### The fast_atanh error table

```
        x    fast_atanh(x)    Math.Atanh(x)   absolute error      ratio
 0.000000         0.000000         0.000000         0.000000     1.0000
 0.500000         0.549305         0.549306         0.000002     1.0000
 0.700000         0.867145         0.867301         0.000155     0.9998
 0.900000         1.455777         1.472219         0.016443     0.9888
 0.950000         1.755443         1.831781         0.076337     0.9583
 0.990000         2.145311         2.646652         0.501341     0.8106
 0.999000         2.268464         3.800201         1.531737     0.5969
 0.999900         2.281835         4.951719         2.669884     0.4608
 0.999990         2.283183         6.103034         3.819851     0.3741
 0.999999         2.283319         7.254329         4.971009     0.3148
 1.000000         2.283333              inf              inf     0.0000
```

**The largest value `fast_atanh` can return is 2.283333**, swept over 2,000,000 points
across its whole reachable range and monotonic throughout. **So every
check-to-variable message is capped at 4.567**, against the exact arithmetic's 37.43 -
a **ceiling ratio of 8.2 to 1**. Its denominator's smallest magnitude on [-1, 1] is
**120**, so it does not vanish where the true function goes to infinity, which is why
upstream needs no clamp on it.

**The clamp on the exact `atanh` is `Math.BitDecrement(1.0)`**, the largest double
strictly below one. It is stated because it **cannot be tuned**: there is no value
between it and the pole to move to, so it is not a threshold chosen to flatter a row.

### What the arithmetic actually did, over 306 real -21 dB slots

```
arithmetic             tanh calls   ON THE CLAMP   fraction   largest msg   mean |msg|
upstream fast_tanh     50,410,062      2,813,051      5.58%        5.6797       0.7125
exact Math.Tanh        50,407,974      2,827,042      5.61%       37.4299       0.7166

atanh calls handed a product already at +/-1 : 80,121 (upstream), 1,116 (exact)
largest |product| handed to atanh            : 1.027844 (upstream), 1.000000 (exact)
```

**The clamp is not idle.** And the largest upstream message reads 5.68 where
`fast_atanh`'s own ceiling is 4.567, which is how the overshoot was found:
`fast_tanh(4.9699)` returns **1.007218**, so the product leaves [-1, 1] and
`fast_atanh`'s denominator falls from 120 to 86.8 toward its root at 1.1035. **The pole
is not reached.**

### The score census - the measurement that decides the unit

Oracle alignment control: **174.0 of 174** on all twelve messages at -5 dB.

```
over the failing trials             n     mean gap       lowest      highest   true higher
the library's decoder             292        -86.7       -275.5         -7.2         0.0 %
the independent decoder           288        -86.4       -274.4         -1.0         0.0 %

the true codeword's own score, over all 306 trials : mean 580.1, 406.8 to 721.9
the library's settled word, over its failures      : mean 662.7
the independent decoder's, over its failures       : mean 660.8
```

**Zero of 292 and zero of 288.** Beside it: at that same alignment the hard decisions
are wrong at a **mean of 30.9 of 174**, reproducing unit 222's *about 31* - but that
number is no longer what the conclusion rests on.

**Truth was used as a diagnostic and never as a decode.** The census is computed after
both decoders answered, from ratios never told what was sent, and no rate in this unit
counts a trial a decoder did not return on its own.

### The independent decoder's controls

```
CONTROL 1  it finds what is there    51 of 51 clean codewords returned EXACTLY, 1 iteration each
CONTROL 2  inverted ratios           0 of 51 passed parity, 0 returned the true codeword
CONTROL 3  5000 Gaussian arrays      2 passed parity, 0 passed the checksum, 0 became a message
                                     the LIBRARY's decoder on the same arrays: 2 of 5000
CONTROL 4  hard bit flips            k=0: 51/51 and 51/51.  k=6: 51/51 and 51/51.
                                     k=12: 32/51 and 38/51. k=17, 24, 31: 0 and 0.
```

Graph built from `LdpcNm` alone: **522 edges**, variable degrees 3 to 3, check degrees
6 to 7. **Control 4 confirms unit 215's 17 with a second instrument**, which matters
because that number is what unit 222's inference leaned on.

### The normalisation audit - twelve terms, ten SAME

SAME: the loop bound (`FTX_LDPC_N` both); the accumulation precision (`float sum`,
`float sum2`, the word `double` appearing **0 times** in the pin's body); the variance
expression `(sum2 - (sum * sum * inv_n)) * inv_n` term for term; the reciprocal formed
once and multiplied twice; **the mean removed from the variance and NOT from the
array**, measured as a count - exactly **one** write to `log174` in the whole function
and it is a `*=`, no subtraction of any kind; the target constant `24.0f`; the scale
factor `sqrtf(24.0f / variance)` then `log174[i] *= norm_factor`; `sqrtf` and not the
double-precision root; **where the call sits** - inside `ftx_decode_candidate`, after
extraction and before `bp_decode`, with **0 of 1** intervening statements mentioning
the array; and exactly **one** call site.

DIFFERING: **the degenerate variance**, which is divergence 23, recorded, deliberate,
and able to fire only on 174 identical ratios; and **what comes back**, `void` against
`float`, which is a return type and changes not one value in the array.

**The port is faithful here too.**

### The normalisation target swept

```
 target      n of 306    on the clamp
    3.0         0           0.00 %
    6.0         5           0.02 %
   12.0        12           0.65 %
   18.0        14           2.68 %
   21.0        14           4.06 %
   24.0        13           5.58 %   <-- upstream's
   27.0        13           7.17 %
   30.0        13           8.78 %
   36.0        13          11.93 %
   48.0        12          17.63 %
   96.0        10          33.28 %
  192.0         2          48.59 %
```

0 wrong at every target and the row at 24 reproduces the before-number exactly. The
clamp column explains the shape: the target decides how hard the decoder is driven into
`fast_tanh`'s saturation, so **the constant is not a free scale**.

### The gates

```
Ft8Sharp tests, from the TRX Counters element, never from a console line:
  entry, before any change                     : 512 total, 511 passed, 0 FAILED, 1 skipped, 4 m 57 s
  exit, after both version bumps               : 518 total, 517 passed, 0 FAILED, 1 skipped, 5 m 38 s
                                                 SIX TESTS ADDED and still the one correct skip
  the one skip                                 : Ft8TableGenerationTests.RewriteTheCheckedInTablesFile,
                                                 the table write gate - and NOT a reference test,
                                                 so the pinned clone is reachable
  and 512 with the four untracked probes and 512 without: they compile and declare nothing
library rebuilt --no-incremental, before and after: 0 warnings, 0 errors
Ft8Sharp.csproj                                : 0 PackageReference, 0 ProjectReference ELEMENTS
attribution from 2828ab6                       : 192 paths at entry, 198 at exit - the six added
                                                 are this unit's own test files - and the
                                                 src/Hamlet. tests/Hamlet. filter STILL RETURNS 0
channel tests, unit 222's own filter strings   : App 9 of 9 in 752 ms; RadioEngine 38 of 38 in 13 m 36 s
  re-run after both bumps                      : App 9 of 9 in 598 ms; RadioEngine 38 of 38 in 14 m 19 s
                                                 BOTH SETS RUN TWICE, both green both times
versions                                       : Ft8Sharp 0.10.6 -> 0.10.7 under HM-DEC-152
                                                 root     1.12.29 -> 1.12.30 under HM-DEC-150
                                                 BOTH PATCHES - no library file changed
divergences in porting-notes.md                : 25, unchanged - none added
wrong messages over every substituted row       : 0 of 5202 slot decodes
```

### What did not move, said plainly

The verdict band. The decibel axis. The candidate limit, the minimum sync score, the
byte-quantised waterfall, the 25-iteration bound, `NormalisedVariance` at 24.0f,
`fast_tanh`, `fast_atanh`. **Not one library file changed except `porting-notes.md` and
the version in the props file**, and `git diff` from `c344786` confirms it.

## 4. What's blocking us

**Two items. Neither is a ruling request from me. The first is criterion 2 itself, so
it bears directly on B.**

### 1. Criterion 2 is still NOT MET, and it is now not reachable by any unit

**13 of 306, 4.2 per cent, 95 per cent Wilson 2.5 to 7.1, at a delivered -21.001 dB**,
against a band fixed in writing before unit 221's first trial and untouched tonight:
40 per cent or better MET, under 25 per cent NOT MET. **NOT MET.**

Beside it, the rows that were meant to move it: **exact arithmetic 15 of 306, exact
arithmetic at 100 iterations 17 of 306, an independent soft decoder 17 of 306.** All
three inside the as-is interval.

**The outcome, in the instruction's own words: the information is not in the ratios,
now measured rather than inferred.** In **zero of 292** failing trials for the
library's decoder and **zero of 288** for the independent one does the true codeword
score higher than the word the decoder settled on. The ratios prefer the wrong answer
everywhere.

**What follows.** **No unit-reachable change to the decoder moves criterion 2.** The
routes left are the owner's - the reference decoder under `HM-OPEN-065`, and the
deliberate-divergence ruling unit 222 put to him - and **this unit has taken neither.**

### 2. Both remaining routes are owner-class, and both are now waiting

Stated as a blockage rather than as a request, because the phase cannot move on
criterion 2 until one of them is decided.

- **`HM-OPEN-065`** needs a compiler run to build `decode_ft8.exe`. That is owner-class
  under `ARBITER.md` section 6 and no unit may do it. It is the only instrument left
  that could say whether `ft8_lib` itself hears these samples at -21 dB.
- **The deliberate-divergence ruling.** Whether this library may hear better than the
  code it was ported from. **This unit added a fourth priced number to it and refused
  to take it**, which is the plan's ruling that inheriting Goba's bugs is accepted
  working exactly as intended - and tonight is the first night that refusal cost
  measurable decodes.

**Not raised, because they are recorded and parked:** the channel set's runtime against
the plan's *about a minute* (unit 222 logged it; tonight it took 13 m 36 s on a machine
about three times slower than unit 222's, and that is a budgeting fact for the next
unit rather than a question); `PROJECT_STATUS.md` reading `HM-DEC-152` where `CLAUDE.md`
holds `CPS-DEC-0152`; and `PHASE_OUTCOME.md`'s header disagreeing with its own entries
on steps 1 and 3. **Citing a recorded issue is not raising it.**

**One piece of housekeeping the next unit should know:** `TestResults223/` at the root
is mine, holds the TRX logs and two scratch files, is untracked and was never
committed, and is disposable. The `.obj` files, `tools/build-ft8-oracle.bat`,
`.run-unit/` and `.unit222/` were left untouched, as instructed.
