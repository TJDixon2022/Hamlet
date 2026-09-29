# Work instruction 492 - a mark is a mark the moment it ends

**Hand run. One unit.** Unit 491 measured that every mark of a keyed call is already found -
right pitch, right length, flat top - and that the fault is delivery: a mark can be handed out
up to a second late, because a pair waits on a second of quiet history. This unit delivers a
mark when it ends.

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
  here; `-m` more than once for a multi-line commit. Scripts go in `.run-unit\unit492-<name>.sh`
  and are not committed.
- Nothing that keys or transmits. Nothing written to the radio.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. What unit 491 measured, and the owner's rule

**Unit 491 refused unit 490's diagnosis and measured the truth.** On the call with bursts in
every gap, **every one of its 65 marks is called** - the same as on a clean call. Nothing is lost.
What happens instead: **a mark is handed out late.** The `L`'s third dit ran 10.520 to 10.565 s
and was handed out at 11.000 s, **435 ms after it ended**, because the check that a paired bar
stands clear of its gap's wander is measured over the bin's last second, and a burst anywhere in
that second holds the pair back. By the time the dit arrived, the run reader had closed the `L`'s
run, and the `L` came out as `E` and `I`.

**The owner's rule, R105, 2026-09-28:**

> *"What we need to do is get the dot and dash by frequency, period, and flatness. When we have a
> dot and a dash isolated, decoding will be easy."*

**So the isolation is done and the delivery is not.** A bar with a flat top, held at one level,
for a dit's length or longer, at a consistent pitch, **is a dot or a dash the moment it ends.**
It does not need a partner, and it does not need a second of quiet history, to be what it already
is.

**Where the noise guard goes instead.** Unit 491 proved the guard does real work: removed
outright, thirty seconds of loud noise alone called 61 marks and printed `NETMITT`. **But a lone
mark never becomes a letter anyway** - unit 490's reader needs a run of marks that agree on pitch
and level, and a sender is printed only after two runs. **The guard belongs where the letters are
made, not where the marks are found.**

---

## 3. The change

### One - a mark is delivered when it ends

In `CwEnvelopeDetector`, a completed bar becomes a `CwMark` **on the hop it ends**, judged on
itself:

- its top is flat within the flatness tolerance (R93, contrast-following, unchanged);
- it is at least the shortest bar long (25 ms, unchanged);
- its pitch is the peak of its own lobe, as unit 490 already finds it;
- it carries its level, its contrast, its length and its times, as unit 490 already gives it.

**It is not held for a partner, and not held for the wander check's one-second window.** The
pairing machinery stays in the tree and keeps driving the detector's own `keying` verdict, the
light and the scope's picture - **do not change what those show.** What changes is that
`MarksSince` hands out a mark as soon as the bar is complete.

**The wander check is not deleted.** It stays exactly where it is for the keying verdict. It
simply no longer gates delivery.

### Two - the guard moves to where letters are made

In `CwRunReader`, a run must earn its letters:

- **a run of one mark makes no letter.** A lone bar, however clean, is not a character.
- **a sender is printed only after two runs**, as unit 490 already has it.
- **add one more, and state it as a rule with its reason:** a run's marks must agree on pitch
  *and* level *and* fall in a plausible dit-and-dah pattern - the run's own short and long marks
  at least 2 to 1, as unit 490 has it. A run whose marks are all the same length and which never
  makes a second run is not a sender.

**The test of whether this is enough is `NoiseAloneReadsNothing`**, which unit 491 wrote. Loud
noise, no station, must print nothing. **If delivering marks early lets noise through the reader,
say so with the count and do not force it green** - then the guard is needed further forward and
the report says where.

### Three - what must not change

- **Do not touch** the flatness tolerance, the shortest bar, the pitch-peak walk, the pairing
  agreement, or unit 491's nearest-bar-at-its-own-level change.
- **Do not touch** the light, the scope's drawing, the layout, the preamp, printed-stays-printed,
  or the verdict row.
- **Do not touch** the lattice, the speed grid, the unit estimator or the emission gate.
- **Unit 489's three switches stay off.**

**Watch it fail first**, with synthetic hops written in the test, and report each case's text and
the worst delivery delay in milliseconds:

1. **`TheCallReadsWholeThroughTheBlips`** - unit 490's call with bursts in every gap. **Red today
   because marks arrive after their run has closed.** Green when the call reads whole.
2. **`NoiseAloneReadsNothing`** - loud noise, no station. Must stay green.
3. **`TheStationPrintedReadsWhole`** - the call plus a second station 200 Hz away. Report what it
   reads; green if it reads the printed station whole.
4. **A lone dit and a lone dah**, each alone in silence. **Neither prints a letter.**

## 4. Also - keep unit 491's pairing change

The owner is asked in 491's report whether to keep the change where a bar pairs with the nearest
bar at its own level. **Keep it.** It reads the bursts case better, and its only cost is thirty
extra missed hops on a weak tone's scope picture, not on the letters. Record that as the answer
in `PARKED.md` and in this unit's report; change nothing.

---

## 5. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 492 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 492.
- Patch-bump `Directory.Build.props`.
- **Append R105 to the rulings section of both `PHASE_PLAN.md` copies**, in the owner's words
  above. **Touch no checkbox.**
- `DECISIONS.md`, newest first, **HM-DEC-197**, headline *A mark is a dot or a dash the moment it
  ends; the noise guard belongs where letters are made*, naming unit 491's 435 ms measurement as
  the reason.

---

## 6. Report

Section 2, for the owner, in plain words:

- rebuild;
- letters no longer split because a dot arrived late - the blocks and the letters should appear
  within a fraction of a second of the sending;
- noise with no station still prints nothing;
- a single lone dit or dah prints nothing, on purpose.

Section 1: what changed, file by file, and that the build and the app line are green.
**Section 3: the four cases' text, and the worst delivery delay before and after in
milliseconds.** Section 4: anything left, a line each.
