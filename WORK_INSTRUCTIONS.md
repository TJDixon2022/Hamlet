# Work instruction 496 - the station's own bin

**Hand run. One unit.** The last thing in the chain, and it has been red and known for two days.
`ThatPitchIsTheStationsOwn`.

**No test against a recording, a fixture, a floor or copied telemetry** (R96). A headless test
driving synthetic hops written in the test itself is allowed; nothing read from disk. Verify by
building `Hamlet.sln` with warnings as errors and running the app carry-forward line. **The
owner's report at the radio is the test.**

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
  here; `-m` more than once for a multi-line commit. Scripts go in `.run-unit\unit496-<name>.sh`
  and are not committed.
- Nothing that keys or transmits. Nothing written to the radio.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. What the owner saw, and what it means

**W1AW on 7.0475, 2026-09-29 20:02 UTC**, input peaking at −13 dB with noise at −22 - the
strongest, cleanest signal in the whole record. The panel read:

> **`tone 600 Hz heard · decoding at 666 Hz`**

and the terminal read `MA S T Q S T D E W 1 A W W 1 A W E E E E E E EEE E E E EE EE IE...`, with
**two blocks in four seconds** on a station sending continuously.

**The detector hears the station at 600 and the reader is grouping marks at 666.** It caught
`W1AW W1AW` in passing, which is how close it is, and then fell apart. Sixty-six hertz is the
shoulder of the tone's lobe, not the tone.

**This is `ThatPitchIsTheStationsOwn`, red since unit 488, at 1024 failures.** Unit 488 measured
the cause on a synthetic 625 Hz station: **two hops in three, the station's own bin calls no bars
and the shoulders do.** At 625 Hz the bin's gaps read about −20 dB while the bins 50 Hz either
side read −42. The mark's energy leaks across the window, so the station's own bin never shows a
clean floor - **the very loudness that makes it the station is what disqualifies it**, because a
bar is called by standing clear of its own gaps.

And unit 488 measured what it costs. Same audio, same decoder:

| the reader fed | reads |
|---|---|
| the shoulder pitch, which is what happens now | `RE    N  D   K` - 5 characters |
| the station's own 625 Hz | `RQ DEN0CAL 0L K` - 12 characters |

**Every unit from 489 to 495 has been downstream of this.**

---

## 3. The change - a bin is chosen by what it does while the key is down

**The station's bin is the loudest one while the marks are up.** That is what makes it the
station. How quiet its gaps look is a property of the window's leakage, not of the sender.

- **When a bar is called in a bin, walk to the true peak of its lobe** - unit 490 already walks to
  the neighbour with the higher mean level over the mark's own hops. **Check that walk reaches the
  peak and does not stop early**, and say in the report what it does at HEAD on a tone whose lobe
  spans several bins.
- **A bin is not disqualified for having noisy gaps when a louder bin beside it is keying at the
  same time.** Where two bins call bars over the same hops, **the one with the higher level during
  the marks is the station**, and the others are its lobe. Fold them into it rather than calling
  them separate marks or preferring them.
- **Do not loosen the flatness tolerance, the shortest bar, the pairing agreement, or the wander
  check.** The fix is choosing correctly among bins that all pass, not letting more through.
- **The mark's reported pitch is the station's bin**, so `CwMark.Hz`, the run reader's grouping,
  `StationPitchHz` and `PrintingHz` all name the station rather than a shoulder.

**How the peak is found, and the tolerance for "the same time", are the author's**, stated in the
report with their reasons, derived from the window's own shape and the hop length - **not from any
recording, and not tuned after reading a result.**

**Watch it fail first.** `ThatPitchIsTheStationsOwn` is the test, red at 1024:

1. **Green when a synthetic 625 Hz station's marks are reported at 625**, not 575 or 675, on
   substantially every hop - say the count before and after.
2. **`NoiseAloneReadsNothing` and `ThreeMinutesOfNoiseReadNothing` stay green.** If choosing by
   loudness-while-keyed lets noise through, say so with the count and do not force the first green.
3. **The clean call, the call with bursts, and the two-station case all still read whole** - units
   492 and 493's five cases. Report their text.
4. **The two-station case is the one to watch**: two stations 200 Hz apart must stay two stations,
   not be folded into one lobe. Report which is printed and that none of the other's marks are in
   it.

**Also report, from the synthetic 625 Hz station:** the text the reader prints before and after
this change. Unit 488's numbers were 5 characters against 12.

---

## 4. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 496 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 496.
- Patch-bump `Directory.Build.props`.
- `DECISIONS.md`, newest first, **HM-DEC-200**, headline *A station's bin is the loudest while the
  key is down, not the one with the quietest gaps*, naming unit 488's measurement - two hops in
  three, −20 dB against −42 - and the owner's W1AW screen, `tone 600 heard, decoding at 666`, as
  the reason.
- **Touch no checkbox in `PHASE_PLAN.md`.**

---

## 5. Report

Section 2, for the owner, in plain words:

- rebuild;
- the panel's two numbers - *tone N heard* and *decoding at N* - should now be the same number on
  a station;
- W1AW should read;
- if they are still different, that number is the fault and it is the one thing to report back.

Section 1: what changed, file by file, how the peak is found and what "at the same time" means,
and that the build and the app line are green. **Section 3: `ThatPitchIsTheStationsOwn`'s count
before and after, the five cases' text, and the 625 Hz station's text before and after.**
Section 4: anything left, a line each.
