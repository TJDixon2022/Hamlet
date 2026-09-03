# Bench check — Hamlet on FT8 at 14.074

This is for you, at the radio, with Hamlet open.

Nothing in this file has been observed at a radio. No unit of this phase has
heard an antenna. Everything below is a prediction taken from the code. **You
are the measurement.**

There is no transmit step anywhere in this check. Nothing here keys the radio.

---

## 1. What to do

### Step 0 — confirm which Hamlet is running (added by unit 234)

**Do this before you plug anything in.**

Open **About** and read the version. It should be **1.12.38**, which is what
this tree carries as of tonight, 2026-09-03.

**Why this step exists, and it is *measured*, not predicted.** Unit 234 read the
telemetry files in `%AppData%\Hamlet\telemetry` and listed every version of
Hamlet that has ever written a line on this machine. **The newest is 1.12.0.**
Every version above it — the thirty-seven patches that carry this phase's FT8
work, the Digital tab's continuous slot watch, the per-slot census and the
capture sheet — has never been seen running here.

**If the number on screen is older than 1.12.38**, then the Hamlet in front of
you does not contain the work this check is testing, and nothing below will
happen however good the band is. What to do about that is **your** call — how
you build and install Hamlet on this machine is yours and no unit has touched
it. This step only tells you which build you are looking at.

*Predicted*: that the About box shows the version. *Measured tonight*: that
1.12.0 is the newest build in this machine's own record.

#### There may be no About box to open (added by unit 235, *measured*)

**Unit 235 looked for an installed Hamlet on this machine and did not find
one.** Desktop, both Start Menus, both Program Files, Local AppData, Roaming
and the whole user profile were searched, with unreadable sub-folders stepped
over and counted rather than silently skipped. **There is no `Hamlet.App.exe`
and no `Hamlet.exe` anywhere outside this repository's own `bin` folders**, and
the only Hamlet-named shortcuts are Windows *Recent documents* entries pointing
at work-instruction zips, the source folder and `Hamlet.sln` — none at an
application.

So if you were expecting an icon, there may not be one, and what you have been
starting may be a build from somewhere else. That would also explain the
1.12.0.

#### How to produce a Hamlet that carries this work (added by unit 235, *measured*)

```
dotnet build -c Release src\Hamlet.App\Hamlet.App.csproj
```

**It lands in `src\Hamlet.App\bin\Release\net8.0\`, and the thing to start is
`Hamlet.App.exe` in that folder.**

*Measured tonight, not predicted*: that command ran here at 17:47 UTC on
2026-09-03, exit code 0, **0 warnings and 0 errors**, in **5.0 seconds**. That
matters more than it sounds, because this project builds with
`TreatWarningsAsErrors` and `GenerateDocumentationFile` both on, so a single
missing doc comment anywhere in units 224 to 234 would have stopped it — and
until tonight **no unit of this phase had ever built the shell as an
application at all.**

*Measured tonight*: the version was then read off the produced binary rather
than off `Directory.Build.props` —

| Read from | `Hamlet.App.dll` | `Hamlet.App.exe` |
|---|---|---|
| `AssemblyVersion` | 1.12.38.0 | (apphost shim, none) |
| Win32 `FileVersion` | 1.12.38.0 | 1.12.38.0 |
| Win32 `ProductVersion` | 1.12.38+daeccb3 | 1.12.38+daeccb3 |

`App.axaml.cs:37` stamps `GetName().Version.ToString(3)`, which for that binary
is **1.12.38** — so that is exactly what the About box and every telemetry line
will show you.

**The tree has since been bumped to 1.12.39** as tonight's patch. Building the
pushed tree gives you **1.12.39**, and anything at or above 1.12.38 carries
this phase's work.

`dotnet publish` was refused by the session's sandbox and was not run, so the
folder above is a build output rather than a published one. It runs; it is just
not self-contained.

---

Then do these in order.

1. Plug the radio in. USB cable to the PC.
2. Start Hamlet.
3. **Check the audio input device in settings.** Hamlet picks one for you — the
   radio's codec if it recognises one, otherwise the system default, otherwise
   whatever is first in the list. **It can pick the laptop microphone.** If the
   one it picked is not the radio, choose the radio's codec yourself. Your
   choice is remembered and wins from then on.
4. **Connect to the radio.** Use the Connect button.
5. **Check that Hamlet is listening.** Look at the CW terminal panel's summary
   line. It reads `listening` when audio is arriving and `not listening` when
   it is not. Before anything decodes, the terminal's own idle line names the
   device — `listening to <device>` — so you can confirm it is the radio.
6. Set the rig to **14.074 MHz**.
7. Set the rig to **USB-D**.
8. Pick the **Digital** tab.
9. **Leave it alone for two minutes.** Do not press anything.
10. **Afterwards, look at the two files below — whether or not anything
    appeared.** This step is added by unit 234 and is the only way a session
    that showed nothing can be diagnosed later.

### Step 10 — the two files to look at afterwards (added by unit 234)

Do this even when it worked. Especially when it did not.

**The log.**

```
%AppData%\Hamlet\telemetry\<today's date>.jsonl
```

*Measured tonight*: driven exactly the way `App.axaml.cs` drives it, Hamlet's
telemetry writer put today's dated file on disk **12 milliseconds** after
start-up, with no clean shutdown needed, holding one `app_start` line. So **if
there is no file with today's date on it, the Hamlet you ran did not write one
— which means it was not this code**, and step 0 is where to look. Every slot
the Digital tab reads writes a line into this file, so a session that decoded
nothing still leaves several hundred of them.

**The captures.**

```
%AppData%\Hamlet\captures\digital\
```

*Measured tonight*: this folder does not exist on this machine, and
`CaptureDigital` creates it the first time a press succeeds — so **no press has
ever written anything here, on any day.** If you pressed *keep the last 30
seconds* and this folder is still absent afterwards, the press refused. *Added
tonight*: from 1.12.38 onward a refused press writes a `digital_capture_refused`
line into the log above, at `warn`, saying which of the four ways it refused —
nothing was listening, no audio had arrived, or the write failed. Before 1.12.38
it left only a status-bar sentence that the next message overwrote.

*Predicted*: what either file will contain after your session. *Measured
tonight*: that both writers put files on disk when they are driven, and what
each file's absence therefore means.

---

**Step 5 is the one nothing else in this project has written down.** The FT8
path reads the CW decoder's audio tap. The CW decoder only exists while Hamlet
is listening to an audio device. So **if Hamlet is not listening, the Digital
tab is deaf and says nothing about it.** Connecting is what starts the
listening.

Hamlet writes the radio's mode from its band plan when you change tabs and when
the dial moves. If the mode changes on its own after you pick the Digital tab,
that is Hamlet following its map, not a fault. Check it still says USB-D.

---

## 2. What should happen, and by when

Count in slots, not in minutes. A slot is fifteen seconds. Four a minute.

- **The first look arms and claims nothing.** The watch does not report the
  slot that was already running when you arrived. That slot is not lost — it
  was never heard.
- **The first slot after that may be refused.** The audio ring holds thirty
  seconds and it starts empty. Until it has filled past the slot that just
  closed, the honest answer is that the slot could not be decoded, and Hamlet
  says so rather than showing an empty table.
- **Expect the first rows within two or three slots.** Call it a minute.
- Each row is one message out of one slot. On a busy 20 m afternoon expect
  **many rows per slot**, not one.
- Rows accumulate. The table is a session, not a snapshot.
- The clock reading sits on the Digital mode strip. Grey is fine. Amber means
  half a second or worse.

If two or three minutes pass with nothing, go to part 3. Do not conclude the
band is dead — read the words on the strip first.

---

## 3. Every way it can look wrong

Each one: what you see, what it means, what to do.

### The strip says the clock has not been measured

> the clock offset has not been measured, so where the fifteen-second
> boundaries fall is not known and nothing was cut

**Meaning.** Hamlet asks a time server on the internet when it starts and every
ten minutes after that. It has not got an answer yet. It refuses to guess where
the slot boundaries are.

**Do.** Wait a minute for the first query to land. If it never lands, check the
PC is on the network and that nothing is blocking outbound time queries. **This
is not a decode failure and the band is not being reported on.**

### The strip says the clock reading is too old

> the clock offset is too old to cut slots against — *followed by how old*

**Meaning.** The last good reading is over an hour stale. Hamlet will not cut
slots against it.

**Do.** Check the network. The query repeats every ten minutes on its own.

### The clock reading is amber

**Meaning.** The PC clock is half a second or more from UTC. FT8 needs about a
second. You are close to the edge.

**Do.** Let Windows resync the clock. Then watch whether rows start.

### The strip says nothing is arriving

> no audio is arriving, so no slot can be cut

**Meaning.** Hamlet is listening to a device that has delivered no samples at
all.

**Do.** Check the radio is on, the USB cable is in, and the input device you
picked is the radio's codec and not the laptop microphone.

### The strip says the audio stopped keeping up

> the audio stopped keeping up with the clock, so which samples belong to this
> slot is not known and nothing was decoded

**Meaning.** The samples stopped arriving while the clock kept running. Hamlet
will not hand over old audio wearing a new slot's timestamp.

**Do.** Disconnect and connect again. If it recurs, note what else the PC was
doing and keep a capture.

### The strip says a slot finished longer ago than the audio kept

> the slot finished longer ago than the audio that is kept, so it could not be
> decoded

**Meaning.** The ring did not hold the whole slot. Expected once at start-up
while the ring fills.

**Do.** Nothing, if it happens once and then stops. If it keeps happening, the
audio stream is stalling — keep a capture.

### The table is empty and the clock is fine

The strip will say one of these:

> one slot decoded, and nothing on the band looked like FT8 at all

> one slot decoded, and of *N* places that looked like a signal, not one came
> out as a message

**Meaning.** The first says nothing FT8-shaped was found. The second says
signals were found and none of them resolved into a message.

**Do.** The second one is the interesting case — it means signal is arriving
and something downstream of the search is not working. Keep a capture. For the
first, check you are actually on 14.074 in USB-D, check the radio's AF output
level, and check Hamlet's input level meter is moving.

### Nothing is listening, and the tab does not say so

**This is the trap.** With nothing listening, the Digital tab's slot watch does
not run at all. There is no refusal sentence, because nothing looked. The
decoded panel reads:

> nothing decoded yet. Every message that comes out of a slot lands here
> exactly as it was sent, before Hamlet makes anything of it.

and the mode strip reads:

> nothing on this frequency yet. FT8 runs in fifteen second slots, so give it a
> slot or two before deciding the band is empty.

**That is the same screen you get from a healthy Hamlet listening to a dead
band.** The two are not distinguishable on the Digital tab.

**Do.** Look at the CW terminal summary. `not listening` is the answer. If it
says that, go back to part 1 step 3.

### The radio is in simulation and you get Morse

**Meaning.** With no real radio connected, Hamlet's simulated rig supplies a
Morse training source at twelve words a minute instead of a sound card. Slots
are cut from it and decoded, and no FT8 comes out of Morse.

**Do.** Connect the real radio. **No FT8 row can ever come from the training
source**, so nothing on the Digital tab in this state is about a band.

### The `snr` column is an em dash

**Meaning.** This decoder produces a Costas synchronisation score, which counts
how far the sync pattern stood above the tones around it. It is not decibels
and it is not calibrated against anything. Putting it under a heading that says
`snr` would be a number you could act on that is not a measurement, so the cell
stays a dash.

**Do.** Nothing. This is a decision you have not made yet, and the dash is
correct until you do. It is not a fault and it is not a missing feature.

### The table empties itself when you retune

**Meaning.** Rows are cleared when the dial moves more than three kilohertz.
Inside three kilohertz the same signals are still arriving through the same
receiver filter, so a small nudge keeps the session. Beyond it the rows describe
a different piece of spectrum.

**Do.** Nothing. This is deliberate. A band change empties the table on purpose.

### The table stops growing at five hundred rows

**Meaning.** The table holds five hundred rows. Beyond that the oldest fall off
the top. A night at 14.074 is over five thousand slots and an unbounded table is
a leak with a scrollbar.

**Do.** Nothing. If you want to keep a session, keep a capture while it is
running.

### The plain-English panel says nothing

**Meaning.** It reads its idle line and nothing else. What a message *means* in
ordinary words is yours to word and nobody has worded it. Three invented cards
were removed from that panel and nothing was put back on purpose.

**Do.** Nothing. This is finished work, not a gap.

---

## 4. What to record when it goes wrong

Press **keep the last 30 seconds** on the waterfall panel of the Digital tab.

It writes two files into:

```
%AppData%\Hamlet\captures\digital
```

named `ft8-<date>-<time>.wav` and `ft8-<date>-<time>.txt`. The text file holds
what the radio was doing — frequency, mode, the clock reading, the band-plan
neighbourhood.

**What a capture is worth.** It is thirty seconds of audio taken from wherever
the ring happened to be. It starts mid-slot. So it holds **two partial slots**,
or one whole slot and two partial ones, and it is not trimmed to boundaries.
It is diagnostic material. **It is not a decode and not a record of what was on
the air.**

Pressing it also decodes what it kept and adds any rows to the same table,
without clearing what is already there.

If nothing is listening, the press writes nothing and the status bar says so.

**Can a captured WAV be replayed through this path offline today?**

**No. Nothing does this.** There is a test in the tree that plays a WAV file
through exactly this path — the ring buffer, the slot watch, the reader, the
table — but it opens files from a fixed folder of somebody else's reference
recordings and there is no way to point it at one of your captures without
editing it. **There is no command, no tool and no menu item that replays a
capture.** If you want one, that is a piece of work, not a switch.

Keep the captures anyway. They are the only evidence a failed session produces.

---

## 5. The full test suite — yours, once, uncontended

Run this once, before you sit down at the radio. Not while anything else is
running, and not while Hamlet is open.

### Read this first: until tonight, this run rewrote your settings (added by unit 235, *measured*)

**This is measured, not predicted, and it was measured on your own folder.**

Unit 235 took a SHA-256 snapshot of every file in `%AppData%\Hamlet`, ran
**one class** of `Hamlet.App.Tests` — nine tests — and snapshotted again:

- **`settings.json` was rewritten.** 1200 bytes to 1352, a different hash.
- **`spots.db-shm` was touched**, two seconds earlier, contents unchanged.

Nothing in those nine tests asked for that. It is the `MainWindowViewModel`
constructor, and it did three things to your folder before a single test body
ran: opened your real `spots.db`, saved a byline index, and **started a live
callsign lookup and saved its answer** — which rewrote your grid square, your
latitude and longitude and your licence class. Twenty files in that project
construct that view model, thirty-nine times.

**So the instruction at the top of this section — run the full suite by hand,
once, immediately before you sit down at the radio — has been rewriting the
settings the radio session depends on, minutes before the session.**

**It is fixed as of 1.12.39.** The data folder now has a seam, the test project
points it at a temporary folder before any test runs, and the same nine tests
were re-run with a snapshot either side: **all fourteen files byte-identical.**
Two tests were added that fail if that ever stops being true.

**What to check afterwards anyway**, because your file has already been through
this several times today:

1. Open **Settings** and confirm your **grid square** and **licence class** are
   what you expect. Those are the two fields measured as having been overwritten
   by the lookup.
2. **Confirm the audio input device.** *Measured tonight*: `AudioInputDeviceId`
   is **absent** from your `settings.json`, which means **no device has ever
   been chosen and Hamlet is picking one for you every time**. That is step 3
   of part 1 and it is the single commonest cause of a silent FT8 screen.
3. Your **callsign is intact** — it was compared before and after and is
   byte-identical.

A backup of the whole folder was taken before any of tonight's runs and is at
`%TEMP%\hamlet-unit235-backup` (14 files, 1,803,040 bytes). **Nothing was
restored from it** — writing into your profile is yours, not a session's.

Three projects, run separately:

```
dotnet test tests\Ft8Sharp.Tests\Ft8Sharp.Tests.csproj ^
  --logger "trx;LogFileName=bench-ft8sharp.trx" --results-directory TestResultsBench

dotnet test tests\Hamlet.App.Tests\Hamlet.App.Tests.csproj ^
  --logger "trx;LogFileName=bench-app.trx" --results-directory TestResultsBench

dotnet test tests\Hamlet.RadioEngine.Tests\Hamlet.RadioEngine.Tests.csproj ^
  --logger "trx;LogFileName=bench-engine.trx" --results-directory TestResultsBench
```

**Read the numbers out of the TRX file, not off the console.** Open each `.trx`
and find the `<Counters ... />` element. It carries `total`, `passed`, `failed`
and `notExecuted`.

Two traps, both already paid for by earlier sessions:

- **No project here prints a summary line**, so a run that stopped halfway looks
  exactly like a run that finished. The TRX counters are the only honest count.
- **The console logs are UTF-16.** Anything searching them as UTF-8 finds
  nothing and reports zero.

**There are two real failures already in the tree and they are not new.** Both
are in `Hamlet.RadioEngine.Tests.Cw`:

- `WhereTheTrackerStartsDoesNotDecideThis`
- `AStationElsewhereIsStillFound`

They are CW, they pre-date this work, and nothing in the FT8 phase touches
them. **A red count of two is the expected reading, not a regression.** What
would be a regression is a different name in that list.

There is also **one skipped test** in `Ft8Sharp.Tests` — a table-writing gate
that is skipped by design.

**Do not run `Hamlet.App.Tests` filtered on `FullyQualifiedName~Views`.** That
filter hung for twenty-five minutes, never returned, and left a process holding
a lock that failed the next build. It is recorded as HM-OPEN-069. Run the whole
project or name individual classes.

**How long.** `Ft8Sharp.Tests` took just under five minutes on this machine
tonight. `Hamlet.App.Tests` is a few minutes. `Hamlet.RadioEngine.Tests` runs
real signal processing over recorded audio and is well over half an hour.
**Budget an hour and leave the machine alone.**

---

## 6. What is not claimed

- **Sensitivity has not been met.** Hamlet's decode rate at −21 dB falls short
  of the published figure. It is measured, it is written down, and the two
  things that would move it are with you rather than with a session. **Strong
  signals are unaffected.** A busy 20 m afternoon is strong signals.
- **The `snr` column carries no number**, because this decoder produces no
  decibels. See part 3.
- **The plain-English panel is empty**, because what a message means is yours
  to word.
- **No unit of this phase has ever heard a radio.** Every row this application
  has put on that table came either from audio it generated itself or from
  somebody else's recording played through it from a file.

**So everything in this sheet is a prediction. The bench check is the
measurement.**
