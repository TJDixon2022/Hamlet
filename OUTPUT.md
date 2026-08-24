# Work instruction 009 — find the pitch, and hold it, without being asked

## 1. What Claude did

Claude Code on the development computer, `C:\Source\HamLet`. The prompt claimed
`PROJECT: Hamlet` and so does `WORK_INSTRUCTIONS.md`; the tree confirms it —
`SHACK_FACTS.md` and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` exist,
neither `CoreHMI.sln` nor `MURC.sln` does, the solution is `Hamlet.sln`, and
`PROJECT_CARD.md` names Hamlet. **Branch `main`**, per §9.5.1. Five tasks; task 5,
the drop candidate, was **not dropped**. Every push succeeded; none was refused.

**Nothing in this report is evidence about the radio.** No rig was connected.

**Nothing was recorded to `DECISIONS.md`.**

**Report shape**: `CLAUDE_CODE.md` §8 says **four** sections; its version line
still reads 1.3, unchanged from when it said five. Followed the section count.
Ninth consecutive unit naming the conflict with `SESSION_PROTOCOL.md` §12.2.

### The two rulings this unit's subsystem answers to, transcribed

**HM-DEC-095** — *"A note is chosen by how it is keyed and never by how loud it
is, the operator's own transmission is not evidence about anybody else, and a
sender's gaps are classified by clustering that sender's own gaps… The old
detector was wrong on all three real recordings, including one answer that is
neither the loudest thing nor the real one nor the configured pitch… What
[separates them] is whether the mark lengths are two clusters or one smear."*

**HM-DEC-127** — *"A confirmed station is not abandoned for a candidate far below
it… the chosen bin carries the same dit and the same dah as the station being
read, thirty-five decibels quieter… It is the station's own image. HM-DEC-095
settled that a note is chosen by how it is keyed and never by how loud it is,
which was about which signal to read on an empty-handed survey; it never settled
whether a candidate may take the tracker away from a station already
confirmed."*

**What they forbid, and it governed every design choice here:** a transform peak
is a *loudness* measurement, so it may never choose which note to read. Every
pitch measurement in this unit refines a candidate the survey has already
admitted on its keying structure, or is scoped to the case where nothing was
admitted at all.

### Mismatches with the instruction

**The line numbers had moved, as warned.** The fallback to the fine-bank centre
is at `CwToneTracker.cs:362`, `:423`, `:969` and `:1129`, not `:356`, `:842`,
`:1002`.

**"Two right out of fourteen" is out of date.** Measured in-tree today by
full-length interpolated transform peak, **Hamlet reports the pitch within one
hertz on six of the fourteen**: `134712`, `004507`, `003016`, `003758`, `001520`
and `012403`.

**And `012403` in particular is not bracketing anything.** The instruction has
`toneHz` at 450 and the sweep bin at 425 around a station at 439.81. In-tree
today Hamlet reports **440.0 against a measured 440.09 — an error of 0.1 Hz.**
That measurement predates unit 1.11.5, which changed the tracker's inputs.

### Task 1 — the pitch error, and what it costs

| capture | true | reported | error |
|---|---|---|---|
| `013347` | 613.14 | 625.0 | +11.9 |
| `013622` | 611.89 | 600.0 | −11.9 |
| `134712` | 500.09 | 500.0 | **−0.1** |
| `004507` | 500.81 | 500.0 | −0.8 |
| `003016` | 668.89 | 670.0 | +1.1 |
| `003126` | 668.96 | 665.0 | −4.0 |
| `003758` | 500.13 | 500.0 | **−0.1** |
| `014854` *(empty)* | 608.73 | 600.0 | −8.7 |
| `014935` *(empty)* | 616.23 | **825.0** | **+208.8** |
| `014113` | 606.98 | 600.0 | −7.0 |
| `014308` | 606.23 | 575.0 | −31.2 |
| `031905` | 499.79 | **300.0** | **−199.8** |
| `001520` | 600.00 | 600.0 | **−0.0** |
| `012403` | 440.09 | 440.0 | **−0.1** |

**The two enormous errors are the fallback, not the estimator.** 300.0 and 825.0
are the ends of the tracker's range, reported when the survey has admitted
nothing.

**Green baseline: 33 failing of 1607 in the engine, 481 of 481 in the app.** The
instruction states 34 of 1605; the two new captures added two theory cases, both
passing, and one of the instruction's known-red is the flaky rig test.

### Task 2 — built, measured, and withdrawn

An interpolated transform peak in the fine bank's own neighbourhood, applied
wherever the survey had admitted a candidate.

**It is not shipped.** On the corpus it improved `004507` and `003126` by a hertz
or three and moved `003016` five hertz the wrong way — and none of those
recordings has a known pitch. **On the one signal whose pitch is known because it
was generated, 613.7 Hz, it answers 619.45 where the peak finder already in the
tree answers 613.64.** A refinement five hertz worse than what was there is not a
refinement, and shipping it on captures nobody can check is the §12.5 failure.

The method and its measurement stay in the file so the next session does not
rediscover it. The two read different windows — one the ring buffer through the
gate's taper, the other the fine bank through the survey's — which is
HM-DEC-119's lesson again.

### Task 3 — an unmeasured pitch says so

`HasMeasuredPitch` distinguishes a pitch measured from admitted keying from the
middle of whatever bank the tracker is pointed at. The capture sheet now
separates three states — measured, not measured, no tone — and writes the pitch
to a tenth of a hertz.

**Refusing to decode at an unmeasured pitch was built, measured, and withdrawn.**
Section 4 has the trade.

### Task 4 — the hold, and what released it

**Implemented as: the mixdown falls back to the last pitch the survey actually
measured, rather than to the bank centre.** Precedence is the operator's lock
first, then the last measured pitch, then the bank.

**What releases it: nothing but a better measurement.** Where the survey admits a
candidate the tracker's own rules choose it and the held pitch is replaced. There
is no timer and no keying detector, deliberately — a release on "the station
stopped" would need a judgement about whether it had, and the survey already
makes exactly that judgement three seconds at a time. What this removes is only
the swing back to a bank centre in the gaps between those judgements.

The operator's lock also takes the measured pitch where there is one, so it can
no longer lock onto a bank centre.

## 2. What Tim should expect

**The radio behaves as it did last night, with one thing that will not happen any
more: the decoder no longer swings its filter back to the middle of its bank
every time the survey's three seconds of history run dry.** On a slow sender that
is most of the time between characters.

**Every capture reads what it read**: `VA3VRR` on `013347`, `N4L` on `134712`,
`AA4MP/4 QNIK` on `003758`, the ARRL bulletin on `004507`, plain English on
`003016` and `003126`. Both empty captures emit nothing.

**`cw-2026-08-22-014113` and `cw-2026-08-22-014308` still read nothing, and
pitch was not the reason.** Section 3.

**The capture sheet's `toneHz` line changed.** It reads to a tenth of a hertz and
says whether the pitch was measured. If it says `NOT MEASURED`, the number beside
it is where the filter is pointed, not where a station is.

**Nothing on the panel changed.** The "Hold this pitch" button is exactly as unit
1.11.3 left it.

**What will look wrong and is not:**

- **33 failing of 1607 in the engine, 481 of 481 in the app** — the failing set is
  **byte-identical to the baseline this unit inherited**. Nothing broke and
  nothing was fixed by luck.
- **Two of those are the known accepted cost** (`clean-12wpm`, `clean-18wpm`,
  which contain exact digital silence) and one is the flaky rig test. Untouched,
  as instructed.
- **`ARecordingWithNoStationInItSaysNothing(014854)` is still green.**

## 3. What you should see

**How many of the fourteen captures Hamlet reports the correct pitch for:**

# **six, and it was already six**

Within one hertz on `134712`, `004507`, `003016`, `003758`, `001520` and
`012403`; within five on `003126` as well. **The instruction's "two out of
fourteen" was measured before unit 1.11.5 and is out of date** — and this unit
did not improve on six, because the refinement it built was measured worse than
what was already there and was withdrawn.

**The two genuinely wrong pitches are both the fallback**: 300.0 reported for a
station at 499.79, and 825.0 for a recording holding nothing. Those are the ends
of the tracker's range, not estimates.

**And whether `014113` and `014308` read:**

# **no — and pitch was not the fault**

| pitch | ratio | characters |
|---|---|---|
| 606.98 (measured) | **0.52** | 0 |
| 606.00 (as reported by the shack chain) | 0.52 | 0 |
| 600.00 (what Hamlet was pointed at) | 0.46 | 0 |

**Mixed down exactly at the measured pitch, a capture said to carry 19 dB of
keying scores 0.52 against a guard of 1.40.** Pointing at it perfectly changes
nothing.

**The reason is in the envelope, and it is a second mechanism Tim needs to know
about before this evening.** Compared with `004507`, which reads:

| | `014113` (unread) | `004507` (reads) |
|---|---|---|
| upper quartile | 0.0314 | **0.0986** |
| 97th percentile | 0.0753 | 0.1052 |
| P97 / P25 | 7.1 (17.0 dB) | 9.4 (19.4 dB) |
| hops where key-down beats key-up | 26.5 % | 37.0 % |

**On a recording that reads, the upper quartile sits close to the 97th
percentile — two clean states, key up and key down. On `014113` it sits at a
third of it.** The envelope is a continuum rather than two states, so there is no
keying structure at the true pitch for a segmental decoder to find, and it is
right to refuse it. HM-DEC-095's own words: what separates a station from
everything else is *"whether the mark lengths are two clusters or one smear"*.
This is one smear.

**Whether that smear is the band, the receiver, or Hamlet's own front end is not
settled by this unit**, and it is the thing to look at next. The recordings are
24 words a minute against `004507`'s 18, and the integrator spans 33 ms, which is
two thirds of a dit at that speed — that is a candidate and not a conclusion.

## 4. What's blocking us

---

**Refusing to decode at an unmeasured pitch costs `N4L`, and the reason is worth
a ruling.**

Built, measured, withdrawn. It costs text on seven captures and the adjudicated
`N4L` on `cw-2026-08-17-134712` outright.

**Why it costs that callsign is the interesting part.** `134712`'s fallback bank
centre is **500.0** and its station sits at **500.09**. The survey never admits a
candidate on that recording, so every character of `N4L` that Hamlet has ever
read came from mixing down at a number nobody measured, which happened to land on
the station to within a tenth of a hertz.

So the choice is between a callsign Hamlet reads by luck and a rule that says it
will not read at pitches nobody measured. §0.0 points one way and the operator's
evening points the other.

**Rejected: shipping it anyway.** A change that costs an adjudicated reading is
not a session's to make.

**Rejected: keeping the fallback silently.** It is what produced 300.0 for a
station at 499.79.

---

**A second mechanism silences two strong captures and it is not the pitch.**

`014113` and `014308` refuse at their measured pitch, and their envelopes have no
two-state structure to find. Three independent chains have failed on them, which
is consistent with the audio rather than with any one decoder.

**What would settle it**: whether the smear is present in the raw audio or is
introduced by the front end. A narrower or wider integrator would move it if it
is ours; nothing would if it is the band's. The integrator's width is parked
(45 Hz against 30 Hz, still unruled), and this is a second reason to settle it.

---

**Task 2's refinement is a better idea than its implementation, and the
implementation is in the tree unused.**

`Refined` sweeps the tracker's ring buffer through the gate's Hann taper;
`MeasuredPeakHz` interpolates the fine bank through the survey's window. On a
known 613.7 Hz signal they answer 619.45 and 613.64. **Which window a pitch is
measured through changes the answer by nearly six hertz**, and nothing in the
tree says which window is the right one to measure a pitch through.

Left in place with its measurement recorded, because the next session will
otherwise build it again.

---

**Seven adjudicable fixtures are available and none is adjudicated.**

The instruction reports the seven W1AW captures as ARRL Propagation Forecast
Bulletin ARLP034, machine-keyed at 17–19 WPM on a 499.9 Hz carrier, with text
confirmed by overlap across consecutive captures. This tree has one of them,
`031905`, which reads `PREDICTED 10.7 K NTIMETER FLUX IS 125`.

**Adjudicating them would roughly triple this project's answer keys**, and every
percentage this unit and the last two have reported rests on three. Named, not
done — §12.5 makes adjudication Tim's.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Eleven inbound, none
ruled, the oldest open since 2026-08-14. Ten consecutive units have now worked
beside rulings they cannot read.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150.**
   *(HM-DEC-095 and HM-DEC-127 govern this unit's subsystem; their index rows are
   transcribed in section 1 and the full records remain unreadable.)*
5. **The tone tracker is a large source of soup** — *narrowed by task 4's hold,
   not closed.*
6. **Whether the integrator ships at 45 Hz or 30 Hz** — *now also bears on the
   two unread captures.*
7. **The guard's gap is two to one**, 0.840 against 1.684, calibrated on two
   empty captures.
8. **A boxcar's nulls made two of five swept offsets pathological best cases.**
9. **Two stations closer than 125 Hz are not named and the operator is not told
   they are not named.**
10. **The keying witness is correct in 5 of 13 captures.**
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).

Still open from earlier units: **the lock helping sometimes and hurting sometimes
with nothing telling the operator which**; **the button added against
instruction**; **two clean fixtures dropped from 9 of 9 because they contain
exact digital silence**; **`001520` scoring in the quadrillions**; **the port and
its reference differing by an integrator**; **`CLAUDE_CODE.md` changing its
report contract without moving its version line.**

New from this unit: **refusing to decode at an unmeasured pitch costs `N4L`**;
**a second mechanism silences `014113` and `014308`**; **two pitch measurements
disagree by six hertz depending on the window**; **seven adjudicable fixtures are
unadjudicated.**

**Build 1.11.6**, confirmed in `Directory.Build.props`, up from 1.11.5.
