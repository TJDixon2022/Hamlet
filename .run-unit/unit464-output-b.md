
**3. MET-CAL, ours** (`CW_SPEC.md` section 11): three bins, over every stretch scored.

| class | set | key | in stretches | right | wrong | added | observed accuracy | stated rate |
|---|---|---|---|---|---|---|---|---|
| sure | real | inferred | 436 | 403 | 28 | 5 | 0.924 | asserted as sent (HM-REQ-013) |
| sure | synthetic | exact | 173 | 159 | 8 | 6 | 0.919 | asserted as sent (HM-REQ-013) |
| dim | real | inferred | 0 | 0 | 0 | 0 | no number: none emitted | at least 0.70 (HM-REQ-014) |
| dim | synthetic | exact | 0 | 0 | 0 | 0 | no number: none emitted | at least 0.70 (HM-REQ-014) |
| placeholder | real | inferred | 15 | - | - | - | unscored | unscored |
| placeholder | synthetic | exact | 2 | - | - | - | unscored | unscored |

**4. How `EveryCharacterCarriesAConfidenceTests` was watched failing first.** The test cannot compile
at the parent commit, where neither `Probability` nor `FldigiConfidence` exists. So case 1 was
watched red in the working tree first, with:
- the property added and defaulting to NaN;
- the stream not yet setting it;
- `FldigiConfidence`'s constants NaN.

It failed with "40 characters without a probability" on `cq-18wpm-15db`
(`.run-unit/unit464-confidence-red.txt`). Then the constants were written back and the stream line
added, and it passed 2 of 2 (`unit464-confidence-green.txt`). Case 2, the hand-built measure (100
at 0.9: 90 right calibrated, 80 right not, 29 not measurable), was written against the finished
`CwCalibration` and was not watched failing.

**5. The commits, with the five at each** (build; engine line; app line; named, captures and
adjudicated floors):

| commit | what | build | engine | app | named | captures | adjudicated |
|---|---|---|---|---|---|---|---|
| `b34f4f56` | task 0: record, runner's writes, entry | 0 errors | 178/178 | 278/278 (DECIDED (8): run and rerun each lost a different test; both types pass alone) | 13/13 | 51/51 | 13/13 |
| `12252dcc` | task 1: the printout and gate files | 0 errors | 178/178 | 278/278 (DECIDED (8): the rerun lost `BindingHealthTests`, which passes alone) | 13/13 | 51/51 | 13/13 |
| `d1abc6cb` | task 1: the fact itself, left out of `12252dcc` | as `12252dcc`, same tree | | | | | |
| `d3fb86fb` | task 2: the property, map, adapter, stream line, test, `CwCalibration` | 0 errors | 178/178 | 278/278 on the rerun | 13/13 | 51/51 | 13/13 |
| `efdd5d11` | task 3: the measure, `calibration.md`, `metrics.md` | 0 errors | task 2's gate: same code, only docs added | | | | |
| `b30bef48` | 9.5 ticked in both copies | plan text only | | | | | |
| task 4's commit | exit round, `output.md` | 0 errors | 178/178 | 278/278 on the rerun | 13/13 | 51/51 | 13/13 |

**The exit round beside the entry:**

| check | entry | exit |
|---|---|---|
| build | 0 errors | 0 errors |
| engine line | 178 of 178 | 178 of 178 |
| app line | 278 of 278 (DECIDED (8)) | 278 of 278 on the rerun (`TheTestsStayOffTheNetworkTests` lost to the dispatcher on the run) |
| floors | named 13/13, captures 51/51, adjudicated 13/13 | 13/13, 51/51, 13/13 |
| real, 23, inferred | CER-SURE 33 of 436, INVENTED 33 / 473, coverage 403 / 473, WBE 37 / 113 | the same |
| synthetic, 12, exact | 14 of 173, 14 / 252, 159 / 252, 44 / 84 | the same |
| the port, per parity.md | real 62 of 239, coverage 177; synthetic 34 of 168, coverage 134 | the same |
| `BothDecodersAreScoredAlikeTests` | 5 of 5 | 5 of 5 |
| the port's own tests | 8 of 8 | 8 of 8 |
| `EveryCharacterCarriesAConfidenceTests` | - | 2 of 2 |
| `TheSpeedFollowsTheSendersMarkPairsTests` | red at 28 of 31 | red at 28 of 31, not required green |
| decode time, ours over 690 s real | 51.93 s | 51.71 s |
| V-11, 35 rows | saved | every row identical |
| our texts (126 lines), classes (35), the port's texts (74) | saved | byte-identical |

Exit prints (`.run-unit/unit464-exit-print.txt`):
- `git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/` prints nothing.
- `git diff 7e209cb4` over the eleven transmit files prints nothing, and all eleven are present.
- `git status` shows `.run-unit/fldigi/` still untracked.
- src this unit is four files, 234 lines added: `CwCharacter.cs` (+16), `CwCharacterProbability.cs`
  (new), `CwProbabilisticStream.cs` (+1), `FldigiConfidence.cs` (new).
- The parity runs' rewrites of `parity.md` are kept under `.run-unit/unit464-parity-*.md`, and the
  committed file was restored each time.

## 4. What's blocking us

1. **R86 and 9.4, as a plain reading.** 9.4 asks for "one technique 9.3 named", and the five
   mechanisms 9.3 named are all refused under R78. That leaves 9.4 no authorable route. R86 holds
   steps 2 to 8 behind 9.4. Step 9's 9.5 to 9.7 carry the loop meanwhile; 9.5 is now ticked, and
   9.6 and 9.7 remain. Only the owner can change how R86 or 9.4 reads. This is logged for the
   owner and is not a stop: it touches neither transmit nor what the product promises the
   operator.
2. **R85 readings, one line each, recorded and acted on:**
   - "right" for a dim character is the scorer's alignment, key character equal to it, exactly as
     for sure;
   - a bin is floor(10p), the last closed at 1;
   - a bin counts at 10 characters and a condition at 30, as DECIDED (4);
   - fldigi's receiver can honestly be read for the level its own squelch metric reads
     (sig_avg over noise_floor, cw.cxx:635-636) and for how far each element fell from the
     dot/dash split it was judged by (cw.cxx:847).
3. **For 9.6, a finding and not a question.**
   - **The port.** Under HM-REQ-124, the port is calibrated on no condition, so it votes nowhere.
     An arbiter built on today's maps would always take ours on a disagreement.
   - **Ours.** Ours' p spans only 0.77 to 0.99, nearly all in bins 8 and 9. It is calibrated
     where it is because it is close to a base rate, not because it picks out its wrong letters
     (task 1's print: MarginLlr's quintiles run 0.84 to 0.98 right).
   - **The port's timing margin** falls in right share at both ends. The form fixed at task 1 was
     linear and was not bent after the print (V-14).

   A better port confidence would need a new form, fixed afresh before measurement. That is a
   later unit's choice, not this one's.
4. **The adapter's input, against the instruction's words.** `FldigiConfidence` needs the port's
   decision rows (`TraceDecisions` on) as well as `Emissions` and `KeyEvents` (section 1). 9.6 will
   have to run the port with the trace on. The trace records and changes nothing; the port's texts
   were byte-identical with it on.
5. **Port characters outside the scored stretches.** The harness locates each decoder's stretches
   in that decoder's own text, not in ours'. 220 of the port's 651 printed non-space characters fall
   outside every stretch located in its text (and 406 of ours' 1032 outside ours'). They carry a p
   and are unscored. This is 463's section 4 item 4 again, parked, and noted only because task 1's
   rows come from the same harness.
