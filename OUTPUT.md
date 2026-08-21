# OUTPUT.md

## 1. What Claude did

### The fitted ratio for all five captures, before and after: **it cannot be reported, because the number was never the decoder's**

Claude Code on the development machine, in `C:\Source\HamLet`, on `main`. The
prompt named `PROJECT: Hamlet`, all four gate checks hold, and **no radio is
attached** (HM-DEC-093).

**`cw-2026-08-21-195617`, `-195742`, `-200036` and `-200134` are not in the
tree.** The unadjudicated folder holds five recordings from the 17th, 18th and
20th and nothing from the 21st. **No figure in the instruction's table could be
reproduced against the audio it came from.**

**And the table is measuring the decoder that was removed.** Task 1's first
question is the whole unit:

| the sheet said | where it came from | what decodes |
|---|---|---|
| `clockFit  dah 15.72 dits` | `CwSpeedEstimator.FittedDahDits` | — |
| `decoderWpm  not proved, rolling 50` | `CwSpeedEstimator` and the settled pass | — |
| `chars  0 emitted` | `_charactersEmitted`, incremented in the old `Emit` | — |
| the text on screen | `CwProbabilisticStream` | **this** |

`CwSpeedEstimator` fits a clock by clustering key-down run lengths and averaging in
key-up gaps. **It has decoded nothing since the decoder was replaced**: all three
of its `Emit` call sites are guarded by `_speed.LooksLikeMorse` and none of them
raises anything a screen can see. It still runs because the element counters, the
watch and the transmit guard hang off it.

**So a dah of 15.72 dits is a true statement about a decoder whose output nobody
can see, printed beside text produced by something else entirely.** It is not
evidence about the clock behind the words, because **the working decoder has no
fitted ratio at all** — a dah is three dits in its model, by construction, with a
Gaussian penalty on how far a segment strays. Nothing there can return 15.72.

### Task 1: the rest of the answer

**Nothing in the working decoder takes the unit, or any part of it, from key-up
gaps.** The unit is `1200 / wpm / hop` — a hypothesis, not a measurement. Key-up
spans are scored against the key-up likelihood exactly as key-down spans are
scored against key-down, and the hypothesis with the best total wins. **There is
no averaging of the kind `Refine` did**, which is what the closed ask was about.
The leading hypothesis in the instruction is killed.

**The grid ran 10 to 32 in steps of 2.** A mismatch to report: the instruction
says the reference decoder starts at 8, and it does not —
`reference_decoder.py` uses `np.arange(10, 34, 2.0)`, which is 10. **The
substantive point stands anyway**: a hypothesis at the very edge of the grid wins
by default rather than on evidence, because there is nothing below it to lose to,
so a ten-word-a-minute sender could not be fitted and a slower one could not be
reached at all.

**Whether the write is attempted:** not applicable — nothing here writes. The
decode is attempted and the number reported about it belongs to a different
decoder.

### Task 2: what was built

**The grid floor comes down to 8.** The ceiling was raised to 40 as the
instruction asked and **it was measured and reverted**: at 40 a fast hypothesis
wins where it should not, and three tests that had been passing went red —
`NoSpeedFasterThanEitherStationIsNamed` naming 38, `NothingIsInventedAtTheHandover`
reading the seam as `T K K E E TETT TT`, and a 25-word-a-minute fist falling from
its bar to 0.47. **The top of the grid invents.** The floor at 8 costs nothing and
is the half this unit is actually about. **Range: 8 to 32.**

**Every number on the sheet now comes from the decoder that produced the text**:

- The `clockFit` line is gone and a `reading` line replaces it —
  `18 WPM won out of 8 to 32, 32.5 better than silence per hop against a gate of
  15` — because the working decoder has a winning hypothesis and a likelihood
  ratio, and no fitted ratio to report.
- The character counters are incremented where characters actually leave, beside
  `CwProbabilisticStream`, instead of on the old path's dead emit.

**The speed badge keeps its old guard, deliberately.** Pointing
`CwDecoder.WordsPerMinute` at the probabilistic pass was built and measured and it
**breaks a §0.0 protection**: that pass reads a twelve-second window, so across a
handover it names a speed between the two stations, and
`NoSpeedBetweenTwoStationsIsEverNamed` caught it naming 18 where one station sends
16. A number describing neither is worse than no number. **So the working
decoder's speed is reported separately and says what it is**, which is
HM-DEC-091's own remedy.

### Task 3: on the audio that is in the tree

Every recording reads **identically** to before, because none of them is slower
than 16 or faster than 28 and the widened floor changes nothing they touch:

| recording | speed found | ratio | text |
|---|---|---|---|
| `004507` | 18 | 32.5 | `E JJ AT ARRL DOT NE T <BT> E ACH STATION HANDLING THIS ME SSAG E PE` |
| `003758` | 16 | 38.8 | `KIS QRL TU E EAN EANDE AA4MP/4 QNIK ...` |
| `003016` | 22 | 24.2 | `I<BT> HADA KPA15TT ITWAS JUNK <BT> ESTILL HVE MY ETO 91B TT JUST VFB TUBELIN` |
| `003126` | 28 | 30.2 | `A OM <BT> E <BT> I WATCH AT L EAST 2 MOVI ES A DAY WID X# WHY NOT ...` |
| `014854` (no station) | — | 6.6 | silent |
| `014935` (no station) | — | 3.3 | silent |

**HM-DEC-120 holds.** The sensitivity sweep invents **0.00 at every level** from
18 dB down to −12, and both recordings holding no keying stay silent offline and
streamed.

### Task 4: which one was wrong on `195617`

**The sidecar.** `0 characters emitted` and `nothing read` came from
`report.CharactersEmitted`, which counted the old path's emissions and has been
zero since the decoder was replaced. The terminal was right. The counters now
count what settles out of the working decoder, so the roster's `chars` column and
the sidecar's `inThis` figures describe the same instant the screen does.

The `text` field is a second, separate thing: it takes `Transcript.PlainText`,
which holds **settled** characters only. Anything still inside the one-second
decision delay is in the tip and not in that string, so a sheet written while the
screen showed only the leading edge would say `nothing read` truthfully. That is
not fixed here and is named in section 4.

### Task 5: one pitch, or which is which

**All three readings come from two instruments, and the disagreement was time.**
The terminal's `500 Hz` and the sidecar's `toneHz 400` are the same property,
`CwDecodeReport.ToneHz`, read at two moments: the capture press awaits the radio
before writing, and the decode poll runs four times a second in the gap. **The
sheet now takes one snapshot at the press** and carries it through the whole file,
and the tone line says so: *the pitch the decoder was following at the moment of
the press*. The keying meter's `400 Hz` is its own independent sweep, already
labelled as such, and **its wording is untouched**.

### Task 6: the copy-speed control is out

The checkbox, the two arrows, the figure, the three-state note, the commands, the
observable members, the decoder's seed API and its tests. **`AppSettings.CopySpeedWpm`
stays** — that is HM-DEC-066's ranking preference, a different thing that happens
to share a name. The roster's `seed` column stays and is always empty, so a roster
started before this build and one started after are the same shape.

**No decision was recorded under §12.1**, and no ruling id was taken: this unit
has no record-the-ruling task and an id must not be invented.

### HM-DEC-150 arrived in the working tree while this session was running

`CLAUDE.md` gained a ruling this session did not make and had not read: **the
minor version is the phase and the patch is the work unit within it**, and a
session reads the current version from `Directory.Build.props`, bumps it, and
reports what it moved from and to.

**Done: 1.10.0 to 1.10.1.** `CHANGELOG.md` still describes HM-DEC-063's older
convention in its own header and is not edited here, because that is a governance
file the ruling did not name and the correction is Tim's to make.

The ruling is committed separately and attributed to him rather than folded into
this unit's commit.

## 2. What Tim should expect

**A slow hand-sent station now has a hypothesis to be fitted to — the grid reaches
8 words a minute instead of stopping at 10 — but nothing in the tree is slow
enough to prove it, so what changed for certain is that every number on the sheet
now describes the decoder that produced the text.**

### The failing-test set, before and after

**55 before, 55 after.**

**Two gone:**

- `ARecordingWithKeyingInItIsReadTests.TheDecoderSaysSomethingAboutIt` — now
  passes.
- `TheOperatorCanSayHowFastItIsTests.TheCallsignSurvivesOnAHeavyFistWhenHeSaysTheSpeed`
  — the file was deleted with the control it tested.

**Two new:**

- `BroadcastWhileBusyTests.ABroadcastDoesNotAnswerTheCommandInFlight` — a rig test.
  **Passes when its class is run alone**, immediately afterwards. `HM-OPEN-055`.
- `WhatBandwidthTheDecoderListensThroughTests.HoldingTheWindowLongInTimeReadsMore`
  on `003016` — **a tie rather than a regression**: the held window reads 49 and
  the followed window now also reads 49, where the followed one used to read
  fewer. The wider grid improved the case the test was measuring against. **The
  assertion was not weakened to make it pass.**

Build clean, no warnings. Pushed to `main`.

### What is different on screen

- **The copy-speed row is gone** from the terminal.
- **The capture sidecar's `clockFit` line is now `reading`** and says which grid
  the speed won out of.
- **`toneHz` says it is the reading at the moment of the press.**
- The speed badge behaves exactly as it did.

## 3. What we should do next

- **Get the evening's five recordings into the tree.** Every figure in this
  instruction was taken from them and none could be checked.
- **The transcript's `text` field on the sidecar excludes the leading edge**, so a
  sheet written while the screen shows only provisional text says `nothing read`.
  That is a second, smaller version of task 4 and it is not fixed.
- **The old decoder is still running for its counters**, and every number it
  produces is a trap of exactly the kind this unit found. Removing it and its
  fifty tests is now the highest-value cleanup in the tree.

## 4. What's blocking us

Nothing blocks the next unit.

**Two asks, both new this session.**

> **The instruction's whole table describes a decoder whose output nobody sees,
> and four evenings of captures have been read the same way.**
>
> `clockFit`, `decoderWpm` and `chars` all came from `CwSpeedEstimator` and the
> settled pass, which have decoded nothing since 2026-08-21's replacement. A dah
> of 15.72 dits beside `T E E E E E TTON KT M 5O` was two decoders on one sheet.
> **That is fixed forward**, but every capture written before this build carries
> the same trap, and any reasoning done from those sheets about the clock behind
> the words was reasoning about the wrong decoder.
>
> **Nothing a session can do about the sheets already written.** What is worth
> deciding is whether the old decoder comes out now rather than later, because
> while it runs it keeps producing numbers that look like measurements of the
> reading and are not.

> **Whether the sidecar's `text` should include the leading edge.**
>
> It takes `Transcript.PlainText`, which is settled characters only. The working
> decoder settles one second behind, so a sheet written while the screen shows
> only the tip records `nothing read` — truthfully about the transcript and
> misleadingly about what he was looking at. **Including the tip would put
> provisional text into a permanent record**, which is the distinction HM-DEC-096
> built; excluding it leaves the roster understating what he saw. Not a session's,
> because the roster is the only instrument this project has for scoring.

### Asks still outstanding

- **Whether the old decoder comes out now**, given every number it produces reads
  as a measurement of the working one. First made 2026-08-21, this session.
- **Whether the sidecar's `text` should include the leading edge.** First made
  2026-08-21, this session.
- **The evening's five recordings are not in the tree.** First made 2026-08-21.
  Waiting on the files. Supersedes the same ask about three earlier ones.
- **Thirty seconds since the last character, for mode-follow's guard.** First made
  2026-08-21. Waiting on one evening's captures.
- **Whether `RfGain`'s hundred per cent is a defect or the right answer.** First
  made 2026-08-21. Waiting on Tim, and on one read of `1A 05 0025`.
- **The likelihood gate at 15.0 wants an evening's captures scored against it.**
  First made 2026-08-21. Waiting on one evening at the rig.
- **The keying meter's provisional thresholds**, including
  `CwKeyingThresholds.ConfidentSwingDb` at 20 dB. First made 2026-08-20. Waiting
  on one evening's roster scored against the `meter` column.
- **HM-DEC-130, whether a message too long for one keyer send may be split.**
  First made 2026-08-18. Waiting on the seam measured into the dummy load.
- **HM-DEC-098, whether §0.2's first sentence is amended to permit an attended
  automatic transmit cycle on the air.** First made 2026-08-17. Waiting on every
  interlock watched to fire into the dummy load.
- **HM-OPEN-033, the cold-start bin choice and `prosigns-easy`.** First made
  2026-08-18; HM-DEC-129 scheduled it rather than closing it.
- **HM-OPEN-007.** Open and unruled since 2026-08-14. Waiting on Tim.

**One item leaves the queue.** The copy-speed control: removed, with its wording,
its commands, its seed API and its tests.
