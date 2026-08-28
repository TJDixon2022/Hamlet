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

# Work instruction 041 — the press, then the mode, then the readout

**ISSUED: 2026-08-28. A fresh order, not an amendment. Follows unit 040.**

**Seven tasks; task 7 is the drop. This is a long unit by instruction —
the 45-to-60-minute window is a floor against trivial units, not a ceiling.**

## Why this unit exists

**The operator graded this afternoon F-minus and the grade is accepted.** Four
defects, in his words:

1. **He hears whale song** — FT8 through the speaker — **and the waterfall shows
   dense speckle rather than the dashes FT8 makes.**
2. **The rig readout says `USB`, not `USB-D`.**
3. **The capture button does nothing.**
4. **Switching to CW puts the radio in CW mode. Switching back to Digital does not
   restore USB-D.**

**The third is the one that makes the first unanswerable.** Without a WAV and a
sidecar, every complaint about the waterfall is a description of a picture, and
this author has now twice reached a wrong conclusion from a screenshot — once
missing `CW`/`FIL2` in the readout, once building a work order on the premise
that no signal could produce what was drawn.

**The capture press has been ordered in units 038, 039 and 040 and dropped from
all three.** It was last in every one. **It is first in this one, and that is the
only protection that has ever worked.**

**The second and fourth are the same organ**: Hamlet already knows the radio is in
USB-D — the "What the radio is doing" window reads `Data mode: on` from `26 00`
and displays it — but the readout does not render it, and the write that
establishes it does not fire on every entry into Digital.

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches.
Trust the tree over this order everywhere they differ.

From unit 040's report, not measured by this author:

- **Engine 28 of 1916, byte-identical to the stable set. App 509 of 509.**
  039's extra, `AConfirmedModeWriteFoldsTheDataVariantTooAsync`, does not appear
  and is confirmed an intermittent.
- The mode write is at **`CivWrites.cs:101`** and now carries the filter byte,
  derived from the block's own width. The passband is established by the `1A 03`
  readback and is **unknown until it arrives.**
- **12 neighborhood blocks state a passband; 93 state none.**
- The CW sidecar already carries `Mode`, `FilterSelection` and `FilterBandwidth`.
- **`IC-7300_ENG_FM_12b.pdf` is not on the machine** and §2.1 forbids committing
  it. `CLAUDE.md` §4 carries command `26` and the filter scale, verified
  column-aware on 2026-08-14, and `1A 03` reads the radio's actual passband in
  hertz — **which is a better source than any table of defaults.**
- **Unit 040's tasks 4, 6, 7 and 8 were not started.** Tasks 1, 4, 6 and 7 of this
  order are those, reordered.

**Record the failing counts from the tree before task 2.**

## Rulings in force

**Transcribed with what was rejected. Do not re-argue either.**

**Tim's rulings, 2026-08-28:**

> **The order of work is: fix the capture button, then fix the automatic USB-D
> setting, then the UI defects.** This is his sequencing, given after the F-minus.

> **The filter write is made once per tune-in and then hands off** — the middle
> option of unit 040's question B, as that unit built it.
>
> Rejected: treating a hand-turned filter the way HM-DEC-056 treats a hand-turned
> mode, suspending the write until the next band change — a filter left narrow in
> a previous session would then silently defeat the fix, which is today's failure
> returning by a different door. Rejected: always writing it — that takes the
> filter knob away on a tab where narrowing onto one signal is normal operating.
>
> **Tim's reason:** a tune-in is an explicit act of arriving somewhere new, and
> re-establishing a filter wide enough to hear what is there is part of arriving.

> **The digital capture gets its own record and its own folder** — `captures\digital\`,
> separate from `CwCaseRoster`. **`MarkCase` is not called.**

> **The digital press works the way the CW one does** — same ring, same window,
> same file shape. **No trimming, no slot alignment.** These files are diagnostic
> material, not corpus; trimming returns when scoring starts.

> **The point of the capture is to give a WAV to match against a screenshot.**

> **The decoder is written in C#, not wrapped.**

> **Static strings unit 037 wrote stay as written until they are live.**

**Standing rulings this unit is bound by:**

- **§0.0 / HM-DEC-009** — never present a guess as a decode; this binds pictures
  as hard as sentences (HM-DEC-092).
- **§0.0.1** — **the app's own record must be enough to tell whether a fault is in
  the signal, the radio, or Hamlet itself. Task 1 is this principle and nothing
  else.**
- **HM-DEC-056** — the operator's own hand wins on the mode and suspends the write
  visibly until the next band change; a value the radio did not confirm is
  unknown, not assumed.
- **§0** — generate from the source of truth; no constants sprinkled through code.
- **§0.2 / HM-DEC-008** — **no transmit work of any kind.**

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` — `STATE`,
`TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what is moving
inside the task. Same every ten minutes while a task runs.

## The tasks

### Task 1 — the capture press works *(first, and not droppable)*

**Nothing else in this unit starts until a press writes a file.**

Trace what you need inside this task rather than before it: the CW capture path,
what it writes, what `MarkCase` appends, and which parts are reusable without
touching it. **Report what you find as part of this task.**

The press on the waterfall header writes to **`captures\digital\`**:

- **Its own record, separate from `CwCaseRoster`. `MarkCase` is not called and the
  CW capture path is not edited.**
- Same ring and same window length as the CW press. **No trimming, no slot
  alignment.**
- Filenames distinguish these from CW captures.

**The sidecar is the point of the task.** §0.0.1 asks whether a later reader can
tell if a fault was in the signal, the radio, or Hamlet. At minimum it carries:

- **Mode, and the data flag separately** — `USB` and `USB-D` must not be the same
  line, because that ambiguity is what cost this author an hour today.
- **Filter slot, and the width in hertz from `1A 03`.**
- Dial frequency, S-meter, preamp, attenuator, AGC, noise blanker, noise
  reduction, front-end overload — **whatever the "What the radio is doing" window
  already reads.** That window is §0.0.1 working; the sidecar should hold the same
  set.
- **The clock offset and its age.**
- **That the file is untrimmed**, so a later scoring run can tell diagnostic
  material from corpus without opening the audio.
- **Every value marked measured or unknown. Nothing defaulted silently** (§0.0).
  A row nobody could read says so, exactly as that window already does.

**Acceptance:** a press writes a WAV and a sidecar the operator can find, and
**the sidecar alone identifies the radio's mode, data flag and passband width** —
the three fields whose absence has cost two hours today.

### Task 2 — entering Digital restores USB-D

**Switching to CW puts the radio in CW. Switching back to Digital does not restore
USB-D.** The two directions are not symmetric and they must be.

- Find why. **Report the cause with file and line** — whether the write does not
  fire on the return, fires without the data flag, or fires and is not confirmed.
- **Whatever establishes CW on entering the CW tab is what should establish the
  digital mode on entering Digital**, with the filter byte unit 040 added.
- **HM-DEC-056 still governs**: the operator's own hand wins, a value the radio did
  not confirm is unknown rather than assumed, and the suspension is visible.
- **The mode written on entering Digital is the one the current neighborhood
  calls for**, generated from the band-plan row, not a constant (§0).

**Acceptance:** from CW at 14.074, switching to Digital leaves the radio in USB-D
on a filter wide enough for the block, confirmed by readback — and switching back
and forth repeatedly does not drift.

### Task 3 — the readout says USB-D

The rig readout renders `USB` while Hamlet holds `Data mode: on` from `26 00` and
displays it correctly in the "What the radio is doing" window. **Two surfaces
disagree about the same measured fact.**

- The readout shows the data variant. **`USB` and `USB-D` are different modes to
  the operator and must look different at a glance.**
- **If the data flag has not been read, the readout says the mode is unknown in
  that respect rather than showing the bare mode** — showing `USB` when the flag
  is unread is the guess §0.0 forbids, and it is exactly the guess that misled a
  reader today.
- The same applies to CW: whatever `26` reports is what is shown.
- **Do not invent a new colour or badge language.** Use what the readout already
  has.

### Task 4 — the Twin PBT, seen but never claimed

**There is no write for this control and the app must not claim to have cleared
something it cannot clear.**

- Read the outer position — `CLAUDE.md` §4 records `14 08`, and records it as the
  row once mistaken for the CW pitch, so **treat it column-aware.**
- **Whether the inner control can be read is unknown and the manual is not on the
  machine.** Check `SHACK_FACTS.md` first — a fact there outranks any inference
  (HM-DEC-093). **If neither answers it, that is an explicit unknown in the ledger,
  not an assumption that the inner is centred.**
- A PBT away from centre narrows the effective passband below whatever the slot
  gives. Hamlet says so in the app's voice and names the remedy: hold
  `TWIN PBT CLR` for one second until the dot beside the width disappears.
- **It suppresses the "you should hear the block now" claim.**

**Raise, do not decide:** whether an *unreadable* inner PBT suppresses that claim
or only qualifies it. Unit 040 costed three options and had no recommendation
because the deciding fact needs the manual. **Carry that table forward with
whatever this session learns added to it.**

### Task 5 — regression fixtures for the tab switch

The failure is a state transition, so the fixtures are transitions:

- **CW tab at 14.074, switch to Digital** — must end USB-D, wide enough.
- **Digital, switch to CW, switch back** — must end where it started.
- **Digital with the operator's hand having changed the mode** — HM-DEC-056's
  suspension, visible.
- **Digital where the readback never confirms** — passband and data flag unknown,
  and no readiness claim.

**A test asserts that entering a tab leaves the radio in a state that tab can
actually work in, or says it does not know.**

### Task 6 — the slot cutter

Cut the audio into **15-second slots aligned to UTC quarter-minutes**, using the
clock offset.

- **Unknown offset means no slots are cut**, and the reason is observable.
- A short slot is discarded and the discard count is observable (§0.0.1).
- Pure over samples and an elapsed time, tested without a wall clock.
- **Nothing consumes the slots yet.**

### Task 7 — the Costas sync search *(the drop candidate)*

The first stage of the C# decoder, on **the FFT frames, not the drawn bitmap.**

- Three Costas arrays of seven symbols at the start, middle and end of each
  transmission. Search a slot's frames across candidate time and frequency
  offsets.
- **Report candidates, not messages** — frequency, time offset, sync score.
  **Nothing goes on the decoded-text panel.**
- Mark located candidates on the waterfall if cheap; skip and say so if not.
- Tested against a fixture. **A synthesised FT8 slot is a unit test and is not
  evidence about yield.**

**Dropped whole if time runs out, and the report says so.**

## Parked — do not touch, do not raise

The whole CW decoder stream and unit 036's residue. The CW capture path itself.
The scanner and the calling cycle. `CHANGELOG.md`. The missing `DECISIONS.md`
records. The phrasebook and the recent-places row. The prefix table and the
plain-English parser. The decoded-text panel's placeholder rows. The mode strip's
static status. **The waterfall's rendering** — it is under suspicion and task 1
exists to produce the evidence, but **nothing about it changes in this unit.**

**Both halves are required: do not touch them, and do not raise them.**

A parked item that genuinely blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **No transmit. Nothing keys the radio.**
- **Do not start task 2 until task 1 writes a file.**
- **Do not call `MarkCase` or touch `CwCaseRoster` or the CW capture path.**
- **Do not change the waterfall's rendering, floor, or colour ramp.**
- **Do not show a bare mode when the data flag has not been read.**
- **Do not claim to have cleared the PBT.**
- **Do not write a mode or filter as a constant.** Generate from the band-plan row.
- **Do not build trimming or slot-aligned capture.**
- **Do not put anything on the decoded-text panel.**
- **Do not report a sync candidate as a decode.**
- **Do not code against a manual value without a source in the tree.**
- **Do not mint a decision id.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 to `output.md` at the repository root, overwritten
and printed. **Read the file's own section count and follow it.**

**The section that says what the owner should expect leads with this: the capture
press writes a WAV and a sidecar, and the sidecar names the mode, the data flag
and the passband width in hertz.**

**The section that reports measurements leads with the engine's failing count,
then task 2's cause with file and line** — why entering Digital did not restore
USB-D.

**If you finish every task, stop and report. Do not start the next unit.**
