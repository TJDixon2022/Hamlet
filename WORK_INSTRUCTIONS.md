# Work instruction 507 - the pattern is the gate

**Hand run. One unit.** Every gate on a single mark since unit 497 has been a decibel test - 6 dB
of edge, 6 dB of narrowness, a shape score whose threshold was set on a strong signal. A decibel
test is a loudness test wearing a shape test's clothes. **CW's shape is a pattern across marks, and
none of it is in decibels.** This unit makes the pattern the gate.

**No test against a recording, a fixture, a floor or copied telemetry** (R96). A headless test
driving synthetic hops written in the test itself is allowed; nothing read from disk. Verify by
building `Hamlet.sln` with warnings as errors and running the app carry-forward line. **The
owner's report at the radio is the test.**

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs
  MUST EXIST:      CW_REQUIREMENTS.md
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln
  root             C:\Source\HamLet

If all five are not as stated, refuse: reply with only the path you are in,
which checks failed, and "wrong project - nothing done."

If all five hold, say "Hamlet confirmed" and continue.
```

---

## 1. Rules

- Take `SESSION.lock` through `tools\arbiter\lock.bat take`, release it at the end. Write nothing
  to `RUN_LEDGER.md`. Touch nothing under `tools\arbiter\`. Tick nothing in `PHASE_PLAN.md`.
- One `dotnet test` invocation per line, filtered, with a `timeout`. Never background and poll.
  The app line loses names to the dispatcher loop; re-run once, count neither way.
- Apostrophes in quoted heredocs break; `;`, `rm` and `git rm` are refused; Python cannot run
  here; `-m` more than once for a multi-line commit. Scripts go in `.run-unit\unit507-<name>.sh`
  and are not committed.
- Nothing that keys or transmits. Nothing written to the radio.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. The owner's ruling, and what the rows showed

**His words, 2026-09-30, R112:**

> *"You're still focused on dB. We need to be focused on the shapes in the noise. They're
> predictable. They're full of good patterns. Chaos and noise have no patterns. All we have to do
> is clearly identify when we have a pattern, a shape, and the translation is easy."*

**What a keyed tone is, as a pattern.** Not one rectangle - a **sequence** of rectangles with a
rule:

1. all at one pitch;
2. all at one height;
3. lengths that fall into exactly two values, about 3 to 1;
4. gaps that fall into three values, about 1 to 3 to 7 - or a Farnsworth sender's own.

**None of that is in decibels.** Every measure is a ratio between marks: this height to that
height, this length to that length. A station 8 dB over the noise satisfies all four as well as
one 38 dB over. **Noise can pass a single-mark test by luck. Noise cannot make five marks that
agree on pitch, agree on height, sort into two lengths at 3 to 1 and space themselves at 1 to 3 to
7.** That pattern is the signature, and it is what the owner's ear does when it picks a weak
station out of hash.

**What the rows showed today.** On 14.053, a station the owner hears *clearly*, weaker than W1AW:
four presses, the meter reading noise, the bars finding nothing, nothing printed. On a stronger
one at 14.0529: meter at 500 Hz with an 83 ms dit, the reader printing at 500, the detector's
watched bin at 350 and then 700. **Every synthetic case ever built was 20 dB or more over the
noise, and every gate was set with those in front of it.** Unit 504 said it: *"a weak station,
about 10 to 16 dB over the noise, may show broken bars."*

---

## 3. The change

### One - the single-mark gates go proportional

A bar becomes a candidate mark on what it is **relative to itself**, not to a decibel figure:

- **flat top**: the top holds within a fraction of its own height above the gap beside it - the
  flatness tolerance already follows contrast (R93); keep that and **drop any absolute floor on
  a weak mark**;
- **ends**: the rise and the fall each take no more than a stated number of hops **to reach a
  stated fraction of the top**, not 6 dB - unit 497's `EdgeDepthDb` becomes a fraction of the
  mark's own contrast;
- **narrow**: the bin stands above its neighbours **by a stated fraction of its own contrast**,
  not 6 dB - unit 498's `NarrowDepthDb` becomes a fraction of the mark's contrast over its gap;
- **the shape score** (unit 502) is rebuilt from those ratios, and **its threshold is set on the
  weakest synthetic station below**, not the strongest.

**Every fraction is the author's**, stated in the report with its reason, and **derived from what a
keyed tone is - a rectangle keeps its proportions at any height - not from a recording, and not
tuned after a result.**

### Two - the pattern across marks is the gate

A candidate mark **stands only if it belongs to a sequence that has the shape.** In
`CwRunReader`, or a new stage between the detector and it:

- **Pitch**: within one bin of the sequence's own.
- **Height**: within a fraction of the sequence's own mean - the same fraction unit 490's level
  tolerance uses, restated as a ratio.
- **Lengths**: the sequence's marks sort into two clusters at 2 to 1 or wider, and every mark is in
  one of them. A mark that is neither a dit nor a dah of this sender is not this sender's.
- **Gaps**: inside the sequence, every gap is one of the sender's three - element, letter, word -
  as unit 500 and 501 measure them.
- **Enough of them**: a sequence needs a stated minimum of marks before it stands - **the count is
  the author's, with the reason that noise's chance of making that many agreeing marks is what the
  count buys**, and unit 493's two-run rule is the floor.

**A candidate that fits no sequence is dropped.** It never becomes a mark, never reaches the scope
or the terminal. **That is where noise dies now** - not at a decibel line on one bar, but at the
pattern it cannot make.

### Three - what the display shows

- **The detector's watched bin follows the sequence being read.** The light, the scope's blocks and
  the verdict row's `marks4s` describe the sender the reader is printing - `PrintingHz`, which
  already exists and was right on today's rows while the watched bin was 150 Hz off. Unit 504
  named this; do it here.
- **`marks4s` counts the marks that stood** - the ones in the sequence - not the watched bin's
  paired bars.

### Four - what must not change

The run reader's letter arithmetic from units 500, 501 and 504; the banking and lone-letter rules
from 498 and 501; the terminal and scope's one truth; the one layout; the tab-is-the-mode; the
preamp; the buttons and the verdict row's other fields.

---

## 4. What to measure

**Watch it fail first**, with synthetic hops written in the test. **The weak cases are the point:**

1. **`CQ CQ DE N0CALL N0CALL K` at 20 WPM at 8, 12, 16 and 24 dB over the noise.** Report what each
   reads before and after, and how many of the 65 marks stood at each. **Red today at 8 and 12** -
   or report that it is not, with the counts.
2. **The same at 5 WPM Farnsworth and at 35 WPM, at 10 dB.**
3. **Thirty seconds of loud noise, and three minutes.** Report how many candidates the loosened
   single-mark gates pass, and how many stand after the pattern - **that second number is the
   unit's reason, and both noise tests must print nothing.**
4. **Two stations, 200 Hz apart, one at 24 dB and one at 10 dB.** The loud one prints; the report
   says whether the quiet one is now found as a second sequence.
5. **Every existing case reads exactly as at HEAD** - the calls at every speed, the Farnsworth
   cases, the speed change, the bursts, the hesitation, `TEST DE W1AW K`, `DE DE`, the lone and
   stray marks. **If any changes, say so with its text; do not force it.**
6. **The verdict row on a driven station**: `scopePitchHz` equals `PrintingHz` while printing, and
   `marks4s` is the count of marks that stood.

---

## 5. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 507 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 507.
- Patch-bump `Directory.Build.props`.
- **Append R112 to the rulings section of both `PHASE_PLAN.md` copies**, in the owner's words
  above. **Touch no checkbox.**
- `DECISIONS.md`, newest first, **HM-DEC-209**, headline *The pattern across marks is the gate;
  a single-mark test is proportional, never a decibel figure*, quoting him, naming that every
  synthetic case had been 20 dB or more over the noise and the gates set on those.

---

## 6. Report

Section 2, for the owner, in plain words:

- rebuild;
- **weaker stations that you hear clearly should now read** - a mark is judged by whether it fits
  the sender's pattern, not by how loud it is;
- noise still prints nothing, because noise cannot make the pattern;
- the light, the blocks and the row now describe the station being printed;
- **if a station you hear still reads nothing, section 3's table says at what strength the bench
  stops reading, and that is the number to report against.**

Section 1: what changed, file by file, every fraction and count with its reason, and that the
build and the app line are green. **Section 3: the strength table at the top - what reads at 8,
12, 16 and 24 dB, before and after - then the noise counts, then the cases.** Section 4: anything
left, a line each.
