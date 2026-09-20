# Unit 372 - the headless flake, measured rather than reported

**What this is.** Work instruction 372 task 1 item 3 asked this unit to run
`TheStopIsAlwaysOnScreenTests` three times and record whether the count was the same each
time. It was not. **This file exists because `output.md` is overwritten** - the unit that
does step 2 would otherwise inherit a rumor instead of a number.

**This is step 2's criterion 2.3 and it is not step 1's work.** Unit 372 recorded it and
repaired nothing (PSK31 plan R14: a unit writes the tests its criteria need and no others).
Nothing in this file was chased.

## The name

    Hamlet.App.Tests.Views.TheStopIsAlwaysOnScreenTests
        .KeyedAtTheOpeningSizeAClickOnTheBarFiresTheAbortWhileItRuns

It is the only name of the five in that type that was ever red. The other four
- `AtEachOf354sNineSizesStopIsInTheStatusBarAndOnTheWindow`,
`AtEachOfTheNineSizesTheSendAreaIsWholeOnTheWindow`,
`WithNothingKeyedItSaysStopAndIsStillPressable` and
`ArmedAtTheOpeningSizeAClickOnTheBarUnarmsItAsBefore` - were green in every run below.

## The counts, on one machine in one session, 2026-09-20

Whole type, filtered, one build each:

| run | result |
|---|---|
| 1 | 5 of 5 |
| 2 | 5 of 5 |
| 3 | **4 of 5** |
| 4 | 5 of 5 |
| 5 | 5 of 5 |
| 6 | **4 of 5** |
| 7 | **4 of 5** |

**Three of seven runs of the type were red.** The single name run alone, seven times:
**two of seven red** (one on its own invocation, then one of the six in a loop).

It was **green in the app carry-forward invocation** both before this unit's first change
and after its last, which is why the list has never carried it as a known red.

## What it fails on, verbatim

    Assert.Equal() Failure: Collections differ
    Expected: ["FE FE 94 E0 1C 00 01 FD", "FE FE 94 E0 17 FF FD", "FE FE 94 E0 1C 00 00 FD"]
    Actual:   ["FE FE 94 E0 1C 00 01 FD", "FE FE 94 E0 17 FF FD", "FE FE 94 E0 1C 00 00 FD",
               "FE FE 94 E0 17 FF FD", "FE FE 94 E0 1C 00 00 FD"]
                                          -> position 3

`TheStopIsAlwaysOnScreenTests.cs` line 231. The frames are key-on, the abort (`0x17 0xFF`)
and PTT off; **the abort pair is on the wire twice.**

## What that means, said plainly and not further than the measurement goes

The test keys a send that plays for twenty seconds, waits for the sink to be entered, clicks
Stop, and reads the port immediately. **The extra pair is a second abort**, and the two
candidates for who wrote it are the click's own `StopNow` and the transmit sequence's
unkey as it comes off the token. Which of the two it is, and whether the duplicate is a
defect in the product or only in the test's timing, **was not determined here** - the
instruction said to record and not chase, and guessing between them would be a finding
presented as an answer.

**What is certain from the numbers**: the read races the sequence's own teardown, so the
count the test asserts is a count taken at a moment that is not fixed. It is a timing
assertion written as an equality.

**Nothing here is evidence about the radio** (`SHACK_FACTS.md` FACT-004). This is a
`FakePort` and a `FakeSink` on a headless window; no device was opened and nothing was keyed.
