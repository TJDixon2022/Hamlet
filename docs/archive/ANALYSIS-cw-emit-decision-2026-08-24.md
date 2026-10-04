# What the corpus reads, 2026-08-24

The emit decision now belongs to the character rather than to the
window. The window ratio survives as an outer silence guard at its
existing value of 1, and each
character must additionally carry more evidence than the key never
having gone down across its own span. That margin is
**1** — the point
where the two explanations are equally good, rather than a place on
the scale that had to be chosen.

`■` counts characters that were heard and could not be resolved. They
are marked rather than removed, so the count of characters does not
change when the judgement does.

Regenerate with:

```
dotnet test tests/Hamlet.RadioEngine.Tests --filter FullyQualifiedName~TheEmitDecisionTable
```

## Through the production path, tracker steering

| capture | holds | window | emitted | ■ | read |
|---|---|---|---|---|---|
| `cw-2026-08-17-013347` | VA3VRR (HM-DEC-145) | 4.0 | 58 | 21 | ` ■ ■■ ■ ■■■■■■■■■■ ■ ■■■ HEEIEE II■ E T E I E E IEEI TEEI T E HA E  WVRR VA3VRR ■ ■ ` |
| `cw-2026-08-17-013622` | unadjudicated | 0.2 | 53 | 23 | `■ ■■ ■■■ ■■ ■■■■■■ ■■ETE TE SEI E EE U EE TSET TEEEE ■T I ET EE■ ■■I ■■ ■` |
| `cw-2026-08-17-134712` | N4L (HM-DEC-144) | 20.0 | 54 | 36 | `■■ ■■ ■ ■■ ■ ■ ■■E■■I■ EEE ■ E ■ ■■ ■ ■■■5I ■■ K ■ ■ ■ N 4LQ ■ K■ ■ ■EE■■ E ■■ ■ E` |
| `cw-2026-08-18-004507` | an ARRL bulletin | 5.8 | 50 | 2 | `E J J A T ■ AR RL D O T N E T <BT> ■ E AC H STA TI ON HANDLING THIS MESSAGE PE` |
| `cw-2026-08-18-003016` | unadjudicated | 4.4 | 57 | 6 | `■ ■ ■ADA KP T15TT IT WAS JUNK ■ <BT> ■ STILL HVE MY E TO 91B ■ TT JUST VFB TUB LIN` |
| `cw-2026-08-18-003126` | unadjudicated | 8.2 | 54 | 9 | `■ ■■■ ■ <BT> I WATCH AT L EAST 2 MOVI ESA DAY WID X■ WHY NOT ■ ■ ■ , WESNRNS , E` |
| `cw-2026-08-18-003758` | AA4MP/4 QNIK (HM-DEC-126) | 6.0 | 62 | 20 | `KI S QR L TU■ ■ EAN EANDE AA4MP/4 QNIK ■ ■ ■■■■ ■ ■ E AN EANQNIK ■ ■ ■■ ■ ■E ■ ■ ■■ EAN E` |
| `cw-2026-08-20-014854` | nothing | 0.8 | 0 | 0 | `(nothing)` |
| `cw-2026-08-20-014935` | nothing | 0.1 | 0 | 0 | `(nothing)` |

## With the pitch held at the measured peak

The lock engaged after eight seconds, at whatever the interpolated
peak said then, and the tracker stopped steering from that moment.

| capture | locked to | emitted | ■ | read |
|---|---|---|---|---|
| `cw-2026-08-17-013347` | 609.3 Hz | 58 | 21 | ` ■ ■■ ■ ■■■■■■■■■■ ■ ■■■ HEEIE■ IEE E T E I E E IEEI TEEI T E HA EWVRR VA3VRR ■ ■ ` |
| `cw-2026-08-17-013622` | refused | 53 | 23 | `■ ■■ ■■■ ■■ ■■■■■■ ■■ETE TE SEI E EE U EE TSET TEEEE ■T I ET EE■ ■■I ■■ ■` |
| `cw-2026-08-17-134712` | refused | 54 | 36 | `■■ ■■ ■ ■■ ■ ■ ■■E■■I■ EEE ■ E ■ ■■ ■ ■■■5I ■■ K ■ ■ ■ N 4LQ ■ K■ ■ ■EE■■ E ■■ ■ E` |
| `cw-2026-08-18-004507` | 525.0 Hz | 52 | 4 | `E J J A T ■ AR RL D O T N E T <BT> ■ E AC H STA TI O N ■ H A N D L I NG T H IS M E S S A G E ■ P E` |
| `cw-2026-08-18-003016` | 669.0 Hz | 58 | 6 | `■ ■ ■ADA KP T15TT IT WAS JUNK ■ <BT> ■ STILAE HVE MY E TO 91B ■ TT JUST VFB TUB LIN` |
| `cw-2026-08-18-003126` | 675.0 Hz | 55 | 9 | `■ ■■■ ■ <BT> I WATCH AT L EAST 2 MOVI ESA DAY WID X■ WHY NOT ■ ■ ■ , WESTERNS , E` |
| `cw-2026-08-18-003758` | 501.0 Hz | 62 | 20 | `KI S QR L TU■ ■ EAN EANDE AA4MP/4 QNIK ■ ■ ■■■■ ■ ■ E AN EANQNIK ■ ■ ■■ ■ ■E ■ ■ ■■ EAN E` |
| `cw-2026-08-20-014854` | refused | 0 | 0 | `(nothing)` |
| `cw-2026-08-20-014935` | 622.6 Hz | 0 | 0 | `(nothing)` |

## Where the margins sit, and why no margin was derived

Task 5 asks for correct characters against invented ones with a
margin in the gap. **On the path production runs there is no such
comparison to make**: both captures holding no station emit nothing
at all, so they contribute no characters minted from noise. The
window guard refuses every one of their windows before any character
is judged.

What can be measured is where the characters of the three adjudicated
callsigns sit against everything else the same recordings produced.

| capture | callsign | its characters | everything else |
|---|---|---|---|
| `cw-2026-08-17-013347` | `VA3VRR` | 6 chars, 2.6 to 9.3, median 3.5 | 52 chars, -47222634.4 to 657261776.1, median 36397937.0 |
| `cw-2026-08-17-134712` | `N4L` | 3 chars, 5.6 to 20.3, median 11.5 | 51 chars, -161.5 to 417.9, median 0.5 |
| `cw-2026-08-18-003758` | `AA4MP/4QNIK` | 11 chars, 20.4 to 51.0, median 34.3 | 51 chars, -16.8 to 36.3, median 18.4 |

**They overlap, and that is the finding.** The characters of an
adjudicated callsign are not separable by this quantity from the
characters around them that nobody can read. So no margin above
nought could be set from this corpus without cutting a callsign, and
the value ships at the one point that needs no calibration: a
character must not be better explained by the key never having gone
down.

