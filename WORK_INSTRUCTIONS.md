# Work instruction 521 - a mark's pitch is where its own keying is, and a light says hold still

**Hand run. One unit, two tasks, commit per task, task 2 not dropped.**

**No test against a recording, a fixture, a floor or copied telemetry** (R96). A headless test
driving synthetic hops written in the test itself is allowed; nothing read from disk. Verify by
building `Hamlet.sln` with warnings as errors and running the app carry-forward line. **Every
existing reading case reads exactly as at HEAD**, or the report says what changed and why.

**Numbering.** This is unit 521, ruling HM-DEC-225. If taken, use the next free and say so.
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
  once for a multi-line commit. Scripts go in `.run-unit\unit521-<name>.sh`, not committed.
- Nothing that keys or transmits. Nothing written to the radio.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. Task 1 - a mark's pitch is where its own keying is

**Two findings, one cause.**

**Unit 520:** no per-mark gate loses a quiet station's marks beside a loud neighbour 200 Hz away.
**The apex climb does** - unit 496's walk to the louder neighbouring bin carries a 12 dB mark up
the neighbour's lobe to 825 Hz, where the key-up test sees a carrier that never keyed with it and
refuses the mark. With the carrier steady, **none** of the clean station's marks land at its own
625 Hz. Three fixes that followed this mark's keying split a lone station across its own lobe,
and were reverted.

**The owner, 2026-10-01:** *"The signal was just as clear one click off in either direction. Only
thing that changed was pitch."* A tone exactly on a 25 Hz bin centre reads; one click off it, the
same signal does not. **Unit 515's five-pitch test was 425, 500, 600, 700, 775 - every one a bin
centre.** W1AW reads because the button tunes to exactly 7.0475 and the pitch is exactly 600: a
bin centre. Ear-tuned stations land between bins.

**The same mechanism.** The climb decides a mark's pitch by comparing the levels of neighbouring
bins. Between two bin centres the two bins are equal and the climb flips hop to hop; beside a loud
neighbour it climbs the wrong way. **Level comparison between bins is the hidden pitch bias.**

**The rule.** **A mark's pitch is where its own keying is, not where the level is highest.** A
tone's energy across the bins is a lobe of known shape - the window's own - centred on the tone.
**Fit that lobe to the hops of this mark**: the three bins round the peak, over the mark's own
duration, and the fitted centre is the mark's pitch, to a fraction of a bin. A loud neighbour has a
different keying and a different lobe centre; it does not key with this mark, so it does not
pull this mark's fit. A tone between bin centres gets its true centre instead of a coin flip.

- The lobe's shape comes from the window the detector already uses; say which and its width.
- The fit is over the mark's own hops only - the hops that keyed with it.
- The 496 walk retires for pitch. **Say what else still read the apex bin** - the fit's lobe
  (517), the narrowness probe's centre (514), the pattern gate's "within one bin" - and that each
  now reads the fitted centre.
- A mark's `Hz` becomes a fraction of a bin; the pattern gate's pitch agreement is on that
  figure.

**Watch it fail first**, synthetic hops written in the test:

1. **The bin-grid sweep, first.** A clean 20 WPM sender at 24 dB at **600, 606, 612, 618 and
   625 Hz** - within one bin - no filter. For each: marks stood, shape score, text. **Red today
   if any between-centre pitch reads worse than 600.** Green when the table is flat. **This is
   the test the week never had.**
2. **Unit 520's case 1 at 200, 150 and 100 Hz** - the clean 12 dB sender beside a 24 dB carrier.
   Red today: `NOAM■HIV5■`, the carrier's reading. Green: the clean sender reads whole, its marks
   at its own pitch; report how many land at 625.
3. **The same with the carrier steady** - 520's probe that found none at 625. Green when the
   clean sender's marks are at 625.
4. **A lone clean sender stays whole** - the case that broke 520's three attempts: no duplicates
   across its own lobe. The fists, whole.
5. **Every existing case reads exactly as at HEAD**: every speed, both Farnsworth cases, the speed
   change, the fists, the bursts, the hesitation, `TEST DE W1AW K`, `DE DE`, the lone and stray
   marks, both noise tests, the five pitches, the drifting station, the quiet dit and dah, the
   strength table with the fit, the strong bulletin identical with the fit on and off, unit 520's
   switch cases.

## 3. Task 2 - a green light says hold still

**The owner, 2026-10-01:** *"I want a green light whenever the first shape is being detected so
that I know to hold on that frequency and not adjust, because you're not hearing it."*

**Why.** Five marks must stand before a sequence exists, and unit 520 adds one word gap before
the first letter prints. On a hand sender at 24 WPM that is several seconds of nothing on the
screen while the shape side is working. The owner reads the silence as "not hearing it" and
moves, and the station he was on never gets its five marks.

**The light**, on the CW tab beside the scope, in words as well as colour (§0.6):

- **dark, `listening`** - no candidate mark in the last two seconds;
- **amber, `shape forming · 3 of 5`** - candidate marks are arriving at one pitch and a sequence
  is building but has not stood; the count fills in as marks arrive;
- **green, `shape found · hold here`** - a sequence stands; letters follow within a word gap;
- **green, `reading`** - a sender is being printed.

The hover says: *"Green means Hamlet has the shape of a station here. Hold the frequency; the
first letters print after one word gap."* The light is fed from the pattern gate's sequences
and the reader's printed sender - the same sources as the row - at the scope's tick rate.

**Nothing else moves** (R101). The row gains `shapeLight` with the word shown.

**Watch it fail first**, headless on the live path: a clean call - dark before the first
candidate; amber with the count climbing to 4; green `shape found` at the fifth mark and before
the first letter; green `reading` from the first letter; dark two seconds after the last mark.
Then loud noise: never green.

---

## 4. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 521 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 521.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first, **HM-DEC-225**, headline *A mark's pitch is where its own keying
  is; a light says hold still*, naming the bin-centre finding and unit 520's climb finding as one
  cause, and quoting the owner on the light.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling.

---

## 5. Report

Section 2, for the owner, in plain words:

- rebuild;
- **a station reads the same one click either way** - its pitch is now found from its own
  keying, not from which 25 Hz bin happens to be louder;
- **a station next to a louder one is no longer pulled onto the loud one and thrown away**;
- **the light**: dark while listening, amber while a shape forms with a count, **green means hold
  the frequency** - letters follow within a word;
- nothing about letters changed.

Section 1: what changed, file by file, per task; the lobe shape used and every place that now
reads the fitted centre; and that the build and the app line are green. **Section 3: the
bin-grid sweep table first, then the neighbour cases, then the light's sequence, then the
existing cases.** Section 4: anything left, a line each.
