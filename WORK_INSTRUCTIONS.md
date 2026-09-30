# Work instruction 513 - a fist is read by which cluster is nearer

**Hand run. One unit.** The detector and the pattern gate hand the reader a clean stream of marks
from a real station, and the reader sorts them into the wrong letters. The station is hand-sent
at 27 WPM. No case on the bench was ever both fast and human.

**No test against a recording, a fixture, a floor or copied telemetry** (R96). A headless test
driving synthetic hops written in the test itself is allowed; nothing read from disk. Verify by
building `Hamlet.sln` with warnings as errors and running the app carry-forward line. **The
owner's report at the radio is the test.**

**Numbering.** This is unit 513, ruling HM-DEC-217. If taken, use the next free and say so.

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
  The app line loses names to the dispatcher loop; re-run once, count neither way.
- Apostrophes in quoted heredocs break; `;` is refused; Python cannot run here; `-m` more than
  once for a multi-line commit. Scripts go in `.run-unit\unit513-<name>.sh`, not committed.
- Nothing that keys or transmits. Nothing written to the radio.
- **Only `CwRunReader` changes.** The detector, the pattern gate, the scope, the terminal, the
  layout, the tab, the buttons and the row are untouched.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. What the owner saw, and what the rows say

**2026-09-30 21:44 UTC, 7.0299, CW, a hand-sent station.** Six presses, all *agree*: pitch 500 Hz
found, **18 to 22 marks in four seconds, a 45 ms dit, score 0.3 to 0.4, swing 20 to 28 dB.** The
detector and the pattern gate handed the reader a clean stream of a real station at about 27
WPM. **The reader printed:**

```
T 25I E NSIE SE THAT AKSN S M N VTGNURING DAYTEMEHEEN N ENFBATELLITISSUREA KI
ENNICENTTKTHEE<BT> EA <BT> WELL TNXFER THE EKS RICKES CE TCHULA I TBVTATE DL OEVAE S
```

Real words survive - `THAT`, `WELL`, `TNX`, `THE`, `<BT>` - inside wrong letters. **Not a spacing
collapse, not noise. The marks are right and they are sorted into the wrong letters.**

**Why.** Every fast case on the bench is machine-sent: dahs exactly 3 dits, gaps exactly 1, 3
and 7. **A human at 27 WPM does not do that.** His dahs run 2.5 to 3.5 dits and his gaps wander;
unit 504 measured that a fist scattered by a third misreads on letter boundaries and said it was
not in any test. Two things in `CwRunReader` are brittle to a fist:

- **The dit-or-dah split is one hard line** at the geometric mean of the sender's short and long
  marks. A fist whose dits and dahs overlap in length has elements fall on the wrong side.
- **The gap boundaries are hard lines too** - √3 dits between element and letter, the measured
  word gap above. At 27 WPM the element gap is 45 ms and the letter gap 130; a fist's element gaps
  drift toward 80 and its letter gaps toward 100, and the √3 boundary at 78 ms lands inside both.

**The owner's ruling, R113:** a fist is read the way an ear reads it - **by which of the sender's
own clusters each mark or gap is nearer**, not by a fixed line.

---

## 3. The change - nearest cluster, not a hard line

In `CwRunReader`:

- **Elements.** The sender's marks are two clusters, dit and dah, each with a centre and a spread
  measured from the sender's own marks. **A mark is the kind whose centre it is nearer, in
  log-length.** Where the two clusters are wide enough to overlap, the mark is the kind whose
  cluster it is more likely from - the spread decides, not the midpoint.
- **Gaps.** The sender's gaps are three clusters - element, letter, word - each with a centre and
  a spread, from the sender's own gaps as units 500, 501 and 504 measure them. **A gap is the kind
  whose centre it is nearer, in log-length**, the same way.
- **The clusters follow the fist.** Centres and spreads are running measures over the sender's
  recent marks and gaps, as the dit already is, so a sender who tightens or loosens up is
  followed.
- **A machine sender reads exactly as now.** With clusters that do not overlap, nearest-centre and
  the geometric mean give the same answer. **Every existing case must read as at HEAD.**
- **The lone-letter banking, the pattern gate's rules and the sender's-own-dit rule (unit 511)**
  are unchanged; they read the element kinds this gives them.

**The spread and how it is measured are the author's**, stated in the report with their reasons,
derived from what a hand does - **not from any recording, and not tuned after a result.**

---

## 4. What to measure

**Watch it fail first**, with synthetic hops written in the test. **The fist is the point.** Build
a fist generator in the test: each element's length drawn from a spread around the ideal, each
gap likewise, with a stated scatter.

1. **`CQ CQ DE N0CALL N0CALL K` at 27 WPM with a fist scattered by 20%** - dahs 2.4 to 3.6 dits,
   gaps likewise. Report what it reads before and after. **Red today, or say so with the text.**
2. **The same at 30% scatter**, unit 504's case: `NICELY FTO ■TOYOK PRIUSBOTH■...` at HEAD.
   Report before and after.
3. **The same at 20% at 12 WPM and at 35 WPM.**
4. **A fist that tightens mid-transmission**, 30% scatter for ten letters then 10%.
5. **Every existing case reads exactly as at HEAD** - the calls at every speed, the Farnsworth
   cases, the speed change, the bursts, the hesitation, `TEST DE W1AW K`, `DE DE`, the lone and
   stray marks, both noise tests, the two-station case, the strength table. **If any changes, say
   so with its text; do not force it.**

**Report, for each fist case, the sender's cluster centres and spreads as measured** beside the
generator's true values.

---

## 5. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 513 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 513.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- **Append R113 to the rulings section of both `PHASE_PLAN.md` copies**: a fist is read by which
  of the sender's own clusters each mark or gap is nearer. **Touch no checkbox.**
- `DECISIONS.md`, newest first, **HM-DEC-217**, headline *A fist is read by the nearer cluster,
  not a hard line*, naming the 21:44 station and that no bench case had been both fast and
  human.

---

## 6. Report

Section 2, for the owner, in plain words:

- rebuild;
- **hand-sent stations at speed should read** - the reader now sorts each dit, dah and gap by which
  of the sender's own clusters it is nearer, the way an ear does with a rough fist;
- machine-sent and slow senders are unchanged;
- **if a fist still reads wrong, section 3's cluster table beside the generator's truth says
  whether the measurement or the sorting was at fault.**

Section 1: what changed in `CwRunReader`, the spread and its reason, and that the build and the
app line are green. **Section 3: the fist cases' text before and after, with the measured
clusters beside the true ones, then the existing cases.** Section 4: anything left, a line each.
