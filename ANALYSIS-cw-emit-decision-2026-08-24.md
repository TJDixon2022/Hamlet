# What the corpus reads, 2026-08-24

The emit decision now belongs to the character rather than to the
window. The window ratio survives as an outer silence guard at its
existing value of 15, and each
character must additionally carry more evidence than the key never
having gone down across its own span. That margin is
**0** — the point
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
| `cw-2026-08-17-013347` | VA3VRR (HM-DEC-145) | 4.0 | 57 | 1 | ` E EI I HIAEIHEEEA E EEE HEEIEE IEE E T E I E E IEEI TEEI T E HA E WVRR VA3VRR ■` |
| `cw-2026-08-17-013622` | unadjudicated | 0.2 | 55 | 0 | `E I5 SHE II 5EIEIE EEETE TE ESE E IE U EEE TSET TEEEE A EE ET EEEE EII EI E` |
| `cw-2026-08-17-134712` | N4L (HM-DEC-144) | 12.6 | 0 | 0 | `(nothing)` |
| `cw-2026-08-18-004507` | an ARRL bulletin | 5.8 | 0 | 0 | `(nothing)` |
| `cw-2026-08-18-003016` | unadjudicated | 3.9 | 0 | 0 | `(nothing)` |
| `cw-2026-08-18-003126` | unadjudicated | 7.3 | 0 | 0 | `(nothing)` |
| `cw-2026-08-18-003758` | AA4MP/4 QNIK (HM-DEC-126) | 6.3 | 34 | 3 | `KI S QR L TU ■ EAN EANDE AA4MP/4 QNIKK ■ ■EE E` |
| `cw-2026-08-20-014854` | nothing | 0.8 | 0 | 0 | `(nothing)` |
| `cw-2026-08-20-014935` | nothing | 0.0 | 0 | 0 | `(nothing)` |

## With the pitch held at the measured peak

The lock engaged after eight seconds, at whatever the interpolated
peak said then, and the tracker stopped steering from that moment.

| capture | locked to | emitted | ■ | read |
|---|---|---|---|---|
| `cw-2026-08-17-013347` | 609.3 Hz | 56 | 1 | ` E EI I HIAEIHEEEA E EEE HEEIE SEE E T E I E E IEEI TEEI T E HA EWVRR VA3VRR ■` |
| `cw-2026-08-17-013622` | refused | 55 | 0 | `E I5 SHE II 5EIEIE EEETE TE ESE E IE U EEE TSET TEEEE A EE ET EEEE EII EI E` |
| `cw-2026-08-17-134712` | refused | 0 | 0 | `(nothing)` |
| `cw-2026-08-18-004507` | refused | 0 | 0 | `(nothing)` |
| `cw-2026-08-18-003016` | 679.7 Hz | 0 | 0 | `(nothing)` |
| `cw-2026-08-18-003126` | 668.8 Hz | 0 | 0 | `(nothing)` |
| `cw-2026-08-18-003758` | 496.9 Hz | 30 | 2 | `KI S QR L TU ■ EAN EANDE AA4MP/4 QNIKK ■` |
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
| `cw-2026-08-17-013347` | `VA3VRR` | 6 chars, 2.0 to 6.7, median 2.7 | 51 chars, -11.7 to 417431412.5, median 51321927.1 |
| `cw-2026-08-17-134712` | `N4L` | none found | none found |
| `cw-2026-08-18-003758` | `AA4MP/4QNIK` | 11 chars, 19.8 to 52.2, median 30.9 | 23 chars, -25.3 to 43.5, median 23.0 |

**They overlap, and that is the finding.** The characters of an
adjudicated callsign are not separable by this quantity from the
characters around them that nobody can read. So no margin above
nought could be set from this corpus without cutting a callsign, and
the value ships at the one point that needs no calibration: a
character must not be better explained by the key never having gone
down.

