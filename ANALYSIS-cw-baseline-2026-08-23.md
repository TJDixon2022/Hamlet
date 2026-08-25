# The CW decoder's baseline, 2026-08-23

Every figure below was measured by code committed in the same session
that wrote this file. Nothing is copied forward from a review or a
brief, and nothing here is a target: it is what the shipped decoder
does today, so later work can be judged by how it moves.

Regenerate with:

```
dotnet test tests/Hamlet.RadioEngine.Tests --filter FullyQualifiedName~TheCwBaselineTable
```

## The corpus, capture by capture

`shipped` is the production path: the streaming windower, with the
sender's unit measured from the window and handed to the decoder as
its only speed hypothesis. `grid` is the offline whole-file decode
with `atWordsPerMinute` null, so the speed grid searches. **The two
differ in more than the speed** because one reads a rolling window and
the other reads the whole file at once, so the gap between them is an
upper bound on what the forced speed is worth rather than a
measurement of it. The production default is untouched either way.

`witness` is `KeyingEnvelope`'s verdict at that character's own
moment, swept 400 to 1200 Hz over six seconds and sharing nothing with
the decoder. `E-share` is the share of emitted letters that are `E`,
and `single-character words` is the share of whitespace-delimited
words that are one character long.

**The span LLR is comparable within a recording and not across
them.** It is a sum of per-hop log-likelihoods, and the per-hop
difference works out at roughly the squared ratio of the signal
amplitude to the noise scale, both of which are estimated from the
recording's own envelope. A quiet recording therefore produces
enormous numbers rather than confident ones, and the estimate setting
that scale is `Percentile(sorted, 25) * 0.6`, which is the very thing
the next unit is scoped to look at.

### `cw-2026-08-17-013347`

30.0 s at 48000 Hz, read at 600 Hz.

**Adjudicated reading: `VA3VRR (HM-DEC-145)`.** Quoted from the ruling rather than from any decoder.

| | shipped | grid |
|---|---|---|
| characters emitted | 130 | 81 |
| E-share | 45 % | 63 % |
| single-character words | 63 % | 55 % |
| words per minute read | 16 | 24.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 16 | 12 % | 80 % | -9.0 / 335.5 / 769.6 |
| said no keying | 50 | 36 % | 50 % | 256.7 / 982820776.1 / 8744467815.7 |
| had not decided | 64 | 59 % | 65 % | 155001981.3 / 717495878.4 / 5913441603.5 |

What each read:

```
shipped: HIAAEEEISIHEHEEEIEEA EA E E EEEEE S HEEHEEIIEEEE II NE IEEE E T E T ET E E I I E E E  E I IEEEEII  TE TEEEEI TI T T E E E HEHAA EE ERWEWHVEVRRAR R S VVAAS■3E3HVEVRRAR R  ■  ■ ■
grid:     E EI EE 5EEETEEE V EEEA E E EE IEEEE I EE IEET E T E EE E E EETEM E TEEEE T E IEEET E O EEETETTW EEMAEAMJOW # # 
```

### `cw-2026-08-17-013622`

30.0 s at 48000 Hz, read at 600 Hz.

| | shipped | grid |
|---|---|---|
| characters emitted | 106 | 108 |
| E-share | 54 % | 37 % |
| single-character words | 41 % | 55 % |
| words per minute read | 30 | 38.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 0 | no characters | no characters | nothing measured |
| said no keying | 49 | 53 % | 45 % | 6.8 / 19.6 / 155.9 |
| had not decided | 57 | 54 % | 35 % | 51122070.2 / 535667334.7 / 2590878389.2 |

What each read:

```
shipped: HE EE IIII I 55EIEIEIEIEE EE EEETETE E TE TE E ESS  ■E E E HIE U U EU EEEEETST■U T TEETEEE E  A A E■ E■  EE EN T EEE EEE■ ■ ■ ■EEI  ■E E E  E E■ ■EEITT
grid:    E EE # EUE# E H EE SEEE E H E EE T EN E E # E T E EE EEE E II E EE EEN IU E SI TIE IT AET T # E T TI## ET #E#E# ####E# # ## # E #### # R# ### ## # ### # # ## # # # # #
```

### `cw-2026-08-17-134712`

30.0 s at 48000 Hz, read at 600 Hz.

**Adjudicated reading: `N4L (HM-DEC-144)`.** Quoted from the ruling rather than from any decoder.

| | shipped | grid |
|---|---|---|
| characters emitted | 49 | 0 |
| E-share | 14 % | no characters |
| single-character words | 73 % | no characters |
| words per minute read | 25 | 40.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 39 | 18 % | 64 % | -54.8 / 9.7 / 1299.0 |
| said no keying | 10 | 0 % | 86 % | -672.5 / -56.0 / 12947.4 |
| had not decided | 0 | no characters | no characters | nothing measured |

What each read:

```
shipped: ■ NTNT T ■  ■ ■  ■ R R SD ' E4LLML ZTZT ■ ■ T ■KK  ■  ■ ■ ■ ■EE■ E■ E E ■ ■ E■ ■ ■
grid:    (nothing)
```

### `cw-2026-08-18-004507`

30.0 s at 48000 Hz, read at 501 Hz.

| | shipped | grid |
|---|---|---|
| characters emitted | 113 | 48 |
| E-share | 13 % | 12 % |
| single-character words | 76 % | 14 % |
| words per minute read | 24 | 18.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 101 | 13 % | 74 % | 174.0 / 594.4 / 1102.8 |
| said no keying | 0 | no characters | no characters | nothing measured |
| had not decided | 12 | 17 % | 88 % | 66.6 / 266.1 / 369.5 |

What each read:

```
shipped: A A T  T E AEARR R RRL L D D M O O T  T N N E  E T  T N <BT><BT>  ■ E ■ E A ANACC H H E SESTATA TA TI I GOTON N I HEHATANTNDDLLIINNGG  T THHIEISS T MEMESSSSSASANAGG E  E A PE
grid:    E JJ AT ARRL DOT NET <BT> EACH STATION HANDLING THIS MESSAG E PE
```

### `cw-2026-08-18-003016`

30.0 s at 48000 Hz, read at 669 Hz.

| | shipped | grid |
|---|---|---|
| characters emitted | 136 | 53 |
| E-share | 11 % | 4 % |
| single-character words | 44 % | 0 % |
| words per minute read | 27 | 22.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 119 | 12 % | 46 % | 88.1 / 337.8 / 883.7 |
| said no keying | 0 | no characters | no characters | nothing measured |
| had not decided | 17 | 6 % | 20 % | 145.5 / 329.1 / 480.5 |

What each read:

```
shipped: ADAA D KAKP EP TAT11H15TE5TT IT ITIT W WAEAS ES JJUJUNTNK K ■ <AS> ■ ■ ■ ■ S STITIRLALL IL HIHVEVE M MKY Y E T E TMO EO O9E9J1T1BB  ■TE ■TT T W JEEJETSTST T V VFFBFB TB TUUBB I LELIN
grid:    I<BT> HADA KPT15TT ITWAS #K <BT> #STILL HVE MY ETO 91B TT JUST VFB TUB LIN
```

### `cw-2026-08-18-003126`

30.0 s at 48000 Hz, read at 675 Hz.

| | shipped | grid |
|---|---|---|
| characters emitted | 127 | 54 |
| E-share | 10 % | 9 % |
| single-character words | 49 % | 44 % |
| words per minute read | 25 | 28.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 115 | 11 % | 43 % | 113.2 / 412.4 / 1335.3 |
| said no keying | 0 | no characters | no characters | nothing measured |
| had not decided | 12 | 0 % | 67 % | -79.3 / 751.8 / 1842.0 |

What each read:

```
shipped: <BT>  ■  ■ N <BT><BT> I I R WAWATTTCCHH AE AT AT L EL EAEEASTST S 22 T MTMOOVVII ES ESAA D DAEDACY AY WIWIDID N XTXY■■ A WIWHNHYY N NGOTOT  ■  ■ ■  ■ M ■, , W WEIESTESTERRNINS TS ■, E
grid:    A OM <BT> # <BT> I WATCH AT L EAST 2 MOVI ES A DAY WID X# WHY NOT # # , WESTERNS , E
```

### `cw-2026-08-18-003758`

30.0 s at 48000 Hz, read at 501 Hz.

| | shipped | grid |
|---|---|---|
| characters emitted | 122 | 60 |
| E-share | 16 % | 13 % |
| single-character words | 64 % | 66 % |
| words per minute read | 24 | 28.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 108 | 17 % | 64 % | -2.8 / 870.9 / 2894.5 |
| said no keying | 0 | no characters | no characters | nothing measured |
| had not decided | 14 | 7 % | 62 % | -82.3 / 1240.0 / 2422.1 |

What each read:

```
shipped: QRR R L EL TSTU U ■ ■  EE EATAN EN EAANNDDE E A AAEA44MMRPTPX/S/4 4 G QTQNINIDK K ■ ■  ■  ■ ■ ■ ■ ■■ ■E I ■E AE ANN E EATANTNQQNNITIKK  ■  ■ ■ ■  ■E ■E ■ ■ ■ ■ ■  ■ ■ ■ EAEANAN E
grid:    K I S QR L TU # #E AN E AN D E AA4MP /4 QNI K # #### # # # E AN E ANQNI K # ## # # E # # # E RN E
```

### `cw-2026-08-20-014854`

30.0 s at 48000 Hz, read at 600 Hz.

**An independent sweep says this holds no keying at all.** The right emission is none.

| | shipped | grid |
|---|---|---|
| characters emitted | 0 | 0 |
| E-share | no characters | no characters |
| single-character words | no characters | no characters |
| words per minute read | 34 | 34.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 0 | no characters | no characters | nothing measured |
| said no keying | 0 | no characters | no characters | nothing measured |
| had not decided | 0 | no characters | no characters | nothing measured |

What each read:

```
shipped: (nothing)
grid:    (nothing)
```

### `cw-2026-08-20-014935`

30.0 s at 48000 Hz, read at 600 Hz.

**An independent sweep says this holds no keying at all.** The right emission is none.

| | shipped | grid |
|---|---|---|
| characters emitted | 0 | 0 |
| E-share | no characters | no characters |
| single-character words | no characters | no characters |
| words per minute read | 40 | 38.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 0 | no characters | no characters | nothing measured |
| said no keying | 0 | no characters | no characters | nothing measured |
| had not decided | 0 | no characters | no characters | nothing measured |

What each read:

```
shipped: (nothing)
grid:    (nothing)
```

### `cw-2026-08-22-014113`

30.0 s at 48000 Hz, read at 600 Hz.

| | shipped | grid |
|---|---|---|
| characters emitted | 0 | 0 |
| E-share | no characters | no characters |
| single-character words | no characters | no characters |
| words per minute read | 20 | 24.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 0 | no characters | no characters | nothing measured |
| said no keying | 0 | no characters | no characters | nothing measured |
| had not decided | 0 | no characters | no characters | nothing measured |

What each read:

```
shipped: (nothing)
grid:    (nothing)
```

### `cw-2026-08-22-014308`

30.0 s at 48000 Hz, read at 600 Hz.

| | shipped | grid |
|---|---|---|
| characters emitted | 0 | 0 |
| E-share | no characters | no characters |
| single-character words | no characters | no characters |
| words per minute read | 36 | 24.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 0 | no characters | no characters | nothing measured |
| said no keying | 0 | no characters | no characters | nothing measured |
| had not decided | 0 | no characters | no characters | nothing measured |

What each read:

```
shipped: (nothing)
grid:    (nothing)
```

### `cw-2026-08-22-031838`

30.0 s at 48000 Hz, read at 600 Hz.

| | shipped | grid |
|---|---|---|
| characters emitted | 125 | 71 |
| E-share | 7 % | 52 % |
| single-character words | 32 % | 62 % |
| words per minute read | 20 | 30.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 97 | 9 % | 31 % | 7.7 / 99.1 / 379.4 |
| said no keying | 15 | 0 % | 25 % | 41.2 / 181.6 / 560.0 |
| had not decided | 13 | 0 % | 50 % | 100.1 / 726.7 / 14448.9 |

What each read:

```
shipped: 33T3Z,, I ■ATM TM TTM,OT E ETMTMT2T■T■■■■■ ■ ■ 22N27MTAT, I ATANTNDD H ■■ E■ WTM TET TEAE■T TTTTH EI■A A TT MEIEATANN N OTT TTTF F U22R■TT■ ■■■■  ■■ ■ ■■■ ■ ■■
grid:     I E # # EE E S E E <HH> #I # 8EE E E EEE #E E E S # <AS> T E W # # SEE E <HH> 5 NI # R # E 6E # AE E EE N E IE EIE E EE #E EN EES 
```

### `cw-2026-08-22-031905`

30.0 s at 48000 Hz, read at 600 Hz.

| | shipped | grid |
|---|---|---|
| characters emitted | 106 | 78 |
| E-share | 12 % | 45 % |
| single-character words | 63 % | 64 % |
| words per minute read | 22 | 30.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 74 | 9 % | 65 % | 12.6 / 628.0 / 9029.6 |
| said no keying | 7 | 0 % | 50 % | 531.3 / 1405.7 / 2437.2 |
| had not decided | 25 | 24 % | 0 % | 97.7 / 534.5 / 1532.0 |

What each read:

```
shipped: <AS><AS>S PEPR■RENEDIDINICCTETENED D R ■1T1O■0R■.T.Z7 7  ■  ■  ■AIEEEI E EAETAIINI I ■  WTW■■ TM U TAXX E IIIS S W 11S1■2I255M■, ,  ■ ■  ■ T ■T T T
grid:    N E IE EIE E EE #E EN EES E T #I ES F E E T I E SEEI S S E E# EENES E S I TI I E EI E EE A #ES S V # IIE E HEE R # # #E I # T A 
```

### `cw-2026-08-22-031948`

30.0 s at 48000 Hz, read at 600 Hz.

| | shipped | grid |
|---|---|---|
| characters emitted | 104 | 70 |
| E-share | 12 % | 40 % |
| single-character words | 37 % | 76 % |
| words per minute read | 18 | 28.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 87 | 14 % | 41 % | 88.9 / 579.9 / 1313.6 |
| said no keying | 5 | 0 % | 0 % | 121.4 / 593.7 / 694.9 |
| had not decided | 12 | 8 % | 0 % | 195.2 / 721.6 / 11740.6 |

What each read:

```
shipped: HM ■0■T MN5E5O,0 E0 W11A1J1E1G90T0Z,, E AEANENDD A J1E1W11T1■00 I WWIEITITHH A A E MMEAEANAN EN GOIOFF I P11R■1T1Z7I7<AR>.E.W
grid:     # # # E T E E E E T # 5 # E E E E NE E E EUE # 5 S I L # TII ES # GE I NIES TSE ED F S # E IE # E #II EE I S E EET # T E
```

### `cw-2026-08-22-032012`

30.0 s at 48000 Hz, read at 600 Hz.

| | shipped | grid |
|---|---|---|
| characters emitted | 120 | 86 |
| E-share | 18 % | 36 % |
| single-character words | 58 % | 69 % |
| words per minute read | 18 | 38.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 106 | 19 % | 61 % | 205.9 / 667.5 / 1443.6 |
| said no keying | 0 | no characters | no characters | nothing measured |
| had not decided | 14 | 14 % | 0 % | 399.5 / 1346.4 / 7633.3 |

What each read:

```
shipped: TETT TF O11AJ1E1G7E7■..A■1I1■..  R LILININNKEKS S T TMO O A ARRTR T I E I KC IC L L E I E SSMO EO R RNO OTETHHEIER R A WEWENBEBSESITITEEES S M MEEMENENTI
grid:     H EIF # EEE # E # IE E I I S E EETE # T E # HE # I4T E I E E E E # I 5T S E EJ SE E A EI # S #HEE # # A RS # E T UEE # #TS #S EE I HS # SR E I N 
```

### `cw-2026-08-22-032050`

30.0 s at 48000 Hz, read at 600 Hz.

| | shipped | grid |
|---|---|---|
| characters emitted | 119 | 74 |
| E-share | 16 % | 34 % |
| single-character words | 71 % | 71 % |
| words per minute read | 28 | 28.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 79 | 15 % | 73 % | 86.5 / 475.4 / 5211.2 |
| said no keying | 23 | 22 % | 0 % | 145.2 / 680.6 / 1447.2 |
| had not decided | 17 | 12 % | 0 % | 386.1 / 1456.0 / 5540.7 |

What each read:

```
shipped: MOURLILLLETETIINN N CCAANN T BBE E U FTFOOUUNUNNM TTT D IE INN T TEAELELEAEWWRRIRITTTETERRNR7,,  R PEPATAKCTCKK E E  ■ ■ ■ ■ ■ B E  I I E I ■■ T E E S H I  SI I
grid:    EEESEIE# I I EE E II S I EHE 5 I 5 E E EEIE I H I E E HE#I E N II IN #E I S N TS IE # I# #I I E I II I # H E E # I I I E I
```

### `cw-2026-08-22-032113`

30.0 s at 48000 Hz, read at 600 Hz.

| | shipped | grid |
|---|---|---|
| characters emitted | 118 | 77 |
| E-share | 25 % | 19 % |
| single-character words | 66 % | 86 % |
| words per minute read | 32 | 38.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 102 | 27 % | 66 % | 21.5 / 707.8 / 2053.2 |
| said no keying | 0 | no characters | no characters | nothing measured |
| had not decided | 16 | 12 % | 50 % | 275.5 / 1042.6 / 6094.0 |

What each read:

```
shipped: KET E T TTK■■ A ANINNN D D IT INTNTEETERRNNETET E ■■ E I E RTRSESITI OONNSS E GOIOFF S ■2T2O00APJNJ66  ■ ■  ■ ■E E E N  ■SE I E  T E E E I E E  ■  ■ ■ ■EEEEE EI ES E■ T I
grid:    E IE I E S #AI S # # <AS> N T E # I I I I EI S A # #IS I I T # N E I I# E S # # # E N E I # NEE # N # A TS # E H # N # S T # E I I I E H #E I # H
```

### `cw-2026-08-22-032129`

30.0 s at 48000 Hz, read at 600 Hz.

| | shipped | grid |
|---|---|---|
| characters emitted | 107 | 58 |
| E-share | 41 % | 40 % |
| single-character words | 63 % | 63 % |
| words per minute read | 40 | 22.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 90 | 47 % | 71 % | 78.3 / 498.7 / 1227.9 |
| said no keying | 0 | no characters | no characters | nothing measured |
| had not decided | 17 | 12 % | 0 % | 51.7 / 205.6 / 3179.3 |

What each read:

```
shipped: 5■ ■MOM EA EAMMM TT66 E WPIPRERGO IO JPAANGEGATATITIMOEONN  ■ E E  E E  IIEEESS EE I ■ E E E S E S E E E E E E  E EEEE EEEE E I E S SI SE HE H IEE I E E EEI
grid:     EI B #EE E TI T ID E I I I E E E E IE S VE I H ITE E I E H I IE IE INI S 5 E TE E SE EEHE I 5S5
```

### `cw-2026-08-23-001520`

30.0 s at 8000 Hz, read at 600 Hz.

| | shipped | grid |
|---|---|---|
| characters emitted | 3 | 0 |
| E-share | 0 % | no characters |
| single-character words | 0 % | no characters |
| words per minute read | 8 | 8.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 3 | 0 % | 0 % | 0.0 / 0.0 / 0.0 |
| said no keying | 0 | no characters | no characters | nothing measured |
| had not decided | 0 | no characters | no characters | nothing measured |

What each read:

```
shipped: ■■■
grid:    (nothing)
```

### `cw-2026-08-23-001831`

30.0 s at 48000 Hz, read at 600 Hz.

| | shipped | grid |
|---|---|---|
| characters emitted | 127 | 0 |
| E-share | 9 % | no characters |
| single-character words | 60 % | no characters |
| words per minute read | 27 | 40.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 47 | 9 % | 50 % | 4.4 / 125.8 / 363.3 |
| said no keying | 27 | 7 % | 0 % | 67.1 / 211.6 / 528.0 |
| had not decided | 53 | 11 % | 69 % | -23.9 / 65.0 / 526.1 |

What each read:

```
shipped: ■ K K ■ ■  ■ T TATA INEFYTTT ■ ■T TTTT A ■ ■ ■ ■ T Q■ G Q Q ■ T ■ NN NN EEE SMGEGEGQ  KIK5T5QQZQ IQ 5T5NNNNDDERELALA  R RARR ER ■ES■ESNSNNNN T TAETAGO O ■ ■ T TUUKKVV■■T■■Q
grid:    (nothing)
```

### `cw-2026-08-23-001952`

30.0 s at 48000 Hz, read at 600 Hz.

| | shipped | grid |
|---|---|---|
| characters emitted | 133 | 105 |
| E-share | 26 % | 38 % |
| single-character words | 58 % | 62 % |
| words per minute read | 32 | 38.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 39 | 15 % | 61 % | -28.8 / 258.8 / 1307.7 |
| said no keying | 55 | 40 % | 59 % | 1.2 / 15.5 / 315.5 |
| had not decided | 39 | 18 % | 53 % | 17.6 / 784.2 / 3881.8 |

What each read:

```
shipped: ■I ■ AS A I EEI ■E E  E  V ■S3FNFN S 5I5ININ A WEWEFFUU ■ ■ ■E ■ 5EENE MTNNNNOEON M ■T E ■T A A KKG9 TOI T T ■ ■  ■ E E  IE ■ ■■ E H ER EE  EE I  I ■E■ ■E ■ I E  II EE■E EE■E S EE E  ■ ■■ E M MN N S  BO00  ■  ■
grid:     ETBEE 5 # # #T # # T E E EE EE#E E H EE E TEEE EE 5 # #EL S I E I EE # E#I E H I #EI NNE E # # ## ## EE E## E #5 E # # #T#EE #T # E E## A # I# EAEDT # I T # <SK>E55I I
```

### `cw-2026-08-23-002016`

30.0 s at 48000 Hz, read at 600 Hz.

| | shipped | grid |
|---|---|---|
| characters emitted | 104 | 0 |
| E-share | 23 % | no characters |
| single-character words | 60 % | no characters |
| words per minute read | 25 | 40.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 4 | 0 % | 50 % | -66.7 / 758.4 / 921.0 |
| said no keying | 66 | 23 % | 50 % | -13.7 / 203.1 / 811.2 |
| had not decided | 34 | 26 % | 70 % | -20.2 / 18.1 / 1902.5 |

What each read:

```
shipped: B BG 0■  ■ ■  ■ ■ ■■■■E E E E ■ E ■  E EE■S■■E I■ ■ T VEKBTNA■■T I■T S IEU IT EET EEZZ ■  ■ ■ ■ ■  ■ ■  ■ O ■ DK B ME ■T■T TEE NH H V VNENN EN JJEEJEENTNGG  ■
grid:    (nothing)
```

### `cw-2026-08-24-012403`

30.0 s at 48000 Hz, read at 600 Hz.

| | shipped | grid |
|---|---|---|
| characters emitted | 19 | 0 |
| E-share | 5 % | no characters |
| single-character words | 50 % | no characters |
| words per minute read | 21 | 40.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 0 | no characters | no characters | nothing measured |
| said no keying | 19 | 5 % | 50 % | 104.2 / 235.5 / 458.3 |
| had not decided | 0 | no characters | no characters | nothing measured |

What each read:

```
shipped: N KKNKDTDO00UUNUN E KK
grid:    (nothing)
```

## The sensitivity sweep

`CQ DE W1AW K` at 18 words a minute, 640 Hz, averaged over 4 noise draws at each level.

**`invented` here counts characters aligned against nothing sent**,
which is `CwMatchKind.Invented`. It is not the column the existing
sweep prints under that name: `CwRefusalFloorTableTests` counts
`CwMatchKind.Wrong`, a substitution at a position where something was
sent, so a transcript full of characters that were never on the air
scores nought there. Both are printed below so the difference can be
seen rather than argued about.

Counts rather than shares, averaged over the seeds. A share needs a
denominator, and correct and invented do not have the same one: a
character that was sent and missed is not the same event as a
character that was emitted and never sent, so putting both over the
message length produces a table whose rows add to more than
everything. The message holds 9 characters.

| generated | correct | wrong | invented | emitted | invented share of what was read | read |
|---|---|---|---|---|---|---|
| 18 dB | 8.0 | 0.0 | 12.0 | 20.0 | 60 % | `Q N DEDE E WWAJ11AARW W N K` |
| 11 dB | 8.0 | 0.0 | 12.0 | 20.0 | 60 % | `Q N DEDE E WWAJ11AARW W N K` |
| 3 dB | 7.5 | 0.3 | 11.8 | 19.5 | 60 % | `Q N DEDE E WWAJ11AARW W N K` |

## The streaming gate, read by read

`Gate = 15` was set from a 3-to-6 against 24-to-39 separation the
offline reference measured on whole files. **The instrument that
actually gates is the streaming windower, and it has never been
measured.** These are its own per-read likelihood ratios, taken from
`CwProbabilisticStream.Last` after every read, split by whether
somebody was keying at that read's own moment.

**The split is by the same independent witness the corpus table
uses**, asked at the moment of each read rather than once per file,
because the question the gate has to answer is whether *this window*
holds keying. A whole-file split would compare recordings, and the
gate never gets to see a whole file.

| recording | witness | reads | ratio P10 / median / P90 |
|---|---|---|---|
| `cw-2026-08-17-013347` | | | |
| | said keying | 11 | 1039.2 / 13552759.6 / 18639627.9 |
| | said no keying | 24 | 23381935.9 / 29846067.0 / 37206257.2 |
| | had not decided | 21 | 7872321.2 / 8888366.1 / 16242796.5 |
| `cw-2026-08-17-013622` | | | |
| | said keying | 0 | nothing measured |
| | said no keying | 35 | 0.1 / 2139937.6 / 8618902.8 |
| | had not decided | 21 | 5590881.7 / 8702389.3 / 10206382.0 |
| `cw-2026-08-17-134712` | | | |
| | said keying | 18 | 1.2 / 3.3 / 3.8 |
| | said no keying | 17 | 0.1 / 0.1 / 0.6 |
| | had not decided | 21 | 0.0 / 0.1 / 0.1 |
| `cw-2026-08-18-004507` | | | |
| | said keying | 49 | 6.2 / 7.4 / 8.7 |
| | said no keying | 0 | nothing measured |
| | had not decided | 7 | 7.7 / 8.4 / 9.0 |
| `cw-2026-08-18-003016` | | | |
| | said keying | 49 | 3.7 / 4.5 / 5.2 |
| | said no keying | 0 | nothing measured |
| | had not decided | 7 | 3.2 / 3.4 / 3.9 |
| `cw-2026-08-18-003126` | | | |
| | said keying | 49 | 4.6 / 5.3 / 6.9 |
| | said no keying | 0 | nothing measured |
| | had not decided | 7 | 5.3 / 6.0 / 6.1 |
| `cw-2026-08-18-003758` | | | |
| | said keying | 49 | 5.9 / 13.0 / 15.1 |
| | said no keying | 0 | nothing measured |
| | had not decided | 7 | 12.8 / 14.7 / 16.1 |
| `cw-2026-08-20-014854` (an independent sweep says this holds no keying at all) | | | |
| | said keying | 0 | nothing measured |
| | said no keying | 35 | 0.5 / 0.7 / 0.9 |
| | had not decided | 21 | 0.1 / 0.2 / 0.4 |
| `cw-2026-08-20-014935` (an independent sweep says this holds no keying at all) | | | |
| | said keying | 0 | nothing measured |
| | said no keying | 35 | 0.1 / 0.2 / 0.2 |
| | had not decided | 21 | 0.2 / 0.2 / 0.3 |
| `cw-2026-08-22-014113` | | | |
| | said keying | 0 | nothing measured |
| | said no keying | 35 | 0.2 / 0.5 / 0.8 |
| | had not decided | 21 | 0.1 / 0.2 / 0.3 |
| `cw-2026-08-22-014308` | | | |
| | said keying | 0 | nothing measured |
| | said no keying | 35 | 0.3 / 0.4 / 0.5 |
| | had not decided | 21 | 0.3 / 0.4 / 0.5 |
| `cw-2026-08-22-031838` | | | |
| | said keying | 42 | 4.2 / 5.6 / 7.3 |
| | said no keying | 7 | 6.5 / 6.6 / 6.8 |
| | had not decided | 7 | 4.4 / 4.8 / 5.5 |
| `cw-2026-08-22-031905` | | | |
| | said keying | 42 | 3.6 / 5.1 / 5.9 |
| | said no keying | 4 | 2.9 / 3.1 / 3.2 |
| | had not decided | 10 | 1.6 / 2.8 / 3.9 |
| `cw-2026-08-22-031948` | | | |
| | said keying | 46 | 4.8 / 8.1 / 9.3 |
| | said no keying | 3 | 5.0 / 5.0 / 5.3 |
| | had not decided | 7 | 8.4 / 9.3 / 10.1 |
| `cw-2026-08-22-032012` | | | |
| | said keying | 49 | 4.8 / 5.5 / 7.1 |
| | said no keying | 0 | nothing measured |
| | had not decided | 7 | 4.7 / 5.6 / 6.0 |
| `cw-2026-08-22-032050` | | | |
| | said keying | 40 | 3.3 / 4.3 / 6.5 |
| | said no keying | 9 | 3.8 / 3.9 / 4.2 |
| | had not decided | 7 | 1.6 / 1.8 / 2.2 |
| `cw-2026-08-22-032113` | | | |
| | said keying | 49 | 6.7 / 7.5 / 8.3 |
| | said no keying | 0 | nothing measured |
| | had not decided | 7 | 4.9 / 5.4 / 6.2 |
| `cw-2026-08-22-032129` | | | |
| | said keying | 49 | 5.9 / 7.5 / 9.1 |
| | said no keying | 0 | nothing measured |
| | had not decided | 7 | 5.1 / 5.4 / 5.8 |
| `cw-2026-08-23-001520` | | | |
| | said keying | 6 | 0.0 / 0.0 / 11402.5 |
| | said no keying | 0 | nothing measured |
| | had not decided | 1 | 0.0 / 0.0 / 0.0 |
| `cw-2026-08-23-001831` | | | |
| | said keying | 19 | 0.6 / 0.7 / 0.7 |
| | said no keying | 16 | 0.5 / 0.5 / 0.6 |
| | had not decided | 21 | 1.0 / 1.4 / 2.0 |
| `cw-2026-08-23-001952` | | | |
| | said keying | 15 | 1.2 / 1.8 / 6.3 |
| | said no keying | 26 | 0.3 / 0.9 / 1.2 |
| | had not decided | 15 | 6.7 / 9.4 / 24.5 |
| `cw-2026-08-23-002016` | | | |
| | said keying | 4 | 0.7 / 0.7 / 0.8 |
| | said no keying | 31 | 0.6 / 0.7 / 1.0 |
| | had not decided | 21 | 0.2 / 0.7 / 1.1 |
| `cw-2026-08-24-012403` | | | |
| | said keying | 0 | nothing measured |
| | said no keying | 35 | 0.5 / 0.8 / 1.6 |
| | had not decided | 21 | 0.1 / 0.2 / 0.2 |

**A read repeats most of its window twice a second**, so these are not
independent samples and a median describes the recording rather than a
decision. What the next unit needs from them is whether the two groups
separate at all on the instrument that actually gates.

**And the ratio's scale is the same one the span LLR's is**: it rests
on the window's own noise estimate, so a window holding nothing can
score higher than a window holding a station, because the estimate
collapses when there is nothing to estimate from. A gate derived from
these numbers without that being fixed first would be a gate on how
quiet the band was.

