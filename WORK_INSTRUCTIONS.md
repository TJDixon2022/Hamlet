# Work instruction 514 - any pitch in the filter

**Hand run. One unit, three tasks, commit per task.** Every station that has read well was near
600 Hz. Stations at 350, 500 and 700 keep ending with the detector on a different bin from the
station. Three causes, all measured tonight, all in this unit.

**No test against a recording, a fixture, a floor or copied telemetry** (R96). A headless test
driving synthetic hops written in the test itself is allowed; nothing read from disk. Verify by
building `Hamlet.sln` with warnings as errors and running the app carry-forward line. **Every
existing reading case reads exactly as at HEAD**, or the report says what changed and why.

**Numbering.** This is unit 514, ruling HM-DEC-218. If taken, use the next free and say so.

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
  once for a multi-line commit. Scripts go in `.run-unit\unit514-<name>.sh`, not committed.
- **Nothing that keys or transmits. Nothing written to the radio** - task 3 reads the filter
  width and says a sentence; it does not change the filter.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. What the rows showed tonight, 2026-09-30

**22:59, 7.0249.** Meter at **500 Hz, 49 ms dit, score 0.28** - a station. Detector: *no keying*,
no pitch. Reader: **printing at 600.** Two rows in a row. The reader had nobody, so `PrintingHz`
was stale; unit 507's `Follow(PrintingHz)` pointed the detector at the stale bin; the meter's
fresh find at 500 went nowhere.

**23:02, W1AW at 7.0475.** Swing **41.9 dB, 0 marks.** Four of five rows `printing None`. The
same bulletin read `APPEARS IN THE SEPTEMBER ISSUE OF QST ON PAGE 28` this morning, when the
reader happened to already be on 600 as the bulletin began. Tonight it was not, and read
`TESLWHLTT J26 IISUE OF QST ON AEA`.

**Across the week:** 350, 500, 550, 700 Hz stations all reached the reader with the detector on
another bin. 600 Hz stations read. **The owner:** *"It seems like CW out in the wild has
varieties of pitch, and you just aren't getting any of that."*

**Three causes:**

1. **The wire.** When the reader prints nobody, the detector has nothing live to follow and sits
   on a stale pitch.
2. **Narrowness reads the filter skirt.** Unit 498's test wants the mark's bin to stand above the
   bins **300 Hz either side**. The owner runs FIL2 at 500 Hz, centred on his 600 Hz pitch: the
   passband is about 350 to 850. A station at 500 has 150 Hz of band below it; the probe at 200
   reads the filter's skirt, not the band, and the comparison is wrong. **It works at 600 and
   fails off-centre**, which is exactly the pattern.
3. **The filter itself.** A station at 380 or 820 arrives attenuated before Hamlet hears it. Not
   fixable in code; the app should say so.

---

## 3. Task 1 - the wire: when the reader has nobody, follow the meter

In `MainWindowViewModel`'s scope tick, where the detector is told to `Follow(PrintingHz)`:

- **While the reader is printing a sender**, follow `PrintingHz`, as now.
- **While it is not**, follow the keying meter's best pitch **when the meter's reading says a
  station** - its verdict is keying, its score is at least its own `KeyingScore` bar and its median
  element is inside its own element bounds. A meter reading of score 0.06 with a 4 ms median is
  noise and is not followed.
- **When neither**, follow the radio's pointer, as before unit 507.
- **Never a stale `PrintingHz`.** A sender released a second ago is not a sender.

**Watch it fail first**, headless on the live path: the detector watching 600, a driven meter
reading keying at 500 with a 49 ms median and score 0.28, the reader printing nobody. **Red
today**: the detector stays at 600. Green when it moves to 500 within a tick. Then the same with
score 0.06 and a 4 ms median: it does not move.

## 4. Task 2 - narrowness reads the band the filter gives it

In `CwEnvelopeDetector`'s narrowness test:

- **The probe distance follows the passband**, not a fixed 300 Hz. The probe sits outside the
  tone's lobe - unit 490's peak walk knows the lobe's width - and **inside the filter's passband**,
  from the rig state's `CwPitch` and `FilterBandwidth`. Where the passband gives less than the
  lobe's width on one side, that side is not read.
- **A mark is judged on the side or sides that can be read.** One readable side is enough; the
  comparison is the same ratio unit 507 set (`NarrowShare`).
- **Where the passband is unknown**, the whole band, as now.

**Watch it fail first**, synthetic: the call at 20 WPM at **425, 500, 600, 700 and 775 Hz**, each
through a 500 Hz passband centred on 600. Report, for each pitch, how many of the 65 marks pass
narrowness and what reads, **before and after**. Red today at the off-centre pitches, or say so
with the counts. The two-station case and both noise tests must still hold; report the counts.

## 5. Task 3 - the app says when a station is at the filter's edge

On the CW tab, beside *tone N Hz heard*, when the printed sender's pitch is **within 75 Hz of the
passband's edge** as the rig state gives it: **`near the filter's edge - the radio is attenuating
it`**, in words, with a hover saying the filter's width and centre and that widening it or
retuning would help. **Nothing is written to the radio.** When the pitch is inside, nothing is
shown.

**Watch it fail first**, headless: a printed sender at 380 Hz with a 500 Hz filter on 600 shows
the sentence; one at 600 does not.

---

## 6. What must not change

`CwRunReader` (unit 513's clusters), the pattern gate, the scope, the terminal, the layout, the
tab-is-the-mode, the preamp, the buttons. **Every reading case as at HEAD, and both noise tests
print nothing.**

---

## 7. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 514 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 514.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first, **HM-DEC-218**, headline *Any pitch in the filter: the detector
  follows the meter when the reader has nobody, narrowness reads the band the filter gives it,
  and the app says when a station is at the edge*, naming tonight's rows.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling.

---

## 8. Report

Section 2, for the owner, in plain words:

- rebuild;
- **a station anywhere inside the filter should now be found and read**, not only ones near 600;
- when the reader has nobody, the detector goes where the meter hears a station instead of sitting
  on the last pitch it had;
- a station at the filter's edge is named as such, so you know it is the radio and not Hamlet;
- **if a station inside the filter still reads nothing, the pitch table in section 3 says which
  pitches the bench reads, and that is the number to report against.**

Section 1: what changed, file by file, per task, and that the build and the app line are green.
**Section 3: the pitch table at the top - marks passing and text at 425, 500, 600, 700, 775
before and after - then the wire cases, then the existing cases.** Section 4: anything left, a
line each.
