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
| characters emitted | 128 | 83 |
| E-share | 38 % | 45 % |
| single-character words | 61 % | 63 % |
| words per minute read | 14 | 28.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 16 | 25 % | 80 % | -66.0 / 10189.0 / 108875.7 |
| said no keying | 52 | 35 % | 50 % | 845981027.2 / 11022493083.9 / 20715631054.4 |
| had not decided | 60 | 43 % | 60 % | 582865711.1 / 1974217838.1 / 16935603866.1 |

What each read:

```
shipped: HIAAEEEISIHEHEIIIA EA E E EII I IIEIEIEIEEEE II IRERFI E T E T ET E E I I E EE E S SEEEEII  TE TEEEEI TI T T E I E HEHETA EE ERWEWHVIVRRAR R S VEVAAS■3E3HVIVRRAR R  E  E E
grid:     E E EE E E E E E E E E EE UE ET E T E EE T E TET ETT E TE ETE T E EEEEET E TTT TTTTTTTTTT ATTNTTTTTTTTTTTTTTTTT E E E 
```

### `cw-2026-08-17-013622`

30.0 s at 48000 Hz, read at 600 Hz.

| | shipped | grid |
|---|---|---|
| characters emitted | 97 | 99 |
| E-share | 43 % | 48 % |
| single-character words | 35 % | 54 % |
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
| said no keying | 38 | 37 % | 0 % | 10180.5 / 52865561.3 / 7220807747.8 |
| had not decided | 59 | 47 % | 45 % | 195111273.3 / 2017836885.4 / 11242243833.3 |

What each read:

```
shipped: HE EE IIII I 55EIEIEIEIEEEII EEUUE E TE TE I ISSEIE E E E E 5IE U U EU ESEEETAT■■UAHA■E■EEEEENN EE ■■ ET ETEETEISIH EH ■■I
grid:    E E IIE E E EE EE E EEEEE I E E I E E E# E T E E EEE E E EEI IU E IIT# EET#R E I INE T5EI IT I5IV #HE IIHEIEIS#<HH>5K5E#E5#EIE#EEEHE ESSHI
```

### `cw-2026-08-17-134712`

30.0 s at 48000 Hz, read at 600 Hz.

**Adjudicated reading: `N4L (HM-DEC-144)`.** Quoted from the ruling rather than from any decoder.

| | shipped | grid |
|---|---|---|
| characters emitted | 18 | 0 |
| E-share | 72 % | no characters |
| single-character words | 79 % | no characters |
| words per minute read | 25 | 32.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 18 | 72 % | 79 % | -323.3 / -269.9 / 9034.7 |
| said no keying | 0 | no characters | no characters | nothing measured |
| had not decided | 0 | no characters | no characters | nothing measured |

What each read:

```
shipped: QQ  ET EKK  E  E E E  E  E E E  E  E E
grid:    (nothing)
```

### `cw-2026-08-18-004507`

30.0 s at 48000 Hz, read at 501 Hz.

| | shipped | grid |
|---|---|---|
| characters emitted | 115 | 48 |
| E-share | 16 % | 12 % |
| single-character words | 71 % | 14 % |
| words per minute read | 24 | 18.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 103 | 16 % | 68 % | 830.1 / 3094.9 / 6243.2 |
| said no keying | 0 | no characters | no characters | nothing measured |
| had not decided | 12 | 17 % | 88 % | 484.6 / 2095.4 / 2519.1 |

What each read:

```
shipped: A A T  T E AEARR R RRL L D D M O O T T N N E  E T  T N <BT><BT>  EE EE A ANAC C H H E STSTATA TA TI I GOTON N I HEHATANTNDDLLIINNGG  ET ETHHIE I S S T MEMESSSSSASANAGG E  E A PE
grid:    E JJ AT ARRL DOT NET <BT> EACH STATION HANDLING THIS MESSAG E PE
```

### `cw-2026-08-18-003016`

30.0 s at 48000 Hz, read at 669 Hz.

| | shipped | grid |
|---|---|---|
| characters emitted | 134 | 54 |
| E-share | 13 % | 7 % |
| single-character words | 45 % | 0 % |
| words per minute read | 25 | 22.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 117 | 15 % | 44 % | 526.2 / 1759.6 / 4690.3 |
| said no keying | 0 | no characters | no characters | nothing measured |
| had not decided | 17 | 0 % | 33 % | 960.2 / 2118.3 / 4567.0 |

What each read:

```
shipped: ADAA D KRKPIPAAA11H15TE5TT IT ITIT W WAEAS ES J■■NTNK K  <AS> ■ ■ E E S STITIRLRLL IL HIHVEVE M MKY Y E T E TMO EO O9E9J1T1BB  ETT ETT T W JEEJETSTST T V VFFBB TB TUUBBEAELILIN
grid:    I<BT> HADA KPA15TT ITWAS #K <BT> ESTILL HVEMY ETO 91B TT JUST VFB TUBELIN
```

### `cw-2026-08-18-003126`

30.0 s at 48000 Hz, read at 675 Hz.

| | shipped | grid |
|---|---|---|
| characters emitted | 124 | 54 |
| E-share | 15 % | 15 % |
| single-character words | 51 % | 42 % |
| words per minute read | 27 | 28.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 112 | 14 % | 46 % | 697.6 / 2374.9 / 6097.4 |
| said no keying | 0 | no characters | no characters | nothing measured |
| had not decided | 12 | 17 % | 67 % | -209.8 / 3384.7 / 6264.3 |

What each read:

```
shipped: <BT>  E  E N <BT><BT> I I R WAWATTTCECHH AE AT AT L EL ER<AS>T<AS>T S 22 T MTMOOVVI I ESESAA D DAEDACY AY WIWIDD D XTXY■■ A WIWHNHYY N NGOTOT  E  E E E M ■, , W WESESTESTERRNINS TS ■, E
grid:    A OM <BT> E <BT> I WATCH AT L EAST 2 MOVI ES A DAY WID X# WHY NOT E E , WESTERNS# E
```

### `cw-2026-08-18-003758`

30.0 s at 48000 Hz, read at 501 Hz.

| | shipped | grid |
|---|---|---|
| characters emitted | 106 | 45 |
| E-share | 28 % | 29 % |
| single-character words | 62 % | 43 % |
| words per minute read | 30 | 16.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 92 | 29 % | 62 % | -331.9 / 5292.8 / 10901.6 |
| said no keying | 0 | no characters | no characters | nothing measured |
| had not decided | 14 | 21 % | 62 % | -476.6 / 7037.1 / 12952.1 |

What each read:

```
shipped: QRR R L EL TSTU U E E  EE EANAN EN EAANNDDE E A AAIA44MMRPTPX/S/4 4 G QTQNINIDK K E E  E  E E E  EE EE A ANN E EATANTNQQNNITIKK   E  E E   E   E EA E RNRN E
grid:    KIS QRLTU EEAN EANDE AA4MP/4QNIK E E EAN EP#IK E E E EIN E
```

### `cw-2026-08-20-014854`

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
| 3 dB | 7.5 | 0.3 | 12.8 | 20.5 | 62 % | `Q N DEDE E WWAJ11AARW W N K` |

## The streaming gate, read by read

`Gate = 15` was set from a 3-to-6 against 24-to-39 separation the
offline reference measured on whole files. **The instrument that
actually gates is the streaming windower, and it has never been
measured.** These are its own per-read likelihood ratios, taken from
`CwProbabilisticStream.Last` after every read, split by whether the
recording holds a station at all.

| recording | station | reads | ratio P10 / median / P90 |
|---|---|---|---|
| `cw-2026-08-17-013347` | yes | 56 | 34.1 / 27939825.2 / 97602462.3 |
| `cw-2026-08-17-013622` | yes | 56 | 3.2 / 5358791.0 / 29896464.5 |
| `cw-2026-08-17-134712` | yes | 56 | 1.7 / 2.2 / 2.3 |
| `cw-2026-08-18-004507` | yes | 56 | 29.3 / 34.9 / 41.6 |
| `cw-2026-08-18-003016` | yes | 56 | 20.3 / 25.3 / 28.9 |
| `cw-2026-08-18-003126` | yes | 56 | 23.8 / 28.0 / 35.2 |
| `cw-2026-08-18-003758` | yes | 56 | 21.3 / 50.8 / 63.8 |
| `cw-2026-08-20-014854` | none | 56 | 2.8 / 6.0 / 7.2 |
| `cw-2026-08-20-014935` | none | 56 | 2.7 / 3.2 / 3.8 |

**A read repeats most of its window twice a second**, so these are not
independent samples and a median describes the recording rather than a
decision. What the next unit needs from them is whether the two groups
separate at all on the instrument that actually gates.

