# Comparing AGC SLOW against FAST on the air

Work instruction 564, HM-DEC-268. One evening, two runs, nothing transmitted.

## The setting to flip

On the CW tab, press the small `⋯` button beside **Scan**. The last row is **AGC in CW**: `SLOW` (the default), `MID`,
`FAST`, or `leave the radio alone`. Hamlet writes it to the radio the next time you come to the CW tab. To make it take
effect at once, go to the Digital tab and back to CW. If you turn the AGC on the radio yourself, Hamlet leaves it there
until you next come to the CW tab, so for a fair comparison don't touch the radio's AGC during a run.

## What to capture

Use the same kind of signal on both settings, as close together in time as you can, so the band has had little chance
to change.

- **Best: a W1AW session on each.** Hamlet records W1AW by itself, from a minute before a scheduled session to two after,
  when you are listening on the CW tab on a W1AW Morse frequency. Run one session on `SLOW` and the next on `FAST`, on
  the same band if you can.
- **Otherwise: two ragchews or two CQ runs**, a few minutes each, one on each setting, with **Record** pressed while the
  station you want is printing (or let the automatic capture save its own five minutes).
- Write down roughly when each run started and which setting it used. Every capture sheet now says it anyway, on the
  `agc` line: what the radio reported, whether Hamlet set it or your hand did, and which choice was in force.

## What to send the web session

- The capture folders from both runs: `%AppData%\Hamlet\captures\` (Record) and `%AppData%\Hamlet\captures\auto\` (the
  automatic ones), each WAV with its `.txt` sheet.
- The day's telemetry: `%AppData%\Hamlet\telemetry\YYYY-MM-DD.jsonl` for the date of the runs. Its `cw_listen` rows,
  every ten seconds, carry `agc` and `agcSetBy`, so the two runs can be split by setting.
- Which recordings may join the board, if any, since R88 still holds until you lift it for them.

## What the web session will measure

- **Letters right against the W1AW reference**, from the ARRL's published text for each session, on each setting.
- **Junk before weak calls**: letters printed before a station's real first word, counted on each setting.
- **The shape score of the printed sender**, its median and its dips, from the `cw_listen` rows and the sheets.
- **The noise humps between letters**: on the printed sender's own window, how high the floor rises in the gaps between
  elements and letters, which FAST is expected to lift and SLOW to hold flat.

The outcome is a recommendation for the default with its numbers, and the choice stays yours.
