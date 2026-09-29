# Work instruction 489 - the decoder gets its ears back

**Hand run. One unit, and it is small on purpose.** The owner has a few hours of usable CW on
the air tonight. This unit is written to be finished inside them.

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
  here; `-m` more than once for a multi-line commit. Scripts go in `.run-unit\unit489-<name>.sh`
  and are not committed.
- Nothing that keys or transmits. Nothing written to the radio.
- **Time matters.** If a change in §4 cannot be made cleanly, do §3 alone, commit, and say so.
  §3 is the unit; §4 is what it should also have.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. What went wrong, and why this is a rollback

**A week ago W1AW on 7.0475 read mostly clean.** Tonight, on the same station at 38 dB swing,
score 0.55, dits of 71 ms - the strongest signal in the record - **zero characters.**

Three gates were put in front of the decoder in two days:

- **unit 486:** nothing is emitted unless the detector says keying now;
- **unit 487:** nothing is emitted unless every element of the character has a block the detector
  called;
- **unit 486 and 488:** the decoder mixes at the detector's pitch.

Each is right in principle. Together they hand the whole decode to a detector that **unit 488
measured calling the station's bars 50 Hz to one side, two hops in three** - at 625 Hz the
station's own bin reads gaps near −20 dB while the shoulders read −42, so the shoulders get
called and the watched pitch is 575 or 675. Unit 488's own table, on a synthetic 625 Hz station:

| the decoder fed | settled text | characters |
|---|---|---|
| sent | `CQ CQ DE N0CALL N0CALL K` | 20 |
| the detector's pitch, which is what the app does now | `RE    N  D   K` | 5 |
| the station's own 625 Hz | `RQ DEN0CAL 0L K` | 12 |

**So the decoder is being pointed at a shoulder and then refused for not matching blocks that are
not where it is listening.** The detector's pitch is not fit to steer the decoder yet.

**The owner's ruling, R102, 2026-09-28:** get last week's reading back tonight. The detector stops
steering the decoder and stops gating it. It keeps the scope, the blocks and the light, and it
keeps teaching, but it no longer decides what the decoder hears or what reaches the screen.

---

## 3. The rollback - three gates off

**One: the decoder mixes where it did before unit 486.** Remove the detector's pitch from the
mixing rungs. The order returns to the operator's lock, then the tracker. `CwDecoder.MixingHz`
stays and still reports where the decoder really mixes - it is the owner's window into this and it
was right to add.

**Two: the block rule is off.** Unit 487's third condition - the blocks under a character's span
must equal its elements - no longer decides whether a character is emitted.

**Three: the keying gate is off.** Unit 486's condition - keying must be true now, and the
character heard in the open stretch - no longer decides whether a character is emitted.

**Keep, all of it:**
- the scope, the blocks, the letters over the blocks, unit 485's hold, unit 487's
  printed-stays-printed, the one layout, the two verdict buttons and the verdict row;
- **the scope still draws only blocks and only letters that sit over blocks.** The screen's rule
  is unchanged; what changes is that the terminal is no longer bound by it;
- every constant, every remark, every test. **Nothing is deleted.**

**How to switch them off.** One named switch each, in the engine, defaulting **off**, with a
remark naming R102, unit 488's measurement, and that the switch exists so they can be turned back
on when the detector's pitch is fit. **Not a deletion, not a comment-out.** The tests that prove
each gate stay green by driving its switch on.

**Watch it fail first**, headless, with synthetic hops written in the test: a clean keyed call at
a pitch the detector calls 50 Hz off. Red while the call reads fewer characters than the same
decoder unbound; green when it reads what it read before unit 486.

## 4. If there is time - the panel stops lying about it

The panel says *tone N Hz · mixing N Hz*. With the rungs changed, `mixing` is the tracker's again
and `tone` is the detector's, and they will often differ. **Say so on the panel**: the words
become *tone N Hz heard · decoding at N Hz*, so the owner reads two numbers that are honestly two
different things rather than a disagreement that looks like a bug.

**If this cannot be done cleanly and quickly, skip it**, commit §3, and say so in section 4.

---

## 5. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 489 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 489.
- Patch-bump `Directory.Build.props`.
- **Append R102 to the rulings section of both `PHASE_PLAN.md` copies**, as a paragraph: the
  detector does not steer or gate the decoder until its pitch is fit; it keeps the scope, the
  blocks and the light; the switches exist to turn the gates back on. **Touch no checkbox.**
- `DECISIONS.md`, newest first, **HM-DEC-194**, headline *The detector stops steering the decoder
  until its pitch is fit*, naming unit 488's measurement as the reason and the switches as the
  route back.

---

## 6. Report

Section 2, for the owner, in plain words, and short:

- rebuild;
- W1AW and other strong stations should read about as they did a week ago, junk between the words
  included;
- the scope still shows blocks and letters over blocks, and still shows nothing when the detector
  hears nothing - **the scope and the terminal will now disagree, and that is the point: the
  terminal is free again and the scope shows what the detector can still only partly do**;
- nothing printed vanishes; the layout is unchanged; the preamp is unchanged.

Section 1: what changed, file by file, the three switch names, and that the build and the app line
are green. Section 3: the before-and-after character counts on the test's call. Section 4:
anything left, a line each.
