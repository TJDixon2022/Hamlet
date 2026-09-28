---
id: HM-DEC-185
date: 2026-09-28
refs: PHASE_PLAN.md R90 and step 12, CwToneSurvey.cs HysteresisDb, CwKeyingMeter.cs ConfidentSwingDb, CwToneTracker.cs MinimumToneHz MaximumToneHz, the owner's verdict rows of 2026-09-28, work instruction 476
---

**A CW signal is detected as the audio envelope standing over a threshold above the tracked
noise floor, at any pitch the radio's filter passes; its frequency is read from the spectrum
after it is detected, and the decoder does not start until it has been.** Tim, 2026-09-28.

**What the code did.** The decoder was always decoding at a guessed pitch. The survey and the
meter tried to correct the guess afterward, each by measuring a bin's level swing over time -
6 dB and 20 dB - which the AGC FAST the app itself sets flattens, and only in bins from 300 to
900 Hz, which is the radio's sidetone setting and not where a received station lands. Twenty
strong stations gave no characters, and the owner's verdict rows showed a keyed station
refused for 1.4 dB of swing while the survey admitted nothing at any pitch.

**What is ruled.** In the owner's words: think like an oscilloscope; once the amplitude is
over a threshold, regardless of frequency, that is probably a character, and everything else
is noise. The detector is built as that, shown on the CW tab first so the owner's ear can
judge it, and wired to drive the decoder in the unit after. The 300 to 900 window, the survey's
swing gate and the meter's swing gate are not repaired; they are superseded.

**Whose words are whose.** The ruling is Tim's; the wording is work instruction 476's record
of it.

