# Work instruction 519 - shape picks the sender, and loudness picks nothing

**Hand run. One unit.** On every station the owner tuned to this morning, the meter found clean
55 to 63 ms marks - the shape was there and the code saw it - and the reader printed from a
different, louder, messier sequence. **Loudness is not shape.** This unit takes loudness out of
every choice the shape side makes.

**No test against a recording, a fixture, a floor or copied telemetry** (R96). A headless test
driving synthetic hops written in the test itself is allowed; nothing read from disk. Verify by
building `Hamlet.sln` with warnings as errors and running the app carry-forward line. **Every
existing reading case reads exactly as at HEAD**, or the report says what changed and why.

**Numbering.** This is unit 519, ruling HM-DEC-223. If taken, use the next free and say so.
**Unit 518 is not run until this has.**

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwPatternGate.cs
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
- Apostrophes in quoted heredocs break; `;` is refused; Python cannot run here; `-m` more than
  once for a multi-line commit. Scripts go in `.run-unit\unit519-<name>.sh`, not committed.
- Nothing that keys or transmits. Nothing written to the radio.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. The owner's ruling, and what the rows showed

**His words, 2026-10-01, R116:**

> *"I don't care what the pitch is. You should find the shape in the noise. It's there. It was
> audible. Let's defocus pitch and emphasize shape."* And: *"I want this to be so much shape that
> I'm shocked."*

**The rows, 14:24 to 14:26, three or four stations he tuned across, sixteen presses.** On every one
the keying meter found a real sender - **55 to 63 ms marks, score up to 0.49**. The pattern gate
stood a sequence. **And the one it stood was never the one with the clean marks**: the meter at
875, the detector's standing pitch at 600 and 650, the reader printing at 700, then chasing.
Twelve *idiot* presses. **The shape was found every time and thrown away for something louder.**

**Why.** Unit 515: *"PitchHz is that sequence's pitch, the loudest where several stand."* Unit 490:
*"one sender is printed: the first to make two runs, the one with the most marks if several
have."* And underneath, every per-mark score (502, 507) and the fit (517) carry the mark's level
as a term. **Loudness decides which sequence stands first, which is printed, and which pitch the
light and the scope show.** A station at the filter's edge, attenuated 10 dB, loses to leakage
and noise that happens to be louder in the passband. A loud carrier with no rhythm can out-rank a
quiet station with perfect rhythm.

**What an ear does.** It picks the one that *sounds like code* - the cleanest rhythm - and follows
it, whatever is louder beside it.

---

## 3. The change - a shape score for a sequence, and loudness in none of it

### One - every sequence gets a shape score

In `CwPatternGate`, every standing sequence carries **one score from the shape of its marks and
gaps together**, 0 to 1, built only from ratios:

- **rectangle quality**: the mean of its marks' per-mark shape scores (502, 507) **with the
  level term removed** - flatness, edges, narrowness, fit; **never contrast, never height**;
- **cluster tightness**: how tightly its dits cluster, how tightly its dahs cluster, and how far
  apart the two centres stand - a sender with a crisp 1:3 scores high, a fist scores lower, noise
  scores near nothing;
- **gap tightness**: the same for its element gaps and letter gaps;
- **consistency**: the share of its marks that fall inside its own clusters;
- **count**: how many marks have stood, saturating - five marks are evidence, fifty are proof,
  five hundred are no more proof than fifty.

**How they combine is the author's**, stated with its reason. **The reason must be about what a
keyed tone is, not about a result.** A product of per-property scores is the obvious choice, as
unit 502 chose: a sequence crisp on four and bad on one is bad.

### Two - loudness picks nothing

- **The sequence printed is the one with the highest shape score**, not the first to stand and not
  the one with the most marks. Unit 490's rule retires.
- **The reading's pitch, the light and the scope follow the printed sequence** - the one with the
  best shape - not the loudest. Unit 515's *"the loudest where several stand"* retires.
- **Once a sequence is being printed it is held** until it has been silent for its own word gap
  plus a dah, as unit 511 holds it. A better-shaped sequence arriving does not take the terminal
  mid-sentence; it takes it at the next silence. An ear does not switch stations mid-word.
- **The per-mark shape score loses its level term everywhere it is used as a score.** The level
  is still measured - the sequence's own level tolerance (490, 511) still groups marks - but
  **nothing ranks by it.** Say in the report every place level or contrast was a term in a score
  and what replaced it.
- **The verdict row** gains `shapeScore` for the printed sequence, and `sequencesStanding`, so
  the owner can see what was chosen over what.

### Three - what must not change

The per-mark gates' pass-or-fail rules (497, 498, 507), the fit's fill-only rule (517), the
reader's letter arithmetic, the terminal, the scope's drawing, the layout, the tab, the buttons.

---

## 4. What to measure

**Watch it fail first**, synthetic hops written in the test. **These are the cases that make the
rule, and they are all about a weaker station with the better shape:**

1. **A clean 20 WPM machine sender at 12 dB beside a loud 24 dB carrier that keys randomly** -
   marks of random length at random gaps, no rhythm. **The clean sender prints, whole.** Red
   today: the carrier out-ranks it.
2. **A clean 20 WPM sender at 10 dB through the filter at the passband's edge, beside a 20 dB
   fist scattered by a third in the passband's centre.** Both are real; the clean one has the
   better shape. **The clean one prints.** Report the two shape scores.
3. **The two-station case**, 24 dB at 625 and 10 dB at 825, both clean: the louder one prints
   because its shape is as good and it stood first - **and the report prints both scores**, and
   they are close.
4. **A sequence being printed is not taken mid-word** by a better one arriving; it is taken at the
   next silence. Report the switch time.
5. **Thirty seconds of loud noise, and three minutes: nothing prints**, and the report gives the
   highest shape score any noise sequence earned beside the lowest a real sender earned in cases
   1 to 3. **They must not overlap.**
6. **Every existing case reads exactly as at HEAD**: every speed, both Farnsworth cases, the speed
   change, the fists, the bursts, the hesitation, `TEST DE W1AW K`, `DE DE`, the lone and stray
   marks, the five pitches, the drifting station, the quiet dit and dah, the strength table with
   unit 517's fit, the strong bulletin identical with the fit on and off.

---

## 5. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 519 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 519.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- **Append R116 to the rulings section of both `PHASE_PLAN.md` copies**, in the owner's words.
  **Touch no checkbox.**
- `DECISIONS.md`, newest first, **HM-DEC-223**, headline *Shape picks the sender; loudness picks
  nothing*, quoting him, and naming the rules of 490 and 515 it retires and every score that lost
  its level term.

---

## 6. Report

Section 2, for the owner, in plain words:

- rebuild;
- **the station with the cleanest rhythm is the one that prints**, however loud its neighbours;
  a loud carrier or a louder sloppy fist no longer takes the terminal from it;
- once a station is being read, Hamlet stays on it until it pauses;
- the row now says the printed station's shape score and how many sequences were standing, so
  **when it picks wrong you can see what it chose over what**;
- nothing about letters changed.

Section 1: what changed, file by file; how the sequence score is built and why; every score that
had a level term and what replaced it; and that the build and the app line are green. **Section
3: cases 1 and 2 with both sequences' scores at the top, then the noise scores against the real
ones, then the existing cases.** Section 4: anything left, a line each.
