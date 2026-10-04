# Soft combining of a repeated transmission — the distances, the budget, and what it moved

**Written by unit 247, the first unit of this phase to attempt step 6.** Everything
below was measured on this tree on 2026-09-05, at `Ft8Sharp` 0.10.7 and
`Ft8Sharp.Deep` 0.3.0. It is written up so the next unit on step 6 does not have to
re-measure any of it.

Nothing here is a plan. Where it names a cost, the cost is measured and the
measurement is named.

---

## 0. The one-line answer

**Soft combining works, it is worth far more than ordered statistics decoding was, and
it stops working about a decibel further down.**

At -21 dB over 306 trials with the later slot displaced in frequency and time — the
conservative half of the pair — the decode rate went from the port's **4.2 per cent
(13 of 306)** to **22.2 per cent (68 of 306)**, 95 per cent Wilson **17.9 to 27.2**,
**zero wrong decodes**, at **128.9 ms a trial** for two slots. **55 of 306 trials had
no single slot decode the message alone and the combination did.**

With both slots on the same bin and the same sample it reads **70.9 per cent (217 of
306)**, Wilson **65.6 to 75.7**, zero wrong, with **200 of 306** trials that no single
slot could reach. §4 has both tables. **The truth on air is somewhere between the two
columns and this project has no fixture that could say where**, so the jittered figure
is the one quoted everywhere a single number is wanted.

**At -24 dB nothing decodes in any column.** Not the port, not ordered statistics
decoding, and not the combination. §1 measured why before a line of the combiner was
written: at that rung the sync search does not return a place near the signal, so
there is nothing to combine.

---

## 1. The trace: does adding two slots reach where one cannot

`tests/Ft8Sharp.Tests/Dsp/Ft8Unit247CombiningTraceTests.cs`. Each of the 51 population
messages synthesised **twice** — the same clean audio at 1000 Hz and offset 5760
samples, two independent `GaussianNoise` draws — and the closest candidate in each slot
compared against **the codeword the ladder knows it transmitted**, by the chain unit 246
verified against 500 random payloads.

**The code's iterative recovery reaches zero at about 17 of 174.** That is the line
every distribution below is read against.

### -21 dB

Slot A seed 220791 delivered -21.004 dB; slot B seed 221791 delivered -21.002 dB. The
two slots are the same rung and the delivered means are what say so.

| what | median | min | max | at or below 17 |
|---|---|---|---|---|
| slot A's closest candidate | 31 | 15 | 81 | **2 of 51** |
| slot B's closest candidate | 31 | 20 | 48 | **0 of 51** |
| combined, oracle pairing, equal weight | 18 | 8 | 48 | **23 of 51** |
| combined, oracle pairing, variance weighted | 18 | 6 | 39 | **24 of 51** |
| combined, highest-scoring candidate each slot | 23 | 8 | 98 | **16 of 51** |
| combined, best over every candidate pair | 18 | 8 | 43 | **23 of 51** |

Slot A's row reproduces unit 246's ceiling exactly — same seed, same block, same
median of 31 — which is what says the two measurements are of the same thing.

```
slot A closest, sorted:
15 16 20 21 21 25 25 25 26 26 27 27 27 27 28 28 29 29 29 29 30 30 30 31 31 31 31
31 31 32 32 33 34 34 34 35 35 36 38 38 38 39 40 40 41 42 43 44 45 71 81

combined, oracle pairing, equal weight, sorted:
8 8 12 13 13 14 14 14 14 14 15 15 16 16 16 16 16 16 17 17 17 17 17 18 18 18 18 19
19 19 19 19 19 20 20 20 20 20 20 21 22 23 24 24 24 24 27 27 29 46 48
```

- **The oracle combination is strictly closer than the better of the two slots on 48
  of 51 trials.**
- **22 of 51 trials had neither slot under 17 and the combination under it.**
- **The ceiling and the oracle pairing agree at 23 of 51**, so a pairing rule has
  essentially no headroom to lose — the best pair over all 7 790 candidate pairs
  examined is the same count as the truth-chosen pair.
- **The realistic rule — highest-scoring candidate in each slot — reaches 16 of 51.**
  That is the gap a rule with no truth in it pays, and it is why the production rule
  iterates over every candidate in the later slot rather than only the best one.

**Summed variance before re-normalisation, over the 51 oracle pairs: median 69.0, min
44.8, max 79.4.** Each input arrives at `Ft8SoftSymbols.NormalisedVariance` of 24, so
two independent hearings sum to about 48 and two that agree everywhere to about 96.
**69 says the two hearings agreed on rather more than half of what they said**, which
is the combining gain visible as a number before any decode.

### -24 dB

| what | median | min | max | at or below 17 |
|---|---|---|---|---|
| slot A's closest candidate | 69 | 42 | 84 | 0 of 51 |
| slot B's closest candidate | 72 | 40 | 84 | 0 of 51 |
| combined, oracle pairing, equal weight | 62 | 39 | 81 | **0 of 51** |
| combined, best over every candidate pair | 60 | 39 | 74 | **0 of 51** |

**Nothing reaches, and the reason is not the combining.** A candidate unrelated to the
transmission has a distance drawn from Binomial(174, 0.5) — mean 87, standard deviation
6.6. A median closest candidate of 69 is under three standard deviations from pure
chance. **At -24 dB the sync search is barely finding the signal**, so the ratios being
added are largely noise, and the summed variance says the same thing: median **48.9**,
which is exactly what two independent vectors of no information sum to.

**This is a synchronisation limit, not a coding one, and it belongs to step 4.**

---

## 2. The pairing, measured before it was designed

Over the same 51 trials at -21 dB, between the two slots' **closest** candidates:

| | median | max | within one bin, 3.125 Hz / 0.16 s |
|---|---|---|---|
| frequency gap | **0.00 Hz** | 1703.13 Hz | **49 of 51** |
| time gap | **0.000 s** | 2.240 s | **49 of 51** |

The two trials that miss are unit 246's two trials with no candidate within 60 of the
truth at all — `HM-OPEN-074` — so they are a synchronisation finding rather than a
pairing one.

**The closest candidate is not the highest-scoring one in 10 of 51 slot A trials and 9
of 51 slot B trials.** Rank of the closest, sorted:

```
slot A: 0 x41, then 1 1 1 1 1 2 3 3 7 13
slot B: 0 x42, then 1 1 1 1 1 1 2 4 7
```

**So a rule that pairs only the two best-scoring candidates is a different rule from
one that pairs the closest**, and this unit built the first while iterating the later
slot's candidates in full — every candidate of the current slot gets its turn, and only
the *partner* is chosen by score.

---

## 3. The rule, and the submission budget

`src/Ft8Sharp.Deep/Ft8DeepCombineSettings.cs`.

**The rule.** For each candidate of the current slot, look through each remembered slot
for candidates within **6.25 Hz** — one FT8 tone — and **0.32 s** — two symbol periods —
and take the **`MaximumPartners` highest-scoring** of them. Submit each combination to
`Ft8CodewordDecoder.Decode`. **The port's parity gate and CRC-14 gate are the only
acceptance and there is no checksum anywhere in `Ft8Sharp.Deep`.**

**The budget.** Every combination put to the CRC-14 is an independent chance of a false
accept at about **one in 16 384**.

| case | candidates/slot | submissions/slot pair | over 306 trials | naive expected wrong |
|---|---|---|---|---|
| pair every candidate with every candidate | 140 | 19 600 | 5 997 600 | **366.1** |
| this rule, worst case at the port's candidate limit | 140 | 140 | 42 840 | 2.61 |
| this rule, at the ladder's observed candidate count | 13 | 13 | 3 978 | **0.24** |

**Defaults are one partner per candidate and one slot of history**, which is the
smallest budget that can produce a combination at all. The worst-case row is above one
and is said to be: it is a slot returning the port's full candidate limit on every one
of 306 trials, and the ladder's slots carry one transmission and return about 13.

**What was actually spent**, counted by `Ft8DeepCombineCounts` rather than estimated —
see the tables in §4. Across the whole -21 dB jittered walk: **50 677 candidate pairs
looked at, 516 combinations submitted, 88 accepted by the port**, for a naive
expectation of **0.031** messages nobody sent. **Zero were returned.**

### What the port does with a wrong pairing

`tests/Ft8Sharp.Deep.Tests/Ft8DeepCombineGateTests.cs`. 56 deliberately wrong pairings
of eight messages, every input decodable on its own, codewords 52 to 94 of 174 apart:

```
ParityNeverSatisfied    51
ChecksumFailed           0
MessageNotReadable       0
Decoded, an input        5
Decoded, NOBODY SENT IT  0
```

**Five wrong pairings did decode, and every one of them returned one of its own two
transmissions.** Where two messages are near-neighbours — `HAMLET 247 A` and
`HAMLET 247 C` differ by one character and their codewords sit 54 apart — the
combination stays inside the stronger one's basin and belief propagation finishes on it.
**That is a real transmission returned, not an invented one**, and §0.0 is about
messages nobody sent. That count is zero.

GATE 1 is parity, at `Ft8CodewordDecoder.cs:80`: *"Until this holds, the bits are the
decoder's closest approach and not a codeword, so there is nothing here to compute a
checksum over."* GATE 2 is the checksum, at `:96`: *"belief propagation can converge on
a perfectly valid codeword that is not the one that was sent, and every parity check in
the code will agree with it. Only the checksum knows."*

---

## 4. The scoreboard, whole

`tests/Ft8Sharp.Tests/Dsp/Ft8Unit247ScoreboardTests.cs`. 306 trials at each rung, two
slots a trial, three columns, the same audio handed to all three. Column one is the port
on the first slot alone; column two is the sibling with ordered statistics decoding on,
on the same first slot; **column three is the sibling with ordered statistics decoding
OFF and combining on**, so the difference between columns one and three is soft
combining and nothing else.

### -21 dB, same placement — both slots on the same bin and the same sample

```
decoder      requested  delivered  trials  DECODED  MISSED  WRONG    rate   lo 95   hi 95    ms/tr
single slot      -21.0    -21.001     306       13     293      0     4.2     2.5     7.1     64.4
single + OSD     -21.0    -21.001     306       33     273      0    10.8     7.8    14.8     72.6
combined x2      -21.0    -21.000     306      217      89      0    70.9    65.6    75.7    129.0

  trials NO single slot decoded alone and the combination DID: 200 of 306
  trials SOME single slot decoded alone, no combining needed:   17 of 306
  trials a single slot decoded and the combination did NOT:      0 of 306

  candidate pairs the rule looked at        48344
  combinations submitted to the port          357
  of those, the PORT took past both gates     216
  naive expected messages nobody sent       0.022

  messages the combining stage added          211
  of those, the message that was sent         211
  of those, a message that was NOT sent         0

  worst single slot 102.5 ms, 11 candidates, 0 combinations - 146x margin against 15 s
```

**4.2 to 70.9 per cent, and 200 of 306 trials that no single slot could reach.** This
is the easy case — a station whose clock and oscillator did not move at all between
slots — and it is quoted as the upper end of a pair, not as the result.

### -21 dB, jittered placement — the later slot 2.00 Hz and 480 samples away

```
decoder      requested  delivered  trials  DECODED  MISSED  WRONG    rate   lo 95   hi 95    ms/tr
single slot      -21.0    -21.001     306       13     293      0     4.2     2.5     7.1     64.2
single + OSD     -21.0    -21.001     306       33     273      0    10.8     7.8    14.8     72.7
combined x2      -21.0    -21.000     306       68     238      0    22.2    17.9    27.2    128.9

  trials NO single slot decoded alone and the combination DID:  55 of 306
  trials SOME single slot decoded alone, no combining needed:   13 of 306
  trials a single slot decoded and the combination did NOT:      0 of 306

  candidate pairs the rule looked at        50677
  combinations submitted to the port          516
  of those, the PORT took past both gates      88
  naive expected messages nobody sent        0.031

  messages the combining stage added            62
  of those, the message that was sent           62
  of those, a message that was NOT sent          0

  worst single slot 101.8 ms, 11 candidates, 3 combinations - 147x margin against 15 s
```

### -24 dB, both placements

```
decoder      requested  delivered  trials  DECODED  MISSED  WRONG    rate   lo 95   hi 95    ms/tr
single slot      -24.0    -24.001     306        0     306      0     0.0     0.0     1.2     63.3
single + OSD     -24.0    -24.001     306        0     306      0     0.0     0.0     1.2     70.9
combined x2      -24.0    -24.000     306        0     306      0     0.0     0.0     1.2    126.6
```

The jittered run at -24 dB is the same three zeros. **76 combinations were submitted at
the same placement and 58 with the jitter, and the port accepted none of them** —
which is the gates working, not the stage failing.

**Zero wrong decodes on all twelve rows across the four configurations.**

### The regression check

The ordered statistics column was re-measured underneath the new one, because unit 247
added a hearing-capture path to `Ft8DeepSlotDecoder` and a claim that it changes no
decision is worth exactly what it is measured at:

```
decoder      requested  delivered  trials  DECODED  MISSED  WRONG    rate   lo 95   hi 95    ms/tr
Ft8Sharp         -21.0    -21.001     306       13     293      0     4.2     2.5     7.1     64.5
Deep OSD on      -21.0    -21.001     306       33     273      0    10.8     7.8    14.8     72.8
```

**Exactly what unit 246 left. Step 2's number did not move underneath step 6's.**

### The time budget

Worst single slot observed rather than the mean, over the whole measurement:

| configuration | worst slot ms | its candidates | its combinations | margin against 15 s |
|---|---|---|---|---|
| -21 dB, same placement | 102.5 | 11 | 0 | 146× |
| -21 dB, jittered | 101.8 | 11 | 3 | 147× |
| -24 dB, same placement | 100.2 | 12 | 0 | 150× |
| -24 dB, jittered | 113.3 | 14 | 0 | 132× |

The worst slot is a slot the *search* was slow on rather than one the combining was
slow on — three of the four carried no combinations at all. **The combining stage costs
nothing measurable at this budget**: one normalisation and one `Ft8CodewordDecoder`
call per submission, and the whole -21 dB jittered walk spent 516 of them across 612
slots.

**A trial costs two slots**, so the combined column reads about 129 ms a trial against
the port's 64 — but **what has to fit inside FT8's 15 seconds is one slot**, and one
slot is about 65 ms plus the combinations it carries.

---

## 5. What the next unit on step 6 should know

1. **The gain is real and it is much larger than step 2's.** Ordered statistics decoding
   moved -21 dB from 4.2 to 10.8 per cent; combining two slots moves it to 22.2 per cent
   with the jitter on. **The two stack in principle and were not run stacked** — column
   three has OSD off, deliberately, so that the difference is attributable to combining
   alone. Running both on at once is the obvious next measurement and it is one call.

2. **Placement jitter costs more than half the gain.** `HM-OPEN-075`. This is a
   synchronisation cost, not a combining one, and step 4's baseband re-sync is what
   would recover it.

3. **-24 dB is out of reach and the reason is measured.** The sync search does not
   return a place near the signal there — median closest 69 of 174 against a chance
   distance of 87 — so no amount of combining or code searching helps. **Do not spend a
   unit taking the combiner deeper without first taking the search deeper.**

4. **More repeats is untried.** `RunRepeats` takes any `repeats >= 2` and
   `Ft8DeepCombineSettings` allows a history of eight. Four repeats is 6 dB of processing
   gain against two repeats' 3 dB, and the budget grows linearly rather than
   quadratically because the rule is per candidate per remembered slot. Nobody has walked
   it.

5. **The weighting is unresolved and it does not matter yet.** Equal weight gave 23 of
   51 at -21 dB and variance weighting 24 of 51, which is one trial and is not a
   difference. The ladder delivers both hearings at the same ratio, which is the
   condition equal weight is optimal under; on real air it is not, and step 5's
   per-message SNR is what would settle it.

6. **The OSD-off, combine-off identity test is not optional and must stay.** Unit 246's
   ruling 4, carried forward. Without it the scoreboard's columns stop being comparable
   and the whole seam stops paying for itself.
