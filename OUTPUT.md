# Work instruction 018 — admit the station the operator can hear

## 1. What Claude did

Claude Code, on the development computer, in `C:\Source\HamLet`. The prompt
claimed `PROJECT: Hamlet`; the tree confirmed it — `SHACK_FACTS.md` and
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` exist, neither
`CoreHMI.sln` nor `MURC.sln` does, `CLAUDE.md`'s header says Hamlet and the
solution is `Hamlet.sln`. Branch `main` throughout, three commits, all pushed,
none refused. Version 1.11.14 to 1.11.15 per HM-DEC-150.

**Nothing here is evidence about the radio.** No rig was connected.

**No decision was recorded under §12.1.** Section 4 carries what needs a ruling.

**Tasks 1, 3 and 4 shipped. Task 2 was measured and deliberately not built**, on
the order's own prohibition. Nothing was dropped for room.

### The order pre-authorised what happened in task 2

> *"where Hamlet's instruments disagree, Hamlet's numbers are the truth about
> Hamlet and the premise is re-examined, not defended."*

It was needed. The premise did not reproduce, and the valve built on it admits
noise more often than it admits the station.

### Task 1 — the capture is banked, and the premise does not reproduce

`cw-2026-08-26-125941.wav`, its sidecar and `cases-2026-08-26.txt` are
committed, floored at their current truth: **zero characters, zero elements**.

The sidecar confirms the order's account of the evening exactly:

```
frequency  14027500 Hz  (read from the radio a moment ago)
toneHz     300.0 Hz  (measured from the keying the survey admitted, ...)
tonePeak   50.2
inThis     0 characters emitted, 0 unsure, 0 elements seen, 0 resolved
keying     no keying at 400 Hz, 37 ms key down, 16 dB swing, 206 key-downs
```

**The external measurements reproduce in-tree, except the one the whole unit
rests on.** Run through `KeyingEnvelope` at the interpolated peak:

| | the order | in the tree | |
|---|---|---|---|
| pitch | 403.5 Hz | **405.0 Hz** | reproduces |
| dah | about 105 ms | **103.4 ms** | reproduces |
| dit | about 28 ms | **31.3 ms** | close |
| speed | 36–43 WPM | **38.4 WPM** | reproduces |
| **dah/dit ratio** | **3.82** | **3.31** | **does not** |

**3.31 is inside the 2.5–3.8 band, not outside it.** The band is not what
refuses this station. What refuses it is the survey's own reading of the same
audio, which at 400 Hz measures `r5.85, dit 45 ms, separation 2.3, n 6/3` and at
425 Hz `r4.05, dit 42 ms, separation 3.5, n 5/3`.

**The mechanism is the survey's time resolution.** Its history hop is
`HopSamples` (240 samples, 5 ms) times `SurveyDecimation` (2), so **ten
milliseconds**. A 31 ms dit is three hops. The gate opens and closes on a 6 dB
hysteresis band, which eats about a hop off each end of a mark, and it eats the
same absolute amount off a dah ten hops long. So the dit reads short by a third
and the dah by a tenth, and a true 3.3 measures as 5.9.

**This is HM-DEC-146's finding again, one instrument further out.** That ruling
established that mark lengths read short below a hundred milliseconds and worse
the shorter they get, measured on generated audio with a dit known to the
millisecond. The band is fine. **The measurement feeding it cannot resolve a
fast fist.**

**The three morning captures are absent a fifth time** — `004808`, `004900` and
`004952` are nowhere in the tree. Checked by name across every folder.

### Task 2 — the valve was measured before it was built, and must not ship

The order specifies: where the dah/dit band refuses, admit a candidate whose
mark lengths form two well-separated clusters — **separation at or above 4 in
the clusters' own units, at least 3 members each** — and *"do not let the valve
admit anything on the empty captures."*

Applied to the survey's own refusals, bin by bin:

| capture | holds | admissions |
|---|---|---|
| `cw-2026-08-26-125941` | **a station** | **6** |
| `cw-2026-08-20-014854` | nothing | **6** |
| `cw-2026-08-20-014935` | nothing | **9** |
| `cw-2026-08-22-014113` | nothing | **13** |
| `cw-2026-08-22-014308` | nothing | **6** |

And on the one capture that holds a station, **only 1 of the 86 refusals at the
station's own bins (375–450 Hz) passes the test.** The other five admissions are
elsewhere in the band.

**It admits noise more often than it admits the station.** That is HM-DEC-120's
line, silence is absolute, and it is the order's own acceptance condition,
failed on the order's own control fixtures. Not built.

The reason it fails is the reason the premise failed: separation is measured in
the clusters' own units, computed by the same survey whose hop cannot resolve a
31 ms dit. **A valve fed a broken measurement is not a valve.**

### Task 3 — the held pitch lets go when the dial moves, shipped

`CwDecoder.Retuned()` releases what was measured on the old frequency:
`_lastMeasuredToneHz`, the held peak `_lastSnrDb`, and through
`CwToneTracker.Forget()` the reported pitch, the keyed pitch the cold-start path
gates on, the level the displacement guard compares against, and both surveys'
history. It is called from `OnFrequencyHzChanged`.

**The hold itself is untouched and that is the load-bearing part.** The tracker
keeps its last measured pitch through a sender's gaps, and the survey holds only
three seconds, so a slow fist would otherwise lose its pitch between characters.
What it could not do was let go. It hangs on the frequency rather than on a
clock, because that is when the evidence stops existing. A station is entitled
to pause for as long as it likes.

**The release keeps what is not about the frequency**: the bank stays pointed
where it is and the learned speed survives, because a fist is a fact about the
operator's habits rather than about a dial reading.

Four tests, on real audio rather than synthetic. Synthesized keying was tried
first and the survey refused it, which is the survey behaving correctly, and
HM-OPEN-018 has that class of fixture on record.

**And the re-decode of `125941` end to end**, which is what the operator would
have seen with the pitch released:

| | |
|---|---|
| toneHz | **400.0 Hz, `measured=False`** |
| characters | **0**, 0 unsure |
| elements | **0** seen, 0 resolved |
| transcript | empty |

**The ghost is gone and the station is not found.** With no held 300 Hz, the
cold-start path centres the bank at **400 Hz, which is where the station
actually is**, and the sheet says `NOT MEASURED: the survey has admitted no
keying, so this is the middle of the bank the decoder is pointed at rather than
a station`. That is the honest sentence. The filter is in the right place. The
survey still refuses to admit what is under it.

The four empty captures behave identically: 575, 600, 825 and 600 Hz, all
`measured=False`, all zero characters. **The release changes nothing on a
capture that stays on one frequency**, which the suite confirms below.

### Task 4 — the margin's share of the span, logged and measured

`CwCharacter.MarginShareForRecord` is on the capture sheet as a third field per
character. **The distribution across 1,583 characters** (3 carried a span at
nought and are excluded), split by whether the recording carries an adjudicated
anchor:

| | n | P10 | P25 | median | P75 | P90 | min | max |
|---|---|---|---|---|---|---|---|---|
| **anchored recording** | 599 | 0.000 | 0.001 | **0.004** | 0.023 | 0.119 | −20.09 | **1.00** |
| **everything else** | 981 | −0.042 | 0.002 | **0.005** | 0.013 | 0.057 | −1.30 | **2.45** |

Swept as a floor:

| floor | anchor kept | everything else kept |
|---|---|---|
| at or above 0.01 | 34 % | 30 % |
| at or above 0.02 | 27 % | 20 % |
| at or above 0.05 | 18 % | 11 % |
| at or above 0.10 | 12 % | 7 % |
| at or above 0.20 | 8 % | 4 % |
| at or above 0.50 | 3 % | 1 % |

**The scale problem is genuinely gone, which is the real finding.** Unit
1.11.14 measured the raw margin reaching 2.98 × 10⁸ on one capture and 1.8 on
another. **The quotient's entire observed range is −20.1 to +2.45**, and 999 of
1,580 characters sit between 0 and 0.05. The noise estimate cancels exactly as
that unit proposed it would.

**It still does not separate.** Medians 0.004 and 0.005. Every floor cuts
correct copy about as fast as soup.

**But it says something worth having.** A median of 0.004 means the runner-up
path finishes within four thousandths of the winner. **The decoder's second
choice fits about as well as its first, essentially always**, which is why the
hypothesis that a letter carved out of continuous tone has a collapsing margin
cannot be tested this way. Every character looks like that by this measure.

**Read the split with care: it is coarser than unit 1.11.14's.** That unit
bucketed by whether each *character* falls inside its recording's anchor text
(n = 131). This one buckets by whether the *recording* carries an anchor at all
(n = 599), because the anchor's character positions are not recoverable from a
re-decode whose text differs. It is a weaker test of separation and a sound one
of scale, which is the question task 4 was asked to settle.

**It is on the sheet rather than left to a reader's arithmetic** because both
inputs are clamped at a million before printing, and on precisely the captures
where the scale problem is worst that clamp fires and the quotient is gone.
Nothing reads it. It is not a threshold.

### The suite

| | baseline | end |
|---|---|---|
| engine | 28 failing of 1831 | **28 failing of 1841** |
| app | 501 passing | **503 passing, 0 failing** |

**Ten tests added and the failure count did not move.** The four release tests,
five re-decode rows folded into one floor, the capture's own floor, and two
sheet tests. An intermediate run showed 30, and that was the two release tests
before their fixture was moved from synthetic keying to real audio.

## 2. What Tim sees at the radio

**The decoder no longer claims a pitch it measured somewhere else.** Tune away
from a station and the held pitch goes with the frequency, so the next sheet
says what it actually knows about where the dial is now. The night of 2026-08-26
would have read `NOT MEASURED` instead of `300.0 Hz (measured from the keying
the survey admitted)`.

**But he would still have read nothing on 14.0275 MHz.** The release removes a
false claim; it does not deliver the station. That is not a partial win dressed
up: the decoder now points its filter at 400 Hz, which is where the station is,
and the survey still refuses to admit it.

**Nothing about any other capture changed.** Same failure set, same counts, and
the release only fires when the frequency changes.

**What will look wrong and is not:**

- **The capture sheet has a third number per character now**,
  `text:span/margin/share`. Nothing reads it. It is there so the next question
  can be asked from a record rather than from a rebuild.
- **`cw-2026-08-26-125941` is in the suite floored at zero.** A floor of nothing
  is still a floor: it stops the recording quietly getting worse while the real
  fix is found.
- **The keying sweep still says `no keying at 400 Hz`** on that sidecar while
  finding 37 ms of key-down and 16 dB of swing there. That contradiction is the
  whole of section 4.

## 3. What you should see

**The miss, re-decoded end to end** (`cw-2026-08-26-125941`, fresh decoder):

| | before | after |
|---|---|---|
| pitch claimed | **300.0 Hz, called a measurement** | **400.0 Hz, called NOT MEASURED** |
| held peak | 50.2, measured elsewhere | released on the QSY |
| characters | 0 | **0** |
| elements | 0 | **0** |

**The station, measured three ways on the same audio:**

| instrument | dah/dit ratio | dit |
|---|---|---|
| the external analysis | 3.82 | 28 ms |
| `KeyingEnvelope` at 405 Hz | **3.31** | 31.3 ms |
| **the survey at 400 Hz** | **5.85** | **45 ms** |

**The valve as ruled, on the order's own controls**: 6 admissions on the capture
that holds a station, and 6, 9, 13 and 6 on the four that hold nothing.

**The suite**: engine 28 failing of 1841 against a baseline of 28 of 1831; app
503 passing of 503.

## 4. What's blocking us

**The unit's premise was wrong about which instrument refuses the station, and
the correction points at a mechanism rather than a constant.**

Ruling asked for:

> **The survey's time resolution is the fault, and the dah/dit band is not
> touched. Where the survey's history hop cannot resolve a sender's dit, it
> measures a ratio no band could accept, and no valve downstream of that
> measurement can be made safe.**

The evidence is in section 1. On `cw-2026-08-26-125941` the same audio measures
3.31 through `KeyingEnvelope` and 5.85 through the survey; the survey's hop is
10 ms and the dit is 31 ms. HM-DEC-146 already established that marks read short
below a hundred milliseconds and worse the shorter they get, on generated audio
with a dit known to the millisecond. **This is that finding in a second
instrument, at a speed nothing in the corpus has ever read.**

*Rejected: widening the dah/dit band.* The order forbade touching its constants
and was right to. The band would have to reach past 5.85 to admit this station,
which is well into where a carrier's smeared single cluster lives, and it would
be a constant widened to accommodate a measurement error rather than a fist.

*Rejected: the admission valve as ruled.* Measured before building, per section
1: it admits noise on all four empty captures more often than it admits the
station on the one that has one. Its separation figure is computed by the same
survey whose hop is the fault.

*Not proposed, because it needs a ruling first:* the survey could run its
history at the tracker's own 5 ms hop rather than at 10, or extract mark lengths
by interpolating the gate's crossings rather than counting hops. Both are
changes to the instrument this project measures everything else with, and
HM-DEC-119 is explicit that a measurement taken through one instrument is not a
fact about another. **That is a work unit with a corpus re-measurement in it,
not a line.**

---

**The release is shipped and its limit should be said plainly.**

Task 3 removes a false claim and finds no station. On the evening in question
the operator would have gone from a confident wrong pitch to an honest absence.
That is the right direction under §0.0 and it is not the thing he asked for. The
station is still unread, and the reason is above.

---

**Three captures have now been asked for five times.**

`cw-2026-08-26-004808`, `-004900` and `-004952`. Nothing in the tree names them:
no test, no sidecar, no catalogue entry. HM-DEC-126 closed an identical case on
that exact evidence after four asks. **This is the fifth**, and the same
reasoning applies, since a question nobody can act on is furniture. Recommend
closing it the same way, reopening if the files appear.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Fifteen inbound. The
oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150, nor for
   Tim's rulings of 2026-08-25 and 2026-08-26.**
5. **The tone tracker** — the confirmation-rule ask stands; fist-quality
   selection is unmeasured.
6. **The integrator width** — bears on `014113`/`014308`.
7. **The guard's gap is two to one**, calibrated on two empty captures.
8. **A boxcar's nulls made two of five swept offsets pathological best cases.**
9. **Two stations closer than 125 Hz are not named.**
10. **The keying meter** — hidden behind a setting; the rebuild is its own unit.
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
12. **The margin does not separate and the reason rules out differences of
    log-likelihoods generally** (2026-08-26, unit 1.11.14). Answered in part by
    task 4: the *quotient* escapes the scale problem and still does not
    separate.
13. **A fifth intermittent**,
    `Rig.ScopeOutputWriteTests.ConfirmedNeedsTheReadbackToAgree` (2026-08-26,
    unit 1.11.14).
14. **The three captures of 2026-08-26**, asked a fifth time, above.
15. **The survey's time resolution**, above. The headline ask of this unit.

New this unit: **the survey's hop cannot resolve a fast fist**, above; **the
valve as ruled admits noise more often than the station**, above.

Closed this unit: **the held pitch outliving its evidence**, shipped; **the
first distribution of `marginLlr / spanLlr`**, measured and logged.

Still open: **the lock's mixed help**; **the "Hold this pitch" button**; **three
fixtures at accepted cost**; **`001520`'s quadrillions**; **the reference and
port integrator difference**; **`CLAUDE_CODE.md`'s version line**; **an
unmeasured pitch costs `N4L`**; **`014113`/`014308`'s second mechanism**; **the
six-hertz window disagreement**; **the short-character bias**; **the Avalonia
geometry offset, still unexplained**; **`CHANGELOG.md` at 1.9.0 against
1.11.15**; **the whole-file second pass**; **the confirmation rule cannot admit
an intermittent station**; **the squelch has no axis**.
