# OUTPUT.md

## 1. What Claude did

### Task 3: what the decoder read against the keys

Claude Code on the development machine, in `C:\Source\HamLet`, on `main`. The
prompt named `PROJECT: Hamlet` and the four gate checks all hold. **No radio is
attached** (HM-DEC-093). **No decoder code changed.**

Both fixtures send `CQ DE N0CALL K`.

| fixture | key | Hamlet reads | of 12 |
|---|---|---|---|
| `farnsworth-light`, cut to `VA3VRR` | `CQ DE N0CALL K` | **`DE N0CALL K`** | 9 |
| `farnsworth-heavy`, cut to `N4L` | `CQ DE N0CALL K` | **`AL K`** | 3 |

**Nothing is invented in either.** Every character read is in the message and in
the right place; what is missing is the front of it — the whole of `CQ ` on the
light fist, and `CQ DE N0C` on the heavy one.

**The dit is short in both, and much shorter on the heavier fist:**

| fixture | true dit | fitted | error |
|---|---|---|---|
| `farnsworth-light` | 100 ms | 95.0 | −5% |
| `farnsworth-heavy` | 56 ms | 47.0 | **−16%** |

**That is the first reproducible Farnsworth failure with an answer key in this
project.** Until now the only evidence of it was two off-air recordings whose
transcripts nobody knows; this is fifteen seconds of audio, generated from a
recipe, whose every character is written down.

**And the failure tracks the fist.** `VA3VRR` sends an element gap of 0.73 dits and
loses a quarter of the message; `N4L` sends 0.64 and loses three quarters. The
lighter the gap relative to the dit, the shorter the fitted dit and the more of the
opening goes missing.

### Task 1: the generator can already do it

**`CwFixtureRecipe` takes `DitMilliseconds`, `DahMilliseconds`,
`ElementGapMilliseconds`, `CharacterGapMilliseconds` and `WordGapMilliseconds`
independently**, and `KeyEdges` lays them down as given. Task 2 was a matter of new
fixtures rather than new code, and no generator change was needed.

**Its own defaults are already Farnsworth** — dit 105, dah 283, element gap 65,
character gap 130, word gap 280 — and its comment says so in terms: "**NOT ONE TO
THREE TO SEVEN.** The station this repository recorded sends element gaps of about
sixty-five milliseconds against dits of a hundred and five."

**A mismatch with the instruction, reported rather than repaired.** It states that
every synthesized fixture in the repository sends textbook 1:3:7. **Most do and
some do not.** `tightfist-easy`, `tightfist-working`, `tightfist-edge` and
`qsk-preamble` are already Farnsworth: dit 94, dah 273, element gap **80**,
character gap 162, word gap 265. The textbook ones are `exchange-*`, `prosigns-*`,
`coverage-*` and `fast-*` in the receiver set, plus the older `clean-12wpm`,
`clean-18wpm` and `prosigns-18wpm`. The gap the suite had was not "no Farnsworth at
all" but **no Farnsworth at either extreme this project has measured**, and that is
what the two new ones fill.

### Task 2: two fixtures cut to the two known fists

| fixture | dit | element gap | dah | character gap | word gap | after |
|---|---|---|---|---|---|---|
| `farnsworth-heavy` | 56 ms | 36 | 238 | 165 | 355 | `N4L`, HM-DEC-144 |
| `farnsworth-light` | 100 | 73 | 274 | 150 | 323 | `VA3VRR`, HM-DEC-145 |

Every figure but one is measured. **The word gap is the exception and it is marked
as one** (§12.4): neither adjudication caught a word gap, because both callsigns
were read out of a single unbroken run of characters. Rather than invent one, it
takes **2.15 times the measured character gap**, which is the ratio the generator's
own default recipe already carries — 280 against 130 — itself modelled on a real
recording. The constant's own documentation says it is an assumption and says what
would replace it.

Both sidecars record that the fixture was generated and from which decision entry
its timing comes (HM-DEC-091).

**One thing had to be undone.** Adding the recipes in the middle of the catalogue
shifted the seed counter and silently re-cut `qsk-preamble`, which the order
forbids. They are appended after it instead, so no existing fixture's bytes moved.

**The tier, proposed rather than assumed** (HM-DEC-114). Neither joins the easy
tier. `farnsworth-light` is fully admissible and joins the general fixture theories,
which are ratchets. **`farnsworth-heavy` is held out**, and the reason is the
finding underneath this one.

### The reference cannot read the heavier fist

Scored with `tools/score-fixtures`:

```
farnsworth-light       100%  ok
farnsworth-heavy         0%  BAD FIXTURE
```

Its sidecar records the reason in the reference's own words: **`read nothing (do
not cluster as Morse)`**. A dah of 4.25 dits failing a check that expects three.

**That is the error class six rulings have gone on closing in Hamlet, alive in the
reference.** HM-DEC-101 makes a fixture the reference cannot read a bad fixture, so
`farnsworth-heavy` is in `NotYetAdmissible` and may not judge Hamlet. **But its
timing is adjudicated to the millisecond from a recording whose callsign was read
out of the gate's own elements**, so this is not a fixture parked for being
inconvenient — it is the reference being wrong about a fist that exists. HM-DEC-101
records that one earlier entry was cleared exactly this way, by fixing the
reference.

### Task 4: the two adjudicated recordings are unchanged

| recording | true dit | fitted | emitted | reads |
|---|---|---|---|---|
| `cw-2026-08-17-134712`, `N4L` | 56.3 ms | 31.3 | 0 | nothing |
| `cw-2026-08-17-013347`, `VA3VRR` | 100.4 | 87.0 | 8 | `■■ ■■VA3VRR` |

Both identical to before, as they must be with no decoder code touched.

## 2. What Tim should expect

**No. The decoder does not handle a Farnsworth sender it has never seen before, and
now there is an answer key that says so.**

It loses the front of the message and keeps the end: `DE N0CALL K` out of
`CQ DE N0CALL K` on the lighter fist, `AL K` on the heavier one. **Nothing is
invented** — every character it does produce is right and in the right place, which
is §0.0 behaving as it should.

**The fitted dit is short by 5% on the light fist and 16% on the heavy one**, and
that is the same failure the two off-air recordings show: `VA3VRR` reads 87.0
against 100.4, `N4L` reads 31.3 against 56.3. **The fixtures reproduce, in
generated audio with a written-down answer, what has only ever been arguable
before.**

**Build clean, no warnings. 2,117 tests, four failing, and they are the four
expected:**

- `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`
- `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`
- `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`
- `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`

**Nothing existing broke and no fixture was re-cut.** `ShortestVote` is still 5,
`MaximumRatio` still 3.8, the separation figure untouched, `Refine` still absent and
not revived.

## 3. What we should do next

- **Fix the dit on a Farnsworth sender**, now that it can be measured against a
  key. Every previous attempt at this was argued against off-air audio nobody had
  scored; `farnsworth-light` reads 9 of 12 with a dit 5% short, and a change either
  moves that number or it does not.
- **Rule on the reference.** It cannot read a 4.25-dit fist, which is why
  `farnsworth-heavy` is held out, and until that is settled the heavier of the two
  new fixtures cannot judge anything.
- **The opening is what goes missing**, in both fixtures and both recordings. That
  is worth a look on its own: the decoder appears to need several characters before
  its clock settles, and on a fourteen-character message that is most of it.
- Adjudicate `cw-2026-08-18-004507` when there is an evening for it.

## 4. What's blocking us

Nothing blocks the next unit. **The heavier fixture is blocked from judging Hamlet
until the reference is ruled on.**

**One ask, new this session.**

> **The reference decoder cannot read a dah of 4.25 dits, and that is a defect in
> the reference rather than in the fixture or the fist.**
>
> `farnsworth-heavy` is generated from `N4L`'s timing to the millisecond: a 56 ms
> dit, a 238 ms dah, a 36 ms element gap, adjudicated in HM-DEC-144 by reading the
> callsign out of the gate's own elements with cuts fitted from that stretch. The
> reference scores it **0%** and says why: `read nothing (do not cluster as Morse)`.
>
> **HM-DEC-101 makes that a bad fixture and it is the right default**, so it sits in
> `NotYetAdmissible` and judges nothing. But the same ruling records that one
> earlier entry was cleared by fixing the reference rather than the fixture, and
> this is that case: the timing is not in doubt, the station is not in doubt, and
> what the reference objects to is a ratio this project has already proved exists on
> the air.
>
> **Rejected: softening the fixture toward three dits** to get it past the gate.
> That would be generating a fist nobody has measured in order to pass a check,
> which is the weak evidence this whole line of work exists to reduce. **Also
> rejected: admitting it anyway** on the grounds that its timing is adjudicated,
> because HM-DEC-101's gate exists precisely to stop a session deciding its own
> fixture is good enough.

### Asks still outstanding

- **Whether the reference should be fixed to read a 4.25-dit fist.** First made
  2026-08-20, this session. `farnsworth-heavy` is generated, scored and held out
  meanwhile.
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

**One item leaves the queue.** Whether the synthesized fixtures should carry
Farnsworth spacing: ruled this session, and two now do at the extremes this project
has measured.
