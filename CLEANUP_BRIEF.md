**PROJECT: Hamlet**

# Work order: read the loud ones whole

Six phases. Reported per §12.2: four sections, **written to `OUTPUT.md` at the
repository root, overwriting it**, and printed to the session as well. **Name
the branch in section 1** (§9.5.1 — it is `main`).

**Read first:** `CLAUDE.md` (§0.0, §0.2.1, §4, §9.5.1, §12),
`SESSION_PROTOCOL.md`, the previous `OUTPUT.md`, `OPEN_ISSUES.md`,
`DECISIONS.md`.

**Rulings in force that the last session did not have**: HM-DEC-111 through 115.
Phases 1 through 4 are four of them. The last session ran an older copy of this
brief and its phases 1, 2, 3, 4 and 6 are done — do not repeat them.

## Standing instruction

A phase needing a ruling records the question in `OUTPUT.md` section 4 and
continues to the next phase. §12.1 unchanged.

**No transmit work of any kind.** Auto-CQ is HM-DEC-098; build nothing toward
it.

---

## Phase 0 — confirm the tree, then commit only to `main`

Tim merged and pushed by hand. `main` is at `901155d`, `feature/honest-cw-detection`
is deleted local and remote, `git diff --stat` between them was empty before the
delete, and `git branch -a` shows `main` alone.

Confirm it, then work on `main` and nowhere else (HM-DEC-113, §9.5.1). Say the
branch in section 1 of the report.

**Also record, and do not chase**: something on that machine commits as `"save"`
while a session runs — it caught the last session's phase 1 work at `20c8ae5`
and discarded the message. Harmless to content, corrosive to history. Note it in
`OPEN_ISSUES.md`.

---

## Phase 1 — gap classes come from the gaps, because operators send Farnsworth (HM-DEC-115)

**This is the phase that gets Tim his transcript. It is first because it is
worth more than everything below it.**

He spent an evening at the radio. The signal he described as sounding next door
is `cw-2026-08-18-004507`: a 40 m traffic net carrying an ARRL bulletin, S4,
tone 501 Hz, `snrDb 43.0`, `inputPeak -13.1 dBFS`. Hamlet emitted **177
characters of which 94 were unsure** and the transcript was unreadable.

Independent analysis of the same file finds the gaps in three clean, widely
separated heaps:

| Gap class | Measured | Count |
|---|---|---|
| Element | **40 ms** | 69 |
| Character | **190–300 ms** | 28 |
| Word | **400 ms and above** | 11 |

The dit measures **57 ms** and the dah **158 ms**, ratio 2.79, about 21 WPM
character speed. So **the element gap is shorter than a dit, and the character
gap is six times the element gap rather than three.**

That is Farnsworth spacing. ARRL bulletins and NTS traffic nets are sent with
it, and it is not exotic — it is close to the most likely thing a beginner tunes
across. **Nothing about a 1:3:7 assumption survives it.** A decoder using dit
multiples gets every character right and puts every space in the wrong place,
which is exactly the transcript on his screen.

Cutting on the measured heaps instead — 100 ms and 350 ms, derived from the data
and from nothing else — reads from the same audio:

> `AT ARRL DOT NET <BT> EACH STATION HANDLING THIS MESSAGE P`

**Every character correct after the first four, which are acquisition.**

HM-DEC-048 ruled in advance that gap classes are clustered from the gaps
themselves and never from dit multiples. The code does not do it. The last
session found part of this independently — a fallback at 0.85 dit, below a
textbook element gap, and an outlier-driven boundary in `GapCuts` — and fixed
the fallbacks to **two dits and five, which is still dit multiples**. Two dits is
114 ms here; the character gap is 240.

Do this:

- Cluster the observed gaps into three classes, continuously, per signal.
  Three heaps this well separated need no cleverness.
- **Remove every remaining dit-multiple fallback from gap classification.**
  Where there are too few gaps to cluster, emit nothing rather than guess a
  multiple — §0.0 prefers silence and already says so.
- **Report the measured element, character and word gap for the current
  signal**, so a Farnsworth sender is visible rather than merely survived.
- A sender whose character gap is a large multiple of the element gap is a
  normal operator, not an anomaly. Nothing warns about it.

## Phase 2 — half-amplitude edges for the clock fit (HM-DEC-112)

The gate threshold sits about eight decibels below the mark level, so it catches
the rising and falling skirts: **every mark is measured too long and every gap
too short, by the detector's own rise time.** Negligible at eleven words a
minute and ruinous at twenty-five.

Measured on `cw-2026-08-18-003126`, cluster-midpoint threshold against detection
bandwidth:

| Bandwidth | dit | dah | ratio | outcome |
|---|---|---|---|---|
| 12 Hz | 64.9 ms | 156.0 | **2.41** | **clock rejected, below the 2.50 floor** |
| 25 Hz | 49.0 | 150.1 | 3.06 | 24.5 WPM |
| 40 Hz | 45.5 | 147.2 | 3.24 | 26.4 WPM |
| 90 Hz | 47.6 | 146.1 | 3.07 | 25.2 WPM |

The same audio with edges at **half amplitude, 6 dB below the local mark
level**:

| Bandwidth | dit | ratio | WPM |
|---|---|---|---|
| 12 Hz | 46.4 | 2.89 | 25.9 |
| 20 Hz | 44.5 | 3.10 | 27.0 |
| 25 Hz | 46.5 | 3.00 | 25.8 |
| 40 Hz | 47.8 | 2.93 | 25.1 |

**The bias disappears.** Take element edges for the clock fit at half amplitude.

Three paths, three thresholds, and they are not the same question:

1. **`CwToneSurvey`** keeps the cluster midpoint — HM-OPEN-023. Applying the
   correction there loses the 13:47 capture's tone entirely, and
   `CwSurveyThresholdPinTests` guards it. Do not disturb that test.
2. **Element extraction and the clock fit** move to half amplitude. This is the
   change.
3. **The settled pass** already uses it (HM-DEC-105).

**One constraint found while measuring**: widening the detection bandwidth
raises the count of sub-15 ms noise marks sharply — 192 of them at 60 Hz on one
capture — and they drag the dit cluster down. **De-glitch before the clock fit,
with a threshold that does not come from the clock it is trying to establish.**

Bandwidth-following-speed is real and is the reference's own unimplemented
specification. It is deliberately **not** in this phase, so the improvement is
attributable. Record what half amplitude alone does to `ACleanSignalDecodesExactly(25)`
and to `fast-easy`.

## Phase 3 — build the sensitivity floor, at 17 (HM-DEC-097, ruled at last)

**HM-DEC-097 ruled the refusal and it was never built.** The last session proved
it: nothing in the decoder implements a floor. The streaming pass gates on
coherence and a plausible speed; the settled pass on six decibels of contrast;
neither is what the ruling describes. The sweep it printed:

```
 18.0 dB → 1.0 dB   right 1.00   wrong 0.00   emitted 9   (every step)
  0.0 dB            right 0.92   wrong 0.14   emitted 9
 -1.0 dB            right 0.81   wrong 0.19   emitted 9
 -2.0 dB            right 0.53   wrong 0.44   emitted 9
 -3.0 dB            right 0.14   wrong 0.25   emitted 3
 -6.0 dB and below                            emitted 0
```

**Perfect from 18 dB down to 1 dB, then a cliff.** It keeps emitting nine
characters all the way to −5.

The ruling's decibels are the broadband ratio the fixture was generated at, and
the decoder measures inside a narrow tone filter, reading about seventeen
decibels higher for the same audio: 17.2–19.0 where the fixture was generated at
0 dB, 15.3–17.1 at −2, 7.6–14.4 at −5.

**Tim ruled: the floor is 17 in the decoder's own margin units.** That is
HM-DEC-097 translated rather than renegotiated — it cuts in at the ruling's own
line. Fifteen was rejected because it lets −2 dB through, which is the case the
ruling names at 0.44 invented. Ten was rejected as leaving open the whole band
the ruling exists to silence.

Acceptance: the sweep is unchanged from 18 dB to 1 dB, and emits nothing below
the cut. `ItGoesQuietRatherThanInventingLettersInTheNoise` passes on the floor
existing, not on its bound moving.

## Phase 4 — the strong-signal bar (HM-DEC-114)

**Ruled: at 15 dB or better in the passband with a steady fist, the decoder
emits the message with zero strangers and zero placeholders, or it is a
defect.**

Convert the easy tiers and `exchange-easy` from ratchets to pass-or-fail.
Ratchets were correct while the audio was unproved; the fixtures are proved now,
so a ratchet records that the decoder is still wrong without ever requiring it
to stop being wrong.

A bar phased by speed was rejected: it would licence 25 WPM to stay broken by
design, and 25 WPM is what is on the air.

**Expect the suite to go red here and leave it red if the bar is not met.** A
test saying *this signal is loud and clean and we cannot read it* is the correct
state of the world and the whole point of the ruling.

## Phase 5 — the four off-air captures become fixtures

The project has never had a real recording with an answer key. It now has four,
all from one evening, all on `main` already or supplied alongside.

| Capture | What it is | Answer key |
|---|---|---|
| `cw-2026-08-18-004507` | ARRL bulletin, 40 m net, S4, 501 Hz, 21 WPM **Farnsworth** | `AT ARRL DOT NET <BT> EACH STATION HANDLING THIS MESSAGE P` after acquisition |
| `cw-2026-08-18-003758` | Same net, station checking in, 501 Hz, ~27 WPM | contains `AA4MP/4` and `QNI` — Hamlet read `DE AA4MP/4 QNIK` on screen and was independently confirmed correct |
| `cw-2026-08-18-003126` | 669 Hz, ~25 WPM, the half-amplitude evidence | contains `<BT>`, `<AR>`, `VFB`, `MY`, `IT` |
| `cw-2026-08-18-003016` | Same station, 30 s earlier | no full key; use for tone tracking only |

**These are off-air recordings of other people's transmissions.** HM-DEC-048
governs what may be committed. If that ruling forbids them as fixtures, say so
and stop — do not decide it. If it permits them, commit them with their sidecars
and their keys, and **assert the keys**.

Two of them are the only evidence in the project that Hamlet has ever read a
real station correctly. `cw-2026-08-18-003758` is a regression test for a
success, which the suite has none of.

**Also worth knowing and not yet used**: `QNI`, `QNX` and `<AS>` in these
recordings are ARRL QN signals — net check-in, net close, and wait. Hamlet reads
them and cannot name them. That is scanner-classifier work and is **not** in this
order; record it in `FUTURE_GOALS.md` or `OPEN_ISSUES.md` as you judge best.

## Phase 6 — DROP THIS ONE IF SHORT OF ROOM

Three small things from Tim's evening at the radio and the last report:

- **The provisional tip renders as a solid green block rather than an italic
  tail.** Confirm whether that is the intended rendering or the tip failing to
  firm up. One screenshot showed `I■` at the leading edge with the block
  persisting.
- **"the second pass has changed its mind about 27 characters" has no scale.**
  Nobody knows whether 27 in a window is normal or alarming. Give the number a
  denominator, or a baseline measured across the fixtures.
- **The first spot refresh runs on the remembered band.** Defect report from the
  last session's section 4: `ReloadSpotsAsync("startup")` runs from the view
  model's constructor, the radio is not connected until the window's `Opened`
  event, and the band scopes what RBN is filtered to and what the skimmer watch
  listens for. It self-corrects on the first band change and has only been seen
  with the training radio. **The trade-off is a burst of wrong-band calls
  against an empty panel for a second or two, so it is Tim's** — put it in
  section 4 with a recommendation rather than fixing it.

If dropped, say so.

---

**If every phase completes, stop and report. Do not start any other work unit,
and build nothing toward auto-CQ.**

## Definition of done

`cw-2026-08-18-004507` decodes to its answer key, or the reason it does not is a
number in section 2. The sensitivity floor exists at 17 and the sweep above 1 dB
is untouched. The easy tiers are pass-or-fail rather than ratchets. Every commit
is on `main`.

**Everything here is provable on the development computer against fixtures, and
none of it is evidence about the radio** (HM-DEC-093). Tim verifies on COM3
against the same net, which runs nightly.
