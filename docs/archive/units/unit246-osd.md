# Ordered statistics decoding in Ft8Sharp.Deep — the ceiling, the orders, and what moved

**Written by unit 246, the first unit of this phase to attempt step 2.** Everything
below was measured on this tree on 2026-09-04/05, at `Ft8Sharp` 0.10.7 and
`Ft8Sharp.Deep` 0.2.0. It is written up so the next unit on step 2 does not have to
re-measure any of it.

Nothing here is a plan. Where it names a cost, the cost is measured and the
measurement is named.

---

## 0. The one-line answer

**Ordered statistics decoding works, it is worth having, and it does not close the
1.5 dB.**

At -21 dB over 306 trials the decode rate went from **4.2 per cent (13 of 306)** to
**10.8 per cent (33 of 306)**, 95 per cent Wilson **7.8 to 14.8**, **zero wrong
decodes**, for **8.4 ms a trial** — thirteen per cent over the port. Step 2's first
exit asks for 40 per cent at -21 dB and that is not met and is not close.

**The interpolated 50 per cent crossing moved from -19.54 dB to -19.81 dB.** So of
the 1.5 dB `HM-OPEN-067` records, ordered statistics decoding closed about
**0.27 dB** and left about **1.2 dB** somewhere else.

---

## 1. The ceiling: what OSD could ever reach, measured before it was built

`tests/Ft8Sharp.Tests/Dsp/Ft8Unit246CeilingTests.cs`. One whole 51-trial block at
-21 dB, the harness's own seed 220791, 1000 Hz, offset 5760 samples. For every
candidate the sync search returned, the hard decision was compared against **the
codeword the ladder knows it transmitted**.

The population, from `Ft8SlotResult`'s own five counts:

```
candidates returned by the search   667
of those, reached parity              3
of those, passed the checksum         3
of those, became text                 3
trials whose own message came back    3 of 51
searches that returned nothing        0
```

**The closest candidate's total hard-decision distance**, all 51 trials, sorted:

```
15 16 20 21 21 25 25 25 26 26 27 27 27 27 28 28 29 29 29 29 30 30 30 31 31 31 31
31 31 32 32 33 34 34 34 35 35 36 38 38 38 39 40 40 41 42 43 44 45 71 81
```

| at or below | trials |
|---|---|
| 10 | 0 of 51 |
| 17 | 2 of 51 |
| 25 | 8 of 51 |
| 35 | 37 of 51 |
| 45 | 49 of 51 |
| 60 | 49 of 51 |

The median is about **31**, which is exactly unit 222's figure, against a code whose
iterative recovery reaches zero at about **17**. That is why belief propagation gives
up.

**How many of those errors fall inside the 91 most reliable positions** — this is
the number that decides which order could possibly reach the trial:

```
0 1 2 2 2 2 2 3 3 3 4 4 4 4 4 4 5 5 5 5 5 6 6 6 7 7 7 7 7 7 7 7 8 8 8 8 8 8 9 9
10 10 10 10 10 11 11 11 12 41 44
```

| order λ | trials whose basis carries at most λ errors |
|---|---|
| 0 | 1 of 51 |
| 1 | 2 of 51 |
| 2 | 7 of 51 |
| 3 | 10 of 51 |
| 4 | 16 of 51 |

**Two trials had no candidate within 60 of the truth at all** — trial 3 at 81 of 174
and trial 7 at 71 of 174, against a chance distance of 87. For those two the sync
search never returned a place near the signal and no amount of code searching helps.

### What that distribution admits, and the caveat on it

**The count is a lower bound on the basis error count.** The most reliable basis is
the first 91 *independent* columns in reliability order, and where a leading column
is dependent the basis reaches one place further down. So the true numbers are at
least these.

**And reachability is not recovery.** An order-λ search reaching the transmitted
codeword also requires that codeword to *win* the soft-distance ranking, and only the
single best-ranked codeword per candidate is submitted to the gate. On this block
order 2 admitted 7 trials by the ceiling and recovered 1.

---

## 2. The generator, and why it is read off the encoder

`Ft8DeepOrderedStatistics` needs the 91 × 174 generator **G**. It does not unpack it
from `Ft8Tables.LdpcGenerator`, whose 83 rows are the *parity checks* in upstream's
packing. The code is systematic in its first 91 bits, so **row *i* of G is what
`LdpcEncoder.Encode` returns for the payload with bit *i* set and every other bit
clear**.

Checked two ways, and the check is the point: the 91 rows form the identity on
columns 0–90, and **500 random payloads encoded bit for bit the same** through the
derived G by GF(2) arithmetic and through `LdpcEncoder.Encode`. 87 000 codeword bits
compared, every one agreed. A mistake here would poison every number in this document
and would look exactly like an algorithm that does not work.

---

## 3. The order table — what each order buys and what it costs

`tests/Ft8Sharp.Tests/Dsp/Ft8Unit246OrderTests.cs`. One whole 51-trial block at
-21 dB, **every row seeing the same seed and the same noise draw**, delivered
-21.004 dB on every row.

| row | decoded | missed | **wrong** | ms/trial | worst slot ms | offered | accepted | re-encodings |
|---|---|---|---|---|---|---|---|---|
| `Ft8Sharp` | 3 | 48 | **0** | 65.5 | 77.8 | 0 | 0 | 0 |
| Deep OSD off | 3 | 48 | **0** | 64.9 | 74.3 | 0 | 0 | 0 |
| Deep order 0 | 3 | 48 | **0** | 66.3 | 102.1 | 664 | 0 | 664 |
| Deep order 1 | 4 | 47 | **0** | 65.8 | 75.5 | 664 | 1 | 61 088 |
| Deep order 2 | 4 | 47 | **0** | 74.3 | 110.1 | 664 | 1 | 2 780 168 |
| Deep order 3 | 5 | 46 | **0** | 311.4 | 511.6 | 664 | 2 | 83 446 208 |

- **Order 0 bought nothing.** Re-encoding the basis as it stands recovered no trial
  the port had missed.
- **Order 1 bought one decode of 51 for no measurable cost.**
- **Order 2 bought nothing over order 1 on this block** and cost 8.8 ms a trial.
- **Order 3 bought one more and cost 246 ms a trial**, with a worst slot of 512 ms.

The cost of an order is `1 + sum over i of C(91, i)`: **1, 92, 4187 and 125 672**
re-encodings per candidate for orders 0, 1, 2 and 3. Those counts are pinned by a
test.

**The default is order 2**, and the reasoning is on
`Ft8DeepOsdSettings.Default`. One decode either way is well inside the noise at 51
trials, so the choice was made on cost against the headroom §1 measured: the ceiling
admits 7 of 51 at order 2 against 2 of 51 at order 1, and order 2's worst observed
slot is 110 ms — a 136-fold margin against FT8's 15 seconds. **Order 3 is not ruled
out.** Separating it from order 2 needs more trials, not a bigger claim.

---

## 4. The scoreboard, whole

`tests/Ft8Sharp.Tests/Dsp/Ft8Unit246ScoreboardTests.cs`. 306 trials at each rung,
three columns, the same audio handed to all three.

```
decoder      requested  delivered  trials  DECODED  MISSED  WRONG    rate   lo 95   hi 95    ms/tr
Ft8Sharp         -19.0    -19.001     306      248      58      0    81.0    76.3    85.0     65.2
Deep OSD off     -19.0    -19.001     306      248      58      0    81.0    76.3    85.0     64.3
Deep OSD on      -19.0    -19.001     306      276      30      0    90.2    86.3    93.0     72.6
Ft8Sharp         -20.0    -20.000     306       73     233      0    23.9    19.4    28.9     64.8
Deep OSD off     -20.0    -20.000     306       73     233      0    23.9    19.4    28.9     64.2
Deep OSD on      -20.0    -20.000     306      125     181      0    40.8    35.5    46.4     72.6
Ft8Sharp         -21.0    -21.001     306       13     293      0     4.2     2.5     7.1     64.1
Deep OSD off     -21.0    -21.001     306       13     293      0     4.2     2.5     7.1     63.7
Deep OSD on      -21.0    -21.001     306       33     273      0    10.8     7.8    14.8     72.5
```

**The OSD-off column equals the port on every rung, decode for decode and miss for
miss.** That is what makes the third column attributable to ordered statistics
decoding and to nothing else, and it is asserted per rung rather than printed and
hoped over.

**Zero wrong decodes on all nine rows.**

The stage itself, across all three rungs: **11 451 candidates offered, 100 codewords
the port then accepted** past its own parity and CRC-14 gates, **47 945 337
re-encodings** spent.

Time budget, worst single slot observed rather than the mean:

| column | worst slot ms | its candidates | margin against 15 s |
|---|---|---|---|
| `Ft8Sharp` | 103.5 | 20 | 145× |
| Deep OSD off | 102.2 | 16 | 147× |
| Deep OSD on | 110.2 | 14 | 136× |

### How much of the decibel that is

Linear interpolation of the decode rate between the -19 and -20 dB rungs, which is
the same arithmetic `HM-OPEN-067`'s "near -19.5" was read off:

| | 50 per cent crossing |
|---|---|
| `Ft8Sharp` | **-19.54 dB** |
| Deep OSD on | **-19.81 dB** |

**About 0.27 dB of the 1.5 dB, and about 1.2 dB still out there.** The interpolation
is over three rungs and is quoted as an interpolation, not as a measured crossing.

---

## 5. What the next unit on step 2 should know

1. **The ceiling is not the binding constraint at order 2 — the ranking is.** On the
   51-trial block the ceiling admitted 7 trials at order 2 and 1 was recovered; over
   306 trials the OSD-on column recovered 33 against the port's 13. The gap between
   admitted and recovered is codewords where the transmitted one was reachable but
   did not win the soft-distance ranking against the single submission allowed.

2. **More submissions is the obvious lever and it is the one that fails quietly.**
   Every codeword put to the CRC-14 is an independent false accept at about one in
   16 384. 140 candidates × an order-2 search of 4187 is 586 180 codewords, about 36
   wrong messages a slot. A unit that wants to submit more than one per candidate
   must state the arithmetic and the expected wrong count, and a wrong decode is
   worse than a missed one.

3. **Two of 51 trials had no candidate near the signal at all.** That is about 4 per
   cent of the population that step 2 cannot reach by any means, and it belongs to
   step 4's baseband re-sync. It is not where the missing 1.2 dB is — 49 of 51 trials
   *did* have a candidate near the signal — but it is a floor under any coding
   approach.

4. **Order 3 is unresolved.** It cost 246 ms a trial against order 2's 74 and bought
   one more decode of 51, which 51 trials cannot separate from noise. 306 trials at
   order 3 is about 25 minutes of wall clock and would settle it.

5. **The OSD-off identity test is not optional and must stay.** Without it the second
   and third scoreboard columns stop being comparable and the whole seam stops paying
   for itself.
