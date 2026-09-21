# The radio sheet - PSK31 and Olivia, one page

The page to have open the first time you work PSK31 or Olivia with Hamlet: which button, in
order; what Hamlet says back at each press; what each sentence that refuses a send means; where
your files are; and what to send back when something goes wrong.

**Nothing here was seen by anyone.** Every sentence below is what the program's own words are,
read out of the program on a development machine and checked by a test - not a screenshot, and
not a description of anything that has been on the air (FACT-004). This page says what Hamlet
does; it never says what the air will do, what the band will be like, or whether anyone answers.

**How the quotes work.** Every sentence Hamlet says is on its own line beginning with `> `, and
nothing else on this page uses `>`. Where something is filled in at the moment it is said, the
changing part is in angle brackets - `<like this>` - and the rest is exactly what you will read.

## 1. Before you press anything

Four things are Hamlet's, and all four are set inside Hamlet.

- **Your callsign and grid square**, in Settings. Hamlet will not send a message with a blank in
  it, so a missing callsign is a refusal and never a half-built call.
- **A transmit sound card**, picked in Settings. Until one is picked, the first press says:
> "No transmit device is chosen. Open Settings and pick the radio's sound card."

- **Hamlet connected**, so there is something to key.
- **Your license class**, in Settings. Hamlet refuses a send outside your privileges, and that is
  the one refusal a different choice in Settings cannot fix.

## 2. PSK31, press by press

**1. Open the Digital tab.** The mode strip is the row of five chips - FT8, FT4, PSK31, WSPR,
Olivia - under the caption:
> "on this frequency"

A chip is lit when the dial is inside that mode's block, and chosen when you have pressed it.
Those are two different facts and they often disagree.

**2. Press the PSK31 chip.** The line beside the strip becomes:
> "listening for PSK31 across the whole passband. Every signal Hamlet is sure is PSK31 gets a line of its own below, with where it sits, how strong it is and its text as it arrives, and a line from somebody calling anybody carries an Answer that replies on his own frequency."

**3. The decoded panel**, until something is read, says:
> "nothing decoded yet. Every station Hamlet is sure is sending PSK31 gets a line here, filling in a character at a time as it arrives. A signal it is not sure of gets no line at all rather than a guess, so an empty panel can mean a quiet band or a signal too rough to read."

**4. The send area**, before anything has gone out, reads:
> "nothing sent yet"

and its hover says:
> "Right-click a decoded row to choose a message, or press CQ."

**5. Press CQ** - the one in the send area, not the `CQ` chip over the decoded list, which is a
filter and sends nothing. Hamlet picks a clear spot itself and says:
> "Sending "<your line>" now, at <n> Hz in the passband."

**6. Right-click any row that names a station** for the seven lines: *Answer him*, *Send my
report*, *Confirm and 73*, *Say again?*, *Please repeat your report*, *QRZ?*, *73 and out*. One
click sends the one you picked, with both callsigns in front and the hand-back behind. A line
Hamlet cannot build right now is on the menu as a note saying why, rather than drawn grey.

**7. Stop** is in the status bar and is always pressable. It reads `Stop` when nothing is armed
and `Stop transmitting` when something is, and it asks nothing first.

**8. When it has gone**, the send area reads:
> "Sent "<your line>" - <n> s of PSK31."

## 3. Olivia - only what is different

Everything in section 2 happens the same way. Four things differ.

- **The variant comes from the station you are answering and is never chosen from a menu.** There
  is no variant control anywhere in Hamlet. A row carries the variant its own announcement named
  and a reply goes out at that variant, because a reply at another one is a reply his decoder
  does not read. A call to anyone goes out at the calling variant the cited table names.
- **Every send begins with a burst naming the variant.** Where Hamlet cannot read the file that
  says how to make that burst, it refuses rather than sending an unnamed signal.
- **Hamlet will not transmit a variant it has not read back off its own audio.** It is a gate,
  and its sentence is in section 4.
- **The panel and the send line say Olivia.** The idle panel reads:
> "nothing decoded yet. Every station Hamlet is sure is sending Olivia gets a line here, filling in a few characters at a time as each block is read. A signal it is not sure of gets no line at all rather than a guess, so an empty panel can mean a quiet band or a signal too weak to read. Capture keeps two minutes of what the radio hears."

and a finished send reads:
> "Sent "<your line>" - <n> s of Olivia."

## 4. When it refuses

Each one is a sentence Hamlet can put in the send area, with what it means and what to do.
Nothing was keyed in any of them.

**Nothing was in the message.** Your callsign is not set, or the line was empty. Set it in
Settings and press again.
> "There was nothing to send."

**There is no way to key anything.** Hamlet is not connected, or no transmit sound card is
picked. Do both, then press again.
> "Hamlet composed the PSK31 call and sent nothing: no radio is connected and no transmit audio device is named in Settings."

**No transmit sound card is picked.** Press the word *Settings* in the sentence: it opens the
picker.
> "No transmit device is chosen. Open Settings and pick the radio's sound card."

**The card is picked but would not open.** Something else has it, or it is unplugged. The name,
the rate and what Windows said are the three things to copy if you send this back.
> "Hamlet composed the PSK31 call and sent nothing: the transmit audio device named in Settings could not be opened. Device: <the card's name>. Rate asked for: <n> samples per second. The operating system said: <what Windows said>."

**There is nowhere clear to call.** Hamlet looked for a gap and there is not one. Wait, or put
the passband somewhere else.
> "Hamlet did not call: the band is too crowded here to call without landing on someone. It looks for a spot at least 150 Hz from every carrier being read and from every candidate over quality 0.4, in the middle of the widest such gap between 400 and 2200 Hz, and there is not one right now. Move the dial a little, or wait for somebody to finish."

**The message is longer than the limit.** A canned or macro line may be 30 seconds of audio and
a line you type 60. Shorten it and press again.
> "Hamlet did not send the PSK31 call: this is <n> s of Psk31 audio, and this send may be at most <n> s so that a continuous carrier cannot run on. Nothing keyed."

**Olivia cannot announce itself.** One of Hamlet's own data files is missing or unreadable and
the sentence names which. Nothing at your end is at fault; send the sentence back.
> "Hamlet did not call in Olivia: a send has to begin with the burst naming its variant, and <which file, and what is wrong with it>. So nothing went out, because a signal nobody can name the variant of is a signal nobody can read."

**Hamlet has not proved it can read that Olivia variant back.** It is a gate and nothing you can
press changes it.
> "Hamlet did not send it: it has not proved to itself that it can read back Olivia <variant>, so it will not put that variant on the air."

**The message holds something the mode cannot carry.** PSK31 carries the Latin-1 set and nothing
else; Olivia names the character. Retype the line in plain letters and digits.
> "Hamlet did not send it: <what the composer said>."

**The chosen mode cannot send at all.** You are on a chip Hamlet can only listen in - WSPR is the
one. Press PSK31 or Olivia.
> "Hamlet cannot send <the mode> yet, so nothing went out. It can hear this mode before it can answer in it, and sending anything else here would put the wrong kind of signal on a frequency people are using for <the mode>."

**The right-click menu is empty.** Your own copy of `canned.json` could not be read. Fix it or
delete it, and Hamlet goes back to the one beside the program.
> "Hamlet could not read its canned lines, so it is offering none: <which file, and what is wrong with it>"

**A capture kept nothing.** The first sentence is the PSK31 and Olivia press; the other three
belong to the `keep the last 30 seconds` press beside it. The last two mean nothing was arriving
yet, and `Could not write the capture` means the folder would not take the file.
> "Nothing arrived while it was running, so no file was kept."

> "Could not write the capture: <what Windows said>"

> "Nothing is listening, so there is no audio to keep. Connect a radio or pick the training radio and press it again."

> "No audio has arrived yet, so there is nothing to keep."

## 5. Where the files are

All of it is under `%AppData%\Hamlet`, which is the one folder Hamlet writes to.

| What | Where, and named |
|---|---|
| The record | `%AppData%\Hamlet\telemetry\` - one file a day, `yyyy-MM-dd.jsonl`, in UTC |
| A PSK31 or Olivia capture | `%AppData%\Hamlet\captures\` - `psk31-yyyy-MM-dd-HHmmss.wav` or `olivia-yyyy-MM-dd-HHmmss.wav` |
| A `keep the last 30 seconds` capture | `%AppData%\Hamlet\captures\digital\` - `ft8-yyyy-MM-dd-HHmmss.wav`, with a `.txt` beside it |
| Your settings | `%AppData%\Hamlet\settings.json`, and your own canned lines at `%AppData%\Hamlet\canned.json` |

**The two capture folders are not the same folder.** The second is the first with `digital` on
the end, and the two buttons write to different ones.

**A capture's name is a timestamp and nothing else** - no callsign, no band, no station. That is
deliberate, and it is also how a file is matched to a line in the record: by the time in its name.

## 6. If it fails

Send back three things. Nothing else is needed.

1. **The sentence on the screen, copied exactly.** It is the one thing that names which of the
   refusals above happened.
2. **Today's record**, `%AppData%\Hamlet\telemetry\<today>.jsonl`. It holds nothing personal: no
   callsign, no grid, none of the decoded text, and not even the capture's own path.
3. **The capture, if you made one**, from `%AppData%\Hamlet\captures\`. Note the time in its name;
   that is what matches it to the record.

**Not worth sending.** A screenshot of the panel - the record says more. The whole `captures`
folder - one file, matched by its time, is enough. And nothing personal is ever wanted here: not
a name, not a location, not a grid, not a callsign.
