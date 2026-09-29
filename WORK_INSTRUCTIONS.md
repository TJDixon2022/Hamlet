# Work instruction 498 - a mark is narrow, and a lone letter waits to be confirmed

**Hand run. One unit.** Two changes: the shape test that is missing, and the suspicion the owner
wants applied to single-element letters.

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

- **Units 496 and 497 must be in the tree.** `ThatPitchIsTheStationsOwn` green and `MarksNeedEdges`
  present. If not, stop at task 0 and say so.
- Take `SESSION.lock` through `tools\arbiter\lock.bat take`, release it at the end. Write nothing
  to `RUN_LEDGER.md`. Touch nothing under `tools\arbiter\`. Tick nothing in `PHASE_PLAN.md`.
- One `dotnet test` invocation per line, filtered, with a `timeout`. Never background and poll.
  The app line loses names to the dispatcher loop; re-run once, count neither way.
- Apostrophes in quoted heredocs break; `;`, `rm` and `git rm` are refused; Python cannot run
  here; `-m` more than once for a multi-line commit. Scripts go in `.run-unit\unit498-<name>.sh`
  and are not committed.
- Nothing that keys or transmits. Nothing written to the radio.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. Why - the owner's words and unit 497's own numbers

**The owner, 2026-09-29, R108:**

> *"Basically, I want you banking potential words until you confirm them, especially T's and E's.
> Treat them with deep suspicion. I still think our shape isn't right. If we had our shape right,
> we wouldn't be seeing this. I don't think the T's and the E's are producing the shape."*

**He is right about the shape, and unit 497 measured it.** On thirty seconds of loud noise:

| | count |
|---|---|
| bars passing height, duration and flatness | 1452 |
| of those, passing unit 497's edge test | 1069 |
| turned away by the edge test | 383 |

**Three quarters of white noise still looks like a keyed mark.** Unit 497's edge test asks that the
level drop 6 dB within four hops either side - **which tests for a change, not for a rectangle.**
Noise that jumps satisfies it: a spike has fast edges.

**What a keyed mark is, and which parts the code checks:**

| the shape | checked? | where |
|---|---|---|
| rises above its gaps | yes | the wander check and pairing |
| lasts a dit or longer | yes | `ShortestBarMs`, 25 ms |
| its top holds one level | partly | the flatness tolerance, but over as few as two hops |
| a fast rise **and** a fast fall bracketing a long flat middle | partly | unit 497, either edge, no middle requirement |
| **its energy is in one bin** | **no** | **nothing checks it** |

**The last one is the strongest test available and it is absent.** A keyed tone is narrow: while it
is up it stands well above the bins either side of it. **Noise is broad** - the same level in the
neighbours, because noise is everywhere. A single test of narrowness would turn away most of what
the edge test lets through, and cost a real station nothing.

---

## 3. Change one - a mark is narrow

A completed bar is handed out as a mark only if, **while it is up, its own bin stands clear of the
bins beside it.**

- Measure over the mark's own hops: the mean level in the mark's bin, against the mean level in the
  bins a stated distance either side - **outside the tone's own lobe**, since unit 490 and 496
  established the lobe spans several bins and unit 496 folds it into the peak.
- **A bar whose neighbours are as loud as it is, is not a mark.** It is broadband noise that
  happened to be flat.
- **The depth and the distance are the author's**, stated in the report with their reasons, derived
  from the window's own shape - how far from the peak the lobe has fallen away - and from what a
  keyed tone's bandwidth is at the speeds in `CW_SPEC.md`. **Not fitted to any recording, and not
  tuned after reading a result.**

**Also tighten the flat middle**, which is the other half of the shape:

- **A mark's flat middle must be at least as long as the shortest bar**, not merely flat for two
  hops inside a longer run. Say what the flatness tolerance is measured over at HEAD and what it
  becomes.

**Do not loosen anything**: not the flatness tolerance's value, the shortest bar, the pairing, the
wander check, unit 491's nearest-bar change, unit 492's rules, unit 496's bin choice, or unit
497's edges. **These are two more conditions on calling a bar a mark.**

## 4. Change two - a lone letter waits to be confirmed

In `CwRunReader`, a **single-element letter - one mark, so `E` or `T` - is banked, not printed**,
until the sender confirms it.

- **Confirmed:** the same sender produces another letter, at the same pitch and level, within a
  stated number of character gaps. The banked letter is then released **in its own place**, so
  `TEST` reads `TEST` - its `T` and `E` are confirmed by the `S` and `T` behind them.
- **Not confirmed:** nothing follows from that sender inside the window. **The banked letter is
  dropped and never printed.**
- **Printed-stays-printed is not broken** (R100): a banked letter has not been printed, so dropping
  it removes nothing from the screen. **Nothing already on the screen may be taken back.**
- **A multi-element letter is not banked.** It prints as it does today.
- **The window is the author's**, stated with its reason, derived from the sender's own character
  gap - not from any recording.

**This replaces the rule unit 493 left**, where once a sender was printed its one-mark letters
printed with it. **That rule is why a string of `E`s trails after a sender has stopped sending:**
the sender was never released, so its stray marks kept printing. Say in the report what that rule
was and that it is gone.

**The scroll and the terminal stay identical** (R106): a banked letter appears in neither, and is
released to both together.

---

## 5. What to measure

**Watch it fail first**, with synthetic hops written in the test:

1. **Thirty seconds of loud noise.** Report the table from §2 with a third column: **of the 1069
   with edges, how many are narrow.** That number is the unit's reason.
2. **`ThreeMinutesOfNoiseReadNothing` and `NoiseAloneReadsNothing` stay green**, with the marks
   handed out per second before and after. Unit 497 measured 36 a second.
3. **The five cases read exactly as they do at HEAD**: the clean call, the call with bursts, the
   two-station case, the lone dit, the lone dah. **If a real mark or letter is lost, the bound is
   too tight - say so with the count and loosen it to what a real mark measures**, not to what
   makes a test pass.
4. **`TEST DE W1AW K`**, new: every letter prints, including the `T`, the `E` and the final `K`.
   **This is the case that proves the banking does not eat real text.**
5. **A lone `E` after a sender stops**, new: the call, then silence, then one stray mark at the
   sender's pitch. **Nothing prints for the stray.**

---

## 6. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 498 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 498.
- Patch-bump `Directory.Build.props`.
- **Append R108 to the rulings section of both `PHASE_PLAN.md` copies**, in the owner's words
  above. **Touch no checkbox.**
- `DECISIONS.md`, newest first, **HM-DEC-202**, headline *A mark is narrow, and a lone letter waits
  to be confirmed*, naming unit 497's 1069-of-1452 as the reason and that nothing had checked
  narrowness.

---

## 7. Report

Section 2, for the owner, in plain words:

- rebuild;
- **the stray `T`s and `E`s should be gone**: a single-element letter now has to be confirmed by
  another letter from the same sender, and a mark now has to be narrow as well as flat;
- `TEST` still reads `TEST`;
- a real station is unaffected - its marks are narrow and its letters confirm each other;
- **if real letters go missing, that is the one thing to report back.**

Section 1: what changed, file by file, the narrowness depth and distance with their reasons, the
confirmation window with its reason, and that the build and the app line are green. **Section 3:
the noise table with the third column at the top, then the five cases plus `TEST DE W1AW K` and
the stray `E`.** Section 4: anything left, a line each.
