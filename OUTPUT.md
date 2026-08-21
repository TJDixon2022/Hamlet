# OUTPUT.md

## 1. What Claude did

### Task 1: the port, against the Python, on the same recording

Claude Code on the development machine, in `C:\Source\HamLet`, on `main`. The
prompt named `PROJECT: Hamlet`, all four gate checks hold, and **no radio is
attached** (HM-DEC-093).

`cw-2026-08-18-004507.wav`, the ARRL bulletin, tone 501 Hz:

```
python   E JJ AT ARRL DOT NE T =    E ACH STATION HANDLING THIS ME SSAG E PE
C#       E JJ AT ARRL DOT NE T <BT> E ACH STATION HANDLING THIS ME SSAG E PE
```

**Character for character identical.** The only difference is `=` against
`<BT>`: the Python's table spells `-...-` as `=` and Hamlet's alphabet spells it
as the prosign it is. Both found **18 words a minute** with no seed, and both put
the likelihood ratio at **32.5**.

The other five agree as well:

| recording | Python | C# | text |
|---|---|---|---|
| `003016` | 24.2, 22 WPM | **24.2, 22 WPM** | `I<BT> HADA KPA15TT ITWAS JUNK <BT> ESTILL HVE MY ETO 91B TT JUST VFB TUBELIN` |
| `003126` | 30.9, 28 WPM | 30.2, **28 WPM** | `A OM <BT> E <BT> I WATCH AT L EAST 2 MOVI ES A DAY WID X# WHY NOT E E , WESTERNS` |
| `003758` | 39.2, 16 WPM | 38.8, **16 WPM** | `KIS QRL TU E EAN EANDE AA4MP/4 QNIK E E EAN EANQNIK` |
| `004507` | 32.5, 18 WPM | **32.5, 18 WPM** | above |
| `014854` | 6.1 | 6.6 | nothing, below the gate |
| `014935` | 2.8 | 3.2 | nothing, below the gate |

The small ratio differences are the tone: the Python finds its own pitch by FFT
and the port is handed the tracker's, which is where it comes from in the app.
**Every speed matches exactly and no text differs.**

### Task 2: streaming, and what it costs

`CwProbabilisticStream` keeps a rolling twelve-second envelope, re-reads it twice
a second, and settles anything more than **one second** behind the newest audio.

**One second is Bell's own figure** and it is the right one here for a reason
that can be stated: the evidence that decides where a character ended is the gap
*after* it, so a delay shorter than a word gap settles the last letter of every
group before the thing that would have corrected it has arrived.

**The cost is 7.4 to 8.4 per cent of real time** on this machine, for the whole
twelve-hypothesis speed search. That was the one figure the reference said nobody
had measured, and it turns out the search is cheap: the offline pass reads thirty
seconds of audio in a fifth of a second. **No hypotheses were dropped.**

Streamed, the bulletin reads:

```
E J J A T AR RL D O T N E T <BT> E ACH STATION HANDLING ET HIS M E S S A G E P E
```

**Word spacing is worse than the offline pass** — a twelve-second window fits its
own word-gap hypothesis, and shorter windows see fewer word gaps to fit it from.
The letters are the same. Not chased, per the instruction.

### Task 3: it is the only thing feeding the terminal

`CwDecoder` now owns a `CwProbabilisticStream`, feeds it the same audio and the
tracker's current pitch, and **`CharacterSettled` and the leading edge are raised
from that stream and from nowhere else.** The old path's own emit sites are
gone: `FlushCharacter` increments a counter instead of speaking, and the
provisional tip site no longer raises anything.

The terminal takes a new `LeadingEdge` event carrying the whole revisable tail,
and `CwTranscript.OfferEdge` **replaces** the tip rather than appending to it —
so a letter the decoder changes its mind about changes on screen, which is the
point of deciding late and is drawn in the italic tip the terminal already has.

**What was kept, as instructed**: `CwToneTracker` and the coarse survey, the
keying meter, the audio tap, the transmit guard, the element counters, the
capture press, the roster, the sidecar and the case measure. All of those hang
off `CwDecoder` and Tim marks cases tonight.

**What was not done: the old decode path is still in the tree.** `CwGate`,
`CwSpeedEstimator`, `CwSettledPass`, `Refine`, the vote window and the element
floors all still run — they feed the counters, the watch and the revisions record
that the sidecar reads — **but nothing they produce reaches a screen.** Deleting
them means triaging every test that describes them, and that is more than
remained of this session. **It is unfinished and it is named as unfinished
rather than reported as done.**

### Task 4: the silence, proved

**Yes: one gate value both reads the stations and silences the empty band, and
the gap is wide.**

| | likelihood ratio |
|---|---|
| four recordings with a station | 24.2 to 38.8 |
| **gate** | **15.0** |
| two recordings with no keying at any pitch | 3.2 and 6.6 |

The synthesized sensitivity sweep, through the wired decoder:

| level | correct | **invented** |
|---|---|---|
| 18 dB down to 3 dB | **1.00** | **0.00** |
| 2 dB | 0.92 | 0.00 |
| 1 dB | 0.14 | 0.00 |
| 0 dB and below | 0.00 | **0.00** |

**Nothing is invented at any level.** HM-DEC-120 holds, and it holds because the
all-key-up hypothesis competes rather than because a guard caught something.

Eleven tests record all of it in `TheProbabilisticDecoderTests`, all green.

**No decision was recorded under §12.1.**

## 2. What Tim should expect

**Tonight the terminal reads a strong signal as words instead of fragments** —
the bulletin recording goes from `O T ■T ■■ T ■T ■ O   ■ N D L I ISE SSRG E ■`
to `E J J A T AR RL D O T N E T <BT> E ACH STATION HANDLING ET HIS M E S S A G E
P E`, with no speed set, no seed and the speed found on its own.

### What got worse

- **Word spacing.** The new decoder breaks words in places the old one did not
  and the streaming path is worse at it than the offline one. Letters are right;
  spaces are scattered.
- **Reach at the very bottom.** At 1 dB the old decoder returned 0.81 of the
  message and this one returns 0.14. Above 3 dB the new one is perfect where the
  old one was not, and **it never invents**, which the old one only avoided by a
  refusal floor.
- **The speed readout says whichever hypothesis won**, which is a different kind
  of number from the old fitted one. Task 5 was to reword the panel around that
  and **it was dropped whole** — the wording beside the text still describes a
  fitted speed and an operator seed. Wrong wording beside right text.
- **The copy-speed control still sets a seed the new decoder does not read.** It
  is inert. Not removed, because removing it touches the panel and that was the
  dropped task.

### The test count

**2,169 tests, 55 failing**, against five before the session started. That is
the replacement, not a surprise: fifty of the fifty-five exist to describe the
decoder that was taken out. `CwDecoderTests` accounts for twenty-three,
`CwFixtureTests` nine, the fixture-tier tests seven, `CwDisplacementFloorTests`
six and `CwSettledPassTests` two, and all of them assert the old architecture's
behaviour. **They are the ones the instruction says to delete, and they were not
deleted.** Three of the rest are the rig tests of `HM-OPEN-055`, which flake and
pass on a rerun.

What matters more is which properties survived, and these were run and pass:

- `CwRefusalFloorTests.NothingIsEmittedAnywhereBelowTheFloor` — nothing invented
  at any level.
- `NothingIsReadFromAudioWithNoKeyingTests` — both empty recordings silent.
- `ASmearIsNotTwoLengthsTests.TheRecordingWithNoKeyingInItNowSaysNothing` —
  silent.
- All eleven of `TheProbabilisticDecoderTests`.

Build clean, no warnings.

Pushed to `main`.

## 3. What we should do next

- **Delete the old decode path and its tests**, which is the unfinished half of
  task 3. Until it is gone the tree carries two decoders, one of them dead, and
  the red count hides anything new.
- **The panel's language**, which was task 5 and was dropped. The speed readout,
  the copy-speed control and the settled-versus-provisional wording all describe
  machinery that no longer exists.
- **Word spacing on the streaming path**, once tonight's captures say whether it
  matters at the rig. A longer window costs latency; a word-gap prior costs
  nothing and was not tried.

## 4. What's blocking us

Nothing blocks the next unit. The decoder is in and reading; what remains is
removal and wording.

**One ask, new this session.**

> **The gate at 15.0 is a number somebody chose in a wide gap, and tonight is the
> first chance to score it.**
>
> Measured on six recordings: 24.2 to 38.8 with a station, 3.2 and 6.6 without,
> and the gate sits at 15. Any value from about 10 to 20 gives the same answers
> on every one of them, so nothing in the tree distinguishes them. **The roster's
> `meter` column and the case press are exactly the instrument for this**: a row
> where he heard a station and the transcript is empty is the gate closing on
> something real, and a row with text he could not have heard is the gate
> letting something through.
>
> Not a session's to settle, because the number decides what the display asserts
> and the evidence to choose it does not exist yet.

### Asks still outstanding

- **The likelihood gate at 15.0 wants an evening's captures scored against it.**
  First made 2026-08-21, this session. Waiting on one evening at the rig.
- **Three recordings named in an earlier instruction are not in the tree**
  (`cw-2026-08-21-015834`, `-020033`, `-015432`). First made 2026-08-20. Waiting
  on the files.
- **The speed control needs an entry in `DECISIONS.md` and an id.** First made
  2026-08-20. Waiting on Tim. **It is now inert**: the new decoder reads no seed,
  so the entry may be a removal rather than a ruling.
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

**Four items leave the queue**, closed by this instruction: whether a clock fit
may exclude runs below a share of its own unit; whether a mark too short to be an
element may be set aside before the clock is fitted; whether the unit may still
be averaged with key-up gaps; and what width the gate should look through. **All
four were questions about a decoder that no longer decides anything.**
