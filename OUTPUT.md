UNIT: 037 — complete at task 6 of 6 — 2026-08-28 11:21

## 1. What Claude did

**Complete. All six tasks ran, including the drop.**

Claude Code, on the development computer, in `C:\Source\HamLet`. The prompt
claimed `PROJECT: Hamlet`; the tree confirmed all four checks — `SHACK_FACTS.md`
and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` exist, neither
`CoreHMI.sln` nor `MURC.sln` does. **Hamlet confirmed.** Branch `main`
throughout, four commits, all pushed, none refused.

**Nothing here is evidence about the radio.** No rig was connected, and nothing
in this unit could involve one — every string is static.

**One thing was recorded for myself under §12.1, and it is flagged for
override:** the version moved **1.11.34 to 1.12.0** rather than to 1.11.35.
HM-DEC-150 says the minor is the phase and a phase bumps it and resets the patch;
the order opens by saying a new phase began this morning. That reasoning runs one
way and supersedes nothing, but **if the digital phase is not meant to start its
count here, the number is one line.**

**No engine file was touched.** `git diff` over this unit's commits against
`src/Hamlet.RadioEngine/` reports **zero files**. **No CW markup was touched
either** — the only lines removed from `MainWindow.axaml` are the comment saying
Digital is empty and the empty `<Grid/>` itself.

### Task 1 — the trace, answered from the code

**The order was right to doubt its own picture, and wrong in the way it
expected.** Nothing canvas-shaped remains; the model is simpler than the order
assumed.

- **How a tab owns its workspace.** `WorkspaceBoundary` is a `Border` holding one
  `Panel` with three sibling grids — `CwWorkspace`, `DigitalWorkspace`,
  `VoiceWorkspace` — each shown by `IsCwMode` / `IsDigitalMode` / `IsVoiceMode`.
  The boundary is the same shape whichever tab is selected; only its contents
  change. **`DigitalWorkspace` was an empty `<Grid/>`** — no view model, no
  placeholder, no stub. Nothing to reuse and nothing to undo.
- **How CW is laid out.** `CwWorkspace` is `ColumnDefinitions="Auto,*"`:
  `SendPanel` fixed at 320, `ReceivePanel` taking the rest and hosting the
  `widget.terminal` template. All in `MainWindow.axaml`.
- **What `CollapsiblePanel` needs.** `Title`, `Summary`, `Family`, `IsExpanded`,
  and — the useful discovery — **`HeaderAction`**, an `object?` slot on the
  header. That is where the capture press went.
- **What `ModePalette` exposes.** `ModeColors` already builds **`FillBrush` and
  `InkBrush`**, so `ModePalette.Digital` is reachable from markup by `x:Static`
  and **no colour literal was needed anywhere.**
- **What `WaterfallControl` needs.** `Source`, `BandLowHz`, `BandHighHz`, `Gain`,
  `TuneCommand`. **With no source it is safe**: `Render` handles the null bitmap
  with an honest empty state — *"no spectrum yet, so connect a radio or pick the
  training radio"* — rather than throwing or drawing anything that asserts a
  signal. So it was placed, as the order hoped.
- **What the capture press is wired to.** `Command="{Binding
  CaptureAudioCommand}"` inside the terminal template. **Reusable as a binding**,
  and left unwired for the reason below.
- **Which tests build the window.** Eighteen files. The ones a new panel must
  satisfy: `BindingHealthTests`, `EveryResourceKeyResolvesTests`,
  `ClippingTests`, `TheTabOwnsTheWorkspaceTests`, `TheTabsAndTheWorkspacesTests`,
  `TheOperatingScreenIsLaidOutAsRuledTests`, `ReturningToCwShowsCwTests`,
  `TheCanvasIsGoneTests`.

**The one gap, reported rather than papered over as the order asked.**
`PanelFamily` had four values — Slate, Amber, Blue, Green — and
`PanelPalette.For` falls through to Slate for anything else. **There was no
digital panel family.** The fix was to add one **derived from
`ModePalette.Digital`** rather than typed, so the panel chrome and the mode
language cannot drift.

## 2. What the owner should expect

**Clicking Digital now shows a screen instead of a blank rectangle:** a mode
strip with FT8 lit, then a waterfall, then decoded text, then what people are
saying.

**Every word of it is a lie in the sense that matters — none of it is
measured.** It is placeholder text so you can say what is wrong with a
finished-looking FT8 session before there is a decoder to argue with. **Nothing
decodes, nothing moves, nothing is wired.**

**The capture press looks pressable and does nothing.** That is deliberate and it
is the one thing on the tab that could mislead once anything else is live: the
trimming rule it must obey — four complete slots on UTC boundaries — does not
exist, and a press that keeps the wrong thing is worse than one that does
nothing. **The CW capture command is directly bindable and was left alone.**

**The three panels collapse and remember it separately from CW's.** Collapsing
the Digital waterfall does not collapse the CW one.

| | before | after |
|---|---|---|
| app | 509 of 509 | **509 of 509** |
| engine | 28 failing, byte-identical | **not re-run — no engine file changed** |

## 3. What you should see

### The mode strip

`on this frequency` · **FT8** lit · FT4 · PSK31 · WSPR · `reading it · 9 messages
this slot`, right-aligned.

**FT8 carries the digital fill, a border and semibold weight**; the other three
are muted ink with no fill and no border. **Lit and unlit differ by three things,
not one**, so the distinction survives a greyscale print and a colour-vision
deficiency (§0.6).

### The three panels

- **`Waterfall`** · `200–3000 Hz · 15 s slots` — the real `WaterfallControl`,
  drawing its own honest empty state. The capture press sits on the header.
- **`Decoded text`** · `14:22:45 UTC · 4 shown` — five monospaced columns under a
  muted header row, the four rows as given, the first drawn selected. **The
  message column is never truncated**; the numeric columns are fixed-width.
- **`What people are saying`** · `4 stations · 2 contacts running`, below the
  decoded text as ruled. Three entries, each an accent rule, a sentence, a muted
  detail line, and the raw decode in monospace. **`Spain` carries the `from the
  prefix list` mark** — the whole reason that entry is in the set.

### What the suite caught, and it earned its keep twice

**Three tests went red and none of them was noise.**

- **`ModePaletteTests.EveryPanelFamilyHasColors`** — enum-driven, so adding a
  family without adding it to `PanelPalette.All` failed immediately. Fixed by
  adding it.
- **`ClippingTests`** — two placeholder sentences could be cut with no sign to
  the operator. Fixed with a trim on the strip's status and wrapping on the
  distance line.
- **`ReturningToCwShowsCwTests.DigitalAndVoiceStayEmpty`** — asserted Digital
  stays empty, which your ruling of this morning reverses. **Re-expressed rather
  than deleted**, as `EachTabShowsItsOwnWorkspace`: Voice still empty, Digital now
  populated, with the ruling that changed it written into the test.

**And then the contrast guard caught a real bug.** `EveryPanelInkClearsAaOnItsOwn
Surface` reported the new family at **1.10 against its own fill** instead of about
twelve. The cause was static initialisation order: `Paper`, the colour the tints
blend toward, was declared *below* `Lavender`, so the blend ran against a default
colour and produced a dark tint. **HM-DEC-036's own test found it before it
reached a screen.** The declaration moved and the comment says why.

### Task 6 — the idle lines, and where they are

`src/Hamlet.App/ViewModels/DigitalIdleText.cs`. Four strings — `ModeStrip`,
`Waterfall`, `Decoded`, `Saying` — in the CW terminal's voice. **The markup still
shows the busy version**, as the order specifies. **Nothing reads them yet and
the class says so on its face.**

### Where the order and the tree differ

- **The canvas is gone and the order half-expected it.** `src/Hamlet.App/Layout/`
  does not exist; `TheCanvasIsGoneTests.cs` does. **The tree's model was used and
  nothing canvas-shaped was reintroduced.**
- **`TheTabOwnsTheWorkspaceTests.cs` and `TheTabsAndTheWorkspacesTests.cs` are in
  `tests/Hamlet.App.Tests/Views/`**, not `Layout/`. `Layout/` holds only
  `TheCanvasIsGoneTests.cs`.
- **The app baseline is 509, as the order carried it.** Confirmed by running it
  before any change.

## 4. What's blocking us

**Nothing blocks the next unit.** The screen is drawn and every string in it is
waiting to be replaced.

Two things need your word, neither urgent:

---

**The wording, which is yours (§12.1), and one line I would change.**

Everything was written as given. **The one I would put differently is the mode
strip's status**, `reading it · 9 messages this slot`. *"Reading it"* reads as a
claim Hamlet is decoding successfully, and on the day that strip is live beside a
slot that decoded nothing, the two will disagree on screen. **`9 messages this
slot` already says it is working**; the words in front add a claim rather than a
fact.

*Offered, not taken:* `FT8 · 9 messages this slot`, or just `9 messages this
slot`.

The idle lines are mine and are equally yours to change — they are four strings
in one file.

---

**The version moved to 1.12.0 on my own reading of HM-DEC-150.**

The order says a new phase opened this morning; HM-DEC-150 says the minor is the
phase. **If the digital phase should not start its count at this unit, say so and
it is one line.**

### Asks still outstanding

**Carried forward per HM-DEC-139 and HM-DEC-140, and deliberately not restated.**
The order parks the whole 1.11.x stream and says *both halves are required: do
not touch them, and do not raise them.* **The thirty-one asks from unit 1.11.34's
list stand unchanged**, none of them touched by this unit, and they are in that
report.

**New this unit, and both are about this tab only:**

1. **The mode strip's status wording**, above — offered, not taken.
2. **The version's phase bump**, above.

**Nothing was closed this unit**, because nothing on that list is in this unit's
scope.
