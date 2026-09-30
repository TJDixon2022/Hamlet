# Work instruction 508 - the icon

**Hand run. One unit, outside the CW phase.** The taskbar icon is a rust square with a small radio
and a clipped feather. The owner chose its replacement on 2026-09-30. **The drawing is done and is
in the tree**; this unit wires it and changes nothing else.

**If 508 is already taken, use the next free number, and say so in the report.**

Three files arrived by zip under `src\Hamlet.App\Assets\`:

- `hamlet.ico` - eight sizes: 16, 20, 24, 32, 40, 48, 64, 256. **The four small sizes are a
  separate drawing**, the quill alone and larger; the four large ones add the four dots.
- `hamlet-icon.svg` - the large drawing, the source of 40 and up.
- `hamlet-icon-small.svg` - the small drawing, the source of 32 and down.

**Do not redraw, recolor, re-export or regenerate any of the three.**

Verify by building `Hamlet.sln` with warnings as errors and running the app carry-forward line
before the first change and after the last. **The owner's look at the taskbar is the test.**

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs
  MUST EXIST:      CW_REQUIREMENTS.md
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln
  root             C:\Source\HamLet

If all five are not as stated, refuse: reply with only the path you are in,
which checks failed, and "wrong project - nothing done."

If all five hold, say "Hamlet confirmed" and continue.
```

---

## 1. Rules

- Take `SESSION.lock` through `tools\arbiter\lock.bat take`, release it at the end. Write nothing
  to `RUN_LEDGER.md`. Touch nothing under `tools\arbiter\`.
- **This unit is not a step of the CW phase.** Touch no copy of `PHASE_PLAN.md`, `PHASE_OUTCOME.md`
  or `PHASE_STATUS.md`. Tick nothing. `PROJECT_STATUS.md` follows the cadence in the prompt and
  names this unit, on unit 505's wrapper pattern.
- One `dotnet test` invocation per line, filtered, with a `timeout`. Never background and poll.
  Never run `Hamlet.App.Tests` unfiltered. The app line loses names to the dispatcher loop; re-run
  once, count neither way.
- Apostrophes in quoted heredocs break; `;`, `rm` and `git rm` are refused; Python cannot run
  here; `-m` more than once for a multi-line commit. Scripts go in `.run-unit\unit508-<name>.sh`
  and are not committed.
- **A file that has to go cannot be deleted here.** Leave it, remove every reference to it, and
  list it once in section 2 for the owner to delete by hand. **Do not empty an `.svg` under
  `Assets\`**; an empty one is a broken resource.
- Nothing that keys or transmits. Nothing written to the radio or the log. **No package.** Touch
  nothing under `src\Hamlet.RadioEngine\`.
- The tree is dirty. **Commit only what this unit changed**, by path. This file and the three
  assets are committed with task 0.
- American spelling. `output.md` at the root, four headings exactly: `## 1. What Claude did`,
  `## 2. What the owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. The owner's ruling

**His words, 2026-09-30, over a picture of his taskbar:**

> *"Now I want to talk about our icon. It looks terrible. Compare it to the other icons. Ours
> looks like an eight-year-old did it."*

He was shown two replacements at taskbar size beside the present one and answered: **"option 1"** -
an amber quill on the dark navy of the achievements standing panel, writing four dots, which is H
in Morse.

**What it supersedes.** The small mark of work instruction 285, `Assets\hamlet-mark-small.svg`, as
the window and taskbar icon; and 285's choice to raster the icon at run time from one drawing and
ship no `.ico`. That choice is why nothing was tuned for small sizes. **The mark was his, and he
has replaced it.**

**What it does not touch.** `Assets\hamlet-logo.svg`, the full logo the About window shows. The
achievement quill. Every other drawing.

---

## 3. Verify this instruction against the tree

Read from a harvest of `57b9759`; units 505, 506 and 507 have run since. **Check each; report
every mismatch in section 1; do not repair this instruction; do not stop over a mismatch** unless
a task is impossible.

- `src\Hamlet.App\Hamlet.App.csproj` is a `WinExe`, carries `<AvaloniaResource Include="Assets\**" />`
  and **no `ApplicationIcon`**.
- `src\Hamlet.App\Controls\AppIcon.cs`: `AppIcon.Small`, a lazy `WindowIcon?` built by rastering
  `SvgMark.Small` at 256 pixels; null where the platform cannot raster.
- `src\Hamlet.App\Views\MainWindow.axaml.cs` sets the window's icon from `AppIcon.Small`.
- `src\Hamlet.App\Controls\SvgMark.cs`: `SmallMarkUri` names `hamlet-mark-small.svg`;
  `FullMarkUri` names `hamlet-logo.svg`, which `AboutWindow.axaml` draws.
- `tests\Hamlet.App.Tests\Views\TheMarksRenderTests.cs` measures the small mark, including that it
  draws above its own viewBox.

---

## 4. The tasks

### Task 0 - before anything changes

Commit this file and the three assets. Run the app carry-forward line and `TheMarksRenderTests`,
filtered. **Report each green or red by name. A red here is inherited and is not chased.**

Confirm the `.ico` holds eight frames at the sizes above and report their sizes as read.

### Task 1 - the program's own icon

`<ApplicationIcon>Assets\hamlet.ico</ApplicationIcon>` in `Hamlet.App.csproj`, so `Hamlet.exe`
carries it in Explorer, on a shortcut and when pinned.

**Test watched failing first:** the project file names an `ApplicationIcon` and the file it names
exists and is an icon of eight frames.

**Drop candidate:** none.

### Task 2 - every window's icon

`AppIcon` loads `avares://Hamlet.App/Assets/hamlet.ico` and hands the platform the file, so
**Windows picks the frame for the size it wants** and nothing is scaled at run time. The raster of
the small mark goes. Never-throw stands: where the icon cannot be loaded the window opens without
one.

**Every window the application opens carries it** - the main window, Achievements, About and each
dialog. Find them all; name each in the report and how it gets the icon. One place sets it if the
framework allows; say which.

**Test watched failing first:** `AppIcon` yields an icon from the `.ico` and not from a raster of
an `.svg`; each window class has it on a rendering host; a missing file yields null and no throw.

**Drop candidate:** the windows other than the main one.

### Task 3 - the old mark leaves

- `SvgMark.Small` and `SmallMarkUri`: if nothing else draws them after task 2, they go. **If
  something else does, leave them and name what.**
- `TheMarksRenderTests`: the tests of the small mark are retired by name with the reason - the
  drawing they measured is no longer used. **The tests of the full logo stay and stay green.**
- `Assets\hamlet-mark-small.svg` is listed for the owner to delete by hand.
- The comments in `AppIcon.cs` and `MainWindow.axaml.cs` that explain 285's run-time raster are
  rewritten to cite this unit and its decision.

**Drop candidate:** none.

---

## 5. Record

- `DECISIONS.md`, newest first, **the next free `HM-DEC` id**, headline *The icon is the amber
  quill on night, shipped as an icon file*, quoting him from section 2, naming what it supersedes,
  and saying whose is whose: the choice is Tim's; the drawing, the separate small drawing without
  the dots, and shipping an `.ico` are the author's under this work instruction, and overrulable.
- The index row for it in `CLAUDE.md` §1. **Fill no other row.**
- Patch-bump `Directory.Build.props`.
- **No phase file.**

---

## 6. What not to do

- **Do not alter the three assets.**
- **Do not change `hamlet-logo.svg` or the About window's drawing.**
- **Do not scale one frame to make another.**
- **Do not let a window fail to open over an icon.**
- **Report mismatches; repair nothing else on the way past** (`CLAUDE.md` §12.6).

---

## 7. Committing and pushing

Commit per task on `main`, by path, each with its red quoted. Push at the end and say whether it
succeeded.

---

## 8. Report

`output.md` at the root, above the four headings:

```
READ IN THIS ORDER.

A. Whether Hamlet.exe carries the icon and which windows do.
B. What became of the old mark and its tests.
C. The rest. Section 4 raises <n> items, none blocking.
```

```
UNIT:       <number> - <complete|stopped> at task N of 3, <dropped or none dropped> - <date time>
UNIT GOAL:  <in your own words>
NUMBER:     frames in the icon: <n> of 8; windows carrying it: <n> of <n>
```

Section 2, for the owner, in plain words:

- rebuild and run;
- the taskbar, the title bar and Alt-Tab show the amber quill on dark navy;
- **if the taskbar still shows the old one**, Windows has it cached: unpin Hamlet and pin it
  again, or sign out and back in;
- the About window still shows the full logo with the radio, unchanged;
- `src\Hamlet.App\Assets\hamlet-mark-small.svg` to delete by hand.

Section 1: what changed, file by file; every mismatch against section 3; the windows found; the
retired tests by name; that the build and the app line are green. Section 3: the eight frame sizes
as read, and each window with its icon. Section 4: anything left, a line each.
