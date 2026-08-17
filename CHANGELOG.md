# Changelog

**This file is an index and not a record.** Every ruling behind every release,
with its reasoning and what was rejected, is in `DECISIONS.md`, newest first.
Writing the reasons out again here would be a second copy of the same facts, and
a second copy drifts (§0). So each release below is one line, a headline, and
the range of `HM-DEC-###` ids it contains. Follow the ids for the why.

Versions follow the convention in `CLAUDE.md` §1 (HM-DEC-063): major breaks an
existing setup or reconceives the application, minor adds a capability the
operator can see and use, patch fixes and polishes without adding one.

---

## 1.7.0

**Hamlet stops hearing things that are not there, and starts hearing things that
are.**

A station answering your call keys for a second or two in half a minute. Hamlet
was measuring how strong it was by averaging across the whole half minute, most
of which is silence, so a signal well out of the noise came out as nothing and
the pitch it reported was wherever the noise happened to be loudest. It now
measures while the station is actually sending.

Nothing is put on screen unless there is a tone to put it there. Half a minute of
band noise used to produce seventeen hundred characters, nearly all of them marked
uncertain; it now produces nothing, which is what is actually there.

No speed is shown unless letters are genuinely resolving, on any screen. The
sending speed had been appearing beside the filter width, in the terminal summary
and in the send panel while nothing at all was being received.

"Keep this audio" now refuses rather than writing the same recording twice, and
says so. If the sound card stops delivering, Hamlet notices within a couple of
seconds and tells you, instead of going on quietly analysing the last thing it
heard.

Rulings HM-DEC-090.

## 1.6.1

**Your layout comes back, and the send buttons know the law.**

The canvas always was being saved. What went wrong is that an arrangement built
on a big window, reopened on a smaller one, put everything faithfully back at
coordinates off the right-hand edge, so it looked like nothing had survived.
Anything entirely out of view is now brought back where you can reach it, and the
canvas tells you it did that rather than leaving you guessing.

The send controls now refuse where your license does not reach. They say which
rule decided it, and they name the nearest frequency on that band you could call
from instead. If Hamlet does not know your license class or cannot read the
frequency, it refuses and says which of the two it could not establish; the
privilege check remains yours to switch off in Settings.

Also fixed: a bad entry in the settings file could stop Hamlet starting at all,
and two buttons in "I can hear it and Hamlet can't" were dead.

Rulings HM-DEC-089.

## 1.6.0

**Hamlet hears further into the noise, and says what it is hearing.**

The decoder now measures the noise beside the signal instead of guessing it from
the signal, and sizes its own smoothing from the length of a dit rather than a
fixed number. Measured against a sweep, it reads a message two decibels further
down than it did, and it now returns easy signals perfectly where it used to get
one character wrong every time.

When nothing is decoding it tells you why. There is a difference between an empty
band, a signal it can see and cannot read, and audio that is not arriving at all,
and those used to look identical.

The input level is on screen while it listens. What comes out of your speaker and
what goes down the USB cable are two different signals with two different levels,
and turning one up does nothing for the other. Hamlet now reads the radio's USB
output level and the level Windows applies, and offers to fix the first.

"I can hear it and Hamlet can't" learned four more things: noise reduction,
the gain control, the filter width and that USB output level.

And there is a button that keeps the last thirty seconds of exactly what the
decoder heard, with everything the radio was doing written beside it. If you can
hear Morse that is not arriving, that file is how it gets fixed.

**The top strip is one row now**, giving about 150 px back to the canvas on every
screen.

Rulings HM-DEC-088.

## 1.5.1

**The canvas can now be operated.**

The tray, the preset buttons and the close buttons on each widget were dead:
seventeen controls that rendered perfectly and did nothing. They work.

And widgets can be dragged. Press anywhere on a widget's header and it follows
the pointer, comes to the front as you move it, lines up with its neighbors when
it gets close, and stays where you drop it. The corner grip resizes. All of it
works collapsed as well as open.

Buttons everywhere now look like buttons. Grey means a control genuinely cannot
be used and nothing else.

A widget you drag in arrives showing its contents rather than shut. The notice
about a panel that is away has moved onto the canvas, and it is louder when
something is happening right now than when it is telling you about something
that will keep.

"Keep it" is now "Save this layout".

Rulings HM-DEC-087.

## 1.5.0

**The screen is yours to arrange.**

The panels are widgets now, on a canvas you lay out however suits what you are
doing. Drag them by the header, resize them by the corner, and they line up with
each other when they get close without being forced onto a grid.

Above the canvas is a row of starting points, named for what you are doing
rather than for a mode: Getting started, Listening around, Making contacts.
Pressing one gives you a fresh copy every time, so rearranging can never spoil
the way back. Name what you have and keep it, in the box on the same bar.

The strip along the top does not move. Band, frequency, mode, where you are and
whether you may transmit are what you need before you need anything else.

The phrasebook comes out on its own when a contact starts and goes away after
you sign off, unless you have moved it, in which case it is yours and stays.

And a panel you have put away still speaks up. If Morse starts arriving while
the terminal is in the tray, a line says so with a button to bring it back, and
nothing that came in while it was away has been lost.

Rulings HM-DEC-086.

## 1.4.1

**The send buttons stop blinking.**

A transmission is now one state from the press to the last dah. The buttons go
unavailable once and come back once, and in between the panel says what is going
out and how much of it is left instead of reporting that the radio is busy.

The duration is worked out from the message and the keyer speed before the first
dit goes out, and the transmit line can only hold that open longer. It is never
allowed to end it early, because sampled at the rate the rig is polled that line
shows a second and a half of apparent quiet in the middle of a real call.

The seconds in the record are now the transmission rather than the handover.

Rulings HM-DEC-085.

## 1.4.0

**Hamlet changes the radio.**

Settings are consequences of intent, never things the operator operates. There
is no noise blanker toggle and there never will be: there is one button that
says "I can hear it and Hamlet can't", and behind it the handful of changes that
usually cause that, each announced in plain words with a way to put it back.

Three tiers, and the tier is the safety design. Nothing on the receive side can
put a signal on the air, which is why doing all of it is one press. Every write
carries its manual page, reads back before it is called done, and says so when
it cannot.

Rulings HM-DEC-084.

## 1.3.0

**Hamlet answers the question it was built for.**

After every send it reports what happened link by link: the radio took the
message, it keyed for this long, it made this much power into this match, this
many skimmers were reporting on the band, and this many copied you. A station
making no power and a band with nobody listening are different facts, and until
now both came out as silence. The Po meter is read for the first time, which is
the only proof that RF actually left the radio.

Every number is measured or it is not shown, and nothing anywhere diagnoses the
station.

Also: sending no longer has a look of its own, and the notice about not being
able to see the back of the radio is gone, replaced by a measurement.

Rulings HM-DEC-082 and HM-DEC-083.

## 1.2.8

**The send buttons stop looking grey when they work.**

They had no style of their own and fell through to the theme's pale default, so
a working button and a refused one looked identical. Ready is filled amber now,
armed is deeper amber, sending is green, and only a refusal is dimmed. Status
messages occupy reserved space rather than appearing and shoving the panel
around. Hamlet reads the SWR meter during a send and says what it measured
without ever claiming what is connected, and the notice about the back of the
radio retires once it has.

Rulings HM-DEC-080 and HM-DEC-081.

## 1.2.7

**The send controls tell the truth.**

Grey means refused and nothing else. The confirming press guards text the
operator edited rather than text Hamlet wrote, so an unedited message sends on
one press and says so. Sending is one stable state rather than a sample of a
transmit line that toggles on every Morse element. The send itself is now in the
record, by length and duration and never by its words. The build date is stamped
at compile time rather than read off a file timestamp that could lie.

Ruling HM-DEC-079.

## 1.2.6

**The send button works.**

Readiness reached Ready and the buttons stayed dead, twice, on live hardware.
The gate was right the whole time: the rig poll rebuilt the buttons four times a
second and a click cannot survive a control that is destroyed under the
operator's finger. The buttons now persist, the command carries the gate rather
than only the visual tree, and the record says what the operator saw beside what
the engine decided.

Ruling HM-DEC-078.

## 1.2.5

**Hamlet can explain itself.**

Telemetry became a decision record rather than a list of completions: every
decision point that can go more than one way now names the branch it took and
the state that decided it. The rig state travels with the record, levels mean
something, the decoder says what it heard and rejected, and a window shows what
Hamlet recently decided beside the one showing what the radio is doing. Also the
callsign resolver, the live-fire hardening of the transmit path, "did anybody
hear me", and the contact tracker.

Rulings HM-DEC-063 through HM-DEC-077.

## 1.2.0

**Hamlet can key the radio.**

CW transmit, behind a guard that answers before anything reaches the air and an
abort that awaits nothing. Favorites that carry the reason you saved them. The
happening-now panel's two lenses and its workability ranking. The digital
neighborhoods, cited. Out-of-band warnings on every surface that speaks. Mode
follows the map, which is the first thing Hamlet writes to a radio. And the
radio's own spectrum in the waterfall.

Rulings HM-DEC-045 through HM-DEC-062.

## 1.0.0

The foundation: the engine and the shell, CI-V, the CW decoder, the Explorer,
the neighborhood map, the band plan and privileges, and the training radio.

Rulings HM-DEC-001 through HM-DEC-044.
