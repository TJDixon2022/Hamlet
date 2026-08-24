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
| `cw-2026-08-17-013347` | VA3VRR (HM-DEC-145) | 20.2 | 59 | 1 | ` E EI I DIAEIHEEEA E EEE IEEEIEE I NEE E T E I E E IEEI TEEI T E HA E WVRR VA3VRR ■ ` |
| `cw-2026-08-17-013622` | unadjudicated | 3.0 | 49 | 1 | `E I5 S5E II 5EIEIE EEETE TE ESEI E II U EEET■AHEEEEN EHI ET IES ` |
| `cw-2026-08-17-134712` | N4L (HM-DEC-144) | 35.8 | 28 | 22 | ` ■ ■ ■ ■ ■E ■ ■ ■ ■ ■ ■ K ■ ■ N4L■ ■K ■■ ■ ■ ■ ■ ■` |
| `cw-2026-08-18-004507` | an ARRL bulletin | 32.8 | 50 | 1 | `E J J A T AR RL D O T N E T <BT> ■E AC H STA TION HANDLING ETHIS MESSAG E PE` |
| `cw-2026-08-18-003016` | unadjudicated | 24.1 | 58 | 3 | `I<BT> HADA KPA15TT IT WAS JUNK ■ ■ STILL HVE MY E TO 91B ■TT JETST VFB TUBELIN` |
| `cw-2026-08-18-003126` | unadjudicated | 40.2 | 53 | 4 | `A OM<BT> ■ <BT> IWATCH AT L EAST 2 MOVI ESA DAY WID X■ WHY NOT ■ ■ , WESNRNS , E` |
| `cw-2026-08-18-003758` | AA4MP/4 QNIK (HM-DEC-126) | 25.6 | 53 | 11 | `KI S QR L TU ■ EAN EANDE AA4MP/4 QNIKK ■ ■ ■ ■E AN EANQNIK ■ ■ ■■ ■ ■ ERN E` |
| `cw-2026-08-20-014854` | nothing | 6.5 | 0 | 0 | `(nothing)` |
| `cw-2026-08-20-014935` | nothing | 2.6 | 0 | 0 | `(nothing)` |

## With the pitch held at the measured peak

The lock engaged after eight seconds, at whatever the interpolated
peak said then, and the tracker stopped steering from that moment.

| capture | locked to | emitted | ■ | read |
|---|---|---|---|---|
| `cw-2026-08-17-013347` | 609.3 Hz | 60 | 2 | ` E EI I DIAEIHEEEA E EEE IHEEIEE E ■ IEE E T E I E E IEEI TEEI T E HA EWVRR VA3VRR ■ ` |
| `cw-2026-08-17-013622` | refused | 49 | 1 | `E I5 S5E II 5EIEIE EEETE TE ESEI E II U EEET■AHEEEEN EHI ET IES ` |
| `cw-2026-08-17-134712` | refused | 28 | 22 | ` ■ ■ ■ ■ ■E ■ ■ ■ ■ ■ ■ K ■ ■ N4L■ ■K ■■ ■ ■ ■ ■ ■` |
| `cw-2026-08-18-004507` | 527.1 Hz | 54 | 4 | `E J J A T AR RL D O T N E T <BT> ■ E AC■ STA TI O N E H AN D L I ■ E ET HIS M E SS RG EE EE■I` |
| `cw-2026-08-18-003016` | 679.7 Hz | 55 | 3 | `I<BT> HADA KPA15TT IT WAS ■NK ■ ■ STILL HVE MY E TO 91B AT JUST VFB TUBELIN` |
| `cw-2026-08-18-003126` | 668.8 Hz | 53 | 4 | `A OM<BT> ■ <BT> IWATCH AT L EAST 2 MOVI ESA DAY WID X■ WHY NOT ■ ■ , WESNRNS , E` |
| `cw-2026-08-18-003758` | 496.9 Hz | 53 | 11 | `KI S QR L TU ■ EAN EANDE AA4MP/4 QNIKK ■ ■ ■ ■E AN EANQNIK ■ ■ ■■ ■ ■ ERN E` |
| `cw-2026-08-20-014854` | refused | 0 | 0 | `(nothing)` |
| `cw-2026-08-20-014935` | 622.6 Hz | 0 | 0 | `(nothing)` |

