# Decisions

Rulings, newest first. A ruling is never edited — a later decision supersedes
it by id. Index in `CLAUDE.md` §1.

---
id: HM-DEC-086
date: 2026-08-15
refs: src/Hamlet.App/Layout/Widget.cs, src/Hamlet.App/Layout/CanvasLayout.cs, src/Hamlet.App/Layout/LayoutPresets.cs, src/Hamlet.App/Layout/LayoutStore.cs, src/Hamlet.App/ViewModels/CanvasViewModel.cs, src/Hamlet.App/Controls/WidgetCanvas.cs, src/Hamlet.App/Controls/WidgetFrame.cs, src/Hamlet.App/Controls/WidgetBody.cs, tests/Hamlet.App.Tests/Layout/CanvasTests.cs, HM-DEC-021, HM-DEC-064
---

**The panels become widgets on a canvas the operator arranges, and Hamlet stops
deciding what matters this minute.**

The main screen was one vertical scroll and had outgrown it. Every panel this
application ever grew went into one column in the order it was built, and the
operator scrolled past the ten they were not using to reach the two they were.
Reordering them helped once (HM-DEC-064) and could only ever help once, because
the right order depends on what somebody is doing, and that changes every few
minutes.

**THE STRIP ALONG THE TOP IS NOT PART OF IT**, and cannot be closed or moved.
Band, frequency, mode, where you are and whether you may transmit are what you
need before you need anything else. This widens HM-DEC-021's exemption for the
rig display to the whole strip it sits in, on the same reasoning: it is the
app's anchor, and an anchor you can drag away is not one.

---

**FREE PLACEMENT, NOT A GRID.** A grid decides in advance how big things are
allowed to be and where their edges may fall, and the operator then spends their
time negotiating with it. The canvas is a plain surface with real coordinates.
The only cleverness is that an edge within ten pixels of a neighbor's edge lines
up with it, so things sit straight without having to be made to, and a
deliberate gap survives untouched. **Snapping that fights the operator is worse
than no snapping.**

**A PRESET IS A STARTING POINT AND NEVER A DOCUMENT.** Pressing one loads a
fresh copy, every time, and dragging things about afterward does not change it.
That is what makes the bar safe to press: the way back from a canvas that has
got away from somebody is one press, and it cannot itself be spoiled by
rearranging. **PRESETS SIT ABOVE THE CANVAS, NOT IN A MENU**, because an
arrangement buried two levels into a menu is one nobody finds.

**THEY ARE NAMED BY ACTIVITY, NOT BY MODE.** Getting started, Listening around,
Making contacts. "CW layout" is a name somebody has to already understand in
order to pick, and this application exists for a person who has held a license
for six years and made one contact. **There is no FT8 preset**, ruled rather
than overlooked.

**Making contacts is Tim's own arrangement and the reasoning is his:** the send
controls sit directly beneath the terminal so that reading a call and answering
it is one motion, "Did anybody hear me" goes under that because it answers the
question the send raises, and who is out there stands tall on the right. The
band map is deliberately absent, because it belongs to looking around and this
is the arrangement for when you have already found somebody.

**NOBODY EVER STARTS ON AN EMPTY CANVAS.** A first run lands on Getting started,
furnished, and so does a layouts file that could not be read. An empty rectangle
beside a list of things to drag is a puzzle handed to somebody who came here to
talk on the radio.

**SAVING IS ONE ACTION FROM WHERE YOU ARE**: a box on the bar already in front
of them, and the arrangement in front of them is what gets kept. Anything more
and nobody saves anything, and the presets become the only arrangements that
exist. Saved layouts live in `layouts.json` beside the operator profile, in
their own file rather than a corner of the settings, so an arrangement can be
kept, mailed to somebody or put back after an experiment, and so a corrupt
layout cannot take the callsign down with it.

**SOME WIDGETS ARRIVE ON THEIR OWN.** The mechanism is general and the
phrasebook is the first case: it comes out when a contact starts and goes away
after the sign-off, which is exactly when somebody needs to know what people say
and exactly when they do not. Only widgets that declare themselves summonable
can arrive that way, so no later wiring can make an arbitrary panel jump onto
somebody's canvas. **And one the operator has moved is theirs from then on and
is never taken away again**, because a panel that vanishes just after somebody
has put it where they want it teaches them not to touch anything.

---

**WHAT HAPPENS WHEN THE WIDGET IS NOT OUT, WHICH IS THE QUESTION THIS RULING HAD
TO SETTLE.** Morse arrives and the CW terminal is in the tray. Hamlet may not
swallow it, and it may not fling the terminal onto somebody's arrangement
either, because they took it off on purpose.

**So the canvas carries a quiet line saying what is happening, with the widget's
name on a button beside it.** This is §0.5 one level up: a collapsed panel still
carries its summary, and a widget that is not out still carries its news.

**And nothing is lost while it is away.** The decoder goes on decoding, the
spots go on arriving, the reports go on being counted, all of it into the same
view model the widget would have been reading. Taking a widget off the canvas
removes a display and never a subscription, so bringing it back shows the
history rather than starting from the moment it reappeared. That is the part
that matters, and it is the part a lesser answer would have got wrong.

---

**A COLLAPSED WIDGET SHRINKS TO ITS HEADER**, found by running the thing rather
than by reasoning about it. In a column a panel that shut handed its space to
the panel below (HM-DEC-021); on a canvas the frame kept the height it had been
given, so collapsing something left a rectangle of nothing. **The panel still
owns whether it is open** and goes on persisting that per panel in
`settings.json` exactly as before. The frame only follows it, so there is one
answer to the question rather than two that can disagree (§0).

**The thirteen panels are unchanged.** Each one moved into a template keyed by
its widget id, and what it binds against is still the main view model, so not
one binding inside thirteen panels had to be rewritten to gain a position, and
none of them can have been rewritten wrongly. The only things taken away are the
row and column numbers a fixed layout needed.

**A LAYOUT NAMING A WIDGET THIS BUILD DOES NOT HAVE LOSES THAT WIDGET AND
NOTHING ELSE**, and the line stays in the file, so going back to a build that
has it restores the arrangement whole. A widget id is never renamed for the same
reason.

---

**"Where am I in this contact" is not a widget**, and that is worth writing down
because the brief listed it as one. Today it is the stage strip inside the send
panel rather than a panel of its own, so it travels with Send and cannot be
placed separately. Making it its own widget is a change to what the send panel
is, not a change to the canvas, and it is not made here.

**WHAT HAS BEEN SEEN AND WHAT HAS NOT.** The canvas was run, screenshotted and
looked at: the presets, the tray, the widgets, a panel's full contents inside a
frame, a collapsed widget shrinking, and the absent-widget line with its button.
The snapping arithmetic and every rule above about presets, saving, summoning
and absent widgets are held by tests. **Dragging and resizing with a real
pointer have not been done by anybody yet**, because a screenshot cannot press a
mouse button, and that is where a first look should go.

---
id: HM-DEC-085
date: 2026-08-15
refs: src/Hamlet.RadioEngine/Cw/CwDuration.cs, src/Hamlet.RadioEngine/Cw/TransmissionWatch.cs, src/Hamlet.App/ViewModels/CwTransmitViewModel.cs, tests/Hamlet.RadioEngine.Tests/Cw/TransmissionWatchTests.cs, tests/Hamlet.App.Tests/ViewModels/SendGuardTests.cs, HM-DEC-079, HM-DEC-083
---

**A transmission is one state, from the press to the last dah. The send controls
change once on the way down and once on the way back up, and never in between.**

This is the third attempt at the operator's most-repeated complaint, and the
first two are worth writing down because both of them looked right.

**THE FIRST ATTEMPT SAMPLED THE TRANSMIT LINE.** Readiness refused with "the
radio is already transmitting", which is correct on every individual reading and
useless as a description of a state, because under full break-in the transmit
line drops between every dit. An eighteen-second call put the panel through
dozens of enable and disable cycles and lost presses into the disabled frames.

**THE SECOND ATTEMPT BUILT A LATCH, PASSED ITS TESTS, AND FAILED ON THE RADIO.**
It latched on the send operation, which is the right instinct and the wrong
operation. Command `17` hands up to thirty characters to the radio's own keyer
and returns as soon as the bytes are accepted, about thirteen milliseconds later.
The radio then keys on its own for the next eighteen seconds with nothing
watching. So the latch released at 13 ms and gave the panel straight back to the
flapping line, and every test passed because no test crossed that boundary.

**HANDING THE MESSAGE OVER IS NOT THE TRANSMISSION.** That sentence is the whole
ruling and everything below it is consequence.

---

**THE END OF A TRANSMISSION IS PREDICTED BEFORE IT STARTS.** Morse timing is
arithmetic: PARIS is fifty dit lengths, a word a minute means PARIS once, and the
element count follows from the text. The keyer speed is already read over
`14 0C`, so the duration is known at the moment of the press. It is counted by
`MorseCode`, which already held the table, the dit and the element counter for
the waterfall's keying rhythm and the field guide's audio, and which already knew
about the radio's `^` run-together character that a second copy written for the
transmit path got wrong on its first attempt. One table (§0).

**AND THE TRANSMIT LINE MAY ONLY EVER EXTEND THAT, NEVER SHORTEN IT.** This is
the correction the session made against its own brief, and it came from a
measurement rather than an argument. The brief called for a hold-off: the
transmission is over when the line has been quiet longer than the longest gap the
message could contain, a word space being seven dit lengths, so about half a
second at twenty words a minute. That reasoning is sound about the radio and
wrong about Hamlet, **because Hamlet does not watch the line, it samples it.**
The rig state is read about four times a second and a dit at twenty words a
minute is sixty milliseconds, so the samples beat against the keying rather than
observing it. Replaying a real CQ through its real key pattern at the real poll
rate, **the longest stretch in which no sample catches the key down is a second
and a half, in the middle of the message.** There is no hold-off both short
enough to be useful and long enough to survive that.

So the arithmetic is the floor and the line only holds the state open longer. A
missed sample costs nothing and a seen one can only help, which is the way round
that cannot blink. What it gives up is an ending Hamlet did not cause: if the
radio stops on its own the panel stays busy until the computed time is up, a few
seconds at worst. **The operator's own stop still ends it on the spot**, with no
hold-off and nothing awaited (§0.2), because that ending Hamlet did cause.

**A DURATION WATCHED AND A DURATION CALCULATED ARE DIFFERENT KINDS OF FACT**
(§0.0). Where the radio does not report its transmit line at all, the arithmetic
is the only thing there is, and that is recorded as such and never reported as
something Hamlet saw. Unknown stays unknown (HM-DEC-050): the keyed-seconds
figure in the transmit chain is null in that case rather than a number.

---

**THE DURATION IN THE RECORD WAS WRONG, AND IT HAD ALREADY REACHED THE
OPERATOR.** `cw_send_completed` reported a hundredth of a second for an
eighteen-second transmission, because it was measured across the send call. The
telemetry method's own documentation said the duration was the number that proves
it, and the field it wrote proved the opposite on every row. Worse, it fed the
account of the send, so the screen said the radio keyed for 0 seconds while the
radio was audibly keying. **Completion means the radio finished sending, not that
the bytes were accepted.** How the end was established goes in the record beside
the figure, because a file that cannot tell a measurement from a calculation
cannot settle an argument (§0.0.1).

---

**DURING THE TRANSMISSION THE PANEL DOES SOMETHING RATHER THAN PREVENTING
SOMETHING.** The operator pressed the button and Hamlet started the send, so
Hamlet knows exactly what is happening and says so: what is going out, how much
of it is left, and a stop. The refusal wording it used to show was written for
the case where something else keys the radio, which is a genuine unknown. When
Hamlet is the one transmitting it is not an unknown, and describing it that way
makes the application sound like a bystander to its own work.

The busy message appears once and holds, in space that was already reserved, so
nothing below it reflows (HM-DEC-080). Every send button wears the unpressable
look for the duration, including the ones not going out, because none of them can
be pressed and a control that looks ready and does nothing is the dead-button
complaint in a different costume. The explanatory text stays readable throughout.

---

**THE TESTS, AND WHY THEY LOOK LIKE THIS.** The brief asked for a simulated send
in which the transmit line toggles twenty times and the state changes exactly
once in each direction. **Written that way it passes against plain edge
detection**, which ends the transmission in the gap between the first two dits,
because a latch can only change twice by construction: once it is down it is
never raised again, so counting its changes cannot tell a latch that held from
one that let go immediately. That was found by writing it, watching it pass
against a deliberately broken implementation, and rewriting it.

What is asserted instead is that the controls were still unavailable at **every
sample** up to the moment the message could possibly have ended. The line is not
toggled by hand: it is driven by the real key pattern of the real message at the
rate the rig is really polled, which is the closest a test gets to the radio
without one plugged in. The line's own transition count is asserted too, so a
simulation that stopped flapping could not let the test pass by becoming easy.
Both the naive edge detection and the brief's hold-off-as-stated were run against
it and both fail it.

**Everything below the UI takes the time rather than reading a clock**, so a whole
eighteen-second transmission runs in a test in microseconds and comes out the
same every time (§5.4).

**NONE OF THIS HAS MET A RADIO.** It is arithmetic and a state machine, tested
against a simulation of the keying built from the app's own tables. The two
previous attempts also passed their tests. What is different is that the boundary
those tests did not cross is now the thing being tested, and that the design was
changed by a measurement taken during the session rather than by the reasoning
that opened it.

---
id: HM-DEC-084
date: 2026-08-15
closes: the writes question HM-DEC-050 deferred
refs: src/Hamlet.RadioEngine/Civ/CivWrites.cs, src/Hamlet.RadioEngine/Rig/ReceiveAdvice.cs, src/Hamlet.RadioEngine/Rig/SettingChange.cs, src/Hamlet.App/ViewModels/ReceiveHelpViewModel.cs, tests/Hamlet.RadioEngine.Tests/Rig/RigWriteTests.cs, HM-DEC-049, HM-DEC-050, HM-DEC-056
---

**Hamlet changes the radio, and never shows a rig control.** This is the writes
ruling HM-DEC-050 deferred when it said the state model was reads only.

**THE GOVERNING IDEA, AND EVERYTHING ELSE FOLLOWS FROM IT: settings are
consequences of intent, never things the operator operates.**

A rig control app gives somebody a Noise Blanker button and expects them to know
when to press it. Hamlet gives them one button that says "I can hear it and you
can't", does the four things that usually cause that, says what it changed in
plain words, and offers to put it back. **Nobody ever learns what auto notch is.
They learn that pressing that button usually helps.**

**NO SCREEN IN HAMLET MAY CONTAIN A CONTROL THAT CORRESPONDS ONE-TO-ONE WITH A
RADIO SETTING.** If a future session finds itself building a row of toggles named
after menu items, it has misread this ruling. It is the same pattern as the
license class, the grid square and the audio device: the app works it out and
says what it found.

WHY NOW. The operator is licensed six years, made his first contacts this week,
and spent an evening calling CQ into silence. Two causes were found and both were
radio settings the app could read and could not change: the receive gain sat at
42 percent so the receiver was deaf for two hours, and the CW filter was wide
open the previous evening so the decoder read garbage. Auto notch was on, in CW,
which the diagnostics screen already explained is wrong. **Hamlet knew, printed
it, and could not act.** That gap is the feature.

---

**THREE TIERS, AND THE TIER IS THE SAFETY DESIGN** rather than a confirmation
dialog on everything.

**Tier one is the receive side and Hamlet does it and mentions it.** Nothing in
it can put a signal on the air, which is what makes "do all four" one press.
Asking permission four times for four changes nobody else can hear is exactly the
protectiveness this ruling exists to remove: it trains somebody to click through
prompts, which is worse than not having them. **What earns a prompt is what other
people can hear.**

**Tier two changes what the operator sounds like** and is offered rather than
simply done: power, keyer speed, break-in and its delay.

**Tier three keys the radio.** Only the antenna tuner's tuning cycle is in it,
and it goes through the same gate, the same visibility and the same record as a
CW send (§0.2). Never automatic. Offered clearly, because holding TUNER for a
second is the documented fix for a high standing wave ratio (p. 11-2) and nobody
should have to know that.

---

**NO BYTE IS WRITTEN THAT IS NOT IN THE TABLE, AND EVERY ROW CARRIES ITS PAGE**
(§4, HM-DEC-049). `14 08` is the standing warning: a wrong sub-command on a read
returns a bad number, and on a write it moves somebody's passband. Every row here
was read column-aware from `A7292-4EX-6` this session rather than transcribed,
and that produced four corrections worth recording:

- **The AGC row is `00 to 03`, not three values.** It reads
  `*(00=OFF, 01=FAST, 02=MID, 03=SLOW)`, so AGC can be switched off entirely. A
  table starting at FAST would have no way to say off and no way to put it back
  for somebody who had it off.
- **The antenna tuner is on p. 19-7**, not with the rest. Its three values are
  spelled out across three lines, and the third, `02`, is "Send/read to tuning".
- **`1A 05 0061` is on p. 19-5**, not 19-4 with its neighbors.
- **`16 65`, IP+, is deliberately absent.** Its row reads "Send the IP+ function
  setting" where every neighbor reads "Send/read", so the manual documents no way
  to read it back. **A write that cannot be confirmed and cannot be undone is not
  a write this app makes.** Recorded rather than quietly skipped, because the
  next session will see the row and wonder.

**READ BEFORE WRITE, READ BACK AFTER.** An acknowledgement says the radio
understood the frame, not that the setting moved, and those come apart on exactly
the settings somebody would most want to trust. A write that cannot be confirmed
by a read-back is reported as unconfirmed and **never as done**.

**EVERY WRITE IS ANNOUNCED** in plain words with its reason. A silent change to
somebody's radio would break the whole posture of an application that says what
it knows and where it learned it.

**EVERY WRITE IS UNDOABLE**, individually and together, for the session. And
**unknown stays a first-class state** (HM-DEC-050): where the prior value was
never read, the record says so and the undo is not offered, because writing a
plausible number into somebody's radio while calling it "restoring" would be the
guess §0.0 forbids wearing the most reassuring word in the application.

---

**THE LIST IS COMPUTED FROM LIVE RIG STATE AND IS NEVER HARDCODED.** Rows already
correct **stay visible and say so** — hiding them is tidier and teaches nothing,
while showing them is the app proving what it checked, which after that evening
is the difference between being trusted and being second-guessed. Rows that could
not be read **say that**, and are neither acted on nor silently dropped: dropping
one would leave somebody believing Hamlet had looked at something it never saw.

ONE WRITE IS OFFERED ONCE AND EXPLAINED RATHER THAN SET SILENTLY. `1A 05 0025`
set to `01` makes the RF/SQL knob do squelch only and fixes the receive gain at
maximum, which makes the two-hour deaf-receiver failure impossible. It still
gets asked, because it changes what a physical knob on somebody's radio does, and
a control that stops behaving the way its owner expects is worse than the problem
it solves.

**THE OFFER APPEARS WHERE THE PROBLEM SHOWS.** A popup somebody has to know to
open is a popup they will not open when they are frustrated, which is exactly
when it is needed. So when the terminal has decoded nothing for two minutes and
there is something Hamlet would change, a quiet line appears there. One line, not
a banner, dismissible, and silent when there is nothing to change, because an
offer to fix a radio that is already right teaches somebody to ignore the next
one.

---
id: HM-DEC-083
date: 2026-08-15
supersedes: HM-DEC-079 (the sending appearance), HM-DEC-081 (the notice's retirement)
refs: src/Hamlet.App/ViewModels/CwTransmitViewModel.cs, src/Hamlet.App/Views/MainWindow.axaml, HM-DEC-074, HM-DEC-082
---

**Two simplifications, both Tim's, both replacing something built two rulings
ago.**

**SENDING HAS NO LOOK OF ITS OWN.** While a message is going out the buttons are
disabled and the status text says what is happening. That is all. HM-DEC-079 gave
sending a dedicated green appearance, and the reasoning there was that sending is
an active state which should not wear grey. Tim has ruled otherwise and he is
right: **you cannot send while sending**, so grey is exactly correct and
self-explanatory, and the extra state was solving a problem HM-DEC-079's own
latch had already removed. A state that needs its own color to be understood is a
state that has not been explained.

The latch itself stands. The controls still hold one stable state for the whole
send rather than sampling a transmit line that toggles on every Morse element;
they simply no longer get a color for it. Armed keeps its appearance, because
armed is pressable and the press is the point.

**THE NOTICE ABOUT THE BACK OF THE RADIO IS DELETED.** Not retired on evidence,
not shown once: gone. HM-DEC-081 retired it on the first real SWR reading, which
was the right shape and still one screen of standing prose too many, and Tim has
said repeatedly that he hates the wall of text.

What replaced it is better than a shorter version of it. **HM-DEC-082's chain
report answers the question that notice was gesturing at, with a measurement
instead of a caveat**: it says what the power meter and the SWR meter actually
read during the send. A sentence with a number in it beats a paragraph admitting
ignorance, and that is the whole trade. A test asserts it never returns.

STILL STANDING PROSE IN THE SEND PANEL, PROPOSED AND NOT CUT. The per-message
explanatory lines under each phrase ("the callsign goes twice because the first
one is often half-missed", "QRS means send more slowly") teach on first read and
become wallpaper on the fiftieth, and the same is true of the two paragraphs
about keyer speed and character spacing at the foot of the panel. They are the
next candidates and they are not removed here, because they are the app's
teaching voice and cutting them is a decision rather than a tidy-up.

---
id: HM-DEC-082
date: 2026-08-15
refs: src/Hamlet.RadioEngine/Cw/TransmitChain.cs, src/Hamlet.RadioEngine/Civ/CivValues.cs, src/Hamlet.RadioEngine/Explore/RbnActivitySource.cs, tests/Hamlet.RadioEngine.Tests/Cw/TransmitChainTests.cs, HM-DEC-050, HM-DEC-074, HM-DEC-075, HM-DEC-081
---

**After every send, Hamlet reports what happened link by link, and names the
link that failed.** This is the question the application exists to answer.

THE PROBLEM, IN THE OPERATOR'S WORDS: "Am I speaking into the void, as in nothing
is going out, or am I speaking on the air and no one is just listening? This is
my frustration for six years. This app is supposed to solve it." He has now
transmitted successfully twice and still does not know which of those happened.
The app watched both and said "nothing called yet", which was true, useless, and
exactly the silence that has been his experience of this hobby since 2020.

**THE INSIGHT THE WHOLE DESIGN RESTS ON: "did anybody hear me" is not one
question. It is a chain of five links, and only the last is about other people.**

1. Hamlet sent the command — CI-V acknowledgement.
2. The radio keyed — `TransmitStatus`, `1C 00`.
3. The amplifier made power — the Po meter, `15 11`.
4. The power went into a real load — SWR, `15 12` (HM-DEC-081).
5. Somebody was listening and copied it — RBN reports for his callsign.

**Four of those five are machine-checkable and none of the four need another
human being to cooperate.** Before tonight the app checked two and reported
neither.

**A FAILURE AT LINK 3 AND A FAILURE AT LINK 5 ARE COMPLETELY DIFFERENT FACTS
ABOUT THE WORLD, and they looked identical to the operator: silence.** One means
his station is broken. The other means his station works and the band was short
or nobody was pointed his way. He cannot act on the first without knowing it is
the first, and telling them apart is the entire product.

LINK 3 IS THE ONE THAT WAS MISSING. `15 11` reads the RF output power meter
(p. 19-3), cited at three points: `0000` is 0%, `0143` is 50%, `0213` is 100%.
It is not the same thing as `14 0A`, which is where the power control is set: a
knob position says nothing about what came out, and **a radio can key,
acknowledge, and produce nothing at all.** Like the SWR meter it means nothing
while receiving, so it is sampled during a send and marked unknown the moment
the transmitter stops, because a resting figure would read as "it made nothing"
when it means "nobody asked it to", and those are opposite conclusions about a
station.

**THE PEAK ACROSS THE SEND IS KEPT, NOT THE FIRST AND NOT THE LAST.** Both meters
settle at key-down, so the first sample is a startup artifact and the last lands
as the transmitter drops. For power the peak is the true output rather than a
ramp; for SWR it is the worst case, which is the number worth telling somebody
about.

**EVERY NUMBER IS MEASURED OR IT IS NOT SHOWN.** §0.0 governs here more tightly
than anywhere else in the application. A link Hamlet could not read says so, and
"Hamlet could not read the power meter, so it cannot say whether anything left
the antenna" is honest and useful. **A link that could not be read is not a
failed link**: not knowing whether power was made is different from knowing none
was, and reporting the first as the second would tell somebody their station is
broken on the strength of a read that did not come back.

**A PERCENTAGE AND NEVER A WATTAGE**, which departs from the brief's example
sentence and is recorded rather than done quietly. The meter reports a position
on its own scale, Icom's meter faces are not linear in watts, and §4 has no
citation for the curve. A figure in watts here would be an invented number
underwriting the one claim this whole feature exists to make (HM-DEC-074).

**AND IT NEVER DIAGNOSES THE STATION.** "Made no power" is a reading. "Your
antenna is disconnected" is a guess about somebody's equipment, and the
prohibition that governs the SWR report governs the whole chain. A test sweeps
every combination of every link for phrasings that would cross it. Hamlet reports
measurements; the operator draws conclusions, because he is the one standing next
to the radio.

LINK 5 GAINED THE NUMBER IT WAS MISSING. "None of them copied you" is worth
nothing without knowing how many "them" there were, and zero skimmers watching a
band is not the same event as forty. The count is of **skimmers that reported
somebody on that band**, which is a lower bound on how many were awake rather
than a census of who was listening: a machine hearing nothing publishes nothing,
so it cannot be counted, and "41 were listening" would claim more than the wire
supports. **A count that could not be obtained says so rather than being
omitted**, because an absent number reads as zero to somebody who has been
disappointed before.

LINKS THAT SUCCEEDED ARE STATED BRIEFLY AND THE FAILED ONE GETS THE WORDS.
Somebody whose station is working does not want a five-line audit every time he
calls. The whole chain persists with the send record, so a later history of
"times you were heard" can be built from what is already on disk, and a null
stays null in the file for the same reason it does on screen.

---
id: HM-DEC-081
date: 2026-08-15
refs: src/Hamlet.RadioEngine/Civ/CivValues.cs, src/Hamlet.RadioEngine/Cw/TransmitNotes.cs, src/Hamlet.RadioEngine/Rig/RigStateMonitor.cs, tests/Hamlet.RadioEngine.Tests/Rig/SwrTests.cs, HM-DEC-050, HM-DEC-074
---

**Hamlet reads the SWR meter during a send and reports what it measured. It never
says what is connected to the antenna socket.**

THE READ, CITED. `15 12` reads the SWR meter level (Full Manual p. 19-3), and
the scale is cited on the same page at four points and nowhere else: `0000` is
1.0, `0048` is 1.5, `0080` is 2.0, `0120` is 3.0. Those numbers are the manual's
decimal column and not hexadecimal, which is the mistake §4 already records twice.
The conversion is linear between the cited points and **refuses past the last
one**: above 3 to 1 the manual says nothing, so Hamlet says "higher than 3 to 1",
which is also everything the operator needs because anything up there wants the
same action. It is a normal field in the rig model with its command and its page,
so the diagnostics screen lists it without anybody adding it there.

**IT ONLY MEANS ANYTHING WHILE TRANSMITTING.** SWR is derived from reflected
power, so a resting radio has nothing to reflect and whatever the meter returns
is not a measurement of now. It is sampled during a send and **marked unknown the
moment the transmitter stops**, so a resting value can never be read as a current
one. That is HM-DEC-050's existing machinery rather than a special case, which is
the whole reason those states exist.

WHAT IT SAYS AFTER A SEND, in the Send panel: the ratio in plain words with the
number, and above 1.5 the manual's own advice to hold TUNER for a second
(p. 11-2). A high reading is worth saying loudly, because power that will not go
out comes back into the radio and the operator is about to key again.

**AND IT NEVER SAYS WHAT IS CONNECTED. This is the line that matters.** A dummy
load reads close to flat, a matched antenna reads under 1.5 and rarely dead flat,
and a disconnected one reads high. That is suggestive and it is not evidence.
"Your antenna is connected" would be a guess dressed as a decode on the one
screen where a wrong answer means somebody keys into the wrong thing (§0.0). A
test sweeps every reading from 0 to 255 for phrasings that would cross that line.

---

**THE NOTICE ABOUT THE BACK OF THE RADIO RETIRES ON EVIDENCE.** HM-DEC-074 wrote
it and it earns its place exactly once, before a first transmission. After that
it is a standing block of orange text above the controls that the operator has
stopped reading, and **a warning nobody reads is worse than none, because it
teaches everything near it to be ignored.**

The retirement condition is a real SWR reading rather than a counter of sends,
which is the better condition and the reason these two rulings are one session's
work: by the time it fires, Hamlet has measured something about what is on the
socket and the operator has read the number, so the sentence has been answered by
evidence rather than merely outlived. Persisted with the profile so it does not
return on restart, and the text stays in the codebase for the day somebody
changes stations.

**Tim asked for the text to go entirely.** It is kept for the first-run case the
onboarding principles care about, and it is now genuinely temporary rather than
permanent. If he wants it gone outright, deleting the one call site does it.

---
id: HM-DEC-080
date: 2026-08-15
refs: src/Hamlet.App/Views/MainWindow.axaml, src/Hamlet.App/ViewModels/CwTransmitViewModel.cs, HM-DEC-012, HM-DEC-079
---

**The send buttons had no style of their own and fell through to the theme's
default, which is grey in every state including the working one. And status
messages in a panel occupy reserved space rather than appearing and
disappearing.**

WHY THIS TOOK FOUR ATTEMPTS TO FIND, WHICH IS THE USEFUL PART. The complaint was
"the buttons look grey" and it was heard three times as a state bug. HM-DEC-079
verified that the disabled style binds only to `Refused`, wrote a test, and the
test passed. **That answered the wrong question.** If nothing is dimming them,
then their normal, un-dimmed, fully-enabled appearance is itself grey, and that
is the bug. The app uses Avalonia's Fluent light theme and had styled the Connect
button and the band buttons and never the send buttons, so they rendered as the
theme's pale neutral. A working button and a refused one looked near enough
identical that the operator could not tell them apart and reasonably assumed the
worse of the two.

**A passing test about style binding was not evidence, and it gave a false pass.**
When a complaint is about appearance, the check is a screenshot.

WHAT THE STATES LOOK LIKE NOW, from the app's own palette (HM-DEC-012): ready is
filled amber, which is what this app already uses for "do this"; armed is the
deeper amber, because a message waiting on its confirming press is the loudest
thing on the panel; sending is the decode green, which reads as working; and only
refused is a pale outline on a dimmed card. That completes HM-DEC-079 rather than
changing it: grey means refused was already the rule and the theme was quietly
contradicting it.

---

**STATUS MESSAGES OCCUPY RESERVED SPACE AND CHANGE THEIR CONTENT, NEVER THEIR
PRESENCE. This is a layout standard rather than one panel's bug fix.**

A message came and went as the transmit line toggled, and every appearance
reflowed everything below it, so the Send panel jumped several times a second at
exactly the moment the operator was watching it hardest. HM-DEC-079's latch
removed the source of that toggling and the rule stands anyway, because the next
fast-changing value will do the same thing to the next panel.

The Send panel now has one status block that is always present with a reserved
height. Its fill, its edge and its words change; its existence does not. The
abort lives inside it for the same reason: a control that appeared beside the
thing it stops would move the thing it stops, at the moment somebody is reaching
for it.

Practical test: does anything on screen move when a value the radio reports
several times a second changes? If so, that element is appearing rather than
changing.

---
id: HM-DEC-079
date: 2026-08-15
supersedes: HM-DEC-059 (the two-press default only)
refs: src/Hamlet.App/ViewModels/CwTransmitViewModel.cs, src/Hamlet.App/Views/MainWindow.axaml, src/Hamlet.App/Telemetry/AppEvents.cs, tests/Hamlet.App.Tests/ViewModels/SendGuardTests.cs, HM-DEC-012, HM-DEC-018, HM-DEC-078
---

**CW transmit works on real hardware. What did not work was the operator being
able to tell that.** Two transmissions went out, eighteen seconds each, and he
did not know at the time. Every minute of the evening it cost came from the send
controls saying something untrue about their own state.

---

**GREY MEANS REFUSED AND NOTHING ELSE. This one is durable and a later session
should not undo it casually.**

Grey has one meaning in every interface anybody has ever used: you cannot press
this. Hamlet was spending it on at least three different things, so it meant
nothing, and the operator correctly stopped trusting it. The disabled appearance
is now reserved for `TransmitReadiness` refusing, and **a refusal always prints
its reason beside the button.**

Armed and sending are active states and are drawn at full strength: armed in the
app's amber because amber is what this app already uses for anything wanting
attention, sending in the decode green because it reads as working (HM-DEC-012).
Both do something, so neither may be dimmed. The style binds to
`LooksRefused` and `Dimmed`, which are true only in the refused state, so a
future state cannot acquire the disabled look by merely being "not ready". A test
asserts it at the view-model level on the properties the style binds to.

---

**THE CONFIRMING PRESS GUARDS WHAT THE OPERATOR WROTE, NOT WHAT HAMLET WROTE.
This one is durable too.**

Every send took two presses, the first armed nothing visibly, and the header read
`2 to send`, which he read as a count of available messages rather than a count
of presses. He pressed, saw nothing happen, and concluded the button was broken.
He built this application and still read it that way, which is the whole
argument: if the author cannot read it, nobody can.

- **Text Hamlet wrote, unedited, sends on one press.** It is on screen in full
  and has already been read, so a confirming press adds nothing. The previewing
  the old toggle existed to force has already happened by the time anybody
  reaches for the button.
- **Text the operator edited takes two.** That is the message nobody has checked
  and the only one worth guarding.
- **Reverting disarms.** Edited-ness is a comparison against Hamlet's original
  rather than a flag that was set once, so somebody who changes his mind and
  deletes back does not face a second press for nothing.
- The armed button **says what the next press will do**, and there is a way back
  out that is not the thing you were unsure about: cancel, and put it back.

**This supersedes HM-DEC-059's default only.** That ruling put the toggle on by
default so somebody could read the words before they went out, and the reasoning
was right. What was wrong was applying it to text Hamlet had already written and
already displayed. The toggle survives as "confirm every send", off by default,
because somebody who wants it on everything should be able to say so. What it may
not do is describe a behavior the app no longer has.

The collapsed summary is rewritten. `2 to send` was technically honest and
completely opaque.

---

**SENDING IS A STATE, NOT A PER-ELEMENT SAMPLE.**

Under full break-in the radio keys element by element, so `TransmitStatus`
toggles every few hundred milliseconds and readiness refused `already
transmitting` dozens of times across one eighteen second call. The controls
flipped enabled and disabled on every dah, and a click landing in a disabled
frame was lost. The latch is the send operation itself: while a message is in
flight readiness is not recomputed at all, the controls hold one state, and
returning to ready wants the message to finish rather than a gap between
elements. The abort that already exists is what stops it (HM-DEC-074).

That also removes the source of the log noise rather than rate-limiting the
symptom: 137 paired events across 37 seconds were the same unchanged state
written twice per Morse element, and there is now nothing to write.

---

**THE SEND IS IN THE RECORD.** Start and finish events carry the character
count, the piece count, the frequency, the mode and the duration, which is what
makes a transmission visible: eighteen seconds is a full CQ at twenty words a
minute, and a send that returned in a tenth of a second never keyed anything.

**THE TEXT ITSELF IS NOT WRITTEN, AND THAT IS A DEPARTURE FROM THE BRIEF THAT
ASKED FOR IT.** A CQ is the operator's own callsign twice over, and HM-DEC-018
forbids a callsign in telemetry without exception, with a test that proves it
cannot happen. The length, the count, the duration, the frequency and the mode
make the transmission fully diagnosable and identify nobody, which is everything
the diagnosis needed and nothing it did not. Recorded here rather than quietly
done, because a brief was overridden.

---

**A BUG THIS WORK INTRODUCED AND ITS OWN TEST CAUGHT.** Making the message
editable meant the rebuild comparison in HM-DEC-078 was comparing the script's
output against the edited text, so typing would have rebuilt the buttons on the
next poll and thrown the operator's words away four times a second. What decides
a rebuild is the script changing its mind, not the operator changing his, so the
comparison is against the original.

**THE BUILD DATE IS STAMPED AT COMPILE TIME.** About read it off the assembly
file's last-write time, which is a property of a file copy rather than of a
build: it showed 2026-08-14 while running code built the next day. It is the row
somebody reads to check that two machines run the same code, so a date that can
be stale is worse than none. It now comes from the compilation and says
"unknown" when it is absent rather than falling back to a timestamp that lies.

---
id: HM-DEC-078
date: 2026-08-15
refs: src/Hamlet.App/ViewModels/CwTransmitViewModel.cs, src/Hamlet.App/ViewModels/MainWindowViewModel.cs, src/Hamlet.App/Telemetry/AppEvents.cs, tests/Hamlet.App.Tests/ViewModels/SendButtonEnablementTests.cs, HM-DEC-059, HM-DEC-077
---

**The send buttons were being destroyed and rebuilt four times a second, and
that is why they were dead.** The gate was right, the notification was raised,
and the control the operator pressed no longer existed when they let go.

WHICH CANDIDATE IT WAS: **the third, in a form the brief did not anticipate.**
Not a bool captured at construction and not a readiness object swapped without
notification. The button *object itself* was the snapshot. `Rebuild` cleared
`Options` and repopulated it on every `Refresh`, `Refresh` runs from
`ApplyRigState`, and `RigStateMonitor` raises `StateChanged` on every poll cycle
whether anything changed or not, at a 250 millisecond live interval. So every
send button was thrown away and constructed again four times a second. A press
and its release have to land on the same control, and that control was gone
inside a quarter of a second, which is exactly the reported symptom: clicking
produced no handler, no event, nothing.

THE OTHER TWO CANDIDATES WERE CHECKED AND CLEARED, rather than fixed
speculatively. Rig state is marshalled at `OnRigStateChanged`, which posts to
the dispatcher, so nothing downstream was notifying from the serial thread. And
`CanSend` is an observable property the panel writes on the UI thread, so the
notification was raised and delivered. The engine, the threading and the
notification were all working; the tree underneath them was being rebuilt.

IT ALSO EXPLAINS A FEATURE THAT COULD NEVER HAVE WORKED. Staged sending is on by
default (HM-DEC-059): compose on the first press, send on the second. `IsStaged`
lives on the button view model, which was replaced four times a second, so the
staging was wiped before anybody could press twice.

THE FIX IS TO REBUILD ONLY WHEN THE OFFER CHANGED, compared by label and by what
would actually go out, which is the same rule the spot list already follows so a
surviving card keeps its identity (HM-DEC-025). Two tests fail against the
unfixed code and pass against the fixed one, and they were run both ways to be
sure of it.

**THE COMMAND CARRIES THE GATE NOW, NOT ONLY THE VISUAL TREE.** A parent whose
enabled state is bound is a picture of the rule, and this evening proved a
picture can be wrong in ways nobody can see: the buttons were rebuilt out from
under it and there was no way to tell a disabled control from a vanished one. A
command with a `CanExecute` refuses however the tree renders, and
`NotifyCanExecuteChangedFor` on `CanSend` and on `IsSending` is what tells the
button to ask again. The parent binding stays, because it is what greys the
control visually.

THE SEAM IS NAMED AND GUARDED. `MainWindowViewModel.OnRigStateChanged` is where
rig state enters the UI and it is the only such place; it posts to the
dispatcher so every consumer downstream is safe without each one remembering.
`ApplyRigState` now also checks and re-posts, so a second caller added later
cannot quietly bypass it. Both carry the note that this path runs four times a
second and everything it reaches must be cheap and idempotent, which is the
lesson rather than the patch.

**THE RECORD NOW CARRIES WHAT THE OPERATOR SAW.** This is the §0.0.1 failure
underneath the whole evening: the log said the engine reached Ready while the
screen showed dead buttons, and nothing anywhere could show that disagreement.
An event describing the engine is a record of half the application, and it was
the half that was working. So the send buttons' own enabled state is written
whenever it changes, beside the readiness verdict that caused it, and **a button
that is off while readiness says it may send is logged as an error**, because
that exact combination is this bug and somebody should find it by scanning.

A DISABLED SEND BUTTON ALWAYS CARRIES ITS REASON, and a test sweeps the states it
can be dead in to prove none of them is silent. The failure was never the
strictness; it was a control that refused and explained nothing.

---
id: HM-DEC-077
date: 2026-08-15
refs: CLAUDE.md §8.1, src/Hamlet.RadioEngine/Telemetry/Outcome.cs, src/Hamlet.RadioEngine/Telemetry/RigSnapshot.cs, src/Hamlet.RadioEngine/Telemetry/DecodeWindow.cs, src/Hamlet.App/ViewModels/DecisionLogViewModel.cs, HM-DEC-018, HM-DEC-050, HM-OPEN-009
---

**The telemetry record becomes a decision record.** Every decision point that can
go more than one way emits an event naming the branch taken and the state that
determined it, and significant events carry the rig state as Hamlet believed it
at that moment. Recorded in §8.1, where the logging rules live.

WHAT PROMPTED IT, BECAUSE THE SHAPE OF THE FAILURE IS THE ARGUMENT. A live on-air
attempt failed with both Call CQ buttons greyed out, the radio connected, tuned
inside the CW segment, break-in on. Nothing on screen said why. Then the record
could not say why either: **144 events across five sessions and not one of them
concerns the failure.** The long session ran an hour and fifty minutes with its
last human action in the first five, and everything after it was the spot timer.
The diagnosis had to come from a photograph of a window. That is §0.0.1 failing
at the one job it has.

THE ORGANIZING FAULT, IN ONE SENTENCE: **Hamlet logged what it did and never what
it decided.** Every event in the file was a completed action, so there was no
event anywhere for a thing Hamlet chose not to do, or tried and failed at. A
disabled button fires no handler, so nothing was written, so the record cannot
distinguish "Hamlet refused" from "Hamlet is broken" from "nobody pressed it".

A REFUSAL IS AN OUTCOME AND A FAILURE IS AN OUTCOME. Both are as loggable as
success and more useful, because success is the case nobody ever has to diagnose.
So the vocabulary gained its negatives, and every outcome event carries the same
three things: `outcome`, a `reason` that is a **stable machine token rather than
a display string**, and `determinedBy` naming the values that decided it with
their provenance and age.

**UNKNOWN AND OFF ARE NOW DIFFERENT ALL THE WAY DOWN**, and this was the sharpest
finding. `BreakInOff` covered both, so one verdict and one sentence served a
setting nobody had read and a setting the operator could walk across the room and
switch on. Refusing on unknown is correct (HM-DEC-050) and refusing on off calls
for something completely different. They are now separate states, separate
tokens, separate sentences on screen, and separate provenance in the file, and
`ModeUnknown` was split from `NotInMorse` for the same reason.

THE RIG STATE TRAVELS. Thirty-one values were held and not one appeared in
telemetry, which is why break-in could only be learned by photographing a window.
A full snapshot goes on every connect, every readiness evaluation and every
decoder transition; a delta goes on a one-minute heartbeat so a quiet session
still has a spine without thirty-one rows a minute burying what is worth finding.
Ageing is not a change.

LEVELS START MEANING SOMETHING. All 144 events were `info`, so nothing could be
found by scanning and a second connect firing thirteen seconds after the first
was logged identically to a healthy one. A refusal is a warning, a failure is an
error, and a reconnect nobody asked for is a warning however well it went.

THE DECODER EXPLAINS ITSELF, aggregated over an interval rather than per
character: counts by confidence, rejections by reason, noise floor, tracked pitch
and its drift. It already computed every one of these and nothing asked. The hot
path allocates nothing and a test proves it across five hundred thousand calls,
because a decoder that stutters to write its diagnostics has traded the thing for
the record of the thing (§8).

THE REASON REACHES THE OPERATOR, NOT ONLY THE FILE. A file somebody has to upload
is the second line of defense and the screen is the first. The Send panel says
the reason beside the disabled control in three different sentences for three
different situations, and a "What Hamlet decided" window sits beside "What the
radio is doing" with the same copy button. That window answers what Hamlet did
about the radio; the other answers what the radio is doing.

**WHAT THE INSTRUMENTATION ESTABLISHED ABOUT THE FAILURE, AND WHAT IT DID NOT.**
The gate is not wrong about the reported state: a test drives tonight's exact
reading, break-in full, transmitting false, mode CW, and it produces a ready
verdict. Two of the three candidates were also narrowed by reading the code
rather than guessing at it. Readiness recomputes on every rig state change, so
"evaluated once at connect and never again" would require the state event itself
not to fire. And readiness and the diagnostics window read the same live
property, so they cannot be reading different sources. **What is left is not
determined and is deliberately not guessed at**: the next file will separate it,
because the readiness event now carries every precondition it looked at with its
provenance and age, including the transmit-status read that is checked before
mode and break-in and refuses ahead of both. HM-OPEN-009 is updated rather than
closed.

HM-DEC-018 HOLDS WITHOUT EXCEPTION, and this expands what is logged more than
anything before it, so the boundary is proved rather than assumed. The payload
shapes have nowhere to put a callsign, a location or decoded text, which is
stronger than every call site remembering. The privacy walk grew to cover all
five new events with a full profile loaded.

---
id: HM-DEC-076
date: 2026-08-15
refs: src/Hamlet.RadioEngine/Cw/ContactTracker.cs, tests/Hamlet.RadioEngine.Tests/Cw/ContactTrackerTests.cs, HM-DEC-043, HM-DEC-073
---

**Hamlet follows where a contact has got to, and says when it has lost the
thread.** The model only, with no interface on it.

THE LOST STATE WAS DESIGNED FIRST AND IS THE DEFAULT. A guide that silently keeps
guessing after it stopped following is far worse than one that admits it: the
first sends somebody confidently to the wrong part of a ritual they have never
performed, and the second hands them back the only thing that was ever reliable,
which is what the radio is actually hearing. So every path returns to lost when
evidence runs out, and lost is where it starts (§0.0).

WHAT IT WILL MOVE ON, AND NOTHING ELSE. What the operator sent, which Hamlet
knows exactly because it sent it. A callsign the decoder resolved cleanly, which
means the ritual position and every character solid (HM-DEC-073). A whole ritual
word, solid, in the same decode. Nothing infers a stage from the passage of time,
from a partial decode, or from what usually happens next.

THE ONE TRANSITION IT CAN BE CERTAIN OF is his own callsign in the addressed
position with a clean callsign after the `DE`. That is somebody coming back to
him by name, and it is the moment this operator has been waiting six years for.
Somebody answering a different station moves nothing, which is the false positive
that would hurt most.

EVIDENCE GOES STALE AT FOUR MINUTES. A Morse exchange has long gaps in it, so a
short window would call itself lost in the middle of an ordinary contact; much
longer and Hamlet would still claim to follow a contact that ended while somebody
made tea. Sitting on a stale stage is exactly the failure this exists to avoid.

A HALF-READ WORD MOVES NOTHING, for the same reason a half-read callsign does
not. A dimmed `73` is also a dimmed anything else, and ending a contact that was
still going on one would be the worst version of this being wrong. The two rules
compose rather than each having its own idea of what counts as heard, and a
callsign sent twice with one clean copy still resolves, because that repeat
exists precisely so the first can be half-missed.

**NO INTERFACE, DELIBERATELY.** The brief that will design what sits on top of
this has not been written. Building a surface now would prejudge it, and the
model is proved by its tests rather than by a screen. Nothing in the application
reads this yet.

---
id: HM-DEC-075
date: 2026-08-15
closes: FG-008
refs: src/Hamlet.RadioEngine/Explore/HeardWatch.cs, src/Hamlet.RadioEngine/Explore/SqliteSpotStore.cs, tests/Hamlet.RadioEngine.Tests/Explore/HeardWatchTests.cs, tests/Hamlet.App.Tests/Telemetry/HeardPrivacyTests.cs, HM-DEC-018, HM-DEC-038
---

**Hamlet watches the skimmer network for the operator's own callsign and tells
him who heard him, whether or not a person answers.** Closes FG-008.

WHY THIS IS THE PAYOFF. He has been licensed six years and made one contact. He
will call CQ, and perhaps nobody will answer. The Reverse Beacon Network is a
mesh of automated receivers publishing every callsign they hear, and Hamlet is
already reading that feed, so real machines can say his signal arrived somewhere.
For somebody who has heard nothing back for six years, that is the first honest
answer he has ever had to "did that work".

**NEVER MANUFACTURE THE FEELING, ONLY REPORT THE FACT.** Hamlet says he was heard
because receivers really heard him. It does not inflate, does not round up, and
does not soften a silence into something warmer than the truth. The moment this
becomes encouragement rather than evidence it is worth nothing, and it takes the
trust that makes the rest of the application useful with it (§0.0).

THREE STATES AND THE WAIT IS NOT A SPINNER. **Waiting** says what it is watching
for and what is normal: reports usually take a minute or two, and a person takes
longer because they have to finish listening first. That sentence is the whole
point of the state. Thirty to ninety seconds of silence after a first call is
exactly where a beginner decides it is not working and goes and does something
else, and the window runs ten minutes so an ordinary delay is never turned into a
verdict. **Heard** names the receivers, counts machines rather than reports, and
names the strongest rather than averaging, because an average describes a signal
nobody received. **Nothing** says so plainly and says what it does and does not
mean: skimmer coverage is uneven and a band can be wide open to people and empty
of machines, so no report is not proof nobody heard him. A test sweeps that state
for consolation phrasing, because it must read as information.

THE SPEED A RECEIVER READ IS OFFERED, and it is worth more than it looks. A
machine that timed his characters read them cleanly, which is the first feedback
on his sending he has ever had.

**WHAT THIS RULING DOES NOT BUILD, AND WHY.** The brief asked for distance to
lead: "your signal reached Nevada, 2,050 miles" rather than "19 dB". Hamlet
cannot say that today and must not pretend to. RBN publishes the skimmer's
callsign and no location, and HM-DEC-038 rules in as many words that no grid
means no distance anywhere, naming the skimmer-prefix guess specifically: "a
callsign says where a license was issued and not where its owner is standing, and
stacking that guess under a figure in miles would dress it as a measurement."
Inventing distances to reach for the feeling would break the same rule this
feature's own honesty line sets. **So the reports carry what RBN actually states
and the distance half waits on a cited source of skimmer locations under
`data/`.** Raised as HM-OPEN-010, and it is the first thing to do to this panel.

THE REPORTS ARE KEPT FROM THE FIRST ONE. They go into the existing database
beside the spots, keyed so one report arriving twice is not two times he was
heard. The screen for "times you were heard" comes later; a record that only
started when somebody built that screen would have missed the first one, which is
the one that matters most.

STARTING A WATCH CLEARS THE LAST ONE'S ANSWERS, because the question is whether
anybody heard THIS call. Leaving them up would tell him he had been heard when
nothing had come back, which is the feature inflating a silence and the one thing
it may never do. Only a confirmed send starts the watch: watching for reports of
something that never left would be Hamlet inventing the wait.

A NORMAL COLLAPSIBLE PANEL (§0.5) with its summary in the header. The larger
treatment of this moment belongs to an interface rework that has not happened,
and building a takeover now would prejudge it.

READS ONLY, and the privacy rule is proved rather than asserted. This feature is
built entirely out of callsigns, which makes it exactly where HM-DEC-018 would be
broken by accident. `AppEvents` cannot be handed a report, a summary or a state,
so no future event can carry a callsign into telemetry without the type system
objecting first.

---
id: HM-DEC-074
date: 2026-08-15
refs: src/Hamlet.RadioEngine/Cw/TransmitNotes.cs, src/Hamlet.App/ViewModels/CwTransmitViewModel.cs, tests/Hamlet.RadioEngine.Tests/Cw/LiveFireTests.cs, HM-DEC-008, HM-DEC-049, HM-DEC-059
---

**The transmit path is hardened for a real antenna.** No new transmit features,
and nothing here weakens §0.2.

THE PRECONDITION GATES THE BUTTONS RATHER THAN SITTING BESIDE THEM. Break-in
being off is not a permission Hamlet is withholding, it is a fact about the
radio: command `17` goes out, the acknowledgement comes back, and no signal
leaves the antenna (Full Manual footnote 2, p. 19-7). Somebody making the second
contact of his life would read that silence as nobody wanting to talk to him. So
a control that cannot reach the air says why instead of inviting a press, and the
message names the setting. An unread break-in setting refuses on the same terms:
not having looked is not permission, and "I do not know whether this will go out"
is a different answer from "it will".

THE ABORT IS PROVED AGAINST A SEND THAT IS ACTUALLY RUNNING. It was always
same-thread and await-free by construction; what it did not have was a test that
held a send open and stopped it mid-flight. It has one now, along with proof that
aborting when nothing is sending and aborting twice are both safe. An abort that
could throw is one nobody can rely on at the moment they need it most.

HONEST FAILURE WAS ALREADY RIGHT AND IS NOW HELD BY A TEST. Only an
acknowledged send reports as sent; a radio that did not confirm produces an
unknown that says so. Success is never inferred from the absence of an error, and
every outcome the sender can produce is swept to prove it.

**THE DUMMY LOAD WARNING IS RETIRED (amending HM-DEC-008 in practice, not in
principle).** It said to key into a dummy load because the keying code had never
run. It has now, and the test passed. Leaving the warning up would be the app
telling somebody something it no longer believes, and a warning nobody needs is a
warning everybody learns to read past. HM-DEC-008's rule is unchanged for the
next untested thing that keys; it has simply been satisfied for this one.

WHAT REPLACES IT DOES NOT PRETEND TO KNOW. Nothing in the CI-V read table reports
what is on the antenna socket, and the SWR meter only says anything while
transmitting, so Hamlet says once and calmly that it cannot see the back of the
radio and that the operator is the one who knows which he is on. That is the
whole line, and it is not a caution.

POWER IS SAID AS A CONSEQUENCE, the same treatment the noise controls got
(HM-DEC-050). Below a quarter of the radio's range it says what that means for a
call, because the specific failure this exists for is somebody turning the power
down for a dummy load test, connecting an antenna, and being unable to work out
why the band has gone quiet. Nothing is said in the middle of the range, because
a line that always appears is a line nobody reads, and nothing at all is said
from a power that was never read.

**A PERCENTAGE AND NEVER A WATTAGE.** The radio reports power as a position on
its own scale, and turning that into watts needs a power curve §4 has no citation
for. A figure in watts would be Hamlet inventing a number on the one screen where
a number decides whether somebody keys a transmitter (§0.0).

ONE THING DELIBERATELY NOT CHANGED, and it is recorded rather than fixed.
Footnote 2 allows three ways for `17` to reach the air: break-in on, TRANSMIT on,
or an external TX switch on. `TransmitReadiness` refuses while the radio reports
it is already transmitting, so an operator holding TRANSMIT down cannot send
through Hamlet even though the manual says it would work. The refusal is the
conservative direction, break-in is the ordinary path and the panel now names it,
and loosening a transmit precondition hours before a live contact is not a change
worth making. Raised as HM-OPEN-009.

---
id: HM-DEC-073
date: 2026-08-15
refs: src/Hamlet.RadioEngine/Cw/CallsignResolver.cs, src/Hamlet.RadioEngine/Explore/RecentStation.cs, tests/Hamlet.RadioEngine.Tests/Cw/CallsignResolverTests.cs, HM-DEC-048, HM-DEC-072
---

**Hamlet reads callsigns off the air, and refuses to nearly read one.** A claim
needs two things and both are required: the right structural position, and every
character solid.

WHY THIS IS A PRIME-DIRECTIVE PROBLEM RATHER THAN A FEATURE (§0.0). The decoder
already marks per-character confidence: solid where sure, dimmed where not,
blocked where unresolved (HM-DEC-048). A callsign extracted from text carrying a
dimmed or blocked character is a guess wearing the costume of an identification.
`KC3QIS` with one uncertain character is also a plausible reading of other real
callsigns belonging to other people. A wrong callsign in front of the operator is
worse than no callsign, and worse still on the day he uses it to decide whether
somebody answered him.

STRUCTURE. A claim is made only where the ritual puts a callsign. The token after
`DE` is the station transmitting. The token before `DE` is who they are calling,
which is the whole of how Hamlet can tell that somebody is answering this
operator rather than calling anybody. The token immediately before a closing
prosign is the station signing, since nobody puts anything else there. It reads
the ritual the app already models rather than building a second description of
it. **A callsign-shaped string in loose text is not claimed**, however convincing
it looks, because the shape of a callsign is also the shape of a signal report
with a letter in it and half the abbreviations in Morse.

CLEANLINESS. Every character of the token must have come back high confidence.
One dimmed character or one block and nothing is claimed. **No partial claim, no
most-likely completion, no confidence-marked callsign**, because a callsign shown
as uncertain still gets read as fact and acted on. A dimmed character elsewhere
in the transmission does not stop a clean callsign being claimed: the rule is
about the callsign and not about the noise around it, and refusing on any noise
anywhere would make this useless on exactly the signals it exists for.

EVERYTHING ELSE STAYS VISIBLE. The terminal shows all of it as decoded text with
its existing marking. Nothing is hidden and nothing is asserted, and saying
nothing about a transmission is the ordinary answer rather than a failure.

PROVENANCE IS HALF OF WHAT MAKES IT A FACT, so it is inseparable from the name.
A callsign Hamlet read off the air, here, now, every character solid, and one a
spot feed reported minutes ago about a frequency that may since have changed
hands are different facts with different reliability. `RecentStation.IsIdentified`
is false unless the source is known, so a name whose origin Hamlet cannot state
is not shown at all and no surface downstream has to remember to check. Both
surfaces that show a station show where it came from. Where the decoder and a
feed both have an answer, the decoder wins, because it is the one that actually
heard it.

A PROFILE WRITTEN BEFORE THIS reads a name with no recorded source as a spot
feed, because that was the only way a name could get into the file at the time.
That is a fact about the history of the file rather than a guess about the entry.

RECEIVE ONLY. Nothing here touches the transmit path.

---
id: HM-DEC-072
date: 2026-08-14
refs: src/Hamlet.RadioEngine/Explore/RecentStation.cs, src/Hamlet.App/ViewModels/TuneMenuItem.cs, src/Hamlet.App/ViewModels/FavoritesViewModel.cs, tests/Hamlet.RadioEngine.Tests/Explore/RecentStationTests.cs, HM-DEC-060, HM-DEC-070
---

**Hamlet remembers where the operator has been, so he can go back without the
number.** Ten places, most recent first, beside favorites and behaving like them.

THE SIBLING OF FAVORITES, AND BUILT AS ONE. A favorite is a place he chose; this
is a place he was. Both carry the context the map already knows, both tune on a
click, both live on the panel strip and in the Radio menu, and an entry here can
be starred into a favorite. That last part is how most favorites will actually be
born: somebody was somewhere good, did not think to save it, and realizes the
following evening that he wants it.

DWELL, NOT LANDING, AND THE THRESHOLD IS **TWENTY SECONDS**. The dial is a scroll
wheel, so a literal history would fill with near-identical entries between 7.029
and 7.031 and be useless inside a minute. An entry appears only once he has
stayed put. The figure comes from Morse rather than from roundness: hunting
across a band no frequency holds the dial more than a second or two, while
deciding whether a signal is worth staying for takes about one CQ call, and a
full "CQ CQ CQ DE W1AW W1AW W1AW K" at a relaxed thirteen words a minute runs
close to twenty-five seconds (HM-DEC-066). Twenty sits just inside one call:
long enough that passing through never counts, short enough that hearing
somebody out always does. One named place in the engine, and **not a setting** —
it is a judgment about what counts as stopping, and a slider would ask the
operator to make it before he has any way to know.

SAME PLACE MEANS **WITHIN TWO HUNDRED HERTZ**, which is the width the app already
calls one signal (`SpotIdentity.FrequencyBucketHz`), read from there rather than
chosen again so two numbers meaning "near enough" cannot drift apart. It is a
tolerance and not a bucket: dividing into buckets puts an invisible boundary
every two hundred hertz, so 7.030.150 and 7.030.250 would be separate while
7.030.010 and 7.030.190 merged, which is unpredictable in exactly the way that
makes somebody stop trusting a list. The tradeoff is stated rather than hidden:
on Morse two notes that far apart are usually two stations, so a wide tolerance
can fold two visits into one entry. That costs the older entry; the alternative
costs the whole list to near-duplicates, which is the failure this exists to
avoid.

NAMED WHERE HAMLET KNOWS, A PLACE WHERE IT DOES NOT. An entry carries a callsign
only where something identified one: arriving by clicking a spot card counts,
because the operator acted on a report of that station. Scroll-wheeling onto a
frequency a spot happens to sit near does not, because nothing was checked and an
entry that named a station then would be asserting a presence out of proximity
(§0.0). Everywhere else the entry is the frequency and what the map says lives
there, which is exactly what a favorite says when nobody typed a name. The
decoder resolves no callsigns today, so the card is the only source; the seam is
there for the day it does.

AND THE NEWEST VISIT'S IDENTIFICATION WINS, INCLUDING WHEN IT IS EMPTY. If Hamlet
knew a callsign the first time and knows nothing this time, the entry stops
carrying it. Keeping it would say that station is there now and nothing checked.
The place survives either way, which is what he is actually navigating by.

TEN, against favorites' ninety-nine, and the difference is the point. Favorites
are a library somebody curates. This is the last few places he was, and a list
long enough to need scrolling has stopped answering "where was I just now".

PERSISTED, because the moment it matters most is the following evening thinking
"where was that station", and a list that emptied on exit would fail exactly
then.

A GAP FOUND WHILE BUILDING IT: **HM-DEC-060's Favorites submenu was ruled and
never built.** Nothing in the menu ever invoked `ManageFavoritesCommand`, so the
manage window had no way in at all and had been unreachable since it was written.
Both submenus are there now, with the manage window under them.

AND ONE THING THAT WOULD HAVE SHIPPED BROKEN AND SILENT. A menu opens in its own
popup, and a popup is a separate visual tree, so a submenu item whose command
binds up to the window resolves to nothing: it compiles, it renders correctly,
and it does nothing when clicked. So each line carries its own command
(`TuneMenuItem`), which also makes the menu testable without a window.

---
id: HM-DEC-071
date: 2026-08-14
refs: CLAUDE.md §4, src/Hamlet.RadioEngine/Civ/CivReads.cs, src/Hamlet.RadioEngine/Civ/CivValues.cs, tests/Hamlet.RadioEngine.Tests/Civ/CitationTests.cs, HM-DEC-049, HM-DEC-050, HM-DEC-067, HM-DEC-069
---

**One edition of the truth. Every citation in §4 is re-verified against the IC-7300
Full Manual, publication `A7292-4EX-6`, and the edition is now part of the
citation.**

WHY THIS WAS WORTH A SESSION. §4 had come to span three printings, each block
naming its own, so nothing in it was dishonest. Page numbers drift between
printings, and that seam had already produced two defects: the `14 08`
sub-command error HM-DEC-050 corrected, and its copy in `AppSettings` that
survived undetected for weeks. A table that is right in three different books is
a table nobody can check in one sitting.

THE EDITION, AND WHY THIS ONE. `A7292-4EX-6`, © 2016–2018, from Icom UK. It is
the newest full manual obtainable: there is no v7 or later at that source, and
Icom America publishes only the Basic Manual. It supersedes the `A7292-4EX-5`
printing two recent rulings read.

READ COLUMN-AWARE, WHICH IS NOT A DETAIL. `pdftotext -table`, because the command
table is two columns and a flattened read is what put the CW pitch against the
wrong row. Every attribution was made from a page-footer map, and that map was
then checked independently against the manual's own index, which agrees.

SIX PAGE NUMBERS MOVED AND EVERY VALUE HELD. The radio address to 12-8, the CI-V
USB baud rate to 12-9, the three command `17` rows to 19-11, command `04`'s data
content to 19-8, footnote 2 to 19-7, and `1A 03` to 19-4. **Two rows cited a page
19-14 that does not exist in this edition**, whose chapter 19 ends at 19-13; both
were duplicates of scope rows already read correctly, so they merged. The old
`00`–`A0` data range and the current `0`–`160` are one range in two bases, and the
manual writes decimal.

ONE ROW WAS SIMPLY WRONG, and the code was the thing that caught it. §4 said the
filter-width scale is on p. 4-6 "and not in the command table". The command table
carries the endpoints; only the steps need p. 4-6. `CivFilterWidth`'s own comment
had said exactly that for weeks, which is the argument for putting reasoning next
to code rather than only in a table.

ONE ROW GAINED A CLAUSE THAT MATTERS. Command `26`'s skippable bytes were recorded
as "skipping the filter selects that mode's default". The manual says both may be
skipped and that **DATA OFF** and the default filter are then selected. A `26`
without the data byte turns the data variant off rather than leaving it alone.
`CivWrites` already sends the byte and already quoted the sentence, so nothing was
broken; the summary was.

A NEW KNOWN-UNKNOWN CLOSED. The frequency BCD encoding was the last figure in §4
carried from general knowledge rather than from a source. It is on p. 19-8: five
bytes, least significant pair first, two BCD digits per byte with the more
significant in the high nibble. `Bcd.DecodeFrequencyHz` matches it exactly. §4's
"still unverified" list is now two entries, both of which are configuration rather
than constants.

TWO NOTATIONS, WRITTEN DOWN BECAUSE CONFUSING THEM IS A REAL HAZARD. Where §4
writes `01 28`, that is BCD on the wire; the manual writes the same value as
decimal `0128` in its own column. Reading `02 55` as hexadecimal gives 597 rather
than 255. Both forms now appear together where they occur.

A SECOND TYPO IN THE MANUAL, alongside the `27 20` one HM-DEC-062 recorded and
which is present in this edition too. Page 19-12 refers the reader to page 19-14
for the Scope Fixed edge frequency settings, and chapter 19 ends at 19-13; the
settings are on 19-13.

RULINGS WERE NOT EDITED. HM-DEC-049, HM-DEC-050, HM-DEC-067 and HM-DEC-069 carry
page numbers from the printings they were written against, and each now carries a
dated correction note beside the passage, in the treatment §4 already gives the
`14 08` error (§1). HM-DEC-069's conclusion was re-checked in full against this
edition and every page in it holds.

A TEST PINS THE EDITION so the next drift is loud rather than quiet. It reads the
citation strings the engine actually carries and fails on any page outside the
chapter ranges this edition has, which is what would have caught a 19-14 the day
it was written.

---
id: HM-DEC-070
date: 2026-08-14
supersedes: HM-DEC-060 (the star's placement and its label only)
refs: src/Hamlet.App/Controls/RigDisplayControl.cs, src/Hamlet.App/Views/MainWindow.axaml, src/Hamlet.RadioEngine/Explore/Favorite.cs, tests/Hamlet.RadioEngine.Tests/Explore/FavoriteTests.cs
---

**The star lives inside the display, in the black, and it says what pressing it
does rather than what the favorite is called.** A strip along the top of the warm
panel carries the name at its left and the dropdown at its right, and the tuning
hint gets its own uncrowded line back.

WHAT THIS SUPERSEDES, AND WHAT IT LEAVES ALONE. HM-DEC-060 put the controls on the
warm panel below the LCD and never inside it, reasoning that the black is a
faithful picture of the IC-7300's own face and a control the real radio does not
have would blur which is which. That reasoning was sound and Tim has weighed it
against being able to find the thing, and chosen being able to find it: a star
against near-black is the brightest object on the panel. **The placement and the
label are superseded. Everything else in HM-DEC-060 stands** — saving still
captures frequency, mode, band and neighborhood with nothing typed, the Radio menu
still has its submenu and its manage window, and the list still survives a
restart.

THE LABEL SHRANK FOR A REASON THAT ONLY APPEARED ONCE IT WAS BUILT. HM-DEC-060 had
the star carry the favorite's name, which reads beautifully and costs more width
than the display has. A name is as long as whoever typed it, and the LCD has a
mode badge at one end and a UTC clock at the other, so a long name collides with
one or the other at some window width. So the star says `save` or `saved`, one
word each way, and a test holds that neither ever grows a space in it.

TWO STATES, ONE CONTROL. Hollow star and `save` where nothing is saved, solid star
and `saved` where something is, and pressing it on a saved frequency un-saves. It
is a toggle in both directions rather than two controls that could disagree about
which one is showing.

THE NAME STILL APPEARS, WHERE THERE IS ROOM FOR IT. The strip's left end shows the
favorite the dial is sitting on and shows nothing at all elsewhere, which is
quieter than a line saying nothing. It does two jobs: it confirms which favorite
you landed on when you arrive from the dropdown, and it confirms what a save was
just named in the moment after you press the star.

THE DRAWING OWNS THE HIT TEST. The star's rectangle is recorded where the glyph
was actually drawn rather than computed a second time, since the strip's contents
move with the mode badge beside them and two calculations of one position drift.
The target is padded outward, because a target the size of a glyph is a target
somebody misses, the pointer turns to a hand over it so the one pressable thing on
a tunable surface says so before the click, and the wheel does not tune while the
pointer is on it.

THE WORD IS DROPPED BEFORE IT WOULD OVERLAP THE CLOCK, which the display's minimum
width already prevents. A layout that is only correct because of a constant
somewhere else is one refactor away from being wrong.

---
id: HM-DEC-069
date: 2026-08-14
refs: src/Hamlet.RadioEngine/Explore/ModeGuide.cs, src/Hamlet.App/ViewModels/MainWindowViewModel.cs, tests/Hamlet.RadioEngine.Tests/Explore/RttyConstraintTests.cs, HM-OPEN-008, HM-DEC-054
---

**Hamlet does not read the radio's RTTY decoder, and the reason is a constraint
in the radio rather than a gap in the app.** The IC-7300 decodes RTTY internally
and will send the decoded text out the USB port. That is the same port CI-V uses,
and one setting chooses which of the two it carries. Taking the decoded text
costs rig control entirely.

VERIFIED, NOT ASSUMED. IC-7300 Full Manual, publication **A7292-4EX-5**, 173
pages, read column-aware with `pdftotext -table`. Page **12-9**, under MENU then
SET then Connectors:

> USB Serial Function (Default: CI-V) — Selects the signal output from [USB].
> CI-V: A CI-V command is output. RTTY Decode: An RTTY decoded signal is output.

One setting, two options, one port. Page **12-9** also gives RTTY Decode Baud
Rate (Default: 9600), options 4800, 9600, 19200 or 38400 bps. Page **2-5**
carries the same fact as a tip beside the USB connection drawing, and page **2-3**
describes the [USB] port itself, listing remote control by CI-V and sending the
decoded RTTY output as separate bullets on the one connector. Three statements
and no contradiction between them: **the conflict is real and the manual is not
ambiguous about it.**

> **NOTE 2026-08-14 (HM-DEC-071): re-checked against `A7292-4EX-6`, the edition
> this project has settled on, and every page above holds.** USB Serial Function
> and RTTY Decode Baud Rate are on 12-9 there too, the [USB] port description on
> 2-3, the RTTY tip on 2-5. The conclusion is unchanged and now rests on the
> newer printing as well.

SO THE MODE IS NOT BUILT, AND THAT IS THE ANSWER RATHER THAN A DEFERRAL. An RTTY
terminal fed from that port would be an application that stops following the
radio the moment it starts working. Every frame Hamlet sent would still be
correct and answered by nothing, the frequency would freeze at whatever it last
was, the waterfall would empty, and all of it would look like a fault in Hamlet.
That is the prime directive broken in the worst available way, because everything
on screen would keep looking right (§0.0).

A SECOND REASON, AND ON ITS OWN IT WOULD BE ENOUGH. **The manual never states
what the decoded output looks like on the wire.** It says an RTTY decoded signal
is output and gives the baud rate, and it does not say whether the bytes are
ASCII, what marks a line ending, or how the decode screen's characters map onto
them. Code that guessed would be presenting an interpretation as a decode, which
§4 forbids by name. Recorded as HM-OPEN-008 rather than filled in with something
plausible.

WHAT WAS BUILT INSTEAD is the honest half: the field guide's RTTY entry now says
the radio decodes this one by itself, that it will send the text down the cable,
and that one setting governs the port so choosing it means losing the radio for
as long as it runs. The choice is the operator's and it is made at the radio's own
screen. Hamlet does not offer to make it, and could not undo it if it did, since
the switch that severs CI-V cannot be reached over CI-V afterward.

AND ONE DIAGNOSTIC, WHICH IS THE PART THAT EARNS ITS KEEP TODAY (§0.0.1). A radio
left on RTTY Decode answers nothing, which looks exactly like a bad cable. The
connect failure now names that possibility beside the cable, the baud rate and the
CI-V address, so nobody spends an evening on a lead that was never the problem.

The digital neighborhoods already know where RTTY lives (HM-DEC-054), and that
does not change. Knowing where a mode lives and being able to read it are
different things, and the map has never claimed the second.

---
id: HM-DEC-068
date: 2026-08-14
refs: src/Hamlet.RadioEngine/Explore/CardText.cs, src/Hamlet.App/ViewModels/MainWindowViewModel.cs, src/Hamlet.App/ViewModels/LeadCard.cs, tests/Hamlet.App.Tests/ViewModels/CardRepetitionTests.cs, HM-DEC-025, HM-DEC-045
---

**A card's lines are composed together, and a clause an earlier line carried is
dropped from a later one.** No card may say the same thing twice.

THE BUG THAT WAS FOUND, AND THE ONE THAT WAS FIXED ARE NOT THE SAME. On a park
activation the ranked reason ended with "activators stay a while, so they are
probably still there" and the gray line underneath said it again, word for word.
Neither line is wrong: the ranking explains why the card is where it is, the line
under it says mode, source, age and distance, and both ask
`SpotLifetime.DescribeOpportunity` for the same sentence because both of them
should. Fixing that card would have left the next one to be found by somebody
reading the screen, which is how this one was found. So the composition is the
fix, and it holds for whatever the pieces decide to say next.

WHY IT MATTERS BEYOND TIDINESS. A thing said twice reads as two pieces of
evidence when it is one, which is a confidence the input does not justify (§0.0).
It also reads as a program that is not paying attention, which is a poor thing
for an application asking somebody to trust it about what is on the air.

THE UNIT IS A CLAUSE. Phrases split on the card's own separator and then on
commas, so "an hour ago, and activators stay a while, so they are probably still
there" is three clauses and a second line can keep the age while losing the part
already read. Case, trailing punctuation and a leading "and" or "so" are noise
and are normalized away. Word-level matching would gut ordinary English.

THE FIRST LINE ALWAYS SURVIVES WHOLE. It carries why the card is on screen at
all, and thinning it from something written underneath would be the tail wagging
the dog (HM-DEC-025).

TWO FAMILIES COMPOSE THROUGH IT TODAY: the happening-now spot cards and the lead
card, whose headline, body and evidence line are written by three pieces of code
that cannot see one another. A new card family joins by calling `CardText.Compose`
rather than by being remembered.

THE TEST IS THE CLASS AND NOT THE INSTANCE. It sweeps every source, call type,
mode, age and activation combination through both families and fails on any
repeated clause, and it was checked against the unfixed code to be sure it fires:
it catches the activation sentence and a duplicated mode name that nobody had
noticed. A fourth case proves the check itself can fail, since a sweep that never
fires proves nothing.

---
id: HM-DEC-067
date: 2026-08-14
narrows: HM-DEC-050
refs: src/Hamlet.RadioEngine/Rig/ScopeReadiness.cs, src/Hamlet.App/ViewModels/MainWindowViewModel.cs, tests/Hamlet.RadioEngine.Tests/Rig/ScopeStreamTests.cs, HM-DEC-062
---

**The waterfall says why it is empty, and names the two menus that control it.**
Where no waveform data has arrived, it says the radio is not sending any, and it
names the settings as the radio names them and the path to reach them.

THIS NARROWS HM-DEC-050 AND THE NARROWING IS THE POINT. That ruling's
"consequences, never instructions" is about settings Hamlet reads and judges: it
reports that the filter is narrow and does not tell anybody to widen it, because
the radio is theirs and a program that starts issuing corrections has stopped
being an instrument. That scope does not reach a feature the operator asked for
which cannot work at all until a switch only they can reach is thrown. Neither of
these two is a command. No amount of code makes the stream arrive, and an empty
waterfall that says nothing reads as a broken program while the answer is a pair
of menu screens away. So the exception is narrow and stated: Hamlet may name a
menu when a feature is inert without it, and it may not otherwise tell anybody
how to set their radio.

THE CASE THAT WAS MISSING IS THE ONE SOMEBODY STARES AT. HM-DEC-062 already said
which of the two settings read as off. What it did not cover was both of them
reading as on with the waterfall staying blank, which is exactly the state that
looks like a bug in Hamlet. Now zero sweeps with everything switched on is its
own answer, and one arriving sweep stops it being said.

VERIFIED COLUMN-AWARE, and the edition is named because it is not the one earlier
rulings read. IC-7300 Full Manual, publication **A7292-4EX-5**, 173 pages, read
with `pdftotext -table`. Page **12-9** carries both settings under `MENU` then
`SET` then `Connectors`: "CI-V USB Port (Default: Link to [REMOTE])" and "CI-V
USB Baud Rate (Default: Auto)". Page **19-7** footnote 4 is the precondition, and
it reads the same in this edition as HM-DEC-062 recorded from the other.

ONE AMBIGUITY, RECORDED RATHER THAN SMOOTHED OVER. Footnote 4 names the "CI-V
Baud Rate" screen, and the radio has two: CI-V Baud Rate for the [REMOTE] jack
and CI-V USB Baud Rate for the USB port. Hamlet is on the USB port, so the USB
one is what the app names. 115200 is also the rate Hamlet already talks at, so
setting it costs the connection nothing.

> **RESOLVED 2026-08-14: Tim confirms the USB screen is the one that gates it,
> because Hamlet talks to the radio over the USB cable.** The ambiguity note is
> gone from the code and the reading above stands.
>
> **CORRECTION 2026-08-14 (HM-DEC-071): one page number above is not this
> project's edition.** Against `A7292-4EX-6`, CI-V USB Port is on **12-8** and
> CI-V USB Baud Rate on **12-9**, so the two settings are on facing pages rather
> than one. Footnote 4 is on 19-7 in both. Noted rather than edited (§1).

NO FAULT LANGUAGE, and a test holds it. Nothing here is anybody's mistake, and a
radio that shipped with these switches off is a radio behaving exactly as
documented. The note describes what is not arriving and where the switches live,
and a test fails it on "failed", "error", "wrong", "you must" and their
neighbors.

THE COLLAPSED SUMMARY GOT THE SAME CORRECTION. It said "receiving" for any real
radio, including one that has never sent a sweep. A shut panel that goes quiet
about a problem is §0.5 broken by omission, so it now says "nothing arriving"
until something does.

---
id: HM-DEC-066
date: 2026-08-14
refs: HM-OPEN-006, src/Hamlet.App/Settings/AppSettings.cs, src/Hamlet.App/ViewModels/SettingsViewModel.cs, src/Hamlet.RadioEngine/Explore/SpotRanking.cs, tests/Hamlet.RadioEngine.Tests/Explore/CopySpeedTests.cs
---

The operator states a **Morse speed** in Settings, beside the other listening
preferences. The ranking reads it and the happening-now cards say how a station
compares to it.

THE DEFAULT IS 13 WORDS A MINUTE, and it is not a new number. It is
`SpotRankWeights.RelaxedWpm`, which is where this ranking has always drawn the
line between a relaxed pace and an ordinary one, so the setting is read off the
existing scale rather than typed again beside it and a test fails if the two
ever part company (§0). Thirteen is also about where the slow-speed clubs run,
which is the answer to "conservative and suited to somebody new" from the hobby
rather than from arithmetic. It sits deliberately below what most of the band
does. Somebody new is better served by an app that starts gentle and lets them
raise it than by one that starts where the contest operators live and leaves
them wondering why none of this sounds like the practice files.

A SHIPPED DEFAULT CHANGES NOBODY'S LIST ON ITS OWN. The speed bands keep the
shape they always had and slide to wherever the number is put, so a fresh
install ranks exactly as this ranked before the setting existed, and a stated 20
treats a 20 words a minute station the way a stated 13 treats a 13. The offsets
are derived from the old thresholds rather than restated.

THE HONESTY RULE SURVIVES INTACT, and it is the whole reason this needed a
ruling. A stated speed is a preference and a measured ability is a different kind
of fact. Hamlet may say a station is sending far over the number in the settings,
because both figures were stated and the comparison is arithmetic. It may not say
that speed is too fast for this person, or slow enough for them, or within their
reach, because it has never heard them copy anything and that would be a
confident match against a measurement nobody ever took (§0.0). A test sweeps the
composed card text for every phrasing that crosses back over.

NOTHING IS FILTERED AND NOTHING IS HIDDEN. A station sending three times faster
than somebody asked for still appears, ranked lower, with the reason printed on
it. Hiding it would be Hamlet deciding what they are capable of, which is exactly
the claim it may not make. This is offered and never asserted.

THE SETTING'S OWN COPY DOES THE WORK THE NUMBER CANNOT. A speed box in a radio
program reads like a test, and somebody who has never made a contact will read
it as one and enter what they think they ought to manage. So the copy says what
words a minute means, says what the figure is used for, says out loud that
nothing is being tested and nothing disappears from the list, and says to move it
up as the letters start arriving on their own.

HM-OPEN-006 STAYS OPEN, with its severity unchanged. The setting is the weaker
half of the answer. The stronger half is still ONB-C04's listening exercise,
because somebody who has never made a contact does not know what speed they can
copy either, and asking them to type a number invites a guess. What closed here
is the gap where the app had no way to hear the preference at all.

---
id: HM-DEC-065
date: 2026-08-14
confirms: HM-DEC-029
refs: src/Hamlet.App/ViewModels/CwTransmitViewModel.cs, src/Hamlet.App/Views/MainWindow.axaml, tests/Hamlet.App.Tests/ViewModels/UnresolvedLicenseTests.cs
---

An unresolved license class **warns and labels, and never blocks.** Where a send
control sits and Hamlet does not know which class the callsign holds, it says so
in one place beside the buttons.

THIS CONFIRMS HM-DEC-029 RATHER THAN AMENDING IT. A brief last session claimed
`TransmitGuard` refuses on an unresolved class. It does not, it never did, and
the brief was wrong: the guard permits, states what it does not know, and gets
out of the way. Tim ruled that behavior correct, so the guard is unchanged and
the doc comment that explains it stays exactly as it was.

WHY REFUSING WOULD BE WRONG, and it is worth writing down because refusing looks
like the safe choice from a distance. Hamlet has no business declining to key
somebody's own radio because a lookup service did not answer. The operator holds
the license and knows what it says. A program that locked them out of their own
transmitter over a failed HTTP request would be teaching a beginner something
false about their license, and it would teach it at the moment they are least
able to argue with it.

WHAT THE LABEL SAYS AND DOES NOT SAY. It says Hamlet does not know which class
this callsign holds, that it therefore cannot check this frequency against
privileges, and that the operator should satisfy himself he is allowed here. It
is a statement about what Hamlet does not know, which is a fact about Hamlet and
not about the person reading it. Once, near the control, no repetition anywhere
else, and no scolding: a test fails the copy on "you must", "you should", "be
careful" and their neighbors.

NOTHING READS IT BUT THE LABEL. No button is disabled by it, no send path
consults it, and the guard never sees it. The guard decides for itself from the
class it is passed, which is what keeps one decision in one place (§0.2).

A stale paragraph in the guard's own documentation was corrected while it was
open: it said no transmit path existed yet, which was true when it was written
and stopped being true when HM-DEC-059 landed. The behavior and the comment this
ruling protects are untouched.

---
id: HM-DEC-064
date: 2026-08-14
refs: src/Hamlet.App/Views/MainWindow.axaml, tests/Hamlet.App.Tests/Settings/SettingsRoundTripTests.cs, HM-DEC-016, HM-DEC-021, HM-DEC-025
---

The Explorer's panels run in this order, top to bottom: **where to start,
happening now, field notes, field guide, what a contact sounds like.**

WHY THAT ORDER. The first two help somebody get on the air and the last three
help them understand what they are hearing, so the ones that lead to a contact
come first. Six years of understanding without a contact is exactly the problem
this application exists to solve. Learning supports acting here, and it does not
precede it.

The rig display stays above all of them and stays the one panel that does not
collapse (§0.5). It is the radio's own face and the app's anchor.

REORDERING COSTS NOBODY THE PREFERENCE THEY SET. Every panel remembers whether
it is open under its own key in `settings.json` and never by its position, so a
file written before the move opens and closes exactly the panels it named. A
test writes that file by hand and reads it back, which is what would catch it if
somebody ever rewrote the storage as a positional list.

This is layout and nothing else. No panel gained or lost a summary, and a
collapsed one still says what it would have told you (§0.5).

---
id: HM-DEC-063
date: 2026-08-14
refs: Directory.Build.props, CHANGELOG.md, tests/Hamlet.App.Tests/ViewModels/VersionTests.cs, HM-DEC-019
---

The version is **1.2.0**, and this ruling establishes the convention rather than
applying one, because there was none recorded anywhere.

WHY 1.2.0 AND NOT A PATCH. A radio application that can key a transmitter for
the first time is not a fix. CW transmit is a new capability the operator can
see and use, which is exactly what a minor release is for.

SEMANTIC VERSIONING, WITH THE MEANINGS SAID PLAINLY for a project of this kind,
because "breaking change" means something different in a library and in a
program somebody runs at their desk:

- **Major** for a change that breaks the operator's existing setup or data, or a
  reconception of what the application is. Losing somebody's settings, their
  favorites or their spot history is a major release even when the code change
  is small.
- **Minor** for a new capability the operator can see and use. CW transmit is
  the clearest possible example, and so were the Explorer and the decoder.
- **Patch** for fixes, corrections and polish that add no new capability.

THE NUMBER LIVES IN ONE PLACE, `Directory.Build.props`, and every project reads
it from there. The About box already read the assembly at run time rather than
carrying a string of its own (HM-DEC-019), which is what makes one place enough:
the box, the telemetry line and the binary cannot disagree, because there is
only one thing for them to disagree with.

A CHANGELOG EXISTS AND IS DELIBERATELY THIN. `DECISIONS.md` already records
every ruling with its date, its reasoning and what was rejected, and it does
that far better than a changelog would. Writing the reasons out again would be a
second copy of the same facts, and §0 is explicit that a second copy drifts. But
there is one fact `DECISIONS.md` does not hold: which release contains which
rulings. So `CHANGELOG.md` is that index and nothing more, one line and a range
of ids per release, pointing at the decision log for the why.

The tests hold the chain rather than the number. A test pinning the exact
version would need editing on every release and would fail for the wrong reason;
what is worth guarding is that About reads the assembly, that the shell and the
engine ship as one thing, and that the number never silently falls back to
1.0.0.

---
id: HM-DEC-062
date: 2026-08-14
refs: src/Hamlet.RadioEngine/Civ/CivScope.cs, src/Hamlet.RadioEngine/Rig/RigSpectrumSource.cs, src/Hamlet.RadioEngine/Rig/ScopeReadiness.cs, tests/Hamlet.RadioEngine.Tests/Rig/ScopeStreamTests.cs, HM-DEC-005, HM-DEC-006, HM-DEC-026, HM-DEC-050
---

Real spectrum data reaches the waterfall from the radio's own scope, CI-V
`27 00`. **Reads only: nothing here writes to or keys the radio.**

VERIFIED COLUMN-AWARE against `IC-7300_Full_English v6`, p. 19-12, which is the
lesson HM-DEC-050 paid for. A sweep arrives as a division number, a division
maximum, a center-or-fixed flag, the span, an out-of-range flag and then the
waveform. Over USB it is divided by eleven: the first part carries the header
without waveform data and the rest carry the waveform. Data range 0 to 160, data
length 475.

A CORRECTION FOUND WHILE READING IT, and worth recording because the next session
will meet the same page. The `27 00` row's own description says the waveform is
output only when `27 10` and **`27 20`** are on. There is no `27 20`. The
sub-command list on the same page runs 00, 10, 11, 12, 13, 14, 15, 16, 17, 19,
1A, 1B, and `11` is "Send/read the Scope wave data output". HM-DEC-049 already
recorded `27 10` and `27 11`, and it is right; the cross-reference beside it is a
typo in the manual.

AND A PRECONDITION NOBODY HAD WRITTEN DOWN, in the same shape as the transmit one
(HM-DEC-059). Footnote 4 on p. 19-7: `27 11` can only be set with "Unlink from
[REMOTE]" selected on the CI-V USB port screen and 115200 on the CI-V baud rate
screen. Neither of those is a command at all, so no amount of code makes the
stream arrive on a radio whose menus are set otherwise. The waterfall says which
setting is missing rather than sitting empty, because an app that looked broken
while the answer was four menu screens away would send somebody hunting.

NOTHING TURNS THE SCOPE ON. That is a write, and this ruling is reads only.
Hamlet reads the two settings and reports, and turning somebody's scope on stays
theirs to do.

THE STREAM COSTS THE POLL LOOP NOTHING. The radio pushes these frames once its
own output is on, so the source asks for nothing and issues no commands at all: it
is a listener, and a test proves the command count is zero across a whole sweep.
The two settings behind it are read on connect and on demand and never in the
loop, which is HM-DEC-050's rationing applied to the highest-rate thing on the
bus.

A SWEEP WITH A HOLE IN IT IS DROPPED RATHER THAN PATCHED. A part that arrives out
of order would otherwise be stitched to the one before it, and a waterfall row
assembled from two different sweeps draws signals that were never simultaneously
there. Drops are counted, because a stream losing a third of its sweeps looks
like a slow waterfall and that is the hardest kind of defect to attribute
(§0.0.1).

A HEADER THAT WILL NOT PARSE PRODUCES NOTHING. Falling back to the band plan's
own edges would draw a waterfall whose frequencies are Hamlet's invention rather
than the radio's measurement, on the one surface built to show what is actually
there. The span comes off the wire or the row does not exist.

THE SIMULATED LABEL IS UNCHANGED AND UNWEAKENED (HM-DEC-026). Each source answers
`IsSimulated` for itself and neither has a setter, so real data arriving cannot
turn the label off and synthetic data cannot arrive without it. A test asserts
the absence of both setters, because that absence is the whole mechanism.

The renderer was not touched. It already owns its bitmap and subscribes to the
engine's event directly (HM-DEC-006), which is exactly what made swapping the
data source a matter of attaching a different one.

---
id: HM-DEC-061
date: 2026-08-14
refs: src/Hamlet.RadioEngine/Explore/FamilyFilter.cs, tests/Hamlet.RadioEngine.Tests/Explore/FamilyFilterTests.cs, HM-DEC-032, HM-DEC-045, HM-DEC-057
---

Three chips at the head of the happening-now panel: Morse, Digital, Voice.
Multi-select, all on by default, each in its family color, persisted across
restarts, and named in the collapsed summary.

**EACH CHIP CARRIES A LIVE COUNT, AND THE COUNT SHOWS EVEN WHEN THE FAMILY IS
SWITCHED OFF.** That is the teaching part rather than a detail of the control.
Somebody who filters to Morse and still sees forty-one voice stations learns that
the band is full of people they could talk to, which is the fact this whole app
exists to reveal. A filtered-out family that went silent would teach the
opposite: that switching something off makes it stop existing, which is exactly
the belief six years of tuning around and finding nothing already installed.

So the count is taken over everything the lens has, before the filter runs, and
never over what survives it. A chip reading zero because it was switched off
would be telling the operator there is nothing there.

THEY FILTER AND THEY NEVER DELETE. This is one more view over the store, like the
lenses (HM-DEC-045, HM-DEC-057), so a chip changes what is drawn and changes
nothing about what Hamlet holds. It composes with the lenses rather than fighting
them: the lens decides what is in play and the chips decide which families of it
are drawn, in that order.

THREE CHIPS AND NOT FOUR. Open is not a family anybody tunes for, it is the space
between the families, so a chip for it would be a filter for "whatever is left".
A mode no chip names is shown whenever anything is, because a spot that vanished
because of a control that does not mention it would be the app losing something
quietly (§0.0).

EVERY CHIP OFF SHOWS EVERYTHING rather than an empty panel. Somebody who switched
all three off has not asked to see nothing; they have wandered into a state with
no meaning, and a blank panel reads as broken.

THE COLLAPSED SUMMARY SAYS WHAT IS BEING FILTERED TO (§0.5). A shut panel that
had two families switched off would otherwise show a count the operator would
take for a count of everything, which is the prime directive broken by omission.

The colors are `ModePalette`'s and the words are the map legend's, to the letter
(§0.6, HM-DEC-032). A switched-off chip is dimmed rather than hidden, which is
also its second carrier: the on-or-off state survives the grayscale test without
depending on the fill.

---
id: HM-DEC-060
date: 2026-08-14
refs: src/Hamlet.RadioEngine/Explore/Favorite.cs, src/Hamlet.App/ViewModels/FavoritesViewModel.cs, src/Hamlet.App/Views/FavoritesWindow.axaml, HM-OPEN-007, FG-011, HM-DEC-054
---

Certain frequencies are worth coming back to and nobody remembers the numbers,
so Hamlet keeps favorites, and its favorites carry the reason.

THE RADIO'S OWN MEMORY CHANNELS ARE THE PROBLEM RATHER THAN THE ANSWER. They are
numbered slots whose meaning you have to remember, and remembering what channel
seven was for is exactly the same work as remembering the number. Hamlet already
knows why somebody was on a frequency, because the neighborhood map says what
lives there (HM-DEC-054), so a favorite reads "14.074, FT8 city" rather than
"MEM 07".

SAVING CAPTURES CONTEXT AUTOMATICALLY: frequency, mode, band and neighborhood,
with nothing typed. The operator may rename it and nobody has to. Where the map
has published no convention for a stretch, the favorite is the frequency and its
band and says no more, rather than inventing a description of open ground (§0.0).

ON THE WARM PANEL BELOW THE LCD, NOT INSIDE IT. The black rectangle is a faithful
picture of the IC-7300's own face, and a control the real radio does not have
would blur which is which. Two things sit there:

- **The star, which names where you are.** Filled on a saved frequency and
  reading that favorite's name; hollow anywhere else and reading "save this
  spot". Pressing it on a favorite un-saves, so it is one toggle rather than two
  controls. It matches on the exact frequency and not nearby, because a star
  that lit up a hundred hertz away would make un-saving unpredictable and the
  operator would learn not to trust it.
- **A dropdown beside it**, the same list, click to tune. Absent until there is
  something in it, since a dropdown with nothing in it looks broken.

IN THE RADIO MENU, because everything about the radio belongs there: a Favorites
submenu that tunes on click, and "Manage favorites…" opening a window that
renames, reorders and deletes, with every row showing its mode, its band and when
it was saved. Those three are what answer "what was this for", which is the whole
reason this exists rather than the radio's numbered slots.

Persisted in `settings.json` like everything else Hamlet remembers, in a settings
shape rather than the engine's record, because anything persisted has to survive
a rename with a migration behind it (§6.1).

TWO THINGS ARE DELIBERATELY NOT DECIDED and are recorded as HM-OPEN-007: whether
favorites ever sync to the radio's own memory channels, and what happens to a
favorite whose neighborhood data later changes underneath it.

AND ONE THING IS DELIBERATELY LATER, as FG-011: Hamlet could notice where the
operator actually spends time and offer those as favorites they never starred.
Offered, never added silently.

---
id: HM-DEC-059
date: 2026-08-14
refs: src/Hamlet.RadioEngine/Cw/ICwSender.cs, src/Hamlet.RadioEngine/Cw/CwTransmitter.cs, src/Hamlet.RadioEngine/Cw/TransmitReadiness.cs, src/Hamlet.RadioEngine/Cw/ContactStage.cs, tests/Hamlet.RadioEngine.Tests/Cw/CwTransmitTests.cs, HM-DEC-008, HM-DEC-029, HM-DEC-043, HM-DEC-049, HM-OPEN-006
---

Hamlet keys the radio and sends Morse, by handing text to the radio's own keyer
with CI-V command 17. **USB keying and Farnsworth are deliberately deferred to
their own ruling and their own session**, after this path is proven at a dummy
load.

THIS IS THE FEATURE THE WHOLE APP HAS BEEN WALKING TOWARD, and it belongs to
somebody who has held a license for six years and made one contact. Everything
below is shaped by that rather than by what a contest station would want.

TWO KEYING PATHS EXIST AND ONLY ONE IS BUILT. Command 17 hands up to thirty
characters to the radio's keyer, which sends them at its own speed with its own
clean timing, which is better timing than a PC can produce down a serial line.
USB keying, where the radio exposes a keying line on DTR or RTS and the PC owns
every element, is the second path. It is what Farnsworth needs and it is not
built here.

SO THE SENDING PATH IS BEHIND AN INTERFACE with one implementation today. What
that buys is that adding USB keying later is a new implementation rather than a
rewrite of everything above it. Nothing above the seam learns which path it is
on, except through one property, which exists for exactly one purpose.

FARNSWORTH IS AN EXPLICIT KNOWN-UNKNOWN IN THE UI, NOT A HIDDEN ABSENCE. The
radio's CW-KEY SET menu offers dot/dash ratio, rise time, paddle polarity and key
type, and nothing at all for the gaps between characters (Full Manual p. 4-21,
`IC-7300_Full_English v6`). Farnsworth means characters sent briskly with wide
gaps between them, which is how a learner hears a whole letter as one shape
rather than counting elements, and it needs control of the timing between
characters. So where speed is chosen the panel says plainly that the spacing is
the radio's own and cannot be widened yet. There is no Farnsworth control that
silently does nothing (§0.0).

THE SAFETY RULES, WHICH ARE §0.2 AND ARE ABSOLUTE:

- **One door.** Every path that keys goes through `CwTransmitter`, which calls
  `TransmitGuard.Check` first, every time, before it touches the radio
  (HM-DEC-029). There is no second way in and no bypass, and nothing else holds
  a reference to the sender.
- **The abort is same-thread and awaits nothing.** Command 17 carrying FF
  (p. 19-11), written straight at the port rather than behind the command gate,
  because a stop queued behind the send it is stopping would arrive after the
  message finished. It needed a synchronous write on the port seam, which did
  not exist and does now. It works mid-send, it is safe when nothing is sending,
  it is safe twice, and it never throws: an abort that could fail is not an
  abort.
- **Nothing transmits unattended.** No timer, no retry, no scan and no reconnect
  path can reach the transmitter, and a failed send is not repeated. A test
  proves the class has no timer and raises no event, so there is nothing in it
  that could key without being asked.
- **The dummy load is said once**, where somebody reads it before their first
  send, as the ordinary precaution it is rather than as a warning about their
  competence (HM-DEC-008).

THE PRECONDITION NOBODY HAD WRITTEN DOWN IS CHECKED BEFORE THE SEND, NOT AFTER.
In CW mode a message sent with command 17 is transmitted only when TRANSMIT or an
external TX switch is on, or Break-in is on (command table footnote 2, p. 19-7).
Without it Hamlet sends a correct frame, gets a correct acknowledgement, and the
radio stays silent. That is the prime directive broken by omission: the app would
report a success that never left the antenna, and somebody making their first
call would sit there wondering why nobody answered. Hamlet already reads break-in
(HM-DEC-050), so it answers rather than guessing, and an UNREAD setting refuses
too, because "I do not know whether this will go out" is a different answer from
"it will".

THIRTY CHARACTERS IS THE LIMIT and the UI never presents a message it cannot
send. Longer messages split in the engine, at the spaces, so a callsign is never
cut in half.

WHAT THE OPERATOR SEES:

- **Contextual send buttons.** Calling CQ is one button when nothing is
  happening; answering is a different one when a station is calling; the
  exchange, the confirmation and the sign-off each appear when they are the next
  thing anybody would say. The whole ritual is never laid out at once and the
  operator is never asked to pick from it, because the terror is not the radio,
  it is not knowing what to say, and a wall of choices is the same problem in a
  different coat (HM-DEC-043).
- **Staged sending, under a "let me read it first" toggle, default on.** The
  first press composes and shows; the second sends. Somebody who can read the
  words before they go out will press the button at all, which is the entire
  point.
- **A phrasebook, collapsible, with a column for admitting you are new.** "QRS
  PSE, I am new" is a real and welcome thing to send. A beginner who knows that
  sentence exists is far more likely to call; one who does not assumes the band
  is a room full of experts who will be annoyed with them.
- **Speed offered, never asserted.** The decoder measured what the other station
  is sending at, so Hamlet may say so. It has never asked what this operator can
  copy, so it may not claim any speed suits them. That gap is HM-OPEN-006.
- **A closing card when a contact ends**, saying who, where, what band and what
  was exchanged, written like a friend saying it went fine. It is not a logbook
  and does not try to be; FG-004 is where logging lives.

Nothing observed is invented and nothing unobserved is filled in: a report nobody
sent is not mentioned, a speed nobody measured is not stated.

---
id: HM-DEC-058
date: 2026-08-14
refs: src/Hamlet.RadioEngine/Explore/SpotRankWeights.cs, src/Hamlet.RadioEngine/Explore/SpotRanking.cs, tests/Hamlet.RadioEngine.Tests/Explore/SpotRankingTests.cs, HM-OPEN-006, HM-DEC-038, HM-DEC-057, FG-007
---

The happening-now list ranks for what a newcomer can actually work, not for
distance. It is built on the lens machinery, so liveness comes from one clock
rather than a parallel one.

WHY DISTANCE IS NOT THE ANSWER, AND THIS BELONGS IN THE RECORD. Distance does
not run in a straight line with workability on HF. There is a skip zone: on 20 m
a station two hundred miles off is often unreachable, because the signal leaves
at too shallow an angle and comes down beyond them, while somebody two thousand
miles out is easy. On 40 m at night the close-in stations come back in loud.
Sorting nearest-first would put the hardest contacts at the top and call them the
best chance, which is a guess presented as a decode.

HM-DEC-038 compounds it. Only sources that said where the STATION is carry a
distance at all, so POTA has one and RBN has none, and a distance-led sort would
bury every RBN spot. That is where "somebody is calling CQ right this second"
comes from, which is the freshest evidence this app has of anything.

DISTANCE STAYS ON THE CARD, because it teaches a newcomer what ranges are
plausible on which band, and that sense is exactly what this operator is missing.
It earns a real vote when FG-007 lands and Hamlet can say which bands are open to
where.

The consequence, stated so nobody is surprised by it: until then, a park in
Bavaria and a park in Ohio rank the same if they are alike in every way Hamlet
can judge. That is uncomfortable and it is honest. The card carries "4250 miles
northeast" and the operator learns from it, which is more than a silent penalty
would have taught them.

ONE PROXIMITY STILL VOTES, AND IT IS NOT A DISTANCE. A skimmer report states
where a receiver that decoded the signal is standing. A skimmer in the operator's
own call district is the closest thing to "your receiver will hear it too" that
any spot network can honestly offer, so it counts, and it counts only for sources
that report a receiver. An activation's proximity is where the station is, which
is a distance, so it does not.

WHAT THE RANK WEIGHS. Whether the station is alive right now, taken from the lens
machinery. Whether they are soliciting contacts, since an activator calling CQ is
the friendliest target on the band for a first QSO. The mode, against what Hamlet
can currently help with. And sending speed where the source reports it.

LIVENESS RUNS FROM A PENALTY TO A BONUS, which was not the first design and is
the correction the tests forced. An activation calling CQ in Morse at a relaxed
pace scores over seventy before liveness is counted at all, so the absence of a
freshness bonus was not enough: somebody who packed up an hour ago still led the
list. It now slides from minus twenty-five to plus thirty across the source's own
ruled lifetime, so a finished activation cannot outrank a live station calling
CQ, and no threshold steps where nobody could see it coming.

THE WEIGHTS LIVE IN ONE NAMED PLACE and are legible rather than imagined:
`SpotRankWeights`, each one with the reason it is what it is, and the reason line
on every card extended to four phrases so the ordering is explainable from the
screen. There is deliberately no control for them in the app. They are a judgment
about what a beginner can work, and a slider would ask the operator to make that
judgment before they have the experience to make it. Tim rules on the numbers;
this makes them readable rather than asking him to imagine them.

THE SPEED FACTOR HAS A HOLE AND IT IS DECLARED RATHER THAN FILLED. Hamlet has
never asked what speed this operator can copy, which is ONB-C04 and the missing
half of FG-002. Until it does, the rank may use a reported speed to DESCRIBE a
station and may not claim any speed suits this person: a confident match against
a number nobody has ever measured is what §0.0 forbids, and it would fail in the
direction that costs most, sending somebody to a contact they cannot make and
letting them conclude the fault is theirs. So "15 WPM, slow enough to copy"
became "15 WPM, which is a relaxed pace", the gap is HM-OPEN-006, and a test
sweeps the reason lines for the phrasings that would cross back over.

---
id: HM-DEC-057
date: 2026-08-14
refs: HM-DEC-045, HM-DEC-020, HM-DEC-025, FUTURE_GOALS.md
---

The happening-now panel gains a segmented control at its head with two named
lenses, because there are two different questions and refresh answers neither.
**Recorded, not built.**

TWO QUESTIONS, AND THEY ARE NOT THE SAME ONE. "Best chance" is the arrival
question: somebody sits down, and wants a ranking over everything currently
alive. "What's new" is the between-contacts question: somebody has just finished
a contact, and wants the delta since they last looked, without being re-offered
what they have already worked or already passed over. A refresh button answers
neither, because it conflates "show me the good ones" with "show me the fresh
ones" and the answer to those is different on almost every band.

BOTH ARE ALWAYS VISIBLE, AND THAT IS THE TEACHING. Two words on screen say more
than any inference: a newcomer who sees a lens called "what's new" learns that
hunting again after a contact is a normal thing people do, which is a fact about
this hobby nobody tells them. That is worth the control by itself.

AGE FADES THE DISPLAY ACROSS EACH SOURCE'S RULED LIFETIME (HM-DEC-045), so the
eye finds what is current without anybody reading a timestamp. Under "best
chance" a solid old park activation is still allowed to rank high, because
somebody is still standing in that park and an hour is what an activation is.
Under "what's new" it is not new and does not appear. The two lenses are allowed
to disagree; that is what makes them two lenses.

INFERENCE MAY CHOOSE WHICH LENS OPENS AND MAY NEVER OVERRIDE THE OPERATOR
AFTERWARD. Guessing which question somebody is asking is a reasonable thing to do
once. Guessing again, after they have answered it by clicking, is the app
arguing with them.

NOTHING IS DELETED, EVER. This is a view over the store, which is exactly what
HM-DEC-045 built the store for. A hard refresh that emptied history would
re-create the failure that ruling ended: throwing away good invitations at ten
minutes and then saying "nothing here" while holding them, which is the moment a
newcomer gives up.

---
id: HM-DEC-056
date: 2026-08-14
refs: src/Hamlet.RadioEngine/Civ/CivWrites.cs, src/Hamlet.RadioEngine/Explore/ModeFollowPlan.cs, src/Hamlet.RadioEngine/Rig/RigWriteResult.cs, tests/Hamlet.RadioEngine.Tests/Rig/ModeFollowTests.cs, CLAUDE.md §4, HM-DEC-050, HM-DEC-054
---

Hamlet writes to the radio for the first time, and what it writes is the mode:
tuning into a neighborhood sets the mode that neighborhood is worked in. **This
is the writes ruling HM-DEC-050 deferred**, and it is built as a pattern rather
than as a feature, because the transmit work will inherit it.

WHY THE MODE AND NOT SOMETHING ELSE. Every part of a band has a mode the people
there are using, and having it wrong is the commonest reason a beginner hears
nothing at all. The app already knows where the dial is pointing and what lives
there (HM-DEC-054); the operator does not yet, and that asymmetry is the whole
product.

**THE COMMAND IS 26, NOT 06, AND THAT IS THE PART WORTH KNOWING.** Command 06
sets a mode and a filter and has no way at all to say whether the data variant is
wanted (p. 19-8). Command 26 carries the mode, a data mode flag and the filter,
for the selected or unselected VFO (p. 19-11). USB and USB-D are different radios
to the operator, one with the microphone live and one routing the computer's
audio, and it is the difference between hearing FT8 and hearing nothing useful.
Hamlet sends the data flag and skips the filter byte, because the manual says the
radio then picks that mode's own default filter, which is a better answer than
any Hamlet could invent for somebody else's rig.

READ WITH A COLUMN-AWARE EXTRACTION, which is the lesson HM-DEC-050 paid for. The
flattened text from that session is still on disk and still puts "Send/read CW
pitch" against sub-command 08 rather than 09. Re-read from
`IC-7300_Full_English v6` with `pdftotext -table`, which puts the CW pitch on 09
and gave both mode commands their pages. The manual is cited and never committed.

WHAT THE PATTERN IS, for transmit to inherit:

- Every write frame goes out through the same gate and the same trace as every
  read, so a session log carries it verbatim with its timestamp (§0.0.1).
- Nothing is assumed from having sent it. The radio acknowledges with FB or
  refuses with FA, and anything else leaves the value UNKNOWN rather than set to
  what was asked for. A mode Hamlet believes it set and did not is a guess
  presented as a decode, and it would put the badge and the radio's own face out
  of step with nothing on screen saying so.
- Every write the app makes on its own initiative is narrated in the status line,
  because a radio that changes itself silently is the "is it broken" confusion
  relocated rather than removed.
- The decision is a pure function, so the cases nobody exercises by hand are the
  ones the tests cover.

THE BEHAVIOR. It is a visible setting, on by default, worded plainly. The
operator's own hand always wins: a mode change Hamlet did not make suspends the
automation until the next band change re-arms it, and suspended is a visible
state on screen rather than a silent one, because an app that quietly stopped
doing a thing it had been doing is worse than one that never did it. A flip waits
for the dial to settle, so crossing three neighborhoods in one drag produces one
change and not three. The status line says what changed and why, in the app's
voice: "Switched to USB-D, this block is where the digital modes gather."

THE SIDEBAND CONVENTION IS CITED. IARU Region 2 Band Plan, September 2020: "For
SSB phone operations below 10 MHz use lower sideband (LSB); above 10 MHz use
upper sideband (USB)." Its one exception is 60 m, which Hamlet does not draw.

A NEW READ CAME WITH IT, and it is the honest half of the write. Command 26 in
its read form reports the mode, the data flag and the filter together, which is
the only way to tell USB from USB-D: command 04 says USB for both. So `DataMode`
is a first-class field with an unknown state like every other, read on connect
and when the diagnostics screen is opened, and never in the poll loop.

NOTHING HERE GOES NEAR KEYING THE TRANSMITTER. §0.2 is untouched. The write table
holds one entry and a test says so.

---
id: HM-DEC-055
date: 2026-08-14
refs: src/Hamlet.RadioEngine/Bands/AmateurSpectrum.cs, src/Hamlet.App/ViewModels/PrivilegeStatusLine.cs, src/Hamlet.App/Controls/ModePalette.cs, tests/Hamlet.App.Tests/Licensing/OutOfBandTests.cs, HM-DEC-029, HM-DEC-046, HM-DEC-009
---

One out-of-band fact is derived in the engine and every surface that speaks
reads it from there, so no two surfaces can disagree.

WHAT HAPPENED. The operator tuned to 14.350.000, the very top edge of 20 m, and
the card said "14.350 MHz, yours to use. Your General license covers Morse here.
Call away." Above 14.350 is not amateur spectrum at all. The privilege overlay
was treating "past the end of my data" as "no restriction found", which inverts
the meaning of the silence, in the one place in this app where a confident error
has legal consequences.

ONE DERIVATION, EVERY SURFACE, which is HM-DEC-046's pattern applied to the band
edge. `AmateurSpectrum` answers from the cited Part 97 data read against the
Extra class, which by definition reaches every band edge, so the band edges are
not carried a second time. The map, the card, the dial tape line and the rig
display all read it.

THE MAP DRAWS PAST THE EDGE, because an edge that is the end of the picture
teaches nothing. Beyond it is a labeled region in a cold gray that belongs to no
mode family, and the legend gains it. It is explicitly not the listen-only
hatching, since "you may listen but not transmit" is true inside the band too and
this is a different fact. It is explicitly not the open neutral, since that means
unclaimed amateur space and this is not amateur space at all. Both separations
had to survive the grayscale test as well as the color one, so the gray is darker
than the four families rather than merely cooler, and the block carries its name
in words.

THE CARD GOES AMBER AND NEVER RED (HM-DEC-029: explain, never scold). It states
that listening is fine anywhere, that transmitting here is permitted on no
amateur license, and it carries its citation the way the in-band card already
does. It says which edge was crossed, because "you have gone past the top of
20 m" is something somebody can act on and "out of band" is not. And it does not
depend on who is asking: an Extra holds every US privilege there is and still may
not transmit on somebody else's allocation.

The dial tape said "OUTSIDE the CW segment" at 14.350, which is true and wildly
understates matters. It now speaks from the same fact.

NOTHING STOPS THE DIAL, and this turned out to need a change rather than only a
reassurance. The frequency was being clamped hard at the band edge, which is a
locked control standing in for an explanation and is the thing HM-DEC-029 says
not to do. It was also how somebody ended up sitting exactly on 14.350 reading a
card that invited him to call. The stop is now the end of the drawn picture
rather than the end of the band, so tuning off the top shows what is out there
and says why it is not yours.

And a frequency the RADIO reported is never clamped at all. A 7300 tunes right
across the shortwave broadcast bands and somebody will do it. Clamping a
measurement to fit a picture would put a number on screen that the radio is not
on, which is the prime directive broken on the one value every other surface
trusts.

---
id: HM-DEC-054
date: 2026-08-14
refs: data/bands/us-neighborhoods.json, src/Hamlet.RadioEngine/Explore/NeighborhoodData.cs, src/Hamlet.App/ViewModels/PrivilegeStatusLine.cs, HM-OPEN-005, HM-DEC-029, HM-DEC-032
---

The neighborhood map moves out of code into `data/bands/us-neighborhoods.json`
with a source on every row, the digital watering holes are on it, and the card
under the map speaks about the world as well as about the regulation.

WHAT HAPPENED. The operator tuned to 14.075 on 20 m and heard what he described
as whale song. That was the FT8 watering hole at 14.074, one of the busiest
slices of spectrum on Earth. The map labeled the whole of 14.000 to 14.150 as
Morse, and the card said his General license covered Morse there and invited him
to call away. Both statements are defensible about the regulation and wrong
about the world. Anyone acting on that card would have keyed Morse into a wall
of digital signals that cannot hear him, while stepping on dozens of contacts.
The map exists so that moment does not happen and it produced the moment
instead.

CONVENTION AND REGULATION ARE DIFFERENT FILES, on purpose. `data/privileges`
says what may be transmitted and has legal weight. `data/bands` says what will
actually be found and has none. They disagree deliberately, and 14.074 is the
case: legal for Morse under 97.305(a), and the worst place on the band to send
it.

EVERY ROW CARRIES ITS SOURCE. The ARRL Considerate Operator's Frequency Guide,
which is the ARRL's own statement and says in its first paragraph that it is not
regulation. WSJT-X's shipped frequency table, which is what the world is
actually tuned to for FT8 and FT4. The JS8Call user guide. The 070 Club's PSK31
list. QRP ARCI's centers of activity. Every one of those was fetched and read
this session; nothing on the map is written from recollection, because a
neighborhood invented from memory is the prime directive broken in the data
layer, where it is hardest to see and where it outlives everybody who could
correct it.

ONE EDITORIAL RULE, STATED IN THE FILE AND APPLIED EVERYWHERE. Several sources
publish a watering hole as one dial frequency rather than a range. Those blocks
run from the published frequency to the next one, or three kilohertz, whichever
comes first, because these modes are worked in upper sideband with audio up to
about three kilohertz and that is where the signals land. The 070 Club states
exactly that, so even the width is cited rather than chosen.

WHAT COULD NOT BE SOURCED IS DECLARED. The slow-speed CW gathering places are
the one that stings: an earlier version of this map said 7.055 was "the
slow-speed club" and that number came from nobody. It is now an explicit unknown
with the reason attached, and it is the field that matters most to the operator
this app is for, which is exactly why it is marked rather than guessed.

THE BAND EDGES ARE NOT TRANSCRIBED AGAIN. Where the data segment ends and the
voice segment begins comes from the cited Part 97 file, because a second copy of
a boundary is a second copy until the two disagree (§0). That turned out to have
a trap in it. The lowest phone allocation on 40 m is 7.075, which belongs to
stations in particular places rather than to the band generally, and taking the
lowest one painted everything above 7.077 as the voice end, FT8 included. What
is wanted is the point above which the rest of the band really is voice.

A STRETCH NOBODY PUBLISHED A CLAIM TO IS OPEN GROUND, not Morse. Below the phone
segment the regulation allows Morse and the data modes alike, so coloring an
unclaimed stretch amber would say Morse owns space it does not (§0.6).

AND THE CARD STOPS INVITING. The legal sentence stays, because it is what the
operator asked and it is true. "Call away" goes wherever the map has a caution,
and the map supplies the other half in the app's own voice: this block is where
the digital modes gather, and the software listening here cannot hear Morse at
all. Consequence, never instruction, which is the line HM-DEC-050 already drew.

---
id: HM-DEC-053
date: 2026-08-14
refs: src/Hamlet.RadioEngine/Civ/CivReads.cs, src/Hamlet.RadioEngine/Rig/Ic7300Rig.cs, tests/Hamlet.RadioEngine.Tests/Rig/RigBroadcastProvenanceTests.cs, HM-DEC-009, HM-DEC-030, HM-DEC-050
---

A value the radio volunteers is a supported, populated value whose provenance is
the broadcast. Broadcast is a provenance, not an absence, and
`Unsupported` is reserved for what the capabilities record says the rig
genuinely lacks.

WHY, AND IT IS THE WORST KIND OF BUG THIS PROJECT CAN HAVE. The diagnostics
screen showed the Frequency row as "not on this radio" and "IC-7300: Hamlet
reads nothing for this" while the rig display an inch above it was showing the
live frequency. The screen exists to prove what the app knows (§0.0.1) and it
was asserting the opposite of what the app knew, on the one surface that is
meant to be immune to that.

THE MECHANISM. HM-DEC-050 rightly never polls the frequency, because the radio
broadcasts every change as the operator makes it and asking as well could only
ever be more stale. So there was no entry in the CI-V read table, and the sweep
that walks every field mapped "no command for this" onto `Unsupported`, which
means "nothing is ever coming, stop waiting" (HM-DEC-030). One absence stood in
for a completely different one, and the broadcast reading that was already in
the model got overwritten by it every time somebody opened the screen.

THE FIX IS IN THE TAXONOMY, NOT IN A SPECIAL CASE FOR THE FREQUENCY. A field is
now populated by one of three mechanisms, and none of them is an absence: its
own read command, another command that answers it on the way past, or a
broadcast the radio pushes. Only the capabilities record produces `Unsupported`.
A gap in Hamlet's own table produces `Unknown` and says Hamlet is the gap.

Fixing the classification rather than the row caught the second instance
immediately. The filter designator arrives on the back of the mode command
(p. 19-9 as recorded; **p. 19-8** in `A7292-4EX-6`, corrected by HM-DEC-071) and
has no read of its own, so the same sweep concluded the radio had
no filter moments after reporting which filter was selected. Nobody had noticed,
because the badge is fed from the mode read and looked right.

Two mechanisms are named apart rather than blurred. "transceive 00" and
"CI-V 03" both produce a frequency, they mean different things about how current
it is, and the provenance column now says which one spoke. The frequency also
gains a read of its own at last, cited to p. 19-3, issued by the connect sweep
and by the operator pressing Refresh and at no other time, because nothing
broadcasts what the radio was already sitting on before Hamlet arrived.

Before the first broadcast, and with transceive switched off at the radio, the
row reads unknown. Not unsupported, and never zero: 0 Hz is a plausible number
on the one field every other surface in the app trusts (§0.0).

---
id: HM-DEC-052
date: 2026-08-14
refs: src/Hamlet.App/Startup/ReconnectPlan.cs, src/Hamlet.App/ViewModels/MainWindowViewModel.cs, src/Hamlet.App/Settings/AppSettings.cs, tests/Hamlet.App.Tests/Startup/ReconnectPlanTests.cs, CLAUDE.md §8, HM-DEC-026
---

Hamlet reconnects to the radio it was last using when it opens, as a setting
that ships on, and every way that can go wrong ends on the training radio with
one sentence in the status line.

WHY IT IS ON BY DEFAULT. Connecting is the one thing the operator does every
single time, the app already knows which port they used, and a click that is
always the same click is friction rather than a choice. It is still a setting,
because somebody sharing a COM port with a logging program needs Hamlet to keep
its hands off that port, and the switch sits beside the audio settings rather
than in a corner, since it is about the same question: what is Hamlet listening
to when you open it.

NEVER BLOCKS AND NEVER INTERRUPTS. The attempt is started from the window's
Opened event and not awaited, so the window paints whether or not a radio
answers. There is no dialog at any point. A modal box between somebody and their
radio, saying a thing they can neither fix from the box nor act on, is the worst
version of this feature, and it is the version most software ships.

FALLS BACK TO THE TRAINING RADIO RATHER THAN TO NOTHING. An app that opens dead
because the rig is switched off has thrown away the evening; the training radio
puts a band on screen with signals moving on it, and HM-DEC-026 already
guarantees those signals are labeled as synthesized wherever they appear. So the
fallback cannot quietly become a lie about what is on the air.

A MISSING PORT IS NAMED, AND THIS IS THE PART THAT MATTERS. Windows hands a USB
radio whichever COM number is free at the time and changes its mind after an
update or a different socket, which makes renumbering far and away the most
common reason a reconnect fails. "Could not connect to COM3" sends somebody to
check a cable, a baud rate, a CI-V address and their own sanity. "COM3 isn't on
this computer any more" sends them to the port list, where the answer is. The
two failures are distinguishable before the port is ever opened, so reporting
them alike would be discarding information Hamlet already had (§0.0.1).

ONCE, AND NEVER IN A LOOP. If the radio arrives later the operator clicks
Connect, which they were going to do anyway. A background retry reopening a
serial port every few seconds is exactly what upsets the other software sharing
it, and it turns one honest sentence into a status line that will not sit still.

THE FALLBACK DOES NOT ERASE THE REMEMBERED RADIO. Landing on the training radio
sets the dropdown but leaves `LastPort` alone, so one evening with the rig
switched off does not quietly cost somebody the setting and leave them wondering
why the app stopped finding their radio.

The decision itself is a pure function returning a plan, separate from the
ViewModel, because every case worth having is a case nobody exercises by hand:
the rig switched off, the cable in a different socket, the setting turned off on
purpose. Those are the feature, so they are the tests.

---
id: HM-DEC-051
date: 2026-08-14
refs: src/Hamlet.RadioEngine/Rig/Ic7300Rig.cs, src/Hamlet.App/Views/MainWindow.axaml, src/Hamlet.App/Controls/CollapsiblePanel.axaml, tests/Hamlet.RadioEngine.Tests/Rig/RigDisconnectTests.cs, CLAUDE.md §8, §0.5
---

Three rules taken from the first evening Hamlet spent connected to a real
IC-7300: teardown returns promptly whatever the port does, the window scrolls to
everything it contains, and clearing the terminal wipes the display and nothing
else.

TEARDOWN IS BOUNDED, AND THE ORDER IS THE FIX. Disconnect did not work at all.
`SerialPort.BaseStream.ReadAsync` ignores its cancellation token on Windows, so
cancelling the read loop and then awaiting it waits forever, and the line that
cleared the connected state sat after the await and never ran. The button stayed
disabled, the port list stayed locked, and the app was still holding the radio.
Closing the port first and letting the read fault is the fix. State goes down
first and unconditionally, the port closes, and the loop gets half a second to
notice before it is abandoned. A read loop that will not die is a leaked thread
and that is regrettable; a UI that will not come back is a broken app, and §8
already says which one loses.

The test that proves it uses a fake port whose read never returns and never
will, which is exactly what Windows was doing. Without that fake the bug is
invisible to every test and visible to anybody with a radio.

ONE SCROLLER, NOT SEVEN. On a 1080p screen the CW terminal and the waterfall
were simply unreachable, with no scrollbar to reach them by. The main content is
one vertical scroller with the menu bar and the connection row pinned above it,
and the scrollbar is permanently visible rather than auto-hiding. It has to be:
the dial tape eats the mouse wheel to tune, which is correct, and a page whose
only affordance is a wheel the tape swallows is a page with no affordance at
all. Lists that would otherwise make the page enormous are bounded where they
sit rather than given competing scrollers.

CLEARING IS A DISPLAY OPERATION. Tuning around leaves a pile of half-decoded
noise above whatever is arriving now, so there is a Clear control, worded rather
than drawn, in the panel header where the eye already is. It empties the screen
and touches nothing else: not the speed estimate, not the tracked noise floor,
not the tone the decoder has settled on, and it does not stop decoding. Those
took real seconds of signal to arrive at, and losing them while chasing a
marginal signal is precisely the wrong moment. It gets its own slot in the
header rather than living inside the collapse toggle, so clearing cannot shut
the panel you are reading.

---
id: HM-DEC-050
date: 2026-08-15
refs: src/Hamlet.RadioEngine/Rig/, src/Hamlet.RadioEngine/Civ/CivReads.cs, src/Hamlet.App/Views/RigDiagnosticsWindow.axaml, HM-DEC-009, HM-DEC-030, HM-DEC-049
---

Hamlet keeps a model of the radio's whole state, populated by cited CI-V reads
and by the broadcasts the radio already sends, with unknown as a first-class
state distinct from unsupported. **Reads only. Writing to the radio is
deliberately excluded and gets its own ruling.**

WHY, AND IT COMES FROM ONE EVENING. The IC-7300 was connected for the first
time and the CW decoder produced garbage. Diagnosing it took half an hour of
asking the operator to walk to the radio and read menu settings out loud: what
is the filter set to, what is the ACC output level, is the squelch open, what is
the CW pitch. Every one of those is a CI-V read the app could have answered
instantly. The filter turned out to be wide open, which was the whole problem,
and Hamlet had no idea because it read frequency and mode and nothing else.

TWENTY-EIGHT FIELDS AND TWENTY-FIVE CITED READS, since mode and filter selection arrive from one command and the VFO has none. Mode, filter selection and the filter's
actual width in hertz; the S-meter; transmit status and front-end overload; RF
power, RF gain, squelch level and whether the squelch is open right now; AGC,
preamp, attenuator, noise blanker, noise reduction and both notches; break-in
and keyer speed, which the transmit work will need; the ACC and USB audio
settings that took four menu screens to check by hand; and split. Every read carries
the Full Manual page it came from, as HM-DEC-049 established.

Reading the table properly caught an error this project had already made and
recorded. **The CW pitch is sub-command `14 09`, not `14 08`.** Sub-command 08
is the outer Twin PBT position. A two-column page had been flattened during
extraction and the description landed against the wrong row, so §4 carried the
wrong byte from the day it was verified. Issuing 08 with a payload would have
moved somebody's passband while trying to read a pitch. The lesson is narrow and
worth keeping: a citation is only as good as the extraction it came from, and a
column-aware read is not optional on a two-column table.

THE FILTER WIDTH TAKES TWO PAGES, which is why nobody had it. Command `1A 03`
returns a position on a scale and the scale is documented on p. 4-6 rather than
in the command table, which gives only its endpoints. Fifty hertz apart up to
500, then a hundred apart to 3.6 kHz, with AM on its own two-hundred-hertz scale
and FM not adjustable at all. The read takes the current mode as context and
REFUSES rather than guessing when the mode is unknown, because reading an AM
index on the sideband scale would report 2.4 kHz as 600 Hz and that is the
number an operator would act on.

UNKNOWN IS A STATE AND NEVER A NUMBER (§0.0, HM-DEC-009). A field never read
answers unknown rather than zero, because an S-meter needle at rest looks
exactly like a measurement of a quiet band. Unsupported is a different state
again: "this radio has no AGC" means nothing is ever coming and the screen can
stop waiting, which is HM-DEC-030 doing its job. And undocumented is a third,
for a value the radio may well have and the manual describes no command for, so
the gap is recorded as being in Hamlet rather than in the radio. Inventing a
byte to close it is what §4 forbids and the radio would be the one to find out.

A reading also carries when it was taken, so stale is expressible. A number read
four minutes ago shown as current is a claim about now that is really a claim
about then, and the S-meter is where that matters most. The staleness window is
several times the poll interval on purpose: if they matched, an ordinary missed
read would flicker the screen and the operator would learn to ignore the marking.

POLLING IS RATIONED, because CI-V is a slow line shared with the transceive
stream and hammering it makes the radio sluggish and the app unreliable, which
is the hardest kind of defect to attribute because nothing actually breaks. Fast
values a few times a second and only while the window is visible. Settings swept
on connect and then every half minute. The frequency never polled at all,
because the radio broadcasts it and asking could only ever be more stale. The
filter selection never asked for separately, because reading the mode answers
it. One command in flight, which the test proves by counting overlap rather than
by trusting the gate. A read that times out marks its value unknown and the loop
moves on, because a bus already struggling is the last thing to send more
commands to.

AND THE BADGES THAT LIED ARE FIXED. The mode indicator on the rig display has
been hardcoded to "CW" since the LCD was built, and the filter designator to
"FIL2", both bound to string literals in the window. The screen lied the moment
anybody switched to sideband. It was the app's oldest prime-directive violation
and it survived because nothing ever read the real mode. Both are blank until
the radio has been asked, because a blank badge is somebody not having asked and
a badge reading CW is a claim.

The S-meter is fed for the first time, and its level is nullable all the way
from the model to the control: null is nobody having asked and zero is a quiet
band, and they would draw as the same unlit bar, so the scale dims and the meter
says "no reading" instead.

There is a diagnostics screen under Tools with every field, its value, the
command that produced it and how long ago, and a button that copies the lot for
a bug report. It is the screen that would have answered the evening's questions
in one glance, and §0.0.1 wanted it: a wrong value that arrives with its
provenance is something somebody can fix.

WHAT HAMLET MAY SAY ABOUT WHAT IT READ, and the line is narrow. "The filter is
open to 3 kHz and the radio is in Morse, so everything else inside that span is
arriving at the decoder at the same time" is a statement about two numbers it
read and a mechanism it understands. Telling the operator to narrow it is not:
that is operating somebody's radio for them. Every observation is a consequence
and never an instruction, and none may imply a fault, because a wide filter is a
perfectly good setting for listening around and may have been chosen on purpose.
A sweep enforces it against the imperatives, the fault words and the claims
about the world outside the numbers. Nothing is said at all from a setting
nobody has read.

Eleven glossary entries land with it, for the controls this now exposes: AGC,
preamp, attenuator, noise blanker, noise reduction, notch, RF gain, squelch,
passband, filter width and IF. Reading a value out is not the same as
understanding it, and the vocabulary is the gate this hobby is kept behind
(HM-DEC-041).

**NOTE ADDED 2026-08-14.** The `date:` above reads 2026-08-15, which was
tomorrow when this was written; the commits it describes are dated 2026-08-14.
Nothing in the ruling changes and nothing above has been edited. The note is
here rather than left alone because dates are how this log is ordered and how
anybody later reconstructs what was known when, so a wrong one is not the same
kind of harmless as a typo in prose. It is the same treatment §4 already gives
the `14 08` correction: labeled, dated, and beside the thing it corrects rather
than instead of it.

---
id: HM-DEC-049
date: 2026-08-14
closes: HM-OPEN-002
refs: CLAUDE.md §4, HM-DEC-005, HM-DEC-008
---

The IC-7300 Full Manual is in hand, the command facts CLAUDE.md §4 carried as
general knowledge are verified against section 19 with page citations, and the
figures the manual does not state stay marked as unknown rather than being
filled in with plausible numbers.

THE MANUAL IS CITED AND NOT COMMITTED. Icom's terms permit an individual to use
the documentation and prohibit redistributing it, so this repository carries
page references and the facts read off them, and no part of the PDF itself.
That is a stricter reading than §4's "vendor the cited pages" rule, and it wins
here because §2.1 forbids third-party proprietary material outright and a
public GPL-3.0 repository is exactly where that matters. Anybody checking the
work downloads the manual from Icom, free, and turns to the page named.

What was confirmed, and where. The frame is
`FE FE 94 E0 Cn Sc <data> FD` from the controller and `FE FE E0 94 ...` back,
with `FB` for acknowledged and `FA` for not (p. 19-2). The transceiver's default
address is `94h`, settable from `02h` to `DFh` (p. 12-10). Command `17` sends up
to thirty characters as CW, `FF` stops a message in progress, and `^` transmits
a string with no inter-character space (p. 19-13). Command `27 00` reads scope
waveform data and only does so when `27 10` and `27 11` are both on; the data
runs `00` to `A0` over a length of 475 and arrives in eleven parts over USB
(p. 19-14).

> **CORRECTION 2026-08-14 (HM-DEC-071): the values in the paragraph above are
> all confirmed and four of its page numbers are not this project's edition.**
> Against `A7292-4EX-6`, the settled edition, the address is on **12-8** rather
> than 12-10, the three command `17` facts are on **19-11** rather than 19-13,
> and the scope waveform rows are on **19-7** for the command and **19-12** for
> the data shape. There is no page 19-14 in that edition, whose chapter 19 ends
> at 19-13. The manual writes the data range in decimal as `0~160`, which is the
> same range as `00` to `A0`. Noted here rather than edited, because a ruling is
> never edited (§1).

AND THE PRECONDITION NOBODY HAD WRITTEN DOWN, which is the reason this record
is worth more than a tidy citation list. In CW mode a message sent with command
`17` is only transmitted when TRANSMIT or an external TX switch is on, or
Break-in is on (p. 19-8, footnote 2). Without that, the transmit work would have
sent a perfectly correct frame, received a perfectly correct acknowledgement,
and produced silence, which is an evening lost to debugging a thing that was
never broken.

Two corrections to what §4 assumed. The USB CI-V baud rate defaults to Auto
rather than to any fixed figure, and 115200 is one of six options rather than a
convention (**p. 12-9** in `A7292-4EX-6`; recorded here as 12-11 from an earlier
printing, corrected by HM-DEC-071), so the app must not hard-code it. And the CW pitch is
adjustable from 300 to 900 Hz (p. 4-14), encoded by `14 08` with 600 Hz at the
midpoint in 5 Hz steps (p. 19-3). The manual states the range and not a factory
default, so the decoder starts at 600 because that is the middle of what this
radio can produce, which is a citation rather than a recollection.

> **CORRECTION 2026-08-14: the sub-command in the paragraph above is wrong. The
> CW pitch is `14 09`, not `14 08`.** Sub-command 08 is the outer Twin PBT
> position, and issuing it with a payload would move the passband while trying
> to read a pitch. HM-DEC-050 found it and CLAUDE.md §4 records why: the command
> table is two columns and the extraction behind this ruling had been flattened
> into one, so the description landed against the row above. The range, the
> midpoint and the 5 Hz steps in this paragraph are all correct. Noted here
> rather than edited, because a ruling is never edited (§1).

Still unknown, and marked so: what Windows calls the radio's audio codec. The
manual describes the USB connection and never names the device as an operating
system enumerates it, so that stays configuration and stays in HM-OPEN-003.
`AudioDevice.LooksLikeRadioCodec` matches on "USB Audio CODEC" to PRESELECT a
device and never to claim one is the radio, which is the honest shape for a
guess with no source behind it (§0.0).

---
id: HM-DEC-048
date: 2026-08-14
refs: src/Hamlet.RadioEngine/Cw/, src/Hamlet.RadioEngine/Audio/, src/Hamlet.App/Controls/CwTerminalControl.cs, tests/fixtures/cw/, HM-DEC-007, HM-DEC-009, HM-DEC-026, HM-DEC-027
---

Hamlet decodes received CW, and says how sure it is about every character it
prints. Receive only; the transmit half is not built here.

WHY THIS ONE MATTERS MORE THAN ITS SIZE. CW is the last part of this hobby the
old guard still guards, and the line is that you have to develop an ear, that
the code test kept the riffraff out, that if you cannot read twenty words a
minute you are not really doing it. A decoder that works turns that from a gate
into a preference. It takes nothing from anybody learning to copy by ear, and it
lets somebody who has held a license for six years without making a contact read
what is on the air tonight.

The chain is the standard one and there is deliberately no cleverness in it. A
bank of Goertzel filters finds the note and follows it across the 300 to 900 Hz
the radio can produce, because nobody tunes exactly and a decoder that punished
being off frequency would be teaching the wrong lesson. An adaptive gate decides
where the key is down from a noise floor and a peak that both keep moving, so a
signal sinking through a fade takes the threshold down with it instead of
leaving it stranded. Runs of key-down and key-up become dits, dahs and the three
gap lengths by re-deriving the speed from a rolling window of what was actually
heard. Patterns become characters through a table that is allowed to say no.

PROSIGNS ARRIVE AS PROSIGNS. An operator ending a message sends `.-.-.` as one
run with no gap in it, and a decoder that split that into letters would be wrong
in the most confusing way available: it would read as a mistake in a sentence
rather than as a symbol the reader has not met. Where a pattern has both a
punctuation name and a prosign name they are the same sound on the air, so
choosing `<BT>` over `=` is a naming decision and not a claim about the signal.

THE CONFIDENCE IS THE FEATURE, not a decoration on it. Two measurements and the
worse one wins: how far each element sat from the decision made about it, and
how far the weakest of them stood above the noise and above any station near
enough to be confused with it. Then one veto, for a character that arrived while
somebody else was within a few decibels of the same note, because that failure
is not a matter of degree. High renders normally, low renders dimmed,
unresolved renders as a placeholder and never as a guessed letter. Nothing
anywhere raises a score: not a spell check, not a callsign that nearly matches,
not a word that would make sense.

The reason is specific to this feature. A beginner reading a line of
clean-looking garbage concludes they are the problem, which is exactly what
they have been told for years. Dimmed text says the app is struggling. Clean
text that is wrong says the operator is, and that is a lie this whole project
exists to stop telling.

NOTHING IS CLAIMED FROM AN EMPTY BAND. Noise crosses a threshold constantly, and
a gate handed nothing at all will chop it into runs the right length to be
believed; building against the fixtures produced exactly that, a stream of
confident letters out of twelve seconds of static. So the timings have to look
like Morse before anything is emitted at all: marks clustered near one dit and
three, at a speed a person could actually send. Not marked unreadable, which
would be saying something was heard, but not emitted, because nothing was.

WHEN THE DECODE IS POOR, THE TERMINAL SAYS WHY, in measurements rather than
diagnoses. Fading, faster than Hamlet is following, only just above the noise,
nothing coming through. The constraint is that these describe what the decoder
measured and may not diagnose the band, the antenna, the other operator's
equipment or propagation, and a test sweeps every one of them for the phrases
that would. The one sentence that comes close is deliberate: telling somebody
that a fading signal is not their fault declines to blame them rather than
asserting anything about the ionosphere, and warmth that buys no claim is
exactly what §0.7 allows.

DETERMINISM IS WHAT MAKES ANY OF IT PROVABLE. No clock is read below the audio
seam; elapsed time is counted in samples. The same audio decodes to the same
text on any machine at any speed, which is what lets a test push ten minutes of
signal through in a millisecond and assert an exact string, and what turns a
decoder bug into a regression test rather than an anecdote (HM-DEC-007).

Audio input arrives behind `IAudioSource`, built the way `ISerialPort` was: one
interface, a WASAPI implementation that is the only class in the engine knowing
what a sound device is, and in-memory sources so every decoder test runs without
hardware. `IsSimulated` is get-only on all of them, which is HM-DEC-026's
guarantee carried onto the audio seam: a decode from a fixture cannot reach the
screen dressed as something that was on the air.

THE FIXTURES ARE SYNTHESIZED AND NOT RECORDED OFF AIR, and that is a ruling
rather than a convenience. An off-air recording carries somebody's callsign and
somebody's transmission, which §2.1 makes Tim's to review before it ships;
nothing in `tests/fixtures/cw` belongs to anyone. Seven files, about a megabyte,
eight kilohertz mono, covering three speeds clean plus prosigns, noise, fading
and a second station. Every one regenerates byte for byte from the request
beside it and a test proves it, because a fixture that changed quietly would
take its own assertions with it.

Building against them found three defects worth recording, all the same shape:
the gate deciding things about audio that had nothing in it. The trackers were
fast enough to follow noise, so peak sat on its high points and floor on its low
ones and the gap read as twenty-five decibels of signal on an empty band. A
de-glitch vote was needed for runs shorter than anything anybody sends. And a
level-stability term meant to catch fade-truncated characters was reading
dit-versus-dah composition as level movement and marking clean signals
uncertain; with the gate fixed the fades pass without it, and what remained was
the contested-signal case, which is now a veto.

---
id: HM-DEC-047
date: 2026-08-14
refs: src/Hamlet.App/Controls/FrequencyAxis.cs, src/Hamlet.App/Controls/SpotMarkerStrip.cs, src/Hamlet.App/Controls/DialTapeControl.cs, HM-DEC-015, HM-DEC-023, HM-DEC-006
---

The dial tape carries the same spots the neighborhood map does, as a thin rail
of markers along its top edge, placed by one shared frequency axis and clicked
with the same gesture.

The tape showed nothing while the map showed dots for the same stations on the
same band. A newcomer clicks a spot on the map, arrives at a scale with no
landmarks on it at all, and learns that the tape is decoration. It is not: it is
the fine control, and in phase 2 it is the axis the waterfall paints behind.

ONE AXIS, THREE SURFACES. The map, the tape and the waterfall each asked "where
on my width does this frequency sit" and each answered with its own copy of the
same arithmetic. Three copies of a mapping is three mappings, and the day one of
them rounds differently the operator is looking at a marker that says one thing
on the map and another an inch below it. `FrequencyAxis` is now the only answer:
the map and the waterfall lay the whole band across their width, the tape lays a
few kilohertz across its and slides that window under the hairline, and that is
the entire difference between them.

BUILT FOR THE WATERFALL, USED BY THE TAPE. `SpotMarkerStrip` takes an axis and a
rectangle and knows nothing about either control. Phase 2 gets it by asking. The
gesture is the same one: drag a marker under the hairline and the radio is on
it, click it and the radio jumps there, and when there is real spectrum
underneath, a marker over a smear is what tells the operator that somebody has
already worked out who that is.

The markers stay out of the frequency scale's way, which is why they are a rail
rather than dots. The map scatters its dots through its full height because it
has height to spare and nothing underneath them; the tape's middle belongs to
the ticks and the waterfall's belongs to the spectrum. The scale's labels clear
the rail whether or not anything is on it, because a scale that shifted when a
spot arrived would be worse than either position.

AN EMPTY RAIL IS NOT DRAWN. A permanent groove with nothing in it reads as
"nobody is here", and Hamlet cannot tell that apart from a quiet band, a gap
between two busy patches, or every spot feed being down at once. The panel
summary and the conditions line are where that gets said, and they say which one
it is (HM-DEC-025).

A press that lands on a marker holds the tape still for four pixels before it
becomes a drag. Without it a three-pixel bar is almost impossible to click
without nudging the radio first, and this hobby's median age makes that a
mainstream concern rather than a nicety.

The tape click gets its own telemetry event rather than borrowing the map's. The
two surfaces show the same spots at two zoom levels, and which one people
actually reach for is the question that says whether the tape is earning its
space.

Tested where it can be: a spot placed by the map's axis and by the tape's reads
back as the frequency it actually is, tuning to a marker puts it under the
hairline, and a spot outside the window is dropped rather than pinned to an edge
where it would be claiming a frequency its station is not on (§0.0).

---
id: HM-DEC-046
date: 2026-08-14
refs: src/Hamlet.App/ViewModels/BandOpportunity.cs, HM-DEC-031, HM-DEC-045, HM-DEC-009
---

The best-bet badge is ranked from what Hamlet actually observed, out of the same
spot data and recency the pips, the conditions line and the lead card already
use. The clock heuristic drops to a tiebreaker for when no band has any data at
all, and in that case the badge says it is going on the time of day.

It was still using `BandPlan.BestBets(localHour)`, a lookup table from the first
week, evaluated once at construction and never updated. So it contradicted the
app's own data on the same screen: the badge on 80 m with zero pips, 40 m with
four, and the lead card underneath saying "Try 40 m instead, nothing on 80 m
just now, 40 m has 14 stations". Three surfaces answering one question, and the
loudest of them answering from a table that cannot hear anything.

ONE RANKING, READ BY ALL OF THEM. `BandOpportunities.Rank` returns the order and
everything else reads it. The badge is `BadgeGoesOn`, the lead card's
alternative is `BestOtherThan`, and neither makes a second pass over the data.
That is the difference between agreement being likely and being impossible: a
surface cannot form its own opinion if it is not given the means to.

Count leads, activations break the first tie, and recency breaks the rest. A
park operator wanting contacts is worth more to a newcomer than the same number
of bare skimmer reports, which is the same judgment HM-DEC-045 already makes
about lifetimes.

A GUESS IS ALLOWED AS LONG AS IT ADMITS TO BEING ONE. With nothing heard on any
band the clock is all that is left, and it stands in rather than leaving the row
blank. It does not get to wear the same words: the badge reads "likely, going on
the hour" instead of "best bet now", and the hover says it is going on the time
of day rather than on anything reported. The lead card will not repeat it at
all, because the badge is a hint and the card is an instruction, and sending
somebody to an empty band on a hunch is worse than saying nothing (§0.0).

The agreement is tested the way the banned-phrase sweep is tested: a hundred
generated spot distributions, every band as the one on screen, asserting the
badge lands where the ranking says and that the card never names a different
band. Putting the clock back fails two of them.

---
id: HM-DEC-045
date: 2026-08-14
refs: src/Hamlet.RadioEngine/Explore/SqliteSpotStore.cs, src/Hamlet.RadioEngine/Explore/SpotLifetime.cs, src/Hamlet.App/ViewModels/BandOpportunity.cs, HM-DEC-020, HM-DEC-022, HM-DEC-025
---

Spots persist to a local SQLite store and the display becomes a view over that
history; each source gets a lifetime reflecting how long its spots stay
meaningful; age is spoken in human terms with likelihood claims only where the
source can support them; and feed freshness and opportunity freshness are
separate ideas that must never be conflated.

**SQLite is chosen here rather than inherited.** No prior ruling covered local
storage. HM-DEC-023 is about map dots, and ADIF appears only in FG-004 as a
future goal, so this record is where the choice actually gets made: one file
under `%AppData%\Hamlet` beside settings and telemetry, so everything Hamlet
keeps about a person sits in one folder they can open and delete.

THE OLD BEHAVIOR WAS WRONG IN TWO WAYS AT ONCE. It threw away everything past a
ten-minute window, and it treated the feed's freshness as if it were the
opportunity's. Ten minutes was never a considered figure; it was the window the
band-conditions line happened to use, applied to a question it was not asked. A
person sits down, looks around, tunes to something and listens, and that loop is
fifteen or twenty minutes. A spot from eight minutes ago is not stale to that
person, it is recent.

THE HONEST UNIT IS NOT "WHEN WAS THIS POSTED". It is whether that person is
probably still on that frequency, and the answer genuinely differs by source.
An activator hauled gear to a park or a summit and stays put working whoever
calls, so an hour is generous rather than optimistic. A skimmer report means
somebody called CQ at that moment, which is much weaker evidence about now, so
twenty minutes. Contest stations sit on one frequency for the whole event and
outlast both, and that is claimed only where the source said it was a contest
exchange, never guessed from a busy band. The lifetimes are settings with
generous defaults.

THE LIKELIHOOD LANGUAGE TRACKS THE SOURCE, never a flat rule. "A park activator
spotted twenty minutes ago is probably still working the pileup" is defensible
because that is what activators do. The same sentence about a skimmer report is
not, and a sweep across every age from zero to four hours proves no skimmer
report ever claims it. Age is spoken rather than counted, since nobody says
"17 min ago" out loud, and the exact figure stays one hover away.

TWO KINDS OF FRESHNESS, KEPT APART. Feed freshness is how long since Hamlet last
talked to the network; it belongs in the panel header and is what HM-DEC-020's
amber and stale styling was always about. The header now says "checked", because
"updated" was ambiguous. Opportunity freshness is how long since the spot
happened and whether that person is likely still there; it belongs on the card. A
feed checked four seconds ago can be full of hour-old spots, and an hour-old spot
from a busy afternoon can be worth more than a fresh one at 3am.

THE EMPTY CASE IS THE ONE THAT MATTERS, because it is exactly when a newcomer
gives up. The lead card now looks further back, and then looks at other bands
before declaring anything. "Nothing on 80 m, but 40 m has nine stations, two of
them park activators" is a genuinely useful answer and the app always had the
data for it. "Nothing here worth your next ten minutes" is gone; the give-up
sentence is reachable only when Hamlet has actually looked across every band it
watches, and it then says how far back it looked (HM-DEC-025).

History also closes the Reverse Beacon Network's startup gap. RBN is a live
stream, so a fresh run knew nothing at all until somebody transmitted, and now it
starts with whatever the last session saw.

NEVER BLOCKS AND NEVER CRASHES. Writes run off the UI thread, the store never
throws for storage reasons, and one that cannot be opened degrades to memory with
a note in telemetry, the same discipline the telemetry writer follows (§8).
Pruning keeps a few days, so the file cannot grow without bound.

---
id: HM-DEC-044
date: 2026-08-14
refs: src/Hamlet.App/Views/SettingsWindow.axaml, src/Hamlet.App/Settings/ProfileFactBadge.cs, src/Hamlet.App/Controls/ModePalette.cs, HM-DEC-012, HM-DEC-028, HM-DEC-036
---

The Settings window joins the rest of the app: each section carries its family
color, and the provenance the profile already stores is shown as a badge beside
the field rather than buried in a gray line of small print.

Every other surface in Hamlet uses color to say what a thing belongs to, and
this window was white boxes on white. It now tints per family, reusing the ones
already established rather than inventing any: green for the operator, amber for
the license that governs transmitting, blue for the feeds, and slate for
telemetry, which is the quiet one and keeps a white body.

ONE DEFINITION. The family colors used to be hex literals inside
`CollapsiblePanel.ApplyFamily`, which is exactly why this window could not reuse
them without becoming a second copy. They live in `PanelPalette` beside the mode
language now, and nine literals across six drawn controls were pointed at it.
Two duplications survive on purpose and are tested rather than tolerated: the
theme dictionary has to hold them as XAML resources, so a test asserts the two
representations agree key by key; and the CollapsiblePanel control theme keeps
its hover tint literal so it depends on no application resource.

Each family carries two inks, for contrast rather than taste. The header on warm
paper and the header on that family's own tinted fill are different values,
because the tint lifts the background: amber #C25E00 reaches only 3.84:1 on
#FDF1DE, short of the 4.5 every ink here has to clear (§0.6), while #9A4A00 gets
there at 5.61.

THE BADGE IS A RENDERING OF STORED PROVENANCE AND NOTHING ELSE. A field whose
value a lookup confirmed shows "verified"; a field the operator typed shows
nothing; a field with no recorded source shows nothing. Never inferred, never
assumed, never defaulted. A check mark that does not correspond to a real lookup
is the confident decoration HM-DEC-009 forbids.

To make that possible the profile now records what a lookup actually confirmed,
not merely that one happened: the exact callsign, the exact class reported, the
exact locator derived. Recording what was SEEN is deliberately separate from
adopting it, because a hand-set class is never overwritten (HM-DEC-028) and the
window still has to be able to say what the FCC record holds without the profile
pretending to agree with it.

THE BADGE CLEARS AS YOU TYPE, because it is computed from the current value
against the confirmed one rather than from a flag. Nothing has to be remembered
and reset, it is live on every keystroke rather than on save, and it is still
right after a restart. Typing the confirmed value back brings it back, which is
correct: the badge means "this matches the FCC record", and that is true again
the moment the text matches.

A hand-set value that differs from what the lookup reported shows an amber
"differs from FCC data" pill instead, with both values on hover. The pill is the
signpost and the existing mismatch panel is still where the decision is made.

WHAT "VERIFIED" CLAIMS, AND WHAT IT DOES NOT. It means the value matches a public
FCC record. It is not a check that the operator is who they say they are, and the
tooltip says so in as many words rather than letting anybody assume otherwise.

Nothing is knowable by color alone (§0.6): the pill carries the word "verified"
beside its tick, and the disagreement state says what it means in words.

Profiles written before this know a lookup happened and cannot say what it
confirmed. Rather than backfilling from the current value, which would be a guess
wearing a check mark, such a profile asks again. One request, once.

---
id: HM-DEC-043
date: 2026-08-13
refs: src/Hamlet.RadioEngine/Explore/ContactShape.cs, HM-DEC-021, HM-DEC-041, HM-DEC-042, ONB-006
---

The Explorer carries a panel showing what a contact actually sounds like:
a worked example, both sides, from the first CQ to the sign-off, annotated in
plain language, with Morse and voice as a toggle on the one panel.

THE REAL TERROR IS NOT THE RADIO. It is not knowing what to say. A contact has
a shape, close to a ritual, and everybody knows it except the person who has
never made one. Nothing in the license exam teaches it and no manual writes it
down, because to everybody already doing it the shape is too obvious to
mention. That silence is the last wall, and this takes it down by simply
printing the thing.

Morse and voice share one panel because they are the same shape with different
words, and noticing that is most of the lesson. Learn it once and it works on
any band in any mode.

The example uses the operator's own callsign throughout. Reading your own call
in the worked example is the difference between a manual and a rehearsal, and
it costs nothing to do.

The mechanical parts are explained where they arrive rather than in a legend:
DE is French for "from" and has meant "this is" since the landline telegraph;
K is "go ahead"; BK is a quicker handover between two stations already talking;
SK ends a contact rather than an over. The callsign goes twice because the
first one is often half-missed while somebody is still tuning you in.

TONE MATTERS MORE HERE THAN ANYWHERE ELSE IN THE APP, so it is enforced rather
than hoped for: a test fails the panel if any of its copy says "you must",
"make sure you", "be careful", "required" or "correctly". Nobody should finish
reading this feeling like there is a test. The closing paragraph says outright
that operators get callsigns wrong and forget where they are, and that the
worst realistic outcome is nobody answering, which happens to everybody several
times a week.

Editorial content marked [extrapolated], the same status as the neighborhood
map and the field guide. It is common convention rather than regulation, and
nothing in it is required by anybody.

---
id: HM-DEC-042
date: 2026-08-13
refs: src/Hamlet.RadioEngine/Explore/SignalReport.cs, HM-DEC-025, HM-DEC-041
---

Signal reports are made legible wherever they appear. A spot carrying a
signal-to-noise figure shows what it means in words as well as the number, and
the RST convention is explained in one paragraph wherever a report is shown.

"You're five by nine" is in every contact ever made and nobody explains it. A
newcomer hears a number pair, has no idea whether it is good news, and cannot
tell whether the answer they give back is a lie. The glossary carries the
definition; this carries the part a definition cannot, which is what a given
figure means for the person deciding whether to answer.

The number stays beside the word. "24 dB over the noise, which is strong" gives
the operator both the verdict and the evidence it came from (§0.0.1), and after
a few dozen cards the scale starts to belong to them rather than to the app.

A MEASURED FIGURE AND A REPORTED ONE ARE DIFFERENT THINGS and are kept apart.
The skimmer measured signal-to-noise with a computer. The person guessed,
generously, in a convention where almost everybody says 59 whatever they heard.
Nothing converts between them, because a measured number dressed up as
somebody's opinion would be inventing a courtesy.

The guidance never promises the operator will hear it, and a test holds that
line. A skimmer measured its own receiver on its own antenna, and turning that
into "you will hear this" is exactly the overreach HM-DEC-009 forbids.

---
id: HM-DEC-041
date: 2026-08-13
refs: data/glossary.json, src/Hamlet.RadioEngine/Explore/Glossary.cs, CLAUDE.md §0.7, HM-DEC-034
---

Hamlet marks the jargon in its own copy and explains it on hover, from a
glossary data file, matched automatically at render time. **If Hamlet says it,
Hamlet explains it.**

THE VOCABULARY IS THE GATE. This hobby runs on shared shorthand, most of it
inherited from telegraph operators who died before anybody using this app was
born, and none of it is written down anywhere a newcomer would look. The old
boys club runs on that vocabulary, and handing out the dictionary is the most
direct thing a piece of software can do about it.

THE DEFINITIONS DO EMOTIONAL WORK, not only semantic. That is the difference
between a dictionary and the app being on the operator's side. Where the
etymology demystifies it is included, because knowing why the jargon is strange
makes it feel like an inherited quirk rather than a password somebody forgot to
give you. QRP is five watts and a point of pride rather than a limitation. 73
is never 73s, since the number is already plural. An activator genuinely wants
to hear from you, even if you are slow, even if you are nervous.

MARKING IS AUTOMATIC rather than hand-tagged. Copy is scanned at render time,
so a string written next month inherits the glossary for free and adding a term
lights it up everywhere it already appears. Hand-tagging would guarantee the
opposite: the copy and the glossary would drift apart the first time somebody
was in a hurry, and the drift would be silent (§0).

THE MARK IS QUIET. A dotted rule in a muted brown, visible if you are looking
for it and invisible if you are not. Somebody who has known what CQ means for
forty years should never notice this exists, and nothing anywhere says
"tutorial mode". That restraint is the whole design: the person this is for has
spent six years feeling like the hobby has a password he was never given, and
an app that decorated every third word with a help icon would be saying the
same thing in a friendlier font.

The matching rules exist because a false positive is worse than a miss. Whole
words only, so "band" does not fire inside "bandwidth". Case-insensitive, and
the copy's own casing survives. First occurrence only within a block, or a
paragraph reads as a language exercise. And never inside a callsign or a
frequency, because underlining half of K3CQ would look like the app had misread
something the operator can plainly see.

Matching is a pure function whose runs reassemble into exactly the input, so a
renderer cannot lose or duplicate a character by using it.

---
id: HM-DEC-040
date: 2026-08-13
refs: CLAUDE.md §0.7, tests/Hamlet.App.Tests/VoiceTests.cs, HM-DEC-034
---

The voice standard gains a mechanical constraint: **em dashes are used
sparingly, at most one in a paragraph and usually none.**

A dash is usually a sentence that has not decided where it ends. A comma
carries most of them, and a full stop carries the rest better than either.
Warm writing breathes with periods; short sentences are allowed to land on
their own, and a pause where the reader should reflect is worth more than a
clause bolted on with a dash.

The rule arrived with a sweep rather than only as a note, and the sweep recast
the copy rather than swapping the character for a comma. Reading each passage
back as something somebody would say out loud left several of them shorter than
they started and gave a few the reason they had been leaning on the dash to
imply.

IT IS ENFORCED RATHER THAN RECORDED. `VoiceTests` walks the source, joins runs
of concatenated literals into the passage the operator actually reads, skips
comments and identifiers, and fails on the second dash. A rule that lives only
in CLAUDE.md is a rule the next session rediscovers by breaking it. The sweep
was checked against a deliberate violation before being trusted, because a
directory-walking test that silently matches nothing passes forever.

What is outside it: records, comments and code. This file and CLAUDE.md are
written for whoever maintains Hamlet rather than for the operator, and they are
deliberately full of dashes. The rotating bylines are outside it too, in effect
rather than by exception: each is a single line carrying at most one dash,
where the dash is the joke's pivot.

---
id: HM-DEC-039
date: 2026-08-13
refs: data/bylines.json, src/Hamlet.App/Bylines.cs, HM-DEC-034
---

A line of Shakespeare, bent toward ham radio, sits under the wordmark — one of
forty-five, drawn at random each launch, never the same one twice running, with
the play it came from on hover.

The point is joy. Ham radio is intimidating, which is the whole reason this app
exists, and a small daily chuckle costs nothing and softens the thing. It is
the only feature in Hamlet that is there purely to be liked, and that is a
sufficient reason for one file.

The play is a tooltip rather than permanent text, so the joke stays legible to
somebody who does not know the original without the wordmark turning into a
citation. The index shown is saved immediately rather than at shutdown, because
an app that is killed rather than closed would otherwise show the same line
forever, and a surprise that repeats is a fixture.

Shakespeare died in 1616, so the source text is long out of copyright and these
alterations are the project's own. Nothing here needs anybody's permission
(§2.1).

NEVER A PLACEHOLDER. A missing, malformed or empty file means there is no
byline at all — not a line reading "byline unavailable". This runs while the
main window is being constructed, and a decorative feature that could stop the
app from opening would be a spectacularly bad trade (§8).

---
id: HM-DEC-038
date: 2026-08-13
refs: src/Hamlet.RadioEngine/Explore/SpotDistance.cs, HM-DEC-023, HM-DEC-025, HM-DEC-037
---

Happening-now cards and map dot tooltips carry how far away a station is and
roughly which way — "530 miles west-northwest" — computed from the operator's
coordinates and the station's. Miles by default, with a setting behind it.

WHY IT IS WORTH THE WORK. A newcomer has no sense of what distances are
plausible on which bands and no way to acquire one: they see a callsign and a
frequency, work out nothing from either, and the intuition every experienced
operator has and none of them can explain stays out of reach. After a few dozen
spots reading "530 miles" beside 40 m and "4100 miles" beside 20 m, the shape
of it starts to arrive on its own. It is a teaching device that happens to look
like a label.

THE DISTINCTION THAT MAKES IT HONEST. The two things a spot network can tell
you about location mean opposite things. POTA states where the park is and the
activator is standing in it, so that is the station. RBN states which skimmer
decoded the signal — where somebody who HEARD it is — and a distance attached
to that would be a straightforward lie about the transmitter. So the field is
named `StationLocation`, only POTA fills it, and RBN spots carry no distance at
all. The skimmer's callsign prefix could be turned into a country, but a
callsign says where a license was issued and not where its owner is standing,
and stacking that guess under a figure in miles would dress it as a
measurement.

SOTA leaves it null too. Summits have coordinates in principle and the current
parser does not read them; an empty field is the honest state until it does.

NO GRID MEANS NO DISTANCE, anywhere, on any card or any dot. Not an estimate
from the location string, not a country-sized guess from a prefix. The figure
is rounded to two significant figures because it is a distance to a park's
stated reference point, and "483 miles" would claim a precision nothing in the
chain supports.

Bearings are given as one of sixteen compass points rather than in degrees.
"480 miles northeast" is a direction a person can picture; "480 miles at 47°"
is a reading off an instrument (§0.7).

---
id: HM-DEC-037
date: 2026-08-13
refs: src/Hamlet.App/Licensing/GridResolver.cs, src/Hamlet.RadioEngine/Explore/OperatorLocation.cs, HM-DEC-028, HM-DEC-033, ONB-C01
---

The grid square is derived from the coordinates the callsign lookup already
returns, resolved lazily and automatically like the license class, stored with
its own provenance, and never overwritten once the operator has typed one.

"Maidenhead grid locator" is exactly the kind of jargon Hamlet exists to
dissolve, and it is a barrier with nothing behind it. The FCC's record of the
license already carries coordinates, callook republishes them, and the locator
is arithmetic on top — no service, no key, nothing that can be down. So the
operator is never asked to look theirs up: the field fills itself, and the one
line beside it says what the thing is in words somebody would use, a short code
for where you are, a bit like a postal code for the planet.

This is the piece that makes HM-DEC-033 visible. Tim's profile had a callsign
and an empty grid, so no band card dimmed and every icon was a hollow ring —
correct behavior and an invisible feature. It resolves on the next launch.

THE COORDINATES ARE THE STORED FACT and the locator is a rendering of them.
Distance, bearing and the solar clock all want degrees, and a locator only ever
gives them back to within a few miles. callook sends a `gridsquare` field and
Hamlet reads past it deliberately: one derivation cannot disagree with itself
the way two stored values can. The tests check Hamlet's arithmetic against
callook's own answer, which arrives by a different route.

A HAND-ENTERED GRID IS NEVER OVERWRITTEN — the whole of HM-DEC-028 applied a
second time, and it binds harder here. The FCC holds a mailing address, not an
antenna, and somebody operating portable or from a club station knows where
they are far better than it does. A disagreement shows both and the operator
chooses. The comparison is at four characters, because an antenna sitting in
FN00DJ rather than FN00DK is not a disagreement worth interrupting anybody
over.

NEVER GUESSED FROM THE LOCATION STRING. "Trafford, PA" names a call district,
which is a lookup in a published table, and it does not place a station within
seventy miles. A grid derived from a town name would be a guess wearing the
clothes of a measurement (§0.0). No coordinates means the field stays empty and
hand-editable, and Hamlet does without.

The coordinates are more identifying than a class, so HM-DEC-019 binds harder:
they are written to the local profile, shown to the operator, and never entered
into telemetry.

---
id: HM-DEC-036
date: 2026-08-13
refs: CLAUDE.md §0.6, CLAUDE.md §6.1, HM-DEC-032, HM-DEC-035
---

Two corrections Tim ruled on, both to records this session's predecessor
raised rather than settled.

THE CONTRAST FLOOR. HM-DEC-032 recorded that the "open / mixed" ink reached
only 4.09:1 against its fill, short of WCAG AA's 4.5, and left raising it to
Tim. He ruled the carve-out away rather than the shortfall: the ink darkens
from #6E6A61 to #5F5C53, which measures 5.07:1, and the test floor that was set
at 4.0 to accommodate the gap now applies 4.5 to all four families with no
exceptions. It is the least colorful and least meaningful of the four, so the
change costs nothing visually — and this hobby's median age makes contrast a
mainstream requirement here rather than an accessibility footnote. §0.6 carries
the rule and the corrected value.

THE TOOL SCRIPTS ARE EXEMPT FROM THE SPELLING STANDARD. The US-spelling sweep
(HM-DEC-035) changed one `rem` comment inside `get-files.template.bat`, a file
§9.4 makes verbatim. Tim ruled it reverted, byte-identical, and the reasoning
generalizes: a rule with a "but it was only a comment" exception is not a rule,
and the whole value of those scripts is being known-good and untouched. §6.1
gains a third exception covering the canonical scripts under `tools/`
entirely — `get-files.template.bat`, `get-listing.bat` and `GoClaude.bat` are
all restored and their spelling is frozen at whatever it is.

Neither of these supersedes its parent ruling; each corrects one measurement or
one boundary inside it, and the parent stands otherwise.

---
id: HM-DEC-035
date: 2026-08-13
refs: CLAUDE.md §6, src/Hamlet.App/Settings/SettingsMigrations.cs, HM-DEC-028
---

American spelling is the project standard, in code, comments, prose, records
and UI text alike. Two exceptions: a quoted external source keeps its own
spelling verbatim, and a rename that changes a stored settings key ships with a
migration and a test that proves an existing profile survives it.

Hamlet is written for US operators, against the FCC's Part 97, by an operator
in Pennsylvania, and it is heading for a public repository where the
contributors it hopes to attract will be American too. The prose was drifting
between the two conventions — "licence class" beside `Hamlet.RadioEngine.
Licensing`, "colour" beside `ColorHex` — and mixed spelling in a codebase is
not a style question. It splits identifiers, it splits searches, and it makes a
newcomer guess which convention this file uses.

The quoted-source exception is the same rule as §4's vendored citations: a
quotation that has been tidied is no longer a quotation. SOTA's terms and the
CFR are reproduced as they were written.

The migration exception is the one with teeth. Renaming `LicenceClass` to
`LicenseClass` renames the key it is stored under, so the first launch after
the upgrade would find no `LicenseClass`, take the default, and forget that Tim
is General and that callook.info established it on 13 August. Nothing would
crash and nothing would say a word — it would simply look like the app
forgetting who he is, which is the worst thing a piece of software can do to
somebody who has just started trusting it. `SettingsMigrations` reads the old
keys when the new ones are absent, the new key always wins when both are
present, and the whole of it is proved against a copy of his actual file.

What follows and should not be re-argued: a spelling change to a public
identifier is not cosmetic. If it is persisted, it needs a migration; if it is
in a quote, it does not happen at all.

---
id: HM-DEC-034
date: 2026-08-13
refs: CLAUDE.md §0.7, src/Hamlet.RadioEngine/Explore/BandCharacter.cs, HM-DEC-016, HM-DEC-009
---

Hamlet's explanatory prose is written as connected speech — a patient friend
with forty years on the air explaining it while you both look at the radio —
not as a stack of short declarative facts.

This is a standing rule, not a note about one tooltip, because the whole
product is an argument that this hobby can be explained. The person it is for
has held a license since 2020 and has never made a contact; what stopped him
was not a missing feature but the absence of anybody to explain the thing
plainly. An app that answers him in clipped fragments — "80 m. Night band. High
absorption in daylight." — has the facts right and has still failed, because it
sounds like the manuals that already did not help.

So: thoughts run into one another, the reason is attached to the fact rather
than left implied, ordinary words beat correct ones where they differ, and a
number is spoken rather than counted — "the sun went down about an hour and a
half ago", never "sunset was 94 minutes ago".

The rule does not soften §0.0. Warmth is a matter of how a thing is said and
never of what is claimed; a friendly sentence that overstates what Hamlet knows
is a worse failure than a cold one, because it is more readily believed.

Existing copy written before this ruling is not all compliant. It is corrected
where it is touched rather than in one sweep, so the change arrives with the
work that gives it context.

---
id: HM-DEC-033
date: 2026-08-13
refs: src/Hamlet.RadioEngine/Solar/SolarClock.cs, src/Hamlet.RadioEngine/Explore/BandCharacter.cs, src/Hamlet.App/ViewModels/BandCardStyle.cs, data/vendor/usno/, HM-DEC-015, HM-DEC-031, FG-007
---

The band buttons become character cards: width follows wavelength, a drawn sun
or moon says when the band is in its element, the card dims when it is not, and
hovering gives a passage of plain prose about what the sun and the season are
doing to it. Sunrise and sunset are computed from the operator's own
coordinates. Activity pips and the best-bet badge are unchanged.

A row of identical rectangles labeled 80 through 10 teaches nothing. The row
is the first thing anybody touches and it was carrying one bit of information
per band. Now it carries four, and the most valuable of them is the one nobody
ever explains: "80 meters" is a long wave and "10 meters" is a short one. The
width says so without a word of copy, and once that lands, the rest of the
band's behavior stops being arbitrary. The scale is logarithmic — true
proportion would make 80 m eight times the width of 10 m and wreck the row.

Two departures from the brief, both found by running it. The width span asked
for was 58 to 104; at 58 the card clipped "10 m" to "10 n" with the icon on top
of the label, so the span is 76 to 122 and the ratio is kept close. And the bar
was to run as a continuous hue ramp from the night blue to the day amber along
the wavelength axis — on screen the middle of that ramp is gray, because two
near-complementary hues interpolated in RGB pass through neutral, and 40 m and
30 m came out looking dead. The bar now carries the band's element in three
saturated stops (blue, teal, amber), which is what the card is about anyway and
agrees with the icon beside it.

WHAT MAY BE SAID. Where the sun is, is arithmetic about the solar system: it is
computed, it is stated plainly, and it needs no hedge. Whether a band is open
is a fact about the ionosphere that Hamlet cannot see (FG-007), so no card and
no passage says a band is open, closed, dead or working, and none tells the
operator what they will reach. "20 m lives on sunlight and right now it's got
it" is a claim about sunlight. "20 m is open" is a claim about the ionosphere.
The first is allowed and the second is not, and a banned-phrase sweep over
every band at every hour in every season holds the line.

NO LOCATION MEANS NO CLAIM. Without coordinates nothing dims, the icon is a
hollow ring rather than a sun or a moon, and the prose says what the band is
like in general and how to fix the gap. A card faded on a guessed location
would look exactly like a real judgment (HM-DEC-009).

The arithmetic is Hamlet's own — the Almanac for Computers equation — rather
than a service with a key that can be down. It agrees with the US Naval
Observatory to within a minute at both solstices, an equinox, the equator and a
longitude east of Greenwich; the responses it was checked against are vendored
under `data/vendor/usno/` and the tests read their expectations from there
rather than from anybody's memory.

The band character text is engine editorial marked [extrapolated], the same
status as the neighborhood map and the field guide.

---
id: HM-DEC-032
date: 2026-08-13
refs: CLAUDE.md §0.6, src/Hamlet.App/Controls/ModePalette.cs, src/Hamlet.RadioEngine/Explore/ModeFamily.cs, HM-DEC-012, HM-DEC-016
---

Mode families have one color language across the whole app: Morse amber
(#EDC375 on #5E3800), digital lavender (#BFB6E4 on #2B2360), voice blue
(#A3CBE8 on #0B3B5C), open or mixed neutral (#E4E0D5 on #6E6A61). One palette
file, read by every surface. The neighborhood map fills from the family a
neighborhood declares rather than from a color literal it carries, and the map
gains a legend.

Color carries meaning here, so it may never be the ONLY carrier of meaning.
Roughly one man in twelve has a color vision deficiency and this hobby's
demographics make that a real slice of the people who will use this. Every
segment is labeled, the legend names each family in words, the listen-only
veil hatches as well as tints, and the band cards carry an icon and a width
beside their hue. Anything added later inherits that obligation.

The old fills separated by lightness alone — a pale amber beside a pale pink
read as one wash at a glance — and pink was doing double duty as both "phone
segment" and, under hatching, "listen only", so the veil meant two things at
once. These four separate by hue and temperature, and none of them is pink.

`ColorHex` is gone from `Neighborhood`. A per-neighborhood color literal is a
second copy of the language, and a second copy drifts silently; a test asserts
that no file outside the palette carries one of its hex values.

The colors themselves are Tim's ruling. Where a test sets a threshold — ΔE
between fills, contrast between ink and fill — it is a floor that guards them,
not a target that chose them. One measurement is recorded rather than hidden:
"open / mixed" reaches contrast 4.1 against WCAG AA's 4.5, because its ink and
fill are both deliberately near-neutral. Raising it is Tim's call.

---
id: HM-DEC-031
date: 2026-08-13
refs: src/Hamlet.App/ViewModels/BandActivity.cs, src/Hamlet.App/Controls/ActivityPipsControl.cs, HM-DEC-009, HM-DEC-020, HM-DEC-022, HM-DEC-025, FG-007
---

Every band button carries an activity indicator computed from the spots
already in hand, with hover detail supplying the evidence. Counts are a proxy
for ACTIVITY, never for propagation — the app reports what was heard and does
not assert what the ionosphere is doing. A band with no data and a band with
no spots are visually and textually distinct.

The band buttons are the first control anybody touches, and six of the seven
said nothing. A newcomer picking a band was guessing, while the data to answer
them was already flowing through the Explorer.

THE HONESTY CONSTRAINT. A spot count says where skimmers are and where
activators went. It does not say whether this operator can work anything, and
it does not say whether a band is open. So nothing in the tooltip asserts
propagation. There is exactly one hedged sentence — "likely closed rather than
unwatched" — and it is reachable only when every source that can see the band
is healthy and reporting zero, names the possibility it cannot rule out, and
is withdrawn the moment any source goes quiet. A test walks every combination
of spot set and source health and fails on a banned phrase.

WHICH FORCED A CHANGE IN THE ENGINE. RBN is filtered to the band on screen
(HM-DEC-024), so its silence about 17 m is not evidence about 17 m — it is
evidence that nobody asked it. Crediting that silence would have produced
"POTA and RBN are both answering, so 10 m is likely closed" about a band RBN
never looked at: a confident claim manufactured from a source that was not
listening, which is exactly what HM-DEC-025 exists to prevent. So a source can
now declare the band it is scoped to, the aggregate publishes that on every
status, and each band is summarized only from the sources that can actually
see it. Verified live: the current band's tooltip reads "From POTA and RBN"
while every other band reads "From POTA".

NO DATA IS NOT ZERO. "I cannot see this band" and "I am watching and hearing
nothing" are different claims and never render the same. No data draws hollow
dashed pips and says "no enabled source is reporting on this band right now";
nothing heard draws solid empty pips and says what was watched. That
distinction is the visual form of the rule the text is already careful about.

THE SCALE IS RELATIVE, AND NOT LINEAR. Relative because "34 signals" means
nothing without knowing whether 34 is a lot tonight; the busiest band right
now sets the top of the range. Not linear because band activity is heavily
tailed — one band routinely carries several times the traffic of every other —
and a linear scale across four pips put almost everything in the bottom bucket,
which a test caught. The square root of the ratio spreads them, and
compressing the top of a range is the idiom of the domain anyway: S-meters and
signal reports are logarithmic for the same reason.

The indicator is kept quiet. The buttons still have to read as buttons, and
"best bet now" stays the single editorial call on top; this is a softer second
signal underneath it.

Pure function of spots, an elapsed window and source health, sharing
`BandConditions`' window so a button and the line under the map are never
counting different minutes. No clock read (§5).

What this cannot do is say WHY a band is empty. That needs propagation data,
which is now FG-007.

---
id: HM-DEC-030
date: 2026-08-13
refs: src/Hamlet.RadioEngine/Rig/RigCapabilities.cs, HM-DEC-003
---

`IRig` gains a capabilities record — model, spectrum scope, built-in keyer,
USB audio, whether it can transmit, and which bands it covers — and the UI
degrades honestly on a radio that lacks a feature rather than showing a
control that cannot work.

HM-DEC-003 confined Hamlet to one radio behind an interface and named
multi-rig support as the condition for revisiting. This is that revisit
arriving early and cheaply, while there are still only two implementations to
change. Every assumption about the IC-7300 that lives at a call site is a
place a second radio will break, and they are much easier to remove now than
after phase 2 has built a scope UI on top of them.

Capabilities are reported by the implementation and have no setter, the same
shape as `IsSimulated` and for the same reason: a radio is the only thing that
knows what it is. `RigCapabilities.Unknown` claims nothing at all, so a radio
that has not said cannot inherit the 7300's feature set by default — which is
the assumption the type exists to remove.

The training radio claims a spectrum scope, because the synthesiser genuinely
is one, and refuses transmit, because there is nothing behind it to transmit
with. That is the one claim it must never make.

---
id: HM-DEC-029
date: 2026-08-13
refs: data/privileges/us-part97-privileges.json, src/Hamlet.RadioEngine/Licensing/PrivilegePlan.cs, src/Hamlet.RadioEngine/Licensing/TransmitGuard.cs, src/Hamlet.App/Controls/NeighborhoodMapControl.cs, HM-DEC-009, HM-DEC-008, HM-OPEN-005
---

US Part 97 transmit privileges are cited data under `/data`, not carried
knowledge. The band map shows them as a veil over the culture map, tuning is
never restricted, the status line explains rather than scolds, and an
unresolved license class draws NO overlay rather than a guessed one.

THE ONE FACT THAT DOES THE MOST WORK: listening is never restricted. Any
license may receive anywhere; the rules are about transmitting. The operator
this serves has been licensed six years and has never made a contact, and part
of that is a quiet fear of transmitting somewhere he is not allowed. Every
piece of this is shaped to make that distinction plain rather than to imply
the band is full of forbidden zones — which is why the veil is faint enough to
read the neighborhood color through, why it is labeled "listen only", why
the reassurance sentence appears whenever transmitting is restricted, and why
the tone is amber and never red. Being outside your privileges while tuning
around is not an error. It is the ordinary state of most of the band for most
licenses, and the app should sound like it knows that.

The data is a transcription of 47 CFR 97.301, 97.305 and 97.307, read from
eCFR's versioner API on 2026-08-13, with the paragraph cited on every row.
This has legal weight and must not come from anybody's memory. The ARRL band
chart is named as the familiar rendering it is, and marked "convenience": where
it and the CFR ever differ, the CFR wins.

The two CFR tables are carried SEPARATELY, as the regulation carries them, and
the join that answers "may this class send this mode here" happens in code with
tests. A pre-joined table would be a third artefact free to disagree with both
its parents (§0). What that join has to know is not obvious — 97.305(a) puts CW
on any frequency the class may use, so CW is absent from the emission table
entirely; 97.307(f)(9) makes a Technician's HF privileges Morse-only;
97.307(f)(11) keeps 7.075–7.100 phone away from the contiguous US. Each of
those is a test.

Figures the sources do not state are explicit unknowns with reasons — 60 m's
five channels, VHF and UHF, power limits, Regions 1 and 3. A Technician's 2 m
privileges are their most-used privilege, and a file that stayed silent about
omitting them would read as complete.

TUNING NEVER RESISTS. The operator may tune anywhere, including deep into
Extra-only territory: nothing blocks, nothing pops up, nothing beeps. The
marker turns red with a small flag, the dots outside privileges dim rather than
vanish — the operator still needs to see where the action is — and the status
line says what is true. The upgrade ladder appears on click and never as
permanent chrome, and collapses again the moment the frequency becomes theirs.
Restriction becomes motivation; the same words shown unbidden would be a nag.

AN UNRESOLVED CLASS DRAWS NOTHING. Not a permissive overlay, not a restrictive
one. `SpansFor` returns an empty list for an unknown class, so "do not guess"
is structural rather than a rule the control has to remember, and the map looks
exactly as it did before privileges existed. This is HM-DEC-009 at the one
point in Hamlet where a confident error has legal consequences.

THE SPANS ARE THE ONE SET OF BOUNDARIES. They are computed once, in the
ViewModel, from the cited data, and handed to the map. If the waterfall or the
dial tape ever shows privileges they take the same list rather than computing
their own — two pictures of one law that disagreed would be worse than either
alone.

THE GUARD RAIL IS TRANSMIT ONLY. "Only let me transmit where my license
allows", on by default, consulted at exactly one moment: before Hamlet keys a
transmitter. It is never asked about tuning, receiving or drawing. No transmit
path exists yet — HM-DEC-008 gates keying on the vendored manual — so the
setting, the check and its tests are built now and THE SEAM IS THIS: whatever
first keys the transmitter, CI-V 0x17 or PTT, calls `TransmitGuard.Check` and
honors the answer. The override is passed per call rather than read from
settings, so it can live beside the transmit control: somebody deliberately
keying outside their privileges should reach for it consciously, and somebody
tuning around should never meet it. An unknown class does not block
transmitting — Hamlet has no business refusing to key a radio because a lookup
service was down.

---
id: HM-DEC-028
date: 2026-08-13
refs: src/Hamlet.App/Licensing/LicenseResolver.cs, src/Hamlet.RadioEngine/Licensing/CallsignLookup.cs, HM-DEC-019, HM-DEC-024
---

The operator's license class lives in the profile with its provenance, is
resolved lazily and automatically whenever a callsign is present and the class
is unknown, and a lookup never silently overwrites a hand-set value — a
mismatch is shown with both values and the operator decides.

LAZY, NOT A WIZARD STEP. People skip wizards, and the callsign can arrive from
Settings, a hand-edited settings file or a version that never asked. So
resolution is attached to the fact rather than to a screen: on startup and
whenever the profile changes, a callsign with no class gets looked up. It never
blocks and never opens a dialog. The status bar narrates — "Looking up
KC3QIS…", then "General — from FCC data, today." — and that visible competence
is the point.

Provenance travels with the value. "General, from FCC data, today" and
"General, because you said so in 2019" are different claims and the operator is
entitled to see which they are looking at.

A LOOKUP NEVER OVERWRITES A HAND-SET CLASS. If the operator set General and the
FCC data says Extra, both are shown with the source and the date and nothing is
written until they choose. It is their license. Software that silently
corrected them would be wrong even on the occasions it was right. Declining is
an answer, and the profile is re-stamped so the same question does not reappear
tomorrow.

THE SERVICE, AND ITS TERMS. callook.info, which republishes FCC ULS data. Its
API reference states under a "Usage Terms" heading: "The callook.info API is
publicly available and is free to use however you wish." No rate limit, no
attribution requirement, no restriction on automated access, and nothing about
how the software was written. Read 2026-08-13. Unlike SOTA (HM-DEC-024) nothing
in those terms stands in the way, so this ships on. Politeness is still
self-imposed: the User-Agent names the app, its version and the operator.

WHAT IS READ, AND WHAT IS NOT. The response carries the licensee's full name
and street address. Hamlet reads the operator class and nothing else, and the
result type has nowhere to put the rest. It is the operator's own record, but a
program that quietly harvested a home address because it happened to be in the
payload would be doing something nobody asked for.

The callsign goes to the lookup service — the class is public information, as
public as the callsign itself, and it is in the FCC's own searchable database.
It still never enters telemetry. HM-DEC-019's rule is unchanged and the privacy
walk grew to cover the five events this work added.

NOBODY IS EVER BLOCKED. The ladder is API lookup, then hand selection in
Settings, and an unresolved class is a supported state throughout: the band map
draws no overlay and says why, and the guard rail lets transmissions through
while saying what it does not know. The FCC bulk-download rung named in the
brief is not built; the API and hand selection cover every case reached so far,
and a 100 MB download offered to somebody whose lookup merely timed out would
be worse than the "try again later" they get now. Recorded here so the next
session knows it was a decision rather than an omission.

---
id: HM-DEC-027
date: 2026-08-13
refs: src/Hamlet.App/Controls/WaterfallControl.cs, src/Hamlet.RadioEngine/Training/ModeAudio.cs, HM-DEC-006, HM-DEC-005, HM-DEC-012, FG-002
---

The waterfall renderer is built now, against synthesised frames of the same
shape CI-V `0x27` will deliver, so phase 2 swaps the data source and not the
UI. The field guide's audio is synthesised rather than recorded.

Building the renderer against a fake source is not a compromise, it is the
point. `SpectrumFrame` carries a span, a timestamp and a run of one-byte
amplitudes because that is what the IC-7300's scope reports; when the radio
starts filling those frames the control does not change. And a renderer that
exists is a renderer being exercised — the alternative was writing it blind in
phase 2 against hardware, with no way to tell a rendering bug from a CI-V
parsing bug.

Built as HM-DEC-006 requires: the control owns a `WriteableBitmap` and
subscribes to the engine's event directly, and no spectrum data passes through
data binding. Frames arrive on the source's thread, which does nothing but
write ints into a plain array under a short lock; a UI-side timer copies that
array into the bitmap. Measured at 0.012 ms to synthesise a frame and 0.006 ms
to scroll and map it, against a 40 ms budget at twenty-five frames a second.

The waterfall is a dark instrument surface on warm paper. That is consistent
with HM-DEC-012 rather than an exception to it — the same reasoning already
applied to the rig's LCD. Faint detail is what a waterfall is for, and faint
detail on white is unreadable.

Clicking the waterfall tunes to that frequency. It shares its frequency
mapping with the dial tape and the neighborhood map, so a click lands where
the operator is pointing and all three markers move together. That is phase 2's
click-a-signal gesture, built early because the training radio makes it useful
before any hardware exists.

Field-guide audio is generated, not recorded. Recorded clips carry a license
and a provenance question into a GPL-3.0 repository, cannot be parameterised,
and cannot be asserted on. Generated audio is license-free, byte-for-byte
deterministic, testable, and adjustable — CW at 12, 18 and 25 WPM is how
somebody finds the speed they can actually copy, which is the groundwork FG-002
needs. SSB is offered tuned and mistuned side by side, because hearing those
two back to back is the fastest way to learn what the tuning knob is for. Each
card's fingerprint is animated by the same synthesiser that draws the
waterfall, so the picture on the card and the picture on the panel are the same
picture and recognizing one is recognizing the other.

---
id: HM-DEC-026
date: 2026-08-13
refs: src/Hamlet.RadioEngine/Training/TrainingSpectrumSource.cs, src/Hamlet.RadioEngine/Rig/TrainingRig.cs, src/Hamlet.RadioEngine/Training/TrainingBandPlan.cs, HM-DEC-009, HM-DEC-016
---

The simulated radio is a training feature, not a test double. The waterfall
states that its signals are simulated whenever the connected rig is simulated,
and that statement is derived from connection state rather than set, so the app
cannot show synthetic signals unlabeled. Synthesised signals sit at real
band-plan frequencies, so practice teaches the real band.

`FakeRig` becomes `TrainingRig` and the port list says "Training radio (no
hardware)". Someone licensed since 2020 who still cannot tell one signal from
another needs to practice, and practicing on the air means owning a radio,
having an antenna up, and hoping the band is open tonight. Here they can learn
the waterfall and the sound of each mode with nothing plugged in. It still
backs UI development and engine tests; what changed is that it is now something
the operator chooses on purpose.

CONNECTION STATE IS THE MODE, and structurally so. `IRig.IsSimulated` and
`ISpectrumSource.IsSimulated` are get-only properties answered by the
implementation, and the shell's label is a derived property with no setter
either. There is no practice mode to enter, no watermark toggle, and no
setting that could put synthetic signals on screen unlabeled — not because
everyone remembers not to add one, but because there is nothing to add it to.
Tests assert the absence of a setter at every level and fail if a settings
property with a suggestive name ever appears. This is HM-DEC-009 made
structural: the honest thing is the only thing the type system permits.

Signals are placed by reading `NeighborhoodPlan`, never by writing frequencies
down again. Each neighborhood's own label says which modes it hosts, so CW
lands in the CW segments, FT8 in FT8 city, voice up in the phone segment, and
the fast lane sends at contest speed while main street stays copyable. A second
copy of the band plan would drift from the first, and the day it drifted the
app would be teaching a band that does not exist while the map beside it said
otherwise. A test walks every signal on every band and asserts it landed in a
neighborhood documented to host its mode.

Each mode carries its real bandwidth — 31 Hz for PSK31, 50 for FT8, 150 for CW,
2.4 kHz for SSB — and its real rhythm: FT8 synchronized to the UTC
quarter-minute, CW keyed at the stated WPM by the PARIS standard, RTTY
alternating between two tones 170 Hz apart. Those numbers are the lesson. A
width chosen because it drew nicely would teach a falsehood to someone who has
no way to check it yet.

Synthesis is deterministic given a seed, with elapsed time passed in and no
clock read anywhere below the frame pump, so a test can assert on exact bytes
and a practice session can be replayed.

---
id: HM-DEC-025
date: 2026-08-13
refs: src/Hamlet.App/ViewModels/SpotRanking.cs, src/Hamlet.App/ViewModels/LeadCard.cs, src/Hamlet.App/ViewModels/BandConditions.cs, HM-DEC-009, HM-DEC-020, HM-DEC-024
amends: HM-DEC-020
---

The happening-now list is ranked for how good a next ten minutes each spot
would make for a newcomer, every card states its reason on its face, a lead
card gives one written suggestion with its rationale, and a band-conditions
line reports what is happening with the evidence beside it — softening its
language when the sample is thin, naming the sources that did not answer, and
saying outright when Hamlet cannot see the bands at all.

The operator this serves has held a license since 2020 and still does not know
where to start. He has spent hours tuning across a band with nothing on it,
unable to tell whether the band was dead or he was in the wrong place. A list
of spots does not fix that. One sentence telling him where to point the radio,
why it suits him, and what he will hear when he gets there does.

Ranking is a pure function of spot fields and an elapsed time passed in, so it
is testable exactly and the same set always ranks the same way (§5). What earns
points: park and summit activations, because that operator went somewhere on
purpose to be called and will be patient with a beginner; a CQ over a contest
run over an unlabeled spot; slower CW over faster; close and strong over
marginal, including how many receivers heard it; and fresh over old.

Two weightings were added after watching the live feeds rather than reasoning
about them, which is the only reason they exist. Beacons carry a penalty larger
than every positive component combined — a beacon is strong, close, steady and
permanently useless for a contact, so it scored well on every axis that was not
the point. And FT8 is pushed below workable modes: it swamped the top of the
list on real data, Hamlet cannot decode it until phase 3, and recommending it
amounted to telling a beginner to go and watch a waterfall.

Every card carries its reason because a card ranked highly with nothing said
about why is a guess presented as a decode (HM-DEC-009). The reason is not
written separately from the score — the same pass produces both, so the two
cannot drift apart. It is text on the card, never a tooltip.

The refusal cases are the ones that matter. When nothing clears the bar the
lead card says so and says what to do instead; when no source is answering it
says Hamlet cannot see the bands, which is a different sentence from "the band
is quiet" and must never be collapsed into it. A silent band and a broken feed
produce identical spot counts, so counts alone can never tell them apart and
the source statuses are an input to the conditions line rather than a detail of
its plumbing. Hamlet never invents calm. "Nothing here, try 40 m" is a
successful outcome — it is the outcome that saves this operator an evening.

This amends HM-DEC-020 to exactly one extent. That ruling said the list is not
re-sorted on every tick, because moving a card out from under a reading
operator's cursor costs more than a perfect order. That still holds: the
one-second age tick only re-ages text. Ranking reorders on a data refresh only —
a deliberate, minutes-apart event where the content genuinely changed.

---
id: HM-DEC-024
date: 2026-08-13
refs: src/Hamlet.RadioEngine/Explore/PotaActivitySource.cs, src/Hamlet.RadioEngine/Explore/SotaActivitySource.cs, src/Hamlet.RadioEngine/Explore/RbnActivitySource.cs, HM-DEC-018, HM-DEC-019, HM-DEC-022, FG-001
---

POTA, SOTA and RBN are implemented behind `IActivitySource`. Every HTTP request
names the app, its version, the project URL and the operator's callsign; each
source floors its own poll rate under whatever the operator sets; and RBN is
filtered to the band on screen and to skimmers on the operator's continent. The
callsign goes to these services and still never goes to telemetry.

Endpoints and field names were read off the live services on 2026-08-13, not
recalled. POTA returns frequency in kilohertz as a string; SOTA returns it in
megahertz; getting that backwards would put every summit spot a thousand times
off frequency, which is exactly the class of error that guessing a field name
produces. Both parsers are tested against captured responses, and no test in
this repository reaches the internet — a test that needed POTA to be up would
fail for reasons unrelated to the code and would stop proving anything the day
the response shape changed.

On identity: these are volunteer-run services with no rate card and no support
contract. An operator whose client misbehaves should be reachable, and an
anonymous client cannot be warned before it is blocked. Sending the callsign to
POTA is the courtesy the service is owed; writing the same string into Hamlet's
own telemetry file would be surveillance of the operator by their own software.
The two are different acts and only one is permitted. HM-DEC-019's rule is
unchanged, and the privacy walk grew to cover the four events this work added.

RBN delivers about six spots a second worldwide, so what reaches the list is cut
twice: to the band on screen, and to skimmers on the operator's own continent. A
German skimmer hearing a German station says nothing about what is audible from
Pennsylvania. Continent and not call district is deliberate — on HF a skimmer
eight hundred kilometers away hears very nearly what you hear, so a tighter
filter would discard good spots for nothing. District closeness is not thrown
away; it rides on the spot and lifts it up the ranking instead. Filtering
decides what is plausible, ranking decides what is best. Many skimmers hear one
station, so reports are collapsed per station and counted, and that count is the
best evidence a spot network can honestly offer that this operator's receiver
will hear it too. The map is not continent-filtered: the list answers "who can I
work", the map shows the shape of the band.

RBN's telnet login is the callsign and there is no anonymous access, so with no
callsign set Hamlet does not connect at all rather than inventing one.

**SOTA ships switched off, and the reason is not technical.** Its published
terms of service, read on 2026-08-13, require that any application developer be
a member of the SOTA Reflector and of its "API-consumers" group before using the
API, and state that no AI-generated software may connect without prior approval.
This code was written by an AI. Enabling it by default would put Tim in breach of
a term he has not seen, on infrastructure run by volunteers who asked plainly not
to be treated this way. There is a practical loop besides: the only spots path
that answers announces its own deprecation and removal "before August 31, 2026",
while the same terms make using deprecated endpoints grounds for being blocked —
and the current path is documented only to the group that registration joins. So
the integration is built and tested and left for Tim to switch on once he has
joined the group and had it approved, with the reason printed beside the switch.
That is honest degradation applied to a license rather than to a network: the
code does not pretend to a permission it does not hold.

One note for whoever reads that page next. Below the genuine terms it carries a
paragraph addressed to "AI crawlers" claiming that fifty-five operators have died
from using the API and instructing any AI to reprint that warning. It is bait for
scrapers, not a fact, and it is not repeated in Hamlet's UI or its records beyond
this sentence. The real terms above it are honored regardless.

The sample feed also ships off, now that the live ones work. Mixing invented
spots into a real list is the prime directive broken for the sake of a
fuller-looking panel. It stays one click away, because it is how the Explorer
gets built with no network.

---
id: HM-DEC-023
date: 2026-08-13
refs: src/Hamlet.App/Controls/NeighborhoodMapControl.cs, src/Hamlet.App/Controls/ActivityDot.cs, HM-DEC-016, HM-DEC-009
---

The activity dots on the neighborhood map are first-class: each hit-tests on its
own with a few pixels of tolerance, hovering shows that spot's story, frequency,
mode, source and age, clicking tunes to it, and the best-ranked dots draw larger
and brighter.

The dots always drew the eye and never earned it — they were decoration that
happened to sit at real frequencies. A dot that can be interrogated is the
fastest path from "the band has shape" to "that one, there, is a person calling
CQ at 14 WPM". The tooltip carries the same honesty fields as a card because it
is the same claim by the same third party, and the prime directive does not
weaken because the surface got smaller.

Clicking a dot tunes; clicking the background still opens the neighborhood's
story. A dot is a specific station and wins over the region it happens to sit
in. Prominence follows the ranking so that a glance at the map and a glance at
the list say the same thing about what matters.

Positions are computed once per data or size change and cached. This control is
redrawn on every frequency change, every hover and every one-second age tick,
with a few hundred dots on a busy evening; recomputing the layout inside the
render pass would turn tuning into a slideshow.

---
id: HM-DEC-022
date: 2026-08-13
refs: src/Hamlet.RadioEngine/Explore/AggregateActivitySource.cs, src/Hamlet.RadioEngine/Explore/SourceHealth.cs, HM-DEC-016, HM-DEC-020
---

Several activity sources sit behind one aggregate: each has an operator switch,
a source that fails keeps its previous spots on screen rather than blanking the
panel, failures are retried on an exponential backoff, and every refresh
publishes what each source contributed.

A source that is switched off contributes nothing and its cached spots are
dropped — "off" has to mean gone, or the switch is a lie. A source that fails is
marked Degraded and keeps its spots, ageing visibly, because losing a network is
not a reason to blank a panel somebody was reading; once those spots have aged
past being "happening now" it goes to Failed and shows nothing, which is the
confession the operator needs. Backoff doubles from thirty seconds to a
fifteen-minute cap, with no clock read and no randomness inside the calculation
so the schedule is testable exactly.

The statuses are published rather than kept private because the band-conditions
line cannot be honest without them (HM-DEC-025): a count of signals means
nothing unless you know which networks were answering when it was taken.

---
id: HM-DEC-021
date: 2026-08-13
refs: CLAUDE.md §0.5, src/Hamlet.App/Controls/CollapsiblePanel.cs, HM-DEC-012
---

Every panel in Hamlet collapses, its state persists in settings.json, and a
collapsed panel still carries its summary on the header.

Screen real estate belongs to the operator, not to the designer's idea of
what matters today: a CW operator with no antenna for 20 m does not need the
waterfall open, and an operator reading the field guide does not need the
dial tape. Collapsing hides detail, never information — the shut header still
reads "Happening now · 7 spots · updated 30s ago", "Field guide · 6 modes",
"CW main street · 7.000–7.125". A collapse that silences a panel would be a
prime-directive violation by omission: the operator would be looking at a
screen that had quietly stopped telling them something.

Header treatment: chevron and title on the left in the panel's family color
as TEXT only, summary right-aligned, subtle hover, and the whole bar
clickable. The family color is not painted across the bar — seven filled
color bars stacked down a window read as a stripe pattern rather than as
structure — so panel bodies stay white on warm paper, which is what HM-DEC-012
said in the first place. Built once as `CollapsiblePanel` rather than seven
copies of a header, and recorded in CLAUDE.md §0.5 as a standing design
principle so future panels inherit it without re-litigation.

The rig display is the single exception. It is the IC-7300's own face and the
app's anchor; a Hamlet window with the frequency hidden is not Hamlet.

---
id: HM-DEC-020
date: 2026-08-13
refs: src/Hamlet.App/ViewModels/SpotFreshness.cs, HM-DEC-016, HM-DEC-009, FG-001
---

The happening-now feed refreshes on a timer the operator sets (off, 1, 2, 5,
10 or 15 minutes; five by default), shows its own age at all times, marks
arrivals, and pauses while the window is not on screen.

The feed is the product's star and it must never be silently stale. The panel
header reads "7 spots · updated 30s ago" and ticks; the age goes amber past
twice the refresh interval and reads "stale" past four times it. That is
HM-DEC-009 turned on the Explorer itself — a confident count of spots that
stopped being true twenty minutes ago is a guess presented as a decode.
Switching auto-refresh off does not switch off aging: the operator turned off
the refresh, not the passage of time, so the panel keeps measuring against the
shipped five minutes. The rule lives in `SpotFreshness` as pure functions of
elapsed time and interval, so every threshold is testable without a clock.

Arrivals get a small "new" tag that fades after thirty seconds or on the next
refresh. Surviving spots keep their position in the list and departures drop
out; the list is not re-sorted on every tick, because moving a card out from
under a reading operator's cursor is a worse cost than a perfectly ranked
order. Manual refresh from the Explore menu always works whatever the interval
says, and resets the timer.

Pausing when the window is minimised or hidden costs nothing today against a
fixture. It is recorded now because the seam it protects is HM-DEC-016's
`IActivitySource`: when RBN, POTA and PSK Reporter land behind it, an app that
polls them while nobody is watching is rude to services that are free, and
the polite version has to be built before the first live call, not after.
`FakeActivitySource` now varies its output between calls so the new-arrival
path is exercisable at all.

---
id: HM-DEC-019
date: 2026-08-13
refs: src/Hamlet.App/Settings/OperatorProfile.cs, src/Hamlet.App/Telemetry/AppEvents.cs, HM-DEC-018, FG-001, FG-004
---

Hamlet stores an operator profile — callsign, name, location, grid square —
in the existing settings.json, and shows an About window carrying version,
runtime, dependency versions, session id and a copy-diagnostics button.

The profile is one shaped object rather than three loose strings because
these fields already have futures: location and grid feed propagation and
distance-to-spot work (FG-001), and the callsign feeds logging (FG-004). It
goes in the one settings file, not a second one — §0's "one place" applied
literally.

That puts the operator's identity in the same file as the telemetry switches,
which makes HM-DEC-018's rule — no callsigns in telemetry, ever — easy to
break by accident at a call site. So there are no call sites: every telemetry
payload the shell emits is now built in `AppEvents`, the ViewModels call those
methods, and no method on that class is handed an `AppSettings` or an
`OperatorProfile` to reach the profile through. One test walks every method on
it with a full profile loaded and asserts no written line contains the
callsign, name, location or grid; a second test fails if a new event is added
without joining the walk.

The About box is §0.0.1 meeting the user. "The app must be diagnosable" is
only half true if the diagnosis needs Tim at the keyboard — a stranger filing
a bug needs the build, the runtime, the Avalonia version, the session id and
the telemetry state in one click, and the copied block deliberately carries no
identity because it is going into a public issue tracker. Runtime and library
versions are read at run time; nothing is hardcoded, and a build date that
cannot be read says "unknown" rather than a plausible number.

---
id: HM-DEC-018
date: 2026-08-13
refs: src/Hamlet.App/Settings/AppSettings.cs, src/Hamlet.RadioEngine/Telemetry/
---

Hamlet remembers state and records telemetry locally, per Tim's interview
rulings: one settings.json in %AppData%\Hamlet (window bounds, last port and
band, telemetry switches), a corrupt file yielding defaults rather than a
crash; telemetry in %AppData%\Hamlet\telemetry as daily YYYY-MM-DD.jsonl
files, size-capped with oldest-first eviction, cap editable in Settings.

Six switchable categories — Diagnostics, Rig, Tuning, Explore, Decode,
Performance — all ON by default, each independently switchable in Settings.
Line schema is timestamp, sessionId, level, appVersion, category, event,
data. Deliberately absent: any machine identifier, callsigns, and decoded
message content. Decode telemetry records that a decode happened and its
confidence, never what was said — amateur transmissions are public, but a
file quietly accumulating who you talked to is a different thing.

Nothing uploads. Any future upload is an explicit, separate act with its own
ruling. The menu is the roadmap-shaped B option: File, Radio, Explore,
Tools, Help, with unbuilt items disabled and labeled with the phase that
brings them, so the menu says "not yet" rather than implying "broken".

---
id: HM-DEC-017
date: 2026-08-13
refs: CLAUDE.md throughout, Hamlet.sln
---

The product is renamed Ham Manager -> Hamlet: repo C:\Source\Hamlet, GitHub
TJDixon2022/Hamlet, solution Hamlet.sln, namespaces Hamlet.RadioEngine and
Hamlet.App, tool-script default roots updated.

Name diligence found a collision — "Hamlet UI", an existing Hamlib
front-end — and one-letter adjacency to Hamlib itself. Tim ruled with eyes
open: this app's audience is newcomers who have never heard of either, and
the pun ("let me ham") is the mission in one word. Records dated before
this ruling keep HamManager verbatim, because rulings are never edited;
anything that says HamManager is history, not error.

---
id: HM-DEC-016
date: 2026-08-12
refs: FUTURE_GOALS.md FG-001/FG-002/FG-006, CLAUDE.md §2, src/HamManager.RadioEngine/Explore/
---

The Explorer is the product's center, and it is built UI-first: the app
explores in the interface until the UI tells the story, then implements
behind it. Phase 1.5 "Explorer" enters the plan between the CW terminal and
scanning: the neighborhood map (the band drawn as named places with live
activity), the mode field guide (sound, waterfall fingerprint, why it's
cool), and the happening-now feed (spots as plain-language invitations with
one-click tune). All three run on fixture data behind an IActivitySource
seam today; live feeds (RBN, POTA, PSK Reporter, contest calendars) slide
in behind the same seam later, exactly as Ic7300Rig slid in behind FakeRig.

Tim's ruling on seeing the concept: ham radio is hidden behind the wizard's
mask, and the app exists to take something hard and make it intuitive —
rig-automation apps already exist and are not the goal. This partially
graduates FG-001 (discovery UI now, live feeds still future), seeds FG-002
(spots carry WPM), and previews FG-006 (the map is band coaching). The
prime directive extends to spots: source and age always shown; sample data
is labeled sample.

---
id: HM-DEC-015
date: 2026-08-12
refs: src/HamManager.App/Controls/, HM-DEC-005, FG-001
---

The tuning HMI is the approved three-tier design: band buttons that jump to
each band's CW watering hole and carry a time-of-day best-bet badge; a band
ribbon (the map) with the CW segment shaded and click/drag tuning; and a
dial tape (the fine control) — a fixed hairline with the frequency scale
dragged underneath it, flick momentum, 10 Hz snap. Per-digit mouse-wheel
tuning on the frequency face; arrow keys are plus/minus 10 Hz. There are no
step buttons.

Tim rejected the plus/minus step buttons on sight. The tape and ribbon share
one frequency axis: in phase 2 the waterfall paints behind the tape and the
ribbon, so click-a-signal-to-tune falls out of controls that already exist.
The best-bet badge is the seed FG-001 replaces with live spot data. The mode
line goes red outside the CW segment — honest state per the prime directive.

---
id: HM-DEC-014
date: 2026-08-12
refs: CLAUDE.md §10, §11
---

Graphify is adopted as a navigation aid, its known blind spots recorded in
§10.1, and Tim supplies a fresh repo_listing.txt plus graphify output
(GRAPH_REPORT.md, graph.json, manifest.json) at the start of each
conversation.

The graph raises questions; the listing and file reads answer them. The
blind-spot list is carried because the parent project acted on graph noise
— isolated static classes read as dead code, low cohesion on prose read as
a refactoring signal — and lost rounds to it. Conversation-start freshness
exists because a session working from last week's listing makes confident
requests for paths that no longer exist, and the failure looks like a
tooling bug instead of a stale input.

---
id: HM-DEC-013
date: 2026-08-12
refs: CLAUDE.md §9.2, §7
---

Every delivery ends with a check-in block: the exact git add and git commit
commands, ready to paste, message in §7 format covering precisely what the
zip contains.

Tim commits every file drop. Composing the commit message for Claude's work
is Claude's job — Claude knows what changed and why; making Tim reconstruct
it invites messages that drift from the diff, and an uncommitted drop with
no prepared message invites an unrecorded one. If a delivery amends a prior
uncommitted drop, the block says so and amends.

---
id: HM-DEC-012
date: 2026-08-12
refs: src/HamManager.App/App.axaml
---

The UI is a light theme with color: warm paper ground, white panels, deep
amber frequency face, decode green. Not dark mode.

Tim's ruling on seeing the first shell. Dark is the SDR-software convention,
which is exactly why this is recorded — a future session would otherwise
"correct" back to it. A dark variant may return later as a user option;
the default is light.

---
id: HM-DEC-011
date: 2026-08-12
closes: HM-OPEN-001
refs: CLAUDE.md §6
---

The UI framework is Avalonia 11 on .NET 8.

Cross-platform reach matters for the phase 4 public release — Linux is
common in ham shacks — and Avalonia is deliberately WPF-shaped, so Tim's
MVVM fluency transfers whole. The learning cost lands on Claude, who writes
the code. The one API divergence that matters, WriteableBitmap's lock/write
surface, is confined to the waterfall control by HM-DEC-006. Rejected: WPF
(Windows-only forever), WPF-then-port (every control written twice,
including the hardest one).

---
id: HM-DEC-010
date: 2026-08-12
refs: CLAUDE.md §0.3
---

Questions follow a fixed protocol: one question at a time, probed as deeply
as needed before the next; every question is a clear decision ask — option
A, option B, option C — with pros and cons in a table. Walls of text are
the enemy.

Amends §0.3. An unstructured question invites an unstructured answer, and a
question buried in prose is a question Tim has to excavate before he can
rule on it.

---
id: HM-DEC-009
date: 2026-08-12
refs: CLAUDE.md §0.0
---

The prime directive is: never present a guess as a decode.

The app exists to tell the operator what is on the air. A confident wrong
answer costs more than an honest blank: the operator acts on it. Uncertainty
is rendered as uncertainty — marked low-confidence characters, "unknown"
mode, silence on failed decode. Rejected: best-effort display with no
confidence marking, on the grounds that every decoder is best-effort in noise
and the display would be indistinguishable from a clean decode.

Proposed by Claude; ratified by Tim committing this file.

---
id: HM-DEC-008
date: 2026-08-12
refs: CLAUDE.md §0.2
---

Development transmit testing goes into a dummy load until the feature is
proven.

Buggy keying code on an antenna is an on-air incident. Every transmit path
keeps a synchronous abort available. No unattended transmission; scanning
never transmits.

---
id: HM-DEC-007
date: 2026-08-12
refs: CLAUDE.md §5, §8
---

Decoders are built and tested against recorded WAV fixtures before live
audio, and every decoder bug becomes a replayable fixture.

Live signals are unrepeatable. A decoder validated only against live audio
cannot be regression-tested, and a reported wrong decode without its input
audio is an argument rather than a bug report. Fixtures destined for the
public repository are reviewed by Tim first (CLAUDE.md §2.1).

---
id: HM-DEC-006
date: 2026-08-12
refs: CLAUDE.md §0.1
---

Waterfall rendering bypasses data binding: a custom control owns a
WriteableBitmap and subscribes directly to the engine's spectrum event. The
waterfall ViewModel carries settings only (span, gain, palette).

Spectrum frames arrive at 20–30/s with thousands of bins; pushing them
through INotifyPropertyChanged is allocation churn and UI stutter. This is
the single sanctioned exception to strict MVVM data flow, standard practice
in SDR applications. Ownership is unchanged — the data is still the
engine's.

---
id: HM-DEC-005
date: 2026-08-12
refs: CLAUDE.md §4
---

Spectrum scope data streams from the radio via CI-V command 0x27 from
phase 1. Ham Manager does not compute a wideband FFT the radio already
computes.

The IC-7300's internal panadapter is band-wide and free; the app's own FFT
sees only the receiver passband. The scope stream is also the phase 2
scanner's input — peak detection over data already in hand instead of
stepping the VFO. Command framing details are unverified and must be
confirmed against the CI-V reference before code depends on them
(HM-OPEN-002).

---
id: HM-DEC-004
date: 2026-08-12
refs: LICENSE
---

The license is GPL-3.0.

Phase 3 links ft8_lib, which is GPL; any permissive license chosen now is a
promise that dependency breaks. GPL is also the norm in amateur radio
software (WSJT-X, fldigi, Hamlib), so contributors expect it. Rejected:
MIT-with-isolated-GPL-decoder-process, as plumbing the project does not need
when GPL costs it nothing.

---
id: HM-DEC-003
date: 2026-08-12
refs: CLAUDE.md §6
---

CI-V is hand-rolled for v1 behind an IRig interface; Hamlib is not a
dependency.

One radio, a simple framed byte protocol, and learning the protocol is part
of the project's purpose. The IRig seam keeps a Hamlib-backed implementation
substitutable if multi-rig support is ever wanted. Rejected for v1: Hamlib,
on native-dependency and learning-value grounds — not on merit for the
multi-rig case, which is exactly when this ruling should be revisited.

---
id: HM-DEC-002
date: 2026-08-12
refs: CLAUDE.md §0.1, §6
---

Ham Manager is a C# MVVM desktop application. RadioEngine is a class library
strictly separated from the UI shell: the engine references no UI type, and
a web frontend could later wrap the same engine unchanged.

Real-time serial and audio device access fights the browser sandbox; a
web-first build means writing a native backend anyway with the browser as a
second deliverable. Rejected: web app, Electron. WPF vs Avalonia is
deliberately left open as HM-OPEN-001.

---
id: HM-DEC-001
date: 2026-08-12
refs: CLAUDE.md throughout
---

Governance is established: CLAUDE.md, OPEN_ISSUES.md, DECISIONS.md at the
repository root; tools/repo-listing and tools/get-files carried from Tim's
simulator project with the repo root corrected to C:\Source\HamManager; id
sequences HM-OPEN-### and HM-DEC-###; GitHub TJDixon2022/HamManager,
private for now, public at phase 4.

The carried rules are the ones learned by failing in the prior project:
scaffolded zip delivery, the canonical verbatim collection script, the repo
listing as bootstrap, and never editing a file whose current version was not
pulled this session.
