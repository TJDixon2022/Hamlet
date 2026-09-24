# What the synthetic CQ calls do not prove

Work instruction 414, task 3; PHASE_PLAN.md 1.4 and CLAUDE.md 12.5. Cited by every key file
in `tests/fixtures/cw/synthetic-cq/`.

**The rule, first: no synthetic case is ever the sole evidence for keeping a change.** A
fixture built from the same misunderstanding as the code proves nothing (12.5), and a
generated call is built from this project's understanding of CW by construction. The cases
are tabled in `baseline.md` under their own heading, **outside every total, including the one
3.2 judges** (PARKED.md P7). A change that improves them and nothing else is not kept.

## What they are

Nine CQ calls, `CQ CQ CQ DE N0CALL N0CALL K`, at 12, 18 and 25 wpm by 15, 5 and 0 dB in the
receiver's passband, one fixed seed each, built by `CwFixtureGenerator.Generate` from the
recipes in `SyntheticCq`. **Their keys are exact**: the generator knows what it sent, so
nobody had to read Morse to write them, and a number scored against one is a measurement and
not an indication. That is what they add. What they do not:

## What they do not prove

- **The spacing is textbook, the same spacing the decoder falls back to.** Every case keys
  1 : 3 : 1 : 3 : 7 from the dit. `CwUnitEstimator.MeasureGaps` returns the textbook wants when
  the gaps it measured show no trough between the character and word heaps
  (`src/Hamlet.RadioEngine/Cw/CwUnitEstimator.cs` 216, unit 413's trace,
  [`unit413-trace.md`](unit413-trace.md)). So these cases hand the decoder exactly the
  spacing it assumes when it cannot measure one, and **cannot see the fault step 3 is
  attacking**. A clean read here says nothing about a sender who spaces letters five dits
  apart, as the 7.052 MHz sender did. The five-unit row, where it exists, is one sender's
  measured spacing and nobody else's.
- **The noise is generated, not the band's.** Gaussian noise shaped by three bandpass
  sections to 350-870 Hz, steady from the first sample to the last. The band's noise has
  crashes, carriers, splatter and a floor that moves; none of it is here.
- **One tone, no second station.** A single note at 615 Hz drifting 3 Hz either side over ten
  seconds. Nothing else keys in the passband, so nothing tests the tracker or the front end
  against a neighbor.
- **The keying is machine-perfect.** Every dit is the same length to the sample, every gap of
  a kind is the same, and the edges are 5 ms raised cosines. No hand, no bug, no keyer with a
  weight setting, no speed change inside a call.
- **Steady signal.** No fading; the catalogue's QSB is not used here.
- **The same call every time.** One text, one callsign, `N0CALL`. A decoder could read it
  well and read another call badly.
- **No reference score yet** (PARKED.md P8). HM-DEC-101's gate asks that a reference
  implementation read a generated fixture before it judges Hamlet; that gate runs in Python
  and was not run on these. Until it is, they judge nothing.

## What they do show

Against exact keys, at the decoder as it stood at unit 414's entry: where a clean,
textbook-spaced CQ is read and where it is lost, speed by signal strength (`baseline.md`,
*Synthetic, exact keys*). At 0 dB the decoder prints nothing at any of the three speeds; at 5
and 15 dB it reads the call, with the exceptions tabled there. Those are facts about this
generator's audio. Whether they hold on the air is what the inferred keys and, at the end, Tim
at the radio are for.
