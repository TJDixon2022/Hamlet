**PROJECT: Hamlet**

# Work order: close the last decoder gap, retire the second band plan, and run the scanner

Six phases. Reported per §12.2: four sections, **written to `OUTPUT.md` at the
repository root, overwriting it**, and printed to the session as well.

**Read first:** `CLAUDE.md` (§0.0, §0.2.1, §4, §12), `SESSION_PROTOCOL.md`, the
previous `OUTPUT.md`, `OPEN_ISSUES.md`, `DECISIONS.md`.

Three rulings arrived after the last session started and it was working from a
`CLAUDE.md` that predates them: **HM-DEC-108, 109 and 110.** All three are
recorded and are phases 1, 2 and 3 below.

## Standing instruction

A phase needing a ruling records the question in `OUTPUT.md` section 4 and
continues to the next phase. §12.1 is unchanged.

**No transmit work of any kind.** Auto-CQ is HM-DEC-098; build nothing toward
it.

---

## Phase 1 — the third confidence measurement (HM-DEC-108)

**Ruled and recorded.** The last two reports raised this identically and it is
answered: confidence gains a third measurement — **how far the gap that ended a
character sat from the boundary it was judged against** — and the **worst of
the three wins**, as the existing two do.

This extends HM-DEC-048 and supersedes nothing. Nothing raises a confidence; the
model gains one more way to lower one.

The fault it catches: single-element characters produced where the pass divides
characters in the wrong place. A lone dah spells T, a lone dit spells E, the
elements themselves are clean, so the timing margin of a dah that really is a
dah is one and the old model could not see the fault at all.

**Barring the settled pass from full strength was rejected** as treating the
symptom by discarding what was right. Leaving it alone was rejected in the
report before it was proposed: two wrong letters in eight at full strength is
one in four, and on a callsign he is about to answer the operator could act on
it.

Measure the stranger counts before and after on `coverage-easy` and
`exchange-easy` — currently two of eight and one of seven — and keep them as
ratchets either way. **This is the last §0.0 gap in the decoder**, and phase 5
of the last order put that transcript on screen, so what it asserts matters more
than it did a session ago.

## Phase 2 — sweep the frequency (HM-DEC-109)

**Ruled and recorded.** The frequency joins Mode and FilterSelection on the
session poll, amending HM-DEC-050 for a third field, for the reason already
written beside those two: a broadcast missed while the app is starting leaves
the model holding a frequency the radio is not on, with nothing to correct it
until the dial is next turned.

- Remove the never-polled marking and the test pinning it, replacing that test
  with one proving the sweep happens.
- **The separate staleness rule for the frequency goes.** Once swept, its age
  means the same as every other field's, which is part of why the sweep is the
  clean answer.
- The on-demand reads built last session before a capture and before a spot
  refresh **may stay if they still earn their place** — an operator's tune still
  in flight winning over both is good behavior. Say which you kept and why.
- Then confirm the downstream chain: the band on screen derives from this
  reading, and the band scopes what RBN is filtered to (HM-DEC-024) and what the
  skimmer watch listens for (HM-DEC-075). **Establish whether a wrong band ever
  reached those** rather than assuming either way. If it did, that is a defect
  report, not a fix, and it goes in section 4.

## Phase 3 — retire `BandPlan` (HM-DEC-110)

**Ruled and recorded.** The jump spot is the **first "CW main street" block** in
the cited conventions. 40 m moves 7.030 to 7.028; 30 m moves 10.110 — which
matched no cited source at all — to 10.103. The other five already are that
block.

The QRP watering hole was rejected as moving all seven and aiming a beginner at
a narrower slice than a segment's main street. Keeping the current numbers as
editorial was rejected because it makes an unsourceable number permanent in a
file whose purpose is citation.

- Band edges from `97.301(b)`, CW segments from the union of Data-carrying
  ranges in `97.305(c)`, both as measured last session and kept as nine tests.
- **The neighborhood file is not the source** and must not be used as one: its
  Morse rows fall short at the top of every band, by 10 kHz on 17 m up to
  230 kHz on 10 m, with a hole on 40 m between 7.040 and 7.050. Those rows are
  published conventions; a CW segment is a regulatory boundary.
- **Verify the cited data column-aware against its sources before re-pointing**
  (§4). This is the step the last session correctly skipped because nothing
  re-pointed; now something does.
- Re-point every caller, delete `BandPlan`, and close HM-OPEN-005.
- **HM-OPEN-005's own claim was wrong** and the correction belongs in the record:
  it said the CW segments are convention rather than regulation and do not align
  with the privilege boundaries. They align to the hertz.

## Phase 4 — run the scanner end to end against the training radio

**Every piece is tested and the phases have never been exercised together.**
This is the first thing that runs the survey, the dwell and the safety envelope
as one.

Against the training radio (HM-DEC-026), which places signals by reading the
neighborhood plan, so it teaches the real band:

- Does the survey rank a keyed signal above a carrier when both are present?
  The engine measured 0.955 against 0.006 on generated sweeps; confirm it
  survives a live-ish source.
- Does the dwell reach the decoder, and does a stop verdict carry its confidence
  to the screen?
- Does the dial come back on every exit route — stop pressed, operator turning
  the dial, and the app killed mid-scan?
- Does a dwell that found nobody still report where it was?

Report what broke. **A first end-to-end run that finds nothing wrong is
suspicious**, so say plainly if that is the result.

## Phase 5 — the eight pre-existing decoder failures

Nine failures, unchanged across three sessions, all pre-dating this run of work:
`ASignalAtTheWrongPitchIsStillFound` at 400, 500, 750 and 875 Hz;
`ACleanSignalDecodesExactly` at 25 words a minute;
`AFadingSignalComesBackRatherThanStayingDead`;
`ItGoesQuietRatherThanInventingLettersInTheNoise`;
`TheSpeedEstimateFollowsAChangeWithinAFewCharacters`; and, in the app,
`ClearingTheTranscriptLeavesTheDecoderAlone`.

**Four are the same test at four pitches, so this is probably fewer faults than
tests.** Take them in one pass.

For each, state whether the fault is Hamlet's or the fixture's, as phase 5 of
the fixture order did. `ItGoesQuietRatherThanInventingLettersInTheNoise` is
governed by HM-DEC-097 — the refusal floor is 0 dB — and its bound follows the
ruling rather than the other way round. **Do not move a bound to make a test
pass** (§12.5).

## Phase 6 — DROP THIS ONE IF SHORT OF ROOM

Push. Fourteen commits are sitting local on `main` across two sessions and
nothing has left the machine.

Then measure what the frequency sweep costs the bus over a simulated evening.
The last report called it "should be invisible" and noted that should-be is not
a measurement.

If dropped, say so.

---

**If every phase completes, stop and report. Do not start any other work unit,
and build nothing toward auto-CQ.**

## Definition of done

The decoder's last §0.0 gap is closed or its number is recorded as a ratchet.
The frequency is swept and one band plan remains in the tree. The scanner has
been run end to end at least once and what broke is written down. The nine
failures are each attributed to Hamlet or to a fixture.

**Everything here is provable on the development computer against the simulator
and the training radio, and none of it is evidence about the radio**
(HM-DEC-093). Tim verifies on COM3.
