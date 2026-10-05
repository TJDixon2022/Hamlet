# The CW keying map

**Where every station keys in the owner's twelve recordings** (work instruction 539, HM-DEC-243), read offline and non-causally by `TheKeyingMapTests`, whether or not a station has a reference. The scoreboard reads it to find what was never sent.

- **How:** at every pitch from 300 to 900 Hz in the detector's own 25 Hz steps, the audio is mixed to nought and low-passed at 30 Hz four times, forward and back. A mark is 25 ms or more where a pitch stands 13 dB over the band at that instant, the band being the median level of every pitch read. The radio's AGC moves every pitch together, so it cancels; noise is flat across pitch, and a keyed tone is not.
- **A neighbour's leak is not a station:** a mark overlapped by one 6 dB louder within 100 Hz is dropped.
- **A station** is a run of neighbouring pitches holding five marks or more. Its spans are its marks joined across gaps under two seconds.
- **A silence** is two seconds or more with no station keying at any pitch.
- **Limits:** a station under 13 dB over the band is not seen, and anything read from it counts as invented. Thirty seconds of loud noise keys no station.
- **Invented:** a printed letter is invented when its marks overlap no map mark within one bin either side of its pitch's nearest grid pitch, with 10 ms of tolerance. The live path's marks start within a few milliseconds of the map's.

### `cw-2026-10-02-200157`, 30.0 s

| station | grid pitches | marks | keys |
|---|---|---|---|
| 663 Hz | 650-675 | 180 | 0.2-7.3 s, 9.6-29.9 s |

Silences of two seconds or more: 7.3-9.6 s.

### `cw-2026-10-03-143906`, 30.0 s

| station | grid pitches | marks | keys |
|---|---|---|---|
| 513 Hz | 500-525 | 175 | 0.0-7.9 s, 13.7-24.5 s, 26.8-27.1 s |

Silences of two seconds or more: 7.9-13.7 s, 24.5-26.8 s, 27.1-30.0 s.

### `cw-2026-10-03-143951`, 30.0 s

| station | grid pitches | marks | keys |
|---|---|---|---|
| 522 Hz | 475-650 | 59 | 5.7-6.4 s, 13.8-29.3 s |

Silences of two seconds or more: 0.0-5.7 s, 6.4-13.8 s.

### `cw-2026-10-03-144020`, 30.0 s

| station | grid pitches | marks | keys |
|---|---|---|---|
| 498 Hz | 475-525 | 31 | 0.8-11.3 s |
| 598 Hz | 575-625 | 60 | 13.6-30.0 s |

Silences of two seconds or more: 11.3-13.6 s.

### `cw-2026-10-03-144045`, 30.0 s

| station | grid pitches | marks | keys |
|---|---|---|---|
| 495 Hz | 475-525 | 5 | 29.5-30.0 s |
| 598 Hz | 575-625 | 126 | 0.1-27.6 s |

Silences of two seconds or more: none.

### `cw-2026-10-03-221502`, 30.0 s

| station | grid pitches | marks | keys |
|---|---|---|---|
| 491 Hz | 475-525 | 210 | 0.0-21.4 s, 24.5-30.0 s |

Silences of two seconds or more: 21.4-24.5 s.

### `cw-2026-10-03-221530`, 30.0 s

| station | grid pitches | marks | keys |
|---|---|---|---|
| 488 Hz | 475-500 | 70 | 0.2-9.2 s |
| 597 Hz | 575-625 | 181 | 9.2-29.4 s |

Silences of two seconds or more: none.

### `cw-2026-10-03-221548`, 30.0 s

| station | grid pitches | marks | keys |
|---|---|---|---|
| 496 Hz | 475-525 | 81 | 17.9-28.7 s |
| 596 Hz | 575-625 | 149 | 0.0-17.9 s |

Silences of two seconds or more: none.

### `cw-2026-10-03-221745`, 30.0 s

| station | grid pitches | marks | keys |
|---|---|---|---|
| 507 Hz | 475-525 | 121 | 1.2-26.3 s |
| 605 Hz | 575-625 | 21 | 2.8-3.1 s, 26.5-30.0 s |

Silences of two seconds or more: none.

### `cw-2026-10-03-221805`, 30.0 s

| station | grid pitches | marks | keys |
|---|---|---|---|
| 508 Hz | 475-525 | 30 | 0.1-6.2 s |
| 605 Hz | 575-625 | 144 | 6.4-30.0 s |

Silences of two seconds or more: none.

### `cw-2026-10-03-221828`, 30.0 s

| station | grid pitches | marks | keys |
|---|---|---|---|
| 606 Hz | 575-625 | 206 | 0.0-30.0 s |

Silences of two seconds or more: none.

### `cw-2026-10-03-221851`, 30.0 s

| station | grid pitches | marks | keys |
|---|---|---|---|
| 607 Hz | 575-625 | 180 | 0.0-29.6 s |

Silences of two seconds or more: none.
Test Run Successful.
Passed: 1
Total time: 4.3855 Seconds
