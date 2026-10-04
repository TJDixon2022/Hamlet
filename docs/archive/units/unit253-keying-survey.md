# Unit 253, task 3 — what already exists for keying

**PROJECT: Hamlet.** Reading only. No file under `src/` was changed by this task.
Line numbers are as of version 1.12.78.

---

## 1. Every place the engine can key or unkey a transmitter today

| What | Where | Keys? |
|---|---|---|
| `IRig.SendCwAsync(string)` → CI-V `0x17` + ASCII text | `Rig/Ic7300Rig.cs:409-434` | **Yes** — the radio keys itself and sends the message, provided break-in or an external TX switch is on (p. 19-8 fn. 2) |
| `IRig.AbortCw()` → CI-V `0x17` + `0xFF` | `Rig/Ic7300Rig.cs:447-463` | **Unkeys.** Stops a keyer message in progress |
| `CivWrites.AntennaTuner` (`0x1C 01`), value `0x02` = `CivWrites.TuneNow` | `Civ/CivWrites.cs:305-311` | **Yes** — a tuning cycle transmits. Tier `RigWriteTier.Keys` |
| `CivWrites.BreakIn` (`0x16 47`) | `Civ/CivWrites.cs:281-283` | No, but it is the precondition that lets `0x17` reach the air. Tier `Transmitted` |
| `CivReads.TransmitStatus` (`0x1C 00`), p. 19-7 | `Civ/CivReads.cs:176-178` | **Read only.** `00=receiving, 01=transmitting` |
| `TrainingRig.AbortCw()` | `Rig/TrainingRig.cs:165` | Nothing physical — the training radio makes its own Morse |

**`0x1C 00` exists in this engine only as a read.** There is no write of it
anywhere in `src/` — no PTT on, and, more to the point of step 1, **no PTT off**.
`grep -rni "ptt" src/Hamlet.RadioEngine` returns four hits and every one of them
is `AutoCallStop.PttPressed`, which is a *detection* of the operator's hand on
the PTT, not a command.

**CI-V `0x1C 00` is not sub-commanded for tuner use.** The tuner is `0x1C 01`
and the read row for transmit status is `0x1C 00`, both cited to Full Manual
p. 19-7. They are different sub-commands on one page in one table.

## 2. What the CW side already does about aborting

The chain is `AutoCall`/`CwTransmitter` → `ICwSender` → `KeyerCwSender` →
`IRig.AbortCw()` → `ISerialPort.Write`.

- `ICwSender.Abort()` — `Cw/ICwSender.cs:106`. Returns `void`; no token, no task.
- `KeyerCwSender.Abort()` — `Cw/KeyerCwSender.cs:126-130`. Sets `_aborting`
  **first**, then calls `_rig.AbortCw()`, "so that a send loop which happens to be
  between pieces stops even if the radio never hears the stop frame. Neither half
  depends on the other noticing anything (§0.2)."
- `CwTransmitter.Abort()` — `Cw/CwTransmitter.cs:155`, a straight forward.
- `AutoCall` calls `_sender.Abort()` on every exit — `Cw/AutoCall.cs:369` and
  `:752`, documented at `:262` as **EVERY EXIT SENDS THE STOP CODE**.
- `Ic7300Rig.AbortCw()` — `Rig/Ic7300Rig.cs:447`. **Synchronous, no `await`,
  bypasses `_commandGate` deliberately** ("a stop queued behind the send it is
  stopping would arrive after the message finished, which is not a stop at all"),
  and swallows every exception ("An abort that could fail is not an abort").
- `ISerialPort.Write(ReadOnlySpan<byte>)` — `Transport/ISerialPort.cs:54`, whose
  doc comment says **THE ABORT PATH AND NOTHING ELSE**.
  `SystemSerialPort.Write` at `:59-60` goes at `SerialPort.Write` rather than at
  `BaseStream`, because that one is genuinely synchronous.

**So the same-thread, no-await, never-throws shape already exists and is already
proven in the transport seam.** What it does not have is the second half.

## 3. The transport when things are wrong

| Condition | What happens today |
|---|---|
| Port closed / disposed | `SystemSerialPort.Write` calls into `System.IO.Ports.SerialPort.Write`, which throws `InvalidOperationException`. `AbortCw` catches `Exception` and returns. **Nothing else is attempted.** |
| Port gone (USB pulled) | Same route — an `IOException` out of the driver, caught and swallowed at `Ic7300Rig.cs:458` |
| Radio does not answer | Irrelevant to the abort by design: `AbortCw` writes and does not read, so there is nothing to time out. This is correct and is kept |
| The write itself throws | Caught at `Ic7300Rig.cs:458-462`, comment: "Nothing here is worth taking the app down for, least of all on the path somebody reaches for when something has gone wrong" |
| An ordinary write | `Ic7300Rig.RequestAsync` — behind `_commandGate`, with a timeout, returning `RigWriteResult.NoAnswer` on `TimeoutException` and marking the mode unknown (`ReportModeUnknown`, `:478`) |

**The gap: a swallowed exception on the one write means the abort did nothing and
told nobody.** There is no second attempt and no returned record of what
happened. `AbortCw` returns `void`.

## 4. The licence gate's shape, and where a send path meets it

`Licensing/TransmitGuard.Check(licenseClass, frequencyHz, mode, guardEnabled)` —
`Licensing/TransmitGuard.cs:67-93`. It is reached from
`Cw/CwTransmitter.Check(TransmitContext)` at `:93-117`, which is separate from
sending on purpose so the UI can show the answer beside the button; the App calls
it at `ViewModels/CwTransmitViewModel.cs:928`.

An FT8 send path would have to pass through the same two calls, with
`TransmitMode.Ft8` in place of `TransmitMode.Cw`, and through
`TransmitReadiness.Check` (`Cw/TransmitReadiness.cs`), which folds the privilege
answer in as a readiness precondition rather than a separate verdict
(HM-DEC-089).

**Two shapes worth knowing before step 5 designs a send path:**

1. **An unknown licence class does not block transmitting** (`:77-85`). It warns,
   names what it does not know, and gets out of the way. That is deliberate and
   documented.
2. **`guardEnabled == false` returns `MayTransmit: true` with
   `WasOverridden: true`** (`:87-90`). The Settings toggle *Only let me transmit
   where my license allows* is a real override, and it is On by default. **This
   does not match the sentence in the new §0.2** that the Settings check "is the
   gate and it is not bypassable from any send path". Raised in unit 253's report.

## 5. What is missing for step 1's abort

Not nothing, and not much.

1. **The PTT-off fallback does not exist.** `0x1C 00 00` is written nowhere.
2. **Nothing attempts two writes independently.** `AbortCw` is one write in one
   `try`; if it throws, the abort is over.
3. **The abort is CW-shaped.** `IRig.AbortCw` is named and documented for a keyer
   message. An FT8 transmission is keyed by PTT and audio, and `0x17 0xFF` means
   nothing to it — which is exactly why the fallback is the half that matters for
   this phase.
4. **Nothing asserts the no-`await` property.** It is true of `AbortCw` today and
   is held by a comment. A test that fails if an `await` were added does not exist.
5. **Nothing asserts it cannot be made conditional.** No flag turns it off today,
   and nothing stops one being added tomorrow.
6. **The abort reports nothing.** §0.0.1 wants every CI-V frame recordable
   verbatim; a `void` that swallowed an exception cannot supply one.
