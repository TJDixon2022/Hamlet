# The corrected noise scale, 2026-08-24

The Rayleigh scale is now taken from the quarter point by identity,
`P25 / 0.759`,
rather than by a factor of six tenths that made it 0.455 sigma. Key-up
is a proper Rayleigh density, so the noise hypothesis stays
competitive in the upper tail where noise actually lives. Both are
estimated over a rolling 2.5 s
span on both paths rather than once per recording.

Regenerate with:

```
dotnet test tests/Hamlet.RadioEngine.Tests --filter FullyQualifiedName~TheNoiseScaleTable
```

## With the outer guard bypassed

Whole-file reads, so the empty captures produce characters that can
be measured at all. `window` is what the guard would have seen.

| capture | window | chars | margins: min / median / max | read |
|---|---|---|---|---|
| `cw-2026-08-17-013347` | 17325602.95 | 100 | -22757244.18 / 38817968.10 / 453074102.29 | ` E EI EE 5 EE ES E EE V EEEH E T EE HEEE E S E E EAE ET E T E EE T E AET ETT E TE ETE T E EEEEET E TTT TTTTTTTTTT TTTTTTTTTTTTTTTTTTTTT E E E ` |
| `cw-2026-08-17-013622` | 4086060.00 | 99 | -0.69 / 3.81 / 152811536.68 | `E EE # ESEF E H EE SEEE E S E EE EEEE ER E T E E EEE E EE E E EEI IA E IETSET AEE E E T EEE E ET IEEE #EEE S E EEEEE E ETE E SEE E SEE E SE E E E E E E` |
| `cw-2026-08-17-134712` | 1.39 | 99 | -31.82 / 0.69 / 41.12 | ` EE EEE EEE EEE EEEE EE E EE E EE I E EI EE E E EIE EI E E EE EE E E I EI E E E E EE E E E E EE E E EE<HH>S I E #HIIHE TE S I E E I# 5E HESI EE EEEE E E ISE E` |
| `cw-2026-08-18-004507` | 6.96 | 48 | 6.07 / 14.24 / 36.49 | `E JJ AT ARRL DOT NET <BT> EACH STATION HANDLING THIS MESSAG E PE` |
| `cw-2026-08-18-003016` | 4.55 | 58 | -7.61 / 7.65 / 17.97 | `I <BT> HAD A KP T15TT IT WAS JUNK E <BT> E STILAI HVE MY E TO 91B E TT JUST VFB TUB LIN` |
| `cw-2026-08-18-003126` | 5.97 | 52 | -10.57 / 9.60 / 20.53 | `A OM<BT> E <BT> IWATCH AT L E<AS>T 2 MOVIESA DAY WID X# WHY NOT E E , WESNRNS , E` |
| `cw-2026-08-18-003758` | 10.80 | 57 | -16.89 / 21.95 / 54.04 | `KI S QRL TU E EAN EANDE AA4MP/4 QNIK E EEEEEE EAN EANQNIK E E E EE S E E EAN E` |
| `cw-2026-08-24-012403` | 1.10 | 49 | -0.20 / 1.24 / 8.33 | ` EE E E E E EE E EE E E EEE E ADMVUT UD0 TN DEQ 6Q E SQ DE KD0UN KD0UN K ` |
| `cw-2026-08-22-031905` | 4.93 | 39 | 0.39 / 8.09 / 18.88 | `TO . PREDICTED 10.7 K NTIMETER FLUX IS 125, 125N` |
| `cw-2026-08-23-001520` | 0.00 | 1 | 0.00 / 0.00 / 0.00 | `#` |
| `cw-2026-08-20-014854` | 0.65 | 81 | -1.89 / 0.97 / 4.61 | ` E E EE 5EEEE E EE N SEIE E E II E TE IE T E EE N TEI E E MING I ETM N ERQ GRE CTIT EE W ID PE T IS SE G T E EIGIE EP S OP 6` |
| `cw-2026-08-20-014935` | 0.11 | 106 | -0.32 / 0.69 / 3.22 | `EE E E EEI I EE IE E INEE E E E I E E E EEEEE IE EI 5 EEE EI ES S E I EI E E I E E E EE EES E E I EEE IE EEE EE EEE E EEE E E EE E E E EEIE EE E EE E SH I E E EEEE E E E` |

## Can the guard go?

The question is whether a character margin exists that silences both
empty captures and keeps all three adjudicated callsigns, with the
outer guard gone.

| | margin |
|---|---|
| the best character either empty capture produces | **4.61** |
| the weakest character of `VA3VRR` | **not read at all** |
| the weakest character of `N4L` | **not read at all** |
| the weakest character of `AA4MP/4QNIK` | 16.73 |
| the weakest character of `KD0UNKD0UNK` | 1.75 |

**No.** An empty capture produces a character scoring 4.61, at or above the weakest character of
`KD0UNKD0UNK`, so a margin that silences the noise cuts the callsign.

**But the question is smaller than it looks, and that is the more
important half.** `VA3VRR` and `N4L` are not read at all on this path, so no margin can keep them:
they are already gone before any character is judged. A margin
chosen from the callsigns that survive would be chosen from a
corpus that has quietly shrunk.

## How much the span matters

The same reads at one and a half, two and a half and four seconds, so
the provisional span arrives with its own sensitivity measured.

| capture | 1.5 s | 2.5 s | 4.0 s |
|---|---|---|---|
| `cw-2026-08-17-013347` | 16776321.8, 103 chars, VA3VRR LOST | 17325602.9, 100 chars, VA3VRR LOST | 16773939.3, 101 chars, VA3VRR LOST |
| `cw-2026-08-17-013622` | 4664523.4, 100 chars | 4086060.0, 99 chars | 4332200.7, 100 chars |
| `cw-2026-08-17-134712` | 1.1, 78 chars, N4L LOST | 1.4, 99 chars, N4L LOST | 1.7, 92 chars, N4L LOST |
| `cw-2026-08-18-004507` | 6.7, 48 chars | 7.0, 48 chars | 7.0, 48 chars |
| `cw-2026-08-18-003016` | 4.6, 56 chars | 4.6, 58 chars | 4.6, 53 chars |
| `cw-2026-08-18-003126` | 5.9, 52 chars | 6.0, 52 chars | 6.3, 52 chars |
| `cw-2026-08-18-003758` | 11.2, 60 chars, AA4MP/4QNIK kept | 10.8, 57 chars, AA4MP/4QNIK kept | 10.4, 56 chars, AA4MP/4QNIK kept |
| `cw-2026-08-24-012403` | 1.1, 52 chars, KD0UNKD0UNK LOST | 1.1, 49 chars, KD0UNKD0UNK kept | 1.2, 46 chars, KD0UNKD0UNK LOST |
| `cw-2026-08-22-031905` | 5.1, 40 chars | 4.9, 39 chars | 4.8, 40 chars |
| `cw-2026-08-23-001520` | 8048633.8, 12 chars | 0.0, 1 chars | 0.0, 1 chars |
| `cw-2026-08-20-014854` | 0.7, 79 chars | 0.7, 81 chars | 0.7, 79 chars |
| `cw-2026-08-20-014935` | 0.1, 107 chars | 0.1, 106 chars | 0.1, 109 chars |

