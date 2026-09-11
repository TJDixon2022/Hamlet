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

**§R6 - Where the activity is.** 14.070 dial, with the ribbon of signals from about
14.0700 to 14.0725. The list shows every signal in the passband; the CQ filter shows the
ones whose text parses as a CQ. The default click target when Tim presses CQ is a clear
spot Hamlet finds in the passband, not a fixed offset - transmitting on top of a
decoded signal is the second thing a beginner is afraid of.

**§R7 - Units and versions.** Units continue from where the tree is. The FT4 phase
closing at Tim's word (*"FT8 and FT4 seem pretty solid"*, 2026-09-11) takes the minor
version bump per `CLAUDE_PHASE_CONTROL.md` §7; the first PSK31 unit lands on the new
minor.

## §4 Exit criteria per step

Must-pass, then nice-to-pass. A step with only its must-pass met is `done`; a step
missing any must-pass is `partial`.

**Step 0 - the seam.**
Must: pressing PSK31 tunes to 14.070 USB-D; the panel names the mode; the log offers
the mode; `BindingHealthTests.TheMainWindowBindsWithoutOneComplaint` green; the mode is
in the Digital family and draws with text colour only.
Nice: the neighbourhood map's PSK31 ribbon lights when the tab is selected.

**Step 1 - hear one.**
Must: a recorded fixture decodes to its known text with the character error rate
stated; the varicode table is cited; the reference commit is recorded and the clone is
outside the tree; the squelch rule is stated and a noise-only fixture produces no text.
Nice: a fixture with a carrier drifting 20 Hz over a minute holds lock.

**Step 2 - hear everyone.**
Must: a synthetic fixture of four simultaneous signals at different offsets decodes all
four; the list shows each with its offset and text; nothing is decoded per character
that should be decoded per signal.
Nice: a signal that stops sending is retired from the list rather than left as a ghost.

**Step 3 - read the conversation.**
Must: a corpus of real PSK31 QSO transcripts yields a state sequence with the unknown
rate stated; no state is asserted that the text does not support; the guessed states are
distinguishable from the certain ones in what the view model exposes; the CQ filter
selects exactly the rows whose text parses as a CQ; the nudge, the worked-fade and entity
resolution run on PSK31 rows with no change to their code.
Nice: a non-standard exchange that skips the report still lands on 73 correctly.

**Step 4 - say it.**
Must: loopback proves the macro text round-trips through Hamlet's own decoder; the chain
to `Played` is unchanged and nothing keys the transmitter at the bench; pressing CQ makes
a receipt with no station facts; an answer retires the receipt and opens a conversation
card; the turn indicator exists and shows *unknown* when it does not know; the drive and
power settings are visible on the panel.
Nice: the turn indicator changes within one character of the other station's turnover
word.

**Step 5 - log and achievements.**
Must: a PSK31 contact logs with RST and exports as ADIF mode `PSK` submode `PSK31`; the
first logged PSK31 contact reveals the mode's records and nothing is visible before it;
the two inherited reds in `TheAchievementsScreenTests` are not made worse.
Nice: the ADIF export imports cleanly into one common logger.

**Step 6 - Tim at the radio.**
Must: he tunes 14.070, sees text, works a station, logs it, and says it passed. No
script can evaluate this.

## §5 Dependency deviation

`PHASE_CONTROL.md` §2 wants at least one step depending on nothing. **This phase is one
pipeline and has none**, as the FT4 phase was. Manufacturing an independent step would
be contrivance. **When a step blocks, there is nowhere to route - work it or halt.**

## §6 Carried into the phase from the FT4 phase

Asks that are open today and that this phase either touches or must not lose:

- **Does the transmission record ask the radio whether it keyed?** Unit 303's proposal.
  **PSK31 makes it sharper**: a continuous carrier that did not key is a long silence,
  not a missed slot.
- **Nothing in this repository can look at a picture.** `Avalonia.Headless.Skia` is
  Tim's to add.
- **Three inherited reds**, never chased.
- **The licence of `assets/world-flat-relief.png`** is unknown.
- **The door sentence** is a placeholder.
- **Acknowledgement indicators** - named by Tim, not yet defined. **§3.1 of this plan
  builds a turn indicator, which may be what he meant. It is not assumed to be.**
- **Card ordering under scroll** - raised twice, unruled.
- **`PHASE_OUTCOME.md` has no `UNIT 306` and no `UNIT 307`** in the FT4 phase's record.
  That file is in git history; this phase starts a fresh one.
- **The popup's size** is a number a session chose.
