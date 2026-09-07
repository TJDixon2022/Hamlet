# Unit 267, task 5 - what step C will need from this

**Named, not built.** Work instruction 267 task 5: *"Do not build the chain. Do not
write the test. Name the pieces and stop."* Nothing here was run and nothing here
was written into `tests/`. Every file and line was read off the tree on 2026-09-07.

`PHASE_PLAN.md` step C, *the whole chain runs from one click, at the bench*, asks
for four things:

1. **One right-click drives the whole chain** - menu, compose, key, play, unkey,
   telemetry - **exercised in one test**, with the transmit endpoint on a loopback
   and CI-V on a fake transport.
2. **The audio that reaches the endpoint decodes back to the message the operator
   clicked.**
3. **The abort fires from the middle of that chain** and the sound stops.
4. **Nothing transmits that the operator did not click**, asserted across the whole
   chain rather than at the menu alone.

---

## Criterion 1 - one right-click drives the whole chain

**Closest:**
`tests/Hamlet.App.Tests/ViewModels/TheLoopbackThroughTheApplicationsSendPathTests.cs:65`,
`AMessageTheApplicationSentLeavesThisMachineAndComesBack`.

It already has more of this than anything else in the tree: the application's own
`BuildTheArmedSend`, a **real `WasapiTransmitSink` built through the panel's own
factory** (`:93-98` - the one test in the project that lets one be constructed),
`FakePort` for CI-V, `panel.AtSlotBoundaryAsync`, and the ordinary unkey asserted
at `:189` (`Assert.Equal(UnkeyRoute.OrdinaryUnkey, run.CameOutOfTransmit)`).

**What it lacks, and it is exactly two things:**

- **The right-click.** The click is `panel.SendMessageCommand.Execute(Message)` at
  `:150` with a `const string Message = "CQ KC3QIS FN00"` at `:82` - **a literal a
  test chose**, not an option a menu offered. Step C's word is *right-click*, and
  the difference is the whole point: a menu that offers the wrong text would pass
  this test unchanged.
- **Telemetry.** Nothing in it reads the telemetry file at all.

**The two pieces that carry the missing halves, and both exist:**

- **The menu half** -
  `tests/Hamlet.App.Tests/Views/TheMenuIsUnderTheMouseTests.cs:114`,
  `EveryStationsPredictedMenuAppearsUnderTheMouse`, raises a real
  `ContextRequested` on a real row control in a real window and reads the flyout
  the markup's own handler built. Its `RightClick` helper and the `[AvaloniaFact]`
  scene are what step C needs. **What it lacks: everything downstream** - fake sink,
  no boundary, nothing keyed.
- **The telemetry half** -
  `tests/Hamlet.App.Tests/ViewModels/TheWholeContactWalksThroughTheApplicationTests.cs:256`,
  `OneSendLeavesOneLineOnDiskAndTheLineNamesNobody`, drives clicks through
  `SendMenuFor` and `SendMessageCommand` and reads the `ft8_transmission` line off
  disk through the application's own `AppSettings` enabled-category predicate.
  **What it lacks: a real endpoint and a decode** - it runs on the fake sink.

---

## Criterion 2 - the audio decodes back to the message the operator clicked

**Closest: the same loopback test**, `:214-233`, and this criterion is **already
met except for where the message came from**:

```csharp
var onTheGrid = Ft8Resample.ToFt8Rate(new MonoAudio(captured.SampleRate, window));
var texts = new Ft8SlotDecoder().Decode(onTheGrid.Samples).Texts;
...
Assert.True(texts.Contains(sent.ReadsBackAs, StringComparer.Ordinal), ...);
```

**What it lacks:** `sent.ReadsBackAs` is a round trip of the literal at `:82`. Step
C wants it compared against **the text on the menu item that was clicked**, which
is `Ft8SendOption.Text` off `vm.SendMenuFor(row)`. That is a one-line change of
*where the expected string comes from* and it is the change that makes the
criterion mean anything.

**Nothing else in the tree decodes application-sent audio back.** The engine's
`tests/Hamlet.RadioEngine.Tests/Audio/TheLoopbackProvesTheWholeChainTests.cs`
composes inside its own test and never touches a caller.

---

## Criterion 3 - the abort fires from the middle of the chain

**There are two closest tests and neither is close enough, because each has the
half the other is missing.**

- `tests/Hamlet.App.Tests/Views/TheOperatorCanStopItTests.cs:241`,
  `AClickWhileTheToneIsPlayingStopsTheSoundAndNotJustTheCarrier`. **Has the
  operator**: an `[AvaloniaFact]`, the real Stop button in the real window,
  pressed while a transmission is in flight. **Lacks the endpoint** - the sink is a
  fake that releases on command, so *the sound stops* is asserted against a fake's
  own bookkeeping and not against a card going quiet.
- `tests/Hamlet.RadioEngine.Tests/Audio/TheStopStopsARealEndpointTests.cs:88`,
  `AStopAThirdOfTheWayInStopsARealCardAndSaysHowMuchWentOut`. **Has the endpoint**:
  a real render device, measured going quiet. **Lacks the operator** - it is
  engine-level, with no panel, no window and no menu; it calls `StopNow` itself.
- `tests/Hamlet.RadioEngine.Tests/Transmit/TheStopStopsTheAudioTooTests.cs:93`,
  `MidTransmissionTheStopTakesTheCarrierOffAndStopsTheSound`, is the same seam one
  level lower again, with the `0x17 FF` and `1C 00 00` frames quoted.

---

## Criterion 4 - nothing transmits that the operator did not click

**Closest, and it is three tests rather than one:**

- `tests/Hamlet.App.Tests/ViewModels/TheSendPathReachesARealRadioTests.cs:220`,
  `OneClickIsOneMessageAcrossTwoBoundaries` - two boundaries, one transmission,
  through the panel. **Lacks:** it is the fake sink, and it asserts at the boundary
  rather than across the chain.
- `tests/Hamlet.RadioEngine.Tests/Transmit/OneClickSendsExactlyOneMessageTests.cs:275`,
  `NothingInTheArmedSendCanStartATransmissionOnItsOwn` - the negative, engine-side.
- `tests/Hamlet.RadioEngine.Tests/Transmit/TheUnkeyHappensWhateverGoesWrongTests.cs:423`,
  `ExactlyOneFileInTheShippedTreeCallsTheSequence` - the static tripwire: exactly
  one file in `src/` calls `_sequence.RunAsync`, so there is no second route to a
  keying frame. **Lacks:** it is a grep over the tree and not a run, which is
  precisely why it is worth keeping beside a run.

**What step C adds to these:** the assertion has to be made **with a real endpoint
open and a loopback capture running**, so that *nothing transmitted* is a silent
card rather than an untouched counter.

---

## The harness pieces that would have to be joined

| Piece | Where it is today | What joining costs |
|---|---|---|
| **The loopback endpoint** | `TheLoopbackThroughTheApplicationsSendPathTests.Preferred(out var why)`, and the `WasapiLoopbackCapture` + `MMDeviceEnumerator` block at `:117-141` | It already refuses gracefully where the machine has no render endpoint (`:69-78`), and that behaviour must survive: **a machine with no card is a normal machine, not a red** |
| **The fake port** | `tests/Hamlet.App.Tests/FakeTransmitParts.cs:17`, `FakePort` | Nothing. It is already what the loopback test uses for CI-V |
| **The decoder** | `WasapiAudioSource.Downmix` (`:134`), `AudioTap`, `Ft8Resample.ToFt8Rate`, `Ft8SlotDecoder().Decode` (`:214-215`) | Nothing, except that the expected string must come from the menu option rather than from a literal |
| **The stop** | `MainWindowViewModel.StopSendingCommand`, the `DigitalStopButton` at `MainWindow.axaml:3119`, and `Ft8ArmedSend.StopNow` | This is the expensive one - see below |
| **The right-click** | `MainWindow.axaml.cs:171` `SendFlyoutFor`, driven as `TheMenuIsUnderTheMouseTests` drives it, and `MenuItem.Command` carrying `SendMessageCommand` with `option.Text` | The whole test becomes an `[AvaloniaFact]` in a headless window |
| **Telemetry** | The `JsonlTelemetry` + `AppSettings.IsTelemetryEnabled` route asserted in `TheWholeContactWalksThroughTheApplicationTests:256` | Little. It reads a file after the run |

---

## Which of the four is the hardest, and why

**Criterion 3, the abort from the middle of the chain, and it is not close.**

Three reasons, in order of how much they cost:

1. **It is the only criterion whose two halves live in different assemblies with
   different test hosts.** The operator's press is proved in an `[AvaloniaFact]`
   against a fake sink; the card actually going quiet is proved in a plain `[Fact]`
   in the engine's project against a real endpoint. Step C's test must be **an
   Avalonia headless window that also opens a WASAPI render endpoint and a loopback
   capture** - nothing in the tree does both, and the headless dispatcher is the
   same thread the Send-area line is posted to.
2. **The stop has a real-time bound and the loopback has real-time latency.**
   Unit 263 measured `StopNow` returning in 0.5-1.3 ms against a stated 250 ms
   bound, and the card going quiet 15-20 ms after it, with a 200 ms endpoint
   buffer. Those are honest measurements of a machine, and asserting them from
   inside a test that is also pumping an Avalonia dispatcher and a capture callback
   makes the margin the thing under test.
3. **A stopped transmission does not decode, so criteria 2 and 3 cannot be the same
   transmission.** Criterion 2 needs a whole 12.64-second slot to reach the
   decoder; criterion 3 truncates one deliberately. Step C says *exercised in one
   test*, so **the one test needs two runs in it** - one clicked and left alone to
   be decoded, one clicked and stopped a third of the way in - and that has to be
   noticed before the test is written rather than after it goes red for looking
   like a decode failure.

**The easiest is criterion 2**, which is met today except for where the expected
string comes from. **Criterion 1 is mostly assembly** - the three pieces all exist
and none of them has to be invented. **Criterion 4 is a re-assertion** of three
things already proved, in the presence of a real endpoint.

**One thing step C should be told before it starts:** the loopback test makes real
sound on a real card and takes about fifteen seconds of wall clock for one slot.
Two runs in one test is thirty-odd seconds plus pre-roll and post-roll, which is
fine for one filtered test and is worth stating in the instruction so the timeout
is chosen rather than discovered.
