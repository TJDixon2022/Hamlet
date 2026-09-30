# Work instruction 503 - the tab is the mode

**Hand run. One unit.** The owner has spent days unable to tell whether decoding is failing or the
radio has been moved to something else. The app has been deciding what he meant from where the
dial sits - the map, mode-follow, block widths, a hold that survives a band change but not a
restart. All of that goes. **The tab is what he means.**

**No test against a recording, a fixture, a floor or copied telemetry** (R96). Verify by building
`Hamlet.sln` with warnings as errors and running the app carry-forward line. **The owner's report
at the radio is the test.**

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
  to `RUN_LEDGER.md`. Touch nothing under `tools\arbiter\`. Tick nothing in `PHASE_PLAN.md`.
- One `dotnet test` invocation per line, filtered, with a `timeout`. Never background and poll.
  The app line loses names to the dispatcher loop; re-run once, count neither way.
- Apostrophes in quoted heredocs break; `;`, `rm` and `git rm` are refused; Python cannot run
  here; `-m` more than once for a multi-line commit. Scripts go in `.run-unit\unit503-<name>.sh`
  and are not committed.
- **Nothing that keys or transmits.** This unit changes what the app writes to the radio's mode,
  filter and receive settings, which §0.2 allows and §12.4 governs: **no value guessed, every write
  named in the report.**
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. What the telemetry showed, and the owner's ruling

**2026-09-30, 12:30 UTC.** The app restarted, read the radio at 7.0472 MHz in CW, and two seconds
later wrote **`mode_followed: Usb, dataMode: true`**. Then CW → USB-D → CW → USB-D at 12:37 and
12:43. The radio was 50 Hz outside unit 499's W1AW block, so the map called the frequency data and
mode-follow wrote it. The button's hold had not survived the restart.

**The owner's ruling, R111:**

> *"If I'm on the CW tab, we have CW settings. If I'm on the data tab, we have data settings. I've
> made the decision that I'm chasing either CW or data by being on the tab, and the settings should
> follow that. Right now, I'm spending a lot of time trying to figure out if the decoding isn't
> working or if the radio has been set to something else."*

---

## 3. The change

### One - the tab sets the mode

- **The CW tab means CW.** While it is selected, the radio is in CW, with the CW filter and the CW
  receive settings from `mode-receiver-conditions.json`. **It stays there**: turn the dial anywhere
  on any band, restart the app, cross any block on the map - still CW.
- **The Digital tab means data.** USB-D, the data filter, the data receive settings. Same
  permanence.
- **The Voice tab means the voice mode** the map says for the frequency - USB above 10 MHz, LSB
  below - and is the only tab where the map still chooses, because voice genuinely differs by band.
- **Selecting a tab writes its mode once**, and again only if the operator changes tab.
- **On startup, the tab that was last selected sets the mode**, before anything else looks at the
  radio.

### Two - the map stops writing the radio

- **Mode-follow from the map is retired.** The map still says what is at a frequency - the card,
  the blocks, the neighbours, the licence line - and it **writes nothing**. `FollowTheMapAsync`
  no longer writes a mode. Say in the report what it still does, if anything, and what is dead.
- **Unit 499's W1AW block width and unit 495's mode-follow hold become irrelevant.** Leave the
  data as it is; the button now only tunes, because the tab already set CW.
- **Unit 499's `TheW1awPressStaysInCwTests` and every test that asserted a mode-follow write** are
  re-pinned to R111: the tab writes the mode, the map does not. Name each.

### Three - the operator's hand on the radio wins

- **If the operator turns the radio's own mode knob**, the app follows the radio, not the tab. His
  hand on the radio is the highest authority (HM-DEC-056).
- **The tab says so plainly**, as the send panel already does: *"The radio is in USB rather than
  Morse. Switch to CW and it will be ready."* That sentence stays and is the only thing that
  happens - **the app does not write the mode back.**
- **His hand on any receive setting still holds** - preamp, AGC, filter - until he changes it or
  changes tab. Changing tab re-applies that tab's settings, which is his decision by the ruling.

### Four - what must not change

The CW detector, the marks, the run reader, the scope, the terminal, the one layout, the W1AW
button's tuning, the verdict buttons and row.

**Watch it fail first**, headless:

1. On the CW tab at 7.0472 - inside the RTTY block by unit 499's data - the app writes CW once and
   never a data variant, however the dial moves. **Red today**: mode-follow writes USB-D.
2. Restart with the CW tab selected: the first mode write is CW.
3. Select the Digital tab: one USB-D write with the data filter and settings; back to CW: one CW
   write with the CW filter and settings.
4. The radio reports USB while the CW tab is selected: the app writes nothing, and the tab's
   sentence says the radio is in USB.
5. **Every mode write the app makes is listed** in the report, with what causes each.

---

## 4. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 503 - STEP 11`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 503.
- Patch-bump `Directory.Build.props`.
- **Append R111 to the rulings section of both `PHASE_PLAN.md` copies**, in the owner's words
  above. **Touch no checkbox.**
- `DECISIONS.md`, newest first, **HM-DEC-207**, headline *The tab is the mode; the map writes
  nothing*, quoting him, naming the 12:30 restart that wrote USB-D two seconds after reading CW,
  and that HM-DEC-056's rule - the operator's hand on the radio wins - stands above it.

---

## 5. Report

Section 2, for the owner, in plain words:

- rebuild;
- **on the CW tab the radio is in CW and stays in CW** - dial, restart, block, whatever;
- on the Digital tab it is in data;
- the map no longer touches the radio;
- if you turn the radio's mode knob yourself, the app follows you and says so, and does not fight
  you;
- **so from now on, if decoding is wrong, it is the decoder.**

Section 1: what changed, file by file, **every mode write the app can now make and what causes
it**, what `FollowTheMapAsync` still does, and that the build and the app line are green.
Section 3: the five cases. Section 4: anything left, a line each.
