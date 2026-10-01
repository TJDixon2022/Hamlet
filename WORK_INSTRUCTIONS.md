# Work instruction 522 - shape first, pitch as a result, and a gauge that fills

**Hand run. One unit, three tasks, commit per task. Task 1 is the unit; tasks 2 and 3 ride on it
and are not dropped.**

**No test against a recording, a fixture, a floor or copied telemetry** (R96). A headless test
driving synthetic hops written in the test itself is allowed; nothing read from disk. Verify by
building `Hamlet.sln` with warnings as errors and running the app carry-forward line. **Every
existing reading case reads exactly as at HEAD**, or the report says what changed and why.

**Numbering.** This is unit 522, ruling HM-DEC-226. If taken, use the next free and say so.
**Unit 518 runs after this.**

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwSequenceShape.cs
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
  once for a multi-line commit. Scripts go in `.run-unit\unit522-<name>.sh`, not committed.
- Nothing that keys or transmits. Nothing written to the radio.
- **Tag HEAD `before-shape-first` before any engine change**, pushed. The per-bin front end stays
  reachable by name.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. The analysis, and the owner's ruling

**Where pitch still runs the show.** Everything in `CwEnvelopeDetector` is measured per 25 Hz bin.
A bar forms in a bin; its flatness, edges and key-up are judged in that bin; the climb picks a
bin; the narrowness probe compares bins; the fit (517) is told which bin; the pattern gate groups
by "within one bin." **Every bin decision is a pitch decision**, and the week's unexplained faults
all live there: a neighbour 200 Hz away steals a station's marks (520, 521); a station reads at
one click and not the next (the owner, 2026-10-01); a dah beside a steady carrier makes no
candidate at all (521); a sequence with a shape score of **0.06** went green on the light
(2026-10-01 18:20) because it passed every per-bin gate. The per-bin gates ask *"is this bin
noisy?"*, not *"is this a rectangle?"*.

**The owner's ruling, R117, 2026-10-01:**

> *"You're still not focusing 100% on shape. You're trying to interpret the noise instead of
> creating the shape patterns. Shape is everything, 100%."* And: *"We lose the weakest stations,
> but pitch becomes largely irrelevant."* **Pitch is an output, never a decision.**

---

## 3. Task 1 - shape first, pitch as a result

**The rule: time first; frequency only to tell overlapping rectangles apart.**

1. **The rectangle is found in time, across the whole passband.** Sum the energy of every bin in
   the passband into one trace per hop - the oscilloscope the owner asked for on 2026-09-28 -
   and **fit rectangles to it**, with unit 517's fit: start, end, height, scored by the share of
   the stretch's variance the rectangle explains, a ratio, never dB. A keyed tone is a rectangle
   there at any pitch. **No bin is chosen. Nothing can be one click off.**
2. **Pitch is a result.** During a fitted rectangle, the energy's centroid across the bins is
   the mark's pitch, to a fraction of a bin. **No climb. No comparison of neighbouring bins. The
   496 apex walk retires.**
3. **Overlapping rectangles are split in frequency, and only then.** Where the whole-band trace
   shows a shape that is not one rectangle - two senders keying at once - look at the bins over
   that stretch: two energy blobs, two centroids, two marks, each fitted on its own blob. A loud
   carrier and a quiet station have different **time** edges; that is what separates them, never
   a level comparison.
4. **The weakest get a second pass.** Where the whole-band fit finds a rectangle's time span but
   scores it low, **fit unit 517's lobe over that span at the centroid** - full sensitivity, now
   that the time is known. A mark passes on either fit.
5. **The per-hop tests retire as gates.** Flatness (R93), edges (497), narrowness (498, 514),
   key-up and promptness (492), the wander check: **none decides whether a stretch is a mark.**
   They may stay as descriptors a mark carries; say which were kept and for what. The fit's
   score and the pattern gate decide.
6. **The pattern gate is unchanged in rule.** It is already pure shape - one height, two lengths
   at 3:1, gaps at the sender's own, consistency, five to stand - and now its "within one bin"
   is "within one bin of the centroid."

**The costs, stated and accepted.** Summing the passband raises the noise floor by about
10·log10(bins), 13 dB for 500 Hz of 25 Hz bins; fitting over a dit's length instead of judging a
5 ms hop buys back about 10 dB at 20 WPM; the lobe pass covers the rest. **Report the floor
before and after on the strength table** - the owner expects a few dB at the weakest and no
worse on a clear station.

**Watch it fail first**, synthetic hops written in the test:

1. **The neighbour**: unit 520's clean 12 dB sender beside a 24 dB carrier keyed at random, at
   200, 150 and 100 Hz. **Red today**: `NOAM■HIV5■`. Green: the clean sender reads whole.
2. **The steady carrier**: 521's single dah at 625 Hz beside a steady 24 dB carrier. **Red today:
   no candidate.** Green: one mark, 180 ms, at 625.
3. **One click either way**: a clean sender at 24 dB at 600, 610, 650, 690, 700, 750 Hz through
   the 500 Hz filter on 600 - and the same with a 100 Hz step of the dial modelled as the whole
   signal shifting by 100 Hz mid-call. Every row reads whole. Report the table.
4. **The 0.06 sequence**: thirty seconds of loud noise; **no sequence scores above 0.2, and the
   light is never green.**
5. **The strength table**: the call at 8, 10, 12, 16, 24 dB, before and after, with marks stood
   and text. Report the floor.
6. **Every existing case reads as at HEAD** - every speed, both Farnsworth cases, the speed
   change, the fists, the bursts, the hesitation, `TEST DE W1AW K`, `DE DE`, the lone and stray
   marks, both noise tests, the two stations, the five pitches, the drifting station, the quiet
   dit and dah, the strong bulletin, unit 520's switch cases, the bin-grid sweep. **Where one
   changes, say so with its text; nothing is forced.**

## 4. Task 2 - the light reads the score

**The owner's rows, 2026-10-01 18:20:** the light went `shape found · hold here` and then
`reading` on a sequence with shape score **0.06 to 0.08** and a meter median of 6 ms - noise.
Unit 520 measured noise's best at 0.173.

**The rule:** green needs a standing sequence **and a shape score above 0.2** - clear of
everything noise has produced. Below that the light stays amber with its count. The hover says
so. `shapeLight` on the row is unchanged in form.

**Watch it fail first**: a driven standing sequence scoring 0.08 - red today, the light is green;
green when it stays amber. The clean call still goes green at the fifth mark.

## 5. Task 3 - the gauge

**The owner, 2026-10-01:** *"Not a countdown, but an indicator that starts off low and as you get
more and more sure, fills in. So I know how it's going."*

**The light becomes a gauge**, same place, same width (R101): one horizontal bar that fills from
left to right, with its words beside it.

- **Empty**, `listening`: no candidate in two seconds.
- **Filling, first stretch**, `shape forming`: marks toward five, as the count already runs - a
  fifth of the bar per mark.
- **Filling, second stretch**, `shape found · hold here`: from the fifth mark, the bar fills with
  the sequence's shape score - 0.2 is where it crosses into the second stretch, 1.0 is full. **A
  clean station fills fast; a rough one slower; both get there.**
- **Full**, `reading`: a sender is printing.
- **Colour follows the fill** - slate, amber, green - and the words carry it (§0.6).

A sequence that never clears 0.2 sits at the boundary with amber words and never says hold here.
The hover: *"Fills as Hamlet grows sure it has a station here. Past the mark, hold the frequency."*
`shapeLight` on the row gains the fill as a number.

**Watch it fail first**, headless: the clean call fills 0.2 → 0.4 → 0.6 → 0.8 across the first
four marks, crosses at the fifth, climbs with the score, reads full from the first letter; loud
noise never crosses.

---

## 6. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 522 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 522.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- **Append R117 to the rulings section of both `PHASE_PLAN.md` copies**, in the owner's words.
  **Touch no checkbox.**
- `DECISIONS.md`, newest first, **HM-DEC-226**, headline *Shape first: the rectangle is found in
  time across the passband and pitch is its centroid; the per-bin gates retire; the light reads
  the score and fills*, naming the tag `before-shape-first` and the four faults it answers.

---

## 7. Report

Section 2, for the owner, in plain words:

- rebuild;
- **a station reads the same at any pitch and one click either way**, because no bin is chosen
  any more - the rectangle is found in time and its pitch is where the energy was;
- **a station beside a louder one is told apart by when it keys, not how loud it is**;
- **the gauge** fills as Hamlet grows sure; past the mark, hold the frequency; it never fills on
  noise;
- **the weakest stations may read a little less far** - the floor before and after is in section
  3, and that is the number to report against.

Section 1: what changed, file by file; what each retired per-hop test became; the costs
measured; and that the build and the app line are green. **Section 3: the neighbour and steady-
carrier cases first, then the click table, then the strength table before and after, then the
gauge's sequence, then the existing cases.** Section 4: anything left, a line each.
