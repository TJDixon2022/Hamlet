# Work instruction 284 - split the admissions from the advice

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

**1. A unit runs no test suite.** **Only the unit test it constructs or rewrites in
this work instruction**, filtered by exact name, foregrounded, with a stated
timeout. **An unfiltered `dotnet test` on any project is forbidden.**

**2. Never background a command and poll for it.** The watchdog fires after twelve
minutes with no status write.

`dotnet build` is allowed, foregrounded, with a timeout.

**Tool fact, ten units old:** this shell will not carry a quoted heredoc containing
an apostrophe, and it collapses a doubled backslash inside one. Use script files.

---

## `ADVANCED` and `DRIFT`

Every bench step of this phase is closed; steps D and E are Tim at his radio.
**`ADVANCED: no` by construction.**

```
DRIFT:  6 consecutive units without advance, carried from unit 283.
```

---

## This unit does one thing

**Five work instructions have aimed at this paragraph and every one missed.**

The reason is the author's and it is the same every time: **each order told the
session where the text was, and each location was wrong.** The status bar. Then
`ReceiveAdvice`. Then one call site. Then one path per mode. Then the top strip.
Each session fixed exactly what it was pointed at, correctly, and the paragraph
stayed on the operator's screen.

**So this order names no file, no line, no class and no surface.** It gives a
reproduction and asks for one change.

**What changed today: it reproduces on the development machine.** Against the
simulated radio, Digital tab, FT8, the bar renders this - his screenshot,
2026-09-08:

```
I could not read the noise blanker, so I have not touched it. I could not read
the noise reduction, so I have not touched it. I could not read the auto notch,
so I have not touched it. The AGC usually wants to be slow here, because dozens
of stations transmit together here and the gain would ride up and down under the
loudest of them, and that is not settled well enough for me to change it on your
radio. Your scope span wants to be 3 kHz across, because a scope showing a couple
of hundred k...
```

**Read what that is.** The first three clauses are **admissions** - Hamlet could
not read a setting and says so. **Those are faults and they belong on the bar
unhovered.** Unit 282 built them deliberately and they are working.

**Everything after them is advice**, and it is **in the same string**. That is why
no sweep could move it: moving the advice would have taken the admissions with it,
and clearing the line would have hidden a fault.

**The change is to split the two.** One value for what Hamlet could not do, one for
what it suggests. The first goes on the bar. The second goes on the hover.

```
UNIT GOAL:    The bar shows only what Hamlet could not do. The advice is on the
              hover. Nothing is deleted.
ADVANCES:     nothing.
```

---

## Tasks

### Task 1 - stand it up and read what it says

**Do not search the source for the sentence.** Five orders have done that and it is
composed at runtime from fragments, so a search returns empty and reads as clean.

- **Stand the application up against the simulated radio, on the Digital tab, on
  FT8**, in the state the screenshot shows.
- **Print what the bar actually renders**, verbatim.
- **Then follow that string back to the code that built it**, and report the path
  with file and line - **found by following the running value, not by grepping.**
- **Report every clause and which kind it is**: an admission, or advice.

**If the reproduction does not appear, say so and stop.** Do not fix a paragraph you
cannot see; that is what the last five units were asked to do.

### Task 2 - split them

- **Two values where there is one.** What Hamlet could not read or could not reach
  is one; what it suggests is the other.
- **The admissions go on the bar, unhovered.** They are faults and unit 282's
  `Admissions` already decides which clauses those are. **Use it rather than
  writing a second rule** - two rules disagree eventually.
- **The advice goes on the hover.** Every word of it, nothing deleted.
- **Split at the source of the composition**, not by cutting the finished string at
  a caller. A caller that slices a sentence is the same fault one layer up.

### Task 3 - a test that stands it up the same way

**The test is the point. Five orders' worth of tests all passed while the paragraph
was on his screen.**

- **Stand the application up in the state task 1 reproduced** - simulated radio,
  Digital tab, FT8, the settings unreadable - and **assert what the bar renders.**
- **Assert the admissions are present and the advice is absent**, by phrase.
- **Assert the advice is reachable on the hover**, so a moved sentence is not a
  deleted one.
- **Watched failing first**, against the tree as it stands, so the red is the
  paragraph the operator is looking at.
- **A headless fixture that cannot enter this state is not a test of it.** If the
  harness will not reproduce it, **say so plainly and say what would.**

### Task 4 - what moved

- **Every clause, and where it went.** Appended to the removal log units 280 to 283
  keep.
- **If any fact left the screen without arriving on the hover, say so under its own
  heading.**
- **Measure the Digital tab before and after** in the reproduced state, not the
  idle one.

---

## What not to do

- **Do not search source for the sentence.** Follow the running value.
- **Do not delete a clause.** Move it.
- **Do not move an admission to the hover.** Faults speak unasked.
- **Do not write a second rule for which clause is which.** `Admissions` decides.
- **Do not slice the finished string at a caller.**
- **Do not fix anything else.** Not a ceiling, not another surface, not a sweep.
  **Five units have been widened past their subject and this one is not.**
- **Do not touch `src/Ft8Sharp/`.**
- **Do not run a test suite.** Only the test you write here.
- **Do not background a command and poll for it.**
- **Do not report `ADVANCED: yes`.**

---

## Parked

Everything on the asks queue, `HM-OPEN-087`, `HM-OPEN-088`, the ceilings, Settings,
`AboutWindow`, the hover ring's findability, the log, the send path, FT4.
**None of it is this unit's.**

---

## Committing and pushing

Commit and push each task before starting the next. Bump the root version's patch by
one. **`Ft8Sharp` does not move.**

---

## Reporting

`output.md` at the repository root, overwritten, four sections per
`CLAUDE_CODE.md` §8.

**NUMBER: the bar's rendered characters in the reproduced state, before and after.**

**Section 3 leads with three things:**

1. **The bar as it renders now**, verbatim, in the state the screenshot shows.
2. **The hover**, verbatim, showing every advice clause arrived.
3. **The test's red**, quoted, from before the split.

**Section 2 says what he will see**: the bar says what Hamlet could not do, and
nothing else.

Write `output.md`, then stop.
