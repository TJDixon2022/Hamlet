# Work instruction 485 - no detection, no letters

**Hand run. One unit.** Three changes the owner asked for at the radio. **No test against a
recording, a fixture, a floor or copied telemetry** (R96). Build, run the app carry-forward line,
and stop. The owner's report at the radio is the test.

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

- **R96: no test against recorded audio, no fixture, no floor, no copied telemetry.** Verify by
  building `Hamlet.sln` with warnings as errors and running the app carry-forward line so nothing
  that worked breaks. A headless test driving synthetic hops written in the test itself is
  allowed where this instruction asks for one; nothing read from disk.
- Take `SESSION.lock` through `tools\arbiter\lock.bat take`, release it at the end. Write nothing
  to `RUN_LEDGER.md`. Touch nothing under `tools\arbiter\`. Tick nothing in `PHASE_PLAN.md`.
- One `dotnet test` invocation per line, filtered, with a `timeout`. Never background and poll.
  The app line loses names to the dispatcher loop; re-run once, count neither way.
- Apostrophes in quoted heredocs break; `;`, `rm` and `git rm` are refused; Python cannot run
  here; `-m` more than once for a multi-line commit. Scripts go in `.run-unit\unit485-<name>.sh`.
- Nothing that keys or transmits. Nothing written to the radio.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. What the owner reported, 2026-09-28

Watching the CW tab on a live band, he sees three different pictures in sequence: **wavy lines**
while nothing is keying, then **bars for a short time** when something is found, then **floating
letters with nothing under them.** And in the terminal, a continuous stream - `I EE IEE EE E`,
`EIES I E F HIH` - on a band where he hears nothing.

**Why, from the tree.** The decoder and the detector are two things that do not talk to each
other. `CwProbabilisticDecoder` has run continuously since long before the detector existed:
it mixes at whatever pitch the tracker holds and emits whatever spells a letter, whether or not
anything is there. `CwEnvelopeDetector` was added to watch, and **has never been wired to gate
it** - that is criterion 12.4, never built. So the scope's bars stop when the detector loses the
signal, and the letters keep coming from noise. **Floating letters with no bars beneath them are
the decoder inventing characters while the detector says nothing is there.**

**His ruling, R97:** *"If it has no detector why are there letters."* No detection, no letters.

---

## 3. The three changes

### One - the scope draws bars only

`CwScopeControl` draws **no level trace, ever.** It draws:

- **a filled block for every mark the detector calls**, on a time axis, its width the mark's
  duration - a short block for a dit, a long one for a dah, with flat tops;
- **nothing between marks**;
- **the letter above the group of blocks it came from**, as unit 480 built it - **and a letter
  draws only if there are blocks beneath it**;
- the *tone · mixing* words, unchanged;
- **when the detector says no keying: an empty panel.** No line, no noise, no letters.

Remove the trace from the frame and from the drawing. Keep the hover texts, reworded for what is
now drawn.

### Two - the detector holds a station through the gaps

The detector drops keying between characters, which is why the picture flickers between three
states. Once it has found bars at a pitch, it **stays on that pitch and keeps reporting keying
through the gaps** until the tone is genuinely gone.

- **Holding:** after keying is found at a pitch, keying stays true while marks keep arriving at
  that pitch, and through gaps between them.
- **Releasing:** keying goes false when no mark has arrived at that pitch for a stated hold time.
  **One second, as a named constant**, with a remark saying it is the longest gap in ordinary
  sending - a word gap at slow speed - and that it is not fitted to any recording. The author may
  choose a different figure and must say why in the same terms.
- While holding, the pitch does not wander: it stays where the marks are.

**Watch it fail first** with synthetic hops written in the test: a keyed tone with ordinary
character and word gaps. Red when keying drops during a gap; green when it holds across the gaps
and releases after the tone stops.

### Three - no detection, no letters

**The decoder emits nothing while the detector says no keying.** Wire the detector's keying
verdict to the decode path:

- while keying is false, the decoder emits no character - no letter, no placeholder, nothing
  reaches the transcript or the scope;
- while keying is true, the decoder runs as it does today, mixed where the tracker has it;
- when keying goes from false to true, the decoder starts fresh: the transcript continues, but
  nothing held from before the silence is settled into it.

**The decoder's own decisions are not changed** - not the lattice, not the unit estimator, not
the emission gate. Only whether it is allowed to emit at all.

**Watch it fail first** with synthetic hops in the test: silence, then a keyed tone, then
silence. Red when characters are emitted during the silences; green when none are.

### And - the top of the window

The owner's layout is **screen 1: the neighborhood panel on the left, the map on the right.**
That arrangement holds on every path - a frequency chosen in the app, a frequency the radio
announces, and any order of frequency and mode. **Make the top row's columns depend on the window
width alone**, never on the mode, the frequency, or how much text the neighborhood panel carries;
make the panel's text fit its space rather than grow it. Unit 389's sun-map rule stays as it is.
The sentence *"Your General license covers Morse here"* must not appear on a data-block
frequency, on any path.

---

## 4. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 485 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 485.
- Patch-bump `Directory.Build.props`.
- **Append R97 to the rulings section of both `PHASE_PLAN.md` copies** - *no detection, no
  letters*, in the owner's words. **Touch no checkbox.**
- `DECISIONS.md`, newest first, HM-DEC-190, headline *The decoder emits nothing while the detector
  says no keying*, quoting him, and naming that the two have never been wired together.

---

## 5. Report

Section 2, for the owner, in plain words:

- rebuild;
- on the CW tab with nobody keying: an empty panel and an empty terminal;
- when somebody keys: blocks appear at the mark durations, letters above them, and the terminal
  fills only then;
- the top of the window does not move when you turn the dial into the data block.

Section 1: what changed, file by file, and that the build and the app line are green.
Section 4: anything left, a line each.
