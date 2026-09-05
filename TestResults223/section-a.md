
## The arithmetic inside the loop, and whether the information was ever there — unit 223

**Unit 222 closed with two things it had not measured, and this unit measured both.** The first it
named itself: every row of its loss budget passed through upstream's `fast_tanh` and `fast_atanh`, so
*a stage every row shares is a stage no row can see*, and it said in terms that pricing them would
have meant running arithmetic the pin does not run. The second it did not name, because it did not
notice: its conclusion that **the information is not in the ratios** was an *inference*, read off a
comparison between about 31 soft-decoded errors and a correcting power of 17 measured over **hard bit
flips**. Those are not the same kind of thing. A soft decoder is given a magnitude per bit and
routinely closes error counts past its hard-decision limit — that is the entire reason
log-likelihood ratios exist rather than bits.

**Both are now measurements. The conclusion survives, and it survives far more sharply than the
inference could have carried it.**

### The ground, and the before-number

The -21 dB rung alone, on unit 221's population, seeds, frequency, offset and trial count, nothing
widened: **13 of 306, 4.2 per cent, 95 per cent Wilson 2.5 to 7.1, at a delivered -21.001 dB, 0 wrong
messages, worst delivery error 0.0406 dB.** Unit 221's figure to the numerator, and the **fourth
process** to produce it.

**One budgeting fact, recorded because two units have now quoted a per-decode cost and they
disagree.** One slot decode cost **75.5 ms** tonight against unit 222's 26.1, the `Ft8Sharp` suite
took 4 m 57 s against its 1 m 42, and the RadioEngine channel set 13 m 36 s against its 7 m 38. The
machine is about three times slower tonight. **Rates are deterministic in the seeds and were
unaffected**; only wall time was.

**And the four untracked probe sources are answered rather than repaired.** `Unit216Probe.cs`,
`Unit217Probe.cs`, `UpstreamSyncSearchProbe.cs` and `UpstreamLdpcProbe.cs` all **compile** — the
build succeeds with them present — and all four are **comment-only**, six to eight comment lines each
and not one declaration, emptied by units 214 to 217 when their reads were committed into the
inventory tests. **The test count is 512 with them and 512 without**, so there is no reproducibility
gap of the kind unit 222 closed for the channel filter strings, and they are left where they are.

### The normalisation audited against the pin — the one stage nobody had read

**Unit 222's audit rule skipped this stage by construction.** That rule picked the stage its largest
budget row named, and **no budget row could ever have named this one**: the normalisation sits
between extraction and correction, every row passed through it, and it cancels out of every delta a
budget can form.

`ftx_normalize_logl` in `ft8/decode.c` against `Ft8SoftSymbols.Normalise` and
`Ft8SoftSymbols.Variance`, term by term, through a checked-in test that **reports skipped when the
clone is absent**. **Twelve terms audited, ten SAME, two DIFFERING and both already recorded.**

| term | pin | port | |
| --- | --- | --- | --- |
| the loop bound | 2 loops, both `FTX_LDPC_N` | both `ratios.Length`, refused unless 174 | SAME |
| the accumulation precision | `float sum`, `float sum2`, the word `double` appearing **0 times** | `0.0f` and `0.0f`, single precision throughout | SAME |
| the variance expression | `(sum2 - (sum * sum * inv_n)) * inv_n` | `(sumOfSquares - (sum * sum * inverseCount)) * inverseCount` | SAME |
| the reciprocal | `1.0f / FTX_LDPC_N` formed once, multiplied twice | `1.0f / ratios.Length`, multiplied twice | SAME |
| **the mean removed from the ARRAY** | **1 write to `log174` in the whole function and it is a `*=`** — no subtraction of any kind | 1 write to `ratios`, and it is a `*=` | SAME |
| the target constant | `24.0f` | `NormalisedVariance = 24.0f` | SAME |
| the square-root scale factor | `sqrtf(24.0f / variance)` then `log174[i] *= norm_factor` | `MathF.Sqrt(...)` then `ratios[i] *= factor` | SAME |
| the root's own precision | `sqrtf`, not the double-precision `sqrt` | `MathF.Sqrt`, not `Math.Sqrt` | SAME |
| **where the call sits** | inside `ftx_decode_candidate`, after extraction and **before** `bp_decode`, **0 of 1** intervening statements mentioning the array | normalise then decode, nothing between | SAME |
| how many times it normalises | 3 mentions, 2 of them the declaration and the definition, so **1 call site** | once per candidate | SAME |
| the degenerate variance | **no guard** — divides unchecked | `if (!(variance > 0.0f)) return variance` | **DIFFERING — divergence 23** |
| what comes back | `void`, the variance discarded | `float`, the pre-scale variance returned | **DIFFERING — a return type, not arithmetic** |

**The port is faithful here too.** Divergence 23 is recorded and deliberate, and the port's side is
the only defensible one — there is no faithful port of dividing by zero and multiplying an array by
the NaN that comes out. **It can only fire on 174 identical ratios**, which no real waterfall
produces, so it cannot cost a decode: the same shape of argument that retired divergence 21 in unit
222. The return type changes **not one value in the array**.

**One mistake of mine the instrument caught before it reached a verdict.** The first slice of the
region between `ftx_normalize_logl` and `bp_decode` started at the open bracket rather than after the
semicolon, so the call's own argument `log174)` sat inside the region and the row read **DIFFERING**
at 1 of 2 statements mentioning the array. It is fixed, the reason is written into the file so nobody
rediscovers it, and it reads 0 of 1.

### The price of the approximation, and it is two decodes in 306

The row unit 222 named and could not take. **A copy of `LdpcDecoder.Decode` in the test project, with
exactly two terms moved** — `Math.Tanh` and `Math.Atanh` in double precision — and everything else
transcribed: the loop bound, the hard decision at the top, the all-zero refusal, `ldpc_check` with
its running minimum and its break at zero, `min_errors` from `FTX_LDPC_M`, both message passes with
their exclusions, the parity row bound at `NUM_ROWS[m]`, the single-precision message arrays.

Same population, same seeds, same frequency, same offset, same 306 trials, same rung.

```
row                                  n     of    rate   lo 95   hi 95    delta  WRONG
A.  as-is, the library's own path    13    306     4.2     2.5     7.1      0.0      0
A'. as-is, TRANSCRIBED (control)     13    306     4.2     2.5     7.1      0.0      0
F.  exact tanh/atanh, 25 iterations  15    306     4.9     3.0     7.9     +0.7      0
G.  exact tanh/atanh, 100 iterations 17    306     5.6     3.5     8.7     +1.3      0
```

**Row A' is the control every row above depends on** and it is why F and G are readable: the same
substituted apparatus, with **upstream's own arithmetic** in it, reproduces row A exactly. A
substituted decoder that did not reproduce the as-is number would be a wiring mistake rather than a
measurement.

**Both F and G lie inside the as-is row's own 95 per cent interval**, which is the reading fixed in
writing before the run.

**And the approximation is not idle, which is what made the row worth taking.** `fast_atanh`'s
**largest returnable value is 2.283333**, swept over 2,000,000 points across its whole reachable
range, so **every check-to-variable message is capped at 4.567** against the exact arithmetic's
37.43 — a ceiling ratio of **8.2 to 1**. Its error against the true function:

```
        x    fast_atanh(x)    Math.Atanh(x)   absolute error      ratio
 0.500000         0.549305         0.549306         0.000002     1.0000
 0.900000         1.455777         1.472219         0.016443     0.9888
 0.990000         2.145311         2.646652         0.501341     0.8106
 0.999000         2.268464         3.800201         1.531737     0.5969
 0.999999         2.283319         7.254329         4.971009     0.3148
 1.000000         2.283333                inf             inf     0.0000
```

Its denominator's smallest magnitude anywhere on `[-1, 1]` is **120**, so it does not vanish where the
true function goes to infinity, which is why upstream needs no clamp on it. And the `fast_tanh` clamp
**is** in play: **5.58 per cent of 50,410,062 calls** land on ±4.97 over 306 real slots.

**One thing neither the instruction nor unit 222 expected, found only because the largest message came
back above the ceiling `fast_atanh` allows.** In exact arithmetic a product of hyperbolic tangents
cannot leave `[-1, 1]`. **`fast_tanh` overshoots one just below its own clamp** — `fast_tanh(4.9699)`
reads **1.007218** — so the product does leave the range the inverse was fitted on, reaching
**1.027844** in these slots, **80,121 `atanh` calls are handed an out-of-range argument**, and
`fast_atanh`'s denominator falls from 120 to **86.8** toward its own root at |x| = 1.1035. **The pole
is not reached.** Recorded as an observation about upstream's arithmetic; it is not a porting error
and **nothing was fixed for it**.

**The clamp on the exact `atanh` is stated because a clamp chosen to flatter a row is the same failure
as a tolerance chosen after the measurement.** It is `Math.BitDecrement(1.0)`, the largest double
strictly below one — **the most generous clamp double precision admits, with no value between it and
the pole to move to.** It cannot be tuned.
