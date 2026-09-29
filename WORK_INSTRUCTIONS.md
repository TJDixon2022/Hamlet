# Work instruction 491 - the bars survive the noise beside them

**Hand run. One unit.** Unit 490 built the reader the owner asked for and it works: on a clean
call the new path reads it whole, and on a call with bursts in every gap **not one burst became a
letter.** What is left is upstream: the detector loses the station's own marks where a burst or a
second station lands near them. This unit fixes that, and only that.

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
  here; `-m` more than once for a multi-line commit. Scripts go in `.run-unit\unit491-<name>.sh`
  and are not committed.
- Nothing that keys or transmits. Nothing written to the radio.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. What unit 490 proved, and the one thing in the way

**The owner's standing rule, R104, 2026-09-28:**

> *"We need to characterize what a signal looks like in noise and then use that characterization
> to identify data worth decoding. If we don't see anything in the noise, we don't even bother
> with the decoder. We focus on looking for those bars of amplitude with the duration that
> indicate a dot or a dash. We got to get that right."*

**Unit 490 got the reader right.** Its three cases, from its own report:

| case | sent | the run reader read |
|---|---|---|
| clean call, one pitch, one level | `CQ CQ DE N0CALL N0CALL K` | **the same, whole** |
| the call with bursts in every gap | `CQ CQ DE N0CALL N0CALL K` | `CQ CT A K EE N0CAAEI D N0CALL K` |
| the call at 625 Hz plus another station at 825 Hz | both | the call only, never the answer |

**And the reader is not what is wrong.** In case 2, **every one of the 65 marks its letters were
read from was the call's own**; not one of the bursts became a letter. In case 3 it printed one
station and took in none of the other's marks, where printing every run took in 21.

**The fault is one line in the detector, and unit 490 named it.** A burst landing in a gap lights
the station's own bin. The gap then fails the detector's check that bars must stand clear of the
wander in their gaps, so **the bars on either side of that gap are never paired, and the
station's own marks are never called at all.** Measured on case 2: **none of the first `CQ`'s four
marks were called.** The wrong letters in case 2 and 3 are not misreadings - they are the
station's marks going missing.

Two red tests are waiting for exactly this, left red on purpose by unit 490:
`TheCallReadsWholeThroughTheBlips` and `TheStationPrintedReadsWhole`.

---

## 3. The change - a noisy gap does not disqualify its bars

In `CwEnvelopeDetector`, the pairing check that bars must stand clear of their gaps' wander is
**the thing that loses the station.** A bar is a bar because of what the bar is: a flat top, held
at one level, for a dit's length or longer. **What happened in the silence next to it does not
make it less of a bar.**

- **A bar is judged on itself**: its own flatness, its own length, its own level. It is not
  disqualified by the level or the wander of the gap beside it.
- **A gap is still a gap** - the run reader needs its length to place letters and words - but a
  noisy gap is a gap that was noisy, not a reason to throw away the marks around it.
- **Two bars still pair** when they agree with each other, as they do today: within the flatness
  tolerance of each other, both at least a dit long. **That agreement is between the bars, not
  between a bar and its gap.**
- **Do not loosen the flatness tolerance, the shortest-bar length, or the pairing agreement.**
  The fix is removing a disqualification, not widening a bar.

**Name in the report the exact check that was removed or narrowed, by file and line, and what it
was for.** If it guards against something real - a slowly rising carrier read as one long bar, say
- then keep that guard and narrow it to the case it was written for, and say which.

**Watch it fail first.** The two reds above are the test:

- `TheCallReadsWholeThroughTheBlips`: the call with bursts 8 dB below it, 40 ms long, at 550 to
  675 Hz, in every gap. **Red today because the call's own marks are not called.** Green when
  every mark of the call is called and the reader reads the call whole.
- `TheStationPrintedReadsWhole`: the call plus a second station 200 Hz away. Green when the
  printed station's letters are read from its own marks and it reads whole.

**And prove noise alone still makes nothing**: a third case, loud noise with no station, asserting
the reader prints nothing. **Red if the removal lets noise pair into runs.** If it does, the guard
was doing real work and the report says so instead of forcing the other two green.

## 4. If there is time - the panel says the pitch it is reading

The panel reads *decoding at N Hz* from the old path's mixing pitch, which no longer describes
what the terminal shows: the run reader reads at the printed sender's own pitch. **Make it the
printed sender's pitch**, and say *no station* when none is printed.

**If this cannot be done cleanly and quickly, skip it**, commit §3, and say so in section 4.

---

## 5. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 491 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 491.
- Patch-bump `Directory.Build.props`.
- **Append R104 to the rulings section of both `PHASE_PLAN.md` copies**, in the owner's words
  above. **Touch no checkbox.**
- `DECISIONS.md`, newest first, **HM-DEC-196**, headline *A bar is judged on itself, not on the
  noise in the gap beside it*, naming unit 490's measurement - none of the first CQ's four marks
  called - as the reason.

---

## 6. Report

Section 2, for the owner, in plain words:

- rebuild;
- a station whose gaps have noise in them should now read whole, where before the letters beside
  the noise went missing;
- noise with no station still prints nothing;
- two stations at once still read as one.

Section 1: what changed, file by file, **the exact check removed or narrowed and what it was
for**, and that the build and the app line are green. **Section 3: the three cases' text, sent
beside read, and the count of the station's marks called before and after.** Section 4: anything
left, a line each.
