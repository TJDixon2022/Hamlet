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

# Work instruction 037 — the Digital tab gets its screen

**ISSUED: 2026-08-28. A fresh order, not an amendment. Follows unit 036.**

**Six tasks; task 6 is the drop.**

## Why this unit exists

**The Digital tab is empty.** Clicking it today gives a blank rectangle where CW
gives a send panel and a terminal. There is no number to lead with because
nothing has ever been drawn there.

A new phase opened this morning with the goal of interpreting one digital mode
at ninety-nine percent, and FT8 is the mode. **This unit builds none of that.**
It builds the screen that work will fill, drawn with static placeholder text, so
the operator can look at a finished-looking FT8 session and say what is wrong
with it before any decoder exists to argue with.

**Nothing in this unit is live. Nothing decodes. Nothing moves.** The strings are
hardcoded and they never change.

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and
report any mismatch, including where the work succeeded anyway.

**This author's picture of the tree was stale when this order was written and was
corrected once already this session.** The canvas of HM-DEC-086 was assumed to
still exist; the listing shows `src/Hamlet.App/Layout/` gone,
`WidgetCanvas`/`WidgetFrame`/`LayoutPresets` gone, and
`tests/Hamlet.App.Tests/Layout/TheCanvasIsGoneTests.cs`,
`TheTabOwnsTheWorkspaceTests.cs` and `TheTabsAndTheWorkspacesTests.cs` present as
of 2026-08-27. **The ruling that retired the canvas is not in this author's
hands.** Trust the tree, and trust that ruling, over this order everywhere they
differ. List the differences.

**In particular:** this order says *fixed panels the tab owns*. If the current
ruling names them something else, or gives tabs a different ownership model, **use
the tree's model and say so** rather than reintroducing anything canvas-shaped.

**Expected state, carried from unit 036 and not measured by this author:** read
the failing counts from the tree itself before task 2, and record them, because
this unit must not change them.

## Rulings in force

**Transcribed in full with what was rejected. Do not re-argue either.**

**Tim's rulings, 2026-08-28, this morning's design conversation:**

> **The digital phase's mode is FT8**, and its goal is interpreting it at
> ninety-nine percent.
>
> Rejected: PSK31 — writable from scratch but nearly dead on the air, so a
> decoder with nothing to decode cannot be scored. WSPR — beacons only, nobody
> talks. FT4 — strictly less active for the same work. RTTY stays off the list
> per FG-012.

> **The decoder is wrapped first and written in C# afterwards.** Tim stated he
> believes writing it in C# from the beginning is best, and accepted the wrap-then-
> write order on the understanding that **if the wrapper costs more than a session
> or two of fighting native binaries and marshalling, it is abandoned and the C#
> implementation begins directly.**

> **The only measurement is against real data from the real radio.** Synthetic
> audio may exist as unit tests during development; it never appears in the
> phase's score.
>
> Rejected: PSKReporter spots as a denominator — different antennas, different
> noise floors, not reproducible. Synthetic slots as the exit criterion — a
> decoder can score perfectly on generated audio and fall over on a real band.

> **The capture press keeps the last four complete slots, trimmed to UTC
> boundaries.** Two slots is not enough to argue about yield; four shows a missing
> station as a gap rather than as bad luck, and catches both sides of an exchange.
>
> Rejected: the raw thirty seconds the CW press keeps — partial slots are
> unscoreable, so a third of each file would be dead weight.

> **UTC is measured and displayed, never corrected.** One SNTP query, the offset
> shown, and trimming refuses while the offset is unknown.
>
> Rejected: trusting the PC clock silently — a drifted clock produces trimmed
> files that are quietly wrong. Disciplining an internal clock — two notions of
> time in one app, and sidecar timestamps that disagree with the file's own mtime.

> **The Digital tab's mandatory elements are four**: the mode list, the waterfall,
> the decoded text, and what people are saying. They are fixed panels the tab
> owns, not widgets.

> **The mode list is a strip across the top of the workspace**, not a left column.
>
> Rejected: a left column matching CW's Send — four short items and a status is a
> row, not a column, and a column would narrow the waterfall for nothing. Rejected:
> putting the mode in the tab header — no room for the other modes, which loses the
> idea entirely.

> **The decoded text panel sits above what people are saying.** Tim is more
> interested in the text and expects to switch between them.

> **The raw decoded line appears under each plain-English message as well**, so the
> literal and the interpretation can be compared at a glance.

> **A decoded callsign's country comes from a vendored prefix table; anything
> deeper is a callook lookup on demand.**
>
> Rejected: callook per callsign — US only, so most DX shows nothing, and dozens of
> requests a minute is not the polite use HM-DEC-028 committed to. Rejected: no
> lookup at all. Tim noted the prefix table is a static file and accepted it on the
> grounds that ITU prefix blocks move every few years rather than every few months,
> and that when it is wrong it is wrong by omission.

> **The capture press sits on the waterfall panel's header, and it eventually comes
> off the screen** — it is a development instrument, not a permanent control.
>
> Rejected: under the decoded text panel matching CW's "I hear a station" — it is
> buried when that panel is collapsed, and the waterfall is where a signal that
> decoded nothing is visible.

> **Each panel says what it is waiting for when nothing has been heard**, matching
> the CW terminal's own idle line.
>
> Rejected: empty panels — indistinguishable from broken. Rejected: one idle
> message for the whole tab — a collapsed panel loses it.

> **Match CW where possible.** Chrome, voice, wording and behaviour carry across
> unless there is a reason they cannot.

> **This unit is limited to the Digital tab for the duration.**

**Standing rulings this unit inherits and must not break:**

- **HM-DEC-032** — one mode colour language, defined once in `ModePalette`.
  Digital is lavender. Colour is never the only carrier of meaning.
- **HM-DEC-021** — every panel collapses, state persists, and a collapsed panel
  still carries its summary on the header.
- **HM-DEC-087** — a binding that does not resolve is a defect, not a
  diagnostic. A control's resting look says press me; grey is reserved for
  genuinely unavailable.
- **HM-DEC-009** — never present a guess as a decode.

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` — `STATE`,
`TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what is moving
inside the task, not restating the task name. Same every ten minutes while a task
runs.

## The tasks

### Task 1 — trace before building

**Say what you find rather than confirming this list.** This author has not seen
the tree since the canvas was removed.

Answer from the code:

- **How does a tab own its workspace?** What `TheTabOwnsTheWorkspaceTests` and
  `TheTabsAndTheWorkspacesTests` assert, and what the Digital tab currently
  resolves to when clicked.
- **How is the CW tab's workspace laid out** — the narrow Send column and the wide
  terminal panel — and in which file. What `TheOperatingScreenIsLaidOutAsRuledTests`
  requires of it.
- **What `CollapsiblePanel` needs** to be given a header, a family colour and a
  right-aligned summary, and how CW's panels supply those.
- **What `ModePalette` exposes for the digital family**, and whether a panel can
  take that family without a new brush being written in C#.
- **What `WaterfallControl` requires today** — its data source, its frequency
  axis, and whether it can be placed and drawn with no live spectrum without
  throwing or drawing something misleading.
- **What the CW capture press is wired to**, and whether that command is reusable
  from another tab as it stands.
- **Which tests will build the window headless and check bindings, resources,
  clipping and layout** — name them, so it is known which ones a new panel has to
  satisfy.

**If any of this already exists in a form this order does not anticipate — a
Digital view model, a placeholder panel, a stub — say so.** That is tokens back.

Report the findings before writing a line of markup.

### Task 2 — the tab's shell and the mode strip

The Digital tab's workspace becomes a vertical stack: the mode strip, then three
panels.

**The mode strip** runs the full width above everything and does not collapse. It
carries the label `on this frequency`, then four mode names in order — **FT8,
FT4, PSK31, WSPR** — and a status at the right end.

- **FT8 is drawn lit**: the digital family's fill, a border, the name in the
  family's dark ink. The status at the right reads `reading it · 9 messages this
  slot`.
- **FT4, PSK31 and WSPR are drawn unlit**, in muted ink, with no fill and no
  border.
- **Lit and unlit differ by more than colour** (§0.6 / HM-DEC-032): the lit one
  carries the border and the weight, so it survives a greyscale print.
- All of it is hardcoded. Nothing computes which mode is lit.

**Both inks come from `ModePalette`.** If the digital family does not already
expose an ink dark enough to clear contrast on its own tint, **report it rather
than writing a hex literal** — a second copy of the palette is what HM-DEC-032
exists to prevent.

**Acceptance:** clicking Digital shows the strip; the window still builds headless
with every binding resolving; the failing-test count recorded in task 1 is
unchanged.

### Task 3 — the waterfall panel and the capture press

A `CollapsiblePanel` in the digital family, header `Waterfall`, summary
`200–3000 Hz · 15 s slots`.

- The body holds `WaterfallControl` **if task 1 found it can be placed with no
  live source without throwing or drawing something that asserts a signal.** If it
  cannot, **draw nothing in the body and say so in the report** — do not
  synthesise spectrum to fill it, and do not write a second waterfall.
- **The capture press sits on the panel's header**, right-aligned, before or after
  the summary as the control allows. Its label is the CW press's wording adapted:
  it keeps the last four complete slots.
- **The press is not wired in this unit.** If task 1 found the CW capture command
  directly reusable, say so in the report and leave it unwired anyway — the
  trimming rule it must obey does not exist yet, and a press that keeps the wrong
  thing is worse than one that does nothing.
- A pressable-looking control that does nothing is acceptable **only because
  every string on this tab is placeholder**. Note it in the report.

**Acceptance:** the panel appears, collapses to its header, and its collapse state
survives a restart the way CW's do.

### Task 4 — the decoded text panel

A `CollapsiblePanel` in the digital family, header `Decoded text`, summary
`14:22:45 UTC · 4 shown`.

Monospaced, five columns, a muted column header row, then four static rows:

```
utc     snr   dt    hz    message
142245  -19  -0.4  1502   W9XYZ K1ABC -13
142230  -13   0.2  1240   CQ K1ABC FN42
142230  -08   0.1  1875   CQ DX EA3QQ JN11
142215  -21   0.6  2310   VE7AA N0RR RR73
```

The first row is drawn highlighted, as the selected one. **Nothing selects
anything** — it is drawn that way so the operator can see what selection will look
like.

Columns line up under their headers. **The message column is the one that must
never be truncated**; the numeric columns may be narrowed if the window forces it.

### Task 5 — what people are saying

A `CollapsiblePanel` in the digital family, header `What people are saying`,
summary `4 stations · 2 contacts running`. **It sits below the decoded text
panel.**

Three static entries, each a left accent rule, a sentence in ordinary text size,
a second line of detail in muted ink, and the raw decoded line in monospace
beneath:

1. Highlighted, in the same treatment as the selected decode row:
   `W9XYZ is answering K1ABC` / `and telling him he's coming in weak` /
   `W9XYZ K1ABC -13`
2. `K1ABC is calling anyone` / `Massachusetts · about 480 miles · strong` /
   `CQ K1ABC FN42`
3. `EA3QQ is calling for distant stations` / `Spain · about 3,900 miles · strong`
   / `CQ DX EA3QQ JN11`

**On entry 3, the word `Spain` carries a small mark reading `from the prefix
list`**, in the shape the settings window already uses for a provenance pill. That
mark is the whole reason this entry is in the placeholder set: it is what a
looked-up fact looks like beside facts that came off the air.

**The voice is the app's own** (HM-DEC-034, §0.7) — connected speech, not a stack
of fields. If any of the wording above reads wrong beside the CW panels, **write
it as given and put your alternative in the report**; the wording is Tim's.

### Task 6 — the idle states *(the drop candidate)*

Each of the three panels and the mode strip gets a second, idle appearance saying
what it is waiting for, in the CW terminal's voice — its line is
`listening to Training radio. Nothing decoded yet.` and these should read as
having been written by the same hand.

Static, like everything else: **the idle text exists in the markup but the tab
shows the busy version.** Put the idle strings where the next unit will find them
and say in the report where they are.

**Dropped whole if time runs out, and the report says so.**

## Parked — do not touch, do not raise

Everything outside the Digital tab. Specifically: the CW decoder work of unit 036
and its residue; the phantom characters; the tracker and admission questions of
tasks 3 and 4 of that unit; the sheet's two spans; the thirty carried asks; the
scanner and the calling cycle; `CHANGELOG.md`; the missing `DECISIONS.md` records
including HM-DEC-086's supersession; the phrasebook and the recent-places row.

**Both halves are required: do not touch them, and do not raise them.** They are
real and they are each their own unit. **This unit is the Digital tab and nothing
else** — that is an explicit instruction from Tim, for the duration.

A parked item that genuinely blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **Do not edit the CW tab.** Not its markup, not its styling, not its view model.
  If something in it must be factored out to be reused here, **stop and report**
  rather than doing it — its decoder is mid-repair and a change there is one Tim
  has to re-judge.
- **Do not build anything live.** No decoder, no audio, no SNTP query, no slot
  timer, no capture wiring, no prefix table. Every string is static.
- **Do not synthesise signals to fill the waterfall.**
- **Do not write a colour literal.** Both inks come from `ModePalette`.
- **Do not reintroduce widgets, a canvas, drag, or a layout store.** The panels
  are fixed and the tab owns them.
- **Do not change the failing-test count** recorded in task 1.
- **Do not mint a decision id.** Any ruling this unit needs goes in the report's
  decision section for Tim to enter.

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 to `output.md` at the repository root, overwritten
and printed. **Read the file's own section count and follow it** — unit 036 found
it at version 1.6 with twelve sections; do not assume that is still true.

**The section that says what the owner should expect leads with this: clicking
Digital now shows a full FT8 session — mode strip, waterfall, decoded text and
plain English — and every word of it is placeholder that will not change until a
decoder exists.**

**The section that reports measurements leads with task 1's trace**: how the tab
owns its workspace, and every place this order's picture of the tree was wrong.

**If you finish every task, stop and report. Do not start the next unit.**
