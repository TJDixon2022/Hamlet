# What this unit moved, and where it went — work instruction 282, task 7

The third of these, after units 280 and 281. **Removing words can remove facts, and
the only defence is a list somebody can read.**

---

## The number this unit is judged by

**The status bar, which is the surface it exists for.**

| | Chars |
|---|---:|
| A tune-in to a CW block, before | **884** |
| The same tune-in, after | **0** |
| Held on the mark beside it | **890** |

The 884 is measured from the nine rows the shipped `mode-receiver-conditions.json`
states for CW, composed through `ReceiverSetupVoice.Say`, not estimated. FT8 composes
448 by the same route.

**It reaches zero rather than being shortened**, because the whole thing is narration
of settings that were changed successfully. **What is not narration still speaks**: an
admission inside the same string — a setting the radio would not confirm, one Hamlet
could not read, one it cannot reach at all — is filtered back out by
`ReceiverSetupVoice.Admissions` and drawn on the bar.

---

## Why three sweeps walked past it

**It is not a string anybody can grep for.** It is composed at run time from rows in
a data file, and its only literal text in `src/` is XML doc comments. **A source
search returns empty and reads exactly like a clean sweep** — which is §12.5 with the
instrument on the wrong side of the glass.

And units 280 and 281 both measured the realized window honestly. **On a headless
training radio no receiver setup ever runs**, so the bar held a short line at
measurement time and an 884-character paragraph in his shack. The instrument was
right; nobody had put it in the state where the fault exists.

**That is what task 2 fixes and it is the part that outlives the paragraph.** Ten
surfaces now carry a ceiling, and the test that matters says the CW tab measures 611
and would have measured 1,495 with the paragraph on the bar, against a ceiling of
750. A ceiling merely above today's figure catches nothing.

---

## Moved to hover, nothing lost

| Was | Chars | Now on screen | Where the words went |
|---|---:|---|---|
| *I turned the auto notch off because it hunts steady tones and Morse is a steady tone, turned the manual notch off because…* — nine clauses | 884 | nothing | the status mark, `StatusTip` |
| The same for an FT8 block, four clauses | 448 | nothing | the same mark |
| *Saved as "…"*, *Connecting to …*, *You set the radio to …*, the grid and licence provenance lines, *Kept the last N seconds …*, the callsign lookup narration | — | nothing | the same mark. **Twelve sites marked as narration**; the other nineteen still speak |

**Every line the status bar has ever drawn is behind that mark**, including the ones
that are also on the screen. A hover that sometimes says nothing teaches somebody not
to bother hovering.

---

## Kept in words, and why

| Kept | Why |
|---|---|
| Every admission inside the tune-in paragraph | A setting the radio would not confirm, one Hamlet could not read, one it cannot reach. **Three kinds of clause in the same string**, and narrating the whole thing would have hidden all three |
| *No answer on COM7…*, *Hamlet could not set the mode…*, *Could not write the capture…*, and sixteen more | A plain assignment is a fault by default. **The default is the safe direction**: a site nobody classified stays on the screen, so the cost of a misjudgement is a sentence he did not need rather than one he did |
| The mode-follow line where the radio declined | The two arms of one expression are different kinds: `decision.Narration` is Hamlet saying what it did, `result.Detail` is the radio declining |

---

## Added rather than removed

**Task 3 put a column back on the log.** `AdifLog` has written `GRIDSQUARE` and
`MY_GRIDSQUARE` as separate fields since unit 274 and the window showed only the
second, so a log of contacts said where he was standing every time and never where
the station he worked was. That is 10 characters of header and a value per row, and
it is the opposite of this unit's other direction — **the rule is text only where he
hovers, not less text**, and a fact he cannot get at any other way belongs on the
screen.

---

## A fact removed

**None.** Nothing was deleted in this unit. Every sentence that left the status bar
is on the mark, and `TheStatusBarStopsLecturingTests` asserts the whole paragraph is
there character for character.

The one thing that changed wording rather than moving is a tooltip that had not
shipped: the first draft of the disabled *Have a look* button's reason sent him to
the Radio menu for the receive help, and that menu carries Connect, Favorites and
Recent and never carried it. **A tooltip naming a screen that does not exist is the
same fault as the button it was explaining**, so it was corrected before the commit
and the test asserts it does not say so.

---

## The application, measured again

Same harness, **and not the same fixture**, which has to be said before the numbers
are compared.

Unit 281 measured with the licence class unset, so the panel was resolving it from
the callsign while the layout was being pumped and the figure depended on how long
the run had been going — the Voice tab read 541 unresolved and 623 resolved, the same
window in two states. **This unit settles the profile**, because a measurement that
races a network lookup cannot carry a ceiling, and because a resolved profile is the
screen he actually operates. That adds the privilege and licence lines and removes
the *Hamlet does not know your license class* fault, which is most of the difference.

**With the byline excluded** — it rotates through forty-five lines of 20 to 104
characters, an 84-character die roll (HM-DEC-039):

| Screen | Chars | Ceiling |
|---|---:|---:|
| SettingsWindow | 1,628 | 1,750 |
| RigDiagnosticsWindow | 1,346 | 1,450 |
| MainWindow — Digital tab | 1,201 | 1,350 |
| AboutWindow | 726 | 850 |
| LogContactWindow | 716 | 850 |
| MainWindow — CW tab | 611 | 750 |
| MainWindow — Voice tab | 601 | 750 |
| FavoritesWindow | 280 | 400 |
| ContactLogWindow | 246 | 350 |
| DecisionLogWindow | 196 | 300 |
| **TOTAL** | **7,551** | **8,800** |

Unit 281's 7,665 was taken with the byline in and the profile unsettled, so **the two
totals are not the same measurement and neither is wrong.** What is comparable is the
status bar, and that is the table at the top.

**Measured twice, identical but for one character of clock.**
