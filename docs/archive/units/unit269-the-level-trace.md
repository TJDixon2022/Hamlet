# Unit 269, task 1 — the level trace

**PROJECT: Hamlet.** Read off the tree on 2026-09-07 at `HEAD` `672df09`, on the
development machine. `SHACK_FACTS.md` FACT-004 throughout: **no serial port was
opened, no render endpoint was opened, and nothing here was played.** No figure in
this file says anything about the IC-7300.

**Measure before building.** Six questions, each answered with a file, a line and
a quotation. *None* is a finding and is written as one.

---

## Q1 — What does it cost Tim to change the drive while he is operating?

### The dialog is modal, and the claim in the instruction is confirmed

`src/Hamlet.App/ViewModels/MainWindowViewModel.cs:4044-4059`, `OpenSettingsAsync`:

```csharp
        AppEvents.SettingsOpened(_telemetry);
        var window = new Views.SettingsWindow
        {
            DataContext = new SettingsViewModel(_settings, _telemetry),
        };
        await window.ShowDialog(desktop.MainWindow);
```

`new Views.SettingsWindow` is at `:4055`, `new SettingsViewModel(_settings, _telemetry)`
at `:4057`, `await window.ShowDialog(desktop.MainWindow)` at `:4059`. **Every line
number the instruction gave for this is right.** `_settings` is the view model's own
`AppSettings` field and is handed straight in, so the settings screen and the main
window are two views over **one** `AppSettings` instance — which is the fact task 2's
fourth assertion rests on.

### What is behind it

`ShowDialog` shows a second top-level `Window` owned by `desktop.MainWindow` and
does not return until it closes. The whole of the main window is therefore behind
it — **there is one main window and the Digital tab is a tab inside it**:

- `src/Hamlet.App/Views/MainWindow.axaml:3128` — `x:Name="DigitalSendReservedLine"`,
  and at `:3119` `x:Name="DigitalStopButton"`, the always-pressable Stop. Both are
  inside `x:Name="DigitalSendReserved"` at `:3081`, which is `Grid.Row="1"` of the
  Digital tab's left column (`x:Name="DigitalLeftColumn"`, `:3010`).
- `:3025` — `x:Name="DigitalWaterfallPanel"`, `Grid.Row="0"` of the same column,
  with `<ctl:WaterfallControl MinHeight="180" Source="{Binding DigitalSpectrum}" .../>`
  at `:3050`.
- The decoded table is the right-hand half of the same tab (`x:Name="DigitalDecodedPanel"`
  at `:3177`, rows at `x:Name="DigitalDecodedRows"` `:3451`; the comment at `:3155`
  reads *"DECODED TEXT IS THE RIGHT-HAND HALF"*).

**The instruction's `:3129` for `DigitalSendReservedLine` and `:3025` for
`DigitalWaterfallPanel` are each one line off in the tree as it stands** — the
`x:Name` attribute is at `:3128` and the opening `<ctl:CollapsiblePanel Grid.Row="0"`
at `:3024`. Reported, not repaired.

**So all three named things — `DigitalStopButton`, the decode table and the
waterfall — are on the window the dialog is shown over.** Avalonia's own
disabling of an owner window while a modal child is up was **not** quotable in
this session (the NuGet package folder is outside the working directory and both
attempts to list it were blocked), so what is asserted here is only what the tree
itself says: the Settings window is a separate top-level window, shown over the
main window, and it does not return until it is closed. **The Stop button is not
gone — it is under another window.** That is the cost, and it is the same cost
whether Avalonia greys the owner or merely covers it.

### Is a slot boundary still driven while the dialog is open? — **Yes.**

From the code, not from a guess. Three links:

1. `MainWindowViewModel.cs:3272-3273` — the tick:

   ```csharp
        _decodeTimer = new DispatcherTimer(
            TimeSpan.FromMilliseconds(250), DispatcherPriority.Background, OnDecodeTick);
   ```

2. `MainWindowViewModel.cs:4898` — the last line of `OnDecodeTick`:

   ```csharp
        OnSlotTick();
   ```

3. `MainWindowViewModel.cs:7613-7618` — `OnSlotTick`'s first line:

   ```csharp
    private void OnSlotTick()
    {
        // **BEFORE THE MODE CHECK, AND IT CAN ONLY FIRE WHAT IS ALREADY ARMED.**
        DriveTheArmedSend();
   ```

   and `DriveTheArmedSend` at `:8247` reaches `_ = AtSlotBoundaryAsync(boundary);`
   at `:8264`.

`OpenSettingsAsync` is `async` and **awaits** `ShowDialog` at `:4059`. An `await`
returns control to the dispatcher; a `DispatcherTimer` is a dispatcher operation
on the same thread. **The boundary keeps arriving while the dialog is up.** So a
transmission armed before the dialog was opened still goes out, and the operator
cannot see it go, cannot read the line that says it went, and cannot reach the
Stop button without closing the dialog first.

### The cost, counted

Between two slots, to change the drive and see what it did, as the tree stands
this morning:

| | |
|---|---|
| Windows he must open | **1** (Settings, modal, over the band) |
| Controls he must cross to reach the drive | **4** — the ⚙ Settings button, the Settings window's Transmit section, `TransmitDriveBox`, then the window's own close |
| Things hidden while he does it | the waterfall, the decode table, **and the Stop button** |

Where the drive is under the waterfall, both counts go to **0 windows and 1
control**, and nothing is hidden.

---

## Q2 — What exactly does the operator read after a send today?

### The string

`MainWindowViewModel.cs:8396`, `LevelLine`, reached from `WentLine` at `:8357-8358`:

```csharp
        return "Sent " + Addressed(text) + "\"" + text + "\" in the slot at "
            + slot + " UTC. " + LevelLine(result.Send.Transmission);
```

and `LevelLine` itself, `:8397-8421`:

```csharp
        var peak = transmission.PeakSample;
        var clipped = 0;

        foreach (var sample in transmission.Samples)
        {
            if (sample is < -1.0f or > 1.0f)
            {
                clipped++;
            }
        }
        ...
        return "It was composed at " + dbfs + " dBFS with "
            + (clipped == 0
                ? "nothing clipped"
                : clipped.ToString(CultureInfo.InvariantCulture) + " samples clipped")
            + " - that is the level Hamlet built, before this machine's own volume "
            + "for that device and before the radio's input gain. Set the radio's "
            + "drive against its own ALC meter.";
```

At the current default — `AppSettings.cs:252`,
`public float TransmitDrivePeak { get; set; } = Ft8Composer.DefaultDrivePeak;`,
which is `0.25` — the whole sentence reads:

> Sent to W1ABC, "W1ABC KC3QIS -10" in the slot at 14:40:30 UTC. It was composed
> at -12.0 dBFS with nothing clipped - that is the level Hamlet built, before this
> machine's own volume for that device and before the radio's input gain. Set the
> radio's drive against its own ALC meter.

### Which quantity each number is

- **`-12.0 dBFS`** is `Ft8Transmission.PeakSample` — the peak of the float array
  the composer produced and the sink was handed. It is **the drive setting read
  back**: `MainWindowViewModel.cs:8192` composes with `_settings.TransmitDrivePeak`,
  and the composer scales to exactly that peak. **It is not a measurement of
  anything that left the machine.**
- **`nothing clipped`** is a count of samples of that same composed array outside
  `[-1, 1]`. The array was built inside the rails by the composer, which refuses a
  drive above `1.0` outright (`Ft8Composer.cs:485`). See Q3 and task 4.

The line's own remarks at `:8381-8390` say so, and name why the better number was
not taken:

> **WHY NOT THE SINK'S OWN `PeakWritten`, WHICH IS THE BETTER NUMBER.** There is
> no route to it from what `Ft8TransmitSequence` returns (unit 265, task 1,
> question 3), and building one would mean changing the keying path

**That reason is measured again in Q3 and it no longer holds** — not because unit
265 was wrong, but because it looked for a route *through what the sequence
returns*, and there is a route that does not go through the sequence at all.

### Does anything on the Digital tab show a level before a send? — **No.**

`DigitalSendLine` is declared at `MainWindowViewModel.cs:8104-8105` and starts at
`NothingHasBeenSent` (`:8092`): *"Nothing has been sent. Right-click a decoded row
to choose a message, or press CQ."* Every assignment of it (`:8036`, `:8059`,
`:8175`, `:8196`, `:8210`, `:8292`, `:8309`) is a refusal or a report of a send
that has already been armed or run. **Before the first transmission of the evening
there is no level anywhere on the tab.** That is Q6 as well, and the answer there
is "none".

---

## Q3 — Is there a route to `WasapiTransmitSink.PeakWritten` and `ClippedSamples` that leaves the keying path untouched?

### **Yes. The route the instruction expects to exist does exist, and task 3 does not need its fallback.**

The two figures are real and already measured on the way out:

`src/Hamlet.RadioEngine/Audio/WasapiTransmitSink.cs:228` and `:238`:

```csharp
    /// <summary>The largest magnitude actually written to the endpoint.</summary>
    /// <remarks>
    /// Measured on the way out, after clamping, so it is what the device was
    /// handed rather than what the caller supplied.
    /// </remarks>
    public double PeakWritten { get; private set; }
    ...
    /// <summary>How many samples had to be clamped on the way out.</summary>
    public long ClippedSamples { get; private set; }
```

both assigned in the PCM conversion's `finally`, `:407-408`:

```csharp
        PeakWritten = peak;
        ClippedSamples = clipped;
```

The interface carries neither. `src/Hamlet.RadioEngine/Transmit/ITransmitAudioSink.cs:59`
is the interface, `:81` is `int EndpointSampleRate { get; }`, `:92` is
`Task<PlayedAudio> PlayAsync(...)`, and `PlayedAudio` at `:15` is

```csharp
public readonly record struct PlayedAudio(int SamplesPlayed, TimeSpan Took);
```

— samples and time, and no level. **So there is nothing to read off the return
value, and unit 265 was right about that.**

### What the route is

The application **constructs the sink itself** and then throws the reference away.
`MainWindowViewModel.cs:8022-8026`:

```csharp
        ITransmitAudioSink sink;

        try
        {
            sink = TransmitSinkFactory(endpoint);
```

`sink` is a **local**. It is read once for its rate at `:8047`
(`var rate = sink.EndpointSampleRate;`) and handed away at `:8067-8068`:

```csharp
        _armedSend = new Ft8ArmedSend(
            new Ft8TransmitSequence(port, sink, _sendLicence, _telemetry));
```

The factory is `internal Func<string, ITransmitAudioSink> TransmitSinkFactory` at
`:7963`, defaulting at `:7964` to `name => new WasapiTransmitSink(name)`.

**The view model already owns the object that has both numbers on it.** Keeping
the reference in a field and reading the two properties off it after
`AtBoundaryAsync` returns is a read of an object the view model built, at a
moment when nothing is transmitting.

### What it touches, and what it does not

**Touches:**

- a new read-only interface in `src/Hamlet.RadioEngine/Transmit/` with the two
  properties and nothing else;
- `WasapiTransmitSink` — its declaration line only; both properties already exist
  with these exact names and types (`double`, `long`) and no body changes;
- `MainWindowViewModel.BuildTheArmedSend` — one assignment beside the existing
  local;
- `MainWindowViewModel.AtSlotBoundaryAsync` — after `AtBoundaryAsync` has already
  returned, in the same place `WentLine` is already called;
- `MainWindow.axaml` — one `TextBlock` in the Send area;
- the tests' `FakeSink`.

**Does not touch, and this is the part that mattered:**

- `Ft8TransmitSequence.RunAsync`, the key, the sink call, the `finally`, the abort
  and the stop — **not one line**;
- `Ft8ArmedSend` — not one line;
- `ITransmitAudioSink.PlayAsync` and `PlayedAudio` — **the interface the sequence
  talks through is unchanged**, so nothing the sequence does is affected by
  whether a sink also implements the report;
- `Ft8Composer`.

The report is asked for **after** the boundary has returned, on the operator's own
thread, with nothing keyed. It cannot delay an unkey, cannot fail a send and
cannot be reached from inside a transmission.

### The one honesty constraint the route imposes

`ITransmitAudioSink` does not carry the report, so `sink as ITransmitLevelReport`
can be null — a sink that does not implement it is legal. **That case must say on
the screen that there is no measurement**, not fall back silently to the composed
peak wearing a measurement's words. The production sink implements it (task 3
asserts that on the type, so it cannot be forgotten), so the null case is a fake
in somebody's future test and not something Tim will see.

---

## Q4 — Where does markup go?

**Under `x:Name="DigitalSendReserved"`**, `src/Hamlet.App/Views/MainWindow.axaml:3081`:

```xml
                            <Border Grid.Row="1" x:Name="DigitalSendReserved"
                                    Margin="0,10,0,0"
                                    MinHeight="72"
```

It is `Grid.Row="1"` directly beneath `DigitalWaterfallPanel` (`Grid.Row="0"`,
`:3024`), and its own comment at `:3056-3058` says the region exists for exactly
this:

> **THE SPACE UNDER THE WATERFALL IS SPOKEN FOR, AND IT SAYS SO** (Tim's ruling:
> keep the room under the waterfall for Send).

Inside it, `:3089`, is a single vertical `StackPanel`:

```xml
                                <StackPanel Orientation="Vertical" Spacing="6">
```

**A control and a readout belong as further children of that `StackPanel`, added
after the existing children** — which is what keeps `DigitalStopButton` where it
is. Stop is in the *first* child, the horizontal `StackPanel` at `:3094`; anything
appended below it does not move, resize, hide or condition it.

`x:Name`s already there that a headless test can find:

| `x:Name` | Line | What it is |
|---|---|---|
| `DigitalLeftColumn` | 3010 | the tab's left column, waterfall over Send |
| `DigitalWaterfallPanel` | 3025 | the waterfall's collapsible panel |
| `DigitalCapturePress` | 3039 | *keep the last 30 seconds* |
| `DigitalSendReserved` | 3081 | the Send area's border |
| `DigitalSendCqButton` | 3097 | CQ |
| `DigitalStopButton` | 3119 | the always-pressable Stop |
| `DigitalSendReservedLine` | 3128 | `{Binding DigitalSendLine}` |
| `DigitalSendLicenceText` | 3137 | the licence guard's own sentence |
| `DigitalSendUnsetText` | 3144 | what is not set in Settings |
| `DigitalDecodedPanel` | 3177 | the decode table's panel, the right-hand half |

---

## Q5 — What do the committed drive tests do about `SettingsStore.Save`?

### They do nothing, and they do not have to, because the assembly does it for them.

`SettingsViewModel.cs:343-344` does save for real:

```csharp
        _settings.TransmitDrivePeak = peak;
        SettingsStore.Save(_settings);
```

and `SettingsStore.Save` (`AppSettings.cs:676`) is
`SaveTo(settings, SettingsPath)`, where `SettingsPath` (`:645`) is
`Path.Combine(DataFolder, "settings.json")`.

But `DataFolder` (`AppSettings.cs:637`) has an **internal setter**, and
`tests/Hamlet.App.Tests/TheOperatorsFolderIsNotOursTests.cs:42-55` moves it before
any test in the assembly runs:

```csharp
    [ModuleInitializer]
    internal static void PointEverythingSomewhereElse()
    {
        Folder = Path.Combine(
            Path.GetTempPath(),
            "hamlet-app-tests-" + Environment.ProcessId.ToString(...));

        Directory.CreateDirectory(Folder);
        SettingsStore.DataFolder = Folder;
```

**So what actually happens when `SettingsCarriesTheTransmitDriveTests.ChangingTheDriveWritesItIntoTheSettings`
runs on this machine:** `panel.TransmitDrivePercent = 40.0` at
`SettingsCarriesTheTransmitDriveTests.cs:64` calls
`OnTransmitDrivePercentChanged`, which calls `SettingsStore.Save`, which writes a
real `settings.json` — **into `%TEMP%\hamlet-app-tests-<pid>\`, not into
`%AppData%\Hamlet\`.** The operator's file is not reached.

The guard's own remarks record why it exists (`:14-21`): unit 235 measured nine
tests of one class rewriting his `settings.json`, 1200 to 1352 bytes and a
different SHA-256, and it is a `[ModuleInitializer]` rather than a fixture because
the reach was through a constructor twenty files call.

**What the new tests must therefore do: nothing extra.** They live in
`tests/Hamlet.App.Tests`, so the module initializer has already run. What they
must **not** do is set `SettingsStore.DataFolder` themselves — the guard sets it
once for the whole run and a test that reassigns it changes it for every other
test in the process. `TheOperatorsFolderIsNotOursTests.cs:99-126` is the one place
that does, and it restores it in a `finally`.

---

## Q6 — What in the tree already shows a transmit level anywhere on the main window?

### **None.**

`grep -i drive src/Hamlet.App/Views/MainWindow.axaml` returns exactly one hit and
it is prose in a comment, `:2280`: *"The band plan is 'the essential driver for a
…'"*. **There is nothing about a transmit drive in the main window's markup.** The
instruction asked for this to be confirmed; it is confirmed.

`grep -i "dbfs" src/Hamlet.App/Views/MainWindow.axaml` returns nothing at all. The
`clip` hits (`:916`, `:947`, `:2446`, `:2451`, `:2453`, `:2456`, `:2998`, `:3001`)
are all layout — `ClipToBounds`, and a comment about a badge being cut off.

**`WaterfallGain` is not it, and it is a receive display control.**
`MainWindow.axaml:468` is its only appearance:

```xml
                        Gain="{Binding WaterfallGain}"
```

It is a property of the spectrum picture — how hard the received magnitudes are
mapped to colour — and it is bound on the CW tab's waterfall, not on the Digital
tab's transmit path. It touches nothing that goes to a sound card and changing it
changes no transmission.

The **only** level an operator can read anywhere in Hamlet today is the one in
`DigitalSendLine` (Q2), it is the composed peak, it exists only **after** a send,
and it is in the Send area rather than under the waterfall.

---

## What task 2 and task 3 are told by this

1. **Task 3 takes the route, not the fallback.** The sink is the view model's own
   object; nothing in the keying path is touched.
2. **The drive control goes after the existing children of the `StackPanel` at
   `MainWindow.axaml:3089`**, so `DigitalStopButton` does not move.
3. **The percent→peak conversion, the `DriveIsUsable` question and the note
   sentence can be shared**, not copied: `SettingsViewModel.cs:330`, `:336` and
   `:357` are three self-contained expressions over one `AppSettings` and one
   static composer call, and lifting them leaves the settings screen's committed
   strings byte-identical.
4. **Both views write one `AppSettings` instance** (`:4057` hands `_settings` in),
   so *the two views cannot disagree* is a property of the tree and not something
   to be arranged.
5. **The new tests need no settings-file precautions** beyond staying in
   `tests/Hamlet.App.Tests` and leaving `SettingsStore.DataFolder` alone.
6. **The clip count on the composed array cannot be anything but zero** — the
   composer refuses a drive above `1.0` (`Ft8Composer.cs:485`) and there is no
   resampler between the composer and the sink (`MainWindowViewModel.cs:8192` is
   the one `ComposeSignal` call site and composes at the endpoint's own rate).
   Task 4 measures that rather than asserting it here.
