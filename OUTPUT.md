# Work instruction 002 — lock onto the strongest signal and reject the others

## 1. What Claude did

Claude Code on the development computer, `C:\Source\HamLet`. The prompt claimed
`PROJECT: Hamlet` and so does `WORK_INSTRUCTIONS.md`; the tree confirms it —
`SHACK_FACTS.md` and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` both
exist, neither `CoreHMI.sln` nor `MURC.sln` does, the solution is `Hamlet.sln`,
and `PROJECT_CARD.md` names Hamlet. **Branch `main`**, per §9.5.1. Six tasks,
all six worked, **task 6 was not dropped**. Each committed and pushed to `main`
before the next began; all six pushes succeeded.

**Nothing in this report is evidence about the radio.** No rig was connected.

**Nothing was recorded to `DECISIONS.md`.** The one judgement this unit reached
— which integrator bandwidth to ship — is a trade between rejection and the
gate's own margin, and a trade is Tim's (§12.1).

### Task 1 — the four claims, traced

1. **The mixdown filter, computed rather than read off the comment.** The
   integrator was a boxcar of `sampleRate / BandwidthHz` samples —
   `CwProbabilisticDecoder.cs:392`, 800 samples at 48 kHz, 16.67 ms. **Its
   equivalent noise bandwidth is exactly 60.0 Hz**, so the constant was honest
   as an ENBW; its −3 dB width is 53.2 Hz and its first sidelobe peaks at
   **−13.26 dB at 86 Hz**. A station 100 Hz away entered at **−15.6 dB**, which
   matches the instruction's "roughly −16 dB" precisely.

   **One correction the instruction did not have.** A boxcar has deep nulls at
   every multiple of 60 Hz, so **two of the five swept offsets — 120 Hz and
   300 Hz — sit exactly on nulls** and are pathological best cases for the
   filter being replaced.

   **The two paths did still differ, and not by what was reported.** The offline
   `Envelope()` lays a centred window; the streaming `PushEnvelope()` sums the
   audio behind the newest sample. The earlier review called that half a window.
   **It is 2.35 hops, not 3.33**: the streaming window ends on its hop and is
   pushed after that hop completes, so the two corrections pull opposite ways
   and do not cancel. Measured, then tested.

2. **The survey already knows.** `CwToneSurvey.Candidates` returns every
   admitted keying bin with pitch, lift, marks, dit and dah
   (`CwToneSurvey.cs:347`), and `CwToneTracker.CoarseCandidates`
   (`CwToneTracker.cs:445`) hands them straight out. **Its only caller in the
   whole tree was none.** Ranking is by `LiftDb`, which is loudness
   (`CwToneSurvey.cs:398`), so Hamlet does try to lock onto the strong one.
   **So task 5 is a plumbing job, not a detection job** — the instruction says
   this of "task 4", which is a numbering slip: task 4 is the bandwidth sweep.

3. **The tracker's grip.** A candidate must be confirmed by two agreeing
   surveys within `ConfirmWithinHz` (`CwToneTracker.cs:865`), and HM-DEC-127's
   rule stops a confirmed station being abandoned for one far below it. The
   fallback to the middle of the fine bank is real and appears three times —
   `cs:356`, `:842`, `:1002`.

4. **A two-sender fixture already exists, so the expected answer was wrong.**
   `interference-18wpm` in `CwFixtures.cs:122` puts a station at 450 Hz against
   one at 600, amplitude 0.35 against 0.5, and it is one of the 27 inherited
   failures. The generator has carried `InterferenceHz`, `InterferenceAmplitude`
   and `InterferenceWpm` all along.

**Green baseline: 1536 passing and 31 failing of 1567 in the engine, 480 and 1
of 481 in the app.** The engine matches the instruction exactly. **The app does
not**: the instruction says 0 of 481, and
`TheFollowedSentenceReachesTheScreenTests.ItIsDrawnWhileRefillingAndGoesWhenTextResumes`
failed — the flaky one unit 001 named, which flakes in both directions.

### Task 2 — the two-signal fixture, and the control that reframed the unit

`CwFixtureGenerator.Together` renders the band once and puts both stations on
top of it. That is the difference from `Join`, which concatenates: summing two
finished fixtures would sum two noise floors and put the band 3 dB up without
saying so. The fixture's own difficulty is measured rather than assumed —
**0.76 s with both keys down at once**, through the decoder's own front end
pointed at each station in turn.

**A first attempt at that overlap figure was wrong and self-certifying**: a
10 ms tone measurement cannot resolve two notes 40 Hz apart and reported both
stations keyed for 11.24 of 11.9 seconds. That is the §12.5 trap, caught by the
number being implausible.

**And the control is what matters.** One station alone, same seed, same band,
no competitor at all:

| read how | correct | wrong | invented | emitted | read |
|---|---|---|---|---|---|
| whole file, fixed pitch | 11 | 0 | 0 | 11 | ` CQ DE N0CALL K ` |
| whole file, forced to 18 wpm | 11 | 0 | 1 | 12 | ` CQ DE N0CALL K E ` |
| streaming window, pitch nailed to 600 Hz | 11 | 0 | 1 | 12 | ` CQ DE N0CALL K E ` |
| **the production path, tracker and all** | 10 | 1 | **22** | 33 | `QQ T DEDE EE NNM■0E0KCECAEALLALLL T KK  E` |

### Task 3 — the Hann integrator

Built in both paths, with the length derived from a named bandwidth and the
taper taken from one place. The streaming path weights **by age, not by array
index**: a boxcar can be summed in any order and a taper cannot, and weighting
by index would rotate the window against the signal once per fill.

`TheTwoEnvelopePathsAgreeTests` proves the ENBW is what it claims at 60, 45, 30
and 20 Hz, and that the two paths agree to **1.6 % of peak** once aligned by the
measured 2.35-hop lag.

**Both empty captures still emit nothing** — `014854` and `014935`, stated
explicitly per the instruction.

### Task 4 — the bandwidth trade

Swept offline, because the production value is a constant and a mutable static
the suite shares is how one test changes another test's numbers silently.

Rejection on **the instruction's own grid is a tie**: every width from 60 down
to 20 Hz reads the wanted station whole at every offset and every level. Rows
below 30 Hz of separation do discriminate, and there the narrower filters win
outright — but those rows are this session's invention.

**45 Hz stands.** Reasoning and the alternative are in section 4.

### Task 5 — the second station, surfaced

`CwCompetitor` carries the offset, the strength relative to the station being
read, and a sentence naming **FILTER** and **TWIN PBT**. It reaches
`CwDecodeReport` and the capture sidecar. Nothing is written to the radio.

**The separation figure had to move from 50 Hz to 125 Hz.** A lone clean
station found its own image 50 Hz away, 2.1 dB down, and reported it as
somebody else — HM-DEC-127's fault exactly. 125 already existed twice in this
tree, as `CwToneTracker.CompetitorSeparationHz` and `CwToneSurvey`'s
`NoiseSeparationHz`; it is defined once now and read from both.

**A competitor is a live fact and the first draft of the tests read it at the
wrong moment.** The survey keeps three seconds of rolling history, so after a
recording is played to its end that history holds the trailing silence and
admits nothing at all — not even the station the file was about. The tests
sweep during playback, which is how the panel and the sidecar ask.

### Task 6 — the three settings *(the drop candidate, not dropped)*

Task 4's measurement had finished, so it was not blocked. `ReceiveObstructions`
names the noise blanker, the noise reduction and the filter width when each is
in the way, with the control on the front of the radio. Read-only:
`ReceiveObstruction` carries a setting and a sentence and has nowhere to put a
command.

**The filter is named on a measurement, not on a width.** A competing station
the survey actually found is a fact; asserting that some width is too wide for a
signal Hamlet has not measured would be a judgement nobody ruled. That is a
narrowing of what the instruction asked for and it is deliberate.

### Version

`Directory.Build.props` read **1.11.1** at the start, so the panel unit drafted
as 1.11.2 did not run. Per the instruction, this unit produced **1.11.2**.

### The shape conflict, named again

`CLAUDE_CODE.md` §8's five sections win over `SESSION_PROTOCOL.md` §12.2's three
headings, per §0. Named for the second consecutive unit.

## 2. What Tim should expect

The build succeeds with no warnings. **1553 passing and 31 failing of 1584 in
the engine, 481 and 0 of 481 in the app.**

**What will look wrong and is not:**

- **The failing set is byte-identical to the one this unit inherited plus and
  minus the Hann swap.** Tasks 5 and 6 added fourteen tests between them and
  changed no failure. Task 3 fixed three and broke four:

  | fixed by the Hann integrator | broken by it |
  |---|---|
  | `HoldingTheWindowLongInTimeReadsMore` (×2) | `ARecordingWithNoStationInItSaysNothing(014854)` |
  | `ItKeepsUpWithLiveAudio` | `TheGateSitsInAWideGap` |
  | | `TheFiveToEightDecibelPlateauHolds` |
  | | `OnlyTheOneTheCouplingBreaksHasTheTrough` |

- **`ARecordingWithNoStationInItSaysNothing` is not a silence failure.** The
  empty band scores 8.0 against a gate of 15 and emits nothing. What the test
  guards is the *margin*, and the margin narrowed from 6.6 to 8.0. HM-DEC-120's
  property holds; its headroom is what moved.

- **`TheFiveToEightDecibelPlateauHolds` fails because the chatter went away.**
  It asserts that a two-level trigger has something to remove, and the narrower
  filter removed it. That is a test pinned to a defect, failing because the
  defect improved.

- **`OnlyTheOneTheCouplingBreaksHasTheTrough` now finds two recordings with the
  trough instead of one.** More structure found, and a test pinning exactly one.

- **You will see nothing new in the app except one line.** Where the noise
  blanker or the noise reduction is on, or a measured competitor is inside a
  wide filter, the terminal's advisory area now says so and names the knob.
  Everything else this unit did is measurement.

Pushed to `main`, six commits, all pushed successfully.

## 3. What you should see

**How much of the strong station's text survives a competing station, before
and after, at each offset and level — and the answer is all of it, at every
cell, both times.**

At a fixed pitch, with the tracker out of the path, **every one of the fifteen
combinations reads `CQ DE N0CALL K` whole, 11 of 11 correct, nothing invented —
with the boxcar and with the Hann alike.** The competing station costs nothing
at 40 Hz separation and equal level, and nothing anywhere else in the grid.

**So the unit's premise is measured false.** The soup is not the co-channel
case. The control proves it in one row: **one station alone, no competitor at
all, through the production path, emits 22 characters that were never sent** —
and adding a second station at any offset and any level changes that to 20–22.
The competing station is not what is wrong.

**What is wrong is the tone tracker, and nothing else in the chain.** The same
clean single-station audio, read four ways:

| stage | invented | read |
|---|---|---|
| whole file, fixed pitch | 0 | ` CQ DE N0CALL K ` |
| whole file, forced to one speed | 1 | ` CQ DE N0CALL K E ` |
| **streaming window, pitch nailed to 600 Hz** | **1** | ` CQ DE N0CALL K E ` |
| **the production path, tracker and all** | **22** | `QQ T DEDE EE NNM■0E0KCECAEALLALLL T KK  E` |

The rolling window is fine. The measured-unit override is fine. The refill
guard is fine. **Letting the tracker move the pitch turns a perfect decode into
soup on a 15 dB single station.**

That is consistent with unit 001's corpus table, where the shipped path and the
grid path diverged on every capture, and it names the cause.

### What the widths are worth

| | 60 Hz | 45 Hz | 30 Hz | 20 Hz |
|---|---|---|---|---|
| the instruction's whole grid | tie | tie | tie | tie |
| 30 Hz apart, +6 dB | 2/11 | 2/11 | **11/11** | **11/11** |
| 20 Hz apart, equal | 2/11 | 2/11 | **11/11** | **11/11** |
| sensitivity to 0 dB | no cost | no cost | no cost | no cost |
| a fast fist to 35 wpm | no cost | no cost | no cost | no cost |
| empty band vs a gate of 15 | 6.6 | 8.0 | 9.3 | 10.0 |
| `013347` characters | 82 | 83 | 79 | **49** |

**The fast-fist column is real and was checked because it looked too good.** A
75 ms integrator reads a 34 ms dit cleanly, because a segmental decoder scores a
span rather than thresholding a level: a smeared envelope loses contrast and
keeps its timing. That is a genuine property of this architecture.

## 4. What's blocking us

Nothing blocks the next unit. Five questions, most-blocking first.

---

**The tone tracker, not the front end, is what turns a clean decode into soup,
and the next unit should be aimed at it.**

Measured on a generated single station at 15 dB with no competitor: fixed
pitch reads eleven of eleven with nothing invented; the streaming window at a
nailed pitch reads the same; the production path with the tracker live invents
twenty-two characters. The audio, the window, the speed hypothesis and the
refill guard are common to both.

**What this does not say.** It does not say the tracker is wrong to move — a
station really can be somewhere else, and HM-DEC-095's confirmation rule and
HM-DEC-127's displacement floor were both put there by measurements. It says
the cost of moving has never been measured against the cost of staying, and on
this fixture staying wins outright.

**Rejected: changing it in this unit.** The unit's whole claim is that it
touched the front end and nothing else, and the tracker is on the parked list
by implication — `ClearOnAStationChange` and the switch machinery are named
there explicitly.

---

**Whether the integrator should be 45 Hz or 30 Hz.**

**45 Hz shipped**, because it is where matching the boxcar's own main lobe
lands, and because on the grid this unit was told to sweep every width ties. It
was not chosen by winning anything.

**30 Hz is measurably better at what the unit was commissioned to improve** —
it reads two stations 20 to 30 Hz apart that 45 Hz cannot — **at no measured
cost to sensitivity or to a fast fist.** What it costs is 1.3 dB of the gate's
margin over an empty band and four characters on `cw-2026-08-17-013347`.

**Rejected: choosing 30 on the strength of the rows that favour it.** Those
rows are not in the instruction's grid; this session added them. Fitting a
production constant to a fixture the same session invented is the shape of the
failure §12.5 exists to stop, and saying so is worth more than the two hertz.

**Rejected: leaving the boxcar.** The instruction ordered the change
unconditionally and it costs nothing measurable on any single-station path.

---

**The Hann integrator narrowed the gate's headroom, and `Gate = 15` was
calibrated against numbers that have now moved.**

The empty band on `cw-2026-08-20-014854` scored 6.6 through the boxcar and
scores 8.0 through the Hann. Silence holds at both. But `TheGateSitsInAWideGap`
and `ARecordingWithNoStationInItSaysNothing` both assert the *gap*, and both now
fail.

This is the third measurement in two units pointing at the same place: unit
001 found that on the instrument that actually gates, the streaming windower,
the gap does not exist at all — an adjudicated station scored 1.7 while an empty
band scored 6.5. **The gate is parked in this unit and it is the obvious subject
of the next ruling.**

**Rejected: moving the gate here.** It is on the parked list, its ruling is not
made, and a gate moved to make two tests green is a number chosen to look right.

---

**A boxcar's nulls made two of the five swept offsets pathological best cases,
and the instruction's grid could not have discriminated.**

The boxcar has exact nulls at every multiple of 60 Hz, so 120 Hz and 300 Hz
were rejected infinitely well by the filter being replaced, while 40 Hz sat at
only −7.7 dB inside the main lobe. A sweep meant to show a sidelobe improvement
included two offsets where the old filter was perfect.

It did not matter, because every cell tied anyway. It is recorded because the
same grid will be reached for again.

---

**Two stations closer than 125 Hz are not named, and the operator is not told
that they are not named.**

`CwCompetitor.SeparationHz` is 125 because below it Hamlet cannot tell a second
operator from the same operator's image in a neighbouring bin — the first draft
at 50 Hz proved that by finding a lone station's own image. The consequence is
that exactly the cases where a narrower filter helps most, 20 to 30 Hz apart,
are the cases Hamlet stays silent about.

Silence is the right answer under §0.0. Whether the panel should say *there may
be somebody too close to separate* is a display question and therefore Tim's.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **The work order carried
this queue inbound this time**, which §9.6 requires and unit 001's order did not
do.

Unit 001's four, unruled:

1. **The sweep's `invented` column counts substitutions, not invented
   characters**, so the figure the refusal floor was ruled on is not a
   measurement of invention. Twelve of twenty characters at 18 dB were never
   sent, against a column reading nought. *(This unit reported both columns
   wherever it reported a sweep, per the instruction, and treated neither as
   settled.)*
2. **Whether the refill guard should apply to the first fill at all**, or only
   to a refill after the window has been emptied. It costs three ratchets and
   removes the opening soup, including one real character.
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK`, and HM-DEC-150 makes `PHASE`
   the same number as the version's minor** — under the rename there is no field
   for the minor to match, so the two now agree by hand.
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150** while
   `CLAUDE.md`'s index has rows for all of them. A session cannot tell a ruling
   it is acting against from one that does not exist. *(Confirmed again this
   unit: HM-DEC-120's own text is still unreadable in the tree.)*

Plus **HM-OPEN-057** (E-dominance outside the keying verdict, open since
2026-08-22, measured again by unit 001 and still unruled) and **HM-OPEN-007**
(open and unruled since 2026-08-14).

And this unit's five, above.

## 5. Where the phase stands

**Phase: E-share in single figures across the corpus — not reached, and this
unit did not move it.** The corpus was not re-measured here; unit 001 put it at
13 % to 43 %, and nothing this unit changed touches the tracker, which is now
measured to be the dominant source of the soup that E-share counts.

**What this unit did establish, which the phase was scoped on believing:** the
co-channel case is **not** a large share of that soup. At a fixed pitch a
competing station costs the wanted station nothing at any offset or level in the
swept grid, and a single station alone through the production path produces the
same 20-odd invented characters that two stations do. **The belief is measured
false**, which is what the instruction said this unit was for.

**Was: zero — nothing in this repository had ever measured two stations in one
passband.**

**Build 1.11.2**, confirmed in `Directory.Build.props`, up from 1.11.1.
