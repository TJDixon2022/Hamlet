# WORK_INSTRUCTIONS.md

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      Hamlet.sln
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwProbabilisticStream.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  src\CoreHMI

These four files are fixed. Do not substitute a different file for any of
them and do not report a check against a file this list does not name.

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project - nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

---

## Why this unit exists

**The window clear is off, by Tim's ruling, and the fault it exposed is the
subject of this unit.**

The clear shipped last session built exactly as ruled — width from
`CwProbabilisticDecoder.BandwidthHz` rather than a literal, "while something was
being read" from the decoder's own current text rather than a proxy. **The
construction was right and the previous order's safety argument was wrong.** It
fired three times across the corpus where the order predicted nought: 650→575 Hz
at fifteen decibels, and 600→675 at nine and at eight.

**All three were the tracker leaving a station it was reading for a bin holding
noise.** Fifteen decibels went from 0.94 right and 0.00 invented to 0.92 and
**0.08**.

**So the clear was correct to fire on moves that should never have happened.** The
defect is upstream of it, in what the tracker chooses.

### And a second measurement points at the same thing

`ANALYSIS-cw-2026-08-22-014113.md`, an independent analysis of a real capture,
found Hamlet's keying sweep reporting `no keying at 625 Hz` on a file holding a
steady station at **608 Hz**, measured over the same six-second window:

| bin | swing |
|---|---|
| 600 Hz | 21.2 dB |
| **608 Hz** | **21.3 dB** |
| **625 Hz** | **17.2 dB** — the one it chose |

**600 Hz was on the grid and 625 won anyway**, four decibels weaker, seventeen
hertz off. The sweep's own report — `5 ms key down, 142 key-downs` — is 0.71 s of
key-down in six seconds, **twelve per cent duty in five-millisecond fragments,
against a measured forty-three per cent in seventy-to-two-hundred-millisecond
elements.** That number describes noise crossing a threshold.

**Two instruments, two recordings, one fault: bins holding noise are winning over
bins holding stations.**

### Ruled by Tim

**The clear goes off.** One line, reversing what shipped.

*Rejected: leaving it on and accepting 0.08 invention at fifteen decibels.*
HM-DEC-120 has been held through four days of pressure and nothing has been traded
for a character count. **A feature that fires only on a bug is not worth the one
property that has never bent.**

*Rejected: gating the clear on the destination having keying.* The keying verdict
takes three seconds to form and the damage happens inside them. **Untested, and
this unit is about the tracker rather than about the clear.**

**The machinery stays in the tree, off, ready for when the tracker is right.**

---

## Verify this instruction against the tree

- **Report mismatches; do not repair the instruction silently.**
- **The previous order asserted the trigger would fire on nothing and it fired
  three times.** Assume nothing in this order about what a measurement will show.
- **The failing set is 28.** Record it exactly before and after, name every
  difference.
- **Report on the sweep AND every real recording together, every time.**
- **A binding that resolves and an element not on screen look the same to a test
  that reads the log.**

---

## Rulings in force

- **HM-DEC-120.** Nothing emitted on audio holding no signal. **Task 1 restores
  it.**
- **HM-DEC-095**, which ruled that a note is chosen by how it is keyed and never by
  how loud it is. **Read it before task 2. It is the ruling this unit is testing
  against reality.**
- **HM-DEC-009** and **§0.0.**
- **HM-DEC-091.**
- **HM-DEC-096** phase 3, the mid-character interlock. **Untouched.**
- **HM-DEC-150**, the version scheme. Task 5.
- **HM-DEC-093** and `SHACK_FACTS.md` — no radio on the development machine.
- **§12.5** — no answer key for a recording nobody has adjudicated.

---

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` per `CLAUDE.md`
**§13**, which names that file's fields — `STATE`, `PHASE`, `BALL`, `NEXT_PASTE`,
`UPDATED`, `NOTE`. `UPDATED` from the clock; `NOTE` says what is moving inside the
task. Also every ten minutes while a task runs.

---

## Task 1 — The clear goes off, on its own, first

One line. **The machinery stays in the tree.**

**Report the sweep and every real recording immediately.** The expectation is
fifteen decibels back to 0.94 right and 0.00 invented, and every recording
character for character as it was.

**Commit this on its own** so it can be reached without the rest of the unit.

**If the sweep does not come back, stop and report.**

---

## Task 2 — Why a noise bin beat a station

**Report before changing anything.**

Three fires are named and reproducible: **650→575 Hz at fifteen decibels, 600→675
at nine, 600→675 at eight.** For each:

1. **What did the tracker score the bin it left, and the bin it took?** Name the
   metric and give both numbers.
2. **What is actually in each bin** — measure the audio, not the tracker's opinion
   of it.
3. **Why did the noise bin win?** Not "it scored higher" — what property of noise
   made it score higher than a station.

Then the same question from the other instrument: **on `cw-2026-08-22-014113.wav`,
why did the keying sweep rank 625 Hz above 600 and 608?**

4. **What does the sweep rank bins by?** The analysis says it does not know and
   calls this a question rather than a defect claim. **Answer it.**
5. Are these the same metric or two different ones? **If two, say whether they
   disagree anywhere else.**

**If the tracker and the sweep are both right and something else moved the
pitch, say so and stop.**

---

## Task 3 — Fix what task 2 found

Gated on task 2. **Build what it found, not what this order guessed.**

HM-DEC-095's finding is the standing one: **noise routinely produces
twenty-five-millisecond marks, which is a legal dit at forty-eight words a
minute, and what noise has never got is a gap between the two mark-length
clusters.** Whether that separation is being computed, and whether it is being
used where these choices are made, is task 2's to establish.

- **Nothing raises a confidence score.**
- **Report the three fires after the change.** They should not happen.
- **Report the sweep's choice on `014113` after the change.** 608 or 600, not 625.

---

## Task 4 — Prove the corpus is unharmed

- The sweep, every level, against the numbers task 1 restored.
- Every real recording, character for character, quoted.
- Both recordings holding no keying silent, offline and streamed.
- **The failing set exactly, every survivor named.**

**If any recording reads worse, stop and report.**

---

## Task 5 — Bump the version

Read the current version from `Directory.Build.props`, bump the patch, report what
it moved from and to. **HM-DEC-150.** One work unit, one patch.

---

## Parked — do not touch, do not raise

- **The window clear itself.** Off, machinery kept. **Do not remove it and do not
  turn it back on.**
- **Elements per character and gap promotion** — 1.54 against a textbook 3 on
  `014113`. Real, its own unit, and the analysis document holds the measurement.
- **The advice line pointing at the antenna** on a capture holding a strong
  signal. Real, §0.0, its own unit.
- **The sidecar asserting `13 emitted` beside `text nothing read`.**
- **`014113` becoming a fixture.** Its own unit. **No transcript is ever asserted
  for it.**
- **Asking the decoder whether a new sender is speaking.**
- **`FollowSpeed` has no supplier**; the reacquiring guard; the mark-and-gap
  witness behind HM-DEC-144 and HM-DEC-145; `HM-OPEN-051`.
- **The twenty-two failures predating the decoder removal.**
- **HM-OPEN-012, HM-OPEN-052, HM-OPEN-053, HM-OPEN-054, HM-DEC-130, HM-DEC-098,
  HM-OPEN-033, HM-OPEN-007.**

---

## Asks still outstanding

Carried inbound per HM-DEC-139, verbatim until ruled. **Verify against
`OPEN_ISSUES.md` and report anything here that is closed, or open and missing.**

- Whether the window clear comes back on once the tracker is right.
- Elements per character, 1.54 against 3, and gap promotion.
- The advice line asserting a cause the app can disprove.
- The sidecar asserting two incompatible things about one span.
- Whether the sidecar's `text` should include the leading edge.
- The captures from the evenings of the 20th and 21st are not in the tree.
- Thirty seconds since the last character, for mode-follow's guard.
- Whether `RfGain`'s hundred per cent is a defect or the right answer.
- The likelihood gate at 15.0.
- The keying meter's provisional thresholds.
- `FollowSpeed` has no supplier.
- The mark-and-gap witness behind HM-DEC-144 and HM-DEC-145.
- HM-OPEN-052, HM-OPEN-053, HM-OPEN-054, HM-DEC-130, HM-DEC-098, HM-OPEN-033,
  HM-OPEN-007.

---

## What not to do

Standing prohibitions are in `CLAUDE.md`. Cited, not restated: 9.5.1 one branch and
it is `main`, **and every session commits and pushes to it**; no interactive or
destructive git; do not invent a ruling id; do not touch coverage thresholds.

Unit-specific:

- **Do not tune a threshold to make the three fires stop.** *They stop because the
  tracker stops choosing noise, or they do not stop.*
- **Do not turn the clear back on.** *Ruled off until the tracker is right.*
- **Do not assert a transcript for `014113`**, and **do not build a validity
  scorer** — one was built during the analysis, reached thirty valid Morse
  characters out of thirty, and returned `ETTT TOGATMETTEMTTEEEATEEEMN`.
- **Do not touch the mid-character interlock or the keying meter's wording.**

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. **§12.2 names the four
headings** — **What Claude did**, **What Tim should expect**, **What we should do
next**, **What's blocking us** — the last carrying **Asks still outstanding** per
HM-DEC-139. No other headings.

**Section 1 opens with the sweep after task 1** — whether turning the clear off
restored 0.00 invented.

**Section 2 states in one sentence whether the app invents anything now.**

**Stop and report.**
