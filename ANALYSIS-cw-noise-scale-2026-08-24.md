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
| `cw-2026-08-17-013622` | 4086059.99 | 99 | -3.03 / 4.26 / 152811536.68 | `E EE # ESEF E H EE SEEE E S E EE E EE E ER E T E E EEE E EE E E EEN IA E IETIEIT AET E E T EEE IT IEEE IHEE EEE E EEEET E TE SEEE E SEE E SE E E E E E` |
| `cw-2026-08-17-134712` | 1.33 | 83 | -48.81 / 0.99 / 41.12 | ` EE EE EEE EEE E E E EE EE I E E EE E E IE EI E EE E E E I EI E I EE E E E EE E E EE<HH>S I E #HEISE T H I E I# HE HESI E EEEE E E IIEE` |
| `cw-2026-08-18-004507` | 6.96 | 48 | 6.07 / 14.24 / 36.49 | `E JJ AT ARRL DOT NET <BT> EACH STATION HANDLING THIS MESSAG E PE` |
| `cw-2026-08-18-003016` | 4.55 | 53 | -14.93 / 7.55 / 17.97 | `I<BT> HADA KPT15TT ITWAS #K <BT> ESTILL HVE MY ETO 91B TT JUST VFB TUB LIN` |
| `cw-2026-08-18-003126` | 5.96 | 54 | -19.02 / 9.60 / 23.94 | `A OM <BT> E <BT> I WATCH AT L EAST 2 MOVI ES A DAY WID X# WHY NOT E E , WESTERNS , E` |
| `cw-2026-08-18-003758` | 10.77 | 60 | -25.85 / 21.95 / 54.04 | `K I S QR L TU E EE AN E AN D E AA4MP /4 QNI K E EEEE E E E E AN E ANQNI K E EE E E E H I E E RN E` |
| `cw-2026-08-24-012403` | 1.10 | 46 | -1.06 / 1.38 / 8.33 | ` I E E E EE E E E E EEE E ADM UUT UD0 TN DEQ 6Q E SQ DE KD0UN KD0UN K ` |
| `cw-2026-08-22-031905` | 4.93 | 39 | 0.39 / 8.09 / 18.88 | `TO . PREDICTED 10.7 K NTIMETER FLUX IS 125, 125N` |
| `cw-2026-08-23-001520` | 13950103585681500.00 | 7 | 15436807382267600.00 / 16842028496714300.00 / 22894144462244400.00 | `#S <HH><HH>H##` |
| `cw-2026-08-20-014854` | 0.65 | 77 | -8.31 / 0.99 / 4.50 | ` E E EE 5EE E EEE N IEIE E E II E TE IE T E E N EEI E MING I ETM N ERQ GRE CTIE EE RID PET ES SE G T EEIGIE # S OP 6` |
| `cw-2026-08-20-014935` | 0.11 | 109 | -2.11 / 0.72 / 3.22 | `EE E EEII I EE IE E INEE E I E I E E E EEI EE IE EE UE EEE EI ES S E S EI E EE I E E E EE EES E E I EEE IE EEE I EEE E IEE EE EE E EE IEEEE EE E EE E I ES I E EEEEE E E E` |

## Can the guard go?

The question is whether a character margin exists that silences both
empty captures and keeps all three adjudicated callsigns, with the
outer guard gone.

| | margin |
|---|---|
| the best character either empty capture produces | **4.50** |
| the weakest character of `VA3VRR` | **not read at all** |
| the weakest character of `N4L` | **not read at all** |
| the weakest character of `AA4MP/4QNIK` | 16.73 |
| the weakest character of `KD0UNKD0UNK` | 1.75 |

**No.** An empty capture produces a character scoring 4.50, at or above the weakest character of
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
| `cw-2026-08-17-013622` | 4664523.4, 99 chars | 4086060.0, 99 chars | 4300204.9, 102 chars |
| `cw-2026-08-17-134712` | 1.1, 74 chars, N4L LOST | 1.3, 83 chars, N4L LOST | 1.6, 80 chars, N4L LOST |
| `cw-2026-08-18-004507` | 6.7, 48 chars | 7.0, 48 chars | 7.0, 48 chars |
| `cw-2026-08-18-003016` | 4.6, 55 chars | 4.5, 53 chars | 4.6, 53 chars |
| `cw-2026-08-18-003126` | 5.9, 52 chars | 6.0, 54 chars | 6.3, 54 chars |
| `cw-2026-08-18-003758` | 11.2, 54 chars, AA4MP/4QNIK kept | 10.8, 60 chars, AA4MP/4QNIK kept | 10.4, 51 chars, AA4MP/4QNIK kept |
| `cw-2026-08-24-012403` | 1.1, 49 chars, KD0UNKD0UNK LOST | 1.1, 46 chars, KD0UNKD0UNK kept | 1.2, 42 chars, KD0UNKD0UNK kept |
| `cw-2026-08-22-031905` | 5.1, 40 chars | 4.9, 39 chars | 4.8, 40 chars |
| `cw-2026-08-23-001520` | 13610741242254900.0, 8 chars | 13950103585681500.0, 7 chars | 13950103585681500.0, 7 chars |
| `cw-2026-08-20-014854` | 0.6, 79 chars | 0.6, 77 chars | 0.7, 80 chars |
| `cw-2026-08-20-014935` | 0.1, 111 chars | 0.1, 109 chars | 0.1, 112 chars |

