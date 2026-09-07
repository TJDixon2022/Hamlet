# PHASE_PLAN.md

**Governed by `PHASE_CONTROL.md`. Approved by Tim, 2026-09-06.**

---

## The phase

**Hamlet works stations on the air.**

## The description

Hamlet reads FT8 as well as anything and cannot answer. On 2026-09-04 it decoded
fourteen messages from one slot on 14.074, five continents, down to -21 dB. On
2026-09-05 cross-slot combining read **252 of 306 trials at -21 dB against the
port's 13**, zero wrong. The operator watches the band and cannot say a word into
it.

**Tim operates a licensed station on an antenna and Hamlet transmits on the air.**
Ruled 2026-09-06. **HM-DEC-008 and HM-DEC-098, which required a dummy load, are
withdrawn in full** - not a stage, not a fallback, not to be referenced by any
unit. Step 0 removes them from `CLAUDE.md` so no session finds a rule it cannot
satisfy and halts on it at two in the morning.

**The shape, ruled 2026-09-06:**

- **One click, one message.** Hamlet transmits because the operator clicked. It
  never sequences, never decides, never continues a contact on its own.
- **Right-click a decoded row and it sends, in the next slot, with no
  confirmation.**
- **Hamlet shows what comes next and forbids nothing.** The expected reply is
  highlighted; everything still valid stays clickable. FT8 loses transmissions
  constantly, so **sending the grid a second time is correct behaviour**, not a
  mistake to be greyed out.
- **A contact is never closed by the app.** Nobody is obliged to send `73`, an
  operator may be working three stations at once, and Hamlet is not the radio
  police. A row shows *waiting on him*, *your move*, *complete* or *gone quiet*,
  with slot counts. **It reports; it does not rule.**

**A session reading this cold should understand:** the receive path is finished
and is not this phase's subject; every transmission follows a click; the abort is
the one thing that must work before anything else does; and **the closing step is
a contact Tim makes, which no unit can perform for him.**

---

## This phase runs unattended until the last step

**Steps 0 to 5 need no radio and no ruling.** Every criterion is reachable by a
unit on the development machine. The waveform is proved against Hamlet's own
decoder, the audio path against a loopback, the state machine against recorded
slots.

**Step 6 is Tim keying a transmitter, and it cannot be automated.** It is the only
step that waits on him, it is last, and nothing before it is blocked by it.

### The steps are a hypothesis, not a contract

The arbiter may, without asking, recording the evidence in `PHASE_OUTCOME.md`:
**reorder** steps that depend only on their stated entry; **replace** a step with
a better approach; **retire** a step measurement shows cannot pay for itself,
closing it *unachievable* with the number; **add** a step the phase needs;
**move a target** found to have been measured wrong.

### Named alternatives to stopping, ruled in advance

| If | Do not stop. Instead |
|---|---|
| a target is not reached | close the step with the figure reached and what was tried |
| an approach fails | abandon it, record its cost, take another |
| the radio is wanted | close on the loopback or the recording, mark it *deferred*, name what Tim must do |
| a defect is found in the receive path | record it, work around it, continue |
| the tree disagrees with this plan | the tree wins. Report the mismatch and continue |
| a licence, naming or scope question arises | **already ruled below.** Do not raise it |
| the shell refuses a call | use the file-editing tools |

---

## The three things the arbiter may not reason past

1. **The abort.** Every path that keys the transmitter has a same-thread,
   no-await abort - CI-V `0x17` with `0xFF`, PTT off as the fallback. **No unit
   ships a keying path before its abort is watched to fire.**
2. **One click, one transmission.** Hamlet transmits because the operator
   clicked. **Never on a timer, never on a decode, never to continue a contact.**
   A transmission he did not ask for is this phase's one unrecoverable fault - it
   goes out over other people's band and cannot be taken back.
3. **Licence privileges.** Hamlet never transmits outside them. The Settings gate
   is not bypassable from any send path.

---

## Rulings in force

**Not to be re-argued by any unit.**

**The dummy load is withdrawn.** HM-DEC-008 and HM-DEC-098 superseded, 2026-09-06.
**Do not reference it, do not propose it, do not treat its absence as a risk.**

**One click, one message.** Ruled 2026-09-06.

**Right-click sends immediately, in the next slot, with no confirmation.** Ruled
2026-09-06.

**Nothing is forbidden in the menu.** The expected next message is highlighted;
everything valid stays clickable; a repeat is correct behaviour and shows its
count.

**A contact is never closed by the app.** Complete is shown when the exchange has
what a QSO needs. `73` is politeness, not a requirement.

**`Ft8Sharp` is a faithful MIT port and nothing in this phase changes a line of
it.** `Ft8Sharp.Deep` is GPL-3.0.

**The engine is not told that tabs exist** (§0.1).

**Nothing interprets a message** (§12.1). A row's state is derived from which
messages passed between two callsigns, which is bookkeeping, not meaning.

---

## What a unit runs

**A unit runs no test suite.** Tim runs them at the end of the phase.

**A unit may run only the unit test it constructs in that work instruction**,
filtered by exact name, foregrounded, with a stated timeout. **An unfiltered
`dotnet test` on any project is forbidden.**

**Never background a command and poll for it.** Three sessions were killed by the
watchdog on 2026-09-05, at 33 to 38 minutes, each sitting in
`until grep -q "exited with code" ...; do sleep 15; done` with a 900,000 ms
timeout against a twelve-minute watchdog.

`dotnet build` is allowed, foregrounded, with a timeout.

**A unit may not add a test without naming the breakage it would have caught.**

Known reds, inherited, **never chased**:
`CwAdjudicationTests.ASpeedChangeInRealisticAudio`; the 51 CW cases in
`docs/unit239-failing-set.txt`; the `Ft8Sharp.Deep.Tests` whole-type-list
tripwire.

---

## Step 0 - the dummy load is gone from the tree

**Delivers:** `CLAUDE.md` §0.2 rewritten and the superseding ruling recorded.

**Entry:** none.

**Exit:**
- §0.2 replaced with the text delivered alongside this plan. **No mention of a
  dummy load survives anywhere in `CLAUDE.md`.** *must-pass*
- A `DECISIONS.md` entry in Tim's name, dated 2026-09-06, superseding HM-DEC-008
  and HM-DEC-098, with his reasoning: licensed operator, licensed bands, live
  antenna. *must-pass*
- **Every other reference in the tree found and reported** - `ARBITER.md`,
  `PHASE_CONTROL.md`, `ft8sharp-spec.md`, older phase archives. Live documents
  are corrected; **archived phase records are left alone and named**, because
  rewriting history is worse than a stale archive. *must-pass*
- The abort requirement and the one-click rule survive the rewrite verbatim.
  *must-pass*

**This step exists because a rule in a file is read by every session.** Deleting
it from a conversation does not delete it from the tree, and a session that finds
a rule it cannot satisfy halts.

**Depends on:** nothing.

---

## Step 1 - the abort works before anything can key

**Delivers:** a proven abort path, ahead of any code that transmits.

**Entry:** step 0.

**Exit:**
- A same-thread, no-await abort: CI-V `0x17` with `0xFF`, PTT off as fallback.
  **No `await` on the abort path, asserted by a test.** *must-pass*
- **Watched to fire**, against a fake CI-V transport, from every state a
  transmission can be in - about to key, keying, mid-transmission, waiting for
  unkey. *must-pass*
- **Fires when the transport is dead**, the port is gone, or the radio does not
  answer. The fallback is exercised, not reasoned about. *must-pass*
- It cannot be disabled, deferred, or made conditional. *must-pass*
- **No transmitting code exists yet when this step closes.** *must-pass*

**This is the one thing that must work when everything else has failed.**

**Depends on:** step 0.

---

## Step 2 - Hamlet's own decoder reads Hamlet's transmission

**Delivers:** FT8 audio generation, proved against the receive path.

**Entry:** step 0. Independent of step 1 - it produces samples and keys nothing.

**Exit:**
- A message becomes 79 symbols becomes audio at the radio's sample rate: Costas
  arrays in place, 6.25 Hz spacing, 0.16 s a symbol, 12.64 s total, continuous
  phase. *must-pass*
- **`Ft8Sharp` decodes it back to the message that went in**, over at least a
  hundred messages including compound callsigns, grids, reports and `RR73`.
  *must-pass*
- **Byte-identical to `ft8_lib`'s encoder** where one is available to compare
  against - the port already encodes, and 51 of 51 fidelity tests pass. **Reuse
  it rather than writing a second encoder.** *must-pass*
- Level and clipping stated, with what the radio's input expects. *must-pass*
- Timing: the audio starts on the slot boundary and runs 12.64 s, measured.
  *must-pass*

**The receive path is the oracle.** If Hamlet cannot read its own transmission,
nothing else in this phase is worth building.

**Depends on:** step 0.

---

## Step 3 - the audio reaches the radio and the radio keys

**Delivers:** audio out to the USB codec, PTT via CI-V, unkey.

**Entry:** steps 1 and 2. **The abort must be proven first.**

**Exit:**
- Audio plays to the radio's USB input at the right device, rate and level.
  *must-pass*
- **Key, transmit, unkey**, with the unkey happening even if the audio path
  throws. *must-pass*
- **A loopback proves the whole chain**: generate, play, capture on the tap,
  decode, and get the message back. **This needs no antenna and no radio state**
  and is the closing evidence for this step. *must-pass*
- The transmitted slot is recorded to telemetry with what was sent and when.
  *must-pass*
- **The licence gate is in the path** and refuses out-of-privilege frequencies,
  asserted by a test. *must-pass*
- **Nothing keys without an operator action reaching this code.** There is no
  entry point that transmits on a timer. *must-pass*

**Depends on:** steps 1 and 2.

---

## Step 4 - the row knows where the contact stands

**Delivers:** per-station QSO state, shown, deciding nothing.

**Entry:** step 0. Independent of 1, 2 and 3 - it is receive-side bookkeeping.

**Exit:**
- Per station: which messages passed each way, when, and how many slots ago.
  *must-pass*
- Four states shown per row - **waiting on him**, **your move**, **complete**,
  **gone quiet** - with slot counts. *must-pass*
- **Complete means the exchange has what a QSO needs**, both calls, both
  grids or reports, both acknowledgements. **`73` is politeness and its absence
  never withholds complete.** *must-pass*
- **Nothing is ever closed, hidden or forbidden by the app.** A complete contact
  still offers `73`; a gone-quiet one still offers everything. *must-pass*
- **A station working three others at once reads as gaps, not as a fault**,
  proved against recorded slots where that happens. *must-pass*
- Derived from recorded captures, not from the air. *must-pass*

**Depends on:** step 0.

---

## Step 5 - right-click and it goes

**Delivers:** the menu, the CQ button, and the reserved Send area filled.

**Entry:** steps 3 and 4.

**Exit:**
- **A CQ button** sending `CQ KC3QIS FN00` from the operator's own settings, no
  typing. *must-pass*
- **Right-click a decoded row** and the menu offers every message valid at that
  point, **with the expected one highlighted and none forbidden**. A repeat shows
  its count - *grid, 2nd time*. *must-pass*
- **Choosing one transmits in the next slot with no confirmation.** *must-pass*
- **One click sends exactly one message.** Asserted by a test: after a send,
  nothing further transmits without another click. *must-pass*
- What is being sent, and to whom, appears in the Send area reserved beneath the
  waterfall. *must-pass*
- **Out of licence privileges, the menu says so and sends nothing.** *must-pass*

**Depends on:** steps 3 and 4.

---

## Step 6 - Tim works a station

**Delivers:** a contact.

**Entry:** steps 0 to 5.

**Exit:**
- **Tim answers a CQ on 14.074 or 7.074 and completes an exchange.** *must-pass*
- The transmitted slots appear in telemetry and the row reads *complete*.
  *must-pass*
- **What he saw, in his words, and anything that surprised him**, recorded.
  *must-pass*

**No unit can perform this step.** It is last, nothing before it is blocked by
it, and the phase is otherwise complete without it.

**Depends on:** steps 0 to 5.

---

## What is not in this phase

- **Automatic sequencing.** *If I get a response, send `73`* - deliberately out.
  One click, one message, and the sequence is a later ruling once the single shot
  has been watched on a real band.
- **Logging.** FG-004 is named in Settings and is its own work.
- **FT4, PSK31, WSPR transmit.** This phase is FT8.
- **CW send**, which §0.2's licence toggle already anticipates.
- **The OSD re-encoding count**, `ReusableWindow`, `ProcessDelayForTests`, the
  tap's owner, the waterfall's first row, unit 237's Extensible conclusion, work
  instruction 231's four tree items, `validate-output.bat`'s permitted-spellings
  bug, the 101.33 ms pulse above 6 kHz, the CW decoder and its inherited reds.
