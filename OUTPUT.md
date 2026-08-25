# Work instruction 014 — ship the re-read, and stop leaving stations

## 1. What Claude did

Claude Code, on the development computer, in `C:\Source\HamLet`. The prompt
claimed `PROJECT: Hamlet`; the tree confirmed it — `SHACK_FACTS.md` and
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` exist, neither
`CoreHMI.sln` nor `MURC.sln` does, `CLAUDE.md`'s header says Hamlet and the
solution is `Hamlet.sln`. Branch `main` throughout, three commits, all pushed,
none refused. Version 1.11.10 to 1.11.11 per HM-DEC-150.

**Nothing here is evidence about the radio.** No rig was connected.

**No decision was recorded under §12.1.** Everything needing a ruling is in
section 4.

### Where the instruction and the tree disagree

- **The re-read is in no commit.** The order says to "find it in history rather
  than rebuilding"; unit 1.11.10 reverted it *before* committing, so
  `git log -S ReadAgain` finds nothing. It was recovered from a session
  scratchpad copy taken mid-work — which held the mechanism but **not** the two
  fixes that made it work, so both had to be re-applied from that unit's report.
  Every number then reproduced exactly, which is the check that it is the same
  thing.
- **The engine baseline was 29 failing of 1789, not 30 of 1674.** The 1674 count
  predates the thirteen captures banked last unit.
- **The order names three flaky tests; there are four.**
  `SynthesisTests.Render_AllocatesNothingPerFrame` failed once in a full run and
  passed three times alone — a GC-sensitive allocation check.
- `CLAUDE_CODE.md` §8 says four sections; its version line still reads 1.3.
- `DECISIONS.md` still has no record for HM-DEC-096–133, 136, 141, 150, nor for
  Tim's five rulings of 2026-08-25.

### Task 1 — the re-read, shipped

Re-applied, and **every number reproduced exactly**: adjudicated characters
**167 of 384**, `AA4MP/4 QNIK` twelve of twelve, the ARRL bulletin at 28,
`cw-2026-08-22-031948` unsure at nought, zero re-reads on all four empty
captures, chunk-size invariance across 240/480/960/1920/4800 and both entry
points, nothing said twice.

`CwSensitivityTests.TheDecoderReadsAsFarDownAsItDidBefore` **went green with
it** — the re-read reaches further down than the decoder did before, which is
a gain nobody predicted.

**Count floors retired on the three recordings an adjudicated anchor covers**,
under Tim's ruling, with the reason in the file rather than in a commit message.
The retirement list is read from the anchors themselves rather than typed again,
so a recording gaining or losing an anchor cannot leave a stale copy behind.
**Element floors do not retire on any recording** — they measure how much of the
signal was seen rather than how it was grouped, and an anchor says nothing about
that. Twenty-four recordings no anchor covers keep their count floors.

**`cw-2026-08-25-012748`'s regression is written into the floors file**, not
retired: sixteen elements to four, four characters to two, no anchor covering
it, the loss on the record with the ruling cited.

### Task 2 — a confirmed station is not abandoned: mechanism found, task stopped

**The displacement guard never runs on this recording.** Traced at the moment of
the move on `cw-2026-08-25-012823`:

```
14.53s  500.0 -> 450.0    reading=NaN  lastKeyed=NaN
```

`_readingDb` and `_lastKeyedHz` are both `NaN`, so
`CwToneTracker.cs:1074`'s first clause is false and HM-DEC-127's guard is
**inert for the whole recording**. The tracker sat on 500 Hz for eleven seconds
having **never confirmed it**: that pitch came from the cold-start
"point at the loudest thing and let the decoder look" path at
`CwToneTracker.cs:1005–1017`, which deliberately moves the filter without
setting a verdict — *"This is not a claim and does not set the verdict."* So when
450 finally confirms, it is the **first confirmed station on the recording**, and
there is nothing to abandon.

**Why 500 never confirms**, at `CwToneTracker.cs:1049`: confirmation needs two
**consecutive** surveys agreeing within `ConfirmWithinHz`. Traced across
13.0–15.0 s the survey holds both bins, and the real one keeps dropping out:

| | 500 Hz — the station | 450 Hz — the rival |
|---|---|---|
| key-down level | −34.3 dB | **−39.0 dB**, 4.7 below |
| lift | 36.0 | 31.4 |
| present in the admitted set | intermittently | on nearly every read |

`Beats` ranks by lift, so 500 wins whenever it is admitted — and consecutive
reads therefore **alternate** 500, 450, 500, 450, fifty hertz apart, twice the
25 Hz confirmation window. Neither confirms. The rival's persistence eventually
gives it two reads in a row and it takes the tracker.

**The fix is not contained, so the task stopped as the order directs.** Either
the confirmation rule tolerates an intermittent station — which is HM-DEC-095's
own constant and changes acquisition everywhere — or the cold-start path starts
setting a verdict it is explicitly written not to set.

**One thing was tried and reverted, and its result is worth keeping.** Lowering
the guard's bar from 25 dB to 3 dB — half the power — **did not fix `012823`**,
because the guard is inert there, and it changed the tracked pitch on **six**
other captures against an acceptance of none. Two of those six changed for the
better: `cw-2026-08-22-031905` held 500 Hz instead of wandering to 300, and
`032113` held 500 instead of 650 — and 499.8 Hz is the documented W1AW carrier
for both. That is a lead for whoever takes the confirmation rule on, not a
change to make now.

### Task 3 — why the re-read destroys `012748`: diagnosed, no fix qualifies

**It replays four times where the capture it helps replays once.**

| | `012748` | `003758` |
|---|---|---|
| replays | **4** | 1 |
| when | 2.5 s, 3.0 s, **19.5 s, 23.0 s** | 2.5 s |
| window held | 508, 608, **2400, 2400** hops | 508 |

It emits two characters in thirty seconds, so the `settled > 0` guard — a proxy
for "still in the opening" — **never closes**, and two replays fire over a
*full* twelve-second window two-thirds of the way through. Each replay wipes the
envelope and rebuilds it.

**But the late replays are not the damage.** Refusing to replay a full window
removes both of them and leaves `012748` at **nought characters and nought
elements** — worse than the two it has. Three variants measured:

| variant | `012748` | `003758` |
|---|---|---|
| **as shipped** | **2 chars, 4 elements** | 58 chars, 124 elements |
| refuse a full window | 0, 0 | 58, 124 |
| repeat guard at one bin inclusive | 0, 0 | 58, 124 |

The shipped form is the best of the four, and `003758` — the capture the
re-read exists for — is untouched by any of them. **The damage is done by the
early replays**, which alternate 375 Hz and 400 Hz half a second apart while the
true station is at 401. Nothing contained fixes it, so nothing was changed.

### The suite

| | baseline | end |
|---|---|---|
| engine | 29 failing of 1789 | **28 failing of 1831** |
| app | 487 passing, 0 failing | **487 passing, 0 failing** |

**Nothing new failed and one test went green** — the sensitivity test, fixed by
the re-read.

## 2. What Tim should expect at the radio tonight

**The opening of a station no longer wears the wrong pitch for the rest of the
contact.** Until now the first two to seven seconds of every signal were
demodulated at whatever the radio's pitch knob said, and the decoder lived with
that for the whole recording. It now goes back over the audio it is still
holding, once it knows where the station actually is, before those characters
reach the screen.

**What that is worth, on recordings you can check:** the callsign
`AA4MP/4 QNIK` now comes back whole where it used to lose its first three
characters, and six more characters of the ARRL bulletin are right. Across every
adjudicated recording, 167 characters of 384 against 158.

**Nothing is ever un-said.** The re-read only touches characters that have not
yet been announced.

**Stations are still walked away from, and `012823` still does it.** That fault
is named now, with the line, and it is not what anybody thought: the guard meant
to stop it was never running on that recording. Section 4 has it.

**What will look wrong and is not:**

- **Three captures emit fewer characters than their old floors** — `003758`,
  `004507` and `031948`. All three see the same or more of the signal and read
  more of it correctly; `031948`'s unsure count went from three to nought. Their
  count floors retired under your ruling and their anchors guard them now.
- **`cw-2026-08-25-012748` got worse** — sixteen elements to four. It is the one
  capture the re-read hurts, no anchor covers it, and its floor was lowered with
  the loss recorded rather than hidden.
- **`032113`, `032012` and `032050` did not move**, as unit 1.11.10 predicted —
  their pitch is measured too late for a live re-read to reach.

## 3. What you should see

**Adjudicated characters with the re-read shipped: 167 of 384**, against a bar
of 167 and a previous achieved total of 158.

| reading | before | after |
|---|---|---|
| `AA4MP/4 QNIK` (HM-DEC-126) | 9 of 12 | **12 of 12, whole** |
| the ARRL bulletin (HM-DEC-115) | 22 of 57 | **28 of 57** |
| the other ten | unchanged | unchanged |

**`cw-2026-08-25-012823`'s tracked pitch across the whole recording:**

```
600 Hz  ->  400 Hz (1.5 s)  ->  500 Hz (3.0 s)  ->  450 Hz (14.5 s)  ->  end
```

It reaches the right answer at three seconds, holds it for eleven, and leaves it
for a rival 4.7 dB quieter. **It ends 49.8 Hz below a true 499.8 and the second
half of the recording is soup.** That is unchanged by this unit — the task
stopped at the mechanism, which is that the guard against exactly this was never
armed, because the correct pitch was never confirmed.

## 4. What's blocking us

**The tracker cannot confirm an intermittent station, and everything downstream
of that is unprotected.**

This is task 2's finding and it is larger than the capture it was found on. Three
lines of code interlock:

- `CwToneTracker.cs:1049` — a candidate confirms only on **two consecutive**
  surveys agreeing within 25 Hz.
- `CwToneTracker.cs:1005–1017` — before anything is confirmed, the filter is
  pointed at the loudest bin, deliberately **without** setting a verdict.
- `CwToneTracker.cs:1074` — HM-DEC-127's displacement guard tests
  `_readingDb`, which only the confirmed path sets.

On `012823` the real station is admitted intermittently and a rival 4.7 dB below
it is admitted almost always, so consecutive reads alternate fifty hertz apart
and **neither confirms**. The tracker tracks the right pitch for eleven seconds
on the unconfirmed cold-start path while the guard sits inert, and the first
thing to confirm is the rival.

Two shapes of answer, and **both change a ruled constant, which is why this is
yours**:

*Confirmation tolerant of gaps* — two agreeing surveys within a short window
rather than strictly consecutive. That is HM-DEC-095's own rule, written against
noise producing one convincing fluke, and loosening it is exactly what that
ruling forbids without measurement.

*The cold-start path setting a reading level* — so the guard protects the bin the
tracker is actually on. The path's own comment says it must not: *"This is not a
claim and does not set the verdict."*

*Rejected: lowering the guard's bar.* Measured — 25 dB to 3 dB does not fix
`012823` at all, because the guard is inert there, and it moves six other
captures' pitch against an acceptance of none.

**A lead worth keeping**: that same rejected experiment made `031905` hold 500 Hz
instead of wandering to 300, and `032113` hold 500 instead of 650. Both are W1AW
captures whose documented carrier is 499.8. Whoever takes the confirmation rule
should measure those two first.

---

**The re-read's own guard is a proxy that does not hold on a quiet recording.**

`settled > 0` is meant to say "still in the opening". On
`cw-2026-08-25-012748`, which emits two characters in thirty seconds, it stays
false for nineteen seconds and licenses a replay of a **full** twelve-second
window. Refusing that was measured and made the capture worse, so the proxy is
not simply too loose — the early replays are what destroy it, alternating 375 and
400 Hz half a second apart around a station at 401.

No contained fix; three variants measured and the shipped form is the best.
It is one capture of thirty-six and its loss is on the record.

---

**The re-read was never in history, and the next unit should not assume a
reverted mechanism is recoverable.**

This unit's first task was written as "re-apply the reverted commit". There was
no commit — unit 1.11.10 reverted before committing, as its own report said, and
what survived was a scratchpad copy of an intermediate state. It reproduced
exactly once the two documented fixes were re-applied, so nothing was lost this
time. **A measured mechanism that is not shipped should be committed behind
something inert, or it exists only in prose.**

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Thirteen inbound. The
oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150 — nor for
   Tim's five rulings of 2026-08-25, two of which this unit acted under.**
5. **The tone tracker** — task 2 names its sharpest fault and stops at the
   mechanism.
6. **The integrator width** — bears on `014113`/`014308`.
7. **The guard's gap is two to one**, calibrated on two empty captures.
8. **A boxcar's nulls made two of five swept offsets pathological best cases.**
9. **Two stations closer than 125 Hz are not named.**
10. **The keying meter** — calibration measured inside an overlap; its own unit.
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).

New this unit: **the tracker cannot confirm an intermittent station**, above;
**the re-read's opening guard does not hold on a quiet recording**, above; **a
measured mechanism that is not shipped should still be committed**, above.

Still open: **the lock's mixed help**; **the "Hold this pitch" button**; **three
fixtures at accepted cost**; **`001520`'s quadrillions**; **the reference/port
integrator difference**; **`CLAUDE_CODE.md`'s version line**; **an unmeasured
pitch costs `N4L`**; **`014113`/`014308`'s second mechanism**; **the six-hertz
window disagreement**; **the short-character bias**; **the Avalonia geometry
offset**; **`CHANGELOG.md` at 1.9.0 against a version of 1.11.11**; **four
intermittent tests, not three**; **the whole-file second pass for late-pitch
captures**.

Closed this unit: **the re-read**, shipped under today's ruling. **The joint
cutter**, settled at a safe weight of nought by unit 1.11.10's second table.
