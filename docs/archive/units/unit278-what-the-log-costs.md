# What the log costs at ten thousand — work instruction 278, task 5

**Measured, not estimated, and not optimised.** Synthesised into a temporary folder
through `SettingsStore.DataFolder` and deleted afterwards; **his real log was never
touched**, and on this machine there is none to touch (task 1).

Printed by `WhatTheLogCostsAtTenThousandTests` on the development machine,
2026-09-08, version 1.12.153, .NET 8 Debug build. **Debug, not Release**, which is
worth saying: these are the slow figures.

---

## The numbers

| | 1,000 records | 10,000 records |
|---|---|---|
| File size | **240,272 bytes** | **2,400,272 bytes** |
| Bytes an entry | 240 | 240 |
| Read and parse | **1.37 ms** | **40.09 ms** |
| Build the window's rows | **0.35 ms** | **19.49 ms** |
| **Total to open the window** | **1.72 ms** | **59.58 ms** |

**The first ask of the count on the main screen at ten thousand: 49.19 ms. The
second ask: 0.000 ms**, because the count and the *worked* mark are two derivations
of one read and the read is kept.

**Unit 274 measured 265 bytes an entry and 0.217 ms to read a hundred.** The size is
now **240**, a little smaller, because these records carry no `FREQ`; the read scales
about as that figure predicts, and nothing here contradicts it.

## Is the window usable at ten thousand rows? Yes, and it does not need paging

**Sixty milliseconds to open, once, on a menu click, in a Debug build.** That is
below what anybody notices as a delay, and it is paid when he asks for the window
rather than while he is working somebody.

**The count on the main screen is the figure that could have mattered and does not.**
It is read lazily on first ask and then kept, so the status bar costs one read per
launch, shared with the mark that was already doing it. Fifty milliseconds once,
never on the decode path, never inside a slot.

**So no paging, and none was added.** The instruction is explicit that paging must
not be added on the strength of a number nobody has looked at, and now that somebody
has looked, the number does not ask for it. **What would change that** is a log an
order of magnitude larger again, where the linear read is around half a second: if
he ever passes a hundred thousand contacts, the window is the thing to measure again.

**One caveat on the rendering, which this measurement does not cover.** The figures
above are the read and the row objects. The `ItemsControl` inside a `ScrollViewer`
realizes every row it is given rather than virtualizing, so ten thousand rows is ten
thousand controls. **That was not measured here** and is not claimed either way; it
is named so the next person measuring knows which half is still open.

## Two defects the measurement found, both fixed

**Neither would have been found by reading the code**, which is the argument for
task 5 not being dropped.

### He would have been congratulated for nine badges at once

At ten thousand records from a cold start the status bar read:

> That is 10 and 25 and 50 and 100 and 500 and 1,000 and 2,000 and 5,000 and 10,000
> contacts logged.

**One line, in the status bar, on first launch.** That is a wall of text rather than
the quiet acknowledgement he asked for, and it happens to anybody who installs a new
version beside a log that already exists.

**The rule that fixes it: the first look seeds where he stands and says nothing.**
`ContactBadgeAnnounced` starts at `-1`, meaning Hamlet has never looked, which is a
different fact from *nothing has been mentioned*. On that first read it is set to
where the log already stands, silently. **An acknowledgement is for a milestone he
has just passed**, and Hamlet was not there for the others.

**A genuine nine-to-twenty-six still earns both**, because by then Hamlet has looked
once and the seed is 0.

### The same number was formatted two ways, inches apart

The status bar read `10000 contacts logged` while the badge line beside it read
`10,000`. **Both now use the grouped form.** Trivial, and exactly the kind of thing
that is invisible until a number gets big enough to have a separator in it.

## What was deliberately not done

- **Nothing was optimised.** The instruction says measure and report, and the
  numbers do not ask for anything.
- **No paging, no virtualization, no cap on rows.**
- **His real log was not read, written, or looked for beyond task 1's count.**
