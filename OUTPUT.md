UNIT: 040 — stopped at task 5 of 8 — 2026-08-28 14:27

## 1. What Claude did

**Stopped. Tasks 1, 2, 3 and 5 are done. Tasks 4, 6, 7 and 8 were not started.**

Claude Code, on the development computer, in `C:\Source\HamLet`. The prompt
claimed `PROJECT: Hamlet`; the tree confirmed all four checks — `SHACK_FACTS.md`
and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` exist, neither
`CoreHMI.sln` nor `MURC.sln` does. **Hamlet confirmed.** Branch `main`
throughout, four commits, all pushed, none refused. Version 1.12.2 to 1.12.3.

**Nothing here is evidence about the radio.** No rig was connected. **Nothing
transmitted, and no rig setting was written by this session** — what changed is
what the tune-in write *will* send when the operator next uses it.

**No decision was recorded under §12.1.** Section 4 carries what needs a ruling,
including the two questions task 4 asks to be raised in options-table form.

**Only task 8 was the named drop, so dropping 4, 6 and 7 as well is a sizing
decision the owner did not make**, and §8 requires it reported as one. This is
it. The hour the operator lost is fixed; the diagnosability half of the unit is
not.

### The manual is not on this machine, and the order forbids coding without it

`IC-7300_ENG_FM_12b.pdf` **is not in the tree and is not on this machine.** §2.1
forbids committing it, so that is correct rather than an oversight — but it means
**the per-mode filter default table the order asks to have confirmed cannot be
confirmed, and nothing was coded against it.**

**It turned out not to be needed, and that is the useful half of the finding.**
`CLAUDE.md` §4 already carries command `26` and the filter scale, verified
column-aware against `A7292-4EX-6` on 2026-08-14. And the application already
reads the radio's **actual** passband in hertz through `1A 03` and
`CivFilterWidth`. **The width the radio really has is a better source than any
table of defaults**, and it is what the code now uses.

**What could not be answered**: whether `14 08` has a companion sub-command for
the *inner* Twin PBT. §4 records `14 08` as the outer position and nothing more.
That is task 4's central question and it is unanswerable here.

## 2. What the owner should expect

**Tuning into an FT8 block now sets the filter as well as the mode, in the same
frame, and the card tells you the dial will sound dead.**

That is the hour you lost, in two parts. The radio landed in CW/FIL2/500 Hz
because **the mode write deliberately skipped the filter byte** — its own comment
called letting the radio choose "a better answer than any Hamlet could invent" —
and skipping it was never neutral: the manual's note is that omitting the
trailing bytes selects the mode's *default* filter rather than leaving it alone.
**The byte was already being sent. Nobody was choosing it.**

**And the card now says the thing nobody told you**: tune to 14.074 and expect
the dial itself to sound dead, because everybody transmits in the 3 kHz above it.
On 80 m it says 2 kHz, because that block is 2 kHz.

**What has not changed:** the PBT is still invisible to Hamlet, and the capture
press still does nothing. Both are in section 4.

| | before | after |
|---|---|---|
| engine | 29 of 1914 (039) | **28 of 1916, byte-identical to the stable set** |
| app | 509 of 509 | **509 of 509** |

**The engine is back to 28** and the failing set is byte-identical by name.
`AConfirmedModeWriteFoldsTheDataVariantTooAsync`, unit 039's extra, **does not
appear** — which confirms it as the intermittent 039 judged it rather than a
regression.

## 3. What you should see

### The defect, with the line

**`CivWrites.cs:101`.** The mode write built `26` with the VFO selector, the mode
and the data flag, and stopped. Its own documentation said so:

> "Hamlet sends the data flag and skips the filter, so the radio picks the filter
> it would have picked for that mode itself, **which is a better answer than any
> Hamlet could invent for somebody else's rig.**"

**That reasoning is what left the radio too narrow to hear the block it had just
been tuned to.**

### What replaced it

**One frame either way.** Command `26` already carries the filter byte, and
skipping it selects the mode's default rather than leaving the filter alone
(p. 19-11, already in §4). So choosing costs nothing on the wire.

**Choosing a slot is not knowing a width, and the code says so.** FIL1 is the
widest slot by the manual, but what it opens onto is whatever the operator
configured it to be. So the passband is established by the `1A 03` readback and
is **unknown until it arrives** — the same rule HM-DEC-056 already applies to the
mode.

**The question is three-valued**, and the four states of task 5 answer it:

| radio state | answer |
|---|---|
| CW / FIL2 / 500 Hz — the failure | **too narrow** |
| USB-D / FIL2 / 1.2 kHz — what the old write could leave | **too narrow** |
| USB-D / FIL1 / 3.0 kHz | **wide enough** |
| the filter has not been read | **unknown** |

**A filter nobody has read is not a filter that is wrong**, and saying either
would be a guess on the one sentence the operator acts on.

### The map walk caught my own mistake on its first run

I added `passbandHz: 3000` to every FT8 row. The walk immediately found that
**the 80 m FT8 block is 2 kHz wide** — so the file was claiming a radio needed
more passband than the block occupies.

**The requirement is derived from the block's own width now**, and the file
carries a flag saying *why* rather than a number that can disagree with it. That
is §0: generated from the source of truth, not typed beside it.

| band | block | needs |
|---|---|---|
| 80 m FT8 | 1999 Hz | 1999 Hz |
| 20 m FT8 | 2999 Hz | 2999 Hz |
| 40 m FT4 | 2999 Hz | 2999 Hz |

**12 blocks state a passband; 93 state none**, and a block stating none produces
no claim about the radio in either direction.

### What the card says now

> *Tune to 14.074, and expect the dial itself to sound dead. Everybody here
> transmits somewhere in the 3 kHz above it, arriving as audio tones rather than
> on the frequency you set, so the band comes alive across the whole block at
> once.*

On 80 m the same sentence says **2 kHz**. Every number is the row's own.

### §0.0.1's question, answered

**Could a session tomorrow tell from Hamlet's own files that the radio was in
CW/FIL2/500 Hz today?** The CW sidecar carries `Mode`, `FilterSelection` and
`FilterBandwidth` — so **for a CW capture, yes.** But the Digital tab has no
capture at all, so **for the screenshot that started this, no**: there was no file
because there is no press. **That is task 6, and it was not reached.**

## 4. What's blocking us

**Two questions task 4 asks to be raised rather than decided, in options-table
form.**

**A. Does an *unreadable* inner PBT suppress the readiness claim, or qualify it?**

| | what it does | for | against |
|---|---|---|---|
| **Suppress** | no "you should hear it" claim while the inner PBT is unknown | the conservative reading of §0.0 — a claim the operator acts on, made over a control that could be closing his window | **fires on every radio where that read does not exist**, which may be all of them, making the claim permanently unreachable |
| **Qualify** | claim it, and say the PBT was not readable | the claim is still useful and the caveat is honest | a caveat beside a confident sentence is read past, which is how HM-DEC-092's picture problem works |
| **Read the outer only** | claim it when the outer is centred, unknown otherwise | outer is `14 08` and verified | the inner alone can close the window, so a centred outer proves nothing |

**I have no recommendation, because the deciding fact — whether the inner is
readable at all — needs the manual I do not have.**

**B. Does the filter write belong to the operator's hand the way the mode does?**

| | what it does | for | against |
|---|---|---|---|
| **Yes, same as mode** | a filter turned by hand suspends the write until the next band change (HM-DEC-056) | **narrowing onto one signal is a deliberate, skilled act**, and overriding it is the app fighting the operator | a filter left narrow from a previous session then silently defeats the fix this unit just made |
| **No, always write it** | every tune-in sets the widest slot the block needs | the failure that cost an hour cannot recur | takes the filter knob away on a tab where narrowing is normal |
| **Write once per tune-in, then hands off** | set it on arrival, never again until the next tune-in | matches what a person would do | the "next tune-in" boundary is not the same as HM-DEC-056's "next band change" |

**Built as the middle one**, because the write only happens on a tune-in and
there was no existing suspension to inherit. **If that is wrong it is a small
change**, and it is the one place this unit may have overreached.

---

**Tasks 4, 6, 7 and 8 were not started.**

Task 6 is the one that matters most: **the sidecar is what §0.0.1 asks for, and
without it a screenshot is still a description rather than evidence.** It is
unblocked — unit 039 established that `MarkCase` must not be called and this
order rules the digital capture gets its own record and folder, so the design
question is settled and only the writing remains.

### Asks still outstanding

**Carried forward per HM-DEC-139 and HM-DEC-140, and deliberately not restated.**
The order parks the CW stream, the CW capture path and the carried asks, and says
*both halves are required*. **The thirty-one asks from unit 1.11.34's list stand
unchanged.**

**Carried from unit 038, still open:**

1. **`ft8_lib` cannot be built here** — no C toolchain. **This order confirms the
   decoder is written in C#.**

**New this unit:**

2. **The inner PBT's readability**, above — needs the manual.
3. **Whether the filter write belongs to the operator's hand**, above.
4. **The manual named in the order is not obtainable from this machine**, so any
   future task requiring a fresh page read has to be met some other way.
5. **Tasks 4, 6, 7 and 8 not started**, above.

**Closed this unit:** **why the radio was too narrow** — the mode write skipped
the filter byte, and skipping it selects the mode's default rather than leaving
it alone. **Whether the map can say what a block needs** — yes, derived from the
block's own width, walked across every band. **Whether 039's extra failure was a
regression** — it was not; the engine is byte-identical at 28.
