# 1. What Claude did

**STATE.** Development computer, `C:\Source\HamLet`, Claude Code surface (§9.5).
**Branch: `main`, and nowhere else** (§9.5.1, HM-DEC-113). The prompt claimed
`PROJECT: Hamlet` and the tree confirms it: `CLAUDE.md`'s header reads
`Project: Hamlet`, the solution is `Hamlet.sln`, the namespaces are `Hamlet.*`.
Gate passed. **Nothing in this report is evidence about the radio**
(`SHACK_FACTS.md`, HM-DEC-093): every number below comes from a fixture, a
generated signal, or one off-air recording decoded on this machine.

**Nothing was recorded under §12.1.** Three questions came up and all three are
in section 4.

**All six phases were worked. Phase 6 was not dropped.**

## Phase 0, the tree

Confirmed: `git branch -a` shows `main` alone, the feature branch is gone local
and remote, HEAD was at `c1a76f8`. Every commit this session is on `main`.

The `"save"` commits are recorded as **HM-OPEN-025** and not chased.

## Phase 1, gap classes from the gaps, and Tim gets most of his transcript

The measurement holds. My own independent pass over the WAV finds exactly the
heaps the brief describes: **69 element gaps near 40 ms, 30 character gaps
between 190 and 300, 14 word gaps above 300**, dits near 50 and dahs near 160.
Hamlet's own fit now reports **element 51 ms, character 241, word 590 — a
character gap 4.7 element gaps long where the textbook says three.**

The settled pass fits three classes to the gaps in log space and **holds no dit
multiple anywhere**. Where the gaps do not form three groups it returns nothing
and the pass emits nothing, because a guessed boundary is a guess about where
the words are.

**Two things had to be right and both were found by measuring, not reasoning.**
The classes are fitted **per signal, not per window**: a settled window is a few
seconds, which holds plenty of element gaps and often no word gap at all, so
three classes cannot be found inside one however cleanly they separate over the
transmission. And the fit is **seeded on percentiles, not on the ends**: seeding
on the smallest and largest gap puts the first centre on whatever the shortest
stray was and leaves it there, merging the element and character heaps. That
variant produced a transcript of nothing at all.

The bulletin now reads

```
JJ AOT NET ■I ECH STAAION HAND■ AHIS MESAGE P
```

against a key of `AT ARRL DOT NET <BT> EACH STATION HANDLING THIS MESSAGE P`.
**The spaces are in the right places for the first time.** `NET`, `ECH`,
`STAAION`, `AHIS`, `MESAGE`, `P` are all correctly divided. What is left is
character-level and belongs to the clock, which is phase 2.

**Applying the same removal to the streaming estimator was tried and reverted on
measurement.** It fixes three tests and breaks four, two of them prime-directive
tests: `NothingIsInventedAtTheHandover` and finding a tone in real two-station
audio. Refusing to accumulate a pattern while the classes are unknown was tried
as well and took the suite from nine failures to eighteen. Resetting the gap
history on a retune was tried and changed nothing. The streaming pass keeps its
fallbacks; the question is in section 4.

## Phase 2, half amplitude for the clock fit — attempted, measured, reverted

**It does what HM-DEC-112 says it does, and it breaks something else.** Four
variants were measured:

| Variant | `ACleanSignalDecodesExactly(25)` | Speed readout at 25 | Suite |
|---|---|---|---|
| As committed | `CQ D■ W1AW K` | 24 wpm | 9 failing |
| Both gate edges at 6 dB | **exact** | 25 | 11, breaks 18 wpm |
| Mark at half amplitude, per-mark peak | **exact** | **29 wpm** | 10 |
| Mark and gap both corrected | **exact** | 25 | **23** |

The ruling's own target is met by every variant that touches the mark: **25
words a minute decodes exactly.** But correcting the mark alone leaves the
displayed speed 16 percent high, because a mark and the silence after it are
complementary and the gate's fall time was taken out of one without being given
back to the other. Correcting both fixes the speed and takes the suite to
twenty-three failures, because the streaming pass still classifies marks by a
dit multiple and the dit has moved under it.

`fast-easy` reads `DE N0CALL N0CALL K` in every variant; only its reported speed
moves, 25 to 30.

**A wrong speed on screen is its own §0.0 problem, so nothing was shipped.** The
question is in section 4. Bandwidth-following-speed was correctly deferred by
the ruling and is not what is missing here.

## Phase 3, the sensitivity floor exists

Built at 17 in the decoder's own margin units, named once in
`CwConfidenceModel.RefusalFloorDb`. **The property the ruling exists for now
holds: the worst invented share across the whole sweep is zero, at every
level.** `ItGoesQuietRatherThanInventingLettersInTheNoise` passes on the floor
existing rather than on its bound moving. Nine failures became eight.

**One thing did not come out as the acceptance predicted.** The sweep is not
unchanged from 18 dB down to 1. It is untouched to 6, then reads 0.94 correct at
5, 0.61 at 4, 0.11 at 3, and nothing at 2 and below. So seventeen bites at about
five decibels broadband on this fixture rather than at nought: **four decibels
of reach given up.** Nothing was adjusted to hide it.

## Phase 4, the bar

The easy tiers and `exchange-easy` are pass-or-fail. Written the looser way
first — no strangers, no placeholders — three of four passed while dropping
their opening characters, and "emits the message" is not satisfied by emitting
most of it. The bar asserts the ruling's own words.

**Four tests are red and are meant to be:**

```
coverage-easy    34567890QRZ?DE/N0CALL      against 1234567890QRZ?DE/N0CALL
exchange-easy    CQDEN0CALLN0CALLK          against CQCQDEN0CALLN0CALLK
tightfist-easy   DETESTK                    against TESTDETESTK
prosigns-easy    CALLIRSK                   against BTN0CALLARSK
```

Three lose only their opening, to acquisition. The fourth reads `IR` where `AR`
was sent, which is a wrong character rather than a missing one and the only
strangers case among them.

## Phase 5, the captures

**The rule permits them and it is not a permission question.** §2.1 says
recorded off-air audio is public by nature and asks only that fixtures committed
to the public repository are reviewed by Tim first, which he did when he
committed `004507`.

That one now carries an **answer key**, which this project has never had: every
fixture until now was synthesized, and so proved only that the decoder agrees
with the generator, or was a capture asserting what was measured because nobody
knew what was sent. The key is asserted and it is red, at **36 characters
against 47**.

**Three of the four are not on the machine.** Searched the repository,
`tests/fixtures/cw/captured`, `%AppData%\Hamlet\captures` and Downloads.
Recorded as **HM-OPEN-026**. `003758` is the one worth chasing: it would be a
regression test for a success and the suite has none.

The QN signals go to **FG-013** rather than being built here.

## Phase 6, the three small things

**The solid green block is not the tip failing to firm up.** It is the
unreadable placeholder, drawn correctly and in the wrong colour: the whole tip
was one run in one ink, so a mark Hamlet could not read came out in the tip's
green and read as a block of something rather than as the glyph that means it
could not tell you what was there. The tip is now split so a placeholder keeps
its amber wherever it sits.

**The revision count has a denominator.** It says how many out of how many and
what share, because a pass revising one reading in twenty is working and one
revising half of them is reading different audio from the first.

The spot-refresh ordering is a trade-off and is in section 4 with a
recommendation, as the order asked.

**No transmit work of any kind was done and nothing was built toward auto-CQ.**

# 2. What Tim should expect

- **Build succeeds, no warnings**, engine and app.
- **1800 tests, 13 failing.** 1373 of 1385 pass in the engine, 414 of 415 in the
  app. 12 tests added.
- **Four of the thirteen are the new bar and are supposed to be red**
  (HM-DEC-114): the four `TheEasyTierIsReadWhole` rows above. Leave them.
- **One more is the new answer key** and is also supposed to be red:
  `TheBulletinDecodesToItsAnswerKey`, at 36 characters against 47. That is the
  definition of done being reported as a number rather than a shrug.
- **Eight are pre-existing** and unchanged in character:
  `ACleanSignalDecodesExactly(25)`, `AFadingSignalComesBackRatherThanStayingDead`,
  four `ASignalAtTheWrongPitchIsStillFound`, `TheSpeedEstimateFollowsAChange`,
  and, in the app, `ClearingTheTranscriptLeavesTheDecoderAlone`.
  **`ItGoesQuietRatherThanInventingLettersInTheNoise` is gone**, fixed by the
  floor.
- **What you will see that is different at the radio.** The transcript of a
  traffic net should have its words divided correctly for the first time. The
  decoder now says nothing at all below its floor rather than producing text
  that is partly invented, and on a weak signal that means silence where there
  used to be letters. A placeholder in the live tip is amber rather than green.
- **`cw-2026-08-18-004507` still does not decode to its key**, and the reason is
  in phase 2: the clock fit is biased and the fix for it makes the displayed
  speed wrong. Section 4, item one.
- **Everything is committed and pushed to `main`.** Nothing local, no branches.

# 3. What we should do next

- Rule on phase 2, section 4 item one. It is the last thing between the bulletin
  and its answer key, and it is one measurement away from being finishable.
- Rebuild the six short fixtures with enough run-up for the detector, which is
  generator work and would clear most of the pre-existing eight along with three
  of the four new bar failures.
- Get `cw-2026-08-18-003758` onto the machine. A suite with no regression test
  for a success cannot tell a repair from a coincidence.
- Look at `prosigns-easy` reading `IR` for `AR`, which is the only strangers
  case left at fifteen decibels and so the only one the bar catches for the
  right reason.
- Rule on the streaming estimator, section 4 item two.

# 4. What's blocking us

---
date: 2026-08-18
refs: CLAUDE.md §0.0, §12.1; HM-DEC-112; HM-DEC-048
---

**Half amplitude is taken for the clock fit and the gap is given back what the
mark sheds, and the streaming pass stops classifying marks by a dit multiple in
the same change.**

HM-DEC-112 is right and half of it cannot ship alone. Taking the mark at half
amplitude makes `ACleanSignalDecodesExactly(25)` decode exactly, which is the
ruling's own named target, and leaves the displayed speed at 29 words a minute
for a 25 word-a-minute signal, because the detector's fall time was taken out of
the mark and not given back to the gap. **A wrong speed on screen is its own
prime-directive problem**, so it was not shipped.

Giving the gap back what the mark sheds fixes the speed exactly — 25 reads 25,
18 reads 17 — and takes the suite from nine failures to twenty-three. The reason
is visible in the code rather than guessed: `ClassifyMark` still splits dit from
dah at two dits, and the dit has just moved under it. The marks want the same
treatment the gaps got in phase 1, which is a boundary fitted between the two
measured clusters rather than a multiple of one of them.

So the change is three things at once, not one: mark at half amplitude, gap
corrected by the same amount, and mark classification fitted rather than
multiplied. Any two of the three make things worse than none.

Rejected: shipping the mark correction alone and accepting a speed readout 16
percent high. The speed is on screen, it is what a beginner uses to decide
whether he could have copied something, and a wrong one teaches him the wrong
thing about himself.

Rejected: widening the detection bandwidth instead, which the ruling already
rejected as costing sensitivity where it is most needed.

---
date: 2026-08-18
refs: CLAUDE.md §0.0; HM-DEC-115; HM-DEC-095
---

**The streaming estimator keeps its dit-multiple fallbacks until the gap classes
can be fitted without costing the handover tests.**

HM-DEC-115 says remove every remaining dit-multiple fallback from gap
classification. It is done in the settled pass, which is where the transcript
comes from and where the ruling's evidence and acceptance both live. **In the
streaming pass it was measured and is net-negative.**

Fitting three classes there needs a gap history long enough to hold word gaps,
which are the rarest class by a wide margin. The estimator's window is twenty
gaps and that is right for following a change of speed. Lengthening it to 256
fixes three tests — the fade, the transcript clear, and the sensitivity one —
and breaks four, of which two are prime-directive tests:
`NothingIsInventedAtTheHandover` and `TheToneIsFoundInRealisticAudio` on the
second station of a two-station recording.

Resetting the history when the tracker retunes was tried, on the reasoning that
the classes belong to a sender, and changed nothing. Refusing to accumulate a
pattern while the classes are unknown was tried and took the suite to eighteen
failures.

What that says is that the streaming pass wants a different arrangement rather
than a longer buffer: the classes have to be attributable to a sender before
they are useful, and a rolling window cannot tell one sender from two.

Rejected: leaving the ruling half-applied and silent about it. Rejected:
applying it anyway and accepting two prime-directive regressions, which trades
the fault the ruling names for a worse one.

---
date: 2026-08-18
refs: CLAUDE.md §0.0.1; HM-DEC-024; HM-DEC-075; HM-DEC-109
---

**The first spot refresh waits for the radio to be asked where it is.**
Recommended, and it is a trade-off so it is yours.

Carried from the last report unchanged. `ReloadSpotsAsync("startup")` runs from
the view model's constructor; the radio is not connected until the window's
`Opened` event; until then the band is whatever `settings.LastBand` says, and
the band scopes what RBN is filtered to and what the skimmer watch listens for.

It has happened once, in session `af471e84`, on the training radio, and never
with a real one. It self-corrects because a band change reloads spots.

**The recommendation is to wait.** What waiting costs is an empty happening-now
panel for the second or two before the radio answers, on a screen the operator
has only just opened. What not waiting costs is a burst of calls to somebody
else's service on the wrong band and a panel briefly showing stations from a
band he is not on, which is a display asserting something false rather than
asserting nothing.

Rejected: asking the radio from the constructor, which would put a serial read
on the path that builds the window.
