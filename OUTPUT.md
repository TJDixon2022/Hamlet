# OUTPUT.md

## 1. What Claude did

**The sweep after task 1: the constant reproduced the old table exactly, and
silenced every real recording.**

| | 18 dB | 10 dB | 5 dB | 3 dB | 2 dB | 1 dB | 0 dB |
|---|---|---|---|---|---|---|---|
| with the constant, right / wrong | 1.00 / 0.00 | 1.00 / 0.00 | 1.00 / 0.00 | 1.00 / 0.00 | 0.92 / 0.00 | 0.14 / 0.00 | 0.00 / 0.00 |

That is the pre-removal table character for character — 27 emitted down to four
decibels, 26 at three, 22 at two, one at one, nothing below. **And all six
recordings emitted nothing at all**, with `retunes 0` on every one of them: the
constant holds every move, not only the ones inside a character, so the tracker
never left the operator's configured pitch and never reached stations sitting at
500, 615, 670 and 675 hertz.

**The diagnosis is confirmed and the stopgap's expectation was wrong.** The
previous session measured the constant on the sweep alone and reported that it
"reproduces the old table exactly"; it does, and the recordings were not part of
that measurement. **The order's stop condition is met on the letter of it**, and
the reason it says to stop — that the diagnosis would then be incomplete and the
rest of the unit aimed at the wrong thing — does not hold: the interlock is
exactly the difference, which is why the constant restores the sweep. So the
constant was committed on its own as instructed, with what it does to real audio
written into the commit message, and the unit continued to the answer Tim ruled
for.

Claude Code on the development computer, `C:\Source\HamLet`, on `main`. Gate
verified against the tree: `Hamlet.sln` and `CwProbabilisticStream.cs` both
present, no `CoreHMI.sln`, no `src\CoreHMI`, and `PROJECT_CARD.md` says Hamlet.
**No radio was connected and nothing here is evidence about the radio**
(HM-DEC-093). Nothing was recorded under §12.1.

### The reconstructed file, checked first

`CwDecoder.cs` on `main` is the probabilistic host. Three checks: it builds with
no warnings and every member the app and tests reference resolves; the whole
suite reproduces **exactly** the thirty failing tests the previous report named,
by name, with no difference; and the host carries each ruled feature the removal
had to keep — the audio tap, the transmit suspension of HM-DEC-147 in all three
of its sites, the tracker, the counters incremented on settled characters, and
both character events raised from `CwProbabilisticStream` and nowhere else.

### Task 2 — what the path carries, and how fresh it is

**The Viterbi path is a sequence of whole segments over the window, and every
segment has a kind.** There are five: a dit and a dah, which are key-down, the gap
between two marks of one character, the gap between characters, and the gap
between words. The path runs to the newest hop in the window, so **the last
segment's kind says what the newest audio is inside of**. Kinds one and two and
the element gap mean a character is part-read; the character gap and the word gap
mean the tracker is free. That is the interlock's question asked of the thing that
already answered it, with nothing inferred and no threshold formed.

**It is half a second old at worst, and the tracker asks on the same half
second.** The window is re-read every `ReadEverySeconds = 0.5`, which is a hundred
five-millisecond hops; the tracker reads its survey every `SurveyEveryHops = 100`
at a five-millisecond hop, which is the same half second. **The one-second
decision delay does not apply** — it governs which characters have settled enough
to emit, not how far the path reaches, and the path reaches the newest audio.

`CwProbabilisticResult` gained `EndsInsideCharacter` and the stream exposes
`InsideCharacter` and `HopsSinceAnswer`. Nothing about the decode changed: the
same path that was already being walked is now asked one more question about
itself.

### Task 3 — the interlock is fed, the constant is out

`_tracker.MidCharacter = _probabilistic.InsideCharacter`, set after the decoder
has read the chunk. **The constant is gone from the tree.**

**A legitimate retune still happens**, which was the whole reason a constant is
not the answer. `Fixtures/CwTwoStationTests` passes — it plays one station, then a
second answering at another speed and another pitch, and asserts the tracker moves
to the answering station and that nothing is invented at the handover. On the real
recordings the tracker now makes one to five retunes each, with one or two of them
station changes, where the constant made none.

**The six tests the order named, each by name:**

| Test | Now |
|---|---|
| `CwSensitivityTests.TheDecoderReadsAsFarDownAsItDidBefore` | **green** |
| `CwSensitivityTests.ItGoesQuietRatherThanInventingLettersInTheNoise` | **still red** — at −4 dB it returns 0.64 of the message as wrong characters |
| `CwAcquisitionWindowTests.TheSlowEndReadsTheMessage(12 wpm, 18 dB)` | **green** |
| `CwAcquisitionWindowTests.AFastFistIsReadWithoutARunUp(25 wpm)` | **green** |
| `CwAcquisitionWindowTests.TheSameFistWithARunUpDoesNot(30 wpm)` | **green** |
| `CwAdjudicationTests.ClearingTheScreenLeavesTheDecoderAloneOnRealisticAudio` | **green** |
| `MostRealRecordingsSitInTheWidestWindow` | **still red** — it asserts the recordings sit in the twenty millisecond window, and nothing sets the tracker's window at all since `FollowSpeed` lost its supplier. Not this unit (§12.6) |

### Task 4 — the sweep, and the recordings

**The sweep is clean from eighteen decibels down to twelve and invents below
that.**

| dB | 18 | 15 | 12 | 11 | 8 | 6 | 3 | 0 | −4 | −6 |
|---|---|---|---|---|---|---|---|---|---|---|
| right | 1.00 | 0.94 | 1.00 | 0.92 | 0.92 | 0.81 | 0.72 | 0.56 | 0.11 | 0.00 |
| wrong | 0.00 | 0.00 | 0.00 | 0.06 | 0.06 | 0.11 | 0.19 | 0.33 | 0.64 | 0.00 |

**Both recordings holding no keying stay silent**, offline and streamed, and
`HoldingItLongStillSaysNothingAboutAnEmptyBand` passes.

**The four station recordings, against the strings in the order:**

| recording | before the removal | now |
|---|---|---|
| `004507` | `E JJ AT ARRL DOT NE T <BT> E ACH STATION HANDLING THIS ME SSAG E PE` | `E AT ARRL DOT NET <BT> E ACH STATION HANDLING ET HIS M E S S A G E P E` |
| `003016` | `I<BT> HADA KPA15TT ITWAS JUNK <BT> ESTILL HVE MY ETO 91B TT JUST VFB TUBELIN` | `E ■I KPA1■IS<HH> ■NK <BT> STILLHVEMY ETO 91B E TT JETST VFB TUBE LIN` |
| `003126` | `A OM <BT> E <BT> I WATCH AT L EAST 2 MOVI ES A DAY WID X# WHY NOT` | `E S 5 IWATTCH ATL E<AS>T 2 IOVI ES A DAY WID X■ WHY N■TT E E , WESTERNS , E` |
| `003758` | `KIS QRL TU E EAN EANDE AA4MP/4 QNIK` | `E ■HES EHEHSE AA■IH/5■IS E E E EAN EANQNI<HH>SK` |

**`003758` and `003016` still read worse than they did before the removal**, so the
order's stop condition at task 4 is met and this is where it stops. Both are much
better than they were with the interlock unfed this morning — `003758` went from
`E URL TS EHEIISEIA■IH/5■IS` to something carrying `AA■IH/5■IS`, and `003016`
recovered `<BT>` and `VFB TUBE LIN` — and neither is back to the string in the
order.

**What the residue actually is, measured rather than supposed.** On the sweep the
tracker makes **exactly one retune at every level**, from the 600 Hz it starts at
to 650, on a fixture whose tone is 640. That move is legitimate, it happens
between characters, and the interlock is right to allow it. What it costs is
this: the stream mixes each hop down at whatever pitch the tracker holds at the
time, so **a move part-way through the window leaves twelve seconds of envelope
taken at two different pitches**, and at low signal-to-noise that incoherence is
enough to lose characters. **The constant looked perfect on this fixture because
it never moved at all**, not because it protected anything.

So the remaining invention is not the interlock and cannot be fixed by it. It sits
in the survey's own following and in how the stream's window survives a retune —
both of which this order parks.

### Task 5 — the version

**`Directory.Build.props` moved 1.10.2 to 1.10.3.** One work unit, one patch,
HM-DEC-150.

### The order, checked against the rulings it cites

Every ruling this order cites says what the order says it says. HM-DEC-120 is the
emission property, HM-DEC-096's phase 3 is the interlock and describes it as the
tracker holding a move until the character in progress ends, HM-DEC-091 is one
source for every number, HM-DEC-150 is the version scheme, HM-DEC-093 with
`SHACK_FACTS.md` is the no-radio rule. **No mismatch to report**, and the two
mis-citations the previous order carried are correctly identified in this one.

### The inbound asks queue

Every id the queue names is `status: open` in `OPEN_ISSUES.md`. Nothing on it is
closed, and nothing open and relevant is missing from it. `HM-OPEN-051` is still
recorded open while HM-DEC-143 closes it, which this order parks.

## 2. What Tim should expect

**The app still invents text, less of it and only below twelve decibels: nothing
invented from eighteen down to twelve, where this morning it invented at every
level including eighteen.**

Build clean, no warnings, version 1.10.3. **28 failing, down from 30.**

Five of the eight new-since-the-removal failures went green. Two of the three
`CwAcquisitionWindowTests` levels are green and two different levels are red in
their place — 35 words a minute and the slow end at three decibels — which is the
same fixture family moving around under a real change rather than a new fault.
`CwAdjudicationTests.ASpeedChangeInRealisticAudio` is newly red: it wants a speed
change reported and gets none, because the tracker now holds through the moment
where it used to move. **The twenty-two that predate the removal are untouched.**

**What will look wrong and is not:** `MostRealRecordingsSitInTheWidestWindow` is
red for a different reason from everything else here — it asserts the tracker's
analysis window follows the fitted speed, and nothing has set that window since
`FollowSpeed` lost its supplier with the gate. That is the second half of the same
hole and it is parked.

## 3. What we should do next

- **Rule on the window after a retune.** Twelve seconds of envelope taken at two
  pitches is what the remaining invention is made of, and it is one decision:
  either the stream drops what it holds when the tracker moves, or it re-mixes,
  or it does neither and the decoder lives with it.
- **Then re-measure the sweep and the four recordings.** `003758` and `003016` are
  the two that have not come back.
- **`FollowSpeed` still has no supplier**, which is why one test asserts a window
  that nothing sets.
- **The reacquiring guard**, parked here, is still the reason
  `NoSpeedIsNamedWithoutCharactersToNameItFrom` is red.

## 4. What's blocking us

Nothing blocks the next unit. One ask.

> **When the tracker moves to a different station, the decoder's window stops
> holding audio mixed at the old pitch.**
>
> The stream mixes every hop down at whatever pitch the tracker holds at that
> moment and keeps twelve seconds of the result. A move part-way through that
> window leaves it holding two pitches at once, and the decode is made over the
> mixture. On the sensitivity sweep the tracker makes exactly one such move, from
> 600 to 650 on a fixture sending at 640, and from eleven decibels down that
> mixture costs characters and produces wrong ones: 0.06 of the message at eleven,
> 0.19 at three, 0.64 at minus four.
>
> **This is not the interlock and cannot be fixed by it.** The interlock now holds
> the tracker inside every character it reads, which is what it is for and what it
> was measured to restore; the move that costs the characters happens between
> characters, legitimately.
>
> **Three answers, and this is the choice**: the window is cleared on a station
> change, which costs up to twelve seconds of reach every time the tracker follows
> somebody; or the held envelope is re-mixed to the new pitch, which is arithmetic
> the stream does not currently do and would keep the audio; or nothing changes
> and the decoder is known to read worse for a window's length after any move.
> **Each one decides what the operator sees after somebody answers his call**, so
> it is Tim's (§12.1).

### Asks still outstanding

Carried per HM-DEC-139, verbatim until ruled.

- Whether the sidecar's `text` should include the leading edge.
- The captures from the evenings of the 20th and 21st are not in the tree.
- Thirty seconds since the last character, for mode-follow's guard.
- Whether `RfGain`'s hundred per cent is a defect or the right answer.
- The likelihood gate at 15.0.
- The keying meter's provisional thresholds.
- HM-OPEN-052, HM-OPEN-053, HM-OPEN-054, HM-DEC-130, HM-DEC-098, HM-OPEN-033,
  HM-OPEN-007.
- **`FollowSpeed` has no supplier**, first made 2026-08-21: the tracker's analysis
  window no longer follows any speed, and one test asserts that it does.
- **The mark-and-gap witness behind HM-DEC-144 and HM-DEC-145**, first made
  2026-08-21: both rulings keep their findings and no test stands behind them.
- **The window after a retune**, first made today, above.
