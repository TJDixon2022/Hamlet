# Work instruction 516 - fit the rectangle, do not check it

**Hand run. One unit.** A weak dah is still a rectangle. The current shape tests look at it one
hop at a time and a weak one breaks into pieces. This unit fits the rectangle as a whole.

**No test against a recording, a fixture, a floor or copied telemetry** (R96). A headless test
driving synthetic hops written in the test itself is allowed; nothing read from disk. Verify by
building `Hamlet.sln` with warnings as errors and running the app carry-forward line. **Every
existing reading case reads exactly as at HEAD**, or the report says what changed and why.

**Numbering.** This is unit 516, ruling HM-DEC-220. If taken, use the next free and say so.

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwRunReader.cs
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
  once for a multi-line commit. Scripts go in `.run-unit\unit516-<name>.sh`, not committed.
- Nothing that keys or transmits. Nothing written to the radio.
- **Only `CwEnvelopeDetector` and what it hands the pattern gate change.** The reader, the gate's
  sequence rules, the scope, the terminal, the layout, the tab, the buttons, the row: untouched.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. The fault, measured, and the owner's rule

**2026-10-01, 7.0265, a Quebec station working Maine at 18 WPM.** Five *idiot* presses: at 01:17
nothing found, meter score 0.05 to 0.09, swing 15; at 01:18 the same station, same pitch,
reading at 65 ms dits, swing 20 to 25; at 01:18:26 marks drop to 6, swing 16; then reading again.
On 7.0361, a station the owner heard, swing 13 to 14, nothing. **Every idiot press tonight is the
same fault: a signal he hears clearly sitting at or below the detector's floor.** Unit 507
measured the floor at about 16 dB and said why: *"at 8 dB four of the call's dahs never become
bars. That is before any gate this unit touched."*

**Why the dahs break.** Every shape test on a bar - flatness (R93), edges (497), narrowness (498),
the shape score (502, 507) - **judges the rectangle one hop at a time**: is this hop within
tolerance of the top, did this hop fall fast enough. On a strong signal every hop is. On a weak
one, noise rides on the top, hops fall outside the tolerance, and the bar splits into pieces of
20 and 30 ms that are neither dit nor dah. The rectangle is still there; the per-hop test cannot
see it through the noise.

**Unit 510 tried a running mean over half a dit and made every row worse.** It blurred the edges
and then applied the per-hop tests to the blurred result. That is the wrong version of the right
idea.

**The owner's rule, R115, 2026-10-01:** *"Focus on shape. If you get the shape, the decode
comes."* A weak dah is a rectangle. **Fit the rectangle; do not check it.**

---

## 3. The change - a rectangle fit

In `CwEnvelopeDetector`, a second way for a stretch of hops in a bin to become a candidate mark,
beside the per-hop tests:

- **For a stretch of hops, find the rectangle that explains them best**: its start, its end, its
  height over the floor either side. Every hop in the stretch and in the gaps beside it votes;
  the top's level is the mean over the top, not any one hop.
- **Score it by how much better the rectangle explains the stretch than noise does** - the share
  of the stretch's variance the rectangle accounts for, a ratio from 0 to 1. **No decibel figure
  anywhere in it.** A weak dah fits a 180 ms rectangle well although no single hop is flat; noise
  fits no rectangle of any width.
- **The fit is tried at the sender's own lengths where a sender stands** - its dit and its dah,
  from the pattern gate - and at a sweep of plausible dit and dah lengths where none does.
- **The same rectangle in the lobe.** A tone's rectangle appears in the bins either side of its
  peak at proportional heights; noise in adjacent bins is uncorrelated. **Fit across the lobe** -
  the peak bin and its neighbours together, each at its share of the height - so a weak mark has
  three bins of evidence instead of one. The lobe's width is what unit 490's peak walk already
  knows.
- **A stretch whose fit scores above a threshold is a candidate mark**, with the fitted start,
  end, height and pitch, and it goes to the pattern gate exactly as a per-hop candidate does. The
  gate's rules are unchanged; it still needs five agreeing marks to stand anything.
- **The threshold is the author's**, stated with its reason, **set under the lowest score a real
  mark of the clean call earns at 24 dB**, with a stated margin - never moved to make a weak case
  pass or a noise case fail. **Report the distribution**: real marks' fit scores at 24, 16, 12
  and 8 dB, and noise stretches' fit scores, and whether they overlap.

**A strong signal is unchanged.** Where the per-hop tests already make a candidate, the fit adds
nothing. The fit exists for the stretches the per-hop tests broke.

**Do not loosen** any per-hop test, the pattern gate, or the reader.

---

## 4. What to measure

**Watch it fail first**, synthetic hops written in the test:

1. **The strength table**, unit 507's: the call at 8, 12, 16 and 24 dB over the noise. **Report, for
   each, how many of the 65 marks the per-hop tests find, how many the fit adds, and what reads,
   before and after.** The 8 and 12 dB rows are the unit's reason. HEAD: 8 dB
   `N ET A EI A DE N0CALL NTJCE AEL K`, 12 dB `CT A CQ DE N0CALL N0CALL K`.
2. **The fit-score distributions** from §3, as a table: real marks at each strength, noise.
3. **Both noise tests print nothing**, with the counts of candidates the fit adds on noise.
4. **A fading station**: the call at 24 dB fading to 10 dB and back over its length, as the Quebec
   station did. Report what reads.
5. **Every existing case reads exactly as at HEAD**: the calls at every speed, both Farnsworth
   cases, the speed change, the fists, the bursts, the hesitation, `TEST DE W1AW K`, `DE DE`, the
   lone and stray marks, the two stations, the five pitches, the drifting station, unit 511's
   quiet dit and dah. **If any changes, say so with its text; do not force it.**

---

## 5. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 516 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 516.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- **Append R115 to the rulings section of both `PHASE_PLAN.md` copies**, in the owner's words.
  **Touch no checkbox.**
- `DECISIONS.md`, newest first, **HM-DEC-220**, headline *A weak mark is a rectangle fitted as a
  whole, not checked hop by hop*, naming tonight's five presses and unit 510's wrong version.

---

## 6. Report

Section 2, for the owner, in plain words:

- rebuild;
- **weaker stations should now read** - a dah that noise used to break into pieces is found as one
  rectangle because the whole shape is fitted at once, across the tone's own bins;
- strong stations and noise are unchanged;
- **the bench's floor before and after, in dB, is the number to report against.**

Section 1: what changed in the detector, how the fit is scored, the threshold and its reason, and
that the build and the app line are green. **Section 3: the strength table at the top, then the
score distributions, then the fading station, then the existing cases.** Section 4: anything
left, a line each.
