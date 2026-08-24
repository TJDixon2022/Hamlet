STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project — nothing done."

If all four hold, say "Hamlet confirmed" and continue.

---

# Work instruction 002 — lock onto the strongest signal and reject the others

## Why this unit exists

**The unit's number: zero. Nothing in this repository has ever measured what
the decoder does with two stations in one passband.** Every fixture holds one
sender; all nine captures were analysed as though one station were present. The
front end's rejection of a competing signal is unmeasured, untested and
unstated.

What is known about the mechanism, from the code rather than from measurement:
`CwToneSurvey` ranks admitted candidates by `LiftDb`, which is loudness, **so
Hamlet already tries to lock onto the strong one**. After it picks a pitch the
audio is quadrature-mixed there and integrated with a boxcar. A boxcar's first
sidelobe is 13 dB down, so a station 100 Hz away enters the envelope at roughly
−16 dB — attenuated, not rejected. **Locking on and ignoring are different
things and only the first exists.** The consequence is not merely added noise:
two tones in one envelope beat at their difference frequency, so a mark from
the strong station arrives amplitude-modulated at 80–150 Hz, which a boxcar
that wide does not smooth. That chatter is sub-dit length, and
`CwUnitEstimator.Runs` splits on short runs rather than merging them, which
biases the measured unit fast — the 24-on-18 clock error, arriving from a
second station rather than from the clock.

**The phase's number, and the operator's own words: the primary target is
isolating the strongest signal, locking on to it, and ignoring everything
else.** The phase is E-share in single figures across the corpus; unit 001
measured it at 13 % to 43 %, with single-character-word share 35 % to 79 %.
Moving this unit's number moves the phase because the co-channel case is
believed to be a large share of that soup, and after this unit the belief is
either measured true or measured false — and the answer scopes everything
after it.

**Build number this unit produces: 1.11.3.** Unit 001 produced 1.11.1; a unit
delivering the panel work described in the ruling below was drafted as 1.11.2.
**If `Directory.Build.props` reads 1.11.1 when this session starts, that unit
did not run: produce 1.11.2 instead and say so in the report.** The number is
measured from the file, never assumed.

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and
report any mismatch, including where the work succeeded anyway.

Known red before a line is written, from unit 001's report: **31 failing of
1567 in the engine, 0 of 481 in the app.** 27 are inherited, 3 are unit 001's
named ratchets (`CwSensitivityTests.TheDecoderReadsAsFarDownAsItDidBefore`,
`CwAcquisitionWindowTests.AFastFistIsReadWithoutARunUp(25)`,
`WhatBandwidthTheDecoderListensThroughTests.HoldingTheWindowLongInTimeReadsMore(003016)`),
and one is flaky under load
(`BroadcastWhileBusyTests.ABroadcastDoesNotAnswerTheCommandInFlight`). Do not
rediscover these and do not fix them.

**`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150.** A ruling
you cannot read may bear on this work. Where a change touches something an
index row in `CLAUDE.md` claims is ruled, transcribe the index row into the
report and proceed with the change only if the index row does not forbid it.

## Rulings in force

**The operator's ruling, given 2026-08-23, transcribed as stated:**

> **Every receive-path setting that stands between the operator and a readable
> signal is named on the panel, with the control that changes it, at the moment
> it is in the way.** HM-DEC-148 did this for the preamp and the attenuator and
> stopped there. The noise blanker, noise reduction and the filter width are the
> same class of fault: Hamlet reads all three from the radio, records them in
> the sidecar, and mentions none of them. **The operator does not know the
> radio, and that is the premise of the application rather than a gap in him.**
> Advice delivered in a chat window is not the application. **Still read-only** —
> HM-DEC-148's reasoning stands, a later unit may offer a button he presses,
> this one does not write. **A control already in the right position is not
> mentioned**, and **a value that could not be read says so** (HM-DEC-009)
> rather than being asserted as off.

Do not re-argue it. **Rejected already, do not revisit:** writing the setting
for him (HM-DEC-148 — the mode-follow write cost an evening); showing every
setting all the time (advice about a knob already right is noise); leaving it
in the sidecar (a file read the next day is not help at the radio).

**HM-DEC-120, the refusal floor.** Full text is inside the unreadable range;
`CLAUDE.md`'s index row reads *"The refusal floor is 14 in the decoder's own
margin units, superseding the 17 of HM-DEC-117's interim."* The property it
protects is measured and holds: **both captures holding no station emit
nothing.** Nothing in this unit may change that. **Note that unit 001 found the
sweep's `invented` column counts `CwMatchKind.Wrong` rather than
`CwMatchKind.Invented`** — that correction is unruled, so this unit reports
both columns wherever it reports the sweep and treats neither as the settled
figure.

**Shape conflict, already known:** `CLAUDE_CODE.md` §8's five report sections
win over `SESSION_PROTOCOL.md` §12.2's three headings, per §0. Name it again.

## Status cadence

After each task, before starting the next, update `PROJECT_STATUS.md` per
`CLAUDE.md` — `STATE`, `TASK: n of m`, `BALL`, `UPDATED` read from the clock,
and `NOTE` saying what is moving inside the task. The same every ten minutes
while a task runs.

## The tasks

### Task 1 — trace before building. Say what you find rather than confirming this list.

Answer from the code, with file and line, and report before writing anything:

1. **The mixdown filter.** Where the quadrature product is integrated, what
   shape the integrator is, and **what its equivalent noise bandwidth actually
   is** — computed from the code, not from the comment beside it. The offline
   `Envelope()` and the streaming `PushEnvelope()` were reported by an earlier
   review to differ (centred versus trailing); say whether they still do.
2. **What the survey knows about competing signals.** Whether `CwToneSurvey`
   retains more than one admitted candidate, what it keeps about each, whether
   the runners-up survive anywhere a caller can reach, and how `LiftDb` is
   computed. **If the survey already knows a second station's pitch and
   strength, task 4 is a plumbing job rather than a detection job — say so, it
   is tokens back.**
3. **The tracker's grip.** How `CwToneTracker` decides to stay on a pitch
   versus move, and what happens when the survey admits nothing (an earlier
   review reported a fallback to the middle of the fine bank).
4. **Whether any fixture or test anywhere puts two senders in one passband.**
   Expected answer is none. If one exists, it is the starting point.

Then build and run the suite once and record the counts as this unit's green
baseline.

### Task 2 — the two-signal fixture, before any fix

**The measurement comes before the change**, so that the change is judged
rather than illustrated. Using the existing deterministic generator, add
fixtures holding **two senders in one passband**: a strong station with known
text at the corpus's typical 18 WPM, and a competing station at a stated
level and offset, both with realistic keying envelopes and the shaped noise
the generator already produces.

Sweep the offset — **40, 80, 120, 200, 300 Hz** — and the interferer's level
relative to the strong station — **0, −6, −12 dB**. Both senders keying, with
overlapping transmissions, because simultaneous keying is the case that beats.
No real callsigns; `N0CALL` conventions as the existing fixtures use.

Measure and record, for each combination, against the strong station's known
text: correct, wrong, **invented counted as `CwMatchKind.Invented`**, and
E-share. **This table is the unit's before-number** and goes in the report.

### Task 3 — window the integrator

Replace the boxcar with a Hann-windowed integrator of the same main-lobe
width. Sidelobes fall from roughly −13 dB to roughly −31 dB for one multiply
per sample; the strong station's own envelope is barely affected because the
main lobe is what carries it.

**This is a front-end change and touches no part of the decode model.**
`LogLikelihoods`, `Gate`, the length penalties, the speed grid and the unit
estimator are all untouched by this task.

Re-run task 2's table. Re-run the sensitivity sweep and the corpus to show the
single-station case did not regress; **both empty captures must still emit
nothing**, and that is stated explicitly in the report rather than implied.

### Task 4 — narrow it, with the cost measured rather than assumed

Make the integrator's bandwidth an explicit named constant with its reasoning
in the doc-comment, and measure the trade at **60, 45, 30 and 20 Hz**:

- **rejection** — task 2's table at each bandwidth;
- **the cost** — the sensitivity sweep and the corpus at each bandwidth, plus
  what happens to a fast fist, since a narrower filter responds more slowly
  and at 30 WPM a dit is 40 ms.

**Choose from the measurement and say why**, in the doc-comment and in the
report. If no bandwidth improves the two-signal case without costing the fast
fist, **say that and leave the constant where task 3 left it** — a negative
result measured is this unit's job done, not a failure.

### Task 5 — tell the operator there is a second station

Where the survey holds a competing candidate, surface its **offset in Hz and
its strength relative to the locked signal** so the panel can name it, in the
form the operator's ruling requires: what is in the way, and the control that
changes it. On the IC-7300 the filter width and PBT are that control; per
HM-DEC-148's precedent the mention names the control rather than the
diagnosis, and **nothing is written to the radio**.

**Where the strong station is already alone, nothing is said.** Where the
survey could not read a second candidate, the panel says nothing rather than
asserting the frequency is clear (HM-DEC-009).

Sidecar carries the same fact for every capture, so tonight's recordings are
self-describing.

### Task 6 — the noise blanker, noise reduction and filter width on the panel *(the drop candidate)*

The operator's ruling applied to the three settings named in it: each is read
from the radio already, each is mentioned only when it is in the way, each
names the control, none is written. Noise blanker and noise reduction reshape
the envelope, and amplitude is what the decoder measures — that is the reason
they belong on the panel and it goes in the code beside them.

**This is the drop candidate. It is dropped whole and the report says it was
dropped.** The operator ruled it secondary to the tasks above; do not
half-build it, and do not start it if task 4's measurement is still running.

## Parked — do not touch, do not raise

- **`LogLikelihoods`, `Gate`, `Percentile(sorted, 25) * 0.6`** — the observation
  model is its own unit and its ruling is not made. This unit's whole claim is
  that it changed the front end and nothing else; a decode-model edit destroys
  that claim.
- **`ClearOnAStationChange`, `Restart()`, the `Skip()` splice wall** — streaming
  hygiene, one later unit, unruled.
- **`CwToneSurvey`'s 2.5–3.8 ratio band and the `well_separated` valve** — sits
  against HM-DEC-095, unruled. **Admission is not touched by this unit**; task 5
  reads what admission already produced.
- **`CwUnitEstimator.Runs` splitting short runs instead of merging them** — real,
  and named in this instruction's reasoning, but it is a separate unit. If task 3
  or 4 moves the measured unit, **report it and leave the estimator alone**.
- **The sweep's `invented` column** — unruled; report both columns, change
  neither.
- **HM-OPEN-058 (`FastestWpm` remarks say forty, constant is 32) and HM-OPEN-059
  (stale semver comments, `CHANGELOG.md` at 1.9.0 against a tree at 1.11.x)** —
  logged, not this unit's.

## What not to do

- **Do not change any decode constant.** Named above; it is the unit's claim.
- **Do not trade HM-DEC-120.** A change that reads better and invents anything
  the empty captures did not invent before is a failed change, and the two empty
  captures are checked explicitly at tasks 3, 4 and 5.
- **Do not write anything to the radio.** Read-only, per the ruling.
- **Do not tune a bandwidth to make a ratchet pass.** Task 4 chooses from a
  table and states the trade; choosing a constant to make a number look right is
  the §12.5 failure.

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch and state
whether each push succeeded.

Report per `CLAUDE_CODE.md` §8, five sections, `output.md` at the repository
root, overwritten and printed. **Section 3 leads with the answer to the
question this unit was commissioned to ask: how much of the strong station's
text survives a competing station, before and after, at each offset and
level.** Section 5 carries the phase number from §4.2 and the build number
confirmed from `Directory.Build.props`.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Unit 001's report
noted that no queue was carried inbound, which §9.6 makes a defect in the
order.** These four are unit 001's, unruled, and are carried here so they are
not lost when `OUTPUT.md` is overwritten:

1. **The sweep's `invented` column counts substitutions, not invented
   characters**, so the figure the refusal floor was ruled on is not a
   measurement of invention. Twelve of twenty characters at 18 dB were never
   sent, against a column reading nought.
2. **Whether the refill guard should apply to the first fill at all**, or only
   to a refill after the window has been emptied. It costs three ratchets and
   removes the opening soup, including one real character.
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK`, and HM-DEC-150 makes `PHASE`
   the same number as the version's minor** — under the rename there is no
   field for the minor to match, so the two now agree by hand.
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150** while
   `CLAUDE.md`'s index has rows for all of them. A session cannot tell a ruling
   it is acting against from one that does not exist.

Plus **HM-OPEN-057** (E-dominance outside the keying verdict, open since
2026-08-22, measured again by unit 001 and still unruled) and **HM-OPEN-007**
(open and unruled since 2026-08-14).

**If you finish every task, stop and report. Do not start the next unit.**
