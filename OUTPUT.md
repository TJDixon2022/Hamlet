# OUTPUT.md

## 1. What Claude did

### Task 1, the four answers

Claude Code on the development machine, in `C:\Source\HamLet`, on `main`. The
prompt named `PROJECT: Hamlet` and the four gate checks all hold. **No radio is
attached, and nothing here needs one** (HM-DEC-093). The decoder was run over
`cw-2026-08-17-134712.wav` hop by hop, 6,000 hops of five milliseconds, reading
only its public surface. Nothing was changed for task 1.

**1. Did the tone survey latch? Yes, almost the whole way through.**
`HasTone` is true on **5,904 of 6,000 hops**, first at 0.5 seconds, and the
tracker settles on **500 Hz**, which is the pitch the independent meter chose.
So `Emit`'s early return on a missing tone never fires. **The instruction's first
lead is dead**: the shape of `107 seen, 0 resolved` is not "nothing was ever
latched".

**But `Verdict.Keyed` is null on all 6,000 hops.** The survey never once judged
anything to be keyed; it reported `Interference` on 1,600 of them, and
`HasKeying` is false throughout. On the control `cw-2026-08-18-004507` the same
survey returns a keyed verdict on 2,294 hops. **That is a symptom of the same
root cause and it is not what blocks emission** — proved in answer 3, which is why
this unit does not land on HM-OPEN-054.

**2. The 107 elements.** Run cleanly over this file alone the gate produces 197
elements from a 600 Hz start, 99 marks and 98 gaps. Across the whole recording
they are a smear: marks median 25 ms with 43 of 99 under 25 and a tail to 595;
gaps median 70 ms out to 1,790.

**Inside the stretch the meter scored 0.37 they are not a smear at all.** From
21.45 s to 23.01 s the gate reads, in order:

```
mark 225  gap  30  mark  55  gap 180  mark  55  gap  40  mark  55  gap  40
mark  60  gap  40  mark  55  gap  30  mark 245  gap 150  mark  60  gap  25
mark 245  gap  40  mark  55  gap  40  mark  55
```

**Dit 55 ms, dah about 235, element gap 35.** That is a steady, ordinary fist at
about 22 words a minute. **The meter's 54 ms and the gate's 55 ms agree to within
a millisecond**, which is the two instruments confirming each other on audio
neither has any reason to flatter.

**The dah is 4.3 dits, not 3.**

**3. Where the first element dies: `Emit` is reached and `LooksLikeMorse` is
false.** It is false on **all 6,000 hops**. `Coherence` peaks at 0.19, 0.25 or
0.33 depending on where the tracker starts, against a floor of 0.35, and averages
0.02. It never crosses. On the control, coherence reaches 1.00 and
`LooksLikeMorse` is true on 3,236 hops.

`Coherence` measures how far each mark sits from **one dit or three dits**, with
the 3 written into the code. A dah of 4.3 dits is 1.3 dits from its target, half
the marks are dahs, and the average error passes the half-dit limit at which the
timings stop counting as Morse. **The decoder heard this station correctly and
discarded it for sending heavy dahs.**

**4. Pointing it at 500 Hz instead of 550 changes nothing.** Started at 600, 550
or 500 the tracker converges on 500 Hz every time; the element counts differ (197,
225, 229) and the characters emitted do not (0, 0, 0). **The instruction's second
lead is dead too.** Measured with a constructor parameter; no default was changed.

### The cause, isolated

Run on the four clean seconds alone, 20.8 s to 25.0 s, with the dit estimate
**identical** in both runs (24 to 26 words a minute either way):

| coherence measured against | `LooksLikeMorse` | coherence | characters |
|---|---|---|---|
| a hardcoded one dit and three | 0 hops | **0.00** | **0** |
| the two lengths this sender uses | 38 hops | 0.38 | 1 |

Same audio, same gate, same elements, same dit. **The ratio is the whole of it.**

### Task 2, the failure held still

`ARecordingWithKeyingInItIsReadTests` reads the fixture and asserts three things,
sharing nothing with the decoder's own judgement of what a tone is (§12.5):

- `AnIndependentInstrumentFindsKeyingInIt` — `KeyingEnvelope` scores 0.37 at
  500 Hz with a 54 ms element. **Passes.**
- `TheDecoderSaysSomethingAboutIt` — the tone latches, 197 elements are measured,
  and something is emitted. **Fails, and is the new red.** It prints the numbers
  every run: elements seen and resolved, characters emitted, and the coherence
  reached against its floor.
- `WhereTheTrackerStartsDoesNotDecideThis` — 600, 550 and 500 all settle on 500.
  **Passes.**

**It asserts that something came out and never what.** The recording has no
adjudicated answer key, adjudicating it is Tim's ear, and a test that checked the
text would be asserting a transcript nobody has confirmed.

### Task 3, the one change

`MeasureCoherence` now measures each mark against the fitted center of the long
marks rather than a hardcoded three dits. That center is already computed a few
lines above it, by the same `TwoMeans` fit `ClassifyMark` uses.

**It is the third place in this decoder to assume textbook timing and the other
two were already fixed.** HM-DEC-115 stopped deriving word and character gaps
from multiples of the dit and clusters the sender's own gaps, because real
operators send Farnsworth. HM-DEC-119 made `ClassifyMark` cut between the two
measured mark clusters, "fitted per signal", after measuring a fist sending dahs
at two and a half. The coherence check was left behind, and it is the one with a
veto over the whole message.

**Nothing about it makes the decoder more willing to emit** (HM-DEC-048). The
question is unchanged and is still the one that tells Morse from an empty band:
do the marks land on two lengths over and over. What changed is that the two
lengths are the sender's rather than a textbook's. **A fitted dah outside two to
five dits is not used** and the textbook three is taken instead, because past five
dits the long cluster is a carrier, a fade or somebody holding the key down.

Five unit tests pin it, including two controls: exponential run lengths, which is
what a gate chopping an empty band produces, still score 0.00, and a held key with
dits scattered around it still scores 0.00.

**Every capture, before and after:**

| capture | emitted before | emitted after |
|---|---|---|
| `cw-2026-08-18-004507` | 19 | **25** |
| `cw-2026-08-18-003016` | 36 | **38** |
| `cw-2026-08-18-003126` | 32 | **34** |
| `cw-2026-08-18-003758` | 14 | 14 |
| `cw-2026-08-17-013347` | 8 | 8 |
| `cw-2026-08-17-013622` | 0 | 0 |
| **`cw-2026-08-17-134712`** | **0** | **0** |
| `cw-2026-08-20-014854` | 1 | 1 |
| `cw-2026-08-20-014935` | 0 | 0 |

**Three of the four that decoded read more and nothing reads less.** The three
standing red tests are unchanged, failing on the same assertions with the same
messages.

**And the fixture still reads nothing over its full thirty seconds.** Task 2's
test still fails.

### Why it is still nothing, and the second cause is named and not fixed

The fix works on clean audio and is defeated on this file by a **second, separate
mechanism**, measured and left alone because the order says change the one thing.

`Refine` averages the mark-derived dit with a gap-derived one. Fed this sender's
own lengths, a 55 ms dit with a 35 ms element gap, it returns **45 ms**. The
fitted dah then reads 235 over 45, which is **5.2 dits**, outside the two-to-five
band, so the textbook three is used and the old behavior returns.

**Its stated premise has already been measured false by a ruling in this
repository.** `Refine`'s comment says a mark measured at a threshold comes out
long by the same amount the gap after it comes out short, so the mean of the two
is the truth. HM-DEC-119 measured that through Hamlet's own detector and found the
gate reads 100 to 110 ms for a true 100 at every speed: **the mark is not long,
and there is nothing to cancel.** HM-DEC-115 measured the other half, that a real
fist's element gap is genuinely shorter than its dit, 40 ms against 57 on
`cw-2026-08-18-004507`, because that is how people send. Averaging the two
therefore shortens the dit by about a fifth on any Farnsworth sender.

`TheDitComesOutShortWhenTheGapIsShorterThanIt` records the size of that bias so a
later session sees the number move. **It asserts the measurement and not that the
behavior is right, which it is not.**

### Task 4, the two from the 19th

`cw-2026-08-20-014854` emits **one** character before the change and **one**
after. `cw-2026-08-20-014935` emits **nothing** before and **nothing** after.
**The change produces nothing new from either**, which is the reading the
instruction asked for: a fix that started printing text from recordings the meter
says contain no keying would be evidence of invention, not of repair. The one
character on `-014854` is not new and is not this unit's.

### Three things the instruction asked to be told

**`ShortestVote` is not implicated, and this is not a place to stop.** It removes
runs shorter than three measurements, fifteen milliseconds. Every mark in the
clean stretch is 55 ms or longer and coherence there is 0.00 regardless, so no
de-glitch window would change it. The chatter marks elsewhere in the file are real
and would be reduced by a wider window, which is a different question about a
different part of the recording.

**A parked item is adjacent and is raised once.** `CwToneSurvey.MaximumRatio` is
3.8, and this fist sends 4.3, which is why `Verdict.Keyed` is null on every hop of
this recording. **That is the same root cause in a second place**, and the survey's
verdict is HM-OPEN-054 and HM-DEC-143 ground. It was not touched, no distinguisher
was built, and it is in section 4 as an ask.

**Nothing in this path has a bound near 69 or 233.** The buffers the trace goes
through are the speed estimator's twenty-mark rolling window, its twenty-gap
window, and the gate's vote window of five to nine. The 69 and 233 remain a
question for their own unit.

## 2. What Tim should expect

**No. The decoder still reads nothing from `cw-2026-08-17-134712`.** It reads
nothing over the full thirty seconds, and there is no text to show you. Half the
cause is fixed and measured; the other half is named, sitting in `Refine`, and was
left alone because fixing two things at once is how the last two evenings were
lost.

**What did change.** Three of the four recordings that decoded now produce more
characters: 004507 from 19 to 25, 003016 from 36 to 38, 003126 from 32 to 34.
Nothing produces less. There is no adjudicated answer key for any of them, so
**more characters is not the same as more correct characters** and none of it is
proof the extra ones are right. What it is evidence of is that the change is not
narrowly fitted to the one recording it came from.

**Build clean, no warnings. 2,069 tests, four failing.** Three are the standing
reds and are unchanged:

- `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`
- `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`
- `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`

**The fourth is new and deliberate**, and it is what task 2 asked for:
`ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`. It is the
first decode failure in this project that can be run a thousand times, and it
prints its own numbers on every run. **It should stay red until it goes green
honestly.**

Sixteen tests were added. Nothing in the gate, the tone survey or the settled pass
was touched, and `ShortestVote` is still 5.

Committed and pushed to `main`.

## 3. What we should do next

- **`Refine`, in its own unit.** It is the other half of tonight's answer, its
  premise has already been measured false by HM-DEC-119, and the fixture in the
  tree tells you immediately whether a change to it worked.
- Then re-run task 2's test. If it goes green, the text it produces still needs
  your ear before it means anything.
- Rule on the tone survey's ratio band in section 4, or park it explicitly for a
  later unit. It is the same fault in a second place and it is on ruled ground.
- The four recordings that decoded now have a floor test under them, set at
  today's counts. If a later change lifts them a long way, raise the floors with
  the measurement beside them.

## 4. What's blocking us

Nothing blocks the next unit.

**One ask, new this session.**

> **`CwToneSurvey.MaximumRatio` stays at 3.8 until it is ruled, and the fist on
> `cw-2026-08-17-134712` is recorded as a station the survey rejects for its
> ratio.**
>
> The survey requires a dah between 2.5 and 3.8 dits before it will call anything
> keyed. This station sends 4.3, measured by the decoder's own gate and confirmed
> to within a millisecond by an instrument that shares no code with it. That is
> why `Verdict.Keyed` is null on all 6,000 hops of the recording while the control
> returns a keyed verdict on 2,294.
>
> **It is the same root cause as tonight's fix in a second place**, and the same
> argument would move it: HM-DEC-119 already fitted the mark boundary per signal
> rather than assuming two dits. **But the survey's verdict is HM-OPEN-054 and
> HM-DEC-143 ground**, which is ruled, unbuilt and parked, and the work order says
> to stop rather than build there. So it was measured and not touched.
>
> **Rejected: widening the band tonight as part of the coherence fix.** Two
> changes in one unit is what makes the next evening's evidence unreadable, and
> this one sits on a ruling that has been waiting for its own session. **Also
> rejected: leaving it unrecorded**, because the next session to trace this
> recording will find `Verdict.Keyed` null and spend an evening rediscovering
> why.

### Asks still outstanding

- **`CwToneSurvey.MaximumRatio` and the 4.3-dit fist.** First made 2026-08-20,
  this session. Waiting on Tim, and on whether it may be worked without opening
  HM-OPEN-054. Nothing was changed.
- **The keying meter's provisional thresholds.** First made 2026-08-20. Waiting on
  one evening's roster scored against the `meter` column. The numbers are in
  `CwKeyingThresholds` and nothing else reads them.
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

**Nothing leaves the queue this session.**
