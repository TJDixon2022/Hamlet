# What the record has to hold to answer a question without asking him

**Work instruction 304, task 1. Reading only; nothing was added while writing this.**

**Tim's ruling, 2026-09-10:**

> *"I want you to collect the information necessary in the telemetry. I'm not typing in
> a bunch of commands for you. The idea is telemetry should be able to diagnose any
> issue."*

**This is a list of facts, not of events.** What has to be *knowable* from one day's
file. Where each fact lives is task 2's and task 3's problem, and designing the
snapshot from what is easy to log is exactly how it would miss what was needed today.

---

## The rule the three failures share

**Every one of them was a state nobody could see, not an event nobody logged.**

The application writes 2,600 events on an ordinary day and they are almost all
*things that happened*. What was missing each time was *what was true* - which sound
card was selected, whether a clock reading existed, whether a port write reached a
radio. **A stream of events cannot answer a question about a state unless something
wrote the state down.**

**And a second rule, learned the same day:** a fact that is absent from the file is
indistinguishable from a fact that was never true. Twenty minutes went on `w32time`
because *asked and failed* and *never asked* left the same empty file.

---

## Mystery 1: the clock says it cannot reach a time server

**What it would take to answer with no question asked:**

| The fact | Knowable today? |
|---|---|
| Whether a query was attempted at all | **Yes, since unit 303** - `clock_query_started` |
| What came of it, distinguishing six failures | **Yes, since unit 303** - `clock_query_finished` with a stable token |
| Whether the offset Hamlet currently holds is known, and its age | **No** |
| Whether the machine has any network at all just now | **No** |
| Which server was asked | **Yes, since unit 303** |
| That the reader is looking at a machine where the category is on | **No** |

**Unit 303 closed the hole and it was not enough.** It records one call. It does not
record **the state the machine was in**: a reader opening the file still cannot say
whether Hamlet was holding a good offset from an hour ago, or nothing at all, at the
moment the operator was looking at the screen.

**Missing facts:** the current offset and its age, at startup and whenever it changes;
whether the process believes it has a network.

---

## Mystery 2: pressing CQ does nothing

**The real cause was a transmit audio device missing from `settings.json` after a
reboot**, and the author bisected two builds and blamed unit 302 first.

| The fact | Knowable today? |
|---|---|
| That a transmit device was named in settings | **No** |
| That the named device was not present on the machine | **No** |
| Which devices *were* present | **No.** `audio_device_chosen` carries one boolean, `looksLikeRadio` |
| Which device was selected for receive, and whether it exists | **No** |
| That readiness refused, and on which field | **Yes** - `transmit_readiness` carries `determinedBy` |
| That the operator could not see the refusal | **No, and it is not a telemetry fact** - it is a screen fault, and unit 303 fixed the Settings half |

**This is the one that shames the record most.** 56 refusals were written, each naming
the field that decided it, and **not one of them named the thing that was actually
wrong**, because the missing device was upstream of the fields readiness looks at.

**Missing facts:** every audio input and output device present, by id and name; which
is selected for receive and for transmit; **whether the selected one exists**; what
`settings.json` named that the machine does not have.

---

## Mystery 3: did anything transmit?

**21 records read `outcome: Sent` for transmissions that never keyed a transmitter**,
and the author read them back as evidence the path worked.

| The fact | Knowable today? |
|---|---|
| That audio was composed and played to a named device | **Yes** |
| That the CI-V frames were written to the port | **Yes** - `Keyed` |
| **That a radio was at the other end of that port** | **No** |
| That the transmitter actually keyed | **No** - ask 1, Tim's, and not this unit's |
| Which device the audio went to | **No** |

**Unit 303 renamed the outcome to `Played`, which stopped the record lying.** It did
not make the question answerable. **A record that says less is better than one that
says more than it knows, and neither one diagnoses anything.**

**Missing facts:** which audio endpoint the transmission was played to; whether the
CI-V link was answering at the time; whether the radio's own transmit state was ever
read. **The third is ask 1 and stays parked.**

---

## The faults this project has already had

From `OPEN_ISSUES.md` and the outcome files. **The same shape every time.**

### The audio device

**The application picks a receive device by matching `USB Audio CODEC` to preselect
one, never to claim one is the radio** (§4, HM-OPEN-003). Nothing writes down which
one it landed on, so a machine that enumerated its devices in a different order after
a driver update is invisible in the record.

**Missing:** the whole device list, and which was chosen and how - matched, remembered
from settings, or defaulted.

### The CI-V link

`civ_link` writes `outcome` and `reason` - `answering` against `commands_unanswered`.
**That is the right shape.** What it does not carry is which port, at what baud, with
which CI-V address, and what the radio answered `04` and `1C 00` with.

**Missing:** port, baud, address, model as read, and when the radio last answered
anything.

### The queue drops

`JsonlTelemetry.DroppedEventCount` exists and **nothing ever writes it**. A file that
lost events looks exactly like a quiet evening. **A record that cannot say it is
incomplete is a record that cannot be trusted about an absence** - which is precisely
what today's three mysteries turned on.

**Missing:** the dropped count, written at least at shutdown and in the bundle.

### The arrival ratio

`arrivalRatio` and `slotArrivalRatio` are already carried on the decode event. **This
one is done and is the model for the rest.**

### The settings file

Nothing records that `settings.json` was read, whether it parsed, what it named that
does not exist, or what was silently defaulted. **Unit 302 found the sweep racing a
background reconnect; unit 303 found a transmit device that had gone.** Neither was
visible in the file.

**Missing:** that it loaded, and every value it named that the machine could not
honour.

---

## What must be knowable, in one list

**Task 2 puts these in a startup snapshot; task 3 keeps them true as they change.**

1. **Audio.** Every input and output device present, id and name. Which is selected
   for receive and for transmit. **Whether each selected one exists.** How it was
   chosen.
2. **The radio.** Connected or not; port, baud, CI-V address; the model as read; when
   it last answered.
3. **The clock.** Whether an offset is held, its value and age, and the last query's
   token.
4. **Transmit readiness.** The state and every field it was decided from.
5. **Versions.** The application, `Ft8Sharp`, `Ft8Sharp.Deep`, the framework, the
   operating-system build.
6. **The settings file.** That it loaded; anything it named that is absent; anything
   defaulted.
7. **The telemetry itself.** Which categories are on, and how many events have been
   dropped. **A category that is off drops its events silently** - measured in
   `JsonlTelemetry.Write`, which returns before serialising - so a reader cannot
   otherwise tell *nothing happened* from *not recorded*.

**And the rule that governs all seven:** a fact that cannot be determined **says so,
with why**. Not absent, not a plausible default. Today's lesson is that a wrong
diagnostic costs more than a missing one.

---

## What is deliberately not here

**Nothing personal** (§2.1): no callsign, no name, no grid, no location. A device
name, a driver, a port, a version, an offset and a hostname are **not** personal, and
the About window already promises exactly that.

**Whether the transmitter keyed** is ask 1 and is Tim's. Task 3 records the readiness
fields either way.
