# Work instruction 490 - a character is a run of marks that agree

**Hand run. One unit.** The detector already measures each mark's pitch, amplitude and length.
The decoder has only ever been given length. This unit gives it all three and lets them define
where a character starts and stops.

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
  here; `-m` more than once for a multi-line commit. Scripts go in `.run-unit\unit490-<name>.sh`
  and are not committed.
- Nothing that keys or transmits. Nothing written to the radio.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. The owner's rule, and why it is new

**His words, 2026-09-28, R103:**

> *"The scrolling tutorial system seems to discover characters a lot more correctly. That's
> because we're going on frequency, amplitude, and duration. Those define a character. Those
> should be consistent. An E followed by a T, if it's a real person doing CW, they will have the
> same amplitude. They will have the same pitch or frequency. They'll have a different duration.
> A dot or a dash is the only thing that varies. Why aren't we using these things that we're
> already discovering?"*

**Because nothing ever connected them.** `CwProbabilisticDecoder` was built before the detector
existed. It reads one mixed audio stream, measures how long the key was down and up, and fits
letters to the timing. **It has never been handed a mark's pitch or a mark's amplitude - only its
length.** The detector measures all three per mark and hands the decoder nothing but a pitch to
mix at. Units 486 to 489 bolted gates onto the decoder's output; none of them gave it the
measurements.

**What the rule changes.** Today a character ends when the silence is long enough. Under his rule
**a character is a run of consecutive marks that agree on pitch and on amplitude**, and the gaps
inside it only say which letter it is. A mark that breaks the agreement is not part of that
character: it is another station, or noise.

**Why this is the right place.** The tutorial strip already groups marks exactly this way, and it
is the part the owner says is usually right. This unit makes the decoder read the same grouping
instead of guessing from timing alone.

---

## 3. The change

### One - the marks reach the decoder whole

`CwEnvelopeDetector` already calls blocks and keeps them (unit 487). **Each block carries its
pitch, its level in dB and its length in milliseconds**, and the decoder is given the list, not
just a pitch to mix at. Name in the report what the detector already exposes and what had to be
added.

### Two - a character is a run of marks that agree

A new path in the engine, beside the existing one, that reads characters from the block list:

- **A run** is consecutive blocks whose pitch is within one bin of each other and whose level is
  within a stated tolerance of the run's own mean.
- **The run ends** at a block that breaks either agreement, or at a gap longer than the run's own
  character gap.
- **The letter** comes from the lengths inside the run - short against long, split at the run's
  own geometric mean - and from the gaps between them.
- **A word gap** is a gap longer than the run's own word gap, measured from the run's own dit.
- **A block that breaks the agreement** starts a new run. It is never folded into the one before.

**The two tolerances - one bin of pitch, and the level tolerance in dB - are the author's**,
stated in the report with their reasons, and **derived from what a sender's own marks do**: a
human's keying holds one pitch and one level, and the wobble is the detector's own measurement
noise. **Neither is fitted to any recording, and neither is tuned after reading a result.**

### Three - the new path is what the terminal shows

The terminal and the scope both read the new path.

- **A letter appears only if the run that made it exists** - so the terminal and the scope agree
  by construction, not by a gate bolted on top. Unit 487's block rule and unit 486's keying gate
  become unnecessary for this path; **leave their switches as unit 489 set them and say so.**
- The old timing-only path stays in the tree, behind a switch, so it can be compared. **Nothing
  is deleted.**
- Unit 487's printed-stays-printed holds: nothing shown is ever removed.

### Four - what the decoder is not

**Do not touch the lattice, the speed grid, the unit estimator or the emission gate.** This unit
does not improve the existing decoder. It builds a second, simpler reader that uses the three
measurements the detector already makes, and points the screen at it.

**Watch it fail first**, headless, with synthetic hops written in the test, and print the text
each path reads:

1. **A clean call at one pitch and one level.** Both paths should read it. Say what each reads.
2. **The same call with noise blips between the letters**, at a different level and a scatter of
   pitches. The old path folds them in; the new path must drop them, because they break the
   agreement. **Red while the new path prints them.**
3. **Two stations at once**, 200 Hz apart, both keying. The old path interleaves them into
   nonsense. The new path must read them as two runs and print one of them - say which and why.

---

## 4. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 490 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 490.
- Patch-bump `Directory.Build.props`.
- **Append R103 to the rulings section of both `PHASE_PLAN.md` copies**, in the owner's words
  above. **Touch no checkbox.**
- `DECISIONS.md`, newest first, **HM-DEC-195**, headline *A character is a run of marks that agree
  on pitch and amplitude*, quoting him, and naming that the decoder had only ever been given
  duration.

---

## 5. Report

Section 2, for the owner, in plain words:

- rebuild;
- on a station: the letters in the terminal are the letters over the blocks, because both now come
  from the same run of marks;
- noise between letters no longer becomes letters, because it does not agree on pitch or level;
- two stations at once read as one station, not as a mixture.

Section 1: what changed, file by file, the two tolerances and their reasons, and that the build
and the app line are green. **Section 3: the three tests' text, old path beside new path.**
Section 4: anything left, a line each.
