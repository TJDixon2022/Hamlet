# Work instruction 501 - Farnsworth spacing, and a lone letter must belong to something

**Hand run. One unit.** Unit 500 measured the reader right on standard timing at every speed and
could not reproduce the owner's wall of `E`s and `T`s. It named why: W1AW's slow code practice is
sent Farnsworth-style, and no case had ever been built that way. This unit builds that case and
makes the reader read it, and it gives single-element letters the suspicion the owner asked for.

**No test against a recording, a fixture, a floor or copied telemetry** (R96). A headless test
driving synthetic hops written in the test itself is allowed; nothing read from disk. Verify by
building `Hamlet.sln` with warnings as errors and running the app carry-forward line. **The
owner's report at the radio is the test.**

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs
  MUST EXIST:      CW_REQUIREMENTS.md
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln
  root             C:\Source\HamLet

If all five are not as stated, refuse: reply with only the path you are in,
which checks failed, and "wrong project - nothing done."

If all five hold, say "Hamlet confirmed" and continue.
```

---

## 1. Rules

- Take `SESSION.lock` through `tools\arbiter\lock.bat take`, release it at the end. Write nothing
  to `RUN_LEDGER.md`. Touch nothing under `tools\arbiter\`. Tick nothing in `PHASE_PLAN.md`.
- One `dotnet test` invocation per line, filtered, with a `timeout`. Never background and poll.
  The app line loses names to the dispatcher loop; re-run once, count neither way.
- Apostrophes in quoted heredocs break; `;`, `rm` and `git rm` are refused; Python cannot run
  here; `-m` more than once for a multi-line commit. Scripts go in `.run-unit\unit501-<name>.sh`
  and are not committed.
- Nothing that keys or transmits. Nothing written to the radio.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. What is known, and what the owner ruled

**Farnsworth.** The ARRL sends its slow code practice with the **characters at 18 WPM** and the
**spaces between characters and words stretched** so the overall speed comes out at 5, 7½, 10, 13
or 15 WPM. Inside a letter the dit is 67 ms and the gaps are 67 ms. Between letters the gap is
whatever makes the speed - at 5 WPM overall it can be over a second. **Morse's 1:3:7 does not hold
between letters on a Farnsworth sender.** Unit 500 built every case with the gaps scaled to the
dit, which is why it read 5 WPM whole on the bench and the owner saw `ST ST TIE TETBA TE TE T TN`
on the air.

**The owner's ruling, R109, 2026-09-30:**

> *"I want to work on the E's and the T's. They very rarely, almost never, will stand on their own.
> They need to be part of something. And T, T, T, T, T or E, E, E, E, E is not part of something.
> We can put a little brains behind this and make sure they're part of something else."*

**Where the banking falls short today.** Unit 498 banks a single-element letter until the same
sender sends another letter within the window. **Another single-element letter counts.** So a
lone `T` is confirmed by a lone `E`, which is confirmed by the next lone `T`, and the whole string
releases. The banking is right in shape and wrong in what counts as confirmation.

---

## 3. Change one - the gaps between letters are the sender's own

In `CwRunReader`:

- **The gap inside a letter** stays as unit 500 left it: the sender's gap dit, boundary at √3.
- **The gap between letters and the gap between words are measured from the sender's own gaps
  between runs**, not derived from the dit. A Farnsworth sender shows two populations of gap
  outside its letters - between-letter and between-word - and the boundary between them is found
  from those gaps, the way the dit and dah are found from the marks.
- **Until the sender has shown enough gaps between runs**, fall back to 1:3:7 on the dit, and say
  in the report how many gaps that takes.
- **A standard-timing sender must read exactly as it does now.** On standard timing the measured
  gaps land at 3 and 7 dits and nothing changes.
- **The silence span** unit 500 built - how long a sender may be quiet before it is forgotten -
  must follow the measured word gap, not the dit, so a 5 WPM Farnsworth sender is not forgotten
  between its words.

## 4. Change two - a lone letter must belong to something

In `CwRunReader`, two rules on single-element letters - `E` and `T`:

1. **A single-element letter is confirmed only by a multi-element letter** from the same sender,
   before or after it within the window. Another single-element letter does not confirm it.
   `TEST` still reads whole: its `T` and `E` are confirmed by the `S`. `DE` reads whole: the `E` is
   confirmed by the `D`. A `T` followed by an `E` followed by a `T`, with nothing else, prints
   nothing.
2. **Three or more single-element letters in a row from one sender is not sending.** The run is
   dropped and the sender is not printed on its account. **Real text does not do it** - the ARRL
   practice text is English, and English does not put `EEE` or `TTT` or `TET` in a row.

**Printed-stays-printed holds** (R100): a banked letter has not been printed, so dropping it
removes nothing from the screen.

**Do not change** the detector, the marks, the narrowness, the edges, unit 496's bin choice, the
keyed-mark rule, the two-run rule, the scope, the terminal, the layout, the preamp or the buttons.

---

## 5. What to measure

**Watch it fail first**, with synthetic hops written in the test:

1. **Farnsworth: characters at 18 WPM, spacing for 5 WPM overall**, sending
   `CQ CQ DE N0CALL N0CALL K`. **Red today** - report what it reads before, and that it reads
   whole after. Then the same at 10 and 13 WPM overall.
2. **Farnsworth `TEST DE W1AW K`** at 5 WPM overall.
3. **A string of lone marks**, `T E T T E` at a station's pitch and level with nothing else,
   **prints nothing**.
4. **`TEST DE W1AW K` and `DE` read whole** at standard timing - the `T`, `E` and `E` confirmed by
   multi-element neighbours.
5. **Every existing case reads exactly as it does at HEAD**: the calls at 5, 10, 18 and 35 WPM,
   the speed change, the bursts, the two stations, the lone dit, the lone dah, the stray dit, both
   noise tests. **If any changes, say so with its text; do not force it.**

**Report, for each Farnsworth case, the sender's true between-letter and between-word gaps beside
what the reader measured.**

---

## 6. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 501 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 501.
- Patch-bump `Directory.Build.props`.
- **Append R109 to the rulings section of both `PHASE_PLAN.md` copies**, in the owner's words
  above. **Touch no checkbox.**
- `DECISIONS.md`, newest first, **HM-DEC-205**, headline *Farnsworth gaps are the sender's own, and
  a lone letter must belong to something*, naming that no case had been built Farnsworth-style
  and that a lone letter had been confirming a lone letter.

---

## 7. Report

Section 2, for the owner, in plain words:

- rebuild;
- **W1AW's slow code practice should read as words** - the letters are sent at 18 WPM with long
  spaces, and the reader now measures those spaces from the sender rather than assuming them;
- **strings of `E` and `T` should be gone**: a lone letter now needs a real letter beside it, and
  three lone letters in a row are thrown out;
- `TEST`, `DE` and every real `E` and `T` inside a word still print;
- **if it still comes out wrong, section 3's gap table is the thing to compare against.**

Section 1: what changed, file by file, how the between-letter and between-word boundary is found,
and that the build and the app line are green. **Section 3: the Farnsworth cases' text before and
after, with the true gaps beside the measured ones.** Section 4: anything left, a line each.
