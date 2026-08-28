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
"wrong project — nothing done."

If all four hold, say "Hamlet confirmed" and continue.

---

# Work instruction 040 — the radio lands ready, and the press works

**ISSUED: 2026-08-28. A fresh order, not an amendment. Follows unit 039.**

**Eight tasks; task 8 is the drop. This is a long unit by instruction —
the 45-to-60-minute window is a floor against trivial units, not a ceiling.**

## Why this unit exists

**Today the operator went to 20 m FT8, heard nothing at 14.074, and spent an
hour getting the radio into a state where FT8 was audible. Four wrong turns, and
each one is a defect in this application.** This is HM-DEC-054's moment happening
a second time, fourteen days after the decision written to prevent it.

The radio was in **CW mode, FIL2, 500 Hz** — a window sitting below the bottom of
the FT8 block. Nothing about the band, the antenna or the decoder was involved.

**And the same state cost a second hour downstream.** The screenshot of that
radio was read as a broken waterfall; unit 039 found the picture was drawing the
receiver's own filter skirt. **§0.0.1 says the app's own record must be enough to
tell whether a fault is in the signal, the radio, or Hamlet itself. It was not.**
The mode and filter existed nowhere in any file Hamlet wrote. That is the second
principle failing, and tasks 2 through 6 are it being fixed.

**Every manual value in this order is re-read from `IC-7300_ENG_FM_12b.pdf`
before it is coded against, per §0.** Page numbers are given so the read is
cheap, not so it can be skipped.

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches,
including where the work succeeded anyway. Trust the tree over this order
everywhere they differ.

From unit 039's report, not measured by this author:

- The digital waterfall's floor is now **per bin, tracked over time**, with a
  second slow average holding how far each bin usually sits above its own floor.
  Saturation on an empty recording went 12.3% to 0.0%.
- **A dead-constant carrier fades from that waterfall.** Recorded, not a fault.
- The tuning marker is drawn only when it falls inside the band; the CW picture
  is unchanged and that was checked.
- The clock queries at startup and every ten minutes; a failed query returns
  unknown and does not erase an earlier good reading.
- **Engine: 29 failing of 1914**, one more than the stable 28. The extra is
  `AConfirmedModeWriteFoldsTheDataVariantTooAsync`, which passes three of three
  in isolation and is a known intermittent. **App: 509 of 509.**
- **The CW capture press calls `MarkCase`, which appends to `CwCaseRoster`** —
  the roster that scores the CW decoder. This is what stopped 039's task 5.
- `Ft8Slots` arithmetic exists and is tested.

**Record the failing counts from the tree before task 2.** If the engine is not
29, or if `AConfirmedModeWriteFoldsTheDataVariantTooAsync` is not the extra, say
so before anything else.

**Tasks 7 and 8 are unit 039's dropped tasks 6 and 7, returned unchanged.**

## Rulings in force

**Transcribed with what was rejected. Do not re-argue either.**

**Tim's rulings, 2026-08-28:**

> **The digital capture gets its own record and its own folder.** `captures\digital\`,
> a record separate from `CwCaseRoster`. The two corpora are measured differently
> — CW by character accuracy, digital by decode yield against WSJT-X — and the CW
> decoder is mid-repair.
>
> Rejected: adding a column to `CwCaseRoster` saying which tab a press came from
> — that changes the CW capture path. Rejected: routing the digital press through
> `MarkCase` at all — every row of that roster asserts the operator heard a station
> Hamlet failed to read.

> **The digital capture press works the way the CW one does** — same ring, same
> window, same file shape. **No trimming, no slot alignment.** The consequence is
> accepted: a 30-second grab starting mid-slot leaves WSJT-X two partial slots it
> cannot score, so **these files are diagnostic material, not corpus.** Trimming
> returns when scoring starts. **Do not build trimming in this unit.**

> **The point of the capture is to give a WAV to match against a screenshot.**
> That pairing is how a complaint about the screen becomes evidence instead of a
> description.

> **The decoder is written in C#, not wrapped.** Unit 038 found no C toolchain and
> Tim's own condition names that as the trigger. **This unit does not start the
> decoder proper** — only the sync search, in the tail, because it runs on the
> transform that already exists.

> **Static strings unit 037 wrote stay as written until they are live**, including
> the mode strip's `reading it · 9 messages this slot`.

**Standing rulings this unit is bound by:**

- **HM-DEC-009 / §0.0** — never present a guess as a decode; **this binds pictures
  as hard as sentences** (HM-DEC-092).
- **§0.0.1** — the app's own record must distinguish a fault in the signal, the
  radio, or Hamlet. **This is the principle tasks 2 through 6 serve.**
- **HM-DEC-054** — the band-plan file's editorial rule that the FT8 block is 3 kHz
  wide.
- **HM-DEC-056** — tuning into a neighborhood writes the mode; the operator's own
  hand wins and suspends the write visibly until the next band change; a value the
  radio did not confirm is unknown, not assumed.
- **§0** — where something can be generated from a source of truth, generate it.
  **No constants sprinkled through code.**
- **HM-DEC-007** — decoders tested against WAV fixtures.
- **§0.2 / HM-DEC-008** — **no transmit work of any kind in this unit.**

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` — `STATE`,
`TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what is moving
inside the task. Same every ten minutes while a task runs.

## The tasks

### Task 1 — trace, and re-read the manual

**Run the engine suite whole and record the number** before anything else.

**Re-read from `IC-7300_ENG_FM_12b.pdf`**, and quote what you find rather than
this order's summary of it:

- **p. 3-3** — the `[DATA]` key on the MODE screen, and in which modes it is
  displayed. The operator concluded USB-D does not exist on this radio because it
  is absent while CW is selected.
- **p. 4-5, 4-6** — the filter selection gesture, the FILTER screen, and **the
  per-mode filter defaults.** This order believes SSB-D's are 3.0 / 1.2 / 500 Hz
  against SSB's 3.0 / 2.4 / 1.8 kHz. **Confirm or correct that table from the
  manual.**
- **p. 19-8, 19-11** — command `26`: the VFO selector, mode, data flag and filter
  byte, and **which read distinguishes USB from USB-D.**
- **p. 19-4, column-aware** — `14 08` and the Twin PBT. `CLAUDE.md` records this
  as the row once mistaken for the CW pitch. **Find whether the inner control has
  a companion sub-command or does not.**

Then trace, and **say what you find**:

- **The tune-in write path.** Where HM-DEC-056's mode write happens, what frame it
  builds, and whether a filter byte is currently sent, defaulted, or omitted.
- **`data/bands/us-neighborhoods.json`** — its row shape, and where a passband
  requirement would go.
- **The neighborhood card** — where its text is composed, and where the dial
  frequency is named.
- **The CW capture path** — what it writes, what `MarkCase` appends, and **which
  parts are reusable without touching it.**
- **What the app currently records about the radio's state**, anywhere. §0.0.1's
  question: could a session tomorrow tell from Hamlet's own files that the radio
  was in CW/FIL2/500 Hz today? **Answer it.**

### Task 2 — the neighborhood carries its passband, and the write sends it

- Add a **passband requirement** to the neighborhood rows in
  `data/bands/us-neighborhoods.json`. FT8 and FT4 need the full **3 kHz** by the
  file's own editorial rule; **PSK31 at 31 Hz does not.**
- **Derive the filter byte from the requirement and the mode's own filter scale.
  Do not write `01` down as a constant.** FIL1 is the widest slot in every mode,
  but what makes it correct here is that 3.0 kHz ≥ 3 kHz — **and that is the
  sentence the code should be able to state.**
- **Send it as part of the existing `26` frame. One write, not two.** `CLAUDE.md`
  already records that omitting the trailing bytes selects DATA OFF and the mode
  default rather than leaving them alone, so **the filter byte is already being
  sent as a default whether or not it was chosen.**
- **Read back** with `26` and the selector alone. **A filter the radio did not
  confirm leaves the passband unknown rather than assumed**, exactly as HM-DEC-056
  already rules for the mode.

**Acceptance:** tuning into FT8 from any starting state produces one `26` frame
carrying USB-D and a filter wide enough for the neighborhood's stated
requirement, and the readback either confirms it or the passband reads unknown.

### Task 3 — the card names the block, not just the dial

**No station transmits on 14.074.** The energy sits from roughly 14.0742 to
14.0770 as audio offsets above the dial in upper sideband — the same physics the
file's 3 kHz editorial rule already encodes.

The card names **both**: the dial to tune to, and the block that dial opens onto.

**"Dead at the published frequency, alive one kilohertz up" is the correct
behaviour of a correctly tuned radio.** An operator not told that concludes the
band is empty or the rig is broken. **That has now happened twice to the one
operator this app is for.**

**Generate the block from the neighborhood row and the mode's sideband, not from
a hand-typed number** (§0).

### Task 4 — an uncleared PBT is a fact, and Hamlet is blind to it

**There is no write available here and it must not be papered over.** The Twin PBT
is a physical control; **the app must not claim to have cleared something it
cannot clear.** What it can do is see it.

- Read the outer position. **If the inner cannot be read, that is an explicit
  unknown in the ledger, not an assumption that it is centred** (§0.0).
- A PBT away from centre **narrows the effective passband below whatever FIL1
  gives.** Hamlet says so in the app's voice, and names the remedy: hold
  `TWIN PBT CLR` for one second until the dot beside the width disappears
  (p. 4-5).
- **It suppresses the "you should hear the block now" claim.** Saying the radio is
  ready while a hand-set PBT closes the window is the prime directive broken on
  the one sentence the operator will act on.

**Raise, do not decide:** whether an *unreadable* inner PBT suppresses the
readiness claim or only qualifies it. Suppressing on an unknown is the
conservative reading of §0.0 and it fires on every radio where that read does not
exist. **Put it in the report in HM-DEC-010's options-table form.**

**Raise, do not decide:** whether the filter write belongs to the operator's hand
the way the mode does. HM-DEC-056 says the operator's own hand wins and suspends
the write visibly until the next band change; a filter turned by hand is the same
gesture, **and it is also how somebody deliberately narrows onto one signal.**

### Task 5 — regression fixtures, as radio states

**The failure is a state, not a signal**, so the fixtures are states:

- **CW / FIL2 / 500 Hz at 14.074** — today's starting state.
- **USB-D / FIL2 / 1.2 kHz** — the state HM-DEC-056 as built can produce, which
  task 2 must make unreachable.
- **USB-D / FIL1 / 3.0 kHz with PBT off centre** — the state task 4 must catch.
- **USB-D / FIL1 / 3.0 kHz, PBT clear** — **the only one that may produce a "you
  should hear it" claim.**

**A test walks the whole neighborhood map and asserts that no tune-in can leave
the radio in a passband narrower than the neighborhood's stated requirement**, on
any band, for any mode family. That is HM-DEC-054's test one layer down: it
asserts the radio can actually hear what the map says lives there.

### Task 6 — the capture press, wired

The press on the waterfall header writes to **`captures\digital\`**, with **its
own record, separate from `CwCaseRoster`.**

- Same ring and same window length as CW. **No trimming, no slot alignment.**
- **`MarkCase` is not called and `CwCaseRoster` is not touched.**
- **The sidecar satisfies §0.0.1**, which is the whole point of this unit: it must
  be enough for a later reader to tell whether a fault was in the signal, the
  radio, or Hamlet. **At minimum that means the mode, the data flag, the filter
  slot and its width, the PBT state including unknown, the dial frequency, and the
  clock offset with its age** — alongside whatever the CW sidecar already carries.
  **Every value marked measured or unknown; nothing defaulted silently** (§0.0).
- **The sidecar records that the file is untrimmed**, so a later scoring run can
  tell diagnostic material from corpus without opening the audio.
- Filenames distinguish these from CW captures.

**Acceptance:** a press writes a WAV and a sidecar the operator can find, and
**the sidecar alone identifies today's failure state** — CW, FIL2, 500 Hz — if the
radio is in it.

### Task 7 — the slot cutter

Cut the audio into **15-second slots aligned to UTC quarter-minutes**, using the
clock offset.

- **Unknown offset means no slots are cut**, and the reason is observable.
- A short slot is discarded and the discard count is observable (§0.0.1).
- Pure over samples and an elapsed time, tested without a wall clock.
- **Nothing consumes the slots yet.** This is the seam the decoder plugs into.

### Task 8 — the Costas sync search *(the drop candidate)*

The first stage of the C# decoder, running on **the FFT frames, not the drawn
bitmap.**

- FT8 carries **three Costas arrays of seven symbols** at the start, middle and
  end of each transmission. Search a slot's frames for that pattern across
  candidate time and frequency offsets.
- **Report candidates, not messages.** Each carries a frequency, a time offset and
  a sync score. **Nothing goes on the decoded-text panel** — no message has been
  read, and HM-DEC-009 is absolute here.
- **Mark located candidates on the waterfall** if that is cheap; skip it and say
  so if it is not.
- Tested against a fixture. **A synthesised FT8 slot is acceptable as a unit test
  and is not evidence about yield** — say which it is.

**Dropped whole if time runs out, and the report says so.**

## Parked — do not touch, do not raise

The whole CW decoder stream and unit 036's residue. The CW capture path itself.
The scanner and the calling cycle. `CHANGELOG.md`. The missing `DECISIONS.md`
records including HM-DEC-086's supersession. The phrasebook and the recent-places
row. The prefix table and the plain-English parser. The decoded-text panel's
placeholder rows. The mode strip's static status.

**Both halves are required: do not touch them, and do not raise them.**

A parked item that genuinely blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **No transmit. Nothing keys the radio.**
- **Do not call `MarkCase` or touch `CwCaseRoster`.**
- **Do not edit the CW capture path, the CW decoder, or the CW markup.** If
  something must be factored out to be reused, **stop and report.**
- **Do not claim to have cleared the PBT.** There is no write for it.
- **Do not write a filter byte as a constant.** Derive it and be able to state
  why.
- **Do not build trimming or slot-aligned capture.**
- **Do not put anything on the decoded-text panel.**
- **Do not report a sync candidate as a decode**, on the screen or in a log.
- **Do not trade the waterfall's determinism across chunk sizes.**
- **Do not code against a manual value this order states without re-reading it.**
- **Do not mint a decision id.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 to `output.md` at the repository root, overwritten
and printed. **Read the file's own section count and follow it.**

**The section that says what the owner should expect leads with this: clicking
into FT8 now puts the radio in USB-D on a filter wide enough to hear the block,
the card says where the signals actually are, and the capture press writes a file
whose sidecar would have identified today's failure in one line.**

**The section that reports measurements leads with the engine's failing count**,
then the manual re-read — every value in this order that the manual confirmed, and
every one it corrected.

**If you finish every task, stop and report. Do not start the next unit.**
