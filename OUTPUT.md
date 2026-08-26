# Work instruction 015 — the squelch, the screen, and the margin worth logging

## 1. What Claude did

Claude Code, on the development computer, in `C:\Source\HamLet`. The prompt
claimed `PROJECT: Hamlet`; the tree confirmed it — `SHACK_FACTS.md` and
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` exist, neither
`CoreHMI.sln` nor `MURC.sln` does, `CLAUDE.md`'s header says Hamlet and the
solution is `Hamlet.sln`. Branch `main` throughout, three commits, all pushed,
none refused. Version 1.11.11 to 1.11.12 per HM-DEC-150.

**Nothing here is evidence about the radio.** No rig was connected.

**No decision was recorded under §12.1.** Everything needing a ruling is in
section 4.

**The status cadence slipped twice** and Tim had to ask for it both times.
Two measurement passes ran roughly twenty minutes each without a write. The
rule is not the problem — §13.2 has carried it since HM-DEC-132 — and the
report says so plainly rather than explaining it away.

### Where the instruction and the tree disagree

- **Tonight's three captures are absent**: no `cw-2026-08-26-004808`,
  `-004900` or `-004952` anywhere in the tree. Every dependent step fell back.
- **`BUILD_SESSION_2026-08-25.md` is not in the tree either** — the plan this
  unit implements. The same failure as `ANALYSIS-2026-08-25-session.md` two
  units ago: everything attributed to it here is from the instruction's own
  quotation of it.
- **The engine baseline was 28 failing of 1831 and the app 487 of 487**, exactly
  as the order states.
- `CLAUDE_CODE.md` §8 says four sections; its version line still reads 1.3.
- `DECISIONS.md` still has no record for HM-DEC-096–133, 136, 141, 150, nor for
  Tim's five rulings of 2026-08-25.

### Task 1 — nothing to bank

The three captures are absent, so the overlap fixture, the control and the
honest-behaviour case are all missing. The upper bound of task 2's window
therefore had to come from the W1AW anchors alone, and the control role fell to
`013520`, `013303` and the twelve anchors, as the order's fallback directs.

### Task 2 — the squelch does not ship, on four measurements

**The axis the unit is built on does not exist at the tracked pitch.** The
plan's duty figures come from the capture sidecars, over a swept pitch, on
broadband audio. Measured rolling over three seconds at the tracked pitch on the
decoder's own envelope — the only duty a live squelch can have — it separates
nothing:

| | Otsu cut | half-amplitude cut |
|---|---|---|
| the four recordings holding **nothing** | median **0.57 – 0.63** | median **0.40 – 0.42** |
| the rag chews that read well | 0.44 – 0.56 | 0.35 – 0.46 |
| `021629`, whose `559 559 IN MI MI` must survive | 0.303 | **0.227** |
| `021825`, which the plan says should shrink | 0.567 | 0.420 |

The empty band sits in the middle of the distribution on both definitions, and
the plan's own 25 % lower bound would hold `021629` silent while leaving
`021825` untouched. This is HM-DEC-119's lesson again: a measurement taken
through one instrument is not a fact about another.

**The fist half of the same test does separate on medians** — the well-read
captures sit at 2.5–3.1 and the four empty ones at 3.4–4.1 — **and it cannot be
bounded.** Built and measured:

| band | adjudicated characters | anchors |
|---|---|---|
| none (as shipped) | **167 of 384** | all twelve green |
| ratio [2.2, 4.0], the plan's own | **121** | seven red; `VA3VRR` and `N4L` **silenced outright** |
| ratio widened to [2.2, 7.0] | **137** | still thirty characters short |

`cw-2026-08-17-013347` runs a median ratio of 4.77 and `134712` 3.76, so the
gate is closed exactly where their callsigns are. The order's own rule —
*"if any floor case changes, the window is wrong: widen it, don't ship"* — is
decisive: widening far enough to keep the anchors still costs thirty
characters. Nothing shipped, nothing left behind.

### Task 3 — the screen, in the part that was available

**Parts 1 and 2 are blocked by task 2.** The separator is triggered by the
squelch having held ten seconds and the dimming is defined as "everything before
the most recent separator". With no squelch there is never a separator, so both
render exactly as before. Section 4 carries the alternative rather than my
inventing one.

**Part 3 shipped.** The keying sweep's advice paragraph — *"the signal is being
lost somewhere between the antenna and Hamlet, and the gain, the filter and the
tuning are the things to try"* — now retires whenever the decoder has found a
tone. It is only ever true where nothing found one; where something has, the
sweep disagreeing with it is a fault in the sweep, and sending the operator to
the radio acts on the wrong instrument.

### Task 4 — the margin, logged and read by nothing

The path search now keeps its runner-up at each hop, and every character carries
`LLR(best) − LLR(second-best)` beside `spanLlr`, on the capture sheet and in the
record and on no display.

The inversion it exists to replace, for the record: on the pile-up of
2026-08-26 the E-soup scored 8003–29261 against silence while the plausible tail
scored 41–437 — **the soup outscoring the copy a hundred to one**, because on
audio that is never silent the all-key-up null is the wrong reference. Against a
second-best *reading*, a letter carved out of continuous tone has an alternative
that fits about as well and the margin collapses toward nought.

**Both figures are clamped to a million and say so when they hit it.** The sheet
has printed `6:27306879.3` and a sheet carrying that is one nobody reads the rest
of. A clamp is a statement about the record's range, not about the measurement,
so a clamped figure is marked rather than quietly made smaller.

**No behaviour changed**, and the engine suite proves it: 28 failing of 1831,
failure set byte-identical to the baseline.

### Task 5 — the sweep goes behind a setting

`AppSettings.ShowKeyingSweep` ships **off**. The panel stops drawing; the meter
keeps computing and keeps writing to the capture sidecar. A setting rather than
a deletion, because the person diagnosing it still needs to see it, and
rebuilding the instrument is its own unit.

### Task 6 — dropped

The drop candidate, dropped whole. Tasks 3, 4 and 5 were what mattered tonight
and the report lands rather than a sixth task half-built.

### The suite

| | baseline | end |
|---|---|---|
| engine | 28 failing of 1831 | **28 failing of 1831**, identical set |
| app | 487 passing | **489 passing, 0 failing** |

## 2. What Tim sees at the radio tonight

**One instrument stops arguing with another.** The keying sweep is off the
terminal. It was wrong on fourteen of twenty recordings against independent
measurement, and this tree has since measured its calibration inside an overlap
rather than a gap — `cw-2026-08-25-021825` holds a station and swings 12.6 dB,
below all four recordings that hold nothing. It is still computing into every
capture sheet; it has stopped asserting on screen.

**And where it is turned back on, it no longer sends him to the radio for a
decoder condition.** The paragraph about the antenna and the gain and the filter
retires whenever something has found a tone. That is the panel that cost him a
trip to the rig on 2026-08-25 when nothing was wrong with it.

**The capture sheet gains a second number per character**, beside the one that
has been misleading: how much better the winning reading was than the next best.
Nothing acts on it yet.

**What has not changed, and should be said plainly:** a quiet frequency does
**not** stay quiet on screen. The squelch is the one thing this unit was aimed
at and it did not survive its own measurement. Soup still reaches the transcript
on an empty band, and the transcript still shows old soup as brightly as current
copy.

**What will look wrong and is not:**

- **The keying sweep panel is gone.** It is a setting, off by default, not a
  deletion.
- **The transcript is unchanged.** Its two improvements were defined in terms of
  a squelch that does not exist.

## 3. What you should see

**The squelch's before and after on `021825`: unchanged, 41 characters either
way — because the squelch does not ship.** The survival list is therefore the
whole corpus, untouched:

| | before | after |
|---|---|---|
| adjudicated characters | 167 of 384 | **167 of 384** |
| `021629`'s `559 559 IN MI MI` | present | present |
| `013520`, `013303` | at their floors | **byte-identical** |
| all twelve anchors | green | **green** |
| the four empty captures | silent | silent |

**What the gate would have cost, measured rather than feared:** at the plan's own
ratio band, 121 of 384 and both adjudicated callsigns gone. That is the number
that stopped it.

## 4. What's blocking us

**The squelch needs an axis, and neither of the two on offer is one.**

This unit's premise is that one quantity sorts every failure of the last two
nights. Measured at the tracked pitch on the decoder's own envelope, duty does
not: the recordings holding nothing sit in the middle of the distribution on both
definitions of it, and the capture whose exchange must survive sits below the
plan's floor. The fist ratio does separate on medians and cannot be bounded
without silencing `VA3VRR` and `N4L`.

The numbers are in section 1 and they are the starting line for whatever comes
next. **The margin logged in task 4 is the candidate replacement** and it was
built for exactly this: it does not care how many elements a character has, and
against a second-best reading rather than against silence it should not invert on
a pile-up. One evening of captures with it in the sheet gives a real
distribution to set a bound from.

*Rejected: shipping the plan's band anyway.* It costs both adjudicated
callsigns.

*Rejected: widening until nothing is red.* Measured — the band that keeps the
anchors is [2.2, 7.0] and it still costs thirty characters.

---

**The transcript's two improvements are defined against something that does not
exist, and the problem they solve is real.**

Tonight's own account is that the first hundred characters of the transcript were
soup decoded two minutes earlier, sitting bright above three correctly-read
callsign tokens. Task 3 solves that with a separator inserted when the squelch
has held, and dimming before the most recent separator. Both are now unreachable.

**The available alternative, which I did not build because it was not
authorised**: dim everything except the most recent stretch, using
`CwTranscript.RecentCharacters` — a constant of 240 already in the tree for
exactly the notion of "recent". It needs no squelch and no separator, it deletes
nothing and keeps everything selectable, and it puts the eye on current copy,
which is what part 2 says it is for. §0.0 makes the screen yours and the
mechanism is a different one from the ruling's, so it is an ask rather than a
change.

---

**The build plan this unit implements is not in the tree.**

`BUILD_SESSION_2026-08-25.md` was quoted throughout the instruction and does not
exist here, so its duty figures could not be checked against the instrument that
produced them — which is precisely where this unit's central measurement went
wrong. `ANALYSIS-2026-08-25-session.md` had the same history two units ago and
was eventually delivered. A plan that a session implements from quotation is a
plan whose numbers cannot be audited.

---

**The status cadence failed twice in one session and both were mine.**

Two measurement passes ran about twenty minutes each with no status write, and
Tim asked for a report both times. HM-DEC-137 already carries this instruction in
two places for the reason that one channel has never held alone; the failure here
was not a missing rule but a session that let a long-running measurement stand in
for progress. Recorded because a cadence that fails silently is worth less than
one that fails loudly.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Thirteen inbound. The
oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150 — nor for
   Tim's five rulings of 2026-08-25, plus tonight's adoption of the build plan.**
5. **The tone tracker** — the confirmation rule's ask stands from 1.11.11; task 6
   was dropped, so its selection half is unmeasured.
6. **The integrator width** — bears on `014113`/`014308`.
7. **The guard's gap is two to one**, calibrated on two empty captures.
8. **A boxcar's nulls made two of five swept offsets pathological best cases.**
9. **Two stations closer than 125 Hz are not named.**
10. **The keying meter** — task 5 hides it; the rebuild is its own unit.
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).

New this unit: **the squelch needs an axis and neither candidate is one**, above;
**the transcript's dimming needs a trigger that exists**, above; **the build plan
is not in the tree**, above.

Still open: **the lock's mixed help**; **the "Hold this pitch" button**; **three
fixtures at accepted cost**; **`001520`'s quadrillions**; **the reference/port
integrator difference**; **`CLAUDE_CODE.md`'s version line**; **an unmeasured
pitch costs `N4L`**; **`014113`/`014308`'s second mechanism**; **the six-hertz
window disagreement**; **the short-character bias** (task 4 logged its
replacement quantity); **the Avalonia geometry offset**; **`CHANGELOG.md` at
1.9.0 against 1.11.12**; **four intermittents**; **the whole-file second pass**;
**the confirmation rule cannot admit an intermittent station**; **tonight's three
captures were never delivered**.
