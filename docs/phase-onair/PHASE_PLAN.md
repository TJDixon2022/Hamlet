# PHASE_PLAN.md

**Governed by `PHASE_CONTROL.md`. Approved by Tim, 2026-09-05.**

---

## The phase

**Everything this project has built reaches the operator's screen, and the
decoder is taken as far as it will go.**

## The description

The previous phase built a better decoder and none of it ever ran on a radio.
`Ft8Sharp.Deep` reads **33 of 306 at -21 dB** against the port's **13 of 306**,
and at the centre of a waterfall cell - where a real station lands, because
nothing on 14.074 arranges itself on an analysis grid - the port reads **0 of
306** and Deep reads **3**. All with **zero wrong decodes**. Hamlet calls the
port. **The gains are real, measured, and invisible.**

That was a defect in the previous plan, not in the work: seven steps and not one
of them wired the sibling into the application.

**This phase is ordered so the operator sees something early and often.** Step 0
puts the existing gains on screen. Step 2 turns the `snr` dash into a number.
Only then does it go back to chasing decibels.

**A session reading this cold should understand:** the port is a faithful MIT
port of `ft8_lib` and never changes; improvements live in `Ft8Sharp.Deep` under
GPL-3.0; every measurement is taken against a synthesized ladder that knows what
it transmitted; and **no step in this phase closes on the owner's eyes, a radio,
or a program that is not on the development machine.**

---

## This phase is built to run unattended, start to finish

**Nothing in it waits on Tim.** Not a ruling, not a reading, not a fixture, not a
licence. Every must-pass criterion below is reachable by a unit on the
development machine with no radio attached.

**Every criterion is of the form *measure and report*, never *achieve N*.** The
previous phase lost a step by writing a target as a gate: step 2's *40 per cent
at -21 dB* is at about 11 per cent with the work built and correct, so a step
that is genuinely finished sits open because a number written before the work
started did not land. **This plan quotes targets as waypoints and closes steps on
evidence.** A step that gets partway closes with the figure it reached and what
it tried.

**The one thing that stops this loop is a genuine repeat** - proposing what has
already been tried and failed, twice - and the three things in the section below
that the arbiter may not reason past. Nothing else here is worth a night.

### The steps are a hypothesis, not a contract

The arbiter may, without asking, and recording the evidence in
`PHASE_OUTCOME.md`:

- **reorder** steps 3, 4 and 5, which depend only on step 0
- **replace** a step with a better approach than the one named
- **retire** a step that measurement shows cannot pay for itself, closing it
  *unachievable* with the number that says so
- **add** a step the phase turns out to need
- **move a target** that is found to have been measured wrong

### Named alternatives to stopping, ruled in advance

| If | Do not stop. Instead |
|---|---|
| a target is not reached | close the step with the figure reached and what was tried. **That is a result** |
| an approach fails | abandon it, record its cost and what it showed, take another |
| a real-air comparison is wanted | close on the ladder, mark it *deferred*, name the fixture in `OPEN_ISSUES.md` |
| a defect is found in `Ft8Sharp` | record it, work around it in the sibling, continue |
| the tree disagrees with this plan | the tree wins. Report the mismatch and continue |
| a licence, naming or scope question arises | **already ruled below.** Do not raise it |
| something outside the phase is discovered | log it in `OPEN_ISSUES.md` and continue |
| the shell refuses a call | use the file-editing tools. Only building and testing need the shell |

---

## The three things the arbiter may not reason past

1. **Transmit.** `CLAUDE.md` §0.2. Nothing keys the radio. Synthesized signals
   are test oracles and never reach a transmitter.
2. **The licensing boundary.** No route to an algorithm goes through WSJT-X's
   source or `ft4_ft8_public/`. Published description only - Fossorier and Lin
   1995, and Franke, Somerville and Taylor, *The FT4 and FT8 Communication
   Protocols*, QEX, July/August 2020 - cited at the point of use.
3. **What Hamlet asserts to the operator.** §0.0 and §12.1. **A message shown
   that nobody sent is worse than a message missed**, and this phase puts a
   decoder in front of him that returns messages the port never would.

---

## Rulings in force

**Not to be re-argued by any unit.**

**`Ft8Sharp` is a faithful MIT port and nothing in this phase changes a line of
it.** Its value is that it cannot drift: every measurement is taken against
something known-identical to upstream. It stays separately publishable.

**`Ft8Sharp.Deep` is GPL-3.0**, matching Hamlet's own release licence. Settled
2026-09-04.

**Hamlet decodes through `Ft8Sharp.Deep`, not the port.** Deep is a proven
superset - whole-result identity over 69 reference recordings and 801 messages.
The port stays in the tree as the instrument.

**Both of the port's gates stay in the path.** Every message shown passed the
port's parity check and its CRC-14, whatever route recovered the codeword.

**There is no WSJT-X on the development machine and no unit may assume one.**
`decode_ft8.exe` is never substituted for it - that is `ft8_lib`, the thing being
improved on.

**A wrong decode is counted separately from a missed one, everywhere.** Every
column measured in this project reads zero wrong and no unit may be the one that
stops checking.

**Work instruction 249 is superseded by step 0** and is not to be run.

---

## What a unit runs, and what it does not

**Tests in this tree have grown faster than their value.** `Ft8Sharp.Tests` is
593 tests in **7 minutes 44 seconds**. `Hamlet.RadioEngine.Tests` is 2,157 and
**has never once completed a whole-project run** - started alone at 08:15 on
2026-09-01 and cut off at 09:16. Nobody knows which tests are expensive because
no run has ever finished.

**Step 1 fixes this and every later unit is cheaper for it.**

Until it does, and after:

- **A unit runs the gate set, every time.** That is the short, named list step 1
  builds: the tests that guard the properties this phase must not break.
- **A unit runs the channels it touched**, whole, one project at a time, never
  concurrently. Contention once turned one standing failure into five.
- **A unit does not run anything else.** Not for completeness, not to be safe.
- **A unit may not add a test without naming the breakage it would have caught.**
  A test that guards nothing that has ever broken is cost without cover.
- **The ladder is a measurement, not a test.** It is run when a step needs a
  number and is never in the gate set.
- **The full engine suite is Tim's, by hand, uncontended, once.** It is not a
  unit's job and its absence never blocks a step.

Known reds, inherited, **never chased**:
`CwAdjudicationTests.ASpeedChangeInRealisticAudio`; the 51 CW cases in
`docs/unit239-failing-set.txt`, which fail at the baseline `d541fc8` too; the
`Ft8Sharp.Deep.Tests` whole-type-list tripwire that goes red whenever types are
added.

---

## Step 0 - Hamlet decodes through Ft8Sharp.Deep

**Delivers:** the Digital tab decoding through the sibling, with fine sync and
ordered statistics on.

**Entry:** none.

**Exit:**
- `Ft8Reader` uses `Ft8DeepSlotDecoder` with both stages on. *must-pass*
- The five-count census still reaches telemetry, the capture sidecar and the
  census line, with the same meanings. If Deep reports them differently, the
  mapping is stated. **A number's meaning does not change silently.** *must-pass*
- **The port's parity and CRC-14 gates are in the path**, asserted by a test.
  *must-pass*
- One slot decodes inside the **15-second budget** with both stages on, margin
  stated. *must-pass*
- The same reference recording returns **at least** what it returned before, and
  every message returned passed the port's gates. *must-pass*
- The sidecar and `ft8_slot` record **which decoder read the slot and which
  stages were on**. Without it, every capture from now on is unattributable.
  *must-pass*
- A comparison mode, off by default, recording both counts side by side.
  *nice-to-pass*

**Nothing about the panel changes here** - no columns, no tooltips, no SNR.

**This is the step that makes the last phase worth having.** Five units of gains
have never run on a radio.

**Depends on:** nothing.

---

## Step 1 - the gate set exists, and the slow tests are named

**Delivers:** a short named list of tests every unit runs, and a duration ranking
of everything else.

**Entry:** none. Best taken immediately after step 0.

**Exit:**
- **Per-test durations for `Ft8Sharp.Tests` and `Ft8Sharp.Deep.Tests`**, from the
  TRX, ranked. The twenty slowest named with their times. *must-pass*
- **A gate set defined and committed** in `docs/gate-set.md`: each entry with the
  property it guards and **the breakage it would have caught**. An entry that
  cannot name one does not belong in it. *must-pass*
- **The gate set runs in under three minutes**, measured. If it cannot, say what
  the floor is and why. *must-pass*
- A command that runs exactly the gate set and nothing else. *must-pass*
- The same ranking attempted for `Hamlet.RadioEngine.Tests`, from whatever a
  partial run reaches. **A cut-off run still times the tests it got to.**
  *nice-to-pass*

**The properties the gate set must cover**, at minimum: Deep is a superset of the
port; the port's gates are in the decode path; `Ft8Sharp` references nothing
outside itself; the ladder reports zero wrong; the census reaches all three
surfaces.

**This step pays for itself inside two units.**

**Depends on:** nothing.

---

## Step 2 - the SNR column shows a number

**Delivers:** a real signal-to-noise ratio per decoded message, on screen.

**Entry:** step 0 complete.

**Exit:**
- SNR computed per decoded message from the known symbol sequence: power in the
  correct bin against the seven wrong bins at the same instant, referenced to
  2500 Hz. *must-pass*
- **Agreement with the ladder's own commanded SNR, mean and 95th percentile both
  quoted**, over at least two hundred synthesized messages. The ladder knows what
  it delivered, so **this closes without WSJT-X.** *must-pass*
- **If agreement is worse than 2 dB the column keeps its dash**, and the step
  closes with the measurement rather than the display. A number 5 dB out is worse
  than no number (§0.0). *must-pass*
- The measurement is report-only and changes no decode arithmetic. *must-pass*
- It reaches the `snr` column at the width already reserved, telemetry and the
  sidecar. *must-pass*
- Agreement with WSJT-X on a real capture. *deferred* - needs a fixture Tim
  generates; **the step closes without it.**

**This is the second thing the operator sees, and the number an FT8 operator
reads first.** The column has carried a dash since the panel was built.

**Depends on:** step 0.

---

## Step 3 - ordered statistics, taken as far as it goes

**Delivers:** OSD tuned and measured to its practical limit.

**Entry:** step 0 complete. Independent of steps 4 and 5.

**Exit:**
- **The 50 per cent crossing and the -21 dB rate quoted with trial counts and
  Wilson intervals**, before and after, separately from every other stage.
  *must-pass*
- **Zero wrong decodes.** An approach that returns one is rejected and another
  taken. *must-pass*
- **Order and search weight stated with the cost each buys, measured.** Not tuned
  until a number passes. *must-pass*
- Worst-case time per slot inside the 15-second budget, margin stated. *must-pass*
- **The step closes on the figure reached.** The previous plan's waypoint was 40
  per cent at -21 dB; the built implementation reads about 11 per cent, which is
  33 of 306 against the port's 13. **Reaching 40 is not this step's gate and
  never was a fact.** *must-pass*

**Depends on:** step 0.

---

## Step 4 - strong signals are subtracted and the slot is read again

**Delivers:** multi-pass decoding with subtraction.

**Entry:** step 0 complete. Independent of steps 3 and 5.

**Exit:**
- Each decoded message is re-synthesised at its measured frequency, time and
  amplitude and subtracted; the residual is decoded again. *must-pass*
- **The ladder shows more decodes from the same audio than a single pass**, at a
  stated SNR with its trial count. *must-pass*
- **Zero wrong decodes introduced by any pass.** Subtraction leaves residue, and
  residue that decodes is this step's specific hazard. *must-pass*
- Pass count and stopping rule stated with what each pass buys. *must-pass*
- Time inside budget, margin stated. *must-pass*
- Decodes per slot against WSJT-X on a real capture. *deferred*

**On a busy band this is worth more than sensitivity.** A station at -5 dB
sitting on one at -18 hides it completely on the first pass, and Tim's 21:58
capture returned 80 candidates and 7 distinct messages from one slot.

**Depends on:** step 0.

---

## Step 5 - repeated transmissions are combined across slots

**Delivers:** combining a station's repeated transmissions before decoding.

**Entry:** step 0 complete. **Not gated on steps 3 or 4** - if either stalls,
this is the step to take instead.

**Exit:**
- A transmission repeated in a later slot at the same frequency is identified and
  its soft values combined with the earlier one before decoding. *must-pass*
- **A message decoded that no single slot could decode alone**, demonstrated on
  synthesized signals at a stated SNR below the single-slot crossing. *must-pass*
- **Every combined decode verified against the ladder's own ground truth** - the
  message that went in - so a gain is never inferred from another decoder.
  *must-pass*
- **Zero wrong decodes.** A wrongly combined pair fails the CRC-14 and is
  discarded rather than shown, which is what makes this safe. *must-pass*
- The gain quoted on the ladder with its trial count. *must-pass*

**This is the step that means better than the best rather than equal to it.** A
CQ repeats every thirty seconds, identically; two repeats is 3 dB of processing
gain and four is 6, and nothing in WSJT-X combines them. **It closes entirely on
synthesized signals** - the ladder knows what it transmitted.

**It is also the most likely to fail, and failing it is an acceptable end to this
phase.**

**Depends on:** step 0.

---

## Step 6 - the closing measurement

**Delivers:** one table saying where the decoder now stands.

**Entry:** steps 0 to 5 closed, in whatever state each reached.

**Exit:**
- **One committed table**: the port, and Deep with each stage on and off, at -19,
  -20 and -21 dB, on grid and at cell centre, 306 trials a cell, with wrong
  counts. *must-pass*
- The 50 per cent crossing for each configuration, interpolated, with its
  interval. *must-pass*
- **Time per slot for the shipping configuration**, with the budget margin.
  *must-pass*
- **What the operator should now see on his own radio**, in plain words, with the
  figure behind each claim. *must-pass*
- The fixtures that would settle the deferred criteria, named, with the command
  Tim runs. *must-pass*

**Depends on:** steps 0 to 5.

---

## What is not in this phase

- **Transmitting FT8.** Its own phase, dummy load first, §0.2 and HM-DEC-098.
- **A priori decoding**, worth 3 to 4 dB on messages addressed to you. It needs a
  station to be in a QSO, so it follows transmit.
- **FT4.** Reuses nearly all of this and follows it.
- **The CW decoder**, the 419 dropped chunks in the 21:58 capture, the 51
  inherited failing cases, the engine project's missing total.
- **The 101.33 ms broadband pulse above 6 kHz** in every capture since 2026-09-03
  21:06, outside a 3.5 kHz filter's passband and not a splice. Something
  host-side adds it after the filter. **Recorded, not chased.**
- **`validate-output.bat`'s permitted-spellings bug**, `ReusableWindow`,
  `ProcessDelayForTests`, the tap's owner, unit 237's Extensible conclusion, work
  instruction 231's four tree items.
