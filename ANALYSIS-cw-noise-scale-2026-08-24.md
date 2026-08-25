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
| `cw-2026-08-17-013347` | 17235760.00 | 81 | -11.62 / 46849864.11 / 453074102.29 | ` E EI EE 5EEETEEE V EEEA E E EE IEEEE I EE IEET E T E EE E E EETEM E TEEEE T E IEEET E O EEETETTW EEMAEAMJOW E E ` |
| `cw-2026-08-17-013622` | 4137086.23 | 108 | -3.73 / 3.69 / 152811536.68 | `E EE # EUE# E H EE SEEE E H E EE T EN E E # E T E EE EEE E II E EE EEN IU E SI TIE IT AET T E E T TIEE ET EETEE EESEEE E II E E EETI E RI EIE EE E EIE E E HE E E E E E` |
| `cw-2026-08-17-134712` | 1.40 | 114 | -81.82 / 0.73 / 41.79 | ` EEE EE E E E E EIE E EIE E E E IEE I E EE S E E SE E E E EI E I EE E EE EE E EE E I E I E E E E A E E E E E EE E I EIIHIIS EH E #5I IISEE A E S ES E E ## E HE H E IEEE EE TES I I EISE E` |
| `cw-2026-08-18-004507` | 6.96 | 48 | 6.07 / 14.24 / 36.49 | `E JJ AT ARRL DOT NET <BT> EACH STATION HANDLING THIS MESSAG E PE` |
| `cw-2026-08-18-003016` | 4.55 | 53 | -14.93 / 7.55 / 17.97 | `I<BT> HADA KPT15TT ITWAS #K <BT> ESTILL HVE MY ETO 91B TT JUST VFB TUB LIN` |
| `cw-2026-08-18-003126` | 5.96 | 54 | -19.02 / 9.60 / 23.94 | `A OM <BT> E <BT> I WATCH AT L EAST 2 MOVI ES A DAY WID X# WHY NOT E E , WESTERNS , E` |
| `cw-2026-08-18-003758` | 10.77 | 60 | -25.85 / 21.95 / 54.04 | `K I S QR L TU E EE AN E AN D E AA4MP /4 QNI K E EEEE E E E E AN E ANQNI K E EE E E E H I E E RN E` |
| `cw-2026-08-24-012403` | 1.10 | 46 | -1.06 / 1.38 / 8.33 | ` I E E E EE E E E E EEE E ADM UUT UD0 TN DEQ 6Q E SQ DE KD0UN KD0UN K ` |
| `cw-2026-08-22-031905` | 4.93 | 39 | 0.39 / 8.09 / 18.88 | `TO . PREDICTED 10.7 K NTIMETER FLUX IS 125, 125N` |
| `cw-2026-08-23-001520` | 0.00 | 1 | 0.00 / 0.00 / 0.00 | `#` |
| `cw-2026-08-20-014854` | 0.65 | 82 | -8.18 / 1.05 / 4.61 | ` EEE E E 5 SEE E EI N SE IE E T II E T E IE T E EE N TEI E E MING I UM N ERQ GRE CTIT EE W ID PE T EES SE G T E EIGIE E P IE OP 6` |
| `cw-2026-08-20-014935` | 0.13 | 127 | -1.64 / 0.68 / 4.05 | `EE E EE ESEE EIEEE IE E E EINEE E EI E N E E E EEI E E 4 E EE EL EEE EI ES S E S E E E E N E E E EEE EI H E E EI EE EE EEE EEEE E E E EE E IEE E I EI E EEE E IEEE E EE E EE E EI ESE EE E E EIEE E E EE` |

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
| `cw-2026-08-17-013347` | 16707718.2, 76 chars, VA3VRR LOST | 17235760.0, 81 chars, VA3VRR LOST | 16672592.2, 76 chars, VA3VRR LOST |
| `cw-2026-08-17-013622` | 4712372.7, 107 chars | 4137086.2, 108 chars | 4328363.8, 107 chars |
| `cw-2026-08-17-134712` | 1.1, 74 chars, N4L LOST | 1.4, 114 chars, N4L LOST | 1.7, 113 chars, N4L LOST |
| `cw-2026-08-18-004507` | 6.7, 48 chars | 7.0, 48 chars | 7.0, 48 chars |
| `cw-2026-08-18-003016` | 4.6, 55 chars | 4.5, 53 chars | 4.6, 53 chars |
| `cw-2026-08-18-003126` | 5.9, 52 chars | 6.0, 54 chars | 6.3, 54 chars |
| `cw-2026-08-18-003758` | 11.2, 54 chars, AA4MP/4QNIK kept | 10.8, 60 chars, AA4MP/4QNIK kept | 10.4, 51 chars, AA4MP/4QNIK kept |
| `cw-2026-08-24-012403` | 1.1, 49 chars, KD0UNKD0UNK LOST | 1.1, 46 chars, KD0UNKD0UNK kept | 1.2, 42 chars, KD0UNKD0UNK kept |
| `cw-2026-08-22-031905` | 5.1, 40 chars | 4.9, 39 chars | 4.8, 40 chars |
| `cw-2026-08-23-001520` | 8048633.8, 136 chars | 0.0, 1 chars | 0.0, 1 chars |
| `cw-2026-08-20-014854` | 0.6, 80 chars | 0.6, 82 chars | 0.7, 81 chars |
| `cw-2026-08-20-014935` | 0.1, 130 chars | 0.1, 127 chars | 0.1, 127 chars |

