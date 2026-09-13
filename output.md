```
READ IN THIS ORDER.
```

A. The phase goal - the screen, done right. FIRST CUT, written at the end of task 0; the final
   report replaces it.
B. Step 0 criterion 3 with the power offer drawn: measured in task 0, not yet asserted.
C. Section 4 is carried at the final cut.

```
UNIT:       340 - stopped at task 0 of 3 (first cut, superseded by the final report) - 2026-09-12 21:43
PHASE GOAL: The main window as Tim's approved mockup, then the achievements pages as trading
            cards, then what the last phase left, then Tim's pass at his window.
UNIT GOAL:  Draw the PSK31 power offer and measure it under the S-meter at 1920 and 1400, with
            the top row holding while it shows.
ADVANCED:   not yet scored - task 0 is a trace
NUMBER:     top row with the offer drawn at 1920: 317 px (FT8 190)
DRIFT:      0
```

## 1. What Claude did

### Task 0 - the trace, numbers before task 1

`TheTopRowTests.Unit340TraceThePowerOfferOnPsk31`, licensed fixture, 1040 px tall, 910 px below
the pills.

| | PSK31 1920 | FT8 1920 | PSK31 1400 | FT8 1400 |
|---|---|---|---|---|
| offer border | 520 x 122 at y 328, visible | 0 x 0, hidden | 520 x 122 at y 328, visible | 0 x 0, hidden |
| offer top - rig display bottom (y 257) | 71 | - | 71 | - |
| sentence / ALC line | 5 lines / 4 lines | - | 5 / 4 | - |
| rig panel / card | 317 / 317 | 190 / 190 | 317 / 317 | 219 / 219 |
| top row, share | 317, 0.348 | 190, 0.209 | 317, 0.348 | 219, 0.241 |
| panels hidden / showing | 376 (0.413) / 323 (0.355) | 503 (0.553) / 450 (0.495) | 376 (0.413) / 323 (0.355) | 474 (0.521) / 421 (0.463) |

- Drawn, visible, non-zero: yes at both widths. Under the rig display: yes. Rig panel the card's
  height: yes.
- **Top row: no.** 317 px at 1920 against 209 (190 + 10%), 108 px over; at 1400 0.348 against
  0.262, 79 px over 238. **Panels: no.** 376 against 455 at both, 79 px short. The offer's border
  made it: 122 px plus its 2 px margin and 3 px spacing is the 127 px the row grew at 1920.
- `TheOperatorCanStopItTests` alone: 7 of 9.
- Carry-forward before any change: app 100 of 100, engine 85 of 85.

## 2. What the owner should expect

Nothing on the screen changed in task 0.

## 3. What you should see

No visible change yet. Computed on the headless host, not seen.

## 4. What's blocking us

Carried at the final cut.
