# Work instruction 497 - a mark has edges

**Hand run. One unit, after 496.** Three of the four things that make CW recognizable are being
used. The fourth - shape - is not, and it is where the false characters come from. **And one small
addition the owner asked for: a line and a dot saying what W1AW is doing next (§3a).**

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

- **Unit 496 must be in the tree first.** It makes a mark's pitch the station's own bin. If
  `ThatPitchIsTheStationsOwn` is still red at HEAD, stop at task 0 and say so: this unit's
  measurements would be taken on a shoulder.
- Take `SESSION.lock` through `tools\arbiter\lock.bat take`, release it at the end. Write nothing
  to `RUN_LEDGER.md`. Touch nothing under `tools\arbiter\`. Tick nothing in `PHASE_PLAN.md`.
- One `dotnet test` invocation per line, filtered, with a `timeout`. Never background and poll.
  The app line loses names to the dispatcher loop; re-run once, count neither way.
- Apostrophes in quoted heredocs break; `;`, `rm` and `git rm` are refused; Python cannot run
  here; `-m` more than once for a multi-line commit. Scripts go in `.run-unit\unit497-<name>.sh`
  and are not committed.
- Nothing that keys or transmits. Nothing written to the radio.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. The owner's philosophy, and the one part of it the code does not use

**His words, 2026-09-29, R107:**

> *"Our philosophy is CW is predictable signal within noise. It has a shape we recognize. It has a
> typical duration. It has a typical height. It has a shape. Are we using all of those factors to
> identify actual signal? Because I'm still seeing a lot of false characters."*

**Three of the four are in the code:**

- **Height** - a bar must rise clear of its gaps (`CwEnvelopeDetector`'s wander and pairing).
- **Duration** - at least `ShortestBarMs`, 25 ms, and a run's dahs at least 2 to 1 over its dits
  (units 490 and 492).
- **Consistency** - since unit 490, the marks of a character must agree on pitch and on level, and
  since unit 493 a run's marks must be keyed.

**Shape is not.** A bar's **top** is tested for flatness within the tolerance (R93). **Nothing
tests its edges.** A real keyed tone rises in a millisecond or two, holds, and falls in a
millisecond or two: a rectangle with soft corners. **Noise does not have edges** - it drifts up
across the threshold, wanders, and drifts back down. A noise burst that happens to sit flat enough
for long enough passes every test in the tree. **That is where the false characters come from.**

---

## 3. The change - a mark rises and falls like a key

A bar is a mark only if it has edges:

- **It rises fast.** From the level of the gap before it to the level of its own top, within a
  stated number of hops.
- **It falls fast.** From its top to the level of the gap after it, within the same.
- **A bar that drifts in or out over many hops is not a mark**, however flat its middle and
  however long it lasts.

**The rise and fall bound is the author's**, stated in the report with its reason, and derived
from **what a keyer does and what the detector's own window can see**: an IC-7300's CW rise time
is a few milliseconds, and the envelope is read in windows, so the bound cannot be shorter than a
window or two. **It is not fitted to any recording, and not tuned after reading a result.** Say in
the report what a real keyed edge measures in the synthetic tests and what the bound was set to.

**What must not change:**

- the flatness tolerance, the shortest bar, the pairing agreement, the wander check, unit 491's
  nearest-bar change, unit 492's three mark rules and three-window bound, unit 496's bin choice;
- the run reader's tolerances, its keyed-mark rule, its two-run rule;
- the light, the blocks, the letters over the blocks, the one layout, the preamp, the buttons.

**This is a new condition on calling a bar a mark, not a loosening or a re-weighting of any
existing one.**

**Watch it fail first**, with synthetic hops written in the test:

1. **Loud noise, thirty seconds, with the edge test off**: report how many bars pass every current
   test - height, duration, flatness - and **how many of those have edges**. That difference is
   the unit's reason, and it is the number to put at the top of the report.
2. **`ThreeMinutesOfNoiseReadNothing` and `NoiseAloneReadsNothing` stay green**, and report how
   many marks are handed out on noise before and after. Unit 492 measured about 50 a second.
3. **The clean call, the call with bursts, the two-station case, the lone dit and lone dah** -
   units 492, 493 and 496's cases. **Every one must read exactly as it does at HEAD.** If a real
   mark is lost, the bound is too tight: say so with the count and loosen it to what a real edge
   measures, not to what makes the test pass.
4. **A slow-risen tone**, new: a tone that fades up over 100 ms, holds 200 ms, fades down over
   100 ms. **It is not a mark.** That is QSB or a carrier coming up, not a key.

---

## 3a. Also - what W1AW is doing next

The owner, 2026-09-29: the hover keeps unit 494's text unchanged, **and one line under the button
says what is happening next, with a dot beside the button for whether a run is scheduled now.**

**The line, under the button:**

- while a Morse run is scheduled: **`Sending now: code bulletin, 18 WPM, until 8:00 PM`**;
- otherwise: **`Next: code practice, fast, 9:00 PM - in 22 minutes`**.

Name the kind (code practice or code bulletin), the speed, and the time in the operator's own
clock. **Use the word `scheduled` in the hover** and never claim W1AW is transmitting: the ARRL
excludes legal holidays and Hamlet does not know them.

**The dot, beside the button:** filled while a Morse run is scheduled, hollow otherwise, **with a
word beside it** - `scheduled` or `quiet` - so colour is never the sole carrier (§0.6). Its hover
is the same line.

**The schedule, from the ARRL, US Central, the same all year.** Only the Morse rows; the digital
and voice rows are not this button's business.

```
  8:00-9:00 AM    code practice   Tue Thu fast, Wed Fri slow
  3:00-4:00 PM    code practice   Mon Wed Fri fast, Tue Thu slow
  4:00-5:00 PM    code bulletin   daily
  6:00-7:00 PM    code practice   Tue Thu fast, Mon Wed Fri slow
  7:00-8:00 PM    code bulletin   daily
  9:00-10:00 PM   code practice   Mon Wed Fri fast, Tue Thu slow
  10:00-11:00 PM  code bulletin   daily
  slow 5 to 15 WPM, fast 10 to 35 WPM, bulletins 18 WPM
```

- **It lives in `data/bands/w1aw-morse.json`**, beside the frequencies, with the ARRL cited as its
  source - **never as a literal in a `.cs` or `.axaml` file.**
- **The conversion to the operator's clock is through the time zone database**, from named US
  Central. **Never a baked-in offset** - Central and Eastern both shift with daylight saving and a
  fixed offset is wrong half the year (§0.0).
- **A day with no Morse run left** reads `Next: code practice, slow, tomorrow 8:00 AM`.
- **The line and the dot update themselves** as the clock passes a boundary, without a press.
- **Neither may resize or move anything** (R101). The CW tab's text count is already over its
  ceiling - **report the new count**; it is not this unit's to fix.
- **Unit 494's hover text is unchanged.** Do not add the schedule to it.

**Watch it fail first:** with the clock driven to a time inside a bulletin, the line reads
`Sending now` with the kind, speed and end time, and the dot is filled and says `scheduled`; driven
to a quiet time, the line names the next run and the minutes to it, and the dot is hollow and says
`quiet`; driven past the last run of a day, the line names tomorrow's first. **The schedule is read
from the data file**, and no time in the test is computed with a fixed offset.

---

## 4. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 497 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 497.
- Patch-bump `Directory.Build.props`.
- **Append R107 to the rulings section of both `PHASE_PLAN.md` copies**, in the owner's words
  above. **Touch no checkbox.**
- `DECISIONS.md`, newest first, **HM-DEC-201**, headline *A mark rises and falls like a key; noise
  drifts*, naming that height, duration and consistency were already used and shape was not.

---

## 5. Report

Section 2, for the owner, in plain words:

- rebuild;
- fewer false letters: a noise burst that was flat and long enough to pass now has to rise and
  fall like a key, and noise does not;
- a real station is unaffected - its keyer's edges are far inside the bound;
- **if real letters go missing, that is the bound being too tight, and it is the one thing to
  report back.**

Section 1: what changed, file by file, the rise and fall bound and its reason, and that the build
and the app line are green. **Section 3: the noise counts from case 1 at the top - bars passing
every other test, and how many had edges - then the five cases' text before and after.**
Section 4: anything left, a line each. **Say in one line what the W1AW line and dot read at the
time of the run, and where the schedule lives.**
