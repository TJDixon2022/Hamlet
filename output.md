# Work instruction 287 — an achievements screen, and the first of each mode

```
UNIT:      287
TASKS:     6 of 6, none dropped
NUMBER:    how many of the six mode firsts can be earned today
           1 - FT8
           CW    : Hamlet keys it and cannot log it
           Voice : Hamlet has no voice path at all
           FT4   : an ADIF submode of MFSK, and no SUBMODE field exists
           PSK31 : an ADIF submode of PSK, and no SUBMODE field exists
           WSPR  : a beacon, so there is no contact to have
ADVANCED:  no
DRIFT:     10 consecutive units without advance, carried from unit 286
VERSION:   1.12.210 -> 1.12.214
BRANCH:    main, pushed
```

---

## 1. What Claude did

**Surface: Claude Code, on the development computer, on `main`.** The prompt claimed
`PROJECT: Hamlet` and the tree confirmed it — `SHACK_FACTS.md` and
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` present, `CoreHMI.sln` and
`MURC.sln` absent, `Hamlet.sln` at the root. **Nothing here is evidence about the
radio.**

**Nothing was recorded under §12.1.** One shell refusal: a heredoc hung and was
backgrounded by the harness; it was stopped rather than polled, and every edit after
it went through a script file, which is the tool fact this order carries.

### Task 1 — what the log can tell you about a mode

**`MODE` holds one value and it is a literal.** `MainWindowViewModel.cs:10132` hands
`Ft8StationConditions` the string `"FT8"`, and `LogContactAsync` takes a decoded FT8
row and is the only route into the log. **The dialog does not let him edit it
either**: `LogContactViewModel`'s own remarks say the observed fields are not editable
and his words go in `COMMENT` and nowhere else.

**Three claims in the order did not survive the tree**, all reported and none
repaired:

| The order says | The tree says |
|---|---|
| CW has no send path | **HM-DEC-059 built one.** `MainWindowViewModel.cs:7791` attaches a `CwTransmitter` over a `KeyerCwSender`. What CW lacks is a way into the log |
| `Tools > My contacts…` | It is under **Radio**, and has been since unit 278 |
| three modes then a fourth fires | **Only three of the six can be matched from a record at all**, so there is no fourth |

**And a second obstacle the order does not mention.** `AdifContact` has no `SUBMODE`
field, so FT4 and PSK31 are not waiting on a transmitter, they are unrepresentable.
Written up in `docs/unit287-what-the-log-says-about-a-mode.md`.

### Task 2 — the achievements screen

**`Radio > Achievements…`, directly beneath `My contacts…`.** His ruling has two
halves that disagree in the tree, and *beside the contact log* is the half carrying
its own reason: achievements are his operating record, the same kind of thing as his
log. **Raised in §4 in case he meant the other half.**

**Four states, because the tree holds four.** The order names two and they are not
enough: CW can be sent and not logged, and WSPR is not a contact. **Each state has
its own mark, its own word and its own sentence**, and an earned row carries a line
of evidence no other row has, so the card reads in grayscale (§0.6).

**A watched red caught a real one.** A `MODE=WSPR` record lit the row as **earned**
and the hover said *you worked this one*, naming a station and a date. That line is
legal ADIF any logger would write, and it still describes a beacon nobody answered.
**A beacon mode is now never earned, including from a record that says it.**

### Task 3 — a mode first announces itself

**`BadgeAward` gained a mode-first shape and `BadgeWindow` shows it.** No second
notice: a second window would be a second place to get wrong the one property that
matters, which is that it never takes his focus, and the one to drift would be
whichever nobody was watching. **Unit 286's six badge tests pass unchanged.**

**Seeded exactly as unit 278 seeds the belt**, and watched failing without it: a first
look at a log holding FT8 and CW raised **two** notices for contacts Hamlet was not
there for.

**Two notices can now stack rather than land on one another.** A belt crossing and a
mode first are separate ladders off the same log, and a notice that is covered is a
notice he never got.

### Task 4 — WSPR, decided honestly

**The row says `not a contact mode`** and the card says why unasked. **No WSPR
achievement was invented.** What it would take is named and left: his own callsign in
the `wsprnet.org` spot database, which is the shape HM-DEC-075 already built for the
skimmer watch — *who heard me*, not *who did I work*. **That is a ruling and it is
his** (§4).

### Tasks 5 and 6

`docs/unit287-what-the-screen-says.md`. **No fact left a screen.** The outcome entry
was appended by `tools\arbiter\outcome-append.bat`, exit 0, as `UNIT 287 - STEP E`.

---

## 2. What Tim should expect

**A screen that says what you have done and what you have not tried.** Radio menu,
under *My contacts…*. The belt at the top with the ring, the count and the bar; the
six modes below.

**One of the six is yours to go and do and the card says which.** FT8 reads *not yet
done* with a hollow ring. CW, FT4, PSK31 and Voice read *waiting on Hamlet* with a
dashed one, because **a thing you have not been given the means to do is not a thing
you have failed to do**. WSPR reads *not a contact mode*, because nobody works
anybody on a beacon.

**Hover any row and it says what stands in the way**, in its own words. The six
explanations run to 1,557 characters and none of them is on the screen until you ask.

**Your first FT8 contact will say so**, in the same small notice the tenth contact
uses. It takes no focus and leaves after eight seconds.

### What will look wrong and is not

- **The achievements item is under Radio, not Tools.** Your log is there, and beside
  your log is where you put this. Say the word and it moves.
- **CW says *waiting on Hamlet* even though Hamlet can key CW.** It can. It cannot
  write the contact down: logging happens from a decoded FT8 line and from nothing
  else.
- **A CW row could still light.** An ADI file is portable, so a record another logger
  wrote into `contacts.adi` counts. That is right: the achievement is what your log
  says you did.
- **Nothing announces on a first look**, however many modes your log holds.

### The build and the tests

Build clean, no warnings. **This unit ran no suite** (HM-DEC-155): only the tests it
wrote or rewrote, plus the neighbours its changes could disturb.

| Class | Result |
|---|---|
| `TheAchievementsScreenTests` | 8 of 8 — new |
| `HowMuchTheApplicationSaysTests` | 5 of 5 — two ceilings added |
| `TheBadgeAnnouncesItselfTests` | 6 of 6 — unchanged, and that is the point |
| `BindingHealthTests` | 1 of 1 |
| `VoiceTests` | 3 of 3 |
| **Total** | **23 of 23** |

**Three reds were watched before they were fixed**, each named in the test that holds
it: the WSPR record lighting the row, the missing seed raising two notices, and an
ordinal comparison reading a lowercase `cw` record as unearned. **Two remarks that
claimed more than had actually been watched were corrected rather than left
standing.**

Inherited reds untouched: `HM-OPEN-088`'s ten, the CW set, the `Ft8Sharp.Deep`
tripwire.

**Five commits, all on `main`, all pushed**, 1.12.210 → 1.12.214. Nothing
uncommitted.

---

## 3. What we should do next

### The screen against a two-mode log

Five records, three FT8 and two CW, `W3YNI` twice so a screen counting stations
rather than contacts would read 4 and be caught.

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

**5 and 5.** One reading through `ContactLogStore`, so the belt, the card and the log
window cannot disagree. **And the earned rows name the earliest contact in the mode**:
the CW records sit out of date order on purpose, and a screen taking the first match
would name `N4L`.

### The ADIF mode strings, cited

Read from the **ADIF Specification, version 3.1.4, released 6 December 2022**, at
`https://www.adif.org/314/ADIF_314.htm`, retrieved **2026-09-08** — the same edition
and page `AdifLog` already cites, so there is one source and not two.

| Hamlet's name | ADIF | A record spells it |
|---|---|---|
| CW | **Mode `CW`** | `MODE=CW` |
| FT8 | **Mode `FT8`** | `MODE=FT8` |
| WSPR | **Mode `WSPR`** | `MODE=WSPR` |
| FT4 | **Submode of `MFSK`** | `MODE=MFSK, SUBMODE=FT4` |
| PSK31 | **Submode of `PSK`** | `MODE=PSK, SUBMODE=PSK31` |
| Voice | **not in the specification** | `MODE=SSB`, `AM` or `FM` |

**`MODE=FT4`, `MODE=PSK31` and `MODE=Voice` are all invalid ADIF.** A first matching
on the obvious string would never fire, and a row that looks earnable and is not is
worse than one that is missing.

### What WSPR does on the card, and why

**It reads `not a contact mode`**, and the card's unasked line explains it: *WSPR is a
beacon rather than a conversation, so there is no contact to log and no first to
earn.*

**A `MODE=WSPR` record does not light it.** Such a record is legal ADIF, any logger
would write it, and it would name a station he never worked: a beacon has no
addressee, no exchange and no moment at which two operators agreed they had made
contact. **That was the watched red of task 2**, and it arrived by the one route
nobody expects, which is a perfectly valid record.

**What it would take instead is a different measurement** and it is not this unit's to
invent (§4).

### The notice on a first FT8 contact

```
first FT8 contact
W3YNI is your first contact on FT8.
It is on the achievements screen from now on.
```

The ring reads **FT8** rather than a running total, in the card's own earned green.
**A second FT8 contact says nothing.**

### Then

1. **Run it and look.** Radio > Achievements. The four marks, the belt, and whether
   the rows read at a glance.
2. **Rule on where the menu item goes** (§4).
3. **Rule on WSPR** if you want it measured rather than explained (§4).

---

## 4. What's blocking us

Nothing blocks the next unit.

### Whether Achievements belongs under Radio or under Tools

**Ruling asked for:** you ruled *an achievements screen off the Tools menu, beside the
contact log*. **The contact log is not under Tools.** It is under Radio, beneath *What
the radio is doing…*, and has been since unit 278.

**What I did:** put Achievements directly beneath *My contacts…* under Radio, because
your reason for it not being under Help is that it is your operating record, *the same
kind of thing as your log*, and that reason points at the log rather than at a menu
name. Tools' own comment in the markup says it keeps the things about Hamlet rather
than about the rig, which cuts the other way.

**Rejected:** putting it under Tools away from the log, which satisfies the letter and
separates the two screens that answer the same question; and moving the contact log to
Tools as well, which is a second ruling nobody asked for.

### Whether WSPR gets an achievement of its own, measured differently

**Ruling asked for:** a first WSPR contact cannot exist. **What could exist is a first
WSPR spot**: your callsign turning up in the `wsprnet.org` database, which is *who
heard me* rather than *who did I work*, and is the shape HM-DEC-075 already built for
the skimmer watch.

**Why it is yours:** it is a new assertion from a new source, it needs a rule about
what counts as a spot, and the order says in as many words not to invent it. The card
names it in the hover and claims nothing.

**Rejected:** deriving it from a log, which is the thing that cannot be done; and
leaving WSPR off the card entirely, which loses the explanation of why it is not
there.

### Asks still outstanding

Carried forward per HM-DEC-139. **This order parks the whole queue**; listed so it
survives.

1. **Two issues of one work-instruction number.** Unit 271, and 252 before it. **The
   author's error.**
2. **`PM95` reads *southern Japan***, unit 271. **Not a defect.**
3. **`HM-OPEN-083` and `HM-OPEN-084`.** In `OPEN_ISSUES.md` by HM-DEC-140.
4. **Three pixels.** **Waiting on Tim.**
5. **`dt` and `hz` were never on the mine list.** **Waiting on Tim.**
6. **Where an outcome entry goes when the unit it corrects has none.** **Tim's.**
7. **`PHASE_OUTCOME.md` is written by a tool no unit is told to run.** **The author's.**
8. **Where the repeat fold stops**, unit 277.
9. **Whether a faded row needs a second carrier of its meaning**, unit 279.
10. **Whether counting subjects is a new assertion**, unit 279.
11. **Whether the fade is still obvious now the row is a bubble**, unit 280.
12. **`HM-OPEN-087`** — thirteen unreferenced `widget.*` templates. **Still Tim's.**
13. **Whether a 14-pixel hover ring is findable**, units 281–283. **Still unseen.**
14. **Whether `AboutWindow` is in scope for the terseness ruling**, unit 281.
15. **What `SenderHelp` is for**, unit 281. `HM-OPEN-088`. **Ten inherited reds.**
16. **Whether an order should name a behaviour rather than a file and line**, unit 283.
    **Answered in practice by unit 284.**
17. **Whether three admissions on the simulated radio is right**, unit 284.
18. **Whether a rasterising test harness is worth a package**, unit 285. **Still open,
    and this unit is the third that could verify nothing about appearance.**
19. **Whether the ring should count down after a stop**, unit 286.
20. **Whether Achievements belongs under Radio or under Tools**, unit 287. **New.**
21. **Whether WSPR gets an achievement measured from spots**, unit 287. **New.**
22. **Whether the log should carry `SUBMODE`**, unit 287. **New.** Without it FT4 and
    PSK31 can never light, and two ADIF submodes of one mode would collapse onto one
    row even with a transmitter for them.
