---
id: HM-DEC-186
date: 2026-09-28
refs: PHASE_PLAN.md R91 and criterion 12.4, the owner's verdict rows of 2026-09-28, CwEnvelopeDetector.cs, CwKeyingMeter.cs, CwToneTracker.cs, work instruction 477
---

**A keyed CW signal is detected as a run of flat-topped bars in a bin - level held within a
small tolerance for at least a dit, dropped, held again - at any pitch the filter passes and
at any loudness; noise is what never holds a level. When the meter finds keying at a pitch,
the tracker mixes there at once and the decoder is told a station is present.** Tim,
2026-09-28.

**What the rows showed.** Unit 476's detector tracked its floor to the middle of a keyed
signal's swing and put its threshold above every mark, seeing zero bars on four stations
including one that translated. The meter found each station's pitch within a second. The
tracker waited on the survey - twenty-two seconds on one station - and on another reached the
pitch but held its keying flag false on an 18 dB swing against a 20 dB gate, so the decoder
was mixed correctly and told nobody was there.

**What is ruled.** The detector's floor and margin are removed and replaced by run detection:
a bin is keying when its level makes flat runs of dit length or longer separated by flat
gaps. The tolerance and the shortest run derive from what a dit is and how little a keyed tone
wobbles, and are fitted to no recording. The tracker takes the meter's pitch the moment the
meter says keying, without waiting for the survey. The tracker's keying flag follows the
meter's verdict, and the 20 dB swing gate no longer decides it. The owner's words: "Real
signals will be bars, not waves."

**Whose words are whose.** The ruling is Tim's; the wording is work instruction 477's record
of it.

