# 1. What Claude did

**STATE.** Development computer, `C:\Source\HamLet`, Claude Code surface (§9.5).
**Branch: `main`, and nowhere else** (§9.5.1). The prompt claimed `PROJECT: Hamlet`
and the tree confirms it: `CLAUDE.md`'s header reads `Project: Hamlet`, the
solution is `Hamlet.sln`, the remote is `TJDixon2022/Hamlet`. Gate passed.
**Nothing in this report is evidence about the radio** (HM-DEC-093) — no code
changed, no tests ran, and nothing was connected.

**Nothing was recorded under §12.1.** One question is in section 4.

## §13.4 — the hazard line

Added, one line, ruling nothing that §0.2 and §0.2.1 do not already rule:

> **HAZARD: Hamlet keys an Icom IC-7300 HF transmitter via CI-V, including an
> unattended repeating cycle — dummy load only until §0.2's first sentence is
> amended by separate ruling (HM-DEC-098, HM-DEC-008) — and it commands the
> operator's VFO under §0.2.1.**

It is in `CLAUDE.md` because that file is read automatically by every session and
the annunciator's card is five standing lines with no room for it. A session that
learned where the repository is without learning that it can put power on somebody
else's frequency has learned the wrong half.

## The ratings — §4 already stands alone, so they are not copied

**The condition in the work order was tested rather than assumed.** §4 carries,
each page-cited to Full Manual `A7292-4EX-6`: command `17` at 30 characters
(19-11), CW pitch 300–900 Hz (4-14), radio address `94h` (12-8), the IF filter
index 50 Hz to 3.6 kHz (19-4, 4-6), CI-V baud (12-9). Those stand alone, and a
second copy is a second thing to drift (§0) — this table already spanned three
printings once and that seam produced two defects (HM-DEC-071).

So §13.4 points at §4 rather than restating it.

## The one thing §4 does not carry

**`100 W` appears nowhere in `CLAUDE.md`.** §4's header calls the radio an
"HF/50 MHz transceiver" and names no wattage; no row cites a page for one. The
figure is genuinely absent rather than merely uncited.

**There is a good reason.** HM-DEC-074 and HM-DEC-082 both rule that Hamlet reports
power as a percentage of the radio's own range and never as a wattage, because it
cannot know what a percentage means in watts at this frequency into this load. No
part of the application has ever needed the number, and §4 records what the
application needs.

**So §13.4 says the hazard without the figure, and the gap is marked rather than
filled** (HM-OPEN-038). The wording you gave last session for `PROJECT_CARD.md`
carried `(100 W)`; that was your assertion and it is committed as yours. Carrying
it forward into §4's neighbourhood would have made it look page-cited when it is
not, and **a hazard line is the worst place in this file to introduce its first
uncited number** (§12.4).

## Recorded

`HM-DEC-133` at the true head of §1. `git diff --stat` on `CLAUDE.md`: **26
insertions, 0 deletions** — no existing row edited. §1's head is now
133, 132, 131, 128, 130, 129: rows four to six are still out of order below the new
rows, which is HM-OPEN-036 and remains uncorrected on purpose.

`PROJECT_CARD.md` untouched, still five lines. `PROJECT_STATUS.md` written twice
per §13.2 — `EXECUTING` when work started, `COMPLETED` at the end — and at no other
time.

# 2. What Tim should expect

- **Build succeeds, no warnings.** No code changed.
- **Tests not run this session.** The suite stands where the last run left it:
  1902 tests, 2 failing, both the recorded baseline.
- **`CLAUDE.md` is 26 lines longer and nothing in it was edited.** The new §13.4
  sits under the annunciator section, not under §0.2, because §0.2 already rules
  this and §13.4 only restates it where a session will meet it.
- **`PROJECT_CARD.md` still has no `HAZARD` line**, as instructed. Anything reading
  only the card still learns nothing about the transmitter; §13.4 is where that
  now lives, and `CLAUDE.md` is read automatically while the card is read by the
  panel.
- **One new open item, severity none.** 26 open in `OPEN_ISSUES.md`.
- **One commit, pushed to `main`.** Nothing local, no branches.

# 3. What we should do next

- The dummy-load evening, per `BENCH_CARD.md`. Nothing in the transmit path should
  move before it, and §13.4 now says so to every session that opens the file.
- HM-OPEN-038, whenever the manual is open for another reason — it is one cited row
  in §4 and then one word in §13.4.
- Circulate the annunciator to `MURC`, `CoreHMI` and `SIMULATOR`. Each needs the
  file in its tree first, its own §13, and its own ruling id.
- HM-OPEN-036, as one deliberate move.

# 4. What's blocking us

Nothing is blocked by this session's work. One question it raised, and two standing
ones unchanged.

---
date: 2026-08-18
refs: CLAUDE.md §4, §12.4; HM-DEC-074; HM-DEC-082; HM-DEC-133; HM-OPEN-038
---

**The transmitter's power output is cited into §4, or the hazard line stays as it
is.**

`100 W` is nowhere in `CLAUDE.md` and no page in §4 cites it. The application has
never needed it, because HM-DEC-074 and HM-DEC-082 have Hamlet report power as a
percentage of the radio's own range and never as a wattage — it cannot know what a
percentage means in watts at this frequency into this load.

What changed is the audience. §0.2 is about what this software can do to the
physical world, and **"keys a transmitter" and "keys a hundred-watt transmitter"
are different warnings** to somebody who has just arrived at the repository. The
line currently says the first.

Two ways, and the choice is yours:

- **Cite it.** One row in §4 naming the page in `A7292-4EX-6` that states the RF
  output, after which §13.4 can name the figure and everything stays inside §4's
  discipline. It needs the manual open, which is why it is not done here.
- **Leave it.** The hazard is correctly stated without a number, and the number
  would be decoration rather than information for every reader who is not standing
  at the radio.

Rejected: writing `100 W` in from general knowledge. §4's whole discipline is that
a figure arrives with the page it was read from, and the row that broke it would be
the one warning somebody about a transmitter.

---

The two standing questions are unchanged and still yours: **whether an attended
automatic cycle may reach an antenna** (§0.2, HM-DEC-098), awaiting the interlocks
watched into the load; and **a callsign too long for one keyer send** (HM-DEC-130),
refused until the seam between two sends is measured into the load.
