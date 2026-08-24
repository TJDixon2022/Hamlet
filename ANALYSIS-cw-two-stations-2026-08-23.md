# Two senders in one passband, 2026-08-23

Nothing in this repository had measured what the decoder does with two
stations in one passband. Every fixture held one sender and all nine
captures were analysed as though one station were present.

The wanted station sends `CQ DE N0CALL K` at 18 words a
minute, 600 Hz, 15 dB over a band of noise
shaped to the receiver's own passband. The competing station sends
`DE N0AAA UP` at 24 words a minute, starting a third of a
second later so its marks land inside the wanted station's rather than
beside them. Both key throughout.

**Integrator: Hann.**

Regenerate with:

```
dotnet test tests/Hamlet.RadioEngine.Tests --filter FullyQualifiedName~TheTwoStationTable
```

## The control: one station, alone

Same recipe, same seed, same band, with the competing station left
out. Read four ways, so a difference can be attributed to a stage
rather than to the whole path.

| read how | correct | wrong | invented | emitted | E-share | read |
|---|---|---|---|---|---|---|
| whole file, fixed pitch | 11 | 0 | 0 | 11 | 9 % | ` CQ DE N0CALL K ` |
| whole file, forced to 18 wpm | 11 | 0 | 1 | 12 | 8 % | ` CQ DE N0CALL K # ` |
| streaming window, pitch nailed to 600 Hz | 11 | 0 | 1 | 12 | 8 % | ` CQ DE N0CALL K ■ ` |
| the production path, tracker and all | 9 | 2 | 20 | 31 | 16 % | `QQ T DEDE E NNM■00B6EIAEARLILLL T KK  ■` |

**How hard the two-station fixture actually is**, at 40 Hz and equal
level, measured through the decoder's own front end pointed at each
station in turn: **0.94 s with both keys down at once**, 
3.38 s of the wanted station alone and 1.05 s of the other alone. A fixture where the
two never collide proves nothing about rejection and looks exactly
like one that does (§12.5).

**`levelDb` is a ratio of keyed amplitudes, not of averages.** The two
stations send different text at different speeds, so their key-down
fractions differ and a whole-recording average of the competing
station sits about six decibels below the wanted one at a stated level
of nought.

## At a fixed pitch, with no tracker

**This is the one a front-end change is judged on.** Nothing moves the filter, so the only thing standing between the competing station and the envelope is the integrator.

`correct` counts characters read as sent, of nine. `invented` counts characters read where nothing was sent at all, which is `CwMatchKind.Invented` and not the `Wrong` the sensitivity sweep prints under that name.

| offset | level | correct | wrong | invented | emitted | E-share | read |
|---|---|---|---|---|---|---|---|
| 40 Hz | +0 dB | 11 | 0 | 0 | 11 | 9 % | ` CQ DE N0CALL K ` |
| 40 Hz | -6 dB | 11 | 0 | 0 | 11 | 9 % | ` CQ DE N0CALL K ` |
| 40 Hz | -12 dB | 11 | 0 | 0 | 11 | 9 % | ` CQ DE N0CALL K ` |
| 80 Hz | +0 dB | 11 | 0 | 0 | 11 | 9 % | ` CQ DE N0CALL K ` |
| 80 Hz | -6 dB | 11 | 0 | 0 | 11 | 9 % | ` CQ DE N0CALL K ` |
| 80 Hz | -12 dB | 11 | 0 | 0 | 11 | 9 % | ` CQ DE N0CALL K ` |
| 120 Hz | +0 dB | 11 | 0 | 0 | 11 | 9 % | ` CQ DE N0CALL K ` |
| 120 Hz | -6 dB | 11 | 0 | 0 | 11 | 9 % | ` CQ DE N0CALL K ` |
| 120 Hz | -12 dB | 11 | 0 | 0 | 11 | 9 % | ` CQ DE N0CALL K ` |
| 200 Hz | +0 dB | 11 | 0 | 0 | 11 | 9 % | ` CQ DE N0CALL K ` |
| 200 Hz | -6 dB | 11 | 0 | 0 | 11 | 9 % | ` CQ DE N0CALL K ` |
| 200 Hz | -12 dB | 11 | 0 | 0 | 11 | 9 % | ` CQ DE N0CALL K ` |
| 300 Hz | +0 dB | 11 | 0 | 0 | 11 | 9 % | ` CQ DE N0CALL K ` |
| 300 Hz | -6 dB | 11 | 0 | 0 | 11 | 9 % | ` CQ DE N0CALL K ` |
| 300 Hz | -12 dB | 11 | 0 | 0 | 11 | 9 % | ` CQ DE N0CALL K ` |

## Through the production path, tracker and all

**This is what the operator would get.** The tracker can walk off to the competing station, and where it does the text collapses for a reason that has nothing to do with the filter.

`correct` counts characters read as sent, of nine. `invented` counts characters read where nothing was sent at all, which is `CwMatchKind.Invented` and not the `Wrong` the sensitivity sweep prints under that name.

| offset | level | correct | wrong | invented | emitted | E-share | read |
|---|---|---|---|---|---|---|---|
| 40 Hz | +0 dB | 8 | 3 | 17 | 28 | 18 % | `QQ T DEDE EE NNM■00DD  E RLILLL T KK  ■` |
| 40 Hz | -6 dB | 8 | 2 | 20 | 30 | 23 % | `QQ T DEDE E NN  E  TE TDCECAEARLILLL T T   ■ ` |
| 40 Hz | -12 dB | 10 | 1 | 20 | 31 | 16 % | `QQ T DEDE E NNM■00DCECAEARLILLL T KK  ■` |
| 80 Hz | +0 dB | 8 | 3 | 18 | 29 | 14 % | `QQ T DEDE E NN U URUPP  ■ ■ E LLALLL T KK  ■` |
| 80 Hz | -6 dB | 8 | 2 | 21 | 31 | 19 % | `QQ T DEDE E NN  ■ ■TETDCECAEARLILLL T T   ■ ` |
| 80 Hz | -12 dB | 9 | 2 | 20 | 31 | 16 % | `QQ T DEDE E NNM■00B6EIAEARLILLL T KK  ■` |
| 120 Hz | +0 dB | 7 | 4 | 18 | 29 | 14 % | `QQ T DEDE E NN U URUPP  ■ ■ E LLILLL T KK  ■` |
| 120 Hz | -6 dB | 9 | 1 | 22 | 32 | 19 % | `QQ T DEDE E NNM■0E0DCECAEARLILLL T T  ■ ■ ` |
| 120 Hz | -12 dB | 9 | 2 | 20 | 31 | 16 % | `QQ T DEDE E NNM■00B6E■AEARLILLL T KK  ■` |
| 200 Hz | +0 dB | 9 | 2 | 20 | 31 | 16 % | `QQ T DEDE E NNM■00B6E■AEARLILLL T KK  ■` |
| 200 Hz | -6 dB | 8 | 3 | 16 | 27 | 15 % | `QQ T DEDE E NNM■00DD  E LLILLL T KK  ■` |
| 200 Hz | -12 dB | 8 | 3 | 16 | 27 | 15 % | `QQ T DEDE E NNM■00DD  E LLILLL T KK  ■` |
| 300 Hz | +0 dB | 9 | 2 | 20 | 31 | 16 % | `QQ T DEDE E NNM■00B6EIAEARLILLL T KK  ■` |
| 300 Hz | -6 dB | 9 | 2 | 20 | 31 | 16 % | `QQ T DEDE E NNM■00B6EIAEARLILLL T KK  ■` |
| 300 Hz | -12 dB | 8 | 3 | 16 | 27 | 15 % | `QQ T DEDE E NNM■00DD  E LLILLL T KK  ■` |

