READ IN THIS ORDER - A the phase goal, B this step and its exit criteria, C what this report adds.

A. THE PHASE GOAL is that Hamlet reads FT8 as well as the best decoder there is and then reads it
   further - the 1.5 dB between -19.5 and -21, and then past it. Step 6 is the half that goes past.
   Steps 0 and 1 as this unit leaves them: step 0 `partial`, step 1 `done`, neither touched.
   Step 2 stays `partial` at 10.8 per cent (33 of 306), 0 wrong - it was cut down by the arbiter
   against unit 246's measured ceiling, not closed, and this unit re-measured it and it still reads
   13 of 306 and 33 of 306 exactly. Steps 3, 4 and 5 are open and untouched, not one line.
   Step 6 goes from `not started` to `partial`. What this unit did to the number it is judged on:
   at -21 dB over 306 trials, 55 trials that no single slot decoded alone were decoded by the
   combination with the placement jitter on and 200 with it off, 0 wrong in both. At -24 dB, 0 and
   0 wrong - nothing decodes there in any column.

B. THIS STEP AND ITS EXIT CRITERIA. Step 6's five must-pass and its one deferred, one by one.
   1 A repeat identified and its soft values combined before decoding - MET. A stated bounded
     pairing rule on frequency and time, with pairs offered and combinations submitted counted.
   2 A message decoded that no single slot could decode alone, at a stated SNR below the
     single-slot crossing - MET at -21 dB, which is 1.2 dB below the -19.81 dB crossing.
     NOT MET at -24 dB: 0 of 306 in every column, and section 3 has the distribution that says why.
   3 Zero wrong decodes, every combined decode passing the same CRC-14 - MET. 0 on all twelve rows
     of four configurations, and nothing in Ft8Sharp.Deep decides a message is real.
   4 The gain measured on the ladder and quoted with its trial count - MET. 68 of 306, 22.2 per
     cent, Wilson 17.9 to 27.2 jittered; 217 of 306, 70.9 per cent, Wilson 65.6 to 75.7 same-placed.
   5 Every combined decode verified against the ladder's own ground truth - MET. 273 combined
     decodes across the -21 dB runs, every one checked, every one the message that was sent.
   6 Decodes WSJT-X did not return on a real capture - DEFERRED by the plan itself. It needs a
     capture fixture nobody has (`HM-OPEN-073`, Tim's) and it gated nothing tonight.
   What is needed to close step 6: exit 2 at a rung below -21 dB. The distribution says that is
   step 4's work and not more combining - see section 4.

C. THIS REPORT, weighed against A and B. The thing here is task 1's summed-distance distribution,
   because it decided the night before a line of the combiner was written: at -21 dB the closest
   candidate in each of two hearings sits a median 31 of 174 from the transmitted codeword against
   a code that recovers to zero at about 17, and their sum sits at a median 18 with 23 of 51 trials
   at or below 17 - so soft combining reaches, and 22 of 51 trials crossed that line where neither
   hearing did. At -24 dB it does not: median 62 combined against 69 and 72 single, 0 of 51 under
   17, because the search is barely finding the signal there. That bears directly on A - it is why
   step 6's number moved 1.2 dB down and why it stops there - and on B's exits 2 and 4.
   Section 4 raises 1 item. It asks for no ruling and it stands in the way of no exit criterion in
   B; it is logged as `HM-OPEN-075` and it is an argument for which step comes next.

UNIT:       247 - complete at task 7 of 7 - 2026-09-05 01:17
PHASE GOAL: Hamlet reads FT8 as well as the best decoder there is, and then reads it further.
UNIT GOAL:  Ft8Sharp.Deep identifies the same transmission repeated in a later slot at the same
            frequency, adds the two slots' log-likelihood ratios before anything decodes them, and
            recovers a message neither slot could give up alone - under the port's own parity and
            CRC-14 gates and a stated, bounded number of submissions to them.
ADVANCED:   yes - step 6 went from not started to partial, and at -21 dB over 306 trials the decode
            rate went from the port's 4.2 per cent to 22.2 per cent jittered and 70.9 per cent
            same-placed, with zero wrong decodes on every row.
NUMBER:     trials only the combination decoded, of 306: -21 dB jittered 55, 0 wrong; -21 dB same
            placement 200, 0 wrong; -24 dB 0 either way, 0 wrong. Suites: Ft8Sharp.Tests 593
            passed / 0 failed / 1 skipped / 8 m 12 s; Ft8Sharp.Deep.Tests 51 / 0 / 0.
DRIFT:      0 consecutive units without advance  (was 0)

## 1. What Claude did

**Complete, at task 7 of 7. Nothing was dropped, including the named drop candidate.** Machine
`C:\Source\HamLet`, project confirmed as Hamlet by the instruction's four filesystem checks - both
must-exist files present, both must-not-exist absent. Branch `main`, which is trunk. Seven commits,
one per task, every one pushed; the last is `10b30fe`.

### The instruction against the tree

**Every claim in the instruction's "Verify this instruction against the tree" section was checked
and every one held.** `Ft8SoftSymbols.RatioCount` at `:73`, `Extract` at `:117`, `Normalise` at
`:287` returning the variance, `Variance` at `:323`, `HardDecision` at `:351`;
`Ft8CodewordDecoder.Decode` at `:70` with GATE 1 at `:80` and GATE 2 at `:96`, both commented as
such; `Ft8Candidate` at `:48` with `FrequencyHz` at `:93` and `TimeSeconds` at `:103`;
`Ft8DeepSlotDecoder.Decode` at `:171` and `:189` with `Osd` at `:130` and `LastOsd` at `:149`;
`Ft8DeepOsdSettings.Default` order 2 at `:86`; the type-list tripwire at
`Ft8DeepSlotDecoderTests.cs:181`; `Ft8LadderHarness.Run` at `:244` with `DefaultSeed` 221001;
`Ft8Step6Ladder.Population()` returning 51 and `CollapseBottomDecibels` -24.0; root version
`1.12.49`, `Ft8Sharp` `0.10.7`, `Ft8Sharp.Deep` `0.2.0`; highest issue id `HM-OPEN-074`.
**No mismatches to report.**

**The going-in baselines both held.** `Ft8Sharp.Tests` measured **586 passed / 0 failed / 1 skipped
/ 5 m 19 s**, against the instruction's 586 / 0 / 1 / 5 m 23 s. `Ft8Sharp.Deep.Tests` was **35 tests
total**, of which 34 passed and 1 - the type-list tripwire - failed the moment task 2 added a type,
which is the tripwire working. The one expected skip is
`Ft8TableGenerationTests.RewriteTheCheckedInTablesFile` and no second skip appeared. **No red
outside the expected set in either suite at any point.**

### What was traced, built and measured

**Task 1, the trace, before a line of the combiner.** `Ft8Unit247CombiningTraceTests` synthesises
each of the 51 population messages twice - same clean audio, two independent noise draws - at
-21 dB and -24 dB, and measures the hard-decision distance from the closest candidate in each slot,
and from their normalised sum, to the codeword the ladder knows it transmitted. Section 3 leads
with the distributions. Slot A's row reproduces unit 246's ceiling exactly, median 31, which is
what says the two measurements are of the same thing.

**Task 2, `Ft8DeepSoftCombiner`.** Normalise each input through the port's own
`Ft8SoftSymbols.Normalise`, add position by position, re-normalise, and return the variance the sum
carried before re-normalisation. Four properties pinned on synthesised ratios: disjoint errors
repair each other at every planted count from 5 to 60; two different codewords combine to something
at least three times further from either transmission than the hearing it came from; a hearing
combined with itself decides exactly what it decided alone, under both weightings, and its array is
not modified; and 72 degenerate combinations - all zero, all equal, both infinities, not-a-number,
mixed - none refused and none producing a ratio that is not finite.

**Task 3, the pairing rule and the budget.** `Ft8DeepCombineSettings`. One FT8 tone of frequency
tolerance and two symbol periods of time tolerance, both read off task 1's measurement rather than
chosen; one partner per candidate per remembered slot, which is the smallest budget that can
produce a combination at all. Section 3 has the arithmetic.

**Task 4, the loop.** `Ft8DeepRepeatDecoder` takes slots in order, returns the ordinary single-slot
result for each and then combines against a bounded history. `Ft8DeepSlotDecoder` grew
`rememberHearings`, off by default, because a finished `Ft8SlotResult` does not carry the ratios
the gate saw. **The superset property is asserted, not intended**, and the port's five counts are
left exactly as the port reported them.

**Task 5, the repeats ladder.** `Ft8LadderHarness.RunRepeats`, beside `Run` and not instead of it -
`Run` at `:244` was not touched. Seed arithmetic in `Run`'s own shape, and a test asserts that
repeat 0 draws bit-for-bit what `Run` draws, decode for decode and to nine decimal places of
delivered ratio.

**Task 6, the scoreboard.** 306 trials at -21 and -24 dB, both placements, four configurations,
twelve rows, plus the step 2 regression column. Section 3 has the tables.

**Task 7, the write-up.** `docs/unit247-combining.md` and `HM-OPEN-075`.

### Decisions this session made for itself, reproduced in full

**1. The weighting is equal, and the measurement did not separate the two.** Task 1 measured both:
at -21 dB, equal weight put 23 of 51 combinations at or below the recovery threshold and
pre-normalisation-variance weighting put 24 of 51. **One trial of 51 is not a difference and it is
not reported as one.** Equal weight is the default because the ladder delivers both hearings within
hundredths of a decibel of each other, which is exactly the condition under which equal weight is
optimal, and because the simpler rule is the one that cannot go wrong. Variance weighting exists
and is documented as a proxy for the fading case the ladder does not present.

**2. The combined column is scored on the union over a trial's slots, not on the last slot alone.**
This was changed during task 5 after the first walk showed 1 trial of 51 reading as *decoded by a
single slot and lost by the combination*, which the superset property makes impossible. It was an
artefact of the scoring - the first slot had decoded the message on its own and the last slot's
result did not carry it - and not a decoder losing anything. With the union it is **0 of 306 on
every configuration**. The two-chances baseline is reported separately so the combining gain is
still isolated.

**3. The combined column has ordered statistics decoding OFF.** Deliberately, so that the
difference between column one and column three is soft combining and nothing else. The two stages
have not been run stacked and that is said rather than implied.

**4. A wrong pairing that returns one of its own two transmissions is not a wrong decode.** Task 3's
gate test found 5 of 56 deliberately wrong pairings decoding, and every one returned one of the two
messages that went into it - where two messages are near-neighbours, the combination stays inside
the stronger one's basin. **§0.0 is about messages nobody sent**, so the assertion is written to
that count, and it is zero. Asserting that no wrong pairing may decode at all would have been
asserting something the phase does not require.

**5. The type-list tripwire was changed three times, deliberately.** Unit 246 left it as a tripwire
for exactly this. It fired at task 2 rather than task 4 because task 2 was where the first new type
landed, and it was rewritten at tasks 2, 3 and 4 as the assembly grew. **A test broke because the
assembly changed and the unit that changed the assembly came here** - it was not discovered
afterwards.

### One thing done wrong and corrected

**Four `UPDATED:` timestamps in `PROJECT_STATUS.md` were composed rather than read from the clock**,
running up to an hour ahead of it, before I checked `date` and found the drift. `CLAUDE_CODE.md` §7
names that as one of the recurring failures the standard exists to prevent. The value was corrected
and every subsequent one was read. The cadence itself was kept: the file was updated after every
task with `STATE`, `TASK: n of m`, `BALL`, `UPDATED` and a `NOTE` about what was moving.

## 2. What the owner should expect

**Hamlet can now hear a call it missed the first time, by adding what it heard then to what it
heard the second time and reading the sum.** Nothing in WSJT-X does this. It is the first thing in
this phase that aims past the best decoder there is rather than at it.

**Nothing is on by default and no behaviour Tim sees has changed.** `Ft8DeepRepeatDecoder` with
combining null is `Ft8DeepSlotDecoder`, which with OSD null is exactly `Ft8SlotDecoder`. The
application does not use the sibling yet.

**What will look wrong and is not:**

- **The combined column costs about 129 ms a trial against the port's 64.** A trial is now *two
  slots*, and what has to fit inside FT8's 15 seconds is one slot. The worst single slot observed
  anywhere in the measurement was **113.3 ms**, a 132-fold margin, and three of the four
  worst-slot observations carried no combinations at all - what is slow is the search.
- **The -24 dB tables are three columns of zero.** That is the measurement, taken deliberately at a
  rung 4.2 dB below the single-slot crossing so that *no single slot could decode this alone*
  needs no argument. Task 1 had already measured why it would be zero.
- **The two -21 dB numbers are very far apart - 22.2 per cent and 70.9 per cent.** They are the
  same measurement with and without a realistic clock and oscillator error between slots. **The
  lower one is quoted everywhere a single figure is wanted**, and the gap is `HM-OPEN-075`.
- **`Ft8Sharp.Tests` now takes 8 m 12 s rather than 5 m 19 s.** The three extra minutes are this
  unit's 306-trial walks. Seven tests were added and one skip is unchanged.
- **`Ft8DeepSlotDecoderTests.TheSiblingHoldsExactlyTheseTypesAndTheListIsAssertedWhole` was
  rewritten.** That is a tripwire unit 246 left on purpose, walked into on purpose.

## 3. What you should see

### 1. Task 1's distances - whether soft combining reaches a codeword at all

`tests/Ft8Sharp.Tests/Dsp/Ft8Unit247CombiningTraceTests.cs`. The same 51 messages heard twice, and
the closest candidate in each hearing measured against **the codeword the ladder knows it
transmitted**. **The code's iterative recovery reaches zero at about 17 of 174.**

**-21 dB.** Slot A seed 220791 delivered -21.004 dB, slot B seed 221791 delivered -21.002 dB - the
same rung, and the delivered means are what say so.

```
what                                          median  min  max   at or below 17
slot A's closest candidate                        31   15   81      2 of 51
slot B's closest candidate                        31   20   48      0 of 51
combined, oracle pairing, equal weight            18    8   48     23 of 51
combined, oracle pairing, variance weighted       18    6   39     24 of 51
combined, highest-scoring candidate each slot     23    8   98     16 of 51
combined, best over every candidate pair          18    8   43     23 of 51
```

**Soft combining reaches, and it reaches by about half the distance.** The median falls from 31 to
18. **48 of 51 trials had the combination strictly closer than the better of the two hearings, and
22 of 51 had neither hearing under 17 and the combination under it.** The ceiling - the best of all
7 790 candidate pairs examined - is the same 23 of 51 as the truth-chosen pairing, so a pairing rule
has essentially no headroom to lose. **Summed variance before re-normalisation: median 69.0, min
44.8, max 79.4**, against 48 for two independent hearings each arriving at the port's normalised
variance of 24 - so the two hearings agreed on rather more than half of what they said.

**-24 dB.**

```
what                                          median  min  max   at or below 17
slot A's closest candidate                        69   42   84      0 of 51
slot B's closest candidate                        72   40   84      0 of 51
combined, oracle pairing, equal weight            62   39   81      0 of 51
combined, best over every candidate pair          60   39   74      0 of 51
```

**It does not reach, and the reason is not the combining.** A candidate unrelated to the
transmission draws its distance from Binomial(174, 0.5) - mean 87, standard deviation 6.6. A median
closest candidate of 69 is under three standard deviations from pure chance: **at -24 dB the sync
search is barely finding the signal**, and the summed variance says the same thing at a median of
**48.9**, which is exactly what two independent vectors of no information sum to. **This is a
synchronisation limit and it belongs to step 4.**

**The pairing, measured before it was designed.** Between the two hearings' closest candidates at
-21 dB: frequency gap median **0.00 Hz**, within 3.125 Hz on **49 of 51**; time gap median
**0.000 s**, within 0.16 s on **49 of 51**. The two that miss are unit 246's two trials with no
candidate near the signal at all. **The closest candidate is not the highest-scoring one in 10 of 51
slot A trials and 9 of 51 slot B trials**, which is why the production rule iterates over every
candidate of the later slot and chooses only the *partner* by score.

### 2. The repeats ladder, whole

`tests/Ft8Sharp.Tests/Dsp/Ft8Unit247ScoreboardTests.cs`. Column one is the port on the first slot
alone; column two the sibling with ordered statistics decoding on, same slot; **column three the
sibling with OSD off and combining on**, all slots in order.

**-21 dB, 306 trials, both slots on the same bin and the same sample:**

```
decoder      requested  delivered  trials  DECODED  MISSED  WRONG    rate   lo 95   hi 95    ms/tr
single slot      -21.0    -21.001     306       13     293      0     4.2     2.5     7.1     64.4
single + OSD     -21.0    -21.001     306       33     273      0    10.8     7.8    14.8     72.6
combined x2      -21.0    -21.000     306      217      89      0    70.9    65.6    75.7    129.0
```

**-21 dB, 306 trials, the later slot 2.00 Hz and 480 samples away** - a third of an FT8 tone and a
quarter of a symbol period, off the block grid and off the sub-block grid:

```
decoder      requested  delivered  trials  DECODED  MISSED  WRONG    rate   lo 95   hi 95    ms/tr
single slot      -21.0    -21.001     306       13     293      0     4.2     2.5     7.1     64.2
single + OSD     -21.0    -21.001     306       33     273      0    10.8     7.8    14.8     72.7
combined x2      -21.0    -21.000     306       68     238      0    22.2    17.9    27.2    128.9
```

**-24 dB, 306 trials, both placements** - the jittered run is the same three zeros:

```
decoder      requested  delivered  trials  DECODED  MISSED  WRONG    rate   lo 95   hi 95    ms/tr
single slot      -24.0    -24.001     306        0     306      0     0.0     0.0     1.2     63.3
single + OSD     -24.0    -24.001     306        0     306      0     0.0     0.0     1.2     70.9
combined x2      -24.0    -24.000     306        0     306      0     0.0     0.0     1.2    126.6
```

**The trials only the combination decoded, and the converse:**

```
configuration              no single slot alone, combination DID   some slot alone   lost by combining
-21 dB, same placement                        200 of 306                17 of 306          0 of 306
-21 dB, jittered                               55 of 306                13 of 306          0 of 306
-24 dB, same placement                          0 of 306                 0 of 306          0 of 306
-24 dB, jittered                                0 of 306                 0 of 306          0 of 306
```

**Lost by combining is zero everywhere, as the superset property requires.**

**Zero wrong decodes on all twelve rows of the four configurations.** Every combined decode was
checked against the message the ladder transmitted: **211 of 211 at -21 dB same placement, 62 of 62
at -21 dB jittered, and none to check at -24 dB.** No combined decode anywhere was a message that
was not sent.

**The step 2 regression check**, re-measured underneath because task 4 added a hearing-capture path
to `Ft8DeepSlotDecoder` and a claim that it changes no decision is worth what it is measured at:

```
decoder      requested  delivered  trials  DECODED  MISSED  WRONG    rate   lo 95   hi 95    ms/tr
Ft8Sharp         -21.0    -21.001     306       13     293      0     4.2     2.5     7.1     64.5
Deep OSD on      -21.0    -21.001     306       33     273      0    10.8     7.8    14.8     72.8
```

**Exactly what unit 246 left. Step 2's number did not move underneath step 6's.**

### 3. The submission arithmetic and the time budget

**The rule.** For each candidate of the current slot, look through each remembered slot for
candidates within **6.25 Hz** and **0.32 s**, take the **1** highest-scoring, and submit that one
combination to `Ft8CodewordDecoder.Decode`. **The port's parity gate and CRC-14 gate are the only
acceptance. There is no checksum anywhere in `Ft8Sharp.Deep`.**

**The budget, before the run**, at one in 16 384 per submission:

```
case                                         cand/slot  subs/slot pair  over 306 trials  expected wrong
pair every candidate with every candidate          140          19 600        5 997 600           366.1
this rule, worst case at the candidate limit       140             140           42 840             2.61
this rule, at the ladder's observed count           13              13            3 978             0.24
```

**The worst-case row is above one and is said to be**: it is a slot returning the port's full
candidate limit on all 306 trials, and the ladder's slots carry one transmission and return about
13. **What is quoted is what was counted, not the bound.**

**What was actually spent**, from `Ft8DeepCombineCounts`:

```
configuration              pairs offered   submitted   accepted by the PORT   naive expected wrong
-21 dB, same placement            48 344         357                    216                  0.022
-21 dB, jittered                  50 677         516                     88                  0.031
-24 dB, same placement            42 840          76                      0                  0.005
-24 dB, jittered                  41 876          58                      0                  0.004
```

**Zero messages nobody sent were returned in any configuration.** At -24 dB the port accepted none
of the 134 combinations put to it, which is the gates working rather than the stage failing.

**And the gate refusing a wrong pairing, in the port's own words.** 56 deliberately wrong pairings
of eight messages, every input decodable on its own, codewords 52 to 94 of 174 apart:
**51 `ParityNeverSatisfied`, 0 `ChecksumFailed`, 0 `MessageNotReadable`, 5 decoded - and every one
of those five returned one of its own two transmissions. 0 returned a message nobody sent.** GATE 1
at `Ft8CodewordDecoder.cs:80`: *"Until this holds, the bits are the decoder's closest approach and
not a codeword, so there is nothing here to compute a checksum over."* GATE 2 at `:96`: *"belief
propagation can converge on a perfectly valid codeword that is not the one that was sent, and every
parity check in the code will agree with it. Only the checksum knows."*

**The worst-case time per slot, worst observed and not the mean:**

```
configuration              worst slot ms   its candidates   its combinations   margin against 15 s
-21 dB, same placement             102.5               11                  0                 146x
-21 dB, jittered                   101.8               11                  3                 147x
-24 dB, same placement             100.2               12                  0                 150x
-24 dB, jittered                   113.3               14                  0                 132x
```

**132-fold margin at the worst.** Three of the four carried no combinations, so what is slow is the
search and not the combining - the whole -21 dB jittered walk spent 516 submissions across 612
slots.

### 4. Both suites

**`Ft8Sharp.Tests`: 593 passed / 0 failed / 1 skipped / 8 m 12 s.** Baseline this session was 586 /
0 / 1 / 5 m 19 s; the seven extra are this unit's measurements. **`Ft8Sharp.Deep.Tests`: 51 passed /
0 failed / 0 skipped.** Baseline 35 / 0 / 0.

**No red outside the expected set.** The one expected skip,
`Ft8TableGenerationTests.RewriteTheCheckedInTablesFile`, is unchanged and no second skip appeared.
`Hamlet.App.Tests` and `Hamlet.RadioEngine.Tests` were not run, as instructed, so
`CwAdjudicationTests.ASpeedChangeInRealisticAudio` and the 51 inherited CW reds were not exercised.

### What a listener would notice

**At -21 dB, roughly one call in twenty comes through today. With the same station repeating itself
in the next slot, between one in five and seven in ten come through - and none of them is a message
nobody sent.** That is a signal about 1.2 dB weaker than Hamlet could read this morning, heard
twice.

## 4. What's blocking us

**Step 6 is not closed. It is `partial`, at four of five must-pass exits met outright and the fifth
met at -21 dB and not at -24 dB.** What the distribution says is in the way is not the combining:
at -24 dB the closest candidate the sync search returns sits a median 69 of 174 from the transmitted
codeword against a chance distance of 87, so there is nothing near the signal to combine, and the
same cause takes back about half the gain at -21 dB the moment the two slots stop sitting on the
same sample and the same bin. **The next move is step 4's baseband re-sync, not another approach at
step 6.**

**One item, and it asks for no ruling.** Logged as logged.

### HM-OPEN-075 - placement jitter takes back about half of what combining reaches

**Logged, not a question.** `owner: claude`, `severity: slows`, `blocks: nothing`.

At -21 dB over 306 trials, combining reaches **200 of 306** trials that no single slot decoded when
the two hearings sit on the same sample and the same bin, and **55 of 306** when the later one is
moved 2.00 Hz and 480 samples - a third of an FT8 tone and a quarter of a symbol period. **That is
149 trials, about half the population, lost to placement.**

**It is not either of the two things this unit's task 7 was told to open an issue for**, and that is
said explicitly in the entry: the summed distance *did* fall below the code's recovery threshold,
and the pairing tolerance the ladder needs *is* narrow. Both came out the good way. This is a third
thing the numbers earned.

**Why it matters for what comes next.** `Ft8SoftSymbols.Extract` reads a candidate at that
candidate's own block and bin, so two differently-placed hearings are read from differently aligned
windows and each carries more of its own alignment error into the sum. **Step 4 - each candidate
re-synced at baseband - is exactly the work that would recover it.** `HM-OPEN-074` already argues
step 4 has something to find for the 4 per cent of trials with no candidate near the signal at all;
**this is a second and much larger argument for the same step**, and the two together are the case
for taking step 4 next rather than step 3 or step 5.

**Nothing waits on an answer.** Step 6's gain is real, measured and quoted with the jitter on, which
is the conservative half of the pair.
