# Every context menu in the application — work instruction 276, task 5

**Reading only. Nothing was changed to produce any of this, and nothing found here
was fixed.** Read from the tree on 2026-09-07 at commit `50768aa`, version
1.12.142 — that is, *after* tasks 2 and 3, so the mine list's menu is present and
counted.

---

## The short answer

**There are two context menus in the whole application and they are the same menu.**
Both row templates in the decoded area carry `ContextRequested="OnDecodedRowContextRequested"`,
deliberately one handler rather than two. **No other control in the app has a
context menu of any kind** — not the band cards, not the spot list, not the
waterfall, not the canvas widgets, not the six other windows.

**Two test classes open a menu.** One of them, after task 3, opens both lists. **The
other still searches the left list alone for a row addressed to the operator, which
is the exact shape of the defect this unit fixed, and it is red for the same reason.**
It is named below and left alone.

---

## Every hit in the markup

A sweep of all nine `.axaml` files under `src/` for `ContextMenu`, `ContextFlyout`
and `ContextRequested` returns four lines in one file, of which two are comments:

| Line | Kind | What |
|---|---|---|
| `MainWindow.axaml:3754` | comment | why there is deliberately no `ContextFlyout` on a row |
| **`MainWindow.axaml:3762`** | **live** | `ContextRequested="OnDecodedRowContextRequested"` |
| `MainWindow.axaml:3986` | comment | why the second list reuses the handler rather than copying it |
| **`MainWindow.axaml:3994`** | **live** | `ContextRequested="OnDecodedRowContextRequested"` |

`App.axaml`, `Controls/CollapsiblePanel.axaml`, `AboutWindow`, `DecisionLogWindow`,
`FavoritesWindow`, `LogContactWindow`, `RigDiagnosticsWindow` and `SettingsWindow`
contain **no hit at all**. Neither does any `.cs` file except the code-behind that
builds the flyout.

## Which control carries each

| # | Control | Inside | Bound to | Added by |
|---|---|---|---|---|
| 1 | the row `Grid`, `ColumnDefinitions="76,48,Auto,*"` | `ItemsControl x:Name="DigitalDecodedRows"` (`:3728`) — the **left** list | `DigitalVisibleDecodes` | unit 268 |
| 2 | the row `Grid`, `ColumnDefinitions="76,*"` | `ItemsControl x:Name="DigitalMineRows"` (`:3966`) — the **right** list | `DigitalMineDecodes` | **this unit, task 2** |

Both are the row template's outermost `Grid`, both carry `Background="Transparent"`
so the whole row width is hit-testable rather than only the glyphs, and both name
the same handler. `MainWindow.axaml.cs:125` reads the row off the sending control's
own `DataContext`, so it neither knows nor cares which list produced it — which is
why one handler serves two lists and why there is no second menu to drift.

**There is no `ContextFlyout` anywhere.** That is deliberate and documented at
`:3754`: a flyout hung on the row would be built once and carry the repeat counts
the row was born with, so it would offer `RRR` as a first send after it had already
gone twice. The menu is built at the moment of the click instead.

## Whether any test opens one

Three test classes were named in task 1. Re-checked here against the tree as it now
stands:

| Test class | Opens a menu? | Which list | State |
|---|---|---|---|
| `Views/TheMenuIsUnderTheMouseTests` | **yes**, real `ContextRequested` on the real row control | **both**, since task 3 (`:536`) | **8 of 8 green**, run by this unit |
| `ViewModels/TheWholeChainRunsFromOneRightClickTests` | **yes**, same technique, `:915` | **left only** (`:901`) | **structurally red — see below** |
| `ViewModels/TheLicenceGateHoldsFromTheClickTests` | no — calls `SendMenuFor` on the view model | — | not a menu test |

Four further classes touch those lists by name without opening a menu:
`Views/TheDecodedColumnsLineUpTests` (both lists, column geometry),
`Views/TheDecodedPanelScrollsItselfTests` (left), `Views/WhatTheSplitCostsTests`
(both, widths).

## The one finding, reported and not repaired

**`TheWholeChainRunsFromOneRightClickTests` has the same defect this unit fixed, and
task 3 did not reach it.** Its `RightClick` helper at `:895`:

```csharp
var rows = scene.Window.GetVisualDescendants()
    .OfType<ItemsControl>()
    .FirstOrDefault(c => c.Name == "DigitalDecodedRows");

var grid = rows!.GetVisualDescendants()
    .OfType<Grid>()
    .FirstOrDefault(g => g.DataContext is DigitalDecodeRow row
        && row.Sender == His && row.Addressee == Mine);
```

It asks the **left** list for a row whose addressee is the operator.
`MainWindowViewModel.WantsRow` (`:1413`) is

```csharp
=> !IsForHim(row) && DecodedFilterRule.Wants(ShowsCqOnly, row.Addressee);
```

so **no row addressed to him is ever on that list**, and the search cannot succeed.
Both of that class's two menu-opening tests (`:292`, `:559`) go through it. It went
red the day unit 273 landed, for precisely the reason the menu test did.

**It was not run and it was not fixed.** Not run because HM-DEC-155 rules that a
unit runs only the test it constructs, and this unit constructed the other class;
the red above is read from the code rather than from a run, and is stated as such.
Not fixed because task 5 says report only, and because §12.6 forbids repairing
things passed on the way. **The repair is one line** — the same
`Where(c => c.Name is "DigitalDecodedRows" or "DigitalMineRows")` task 3 put in the
other helper — and it wants its own unit so that somebody watches it go green.

## What is not here, and is not a defect

No other surface in the app offers a right-click. The band cards, the happening-now
spot cards, the map dots, the dial tape, the waterfall and the canvas widgets all
answer a **left** click and carry hover detail; none has ever declared a context
menu, so none is broken. Recorded so the next sweep does not read this section as a
list of omissions.
