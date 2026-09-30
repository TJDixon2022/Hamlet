# Work instruction 502 - one shape, one score

**Hand run. One unit, after 501.** Seven independent tests each let some chaos through. A keyed
tone is one shape. This unit judges a mark by that shape as a whole.

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

- **Unit 501 must be in the tree.** If its Farnsworth test is absent, stop at task 0 and say so.
- Take `SESSION.lock` through `tools\arbiter\lock.bat take`, release it at the end. Write nothing
  to `RUN_LEDGER.md`. Touch nothing under `tools\arbiter\`. Tick nothing in `PHASE_PLAN.md`.
- One `dotnet test` invocation per line, filtered, with a `timeout`. Never background and poll.
  The app line loses names to the dispatcher loop; re-run once, count neither way.
- Apostrophes in quoted heredocs break; `;`, `rm` and `git rm` are refused; Python cannot run
  here; `-m` more than once for a multi-line commit. Scripts go in `.run-unit\unit502-<name>.sh`
  and are not committed.
- Nothing that keys or transmits. Nothing written to the radio.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. The owner's principle

**His words, 2026-09-30, R110:**

> *"Shape is the key to us getting really good CW. Noise is chaos. It can be anything. And you've
> been randomly trying to extract order from that chaos. But we know that CW is not any order.
> It's a particular shape and size and width. And we need to focus just on that."*

**What the code has, and what it lacks.** A bar becomes a mark by passing a row of independent
yes-or-no tests: it stands clear of its gaps (the wander check), it is a dit or longer (25 ms), its
top is flat (R93), it has edges (unit 497, 6 dB within four hops), and it is narrow (unit 498,
6 dB above the bins 300 Hz either side). **Each test lets through the noise that happens to pass
it.** A spike passes the edge test; a flat wobble passes the flatness test; a broadband thump
long enough passes the length test. What survives all five - 146 of 1452 noise bars in unit 498's
count, about 5 a second - is noise that happened to pass each one narrowly.

**A keyed tone is one shape**, and a real mark does not pass each test narrowly. It passes every
test by a wide margin at once: its top is flat to a fraction of a decibel, its edges are a few
milliseconds, it stands 15 to 30 dB above the band beside it, and it is exactly a dit or exactly
a dah. **Noise that squeaks past five gates does not look like that.** One score that measures
how far a bar sits inside the shape - not whether it crossed each line - separates the two.

---

## 3. First, the crash unit 501 found

`CwEnvelopeDetector.cs` line 1160 walks a mark back to where its tone rose and can step to hop −1
in the first seconds of audio, where `Level(-1)` throws `IndexOutOfRangeException`. Unit 501 hit it
on seed 5035 and moved its test to seed 5036 rather than touch the detector out of scope. **Guard
it here**: the walk stops at hop zero. One line. Then restore that test to seed 5035 and show it
passes. **This is the owner's go-ahead.**

## 3b. Second, which gate is killing W1AW - measured, not guessed

The owner's verdict rows on W1AW, 2026-09-30 12:09 UTC, CW, 7.0475: **meter at 725 Hz, dit 90 ms,
score 0.39** - W1AW's 18 WPM letters, plainly - **bars say keying, and `marks4s` is 0.** Zero
marks in four seconds on a strong, clean, correctly tuned station. It was 6 to 15 on stations
earlier in the week. **One of the mark gates added since is turning away every real mark.**

**Measure it before building anything else:**

- A synthetic 18 WPM call through the detector, **with each mark gate switched off in turn**:
  unit 492's three mark rules, unit 497's edges (`MarksNeedEdges`), unit 498's narrowness
  (`MarksNeedNarrowness`), and the pairing. For each: how many of the 65 marks are handed out.
- **Do the same with the passband set to the owner's: `CwPitch` 600, `FilterBandwidth` 500.**
  Unit 498's narrowness wants 6 dB above the bins 300 Hz either side, and in a 500 Hz passband
  there is room to read one side at most. **Report what the narrowness test reads on a 725 Hz
  tone in a 500 Hz filter centred on 600.**
- **Name the gate**, and fix it: if it is narrowness, the probe distance follows the passband -
  outside the lobe but inside the filter - and reads whichever side is available; if it is
  another, say what and why.
- **Report the table** in section 3, above the noise table.

**If no gate turns away the synthetic marks, say so** - then the fault is in what the app hands
the detector on the air, and the report names what differs between the test's audio and the
tab's.

## 3c. Third, the reader's gaps do not collapse mid-sender

**The owner's screen, 2026-09-30, a hand-sent QSO on 40 m:** `NICELY INTO MK TOYOTA PRIUS BOTH WITH
AND WI` - twenty letters of clean English - and then `N 1 3 L K A N N N I 2 M / 4 4`, every mark its
own letter. The bulletin did the same: `QST DE W1AW ... PRACTICE AT 5 7.5 10 13` and then
`I C M T S T E I E 2 N C D B Y`. **The reader reads the sender well, then loses its gap measurement
and spaces every letter.**

**Unit 501's cluster walk re-measures the letter gap from the sender's recent gaps between runs.**
One odd gap - a sender pausing to think, a burst of noise splitting a letter, a missed dit that
turns one gap into two - can move the lowest cluster, and once the letter gap is wrong every gap
inside a letter reads as a gap between letters.

**The rule:** once a sender's letter gap has been measured on real letters, **one gap does not move
it.** A sustained change over several letters does - a sender genuinely speeding up or slowing
down. A single gap that does not fit the measurement is read against the measurement that stands,
not used to re-measure it.

- **How many letters make a sustained change, and how far a gap may sit from the measurement before
  it is odd rather than new, are the author's**, stated with their reasons, derived from what a
  hand on a key does - a sender's own gaps scatter by tens of percent - and not from a recording.
- **The 10-to-20 WPM case must still follow the change.** That is the difference between a sustained
  change and one odd gap, and the report says where the line is.

**Watch it fail first**, synthetic: a 20 WPM sender sending `NICELY INTO MK TOYOTA PRIUS` with one
gap in the middle stretched to a word gap and a half, then continuing. **Red today** - report what it
reads before and that it reads whole after. Then the same with a dit dropped from one letter.

## 3a. The change - a shape score

In `CwEnvelopeDetector`, every completed bar gets **one score, from all of its properties
together**, and it becomes a mark only above a threshold on that score.

**The properties, each as a distance from the ideal rather than a pass or fail:**

- **Flatness** - how far the top wanders from its own mean, in dB, over the whole top. An ideal
  mark wanders a fraction of a decibel.
- **Edges** - how many hops the rise and the fall each take, from the gap level to the top and
  back. An ideal mark takes one or two, the window's own spread.
- **Narrowness** - how far the bar's bin stands above the band either side, in dB. An ideal mark
  stands 15 dB or more.
- **Contrast** - how far the top stands above the gaps either side of it, in dB.
- **Length** - how close the bar's length is to a dit or to a dah of the sender it belongs to, if
  a sender is known; how close to any plausible dit if not. A bar that is neither is far from the
  shape.

**How they combine is the author's**, stated in the report with its reason. **The reason must be
about the shape of a keyed tone, not about a result.** A product of per-property scores, each from
0 to 1, is the obvious choice: a bar that is perfect on four and bad on one is bad. **Do not weight
them by looking at what makes the tests pass.**

**The threshold is the author's**, derived from what a real keyed mark scores on the synthetic
cases - **the lowest score any real mark of the clean call earns is the floor the threshold sits
under**, with a margin the report states. It is not set by looking at what turns noise away.

**The five existing tests stay in the tree** and stay on: they are cheap and they are the floor.
The score is a sixth condition, on bars that passed all five. **Nothing is loosened.**

**The mark carries its score**, so the run reader, the scope and the verdict row can see it. The
scope's hover on a block says its score.

---

## 4. What to measure

**Watch it fail first**, with synthetic hops written in the test:

1. **Thirty seconds of loud noise.** Unit 498's table, extended: passing the older tests, with
   edges, narrow, **and above the shape threshold**. That last number is the unit's reason.
   Report the marks handed out per second before and after; unit 498 measured 5.
2. **The distribution of scores** on the clean call's 65 marks, and on the noise bars that passed
   all five tests: the lowest real score, the highest noise score, and whether they overlap.
   **If they overlap, say so and by how much**, and do not move the threshold into the overlap.
3. **Every existing case reads exactly as it does at HEAD**: the calls at 5, 10, 18 and 35 WPM,
   the Farnsworth cases, the speed change, the bursts, the two stations, `TEST DE W1AW K`, the
   lone dit, the lone dah, the stray dit, the string of lone marks, both noise tests. **If a real
   mark is lost, the threshold is too high - say so with the count and set it under the real
   floor**, never to what makes a noise case pass.
4. **A weak station, 10 dB over the noise.** Report what it reads before and after, and the scores
   its marks earn. This is the case the score must not break: a weak mark is still the shape,
   smaller.

---

## 5. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 502 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 502.
- Patch-bump `Directory.Build.props`.
- **Append R110 to the rulings section of both `PHASE_PLAN.md` copies**, in the owner's words
  above. **Touch no checkbox.**
- `DECISIONS.md`, newest first, **HM-DEC-206**, headline *A mark is judged by its whole shape, not
  by crossing five lines*, quoting him.

---

## 6. Report

Section 2, for the owner, in plain words:

- rebuild;
- fewer false marks still: a mark now has to look like a keyed tone as a whole, not just clear
  five separate bars;
- a real station, strong or weak, is unaffected - its marks score far inside the shape;
- **if real letters go missing, that is the threshold, and section 3's score table names the
  floor.**

Section 1: what changed, file by file, how the score is built and why, the threshold and its
reason, and that the build and the app line are green. **Section 3: the noise table at the top,
then the score distributions, then the cases.** Section 4: anything left, a line each.
