# OUTPUT.md

## 1. What Claude did

### Task 1, the table, reproduced in the repository

Claude Code on the development machine, in `C:\Source\HamLet`, on `main`. The
prompt named `PROJECT: Hamlet` and the four gate checks all hold. **No radio is
attached to this machine, so nothing here is evidence about the radio**
(HM-DEC-093).

**The six recordings were not in the tree when the session started and arrived
part way through**, as `6f93c32 Captures from the shack, unadjudicated`, pushed
while task 2 was being built. Task 1 was run twice: once on the four captures
that were there, and again on all ten once the rest appeared. The table below is
the second run, and it is the instruction's own table.

Six-second windows, sweeping 400 to 1200 Hz in 25 Hz steps, best tone per window:

| recording | characters the decoder emitted | median of windows | tones chosen | best window |
|---|---|---|---|---|
| `cw-2026-08-18-004507` | 177 | **57 ms** | 500 Hz, all five | 500, 58 ms, score 0.29 |
| `cw-2026-08-18-003126` | 168 | **49 ms** | 675 Hz, all five | 675, 50 ms, score 0.37 |
| `cw-2026-08-18-003016` | 69 | **48 ms** | 650 to 675 Hz | 675, 49 ms, score 0.34 |
| `cw-2026-08-18-003758` | 41 | **44 ms** | 500 Hz in four of five | 500, 45 ms, score 0.27 |
| `cw-2026-08-20-014854` | **nothing** | **7 ms** | 575 to 650 Hz, wandering | 600, 11 ms, score 0.18 |
| `cw-2026-08-20-014935` | **nothing** | **5 ms** | 575 to 850 Hz, wandering | 575, 5 ms, score 0.06 |
| synthesized noise, control | none | **2 to 3 ms** | 425 to 1200 Hz, wandering | score 0.00 throughout |

**44 to 57 milliseconds against 5 to 7. The separation reproduces exactly**, and
the instruction's figures are confirmed on its own recordings.

**But it does not reproduce from the instruction as written, and that is worth
recording.** The instruction says to take the best tone in each window and does
not say what best means. The two obvious answers were built and measured and both
are wrong, on `cw-2026-08-18-004507`, the recording known to key at 500 Hz with a
57 ms dit:

| ranking | pitch it chose | median it reported |
|---|---|---|
| widest envelope swing | 700 and 800 Hz in every window | 3 to 6 ms |
| largest element share | 425 Hz in every window | 6 to 7 ms |
| **element share times element purity** | **500 Hz in every window** | **56 to 58 ms** |

Swing fails because a pitch outside the receiver's own filter has almost nothing
in it, so its quiet tenth approaches zero and the decibel difference runs to
ninety while measuring silence. Element share fails because off the station's
pitch the envelope crosses its threshold two hundred times in six seconds, and
enough of those crossings last twenty milliseconds to carry the share past the
real answer. **What works is asking both questions**: was most of this stretch
spent keyed down for an element's length, and were most of the key-downs elements
rather than chatter.

**And the two gates are both load-bearing, which the new files proved.**
`cw-2026-08-20-014854` has three windows scoring 0.18, 0.13 and 0.09, at or above
the score threshold. Their medians are 11, 7 and 7 milliseconds. **The score alone
would have called that station keying.** The element-length gate is what stops it,
and without those two recordings nothing here would have caught that.

### Task 2, 3 and 4, the meter

`KeyingEnvelope` moved from the test project into the engine, because the meter
needs it live. **It still shares no code with the decoder** (§12.5): no Goertzel
bank, no gate, no tracker, no reference to any of them. It was made cheap enough
to run continuously by replacing a cosine call per sample with a rotating phasor
and two full-length arrays with a ring, which left every measured number identical
and cut the time by two thirds.

`CwKeyingMeter` runs a six-second window, updated once a second, sweeping its own
pitch, and **never asks the decoder anything**. On the terminal, above the
`I hear a station` button, it shows one of three words in a color that only
agrees with the word (§0.6) and, beside it, the pitch, the median key-down, the
swing and the number of key-downs.

**Cost: 73.5 milliseconds per update**, sweeping 33 candidates over six seconds of
48 kHz audio, measured and printed by a test. About seven per cent of one core at
one update a second, and it runs on a worker rather than the interface thread,
where seventy milliseconds would be a visible hitch every second.

**The holding rule, stated exactly.** One window that looks like keying puts the
meter into **keying** immediately. It leaves only after **fifteen consecutive
windows** show nothing. With a six-second window recomputed each second that is
about twenty seconds from the last element: six for the window to empty of it and
fifteen more for the run.

**Five was tried first and measured, and it broke.** Played end to end with an
eight-second gap in the middle, the meter used its whole budget and changed its
mind while the contact was still going on. Eight seconds is barely long enough for
the other operator to send a callsign. Fifteen sits through a short over at this
project's own reference copy speed of thirteen words a minute.

**The long hold costs almost nothing, and this is why.** While it holds, the word
says so in a line underneath, and **the numbers beside it are always the newest
window's**. When Tim turns a knob the figures answer within six seconds whatever
the word still says. The word is the summary; the numbers are the instrument.

**Thresholds are provisional and are all in one place**, `CwKeyingThresholds`,
each with the measurement it came from beside it. Six recordings on two nights is
a small sample however wide the gap in it.

**Task 4.** The sidecar gains a `keying` line and the roster gains one column,
`meter`, between `chars` and `text`. `read` is still last and still empty, and no
other column moved.

### Task 5, not dropped, and run against the real six

`TheMeterRunsOnLiveAudioTests` drives the meter the way the application drives it:
a recording played into a real `AudioTap` through `BufferedAudioSource` a chunk at
a time, the meter asked once a second, no decoder anywhere in the test.

- **keying** on all four that decoded, ending in keying.
- **no keying** on both presses from the 19th, never once claiming otherwise.
- **no keying** on half a minute of synthesized noise.
- **listening** before a full window has arrived.
- **keying held** through a twelve-second gap between overs.

### Two corrections to last session's report

**The band label.** Last session concluded that `cw-2026-08-18-003016`'s
`14028000 Hz` beside `40 m` came from the surviving `SelectedBand` fallback in
`CapturedBand()`. **That file is now readable and it says otherwise.** Its
frequency line reads `(read from the radio)`, so the read branch was taken, and
its band line carries no provenance clause at all, which the current code always
writes. It was produced by a binary predating `a50bc47`. **The gap that was fixed
was real and this file is not an instance of it.**

**The 69 and 233.** Both files are now readable and both are the first capture of
their session: `cw-2026-08-18-003016` reads `sinceLast 69 characters, 752
elements` and `cw-2026-08-20-014854` reads `sinceLast 69 characters, 359837
elements`. So neither is a stale value carried from an earlier run, which was the
leading hypothesis. **Two separate sessions, one decoding a real station in half a
minute and one accumulating from noise across seven hours, both arrived at exactly
69 characters and 233 resolved elements.** No mechanism in the tree produces that,
and it remains unexplained. It is noted rather than theorised about.

## 2. What Tim should expect

**Build clean, no warnings. 2,053 tests, three failing, and they are the three
that were failing before:**

- `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`
- `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`
- `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`

Twenty-seven tests were added and all pass. **Nothing in the decoder changed.**
`CwGate`, `CwSettledPass`, `CwToneSurvey` and `CwDecoder` are untouched and
`ShortestVote` is still 5. The meter drives nothing: it does not retune, does not
switch the decoder on or off, does not gate the capture, and nothing writes to the
radio.

### What it will show you tonight

**Tuned across a band with nothing on it**, you will see `listening` in grey for
the first six seconds while the window fills, then a line like `725 Hz, key down
3 ms, 13 dB between quiet and loud, 570 key-downs`. **The pitch will wander from
one update to the next**, because there is nothing there to settle on. After
fifteen more seconds the word turns amber and reads `no keying here`. That is the
picture of an empty band, and the three-millisecond key-down and the
thirteen-decibel swing are what say so.

**On a station you can hear**, within six seconds the word goes green and reads
`somebody is keying`, and the line beside it settles on one pitch and stays there:
`500 Hz, key down 57 ms, 25 dB between quiet and loud, 34 key-downs`. **The pitch
holding still is the clearest single sign and it arrives before the word
changes.** The key-down jumps from single digits to somewhere between about forty
and eighty, and the swing roughly doubles.

**When he stops to listen**, the word stays green and a grey line appears under it
saying the last few seconds went quiet and this is the last thing it was sure of.
The numbers underneath drop to the empty-band picture straight away. That is
correct and is what the hold is for.

**How to use it for the fault.** If you can hear a station and the meter says
`no keying here` with a key-down in single digits, the signal is being lost before
Hamlet sees it, and the audio gain, the filter and the tuning are what to move.
**Watch the key-down number rather than the word**: it answers within six seconds
of each change and needs the decoder to have found nothing at all.

**Both presses on the 19th read exactly like the empty band.** Played through the
meter now, `cw-2026-08-20-014854` and `cw-2026-08-20-014935` settle on `no keying
here` and never once say otherwise. Had this existed that evening it would have
told you at the radio what the roster told you the next morning.

**One thing that will look odd and is not.** The meter can say `somebody is
keying` while the transcript above stays empty. That is not a contradiction, it is
the instrument doing its job: `cw-2026-08-17-134712` produced no characters at all
and one of its windows scores 0.37 at 500 Hz with a 54 ms element, the highest
score of any window measured anywhere.

Committed and pushed to `main`, rebased onto your captures commit.

## 3. What we should do next

- **Take it to the rig and find the fault.** Nothing more should be built on top
  of the meter until an evening's rows exist.
- **Look at `cw-2026-08-17-134712`.** It is committed, it contains keying at
  500 Hz with a 54 ms element scoring higher than any other window measured, and
  the decoder emitted nothing from it. That is a decoder question with its fixture
  already in the tree, which is rare here.
- Adjudicate the five unadjudicated captures when there is an evening for it. The
  meter's table above is measurement and not a verdict, and four of them are used
  as `decoded` in a test only on the strength of their own sidecars' character
  counts.
- When an evening of rows exists, compare the `meter` column against `read`. The
  rows where Tim heard a station and the meter did not are the evidence this unit
  was built to collect.

## 4. What's blocking us

Nothing blocks the next unit.

**One ask, new this session.**

> **The keying meter's thresholds stand as provisional until an evening's roster
> has been scored against them, and are not moved to make any single recording
> pass.**
>
> They are five numbers in `CwKeyingThresholds`: a six-second window, a key-down
> between 25 and 250 milliseconds, a score of 0.10, and fifteen quiet windows
> before the word changes. Each has the measurement it came from beside it.
>
> **The gap is wide and the sample is small, and those are different facts.**
> Three windows of `cw-2026-08-20-014854` scored at or above the score threshold
> and were caught only by the element-length gate, which is the sample teaching
> something on its sixth recording, and there is no reason to think a seventh
> would not. The roster is already the instrument for finding out: every row now
> carries what the meter said, so an evening produces a column scoreable against
> Tim's own ear.
>
> **Rejected: tuning them now so `cw-2026-08-17-013622` reads as keying.** That
> recording produced one character and nobody knows whether there is a readable
> station in it, so moving a threshold to include it would be fitting to a case
> whose answer is not known. **Also rejected: holding the meter back for more
> recordings**, which would leave him at the rig with no instrument, and the
> recordings come from evenings at the rig.

### Asks still outstanding

- **The keying meter's provisional thresholds.** First made 2026-08-20, this
  session. Waiting on one evening's roster scored against the `meter` column. The
  numbers are in `CwKeyingThresholds` and nothing else reads them.
- **Whether `SHACK_FACTS.md` still holds that CI-V Transceive is off.** First made
  2026-08-20. Waiting on one capture taken with the radio connected, so the
  `broadcast` line added last session has something to report. The change is in
  the tree at `MainWindowViewModel.BroadcastDuringCapture` and
  `CivLinkHealth.LastTransceiveUtc`, and nothing acts on the answer yet.
- **HM-DEC-130, whether a message too long for one keyer send may be split.**
  First made 2026-08-18. Waiting on the seam between two sends measured into the
  dummy load. `CwMessage.Split` already exists and nothing calls it for this.
- **HM-DEC-098, whether §0.2's first sentence is amended to permit an attended
  automatic transmit cycle on the air.** First made 2026-08-17. Waiting on every
  interlock watched to fire into the dummy load, including the link pulled
  mid-cycle. The cycle is built and is dummy-load only.
- **HM-OPEN-033, the cold-start bin choice and `prosigns-easy`.** First made
  2026-08-18; HM-DEC-129 scheduled it rather than closing it. Waiting on its own
  work order. The fixture is held out and the survey defect is untouched.
- **HM-OPEN-007.** Open and unruled since 2026-08-14, named in HM-DEC-140 as the
  reason the queue's own premise is worth re-testing. Waiting on Tim.

**Two items leave the queue.** The work order's *do not push*, which this order
answers in terms. And last session's question about the two nights reading 69 and
233, which is now answered as far as the tree can answer it: both are first
captures of their session, so nothing was carried, and the identity is a
coincidence nothing in the code produces.
