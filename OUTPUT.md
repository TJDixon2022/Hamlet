# OUTPUT.md

## 1. What Claude did

### Task 3: the question, and the answer is no

Claude Code on the development machine, in `C:\Source\HamLet`, on `main`. The
prompt named `PROJECT: Hamlet` and the four gate checks all hold. **No radio is
attached** (HM-DEC-093). **Nothing in `src/` changed**, which is this unit's whole
point. The amplitude rule is in the tree and `Refine` is not, as the instruction
states.

Last session's fatal pair, re-measured on windows the transitions chose:

| recording | characters | lowest | median | highest |
|---|---|---|---|---|
| `cw-2026-08-17-013347`, real | 14 | **0.353** | 0.362 | 0.400 |
| `cw-2026-08-18-003016`, real | 53 | **0.363** | 0.745 | 0.864 |
| `cw-2026-08-18-004507`, real | 46 | 0.682 | 0.825 | 0.905 |
| `cw-2026-08-18-003126`, real | 51 | 0.915 | 0.953 | 0.953 |
| `cw-2026-08-18-003758`, real | 20 | **no burst to fit** | | |
| **`cw-2026-08-20-014854`, invented** | 1 | **0.557** | 0.557 | 0.557 |
| `cw-2026-08-20-014935` | 0 | — | | |
| `prosigns-easy` | 11 | 0.953 | 0.987 | 0.997 |
| `exchange-easy` | 28 | 0.985 | 0.989 | 0.996 |
| **`tightfist-easy`** | 23 | **no burst to fit** | | |

**A real character still comes out below an invented one, and the overlap is now
wider.** Last session a real character emitted at 0.389 against an invented one at
0.470, an overlap of 0.081. Choosing the window puts real characters at **0.353**
and the invented one at **0.557**: an overlap of **0.204**. **Choosing the window
made it two and a half times worse.**

**And there is a second reason, which is worse than the first.**
`cw-2026-08-18-003758` emits twenty real characters and `tightfist-easy` emits
twenty-three, and **at no emission does either have a burst that can be fitted at
all.** A gate requiring one would silence both outright, and `tightfist-easy` is an
easy-tier fixture, which HM-DEC-114 makes a hard failure.

**The idea is dead. A fifth candidate for HM-OPEN-054 is eliminated.** Task 4 built
nothing, as instructed.

### Task 1: how the window was chosen, and whether it finds the callsign

**The method.** The gaps between consecutive transitions are split into a short
group and a long one by their own two means, seeded from the extremes. A burst is a
maximal run of transitions linked by gaps from the short group. **Nothing is
declared in advance** — no length, no count, no rate — and the only floor is the
eight edges the existing clock fit already refuses to run below. HM-DEC-144's known
boundaries are not an input: using the answer to find the answer proves nothing.

**It does not find `N4L`.** On `cw-2026-08-17-134712` it returns eight bursts, and
the one covering the callsign runs **17.82 s to 27.57 s with 114 transitions**,
fitting a clock at 85 ms with an agreement of **0.220**. The callsign's own
twenty-one edges are a fifth of a window otherwise made of band noise. Handed the
callsign window by name, the same fit gives 0.677 at 48 ms. **The method cannot
recover by itself the window that made this idea look promising.**

Its strongest burst on that recording is at 4.08–4.39 s, agreeing 0.727 at 59 ms.
**That is noise.**

**And it confidently proposes windows where there is nothing to find**, which the
instruction named as the most valuable possible result:

| recording | bursts | strongest agreement | interval |
|---|---|---|---|
| **`cw-2026-08-20-014854`**, holds nothing | **20** | **0.721** | 15 ms |
| **`cw-2026-08-20-014935`**, holds nothing | **24** | **0.736** | 175 ms |
| `cw-2026-08-17-013347`, decodes `VA3VRR` | 2 | 0.393 | 199 ms |
| `cw-2026-08-17-013622` | 3 | 0.572 | 37 ms |

**Both empty recordings produce better-fitting bursts than a recording that decodes
a real callsign.**

### Task 2: the clock on those windows

| recording | bursts | strongest | interval found |
|---|---|---|---|
| `cw-2026-08-17-013347` | 2 | 0.393 | 199 ms |
| `cw-2026-08-17-013622` | 3 | 0.572 | 37 |
| `cw-2026-08-17-134712` | 8 | 0.727 | 59 |
| `cw-2026-08-18-004507` | 6 | 0.905 | 21 |
| `cw-2026-08-18-003016` | 12 | 0.864 | 19 |
| `cw-2026-08-18-003126` | 9 | 0.968 | 47 |
| `cw-2026-08-18-003758` | 11 | 1.000 | 25 |
| `cw-2026-08-20-014854` | 20 | 0.721 | 15 |
| `cw-2026-08-20-014935` | 24 | 0.736 | 175 |
| `coverage-easy` | 4 | 0.994 | **100** |
| `exchange-easy` | 6 | 0.995 | **100** |
| `prosigns-easy` | 4 | 0.990 | **101** |
| `fast-easy` | 5 | 0.965 | **48** |
| `tightfist-easy` | 1 | 0.642 | 19 |

**Recordings holding a station: 0.393 to 1.000. Recordings holding nothing: 0.721
and 0.736. They do not separate** — the empty pair sits in the middle of the range
the real ones occupy.

**The intervals are the tell.** On the easy tier every fit lands on the sender's
own dit: 100, 100, 101, 48. On the real captures they land on 15, 19, 21, 25, 37,
59, 175 and 199 milliseconds, which are not anybody's dit. **The clock is fitting
the noise's own texture, and fitting it well.** On `134712` the interval found is
59 ms, close to HM-DEC-144's 56.3 — but on a burst at 4 seconds, nowhere near where
the callsign is.

**One control was weak and is reported as one.** Half a minute of shaped noise with
no tone in it produced **no transitions at all**, because the tracker never latches
without a tone, so there was nothing to fit. The two real recordings holding no
keying are the meaningful controls, and they produce four hundred and ninety
transitions between them.

### Task 4: what it means

**The separation does not exist, so there is nothing to say about what a gate
would need.** Choosing the window was the last structural idea available without
crossing into character structure, and it fails twice over: it puts real characters
further below invented ones than the inherited window did, and it leaves two
recordings that decode real text with no window to fit at all. What would have to
be true for it to work is that bursts of dense transitions are a proxy for somebody
sending, and **on real off-air audio they are not** — band noise chatters in bursts
too, and it does so more often and more regularly than a station sending a callsign
does.

## 2. What Tim should expect

**No. Choosing its own window does not rescue the clock; it makes it worse.**

**Nothing shipped and nothing in `src/` changed.** What you are running tonight is
exactly what you were running before: the amplitude rule, and no `Refine`.

**Five candidates for HM-OPEN-054 have now been measured and eliminated**, each
with numbers: the survey's verdict, the ratio band, the transition clock, the
peak's prominence, and now the chosen window. **The one thing every one of them has
in common is that it tries to tell a station from noise by the shape of the
transitions**, and on real off-air audio the noise has a shape too.

**The number that keeps this alive.** `134712`'s callsign window, when handed over
by name, fits a clock at 48 ms and agrees at 0.677 against a hand-verified dit of
56.3 ms. **The information is in the audio.** Nothing found so far can locate that
window without being told where it is.

**Build clean, no warnings. 2,100 tests, four failing, and they are the four
expected:**

- `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`
- `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`
- `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`
- `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt`

Four tests were added, all measurement, all passing. The character-structure
boundary was not crossed: nothing here asked which character a mark belonged to or
distinguished element gaps from character gaps.

## 3. What we should do next

- **Stop looking for a keying test and consider whether `Refine` needs one.**
  Five candidates in five sessions have failed the same way. `Refine` is blocked
  because it invents on one recording; the question that has not been asked is
  whether the invention can be prevented at its own source rather than by a gate
  standing in front of it.
- **The one thing that does separate these recordings is the keying meter**, which
  is forbidden to the decoder for a good reason. **That reason deserves
  re-examining as a question rather than treated as settled**, because it is now
  the only instrument in the project that has ever told these recordings apart, and
  five sessions have been spent looking for a second one.
- **Adjudicate `cw-2026-08-18-004507`.** It went 25 to 26 characters two sessions
  ago and nobody knows whether either number is any good. It is the recording with
  the most text in it.
- Keep a second recording with a readable callsign when one is heard. `N4L` is
  still the only ground truth in nine real recordings.

## 4. What's blocking us

**`Refine` is still blocked and HM-OPEN-054 is still open**, with five candidates
eliminated.

**One ask, new this session.**

> **The chosen window is rejected, and with it the whole family of tests that ask
> whether the transitions look like keying.**
>
> Five have now been measured: the survey's verdict, the ratio band, the transition
> clock on the decoder's window, the peak's prominence, and the clock on a window
> the transitions chose. **All five ask the same question in different words** —
> does this pattern of on and off times look like a person sending — and all five
> fail on the same audio, because band noise through a narrow filter produces
> patterns that look like a person sending.
>
> The measurements say so plainly. `cw-2026-08-20-014935` holds no keying at any
> pitch and produces twenty-four bursts, the best fitting a clock at 0.736;
> `cw-2026-08-17-013347` decodes a real callsign and manages 0.393. A real
> character comes out at 0.353 and an invented one at 0.557.
>
> **Rejected: a sixth test of the same family.** The failure is not in any one
> statistic and a sixth would cost another session to find that out. **Also
> rejected: relaxing the withdrawal condition on `Refine`** so it ships with the
> invention, which HM-DEC-090 settled: seventeen hundred characters once came out
> of half a minute of band noise, every one marked, and marking was not enough.

### Asks still outstanding

- **Whether the whole family of transition-shape tests should be abandoned**, and
  what replaces it. First made 2026-08-20, this session. Five candidates
  eliminated with numbers. Nothing is in the tree.
- **What keeps the decoder silent on an empty band, so `Refine` can ship.** First
  made 2026-08-20. `Refine` is not in the tree.
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

**Nothing leaves the queue this session.**
