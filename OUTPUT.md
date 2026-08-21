# OUTPUT.md

## 1. What Claude did

### Task 1: how quickly transmit state is known, and by whom

Claude Code on the development machine, in `C:\Source\HamLet`, on `main`. The
prompt named `PROJECT: Hamlet`, all four gate checks hold, and **no radio is
attached** (HM-DEC-093), so every transmit test drives the state directly.

**The poll is four times a second and full break-in is invisible to it.**
`RigField.TransmitStatus` is a `Live` field, and `RigPollPlan.LiveInterval` is
**250 ms**. Break-in switches between elements — tens of milliseconds — so the
poll cannot see a single element, and **nothing in this unit's hold-off is
measured against break-in because nothing here can measure it.** That is stated
rather than assumed, as the instruction asked.

| question | answer |
|---|---|
| where it arrives | `CivReads.cs:177`, `RigField.TransmitStatus`, `1C 00`, cited to Full Manual 19-7 |
| what carries it | `RigState[RigField.TransmitStatus]`, and `RigState.IsTransmitting` when the number is 1 |
| polled or broadcast | **polled**, `RigPollRate.Live`, every 250 ms |
| how stale it can be | `LiveFreshFor` is **1.5 s**; a reading older than that is marked stale rather than shown as current |
| what the diagnostics screen reads | `RigReadout.cs:137` maps the field to the label `Transmitting`, off the same `RigState` |

**The "transmit guard" is a different fact entirely.** `CwTransmitGuard` reads
**broadband audio level** and answers whether the *receiver is muted*
(HM-DEC-095), which it uses to stop the gate's trackers learning from silence. It
is not the radio's transmit state and it never was. Both are kept, and they are
not the same thing.

**Every consumer of transmit state today**: `AutoCall` (the dummy-load cycle),
`TransmitReadiness` (refuses a send while already keyed), `BandScanner` (stops
scanning), `TransmissionWatch` (the send-duration record), `CwTransmitViewModel`
(the keyed indicator) and `RigStateMonitor` (marks SWR and power unknown when the
transmitter is off). **Not one of them is the decoder.** Hamlet has read this
correctly for months and nothing stopped it decoding the operator's own sending.

### Task 2: decoding suspends, and what the hold-off rests on

`CwDecoder.RadioIsTransmitting(bool?, DateTime)` takes the radio's own answer.
While suspended, `Process` takes the audio into the tap and **returns before the
tracker, the gate or the probabilistic decoder sees any of it** — so the survey
cannot retune to a sidetone either, and the pitch being read is still there
afterwards.

**Half a second, asymmetric on purpose.** Suspension is immediate; resumption
waits. A late suspension puts his sending on screen as somebody else's and an
early resumption does the same with the tail of it. The figure is measured
against **two poll intervals**, so one dropped reply cannot resume
mid-transmission, and against `CwTransmitGuard`'s own measurement of about
**24 ms of transmit-receive hang** with a ramp behind it, which it already holds
150 ms for.

**Suspension does not cost the station.** The new decoder holds no fitted speed
and no tracked noise floor to lose — the speed is an outer hypothesis re-searched
every read, the noise scale comes from the window's own percentiles. What it holds
is the twelve-second envelope, and suspension **drops** the audio rather than
feeding the decoder silence, so the evidence either side of a transmission is
intact when it ends.

**But the audio clock keeps running.** `CwProbabilisticStream.Skip` advances the
hop count without touching the envelope. Without it every character read after a
transmission was stamped as though the transmission had never happened — measured:
43 characters all stamped inside a fifteen-second suspension. With it, none are.

**Nothing is held and released later.** The audio is dropped before any decoder
sees it, so there is nothing to release.

### Task 3: what the terminal says

> You are sending, so Hamlet is listening to you rather than to the band.
> Whatever you key is yours and never appears here as somebody else's. It picks
> the band up again a moment after you stop.

### Task 4: how sent and received are kept apart

**Only the decoder can write to the transcript, and it only ever reads audio.**
There are four ways in — `Offer`, `OfferEdge`, `Settle`, `Append` — and a sweep of
every `.cs` file under `src/Hamlet.App` finds exactly four call sites, all of them
attaching or detaching a `CwDecoder` event:

```
MainWindowViewModel.cs:3109  _decoder.LeadingEdge      += Transcript.OfferEdge;
MainWindowViewModel.cs:3110  _decoder.CharacterSettled += Transcript.Settle;
MainWindowViewModel.cs:3157  _decoder.LeadingEdge      -= Transcript.OfferEdge;
MainWindowViewModel.cs:3158  _decoder.CharacterSettled -= Transcript.Settle;
```

`CwDecoder` raises both only from `CwProbabilisticStream`, which is fed audio and
nothing else. Sent text goes the other way entirely: composed as a `CwMessage` and
handed to the radio's keyer as CI-V `17`. **No file in the engine both composes a
keyer message and raises a decoded character**, and a test asserts that too.

**What would have to go wrong**: somebody would have to call one of those four
methods from a path that is not the decoder's — the send panel, the phrasebook,
the auto-call cycle. The sweep fails if they do. **This holds whatever the
transmit state says**, which is what the instruction asked for.

### Task 5: the four tests, and HM-DEC-120

Six tests in `HamletDoesNotDecodeYourOwnSendingTests`, all green:

| | result |
|---|---|
| suspended while transmit is asserted | **0 characters**, 6,000 chunks dropped |
| resumes when it drops | 66 characters, `...EACH STATION HANDLING ET HIS MESSAGE PE` |
| break-in cycling costs nothing | 67 characters, tracker still on 501 Hz, bulletin still read |
| nothing from the suspended stretch is released | **0** stamped inside it, against 77 when never suspended |
| suspension immediate, resumption waits | asserted at 100, 500 and 700 ms |
| not knowing is not transmitting | decoding runs on an unknown state |

**HM-DEC-120 still holds**, run rather than assumed: sixteen tests covering
`NothingIsEmittedAnywhereBelowTheFloor`, both recordings holding no keying, and
the whole probabilistic decoder suite — all pass. The sensitivity sweep still
invents nothing at any level.

### Task 6: the ruling is HM-DEC-147

**Established from both places, as instructed.** `DECISIONS.md` holds 001–095 then
134–146; `CLAUDE.md` §1 holds index rows up to 146; a sweep of every `.md` and
`.cs` in the tree finds nothing above 146. **147 was free** and is now recorded in
`DECISIONS.md` with an index row at the top of §1, which `DecisionLogOrderTests`
passes on.

### Task 7: the screen does not move

**One advisory region of fixed height**, 72 px, below the transcript. Its content
swaps by priority and it holds its space whether or not it has anything to say.
The order is in `MainWindowViewModel.Advisories`:

1. **transmit suspension** — a terminal that has stopped without saying why reads
   as a quiet band
2. the handover note — somebody else started sending
3. the tip mark — nothing is coming behind the leading edge
4. the decode note
5. the capture note — what Windows is doing to the input
6. the decoder story — what Hamlet can see when it produces nothing
7. the ceiling note
8. the copy-speed note

**Two rows below it became always-present with their contents changing**: the
revisions row and the receive offer, both fixed at 26 px with their buttons
appearing rather than the rows. **Above the transcript**, the input-level text now
occupies reserved space instead of appearing on content — it sat above the
transcript, so it appearing pushed the thing he is reading down the screen.

**Nothing was removed.** The keying meter is where it was and its wording is
untouched. **And several of them were saying versions of the same thing at once**,
which is why a single region rather than a stack of always-present panels: the
decode note, the decoder story and the tip mark are all ways of saying nothing is
being read.

**To add a message**: put it in `Advisories` at the place its urgency earns. Do
not add a panel below the transcript and do not make an existing one conditional;
either brings the jump back.

`BindingHealthTests` passes, so every binding in the rebuilt window resolves.

### Task 8: dropped, and it is a judgement between two costs

The copy-speed control is inert — the new decoder reads no seed. Making it live
means constraining the hypothesis search, which is a design question about a
decoder three days old; removing it means taking away a control ruled two days
ago. **A control that looks live and does nothing is its own confident wrong
answer**, and which way to resolve that is Tim's. The ask is in section 4.

## 2. What Tim should expect

**While you are sending, the terminal says so and reads nothing** — "You are
sending, so Hamlet is listening to you rather than to the band" — instead of
filling with fragments of your own keying presented as a station. **And the screen
does not move any more**: the advisories below the transcript now share one region
of fixed height that swaps its contents, so nothing appearing or disappearing
reflows what you are reading.

### The failing-test set, before and after

**55 before, 55 after, and the sets are identical.** No test that was passing now
fails, and no test that was failing now passes. The full list is unchanged from
the one this unit inherited: fifty describe the decoder removed two units ago,
three are `HM-OPEN-055`'s flaking rig tests, and the remaining two are
`CwTerminalTests` cases that also belong to the old decoder.

Nine tests were added, all green: six in
`HamletDoesNotDecodeYourOwnSendingTests` and three in
`SentTextNeverEntersTheReceivedStreamTests`.

Build clean, no warnings. Pushed to `main`.

### What to watch for tonight

- **Resumption takes half a second.** If you send a short burst you will see the
  band come back a beat after you stop. That is the hold-off, not a stall.
- **The advisory region is one message at a time.** If you were used to seeing two
  boxes at once, the second is now behind the first by priority rather than gone.
- **The copy-speed control still does nothing.** It is inert and was left alone
  deliberately; see section 4.

## 3. What we should do next

- **Resolve the copy-speed control**, which is the only thing on that screen that
  claims to do something and does not.
- **Delete the fifty dead tests**, which is its own unit and is now the only thing
  standing between this project and a green suite.
- **Score the likelihood gate** against tonight's roster, which is what it has been
  waiting for.

## 4. What's blocking us

Nothing blocks the next unit.

**One ask, new this session.**

> **The copy-speed control is inert and which way to fix it is a judgement between
> two costs.**
>
> It sets a seed that the probabilistic decoder does not read, and the wording
> beside it describes a fitted speed and an operator seed that no longer exist.
> **A control that looks live and does nothing is its own confident wrong answer**
> (§0.5.1: a control's resting appearance says it can be pressed).
>
> **Making it live** means the operator's figure narrows or weights the hypothesis
> search — a real design question about a decoder that is three days old, and one
> that could reintroduce the failure HM-DEC-146 records, where a figure wrong by
> six words a minute cost five sixths of the copy.
>
> **Removing it** takes away a control ruled two days ago and built on a
> measurement that was real at the time: on the heavy fist it took the callsign
> from lost to read. That measurement was against the old decoder, which no longer
> exists.
>
> Not settled here, because §12.1 puts anything touching what the display asserts
> outside what a session may decide, and because the honest answer may be neither
> — the new decoder found 16, 18, 22 and 28 words a minute on four recordings with
> no help at all.

### Asks still outstanding

- **The copy-speed control: make it live, or remove it.** First made 2026-08-21,
  this session. Waiting on Tim. It supersedes the older ask about giving it a
  `DECISIONS.md` id, which was about a control that then did something.
- **The likelihood gate at 15.0 wants an evening's captures scored against it.**
  First made 2026-08-21. Waiting on one evening at the rig. The roster's `meter`
  column and the case press are the instrument.
- **Three recordings named in an earlier instruction are not in the tree**
  (`cw-2026-08-21-015834`, `-020033`, `-015432`). First made 2026-08-20. Waiting
  on the files.
- **The keying meter's provisional thresholds**, including
  `CwKeyingThresholds.ConfidentSwingDb` at 20 dB. First made 2026-08-20. Waiting
  on one evening's roster scored against the `meter` column.
- **HM-DEC-130, whether a message too long for one keyer send may be split.**
  First made 2026-08-18. Waiting on the seam measured into the dummy load.
- **HM-DEC-098, whether §0.2's first sentence is amended to permit an attended
  automatic transmit cycle on the air.** First made 2026-08-17. Waiting on every
  interlock watched to fire into the dummy load. **This unit did not transmit, did
  not enable transmitting and did not touch the interlocks.**
- **HM-OPEN-033, the cold-start bin choice and `prosigns-easy`.** First made
  2026-08-18; HM-DEC-129 scheduled it rather than closing it.
- **HM-OPEN-007.** Open and unruled since 2026-08-14. Waiting on Tim.

**One item leaves the queue.** The speed control's `DECISIONS.md` entry, replaced
by the sharper question above: the control it was going to record no longer does
anything.
