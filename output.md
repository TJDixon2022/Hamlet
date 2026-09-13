```
READ IN THIS ORDER.
```

A. The phase goal - the screen, done right. Step 0 partial as the evidence stands: this is a
   first cut written at the end of task 0, and tasks 1 and 2 have not run yet.
B. Step 0 and its exit criteria: see section 1's task 0 table.
C. The report last. Section 4 is not written yet.

```
UNIT:       339 - stopped at task 0 of 3 - 2026-09-12 21:13
PHASE GOAL: The main window laid out as Tim's approved mockup, then the achievements pages as
            trading cards, then what the last phase left, then Tim's own pass at his window.
UNIT GOAL:  A named, green test with its result for every step 0 exit criterion at 1920 and
            1400, then step 1's ground measured without building.
ADVANCED:   no - first cut at the end of task 0; task 1 has not run
NUMBER:     step 0 criteria with a green named test at both widths: 3 of 6 before task 1
DRIFT:      0
```

## 1. What Claude did

**Stopped at task 0 of 3 in this first cut.** Tasks 1 and 2 are next in this session, and this
file is overwritten when they finish.

### Task 0 - the trace

Every test in `TheTopRowTests`, `TheWorkingPanelsTests`, `BindingHealthTests`, `VoiceTests` and
`TheOperatorCanStopItTests`, in one filter: 25 of 29 passed. All 20 step 0 tests are green.

| Test | Result | Criterion | Widths realized | Numbers |
|---|---|---|---|---|
| TheTopRowTests.AtNineteenTwentyTheTopRowIsAbout190AndTheWorkingCardTakesTheRest | pass | 1 | 1920 asserted, 1400 printed | top row 190 px at 1920, 219 at 1400 |
| TheTopRowTests.TheGreenBlockIsInsideTheCardUnderTheStripWithTheBandLargest | pass | 2 | 1920 only | band 20 px, next largest 15 |
| TheTopRowTests.TheWorldClockIsAtTheCardsRightEndWithOneMarker | pass | 2 | 1920 only | clock 246 x 134, markers 1 |
| TheTopRowTests.DriveAndThePowerOfferAreUnderTheRigAndTheSendAreaKeepsCqAndStop | pass | 3 | 1920 only | rig panel 190, card 190 |
| TheTopRowTests.AtFourteenHundredTheLicensedTopRowIsTheMockupsShare | pass | 3, 5 | 1920 and 1400 | 1400: top row 219 px (0.241), panels 474 (0.521) |
| TheTopRowTests.TheBestBetPillAndTheGreenBlockNameTheSameBandOnTheWindow | pass | nice-to-pass | 1920 | 20 m with the check, 40 m without |
| TheWorkingPanelsTests (8 methods) | pass | 1, 4, 5 | see section 3 | see section 3 |
| BindingHealthTests.TheMainWindowBindsWithoutOneComplaint | pass | 6 | 1 window | - |
| VoiceTests (5 methods) | pass | 6 | none | - |
| TheOperatorCanStopItTests | 5 of 9 | not on criterion 6 | 1400 x 1400 | 4 red, 3 not expected |

## 2. What the owner should expect

Nothing has changed on the screen yet.

## 3. What you should see

No visible change yet. This is a first cut, and tasks 1 and 2 are next.

## 4. What's blocking us

Not written yet. Unit 338's section 4 is carried here when the report is finished.
