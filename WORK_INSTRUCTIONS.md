# Work instruction 228 - why nothing is decoding, said before the operator has to ask

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project - nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

Carried forward unchanged per `ARBITER.md` section 7.

---

# THIS IS A SEED INSTRUCTION

The launcher was given `--seed`. Iteration 1 executes this file; the arbiter
authors every unit after it. Read `PHASE_PLAN.md` first.

---

# Why this unit exists

**Tomorrow morning the owner tunes to 14.074 and finds out what this decoder can
do. Tonight's job is to make sure that measurement cannot be wrecked by something
that has nothing to do with the decoder.**

The state of the art on FT8, put plainly by someone asked to summarise it:

> When FT8 fails, it is almost never the decoder. It is clock drift, wrong audio
> device, radio in the wrong mode, or transmitting with ALC slamming.

**Hamlet knows all three of those and says none of them in one place.** The clock
is measured and described. The waterfall summary knows whether the source is
simulated. The rig state knows the mode. An operator looking at an empty decoded
table has to assemble that himself from three panels, and the one thing he cannot
tell from a blank table is whether the band is quiet or the setup is wrong.

**An empty panel is indistinguishable from a broken one** (Tim, 2026-08-28). That
ruling is why every panel carries an idle line. This unit applies it to the case
the idle lines cannot cover: **the table is empty and there is a reason.**

```
  PHASE GOAL:   Hamlet hears FT8 off the radio and displays the decoded text
                on screen.
  STEP:         7 of 7 - Hamlet displays decoded FT8.
  UNIT GOAL:    When nothing is decoding and something is wrong, the Digital
                tab says which thing, in the operator's words.
  ADVANCES:     step 7, and the bench check tomorrow.
```

**This is a small unit and it is meant to be.** The band is closed, the owner is
at the desk, and the value is entirely in tomorrow morning not being wasted.

---

# What already exists - use it, do not rebuild it

Read from a harvest taken 2026-09-02. **Verify each against the tree.**

- **`ClockOffsetLine`** - `ClockOffset.Describe(DateTime.UtcNow)`, already a
  sentence in the operator's language.
- **`ClockIsConcerning`** - `ClockOffset.IsConcerning`, already the amber test.
- **`DigitalWaterfallSummary`** - already says `no slot grid until the clock is
  checked` when the clock is unknown, and appends `simulated` when
  `DigitalSpectrum.IsSimulated`.
- **`DigitalSpectrum.IsSimulated`** - true when the training radio is the source
  rather than the rig.
- **`RigState`** - carries the radio's mode. `DataMode` distinguishes USB from
  USB-D and is UNKNOWN when unconfirmed, per HM-DEC-056.
- **`DigitalDecodes`** - the bound collection, empty until something decodes.
- **`DigitalIdleText`** - four idle strings, the owner's voice, **not deleted.**

---

# Tasks

## Task 1 - the ground

`Ft8Sharp` totals, attribution, channel tests. Then confirm the six things above
exist and are shaped as described. **Report every mismatch.**

## Task 2 - one line that says why

A single readiness line on the Digital tab, above or beside the decoded table,
that is **empty and invisible when everything is right** and says the first thing
that is wrong when something is.

The order matters, because the first wrong thing makes the rest moot:

1. **Nothing is listening.** No audio source at all.
2. **The source is simulated.** The training radio, not the rig. Real FT8 will
   never appear and the operator should know before he waits.
3. **The clock is out.** `ClockIsConcerning`. FT8 needs the PC within about a
   second of UTC or nothing decodes and it fails silently. **This is the single
   commonest newcomer failure in this mode** and the one the operator is least
   likely to suspect.
4. **The radio is not in a data mode.** USB-D is what FT8 is worked in. If
   `DataMode` is UNKNOWN, say unknown rather than wrong - an unconfirmed read is
   not a fault (HM-DEC-056).
5. **The dial is not in a digital neighborhood.** The map already knows.

**Write it in the CW terminal's voice**, the way `DigitalIdleText` is: the reason
attached to the fact, and a way forward rather than a bare absence. Not
`ERROR: CLOCK OFFSET`. Closer to *the PC clock is four seconds off UTC, and FT8
needs it within about one - nothing will decode until that is fixed.*

**When all five are right and the table is still empty, say nothing.** A quiet
band is not a fault and the idle line already covers it.

## Task 3 - the tests

Each of the five conditions, asserted at the view-model seam without opening a
window. And the control that matters: **all five right produces no line.** A
readiness line that always says something is one the operator stops reading.

## Task 4 - the mode strip, if the window allows - DROP CANDIDATE

**Named as the drop candidate. Dropped whole, and say so.**

The mode strip lights the digital mode the dial is sitting in. If it is still
static, wire it to the neighborhood the map already resolves. If it is already
live, say so and drop this.

---

# What not to do

- **Do not delete `DigitalIdleText`.** It is the owner's voice.
- **Do not decide what a message means in plain English.** `CLAUDE.md` 12.1. The
  "What people are saying" panel is not this unit's.
- **Do not put a number in the `snr` column.** Unit 226 refused it for a reason
  and what goes there is the owner's under 12.1.
- **Do not correct the clock.** Hamlet measures and never sets it.
- **Do not touch the decoder, any threshold, or `Ft8Sharp`.**
- **Do not run the full Hamlet suite.**
- **No transmit work.** `CLAUDE.md` 0.2.

---

# Committing, pushing, reporting

Commit and push each task. Take the version bumps.

`output.md` per `CLAUDE_CODE.md` section 8. **Exactly four top-level sections at
`##` level:**

```
## 1. What Claude did
## 2. What the owner should expect
## 3. What you should see
## 4. What's blocking us
```

Then run `tools\arbiter\validate-output.bat output.md` and fix until it exits 0.
**If it is refused, say so and hand-check the six rules against the script's own
body, as units 224 through 227 did.**

**Section 3 leads with what the line says on this machine right now**, with the
band closed and whatever the audio source currently is.

Then stop.

---

```
ARBITER-DECISION
STEP: 7
APPROACH: one readiness line on the Digital tab that names the first thing standing between the operator and a decode - no source, simulated source, clock out, wrong mode, wrong neighborhood - and says nothing at all when none of them is wrong
MOVE: continue
WHY: The owner tunes to 14.074 tomorrow morning to establish the baseline this whole phase exists to produce, and the state of the art on FT8 is that failures are almost never the decoder - they are clock drift, the wrong audio device, or the radio in the wrong mode. Hamlet already knows all three and says none of them in one place, so an operator looking at an empty table cannot tell a quiet band from a wrong setup. That is the case the idle lines cannot cover, and Tim's own 2026-08-28 ruling that an empty panel is indistinguishable from a broken one is what this applies. It is deliberately a small unit: the band is closed tonight and the value is entirely in tomorrow morning not being wasted on something that has nothing to do with the decoder.
STATE: partial
DECIDED: That the line is ordered and shows only the first wrong thing, because the first one makes the rest moot - a simulated source means the clock does not matter yet. That silence is the correct output when everything is right, and that this is asserted by a test, because a readiness line that always says something is one the operator stops reading. That an UNKNOWN data mode is reported as unknown rather than as wrong, per HM-DEC-056, since an unconfirmed read is not a fault. That the wording follows DigitalIdleText's voice rather than an error format - the reason attached to the fact and a way forward, which is 0.7 and HM-DEC-034. That nothing in the decoder, the thresholds or Ft8Sharp is touched, because tomorrow's measurement is worthless if tonight moved the thing being measured. That the snr column stays a dash, because unit 226 refused to print a Costas sync score under a decibel heading and what goes there is the owner's under 12.1. That the mode strip is the drop candidate because it is cosmetic beside the readiness line.
LICENCE: PHASE_PLAN.md's step 7; the plan's 2026-09-02 ruling that steps 6 and 7 do not depend on each other, which lets step 7 move while step 6 waits on an owner ruling; the plan's 2026-09-01 ruling on when a step is done; HM-DEC-021, each panel carrying its own news, extended here to the case no panel covers; HM-DEC-034 and 0.7 for the voice; HM-DEC-056 for reporting an unconfirmed mode as unknown; CLAUDE.md 12.1, which keeps the plain-English panel and the snr column out of this unit; CLAUDE.md 0.2. Reported plainly: this instruction was written from a harvest of src/Hamlet.App taken 2026-09-02 and every claim about ClockOffsetLine, ClockIsConcerning, DigitalWaterfallSummary, IsSimulated, RigState and DigitalDecodes is to be checked rather than trusted.
ACCOMPLISHED: Tomorrow morning the owner tunes to 14.074 and either sees callsigns or does not. If he does not, he currently has no way to tell whether the band is quiet, the clock drifted, the radio is in plain USB, or Hamlet is listening to its own training radio - and finding that out by elimination is how a morning gets spent. After tonight the tab tells him which one, in a sentence, in the voice he wrote for it in August, and says nothing at all when the answer is simply that nobody is transmitting. It is the smallest possible change that protects the measurement this entire phase was built to take.
ADVANCES: Step 7, and the bench check. The step is not expected to close in this unit; the arbiter continues.
END-ARBITER-DECISION
```
