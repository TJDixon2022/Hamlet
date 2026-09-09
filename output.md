READ IN THIS ORDER

A. **The phase goal is that FT4 works exactly the way FT8 does**, and where every
   step stands: step 0 `done`; step 1 `partial`, its remainder the 4.48-against-5.04
   question with Tim; step 2 `partial`, and **this unit's task 2 discharges its
   criterion 4 remainder** — the last fifteen-second assumptions on the operator's
   own path were the ledger's `SlotsAgo` and the forbidden second copy of it in the
   view model, and both are gone; step 3 `done`; **step 4 `done` after this unit**,
   answered under **both** readings of criterion 2 — under unit 292's reading it was
   closed by naming and stays closed, and under the independent `STATE_AFTER` on unit
   293 the two surfaces it named as still not doing on FT4 what FT8 does, the ledger
   and the right-click menu, are now doing it and are asserted doing it; steps 5 and
   6 `not started` and Tim's, at his own radio.

B. **Step 4 and its four exit criteria:**
   1. pressing FT4 tunes to the band's FT4 frequency and decodes, through the same
      path the FT8 button uses — **met**, by unit 292, unchanged here
   2. the panel, conversation, ring, filters, tooltips, ledger and right-click menu
      all work unchanged, and the report names anything that did not — **met, this
      unit**. **The ledger: met.** A station heard four FT4 slots ago read
      *your move, 2 slots* and reads **gone quiet, 4 slots**; gone quiet was first
      reported at FT4 slot 8 and is reported at slot 4, which is the true count.
      **The menu: met.** **5 shapes on FT4 against 5 on FT8**, the same five in the
      same order. Of the seven surfaces, **none is still doing on FT4 something
      different from FT8**; the tooltips at `App.axaml:49` and `:50`, named by unit
      292 and parked, are the one item left and they are mode-neutral text rather
      than a behaviour that differs.
   3. one click, one transmission, through the same abort — **met**, by unit 293,
      unchanged here and re-asserted
   4. a whole exchange runs from one right click at the bench — **met**, by unit 293,
      unchanged here

C. **What this report adds, and what of it bears on A and B. Section 4 raises 3 items
   and none is in the way of a criterion in B**; all three are beside one, and two of
   the three are new measurements this unit took rather than carried findings.
   **The ladder measured** 970 messages over five rungs (−13, −10, −7, −4, −1 dB)
   at 106 trials a rung and two placements: **mean absolute error 0.58 dB, 95th
   percentile 1.41 dB**, against a **2.0 dB** threshold written into the test before
   it was run. **That opened task 4's gate**, so task 5 ran and the figure went on the
   air. **Tim can now complete a conventional FT4 exchange** — the report and
   roger-and-report shapes the exchange needs exist on an FT4 row for the first time,
   which is step 6's must-pass and the reason this unit was authored. **Task 7 was not
   dropped**; all seven tasks ran.

---

```
UNIT:       294 - complete at task 7 of 7, nothing dropped - 2026-09-09 13:09
PHASE GOAL: FT4 works exactly the way FT8 does.
UNIT GOAL:  An FT4 row carries a measured signal-to-noise ratio with its error
            stated, so the right-click menu offers the same five message shapes on
            FT4 that it offers on FT8 - and the contact ledger counts FT4 slots on
            FT4's grid rather than FT8's.
ADVANCED:   yes - step 4's criterion 2, both of the two surfaces the independent
            reading named; and step 2's criterion 4 remainder with them
NUMBER:     the estimator's mean absolute error is 0.58 dB and its 95th percentile
            1.41 dB over 970 measured messages, 106 trials at each of five rungs and
            two placements; the right-click menu offers 5 shapes on an FT4 row
            against 5 on an FT8 row, where it offered 3 against 5 at HEAD 36dc001
DRIFT:      0 consecutive units without advance  (was 0)
```

## 1. What Claude did

**Complete, at task 7 of 7. Nothing was dropped, including the named drop
candidate.** Development machine, project confirmed as Hamlet by the instruction's
four checks — `SHACK_FACTS.md` present, `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs`
present, `CoreHMI.sln` absent, `MURC.sln` absent — on branch `main`, from HEAD
`36dc001` to `52b9902` in five commits, each pushed before the next task began.

### The version disagreement the instruction asked about

**The root version read `1.12.240`, not the `1.12.239` unit 293's report claims.**
The tree was read rather than either figure assumed. It was bumped once per commit
and now reads `1.12.245`. `Ft8Sharp.Deep` went `0.8.0` to `0.9.0` on the commit that
added the estimator and not before — a new public type in a library is not a patch.
**`Ft8Sharp` did not move**: unit 289 built everything FT4 needed in the port and
this unit called it without changing a line.

### What was built

**Task 1 — the trace, driven.** Two of its three measurements had never been driven.
The ledger was run with FT4 traffic and the numbers printed beside the true ones
before anything was changed.

**Task 2 — the ledger counts on the grid it is running.** `Ft8StationRecord.SlotsAgo`
counted `Ft8Slots`' static fifteen-second boundaries whatever mode the tab was in.
`SlotGrid.Ft4` already existed and already carried the instance `BoundariesBetween`,
so the arithmetic was right and the grid reaching it was not. The grid is now a
parameter **with no default**, and `MainWindowViewModel.Quiet`'s second copy of the
arithmetic was removed rather than threaded.

**Tasks 3 and 4 — the estimator and its measured error.** `Ft4DeepSignalToNoise`,
`Ft4DeepBaseband` and `Ft4DeepMessageSymbols` in `Ft8Sharp.Deep`, report-only,
gated on a ladder run at rungs chosen from FT4's own decoding.

**Task 5 — the ratio reaches the row.** `Ft8Reception.ReadFt4` measures its messages
the way the FT8 path does, and the menu offers five shapes.

**Tasks 6 and 7 — bookkeeping, and the drop candidate.** The `PHASE_OUTCOME.md`
entry, its replay arguments, and the two sentences that named FT8 on an FT4 path.

### The decisions made for this session, reproduced in full

**One — the estimator's seam is neither of the two the instruction offered, and the
third was better than both.** The instruction named two honest alternatives: give
`Ft8DeepBaseband` a geometry and let FT8 pass its own — the smaller file and the
larger risk, because that file is on FT8's decode path — or build FT4's mixing and
correlation beside it, which is a copy, and a copy is what the port's whole argument
is against. **I took a third: `Ft4DeepBaseband` composes.** Mixing a real passband
signal to complex baseband, low-pass filtering it and decimating it is
**protocol-free** — nothing in those three steps knows how many tones there are, how
long a symbol is, or how many symbols a frame holds — so `Ft4DeepBaseband` asks
`Ft8DeepBaseband.Build` for exactly that and does FT4's own four-tone correlation
over the samples it exposes. **Not one line of a decode-path file changed**, and the
401-tap filter and the phase-block mixer are not duplicated. Where the tones sit is
**read back** from `CentreFrequencyHz` rather than predicted, so **no FT8 constant
appears anywhere in FT4's arithmetic**; if FT8's centring convention ever moved, FT4
would follow it rather than break. **The one price is named in the file rather than
papered over**: `Ft8DeepBaseband.Build` refuses a rate at which *FT8's* symbol is not
a whole number of baseband samples, and FT4 inherits that precondition though it does
not need it. At 12 kHz decimating by 24 both hold — 80 samples in FT8's symbol, 24 in
FT4's — so nothing is refused today.

**Two — the ledger's grid is a required parameter with no default.** A default of
FT8's grid would have kept every existing caller compiling and would have let the
next FT4 caller reintroduce the same silence. Instead the four FT8 control suites now
name `SlotGrid.Ft8` at every call, **which turns *FT8 unchanged* from a claim in a
report into an assertion in a test**. It cost about thirty-five call sites and they
are all controls.

**Three — the ramp symbols are measured, not excluded, and the figure is quoted both
ways.** FT4's symbols 0 and 104 carry no codeword bits but they do carry a *tone* —
upstream writes tone 0 and `Ft4SymbolEncoder` ports it — so what was transmitted is
known for them exactly as for the other 103, and **a known tone is measurable**. What
they differ in is the envelope, and that is worth **+0.011 dB** measured over 970
messages. Excluding them would have dropped the count to 103 and made `MinimumSymbols`
a fraction of a frame the estimator does not read.

**Four — the arbiter's one-root-cause reading of the ledger was checked against the
tree and is right.** Unit 293's section 4 gave *gone quiet* and *slots ago* as two
defects. The decision at `Ft8ContactState.cs:141` is `sinceHeard >= GoneQuietAfterSlots`,
**in slots**, and `sinceHeard` is `SlotsAgo`'s answer — so putting the grid into
`SlotsAgo` put it into the threshold with it and `GoneQuietAfterSlots` was never
touched. `GoneQuietAfterSeconds` is a derived figure that nothing in `src/` reads;
two test messages print it, so it follows the grid — 60 s on FT8, 30 s on FT4.

**Five — `MinimumSymbols` for FT4 is 53 of 105, by FT8's rule and not its number.**
FT8's is 40 of 79, half the frame, so that an estimate over a fragment is *no
measurement* rather than a noisier one. Half of 105 is 52.5, rounded **up** so that
*at least half the frame* is true rather than nearly true. It is a judgment, no
document rules on it, and it is stated as one.

**What was not settled.** The 4.48-against-5.04 figure, the version scheme, unit 289's
widened candidate sweep, and the four inherited reds unit 290 found — all four remain
with Tim, none was touched, and **no transmission length was put on any screen or into
any sentence as a literal**.

### One mismatch between the instruction and the tree

**The instruction says `Ft8Reception.cs:700-707`'s remark is the sentence to falsify
and that unit 291's lesson applies. It does — and there was a second one on the same
file that the instruction did not name.** `Ft8Reception.cs:382`, the class remark on
`Ft8Reader` itself, said *no signal-to-noise ratio is produced, and none is invented*.
That stopped being true for **FT8** at unit 251, three phases of work ago, and it was
still there. Both were rewritten. That is two sentences of unit 291's shape found by
walking for them, one of which was on a path this unit never otherwise touched.

## 2. What the owner should expect

### Whether you can now answer a station on FT4 and give it a signal report

**Yes, and that is the whole of what this unit was for.** Right-click an FT4 row and
the menu offers the same five things it offers on FT8: **grid, report, roger and
report, acknowledge, 73**. Before tonight it offered three, and the two missing ones
were exactly the two a conventional exchange needs — you could send a grid, an `RRR`
and a `73`, and nothing else. **A station that calls you on FT4 and sends his grid
expects a report back, and Hamlet could not compose one.**

### What the figure's error actually is, in the words you would use

The `snr` cell on an FT4 row is now a number instead of a dash, and the number in the
report the menu offers is the same number the cell shows — they cannot disagree.

**On an average signal it is right to about half a decibel.** Measured over 970
synthesized messages the mean error is **0.58 dB** and nineteen out of twenty are
inside **1.41 dB**. In practice that means a station you would call −10 reads −10, and
occasionally −11 or −9.

**On a very strong station it reads about a decibel pessimistic, and that is worth
knowing before you send one.** The error is not the same at every level: at the weak
end it is essentially nil, and at −1 dB the estimator reads **1.2 dB low**. So a
booming local station you might call −1 may come out as −2. The reason is in the
arithmetic and is not a bug in it: the noise is measured from the three tones that
were *not* sent at that instant, and FT4's smoothing puts a little of the transmitted
tone into its neighbours — a term that grows with the signal until, on a very strong
one, it is most of what those three bins hold. **It is reported rather than corrected**,
because a correction fitted to that table would turn a measurement into a fit, which
is the one thing the file forbids.

**Nothing is ever invented.** A message whose bits cannot be recovered exactly from
its own text gets no figure at all — the cell keeps its dash and the menu goes back to
three shapes with the reason said out loud. That matters more here than on a screen: a
report goes on the air, to another operator, and into his log.

### What will look wrong but is not

**The contact column will say *gone quiet* about FT4 stations sooner than it used to** —
after four FT4 slots, thirty seconds, instead of after eight. **The old behaviour was
the wrong one.** It was counting fifteen-second boundaries in seven-and-a-half-second
traffic, so it reported half the true age and held a row at *your move* for a station
that had been silent for half a minute. Four slots is two transmission opportunities
missed, which is the same argument on both modes.

**FT4 rows in a clean test slot read around +4 dB.** That is right — those slots have
no noise added at all.

### What still stands between you and step 5

**Step 5 is *Tim hears FT4* and nothing on the bench is in its way.** Pressing FT4
tunes and decodes, the slots are 7.5 seconds, the log says `MODE=MFSK` plus
`SUBMODE=FT4`, one click sends one transmission through the same abort, and now the row
carries a report you can send. What is left is the radio: a band with FT4 on it, the
dial on the right frequency, and the clock. **One thing about the receiver is worth
knowing before you sit down**, and it is in section 4: FT4's decoder is measurably less
sensitive when a signal does not land on its analysis grid — about 2 to 3 dB — so weak
FT4 stations will be missed more often than the ladder's headline figures suggest. That
is a decoder property, it belongs to the widened candidate sweep already with you, and
it is not something this unit was licensed to change.

## 3. What you should see

### 1. The trace — what the operator was told about an FT4 station at HEAD `36dc001`

**This leads because the ledger had never been driven and nothing later can recover the
starting number.** Unit 293 read the arithmetic and named it; nobody had run a slot
through it.

**The ledger, driven with FT4 traffic.** A station heard in FT4 slot 0, read at each
slot after it:

| true FT4 | seconds | `SlotsSinceHeard` | the row said | what it should have said |
|---|---|---|---|---|
| 1 | 7.5 | 0 | your move, 0 slots | your move, 1 slots |
| 2 | 15.0 | 1 | your move, 1 slot | your move, 2 slots |
| 3 | 22.5 | 1 | your move, 1 slot | your move, 3 slots |
| **4** | **30.0** | **2** | **your move, 2 slots** | **gone quiet, 4 slots** |
| 8 | 60.0 | 4 | gone quiet, 4 slots | gone quiet, 8 slots |
| 12 | 90.0 | 6 | gone quiet, 6 slots | gone quiet, 12 slots |

**Gone quiet was first reported at FT4 slot 8, sixty seconds. It is true from slot 4,
thirty seconds.** Exactly half, at every row, which is the one-root-cause reading
confirmed by driving it.

**The menu and the cell.** **3 shapes on FT4 against 5 on FT8** — confirming unit 293's
report — and the FT4 three were *grid, acknowledge, 73*. **An FT4 row's `snr` cell held
`—`**, `DigitalDecodeRow.NoMeasurement` at `:377`. The shape that was missing is the one
step 6 needs: the station had sent his grid, so the *expected* reply was `report`, and it
was the one shape the FT4 menu could not offer at all.

**Every place the ratio was lost, from decode to menu item**, each marked constant, type
or default, in the three categories units 292 and 293 used:

| # | Where | Kind | What it was |
|---|---|---|---|
| 1 | `Ft8Reception.cs:707` | **default** | `SignalToNoise = Summarise(Array.Empty<double?>())` — every FT4 slot's spread was `Ft8SlotSnrs.None` |
| 2 | `Ft8Reception.cs:726-734` | **default** | the `Ft8Decode` built in `ReadFt4` set no `SignalToNoiseDb`, so every FT4 decode carried `null` |
| 3 | `Ft8Reception.Measure:803` | **constant** | `stackalloc byte[Ft8SymbolEncoder.SymbolCount]` — 79, FT8's frame |
| 4 | `Ft8Reception.Measure:814` | **type** | `Ft8DeepSignalToNoise.Estimate`, whose every constant is FT8's |
| 5 | `Ft8Reception.Measure:818` | **constant** | `Ft8DeepSlotDecoder.CandidateTimeBiasSeconds`, minus one **FT8** symbol |
| 6 | `Ft8DeepSignalToNoise.cs:111` | **constant** | `BinBandwidthHz => Ft8DeepBaseband.ToneSpacingHz`, 6.25 Hz |
| 7 | `Ft8DeepSignalToNoise.cs:86` | **constant** | `MinimumSymbols = 40`, half of 79 |
| 8 | `Ft8DeepSignalToNoise.cs:235` | **constant** | the grid sized `SymbolCount * ToneCount`, 79 × 8 |
| 9 | `Ft8DeepBaseband.cs:65-68` | **constant** | the tone bank is `Ft8SymbolEncoder.ToneCount` long |
| 10 | `Ft8DeepBaseband.cs:110` | **constant** | `ToneSpacingHz` from `Ft8WaterfallGeometry.SymbolPeriodSeconds` |
| 11 | `Ft8DeepBaseband.cs:151` | **constant** | the centre is `base + 3.5 * ToneSpacingHz` — 3.5 because there are eight tones |
| 12 | `Ft8DeepBaseband.cs:294-297,326` | **constant** | `TonePowerGrid` checks 79 × 8 and steps by 0.160 s |
| 13 | `Ft8DeepMessageSymbols.cs:94` | **constant** | `Ft8SymbolEncoder.Encode` — the one line that is FT8's modulation |
| 14 | `DigitalDecodeRow.cs:377,394` | **default** | `NoMeasurement = "—"`, returned by `FormatSnr` for null |
| 15 | `MainWindowViewModel.cs:10502` | **default** | `MeasuredReport` returns null where the cell will not parse |
| 16 | `Ft8SendOptions.cs:265-268` | **default** | `TextFor` returns **null** for `Report` and `RogerAndReport` where `reportDecibels` is null — **confirmed at the lines the instruction gave** |
| 17 | `Ft8SendOptions.cs:278-279` | **constant** | their labels, `report` and `roger and report` — **confirmed** |

**Every sentence in the tree asserting Hamlet cannot measure a ratio on FT4**, and
whether it was on screen:

| Where | On screen? | What it said |
|---|---|---|
| `Ft8Reception.cs:640-647` | no — a code remark | *there is no FT4 equivalent in this tree, so every FT4 row carries null* |
| `Ft8Reception.cs:703` | no — a code comment | *no FT4 symbol layout to measure a ratio against* |
| `Ft8Reception.cs:382-387` | no — a code remark | *no signal-to-noise ratio is produced, and none is invented* — **and this one was stale for FT8 too, since unit 251.** Not named by the instruction; found by walking |
| `OneClickOneFt4TransmissionTests.cs:341-350` | no — a test remark | *every FT4 row's ratio is null … there is no FT4 equivalent in this tree* |
| `PressingFt4TunesAndDecodesFt4Tests.cs:153-156` | no — a test comment | *the estimator packs the text back to FT8's 79 symbols … so every row is not observed* |

**None was on screen.** Unit 291 found seven of this shape with two on screen; these
five are all in remarks and comments. **The operator-facing statement was not a sentence
at all — it was the dash in the `snr` cell and the two absent menu items**, and both were
correct at the time, because nothing had measured a ratio. All five remarks and comments
were rewritten.

**What `docs/unit251-snr-trace.md` §4 actually computes, in my own words.** Over one
symbol, FT8's eight tone exponentials are orthogonal, because the tone spacing is the
reciprocal of the symbol period — so correlating a symbol's window against each tone in
turn is the matched filter for the alphabet, and each output is one bin. A real tone of
power `S` mixed to baseband has squared magnitude `S/2`, and correlated coherently over
`L` samples gives `(S/2)L²`; noise of one-sided density `P` becomes a complex process of
density `P/2`, and one bin collects `L` times its per-sample variance, `L(P/2)R`.
Dividing, the halves cancel and so do `L`, `R` and the decimation, leaving `S·T/P` —
**a signal-to-noise ratio in a noise bandwidth of exactly `1/T`, the tone spacing**,
independent of how the baseband was built. Carrying that to the 2500 Hz convention is one
logarithm of two published bandwidths. **The whole derivation is written in `T` and never
in FT8's number**, which is why putting FT4's `T = 0.048 s` through it is a derivation and
not a transliteration: the bin is 20.8333 Hz and the offset `10 log10(120) = 20.79 dB`.

### 2. The ladder

**The threshold, stated before the numbers.** **2.0 dB mean absolute error**, which is
`PHASE_PLAN.md`'s own figure for whether a ratio may reach a surface and the one unit
251's verdict on FT8 was taken against. It is a `const` in the test file, written before
the test was first run. **The FT8 regression guard's 1 dB was deliberately not used**:
that is a guard set at four times the headroom of a figure already measured, and there
was no FT4 figure to set headroom against — inventing one would have been a prediction
dressed as a gate.

**The rungs are FT4's own.** `Ft4SensitivityLadderTests` was run for this unit and
**settles the disputed question of whether it does anything**: it is a real `[Fact]`, it
runs in 1 m 9 s, and over its 106-message corpus FT4 decodes **106 of 106 at 0, −5, −10
and −13 dB, 49 of 106 at −15, and 0 of 106 at −17, −19 and −21**, with 0 wrong decodes
across 848 trials and 0 messages out of 20 slots of noise alone. The estimator's rungs are
therefore **−13, −10, −7, −4, −1** — the five at which the decode rate is one, so the
agreement figure is taken on no selected sample — and twelve decibels of span, so a bias
would show as an offset and a scale error as a tilt. **Nothing is claimed at −15 dB or
below.**

```
placement     rung   trials  decoded  measured   MAE ref   p95 ref   bias ref   MAE raw   p95 raw   no ramps
on grid        -13      106      105       105      0.30      0.72      -0.07      0.31      0.80       0.31
on grid        -10      106      106       106      0.32      0.70      -0.19      0.33      0.70       0.32
on grid         -7      106      106       106      0.37      0.87      -0.32      0.39      0.87       0.36
on grid         -4      106      106       106      0.62      1.07      -0.62      0.63      1.11       0.61
on grid         -1      106      106       106      1.18      1.67      -1.18      1.21      1.68       1.16
cell centre    -13      106       17        17      0.26      0.85       0.18      3.72      4.58       0.25
cell centre    -10      106      106       106      0.31      0.75      -0.20      5.49      6.43       0.31
cell centre     -7      106      106       106      0.38      0.76      -0.32      7.50      8.45       0.37
cell centre     -4      106      106       106      0.60      1.10      -0.60      9.77     10.62       0.58
cell centre     -1      106      106       106      1.22      1.77      -1.22     12.49     13.23       1.20

BOTH           all     1060      970       970      0.58      1.41      -0.51      4.23     12.55       0.57
```

**1060 trials, 970 decoded, 970 measured, 0 re-pack refusals, 0 unmeasured.**
**Refined: 0.58 dB mean absolute error, 1.41 dB at the 95th percentile. Unrefined: 4.23 dB
and 12.55 dB** — so the alignment search is most of the work, and vastly more so than on
FT8, because FT4's analysis cell is four times larger in frequency.

**The ramp-symbol decision, with the figure both ways.** All 105 symbols are measured, the
two ramps included, for the reason in section 1. **Reading only the 103 non-ramp symbols
moves the figure by +0.011 dB** over 970 messages. The `no ramps` column above is the mean
absolute error the other way at every rung; it differs from the refined column by at most
0.02 dB anywhere.

**The candidate time bias, and how it was found.** Not assumed from FT8's, and not derived
— **measured, the one way that needs no estimator in the loop**: the true start of a
synthesized signal is known exactly, because this test writes the lead itself, so the bias
is a subtraction of the decoder's reported `TimeSeconds` from it. **529 on-grid trials
produced one distinct value, −0.048000 s**, which is minus one FT4 symbol period. FT8's is
−0.160000 s and does not transfer; a caller using it here would measure a window two FT4
symbols late.

**The purity check.** The same FT4 slot decoded again after the estimate has been taken
returns the identical `Ft8SlotResult` — all five stage counts, the message count, and every
message's text, candidate score, frequency and `dt` — asserted on 10 trials at every rung
and both placements. **The samples come back unchanged**, checked by a running hash over
the whole slot before and after.

**The spread against FT8's, as predicted and as measured.** FT4's noise is estimated from
**three** wrong bins where FT8 has seven — 105 × 3 = 315 independent looks against 553 —
so the noise estimate's own relative standard error is `sqrt(553/315) = 1.32` times FT8's.
Measured, FT4's 0.58 dB against FT8's 0.26 dB is a factor of **2.2**, so the extra
bins are not the whole story: the rest is the strong-rung bias in section 4, which is
absent from FT8's ladder because FT8's rungs stop at −6 dB.

### 3. Criterion 2's two surfaces

**The menu.** **5 shapes on an FT4 row against 5 on an FT8 row** — both counts asserted
even though they are now equal, so a change removing a shape from one path cannot pass on
the other. The five, in exchange order and identical on both: **grid, report, roger and
report, acknowledge, 73**. End to end through `Ft8Reader.Read` with `DigitalMode.Ft4`, one
synthesized slot:

```
delivered   -8.01 dB in 2500 Hz
the row     -8.35 dB, so the snr cell reads -8 where it read the dash
the menu    W9GAP KC3QIS -08
```

**The menu's number is the row's number**, asserted, so the operator and the band cannot be
reading different figures.

**A station with no measurement still offers three and says why.** `grid, acknowledge, 73`,
with `no signal report has been measured for this station, so the messages that carry one
are not offered` in `Ft8SendMenu.Absent`. **Null means not observed and the cell keeps its
dash**, asserted for both `null` and `NaN`. This case is asserted because without it a
later unit could make the report unconditional by substituting a floor and every other test
would still pass.

**The ledger, on FT4's grid, with the true figures beside them.**

| | at HEAD `36dc001` | now | true |
|---|---|---|---|
| heard 4 FT4 slots (30.0 s) ago | *your move, 2 slots* | **gone quiet, 4 slots** | gone quiet, 4 slots |
| gone quiet first reported at | FT4 slot 8, 60.0 s | **FT4 slot 4, 30.0 s** | slot 4, 30.0 s |
| `GoneQuietAfterSeconds` | 60 s on both modes | **60 s FT8, 30 s FT4** | derived from the grid |

The count is asserted at **every slot from 1 to 12**, which is where a partial fix would
break — *slots ago* following the mode while *gone quiet* did not would fail in the middle
of that table rather than at one row.

**`Ft8ArmedSend.Arm` has exactly one caller in `src/`** — `MainWindowViewModel.cs:10627` —
**and this unit added none.** Two new menu entries are two new ways to click and not a
second way to transmit: `Ft8SendOptions` is a static composer with no field, no event and
no member whose name touches `Arm`, `Send`, `Transmit`, `Key`, `Ptt` or `Start`, asserted
by reflection over every member it declares.

### 4. What FT8 did before and after

**`Ft8Unit251SnrAgreementTests` re-run gives the identical table**: **0.26 dB mean absolute
error, 0.62 dB at the 95th percentile, 510 of 510 decoded and 510 measured**, which is
exactly the figure `Ft8DeepSignalToNoise.cs:341` records unit 251 measuring. **No FT8
decode moved.**

**The FT8 ledger row is unchanged and it is asserted rather than described.** The four
control suites now name `SlotGrid.Ft8` at every call and **every figure they assert is the
one they asserted at HEAD `36dc001`, to the count** — `N5TT` 5 slots, `VK2PQ` 11, `G4XYZ` 3
heard and 2 sent, `W1ABC` 5 and 4, the never-sent null, and the same-slot zero. The same
walk on FT8 traffic trips gone quiet at slot 4 and reads *your move, 3 slots* at 45 s,
exactly as before. The four states resolve identically across the whole band scene and the
whole four-state walk.

**The FT8 menu, ratio and rows are unchanged**: 5 shapes, the same order, the same
`-14` report text, and the FT8 read through `Ft8Reader` gives what it gave before this unit.

**Two inherited tests went red for the right reason and the tests were fixed, not the
behaviour.** `PressingFt4TunesAndDecodesFt4Tests` asserted every FT4 decode carries a null
ratio, bundled under the same reason as `PortComparison` being null — **they were never one
reason**, and only the second still holds. `OneClickOneFt4TransmissionTests` asserted the
FT4 menu is short two shapes; it now asserts **five with a measured ratio and three
without one**, which is the case that moved and the case that must not. `TheRowSaysOneOfFourThingsTests:236`
asserted `Assert.Equal(60.0, GoneQuietAfterSeconds)` and now asserts 60.0 on FT8 **and 30.0
on FT4** beside it. **In no case did fixing a test change what the screen asserts to the
operator** except where that assertion was the defect.

### The two sentences of task 7, quoted before and after

**The connect-time rate guard.** Before, whatever mode the tab was running:

> …speaks 8001 samples per second, and **an FT8 transmission** cannot be built at that
> rate: …

After, with FT4 chosen:

> …speaks 8001 samples per second, and **an FT4 transmission** cannot be built at that
> rate: …

The mode's name is read off `SlotGrid`, the same one value the grid, the cutter and the
decoder derive from, so there is no second spelling to drift. Both sentences are now pinned
by tests. **No transmission length appears in either.**

**The fit refusal's tail clause.** Before:

> …with no silence on either end **- a padded slot is what a decoder reads, not what goes
> on the air.**

After:

> …with no silence on either end. **The commonest cause is a padded slot - what a decoder
> reads rather than what goes on the air - but what is measured here is the length and not
> the reason.**

Every number in it was right and stays right, and all of them still come off `send.Grid`.
The branch fires on any audio longer than the slot has room for; the padded slot is the
commonest thing that produces one and is not the only one, and **the line has measured two
lengths rather than made a diagnosis**.

### Everything that was run, filtered by exact name and foregrounded

| Suite | Result | Wall clock |
|---|---|---|
| `Unit294WhatTheOperatorIsToldAboutAnFt4StationTests` (task 1, first form) | 2 passed | 1.12 s |
| the 4 contact controls + unit 294, `Hamlet.RadioEngine.Tests` | **32 passed** | 3.46 s |
| `TheContactStandsAfterHisLastTransmissionTests`, `TheWholeContactWalksThroughTheApplicationTests` | **7 passed** | 1.99 min |
| `Ft4SensitivityLadderTests` | **1 passed** | 1 m 9 s |
| `Ft4Unit294SnrAgreementTests` | **1 passed** | 1 m 34 s |
| unit 294 + contact controls after task 5 | **31 passed** | 2.86 s |
| `Ft8Unit251SnrAgreementTests` (the FT8 control) | **1 passed** | 2 m 27 s |
| the 3 FT4 chain suites, `Hamlet.App.Tests` | **16 passed** | 16.97 s |
| `TheSendPathComposesAtTheEndpointsRateTests` | **5 passed** | 2.49 s |
| the 2 transmit-refusal suites, `Hamlet.RadioEngine.Tests` | **15 passed** | 1.48 s |

**No unfiltered `dotnet test` was run on any project.** Every `dotnet build` was
foregrounded with a stated timeout and none was counted. **Nothing was backgrounded and
polled for.**

### Bookkeeping

**`PHASE_OUTCOME.md`**: unit 294's entry appended with the file-editing tools in the format
`outcome-entry.py` produces, saying so on its own face, with the arguments at
`tools/arbiter/unit294-append.bat` so it can be replayed rather than reconstructed. **No
`.bat` under `tools/arbiter/` was attempted** — five units settled it and a sixth
measurement is worth nothing. **Units 289's to 293's entries were not touched**, both
`STATE_AFTER` verdicts each carries included.

**`PHASE_STATUS.md`**: `WORK_INSTRUCTION:` set and nothing else. **`CURRENT_STEP:` still
reads `1` while the phase is on step 4** — reported for the fifth time, not repaired, and
it is the launcher's.

**`RULES_AT`**: re-measured and **there is nothing to repair**. `CPS-DEC-` appears **zero**
times in `CLAUDE.md`, `DECISIONS.md` and `PROJECT_STATUS.md`; `HM-DEC-160` appears **once
in each**; the three agree. Sixth unit to report it. The reload is the launcher's file.

**The untracked leftovers**: one attempt, refused. `rm` is blocked outside the session's
allowed directories and `git rm --cached` does not apply to files that were never in the
index. `.unit290-commit.txt`, `tools/census15.sh` and
`tests/Ft8Sharp.Tests/Unit289SourceProbe.cs` are left, **uncommitted**. **A fourth joins
them and it is mine**: `tools/unit294-thread-grid.py`, written for a shell call that turned
out to need approval, never run, dead, referenced by nothing, and not removable for the
same reason. **The tree a fresh clone builds still differs from the tree that was tested**,
seven units old now.

## 4. What's blocking us

**Nothing is blocking. Three items are raised and all three are beside a criterion in B
rather than in the way of one.** Two are new measurements from this unit and one is
carried.

### 1. FT4's decoder loses 2 to 3 dB when a signal does not land on the analysis grid

**Ruling wanted:** whether unit 289's widened FT4 candidate sweep — the −10 to 51 blocks
already with you — should be reconsidered in the light of a measured off-grid sensitivity
loss, or whether this is a separate finding needing its own unit.

**Reasoning:** the estimator's ladder ran every rung at two placements, and the decode
counts are the finding rather than the agreement figures. **At −13 dB FT4 decoded 105 of
106 on grid and 17 of 106 at the cell centre** — half a waterfall step in time and half a
step in frequency, which is where a real station lands, because nothing on 14.080 arranges
itself on Hamlet's grid. At −10 dB and above both placements decode 106 of 106, so the loss
is at the sensitivity edge and is somewhere around 2 to 3 dB of it. **FT8 shows no such
gap** at any of its five rungs, 510 of 510 at both placements — but FT8 has fine sync and
ordered statistics behind it and FT4 has neither, so the two are not comparable and this is
not evidence of a defect.

**What was rejected and why:** chasing it. The candidate sweep is explicitly with you and
the instruction says to call it and not retune it. **It is reported as a miss rather than
anything being widened.** It bears on step 5 — Tim will miss weak FT4 stations more often
than the published threshold suggests — and on nothing in step 4.

### 2. The estimator reads about 1.2 dB low on a very strong station

**Ruling wanted:** whether a systematic error that grows with signal strength, from −0.03 dB
at −13 dB to −1.20 dB at −1 dB, is acceptable on a number that goes on the air, or whether a
later unit should model the leakage term rather than leave it.

**Reasoning:** the noise is estimated from the three tones that were *not* sent at that
instant, and GFSK smoothing puts a signal-proportional share of the transmitted tone into
its neighbours. At a weak rung that share is lost in the real noise; at −1 dB it is most of
what those three bins hold, so the ratio is pulled down. **The whole ladder still clears the
2 dB gate at 0.58 dB**, and the effect is confined to signals stronger than about −5 dB,
where a decibel of pessimism costs an operator nothing he would notice.

**What was rejected and why:** correcting it in this unit. Any correction I could write
tonight would be fitted to that ten-row table, and `Ft8DeepSignalToNoise.cs:51-58` says in
terms that a later unit which fits a constant to the ladder has turned a measurement into a
fit. **It is stated in the estimator's own remarks, in `Ft8Reception`'s, and in section 2 in
the words an operator uses**, which is the honest disposal of a known bias. A unit that
modelled the leakage from the published pulse shape rather than from the table would be a
different and legitimate thing.

### 3. The working tree still is not what a fresh clone builds — now four files

**Ruling wanted:** whether somebody with a shell that can delete should remove
`.unit290-commit.txt`, `tools/census15.sh`, `tests/Ft8Sharp.Tests/Unit289SourceProbe.cs` and
`tools/unit294-thread-grid.py`, or whether one of them should be tracked instead.

**Reasoning:** the third is a `.cs` file in a test project, so **the tree that is tested is
not the tree a fresh clone builds**, and that has been true for seven units. Seven sessions
have now failed to remove them; this one made one attempt, as instructed, and the harness
refused. The fourth is mine and is dead.

**What was rejected and why:** committing them to make the warning go away, which the
instruction forbids and which would make the difference permanent instead of visible; and
spending a second call on the shell, which the instruction caps at one attempt.

### Carried, unchanged, and not re-argued

The four questions with you — **the 4.48-against-5.04 transmission figure**, **the version
scheme (HM-DEC-150 against what `Directory.Build.props` has been doing)**, **the widened
candidate sweep**, and **the four inherited reds unit 290 found** — are all untouched. So
are the two field-guide frequencies needing a citation, the frequency table's missing 30 m
and 17 m FT4 rows, and the two tooltips at `App.axaml:49` and `:50`. **None of them is in
the way of step 4, and none was settled from memory.**
