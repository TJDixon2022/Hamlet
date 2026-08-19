PROJECT: Hamlet
ISSUED: 2026-08-19

## Asks still outstanding (inbound, per HM-DEC-139)

| Ask | First made | Waiting on |
|---|---|---|
| **Whether an attended automatic cycle may reach an antenna** (§0.2, HM-DEC-098) | 2026-08-17 | Interlocks watched into the dummy load. **Not tonight — tonight is hand sending** |
| **A callsign too long for one keyer send** (HM-DEC-130) | 2026-08-18 | Five minutes at the bench. **Phase 3 makes this bite less tonight** |
| **Whether the star asks for a name at the moment of saving** (HM-DEC-060, HM-DEC-134) | 2026-08-18 | Nothing but the ruling |
| **Whether Hamlet may ever ask the radio to send its spectrum** (HM-DEC-062, HM-OPEN-042) | 2026-08-18 | The ruling. Lower value than believed — the waterfall works and drives the scan |
| **What repair the 096-to-133 hole gets** (HM-OPEN-045) | 2026-08-19 | **Withdrawn as not worth the time.** The reasoning is in `CLAUDE.md` §1 where sessions read it |

---

# Work order — tonight, on the air: read CW we currently can't, and answer it

**Six phases. Phase 6 is the one to drop.**

Gate first (HM-DEC-099). Write `PROJECT_STATUS.md` now, at every phase boundary,
and at the finish.

## The test every phase is judged by

**Tonight the operator is going to find live CW that Hamlet cannot currently
read, and hold conversations.** Every phase below serves one of three things: he
can read it, he can answer it, or tonight's failures become tomorrow's fixtures.

**If a phase does not serve one of those, it is in the wrong order and you should
say so rather than working it.** This project has spent several days on records
while the operator's evenings went unserved; do not add to that.

Records work in this order is limited to what a phase produces as a by-product.
**Do not open `DECISIONS.md` except to record a ruling a phase actually needs.**

---

## Phase 1 — Nothing takes him out of CW while he is working CW

**This is the one that stops a contact, so it is first.**

Session `9f9d23eb`, 2026-08-18: mode-follow wrote USB with data mode on,
repeatedly, while he sat on CW main street with the CW terminal decoding at
500 Hz. `send_buttons_enabled` refused `not_in_morse` from 20:30:07 to 20:31:13.
**Sixty-six seconds unable to answer, caused by the app.**

- Mode-follow must not move him out of CW while the CW terminal is running or he
  is inside a CW segment of the band plan. Following a spot's mode is defensible;
  overriding what the operator is visibly doing is not.
- HM-OPEN-041's trigger is still unidentified — something recomputes `Decide` at a
  cadence nothing explains, and the last session guarded the write without finding
  the caller. **Find the caller.** A guard in front of an unexplained loop is a
  symptom treated.
- Readiness reasons must be honest. At 00:29:23.700 a refusal read
  `reason: already_transmitting` with `readinessState: OutsidePrivileges` and
  `disagreesWithEngine: false`. If he cannot send tonight, the reason on screen is
  what he will act on.

## Phase 2 — Read what he currently cannot (the settled pass gap)

`DECODER_AND_SCANNER_BRIEF.md` phase 4 names this as the largest known gap and
its most valuable phase, and it is exactly tonight's goal. **Check what of that
brief is already built before starting** — several phases may be done.

On `exchange-easy`, which the reference reads at 100%: the provisional tip reads
18 characters with 0% unresolved; the settled pass reads 15 with 73%, and 10 with
50% after HM-DEC-105. The settled pass exists because its clustering gate is
better, and on sound audio it currently is not.

The lead is in the brief: the reference de-glitches at 20 ms, extracts runs, fits
the clock, then **de-glitches again at 0.4·dit and re-reads every run.** Hamlet's
settled pass has a per-character trailing window and no second read of the same
evidence.

**If it cannot be closed, say so with a number and stop.** HM-OPEN-017's
labelled-approximation fallback is taken by ruling, not by a session's judgment.

## Phase 3 — He can answer, in a real exchange

Hand sending, not the automatic cycle. **No work toward auto-CQ; HM-DEC-098 is
unruled and dummy-load only.**

- **The long-message refusal will bite tonight.** HM-DEC-130 refuses a message too
  long for one keyer send, and a real exchange carries his call, the other
  station's, a report and a name. At 20 WPM a 32-character send keys for 18.9
  seconds. Establish exactly what length refuses at his keyer speed, and make the
  refusal legible *before* he presses send rather than after — he should not
  compose a reply and find out it will not go.
- **The seam itself stays refused** until it is measured at the load. What can
  change tonight is that he knows the limit while typing, not that Hamlet splits.
- Walk the CW terminal's send path end to end as an operator would in a QSO:
  answer a call, send a report, send a name and a 73. Anything that refuses,
  stalls, or reports wrongly is the finding.

## Phase 4 — Tonight's failures become tomorrow's fixtures

**This is the phase that makes the evening compound**, and it is new.

He is going hunting for CW that Hamlet cannot read. Every such signal is worth
more as a recording than as a memory, and `FIXTURE_BRIEF.md` already governs what
a fixture needs.

- **One control, at the terminal: keep this one.** It writes the audio and the
  sidecar together, with the frequency, the mode, the measured speed if there is
  one, and what Hamlet did or did not emit.
- **The sidecar must not assert what it did not measure.** HM-DEC-111 exists
  because a sidecar wrote a frequency read sixty seconds and two tunings earlier
  and asserted a freshness it did not have. An earlier report found the sidecar and
  the telemetry disagreeing on the same capture — `7025400` against `14028000`,
  one file, two paths, one wrong. **Verify that is fixed before shipping this**, or
  the fixtures from tonight will be labelled wrong.
- A capture with no decode is the most valuable kind here and must be keepable.
  Do not require a successful decode to save one.

## Phase 5 — The suite has to mean something for phase 2 to be measurable

Four false failures under load, from headless window tests that build a real
window and lose races. A standing baseline of two that is sometimes six is a
baseline nobody reads, and phase 2's whole method is believing the instrument
(§12.5).

Stabilize or isolate them. Make the standing red count unambiguous and state what
it is.

## Phase 6 — The two standing decode failures (DROP THIS ONE IF SHORT)

`ClearingTheTranscriptLeavesTheDecoderAlone` and
`TheBulletinDecodesToItsAnswerKey`, left red deliberately by HM-DEC-114. Phase 2
may move them on its own; check before doing separate work, and if phase 2 fixed
them say so rather than claiming a phase.

**Drop this whole if short and say you dropped it.**

## Reporting

`OUTPUT.md`, four sections (HM-DEC-106), section four carrying the asks queue.

**Section two is written for a man about to sit down at his radio tonight.** What
he can do that he could not do this morning, in the order he will hit it: what
reads now, what sends now, what to press when something defeats the decoder.
Nothing in section two that he cannot act on from the operating position.

**If you finish every phase, stop and report.**
