# WORK_INSTRUCTIONS.md

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      Hamlet.sln
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwGate.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  src\CoreHMI

These four files are fixed. Do not substitute a different file for any of
them and do not report a check against a file this list does not name.

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project — nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

---

## Why this unit exists

**The gate reads short marks short, and the de-glitch window is the named suspect.**

Last session measured what the gate hands over against what was generated:

| fixture | true dit | gate reads | fitted |
|---|---|---|---|
| `coverage-easy` | 100 | 101.4 (+1%) | 99.5 |
| `exchange-easy` | 100 | 102.8 (+3%) | 99.5 |
| `farnsworth-light` | 100 | 102.6 (+3%) | 95.0 |
| `fast-easy` | 48 | **45.3 (−6%)** | 48.0 |
| `farnsworth-heavy` | 56 | **48.9 (−13%)** | 47.0 |

**Dahs are long by 1–2% at every length. It is short marks specifically that read
short**, and the shorter they are the worse it gets.

**HM-DEC-119 does not hold at short dits.** It says a mark reads long by nought to
ten per cent at every speed, and it is true at 100 ms and false below. **Four
sessions of reasoning about `Refine` cited it as measured fact. It was measured at
one speed.** *That is a finding in its own right and belongs in the report.*

The suspect the last session named and correctly did not touch: **the hop is 5 ms
and `ShortestVote` is five measurements, so a 56 ms dit is eleven hops through a
five-wide median while a 300 ms dah is sixty.** A median filter removes runs
shorter than half its window, and the shorter the mark the larger the fraction of
it that is at risk.

**Tim's ruling, this session: `ShortestVote` comes off the park.**

It was parked on 08-19 because that was the wrong evening to move the instrument
and the subject together, with a measured improvement of 13 to 27 of 43 on the
bulletin's leading edge and five synthesized tests broken, two of them about
acquisition. **That reason has expired.** Since then the project has gained two
adjudicated fists (`N4L` at 56.3 ms, `VA3VRR` at 100.4), two generated fixtures with
written answer keys at both extremes, and a reference that reads both. **The
acquisition tests it breaks can now be examined against evidence rather than
argued about.**

*Also carried: the light fist loses its dit in the estimator instead — the
averaging window is twice the mark-derived dit, so on a 100 ms sender the window is
200 ms and this sender's 150 ms character gaps fall inside it alongside its 73 ms
element gaps. **That is a separate mechanism and it is not this unit.***

---

## A naming correction, recorded once

The previous orders said "`Refine` is not in the tree". The method is at
`CwTiming.cs:1151` and is called at 649. **What has been proposed and withdrawn
four times is its *removal*, and the orders have been calling the removal by the
method's own name.** On the light fist, removing it is the whole loss. *Say
`Refine`'s removal where that is meant.*

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim; report mismatches and do
not repair the instruction silently.

- **Expected red, do not rediscover:**
  `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`,
  `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`,
  `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`,
  `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`,
  `TheToneIsFoundInRealisticAudio(farnsworth-heavy)`.
  **2,117 tests, five failing. Anything above five is new.**
- `prosigns-easy` and `tightfist-easy` were excluded from last session's gate table
  as not comparable: they run elements together, so a midpoint split catches merged
  pairs.

---

## Rulings in force

**§9.5.1 — one branch, `main`, commit *and push*.**

**HM-DEC-144 — `N4L`, dit 56.3 ms, dah 238.3, element gap 35.6, ratio 4.24.**

**HM-DEC-145 — `VA3VRR`, dit 100.4 ms, dah 274.3, element gap 73.3, ratio 2.73.**

**HM-DEC-119 — now known not to hold below 100 ms.** *Do not cite it for short
marks. Task 3 records the correction.*

**HM-DEC-114 — the easy tier passes or fails.**

**HM-DEC-101 — a fixture the reference cannot read is a bad fixture.**

**HM-DEC-048 — nothing raises a confidence score.**

**HM-OPEN-054 stays open. No transition-shape test, no gate in front of emission.**

**The keying meter is not read by the decoder.**

**`MaximumRatio`, `MinimumSeparation` and the five-dit bound stay put.**

**HM-DEC-093 — no radio.**

---

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` per `CLAUDE.md` §13 —
the six fields §13 names, **`PHASE`**, `UPDATED` from the clock, `NOTE` saying what
is moving inside the task. Also every ten minutes while a task runs.

---

## Task 1 — Is the de-glitch the cause? **CHANGE NOTHING.**

Measure the gate's mark lengths with the de-glitch bypassed entirely, on
`farnsworth-heavy`, `fast-easy`, `farnsworth-light` and `coverage-easy`.

- **If the −13% and −6% disappear, the de-glitch is the cause and task 2 follows.**
- **If they do not, `ShortestVote` is not the mechanism** — say so, say what the
  lengths are without it, and **stop.** *Unparking it was justified by this
  suspicion and the suspicion is testable.*
- Report the width in hops for each fixture's dit and dah, so the asymmetry is
  visible as a number rather than an argument.

---

## Task 2 — Fit the window, only if task 1 confirmed it

**Do not simply set `ShortestVote` to 7.** That was the 08-19 proposal and it is
still a constant; **the window should be fitted to the shortest mark the signal
actually contains.** *Seventh instance of the error class six rulings have gone on
closing — and this one is the error class appearing inside the de-glitch itself.*

If a fitted window cannot be made to work, **5 → 7 measured and reported is an
acceptable fallback**, but say plainly that it is a constant and what it would cost
at speeds outside those tested.

| | required |
|---|---|
| **`farnsworth-heavy` gate reads** | **within 5% of 56 ms** |
| **`fast-easy` gate reads** | **within 5% of 48 ms** |
| `coverage-easy`, `exchange-easy` gate reads | **within 5%** |
| **`farnsworth-heavy`** | **> 3 of 12** |
| `farnsworth-light` | ≥ 9 of 12 |
| `cw-2026-08-20-014854` | **0** |
| `cw-2026-08-20-014935` | **0** |
| `004507` | ≥ 25 |
| `003016` | ≥ 38 |
| `003126` | ≥ 35 |
| `003758` | ≥ 14 |
| `013347` | ≥ 8, **and `VA3VRR` still readable** |
| the easy tier and every other fixture | **whole** |

**The five tests it broke on 08-19, two of them about acquisition: name each one,
say whether it still breaks, and if it does, say what it asserts and against what
audio.** *They are the reason it was parked and they now have adjudicated fists to
be examined against. A test that breaks because it encodes the old wrong
measurement is a different thing from one that breaks because the change is wrong,
and the report must say which.*

**If an acquisition test breaks and the change is right, stop and report rather
than editing the test.** That is Tim's ruling to make.

---

## Task 3 — Record the HM-DEC-119 correction

Whatever else happens, record that HM-DEC-119's figures hold at 100 ms and fail
below — with last session's table in the entry, indexed in `CLAUDE.md` §1.

*Four sessions cited it as measured fact about every speed. The next one should
not.*

---

## Task 4 — What it means on the air

One paragraph, and it is what Tim reads first.

**On `CQ CQ DE <callsign> K`, does the callsign survive on each fist?** Last
session: light yes, heavy no, losing nine characters. **Say the number now, on
both**, and say whether `cw-2026-08-17-134712` emits anything.

---

## Parked — do not touch, do not raise

- **The light fist's estimator window.** *Named, separate, next.*
- **`Refine`'s removal.** Not to be revived in this unit.
- **A transition-shape test, or any gate in front of emission.**
- **Character structure**, and the keying meter as something the decoder reads.
- **The bulletin's standing red.** *Report its count if it moves.*
- **Why the 19th's stations are missing from the audio.**
- **The 69 and 233.**
- **Adjudicating by ear.** Tim's.
- **HM-OPEN-052**, rulings 096–133, the scorer, `CaptureAudioAsync` end to end,
  `TheRosterIsOneFilePerEvening`.

---

## What not to do

Standing prohibitions are in `CLAUDE.md`. Cited, not restated: §9.5.1 one branch
and it is `main`; no interactive or destructive git; do not invent a ruling id; do
not touch coverage thresholds.

- **Do not re-cut or soften any fixture.**
- **Do not edit a failing acquisition test to make the change fit.** *Stop and
  report.*
- **Do not work the light fist's mechanism.** *One at a time.*
- **Do not tune to one fixture.** *Two generated fists, two adjudicated recordings
  and the whole suite are the guards.*

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. Four sections, no
other headings, per §13: **What Claude did**, **What Tim should expect**, **What we
should do next**, **What's blocking us** — the last carrying **Asks still
outstanding** per HM-DEC-139. **HM-OPEN-053 leaves the queue; it was unparked and
ruled.**

**Section 1 opens with task 1**: whether bypassing the de-glitch removes the error.

**Section 2 opens with task 4** — whether a callsign at the front of a call now
survives on each fist.

**Stop and report.**
