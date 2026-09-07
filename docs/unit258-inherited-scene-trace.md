# Unit 258, task 1 - what the killed unit left, and does it stand up

**This answers only what `docs/unit257-contact-state-survey.md` could not, because
that survey was written before the fixture existed. The survey is not re-derived
and is not rewritten.**

Read off the tree at `HEAD a68062e` on 2026-09-06, after unit 257's four
uncommitted files went in exactly as they arrived.

---

## 1. Does the inherited test project compile?

**Yes, first time, with nothing fixed.**

```
dotnet build tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj
Build succeeded.
    0 Warning(s)
    0 Error(s)
Time Elapsed 00:00:06.22
```

Started 22:29:47, finished 22:29:53. **Compile errors were expected in the three
files that had never been through a compiler and there were none, so the list of
errors fixed is empty.** All three - `Ft8BandScene.cs`, `Ft8SceneCorpus.cs` and
`TheBandSceneIsWhatHamletsDecoderReadTests.cs` - are byte for byte as unit 257
left them.

---

## 2. Is the committed corpus what the decoder returned?

**Yes. `CORPUS ROUTE: audio`, on the first attempt of the three the instruction
allowed.**

```
dotnet test tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj
  --filter "FullyQualifiedName~TheBandSceneIsWhatHamletsDecoderReadTests"

Passed!  - Failed: 0, Passed: 2, Skipped: 0, Total: 2, Duration: 4 s
```

Started 22:30:05, finished 22:30:16. **Both tests in the class passed** - the one
that regenerates the scene and matches the committed file, and
`EveryTransmissionOfTheThreeAtOnceStationIsInTheCorpus`, which checks by name that
`G4XYZ` lost nothing.

The generator's own printed counts:

```
slots                : 12
signals composed     : 21
decodes returned     : 21
composed but not read: 0
busiest slot         : 3 signals (cap 8)
wall clock           : 3.91 s
```

**All 21 composed signals decoded. None were lost.** 3.91 s of wall clock for
twelve slots, against unit 257's prediction of about 7 s for twelve slots through
Deep - comfortably inside it, and nowhere near the minutes that would have meant
something was wrong.

**Why this settles the provenance rather than merely agreeing with it.** The test
does not read the corpus and compare it to the script. It composes the audio,
sums each slot, decodes it, writes what came back through `Ft8SceneCorpus.Write`,
and asserts string equality against the committed file. A corpus written from the
script would fail at the first slot whose decodes came back in a different order
from the script's listing - and slots 4, 6, 8 and 10 are exactly that. In slot 4
the script lists `K9RST`, `G4XYZ`, free text; the committed file holds `G4XYZ`,
`K9RST`, free text. **Nobody typed that order.**

### One mismatch with the instruction, reported and not repaired

**The instruction says 22 signals and 22 decode lines. The tree says 21 of each.**
`Ft8BandScene.Signals` holds 21 entries and `unit257-band-scene.corpus.txt` holds
21 decode lines under 8 lines of header, for the 29 lines the instruction counted
correctly. **The tree wins.** Nothing was changed to reconcile it; every figure in
this unit is 21.

---

## 3. Which decoder did the inherited test use?

**`Ft8DeepSlotDecoder` with both stages on - the one the application runs.**

| Where | Construction |
|---|---|
| The application | `src/Hamlet.RadioEngine/Audio/Ft8Reception.cs:460-462` |
| The scene generator | `tests/Hamlet.RadioEngine.Tests/Contacts/TheBandSceneIsWhatHamletsDecoderReadTests.cs:167-169` |

Both read:

```csharp
new Ft8DeepSlotDecoder(
    osd: Ft8DeepOsdSettings.Default,
    fineSync: Ft8DeepFineSyncSettings.Default);
```

Ordered statistics and fine sync both on, both defaulted, no third argument on
either side. **They are the same decoder built the same way**, so the corpus is
evidence about the decoder Hamlet actually runs and not about one nobody uses.

---

## 4. The state each station should read - WRITTEN DOWN BEFORE THE LEDGER EXISTS

**Read off `Ft8BandScene.cs` by hand. This is the list `NUMBER:` is scored
against, and it is committed before a line of the ledger is written so that the
score cannot be written after the fact.**

**Five stations are booked.** `DL1QQ` and `JA1ZZ` are addressees of `G4XYZ`'s
traffic and never transmit, so they must not appear at all - which is itself
predicted here and asserted.

The moment of evaluation is **the boundary of slot 13**, and the second column is
the boundary of slot 11, the scene's last.

| Station | At slot 11 (scene's end) | At slot 13 (evaluated) | Why, from the script alone |
|---|---|---|---|
| `W1ABC` | complete | **complete** | Slots 6-9. His CQ, our `-12`, his `R-15`, our `RRR`. Both calls, both reports, both acknowledgements, **and no `73` anywhere in it**. |
| `K9RST` | complete | **complete** | Complete at slot 5 - his `EM12`, our `-13`, his `R-09`, our `RRR`. His `73` at slot 6 arrives after complete and changes nothing. |
| `G4XYZ` | waiting on him | **waiting on him** | We came back to him at slot 11 and he has not answered. He last transmitted at slot 10, three slots before the evaluation, so he is **never gone quiet** - he is working `JA1ZZ` and `DL1QQ` in slots 0, 4, 6 and 8 and calling CQ at 10. |
| `N5TT` | your move | **gone quiet** | He answered our CQ at slot 8 and we never came back. At slot 11 that is 3 slots and the ball is ours; at slot 13 it is 5 slots, which crosses the threshold. |
| `VK2PQ` | gone quiet | **gone quiet** | He answered our CQ at slot 2 and was never heard again - 9 slots at the scene's end, 11 at the evaluation. |
| `DL1QQ`, `JA1ZZ` | not booked | **not booked** | Addressed by `G4XYZ`, never senders, never addressed by us. |

**And two messages the splitter refuses**, `TNX BOB 73 GL` at slot 4 and
`ABCDEFGHIJKLM` at slot 10. Neither books a station and neither may crash
anything.

**The threshold and the moment are chosen in task 4 and argued there.** They are
written into this table in advance so that the prediction is a prediction.
