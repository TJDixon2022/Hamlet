PROJECT: Hamlet
ISSUED: 2026-08-19

## Asks still outstanding (inbound, per HM-DEC-139)

| Ask | First made | Waiting on |
|---|---|---|
| **Whether an attended automatic cycle may reach an antenna** (§0.2, HM-DEC-098) | 2026-08-17 | The bench evening. `BENCH_CARD.md` can now be followed end to end |
| **A callsign too long for one keyer send** (HM-DEC-130) | 2026-08-18 | The seam measured at the bench, from the send panel |
| **Whether the star asks for a name at the moment of saving** (HM-DEC-060, HM-DEC-134) | 2026-08-18 | Nothing but the ruling |
| **Whether Hamlet may ever ask the radio to send its spectrum** (HM-DEC-062, HM-OPEN-042) | 2026-08-18 | The ruling |
| **What HM-DEC-088's duplicate becomes** (HM-OPEN-046) | 2026-08-19 | **Ruled A on 2026-08-19: the later ruling takes the next free id. Phase 0 below.** |

HM-OPEN-045 is withdrawn — the reasoning lives in `CLAUDE.md` §1 where sessions
actually read it, and recovering it was not worth the time.

---

# Work order — resume: the previous session dropped mid-phase-4

**The previous run of this work order died on a connection drop while working
phase 4.** Phases 1 to 3 completed and were pushed; commit `2dd617e` was on
`main` with the push matching origin, and `SendLengthIsLegibleTests.cs` was
written. `PROJECT_STATUS.md` was left reading `EXECUTING · phase 4 of 6` with a
stale timestamp, which is the annunciator working, not a second fault.

**Four phases**, fewer than §12.3's five or six because three are already done.

Gate first (HM-DEC-099). Write `PROJECT_STATUS.md` now — it is currently lying —
then at every phase boundary and at the finish.

## Before anything: establish what actually landed

`git log` since the previous report, and `git status`. **Do not trust the summary
above.** State what phases 1 to 3 left behind, whether anything from phase 4
reached a commit, and whether the working tree is clean. If phases 1 to 3 are
*not* on `main`, say so and stop — that is a different problem from this one.

## The test every phase is judged by

**The operator is going to sit at his radio tonight, find live CW that Hamlet
cannot currently read, and hold conversations.** Every phase serves one of three
things: he can read it, he can answer it, or tonight's failures become tomorrow's
fixtures. If a phase does not, say so rather than working it.

---

## Phase 0 — HM-DEC-088's duplicate (ten minutes, do this first)

Two different 2026-08-16 rulings share HM-DEC-088, which §2.1 forbids outright.
**Tim ruled A: the later ruling takes the next free id.**

- **The tiebreak comes from `git log`, not from judgment.** Whichever row was
  committed first keeps 088. Do not decide it by which has more citations — that
  criterion is the one that tempts a session into leaving the collision alone.
- Re-point every citation aimed at the renumbered one. `DecisionLogOrderTests`
  names 88 as the known reuse; that allowance comes out once the reuse is gone.
- **This is a clerical correction, not a supersession.** An id that was never valid
  is not a ruling being overturned, so §12.1 clause 2 does not apply and this does
  not need a further ruling. Record it as the correction it is.

## Phase 1 — Tonight's failures become tomorrow's fixtures (the dropped phase)

He is going hunting for CW that Hamlet cannot read. Every such signal is worth
more as a recording than as a memory, and `FIXTURE_BRIEF.md` governs what a
fixture needs.

The previous session got as far as *checking the sidecar defect first*, which was
the right instinct. Pick up there.

- **Verify the sidecar frequency defect is fixed before building on it.** An
  earlier report found the sidecar and the telemetry disagreeing on one capture —
  `7025400` against `14028000`, one file, two paths, one wrong. HM-DEC-111 exists
  because a sidecar asserted a freshness it had not measured. If tonight's fixtures
  carry the wrong frequency they are worse than no fixtures.
- **One control at the terminal: keep this one.** Audio and sidecar written
  together, with the frequency, the mode, the measured speed if there is one, and
  what Hamlet did or did not emit.
- **A capture with no decode is the most valuable kind here.** Do not require a
  successful decode to save one.

## Phase 2 — Read what he currently cannot (the settled pass gap)

**This is the phase that decides whether tonight is worth sitting down for**, and
the previous run never reached it.

`DECODER_AND_SCANNER_BRIEF.md` phase 4 names it as the largest known gap and its
most valuable work. **Check what of that brief is already built before starting.**

On `exchange-easy`, which the reference reads at 100%: the provisional tip reads
18 characters with 0% unresolved; the settled pass reads 15 with 73%, and 10 with
50% after HM-DEC-105.

The lead is in the brief: the reference de-glitches at 20 ms, extracts runs, fits
the clock, then **de-glitches again at 0.4·dit and re-reads every run.** Hamlet's
settled pass has a per-character trailing window and no second read of the same
evidence.

**If it cannot be closed, say so with a number and stop.** HM-OPEN-017's
labelled-approximation fallback is taken by ruling, not by a session's judgment.

## Phase 3 — The two standing decode failures (DROP THIS ONE IF SHORT)

`ClearingTheTranscriptLeavesTheDecoderAlone` and
`TheBulletinDecodesToItsAnswerKey`, left red deliberately by HM-DEC-114. Phase 2
may move them on its own; check before doing separate work, and if phase 2 fixed
them say so rather than claiming a phase.

**Drop this whole if short and say you dropped it.**

## Already done, do not redo

- **Mode follow** — HM-OPEN-041 closed. The trigger was `ScheduleModeFollow` firing
  on every `FrequencyHz` change, and the snap-back was changing it twice per tune.
  `DialGuard` shipped; `ModeFollowReschedules` asserts the quiet case. **The
  sixty-six-second `not_in_morse` refusal on 2026-08-18 was the tracking bug wearing
  a different costume and should not recur.** Verify only.
- **The long-message refusal made legible before send** — phase 3 of the previous
  run, `SendLengthIsLegibleTests.cs`.
- **The suite's flake** — HM-OPEN-024 closed, parallelization disabled for the app
  assembly, standing red is exactly two.

## Named and left (§12.6)

The four unruled asks above. Do not build around any of them. **No transmit work
toward auto-CQ** — HM-DEC-098 is unruled and dummy-load only.

## Reporting

`OUTPUT.md`, four sections (HM-DEC-106), section four carrying the asks queue.

**Section two is written for a man about to sit down at his radio tonight**: what
he can do that he could not this morning, in the order he will hit it, and what to
press when something defeats the decoder. Nothing in it he cannot act on from the
operating position.

**If you finish every phase, stop and report.**
