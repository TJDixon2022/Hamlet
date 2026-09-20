READ IN THIS ORDER.

A. The phase goal - Hamlet holds what it has. No step moves.
B. No criterion changes state; this clears a PSK31 blocker from Tim's record.
C. The report last, and section 4 raises 6 items on top of the carried queue.

UNIT:       371 - complete at task 3 of 3 - 2026-09-20 19:10
PHASE GOAL: Hamlet keeps what it already has working - the hardening phase, paused for this one repair.
UNIT GOAL:  A station who answers Tim's CQ gets his conversation card even when the parser is not certain, marked as a guess, with Report offered on his click, and his row is never hidden.
ADVANCED:   blocker
NUMBER:     answers to the operator that open a card: certain only -> certain or guessed
DRIFT:      carried

## 1. What Claude did

**Surface and gate.** Claude Code on the development machine, branch `main`. The prompt claimed
`PROJECT: Hamlet`; the tree confirmed it: `PROJECT_CARD.md` says `PROJECT: Hamlet`, `Hamlet.sln`,
`SHACK_FACTS.md` and `src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs` exist, `CoreHMI.sln`
and `MURC.sln` do not, root `C:\Source\HamLet`. **Nothing in this report is evidence about the
radio** (FACT-004): the operator's own record was read only as the work order quotes it, and every
number below is from the bench. Status written with `tools/status.sh` after every task and commit.

**Commits on `main`, pushed at the end:**

| Commit | What |
| --- | --- |
| `f06db5b7` | task 0 - the unit opens, 1.13.57 -> 1.13.58 |
| `b6f67f03` | task 1 - the repair and `TheAnswerYouHeardTests` |
| `6502c5b2` | task 1's §R12 rewrites, four tests, their own commit |
| `0d309edc` | task 2 - `TheRowForYouIsNeverHiddenTests`, no code changed |
| `721d66fc` | §R12 rewrites of the two names on the carry-forward list |

### Task 0 - the record

Carry-forward before any change: **engine 146 of 146, app 206 of 206**, both first runs.
`UNIT 371` appended to `PHASE_OUTCOME.md` as a carried repair with `ADVANCED: blocker`; version
**1.13.57 -> 1.13.58**.

### Task 1 - an uncertain answer opens his card

**What was happening, in one line:** `ShowPsk31Cards` took only parses with `IsCertain: true` as
answers, so the station who came back to Tim twice - `toOperator: true`, `turnover: true`,
`Chat`, `certain: false` - was never an answer, no card opened, and the Calling receipt stayed.

Changed:

- **`MainWindowViewModel.ShowPsk31Cards`**: an answer is now a parse addressed to the operator,
  naming a speaker who is not him, that is **either certain or hands the turn back**. A certain
  parse behaves exactly as before; an uncertain one must hand over, which is what separates an
  answer from a fragment of somebody else's over that happens to carry his callsign. The receipt
  retires on any answer.
- **`Psk31Offer.For`** (engine): the certainty gate moves off the offer and onto the macro. On a
  guessed *your turn* the **Report** is offered and nothing else; **Confirm keeps §R1's gate**,
  because it claims a contact and signs off on it. The rule line in `Psk31Offer.Table` says so.
- **`Ft8ContactCard.OfferNote`**: *not sure it is your turn*, beside the offered button, drawn in
  `MainWindow.axaml` next to it and empty when there is nothing to doubt. The turn word itself
  already said *Your turn, a guess* and is unchanged.
- **`Psk31Events.AnswerTaken`**: a new `psk31_answer_taken` carrying the offset and `certain`,
  once per station, **no callsign and no text** (HM-DEC-018).

**`TheAnswerYouHeardTests` (app), watched red first**: copied into a worktree at the task 0 commit,
**4 of its 5 failed there** (the fifth only asserts what the parser says, which this unit does not
change); the new property did not exist there, so that assertion was stripped in the copy to make
the behavioural red visible. **6 of 6 green after**, the sixth being the Olivia line §9 asked for.

**§R12 rewrites, in their own commits.** Six tests asserted the shut door. Four in `6502c5b2`:
`ThePsk31OfferTests` (engine) on two corpus walks, `ThePsk31CqGoesOutTests`' receipt case, and
`ThePsk31ExchangeTests`' guessed case. Two more in `721d66fc`, found by the carry-forward run:
`ThePsk31ConversationCardTests.AGuessedAddresseeOpensNoCard` and the app's
`ThePsk31OfferTests.TheCardNamesTheMacroOnlyOnACertainYourTurn`. Each now guards the rule: an
offer only on his turn, a Report and never a Confirm on a guess, the receipt retired by a guessed
answer that hands over, and **still nothing at all for a line that names nobody or hands nothing
back**.

### Task 2 - a row addressed to you is never hidden

**No code needed changing, and the instruction's premise does not hold in the tree.** Measured
with the CQ filter on:

- A PSK31 row addressed to the operator is on the for-you side **live and after its carrier
  goes**. The ended row from the record's 22:20:43 is on the screen, not hidden.
- Where two stations answer at once, one is the conversation and the other is a name on the
  waiting strip; **no row that spoke to him is on no list**.
- A PSK31 row addressed to somebody else is **also shown**, because the CQ toggle does not hold
  back a PSK31 row at all - unit 337 task 1 and PSK31 plan §R9, after an evening when a carrier
  emitted 262 characters with no turnover and was held off the list.

So the second half of the order - *one addressed to someone else is not shown* - would be a
**change** to R9 rather than a repair, and this unit did not make it (section 4 item 1).
`TheRowForYouIsNeverHiddenTests` asserts what is true today, including that case, so a later unit
that starts hiding PSK31 rows has to answer for it rather than discover it on an evening.

**Recorded under §12.1: nothing.**

**Mismatches with the work order:**
- **Task 2's premise**: an ended non-CQ PSK31 row *is* shown, and the filter hides no PSK31 row.
- **The turn indicator**: the order asks for *his turn?* with a question mark; the card already
  says *Your turn, a guess* / *His turn, a guess* in words, which §0.6 prefers to punctuation
  (grayscale), and the decision block leaves the doubt word to the author. **Kept as it was.**
- **"Python cannot run here"**: Python scripts written to the scratchpad and run as `python
  file.py` ran throughout, as they did for units 361 and 362.
- The order says two tasks in its header and lists three (0, 1, 2), and its report template counts
  *task N of 3*.

## 2. What the owner should expect

**When somebody answers your CQ now, his card appears even if Hamlet is not quite sure it was an
answer.** The card says *Your turn, a guess* where the reading was not clean, the Calling receipt
comes off the panel because somebody did come back to you, and the button offering to tell him how
he is coming through is there with *not sure it is your turn* printed beside it - **and nothing
goes out until you click it**. A guessed reading still cannot offer the sign-off, because that one
claims a contact was made. If two stations answer, they each get their own card, as before.

- Build clean, **1.13.58**. Five commits on `main`, pushed at the end.
- Carry-forward after: **engine 146 of 146, app 206 of 206**. The app run in the middle of the unit
  showed 204 of 206 - the two tests that asserted the old rule - and both were rewritten under §R12
  and are green; **no name that was green before this unit is red after it** (HM-DEC-165).
- **What will look wrong but is not**: a PSK31 row addressed to another station stays on the
  decoded list under the CQ filter. That is R9, older than this unit, and section 4 asks whether
  you want it changed.
- Nothing on the transmit side changed: the card appears, the click sends.

## 3. What you should see

**The record replayed at the bench** (`TheAnswerYouHeardTests`, computed, not seen). The fixture is
his answer with a damaged report, which is what made the real one uncertain:

```
parse   : Speaker = W1AW, Addressee = KC3QIS, Kind = Report, HandsOver = True,
          IsCertain = False, IsForOperator = True
card    : W1AW, turn [Your turn, a guess] guess True, offered Report,
          action [Tell him how he is coming through], note [not sure it is your turn]
receipt : gone from the panel
record  : psk31_answer_taken {"offsetHz":1733,"certain":false}
keyings : 1 after the CQ and before the click; 1 more after the click
```

| Case | Before this unit | After |
| --- | --- | --- |
| guessed answer, hand-back to him | no card, receipt stays | his card, marked a guess, receipt retired |
| what it offers | nothing | Report, with *not sure it is your turn* |
| a guessed Closing or End | nothing | still nothing - Confirm keeps its gate |
| certain answer | card, Report offered | unchanged |
| line naming nobody, no hand-back | no card | unchanged - no card |
| Olivia station answering | no card | the same guessed card, by construction |

**The CQ filter, measured** (`TheRowForYouIsNeverHiddenTests`, filter on):

| Row | Decoded list | For-you side | Waiting strip |
| --- | --- | --- | --- |
| his answer, live | - | yes | - |
| his answer, ended | - | yes | - |
| two stations answering | - | the conversation | the other, by name |
| addressed to somebody else | yes (R9) | - | - |

| Tests | Result |
| --- | --- |
| `TheAnswerYouHeardTests` | 4 of 5 red on the unchanged tree; 6 of 6 green |
| `TheRowForYouIsNeverHiddenTests` | 5 of 5 green with no code change |
| `ThePsk31CqGoesOutTests`, `ThePsk31ExchangeTests`, `TheCqReceiptTests`, `TheTypedLineGoesOutTests` | 32 of 32 |
| `ThePsk31OfferTests`, `ThePsk31TurnTests` (engine) | 16 of 16 |
| `ThePsk31ConversationCardTests`, `ThePsk31OfferTests` (app) | 10 of 10 |
| carry-forward, before and after | engine 146 of 146, app 206 of 206 |

**Every appearance claim is computed, not seen** (FACT-004).

## 4. What's blocking us

**Nothing blocks the hardening phase.** Six new items; the first wants a ruling and the rest are
findings. The carried queue follows them.

### Raised by this unit

**1. Does the CQ filter hide a PSK31 row addressed to somebody else?**

*Ruling wanted; nothing is blocked meanwhile.* The order says the filter *hides rows that are
neither CQ nor for him*, and today it holds back no PSK31 row at all: unit 337 exempted them
because a PSK31 row has no addressee until a turnover has been read, after an evening when a
carrier emitted 262 characters with no turnover and was held off the list. **What this unit did**
was assert today's behaviour rather than change it, because changing it would reopen that fault
for every carrier before its first hand-back. *Rejected*: hiding rows whose latest parse names
another station, which is the order's wording and would still hide a carrier that has not handed
over yet unless the rule is written as *hide only once an addressee has been read*.

**2. A guessed answer with no speaker still opens no card.**

*A finding.* The card is keyed by station, so a parse Hamlet cannot put a name to opens nothing -
its row is on his side and marked a guess, which is where it was before. In the record, the parser
named the speaker; had it not, the card would still not appear.

**3. The turn indicator keeps its words rather than a question mark.**

*A finding, the author's call under the decision block.* The card says *Your turn, a guess*; the
order suggested *his turn?*. In grayscale a word survives and a question mark is easy to miss
(§0.6), and the word was already in the tree from unit 319.

**4. Six tests asserted the shut door, two of them on the carry-forward list.**

*A finding about coverage, not a defect.* The door was guarded in six places, which is why the
middle carry-forward run was 204 of 206. All six now guard the rule, each in a §R12 commit.

**5. `psk31_answer_taken` fires once per station per session, not per line.**

*A finding.* A station who answers, goes, and answers again writes one line. The card's own
history is what carries the rest, and `psk31_line_parsed` already writes every line.

**6. Python runs here, contrary to the order's tool facts.**

*A mismatch, reported for the next order.* Scripts written to the scratchpad and run as
`python file.py` worked throughout, as in units 361 and 362.

### Asks still outstanding - carried from unit 369's section 4, per HM-DEC-139, verbatim

The words below are unit 369's, from its line under `## 4. What's blocking us` to its end, as
committed in `6460852e`. Only that top-level heading is dropped, so this report keeps four
sections. This unit answers none of them.


**Carried forward per HM-DEC-139, verbatim from the last report's queue: the queue
was empty.** Unit 368's report stopped at its phase check and raised nothing, and
the intervening session recorded no asks. Five new items below, most-blocking
first.

**1. Criterion 0.1 presupposes a commit that does not exist, and step 0 is marked
done anyway.** The search is complete and the cause is named, but the answer is a
negative and the line predates the window the criterion names. *Ruling wanted:*
whether that satisfies 0.1 or whether step 0 goes back to partial. *Reasoning:* the
criterion's purpose is to know what broke it before trusting the fix, and that
purpose is served — the mechanism is named, reproduced and repaired. *Rejected:*
naming the closest-fitting commit, which would be a guess presented as a finding.

**2. The `no_transmit_device` refusal was never wrong about the device — the
refusal Tim actually saw was `transmit_device_would_not_open`, and the split above
assumes his device id was still in the file at that moment.** If instead the file
had been reset to defaults, he would have seen `no_transmit_device`, and unit 362's
report says he saw the other. *That means the device id survived and the device
genuinely would not open* — which is a different fault from the settings loss, and
this unit repaired both without proving which one he hit. *Ruling wanted:* whether
that matters enough to chase. His telemetry from 2026-09-14 to 09-19 would settle
it in one read; nothing in this repository has it.

**3. `LearnedAlcReference.Ago()` counts only in seconds and whole minutes.** Now
that the reference survives a restart, a legitimate value is *4320 minutes ago*.
Honest but poor. *Ruling wanted:* whether to extend it to hours and days. *Not done
here* because `TheAlcSentenceTests` asserts the current forms and §10 said not to
touch wording beyond the refusal's.

**4. The archived Olivia plan's checkboxes say 25 of 40, not 38 of 40.**
`docs/phase-olivia-run/PHASE_PLAN.md` has 25 boxes ticked and 15 open, including
several the unit reports say were met (1.5, 4.1–4.8, 5.1–5.3). HM-DEC-166 records
38 of 40 as ruled, which comes from the reports. *Reported, not repaired* — §5 says
repair nothing but this unit's, and an archived phase's plan is not this unit's.

**5. No settings file in the tree was ever reconstructed before this unit, and the
three fixtures are mine.** They carry the right key set, but their *values* are
invented — nobody's real 1.13.30 file was available. They prove the shape loads,
not that Tim's particular file does. *Raised once, not a blocker.*
