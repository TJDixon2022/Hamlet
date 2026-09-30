READ IN THIS ORDER. A. Whether Hamlet.exe carries the icon and which windows do. B. What became of the old mark and its tests. C. The rest. Section 4 raises 2 items, none blocking.

```
UNIT: 508 - complete at task 3 of 3, none dropped - 2026-09-30
UNIT GOAL: the amber quill on night is the icon of Hamlet.exe and of every window, and the old small mark is retired
NUMBER: frames in the icon: 8 of 8; windows carrying it: 10 of 10
```

## 1. What Claude did

Claude Code on the development machine, branch `main`. The prompt claimed `PROJECT: Hamlet`; `WORK_INSTRUCTIONS.md` opens with the same gate and `PROJECT_CARD.md` says `PROJECT: Hamlet`. Nothing in this report is evidence about the radio. SESSION.lock was taken through `tools\arbiter\lock.bat take` and released at the end. Nothing was written to `RUN_LEDGER.md`, nothing under `tools\arbiter\` or `src\Hamlet.RadioEngine\` was touched, and no phase file was touched or ticked.

**A. Hamlet.exe and the windows.**

- `src/Hamlet.App/Hamlet.App.csproj`: `<ApplicationIcon>Assets\hamlet.ico</ApplicationIcon>`, after `ApplicationManifest`. The built `Hamlet.App.exe` yields the icon when read back with the Windows shell: navy at the edge (25, 40, 54), amber-brown at the centre.
- `src/Hamlet.App/Controls/AppIcon.cs`, rewritten: `AppIcon.Source` is `avares://Hamlet.App/Assets/hamlet.ico`; `AppIcon.Current` loads it once; `AppIcon.Load(uri)` copies the file's bytes and hands them to the platform whole as a `WindowIcon`, so Windows picks the frame drawn for the size it wants. Nothing scales a frame. Any failure returns null and throws nothing.
- `src/Hamlet.App/App.axaml`: one style, `:is(Window)`, sets `Icon` to `{x:Static ctl:AppIcon.Current}`. This is the one place every window gets it.
- `src/Hamlet.App/Views/MainWindow.axaml.cs`: the per-window icon set is removed, and the comment now cites work instruction 508 and HM-DEC-212.
- Windows found, all ten carrying the icon through the style: `MainWindow`, `AchievementsWindow`, `AboutWindow`, `BadgeWindow`, `ContactLogWindow`, `DecisionLogWindow`, `FavoritesWindow`, `LogContactWindow`, `RigDiagnosticsWindow`, `SettingsWindow`. There is no other `Window` type in the shell.
- `tests/Hamlet.App.Tests/Views/TheIconTests.cs`, new, with every test watched failing first:
  - `TheProgramCarriesTheIconFile` was red on *"Hamlet.App.csproj names no ApplicationIcon"*. It now passes and reads frames 16, 20, 24, 32, 40, 48, 64, 256.
  - `TheWindowIconIsTheIconFile` was red on a stub. It now checks that the icon's source is `hamlet.ico` and that the file it loads through the same resource loader holds the eight frames (20,122 bytes). The headless platform keeps no bytes of an icon it is handed, since its `Save` writes nothing, so the test reads the source and not the platform's copy.
  - `EveryWindowCarriesTheIcon` was red on *"windows without the icon: AboutWindow, AchievementsWindow, BadgeWindow, ContactLogWindow, DecisionLogWindow, FavoritesWindow, LogContactWindow, RigDiagnosticsWindow, SettingsWindow"*, because only `MainWindow` set one. It now shows each of the ten windows on the headless host and passes 10 of 10.
  - `AMissingIconFileIsNoIconAndNoThrow` was red on a `NotImplementedException` stub. It now passes: a missing file gives null.

**B. The old mark and its tests.**

- `src/Hamlet.App/Controls/SvgMark.cs`: `SmallMarkUri`, `SmallImage` and `Small` are removed, because nothing else drew them. One remark now reads "the full mark uses" where it had said "these two files use".
- `AppIcon.Raster`, `AppIcon.Build`, `AppIcon.RenderedAt` and `AppIcon.Small` are removed.
- Two tests in `TheMarksRenderTests` are retired by name, because they measured a drawing no longer in the application:
  - `WhatSurvivesOfTheSmallMarkAtIconSizes` did the stroke arithmetic of the small mark at icon sizes.
  - `TheIconBuildsAtTheSizeItClaims` rasterised the small mark into the window icon.

  A comment in the test file says so where they stood.
- The full-logo tests stay green, 4 of 4: `BothMarksLoadAndDraw`, `EachMarkIsMeasuredAgainstItsOwnViewBox`, `EveryShapeCarriesInkAndASize` and `TheAboutWindowDrawsTheMark`. The first three kept their names and now loop over the full mark only.
- `src/Hamlet.App/Assets/hamlet-mark-small.svg` is left in place and not emptied. It is listed for you to delete.

**C. The rest.**

- Checked against the order's section 3: the csproj was `WinExe` with `AvaloniaResource Include="Assets\**"` and no `ApplicationIcon`; `AppIcon` rasterised the small mark; `MainWindow` set the icon in code; `SvgMark` carried both marks; `TheMarksRenderTests` measured the small mark. No mismatches.
- The three assets were committed as given in task 0 and not altered. `hamlet-logo.svg` and the About window drawing are unchanged.
- Version: 1.13.194 to 1.13.195 in `Directory.Build.props`.
- Build: `Hamlet.sln` with warnings as errors, RC=0.
- App carry-forward line: 277 of 278. The one lost was `TheCarrierHoldsTheButtonsTests.HisCardIsDrawnInTheSendingGreenWithTheWord`, in 1 ms, to *"You've caused dispatcher loop"*. Re-run alone once it gave 8 of 8, counted neither way.
- `BindingHealthTests` 1 of 1; `TheIconTests` and `TheMarksRenderTests` 8 of 8.
- Commits, pushed to `main`: `a2fc8390` (task 0), `cfc6974d` (task 1), `d36d8591` (task 2), `266337ac` (task 3), and the closing commit with the records and this report.
- Recorded, as the order directed: **HM-DEC-212**, in full in `DECISIONS.md`:

> **The icon is the amber quill on night, shipped as an icon file.** Tim, 2026-09-30: *"Now I want to talk about our icon. It looks terrible. Compare it to the other icons. Ours looks like an eight-year-old did it."* Offered drawings, he chose the first: *"option 1"*.
>
> **What is built.** `Hamlet.exe` carries `Assets\hamlet.ico` as its `ApplicationIcon`, so Explorer, a shortcut and a pinned taskbar entry show it. The file holds eight frames, 16, 20, 24, 32, 40, 48, 64 and 256 pixels, each drawn for its size. Every window the application opens carries the same icon from one style in `App.axaml`, and the file is handed to the platform whole so Windows picks the frame drawn for the size it wants; nothing scales one frame to make another. A missing or unreadable file loads as no icon and the window opens without it (§8, never-throw).
>
> **What it supersedes.** Work instruction 285 made `Assets\hamlet-mark-small.svg` the window and taskbar icon and chose to rasterise it at run time, one 256-pixel render scaled down, rather than ship an `.ico`. Both go. The small mark is no longer drawn anywhere; its two tests are retired and the file is left for the owner to delete by hand. The full logo, `hamlet-logo.svg`, and the About window that draws it are unchanged.
>
> **Whose is whose.** The choice of the amber quill on night is Tim's. The drawing, the separate small drawing without the dots for the smallest frames, and shipping it as an `.ico` are the author's under work instruction 508, and each is Tim's to overrule.

  The index row is added at the top of `CLAUDE.md` §1. No other row was filled.

## 2. What the owner should expect

- Rebuild and run Hamlet. The taskbar, the title bar and Alt-Tab show the amber quill on dark navy, and so does every window you open from it: Achievements, About, Settings, the logs and each dialog.
- If the taskbar still shows the old icon, Windows has cached it. Unpin Hamlet and pin it again, or sign out and back in.
- The About window still shows the full logo, unchanged.
- One file is for you to delete by hand: `src\Hamlet.App\Assets\hamlet-mark-small.svg`. Nothing refers to it any more.
- `DecisionLogOrderTests.EveryRulingAppearsOnceAndTheGapsAreTheKnownOnes` is still red on the index gaps 166, 182 and 189 to 210, as it was before this unit. The order said to fill no other row. `TheDecisionLogIsNewestFirst` passes with HM-DEC-212 on top.

## 3. What you should see

The eight frames in `hamlet.ico` as read from its directory, all 32-bit and stored as PNG:

| Frame | Bytes |
|---|---|
| 16 × 16 | 593 |
| 20 × 20 | 753 |
| 24 × 24 | 927 |
| 32 × 32 | 1,129 |
| 40 × 40 | 1,682 |
| 48 × 48 | 2,083 |
| 64 × 64 | 2,428 |
| 256 × 256 | 10,393 |

Each window and where its icon comes from:

| Window | Icon |
|---|---|
| Hamlet.exe (Explorer, shortcut, pinned) | `ApplicationIcon` `Assets\hamlet.ico` |
| MainWindow | `hamlet.ico`, through the `App.axaml` style |
| AchievementsWindow | same |
| AboutWindow | same |
| BadgeWindow | same |
| ContactLogWindow | same |
| DecisionLogWindow | same |
| FavoritesWindow | same |
| LogContactWindow | same |
| RigDiagnosticsWindow | same |
| SettingsWindow | same |

## 4. What's blocking us

Nothing blocks. Two items:

1. `src\Hamlet.App\Assets\hamlet-mark-small.svg` is waiting for you to delete it by hand, since this session cannot delete a file.
2. The headless test platform cannot show which frame Windows picks at a given size, because its icon stub keeps no bytes. Only your eyes on the taskbar, the title bar and Alt-Tab can confirm the small frames look right.

### Asks still outstanding

- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25, waiting on
  the owner; no change sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8
  record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, so nothing on it is ever
  revised, at the cost of seconds of lag. On the run path, now the only path to the screen, the
  terminal shows only settled text. The ask stands only for the timing-only path, which no longer
  reaches the screen; no change for it sits in the tree.
