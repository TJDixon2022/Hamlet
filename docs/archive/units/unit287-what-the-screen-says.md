# What the screen says it has, against what the log holds — work instruction 287, task 5

The seventh of these, after units 280 to 286.

---

## The number

**How many of the six mode firsts can be earned today.**

| | |
|---|---:|
| Rows on the card | **6** |
| **Earnable through Hamlet tonight** | **1** — FT8 |
| Recognizable from a record at all | 3 — CW, FT8, Voice |
| Cannot be represented in a record | 2 — FT4, PSK31 |
| Not a contact in the first place | 1 — WSPR |

**One**, and the five reasons are four different reasons. That is why the card
does not draw them all the same way.

---

## The screen against a synthesised two-mode log

Five records, three FT8 and two CW, with `W3YNI` in there twice so that a screen
counting stations rather than contacts would read 4 and be caught. **This machine
has no contact log and never will** (`FACT-006`), so the log is one the test builds.

```
the log holds 5 records
the screen counts 5
the belt reads   5 contacts logged, on the white belt.
the card reads   The first of each mode: 2 of 6.

●  CW     earned              VA3VRR on 2026-08-17, 40m
●  FT8    earned              W3YNI on 2026-08-14, 20m
◌  FT4    waiting on Hamlet   (nothing to show)
◌  PSK31  waiting on Hamlet   (nothing to show)
—  WSPR   not a contact mode  (nothing to show)
◌  Voice  waiting on Hamlet   (nothing to show)

unasked: 3 of these are waiting on Hamlet rather than on you. WSPR is a beacon
rather than a conversation, so there is no contact to log and no first to earn.
Hover any row and it says what stands in the way.
```

**The derived count and the record count agree: 5 and 5.** They are the same
reading, taken once through `ContactLogStore.ReadRecords`, so the belt, the card
and the log window cannot come to disagree about what is in the file.

**And the earned rows name the earliest contact in the mode rather than the first
one read.** The two CW records sit in the file out of date order on purpose:
`VA3VRR` on the 17th is written before `N4L` on the 3rd of September, and a screen
taking the first match would name the wrong station on the row he is proudest of.

---

## Which figures are derived, and from what

**Three kinds of thing are on this screen and they are not equally trustworthy.**

### Read from the records, and nothing else

| On screen | From |
|---|---|
| `VA3VRR`, `W3YNI` | `CALL` |
| `2026-08-17`, `2026-08-14` | `QSO_DATE` and `TIME_ON`, as `StartedUtc` |
| `40m`, `20m` | `BAND` |
| Which rows are earned at all | `MODE`, matched against ADIF's own spellings |

**A field the record does not carry says `not recorded`** rather than being left
blank or filled in with something plausible, which is `ContactLogRow`'s rule and
the same one (§0.0).

### Derived, and here is the derivation

| On screen | Derived from |
|---|---|
| `5` in the ring, and `5 contacts logged` | a count of the records handed in |
| `white belt` | `ContactBelt.For(5)` |
| the progress bar | `ContactBelt.Progress(5)` |
| `5 to go until 10.` | `ContactMilestones`, off the same count |
| `2 of 6` | a count of the earned rows |
| `3 of these are waiting` | a count of the rows in that state |

**Nothing here is remembered.** Every figure is taken from the records at the
moment the window opens, so a log that shrinks shows fewer firsts and a smaller
belt. **A remembered first would go on asserting a contact after the record behind
it had gone**, which is a claim with no evidence under it, and the one thing this
screen must never do.

### Asserted by Hamlet about itself, which is the weakest of the three

**Which unearned state a row is in is not read from anything.** It is a list in
`AchievementsViewModel.StandingOf`, written from what the tree held on 2026-09-08:
FT8 is his to go and do, WSPR is not a contact, and everything else is waiting on
Hamlet.

**It will go stale the day a send path lands**, and it is one method with one list
rather than a flag on six rows precisely so that the day it goes stale there is one
place to correct. **Said out loud here rather than left to be discovered**, because
a sentence on screen that used to be true is the hardest kind of wrong to notice.

---

## The notice, on a first FT8 contact

```
first FT8 contact
W3YNI is your first contact on FT8.
It is on the achievements screen from now on.
```

The ring reads **FT8** rather than a running total, because a mode first is not
counted in anything. Its ink is the card's own earned green, so the notice and the
row it just filled in are the same green and not two greens.

**A second FT8 contact says nothing**, and neither does the same log read again.

**And a first look at a log that already holds modes says nothing at all**, which
is unit 278's seeding rule applied to the second ladder. Watched failing first:
with the seeding branch removed, a first look at a log holding FT8 and CW raised
two notices, for contacts Hamlet was not there for.

---

## What the reading changed about the plan

**Three claims in the work instruction did not survive contact with the tree**, and
all three are reported rather than repaired.

1. **`Tools > My contacts…` is under Radio, not Tools.** It has been since unit 278.
   The achievements item went directly beneath it, because *beside the contact log*
   is the half of the ruling that carries its own reason.
2. **CW has a send path.** HM-DEC-059 built it and `MainWindowViewModel` attaches a
   `CwTransmitter` over a `KeyerCwSender`. What CW lacks is a way into the log.
3. **There is no fourth mode to fire on.** The task-3 test asks for a log holding
   three modes and then a fourth firing; only three of the six can be matched from a
   record at all, so the test seeds with two and fires on the third.

---

## A fact that left a screen

**None.** Nothing was deleted from any surface. Two windows gained content: the
achievements screen is new, and the notice learned a second thing to announce.
