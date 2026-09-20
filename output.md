```
READ IN THIS ORDER.

A. The phase goal - Hamlet holds what it has. Step 0 done, 1-5 not started.
B. Step 0's criteria 0.1 to 0.5 - 0.2, 0.3, 0.4 and 0.5 met and measured. 0.1 is
   answered and its answer is a NEGATIVE: there is no such commit, and the line
   that dropped the device is named below and predates 1.13.30.
C. The report last, and section 4 raises 5 items on top of the carried queue.
```

```
UNIT:       369 - complete at task 3 of 3 - 2026-09-20 17:21
PHASE GOAL: Stop Hamlet losing things it already had. Everything banked in the
            PSK31 and Olivia threads that is screen, record or test, hardened
            while Tim is away, needing neither the radio nor him.
UNIT GOAL:  A setting Tim chose once outlives an upgrade, and a send that cannot
            go says which of the two faults it actually is - nothing chosen, or
            a chosen device that will not open.
ADVANCED:   yes - step 0 closes; the loader no longer discards a settings file,
            and the refusal that named the wrong fault is two refusals now
NUMBER:     settings-file shapes that load whole 0 -> 3; the commit that dropped
            the device: NONE EXISTS - the line is AppSettings.cs LoadFrom's
            `return new AppSettings()`, older than 1.13.30
DRIFT:      carried - 0 consecutive units without advance (was carried)
```

## 1. What Claude did

**Complete, at task 3 of 3.** All three tasks done, each committed and pushed on
its own. Machine QUIVERFULL, project Hamlet (gate verified against the tree:
`SHACK_FACTS.md` and `CwProbabilisticDecoder.cs` present, `CoreHMI.sln` and
`MURC.sln` absent, root `C:\Source\HamLet`, `Hamlet.sln`), branch `main`.
`PHASE_STATUS.md` line 1 names *Hamlet holds what it has*, so the phase was
installed and the previous session's block is cleared.

Commits: `cd0d37a0` task 0, `564f8475` task 1, `b212968e` task 2.

### The commit and the line — this is criterion 0.1, and it is a negative

**No commit between 1.13.30 and 1.13.48 dropped the transmit device, because no
commit in that window touched the settings model, its loader, its migrations or
the transmit-device picker.** The evidence, all of it re-runnable:

| Measured | Result |
|---|---|
| `<Version>1.13.30</Version>` set at | `681d45c8` |
| `<Version>1.13.40</Version>` set at | `d66a6ace` |
| `<Version>1.13.48</Version>` set at | `ec4b466e` |
| Commits in `681d45c8..ec4b466e` | 119 |
| …naming any file under `src/Hamlet.App/Settings/` | **0** |
| `git diff 681d45c8 ec4b466e -- src/` | 31 files, `AppSettings.cs` not among them |
| `git diff 681d45c8 HEAD --` settings, `SettingsViewModel.cs`, `SettingsWindow.axaml`, `App.axaml.cs` | **empty** |

The settings model, its loader, the settings UI and `App.axaml.cs` are
byte-identical from 1.13.30 to HEAD. The loader did not change, so the loader is
not where a change dropped the value.

**The line is real, though, and I found it. It is older than 1.13.30.**

```
src/Hamlet.App/Settings/AppSettings.cs   SettingsStore.LoadFrom
        catch (Exception)
        {
            return new AppSettings();      <-- this line
        }
```

`LoadFrom` deserialized the whole file in one call and answered **every**
exception with bare defaults. So **one value anywhere in `settings.json` that the
build could not parse discarded every other value in it** — transmit device,
receive device, grid, callsign, license class, drive, the lot. `App.axaml.cs:141`
saves on exit, so those defaults then went over Tim's own file and the loss became
permanent. From his chair that is exactly *"the settings lost the listing
setting"*: he lost the device, re-chose the USB Audio CODEC, and FT8 transmitted.

I found it by accident and then on purpose: my first reconstructed fixture used
two enum member names that do not exist (`SetByHand`, `Lookup` — the real ones are
`EnteredByOperator` and `LookedUp`), and a well-formed file with 47 good keys and
one bad one came back **entirely empty**. That is the fault, reproduced.

**The file's own remarks had seen half of it.** `SettingsStore` carries a
`JsonStringEnumConverter` because without it a hand-edited `"General"` threw and
*"EVERY setting silently reverts to defaults, which is a spectacular punishment for
a reasonable guess."* That fixed the one value it named. The punishment was never
specific to that guess.

### Task 0 — the unit opens

`PHASE_OUTCOME.md` carries `UNIT 369` under step 0. Patch-bump 1.13.56 → 1.13.57
with its comment block. `PROJECT_CARD.md`'s `PHASE` and `PHASE_SET` moved to the
hardening phase. **`DECISIONS.md` HM-DEC-166** records, as Tim's ruling of
2026-09-20, the hardening phase set and the Olivia phase archived at 38 of 40 with
5.4 and 6.1 his — and states that the ruling is what licenses the card's two lines
moving, since the card is changed only by ruling. Carry-forward before any change:
**app 190 of 190, engine 146 of 146**, `TheSendReachesTheAirTests` among them.

### Task 1 — settings survive an upgrade

`TheSettingsSurviveAnUpgradeTests`, **watched failing first**, now 15 of 15.

Three fixtures reconstructed from the tree at 1.13.30, 1.13.40 and HEAD. They are
JSON text, not a round-trip of today's class, because a fixture built by
serializing the current type agrees with it by construction and could never catch a
renamed key. The three shapes are identical — which is itself the evidence for 0.1
— so they carry the same 47 keys with different values, separated only by
`Psk31AlcReference`, which this unit adds.

- **`NothingTheFileCarriesIsDropped`** walks every key in the file rather than a
  list of seven names, so a field dropped by a rewrite years from now fails here
  without anybody having thought to add it.
- **`OneUnreadableValueCostsThatValueAndNotTheFile`** and its nested twin are the
  two that were red. They are the fault above.
- The seven values 0.2 names are asserted by value on each fixture; a missing new
  field takes its default and is written back; each file round-trips byte-identically.

**The repair.** The whole-file read is tried first and is unchanged for every file
in ordinary use. Where it throws, `Salvage` reads the file property by property,
keeps everything readable, defaults only what is not, and **descends into a nested
object rather than writing it off** — the operator profile holds four of the seven
values 0.2 names, so one bad field inside it must not cost the callsign beside it.
A file that is not JSON at all is still defaults; HM-DEC-018 is untouched and
`SettingsRoundTripTests.CorruptSettingsFile_YieldsDefaults` still says so.

**The ALC reference is now a setting, and it was not one before.** Criterion 0.2
names it among the values that must survive. It did not exist in `AppSettings`: it
was learned into `MainWindowViewModel.Psk31AlcReference` and written nowhere, so
every restart threw away a measurement that had cost a transmission to take. It is
now on `AppSettings` beside the drive — same subject — read back at construction
and saved where it is learned, carrying `TakenUtc` so a reference from last week
reads as last week's (HM-DEC-111).

### Task 2 — a missing device says so

`TheRefusalNamesTheFaultTests`, **watched failing first**, now 7 of 7.

- **0.3.** No device chosen yields `send_refused stage arm reason
  no_transmit_device` and, on the panel, *No transmit device is chosen. Open
  Settings and pick the radio's sound card.* — the whole of what it says, on both
  the FT8 path and the keyboard-mode path, not a clause buried after *Hamlet
  composed … and sent nothing*. The sentence is one constant and the view takes its
  three pieces from it, so the screen and the test cannot drift.
- **0.3, the link.** A new `hm-inline-link` button style: no chrome at rest,
  underlined and amber so it still says it can be pressed (§0.5.1, HM-DEC-087),
  never greyed. **Only the no-device refusal offers it** — a device that is
  unplugged is not fixed by opening the picker, and advice that does not work is
  worse than none.
- **0.4.** A chosen device that will not open keeps
  `transmit_device_would_not_open` and carries the device, the rate asked for
  (12000) and the OS error text — in `send_refused`, in every `transmit_path`
  attempt, and on the panel in words. The keys are written only where there is a
  device to name: an absent key and an empty one are different pictures.

### Decisions I made for myself, reproduced in full

1. **I recorded 0.1 as a negative rather than naming a commit that fits.** The
   criterion presupposes a commit in that window. The evidence says there is none.
   Naming a plausible one would have been an invented answer (§0.0).
2. **I added ALC-reference persistence, which is new storage rather than only "how
   a setting survives."** §10 says not to change what a setting means; this changes
   nothing's meaning, and 0.2 and task 1 both name the ALC reference among the
   values that must survive, so the criterion cannot be met honestly without it.
3. **I put two names on `docs/carry-forward-tests.txt`** —
   `TheSettingsSurviveAnUpgradeTests` whole, and
   `TheRefusalNamesTheFaultTests.NoDeviceChosenYieldsItsOwnReasonAndItsOwnSentence`
   by type and method. The work instruction did not ask for this. The list's own
   rules say a mode guard goes on permanently; settings and the refusal sentence
   are not modes, but both are the fault that took FT8 off the air for five days.
   Cost measured: under 0.4 s for the whole settings type.
4. **Nothing was dropped.** Tasks 0, 1 and 2 are all done. The named drop candidate
   — task 2's Settings link — was built, not dropped.

## 2. What the owner should expect

**Your settings will not be lost like that again.** Hamlet used to throw away your
entire settings file if there was one value in it that the new build could not
read, and then write the empty one back over yours when you closed the app — which
is how the transmit device went missing. It now keeps every value it can read and
only forgets the one it genuinely cannot.

**And if a device is ever missing you will be told which fault it is.** Press CQ
with no sound card picked and the send area says *No transmit device is chosen.
Open Settings and pick the radio's sound card*, with **Settings** as a word you can
click to go straight there. If you have picked one and it will not open — unplugged,
moved to another USB socket — it says that instead, and it now names the device, the
rate it asked for and exactly what Windows said back.

**What will look wrong but is not.** The ALC reference now survives a restart, so
the line under the S-meter may say something like *learned from FT8 send at 23:40
UTC, 4320 minutes ago*. That is correct and deliberate — the reading is real and
its age is part of it — but the wording only counts in minutes, so an old one reads
as a large number of them. Item 3 in section 4.

## 3. What you should see

**The question this unit was commissioned to ask: which commit dropped the transmit
device on load? Answer: none did.** 119 commits between 1.13.30 and 1.13.48 and not
one of them touched the settings model or its loader; the diff from 1.13.30 to HEAD
over those files is empty. The line that dropped it is `return new AppSettings()` in
`SettingsStore.LoadFrom`'s catch, and it is older than 1.13.30 — it fires on any
single unreadable value and takes the whole file with it.

**Settings-file shapes that load whole: 0 → 3.** Nothing in the tree tested this
before; three reconstructed files now load with every one of their 47 keys intact
and round-trip.

In the application, in your terms:

- Hamlet stops forgetting what you chose. A settings file it cannot fully
  understand costs you one value instead of all of them.
- The reference Hamlet measured off your own FT8 sends is still there tomorrow
  morning. It used to be gone every time you closed the app, and buying it back
  cost a transmission.
- A send that cannot go tells you the truth about why, and the fix is one click
  away inside the sentence.

Tests: app **206 of 206** on the carry-forward list (190 before this unit, plus its
16), engine **146 of 146**, unchanged. `TheSendReachesTheAirTests`,
`TheUnslottedSendTests`, the byte-identical tests and `BindingHealthTests` all green
and unedited — criterion 0.5. No red after that was green before.

## 4. What's blocking us

**Carried forward per HM-DEC-139, verbatim from the last report's queue: the queue
was empty.** Unit 368's report stopped at its phase check and raised nothing, and
the intervening session recorded no asks. Five new items below, most-blocking
first.

**1. Criterion 0.1 presupposes a commit that does not exist, and step 0 is marked
done anyway.** The search is complete and the cause is named, but the answer is a
negative and the line predates the window the criterion names. *Ruling wanted:*
whether that satisfies 0.1 or whether step 0 goes back to partial. *Reasoning:* the
criterion's purpose is to know what broke it before trusting the fix, and that
purpose is served — the mechanism is named, reproduced and repaired. *Rejected:*
naming the closest-fitting commit, which would be a guess presented as a finding.

**2. The `no_transmit_device` refusal was never wrong about the device — the
refusal Tim actually saw was `transmit_device_would_not_open`, and the split above
assumes his device id was still in the file at that moment.** If instead the file
had been reset to defaults, he would have seen `no_transmit_device`, and unit 362's
report says he saw the other. *That means the device id survived and the device
genuinely would not open* — which is a different fault from the settings loss, and
this unit repaired both without proving which one he hit. *Ruling wanted:* whether
that matters enough to chase. His telemetry from 2026-09-14 to 09-19 would settle
it in one read; nothing in this repository has it.

**3. `LearnedAlcReference.Ago()` counts only in seconds and whole minutes.** Now
that the reference survives a restart, a legitimate value is *4320 minutes ago*.
Honest but poor. *Ruling wanted:* whether to extend it to hours and days. *Not done
here* because `TheAlcSentenceTests` asserts the current forms and §10 said not to
touch wording beyond the refusal's.

**4. The archived Olivia plan's checkboxes say 25 of 40, not 38 of 40.**
`docs/phase-olivia-run/PHASE_PLAN.md` has 25 boxes ticked and 15 open, including
several the unit reports say were met (1.5, 4.1–4.8, 5.1–5.3). HM-DEC-166 records
38 of 40 as ruled, which comes from the reports. *Reported, not repaired* — §5 says
repair nothing but this unit's, and an archived phase's plan is not this unit's.

**5. No settings file in the tree was ever reconstructed before this unit, and the
three fixtures are mine.** They carry the right key set, but their *values* are
invented — nobody's real 1.13.30 file was available. They prove the shape loads,
not that Tim's particular file does. *Raised once, not a blocker.*
