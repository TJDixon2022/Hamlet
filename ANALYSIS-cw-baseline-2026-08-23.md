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
| characters emitted | 130 | 83 |
| E-share | 42 % | 25 % |
| single-character words | 63 % | 67 % |
| words per minute read | 8 | 28.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 15 | 7 % | 75 % | -81.2 / 10754.6 / 102656.2 |
| said no keying | 50 | 34 % | 50 % | 1927827233.3 / 32512988061.6 / 131131982513.3 |
| had not decided | 65 | 57 % | 68 % | 772695313.1 / 2759309529.6 / 27505920432.8 |

What each read:

```
shipped: DIAAEEEISIHEHEEEIEEA EA E E EEEEE I IEEEEEEIIEEEE II IRERUI E T E T ET E E I I E E E  E I IEEEEII  TE TEEEEI TI T T E I E HEHAA EE ERWEWHVEVRRAR R S VVAAS■3E3HVIVRRAR R  ■  ■
grid:     # # EE # # # # # # # # # # AE ET E T E EE T E TET ETT # TE ETE T E EEEEET E TTT TTTTTTTTTT TTTNTTTTTTTTTTTTTTTTT # # # 
```

### `cw-2026-08-17-013622`

30.0 s at 48000 Hz, read at 600 Hz.

| | shipped | grid |
|---|---|---|
| characters emitted | 101 | 97 |
| E-share | 47 % | 47 % |
| single-character words | 29 % | 51 % |
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
| said no keying | 42 | 43 % | 0 % | 5529.5 / 131836000.0 / 20867311951.6 |
| had not decided | 59 | 49 % | 40 % | 276171955.3 / 2830269371.2 / 17932946347.2 |

What each read:

```
shipped: HE EE IIII I 55EIEIEIEIEEE IE EEUUE E TE TE E ESSEIEI E E E 5II U U EU ESEEETST■■AAHAHEEEEEEEENN EE EHIHI ET ET ET IEEIES ES ■■I
grid:     EE EIE E E EE #E # EEE E I E E I E E E# E T E E EEE # E EET IU E IET# EET#LE I INE E5EI ITI#V #H IIREIEIS#5EHK5E#IE5#EI#EEEHE ESIHI
```

### `cw-2026-08-17-134712`

30.0 s at 48000 Hz, read at 600 Hz.

**Adjudicated reading: `N4L (HM-DEC-144)`.** Quoted from the ruling rather than from any decoder.

| | shipped | grid |
|---|---|---|
| characters emitted | 25 | 0 |
| E-share | 8 % | no characters |
| single-character words | 86 % | no characters |
| words per minute read | 25 | 32.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 25 | 8 % | 86 % | -564.1 / 436.9 / 16049.0 |
| said no keying | 0 | no characters | no characters | nothing measured |
| had not decided | 0 | no characters | no characters | nothing measured |

What each read:

```
shipped: N4E4LLMLQQ  E ■KK  ■  ■ ■ ■  ■  ■ ■ ■  ■  ■ ■
grid:    (nothing)
```

### `cw-2026-08-18-004507`

30.0 s at 48000 Hz, read at 501 Hz.

| | shipped | grid |
|---|---|---|
| characters emitted | 113 | 48 |
| E-share | 13 % | 12 % |
| single-character words | 70 % | 14 % |
| words per minute read | 24 | 18.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 101 | 13 % | 67 % | 1100.3 / 3486.2 / 7243.2 |
| said no keying | 0 | no characters | no characters | nothing measured |
| had not decided | 12 | 17 % | 88 % | 280.2 / 1393.9 / 1688.2 |

What each read:

```
shipped: A A T  T E AEARR R RRL L D D M O O T  T N N E  E T  T N <BT><BT>  ■E ■E A ANACC H H E SESTATA TA TII GOTON N I HEHATANTNDDLLIINNGG  T THHIEISS T MEMESSSSSASANAGG E  E A PE
grid:    E JJ AT ARRL DOT NET <BT> EACH STATION HANDLING THIS MESSAG E PE
```

### `cw-2026-08-18-003016`

30.0 s at 48000 Hz, read at 669 Hz.

| | shipped | grid |
|---|---|---|
| characters emitted | 137 | 54 |
| E-share | 12 % | 6 % |
| single-character words | 46 % | 0 % |
| words per minute read | 27 | 22.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 120 | 13 % | 46 % | 496.3 / 2284.1 / 5364.1 |
| said no keying | 0 | no characters | no characters | nothing measured |
| had not decided | 17 | 0 % | 33 % | 1116.8 / 2455.0 / 5708.7 |

What each read:

```
shipped: ADAA D KRKPIPAAA11H15TE5TT IT ITIT W WAEAS ES J■JUNTNK K ■ <AS> ■ ■ ■ ■ S STITIRLRLL IL HIHVEVE M MKY Y E T E TMO EO O9E9J1T1BB  ■TE ■TT T W JEEJETSTST T V VFFBFB TB TUUBBEIELELIN
grid:    I<BT> HADA KPA15TT ITWAS #K <BT> #STILLHVEMY ETO 91B TT JUST VFB TUBELIN
```

### `cw-2026-08-18-003126`

30.0 s at 48000 Hz, read at 675 Hz.

| | shipped | grid |
|---|---|---|
| characters emitted | 127 | 54 |
| E-share | 10 % | 9 % |
| single-character words | 46 % | 44 % |
| words per minute read | 25 | 28.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 115 | 11 % | 43 % | 640.2 / 2637.1 / 7049.3 |
| said no keying | 0 | no characters | no characters | nothing measured |
| had not decided | 12 | 0 % | 57 % | -316.6 / 3846.8 / 8390.7 |

What each read:

```
shipped: <BT>  ■  ■ N <BT><BT> I IRWAWATTTCCHH AE AT AT L EL EAEEASTST S 22 T MTMOOVVII ES ESAA D DAEDACY AY WIWIDID N XTXY■■ A WIWHNHYY N NGOTOT  ■  ■ ■  ■ M ■, , W WEIESTESTERRNINS TS ■, E
grid:    A OM <BT> # <BT> I WATCH AT L EAST 2 MOVI ES A DAY WID X# WHY NOT # # , WESTERNS , E
```

### `cw-2026-08-18-003758`

30.0 s at 48000 Hz, read at 501 Hz.

| | shipped | grid |
|---|---|---|
| characters emitted | 112 | 46 |
| E-share | 15 % | 15 % |
| single-character words | 64 % | 43 % |
| words per minute read | 24 | 16.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 98 | 16 % | 64 % | -544.9 / 5274.6 / 12428.7 |
| said no keying | 0 | no characters | no characters | nothing measured |
| had not decided | 14 | 7 % | 62 % | -501.7 / 7711.9 / 12178.6 |

What each read:

```
shipped: QRR R L EL TSTU U ■ ■  EE EATAN EN EAANNDDE E A AAEA44MMRPTPX/S/4 4 G QTQNINIDK K ■ ■  ■  ■ ■ ■  ■E ■E AE ANN E EATANTNQQNNITIKK  ■  ■ ■ ■  ■  ■ ■ ■  ■ ■  EA ERNRN E
grid:    KIS QRLTU #EAN EANDE AA4MP/4QNIK # # EAN EPQNIK # # # EIN E
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

### `cw-2026-08-22-031905`

30.0 s at 48000 Hz, read at 600 Hz.

| | shipped | grid |
|---|---|---|
| characters emitted | 96 | 67 |
| E-share | 9 % | 37 % |
| single-character words | 76 % | 62 % |
| words per minute read | 23 | 28.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 64 | 3 % | 81 % | -3179.8 / 9925.4 / 35480.4 |
| said no keying | 7 | 0 % | 60 % | 11756.4 / 31282.6 / 75097.3 |
| had not decided | 25 | 28 % | 33 % | 2111.8 / 6990.0 / 24637.0 |

What each read:

```
shipped: <AS>. W PEPRERENEDIDINICCTETENED D R ■1T1O00R■.T.O■ ■  ■  ■  ■ ■  ■  ■ ■ ■ ■ ■  ■ D FEFLLAATAXX E III S S O 10 S0 ■2 I255M■, ,  ■ ■  ■ ■ ■  ■ ■ ■
grid:    D EEIE HE E IE <HH>E ET EES I TE I EH # E E T # I #E E EE# H# E SEI TI II H E EE A #5 S V # S # S E # EH # # # T T 
```

### `cw-2026-08-23-001520`

30.0 s at 8000 Hz, read at 600 Hz.

| | shipped | grid |
|---|---|---|
| characters emitted | 92 | 22 |
| E-share | 5 % | 5 % |
| single-character words | 30 % | 14 % |
| words per minute read | 26 | 8.0 |

The witness split, over the shipped decode:

**Three rows and not two.** `listening` is the meter before it has
formed a verdict at all, which is its first six seconds and any
stretch where it has not yet seen enough, and folding that into
`no keying` would report an absence of evidence as evidence of
absence (§0.0).

| witness | characters | E-share | single-char words | span LLR P10 / median / P90 |
|---|---|---|---|---|
| said keying | 80 | 5 % | 30 % | 530346081121.5 / 1780389687144.9 / 4621471536533.0 |
| said no keying | 0 | no characters | no characters | nothing measured |
| had not decided | 12 | 8 % | 0 % | 1060692162243.0 / 4579736709965.3 / 5871515142557.2 |

What each read:

```
shipped: CH33E3MZQQIIISS T DKK    T DCCTGQQ T KCCNGQQ  N DDEE E DKKNKCECSTTTTT33N TTTTTTQIII TTT TT T TTKT TKTTT TT TTT TTTT
grid:    AC3QIS K CQ CQ DE KC3QIS KCV
```

### `cw-2026-08-24-012403`

30.0 s at 48000 Hz, read at 600 Hz.

| | shipped | grid |
|---|---|---|
| characters emitted | 0 | 0 |
| E-share | no characters | no characters |
| single-character words | no characters | no characters |
| words per minute read | 21 | 28.0 |

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
| | said keying | 11 | 20.1 / 38.9 / 192.2 |
| | said no keying | 24 | 8549784.8 / 161634397.2 / 229555481.1 |
| | had not decided | 21 | 41550404.2 / 45142008.9 / 61204160.3 |
| `cw-2026-08-17-013622` | | | |
| | said keying | 0 | nothing measured |
| | said no keying | 35 | 2.9 / 7.0 / 65438853.3 |
| | had not decided | 21 | 31335836.0 / 41493187.8 / 49583861.6 |
| `cw-2026-08-17-134712` | | | |
| | said keying | 18 | 6.7 / 20.6 / 22.2 |
| | said no keying | 17 | 2.4 / 2.5 / 3.8 |
| | had not decided | 21 | 2.4 / 2.5 / 2.6 |
| `cw-2026-08-18-004507` | | | |
| | said keying | 49 | 34.3 / 39.6 / 49.0 |
| | said no keying | 0 | nothing measured |
| | had not decided | 7 | 44.8 / 48.0 / 54.0 |
| `cw-2026-08-18-003016` | | | |
| | said keying | 49 | 24.5 / 29.4 / 32.0 |
| | said no keying | 0 | nothing measured |
| | had not decided | 7 | 22.0 / 22.8 / 25.6 |
| `cw-2026-08-18-003126` | | | |
| | said keying | 49 | 27.7 / 32.9 / 38.3 |
| | said no keying | 0 | nothing measured |
| | had not decided | 7 | 36.6 / 38.5 / 39.3 |
| `cw-2026-08-18-003758` | | | |
| | said keying | 49 | 25.9 / 57.0 / 65.3 |
| | said no keying | 0 | nothing measured |
| | had not decided | 7 | 60.6 / 88.2 / 91.8 |
| `cw-2026-08-20-014854` (an independent sweep says this holds no keying at all) | | | |
| | said keying | 0 | nothing measured |
| | said no keying | 35 | 6.6 / 7.7 / 8.4 |
| | had not decided | 21 | 3.3 / 3.5 / 3.6 |
| `cw-2026-08-20-014935` (an independent sweep says this holds no keying at all) | | | |
| | said keying | 0 | nothing measured |
| | said no keying | 35 | 3.0 / 3.7 / 4.2 |
| | had not decided | 21 | 3.2 / 3.5 / 3.7 |
| `cw-2026-08-22-031905` | | | |
| | said keying | 42 | 23.9 / 28.5 / 32.2 |
| | said no keying | 4 | 20.8 / 21.2 / 22.8 |
| | had not decided | 10 | 10.1 / 14.8 / 18.4 |
| `cw-2026-08-23-001520` | | | |
| | said keying | 49 | 12391793823.6 / 13612218711.0 / 14750207662.0 |
| | said no keying | 0 | nothing measured |
| | had not decided | 7 | 15019761337.9 / 16310383275.5 / 16642275185.7 |
| `cw-2026-08-24-012403` | | | |
| | said keying | 0 | nothing measured |
| | said no keying | 35 | 5.3 / 6.5 / 9.8 |
| | had not decided | 21 | 2.6 / 3.0 / 3.8 |

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

