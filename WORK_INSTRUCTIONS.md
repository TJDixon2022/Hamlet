# Work instruction 488 - the pitch the detector found reaches the decoder

**Hand run. One unit. One wire.** The detector finds a station and reports no pitch; the decoder
then reads a bin forty to a hundred and seventy hertz away and prints nothing. **No test against
a recording, a fixture, a floor or copied telemetry** (R96). A headless test driving synthetic
hops written in the test itself is allowed; nothing read from disk. Build, run the app
carry-forward line, and stop.

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs
  MUST EXIST:      CW_REQUIREMENTS.md
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln
  root             C:\Source\HamLet

If all five are not as stated, refuse: reply with only the path you are in,
which checks failed, and "wrong project - nothing done."

If all five hold, say "Hamlet confirmed" and continue.
```

---

## 1. Rules

- Take `SESSION.lock` through `tools\arbiter\lock.bat take`, release it at the end. Write nothing
  to `RUN_LEDGER.md`. Touch nothing under `tools\arbiter\`. Tick nothing in `PHASE_PLAN.md`.
- One `dotnet test` invocation per line, filtered, with a `timeout`. Never background and poll.
  The app line loses names to the dispatcher loop; re-run once, count neither way.
- Apostrophes in quoted heredocs break; `;`, `rm` and `git rm` are refused; Python cannot run
  here; `-m` more than once for a multi-line commit. Scripts go in `.run-unit\unit488-<name>.sh`,
  and **nothing under `.run-unit\` is committed**.
- Nothing that keys or transmits. Nothing written to the radio.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. The evidence

**The owner's verdict rows, 2026-09-28, 23:38 to 23:39 UTC, 7.0328 to 7.0330 MHz.** Read them
from `%APPDATA%\Hamlet\telemetry\2026-09-28.jsonl`, event `owner_verdict`. **Do not copy the
file into the tree; read nothing from disk in a test.** The figures:

| time | verdict | bars | marks in 4 s | `scopePitchHz` | meter | meter Hz | median | score | mixing |
|---|---|---|---|---|---|---|---|---|---|
| 23:38:25 | idiot | say keying | 0 | **null** | keying | 525 | 47 ms | 0.23 | **352** |
| 23:38:34 | idiot | say keying | 14 | **null** | keying | 625 | 51 ms | 0.24 | **584** |
| 23:38:35 | agree | say keying | 0 | **null** | keying | 625 | 51 ms | 0.36 | **578** |
| 23:39:03 | idiot | say keying | 14 | **null** | keying | 625 | 51 ms | 0.25 | **584** |

**A 51 ms dit is 23 words a minute, and a score of 0.36 with a 22 dB swing is a strong, clean
station.** The owner heard it plainly. Hamlet printed nothing at all.

**What the rows say, and it is one thing.** `scopePitchHz` is **null on every row where the bars
say keying.** The detector is calling keying, counting fourteen marks in four seconds, and
reporting no pitch. Unit 486 built the mixing pitch's second rung from the detector's watched
pitch, fed from the view model while keying; **that rung is being fed null**, so the tracker's own
guess wins and the decoder reads 584 while the station keys at 625 - and once, 352 against 525.

**Why that ends in zero characters rather than junk.** Unit 487's rule is that a character is
emitted only if the blocks under its span match its elements. The decoder is reading a bin beside
the station, so its characters' elements line up with no block, and nothing is printed.
**487 did not break this; it exposed it.** Before 487 the same fault produced a wall of `E`s and
`T`s from the wrong bin.

---

## 3. Change one - keying always comes with a pitch

**`CwEnvelopeDetector` never reports keying without a pitch.** If it called bars, it called them
in a bin, and that bin is the pitch.

- Wherever the reading's keying is true, its pitch is the bin the bars were called in - the bin
  the hold is following (unit 485), not null and not NaN.
- **Find why it is null today.** The pitch may be set only on the hop a mark is up and cleared in
  the gap, while keying is held across gaps; or it may be set on one reading and not another.
  **Name the cause in the report** - do not paper over it by copying a value forward.
- The reading's pitch and its keying change together: both true, or both absent.

**Watch it fail first**, headless, with synthetic hops written in the test: a keyed tone at 625 Hz
with ordinary character gaps. Red while any reading has keying true and no pitch; green when
every such reading carries 625.

## 4. Change two - that pitch is what the decoder mixes at

**While the detector says keying, the decoder mixes at the detector's pitch.** Unit 486 built the
rung; this makes it carry.

- `MainWindowViewModel` feeds the decoder the detector's pitch whenever keying is true, and
  nothing otherwise. **Assert in a test that it is fed a number, not null**, for a driven
  detector reporting keying at 625.
- The rung's order is unchanged: the operator's lock first, the detector's pitch second, the
  tracker third.
- **While keying holds through a station's gaps, the mixing pitch holds with it** - it does not
  fall back to the tracker between characters.

**Watch it fail first**, headless: a driven detector reporting keying at 625 Hz while the tracker
holds 584. Red while the decoder mixes at 584; green when it mixes at 625 within a hop and stays
there through a gap.

**The decoder's own decisions are not changed** - not the lattice, not the unit estimator, not
the emission gate, not unit 487's block rule. Only where it is pointed.

## 5. And say, from the rows

In the report, state plainly: **with the pitch carried, would the station of 23:38:34 have been
decoded?** Drive a synthetic tone at 625 Hz, 23 words a minute, at the contrast those rows show,
and give the characters before and after the change. If it still prints nothing, say so and say
what the next thing in the way is. **Do not tune anything to make it print.**

---

## 6. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 488 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 488.
- Patch-bump `Directory.Build.props`.
- `DECISIONS.md`, newest first, **HM-DEC-193**, headline *The detector's pitch is what the decoder
  mixes at*, naming that the reading carried keying with a null pitch, that unit 486's rung was
  therefore fed nothing, and that unit 487's block rule turned the resulting wrong-bin decode
  from junk letters into silence.
- **Touch no checkbox** in `PHASE_PLAN.md`. No new ruling: R97 and 12.4 already say this.

---

## 7. Report

Section 2, for the owner, in plain words:

- rebuild;
- on a station, the panel's *tone* and *mixing* numbers should be the same number, and stay the
  same through the gaps between letters;
- characters should appear over their blocks.

Section 1: the cause of the null pitch, named, and what changed file by file, and that the build
and the app line are green. Section 3: §5's before-and-after characters. Section 4: anything
left, a line each.
