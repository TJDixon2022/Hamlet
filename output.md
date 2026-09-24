READ IN THIS ORDER.

A. The banner's old sentence and its new one, quoted. Old, as Tim photographed it: "I asked for the RF gain to be 100% and the radio did not confirm it, so I do not know where it is now." New, when a read-back is held: "I asked for the RF gain to be 100%, and the radio read it back as 100% at 21:04:07." When no read-back is held, the old sentence stands word for word.
B. Step 6's criteria: 6.1 the banner - met and ticked. 6.2 the sidecar's three sentences - keying and elementHz met, tonePeak dropped as the named candidate, so 6.2 stays open. 6.6 the exit round - met and ticked. 6.3, 6.4 and 6.5 not started.
C. The rest. Section 4 raises 2 items. Both bear on B: the first is why the new banner sentence will appear on every CW tune-in, and the second is what 6.2 needs before it can close.

UNIT:       411 - complete at task 4 of 4, tonePeak dropped within task 2 - 2026-09-23 21:25
PHASE GOAL: Hamlet reads a CQ call on the air correctly, measured as edits against a key over a scored region, with step 6 making what the screen states about the radio and the signal true while the CW work goes on
UNIT GOAL:  The RF gain banner states the read-back Hamlet holds, and the sidecar's keying, elementHz and tonePeak lines stop contradicting the lines beside them
ADVANCED:   yes - 6.1, the criterion this unit was aimed at, is met: watched red on a held read-back, then green, both sentences quoted above
NUMBER:     sentences that contradicted the tree: 4 -> 1, tonePeak remains
DRIFT:      0

## 1. What Claude did

**Complete at task 4 of 4.** Tasks 0 to 4 were all done. **tonePeak was dropped whole within task 2.** It was the named drop candidate, and it was dropped for a reason besides the clock (section 4, item 2). Task 3, also a named candidate, was **not** dropped. Machine QUIVERFULL, project Hamlet, gate confirmed at `C:\Source\HamLet`, branch `main`. HEAD at entry was `bcebdfea`. The four task commits are `ed3c7a17`, `c62bf57a`, `ae36cc30` and `642ccdfa`. The report commit follows them.

**Verifying the instruction against the tree (§5). Mismatches are reported here and none was repaired.**
- **The banner is not in the app.** Its sentence is composed in `src/Hamlet.RadioEngine/Rig/ReceiverSetupVoice.cs`, in both `Say` and `Admissions`. The app reaches it through `MainWindowViewModel.EstablishReceiveConditionsAsync`, which calls `Narrate(Say, Admissions)`.
- **The read-back is held in `Ic7300Rig.SetSettingAsync`, and the banner's code could not reach it.** After the write, the rig reads `14 02` and raises `ValuesReported`, which is what the radio-state dialog shows. It then compares the reading with the value written and returns only `ReadBackDisagreed`, so the reading is dropped.
- **Why they disagree:** the CW row in `data/bands/mode-receiver-conditions.json` wants `rfGain` at **255**, on the radio's scale. `CivDecode.DecodePercent` reads the gain back as a percent, **100**. The two values never compare equal. So every CW tune-in writes RF gain, and every one is filed as not confirmed, including when the radio was already at 100%.
- **`cw-2026-09-23-173723.txt` is not in the tree.** Only the `.wav` and `.key.md` are. The contradictions were reproduced on the saved `.wav` instead.
- **tonePeak** is `report.SnrDb`: a held peak that decays across captures (HM-DEC-090 and HM-DEC-091, per the writer's own comment). I did not trace it further than that.
- **The keying verdict does come from `CwKeyingMeter`'s four-part test:** score, element median at least 25 ms, element median at most 250 ms, and swing. On 17:37, windowed the way the live meter runs, **only the swing fails: 16.3 dB against 20.** The score was 0.108 against 0.10, and the median was 71 ms.
- `PHASE_STATUS.md` read step 0 done and `CURRENT_STEP 1`, and the plan carries step 6, as stated. The step-6 lines in `PHASE_STATUS.md`, `PHASE_OUTCOME.md` and `PHASE_PLAN.md` were uncommitted when the session started. They were committed with task 0.
- **A standards note (CLAUDE_CODE.md §0).** Plan §6 names "a fact the product states to the operator about a signal" as one of the three stops. This unit changes exactly such sentences, under R62 and HM-DEC-170, which commission it. I read the stop as governing un-commissioned changes, and I continued.

**Task 0, the record.** HM-DEC-170 is in `DECISIONS.md` and at the top of `CLAUDE.md` §1. `PHASE_STATUS.md` names unit 411 with `CURRENT_STEP: 6`. `PHASE_OUTCOME.md` has its `## UNIT 411 - STEP 6` entry. The version went from 1.13.97 to 1.13.98. Entry round:

| Run | Result | Time |
|---|---|---|
| Engine carry-forward | 178 of 178 | 385 s |
| App carry-forward | 278 of 278 | 169 s |
| Captures | 37 of 37 | 94 s |
| Adjudicated | 13 of 13 | 29 s |
| Clean synthetics | 2 of 2 | 1 s |

**Task 1, the banner (6.1).** First the trace, a theory that asserts nothing. It printed that the condition wants 255, shown as 100%, and that the read before the write was 42% or 100%, from `CI-V 14 02`. One write of 255 was sent, and the read-back afterwards was `100%` from `CI-V 14 02`. The result was `NotConfirmed` with `now=-`, and the banner said it did not know.

The changes:
- `RigWriteResult` gained a `ReadBack` property.
- `Ic7300Rig` attaches the reading it just took to `ReadBackDisagreed`. **No extra byte is sent.**
- `ReceiverSetup` passes the reading's text and time into `ConditionResult`, which gains `NowAtUtc`.
- `ReceiverSetupVoice` states the value and its time. With no read-back held, the old sentence is unchanged.

`TheBannerSaysWhatTheRadioReadBackTests` failed 2 of 5 before the change and passed 5 of 5 after. It drives the real `Ic7300Rig` against `ScriptedRadio`, which now answers `14` levels only where a test puts one, so no older test sees a difference. Seven neighboring types are green.

**Decision made for myself:** the sentence gives the **clock time, not an age**. The banner stays on the bar after it is written, and "a moment ago" would stop being true while he read it. It uses local time, in the same form as the app's other `HH:mm:ss` times.

**Task 2, the sidecar (6.2, partial).** `TheSidecarDoesNotContradictItselfTests` runs saved audio through six-second windows, one a second, the way the live meter runs. It failed 3 of 3 before the change and passed 3 of 3 after. Before the change:
- 17:37: `no keying at 575 Hz, 71 ms key down, 16 dB swing, 109 key-downs`
- 014113: `no keying at 650 Hz, 48 ms key down, 14 dB swing, 211 key-downs`

**The keying decision:** the meter's verdict is untouched, and every number stays. On a no-keying verdict, the counts are called rises above the threshold rather than key-downs, and the line names which of the meter's tests failed against the meter's own bar. The keying and held wordings are unchanged.

**The elementHz decision:** all three branches now say "each element's own pitch not measured", because the elements themselves were measured, by their timing. Three sheet neighbors are green, including `TheCaptureButtonTests`.

**tonePeak was dropped whole** (section 4, item 2).

**Task 3, the re-read.** The sheet's three lines are now composed in `TonePeakRecordLine`, `KeyingRecordLine` and `ElementHzRecordLine`. The writer calls these, and the move did not change the text. `TheSidecarIsReReadTests` (2 of 2) runs a fresh decoder and a fresh meter over the saved `.wav` and writes the lines to `.run-unit/unit411-sidecar-cw-2026-09-23-173723.txt` and `...-014113.txt`. **The rest of the sheet is not regenerated.** It is composed inside the view model from a live session. No capture in the tree was edited. While checking the regenerated text I found an overclaim in my own new caption ("not this file") and removed it.

**Task 4, the exit round (6.6).**
- `Hamlet.sln` builds with warnings as errors.
- Engine carry-forward: **178 of 178** in 373 s.
- App carry-forward: **277 of 278** in 162 s. `ThePsk31OfferTests.TheOfferIsOneButtonAndItIsTheOneTheEngineNamed` was lost to the dispatcher loop before any assertion. Re-run once alone, it passed 1 of 1, so it is counted neither way.
- Floors: captures 37 of 37 in 92 s, adjudicated 13 of 13 in 29 s, synthetics 2 of 2.
- Every type touched is green, with one exception. **`HowMuchTheApplicationSaysTests.AddingASentenceToACappedSurfaceTurnsItRed` went red once, on an assertion.** It expected 1779 and got 1844: 65 more characters on the Digital tab than 1379 plus 400. Re-run alone, it was green at 1779. It was also green at task 1 at 1779. `OPEN_ISSUES.md` already records that the Digital tab's figure "moves between runs of the same code". Nothing this unit changed composes Digital-tab text, and the test is not a carry-forward name. It is reported here rather than counted green.

**The diffs.** The transmit files against `7e209cb4` print nothing. `src/Hamlet.RadioEngine/Cw` against `bcebdfea` prints nothing, so **nothing that decides a character changed.** `src` changed in five files: `MainWindowViewModel.cs`, `Ic7300Rig.cs`, `ReceiverSetup.cs`, `ReceiverSetupVoice.cs` and `RigWriteResult.cs`.

`Ic7300Rig.cs` is not one of the named transmit files, but it holds the keying path. Its change is confined to the `return` of `SetSettingAsync` and sends nothing new:

```
-                : RigWriteResult.ReadBackDisagreed(write.Label);
+                : RigWriteResult.ReadBackDisagreed(write.Label) with
+                {
+                    ReadBack = values.FirstOrDefault(
+                        v => v.Field == write.Field && v.IsKnown),
+                };
```

**Also seen, not touched:** 15 new captures `cw-2026-09-24-003901` to `004550`, plus `cases-2026-09-23.txt`, appeared untracked in `tests/fixtures/cw/captured/unadjudicated/`. They were written between 20:39 and 20:45 local, before any test in this session ran, so they are the running app's. I did not commit or edit them.

## 2. What the owner should expect

Where the RF gain banner used to say it did not know, you will now read: *"I asked for the RF gain to be 100%, and the radio read it back as 100% at 21:04:07."* That is the reading the radio-state dialog shows, with the clock time it was taken. If the radio does not answer the read-back at all, you still get the old sentence, because then it is true.

**What will look wrong but is not:** this sentence will appear on **every CW tune-in**, even when the gain was already at 100%. The cause is the 255-against-100 comparison. Fixing that changes what is sent to your radio, so it is section 4's question and not this unit's.

The capture sidecar's `keying` line no longer says "no keying" and "98 key-downs" together. It now reads, for example, *"no keying at 575 Hz: 109 rises above the threshold, median 71 ms, 16 dB swing; not called keying on a 16 dB swing where it needs 20"*. `elementHz` says it was each element's own pitch that was not measured.

**What has not changed:** the radio and every byte sent to it, the decoder and every character it reads, the keying meter's verdict, and the text on the CW tab. `tonePeak` still prints as before.

## 3. What you should see

**The banner now tells you the RF gain the radio reported, and when.** At 100% it reads *"I asked for the RF gain to be 100%, and the radio read it back as 100% at <time>"*, the same figure the radio-state dialog shows, instead of saying Hamlet does not know. Criterion 6.1 was watched red on that exact case and is now green.

On the next capture you keep, the sidecar's lines will read like this, regenerated from your 17:37 recording:

Before, from your sidecar and from the unchanged code on the same audio:
```
tonePeak   25.8  (the highest the tracked tone ever stood above the noise beside it, held and decaying; not a figure about this recording)
keying     no keying at 575 Hz, 69 ms key down, 16 dB swing, 98 key-downs  (an independent sweep ...)
elementHz  not measured  (the decoder in this build does not say where each element began and ended, so no element's own pitch was measured)
```

After, regenerated by task 3 with a fresh decoder and meter over the 17:37 file only:
```
tonePeak   25.7  (the highest the tracked tone ever stood above the noise beside it, held and decaying; not a figure about this recording)
elements   111 seen, 111 resolved  (this file only, a fresh decoder)
keying     no keying at 575 Hz: 109 rises above the threshold, median 71 ms, 16 dB swing; not called keying on a 16 dB swing where it needs 20  (an independent sweep of 400 to 1200 Hz in 25 Hz steps over the last six seconds, sharing nothing with the decoder)
elementHz  each element's own pitch not measured  (the elements counted above were measured by their timing, but the decoder in this build does not say where each one began and ended, so none of them had its own pitch taken)
```

The keying line now shows **why** 17:37 was called no keying: its swing alone, 16 dB against a bar of 20. That is the figure unit 409 found runs low. `tonePeak` is unchanged, and it is the one sentence of the four still open.

## 4. What's blocking us

**1. Compare the RF gain on one scale, so an RF gain already at full is neither written nor reported as unconfirmed.**
- **Ruling wanted:** in a later unit, make `ReceiverSetup` and `Ic7300Rig.SetSettingAsync` compare the RF gain on one scale, either the condition in percent or the read-back in raw units.
- **Reasoning:** today every CW tune-in writes 255 to `14 02`, even at 100%. Every one is filed `NotConfirmed`, so the memory never records it, and "your hand wins" (HM-DEC-056) can never apply to RF gain. With this unit, the banner's sentence is true but appears on every tune-in. After the fix it would appear only when something is actually off.
- **Rejected, and why:** fixing it here. It changes what is sent to the radio, and step 6 changes only what the operator reads (R62, §6). The fix would also move the outcome to `AlreadyRight` or `Changed` and change what the hover narrates.

**2. What tonePeak should be in a per-capture sidecar. 6.2 cannot close without this.**
- **Ruling wanted:** choose one of three.
  - (a) Print a figure measured over this recording only, and label it that way. A fresh decoder over 17:37 gave 25.7 where the live sheet said 25.8, so the number is close, but it is not the same measurement.
  - (b) Stop printing it in the per-capture sidecar. Keep it on the roster's `tonePeakDb` column and on screen.
  - (c) Keep it as it is. Its caption already says it is not about this recording.
- **Recommendation:** (a), because it keeps a figure and makes it true.
- **Reasoning:** the writer's own comment records HM-DEC-091: *"The number is not deleted and not changed, because it measures something real and something else was built on it."* Both options the instruction offered, a per-recording figure or not printing it, change or remove that number. So this is a choice between the instruction and a recorded ruling, and it is Tim's to make, not the author's.
- **Rejected, and why:** picking one silently. Either choice would override HM-DEC-091 without a ruling. The cost of running a decoder at the press has also not been measured.
