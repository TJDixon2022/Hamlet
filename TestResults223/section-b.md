
### Is the information in the ratios? Measured rather than inferred

**An independently written soft decoder, and deliberately different arithmetic at every choice.**

- **The domain.** Upstream multiplies hyperbolic tangents; this works in the **log domain** through
  Gallager's `φ(x) = -log tanh(x/2)`, which is its own inverse, so the check update is a **sum** and
  no product of tangents is ever formed. Public literature, and nothing was read from
  `ft4_ft8_public/` or WSJT-X for it.
- **The sign convention.** Internally `L = log(P(0)/P(1))` — the **opposite** of the library's — so a
  convention error in either would show as total disagreement rather than as a small one.
- **The exclusion.** Prefix and suffix sums, so a message never has its own term subtracted back out
  and there is no cancellation anywhere.
- **The graph.** Built from **`LdpcNm` alone**, with the variable-side incidence **counted** rather
  than read from `LdpcMn`, so a fault in that table could not be shared between the two decoders. It
  finds **522 edges**, variable degrees 3 to 3, check degrees 6 to 7.
- **The precision and the bound.** Double throughout, exact `Exp` and `Log`, **100 iterations —
  four times upstream's 25.**

**Watched refusing before it was believed, and one of the four controls refused on its first run.**

```
CONTROL 1  it finds what is there    51 of 51 clean codewords returned EXACTLY, in 1 iteration each
CONTROL 2  inverted ratios           0 of 51 passed parity, 0 returned the true codeword
CONTROL 3  5000 Gaussian arrays      2 passed parity, 0 passed the checksum, 0 became a message
                                     the LIBRARY's decoder on the same arrays: 2 of 5000
CONTROL 4  hard bit flips            k=0: 51/51 and 51/51.  k=6: 51/51 and 51/51.
                                     k=12: 32/51 and 38/51. k=17, 24, 31: 0 and 0.
```

**Control 3 was written expecting zero on its first line and did not get it, and that is reported
rather than smoothed.** At 500 arrays it read 1; it was **widened to 5000 rather than loosened**, and
at 5000 the independent decoder and the library's own read **identically** at 2 of 5000. Neither
produces a message, which is the gate row H actually counts at. **Control 4 confirms unit 215's 17
with a second instrument**, which matters because that number is what unit 222's inference leaned on.

**One bound in the control file was widened after a result was seen, and it is declared rather than
buried.** The assertion that `φ` is its own inverse was written at a relative 1e-9 across the whole
table and read 1.6e-3, at x = 35. **The reason is the type and not the algorithm**: `φ`'s tail is
`2·exp(-x)`, which by x = 35 is 1.3e-15, so the round trip is reading back a number with nothing left
of it to read. The assertion is now scoped to x ≤ 20, where `φ` is still 4.1e-9 — and a message of
*that* size cannot change a decision taken on sums of order one. **No measurement moved and the
verdict band was not touched.**

**Row H, over the identical ratios, at the same candidates, on the same 306 trials:**

```
row                                  n     of    rate   lo 95   hi 95   WRONG
H.  independent soft decoder         17    306     5.6     3.5     8.7       0
```

**Inside the as-is row's interval, as F and G are. All three substituted rows land inside it.**

### THE INFORMATION IS NOT IN THE RATIOS, NOW MEASURED RATHER THAN INFERRED

**The measurement that settles it is not the rate.** At the oracle alignment — swept, with its control
reading **174.0 of 174** on all twelve messages at -5 dB, so it is the most generous place either
decoder could be asked to work — the score of a word is
`sum over bits of ratio times (2*bit - 1)` on the normalised ratios: the log-likelihood of that word,
up to a constant. **Higher is more likely.**

```
over the failing trials             n     mean gap       lowest      highest   true higher
the library's decoder             292        -86.7       -275.5         -7.2         0.0 %
the independent decoder           288        -86.4       -274.4         -1.0         0.0 %

the true codeword's own score, over all 306 trials : mean 580.1, 406.8 to 721.9
the library's settled word, over its failures      : mean 662.7
the independent decoder's, over its failures       : mean 660.8
```

**In not one failing trial — not a minority, zero of 292 and zero of 288 — does the true codeword
score higher than the word the decoder settled on.** The least negative gap anywhere is -7.2 and
-1.0. **The ratios themselves prefer the wrong answer.**

So no decoder that searches these ratios for the most likely codeword can recover the message,
however patient or however exact. That is **a fact about the ratios and not about any decoder**, and
it means **criterion 2 is not reachable by any change to the correction stage.**

Beside it, for the record: at that same oracle alignment the hard decisions are wrong at a **mean of
30.9 of 174**, which reproduces unit 222's *about 31* — but that number is no longer what the
conclusion rests on.

**Truth was used as a diagnostic and never as a decode.** The census is computed *after* both
decoders have answered, from ratios that were never told what was sent. **No rate in this unit counts
a trial a decoder did not return on its own**, and no function that returns a decode takes the
message, the codeword, the frequency or the time.

### The normalisation target, swept — and upstream's constant vindicated

`NormalisedVariance` is `24.0f` and upstream's own comment calls it an *experimentally found
coefficient*. **This is the first time this tree has measured it.**

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

0 wrong at every target, and **the row at 24 reproduces the before-number exactly**, which is what
makes this a sweep of the target rather than of something else. **24 sits on a broad plateau** —
everything from 18 to 36 reads 13 or 14 — and **the best row in the whole sweep is one decode better
and lies inside the 24 row's own interval.** A sweep's best row is the maximum of twelve draws and is
biased upward by construction, so it is not read as a gain.

**And the clamp column explains the shape.** The target decides how hard the decoder is driven into
`fast_tanh`'s saturation, so the constant is **not a free scale**: upstream chose a number that puts a
typical ratio right on the clamp — variance 24 is a standard deviation of 4.9 against a clamp at
4.97. **24.0f stays where upstream put it.**

### No fix was licensed, and tonight the refusal cost something

**Both conditions failed.** Condition 1: the audit named two differing terms and **neither is a place
where this port's arithmetic differs from the pin** — one is divergence 23, already recorded and
unable to fire on real ratios, the other a return type. Condition 2 fails outright: **no row
attributed a decode at -21 dB to any difference**, and the score census says why in a way no rate
can.

**So nothing under `src/Ft8Sharp/` changed except this file and the version in the props file**, and
git confirms it.

**This is the first night a substituted row plainly decoded better, and the answer is still no.**
Four numbers in this path were measured to be worth decodes and **not one of them moved**:

| what could have been moved | measured worth at -21 dB | whose number it is |
| --- | --- | --- |
| exact `tanh` and `atanh` for `fast_tanh`/`fast_atanh` | +2 decodes in 306 | upstream's, ported faithfully |
| exact arithmetic at 100 iterations | +4 decodes in 306 | upstream's `kLDPC_iterations` |
| an independent soft decoder | +4 decodes in 306 | not a decoder Hamlet would run |
| a normalisation target of 18 rather than 24 | +1 decode in 306 | upstream's *experimentally found coefficient* |

**Measuring what upstream's approximation costs is what step 6 exists for; replacing it is the
owner's ruling.** The plan's ruling that *inheriting Goba's bugs is accepted* is what licensed every
row above and equally what forbids taking any of them. **This unit adds a fourth number to the
deliberate-divergence question unit 222 put in front of the owner, and does not take the ruling.**

**No divergence added — still 25.** The verdict band is untouched and nothing was re-binned.
**Criterion 2 stands NOT MET at 13 of 306, 4.2 per cent**, and the curve stands as units 221 and 222
measured it; it was not re-run to fill a section, because no library file changed.

### What this is evidence about, and five things it is not

**It is evidence** that upstream's `fast_tanh` and `fast_atanh` cost **two decodes in 306** at -21 dB
and no more, priced against a transcription control that reproduces the as-is row exactly; that the
approximation is nevertheless **materially in play** — a message ceiling 8.2 times lower than the
exact one and 5.58 per cent of calls on the clamp; that **the normalisation is a faithful port**, ten
of twelve terms identical and both differences already recorded; that **24 is a good number on a
broad plateau**; and above all that **the true codeword is not the most likely word these ratios
describe, in every failing trial**, so the recovery is not there to be had.

**It is not evidence about:**

1. **What `ft8_lib` itself would decode at -21 dB on these same samples.** Nothing tonight ran
   upstream's decoder, because building it is owner-class — `HM-OPEN-065`, cited and not re-raised.
   Everything here says what *these ratios* contain; it cannot say what the C achieves on its own
   extraction of its own audio.
2. **Whether a different EXTRACTION would put the information in the ratios.** The finding is about
   the ratios this library produces, and unit 222 measured three ratio rules agreeing at 143.1, 143.1
   and 143.5 of 174 — but three rules are not all rules, and nothing tonight went above extraction.
3. **The published figure itself.** The QEX paper is not on this machine, so **-21 dB at 50 per cent
   remains an assumption taken from the plan**, honestly labelled.
4. **Impaired air.** Everything here is aligned to the block grid, on a bin centre, alone in the
   passband, with no drift; and nothing here touches criterion 3's 760 of 1298.
5. **Anything reaching a radio or a screen.** The encoder ran thousands of times tonight to build
   samples in memory and nowhere else — no device, no stream, no port, no file. And nothing here is
   evidence about the CW decoder.
