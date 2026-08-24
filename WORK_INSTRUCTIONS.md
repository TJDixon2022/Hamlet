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

# Work instruction — find the pitch, on the capture that has no excuses

**This replaces the co-channel work instruction dated 2026-08-23. That
instruction is withdrawn and must not be run.** It was aimed at competing
signals inside the passband. The capture that matters has no competitor and the
decoder read nothing from it, so the premise was wrong.

## Why this unit exists

**The unit's number: zero elements.** `cw-2026-08-24-012403` — 7.0359 MHz,
40 m, S4, thirty seconds — carries a machine keyer at 20 WPM sending
`CQ CQ CQ DE KD0UN KD0UN K`. Measured independently: dit 65 ms, dah 182 ms,
element gap 54 ms, character gap 172 ms, word gap 412 ms — **3.06, 2.89 and
6.92 units**, the tidiest element structure in the corpus. It stands **20 dB
above everything more than 40 Hz away** for the last twenty seconds. The
sidecar reads `inThis 0 characters, 0 unsure, 0 elements seen, 0 resolved`.

Not zero characters. **Zero elements.** The front end saw no keying in a signal
a beginner could copy by ear.

Every earlier zero-element capture had an excuse — a pileup, four stations in a
500 Hz filter, a 0.6 dB lead. This one has none, which is what makes it worth a
unit.

**What is upstream of it.** The tone is 439.9 Hz. Hamlet's `toneHz` reports
**450** and its sweep bin reports **425** — ten hertz high and fifteen low, in
the same file, bracketing the signal from opposite sides with nothing landing on
it. Across fourteen captures the tone report is exactly right **twice**, one of
which is a synthetic file. The W1AW carrier held 499.9 Hz ±0.1 for four minutes
and was reported as 495, 300, 500, 475, 475, 475 and 475.

An earlier review of this tree (`CW_CODE_REVIEW.md`, finding F4) predicted this
shape: `CwToneSurvey`'s dah/dit admission band can refuse a real station, and
when nothing is admitted `CwToneTracker` reports the middle of the fine bank —
a pitch nobody is keying. **That prediction was made and then deprioritised
twice. It is this unit.**

**The phase's number:** E-share in single figures across the corpus, the
adjudicated readings intact, nothing invented. Unit 001 measured E-share at
13 % to 43 %. Moving this unit's number moves the phase because **a decoder
mixing down 10 Hz off the signal has been the input to every measurement taken
so far** — including the observation-model diagnosis, which was formed from
soup that may have been produced from the wrong pitch.

**Build number: read `Directory.Build.props` and increment the patch by one.**
It read `1.11.1` after unit 001. If it still reads `1.11.1`, the withdrawn unit
never ran; produce `1.11.2`. If it reads higher, that unit ran despite being
withdrawn — **say so in the report, prominently**, and increment from what is
there. The number is measured, never assumed.

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and
report mismatches, including where the work succeeded anyway.

**Known red, do not rediscover and do not fix:** unit 001 reported 31 failing of
1567 in the engine and 0 of 481 in the app. 27 inherited; three are unit 001's
named ratchets (`CwSensitivityTests.TheDecoderReadsAsFarDownAsItDidBefore`,
`CwAcquisitionWindowTests.AFastFistIsReadWithoutARunUp(25)`,
`WhatBandwidthTheDecoderListensThroughTests.HoldingTheWindowLongInTimeReadsMore(003016)`);
one is flaky under load
(`BroadcastWhileBusyTests.ABroadcastDoesNotAnswerTheCommandInFlight`).

**`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150** while
`CLAUDE.md`'s index has rows for all of them. Where this unit touches something
an index row claims is ruled, transcribe the row into the report and proceed
only if it does not forbid the change.

**`W1AW_BRIEF.md` is withdrawn.** ARLP034 is not published — the ARRL
propagation archive stops at ARLP033, 14 August 2026. Any figure in this tree
that scores against "published W1AW text" is scoring against something that was
never fetched. Do not use it and do not repair it here; report anything found
that depends on it.

## Rulings in force

**HM-DEC-120, the refusal floor.** The full record is inside the unreadable
range; `CLAUDE.md`'s index row reads *"The refusal floor is 14 in the decoder's
own margin units, superseding the 17 of HM-DEC-117's interim."* The property it
protects is measured and holds: **both captures holding no station emit
nothing.** Nothing in this unit may change that, and each task that touches the
signal path checks it explicitly and says so in the report.

**Do not re-argue it. Rejected already:** trading invention for reach; lowering
a bar to accommodate a change; tuning a constant until a ratchet passes.

**Unit 001 found the sensitivity sweep's `invented` column counts
`CwMatchKind.Wrong` rather than `CwMatchKind.Invented`.** That correction is
unruled. Report both columns wherever the sweep appears; treat neither as
settled.

**The keying witness is not a referee this unit reports against.** An
independent shack-side measurement finds it correct in **5 of 13 captures**,
including `6 ms key down` verdicts that are arithmetically noise — 73 × 6 ms is
0.44 s of key-down in a six-second window against a real duty near 40 % — and
`0 dB swing` on a synthetic tone standing 135 dB above its floor. **Unit 001's
acceptance table split E-share by that verdict, so those numbers are not
interpretable and must not be cited as a baseline.** Task 5 addresses the
witness; until it lands, report absolute counts, not splits.

**Shape conflict, already known:** `CLAUDE_CODE.md` §8's five report sections
win over `SESSION_PROTOCOL.md` §12.2's three headings, per §0. Name it again.

## Status cadence

After each task, before starting the next, update `PROJECT_STATUS.md` per
`CLAUDE.md` — `STATE`, `TASK: n of m`, `BALL`, `UPDATED` read from the clock,
and `NOTE` saying what is moving inside the task. The same every ten minutes
while a task runs.

## The tasks

### Task 1 — the unit is the measurement, so task 1 is the measurement

**Do this before reading any other code.** On `cw-2026-08-24-012403`, force the
mixdown to **439.9 Hz** and count elements through the existing chain.

- **If elements appear**, tone acquisition is the fault, tasks 3 and 4 are the
  unit, and everything diagnosed downstream of the front end was diagnosed
  through a broken front end. Say so plainly.
- **If they do not**, the fault is below the tone. **Stop, report that, and do
  not build tasks 3 and 4** — the unit's premise is dead and the remaining
  tokens are better spent telling Tim so than building on it.

Then trace, answering from the code with file and line:

1. **How the pitch reaches the mixdown.** What `CwToneSurvey` admits, on what
   test, how admitted candidates are ranked, and **what `CwToneTracker` reports
   when nothing is admitted.** Whether a 25 Hz grid exists and where.
2. **Whether the survey refused this capture**, and if so on which test.
   `CW_CODE_REVIEW.md` F4 names the dah/dit band 2.5–3.8; this station measures
   182/65 = **2.80**, inside it, so if the survey still refused, the reason is
   something else and naming it is the finding.
3. **Whether the mark classifier and the gap classifier share a unit.** A
   shack-side forced-unit sweep across 8–44 WPM produced *either* E-heavy output
   *or* single-character fragmentation, never both — and Hamlet produces both at
   once, which suggests two classifiers on different units. Answer from the code;
   this is a trace question, not a change.

Build and run the suite once; record counts as the green baseline.

### Task 2 — save the capture as a fixture

`cw-2026-08-24-012403`, WAV and sidecar, committed as a first-class fixture with
its measured truth beside it: tone 439.9 Hz, 20 WPM, dit 65 ms, dah 182 ms,
gaps 54 / 172 / 412 ms, ratios 3.06 / 2.89 / 6.92, text
`CQ CQ CQ DE KD0UN KD0UN K`, margin 20 dB over everything beyond 40 Hz.

**The text is an independent decode, labelled as such, not published truth** —
see the withdrawal above. It is strong evidence and it is not an ARRL
transcript, and the fixture's own file says which it is.

A test asserts what the decoder currently does with it. **That test is expected
to fail after task 3 and that is the point** — it is the unit's before-number,
not a bar.

### Task 3 — find the pitch properly

Replace the coarse pitch estimate with a full-length transform peak, interpolated
between bins, so the reported tone resolves to a fraction of a hertz. One
transform over the window is the whole cost. **Remove the 25 Hz grid wherever
task 1 found it** — nothing in this chain has a reason to quantise pitch.

Both places that report a tone are corrected together: the decoder's `toneHz`
and the sweep's bin. **The two currently disagree by 25 Hz on the same file and
neither is right**; after this task the sidecar carries one measured pitch and
says where it came from.

Re-run: the new fixture, the whole corpus, the sensitivity sweep. **Both empty
captures must still emit nothing**, stated explicitly. Report the tone table —
measured against reported, all fourteen captures — as the after-number against
the two-in-fourteen before.

### Task 4 — hold the pitch

A tone found once must be held while the station keys and released when it stops.
Task 1's trace says what the tracker does today; this task fixes what it found —
the fallback to a bank centre when nothing is admitted is not a pitch and must
not be reported as one. **Where no pitch has been measured, the sidecar says so**
(HM-DEC-009), rather than reporting a number nobody keyed at.

Re-run everything task 3 re-ran, including both empty captures.

### Task 5 — the keying sweep *(the drop candidate)*

The sweep is on screen every time the decoder is silent and it is wrong more
often than right.

**Measure it after task 3 before rebuilding any of it.** Some of its errors are
downstream of a pitch that was never right, and those cost nothing to fix.
Report the 13-capture table again after tasks 3 and 4; **rebuild only what is
still wrong**, and specifically the `6 ms key down` arithmetic and the `0 dB
swing` on a signal 135 dB above its floor, which is the one answer that is
impossible.

Making an existing on-screen verdict correct is a defect fix, not a new
assertion, so it needs no display ruling. **Adding anything new to the panel
does** — do not.

**This is the drop candidate. Dropped whole, and the report says it was
dropped.**

## Parked — do not touch, do not raise

- **`LogLikelihoods`, `Gate`, `Percentile(sorted, 25) * 0.6`.** The
  observation-model diagnosis was formed from output that may have come from the
  wrong pitch. It is not being acted on until this unit says what the front end
  was actually doing.
- **The co-channel work** — boxcar sidelobes, Hann window, integrator bandwidth.
  Real, and at the back of the queue where the evidence puts it.
- **`ClearOnAStationChange`, `Restart()`, the `Skip()` splice wall.**
- **`CwUnitEstimator.Runs` splitting short runs instead of merging.** If task 3
  moves the measured unit, report it and leave the estimator alone.
- **The `characters emitted` / `text nothing read` contradiction** (four
  sightings: `014113` 13 emitted with an empty transcript, `001831` 26 emitted).
  Real, its own unit, and **`012403` reports 0 and 0 consistently**, so it does
  not block this one.
- **`tonePeak` inflation** (24.7 against ~19, 50.1 against ~21, 62–78 against
  ~26, and a synthetic file inflating the other way). Fourth sighting. Its own
  unit — **but if task 3's transform makes the honest figure free, say so.**
- **HM-OPEN-058, HM-OPEN-059.**

## What not to do

- **Do not change any decode-model constant.** The unit's claim is that it moved
  the front end and nothing else; an edit there destroys the claim.
- **Do not trade HM-DEC-120.** Both empty captures are checked at tasks 3, 4 and
  5 and the result is stated, not implied.
- **Do not add anything to the panel.**
- **Do not build tasks 3 and 4 if task 1's forced-mixdown test finds no
  elements.** Report and stop.
- **Do not cite unit 001's witness-split table as a baseline.** The referee is
  under measurement in task 5.

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch and state
whether each push succeeded.

Report per `CLAUDE_CODE.md` §8, five sections, `output.md` at the repository
root, overwritten and printed. **Section 3 leads with the answer to the question
this unit was commissioned to ask: how many elements the decoder reads from
`cw-2026-08-24-012403`, before and after.** Zero is the before-number. Section 5
carries the phase number and the build number confirmed from
`Directory.Build.props`.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140, so they survive
`output.md` being overwritten. Unit 001 noted that no queue was carried inbound,
which §9.6 makes a defect in the order rather than in the session.

1. **The sweep's `invented` column counts substitutions, not invented
   characters**, so the figure the refusal floor was ruled on is not a
   measurement of invention — twelve of twenty characters at 18 dB were never
   sent, against a column reading nought.
2. **Whether the refill guard should apply to the first fill at all**, or only to
   a refill after the window has been emptied. It costs three ratchets and
   removes the opening soup, including one real character.
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK`, and HM-DEC-150 makes `PHASE`
   the same number as the version's minor** — under the rename there is no field
   for the minor to match, so the two now agree by hand.
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150.** A
   session cannot tell a ruling it is acting against from one that does not
   exist.
5. **New, from the shack-side field report: the keying witness is correct in 5
   of 13 captures**, and it is the thing on screen when the decoder is silent.
   Task 5 measures it; whether a witness that cannot be made reliable should be
   shown at all is a ruling nobody has made.

Plus **HM-OPEN-057** (E-dominance outside the keying verdict, open since
2026-08-22) and **HM-OPEN-007** (open and unruled since 2026-08-14).

**If you finish every task, stop and report. Do not start the next unit.**
