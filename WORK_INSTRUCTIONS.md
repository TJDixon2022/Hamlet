# Work instruction 486 - the screen tells the truth, and the preamp stops fighting

**Hand run. One unit.** Three things the owner reported at the radio with build 1.13.172.
**No test against a recording, a fixture, a floor or copied telemetry** (R96). Build, run the
app carry-forward line, and stop. The owner's report at the radio is the test.

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

- **R96: no test against recorded audio, no fixture, no floor, no copied telemetry.** A headless
  test driving synthetic hops written in the test itself is allowed; nothing read from disk.
  Verify by building `Hamlet.sln` with warnings as errors and running the app carry-forward line.
- Take `SESSION.lock` through `tools\arbiter\lock.bat take`, release it at the end. Write nothing
  to `RUN_LEDGER.md`. Touch nothing under `tools\arbiter\`. Tick nothing in `PHASE_PLAN.md`.
- One `dotnet test` invocation per line, filtered, with a `timeout`. Never background and poll.
  The app line loses names to the dispatcher loop; re-run once, count neither way.
- Apostrophes in quoted heredocs break; `;`, `rm` and `git rm` are refused; Python cannot run
  here; `-m` more than once for a multi-line commit. Scripts go in `.run-unit\unit486-<name>.sh`.
- **Nothing that keys or transmits.** Change three of §4 writes a receive setting to the radio,
  which §0.2 allows and §12.4 governs: no value is chosen by guess, and the report names every
  byte that changes.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. What the owner saw, 2026-09-28, on build 1.13.172

Two screenshots of the CW tab, both with the scope's blocks drawing correctly - flat tops, a
short block and a long one labelled `242 ms`, which is unit 485's work landing.

1. **The panel says `no keying · mixing 531 Hz`, and the terminal below reads
   `DE ES E EEE5 SEEEE E`.** Letters arriving while the panel says nothing is there.
2. **The panel says `tone 675 Hz · mixing 536 Hz`,** and a letter `E` floats with no block under
   it, while the terminal reads a wall of `E`s, `T`s and fragments.

**Why, from unit 485's own report.** The gate it built judges a character by **when its audio was
heard**, not by what the screen says now: *"It is judged by when the audio was heard, not by when
it settles, because the settled pass runs seconds behind."* So characters from a stretch when the
gate was open keep landing in the terminal for seconds after the detector has let go, under a
panel that already says no keying. **The gate is working as built and not as the owner meant.**

**And the pitch.** `tone 675 · mixing 536` is the detector finding a station at 675 Hz while the
decoder reads 139 Hz away. Unit 477's task 2 was to make the tracker take the detector's pitch
within a hop; **it was never built.** That is why the terminal fills with single-element noise:
the decoder is reading an empty bin beside the station.

---

## 3. Change one - the screen tells the truth

**Nothing reaches the terminal, the leading edge or the scope while the panel says no keying.**
The test is the screen at that moment, not the audio clock.

- When the detector's keying is false, the decoder emits nothing to any surface. **Characters
  still in flight from before are dropped, not flushed.**
- When keying goes true, emission resumes. Nothing held from before the silence is let out.
- The panel's words and what the terminal shows change together, in the same update.

**Watch it fail first**, headless, with synthetic hops written in the test: a keyed call, then
silence. Red while any character reaches a surface after keying goes false; green when none does.

**Say in the report what is lost:** the tail of an over, whose last letters settle after the
detector lets go. That is the cost of the owner's rule and he has ruled it.

## 4. Change two - the decoder listens where the detector hears

**The tracker mixes at the detector's pitch, within a hop of the detector finding it.** This is
unit 477's task 2, never built.

- When `CwEnvelopeDetector` reports keying at a pitch, `CwToneTracker` mixes at that pitch on the
  next hop. It does not wait for the survey or the meter.
- While the detector holds a station through its gaps (unit 485's hold), the pitch holds with it.
- When the detector lets go, the tracker behaves as it does today.

**Watch it fail first**, headless: a driven detector reporting keying at 675 Hz while the tracker
holds 536. Red while the tracker stays at 536; green when it moves within a hop.

**The decoder's own decisions are not changed** - not the lattice, not the unit estimator, not the
emission gate. Only where it is pointed.

## 5. Change three - the preamp stops fighting

The owner: *"The system puts preamp into mode 1 for data - fine - but does not restore it in CW to
off and worse, keeps putting it at 1 when I manually set it off."*

**Three faults, and the third is his ruling, given here:**

1. **Coming back to CW does not restore what CW wants.** Find what the CW and the data receive
   conditions in `data\bands\mode-receiver-conditions.json` ask for on 20 m, and why the preamp
   stays at 1 after data has set it. Name it in the report.
2. **His hand does not win.** Unit 419 built HM-DEC-056's rule - a value the operator sets himself
   is not overwritten by a later tune-in of the same mode. It is not holding for the preamp. Find
   why and fix it, so that once he sets the preamp off by hand it stays off until he changes it or
   the radio is power-cycled.
3. **His ruling, R98:** *"I still hate the preamp crap."* **The CW receive condition asks for the
   preamp off.** This overrides the manual-derived value of HM-DEC-176 for CW only: the manual
   quotes Icom's sensitivity figures with preamp 1 across HF, and the owner prefers it off. The
   condition's text says so, cites his ruling, and keeps the manual's reasoning as history.
   **The data-mode conditions are not changed** - they may still ask for preamp 1 - so returning
   to CW must actually set it off, which is fault 1's job.

**No value is guessed** (§12.4). The report tables, for CW and for each data mode the conditions
file speaks for: what is asked, what is written, and whether the operator's own change survives a
later tune-in.

---

## 6. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 486 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 486.
- Patch-bump `Directory.Build.props`.
- **Append R98 to the rulings section of both `PHASE_PLAN.md` copies** - the preamp off in CW, in
  the owner's words. **Touch no checkbox.**
- `DECISIONS.md`, newest first, HM-DEC-191, headline *The preamp is off in CW, and the operator's
  hand holds*, quoting him, naming that it overrides HM-DEC-176 for CW only.

---

## 7. Report

Section 2, for the owner, in plain words:

- rebuild;
- quiet band: empty panel, empty terminal, and they stay empty;
- a station keying: blocks, letters over them, and the terminal filling at the same time - and
  when it stops, both stop together;
- the *tone* and *mixing* numbers should now be the same number;
- tune into CW: the preamp goes off; set it off yourself in data or CW and it stays off.

Section 1: what changed, file by file, and that the build and the app line are green.
Section 4: anything left, a line each.
