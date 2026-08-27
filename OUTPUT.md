# Work instruction 025 — the cuts, not the letters

## 1. What Claude did

Claude Code, on the development computer, in `C:\Source\HamLet`. The prompt
claimed `PROJECT: Hamlet`; the tree confirmed all four checks — `SHACK_FACTS.md`
and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` exist, neither
`CoreHMI.sln` nor `MURC.sln` does. Branch `main` throughout, five commits, all
pushed, none refused. Version 1.11.21 to 1.11.22 per HM-DEC-150.

**Nothing here is evidence about the radio.** No rig was connected.

**No decision was recorded under §12.1.** Section 4 carries what needs a ruling.

**All four tasks ran, including the drop.** The joint decoder is built and ships
**off**, per the ruling's own second branch.

## 2. Whether it ships on, and what to expect at the radio

**It ships off.** `AppSettings.UseJointDecoder`, default false. The ruling says
default on only if every floor and every anchor is green, and they are not.

**What it does when thrown, measured:**

| capture | before | after |
|---|---|---|
| `cw-2026-08-25-013637` | `AB OV E` | **`ABOVE`** |
| `cw-2026-08-25-013637` | `BR EE Z E` | **`BREEZE`** |
| `cw-2026-08-25-013637` | `REV■R` | **`REVER`** |

**And it loses every word space.** The same capture reads
`■■TEMPNEVENTREVERGOTABOVE■75FES■CLEARSKYLITEBREEZEALLDAYJUSTAWESO` — the
letters repaired and the words run together. `cw-2026-08-18-004507`'s anchor is
`N HANDLING THIS MESSAG`, which needs those spaces, so turning it on takes that
anchor red. **Shipping it on while an anchor is red is the one thing the order
forbids outright.**

**So tonight, nothing changes unless he throws the switch**, and if he throws it
he gets better letters and no spaces. That is worth having on a callsign and not
on a rag-chew, which is exactly why the ruling wanted a switch rather than a
replacement.

**Three smaller things did change, unconditionally:**

- **After a Clear the sheet no longer says "everything read since the decoder
  started listening"** over a transcript that begins at the clear. It names the
  moment: `since the transcript was cleared at 21:04:11 UTC`.
- **`competing` no longer says `none found` in every sidecar.** It now says what
  the survey did see — the loudest thing in the band, its lift, its duty — and
  plainly that nothing has judged it to be a station.
- **The keying sweep needed no change.** It is already behind
  `AppSettings.ShowKeyingSweep`, already off.

**What will look wrong and is not:**

- **The engine shows 28 red.** Byte-identical to the stable set, with the setting
  off, which is the whole point of shipping it off.
- **Two acceptance lines could not be measured at all.** `011447` and `011514`
  are not in the tree.

## 3. The four named failures, before and after

**`cw-2026-08-25-021410`** — the order quotes `ATEEKEND`, `TTHINKING`, `FLENX`.
**None of those three strings is what the tree produces.** Verbatim, today:

```
 ■ ■ ■ M ■ ■ ■ ■ T O MTT T  Y M TT ■ ■ O AO IHI DT ■RIGHR IS ■ FLENT 66OAM
```

`FLENT`, not `FLENX`. No `ATEEKEND` and no `TTHINKING` anywhere. With the cutter
on:

```
■■■M■■■■TOTTTTTYMTT■■OMATTTIHIDT■RIGHRIS■FLENT66OAM
```

**Neither reading contains the target words**, so those three acceptance lines
are not met and could not have been — the capture does not say them.

**`cw-2026-08-25-013637`** — both lines met:

| | verbatim |
|---|---|
| before | ` TE MP NEVEN T REV■R G O T AB OV E ■7 5 F ES ■CLEAR S KY LI TE BR EE Z E ALL DAY JUST AWE SO` |
| after | `■■TEMPNEVENTREVERGOTABOVE■75FES■CLEARSKYLITEBREEZEALLDAYJUSTAWESO` |

**`AB OV E` → `ABOVE` ✓, `BR EE Z E` → `BREEZE` ✓**, and `REV■R` → `REVER`
besides. **At the cost of all nineteen spaces.**

**`cw-2026-08-25-011447`** — `USEDTOUSEAFIRM` → `USED TO USE A FIRM`.
**Unmeasurable: the fixture is not in the tree**, and it is not in this zip.

**`cw-2026-08-25-011514`** — `OUTOFALT`. **Unmeasurable, same reason.**

### The other fixtures

| capture | before | after |
|---|---|---|
| `cw-2026-08-18-004507` | `...AC H STA TION HANDLING THIS MESSAGE PE` | `...EACHSTATIONHANDLINGTHISMESSAGEPE` |
| `cw-2026-08-17-134712` | `...N4 L ZT ■K...` | `...N4LTNET■K...` |
| `cw-2026-08-25-021825` (noise) | 41 characters, mixed blocks and letters | same shape, spaces gone |
| both silence controls | nothing | **nothing** |

**`021825` still yields blocks rather than letters** — the guard's own acceptance
line, met. Both silence controls emit nothing with the cutter on and off.

### Task 3 — the constrained margin

Second-best is now the same span and the same element boundaries read as a
different character, which is what the analysis asked for.

| | n | P10 | median | P90 | max |
|---|---|---|---|---|---|
| **anchored recordings** | 522 | 1.728 | **4.622** | 11.860 | 14.8 |
| everything else | 843 | 1.290 | **4.441** | 8.889 | 13.6 |

**The scale problem is solved and the separation is not.** The whole observed
range is 1.29 to 14.8, where the old margin printed `6:27306879.3` and needed
clamping to stay readable. But the medians are 4.622 against 4.441, where the
analysis's target was an order of magnitude.

**The split is by recording rather than per character, and that limits it.** This
corpus has no character-by-character truth, so "anchored" stands in for
"correct", and an anchored recording contains plenty of soup outside its anchor.
Measured and reported only; nothing changed on it.

### The suite

| | baseline | end |
|---|---|---|
| engine | 28 of 1841, stable set | **28 of 1841, byte-identical** |
| app | 503 of 503 | **503 of 503** |

Diffed rather than totalled. No intermittent fired in either run.

### Where the instruction and the tree disagree

- **Four of the seven named fixtures are absent**: `011447`, `011514`, `011112`,
  `011617`. Two acceptance lines rest on them.
- **`021410`'s three quoted failures are not in the tree's reading of it.** It
  produces `FLENT`, and neither `ATEEKEND` nor `TTHINKING` appears at all.
- **`013637`'s quotes are one space out** — the tree reads `AB OV E` and
  `BR EE Z E`, the order quotes `AB OVE` and `BREE Z E`. The fault is the same.
- **The keying-sweep mismatch is answered**: `AppSettings.ShowKeyingSweep` exists
  in the tree, defaults off, and gates the meter. **The analysis is reading older
  captures.**
- **`tools/reference-decoder/` holds `README.md` and `reference_decoder.py`
  only** — carried correctly from unit 1.11.20's correction.
- **The baseline was 28 and byte-identical**, as stated.

## 4. What's blocking us

**The cutter repairs the letters and cannot find the words, and the reason is a
ruling this project already made.**

Ruling asked for:

> **The joint cutter ships off until it can find word gaps on a compressed fist.
> It scores the closing gap against three units and seven, and on
> `cw-2026-08-25-013637` at thirty words a minute the word gap runs well under
> one unit, so three wins every time and every space is lost. That is HM-DEC-115
> arriving a second time — gaps are clustered from the sender's own keying and
> never taken as multiples of the dit. The cutter already accepts the three
> fitted classes; what it does not always get is them.**

The evidence is in section 3. The cutter takes `gapHops` when the caller has
them, and the streaming path supplies them only while `_structureHeld`. **Where
they are absent it falls back to 1u/3u/7u, and that fallback is what loses the
spaces.**

*Rejected: raising the validity term to recover the spaces.* The order forbids
tuning it upward to reach an acceptance line, and it would not have helped —
the word/character choice is a duration comparison the bonus does not enter.

*Not proposed, because it needs a ruling:* whether the cutter should fit its own
three gap classes from the marks it is already holding, rather than depending on
the first pass to hand them over. **That is a second clustering stage inside the
decode path and it touches what the display asserts**, so it is Tim's.

---

**Two acceptance lines could not be measured, and one set of three was measured
against text the capture does not contain.**

`011447` and `011514` are absent, so `USEDTOUSEAFIRM` and `OUTOFALT` are
unmeasurable rather than failed. And `021410` reads `FLENT` where the order
expects `FLENX`, with no `ATEEKEND` and no `TTHINKING` in it at all — so the
three lines resting on that capture are not tests this tree can pass or fail.
**Five of the eight named acceptance lines were therefore unmeasurable; two were
met; one — the spaces — was broken.**

---

**A defect found in this unit's own first build, worth recording because it
wastes a session when it recurs.**

The setting was first built as a mutable static on the decoder. **xUnit runs test
classes in parallel**, so the decode path read whatever another test had last
left there, and the offline route measured itself as unchanged while the
streaming route plainly was not. It is an instance property now. **A mutable
static read by the decode path cannot be measured by this suite.**

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Nineteen inbound. The
oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150 — nor for
   Tim's rulings of 2026-08-25/26/27, including the two this unit acts under.**
5. **The tone tracker** — six axis families measured; the operator's assertion is
   the way round it meanwhile.
6. **The integrator width** — settled at 45 Hz, with the sharp-peak caveat.
7. **The guard's gap is two to one**, calibrated on two empty captures.
8. **A boxcar's nulls made two of five swept offsets pathological best cases.**
9. **Two stations closer than 125 Hz are not named** — the operator's own item
   five, still not attempted.
10. **The keying meter** — its measurement found a station its verdict denied.
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
12. **The gate opens on everything, including two empty recordings** (1.11.18).
13. **Pooling versus a held peak** (1.11.20), half closed by 1.11.21.
14. **The constrained margin is bounded and still does not separate**, above.
15. **The joint cutter cannot find word gaps on a compressed fist**, above — the
    headline ask.
16. **Four fixtures are absent and five acceptance lines were unmeasurable**,
    above.
17. **A mutable static in the decode path cannot be measured under xUnit**,
    above.
18. **`013347` returns a likelihood ratio of 17.2 million**, with `001520`'s
    quadrillions. Parked, raised once.
19. **The keying-sweep mismatch is answered** — the tree is correct and the
    analysis reads older captures. **Closed.**

New this unit: **the cutter's word-gap fallback**, above; **five unmeasurable
acceptance lines**, above; **the parallel-unsafe static**, above.

Closed this unit: **the element-to-character decision**, built as ruled and
shipped behind its switch, repairing `ABOVE`, `BREEZE` and `REVER`. **The
constrained margin**, measured. **All three of task 4's items.**

Still open: **the lock's mixed help**; **three fixtures at accepted cost**; **the
reference and port integrator difference**; **an unmeasured pitch costs `N4L`**;
**the six-hertz window disagreement**; **the short-character bias**; **the
Avalonia geometry offset**; **`CHANGELOG.md` at 1.9.0 against 1.11.22**; **the
whole-file second pass**; **the squelch has no axis**; **the three morning
captures of 2026-08-26**; **seven timing intermittents, none of which fired
today**.
