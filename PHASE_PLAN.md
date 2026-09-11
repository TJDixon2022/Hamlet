PHASE: Hamlet works PSK31 the way it works FT8
PHASE_SET: 2026-09-11
DESCRIPTION: A third digital mode with the same two cards, the same one-click exchange, the same log and the same achievements, on a modem Hamlet builds itself.
STEP: 0 | The seam - PSK31 exists as a mode. Family colour, the cited 14.070 watering hole, a tab, a log mode and submode, a telemetry mode field. Pressing it tunes USB-D to 14.070 and shows an empty panel that names itself. Nothing decodes.
STEP: 1 | Hear one - a single-channel BPSK demodulator and varicode decoder with AFC and bit-clock recovery, proved against recorded fixtures with a stated character error rate.
STEP: 2 | Hear everyone - signals found across the passband, each with its own demodulator, into the same decoded-text list FT8 uses, with frequency, strength and text as it arrives.
STEP: 3 | Read the conversation - the parser that turns free text into exchange state, with an explicit unknown. The CQ list is the rows whose text parses as a CQ. Worked-fade, entity resolution and the nudge reuse unchanged.
STEP: 4 | Say it - the modulator and the macro exchange through the proved transmit chain. One click, one transmission. Receipt and conversation cards on the same panel; the slot clock replaced by whose turn it is. Proved by loopback at the bench.
STEP: 5 | Log and achievements - RST in the log, ADIF PSK with submode PSK31, and the PSK31 records revealed by the first contact and absent until then.
STEP: 6 | Tim at the radio - tune 14.070, see text, work a station, log it. Only he can close it.

---

# PSK31 phase plan - the reasoning under the step list

**Set 2026-09-11 by the web thread, from Tim's instruction: "I want it to work. Figure it
out."** Where a ruling was needed and Tim had not given one, this plan makes it, marks
it as the author's, and records it under §R below so he can overrule any of them with
one word. The step list above is the machine-readable plan. Everything under the rule
is why.

## §1 The one fact the whole phase is built on

**FT8's contact is a protocol. PSK31's is a conversation.**

FT8 gives Hamlet fifteen-second slots and thirteen-character messages with a fixed
grammar - CQ, grid, report, R-report, RR73, 73 - so *Waiting on him* and *He last sent
RR73* are read straight off the wire.

PSK31 is BPSK at 31.25 baud carrying free text, keyboard to keyboard. No slots, no
grammar, no acknowledgement primitive. A QSO is a convention that people mostly follow:
`CQ CQ CQ de KC3QIS KC3QIS K`, an answer, RST-name-QTH, `73`, `SK`. Everyone types it
differently and many type more.

So "exactly like FT8" is achievable at the **product** layer - the same two card types,
the same one-click exchange, the same log, the same achievements - and is achieved by
Hamlet **sending a fixed macro exchange and parsing what comes back** to infer the
state. Some of the time it cannot tell. The honest display for that is a state called
**unknown** (§0.0 / HM-DEC-092: never present a guess as a decode; a picture binds as
hard as a sentence).

## §2 What is the same, by ruling

Everything Tim has ruled for FT8 and FT4 applies unchanged unless §3 says otherwise:

- **Two card types** (R1, 2026-09-11): a CQ receipt and a conversation card. The receipt
  carries what was sent and when, Log, dismiss, **and no station facts** (R2). One
  receipt, refreshed on repeat press, never a second (R3). **Last call only, no count**
  (R4). The receipt retires when anyone answers; two answers make two conversation cards
  and nobody inherits the call (R5). Conversation cards unlimited with a vertical scroll
  (R6).
- **The map** on the conversation card face, row fitted to the map (R7), opening in a
  popup zoomed to the great-circle path (R8) with the zoom capped (R9).
- **The decoded-text list** with the worked-fade at 0.55 opacity (unit 279), entity
  resolution through `DxccPrefixes.EntityOf` with the `CQ` guard (unit 310), and **the
  achievement nudge** under the four rulings of 2026-09-10: lift plus the quill vane in
  decode green `#3B6D11`; sticky per station with a cap of two; the achievement set is
  the source and no rarity ordering is invented; a door is marked and never named.
- **One click, one transmission** (§0.2). A mark is a nudge, never an arming.
- **The transmit chain** `cq_pressed -> ... -> ft8_transmission Played`, proved from
  `2026-09-10.jsonl`, is the chain PSK31 transmits through. Nothing in this phase
  changes what keys the transmitter; step 4 adds a second audio generator behind the
  same proved path.
- **The dummy load is withdrawn in full** (Tim, 2026-09-06). No compensating control.
- **Achievements** under `ACHIEVEMENTS_PHILOSOPHY.md`: §3.1 absent, not dimmed - no PSK31
  card exists until the first PSK31 contact; §3.2 that contact reveals the mode's
  records; §4 counts say worked, never confirmed; §3.7 restraint.
- **Family colour** for Digital (`CLAUDE.md` mode palette, HM-DEC-032): PSK31 is already
  in the Digital family table. Text colour only, never a fill (§0.5).
- **The band map** already carries PSK31 at 14.070 as cited data under `data/bands/`
  (HM-DEC-054). Step 0 reads it; it does not add a row.
- **Rules that killed sessions** (HM-DEC-155): a unit runs only its own tests filtered by
  exact name; nothing is backgrounded and polled.

## §3 What is different, and cannot be made the same

1. **There is no slot clock.** The `SlotClock` control (unit 305) has no meaning in a
   mode with no slots. On a PSK31 conversation card its place is taken by **whose turn it
   is**: *your turn*, *his turn*, *he is still sending* (carrier present), *unknown*.
   Carrier-present is a fact from the demodulator; the rest is from the parser and marked
   as §R1 says.
2. **The report is RST, not dB.** FT8 exchanges signal-to-noise in dB; PSK31 exchanges
   `599`-style RST, conventionally `599` for a clean copy. The log gains an RST field for
   this mode, and the FT8 dB field is not reused to hold it.
3. **It is a continuous carrier at full duty.** FT8 keys for 12.6 seconds in 15; PSK31
   keys for as long as the text takes, at 100% duty. **On a 100 W IC-7300 this is a
   heat and linearity question**, and the drive level is the difference between a clean
   31 Hz signal and splatter across the whole watering hole. §R4.
4. **Text arrives one character at a time**, not a message per slot. The list and the
   conversation append as characters decode. A "message" for the parser is what arrived
   between two turnovers, not what arrived in a slot.
5. **There is no fixed message length**, so there is nothing like the thirteen-character
   check that FT8 uses to reject garbage. Garbage from noise looks like text. Step 1
   must state its squelch rule and step 3 must never parse a state out of a line the
   squelch did not pass.

## §R Rulings this plan makes on the author's behalf

Each of these is the author's, made because Tim said *figure it out*, and each can be
overruled with one word. None carries an `HM-DEC-` id and none may be given one by a
session.

**§R1 - How much Hamlet asserts about an exchange it can only partly read.** **Strict on
anything that drives a transmission; permissive on what the card displays; every guessed
state marked as a guess.** A macro is offered for one click only when the parser is
*certain* whose turn it is. The card may show a state inferred from turnover words,
elapsed silence and whose callsign appeared last, and when it does the state word is
visibly a guess (§0.6: not by colour alone). Rejected: strict everywhere, because the
card would say *unknown* through most real QSOs and read as broken; permissive
everywhere, because a wrong guess that sends a macro into another operator's turn is a
transmission Tim did not choose.

**§R2 - The macro exchange Hamlet sends.** The standard minimum, and nothing chatty:

```
CQ:      CQ CQ CQ de KC3QIS KC3QIS KC3QIS pse K
Answer:  <HIS> de KC3QIS KC3QIS K
Report:  <HIS> de KC3QIS  RST 599 599  Name Tim Tim  QTH Trafford PA  Grid FN00 FN00  BTU <HIS> de KC3QIS K
Confirm: <HIS> de KC3QIS  R R  TNX for the QSO  73 73  <HIS> de KC3QIS SK
```

Name, QTH and grid come from Settings; nothing is typed at the moment of sending. Free
typing is **not** in this phase - one click, one transmission, and the macros are the
whole vocabulary. Rejected: a keyboard, because it makes the exchange something other
than FT8's and reopens every question about what a card asserts.

**§R3 - What the parser recognises, and what it will not.** Callsigns (the same rule the
decoded list uses today), `de`, `CQ`, RST in `5NN` or `599` form, a grid square, the
turnover words `K`, `KN`, `BTU`, `OVER`, and the closers `73`, `SK`, `CL`. It also
recognises **its own callsign**, which is how it knows a line is addressed to Tim.
Anything else is text and is shown as text. Rejected: parsing name and QTH, because they
are for reading, not for state, and a wrong parse of them changes nothing Hamlet does.

**§R4 - Drive and power.** The step-4 unit sets the IC-7300's data-mode input level so
that **the ALC meter shows no deflection** on the macro tone, and defaults the RF power
for this mode to **half the radio's rated output** because the carrier is continuous.
Both are settings Tim can change; both are shown on the mode's panel so he can see what
Hamlet chose; neither is silent. The unit cites the page of the IC-7300 manual it read
for the input-level setting rather than naming it here from memory. Rejected: full
power, because splatter into 14.070 is what a beginner is most afraid of doing and this
application exists to stop that.

**§R5 - The reference implementation.** `fldigi`, which is GPL-3 - the same licence as
Hamlet. Cloned outside the tree at `C:\Source\fldigi`, **pinned to one commit that the
step-1 unit records in its report**, never committed, never ported wholesale - the same
rule the FT8 phase set for `ft8_lib`. What is read from it: the varicode table (which is
a published standard and may be transcribed with its source cited), the raised-cosine
shaping and the demodulator structure. What is written for Hamlet is Hamlet's.

**§R6 - Where the activity is.** 14.070 dial, with the ribbon of signals above it
across the block the cited band row gives - **14.070 to 14.074** (corrected 2026-09-11
from the author's 14.0700-14.0725, which was recalled rather than read; the row wins,
HM-DEC-054). The list shows every signal in the passband; the CQ filter shows the
ones whose text parses as a CQ. The default click target when Tim presses CQ is a clear
spot Hamlet finds in the passband, not a fixed offset - transmitting on top of a
decoded signal is the second thing a beginner is afraid of.

**§R7 - Units and versions.** Units continue from where the tree is. The FT4 phase
closing at Tim's word (*"FT8 and FT4 seem pretty solid"*, 2026-09-11) took the minor
version bump; unit 312 landed on 1.13.0 and each unit since takes a patch. Whether the
first unit of a phase is x.y.0 or x.y.1 under HM-DEC-150 is carried as an open ask and
does not change what a unit does.

**§R8 - The CQ receipt carries no Log** (Tim, 2026-09-11: *"CQ should not have log
option, that is self-gratification"*). Supersedes R2's Log. A CQ is not a contact; the
Log belongs on the conversation card of whoever answers. Already built by unit 314;
recorded here so the *say it* step inherits it.

**§R9 - Squelch and the honest character** (§0.0 applied to a text mode). A character
the demodulator was not sure of is not shown - no `?`, no dimmed maybe. Silence. Unit
314's squelch measures keying shape, threshold 0.90 on a scale where noise is 0.637;
the number is the author's and is `Psk31Demodulator.SquelchQuality`.


**§R10 - Tim, 2026-09-11: the transmit chain may carry a send that has no slot.** Ruled
on the arbiter's question of unit 317, option A. `OperatorSend` carries either a slot, as
today, or *now*. A send with no slot is not held to a slot fit; it is held to **a stated
maximum length** so a continuous carrier cannot run on - the unit states the number and
why, and it is not more than thirty seconds, which is the Report macro with idle either
side. The operator's click fires a no-slot send at once rather than at a boundary. **The
gate, the single `PttOn` use site, the `finally` that unkeys or aborts, and `StopNow` stay
one code path shared by all three modes.** FT8 and FT4 must stay byte-identical, proved
by the tests that guard them today, run filtered. The transmission record says which mode
went out and carries no slot where there was none. Rejected: a second sequence (a second
`PttOn` site), an invented PSK31 slot (a slot that does not exist in the record that is
evidence), shortened macros (§R2 overruled from underneath), and splitting a macro across
slots (one click, several keyings).

**This ruling amends §2 of this plan**, which said nothing in the phase changes what keys
the transmitter. It does now, in the one way ruled above and no other. §6's `MOVE: stop`
on the transmit chain stays in force for anything beyond it.

**§R11 - Tim, 2026-09-11: drive and power. The operator sets nothing at the radio.** Ruled
on the arbiter's item 43, option A, read the way this application exists to be read: *"I
don't know anything about the radio… I'm writing this app for people who are the same way."*
FT8 already transmits cleanly through this radio on the same USB path and the same input
level; PSK31 uses that path unchanged and asks the operator for nothing. Hamlet's own drive
level is composed at the value that works for FT8. RF power for PSK31 defaults to **half**
and is **offered** as a percentage beside the drive, never written silently (HM-DEC-084,
HM-DEC-074). During a PSK31 send Hamlet reads the ALC and, if it deflects past the zone,
says so **in plain words** on the panel - not a meter, a sentence a person with no shack
years can act on - and the record carries the reading with its age. The manual pages for
anyone who wants them are 12-10 and 4-31, recorded in `docs/psk31-reference.md`. **No unit
asks the operator to set a level, read a meter, or know what ALC is.** §R4 is superseded.

**§R12 - Tim, 2026-09-11: a session fixes its own tests and never asks the owner to approve
it.** The one rule that is the owner's, and it is already ruled: **Hamlet never transmits
without the operator's click** (§0.2). Every test that exists to guard that rule is worded
to guard that rule and nothing more. A test a session wrote while a door was shut, that
later blocks the unit told to open the door, is **the session's to rewrite in its own
commit** so it guards the rule and not the shut door - and that is not a ruling, not an
ask, and not a stop. `ThePsk31ConversationCardTests.NothingOnTheCardTransmits` is the first
such test and is rewritten by the next unit. **The arbiter never puts the wording of a
test to the owner.** Item 45 is closed by this ruling.

**§R13 - Tim, 2026-09-11: telemetry is a must-pass on every remaining step.** Every stage a
step adds writes an event in the `psk31` category that lets a person diagnose that stage
from the file alone, proved by assertion against a fixture, with nothing personal in it
(HM-DEC-018, §2.1). Unit 322 built the receive and transmit events; a step that gives an
event its first production call site proves the event fires there. Step 5 adds the log
and achievement events. Step 4's `psk31_send_*` events gain their call sites when the door
opens.

**§R14 - Tim, 2026-09-11: eyes on the prize.** *"We don't focus too much on pointless
testing. We remember what the phase goal is."* A test exists to prove an exit criterion.
A unit writes the tests its criteria need and no others; it does not add guards for doors
it is not building, pins against changes it is not making, or tests of a test. **The
measure of a unit is whether Tim can see PSK31 coming in and going out**, not the count of
green.

## §4 The steps

Each step verifies its own ground: entry criteria are checked by the step, not
inherited from the previous report. Exit criteria are tiered. **A step's exit is its own
assertions passing plus the carry-forward list green** - `docs/carry-forward-tests.txt`,
run filtered and foregrounded - and **not the whole suite**. `PHASE_CONTROL.md` §2 says
*the whole suite green*; HM-DEC-155 forbids an unfiltered run and `CLAUDE_CODE.md` wins
on the shape of a unit (`PHASE_CONTROL.md` §0). The conflict is named here so it is
carried rather than resolved by a session at three in the morning. Known reds are never
on the carry-forward list.

## Step 0 - The seam

**Delivers:** PSK31 exists as a mode. Pressing it tunes USB-D to the cited 14.070 and
shows a panel that names itself. Nothing decodes.

**Entry:** the tree is Hamlet's; the FT4 phase is closed at Tim's word.

**Exit:**
- Pressing PSK31 asks the radio for 14.070 USB-D, and the frequency came from the
  cited band row, not a constant. *must-pass*
- The panel names the mode and says plainly nothing can be read yet. *must-pass*
- The log offers the mode; the telemetry mode field says PSK31. *must-pass*
- `BindingHealthTests.TheMainWindowBindsWithoutOneComplaint` green. *must-pass*
- The mode is in the Digital family and draws with text colour only. *must-pass*
- The neighbourhood map picks out the PSK31 block when the tab is selected. *nice-to-pass*

**Done by unit 312.** Found that most of the seam already existed and that the missing
piece was an untruthful panel running FT8's decoder under the PSK31 label.

**Depends on:** nothing.

## Step 1 - Hear one

**Delivers:** a single-channel BPSK demodulator and varicode decoder, with AFC, bit-clock
recovery and a squelch, proved against fixtures with a stated character error rate, and
wired so pressing PSK31 shows text arriving from one spot.

**Entry:** step 0 done; the PSK31 tab runs no FT8 decoder and reaches no send path.

**Exit:**
- The varicode table is in the tree as cited data; all 256 codes round-trip. *must-pass*
- The QSO fixtures decode at or under the stated ceilings: clean 0.01, +10 dB 0.02,
  +3 dB 0.05, -3 dB 0.10, two-signal 0.05 on the 1000 Hz channel with none of the other
  station's text. *must-pass*
- The noise-only fixture produces zero characters, and the squelch rule and its number
  are stated in one place. *must-pass*
- The reference implementation is pinned outside the tree and nothing is ported.
  *must-pass*
- Every fixture's hash matches the manifest before use. *must-pass*
- The drift fixture decodes at or under 0.05 with AFC. *nice-to-pass*
- A weak-signal number is measured and reported with no ceiling. *nice-to-pass*

**Done by unit 314**, every QSO fixture at 0.0000, -10 dB measured at 0.1502. **Every
fixture is synthetic**; nothing has been measured against the air.

**Depends on:** step 0.

## Step 2 - Hear everyone

**Delivers:** every PSK31 signal in the passband found and decoded at once, into the same
decoded-text list FT8 uses - one row per signal with its offset, its strength and its
text as it arrives - so the panel is a list of stations rather than one channel.

**Entry:** step 1 done - `Psk31Demodulator` decodes the clean fixture at 0.0000 and
emits nothing on noise, checked by running those two tests before anything is built.

**Exit:**
- The two-signal fixture yields two rows, one per carrier, each decoding its own text at
  or under 0.05, with nothing of one in the other. *must-pass*
- `assets/fixtures/psk31-four-signals.wav` - four signals at 700, 1100, 1600 and
  2200 Hz at different levels, one drifting - yields four rows, each decoding its own
  text at or under 0.10 **over the span the carrier was on**. The author's reference
  reads all four at 0.0000 on that span, and 0.125, 0.386, 0.183 and 0.000 when its
  unsquelched output after each carrier stops is counted - `manifest-step2.json`.
  *must-pass*
- The noise-only fixture yields zero rows. *must-pass*
- A carrier that stops sending is retired from the list rather than left as a ghost;
  the rule and its timing are stated. *must-pass*
- Signals are found by measurement across the passband - a search the unit describes -
  not by a fixed list of offsets. *must-pass*
- The search and all demodulators together keep up with real time on this machine,
  measured as a number. *must-pass*
- `assets/fixtures/psk31-two-signals-100hz-apart.wav` - both carriers decode, at or
  under 0.05 each. *nice-to-pass*
- Off-air audio, if Tim has recorded it, decodes to something a person can read; the
  count of rows and a sample of text are reported, no ceiling. *nice-to-pass*

**Rows are text only.** No callsign is parsed out, no row is clickable, nothing is a
station yet. That is step 3.

**Depends on:** step 1.

## Step 3 - Read the conversation

**Delivers:** the parser that turns each row's free text into exchange state - callsigns,
`de`, `CQ`, RST, grid, the turnover words, 73 and the closers - with an explicit
**unknown** where the text does not support a state. The CQ filter becomes the rows
whose text parses as a CQ. Rows become stations: worked-fade, entity resolution and the
achievement nudge run on PSK31 rows unchanged.

**Entry:** step 2 done - the two-signal fixture yields two rows of text, checked by
running that test first.

**Exit:**
- `assets/fixtures/psk31-transcripts/corpus.json` - eight transcripts, 32 lines, each
  line carrying what a correct parser must conclude - yields, per line, the speaker,
  the addressee, the kind and the turnover as the corpus states them, and **no state is
  asserted that the corpus marks uncertain**. *must-pass*
- The unknown rate per transcript is stated as a number beside the corpus's expected
  rate; on the garbled transcript it is **not lower** than the corpus expects.
  *must-pass*
- A row addressed to the operator's own callsign is recognised as such. *must-pass*
- The CQ filter selects exactly the rows whose text parses as a CQ, and nothing else.
  *must-pass*
- Worked-fade, `DxccPrefixes.EntityOf` with its `CQ` guard, and the nudge run on PSK31
  rows **with no change to their code**; a Costa Rican station calling CQ gets the same
  quill it would on FT8. *must-pass*
- Guessed states are distinguishable from certain ones in what the view model exposes
  (§R1), not by colour alone. *must-pass*
- Name and QTH are shown as text and never parsed into state (§R3). *must-pass*
- A non-standard exchange that skips the report still lands on 73 correctly.
  *nice-to-pass*

**Nothing in this step transmits or offers a click that transmits.** Rows become
clickable stations only in the sense FT8 rows are; the action behind the click is step 4.

**Depends on:** step 2.

## Step 4 - Say it

**Delivers:** the modulator and the four macros of §R2 through the proved transmit
chain, one click one transmission; the CQ receipt and conversation cards on the PSK31
panel under R1-R6 and R8; the slot clock replaced by whose turn it is - *your turn*,
*his turn*, *he is still sending*, *unknown*; drive level and power visible on the panel
under §R4.

**Entry:** step 3 done - the parser is certain whose turn it is on the clean corpus
entries, checked by running that test first. The chain `cq_pressed → … → Played` is as
`2026-09-10.jsonl` proved it, and §R10 is the only licence to change it.

**Progress at 2026-09-11 (units 318-322):** the modulator, loopback, bandwidth, the
unslotted send under R10, the certainty gate, the turn indicator and the conversation card
are built and green. **What remains is the press half**: the CQ that sends once on a clear
spot, the receipt, its retirement by a certain answer, the answer and report and confirm
macros offered on certainty, and the power offer under §R11. The test that stood in the
way is the next unit's to rewrite under §R12.

**Exit:**
- **Loopback:** each of the four macros is modulated by Hamlet, decoded by Hamlet's own
  demodulator, and comes back identical. *must-pass*
- The modulated signal is BPSK at 31.25 baud with raised-cosine envelope, measured:
  occupied bandwidth stated, and under 100 Hz at -30 dB. *must-pass*
- Pressing CQ under PSK31 makes a receipt with no station facts and no Log; an answer
  retires the receipt and opens a conversation card; two answers make two cards.
  *must-pass*
- A macro is offered for one click only when the parser is certain whose turn it is
  (§R1 strict side). *must-pass*
- The turn indicator exists on the PSK31 conversation card, shows *unknown* when it
  does not know, and the `SlotClock` is not shown for this mode. *must-pass*
- Under §R11: the power offer is on the panel beside the drive, defaulting to half;
  nothing is asked of the operator at the radio; during a send an ALC reading past the
  zone becomes a plain sentence on the panel and an event in the record. *must-pass*
- Nothing keys the transmitter at the bench. The chain to `Played` is changed only as
  §R10 allows: one `PttOn` site, one unkey path, FT8 and FT4 byte-identical under their
  guarding tests, and a no-slot send capped at a stated length. *must-pass*
- A no-slot send longer than the cap is refused before it arms, and the refusal is a
  record, not a silence. *must-pass*
- The turn indicator changes within one character of the other station's turnover word.
  *nice-to-pass*

**Depends on:** step 3.

## Step 5 - Log and achievements

**Delivers:** a PSK31 contact logs with RST, exports as ADIF `MODE=PSK` `SUBMODE=PSK31`,
and the mode's achievement records are revealed by the first PSK31 contact and absent
before it.

**Entry:** step 4 done - a loopback exchange reaches 73, checked by running that test
first.

**Exit:**
- A PSK31 contact logs with an RST field, and the FT8 dB field is not reused for it.
  *must-pass*
- The ADIF export carries `MODE=PSK` and `SUBMODE=PSK31` (already in `ContactModes`
  from unit 287; asserted, not rebuilt). *must-pass*
- Before the first PSK31 contact no PSK31 card is visible on the achievements screen
  (§3.1, absent not dimmed); after it, the mode's records appear. *must-pass*
- The two inherited reds in `TheAchievementsScreenTests` are not made worse. *must-pass*
- The ADIF export imports cleanly into one common logger, named. *nice-to-pass*

**Depends on:** step 4.

## Step 6 - Tim at the radio

**Delivers:** Tim tunes 14.070, sees text, works a station, logs it, and says it passed.

**Entry:** step 5 done.

**Exit:**
- Tim says it passed. *must-pass* **No script can evaluate this and none should try.**

**Depends on:** step 5.

## §5 Dependencies, and the named deviation

`PHASE_CONTROL.md` §2 wants at least one step depending on nothing. **Step 0 depended on
nothing and is done. Everything after it is one pipeline** - each step's entry is the
previous step's exit - as the FT4 phase was. Manufacturing an independent step would be
contrivance. **When a step blocks, there is nowhere to route: work it or halt.**

## §6 Branching - the judgments the owner has an opinion about

These are the handful of calls the arbiter would otherwise get wrong at three in the
morning. Everything else is the arbiter's judgment, reported.

- **A must-pass ceiling is missed by a little.** Ship the decoder or parser anyway,
  report the number, mark the step `partial`, and move on. A decoder at 0.12 on a
  fixture is a finding about the fixture. **Do not delete the assertion and do not loosen
  the ceiling.**
- **The parser cannot tell.** The answer is `unknown`, and a high unknown rate is a
  number to report, not a reason to guess. **Never loosen §R1's strict side** - a macro
  is offered only on certainty. If the unknown rate makes step 4 unusable, that is
  `MOVE: stop` with the number, because it changes what the product can promise.
- **Anything would touch the transmit chain, the keying path, or `Played` beyond what
  §R10 allows.** `MOVE: stop`. Risk posture is the owner's. What §R10 allows is ruled and
  is not a stop.
- **A package would be needed** - an FFT library, a DSP package, `Avalonia.Headless.Skia`.
  `MOVE: stop`. §0.4.
- **Reading fldigi tempts a port.** Read the structure; write Hamlet's own. If a unit
  cannot proceed without copying, `MOVE: stop` and say what it would copy.
- **Real off-air audio does not exist yet.** Proceed on the synthetic fixtures, say every
  number is synthetic, and raise the recording again in section 4. **Do not stop for it.**
- **A step-0 or step-1 defect is found later** - the tab reaching a send path again, a
  character shown that the squelch should have swallowed. Fix it in passing as the first
  task of the current unit and say so; do not reopen the earlier step.
- **The card-rebuild root** (cards remade every slot, per-card state lost) is met.
  It is not this phase's. Work around it for PSK31's rows the way unit 314 did - replace
  in place - and log it, not chase it.
- **A drop candidate has to be taken.** Take it, say which, mark the step `partial`
  only if a must-pass went with it.

## §7 Carried into the phase from the FT4 phase

Open asks this phase touches or must not lose. Every unit carries them in its section 4.

- **Does the transmission record ask the radio whether it keyed?** Unit 303's proposal.
  Step 4 makes it sharper: a continuous carrier that did not key is a long silence.
- **Nothing in this repository can look at a picture.** `Avalonia.Headless.Skia` is
  Tim's to add.
- **Three inherited reds**, never chased.
- **The licence of `assets/world-flat-relief.png`** is unknown.
- **The door sentence** is a placeholder.
- **Acknowledgement indicators** - named by Tim, not yet defined. Step 4's turn indicator
  may be what he meant; it is not assumed to be.
- **Card ordering under scroll**, and its root, the per-slot card rebuild.
- **The popup zoom cap of 2.0x** and **the 2 px map outline** are sessions' numbers.
- **Version numbering** - x.y.0 or x.y.1 for a phase's first unit.
- **Real off-air PSK31 audio** - only Tim can record it; two or three minutes on 14.070
  with a few signals on it.

## §8 Revision record

- **2026-09-11, revised by the web thread after reading `PHASE_CONTROL.md` and
  `ARBITER.md`.** The first version carried its per-step content as bold paragraphs under
  an exit-criteria heading; the launcher's step extraction reads `## Step N` sections and
  found none. This version gives every step its own section with Delivers, Entry, Exit
  tiered and Depends on, in the form the FT4 plan used; adds the branching section §1 of
  `PHASE_CONTROL.md` requires and the first version omitted; names the whole-suite
  conflict with HM-DEC-155; corrects §R6 to the cited row; and records R8 and R9 from
  rulings made after the plan was set. **The step list and the phase name are unchanged**,
  so the outcome and status files stay consistent with it. Steps 0 and 1 are recorded as
  done by units 312 and 314 in `PHASE_OUTCOME.md`, which is the authority on state; this
  plan carries no state.
- **2026-09-11, second revision.** §R10 added on Tim's ruling of the same day, answering
  the arbiter's stop of unit 317 with option A. §2's premise corrected, step 4's entry and
  exit amended, §6's transmit-chain stop narrowed to what §R10 does not cover. The step
  list is unchanged.
- **2026-09-11, third revision.** §R11 (drive and power: the operator sets nothing), §R12
  (a session fixes its own tests), §R13 (telemetry must-pass on every remaining step), §R14
  (eyes on the prize) added on Tim's rulings of the same evening. Step 4's drive-and-power
  exit rewritten under §R11 and its entry annotated with what units 318-322 delivered. §R4
  superseded. The step list is unchanged.
