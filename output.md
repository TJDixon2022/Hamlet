# Work instruction 303 — the clock query says what it did

**READ IN THIS ORDER.** The clock query **works on this machine**, every time, and I
could not reproduce your fault here. What I did instead is make it impossible for the
next occurrence to be invisible, and I found two other things that were asserting
what they did not know.

A. **The phase goal — FT4 works exactly the way FT8 does.** This unit advanced no
   step of it. It closed a hole in the record, corrected two false sentences, and
   changed one word that mattered.

B. **Step 4 and its exit criteria** — pressing FT4 tunes and decodes; the panel, the
   conversation, the ring, the filters, the tooltips, the ledger and the right-click
   menu all working unchanged; one click, one transmission; a whole exchange from one
   right click at the bench. **None was measured tonight.** Step 4 stays `partial`.

C. **The report last, and section 4 raises 4 items.** One needs a measurement from
   your machine and nothing else will settle it.

```
UNIT:       303 — complete at task 7 of 7, none dropped — 2026-09-10 11:33
PHASE GOAL: FT4 works exactly the way FT8 does.
UNIT GOAL:  The clock query records what it did, the reason it is not happening is
            found, a lost audio device is not silent, and Sent means sent.
ADVANCED:   no — no phase step moved. Step 4 stays partial: nothing tuned, nothing
            transmitted, no exchange run at the bench.
NUMBER:     clock telemetry events, before and after
            BEFORE: 0. Across 2,600 events and eleven app starts, not one naming a
            time server, an offset, an attempt or a failure.
            AFTER:  2 per query — the attempt, then the outcome — with six failure
            paths each carrying their own stable token. Measured from the real
            view model: 2 records written, offset 1.884 s.
DRIFT:      7 consecutive units without advance  (was 6, carried from unit 302)
```

---

## 1. What Claude did

**Complete. Seven tasks of seven, none dropped, including the named drop candidate.**
Development machine, prompt claimed `PROJECT: Hamlet`, branch `main`, six commits,
all pushed. Root version 1.12.265 to **1.12.266**, bumped once. **No file under
`src/Ft8Sharp/` was touched**, and nothing here transmits.

**Nothing in this report is evidence about the radio computer** (`SHACK_FACTS.md`,
HM-DEC-093). That matters more than usual tonight: the fault you are seeing is on
your machine and every measurement below is from this one.

**Nothing was recorded to `DECISIONS.md`.** The five judgements made on this
session's authority are in `PHASE_OUTCOME.md`'s entry and listed below.

### Task 1 — the query records what it did

**Two events per query: the attempt, then the outcome.** The attempt is written
**before the socket opens**, which is the half that makes an absence readable — after
this, no record at all means nothing was tried.

**Six ways to fail returned one value.** `SntpClock` collapsed a name that would not
resolve, a packet that never came back, a reply shorter than 48 bytes and a timestamp
that would not parse into `ClockOffset.Unknown`, with nothing to tell them apart.

**And its caller claimed nothing could throw, with nothing verifying it.**
`QueryTheClockAsync`'s own remarks said every failure returns unknown; `SntpClock`
catches four exception types and there are more than four ways to fail. An unhandled
one inside a fire-and-forget task takes the application down. It now catches and
records.

**`Diagnostics`, not an eighth category** — About still reports 7 of 7.

### Task 2 — why it is not asking

**It is asking, and it is working.** Section 3 carries the stage-by-stage run.

**What I can rule out for your machine**, from what the code does rather than from a
theory: it is called from the constructor unconditionally and on a ten-minute timer,
so it is not a call that never happens; and **Hamlet binds an ephemeral port and only
sends to 123**, so whatever holds 123 on your machine is not in the way. That removes
the port-contention theory your table records as already tried.

**I did not claim a repair for a cause I could not see.**

### Task 3 — the message says the true fault

`ClockOffset.Describe` says *clock not checked yet, so slots cannot be cut* for **both**
no-offset states. **Saying "not checked yet" about a query that was made and failed is
the application asserting something it does not know**, and it is the sentence that
sent twenty minutes to the wrong machine. Watched failing before it was fixed.

### Task 4 — a lost audio device is not silent

**Reported before changed, as the instruction asks.** `ChooseEndpoint` returns null
where the saved id names no endpoint; nothing is substituted and the id is kept. That
half was already right.

**What was missing was the visible statement**, and it is now on the Settings window
in amber, naming the device.

**And what I did not find, which matters more.** I suspected the two-way picker
binding of erasing the saved device, **wrote a guard for it, then measured it innocent
twice**, and removed the guard rather than ship a fix for a mechanism I had disproved.
The disproof is written into the handler's own remarks.

### Task 5 — the achievement mark, option B

No pill, **27 px**, filled green at rest, orbit ring when something is new, belt pill
unchanged. **The ceiling was confirmed rather than assumed.**

### Task 6 — `Sent` means sent

**It never did.** Section 3 carries what it meant and what it means now.

### Task 7 — the outcome entry

Filed as **`UNIT 303 - STEP 4`** with nothing renumbered by hand.

### The five things decided on this session's authority

1. **The attempt is written before the socket opens**, so an absence is readable.
2. **`Diagnostics` rather than an eighth category.**
3. **The caller catches and records**, because it catches more than `SntpClock` does.
4. **I removed my own speculative guard** rather than shipping a disproved fix.
5. **Option B spends one of the mark's two greyscale carriers**, deliberately, on your
   ruling — the ring is now doing that work alone.

---

## 2. What the owner should expect

**The clock line tells you which problem you have.** If nothing has come back yet it
says Hamlet is asking. If a query was made and failed it says so and names what
failed. **It will no longer tell you the clock has never been checked when it has.**

**And the next occurrence will be in the file.** Open `%AppData%\Hamlet\telemetry\`
and look for `clock_query_started` and `clock_query_finished`. **If you see a started
with no finished, the query hung. If you see neither, nothing asked.** Those were the
same picture before tonight.

**A missing sound card says so.** Open Settings and, if the transmit device named in
your file is not on the machine, an amber line says which one is missing and that
sending is refused until it comes back or you pick another. Nothing is substituted.

**The feather is bigger and green at rest**, 27 px, filling the bar.

**What will look wrong and is not:**

- **`ft8_transmission` says `Played` where it used to say `Sent`.** The behaviour did
  not change; the word did, because it was false. Old records saying `Sent` mean
  exactly what `Played` means now, and none were deleted.
- **Two inherited reds.** `TheFitGuardAsksAboutTheGridTheSendIsOnTests` has one and
  `TheAchievementsScreenTests` has two; all three fail identically with this unit's
  changes stashed. Left alone under §12.6.

**Build:** succeeded, 0 warnings, 0 errors. **Tests:** 20 constructed in this
instruction across six classes, **all green**, filtered by exact name and foregrounded.
**No suite was run** (HM-DEC-155). Gates re-run: `BindingHealthTests` green, the
character ceiling 5 of 5 green, `VoiceTests` green.

**Pushed:** six commits to `main`.

---

## 3. What you should see

### 1. Three clock records, quoted

```
{"event":"clock_query_started","data":{"server":"pool.ntp.org"}}

{"event":"clock_query_finished","level":"info",
 "data":{"server":"pool.ntp.org","outcome":"measured","reason":"measured",
         "detail":"the server answered in 78 ms",
         "offsetSeconds":1.884,"roundTripMs":78.4}}

{"event":"clock_query_finished","level":"warn",
 "data":{"server":"pool.ntp.org","outcome":"failed","reason":"timeout",
         "detail":"nothing came back within 3000 ms"}}

{"event":"clock_query_finished","level":"warn",
 "data":{"server":"pool.ntp.org","outcome":"failed",
         "reason":"threw_InvalidOperationException",
         "detail":"the socket was already in use"}}
```

**A failure is a warning and a measurement is not**, so a query nobody could complete
can be found by scanning. **Nothing personal is written** and a sweep asserts it: a
hostname is not personal and neither is an offset.

### 2. Why the query is not happening — and on this machine, it is

Stood up, not searched for. Each stage on its own:

```
resolve : 4 addresses, first 142.248.80.92
send    : 48 bytes away
receive : 48 bytes from 142.248.80.92:123
parse   : 2026-09-10T15:17:48.2589990Z
offset  : 1.874 s, in 61 ms
```

And the **real view model**, built with a real telemetry sink, `MainWindowViewModel`'s
own constructor at `MainWindowViewModel.cs:5386`:

```
clock records written: 2
  clock_query_started   server=pool.ntp.org
  clock_query_finished  outcome=measured  offsetSeconds=1.884
ClockOffset.IsKnown = True
the line on screen  = clock is 1.88 s slow, checked just now
```

**So it does not stop anywhere here.** It is called at `MainWindowViewModel.cs:5386`
from the constructor and at `:6061` from a ten-minute timer, and both work.

**One theory removed for your machine**: Hamlet's socket binds an **ephemeral** port
(61906 this run) and only sends *to* 123, so `w32time` holding 123 cannot be in the
way. That is consistent with what you already found — stopping the service changed
nothing.

**I am not claiming a repair for your fault**, because I have not seen it.

### 3. The message, for the fault that is actually true

```
never asked yet :  Hamlet is asking a time server what the time really is, and
                   until it answers there is no way to cut the slots.

timed out       :  Hamlet asked pool.ntp.org what the time really is and nothing
                   came back, so the slots cannot be cut yet. It will keep trying.

no such host    :  ...and that name could not be looked up, which usually means
                   there is no network just now, so the slots cannot be cut yet.

threw           :  ...and something went wrong inside Hamlet, so the slots cannot
                   be cut yet.
```

**Before, all four read *clock not checked yet, so slots cannot be cut*.** One is
*wait a moment* and the others are *something is wrong*. A measured offset reads
exactly as it always did. It stays on screen unhovered, because `Ft8Slots.TrueUtc`
returns **null** without an offset, so no slot boundary can be cut at all.

### 4. What `Sent` meant, and what it says now

**It was set by three local successes and nothing else:**

1. a CI-V PTT-on frame handed to the serial port **without throwing**;
2. the sink reporting it played **every sample** it was given;
3. a PTT-off frame handed to the port **without throwing**.

Its own doc comment said *the whole signal went out and the radio came back to
receive*. **Not one of those three is a fact about the radio.** Writing bytes to a COM
port succeeds whether or not a radio is listening at the other end — which is exactly
how 21 records came to say `Sent` for transmissions that never keyed a transmitter.

**It is now `Played`.** The enum's own remarks carry the whole finding, `TransmitRun.Sent`
became `AudioWentOut`, **and no record was deleted.**

**Can Hamlet know the real thing? Partly, and that is a real repair I did not make.**
The radio reports transmit state on `1C 00`, which HM-DEC-147 already polls four times
a second, and power on `15 11`, which HM-DEC-082 already reads as *power made*. Joining
those to this outcome would let the record say the transmitter keyed. **The instruction
scopes task 6 to the word and I kept to it.**

---

## 4. What's blocking us

### 1. The clock fault is on your machine and needs one measurement from it

**This is the only item that needs you, and it will settle in a minute.**

Run Hamlet, then open `%AppData%\Hamlet\telemetry\` and find today's file:

- **`clock_query_started` with no `clock_query_finished`** — the query hung; the
  `detail` on the next one that does finish will say where.
- **`clock_query_finished` with `reason` set** — that token is the answer. `timeout`,
  `socket_HostNotFound`, `socket_NetworkUnreachable` and the rest each mean something
  different and each has its own fix.
- **Neither** — nothing asked, and that is a different bug from any of the above.

**Before tonight all three of those looked identical.** They do not any more.

### 2. Whether the transmission record should ask the radio

**A ruling, and it is the one with the most behind it.** `Played` is honest but it is
not what you want to know. Hamlet already polls `1C 00` four times a second and reads
`15 11` for power made.

> **The transmission record says whether the transmitter keyed, taken from the radio
> rather than from the port write.** `Played` stays as what the audio path did;
> a second fact says what the radio did, and where the radio did not answer it says
> unknown rather than assuming either way.
>
> **Why:** the record exists to tell whether a fault is in the signal, the radio or
> Hamlet (§0.0.1), and today it cannot distinguish *the radio was off* from *it went
> out fine*. **What is rejected:** inferring keying from a successful port write,
> which is what produced the 21 records.

**Not started.** It is a change to what the record asserts, which is yours (§12.1).

### 3. Two inherited reds, named and left alone

`TheFitGuardAsksAboutTheGridTheSendIsOnTests.BothFt8RefusalSentencesAreWhereTheyWereBeforeThisUnit`,
and `TheAchievementsScreenTests`' two. **All three fail identically with this unit's
changes stashed**, proved by stashing. Neither is in `docs/unit239-failing-set.txt`.

### 4. `Ft8Sharp` did not move

No file under `src/Ft8Sharp/` was read, edited or built.

### Asks still outstanding

Carried verbatim per HM-DEC-139.

1. **Nothing in this repository can look at a picture.** *First made 2026-09-09, unit
   300.* Six units running. Real pixels want `Avalonia.Headless.Skia`, and **a package
   is a dependency decision rather than a session's** (§0.4). **Waiting on:** your
   ruling. **Where it sits:** nowhere — no package has been added.

2. **The map image.** *First made 2026-09-09, unit 301.* The projection is built and
   proved against independent trigonometry; the picture and its three numbers are
   missing. **Waiting on:** a file from you. **Where it sits:**
   `assets/azimuthal-map.md`.

3. **`TheAchievementsScreenTests` has two red, inherited.** *First made 2026-09-10,
   unit 302.* Not in `docs/unit239-failing-set.txt` and named by no order. **Waiting
   on:** an order that takes them. **Where it sits:** unchanged.

4. **Two ±2 wobbles** on `SettingsWindow` and `AboutWindow`, far under their ceilings.
   *First made 2026-09-10, unit 302.* **Waiting on:** an order that takes them.
   **Where it sits:** unchanged, and both still read 1628 and 726 tonight.

**And one dropped rather than carried.** The achievement mark's size was ask 5 inbound;
task 5 built option B and it is closed.
