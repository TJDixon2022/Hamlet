# Work instruction 031 — give the operator his send button back

## 1. What Claude did

Claude Code, on the development computer, in `C:\Source\HamLet`. The prompt
claimed `PROJECT: Hamlet`; the tree confirmed all four checks — `SHACK_FACTS.md`
and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` exist, neither
`CoreHMI.sln` nor `MURC.sln` does. **Hamlet confirmed.** Branch `main`
throughout, three commits, all pushed, none refused. Version 1.11.27 to 1.11.28
per HM-DEC-150.

**Nothing here is evidence about the radio.** No rig was connected, nothing
transmitted, and no test presses anything that could key one. **Tim verifies at
the rig.**

**No decision was recorded under §12.1.** The one this unit turns on —
HM-DEC-059 — was already ruled. Section 4 carries what needs a ruling.

**All five tasks ran, including the drop. Nothing was left.**

**No decoder file was touched.** `git diff` over this unit's three commits
against `src/Hamlet.RadioEngine/` reports **zero files**.

### Task 1 — the path was there, and so was the ruling

**The order was right and the premise behind 026, 027 and 028 was wrong.**
Read before anything was changed:

| what | where |
|---|---|
| the view model | `MainWindowViewModel.Transmit`, `MainWindowViewModel.cs:1315` |
| attached to the rig | `MainWindowViewModel.cs:5030` — `Transmit.Attach(new CwTransmitter(new KeyerCwSender(rig)))` |
| the press | `CwTransmitViewModel.PressAsync`, `[RelayCommand(CanExecute = nameof(CanPress))]`, calling `_transmitter.SendAsync(message, context)` |
| the interlocks | `TransmitReadiness.Check`, eleven refusal states |
| the tests | 48 across five files — `CwTransmitTests` 17, `TransmitChainTests` 12, `TransmitPrivilegeTests` 11, `CwTransmitGuardTests` 5, `TheRefillGuardActuallyRunsTests` 3 |
| the ruling | **HM-DEC-059**, `CLAUDE.md:446` — *"Hamlet keys the radio and sends Morse, by handing text to the radio's own keyer with CI-V `17`"* |

**HM-DEC-098 is a different ruling about a different thing.** It governs the
**automated repeating cycle** and says dummy load only. It says nothing about
the operator pressing a button, and three orders cited it as though it did.

**Path, tests and ruling all present, so tasks 2 to 4 proceeded.**

**What was actually removed was one button.** `widget.send` was still sitting in
`MainWindow.axaml` at line 1152, complete, with `PressCommand` on every
contextual option — an orphaned template nothing instantiated.

### Task 2 — the button

Rebuilt as a permanent panel rather than restored from the orphaned template,
because the CW workspace has panels and not widgets now.

- **`Send` and `Clear` at the top beside the title**, as asked. Clear is coloured
  as an action rather than left as chrome.
- **The send button carries `Transmit.PressCommand`** with `Transmit.OwnWords`
  as its parameter — **the same door the contextual options use**, with the same
  readiness check, the same guard, the same watch, the same chain report, the
  same abort and the same record. **There is not a second path to the
  transmitter and this is not one.**
- `OwnWords` is a `SendButtonViewModel` like any other, so nothing about it is
  exempt from anything.

### Task 3 — the paragraph

The line saying nothing leaves the radio is gone, because it is no longer true.
What replaced it says what each control does: CQ puts your callsign on the band,
RST is your honest word on how well you are hearing the other station, 73 means
best wishes, Clear empties the line and sends nothing, Send puts what is on the
line on the air.

**The macros write the line the button sends.** One message, not three: the box,
the macro buttons and the command parameter are the same object, and a test
asserts it. If they were separate the operator could read one thing and transmit
another (§0.0).

## 2. What the operator sees

**The Send panel has a Send button again**, top right beside the title with
Clear next to it, and it is wired to the transmitter.

**It will be grey when he opens the app, and that is correct.** With nothing
connected the panel refuses with *"Training radio does not transmit, so this is
receive only"*, printed under the buttons. On a connected IC-7300 in CW with
break-in on, it goes live.

**What will look wrong and is not:** the engine suite is red at its known
baseline, unchanged. That set is 28 decoder tests and no decoder file was
touched this unit.

| | baseline | end |
|---|---|---|
| engine | 28 of 1841, stable set | **28 of 1845, byte-identical** |
| app | 507 of 507 | **509 of 509** |

Two tests added to the app suite; four to the engine suite, all green. The
engine total moves 1841 to 1845 and the failing set does not move at all: the
28 names are the same 28, compared against the stable list rather than counted.
It ran in 17 minutes 37 seconds.

## 3. The count

### Task 4 — every interlock, and whether a test covers it

**Eleven refusal states. Ten proved refusing, one proved unreachable.**

| interlock | covered before | covered now |
|---|---|---|
| `NotConnected` | yes | yes |
| `RadioCannotTransmit` | yes | yes |
| `AlreadyTransmitting` | **no** | **yes — new** |
| `ModeUnknown` | **no** | **yes — new** |
| `NotInMorse` | yes | yes |
| `BreakInUnknown` | yes | yes |
| `BreakInOff` | yes | yes |
| `LicenseClassUnknown` | yes | yes |
| `FrequencyUnknown` | yes | yes |
| `OutsidePrivileges` | yes | yes |
| `ListenOnly` | no | **unreachable — see below** |

**Every one of the ten is required to carry a sentence, not only a token.** The
assertion is on `Detail` and not on `Reason`, deliberately: the token is a
machine string and is never empty, so asserting it would have proved nothing
about what reaches the screen. A refusal with no sentence is a grey button the
operator cannot argue with (HM-DEC-080).

**`ListenOnly` is not an uncovered interlock, it is an unreachable state, and it
is reported as one rather than quietly counted.** It means the class holds this
stretch but not in this mode. **Morse has no such stretch**: 97.305(a) permits CW
on any frequency authorised to the control operator, which is why CW is absent
from the emission table, and `PrivilegePlan.ModeAllowed` returns true for
`TransmitMode.Cw` before it reads a single row. **Swept rather than argued** —
every class at every 5 kHz from 1.7 to 29.8 MHz, **28,105 asks: 2,479 allow
Morse, 25,626 refuse it, none refuse it as a mode.** The sweep also asserts it
found both allowed and refused cases, because a plan that permitted everything
would satisfy the same test and prove nothing.

**It is live code all the same**, reached by the band map drawing listen-only
stretches for data and phone. Nothing here says to delete it.

### The view-level half, and what it caught

**The engine tests prove the check refuses; they cannot see whether the refusal
reaches the button.** So one view test asks the button — `CanExecute`, never a
press, because pressing is what Tim does at the rig.

**It went red on its first run and the reason is worth keeping.** It asserted
`send.IsEnabled` and read **true**, with `CanExecute` false and the button
correctly dead. `IsEnabled` is the local value nobody has set, so it reads true
forever; **what a command drives is `IsEffectivelyEnabled`**. That is the same
trap unit 1.11.25 recorded for `IsVisible` versus `IsEffectivelyVisible`, in a
different property, caught by the same reasoning and written into the test's own
comment.

### The owned-property list

**`SendText` retired; `OwnWords.Message` added.** The property `SendText` named
no longer exists on the view model — unit 1.11.24's send line was a box nothing
transmitted, and the line the operator now edits is
`Transmit.OwnWords.Message`. **An entry naming a property nobody has is not
harmless: it reads as coverage.** Four properties, 13 view test files scanned,
no offences.

### Task 5 — the other fourteen, read against the tree

**Report only. Nothing restored, nothing scheduled.** Written into
`ABANDONED_WIDGETS.md` as well, so the finding outlives this file.

**Two are working capabilities with a live rig attached and no control
anywhere** — the same shape Send was in:

| widget | view model | engine | attached |
|---|---|---|---|
| **Scanner** | `ScanViewModel`, 598 lines | `BandScanner` 621 lines, plus `ScanDwell`, `ScanSegments`, `ScanStop`, `ScopeBinSurvey` | `MainWindowViewModel.cs:5041` |
| **Call CQ on a cycle** | `AutoCallViewModel`, 461 lines | `AutoCall` 759 lines, plus `AutoCallAnswers` | `MainWindowViewModel.cs:5042` |

Those two attach lines sit **directly below the transmit attach at 5030**. Six
test files cover the scanner and four the calling cycle. **Both carry the
interlocks their rulings demand and both are live right now with nothing on
screen to trip them**, and the two ask each other rather than tracking each
other, wired as predicates at `MainWindowViewModel.cs:2054-2065`.

**The calling cycle is the one to be careful about.** It is the only thing in
this application that transmits without a hand on it, and HM-DEC-098 requires its
interlocks watched firing into a dummy load first. **A surface for it is a
separate decision from a surface for the scanner, and neither follows from the
send button coming back.**

**Three do real work whose only output was the deleted picture:** the
**waterfall** (`RigSpectrumSource` attached, asking the radio for CI-V `27 00`,
every pixel gone — so HM-DEC-093's frame counters have nowhere to appear and
"nothing has ever arrived" and "the band is quiet" are the same sight again);
**did anybody hear me** (`HeardWatch` filters the feed for his own callsign and
the answer is computed and displayed nowhere); and the **dial tape**.

**Nine were pictures over data the engine still computes**, and rebuilding one is
markup rather than machinery.

**And the markup count**: fifteen `widget.*` templates are still defined in
`MainWindow.axaml` and **two are still used** — `widget.map` at line 2308 and
`widget.terminal` at line 2708. `widget.send` joined the orphans this unit.
**Thirteen templates, roughly 1,700 lines, are dead markup that no test can
see**, because `BindingHealthTests` and `EveryResourceKeyResolvesTests` both walk
the live window and a template nothing instantiates is never built.

## 4. What's blocking us

**Two capabilities are attached to the radio with no way to reach them, and one
of them transmits.**

Ruling asked for:

> **The scanner and the calling cycle are wired to the live rig and have no
> control on any screen.** Both view models are constructed, both are handed the
> rig on connect at `MainWindowViewModel.cs:5041-5042`, both are fully tested,
> and neither can be started or stopped by the operator. **This is the same fault
> that removed the send button, found twice more by looking.**

The scanner is the smaller question: it moves the dial, §0.2.1 governs it, and
its interlocks are built and tested. **The calling cycle is not the same
question.** It is automated transmission, HM-DEC-098 says dummy load only until
its interlocks have been *watched* firing, and giving it a button is the step
that makes that evening possible rather than the step that follows it.

*Not proposed, because it needs a ruling:* whether either gets a surface in the
CW workspace, and if so whether the calling cycle's arrives disabled until the
dummy-load evening has happened.

---

**Nothing checks that deleting a surface is not deleting a capability, and this
unit found two more instances of it.**

That was inbound ask 22 and it is now measured rather than suspected. **The
tests could not have caught any of the three**, because every guard in this suite
walks the live window, and a capability with no surface is invisible to all of
them by construction.

*Not proposed:* a check that every view model constructed by `MainWindowViewModel`
is reachable from some binding in the markup. It would have caught all three, and
it needs a ruling because it would also fire on view models that are legitimately
model-only, of which HM-DEC-076's contact tracker is one by explicit ruling.

---

**Thirteen dead `DataTemplate` blocks sit in `MainWindow.axaml` and nothing can
tell them from live ones.**

Roughly 1,700 lines. Deleting them is not this unit's to do (§12.6) and it is
named here because **the markup is dead rather than dormant** — and because
`widget.send` sat there complete and unreachable for three units while three
orders forbade building what it already contained.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Twenty-four inbound
after this unit's closures. The oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150** — and
   **HM-DEC-098, which was misread three times to remove a working button, is
   inside that range.** The cost of the missing records is now measured: a
   capability nobody could look up, removed by orders citing the ruling that did
   not cover it.
5. **The tone tracker** — six axis families measured; the question is a design
   one.
6. **The integrator width** — settled at 45 Hz, with the sharp-peak caveat.
7. **The guard's gap is two to one**, calibrated on two empty captures.
8. **A boxcar's nulls made two of five swept offsets pathological best cases.**
9. **Two stations closer than 125 Hz are not named** — the operator's own item
   five, still not attempted.
10. **The keying meter** — its measurement found a station its verdict denied.
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
12. **The gate opens on everything, including two empty recordings** (1.11.18).
13. **The joint cutter cannot find word gaps on a compressed fist** (1.11.22) —
    the next decode question, still unruled.
14. **The constrained margin is bounded and still does not separate** (1.11.22).
15. **Four fixtures are absent and five acceptance lines were unmeasurable**
    (1.11.22).
16. **HM-DEC-086's supersession needs a record** (1.11.25).
17. **The phrasebook's arrival and the absent-widget news are gone** (1.11.25).
18. **The recent-places row has no home** (1.11.26), three options costed.
19. **The owned-property list has no enforcement of staying current** (1.11.27).
20. **A test resolved an ambiguous control by accident** (1.11.27).
21. **A deleted widget's description was the only record of a working
    capability**, and it took the operator to notice. **Nothing checks that a
    deletion is not removing something in use** — now measured, above.
22. **`013347` returns a likelihood ratio of 17.2 million**, with `001520`'s
    quadrillions. Parked, raised once.
23. **The scanner and the calling cycle are attached to the rig with no
    control**, above.
24. **Thirteen dead `DataTemplate` blocks nothing can distinguish from live
    ones**, above.

New this unit: **the scanner and the calling cycle have no surface**, above;
**the dead templates**, above.

Closed this unit: **the send button** — restored, wired to the one transmit path,
and its interlocks proved. **Inbound ask 18, "engine code behind the abandoned
widgets is unreachable"** — read in full and answered: two are capabilities,
three do work with no output, nine were pictures.

Still open: **the lock's mixed help**; **three fixtures at accepted cost**; **the
reference and port integrator difference**; **an unmeasured pitch costs `N4L`**;
**the six-hertz window disagreement**; **the short-character bias**; **the
Avalonia geometry offset**; **`CHANGELOG.md` at 1.9.0 against 1.11.28**; **the
whole-file second pass**; **the squelch has no axis**; **the three morning
captures of 2026-08-26**; **seven timing intermittents, none of which fired
today**.
