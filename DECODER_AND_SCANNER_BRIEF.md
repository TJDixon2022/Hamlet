**PROJECT: Hamlet**

# Work order: finish the decoder, then build the band scanner

Eight phases. **This exceeds §12.3's five or six deliberately** — Tim ruled the
combination knowing the length, because he is away and a long unattended run is
worth more to him than a short one he has to shepherd.

Reported per §12.2: four sections, **written to `OUTPUT.md` at the repository
root, overwriting it**, and printed to the session as well.

**Read first:** `CLAUDE.md` (§0.0, §0.2, §0.2.1, §12), `SESSION_PROTOCOL.md`,
`BATCH_BRIEF.md` (session 2 is phases 6–8 below), `GENERATOR_BRIEF.md`,
`OPEN_ISSUES.md`, `DECISIONS.md`.

## Standing instruction for this run — Tim is away

Normally a session that needs a ruling stops and asks (§9.5). **For this work
order only: record the question in `OUTPUT.md` section 4 and continue to the
next phase.** A phase that cannot proceed without a ruling is skipped, named as
skipped, and does not stop the run. Do not decide it yourself — §12.1's four
part test is unchanged, and anything touching §0.0, §0.2 or what the display
asserts is Tim's without exception.

**No transmit work of any kind.** Auto-CQ is HM-DEC-098 and is not in this work
order; build nothing toward it.

---

## Phase 1 — withhold the speed while the clock re-acquires (HM-OPEN-022)

**Ruled: the decoder names no speed until the new clock is proved.**

Measured on the two-station recording, 11 WPM handing to 22, the decoder named
16 and 18 — the average, describing nobody — with excursions to 24, 26, 27, 29,
31, 34, 36, 41 and 44, before coming to rest correctly. A speed is a fact about
somebody's keying, and 16 WPM is a fact about neither station.

Marking it unsettled was **rejected**: HM-DEC-090 requires one guarded answer
read by every surface, and an unsettled 44 is still 44 on the screen a beginner
uses to judge whether he could have copied the exchange. He concludes it was
beyond him when it was not. Showing the last proved speed was rejected as
asserting a stale fact as current.

The field goes blank, or says it is re-acquiring. It does not carry a number.

## Phase 2 — the half-amplitude correction stays out of the tone survey (HM-OPEN-023)

**Ruled: HM-DEC-105's correction applies in the settled pass and not in
`CwToneSurvey`.**

The two stages answer different questions. The settled pass measures how long a
mark is, where half amplitude is the true edge. The survey decides whether
anyone is keying at all, judged on cluster separation, which shortening every
mark tightens. **A correction that improves one measurement and breaks another
is not one correction.**

The deciding evidence is not the five noiseless fixtures it costs but the real
13:47 off-air capture, where applying it in the survey loses the tone entirely.

Add a test that pins the 13:47 capture's tone being found, so a future session
cannot quietly unify the two definitions in the name of consistency. Name the
test for what it proves.

## Phase 3 — repair the reference's gap classifier at 25 WPM (HM-DEC-103)

At 25 WPM the reference fits the clock correctly — dit 55, dah 152, ratio 2.77
— and then returns **every element as its own character**. The same message ten
decibels weaker reads 63%, which is what says the fixture is sound and the
reference is not.

**Start with the defect already found:** the reference hardcodes a 50 ms window
while its own specification says bandwidth follows speed — a rule written and
never implemented. That is likely the root cause of the gap classifier's
failure, and it is the same class of error as the published dit 106 / dah 283
turning out to be a 50 ms window's artifact rather than the station.

**The reference is the control for every fixture in the suite.** A blind spot in
it is worse than a blind spot in Hamlet, because it silently certifies or
condemns everything else. Exempting `fast-easy` from the gate was rejected
(§12.5).

Then `fast-easy` passes, and `clean-25wpm` retires with its reason recorded —
one retirement, per HM-DEC-103.

## Phase 4 — the settled pass gap (HM-DEC-102). GIVE THIS THE MOST ROOM.

On `exchange-easy`, which the reference reads at 100%: the provisional tip
reads 18 characters with 0% unresolved; the settled pass reads 15 with 73%,
and 10 with 50% after HM-DEC-105. The gap survives on proved audio and is
recorded as a ratchet.

**The settled pass exists because the clustering gate is better. On sound audio
it currently is not.** That is the largest known gap between Hamlet and the
reference and it is this work order's most valuable phase.

The reference's own structure is the lead: it de-glitches at 20 ms, extracts
runs, fits the clock, then **de-glitches again at 0.4·dit and re-reads every
run**. Hamlet's settled pass has a per-character trailing window and no second
read of the same evidence. Whether the streaming form can be made to re-read
within its window, and what it costs, is the question.

If it cannot be closed, **say so with a number and stop** — HM-OPEN-017's
labelled-approximation fallback is still available and is taken by ruling, not
by a session's judgement.

## Phase 5 — the `MVRR` shortfall

Capture 013347: the reference reads `MVRRVA3VRR` at high confidence with dit
106, dah 283. Hamlet's settled pass reads `MVRR` and stops partway through the
callsign.

**The leading hypothesis is dead** — the corrected tightfist fixtures did not
move it, and the control's own control is untouched, reading the capture
identically. A fresh hypothesis is needed. Phase 4 may resolve it as a side
effect, since both symptoms are the settled pass stopping early; check before
investigating separately.

---

# The band scanner — `BATCH_BRIEF.md` session 2, governed by §0.2.1

**§0.2.1 and HM-DEC-107 now govern every tuning write.** Read them before
phase 6. A scan never transmits, never runs while a transmit path is armed, and
refuses to start before `RigStateMonitor.Populated`.

## Phase 6 — per-bin statistics from the sweeps already arriving

The waterfall delivers about 4.5 sweeps per second. **It cannot identify
Morse** — a dit at 20 WPM is 60 ms and seeing elements would need roughly 30
sweeps per second, so the keying is aliased completely. What it can measure,
over a 10–30 second window per bin, is occupancy and variability:

- a steady carrier is high amplitude with low variance,
- an operator sending is intermittent presence at roughly 40–70% duty,
- empty spectrum is flat.

The scope span is around 500 kHz against a 500 Hz receive passband, so the
survey covers roughly a thousand times more spectrum than the operator can
hear. Rank bins by intermittency, **explicitly demoting steady carriers**,
which §1.4 already reports as interference. Bin-to-frequency mapping comes from
the sweep header parsed in the scope work.

**The waterfall proposes; the audio decoder confirms.** Nothing here decides
that a signal is CW.

## Phase 7 — the dwell loop and the stopping classifier

Dwell on each ranked candidate long enough for the real decoder to run —
10 to 20 seconds is roughly two CQ cycles — and score what came back. Stop on
something that decodes; log and move on if not.

"A conversation" in decoder terms is a callsign-shaped token, `DE`, `CQ`, `K`,
`73`, or a pattern repeated across the window. **A scanner that stops on `CQ` is
worth ten times one that stops on "there is a tone here."** Carry confidence
through: stopping on a 0.3-confidence maybe-CQ must look different on screen
from stopping on a clean one.

## Phase 8 — the safety envelope, and the band-plan data file

Everything in §0.2.1, built and tested rather than asserted:

- The starting frequency restored on every exit route, including a crash-safe
  path on next connect.
- The configured band-plan segment as a **data file the operator edits**, with
  source marks per §0. No frequency literal in code.
- Abort on a touched dial, on PTT, on rig state going unknown or stale, and on
  an unanswered read.
- One always-visible stop control, and the scan saying plainly that it is
  moving the dial.

Test the aborts by simulating each, not by reasoning about them.

## Drop candidates, in this order

1. **HM-OPEN-014's flake** — `TheDecoderAggregationDoesNotAllocatePerCharacter`
   passes in isolation and fails under concurrent load.
2. **Phase 5**, if phase 4 has consumed the room. It is the smaller prize and
   phase 4 may resolve it anyway.

Say which were dropped. Do not half-build one.

**If every phase completes, stop and report. Do not start the UI work order,
and build nothing toward auto-CQ.**

---

## Definition of done

Phases 1–3 ruled and implemented, with the 13:47 pin test and `clean-25wpm`
retired. Phase 4 either closes the gap or reports it as a number. Phases 6–8
give a scanner that tours a configured segment, stops on something that
decodes, restores the dial, and refuses to start when rig state is not
populated — **all provable on the development computer against the simulator
and the fixtures, and none of it evidence about the radio** (HM-DEC-093). Tim
verifies on COM3 when he returns.
