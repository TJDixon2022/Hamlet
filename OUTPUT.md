# OUTPUT.md

## 1. What Claude did

### Task 1: the five recordings this unit is built on are not in the tree

Claude Code on the development machine, in `C:\Source\HamLet`, on `main`. The
prompt named `PROJECT: Hamlet` and the four gate checks all hold. **No radio is
attached** (HM-DEC-093).

**`cw-2026-08-2?-005902`, `-010133`, `-010244` and `-010336` do not exist**, in
`tests\fixtures\cw\captured\unadjudicated\`, anywhere else in the repository, or
in the app's own capture folder on this machine. That folder has one commit
behind it, `6f93c32 Captures from the shack, unadjudicated`, and the newest file
in it from the 20th is `cw-2026-08-20-014935`. **So parts 1 and 4 of task 1
cannot be answered**: nothing here can say why `010336` locked and the other four
did not, and no number in the order's table can be reproduced against the audio it
was taken from.

Everything below is the same set of questions asked of the nine recordings that
are in the tree.

### What the rolling window measures, and which half poisons it

`CwSpeedEstimator` keeps the last twenty key-down lengths and the last twenty
key-up lengths and re-derives on every one of them.

**It uses both, and the key-up half is the contaminating one.** The dit comes
first from a two-means fit over key-down lengths in log space, taking the median
of the short cluster, which is exactly what task 2 asks for and is already there.
Then `Refine` averages that answer with the mean of every key-up gap shorter than
twice it. **On `cw-2026-08-17-013347`, whose dit is 100.4 ms read by hand
(HM-DEC-145), the key-down fit alone gives 100.0 and `Refine` turns it into
87.0.**

**Does it assume 1:3:7? In two places out of three.** The gaps are clustered from
the sender's own sending and never from multiples (HM-DEC-115), so the 3 and the
7 are not assumed. The dah is fitted per sender, but only inside two to five dits,
and **outside that band it falls back to a textbook three**, which is where a
wrong dit stops being an error and becomes a collapse. And `Refine`'s averaging
assumes the element gap equals the dit, which is a 1:1 assumption that is false
for every Farnsworth fist this project has measured.

### Where the sub-20 ms runs come from, and whether they reach the tracker

**They come from the gate, they survive the de-glitch, and every one of them
reaches the tracker.** `CwDecoder.OnMarkEnded` hands the estimator every mark
that was not truncated, at any length. Nothing between the gate and the estimator
has ever asked how long a mark is. The only thing ever set aside is a mark that
was too **quiet** (HM-DEC-144), and that rule needs the heights to fall into two
separated groups before it does anything at all — on an empty band, or a station
buried in one, they do not.

**The de-glitch is not the mechanism and `ShortestVote` is untouched**
(HM-OPEN-053). Its own note claims a median over five measurements "removes any
run shorter than three". That is true of an isolated run and false of alternating
chatter, because three of the five in the window can be down without any two of
them being adjacent. Measured on `cw-2026-08-17-013622`, marks of a single five
millisecond measurement reach the estimator six times and marks of two reach it
nine times, with the vote window at five throughout.

### The lead, tested on the recordings that exist

| recording | marks | under 20 ms | share | locked | characters |
|---|---|---|---|---|---|
| `013347` (`VA3VRR`) | 71 | 2 | 3% | 21.3 s | 8 |
| `003016` | 189 | 11 | 6% | 5.0 s | 38 |
| `003126` | 195 | 24 | 12% | 4.2 s | 35 |
| `004507` | 156 | 29 | 19% | 6.7 s | 25 |
| `003758` | 200 | 41 | 21% | 5.4 s | 14 |
| `013622` | 175 | 50 | **29%** | **never** | 0 |
| `134712` (`N4L`) | 97 | 39 | **40%** | **never** | 0 |
| `014854` (no station) | 207 | 27 | 13% | never | 0 |
| `014935` (no station) | 244 | 35 | 14% | never | 0 |

**Supported, with one qualification that matters.** Every recording holding a
station between 3 and 21 per cent locks and reads; the two at 29 and 40 per cent
never lock, and one of those holds `N4L`, whose timing is adjudicated. **But it is
not a station detector**: the two recordings holding no keying at all sit at 13
and 14 per cent, inside the reading band. A high share predicts failure; a low one
predicts nothing.

### Where the trace contradicts the instruction

`KeyingEnvelope` does compute exactly the envelope the order describes, and it is
what was used. **Its two-means answer disagrees with this project's own
adjudicated ground truth, and it disagrees in one direction.**

| recording | hand-read dit | envelope dit | hand-read dah | envelope dah |
|---|---|---|---|---|
| `013347` (HM-DEC-145) | 100.4 ms | **65.7** | 274.3 ms | 268.1 |
| `134712` (HM-DEC-144) | 56.3 ms | **67.1** | 238.3 ms | 241.0 |

The dah lands within three per cent on both. The dot is 35 per cent short on one
and 19 per cent long on the other, and the order's `(dot + dah/3) / 2` gives 77.5
and 73.7 against a true 100.4 and 56.3. **The dah is the robust half of that fit
and the dot is not.** This does not contradict the order's own table, which was
taken from different audio; it does mean the method should not be adopted on the
strength of it.

### Task 2: three shapes built and measured, and none of them ships

**Shape one, the order as written**: set aside marks under half the fitted dit
before fitting. `cw-2026-08-17-134712`'s dit goes 31.3 to 39.0 ms and it starts
locking, `003758` goes 34.3 to 41.8, and the character counts rise — 25 to 27, 38
to 39, 35 to 36, 14 to 19. **And it breaks HM-DEC-120.** The sensitivity sweep
invents six to eight per cent of what it emits at minus three, minus four and
minus five decibels, where that ruling's whole property is that nothing is
invented at any level. It also broke `ASmearIsNotTwoLengthsTests` and both seeded
cases of `ARecordingWithNoKeyingStaysSilentAtEverySpeed`.

**Shape two**: the same exclusion, but the trimmed set feeds only the clock fit
while the is-this-Morse verdicts still see every mark, so nothing is flattered by
discarding data. Every §0.0 guard holds, nothing is invented anywhere, and
`134712`'s dit still improves to 39.0. **And every character gain disappears** —
25, 38, 35, 14 again. The gains and the breakage were the same change.

**Shape three**: shape two, plus a gate so the exclusion only runs where the
marks stand above the refusal floor, on the reasoning that below it nothing is
emitted anyway. Safe, suite back to its five, **and it does nothing at all on any
recording that needs it**, because the recordings that need it sit below 14 dB.

**And the order's other half was measured too.** "Never derive the unit from
key-up" means removing `Refine`. On `013347` that takes the fitted dit from 87.0
to **100.0** against a hand-read 100.4, which is the best number this project has
ever produced for that recording. It also turns thirteen tests red, including
`TheCleanRecordingsDecodeExactly` at both 12 and 18 words a minute, the speed
readout, the bulletin's words, `prosigns-edge`, and a §0.0 guard. `Refine` is
right for a textbook fist, where the element gap is the dit and the averaging
cancels the gate's edge bias exactly as HM-DEC-119 describes, and wrong for every
Farnsworth fist, where it averages two different quantities.

**So task 2 weighs two costs against each other, which §12.1 puts outside what a
session may settle.** Nothing was shipped and `CwTiming.cs` is byte-identical to
what it was at the start. The ask is in section 4.

### Tasks 3, 4 and 5, all built

**Task 3.** The tracker's figure now supersedes the operator's the moment there is
a station to track, and the evidence is the keying meter's swing rather than the
tracker's opinion of itself. `CwKeyingThresholds.ConfidentSwingDb` is 20 dB,
measured in the tree rather than carried from the order: the six recordings
holding a station swing 21.8 to 91.5 dB and the two holding nothing swing 14.1 and
17.7. His figure comes straight back when the station goes. **A held verdict now
prints the verdict and no measurements**, on screen and in the sidecar, which
supersedes the note in `CwKeyingThresholds` that said the numbers should keep
moving.

**Task 4.** `snrDb` is a held peak of how far the tracked bin stood above the
noise beside it, rising at once and falling about a decibel a second — built that
way by HM-DEC-090 so a station keying for a second and a half inside thirty would
not average away to nothing. It is not a figure about a recording, and the fault
reproduces exactly in the tree: **41.7 on `cw-2026-08-20-014854` and 38.4 on
`-014935`, neither of which holds keying at any pitch, against 34.7 on
`cw-2026-08-17-013347`, which is the one this decoder reads a callsign out of.**
It is not deleted and not changed. The roster column is now `tonePeakDb`, the
sidecar line is now `tonePeak` and carries a sentence saying what it measures, and
a test holds the finding so nobody quietly relabels it back.

**Task 5.** The roster's `text` cell now carries the same clause `chars` already
had, in the same words.

**No decision was recorded under §12.1.**

## 2. What Tim should expect

### Does a strong station at 14 words a minute now read without you setting it?

**Not proven, and on the evidence here probably not yet.** What changed is that a
figure you set wrong can no longer hold the reading back: once the keying meter
sees a real station the tracker's own number governs and the panel says so, so the
exact failure of the 20th — twenty set against a station sending fourteen, two
characters in thirty seconds — cannot repeat. **Nothing this session made the
tracker itself better at finding fourteen**, because the change that would have
was measured three ways and every version that helped also made the decoder invent
characters on audio holding nothing.

### At the rig

Leave the speed control off. If a station beats Hamlet, tick it and set roughly
what you hear; the line under the transcript now has three things to say instead
of two, and the new one is **"Hamlet can hear somebody keying and has worked the
speed out for itself"**, which means your figure has stepped aside and is waiting.
If the keying line ever says it is holding through a quiet stretch, it will now
show no numbers at all — that is deliberate, and it is the fix for the `9 ms key
down` that started this.

### What will look wrong and is not

**The order's expected-red list is two short.** It names three; the tree has been
at five since before this session, and still is:

- `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`
- `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`
- `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`
- `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`
- `TheToneIsFoundInRealisticAudio(farnsworth-heavy)`

The last two are not in the order's list and were red before this session started.
**2,136 tests, five failing, and they are those five.** Build clean, no warnings.
Ten tests were added, all green.

**The roster gains a renamed column.** `snrDb` is now `tonePeakDb`. A roster file
started before this build and appended to after it would have rows of two
different widths, exactly as the `seed` column did yesterday.

**And `hasTone` reads true on both recordings that hold no keying.** Named and
left (§12.6); it is the same latch `tonePeakDb` comes from and it is not this
unit's.

Pushed to `main`.

## 3. What we should do next

- **Get the five recordings into the tree.** They are the only five-point sample
  this project has of one operator, one fist, four failures and one success, and
  the whole reasoning of this unit rests on them. Nothing here could check a single
  figure in the table.
- **Rule on task 2**, which is one question with two halves and both are measured:
  whether a mark too short to be an element may be set aside before the clock is
  fitted, and whether the unit may still be averaged with key-up gaps. The numbers
  for both are in section 4.
- **The lock is lost near the end of almost every real recording** — 25.0, 25.7 and
  27.5 seconds on thirty-second captures — and that is where the callsign usually
  is. Not investigated; noticed while tracing.

## 4. What's blocking us

**The five recordings the unit is built on are missing**, which blocked parts 1
and 4 of task 1 outright and left task 2 to be measured against different audio.

**Three asks, all new this session.**

> **May a mark too short to be an element be set aside before the clock is fitted,
> at the cost of HM-DEC-120's zero-invention property?**
>
> Setting aside marks under half the fitted dit takes `cw-2026-08-17-134712` from
> a fitted dit of 31.3 ms to 39.0 against a hand-read 56.3, makes it lock for the
> first time, and raises the character counts on four real captures — 25 to 27, 38
> to 39, 35 to 36, and 14 to 19. **It also makes the sensitivity sweep invent six
> to eight per cent of what it emits at minus three to minus five decibels**, where
> HM-DEC-120's property is that nothing is invented at any level, and it makes the
> synthetic smear in `ASmearIsNotTwoLengthsTests` pass for Morse.
>
> **Rejected, measured: applying it to the clock fit alone** so the is-this-Morse
> verdicts still see every mark. That is the honest structure, it breaks nothing,
> and it delivers none of the character gains — the gains and the breakage are the
> same change. **Rejected, measured: gating it on the refusal floor**, which is
> safe and does nothing, because every recording that needs it sits below the
> floor.
>
> This is Tim's because it weighs a real gain on real audio against a property he
> ruled and a session may not trade (§12.1).

> **May the unit still be averaged with key-up gaps, given what removing `Refine`
> does?**
>
> Task 2 rules "never derive the unit from key-up". Removing `Refine` takes
> `cw-2026-08-17-013347`'s fitted dit from 87.0 ms to **100.0** against a
> hand-read 100.4 (HM-DEC-145), which is the closest this project has come on that
> recording. It also turns thirteen tests red: `TheCleanRecordingsDecodeExactly` at
> 12 and 18 words a minute, the speed readout at 18, the bulletin's words,
> `prosigns-edge`, `AHeavyFistIsStillMorse`, and a §0.0 guard.
>
> The reason is now measured rather than guessed. `Refine` cancels the gate's edge
> bias by averaging a mark that reads long with a gap that reads short, which is
> exactly right when the element gap is the dit and exactly wrong when it is not.
> HM-DEC-115 measured that a real fist's element gap is shorter than its dit, and
> both of this project's adjudicated fists are Farnsworth. **A conditional form is
> possible** — average only where the fitted element gap and the fitted dit agree
> — but the tolerance is a number nobody has measured, and its removal has been
> proposed and withdrawn four times, so nothing was built.

> **The five recordings from the evening of the 20th are not in the tree.**
>
> The order says they are committed under
> `tests\fixtures\cw\captured\unadjudicated\`. That folder's newest file from the
> 20th is `cw-2026-08-20-014935` and nothing named `005902`, `010133`, `010244` or
> `010336` exists in the repository or on this machine. They are the only sample
> this project has of one fist across five recordings with four failures and one
> success, and every figure in the order's table is unverifiable without them.

### Asks still outstanding

- **Whether a mark too short to be an element may be set aside before the clock is
  fitted.** First made 2026-08-20, this session. Waiting on Tim. Nothing is in the
  tree; `CwTiming.cs` is unchanged.
- **Whether the unit may still be averaged with key-up gaps.** First made
  2026-08-20, this session. Waiting on Tim. Supersedes the older, vaguer ask about
  `Refine`, which had no numbers behind it and this one does.
- **The five recordings from the evening of the 20th.** First made 2026-08-20,
  this session. Waiting on the files.
- **The speed control needs an entry in `DECISIONS.md` and an id.** First made
  2026-08-20. Waiting on Tim. The code is on `main` and this session changed when
  the seed yields, so the entry now has to carry that too.
- **The keying meter's provisional thresholds.** First made 2026-08-20. Waiting on
  one evening's roster scored against the `meter` column.
  `CwKeyingThresholds.ConfidentSwingDb` joins them at 20 dB.
- **Whether `SHACK_FACTS.md` still holds that CI-V Transceive is off.** First made
  2026-08-20. **This may now be answerable**: the order records 110 of 110,492
  frames as the radio announcing something, which is what the `broadcast` line was
  built to measure.
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

**One item leaves the queue.** What to do about `Refine` is replaced by the
sharper version above, which carries the measurement it was missing.
