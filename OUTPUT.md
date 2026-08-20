# OUTPUT.md

## 1. What Claude did

### Task 1: the rule, stated before it was built

**A mark is set aside as not this sender's when the heights of the marks in the
window fall into two groups and it is in the low one.** Whether there are two
groups at all is asked by fitting two centres to the heights and measuring how far
apart they sit **counted in their own scatter**, which is the same test and the
same figure `CwToneSurvey.MinimumSeparation` already applies to mark lengths, and a
ratio rather than a level so it survives a fade. **It reads both statistics: a mark
is quiet only when its average height and its loudest moment are both on the low
side.** That is what answers `cw-2026-08-18-004507`'s stretched marks, real
elements of 90 to 275 ms that the gate held open across a dip, whose average sags
to 11–15 dB while their loudest moment stays with the plateau at 18–26; reading the
average alone discards them, and reading the loudest alone separates the callsign
window by only 4.4 dB against 10.1 for the average. **Where every mark is real
there is one height group and nothing is dropped**, which is `tightfist-easy` and
every clean fixture in the suite. **A window that is entirely chatter also has one
height group**, so an empty band is left exactly as it was: this rule cannot rescue
one and does not try (§0.0). Where the heights are not known at all, nothing is
judged (HM-DEC-091).

### Task 3: the nine captures and the easy tier

| capture | before | after | required | |
|---|---|---|---|---|
| `cw-2026-08-18-004507` | 25 | **26** | ≥ 25 | ✓ |
| `cw-2026-08-18-003016` | 38 | 38 | ≥ 38 | ✓ |
| `cw-2026-08-18-003126` | 34 | **36** | ≥ 34 | ✓ |
| `cw-2026-08-18-003758` | 14 | 14 | ≥ 14 | ✓ |
| `cw-2026-08-17-013347` | 8 | 8 | ≥ 8 | ✓ |
| `cw-2026-08-20-014854` | 1 | 1 | no more than 1 | ✓ |
| `cw-2026-08-20-014935` | 0 | 0 | 0 | ✓ |
| **the easy tier** | whole | **whole** | whole | ✓ |
| `cw-2026-08-17-134712` | 0 | **0** | reported | |
| `cw-2026-08-17-013622` | 0 | 0 | reported | |

**The easy tier is whole. Every fixture passes**, including `exchange-easy` and
`tightfist-easy`, which is where the previous attempt at this line failed.

**The dit, before and after:**

| capture | before | after |
|---|---|---|
| `cw-2026-08-17-013347` | 87.0 ms | 87.0 |
| `cw-2026-08-17-013622` | 21.6 | 21.6 |
| **`cw-2026-08-17-134712`** | **24.5** | **31.3** |
| `cw-2026-08-18-004507` | 54.2 | 54.2 |
| `cw-2026-08-18-003016` | 47.9 | 47.9 |
| `cw-2026-08-18-003126` | 49.8 | 49.8 |
| `cw-2026-08-18-003758` | 34.3 | 34.3 |
| `cw-2026-08-20-014854` | 42.7 | 42.7 |
| `cw-2026-08-20-014935` | 40.9 | 40.9 |

**These are end-of-file figures and only one moves, which is the point.** By the
last sample of every recording but `134712` the window holds one height population
and the rule does nothing, exactly as designed. **The number that matters is inside
the callsign**, where the rule sets aside eight to twelve of the twenty marks and
the dit rises from **35–40 ms to 46.5–51.0** against HM-DEC-144's hand-verified
**56.3**. Coherence reaches 0.49 there against a floor of 0.35, and
`LooksLikeMorse` goes true for the first time on that recording.

### Task 2: what shipped

Two changes, and they are one mechanism.

**`CwDecoder` carries each mark's height into the estimator.** `OnMarkEnded`
already had the mark's average signal-to-noise figure; a running maximum is kept
beside it, four instructions per hop, and both go in with the length.

**`CwSpeedEstimator` applies the rule before it fits anything**, and everything
downstream reads the survivors: the two-way length fit, `MedianOfShortCluster`, and
`MeasureCoherence`.

**`MedianOfShortCluster` is not deleted and its reason is unchanged.** It exists
because a handful of very short marks survive the gate on any real signal and an
average is defenceless against them (HM-DEC-095), and that is still true. What
changed is that it no longer reaches past the drop: it took the median of every
mark below the dit-and-dah cut, which on `134712` is 145 ms, so it put all nine
chatter slivers straight back into the estimate that had just excluded them.

**The three-way length fit was not needed and is not in the tree.** With the
chatter removed by height, the existing two-way fit on the survivors gives the dit
directly. That is why the easy tier survives this time: the previous attempt
separated the populations by length, which cannot tell a merged element from a
sliver, and dropped `tightfist-easy`'s real dits.

**One thing was got wrong and found by a test.** The separation test read a scatter
of nought as *no* separation, which is exactly backwards: two groups of identical
height are as far apart as two groups can be. It showed up on synthesized marks and
never on real audio, which always has scatter, and the floor is now a thousandth of
the gap rather than a level, so it stays a ratio.

### Task 4: the fixture

`ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt` **stays red.**
Nothing was tuned to make it pass.

**Where it dies now, and it is much further along than it was.**
`LooksLikeMorse` is true for the first time on this recording, from **22.55 s to
22.86 s**, with coherence at 0.42 to 0.49. **No character boundary falls inside
that window.** The gap before it, at 22.49 s, is 150 ms and ends the `4`; the gaps
inside are 25 to 40 ms element gaps; the next character ends after 23.01 s, by which
time coherence has fallen back. So the decoder believes it is hearing Morse for
about three hundred milliseconds in the middle of the final letter and releases
nothing during it.

**The remaining gap is `Refine`,** which holds the dit at 46.5–51.0 against 56.3 by
averaging in the sender's 35 ms element gaps. That is a fifth short, and a fifth is
the difference between coherence flickering and coherence holding.

### Task 5: `Refine`, measured and withdrawn again

Measured on top of everything above. **It does not ship.**

| capture | with the rule | with the rule and `Refine` |
|---|---|---|
| `cw-2026-08-17-134712` | 0 | **1** |
| `cw-2026-08-18-004507` | 26 | 26 |
| `cw-2026-08-18-003016` | 38 | **43** |
| `cw-2026-08-18-003126` | 36 | 36 |
| `cw-2026-08-18-003758` | 14 | **15** |
| **`cw-2026-08-20-014854`** | **1** | **5** |
| `cw-2026-08-20-014935` | 0 | 0 |

**`cw-2026-08-20-014854` reads `U EE ■ ■`** out of a recording the keying meter
finds no keying in at any pitch. The withdrawal condition is unchanged and
non-negotiable, so it is out.

**And the amplitude rule does not save it, for a reason worth stating.** That
recording is entirely chatter, so it has one height population, so the rule
correctly does nothing — which is the behaviour that keeps every clean fixture
safe. `Refine`'s removal then lengthens the dit until noise passes the coherence
check. **The rule protects a window with a station in it and cannot protect a
window without one.**

## 2. What Tim should expect

**Yes, something shipped, and no, it does not yet read
`cw-2026-08-17-134712`.**

What changed in what you will run tonight: the decoder now sets aside marks that
are too quiet to be the station's before it works out the speed. On every clean
signal and every empty band **that does nothing at all** — one height group, no
drop, identical behaviour. On a recording where a real station is buried in a gate
chattering on band noise, it keeps the station's marks and throws the chatter out
of the speed estimate.

**Two captures read more and none reads less**: `004507` from 25 characters to 26
and `003126` from 34 to 36. Neither has an adjudicated answer key, so **more
characters is not the same as more correct characters**, and you are looking at
`004507` this afternoon, which will say more about that than any count.

**The easy tier is whole**, which is the guard the last two attempts at this line
failed.

**Build clean, no warnings. 2,089 tests, four failing, and they are the four
expected:**

- `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`
- `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`
- `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`
- `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`

Six tests were added. `ShortestVote` is still 5, `MaximumRatio` is still 3.8, and
the gate, the survey and the meter were not touched.

**One caveat that belongs in front of all of it.** `N4L` is the only adjudicated
ground truth in nine real recordings, and every number above about which marks are
the station's rests on that one station. A second recording with a readable
callsign would double the evidence this whole chain stands on.

## 3. What we should do next

- **`Refine`, with something that keeps an empty band silent.** It is now the only
  thing between the decoder and `134712`, it has been measured three times, and
  each time the same recording has stopped it. What it needs is not a better dit
  but a reason for the decoder to stay quiet where there is no station, and that is
  a different question from this one.
- **Adjudicate `cw-2026-08-18-004507`.** It went from 25 to 26 characters tonight
  and nobody knows whether either number is any good. It is the recording with the
  most text in it and the one you are already listening to.
- Keep a second recording with a readable callsign when one is heard.
- The keying meter's thresholds are still unscored against an evening's roster.

## 4. What's blocking us

Nothing blocks the next unit.

**One ask, new this session.**

> **The decoder needs a reason to stay silent on a band with no station on it, and
> it is not the amplitude rule's job.**
>
> `Refine` has now been measured three times. Alone it read five characters out of
> `cw-2026-08-20-014854`; with the mark sample corrected by the three-way length
> fit it read one; with the sample corrected by the amplitude rule it reads five
> again. Each time the fault is the same shape: removing the gap average lengthens
> the dit until gate chatter passes the coherence check.
>
> **The amplitude rule cannot close it and should not be asked to.** That recording
> holds no station at any pitch, so its marks are one height population, so the
> rule correctly does nothing. **The behaviour that makes it safe on every clean
> fixture is the same behaviour that leaves it powerless here**, and bending it to
> catch this case is how it would start discarding the quiet end of real signals.
>
> **Rejected: shipping `Refine` anyway because the invented characters are marked.**
> HM-DEC-090 settled that: seventeen hundred characters once came out of half a
> minute of band noise, every one marked, and marking was not enough. **Also
> rejected: a floor on how short a dit may be**, which would be the seventh
> instance of the error class five rulings have gone on closing.

### Asks still outstanding

- **What keeps the decoder silent on an empty band, so `Refine` can ship.** First
  made 2026-08-20, this session. Waiting on Tim or on a unit scoped to it.
  `Refine` is not in the tree.
- **The keying meter's provisional thresholds.** First made 2026-08-20. Waiting on
  one evening's roster scored against the `meter` column.
- **Whether `SHACK_FACTS.md` still holds that CI-V Transceive is off.** First made
  2026-08-20. Waiting on one capture taken with the radio connected, so the
  `broadcast` line has something to report. The change is in the tree at
  `MainWindowViewModel.BroadcastDuringCapture` and `CivLinkHealth.LastTransceiveUtc`.
- **HM-DEC-130, whether a message too long for one keyer send may be split.**
  First made 2026-08-18. Waiting on the seam between two sends measured into the
  dummy load. `CwMessage.Split` already exists and nothing calls it for this.
- **HM-DEC-098, whether §0.2's first sentence is amended to permit an attended
  automatic transmit cycle on the air.** First made 2026-08-17. Waiting on every
  interlock watched to fire into the dummy load, including the link pulled
  mid-cycle. The cycle is built and is dummy-load only.
- **HM-OPEN-033, the cold-start bin choice and `prosigns-easy`.** First made
  2026-08-18; HM-DEC-129 scheduled it rather than closing it. Waiting on its own
  work order.
- **HM-OPEN-007.** Open and unruled since 2026-08-14, named in HM-DEC-140 as the
  reason the queue's own premise is worth re-testing. Waiting on Tim.

**One item leaves the queue.** Amplitude as a relative discriminator, and which
statistic it reads: ruled last session, built this one, and it reads both, with the
loudest moment rescuing what the average would have discarded.
