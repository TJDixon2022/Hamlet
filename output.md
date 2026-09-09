# Work instruction 298 — the achievements screen grows as he operates

**READ IN THIS ORDER.** The screen is the deliverable, so section 3 is what he will
see.

A. **The phase goal — FT4 works exactly the way FT8 does.** This unit advanced no
   step of it. It rebuilt the achievements screen, which is what step 3's fourth
   criterion is written about, and that criterion now means something different.

B. **Step 3 and its exit criteria** — `SUBMODE` carried and round-tripping; an FT4
   contact logging as `MODE=MFSK, SUBMODE=FT4`; a record with no submode still
   round-tripping; and **the achievements screen's FT4 row lighting, with unit 287's
   four states still reading correctly.** The first three were not touched. The
   fourth is the one to read: the four states are still computed and the mode-first
   badge still fires off them, but the window no longer draws six rows, so what
   lights for FT4 is a mode tab that appears the moment an FT4 contact is logged.
   Step 3 stays `done`.

C. **The report last, and section 4 raises 4 items.** None blocks the next unit.
   Two want a ruling, one is a defect in the work-instruction template that has now
   bitten twice, and one is a limitation named rather than repaired.

```
UNIT:       298 — complete at task 8 of 8, none dropped — 2026-09-09 17:28
PHASE GOAL: FT4 works exactly the way FT8 does.
UNIT GOAL:  The achievements screen shows only what he has opened, every unlock
            reveals more than it fills, and a handful of named challenges stay
            visible because their purpose is to be a target.
ADVANCED:   no — no phase step moved. Step 3 stays done and none of its criteria
            changed value; this unit re-cut the screen its fourth is about.
NUMBER:     how many cards a fresh log shows, and how many one 40 m contact reveals
            A FRESH LOG:      0 record cards, 8 standing targets
            ONE 40 m CONTACT: +1 tab, +1 continent, +7 records (20 -> 27)
            and every one of the seven is filled rather than blank.
DRIFT:      2 consecutive units without advance  (was 1, carried from unit 297)
```

---

## 1. What Claude did

**Complete. Eight tasks of eight, none dropped, including the named drop candidate.**
Development machine, prompt claimed `PROJECT: Hamlet`, branch `main`, nine commits,
all pushed. Root version 1.12.260 to **1.12.261**, bumped once. **No file under
`src/Ft8Sharp/` was touched**, and nothing here reaches a send path.

**`ACHIEVEMENTS_PHILOSOPHY.md` was read in full before task 1**, as the instruction
requires, and its §2 governed every decision below: *a wall of empty cards reads as
failure to somebody who has felt like a failure at this hobby for years.*

The gate held: `SHACK_FACTS.md` present, `CwProbabilisticDecoder.cs` present, no
`CoreHMI.sln`, no `MURC.sln`. This unit was numbered **298** from `PHASE_OUTCOME.md`;
the last entry there was 297.

**No test suite was run.** Twenty-five tests were constructed in this instruction
across four classes and run filtered by exact name, foregrounded, with a 500-second
timeout: **25 of 25 green.** Every `dotnet build` was foregrounded. Nothing was
backgrounded and nothing was polled.

**Task 1 — reading only**, in `docs/unit298-achievements.md`. Nine of the eleven
cards the instruction lists are computable with file and line; two are not.

**Task 2 — the unlock model, and a continent that is cited.** The finding that
decided the shape of the night is in section 3.

**Task 3 — records sliced.** How far, how faint, when; tabbed Everything, then each
band, then each mode.

**Task 4 — continents and regions.** Three regions ship, one was refused by its own
generator.

**Task 5 — eight standing challenges**, each with a hover that teaches.

**Task 6 — the reveal**, through unit 286's window, never on a first look.

**Task 7 — the trace**, done by subtraction, and the screen wired to the window.

**Task 8 — the outcome entry**, filed against step 3.

**Four decisions made for themselves, each reported.**

1. **The six mode-first rows came off the window.** §3.1 forbids a card for something
   he has not opened and the instruction says the `1 of 6` style must not survive.
   **But the fact underneath four of them is kept**, as one line: *Hamlet cannot work
   CW, FT4, PSK31 and Voice yet, so nothing here is waiting on you for those.* That
   is the application owning up rather than four things he has not got round to, and
   deleting it would have been a regression dressed as a tidy-up.
2. **The Mediterranean is refused by its own generator rather than filed under one of
   its two continents.** Its members span Asia and Europe — Cyprus is `AS` in the
   ARRL's own column — and a region is drawn inside a continent. It is in the data
   file's `refused` list with its reason, and `HamletRegions.Refused` reads it back,
   so the omission is visible rather than silent.
3. **Sub-region membership is editorial, and it is declared as such in three places**
   — the data file, the generator's header, and on every group on screen. Every
   entity name and every prefix is verified against a cited table, so nothing
   invented can get in.
4. **The screen was wired into the window**, which the instruction does not name as a
   task. A screen nobody can open is HM-DEC-154's own finding in a smaller place: *a
   measured gain nobody can see is indistinguishable from none.*

**Three mismatches against the instruction, reported and not repaired.**

1. **Contact duration is not absent.** The instruction says *transmit power and
   contact duration are likely absent — name them and leave them.* `TIME_OFF` is
   written (`AdifLog.cs:268`) and read back (`:479`), so the span of an exchange is
   derivable. **Transmit power genuinely is absent**, and by decision rather than
   oversight: HM-DEC-074 and HM-DEC-082 have Hamlet report power as a percentage and
   never as a wattage, so there is no wattage anywhere in the application to log.
2. **`WORK_INSTRUCTIONS.md`'s heading carries no unit number, for the second unit
   running.** `outcome-append.bat` resolves the number from that heading — unit 266's
   repair, and it is right — so it fell back and filed this entry as 296. The entry
   body already carried `UNIT_AS_CALLED: 298`; the heading was corrected in place, as
   unit 297 did. **The instruction was not repaired.** Two in a row is worth a line in
   the next order rather than a third correction, and it is section 4's third item.
3. **Unit 252's DXCC table carries no continent**, which the instruction asks about
   directly. It does now — see section 3.

## 2. What the owner should expect

**A screen that grows as he operates and always has a next thing on it.** Open it on
a fresh log and there are no records at all — not dimmed, not dashed, absent — and
eight things to go and try, each with a sentence explaining why it is worth trying.
Log a contact on a band he has not worked and a tab appears with the records his own
log can already fill, and a notice says so once.

**What will look wrong and is not.**

- **The six mode-first rows are gone.** They are replaced by mode tabs that appear
  when he works a mode, and by one line saying which modes Hamlet cannot work yet.
- **The header says `4 views open, 27 records in them` and never `27 of 90`.** There
  is no total on this screen, deliberately, and a sweep asserts it.
- **A continent card says `2 of 45 worked`.** That denominator is what Hamlet can
  *recognise* — 303 entities out of the publication's 340 — rather than DXCC's own
  total. It is a smaller claim than the truth and the honest direction to be wrong in.
- **The Mediterranean is not there.** Refused with its reason; see section 4.
- **A challenge that is not earned is drawn a little quieter but is not greyed.** It
  is a target, not a disabled control.
- **Nothing on this screen says `confirmed`**, and a sweep asserts it. DXCC is counted
  by confirmations and Hamlet has none of them.
- **The suite was not run and its state is unknown to this unit.** The four inherited
  reds were not chased and not looked at.

## 3. What you should see

### 1. A fresh log's screen, quoted whole

Printed by the headless window test, so this is what the window actually drew:

```
0
0 contacts logged, on the white belt.
10 to go until 10.
Nothing here yet. The first contact you log opens the first of these, and every
one after that opens more.
Hamlet cannot work CW, FT4, PSK31 and Voice yet, so nothing here is waiting on
you for those.

Things to go and try
  First past 500 miles
    No contact has carried a grid square yet, so there is nothing to measure this against.
  First report below -10 dB
    No contact has carried a report yet, so there is nothing to measure this against.
  A contact on 80 m
    No band worked yet.
  A contact on 80 m or 40 m after dark
    Nothing on 80 m or 40 m after dark yet.
  A contact on the grey line
    No contact yet inside the hour either side of your sunrise or sunset.
  A contact on a new continent
    No contact has resolved to a continent yet.
  5 grid squares worked
    No contact has carried a grid square yet.
  5 contacts in one day
    No contact carries a date yet.
```

**Nothing unearned is on it, and that is asserted rather than eyeballed.** The test
sweeps for `Furthest`, `Faintest` and `Busiest` and finds none, and separately
asserts the targets *are* there — because the failure mode is a tidy-up that removes
both halves.

### 2. The same screen after one 40 m contact

Three FT8 contacts on 20 m give **3 tabs and 20 cards**. One contact with Ireland on
40 m gives **4 tabs and 27 cards**:

```
4 views open, 27 records in them.

=== 40 m ===  6 records
  How far
    Furthest on 40 m : 3,400 miles   EI4GNB · Ireland · 3,400 miles
  How faint
    Faintest you have been heard on 40 m : -9 dB    EI4GNB · Ireland · 3,400 miles
    Faintest you have heard on 40 m : -14 dB        EI4GNB · Ireland · 3,400 miles
  When
    First on 40 m : 10 September 2026                EI4GNB · Ireland · 3,400 miles
    Busiest day on 40 m : 1 contact                  10 September 2026
    Busiest hour on 40 m : 02:00 UTC                 1 contact

=== Europe ===  1 of 63 worked
    Ireland : 1 contact   EI4GNB · Ireland · 3,400 miles

opened : band-40m, continent-EU
```

**And it says so once**, through unit 286's window and not a second one:

> **40 m is open** — That opens 40 m, with 6 new records in it.
> **Europe is open** — That opens Europe, with one new record in it.

**Never on a first look.** A log of fourteen imported contacts announces **nothing**;
it writes down what it saw and stays quiet, so the next contact is news. Watched
failing: replacing the silent-seed branch gives **4 notices** on a log the operator
has never seen Hamlet read.

**A region appears only once he has worked something in it**, and names what to look
for next:

```
Central America  1 of 7 worked
  (Hamlet's own grouping, not an official DXCC category)
  -> look for V3 HU TD HQ
```

Those four prefixes were **read out of the cited table**, not typed.

### 3. The visible challenges, and one hover in full

The eight are quoted above. Here is *A contact on 80 m or 40 m after dark*, whole:

> **Daylight thickens a layer of the atmosphere that soaks up low frequencies, and
> after dark it thins.** That is the whole reason 40 m is a short, noisy band in the
> afternoon and a long-distance band at midnight, and why the people you hear on it
> at those two hours are completely different. It is the single easiest piece of
> propagation to go and prove to yourself.

**That sentence is §3.5's own claim about what the product is**, and the card is the
reason he reads it. A sweep asserts every card's hover is a paragraph rather than a
label and is never merely its own title.

**And the grey line challenge is computed, not decorative**: it looks for a contact
inside an hour either side of *his* sunrise or sunset, from his own coordinates,
using the same figure the sentence claims.

### 4. Task 7's trace, and what failed a §5 question

**§5 question 1 is proved by subtraction rather than asserted.** Take a field out of
the log and watch the cards that claim it disappear:

| Field removed | Cards that go |
|---|---|
| `GRIDSQUARE` | Furthest so far, on 20 m, on 40 m, on FT8 |
| `RST_RCVD` | Faintest you have been heard — all four scopes |
| `RST_SENT` | Faintest you have heard — all four scopes |
| `QSO_DATE` | First, Busiest day and Busiest hour — all twelve |

**A log with a callsign, a band and a mode and nothing else claims no figure at all.**

**§5 question 4 is swept**: every card on the screen carries a hover of more than 150
characters that is not its own title.

**§5 question 3, the half that can be asserted**: no string the screen produces
contains *failed*, *failure*, *streak*, *you have not operated*, *you have only*, *you
did not* or *behind*. **The sweep corrected itself once** — the bare word *only* is
not a shame marker, because the faint-signal hover reads *a person would hear only
hiss*, which is teaching.

**One card failed a §5 question and does not ship.** *The Mediterranean* fails
question 1 in an unusual way: its members are computable, but **the group cannot be
placed**, because §3.1 draws a region inside a continent and this one spans two.
Refusing it is the generator's own doing and the reason is in the data file.

**§5 questions 2 and 3 are judgements and here is the card-by-card answer.**

| Card | Q2 — worth trying? | Q3 — invitation or failure? |
|---|---|---|
| Distance ladder | Yes. It is the fact that HF distance is about the hour and the band, not power | Invitation. It shows his own best beside it |
| Faint ladder | Yes. It teaches that a minus number is ordinary | Invitation |
| A new band | Yes. §3.5's own example | Invitation, and it names **one** band rather than the six he has not worked |
| Low band after dark | Yes. The easiest propagation to prove to yourself | Invitation |
| Grey line | Yes. Something he would not know to try | Invitation |
| A new continent | Yes | Invitation |
| Grid squares | Yes. It maps what his station actually covers | Invitation |
| A busy day | Yes. It teaches what a band does while you watch it | Invitation. It says contests are the easiest place and that he need not enter one |
| Record cards | Not applicable — they are *look what you collected* | Never shown unearned, so the question does not arise |
| Continent and country cards | Not applicable | Never shown unearned |

**No record card can fail question 3 by construction**, because a record that cannot
be filled is not built.

## 4. What's blocking us

Nothing blocks the next unit. Four items, in the order they matter.

---

**The DXCC continent column is now in the tree, transcribed from the cited
publication and cross-checked — and whether that download counts as a transcription
is Tim's to confirm.**

Task 1 found `data/callsigns/dxcc-prefixes.json` carried no continent and that the
string does not occur in it. **Task 4 cannot exist without one.** The cited ARRL DXCC
List does carry a Continent column, and the file downloaded from the URL in the
source note **agrees with that note on both the document date (January 2026) and the
stated entity total (340)**.

So it was transcribed the way unit 252 transcribed the prefixes:
`tools/dxcc/transcribe-continents.py` reads the column and **cross-checks every row
against `arrl-dxcc-current.txt`**, which unit 252 transcribed from the same
publication and committed. **282 rows join on prefix and entity name, 23 more on a
prefix that is unique in the publication, 1 does not join and is dropped, and 2 that
carry two continents — Maldives `AS,AF` and Republic of Turkiye `EU,AS` — are
declined rather than having one chosen.** 303 entities carry a continent and the
counts are written into the data file.

What was rejected. **Writing 275 entity-to-continent pairs from a model's memory**,
which is exactly what unit 252 built the expander to prevent and is the one thing
nobody would ever check — a continent card is not a figure anybody verifies.
**Abandoning task 4**, which was the alternative before the column was found, and
which would have cost the whole of §3.2's own worked example.

**What wants confirming**: that fetching the cited URL and machine-checking it
against an existing committed transcription is *transcription* rather than *a second
source*. The reasoning is that both files are the same publication, the cross-check is
the evidence, and every disagreement is printed. **If that is not what he wants, the
table comes out and task 4 with it**, and that is a one-line change to the csproj.

---

**The Mediterranean spans two continents and is refused. Either it loses a member, or
regions stop living inside continents.**

Its members are Croatia, Cyprus, Greece, Italy, Malta, Portugal and Spain. **Cyprus is
`AS` in the ARRL's own column**, so the region spans Asia and Europe, and the screen
draws a region inside a continent — filing it under either would put it in the wrong
place. The generator refuses it, records the reason in the data file, and
`HamletRegions.Refused` reads that back.

What was rejected. **Dropping Cyprus on this unit's own authority**, which is an
editorial call about somebody else's country made to make a layout work.
**Filing it under Europe anyway**, which is the misplacement the check exists to
catch. **Showing it at the top level rather than inside a continent**, which is a
layout change that would want its own look.

What would settle it. Either a ruling that Cyprus comes out of Hamlet's Mediterranean,
or a ruling that a spanning region is drawn above the continents rather than inside
one. **Three regions ship meanwhile** and the fourth is one line in
`tools/dxcc/hamlet-subregions.txt` away.

---

**A work instruction's heading should carry its unit number, because the arbiter's own
script reads it from there.**

`outcome-append.bat` resolves the unit number from `WORK_INSTRUCTIONS.md`'s heading
rather than from the argument it is given. That is unit 266's repair and it is right:
two callers were passing different numbers and the heading is the tie-break.

**But the last two orders have carried no number in their headings** — `# Work
instruction - the conversation becomes cards` and `# Work instruction - the
achievements screen grows as he operates` — and instead tell the session to take the
next number from `PHASE_OUTCOME.md`. **The script cannot read that instruction**, so
it falls back and files the entry under the previous unit's number, and both units
have corrected the heading by hand afterwards.

What was rejected. **Changing the script to prefer its argument**, which would undo
unit 266's repair and reintroduce the double-entry it fixed. **Repairing the
instruction**, which its own *report mismatches, do not repair* rule forbids.

What would settle it: the order's heading reads `# Work instruction 299 — ...`, or the
numbering line tells the session to write the number into the heading before task 1.

---

**The screen still cannot name a US state, and that closes a whole family of
achievements the philosophy explicitly lists.**

`ACHIEVEMENTS_PHILOSOPHY.md` §3.8 names **US states** among the eight things 150 to
250 achievements are to be built from. Hamlet cannot compute one. `DxccPrefixes` names
a DXCC entity, which for all fifty states is *United States of America*; a US call
area is historical rather than a residence, so `W6` in Ohio is ordinary; and a
four-character grid square is a box about seventy miles across that straddles state
lines. **This is unit 297's finding again, and it now blocks a named part of the
plan.**

What was rejected. **Deriving a state from the callsign prefix** and **deriving one
from the grid**, both for the reasons above. **Writing a grid-to-state table**, which
is the memory-transcription this unit spent its second task avoiding.

What would settle it. The parked callook lookup returns a licensee's address, which is
cited data with a state in it — so **the Worked All States family waits on that
instruction rather than on a ruling**, and it is named here so the next author does
not plan it before the data exists.
