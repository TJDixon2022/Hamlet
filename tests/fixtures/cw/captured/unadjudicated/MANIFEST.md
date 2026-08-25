# CW captures — the session of Monday 24 August 2026 (local), 0115–0219 UTC on 25 August

**These are last night's captures and nothing else.** Thirteen 30-second WAVs
at 48 kHz, each with its sidecar, timestamped `cw-2026-08-25-011552` through
`cw-2026-08-25-021825`.

**On the dates.** The capture filenames carry UTC; the shack is UTC−04:00. The
session ran 21:15–22:19 local on Monday 24 August, which is 01:15–02:19 UTC on
25 August. That is why `cases-2026-08-24.txt` — the app's own evening list for
this session, included unrenamed — carries the 24th while every capture carries
the 25th. **The two dates are the same evening.** If a previous archive looked
old, it was the separate `cw-captures-2026-08-22_to_08-24.zip`, which holds
fourteen entirely different captures from 22–24 August and shares no file with
this one.

Also included: `ANALYSIS-2026-08-25-session.md`, which carries the full
measurements and the fix list these fixtures support.

All 40 m, input −13.3 dBFS throughout. This is the evening the decoder started
working.

**Permanent read-only fixtures (HM-DEC-091). Nothing edits a WAV or a sidecar.**
Independent readings below are one imperfect chain's decode — score movement
against them, not correctness.

| file | tone | WPM | duty | role |
|---|---|---|---|---|
| 011552 | 500.3 | 23.1 | 40% | K1ZJA call; early lock |
| 012748 | 401.2 | 21.8 | 46% | **Bug A fixture: 2 chars emitted, 113 marks present, everything else right** |
| 012823 | 499.8 | 22.5 | 43% | negative control: tone 50 Hz off + clock 9.5 high → soup |
| 012922 | 492.3 | 21.5 | 39% | lock recovering |
| 013010 | 501.1 | 26.8 | 40% | full QSO; confidence-gate control (gate must not damage it) |
| 013150 | 501.4 | 27.6 | 42% | `CQ CQ CQ DE ND4K` |
| 013303 | 501.4 | 28.0 | 42% | **Hamlet beat the independent chain** — floor case |
| 013402 | 536.6 | 30.9 | 46% | 0 unsure at the old grid ceiling |
| 013520 | 536.8 | 30.8 | 43% | reference case: 59 chars, 1 unsure |
| 013637 | 536.2 | 30.6 | 47% | gap clusters merge at speed (24/28/171 ms) — joint-cutter fixture |
| 021410 | 540.7 | 18.2 | 38% | machine-grade fist, separable gaps, still miscut (`ATEEKEND`) |
| 021629 | 504.7 | 20.8 | **24%** | confidence-gate fixture: `559 559 IN MI MI` buried in noise |
| 021825 | 394.0 | 17.9 | **18%** | confidence-gate fixture: 8-second `KC1UEK` call in 30 s of invented text |

The duty law the session obeys: 38–47% → readable (0–8 unsure); 24% → real
content buried; 18% → mostly invented. Same input level, tone locked within a
few hertz in every case. The remaining fault is decoding silence, and the
sidecars' own `spanLlr` values already separate signal characters (150–6200)
from noise characters (0.9–87, several negative). Details, thresholds and the
per-element normalisation are in the analysis document.
