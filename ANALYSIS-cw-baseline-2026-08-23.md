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
| characters emitted | 129 | 81 |
| E-share | 45 % | 63 % |
| single-character words | 62 % | 55 % |
| words per minute read | 16 | 24.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 15 | 13 % | 75 % | -29.1 / 335.5 / 769.6 |
| said no keying | 50 | 36 % | 50 % | 256.7 / 982820776.1 / 8744467815.7 |
| had not decided | 64 | 59 % | 65 % | 155001981.3 / 717495878.4 / 5913441603.5 |

What each read:

```
shipped: HIAAEEEISIHEHEEEIEEA EA E E EEEEE S HEEHEEIIEEEE II NE IEEE E T E T ET E E I I E E E  E I IEEEEII  TE TEEEEI TI T T E E E HEHAA EE ERWEWHVEVRRAR R S VVAAS■3E3HVEVRRAR R  ■  ■
grid:     E EI EE 5EEETEEE V EEEA E E EE IEEEE I EE IEET E T E EE E E EETEM E TEEEE T E IEEET E O EEETETTW EEMAEAMJOW # # 
```

### `cw-2026-08-17-013622`

30.0 s at 48000 Hz, read at 600 Hz.

| | shipped | grid |
|---|---|---|
| characters emitted | 105 | 99 |
| E-share | 60 % | 63 % |
| single-character words | 41 % | 56 % |
| words per minute read | 30 | 32.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 0 | no characters | no characters | nothing measured |
| said no keying | 48 | 67 % | 45 % | 7.2 / 21.5 / 155.9 |
| had not decided | 57 | 54 % | 35 % | 51122070.2 / 535667334.7 / 2590878389.2 |

What each read:

```
shipped: HE EE IIII I 55EIEIEIEIEE EE EEETETE E TE TE E ESS  ■E E E HIE U U EU EEEEETST■U T TEETEEE E  A A EE EE  EE EN T EEE EEEE E I IEEI  EE E E  E EI EETET
grid:    E EE # ESEF E H EE SEEE E S E EE E EE E ER E T E E EEE E EE E E EEN IA E IETIEIT AET E # T EEE IT IEEE IHEE #EE # EEEET # TE #EEE # SEE # SE E # E # E
```

### `cw-2026-08-17-134712`

30.0 s at 48000 Hz, read at 600 Hz.

**Adjudicated reading: `N4L (HM-DEC-144)`.** Quoted from the ruling rather than from any decoder.

| | shipped | grid |
|---|---|---|
| characters emitted | 41 | 0 |
| E-share | 37 % | no characters |
| single-character words | 73 % | no characters |
| words per minute read | 25 | 28.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 35 | 43 % | 69 % | -68.2 / 9.2 / 1497.9 |
| said no keying | 6 | 0 % | 100 % | -188.8 / -135.4 / 24753.0 |
| had not decided | 0 | no characters | no characters | nothing measured |

What each read:

```
shipped: K K K  ■ ■  ■T NSN4E4LLMLQQ  E ■KK  ■  ■ ■ ■ HEEE EE E E E E EE E E
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
| E-share | 24 % | 27 % |
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
| said keying | 108 | 26 % | 64 % | -2.8 / 870.9 / 2894.5 |
| said no keying | 0 | no characters | no characters | nothing measured |
| had not decided | 14 | 7 % | 62 % | -82.3 / 1240.0 / 2422.1 |

What each read:

```
shipped: QRR R L EL TSTU U ■ ■  EE EATAN EN EAANNDDE E A AAEA44MMRPTPX/S/4 4 G QTQNINIDK K ■ ■  ■  ■ ■ E E EE EE I ■E AE ANN E EATANTNQQNNITIKK  ■  ■ ■ ■  EE EE H H I E E  E ■ ■ EAEANAN E
grid:    K I S QR L TU # #E AN E AN D E AA4MP /4 QNI K # #EEE E E # E AN E ANQNI K # #E E E E H I # E RN E
```

### `cw-2026-08-20-014854`

30.0 s at 48000 Hz, read at 600 Hz.

**An independent sweep says this holds no keying at all.** The right emission is none.

| | shipped | grid |
|---|---|---|
| characters emitted | 0 | 0 |
| E-share | no characters | no characters |
| single-character words | no characters | no characters |
| words per minute read | 32 | 28.0 |

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
| words per minute read | 32 | 32.0 |

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
| words per minute read | 32 | 24.0 |

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

### `cw-2026-08-22-031905`

30.0 s at 48000 Hz, read at 600 Hz.

| | shipped | grid |
|---|---|---|
| characters emitted | 103 | 78 |
| E-share | 14 % | 45 % |
| single-character words | 67 % | 64 % |
| words per minute read | 22 | 30.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 71 | 10 % | 69 % | 12.6 / 600.4 / 8969.7 |
| said no keying | 7 | 0 % | 50 % | 531.3 / 1405.7 / 2437.2 |
| had not decided | 25 | 28 % | 0 % | 97.7 / 534.5 / 1532.0 |

What each read:

```
shipped: <AS><AS>S PEPRERENEDIDINICCTETENED D R ■1T1O00R■.T.Z7 7  ■  ■  ■AIEEEI E EAETAIINI I ■  S ■LA U TAXX E IIIS S W 11S1■2I255M■, ,  ■ ■  ■ T ■T T T
grid:    N E IE EIE E EE #E EN EES E T #I ES F E E T I E SEEI S S E E# EENES E S I TI I E EI E EE A #ES S V # IIE E HEE R # # #E I # T A 
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

### `cw-2026-08-24-012403`

30.0 s at 48000 Hz, read at 600 Hz.

| | shipped | grid |
|---|---|---|
| characters emitted | 19 | 0 |
| E-share | 5 % | no characters |
| single-character words | 50 % | no characters |
| words per minute read | 21 | 32.0 |

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
| | said keying | 11 | 1038.2 / 13552759.6 / 18639627.9 |
| | said no keying | 24 | 23381935.9 / 29846067.0 / 37206257.2 |
| | had not decided | 21 | 7872321.2 / 8888366.1 / 16242796.5 |
| `cw-2026-08-17-013622` | | | |
| | said keying | 0 | nothing measured |
| | said no keying | 35 | 0.1 / 2139937.6 / 8618902.8 |
| | had not decided | 21 | 5590881.7 / 8702389.3 / 10206382.0 |
| `cw-2026-08-17-134712` | | | |
| | said keying | 18 | 1.2 / 3.1 / 3.6 |
| | said no keying | 17 | 0.0 / 0.1 / 0.7 |
| | had not decided | 21 | 0.0 / 0.0 / 0.0 |
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
| | had not decided | 21 | 0.1 / 0.2 / 0.2 |
| `cw-2026-08-22-014308` | | | |
| | said keying | 0 | nothing measured |
| | said no keying | 35 | 0.3 / 0.4 / 0.5 |
| | had not decided | 21 | 0.3 / 0.4 / 0.5 |
| `cw-2026-08-22-031905` | | | |
| | said keying | 42 | 3.6 / 5.1 / 5.9 |
| | said no keying | 4 | 2.9 / 3.2 / 3.3 |
| | had not decided | 10 | 1.6 / 2.8 / 4.0 |
| `cw-2026-08-23-001520` | | | |
| | said keying | 6 | 0.0 / 0.0 / 11402.5 |
| | said no keying | 0 | nothing measured |
| | had not decided | 1 | 0.0 / 0.0 / 0.0 |
| `cw-2026-08-24-012403` | | | |
| | said keying | 0 | nothing measured |
| | said no keying | 35 | 0.5 / 0.8 / 1.6 |
| | had not decided | 21 | 0.1 / 0.2 / 0.3 |

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

