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

**Three sessions have converged on one line, and the evidence for changing it is
now complete.**

`MedianOfShortCluster` takes the median of every mark below the dit-and-dah cut.
On `cw-2026-08-17-134712` that cut is 145 ms, so the median re-admits the nine
chatter slivers that the three-way fit had just excluded, and the dit reads 35–40
ms against a hand-verified truth of 56.3. Coherence is 0.00, `LooksLikeMorse` is
false, and nothing is emitted. **The fit finds the dit cluster at 51.2 ms; the
median throws it away one line later.**

The pieces are all measured and none of them is in the tree:

- **The three-way fit** (session of 08-20) finds `134712`'s three mark populations
  at 14.3, 51.2 and 238.1 ms and the drop fires correctly. Withdrawn because it
  broke `exchange-easy` and `tightfist-easy`, which HM-DEC-114 makes pass-or-fail.
- **`Refine`** was withdrawn earlier for manufacturing five characters from
  `cw-2026-08-20-014854`; with the sample corrected first, it returns to one, which
  is what that recording produces today. **The invention was the poisoned dit.**
- **Amplitude** (last session) separates `134712`'s eleven real elements at
  24.4–24.7 dB above the floor from its nine slivers at 8.1–14.2 dB. A gap of
  **10.1 dB**, still **6.9 dB** after ten decibels of added noise. On
  `tightfist-easy`, where every mark is real, there is one population and no low
  group at all.

**Length alone cannot tell a merged element from a sliver. Amplitude can.** That
is what makes this unit possible and it is why the previous attempt failed.

**Tim's ruling, last session:** amplitude is admitted as a candidate discriminator,
and **a rule built on it must be relative — each mark compared to the marks around
it, never to a fixed number of decibels.** Across ten decibels of added noise the
station falls from 24.6 to 19.7 dB above the floor and the chatter from 14.5 to
12.8; the gap holds but neither figure stays put, so any absolute height is wrong
at one end of that range.

---

## The two questions the last session said must be answered first

**Both are in the tree already and neither needs new audio.**

**One — `cw-2026-08-18-004507`'s stretched marks.** It contains real elements of
90, 160, 180, 205 and 275 ms whose envelope *median* sits at 11–15 dB while their
*peak* is 18–26. Those are marks the gate held open across a dip. **A rule reading
the median alone discards them, and they are real.** Reading the peak alone gives
only a 4.4 dB gap on the callsign window against 10.1 for the median, so the peak
alone is the weaker statistic.

**Neither statistic works on its own. Say which you chose and how it answers this
case**, before any table.

**Two — everything rests on one station.** `N4L` is the only adjudicated ground
truth in nine real recordings. That is a fact about the evidence, not a task, and
it belongs in the report's caveats.

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim; report mismatches and do
not repair the instruction silently.

- **Expected red, do not rediscover:**
  `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`,
  `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`,
  `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`,
  `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`.
  **2,083 tests, four failing. Anything above four is new.**
- `MarkAmplitudeTests` holds last session's measurement and classifies nothing.

---

## Rulings in force

**§9.5.1 — one branch, `main`, commit *and push*.**

**HM-DEC-144 — `cw-2026-08-17-134712` holds a station, callsign `N4L`, elements
ending 21.45–23.01 s, dit 56.3 ms, dah 238.3 ms, ratio 4.24.** The ground truth.

**HM-DEC-114 — the easy tier passes or fails.** *`exchange-easy` and
`tightfist-easy` broke the last attempt. If they break again, that is a hard
failure and the change does not ship.*

**HM-DEC-048 — nothing raises a confidence score.** *This unit makes the decoder
measure better. It may not make it more willing to guess.*

**HM-DEC-090 — marking is not a substitute for silence.** *`cw-2026-08-20-014854`
and `-014935` hold no keying at any pitch. Text from either is invention.*

**HM-OPEN-054 and HM-DEC-143 remain parked.** Amplitude is a property of one mark.
**Whether a mark took part in a character is structure. If the work reaches for it,
stop.**

**HM-OPEN-053 — `ShortestVote` stays at 5. `MaximumRatio` stays at 3.8.**

**HM-DEC-093 — no radio.**

---

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` per `CLAUDE.md` §13 —
the six fields §13 names, **`PHASE`**, `UPDATED` from the clock, `NOTE` saying what
is moving inside the task. Also every ten minutes while a task runs.

---

## Task 1 — The relative amplitude rule, stated before it is built

One paragraph: what the rule is, which statistic it reads, how it answers
`004507`'s stretched marks, and what it does when every mark is real.

- **Relative, per Tim's ruling.** Fitted from the marks in the window. **No mark
  may be judged against a fixed number of decibels.**
- **It must be a no-op where there is one population.** On `tightfist-easy` every
  mark is real; the rule must drop nothing there. *That fixture is the test that
  the rule is not simply discarding the quiet end of everything.*
- Say what it does with a window that is entirely chatter, which is what an empty
  band gives it.

**Report this paragraph before any code.**

---

## Task 2 — Build it, with the three-way fit and `MedianOfShortCluster`

All three together, because they are one mechanism: the fit finds the populations,
amplitude decides which to drop, and the median must stop re-admitting what was
dropped.

- `MedianOfShortCluster` exists for HM-DEC-095's reason — a handful of very short
  marks survive the gate on any real signal and an average is defenceless against
  them. **That reason is still valid.** Where a chatter cluster has already been
  identified and set aside, the protection is doing the harm it was written to
  prevent. **Do not delete it; make it operate on what survives the drop.**
- Fitted from the signal, not a new constant. *Seventh instance of the error class
  five rulings have gone on closing.*

---

## Task 3 — The nine captures and the easy tier

| | required |
|---|---|
| `004507` | ≥ 25 |
| `003016` | ≥ 38 |
| `003126` | ≥ 34 |
| `003758` | ≥ 14 |
| `013347` | ≥ 8 |
| `014854` | **no more than 1** |
| `014935` | **0** |
| the easy tier | **whole, every fixture** |

**Report `134712` and `013622` whatever they do.** Both emit nothing today and
both are what this chain was built to reach.

**If the easy tier breaks, do not ship, and report which fixture and by what
text.** *The last attempt read `VQCQDEN0CALLN0CALLK` for `exchange-easy` and
`TEDETESTK` for `tightfist-easy`; those strings are the diagnostic.*

Also report the dit for all nine, before and after. **On `134712` the target is
near 56.3 ms** — HM-DEC-144's figure, and the one number in this project that is
known rather than estimated.

---

## Task 4 — The fixture

Re-run `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`.

- **If green, print what the decoder read and say whether `N4L` is in it, at the
  right place.** That is the first time a decode has ever been checkable against a
  known answer in this project.
- **If red, say precisely where it dies now.**
- **Do not tune anything to make it pass.**

---

## Task 5 — `Refine`. **DROP CANDIDATE.**

Only if tasks 2 and 3 shipped clean. Re-measure it on top, four columns.
**Withdrawal condition unchanged and non-negotiable:** any text out of `014854` or
`014935` and it does not ship.

**Drop it whole if the session runs long.** *Tonight is an evening at the rig and
the chatter fix is what matters for it.*

---

## Parked — do not touch, do not raise

- **HM-OPEN-054, HM-DEC-143**, and anything about which character a mark belongs
  to. **The nearest boundary. If the rule needs to know, stop.**
- **The keying meter and `CwKeyingThresholds`.** *It is the independent witness.*
- **`MaximumRatio`.** Widened once, moved no character count anywhere.
- **Why the 19th's stations are missing from the audio.** Five theories dead.
- **The 69 and 233.**
- **Adjudicating any recording.** Tim's ear. *He is doing `004507` this afternoon.*
- **HM-OPEN-052**, the five synthesized tests, rulings 096–133, the scorer,
  `CaptureAudioAsync` end to end, `TheRosterIsOneFilePerEvening`.

---

## What not to do

Standing prohibitions are in `CLAUDE.md`. Cited, not restated: §9.5.1 one branch
and it is `main`; no interactive or destructive git; do not invent a ruling id; do
not touch coverage thresholds.

Unit-specific:

- **Do not introduce a fixed amplitude threshold.** *Tim ruled it relative and it
  would be the seventh instance of the class.*
- **Do not ship if the easy tier breaks.** *HM-DEC-114, and it is how the last
  attempt failed.*
- **Do not ship anything producing text from `014854` or `014935`.**
- **Do not tune to `134712`.** *The nine-capture table and the easy tier are the
  guards.*
- **Do not touch the gate, the survey or the meter.**

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. Four sections, no
other headings, per §13: **What Claude did**, **What Tim should expect**, **What we
should do next**, **What's blocking us** — the last carrying **Asks still
outstanding** per HM-DEC-139.

**Section 1 opens with task 1's paragraph and then task 3's table.**

**Section 2 says plainly whether anything shipped, and whether the decoder now
reads `cw-2026-08-17-134712`.** He is going to the rig tonight and needs to know
whether what he is running changed.

**Stop and report.**
