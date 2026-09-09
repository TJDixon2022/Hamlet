# Work instruction 287 - an achievements screen, and the first of each mode

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

---

## THE TWO RULES THAT KILLED SESSIONS

**Tim's rulings of 2026-09-05, HM-DEC-155.**

**1. A unit runs no test suite.** **Only the unit tests it constructs or rewrites in
this work instruction**, filtered by exact name, foregrounded, with a stated
timeout. **An unfiltered `dotnet test` on any project is forbidden.**

**2. Never background a command and poll for it.** The watchdog fires after twelve
minutes with no status write.

`dotnet build` is allowed, foregrounded, with a timeout.

**Tool fact, thirteen units old:** this shell will not carry a quoted heredoc
containing an apostrophe, and it collapses a doubled backslash inside one. Use
script files.

---

## `ADVANCED` and `DRIFT`

Every bench step of this phase is closed; steps D and E are Tim at his radio.
**`ADVANCED: no` by construction.**

```
DRIFT:  9 consecutive units without advance, carried from unit 286.
```

---

## Why this unit exists

**Tim's ruling, 2026-09-08: an achievements screen, off the Tools menu, beside the
contact log.**

**Not Help.** Help is *how do I use this*. **Achievements are his own operating
record**, which is the same kind of thing as his log, and unit 278 already put
`Tools > My contacts...` there.

**And a second kind of achievement.** The belt is one ladder climbed by counting.
**These are a card filled in** - the first contact in each mode - and the empty ones
say what he has not tried yet.

His list: **CW, FT8, FT4, PSK31, WSPR, Voice.**

**Two of those need care and neither is a reason to drop them.**

**Five of the six cannot be earned yet.** Only FT8 can transmit; CW, FT4, PSK31 and
Voice have no send path. **They show as awaiting one, not as failures**, because a
thing he has not been given the means to do is not a thing he has failed to do.

**WSPR is not a contact mode at all.** It is a beacon: he transmits and sees where he
was heard, and nobody works anybody. **A *first WSPR contact* can never fire from a
contact log.** It belongs on the card as something measured differently, or it is
left off with the reason recorded. **Do not quietly make it a QSO.**

```
UNIT GOAL:    Tools > Achievements shows the belt and the first contact in each
              mode, earned ones lit and the rest honest about why not.
ADVANCES:     nothing.
```

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches.
Report them; do not repair the instruction.

- **`Tools > My contacts...`** is unit 278's log window. **`Tools > About`** exists.
  **Find where a third item goes** and follow the menu's own convention.
- **`AdifLog` writes `MODE` on every record** and unit 275 fixed `BAND` to ADIF's
  spelling. **Check what `MODE` actually contains** - the values, not the field -
  before matching on it.
- **Unit 278 built the badge thresholds**; unit 281 made them belt ranks ending gold;
  unit 286 built `BadgeAward` and `BadgeWindow`, which **does not activate, does not
  enter the taskbar, and leaves after eight seconds.** **Reuse that for a mode first;
  do not build a second notice.**
- **Unit 278 seeds the badge level silently on a first look**, so a fresh install
  announces nothing. **A mode first needs the same treatment** or an existing log
  announces six at once.
- **`HowMuchTheApplicationSaysTests` caps every window.** A new window needs a
  ceiling, measured from what it holds, with the margin chosen as unit 282 chose
  its - **against the fault rather than the noise.**
- **`FACT-006`**: this machine has no contact log and never will. **Build against
  synthesised logs.** His real log held 5 records on 2026-09-08 and he reported 8
  contacts on 2026-09-08.
- Root version after unit 286 was **1.12.210**. **Read it, do not assume.**

Known reds, inherited, **never chased**:
`CwAdjudicationTests.ASpeedChangeInRealisticAudio`; the 51 CW cases in
`docs/unit239-failing-set.txt`; the `Ft8Sharp.Deep.Tests` whole-type-list tripwire;
`HM-OPEN-088`'s ten.

---

## Rulings in force

**Tim's, 2026-09-08:**

- **An achievements screen off the Tools menu**, beside the contact log.
- **The first contact in each mode is an achievement**: CW, FT8, FT4, PSK31, WSPR,
  Voice.
- **Text only where he hovers**, and a fault speaks unasked.
- **The count is every logged contact**, not distinct callsigns.

**Standing:**

- **§0.0 and HM-DEC-092.** Never present a guess as a decode. **This unit's exposure
  is an achievement claimed that was not earned**, and a screen of his own record is
  a place he will trust without checking.
- **A log record is a statement the operator made and is not a unit's to revise.**
  **This unit reads and never writes.**
- **§0.6.** Colour is never the only carrier. **Earned and unearned must differ by
  more than a hue.**
- **§0.1.** The engine is not told that tabs exist.
- **One click, one transmission.** **Nothing in this unit transmits**, and nothing on
  an achievements screen may reach a send path.
- **`Ft8Sharp` is a faithful MIT port and nothing changes a line of it.**

---

## Status cadence

After each task, before starting the next, update `PROJECT_STATUS.md` per
`CLAUDE.md` - `STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, and
`NOTE` saying what is moving inside the task. The same every ten minutes while a
task is running. **Use the file-editing tools if the shell refuses.**

---

## Tasks

### Task 1 - what the log can actually tell you about a mode

**Reading only. Build nothing.**

- **What `MODE` holds** on a real-shaped record, and what value each of the six modes
  would write. **ADIF has its own spellings** - report them and cite where you read
  them, as unit 274 cited the specification.
- **Which of the six can be logged at all today**, and which have no send path.
- **What a WSPR record would even look like**, if one could exist. **Say plainly
  whether it can**, and if not, what would have to be measured instead.
- **Do not guess a mode's spelling.** A first that never fires because the string is
  wrong is worse than one that is missing, because it looks earned-able and is not.

### Task 2 - the achievements screen

**This is the goal task.**

- **`Tools > Achievements...`**, beside the contact log, following the menu's
  convention.
- **The belt at the top** - the rank, the count, the progress to the next.
- **The mode firsts below**, as a card. **Earned ones show what was earned**: the
  callsign, the date, the band.
- **Unearned ones say why they are unearned**, and there are two different reasons:
  **not yet done** for a mode he can transmit in, and **awaiting a send path** for
  one he cannot. **Those must not look the same** - the second is not his failure.
- **Earned and unearned differ by more than colour** (§0.6).
- **It reads and never writes.**
- **A new window needs a ceiling** in `HowMuchTheApplicationSaysTests`.

### Task 3 - a mode first announces itself

- **Reuse `BadgeAward` and `BadgeWindow`.** **Do not build a second notice.**
- **It fires once, on the first contact in a mode**, and never again for that mode.
- **It never fires on a first look at an existing log.** Unit 278's seeding rule
  applies to mode firsts too, or **an existing log announces six at once.**
- **It must not cost him a contact**: no focus, no blocked click, no closing an open
  right-click menu. Unit 286 built that and it is not to be weakened.
- Test, watched failing first: a first FT8 contact fires once; a second does not; a
  fresh log holding three modes announces nothing on first look, and then a fourth
  mode does fire.

### Task 4 - WSPR, decided honestly

**Whatever task 1 found, act on it and say which:**

- **If a WSPR record can exist and be recognised**, it is a mode first like the
  others.
- **If it cannot** - and a beacon mode has no contacts - **the card says so in his
  own terms** rather than showing a first that can never be earned. **A row that can
  never light is a promise the application cannot keep.**
- **Do not invent a WSPR achievement** from spots or anything else. **Name what it
  would take and leave it** - that is a ruling and it is Tim's.

### Task 5 - what the screen says it has, against what the log holds

**Named drop candidate.**

- **Run the screen against a synthesised log** holding contacts in two modes and
  report what it shows for all six.
- **Report the count it derives against the number of records**, so the belt and the
  card cannot disagree with the log window.
- **If any figure on this screen is derived rather than read**, say which and from
  what.

### Task 6 - the outcome entry

- **Append this unit's entry to `PHASE_OUTCOME.md`** through
  `tools\arbiter\outcome-append.bat`. **If the shell refuses, append with the
  file-editing tools in the format the existing entries use** and say so.

---

## Parked - do not touch, do not raise

**The whole asks queue**, including whether the ring counts down after a stop, the
three pixels, `HM-OPEN-087`, `HM-OPEN-088`, the hover ring's findability, and whether
a rasterising harness is worth a package. **None of it is this unit's.**

Also parked: **achievements beyond the belt and the mode firsts** - first DX, first
band, first thousand miles, first weak signal. **Named so they are not invented
here.** FT4, the send path, the abort, the composer, the log's fields, Settings.
**Anything in `src/Ft8Sharp/`.**

---

## What not to do

- **Do not write to his log.** Read only.
- **Do not guess a mode's ADIF spelling.** Cite it.
- **Do not show a first that can never be earned** without saying so.
- **Do not make a mode without a send path look like a failure.**
- **Do not build a second notice window.**
- **Do not fire on a first look at an existing log.**
- **Do not invent an achievement this order does not name.**
- **Do not let anything on this screen reach a send path.**
- **Do not distinguish earned from unearned by colour alone.**
- **Do not touch `src/Ft8Sharp/`.**
- **Do not run a test suite.**
- **Do not background a command and poll for it.**
- **Do not report `ADVANCED: yes`.**

---

## Committing and pushing

Commit and push each task before starting the next. Bump the root version's patch by
one. **`Ft8Sharp` does not move.**

---

## Reporting

`output.md` at the repository root, overwritten, four sections per
`CLAUDE_CODE.md` §8.

**NUMBER: how many of the six mode firsts can be earned today, and why the rest
cannot.**

**Section 3 leads with four things:**

1. **The achievements screen against a two-mode log**, showing all six rows and what
   each says.
2. **The ADIF mode strings**, cited.
3. **What WSPR does on the card**, and why.
4. **The notice on a first FT8 contact**, quoted.

**Section 2 says what he will see**: a screen that says what he has done and what he
has not tried.

Write `output.md`, then stop.
