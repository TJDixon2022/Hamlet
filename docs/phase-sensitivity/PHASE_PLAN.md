# PHASE_PLAN.md

**Governed by `PHASE_CONTROL.md`. Approved by Tim, 2026-09-04.**

---

## The phase

**Hamlet reads FT8 as well as the best decoder there is, and then reads it
further.**

## The description

Hamlet decodes FT8 off the air. On 2026-09-04 at 21:41 UTC it read fourteen
messages from one slot on 14.074, and at 21:58 a capture recorded arrival at
100 per cent, 80 candidates, 16 codewords, 16 checksums, 16 messages and a top
Costas match of 29 of 21 possible sync symbols. The audio path is finished and is
not this phase's subject.

**What is not finished is how deep it hears.** `HM-OPEN-067` records the number:
the 50 per cent crossing sits near **-19.5 dB** against a published threshold of
**-21**, measured on 306 trials a rung with the SNR axis checked against a second
instrument sharing no line of code with the first, agreeing to 0.0098 dB mean.
**The shortfall is about 1.5 dB and it belongs to the receiver.**

Unit 222 then took that 1.5 dB apart and could not find it in any one stage.
Oracle alignment, unquantised magnitudes, physics-derived ratios and four times
the iteration bound each bought a result inside the as-is 95 per cent interval.
Its finding, which is this phase's starting point: **the information is not in
the ratios.** At -21 dB the hard decisions carry about **31 bit errors** against
a code whose recovery reaches zero at **17**. The demodulator is not the problem.
**Belief propagation gives up while the answer is still reachable.**

Everything attempted in the previous phase was a fidelity audit - four
instruments spent proving this port matches `ft8_lib`. It does. **`ft8_lib` is
what is 1.5 dB short**, and nothing in this tree has ever tried to do better than
it.

**This phase is the first deliberate divergence in the project**, and it is why
step 0 exists: nothing may be claimed against a decoder nobody has measured
against.

**A session reading this cold should understand:** the port is correct and stays
correct; improvements go in a sibling and never in the port; every step closes on
a number measured against an external instrument; and the last step aims past the
best decoder in the world rather than at it.

---

## The steps

```
STEP: 0 | there is a scoreboard, and it reads WSJT-X
STEP: 1 | Ft8Sharp.Deep exists and changes nothing
STEP: 2 | ordered statistics decoding closes the code gap
STEP: 3 | strong signals are subtracted and the slot is read again
STEP: 4 | each candidate is re-synced at baseband
STEP: 5 | Hamlet's own SNR is measured and shown
STEP: 6 | repeated transmissions are combined across slots
```

**A step is an outcome, not a work instruction.** The arbiter authors as many
units against a step as it takes. Sizing is the arbiter's judgment per unit and
**small units are the default**; when a unit is too big the answer is a second
unit rather than a sacrifice.

**Steps 2, 3 and 4 are independent of one another and are ordered by decibels per
unit of effort, not by dependency.** Any of them may be re-ordered by the arbiter
on measured evidence from step 0's scoreboard, and the reason recorded. Step 6
depends on nothing but is last because it is the only step whose result nobody
has ever published.

### The steps are a hypothesis, not a contract

**The phase is the goal. The steps are today's best guess at how to reach it, and
they were written before a line of it was tried.** No plan survives contact. The
arbiter is expected to change this plan, and changing it is the arbiter doing its
job rather than exceeding its authority.

**The arbiter may do all of this without asking:**

- **Reorder steps.** Steps 2, 3, 4 and 5 depend only on step 1. If the scoreboard
  says subtraction is worth more than OSD tonight, take it first.
- **Replace a step with a better one.** If the evidence points at an approach not
  named here, take it. The steps name the approaches known on 2026-09-04, not the
  only ones permitted.
- **Retire a step.** If measurement shows a step cannot pay for itself, close it
  *unachievable* with the number that says so and move on. That is a result.
- **Add a step.** If something is discovered that the phase needs, add it.
- **Move a target that was measured wrong.** Every number here is quoted from a
  prior measurement. If a target is found to be wrong, the target changes and the
  finding is recorded - the plan does not get to be more right than the tree.
- **Split, merge or re-scope any step's criteria**, provided the record says what
  changed and why.

**Every change goes in `PHASE_OUTCOME.md`** with the evidence behind it. The
record is the constraint, not permission.

### Stopping is a failure, and the plan owes an alternative to each one

**A halt costs a night.** The loop exists so work continues while Tim sleeps, and
a stop condition that fires at 2 a.m. for something the arbiter could have decided
is the most expensive outcome in this project. **Treat any stop as a last resort
and prefer a recorded decision plus continued work.**

**A must-pass criterion no unit can satisfy is a plan defect, not a blocked
step.** The previous phase lost step 6 to exactly that - criterion 2 needed a
sensitivity the units could not reach, and the step sat blocked for a week. In
this plan, **every must-pass criterion is reachable by unit effort alone.**
Anything that needs Tim, a radio, or a program that is not on the development
machine is **deferred**, never must-pass.

**Named alternatives to stopping, ruled in advance:**

| If | Do not stop. Instead |
| --- | --- |
| the baseline does not reproduce | record both figures with their delivered SNR, adopt the new one as the baseline with its provenance, move every target by the same offset, and continue. **The relative gain is what this phase is measuring.** |
| a real-air comparison is needed and no fixture exists | close the step on the ladder, mark the real-air criterion *deferred*, name the fixture needed in `OPEN_ISSUES.md`, and continue |
| a step's target is not met | that is evidence, not a halt. Try the next approach. A step stays open while the number is still moving, and closes *unachievable* with a figure when it is not |
| an approach turns out to be wrong | abandon it, record what it cost and what it showed, take another |
| a licence or naming question arises | **already ruled below.** Do not raise it |
| a unit finds a defect in `Ft8Sharp` | record it, work around it in the sibling, continue. Changing the port is its own unit and its own decision |
| the tree disagrees with this plan | the tree wins. Report the mismatch and continue against the tree |
| something outside the phase is discovered | log it in `OPEN_ISSUES.md` and continue. Do not chase it and do not stop for it |

**What still stops the loop, and it is a short list:** the three things below that
the arbiter may not reason past, and a genuine loop - proposing what has already
been tried and failed, twice. Nothing else here is worth a night.

### What a unit runs

Carried forward from the previous phase and unchanged. `Ft8Sharp.Tests` and
`Ft8Sharp.Deep.Tests` every unit. The channels a unit could affect, whole, run
one project at a time and never concurrently. **The full engine suite is Tim's,
by hand, uncontended, once** - it has been missing from three consecutive reports
and that remains its own outstanding item rather than a unit's job.

---

## The three things the arbiter may not reason past

Facts about the destination, not fences. Reaching the goal by crossing one of
them is not reaching the goal.

1. **Transmit.** `CLAUDE.md` §0.2 is untouched. Nothing in this phase keys the
   radio. Step 6 synthesises signals as test oracles and for subtraction, and
   they never reach a transmitter.
2. **The licensing boundary.** No route to an algorithm goes through WSJT-X's
   source or `ft4_ft8_public/`. **WSJT-X is a measuring instrument in this phase
   and never a source.** Every algorithm comes from published description -
   Fossorier and Lin 1995 for ordered statistics, and the QEX paper (Franke K9AN,
   Somerville G4WJS, Taylor K1JT, "The FT4 and FT8 Communication Protocols," QEX,
   July/August 2020) - cited in `porting-notes.md` at the point of use.
3. **What Hamlet asserts to Tim.** §12.1 and §0.0. A decode this phase produces
   that nobody sent is worse than a decode it misses, and step 2's false-decode
   criterion is where that is enforced.

---

## Rulings in force for this phase

Taken with Tim on 2026-09-04. **Not to be re-argued by any unit.**

**The seam is split, and this settles the divergence question open since
2026-08-31.** `Ft8Sharp` stays a faithful MIT port of `ft8_lib`, byte-identical
in behaviour, and **nothing in this phase changes a line of it**. The
improvements live in a sibling, `Ft8Sharp.Deep`, which consumes the port's
candidates and soft values and does more with them.

**The port is the instrument.** Its value now is that it cannot drift: every
measurement in this phase is taken against something known-identical to upstream,
so a regression in the sibling is always visible. Diverging in place would have
spent that instrument to buy convenience, in exactly the stage where the loss
lives.

**The licence follows the seam and is settled now, so no step waits on it.**
`Ft8Sharp` remains MIT and separately publishable because no divergent line ever
enters it. **`Ft8Sharp.Deep` is GPL-3.0**, matching Hamlet's own release licence,
carrying its own `LICENSE` and a `NOTICE` citing the published sources it
implements. Ruled by Tim, 2026-09-04. **No unit raises this and no step is held
by it.**

**WSJT-X may be run as a measuring instrument, on the shack machine only.** The
previous plan left this unruled and named it as the arbiter's to raise; it is
ruled now. It decodes the same WAV, its output is compared message by message,
and **its source is not read.** This is the *testing rather than derivation* the
spec already permits.

**There is no WSJT-X on the development machine and no unit may assume one.**
Tim's ruling, 2026-09-04. The only machine that can measure against it is the one
with the radio attached (`SHACK_FACTS.md` FACT-004). **A unit that cannot close
without a real-air comparison says so and stops**; it does not substitute
`decode_ft8.exe`, which is `ft8_lib` and therefore the thing being improved on.

**Tim generates the capture fixtures.** Ruled 2026-09-04. He runs one command at
the shack per batch of captures and commits the result. This keeps steps 3 and 5
closing on numbers the arbiter can check itself rather than on an eye at the
radio.

**Nothing is claimed without the scoreboard.** No unit in steps 1 to 6 may report
an improvement except as a number on step 0's instrument. A decode rate quoted
without it is not evidence.

**Unit numbering continues from the previous phase.** Hamlet's versioning is
untouched, patch per work unit, minor when this phase closes. `Ft8Sharp` does not
move unless a unit finds a genuine porting defect in it, which would be its own
finding.

---

## Step 0 - there is a scoreboard, and the arbiter can read it

**Delivers:** a harness the arbiter runs every unit, scoring a change against a
controlled SNR axis, plus a fixture format for comparing against WSJT-X on real
air.

**Entry:** none. This is the first step and everything after it is scored on it.

**Exit:**
- **The ladder runs in the loop.** Synthesized messages at known SNR, decoded by
  `Ft8Sharp` and `Ft8Sharp.Deep`, scored against the message that went in. No
  external decoder is involved and none is needed: the ground truth is what was
  transmitted. *must-pass*
- The as-is baseline is reproduced rather than inherited and committed:
  **4.2 per cent at -21 dB, 306 trials, 0 wrong.** A run that does not reproduce
  it is a finding before it is a baseline. *must-pass*
- **A wrong decode is counted separately from a missed one**, everywhere, in
  every report. A message returned that was not sent is the one failure this
  phase cannot trade against rate. *must-pass*
- **A capture fixture format**: a committed text file per real capture, naming
  the capture, its UTC and its SHA-256, listing what WSJT-X returned for it -
  message, frequency, dt and SNR per row. The harness reads it and scores
  `Ft8Sharp.Deep` against it. *must-pass*
- **A fixture whose named capture is absent, or whose hash does not match, fails
  loudly rather than passing quietly.** A stale fixture silently measures the
  wrong thing. *must-pass*
- One real fixture exists, generated by Tim at the shack, committed. *deferred* -
  the format, the reader and the generator are must-pass; the fixture itself
  arrives when Tim runs it and no step waits on it
- A command Tim runs at the shack to produce a fixture from a capture, in one
  step, with no editing. *must-pass*

**Why the split.** There is no WSJT-X on the development machine and there will
not be; the only machine that can run it is the one with the radio on it. So the
instrument the arbiter turns every unit is the **ladder**, which needs no external
decoder because it knows what it transmitted. Steps 1, 2, 4 and 6 close on it
entirely.

**WSJT-X enters as committed fixtures, not as a program.** Steps 3 and 5 are
comparisons against real air, which is shack work in any case. Tim generates a
fixture once per batch of captures; the arbiter then scores against a file. This
is the same pattern the previous phase used for reference recordings, which held
for forty units.

**Without this step, an improvement and a bug that decodes noise look
identical.**

**Depends on:** nothing.

---

## Step 1 - `Ft8Sharp.Deep` exists and changes nothing

**Delivers:** a sibling project that reproduces the port's results exactly.

**Entry:** step 0 complete.

**Exit:**
- `Ft8Sharp.Deep` compiles, has its own tests, and **a mechanical test asserts
  `Ft8Sharp` references nothing outside itself**, preserving the boundary that
  keeps the port publishable. *must-pass*
- Given the same audio, `Ft8Sharp.Deep` returns **exactly** what `Ft8Sharp`
  returns, on the reference recordings and the ladder. *must-pass*
- The scoreboard shows both columns identical. *must-pass*
- The sibling's `NOTICE` cites the published sources for anything it will
  implement, before it implements them. *must-pass*

**A step that changes no behaviour is the point.** It establishes that the seam
costs nothing, so every later difference is attributable to one named change.

**Depends on:** step 0.

---

## Step 2 - ordered statistics decoding closes the code gap

**Delivers:** an OSD stage that runs when belief propagation fails to converge.

**Entry:** step 1 complete.

**Exit:**
- **Decode rate at -21 dB reaches 40 per cent or better** on the same 306-trial
  ladder that reads 4.2 per cent today - the verdict band fixed in writing by the
  previous phase and not re-set here. *must-pass*
- **The step stays open while the number is still moving** and closes
  *unachievable*, with the figure reached and what was tried, when it is not.
  Falling short is a result this phase can carry; stopping the loop over it is
  not. *must-pass*
- **Zero wrong decodes across the whole ladder.** A message returned that was not
  sent is not traded against rate, whatever the rate (§0.0). If an approach
  produces one, **that approach is rejected and another is taken** - the step does
  not close and does not stop. *must-pass*
- Order and search weight are stated with the cost each buys, measured, not
  tuned to a target. *must-pass*
- Implementation is from Fossorier and Lin 1995 and the QEX paper, cited at the
  point of use in `porting-notes.md`. **No WSJT-X source is read.** *must-pass*
- Worst-case time per slot stays inside the 15-second budget with margin stated.
  *must-pass*
- The scoreboard shows decodes-per-slot improving on real captures. *nice-to-pass*

**This is where the 1.5 dB is.** Unit 222 measured 31 hard-decision errors
against a code that recovers to zero at 17, and proved the extra information is
not in the ratios. OSD reaches codewords BP cannot, from the same ratios, by
re-ordering bits by reliability and searching low-weight patterns among the most
reliable. **`ft8_lib` has no OSD at all.**

**If the rate does not move, the step has still done its job** and the arbiter
reasons from the scoreboard about where it went instead - the diagnosis is worth
more than the guess.

**Depends on:** step 1.

---

## Step 3 - strong signals are subtracted and the slot is read again

**Delivers:** multi-pass decoding with subtraction of decoded signals.

**Entry:** step 1 complete. Independent of step 2. A capture fixture improves this step and does not gate it.

**Exit:**
- Each decoded message is re-synthesised at its measured frequency, time and
  amplitude and subtracted from the audio; the residual is decoded again.
  *must-pass*
- **The ladder shows more decodes from the same audio than a single pass**, at a
  stated SNR with its trial count. *must-pass*
- **Decodes per slot on real captures within 10 per cent of WSJT-X's**, across at
  least twenty slots. *deferred* - needs a fixture Tim generates; the step closes
  without it and this is recorded in `OPEN_ISSUES.md` by name
- **Zero wrong decodes** introduced by any pass. Subtraction leaves residue, and
  residue that decodes as a message is this step's specific hazard. *must-pass*
- Pass count and the stopping rule are stated with what each pass buys. *must-pass*
- Time per slot inside budget with margin stated. *must-pass*

**On a busy band this is worth more than sensitivity.** A station at -5 dB
sitting on one at -18 hides it completely on the first pass. The 21:58 capture
returned 80 candidates and 7 distinct messages from one slot, which is what a
single pass through a crowded band looks like.

**Depends on:** step 1.

---

## Step 4 - each candidate is re-synced at baseband

**Delivers:** fine time and frequency synchronisation per candidate.

**Entry:** step 1 complete. Independent of steps 2 and 3.

**Exit:**
- Coarse candidates from the existing search are mixed to baseband, filtered, and
  re-synced at sub-symbol time and sub-hertz frequency before extraction.
  *must-pass*
- **The ladder's 50 per cent crossing moves down, measured**, with the figure
  quoted and its trial count. *must-pass*
- Zero wrong decodes. *must-pass*
- The gain is quoted separately from steps 2 and 3, on the scoreboard, so three
  changes do not credit each other. *must-pass*

**Worth a fraction of a decibel on its own and it makes everything above it work
better**, because OSD and subtraction both depend on the ratios being taken at
the right place.

**Depends on:** step 1.

---

## Step 5 - Hamlet's own SNR is measured and shown

**Delivers:** a real signal-to-noise ratio per decoded message, on screen.

**Entry:** step 0 complete. Independent of steps 2, 3 and 4. Fixtures improve this step and do not gate it.

**Exit:**
- SNR is computed per decoded message from the known symbol sequence: power in
  the correct bin against the seven wrong bins at the same instant, referenced to
  2500 Hz. *must-pass*
- **Agreement with the ladder's own commanded SNR within 2 dB, mean and 95th
  percentile quoted**, across at least two hundred synthesized messages. The
  ladder knows what it delivered, so this closes without WSJT-X. *must-pass*
- **Agreement with WSJT-X within 2 dB on real captures.** *deferred* - needs a
  fixture; recorded in `OPEN_ISSUES.md` by name
- **If it does not agree on the ladder, the column keeps its dash** and the step
  closes with the measurement rather than the display. A number 5 dB out is worse
  than no number. *must-pass*
- The measurement is report-only and changes no decode arithmetic. *must-pass*
- The `snr` column shows it, at the width already reserved. *must-pass*

**This is the dB axis of everything else.** It is also the number an FT8 operator
reads first, and the column has carried a dash since the panel was built because
nothing in the path measured it.

**Depends on:** step 0.

---

## Step 6 - repeated transmissions are combined across slots

**Delivers:** coherent or soft combining of a station's repeated transmissions.

**Entry:** step 1 complete. Best taken after steps 2 and 4, and **not gated on
them** - if they stall, this is the step to try instead.

**Exit:**
- A transmission repeated in a later slot at the same frequency is identified and
  its soft values combined with the earlier one before decoding. *must-pass*
- **A message is decoded that no single slot could decode alone**, demonstrated
  on synthesized signals at a stated SNR below the single-slot crossing.
  *must-pass*
- **Zero wrong decodes.** Every combined decode passes the same 14-bit CRC, which
  is what makes this safe: a wrongly combined pair fails the checksum and is
  discarded rather than shown. *must-pass*
- The gain is measured on the ladder and quoted with its trial count. *must-pass*
- **Every combined decode is verified against the ladder's own ground truth** -
  the message that went in - so a gain is never inferred from a decoder that
  might also be wrong. *must-pass*
- The scoreboard shows decodes WSJT-X did not return on a real capture.
  *deferred* - needs a fixture Tim generates; the step closes without it

**This is the step that means best in class rather than equal to it.** A CQ is
repeated every thirty seconds, identically, and nothing in WSJT-X combines
repeats. Two repeats is 3 dB of processing gain and four is 6. The theoretical
floor for this code and modulation is near -24 dB; WSJT-X reaches -21 without a
priori information. **This is the only route below it that does not require
knowing what the message says in advance.**

**It is also the step most likely to fail**, and failing it is an acceptable end
to this phase. Steps 0 to 5 deliver a decoder equal to the best there is. Step 6
is the attempt to pass it.

**Depends on:** step 1 only. **It closes entirely on synthesized signals** - the
ladder knows what it transmitted, so *a message decoded that no single slot could
decode alone* is provable without a radio, without WSJT-X and without Tim.

---

## What is not in this phase

Named so the arbiter logs rather than chases them:

- **A priori decoding.** Worth 3 to 4 dB on messages addressed to you, and it
  needs a station to be in a QSO. It belongs after transmit exists.
- **Transmitting FT8.** Its own phase, dummy load first, §0.2 and HM-DEC-098.
- **FT4.** Reuses nearly all of this and belongs after it.
- **The decoded text panel's remaining work.** Work instruction 241 is running
  and its outcome is not this phase's.
- **The CW decoder**, including the 419 dropped chunks the 21:58 capture recorded
  and the 51 inherited failing cases. Nothing here is evidence about it.
- **`Ft8Sharp.Deep`'s licence.** Tim's, owed before step 2 ships.
