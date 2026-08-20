# OUTPUT.md

## 1. What Claude did

### Task 1, the table, reproduced in the repository

Claude Code on the development machine, in `C:\Source\HamLet`, on `main`. The
prompt named `PROJECT: Hamlet` and the four gate checks all hold. **No radio is
attached to this machine, so nothing here is evidence about the radio**
(HM-DEC-093).

**Two mismatches against the instruction, reported and not repaired silently.**

**First: four of the six recordings the table is drawn from are not in the tree.**
There is no `tests\fixtures\cw\captured\unadjudicated\` directory, and
`tests\fixtures\cw\captured\` holds the same four files it held last session.
`cw-2026-08-20-014854` and `cw-2026-08-20-014935` are not there either. So the
instruction's table cannot be reproduced as written, and what follows is every
recording that is in the tree, measured exactly the way the instruction
describes.

**Second, and more important: the separation does not reproduce from the
instruction as given, because the instruction does not say how to pick the best
tone in a window, and the two obvious answers are both wrong.** Measured, not
argued, on `cw-2026-08-18-004507`, the one recording here known to contain a
readable station keying at 500 Hz with a 57 ms dit:

| ranking | pitch it chose | median it reported |
|---|---|---|
| widest envelope swing | 700 and 800 Hz in every window | 3 to 6 ms |
| largest element share | 425 Hz in every window | 6 to 7 ms |
| **element share times element purity** | **500 Hz in every window** | **56 to 58 ms** |

The swing ranking fails because a pitch outside the receiver's own filter has
almost nothing in it, so its quiet tenth approaches zero and the decibel
difference runs to ninety while measuring silence. The share ranking fails
because off the station's pitch the envelope crosses its threshold two hundred
times in six seconds, and enough of those crossings last twenty milliseconds to
carry the share past the real answer. **What separates them is asking both
questions**: was most of this stretch spent keyed down for an element's length,
**and** were most of the key-downs elements rather than chatter. At 500 Hz that
scored 0.29 and nothing else in eight hundred hertz of candidates reached 0.10.

With that ranking, six-second windows, 400 to 1200 Hz in 25 Hz steps:

| recording | what the decoder made of it | median of windows | tones chosen | best window |
|---|---|---|---|---|
| `cw-2026-08-18-004507` | 177 characters | **57 ms** | 500 Hz, all five windows | 500 Hz, 58 ms, score 0.29 |
| `cw-2026-08-17-134712` | **nothing at all** | 5 ms | 425 to 875 Hz | **500 Hz, 54 ms, score 0.37** |
| `cw-2026-08-17-013347` | 1 character | 9 ms | 550 to 850 Hz | 625 Hz, 87 ms, score 0.19 |
| `cw-2026-08-17-013622` | 1 character | 5 ms | 575 to 650 Hz | 625 Hz, 10 ms, score 0.03 |
| synthesized noise, the control | none | **2 to 3 ms** | wanders, 425 to 1200 Hz | score 0.00 in every window |

**The separation the unit rests on does hold**, and holds by a very wide margin:
54 to 58 milliseconds with a score from 0.18 to 0.37 where there is keying,
against 2 to 3 milliseconds and a score that rounds to nought where there is not.
That is the factor of six the instruction reported, and rather more.

**And the third row is the whole argument for building this.**
`cw-2026-08-17-134712` decoded nothing at all: `characters 0 emitted, 0 unsure`,
on a capture whose own sidecar claims 35.2 dB. One of its six-second windows
scores 0.37, the highest of any window measured, at 500 Hz with a 54 ms element.
**There is a station in that recording and the decoder produced not one
character from it.** No claim is made here about what it sent.

### Tasks 2, 3 and 4, the meter

`KeyingEnvelope` moved from the test project into the engine, because the meter
needs it live. **It still shares no code with the decoder** (§12.5): no Goertzel
bank, no gate, no tracker, no reference to any of them. It was also made cheap
enough to run continuously, by replacing a cosine call per sample with a rotating
phasor and two full-length arrays with a ring, which left every measured number
identical and cut the time by two thirds.

`CwKeyingMeter` runs a six-second window, updated once a second, sweeping its own
pitch. **It never asks the decoder anything.** On the terminal, above the
`I hear a station` button, it shows one of three words in a color that only
agrees with the word (§0.6) and, beside it, the pitch, the median key-down, the
swing and the number of key-downs.

**Cost: 73.5 milliseconds per update**, sweeping 33 candidates over six seconds
of 48 kHz audio, measured and printed by a test. That is about seven per cent of
one core at one update a second, and it runs on a worker rather than on the
interface thread, where seventy milliseconds would be a visible hitch every
second.

**The holding rule, which the instruction asked to be stated exactly.** One
window that looks like keying puts the meter into **keying** immediately. It
leaves only after **fifteen consecutive windows** show nothing. With a six-second
window recomputed each second that is about twenty seconds from the last element:
six for the window to empty of it and fifteen more for the run.

**Five was tried first and measured, and it broke.** Played end to end with an
eight-second gap in the middle, the meter used its whole budget and changed its
mind while the contact was still going on. Eight seconds is barely long enough
for the other operator to send a callsign. Fifteen sits through a short over at
this project's own reference copy speed of thirteen words a minute.

**The long hold costs almost nothing, and this is why.** While it holds, the word
says so in a line underneath, and **the numbers beside it are always the newest
window's**. So when Tim turns a knob, the figures answer within six seconds
whatever the word still says. The word is the summary; the numbers are the
instrument.

**Thresholds are provisional and are all in one place**, `CwKeyingThresholds`,
each with the measurement it came from written beside it. They come from five
recordings on two nights, four real and one synthesized, and a wide gap measured
on a small sample is still a small sample.

**Task 4.** The sidecar gains a `keying` line and the roster gains one column,
`meter`, between `chars` and `text`. `read` is still last and still empty, and no
other column moved. Both carry the state and the three numbers as the meter had
them at the moment of the press.

### Task 5, not dropped

`TheMeterRunsOnLiveAudioTests` drives the meter the way the application drives
it: a recording played into a real `AudioTap` through `BufferedAudioSource` a
chunk at a time, the meter asked once a second, no decoder anywhere in the test.
It reaches **keying** on the recording that decoded and on the one that decoded
nothing, settles on **no keying** on half a minute of noise, says **listening**
before a full window has arrived, and survives a twelve-second gap between overs
without changing its mind.

## 2. What Tim should expect

**Build clean, no warnings. 2,043 tests, three failing, and they are the three
that were failing before:**

- `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`
- `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`
- `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`

Seventeen tests were added and all pass. **Nothing in the decoder changed.**
`CwGate`, `CwSettledPass`, `CwToneSurvey` and `CwDecoder` are untouched, and
`ShortestVote` is still 5. The meter drives nothing: it does not retune, does not
switch the decoder on or off, does not gate the capture, and nothing writes to
the radio.

### What it will show you tonight

**Tuned across a band with nothing on it**, you will see `listening` in grey for
the first six seconds while the window fills, and then a line like
`725 Hz, key down 3 ms, 13 dB between quiet and loud, 570 key-downs`. The pitch
will wander from one update to the next, because there is nothing there to settle
on. After fifteen seconds of that the word turns amber and reads `no keying
here`. **That is the picture of an empty band**, and the two numbers that say so
are the three-millisecond key-down and the thirteen-decibel swing.

**On a station you can hear**, within six seconds the word goes green and reads
`somebody is keying`, and the line beside it settles on one pitch and stays
there: `500 Hz, key down 57 ms, 25 dB between quiet and loud, 34 key-downs`. The
pitch stops wandering, the key-down jumps from single digits to somewhere between
about forty and eighty milliseconds, and the swing roughly doubles. **The pitch
holding still is the clearest single sign**, and it is visible before the word
changes.

**When he stops to listen**, the word stays green and a grey line appears under
it saying the last few seconds went quiet and this is the last thing it was sure
of. The numbers underneath drop to the empty-band picture immediately. That is
correct and is what the hold is for.

**And here is how to use it for the fault.** If you can hear a station and the
meter says `no keying here` with a three-millisecond key-down, the signal is
being lost before Hamlet ever sees it, and the audio gain, the filter and the
tuning are what to move. Watch the key-down number rather than the word: it will
answer within six seconds of each change, and it does not need the decoder to
have found anything at all.

**One thing that will look odd and is not.** The meter can say `somebody is
keying` while the transcript above stays empty. That is not a contradiction, it
is the instrument working: it is telling you there is a signal the decoder could
not read, which is exactly the case `cw-2026-08-17-134712` is in.

Committed and pushed to `main`.

## 3. What we should do next

- **Take the meter to the rig and find the fault.** That is what it is for, and
  nothing further should be built on top of it until an evening's rows exist.
- Get the four unadjudicated captures and the two from the 20th into the tree.
  Every number in section 1 rests on one real recording with a readable station
  and one synthesized control, which is thin.
- **Look at `cw-2026-08-17-134712` again.** It is in the repository, it contains
  keying at 500 Hz with a 54 ms element and a score higher than any other window
  measured, and the decoder produced nothing from it. That is a decoder question
  with a fixture already committed for it, which is rare here.
- When an evening of rows exists, compare the `meter` column against the `read`
  column. The rows where Tim heard a station and the meter did not are the
  evidence this unit was built to collect.

## 4. What's blocking us

Nothing blocks the next unit.

**One ask, new this session.**

> **The keying meter's thresholds stand as provisional until an evening's roster
> has been scored against them, and are not moved to make any single recording
> pass.**
>
> They are five numbers in `CwKeyingThresholds`: a six-second window, a key-down
> between 25 and 250 milliseconds, a score of 0.10, and fifteen quiet windows
> before the word changes. Each has the measurement it came from written beside
> it. The sample behind them is one real recording containing a readable station,
> two containing one character each, one containing keying the decoder missed, and
> one synthesized control.
>
> **The gap they sit in is wide and the sample is small, and those are different
> facts.** A threshold that is right on five recordings and wrong on the sixth is
> exactly what this project has been caught by before (§12.5), and the roster is
> already the instrument for finding out: every row now carries what the meter
> said, so an evening produces a column that can be scored against Tim's own ear.
>
> **Rejected: tuning them now to make `cw-2026-08-17-013622` read as keying.**
> That recording decoded one character and nobody knows whether there is a
> readable station in it, so moving a threshold to include it would be fitting to
> a case whose answer is not known. **Also rejected: waiting for more recordings
> before shipping the meter at all**, which would keep him at the rig tonight with
> no instrument, and the recordings come from evenings at the rig.

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

**One item leaves the queue.** Whether the work order's *do not push* was meant
as a ruling: this order answers it in terms, saying the previous four orders
misquoted §9.5.1 and that the error was in the orders. Nothing further is needed.
