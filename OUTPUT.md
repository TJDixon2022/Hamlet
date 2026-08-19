# 1. What Claude did

## The mechanism

**I cannot name a change that stops a broadcast reaching the model, because the
reproduction this order specified passes at `HEAD`.** What I can name exactly is
the measurement the order was written on, and it is an artifact:

> **`DeterminedBy.From`, `src/Hamlet.RadioEngine/Telemetry/Outcome.cs:81` —
> `RigValueState.Known => Read`. There is no `broadcast` value in that
> vocabulary and there never has been.**

Every known rig value was recorded as `read`, whatever produced it. A frequency
the operator's own dial pushed and one polled thirty seconds later were the same
word in the file. So **"broadcast by any label 0" is what the record returns for
a working radio**, and the count separates nothing.

The control is on this machine, in the operator's own telemetry, and it settles
it:

| File | Builds | Frequency observations | Broadcast |
|---|---|---|---|
| `2026-08-15.jsonl` | 1.2.0 – 1.4.0 | 22 (`read` 12, `unknown` 10) | **0** |
| `2026-08-16.jsonl` | 1.4.1 – 1.6.0 | 53 (`read` 27, `unknown` 25, `stale` 1) | **0** |
| `2026-08-17.jsonl` | 1.8.1 | 2 | **0** |
| `2026-08-18.jsonl` (yours) | 1.9.0 | 46 | **0** |

**Zero on the builds that tracked the dial, zero on the builds that do not.** Max
age on 08-16 was 122.8 s, worse than 08-18's 60.2. That figure is not measuring
what it was read as measuring.

The order says that if the reproduction passes, the fault is downstream of the
state model and its premise is wrong, and to say so. **It passes**, in both
orders, with a command in flight and with the broadcast arriving before the reply
Hamlet was waiting for. I did not build around that. I built the instrument that
settles it, because nothing in this application could answer the question either
way.

## What was genuinely broken, and is fixed

**`AppEvents.CivLink` was built by HM-DEC-092 and nothing has ever called it.**
Sent, answered, unanswered, the port and the rate: the link's own health has
never once reached the record, on any build. Ruled, built, never invoked, which is
the third time in this repository. The heartbeat calls it now.

**Nothing counted what arrived.** Four counters and two clocks now sit at the top
of `Ic7300Rig.HandleFrame`, **before** the echo test, before the transceive and
scope branches claim anything, and before the pending-request test discards the
rest — so a frame that arrives and is thrown away is still visible. Inbound, from
the radio's address, addressed to nobody (`00`), carrying a transceive command
(`00`/`01`), carrying the scope's (`27`), and the byte total.
`IsRadioBroadcasting` is **null** before anything has arrived, because a quiet
link and a radio that is not announcing are different facts.

**The record can say `broadcast`.** `RigValue.IsBroadcast` names the mechanism off
the source string the dispatcher already writes (`transceive 00`, `transceive
01`), and the provenance follows it. The order's own acceptance — "frequency
observations carry broadcast provenance at ages near zero" — was not expressible
before this change.

## The two candidates the next connect will decide between

Both are changes we made, and one number tells them apart.

**One: the radio is not announcing.** Then the frequency has only ever had the
poll, HM-DEC-109 made that poll the display's correction path, and thirty seconds
is exactly what "it does not track any more" feels like. `inboundTransceive` will
be 0 with `inbound` large.

**Two: the scope output took, and is drowning the cable.** From 1.8.0 Hamlet asks
for `27 11` automatically at connect, and HM-OPEN-042 — found last session — means
**the answer to that write was never readable**: the readback waited for an
acknowledgement the radio does not send, so a successful write and a silent one
looked identical, and Hamlet has been reporting it failed without knowing. If it
succeeded, waveform frames arrive continuously on the cable the dial's own
announcements share. `inboundScope` and `scopeShare` will be large.

## What I read

`git log -p` on the reader, the parser, the dispatcher, the provenance model and
the state model. Every named suspect:

- **HM-DEC-051** (`eb17c65`) touches teardown ordering and adds no filter. Its own
  commit message is evidence for the other side of this: *"Connecting worked and
  the app followed the radio"*, against a real IC-7300 on COM3.
- **HM-DEC-109** (`ad93fb4`) moved the frequency from `Never` to `Session` and
  removed an on-demand read. It **added** a mechanism and removed none. It is the
  backstop, and it is load-bearing only if candidate one is true.
- **The dispatcher matching path**: an unsolicited frame never reaches it. The
  transceive branches return at `Ic7300Rig.cs:725` and `:742`, before
  `_pending` is consulted at `:771`. The test proves it with a command in flight.

## Recorded under §12.1

Nothing. No ruling was made or needed.

# 2. What Tim should expect

**Whether the frequency on screen can be trusted: no change yet, and I will not
claim one.** No radio was connected in this session (HM-DEC-093), so nothing here
is evidence about yours. The engine delivers a broadcast to the model in 8 tests;
that has always been true and was true in your build too.

**One connect answers it.** Open the app, turn the dial, and read
`civ_link` in today's telemetry:

- **`inboundTransceive`** — above zero means the radio announces the dial, and the
  path is alive. Zero, with `inbound` in the hundreds, means it is not, and that
  is candidate one, settled.
- **`radioIsBroadcasting`** — `true`, `false` or absent. Absent means nothing has
  arrived at all, which is a third thing.
- **`inboundScope` and `scopeShare`** — a large share is candidate two, and it
  also means `27 11` has been succeeding while Hamlet reported it failed.
- **The frequency's own `provenance` in any `determinedBy` block** — it can now
  say `broadcast`, and its age should be a fraction of a second rather than tens.

**Build succeeds, no warnings. 1,940 tests, 2 failing, both the standing decode
baseline** — `ClearingTheTranscriptLeavesTheDecoderAlone` and
`TheBulletinDecodesToItsAnswerKey`. Neither is touched by this work.

**Nothing about the user interface changed.** This session added a record and a
test, and repaired nothing you can see.

**One flake worth knowing about, since you will hit it.** Running both suites at
once, one pass reported five app failures; two runs immediately after reported the
one standing failure, and the extra four were the headless window tests, which
build a real window and lose races under load. They are not a regression from this
work. If you see a red count above two, run `dotnet test tests/Hamlet.App.Tests`
on its own before believing it.

**Two commits, pushed to `main`. Nothing local, no branches.**

# 3. What we should do next

- **Connect and read one `civ_link` line.** It is one evening and it decides
  between the two candidates, and no amount of further reading here will.
- If it is candidate two, the fix is to stop asking for the scope output
  automatically until the waterfall is proven, and that is one line plus a ruling.
- If it is candidate one, the question is whether Hamlet should read the radio's
  transceive setting and say plainly that the dial is not being announced, rather
  than tracking at poll speed and looking broken.
- HM-OPEN-042's remaining half is unchanged: whether `27 11` now confirms.

# 4. What's blocking us

**The premise of this order.** I have to hand that back rather than work around it,
because the order says to.

---
date: 2026-08-18
refs: WORK_INSTRUCTIONS.md, HM-OPEN-042, HM-DEC-109, HM-DEC-092
---

**The frequency-provenance count that this order rests on cannot support it, and
the next step is one connect rather than another session of reading.**

"Six sessions, forty-six observations, broadcast by any label 0" was read as the
broadcast path being dead. There is no broadcast label in the vocabulary; every
known value said `read`. The same count is zero on 2026-08-15 and 2026-08-16,
which are builds you have told me tracked the dial correctly.

What is not in doubt: **the app is not tracking your radio and that is our
doing.** Two candidates above, both ours, and the numbers that separate them are
in the build now.

Rejected: naming a mechanism on the strength of the reading it would have to
contradict. The reproduction the order specified passes at `HEAD` in both orders,
so any story that ends with "the frame is discarded in the dispatcher" has to
explain a green test, and I would be building around evidence rather than on it.

What is wanted is your ruling on whether that connect happens before any further
work here, or whether you want candidate two acted on speculatively — stopping
the automatic scope request at connect — on the grounds that it costs a feature
that has never once drawn a pixel and might be eating the link.

---

The two standing questions are unchanged and still yours: **whether an attended
automatic cycle may reach an antenna** (§0.2, HM-DEC-098), awaiting the interlocks
watched into the load; and **a callsign too long for one keyer send**
(HM-DEC-130), refused until the seam between two sends is measured into the load.
