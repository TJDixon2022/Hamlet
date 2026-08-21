# OUTPUT.md

## 1. What Claude did

### Task 1: the seam, named

Claude Code on the development machine, in `C:\Source\HamLet`, on `main`. The
prompt named `PROJECT: Hamlet`, all four gate checks hold, and **no radio is
attached** (HM-DEC-093).

`CwToneTracker.Measure()` built **one** tapered buffer per hop and ran **both**
banks over it:

| shared | what it is |
|---|---|
| `_windowHops`, `WindowSamples` | one width, set by `FollowSpeed` from the fitted speed |
| `_hann`, `_hannHops` | one taper, rebuilt whenever that width moves |
| `_scratch` | one tapered buffer, filled once per hop from the ring |
| `Goertzel(coefficient, length)` | reads `_scratch` and nothing else |

Over that one buffer: the **fine bank** produced `_fineDb` (which feeds
`_fineSurvey`, the refinement) and the winning bin (which becomes `ToneHz` and the
gate's `PowerDb`); the **coarse bank** produced `_coarseDb` (which feeds `_survey`,
the station finder) and, from bins more than 125 Hz away, the gate's `CompetitorDb`
and `NoiseDb`. **Neither bank belongs to one consumer**, which is why the seam is
not "coarse against fine". It is one buffer answering two questions.

Call sites that move the shared width: `CwToneTracker.FollowSpeed`, called every
hop from `CwDecoder.OnReading` with the rolling fitted speed, and the constructor.
The ring is `MaximumWindowSamples = HopSamples * NarrowWindowHops`, so **50 ms is
the most audio there is** and no gate window can exceed it without a bigger ring.

### Task 2: separated, and proved a no-op

The tracker now prepares a second tapered buffer when the two widths differ, and
each measurement goes to the consumer it belongs to:

- **Which bin wins is the survey's question**, so the fine bank is still read
  through the survey's window. Reading it through the gate's was built first and
  measured: it turns the whole displacement suite red, because where a station
  sits is a fact about frequency.
- **How loud that bin is right now is the gate's**, and so are the competitor and
  the noise beside it. Those two move together deliberately: taking the tone at one
  bandwidth and the noise at another makes `SnrDb` a fact about the filters rather
  than about the band.
- `_coarseDb` and `_fineDb`, which the two surveys read, come from the survey's
  window always.

**With `GateWindowHops` unset the gate takes the survey's window, both passes are
the same arithmetic over the same buffer, and the suite is unchanged at five.**
That is the proof asked for, run rather than asserted. `CwDecoder.cs` is
byte-identical.

### Task 3: swept, and the width is a judgement between two costs

Characters read, every real recording, every width, survey untouched throughout:

| recording | follow | 20 | 25 | 30 | 35 | 40 | 45 | 50 |
|---|---|---|---|---|---|---|---|---|
| `013347` | 8 | 5 | 6 | 8 | 8 | 8 | 9 | 9 |
| `013622` | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| `134712` | 0 | 1 | 0 | 1 | 3 | 2 | 1 | 0 |
| `004507` | 25 | 25 | 31 | 25 | **33** | 28 | 26 | 35 |
| `003016` | 38 | 36 | 35 | 38 | 39 | 45 | 44 | 45 |
| `003126` | 35 | 37 | 35 | 41 | **43** | 43 | 36 | 35 |
| `003758` | 14 | 14 | 15 | 16 | 17 | 16 | 18 | 9 |
| **total** | **120** | 118 | 122 | 129 | **143** | 142 | 134 | 133 |
| `014854` (no keying) | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| `014935` (no keying) | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |

**Every width leaves both recordings that hold no keying silent.** That is the
first time any narrowing has managed it, and it is exactly what the separation
bought: the three earlier attempts moved the survey too.

Invention on the synthesized sensitivity sweep, which is the other half of
HM-DEC-120:

| gate | worst invented | reads 80% down to |
|---|---|---|
| follow | 0.000 | 1.0 dB |
| 25 ms | 0.000 | 1.0 dB |
| 30 ms | 0.000 | 1.0 dB |
| **35 ms** | **0.000** | 1.0 dB |
| 40 ms | 0.000 | 1.0 dB |
| 50 ms | **0.111** | — |

**On those two measurements 35 ms wins outright**: it reads 143 against 120, it
invents nothing at any level, it costs no sensitivity, and both empty recordings
stay quiet. **And fixed at 35 ms it turns fourteen tests red** — the four
`CwDisplacementFloorTests` cases, `TheEasyTierIsReadWhole(exchange-easy)` which
HM-DEC-114 makes pass or fail, both `VA3VRR` element tests, and
`CwSettledPassTests.TheSettledPassNoLongerStopsShortOfTheCallsign`.

So the answer is not a constant. Real off-air captures want a window the
synthesized fixtures do not, **and that is a judgement between two bodies of
evidence rather than a number to be read off a sweep**, which task 3 says is
Tim's. Nothing is set; the knob is there and the sweep is a test.

### Task 4: does the loop still close?

**It cannot be answered as a table this session**, because with no width set the
gate still takes the survey's window and the loop is exactly where it was: eight
of nine recordings at 75 Hz, fitted at 22 to 56 words a minute. What changed is
that **the loop can now be cut without touching acquisition**, which was the whole
obstacle. The moment a width is set, the gate's bandwidth stops depending on the
fitted speed entirely.

**No decision was recorded under §12.1.**

## 2. What Tim should expect

### What the bulletin recording reads

As the tree stands, with no gate width set, `cw-2026-08-18-004507` reads:

```
O T ■T ■■ T ■T ■ O   ■ N D L I ISE SSRG E ■
```

With the gate held at 35 ms, the same recording reads:

```
OT ■ET  EAC STATION HANDLING HIS ESSAGEP
```

And `cw-2026-08-18-003126` goes from

```
  ■ ■■ TL EA■T 2 MOVIISA DAYI■XYL W HY■T  TRNS ,
```

to

```
■E■T ■ ■T L E■T 2 MOVIESA DAY WID X■ WHYENOT   WESTE RNS ,
```

**The second of each pair is what the app would show if you set the width, and it
is not shipped**, because setting it costs the easy tier and the displacement
suite.

### What is different in the app

**Nothing an operator can see.** The separation is machinery: it lands as a proved
no-op so that the width can be chosen in a later unit without the station-finding
collapse that killed the last three attempts.

### What will look wrong and is not

**2,158 tests, five failing**, and they are the five the order names:

- `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`
- `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`
- `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`
- `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`
- `TheToneIsFoundInRealisticAudio(farnsworth-heavy)`

Build clean, no warnings. Six tests added, all green.

**Two rig tests flaked once each across the session's full runs** and passed on
their own immediately after —
`RigBroadcastProvenanceTests.ABroadcastFrequencySurvivesTheSweepWithItsProvenance`
and `AReadIsAnsweredOnlyByItsAnswerTests.AReplyToAnotherCommandDoesNotCompleteThisOne`.
That is `HM-OPEN-055`, already recorded, and nothing in the rig code was touched.

**The new tests are slow.** `TheGateHasItsOwnWindowNowTests` runs nine recordings
at eight widths and two sensitivity sweeps, about two and a half minutes.

Pushed to `main`.

## 3. What we should do next

- **Rule on the gate's width**, which is now one question with a complete sweep
  behind it and no way to answer it from the numbers alone.
- **If the answer is that no constant is right, the width should follow the
  *proved* speed rather than the rolling one.** That guard already exists, it is
  what every surface uses to decide whether a speed may be shown at all, and it is
  the one estimate in this decoder that is not fed by the filter it would be
  choosing. It was not tried this session because it is a second variable.
- **The speed fit is still the thing everything hangs off**, and three asks about
  it are outstanding.

## 4. What's blocking us

Nothing blocks the next unit. The machinery the last ruling called for is built,
proved and pushed; what is not settled is the number it exists to let somebody
choose.

**One ask, new this session.**

> **What width should the gate look through, given that no constant satisfies both
> bodies of evidence?**
>
> Thirty-five milliseconds reads 143 characters across the six real recordings
> with content in them against 120 for following the fitted speed, invents nothing
> at any level on the sensitivity sweep, costs no sensitivity, and leaves both
> recordings holding no keying silent. **Fixed at thirty-five it also turns
> fourteen tests red**, including `TheEasyTierIsReadWhole(exchange-easy)`, which
> HM-DEC-114 makes pass or fail, and the four `CwDisplacementFloorTests` cases.
>
> The easy tier sends at twelve words a minute and had a fifty millisecond window
> before; the real captures are senders working near fourteen whose fits read
> twenty-two to fifty-six, so they had twenty. **One number cannot be right for
> both**, and choosing between real off-air audio and synthesized fixtures is not
> something a session may settle (§12.1).
>
> **The shape that might dissolve it**: let the width follow the *proved* speed —
> the guarded one, which requires a tone located, a character resolved, no
> re-acquisition in progress and a dit the settled pass has proved — rather than
> the rolling estimate. Not tried, because it is a second variable and this unit
> was one at a time.

### Asks still outstanding

- **What width the gate should look through.** First made 2026-08-21, this
  session. Waiting on Tim. Nothing is set; the sweep is in
  `TheGateHasItsOwnWindowNowTests`.
- **Three recordings named in an earlier instruction are not in the tree**
  (`cw-2026-08-21-015834`, `-020033`, `-015432`). First made 2026-08-20. Waiting on
  the files.
- **Whether a clock fit may exclude runs below a share of its own unit, and in
  which instrument.** First made 2026-08-20. Waiting on Tim.
- **Whether a mark too short to be an element may be set aside before the
  decoder's clock is fitted, at the cost of HM-DEC-120's zero-invention
  property.** First made 2026-08-20. Waiting on Tim.
- **Whether the unit may still be averaged with key-up gaps.** First made
  2026-08-20. Waiting on Tim. Removing `Refine` puts `013347` at 100.0 ms against
  a hand-read 100.4 and turns thirteen tests red.
- **The speed control needs an entry in `DECISIONS.md` and an id.** First made
  2026-08-20. Waiting on Tim. The code is on `main`.
- **The keying meter's provisional thresholds**, including
  `CwKeyingThresholds.ConfidentSwingDb` at 20 dB. First made 2026-08-20. Waiting
  on one evening's roster scored against the `meter` column.
- **HM-DEC-130, whether a message too long for one keyer send may be split.**
  First made 2026-08-18. Waiting on the seam between two sends measured into the
  dummy load.
- **HM-DEC-098, whether §0.2's first sentence is amended to permit an attended
  automatic transmit cycle on the air.** First made 2026-08-17. Waiting on every
  interlock watched to fire into the dummy load, including the link pulled
  mid-cycle. The cycle is built and is dummy-load only.
- **HM-OPEN-033, the cold-start bin choice and `prosigns-easy`.** First made
  2026-08-18; HM-DEC-129 scheduled it rather than closing it. Waiting on its own
  work order.
- **HM-OPEN-007.** Open and unruled since 2026-08-14, named in HM-DEC-140 as the
  reason the queue's own premise is worth re-testing. Waiting on Tim.

**One item leaves the queue.** Whether the coarse survey and the detection filter
may stop sharing an analysis window: ruled in this unit's instruction, built,
proved a no-op, and pushed.
