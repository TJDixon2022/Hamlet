# Work instruction 477 - bars, not waves

**One unit. Seed it and drop STOP, or run it by hand.** It replaces the envelope detector's
floor-and-margin with the rule the owner gave - a keyed tone is a flat-topped run, noise never
holds a level - and it wires the meter's verdict to the tracker and the decoder so a station
found is a station read. Four tasks.

**The owner's rule, 2026-09-28, R91:** *"This is all about signal in noise. I want as wide as
possible without garbage sneaking in. We can tune out garbage by seeing if the signal stays at
an amplitude for a period. Real signals will be bars, not waves."*

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

## 1. Rules, short

**R88 - the corpus is banned.** No task reads a recording, runs a floor, the engine
carry-forward line, a metric or the keyed set. Entry and exit: the app carry-forward line and
the types this unit touches; a touched type that reads a recording is not run and is named.

**HM-DEC-155.** No suite. Named types only, one per invocation, own `timeout`. Never
background and poll. The app line loses names to the dispatcher loop; re-run once, count
neither way.

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm` is
refused; Python cannot run here; `-m` more than once for a multi-line commit. Scripts go in
`.run-unit\unit477-<name>.sh`, run with `sh`.

**By hand:** take `SESSION.lock` through `tools\arbiter\lock.bat take`, release it at the end,
write nothing to `RUN_LEDGER.md`, touch nothing under `tools\arbiter\`.

**The four report headings, exactly:** `## 1. What Claude did`, `## 2. What the owner should
expect`, `## 3. What you should see`, `## 4. What's blocking us`. `UNIT:` line without brackets.
`ADVANCES: step 12 criterion 4`. **Line C of the ordering block names how many items section 4
raises.** Nothing in section 4 halts this unit.

---

## 2. Why this unit exists

```
PHASE GOAL: Hamlet meets the CW requirements.
UNIT GOAL:  A keyed station at any pitch is found by its bars and read at
            its pitch within a second of being found.
ADVANCES:   step 12 criterion 4
```

**What the owner's rows say, 2026-09-28, 13:22 to 13:29 UTC, four stations on 20 m.** Read
them from `%AppData%\Hamlet\telemetry\2026-09-28.jsonl`, event `owner_verdict`; the figures
below are from that file.

1. **The scope built by 476 saw zero marks on every station, including the one that
   translated.** On the sweet spot - 14.0326, meter keying at 20 to 25 dB swing, median 44
   ms, translated well - the scope's envelope moved between -17 and -28 dB, its floor sat at
   -20, and its threshold at -11. **The floor tracked the middle of the swing, and the
   threshold landed above every mark.** A floor that averages a keyed signal is at the marks.
2. **The meter finds the station in one second, every time.** 600 Hz at the first press on
   14.0327; 375 Hz at the second press on 14.0321; the sweet spot's 500. Swing 18 to 30 dB,
   median 44 to 52 ms on the good rows. **The meter is the detector the owner asked for, and
   it is already in the tree.**
3. **The tracker does not obey it.** On 14.0327 the meter said 600 Hz on the first row; the
   tracker mixed at 500, then 500, then 575, and reached 600 on the fourth row, twenty-two
   seconds later, only when the survey admitted 600. The decoder read an empty bin the whole
   time. On 14.0321 the tracker did reach 375 Hz, but `trackerHasKeying` was false on four of
   six rows while the meter said keying with score 0.26 to 0.29 - the tracker's own keying
   flag rests on the 20 dB swing gate, and the station swung 18. **The decoder was mixed at
   the right pitch and told nobody was there.**

**The rule, in the owner's words, made operational.** A keyed tone holds a level: it goes up,
sits flat within a small tolerance for tens of milliseconds, drops, sits flat again. Noise
wanders. QSB is a slow wave. A carrier is a bar that never drops. **So a bin is keying when its
level makes flat runs of dit length or longer, separated by flat gaps; a bin whose level never
holds flat is noise at any loudness.** No floor, no margin, no swing threshold. The tolerance
for *flat* and the shortest run worth calling a bar come from what a dit is and how little a
keyed tone wobbles - not from a recording and not from the owner's rows.

---

## 3. Verify against the tree

- `CwEnvelopeDetector` from unit 476: its floor rule, its 9 dB margin, its per-hop outputs,
  and where `CwScopeControl` reads them. **The floor and the margin go; the rest stays.**
- `CwKeyingMeter`: its per-bin sweep, its four-part verdict, `ConfidentSwingDb` at 20, and the
  hop-level series each bin's verdict rests on - **that series is where the runs are.**
- `CwToneTracker`: how it takes a candidate from the survey, what sets `HasKeying`, and where
  the decoder reads the mix pitch. `CwToneSurvey`: what admits a bin and how the tracker
  consumes it.
- `CwDecoder`: the seam where the tracker's pitch and `HasKeying` reach the decode path and the
  emission gate.
- The rig state's `FilterBandwidth` and `CwPitch`, for the sweep's edges.

## 4. Rulings in force

`PHASE_PLAN.md` R77 to R91 and §6. **R88** the corpus is banned; steps 11 and 12 only.
**R90** the oscilloscope. **R91** bars, not waves. **§0.0** every threshold's remark says what
it rests on, and none rests on a recording. **§0.2** nothing that keys or transmits. **§0.6**
color never the sole carrier. **HM-DEC-155, HM-DEC-165, FACT-006.**

**Record this in `DECISIONS.md`, newest first, and one row at the top of `CLAUDE.md` §1's
table dated 2026-09-28, headline **A keyed signal is a run of bars; the tracker obeys the
meter**, ref HM-DEC-186:**

```
---
id: HM-DEC-186
date: 2026-09-28
refs: PHASE_PLAN.md R91 and criterion 12.4, the owner's verdict rows of 2026-09-28, CwEnvelopeDetector.cs, CwKeyingMeter.cs, CwToneTracker.cs, work instruction 477
---

**A keyed CW signal is detected as a run of flat-topped bars in a bin - level held within a
small tolerance for at least a dit, dropped, held again - at any pitch the filter passes and
at any loudness; noise is what never holds a level. When the meter finds keying at a pitch,
the tracker mixes there at once and the decoder is told a station is present.** Tim,
2026-09-28.

**What the rows showed.** Unit 476's detector tracked its floor to the middle of a keyed
signal's swing and put its threshold above every mark, seeing zero bars on four stations
including one that translated. The meter found each station's pitch within a second. The
tracker waited on the survey - twenty-two seconds on one station - and on another reached the
pitch but held its keying flag false on an 18 dB swing against a 20 dB gate, so the decoder
was mixed correctly and told nobody was there.

**What is ruled.** The detector's floor and margin are removed and replaced by run detection:
a bin is keying when its level makes flat runs of dit length or longer separated by flat
gaps. The tolerance and the shortest run derive from what a dit is and how little a keyed tone
wobbles, and are fitted to no recording. The tracker takes the meter's pitch the moment the
meter says keying, without waiting for the survey. The tracker's keying flag follows the
meter's verdict, and the 20 dB swing gate no longer decides it. The owner's words: "Real
signals will be bars, not waves."

**Whose words are whose.** The ruling is Tim's; the wording is work instruction 477's record
of it.
```

---

## 5. The tasks

### Task 0 - the record and the entry round

`PHASE_OUTCOME.md` gets `## UNIT 477 - STEP 12` from the block at the foot. `PHASE_STATUS.md`
names 477 and `CURRENT_STEP: 12`. Patch-bump. `DECISIONS.md` HM-DEC-186 and the `CLAUDE.md`
row. **Tick 12.1, 12.2 and 12.3 from unit 476's work if the report's refusal left them
unticked** - the detector, the scope and the row are in the tree. Entry round: the app
carry-forward line.

### Task 1 - bars, not waves (12.1 rewritten)

Replace `CwEnvelopeDetector`'s floor-and-margin with run detection, **per bin across the whole
passband** - `CwPitch` plus and minus half `FilterBandwidth`, or the whole audio band when the
rig state is unknown - at the meter's bin spacing:

- **A run**: consecutive hops whose level stays within a tolerance of the run's own mean. The
  tolerance is the author's, stated in the report with its reason, and it comes from how much
  a keyed tone's level wobbles across a mark - a few dB - not from a recording.
- **A bar**: a run at least as long as the shortest dit anyone sends - **25 ms**, from
  `CwToneSurvey.ShortestDitMs`, which the radio's own keyer bounds - that stands above the
  runs either side of it.
- **A gap**: a run below the bars either side.
- **Keying in a bin**: at least two bars separated by a gap within the last second. **Not
  eight marks and three seconds** - two bars is a dit and a dit, and the owner hears that.
- **The pitch**: the bin with the most bars in the last second. **Contrast**: that bin's bar
  level over its gap level, in dB - the swing, measured, never gated.
- **Per hop, still**: mark or gap in the found bin, run length, marks in the last four seconds,
  so `CwScopeControl` and the verdict row keep their fields. **The scope's floor line becomes
  the found bin's gap level and its threshold line the midpoint between gap and bar; both are
  measured, and the labels say so.**

**No floor tracker. No margin constant. No swing gate.** If any number in this task is chosen
by looking at the owner's rows or a recording, the report says so and it is wrong.

**Watch it fail first** on synthetic hops in the test - a keyed tone at a known pitch and
speed over noise that wanders - red on a stub, green when bars land on the marks, gaps on the
gaps, and the pitch reads the tone. **Then a second synthetic: a steady carrier.** It must
read as *not keying* - one bar with no gap. **Then a third: noise alone, loud.** Not keying.

### Task 2 - the tracker obeys the meter (12.4, first half)

Two wiring changes and nothing else:

1. **The pitch.** When `CwKeyingMeter`'s verdict is *keying* at a pitch, `CwToneTracker` mixes
   at that pitch on the next hop. It does not wait for the survey. If the survey later admits a
   different bin, the meter's pitch still wins while the meter says keying there. **Watch it
   fail first**: a driven meter reading of keying at 600 Hz while the tracker holds 500; red
   when the tracker stays at 500, green when it moves.
2. **The keying flag.** `CwToneTracker.HasKeying` is true when the meter's verdict is keying.
   `ConfidentSwingDb` no longer decides it - **the constant stays, its remark says it is no
   longer consulted for this flag and why.** Watch it fail first: a meter reading of keying at
   18 dB swing; red when `HasKeying` is false, green when true.

**The decoder is not changed.** It reads the tracker's pitch and flag as it always did; what
changes is that they now arrive within a hop of the meter's verdict rather than twenty-two
seconds later or never.

### Task 3 - the run detector reaches the meter (12.4, second half)

**Only if task 1 and task 2 are green.** The meter's own four-part verdict keeps its score and
its element-duration tests. **Its swing test is replaced by task 1's bar test**: a bin passes
when it makes bars, at any swing. Watch it fail first: the 18.6 dB station's profile from the
owner's row at 01:16:22 - score 0.27, median 48 ms, swing 18.6 - read as keying. Red at the
swing gate, green with the bar test.

**Drop candidate:** whole task. If dropped, the meter still gates on 20 dB and task 2's wiring
still lands; the report says so.

### Task 4 - the exit round

`Hamlet.sln` builds with warnings as errors. The app carry-forward line. Every type touched
that reads no recording. `src\Hamlet.RadioEngine\Cw` diff against entry names every file, and
the report says in one line what each change does. Transmit files print nothing against
`7e209cb4`. **No recording was read.**

---

## 6. Do not

- Do not keep a floor tracker, a margin, or a swing threshold as the thing that decides
  keying. The bars decide.
- Do not fit any number to the owner's rows or a recording. The rows are evidence the old
  gates failed; they are not a calibration set.
- Do not limit the sweep to 300 to 900 Hz.
- Do not change the decoder's decode path. Change what the tracker tells it, and when.
- Do not remove or change the light, the strip, the buttons or the verdict row.
- Do not read, run or measure against any recording.
- Do not touch what keys or transmits.
- **No unfiltered `dotnet test`. Never background and poll. Never compose a timestamp.**

## 7. Report

`output.md` at the root, four headings exactly.

```
READ IN THIS ORDER.

A. What decides keying now, in one sentence, and what the tracker does the
   moment the meter finds a station.
B. Step 12: 12.1 the detector rewritten, 12.4 the wiring, task 3 kept or dropped.
C. The rest. Section 4 raises <n> items, none blocking. No recording was read.
```

```
UNIT:       477 - <complete|stopped> at task N of 4, <dropped or none dropped> - <date time>
PHASE GOAL: <in your own words>
UNIT GOAL:  <in your own words>
NUMBER:     gates removed: floor, margin, swing; tracker follows the meter within 1 hop; recordings read: 0
```

**Section 2 tells the owner:** rebuild, tune a station at any pitch, and look at three things:
bars under the trace as you hear keying, the mixing line jumping to where the meter's line is
within a second, and characters. Then press. And what a carrier and plain noise now look
like on the scope - no bars.

**Section 3 gives the three synthetic tests' outputs:** the keyed tone with its bars, the
carrier with one bar and no gap, the loud noise with none.

---

```
ARBITER-DECISION
STEP: 12
APPROACH: replace the envelope detector's floor and margin with run detection per bin across the passband - a bin is keying when it makes flat-topped bars of dit length separated by gaps - wire the tracker to take the meter's pitch and keying verdict within a hop, and replace the meter's swing gate with the bar test
MOVE: continue
WHY: PHASE_PLAN.md step 12 criterion 12.4 asks that the detector drive the decoder - nothing decoded until a mark is found, the decoder mixed at the detector's pitch and started when the tone is present - judged by the owner's ear and his verdict rows, and those rows show the meter finding each station within a second while the tracker waited on the survey or held its keying flag false on an 18 dB swing
STATE: partial
DECIDED: the flatness tolerance, the bar-count that makes keying, and whether task 3 is reached are the author's, overrulable; every number's remark names its reason and none rests on a recording
LICENCE: PHASE_PLAN.md R88, R90, R91, section 6, step 12; HM-DEC-186; CLAUDE.md 0.0, 0.2 and 0.6; HM-DEC-155; FACT-006
ACCOMPLISHED: a keyed station at any pitch in the filter is found by its bars and read at its pitch within a second, which is what the owner's ear has been doing all week and Hamlet has not
ADVANCES: step 12 criterion 4
END-ARBITER-DECISION
```
