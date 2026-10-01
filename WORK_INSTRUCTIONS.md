# Work instruction 517 - the fit fills gaps, the spaces come from the shape

**Hand run. One unit, two tasks, commit per task, drop from the back.**

**No test against a recording, a fixture, a floor or copied telemetry** (R96). A headless test
driving synthetic hops written in the test itself is allowed; nothing read from disk. Verify by
building `Hamlet.sln` with warnings as errors and running the app carry-forward line. **Every
existing reading case reads exactly as at HEAD**, or the report says what changed and why.

**Numbering.** This is unit 517, ruling HM-DEC-221. If taken, use the next free and say so.

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
  once for a multi-line commit. Scripts go in `.run-unit\unit517-<name>.sh`, not committed.
- Nothing that keys or transmits. Nothing written to the radio.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. Task 1 - the rectangle fit, done right

**What happened to unit 516.** It built the rectangle fit the owner asked for - a weak dah fitted
as a whole rather than checked hop by hop - and on the air, W1AW at 18 WPM lost every dah:
`S I SEEIIE IIIS SIIE I5I`, dits only. Built at 515 the same bulletin read whole, so 516 was
reverted (`8a85e6ab`, `6bf777cf`). Its change is at `f917267a`, reachable.

**Why.** The fit was tried everywhere, including across stretches the per-hop tests had already
made a mark of. On a strong dah it found a dit-sized rectangle on part of the top and handed the
gate a second mark overlapping the first; the gate took the shorter. **The idea was right; the fit
competed with marks instead of filling where they were missing.**

**The rule it lacked:** **the fit only fills stretches no per-hop mark covers.** A fit candidate
whose span overlaps a mark the per-hop tests found is dropped, always. The fit exists for the
pieces noise broke - the 20 and 30 ms fragments at 8 dB that are neither dit nor dah - and
nowhere else.

**Build it again, from `f917267a`, with that rule**, and everything else of 516's design as it
was: the fit's score as a ratio of variance explained, no dB; tried at the sender's own lengths
where one stands and a sweep otherwise; fitted across the lobe; the threshold set under the lowest
real-mark score at 24 dB with a stated margin, never moved for a weak or a noise case.

**Watch it fail first, and this is the test 516 did not have:**

1. **A strong machine-sent bulletin at 18 WPM, 24 dB, with dahs** - `THE QUICK BROWN FOX JUMPS
   OVER THE LAZY DOG 0123456789` - **reads identically with the fit on and off.** Every dah
   intact. **Red on 516's `f917267a`, green here.** This is the gate against what happened on the
   air.
2. **The strength table**, unit 507's: the call at 8, 12, 16 and 24 dB, per-hop marks found, fit
   marks added, text before and after. **8 and 12 dB are the reason.**
3. **The fading station**: the call at 24 dB fading to 10 and back.
4. **The score distributions**: real marks at each strength, noise, and whether they overlap.
5. **Both noise tests print nothing**; count the candidates the fit adds on noise.
6. **Every existing case reads exactly as at HEAD.**

## 3. Task 2 - the spaces come from the shape

**The owner's screen, 2026-10-01, 7.0265, a Quebec station working Maine at 18 WPM:**
`I IE KI1MMRDEVE2JDLGNGNAGNDROMMAURO ,UREEI INSEAOPJEANJEANESQTHQUEBEC,HW?IAMMMRDEVE2JDEIK`.
That is `KI1MM DE VE2JD ... NAME IS JEAN JEAN ... QTH QUEBEC, HW? KI1MM DE VE2JD K` with the
letters mostly right and **almost no spaces**. Marks flowing, pitch found, seven of twelve
presses *agree*.

**Why.** The word gap is decided in `CwRunReader` by unit 513's clusters: letter gaps against
word gaps, nearer centre. A hand sender whose word gaps run short - many ops barely pause between
words - never forms a word cluster separate from the letter cluster, so every gap is a letter gap
and the text runs together. Unit 513's own report: *"letter and word gaps can merge on a
tightening fist."*

**The owner's direction, the 80/20 path:** the shape side hands the reader labelled elements; the
reader is a Morse table. **Gap kind is a property of the sequence, decided in `CwPatternGate`,
not in the reader.**

- **The gate labels every gap in a standing sequence** element, letter or word, from the
  sequence's own gaps: the element gaps are the ones inside letters; the letter gaps are the next
  cluster up; **the word gap is the next cluster up from that, or, where the sequence shows only
  two clusters, any gap past 1.5 times the letter centre.** Half-way to Morse's 7:3 in ratio, and
  a sender who sends 4-dit word gaps still gets his spaces.
- **The reader receives the labels** and places letters and spaces from them. Its own gap
  arithmetic from units 500, 501, 504 and 513 **moves into the gate** or retires; say which.
  **After this task `CwRunReader` has no gap constant in it.**
- **A machine sender reads exactly as now.** Its three clusters are plain and the labels match.

**Watch it fail first**, synthetic hops written in the test:

1. **A hand sender at 18 WPM whose word gaps are 4 dits**, not 7, sending `KI1MM DE VE2JD NAME IS
   JEAN QTH QUEBEC HW`. **Red today** - runs together - green with the spaces.
2. The same at 5 dits, and at a proper 7.
3. **Every existing case reads exactly as at HEAD** - the calls at every speed, both Farnsworth
   cases, the speed change, the fists, the bursts, the hesitation, `TEST DE W1AW K`, `DE DE`, the
   lone and stray marks, both noise tests, the two stations, the five pitches, the drifting
   station, the quiet dit and dah, the strength table.

**Drop candidate:** this task, with the reason stated.

---

## 4. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 517 - STEP 12`, one paragraph naming what landed.
- `PHASE_STATUS.md`, both copies: names 517.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first, **HM-DEC-221**, headline *The fit fills only what the per-hop
  tests left; the gap kinds come from the shape*, naming 516's revert and why, and the Quebec
  station.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling.

---

## 5. Report

Section 2, for the owner, in plain words:

- rebuild;
- **weaker stations should read further down**, and a strong one reads exactly as it did - the
  fit now only fills what the per-hop tests broke, never touches a mark they found;
- **hand senders who barely pause between words get their spaces**, because the word gap is now
  read from the sender's own gaps in the shape stage, not guessed in the reader;
- **the bench floor before and after, and the W1AW-style bulletin reading identically, are the
  two numbers to report against.**

Section 1: what changed, file by file, per task, and that the build and the app line are green.
**Section 3: the bulletin-identical check first, then the strength table, then the word-gap
cases, then the existing cases.** Section 4: anything left, a line each, and any task dropped.
