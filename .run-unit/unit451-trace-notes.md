# Unit 451 task 1 - every way the decoder comes to report a speed

HEAD ba05e6e1 (f96cd04e plus the task 0 records). Nothing in `src` changed. The printer is
`tests/Hamlet.RadioEngine.Tests/Cw/WhatTheSpeedCanSayItProvedTests.cs`; its output is
`.run-unit/unit451-trace-speed-state.txt` (run 2). Run 1, on the latest survey alone, is kept as
`.run-unit/unit451-trace-speed-state-run1.txt`.

## Definition (one sentence)

**proved:** `CwDecoder.WordsPerMinute` names a number (`CwDecoder.cs` 445-454: the window read a
character, the clock is not re-acquiring, the rounded speed is within 6 to 48), the window's unit
behind it was measured from the keying and not won on the grid (`CwProbabilisticStream.cs` 431-435,
and the marks' overrule at 505-514), and the tracker found keying within half the mixdown filter of
the pitch being read inside its own recent span of six surveys, three seconds
(`CwToneTracker.KeyingRecently` 712, `KeyingFoundAt` 1217-1224, `_lastKeyedHz`; the half-width is
`CwDecoder.cs` 733's).

Why six surveys and not the latest: run 1 used the latest survey's keyed verdict alone and a clean
12 wpm send at 15 dB came out proved on 900 of 5681 hops. The tree says why that is the wrong
question for a reading that trails the survey (`CwToneTracker.cs` 702-712).

## The path table

| # | file and line | evidence | current or remembered | state | reason |
|---|---|---|---|---|---|
| 1 | `CwProbabilisticStream.cs` 185, 401-404 (`Last` is `None` until the window refills, 3 s) | nothing | - | none | nothing has been read |
| 2 | `CwProbabilisticDecoder.cs` 811-818 (ratio under the gate: `Text` empty, `WordsPerMinute` the grid's winner over noise) | nothing (the grid's winner) | current window, no reading in it | none | the null hypothesis won the window; its winning speed is nobody's. HEAD's sheet prints it as "the decoder's own best hypothesis" |
| 3 | `CwDecoder.cs` 421-425, 445 (`SpeedIsReacquiring` after a follow of half the filter or more, 728-737) | the rolling reading, over a window straddling two pitches | current, but about two senders | hypothesis | a reading exists; HEAD withholds the number |
| 4 | `CwDecoder.cs` 452-454 (rounded outside 6 to 48) | the rolling reading | current | hypothesis | HEAD withholds the number. Unreachable today: the search is 8 to 40 (`CwProbabilisticDecoder.cs` 471, 492) |
| 5 | `CwProbabilisticStream.cs` 431-435 null, `Decode` searches 8 to 40 in steps of 2 (768-783) | the rolling reading, the grid's winner | current | hypothesis | HEAD shows the number; no dit was measured |
| 6 | `CwToneTracker.cs` 1008-1010, 1217-1224 (`KeyingRecently` false) | a measured unit in a window whose keying has stopped | remembered: the window holds up to 12 s after the keying | hypothesis | HEAD shows the number; a hold past its keying |
| 7 | `_lastKeyedHz` more than 30 Hz from `Stream.ToneHz` | measured unit, keying found elsewhere | remembered at this pitch | hypothesis | HEAD shows the number |
| 8 | all of the above pass | measured unit, keying at the pitch within six surveys | current | proved | |
| - | `MainWindowViewModel.cs` 12820-12823 (no decoder) | nothing | - | (sheet: not tracking) | no decoder to ask |

Hops by path, every keyed and synthetic recording (run 2): 8 on the latest survey 55742, 8 within
six surveys 30004; 6 35997; 2 24638; 3 24452; 1 20965; 5 1901; 7 200; 4 0.

## Every place in `src` that shows a speed

- `MainWindowViewModel.SpeedForTheRecord` 12818-12849: the sheet's `decoderWpm` line.
- `MainWindowViewModel.FitLine` 12859-12919: the sheet's `reading` line and the roster's fit column
  (20968), `Reading.WordsPerMinute` "won out of 8 to 40".
- `MainWindowViewModel.TerminalSpeedText` 9216, from `DetectedWpm` (11375): the terminal header,
  `MainWindow.axaml` 1665-1666.
- `MainWindowViewModel.TerminalSummary` 9301: the collapsed header, "N WPM · tail".
- `MainWindowViewModel.SpeedReacquiringText` 9229-9230: the "working out the speed" pill,
  `MainWindow.axaml` 1682-1683.
- `CwTransmitViewModel.SpeedOffer` 797, from `Transmit.HeardWpm` (11468): "They are sending at
  about N words a minute", `MainWindow.axaml` 2231.
- `CwCaseRoster.Row` 219-221, from `CwCase.Wpm` (`MainWindowViewModel.cs` 20947): the roster's
  speed column.
- Read but not shown: `DetectedWpm > 0` in the connection advice (8021); `ContactClosing` 87 and
  110 take a `ContactSummary.WordsPerMinute` that nothing in `src` constructs; `CwCharacter`'s
  per-character speed (`CwProbabilisticStream.cs` 702) reaches no display.

## What a clear and a refinement do

**A transcript clear** is `MainWindowViewModel.ClearTerminal` 10827-10832: it stamps `_clearedUtc`,
clears `Transcript`, and raises `TerminalSummary`. It does not reach `_decoder`: the stream's
envelope and `Last` (`CwProbabilisticStream.cs` 185), the tracker's pitch and survey history, the
decoder's held SNR (`CwDecoder.cs` 795-814) and the counters all run on. Its remarks (10814-10823)
state HM-REQ-035's intent. `Restart()` (`CwProbabilisticStream.cs` 319-337) is the only thing that
empties the window, reachable only behind `ClearOnAStationChange`, `const false` (`CwDecoder.cs`
159, 698). Stopping the decoder (11340-11360) is not a clear: it drops the decoder altogether.

**A refinement for the same station**, in the tree, is the tracker's `Switch` with `refining`
(`CwToneTracker.cs` 1236-1260): a move of at most one coarse bin, 25 Hz (`ConfirmWithinHz`, 248),
while a pitch is held. It counts `Retunes` and not `Follows` or `StationChanges`. `CwDecoder`
watches `Follows` only (728-737), and even a follow marks a discontinuity only at half the filter,
30 Hz, or more. So a refinement leaves `_samplesAtDiscontinuity` alone, `SpeedIsReacquiring` false
and the window as it was; the stream is re-pointed (`_probabilistic.ToneHz`, 617-621) and nothing
else in it moves. The fine bank's in-reach reading (1181-1190) moves the reported pitch without a
switch at all. The operator's `Lock()` (304-322) re-points the mixdown at the measured peak, and
`Unlock()` lets it follow again: neither touches the window, the clock or the follows.

**`Retuned()`** (385) is only `Unlock()`: the operator's dial move. The tree cannot tell a nudge
from a QSY there, because it takes no size. `AHeldPitchDoesNotOutliveItsEvidenceTests.TheReleaseStartsTheReadingFresh`
asserts that `Reading.WordsPerMinute` goes to 0 after `Retuned()`, and it is red at HEAD because
`Retuned()` keeps everything. Its own remarks call the call a QSY, a move beyond the filter's width,
which is not a refinement, so its assertion does not contradict HM-REQ-036 as the requirement is
worded. It would if `Retuned()` were ever called for a nudge inside the station. Nothing in it is
edited.

The HM-REQ-035 test is built on `ClearTerminal` against a twin decoder fed the same audio with no
clear. The HM-REQ-036 test is built on a 20 Hz step of the keyed tone (inside `ConfirmWithinHz`, so
the tracker's refinement path) and on the operator's `Lock()` at the measured peak.

## The sheet at HEAD

`.run-unit/unit451-app-speedlines-before.txt`: every speed surface's words on the 23 keyed
recordings at HEAD, read at the end of each file through `BufferedAudioSource`.
