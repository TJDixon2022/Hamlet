READ IN THIS ORDER.

A. Hamlet reads a CQ call correctly. Steps 0, 1, 2 done; 3 partial,
   3.4 only open and not flippable this unit; 4 not started; 5 the
   owner's; 6 partial.
B. Step 6: 6.7 met - red quoted in section 1, cost at capture 862 ms
   on a pool thread, not the UI thread; 6.2 not met - its three named
   clauses hold, its lead sentence fails on the keying caption's sweep
   range; 6.6 held.
C. The rest. Section 4 raises 2 items; P10 is in the way of 6.2, none
   in the way of 6.7.

```
UNIT:       417 - complete at task 3 of 3, none dropped - 2026-09-24 09:57
PHASE GOAL: Hamlet decodes a real CQ call into the text that was sent, measured against keys, and the screen says nothing about the signal that is not so
UNIT GOAL:  The strength figure on each capture's sheet is measured over that capture's own audio and says so, watched failing first against the held figure, with its cost at the press measured
ADVANCED:   yes - 6.7 ticked with its red quoted and its cost stated; 6.2 not ticked, on P10
NUMBER:     tonePeak on 17:37 25.7 held -> 26.8 dB over the recording; capture cost 862 ms, on a pool thread
DRIFT:      0
```

## 1. What Claude did

**Complete, task 3 of 3, none dropped.** QUIVERFULL, Hamlet confirmed by the gate, `main`,
entry HEAD 12e2d442.

**Section 5 against the tree - no mismatch in the five checks.** `TonePeakRecordLine`
(`MainWindowViewModel.cs` 12373 at entry) was the only place the line was composed, called
once from `CaptureNotes`; `TheSidecarIsReReadTests` asserted `tonePeak   `; `SnrDb` is built
from `CwDecoder._lastSnrDb`; the roster's `tonePeakDb` reads `SnrDb` from the report
(`MainWindowViewModel.cs` 20601) and nothing parses the sidecar line; `CaptureNotes` has
the audio. Keyed totals at HEAD 167 over 565, the ten 35 over 156, 17:37 19 over 25.
**One figure did not match:** section 4 of the instruction quotes the held figure as 41.7 on
014854, 38.4 on 014935 and 34.7 on 013347. A fresh decoder replaying each file today gives
54.7, 56.8 and 32.2, and the saved sidecars carry `snrDb` 42.9 and 28.8 from the live
evening. The ordering the instruction complains of still holds: both empty files sit above
013347.

**Task 0.** Version 1.13.104, PHASE_STATUS names 417 and step 6, PHASE_OUTCOME entry, P9's
answer in PARKED.md. Entry round: engine 178/178; app 274/278, four lost to Avalonia's
*"You've caused dispatcher loop"* and 4/4 alone; captures 51/51, adjudicated 13/13, clean
2/2, TheSidecarDoesNotContradictItselfTests 3/3, TheSidecarIsReReadTests 2/2.
I ran the four dispatcher-loop tests as four types in one invocation, not one type
per invocation as HM-DEC-155 says. They passed; I'm reporting it so nobody finds it later.

**Task 1, the trace.** `WhatTheTonePeakIsAboutTests` (asserts nothing) and
`ToneOverNoiseByHand`, in `tests\Hamlet.App.Tests\Cw`.

**DECIDED, author's, overrulable - the method.** It is the held figure's own quantity over this
file's samples only:
- **Window:** a 40 ms Hann window, moved every 5 ms (the tracker's hop).
- **Tone:** the Goertzel power at `report.ToneHz`, the pitch the decoder tracked.
- **Noise:** the median Goertzel power of the 25 Hz grid from 300 to 900 Hz, leaving out every
  grid pitch within 125 Hz of the tone (`CwCompetitor.SeparationHz`). The window is the same
  one the tone is measured through.
- **Figure:** a reading counts only as the middle of five in a row; the figure is the highest
  such reading over the file. Nothing is held from before the file and nothing decays.
- **No tracked pitch:** where the decoder has no tone, or never measured its pitch (the
  `duty` line's gate), the line says so in words and prints no number. It does the same where
  the file holds fewer than five windows.

After seeing the first figures, I checked two alternatives before keeping this method. The
reason was what it measures, not how it orders the four:
- **Frequency neighbours:** each file's mean spectrum shows the grid below 550 Hz on 014854 and
  014935 sitting 25 to 44 dB under the passband. The neighbours can be the receiver's stopband
  (P11).
- **The tone's own quietest fifth:** it gave 93.3 dB on 013347, whose tone bin falls to
  -51.8 dB between elements under a steady -22 dBFS.

The held figure's definition was kept because the alternative fails worse. The gate hides the
stopband cases, which occur at unmeasured pitches.

| capture | held (replayed) | recording figure | at the pitch regardless | keying windows of 25 |
|---|---|---|---|---|
| 014854 | 54.7 | not measured | 50.2 | 0 |
| 014935 | 56.8 | not measured | 52.7 | 0 |
| 013347 | 32.2 | 33.4 | 33.4 | 10 |
| 17:37 | 25.7 | 26.8 | 26.8 | 0 |
| 014113 | 24.7 | not measured | 30.9 | 0 |

Ordering by held: 014935 > 014854 > 013347 > 17:37 > 014113. By the recording figure: 013347
33.4 > 17:37 26.8; the other three not measured. The by-hand version cost 835 to 970 ms per
30 s file.

**Task 2, the line.** `TheTonePeakIsAboutThisRecordingTests` was committed red at a9339312,
3 of 3:
> the sheet prints 25.7 and this recording measures 26.8
> the sheet prints 32.2 and this recording measures 33.4
> Assert.StartsWith() Failure: String "tonePeak   54.7  (the highest the tracked"... Expected start: "tonePeak   not measured"

Green 3 of 3 at 70cc798a:
- The measurement is `RecordingToneOverNoise` in `src\Hamlet.RadioEngine\Audio`: engine,
  per §0.1, and outside `Cw`.
- `TonePeakRecordLine(audio, report)` composes the line.
- **The held figure stays** on its own `heldPeak` line under its own caption, which names it
  as the roster's `tonePeakDb`.
- `SnrDb` is unchanged.

**Cost at capture:** 862, 862 and 860 ms on the longest capture in the tree (30.0 s at 48 kHz,
Debug test build). That is well over 50 ms, so it runs through `Task.Run` on a pool thread,
after the WAV is on disk. The UI thread holds only the await's continuation. I did not time the
UI thread itself.

**Test edits between red and green,** forced by the new signature: the calls now pass
`audio`. `TheSidecarIsReReadTests` got the same call change. It also got two changes the
wording did not force: its header says 417 and it writes `unit417-sidecar-*.txt`. Without that
it would have overwritten unit 411's evidence.

**6.7 ticked. 6.2 not ticked, clause by clause, on the regenerated sheets:**
- **tonePeak:** about this recording or not printed as one - **holds**.
- **elementHz:** does not report nothing measured while resolving elements; byte-identical
  to 411's - **holds**.
- **keying:** does not say no keying while counting key-downs; byte-identical to 411's -
  **holds**.
- **The lead sentence fails:** "every sentence ... is true of that capture". The keying caption
  says *an independent sweep of 400 to 1200 Hz*, but `CwKeyingMeter` sweeps through
  `KeyingEnvelope.Best`, which runs 300 to 900. The instruction said to leave `keying` as 411
  left it, so I did not repair it (P10).

**Task 3, the exit round.** Hamlet.sln builds with warnings as errors, non-incremental, 0
warnings:
- **Carry-forward:** engine 178/178. App 277/278; the one lost to the dispatcher loop was
  `TheFavoritesAreUnderTheGreenZoneTests`, 1/1 alone.
- **Floors:** captures 51/51, adjudicated 13/13, clean 2/2.
- **Named types:** TheSidecarDoesNotContradictItselfTests 3/3, TheSidecarIsReReadTests 2/2,
  CaseRosterSurvivesAnEveningTests 6/6, TheTonePeakIsAboutThisRecordingTests 3/3,
  WhatTheTonePeakIsAboutTests 1/1.
- **Keyed totals:** all keyed 167 over 565, the ten 35 over 156, 17:37 19 over 25. Identical
  to entry.
- **Source diffs:** `src\Hamlet.RadioEngine\Cw` prints nothing against entry. The transmit
  files print nothing against 7e209cb4.

Commits 4e4353f3, 2948ff53, a9339312, 70cc798a, and the exit commit carrying this file.
Every push so far returned 0; the exit push is reported in the terminal.

## 2. What the owner should expect

The strength figure on each capture's sheet is now about that capture. For 17:37 the sheet said
25.7 under a caption admitting it was not about the recording. It now says 26.8, *a figure
about this recording*, measured over the 30 seconds in the file at 600 Hz. On a recording where
Hamlet never found the station's pitch, it says `not measured` in words instead of a number:
014113 used to print 24.7, and the two empty recordings used to rate highest of all. Measuring
takes 862 ms per capture, off the UI thread, after the WAV is saved, so the window does not
freeze. The sheet appears under a second after the recording.

**Will look wrong but is not:**
- The roster's `tonePeakDb` column still carries the held figure. It now matches the sheet's
  new `heldPeak` line, not its `tonePeak` line.
- Sidecars already in the tree still say what they said.

## 3. What you should see

17:37:
```
before: tonePeak   25.7  (the highest the tracked tone ever stood above the noise beside it, held and decaying; not a figure about this recording)
after:  tonePeak   26.8  (a figure about this recording: over the 30.0 seconds in this file, the highest the tone at 600.0 Hz stood above the noise beside it, in dB)
```
014113:
```
before: tonePeak   24.7  (the highest the tracked tone ever stood above the noise beside it, held and decaying; not a figure about this recording)
after:  tonePeak   not measured  (no pitch was measured, so there is no tone in this recording to say the strength of)
```

| capture | held figure | recording figure | holds keying |
|---|---|---|---|
| cw-2026-08-20-014854 | 54.7 | not measured | no - keying at no pitch; meter 0 of 25 windows |
| cw-2026-08-20-014935 | 56.8 | not measured | no - keying at no pitch; meter 0 of 25 windows |
| cw-2026-08-17-013347 | 32.2 | 33.4 | yes - adjudicated VA3VRR; meter 10 of 25 windows |
| cw-2026-09-23-173723 | 25.7 | 26.8 | yes - a keyed CQ call per its key file; the meter calls 0 of 25 windows keying, a 16 dB swing under its 20 |

Held figures are a fresh decoder replaying the file alone. Every sheet now carries a `heldPeak`
line under `tonePeak`, holding the old number under its own caption.

## 4. What's blocking us

Nothing blocks the phase (R65). Both items are parked in `docs\phase-correctness\PARKED.md`.

**P10 - the keying caption names a sweep range nobody swept.** Proposed ruling: the caption
reads its range from `KeyingEnvelope`'s own constants (300 to 900 Hz), watched failing first,
and 6.2 ticks with it. Reasoning: it is the one false sentence standing between 6.2 and met.
Rejected: fixing it in this unit, because the instruction fixed `keying` as 411 left it.
Rejected: ticking 6.2 anyway, because its lead sentence is not true.

**P11 - "the noise beside the tone" can be the receiver's stopband.** Measured on 014854 and
014935, where the grid below 550 Hz sits 25 to 44 dB under the passband. The sheet never prints
the figure there, because the pitch was not measured. Whether noise should be taken inside the
passband only is the owner's, because it would touch the held figure that HM-DEC-091 protects.
Rejected for now: the tone's own quiet floor, which read 93.3 dB on 013347.
