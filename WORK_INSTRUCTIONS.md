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

# Work instruction 051 — the threshold, the window, and the squelch

**ISSUED: 2026-08-30. A fresh order, not an amendment. Follows unit 050.**

**Seven tasks; task 7 is the drop.**

**Numbering note: two orders in the tree are numbered 050 and both were
executed. This order is 051 and the tree's numbering governs from here.**

## Why this unit exists

**2026-08-30, 00:15–00:17 UTC, 7.058 MHz. The operator heard a station clear as
day, pressed capture twice, and Hamlet refused to admit either one — then printed
61 characters from the frequency it had just refused.**

The sidecars:

```
toneHz    599.0 Hz  (NOT MEASURED: the survey has admitted no keying, so this is
                     the loudest bin in the band rather than a station)
unkeyed   YES  (61 characters reached the screen from a pitch chosen by the
                loudest bin in the band, with no keying admitted here)
competing none admitted, and the survey is not silent: the loudest thing in the
          band is at 600 Hz, +17.6 dB over the band floor, keyed 39% of the
          time. Nothing has judged it to be a station
```

**The pitch was right. Unit 050's spectral peak found 599–600 Hz and the station
is at 600 Hz.** The failure is entirely downstream of it.

### The cause, measured

**The key-down threshold is set by an Otsu two-class split of the envelope over
the whole recording. Otsu assumes two classes of comparable mass.** The station
occupies about 12% of that file, so there are not two classes — **Otsu split the
noise distribution down the middle and returned a threshold inside the hiss.**

Measured both ways over the last 15 seconds:

| bin | duty at the Otsu threshold | duty at a threshold above the noise |
|---|---|---|
| 450 Hz | 65% | **0%** |
| 500 Hz | 68% | **0%** |
| 550 Hz | 45% | **0%** |
| **575 Hz** | 28% | **21%** |
| **600 Hz** | 27% | **23%** |
| **625 Hz** | 31% | **21%** |
| 650 Hz | 55% | **0%** |
| 700 Hz | 69% | **0%** |
| 775 Hz | 63% | **0%** |

**With a noise-split threshold every bin reads 45–69% and nothing stands out.
With a threshold above the noise exactly one 50 Hz band lights up and the entire
rest of the passband goes to zero. Same audio, opposite verdicts.**

The envelope at 600 Hz over seconds 27 and 28, 5 ms per character, level 0–9 over
−54 to −26 dB:

```
27  5432553455445554467888888888888888864578888741222016888888888888888887512132147888888888888888876578
28  6664203556656524566557888888888888888888888888888888888876788888886431111478888888888888788864121135
```

**Runs of 60–420 ms at −26 dB against a −40 dB floor. Nobody looking at that
should call it noise.**

### What this retires

**The admission test is not broken.** Fed an inflated duty of 39% and an
understated 19 dB swing, "is this one station keying?" correctly answers no — **to
the wrong question.** The test is sound and its inputs are not.

**That also explains unit 043's finding** — a carrier at 802.7 Hz standing 21 dB
over the floor that admission refused — without needing a second theory.

**And it is the third time this class of fault has appeared**: `tonePeak` sampled
its noise reference outside the passband; the `spanLlr` inversion chose its null
hypothesis wrongly; now the threshold is derived from a split of a window that is
mostly silence. **Three times the measurement was right and the reference it was
measured against was wrong.**

### The second failure, and it is one line

**`unkeyed YES` reports that 61 characters reached the screen from a pitch nothing
judged to be a station. It should have prevented them.**

**This has now been ordered three times** — unit 043's task 2, unit 044's clock
variant, and this. **The operator was told two days ago that random characters on
noise had stopped, and tonight 61 of them reached the screen.** Task 1 establishes
whether that refusal ever shipped, shipped narrowly, or regressed. **A fourth
order for the same one-line fix would be the real failure.**

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches. Trust
the tree over this order everywhere they differ.

From unit 050's report:

- Corpus **yield 0.914, precision 0.858, substitutions 30** over 384 adjudicated
  characters — **the phase goal of 0.85 passed.**
- `CwSpectralPeak` ships and feeds the streaming path. The tone tracker was more
  than 100 Hz from the station on four captures of twelve.
- **`N4L` on `cw-2026-08-17-134712` fell from 1.000 to 0.333** and is Tim's ask.
- `TheSilencePropertyIsLockedTests` — 6 passing, green, unmodified.
- Engine, nine folders batched: 1072 passing, 0 failing. **The host crash is in
  the app suite too and HM-OPEN-061 names only one engine class.**
- Eight character-count floors red on unadjudicated audio.

**Record both suites and the corpus score before task 2.**

## Rulings in force

**Transcribed with what was rejected. Do not re-argue either.**

**Tim's rulings:**

> **`N4L` is retired as a reading anchor and the measured pitch is kept.** The
> decoder's own comment already records that the callsign was read only because an
> unmeasured bank centre of 500.0 landed within a tenth of a hertz of a station at
> 500.09. **Re-express the anchor with its reason, as unit 036 did; do not delete
> it. It returns when the peak can find that station honestly.**
>
> Rejected: keeping the tracker — four captures abandoned 100 to 200 Hz from their
> station. Rejected: steering the peak with the tracker — built and measured in
> unit 050 at a cost of 2.9 points, and it did not recover the capture.

> **The phase goal is 85% correct CW, precision before yield.** It is met at 0.858
> **on twelve adjudicated captures, and it failed completely on a strong clear
> signal on the air the same night.** The corpus is not the goal; the air is.

> **Do not break the silence behaviour.**

> **The only measurement is against real data from the real radio.**

> **FT8, FT4 and every other digital mode are outside this conversation's scope.**

**Standing rulings this unit is bound by:**

- **§0.0 / HM-DEC-009** — never present a guess as a decode. **61 characters from
  an unadmitted pitch is this rule broken, and the field that would have stopped
  them already computes correctly.**
- **HM-DEC-120** — nothing emitted on audio holding no signal, and no letters from
  a pitch nobody judged to be a station. **Tightened only.**
- **§0.4** — reproduce, then change, then measure.
- **HM-DEC-007** — tested against WAV fixtures. **HM-DEC-091** — captures are
  read-only.
- **§0.2 / HM-DEC-008** — **no transmit work of any kind.**

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` — `STATE`,
`TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what is moving
inside the task. Same every ten minutes while a task runs.

## The measurement rule that governs every task

**Every change is measured with `CwAccuracy` over the whole scored corpus, before
and after.** Every task reports **precision, yield, substitutions**.

- **Precision must not fall below 0.858.** A change that lowers it is reverted and
  reported.
- **Floors only rise.** The rag-chew evenings, the W1AW seven, KD0UN and the 8 kHz
  synthetic read the same or better.
- **`TheSilencePropertyIsLockedTests` runs after every task and may not be
  modified.**

## The tasks

### Task 1 — has the refusal ever shipped?

**Answer first, before anything is built.**

`unkeyed` computes correctly and prints the right answer. **Does anything act on
it?**

- **Find every place the emit path is gated**, with file and line. Name what each
  gate tests.
- **Is there a gate on "the survey admitted no keying"?** If there is, **why did 61
  characters pass it on `cw-2026-08-30-001650`?** If there is not, say so plainly —
  **it has been ordered three times and this is the fourth.**
- **Report which of unit 043's and unit 044's refusals are in the tree today**, and
  for any that are not, why.

**This is the highest-value line in the unit** because the operator has been told
the problem was solved and it was not.

### Task 2 — the squelch is wired

**If the survey admits no keying, nothing is emitted.**

- **Wire the emit path to the same condition `unkeyed` already computes.** Do not
  invent a second test for the same state.
- **Blocks rather than deletions**, as unit 036 ruled, so no character position is
  lost and only the assertion goes.
- **Report the cost per capture before declaring the task done**, not after.

**Acceptance:** `cw-2026-08-30-001650` and `-001547` emit **no letters** while the
survey refuses them. **`TheSilencePropertyIsLockedTests` stays green.** The corpus
score does not fall.

**Note the ordering: after task 3 these frequencies become admitted and decoding
resumes legitimately. Both halves are needed — this one stops the invented
characters, task 3 stops the false rejection.**

### Task 3 — the threshold comes from the signal, not from a split

Replace the Otsu split with a threshold placed relative to the envelope's own
percentiles:

```
floor  = percentile(envelope, 20)          # noise, robust to a busy signal
peak   = percentile(envelope, 98)          # signal, robust to clicks
if (peak - floor) < MIN_SWING:  no station here
thr    = floor + FRACTION * (peak - floor)
```

- **Sweep `FRACTION` rather than fixing it at 0.5, and report the curve.** The
  reported dah:dit ratio on this capture is **2.1 against a textbook 3.0**, and an
  independent bench measured 2.87 and 3.22 on other captures of this corpus.
  **A ratio near 2 is the signature of a threshold still clipping the leading and
  trailing edges of dahs**, so 0.5 may be too high. **Measure it.**
- **Sweep `MIN_SWING` and report that curve too.** It is the "no station here"
  test and it is now load-bearing.
- **Keep the ±6 dB Schmitt hysteresis.** It is measured and it works.
- **Adopt only a value on a monotonic region of the sweep.** Unit 045 refused to
  adopt off a non-monotonic curve and that is the standard.

**Acceptance:**
- On `cw-2026-08-30-001650` the survey admits 575–625 Hz, `unkeyed` reads **NO**,
  and duty reports about **23%**, not 39%.
- **On the known-good captures the threshold lands within a decibel or two of
  where it lands today and the fixtures do not move.** That is what makes this safe
  — signal and noise have comparable mass there, so Otsu was working.
- **Corpus precision does not fall below 0.858.**

### Task 4 — an intermittent station is not judged on a silent window

The station on `-001650` is absent for the first 15 seconds and present for the
last 15. **A duty or a swing computed over the whole window describes neither
half.**

- **Compute the admission statistics over the strongest contiguous few seconds in
  the window, not over the whole of it.** State the length chosen and why.
- **A station unmistakable for six seconds is a station**, even if the surrounding
  twenty-four are empty.
- **The window is chosen by signal strength, not by where characters were
  emitted** — choosing it by the decoder's own output would make the test circular.

**Acceptance:** `-001547`, the same frequency a minute earlier and weaker, either
becomes admitted or stays refused with task 2 suppressing emission. **Both are
acceptable outcomes. 45 characters on screen is not.**

### Task 5 — the two captures become fixtures

- **`cw-2026-08-30-001650`** — the primary. Station at 600 Hz, about 28 WPM,
  present from ~16 s. **Acceptance as task 3 states it.**
- **`cw-2026-08-30-001547`** — the same frequency weaker.
- **Record the measured element structure** — dit about 42 ms, dah about 88 ms,
  gaps 42 / 90–135 / 245–502 ms — **as measurements, not as an asserted
  transcript.** No text is adjudicated for these captures and none is claimed.
- **The dah:dit ratio of 2.1 is recorded as unexplained** and task 3's sweep is
  the evidence about it.

### Task 6 — `N4L` re-expressed

Per Tim's ruling above.

- **Re-express the anchor with its reason in the test itself** — that the callsign
  was read from a bank centre of 500.0 against a station at 500.09, that it is
  retired as a reading anchor, and **that it returns when the peak finds that
  station honestly.**
- **Do not delete it. Do not change the decoder to satisfy it.**
- **Write the amendment to HM-DEC-144 into the report for Tim to enter.**

### Task 7 — is 1.1 hertz a floor or a bias? *(the drop candidate)*

**Measure only. Change nothing.**

Unit 050 could not settle whether `CwSpectralPeak` can be accurate to a tenth of a
hertz on a keyed signal. **Keying spreads a tone into sidebands, so the peak of an
averaged spectrum is not exactly the carrier.** Whether the 1.1 Hz error on
`cw-2026-08-17-134712` is a floor or a fixable bias decides whether `N4L` returns.

- **Measure the peak's error against the true carrier across the corpus**, using a
  method that does not share code with `CwSpectralPeak`.
- **Report whether the error is systematic** — a consistent offset would be a bias
  and correctable — **or scattered**, which would be the floor.
- **Report how it varies with duty cycle and with speed**, since both change the
  sideband structure.

**Dropped whole if time runs out, and the report says so.**

## Parked — do not touch, do not raise

**FT8, FT4 and every other digital mode**, the digital tab, the digital capture
press, the waterfall.

**The confidence work.** Seven quantities measured, none discriminates. **Do not
add an eighth, do not tune the temperature, do not touch the emission gate or the
character floor** beyond wiring task 2's squelch.

**The joint decoder.** `DEV_ANALYSIS_2026-08-27.md` puts it next and its evidence
stands, **but a decoder that cuts characters perfectly is worth nothing on a
frequency the survey has refused to admit. Detection is upstream of everything.**
It is the unit after this one.

Also: the lattice's structure; the evidence term's magnitude; the settings
contract; the scanner and the calling cycle; `CHANGELOG.md`; the missing
`DECISIONS.md` records; the phrasebook and the recent-places row; the Twin PBT;
the answer key's licensing; the dial-move threshold; the transcript break's
wording.

**Both halves are required: do not touch them, and do not raise them.**

A parked item that genuinely blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **No transmit. Nothing keys the radio.**
- **Do not break the silence property**, and **do not modify its lock.**
- **Do not let precision fall below 0.858.** Revert and report.
- **Do not let a floor fall.** Floors only rise.
- **Do not invent a second test for a state `unkeyed` already computes.**
- **Do not choose task 4's window from the decoder's own output.** Circular.
- **Do not fix `FRACTION` at 0.5 without sweeping it.** The 2.1 ratio is evidence
  against it.
- **Do not adopt a value off a non-monotonic sweep.**
- **Do not delete an anchor.** Re-express it with its reason.
- **Do not assert a transcript for the two new captures.** No truth is adjudicated
  for them.
- **Do not change the spectral peak.** Task 7 measures it.
- **Do not mint a decision id.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused push
is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 to `output.md` at the repository root, overwritten
and printed. **Read the file's own section count and follow it.**

**Write `output.md` before you stop, for any reason at all. Do not hold it behind a
regression run.**

**The section that reports measurements leads with task 1's answer — whether
anything has ever acted on `unkeyed` — and then task 3's threshold sweep with the
duty table reproduced from the tree.**

**The section that says what the owner should expect leads with whether
`cw-2026-08-30-001650` now reads, and with the corpus precision beside it.**

**If you finish every task, stop and report. Do not start the next unit.**
