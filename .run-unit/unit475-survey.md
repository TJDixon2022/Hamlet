unit 475 task 2 - what CwToneSurvey.HysteresisDb gates, printed from the source (CwToneSurvey.cs, not changed)

`HysteresisDb` is 3.0 (line 195), "three either way, so six across". It is used in one place,
`Examine` (lines 564-565). For each bin the survey first splits that bin's own level history, in
decibels, into two clusters by a settled two-means (`Clusters`, lines 609 on); `midpoint` is the
mean of the two cluster means. A bin's mark mask then turns on only when its level goes above
`midpoint + 3` and off only when it drops below `midpoint - 3`. The marks that mask yields are
what everything after it judges: at least 8 marks (`MinimumMarks`), dits of 25 to 200 ms, two
duration clusters separated by at least 4.0 (`MinimumSeparation`), and a dah-to-dit ratio of 2.5
to 3.8. So the 3 dB is not a bar on how far a signal swings: it asks that the bin's quiet and loud
cluster means sit **more than about 6 dB apart**, or the mask never toggles and the bin is refused
for too few marks. The meter now accepts a swing of 17 dB, but that is a 10th-to-90th percentile
spread of a 1 ms linear envelope over six seconds, not a gap between cluster means in the survey's
own bins at its own hop, so the two figures are not directly comparable. Even so, a station
swinging 17 to 22 dB on the meter would have to lose more than two thirds of that contrast in the
survey's bins to fail a 6 dB span. **Reading the source, the hysteresis alone is an unlikely reason
for "survey nothing" on all seven rows. The mark-duration gates after it (count, dit range,
separation, ratio) are the likelier refusal, and none of their figures is in the verdict row.**
Not changed in this unit. If the owner presses "You're an idiot" while the meter says keying and
the survey still admits nothing, the next unit has a candidate. It should first record which
survey test refused, not assume it was the 3 dB.
