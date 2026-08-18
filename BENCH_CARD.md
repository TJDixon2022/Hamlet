**PROJECT: Hamlet**

# Bench card — the dummy load evening

Not a work order. A checklist for the operator, at the radio, with a dummy load
on the antenna socket. **This is the evidence HM-DEC-098 makes the precondition
for the antenna question**, and no test can stand in for it: seventeen passing
tests against a fake that keys nothing are reasoning with better bookkeeping.

Bring a notebook. **Write down which sentence Hamlet gives for each stop**, not
just that it stopped. A stop that names the wrong reason is worth nothing on an
evening when several things could have caused it.

---

## Before anything

- [ ] **Dummy load on the antenna socket.** Check it twice. HM-DEC-008 and
      HM-DEC-098 both require it, and nothing below reaches an antenna.
- [ ] Radio in **CW**, break-in **full**, keyer speed where you normally run it.
- [ ] Connect Hamlet on COM3. Wait for the rig facts to fill in before touching
      the panel.
- [ ] Open **Call CQ on a cycle** in the tray.

## The panel, before arming

- [ ] It opens with **an empty message box**. That is the design — Hamlet writes
      nothing that goes out under your callsign.
- [ ] Type your call. `CQ CQ DE KC3QIS KC3QIS K` is 24 of the keyer's 30.
- [ ] Read the **five fact lines**: message, frequency, rounds and how many
      minutes that is, break-in, power as a percentage. **Anything Hamlet has not
      read should say it has not read it** — not blank, not a guess.
- [ ] **Turn break-in off at the radio.** The panel should say so and **refuse to
      arm**. Write down its sentence.
- [ ] Break-in back on. Confirm the refusal clears.

## One clean cycle

- [ ] Arm. Note that **start is not offered until arming has happened**.
- [ ] Start. Watch **two or three rounds** go out.
- [ ] The **pinned strip** shows a green transmitting line and the stop control,
      beside the scanner's.
- [ ] The **log fills**: timestamp, frequency, message, round number.
- [ ] Confirm the listen window does not open until the radio has finished
      keying — it waits the message's own duration plus about a quarter second.

## Then break it, one at a time

Restart the cycle before each. **Write the sentence each one gives.**

- [ ] **Escape**, from a window that does not have the panel focused. It is
      handled at the window so it should work whatever has focus.
- [ ] **The stop control** in the pinned strip.
- [ ] **Touch the dial** mid-cycle. This one had a real bug: the baseline used to
      swallow the first move, so a hand on the dial during the first
      transmission was consumed as "where the dial is". It should stop now and
      say the dial moved.
- [ ] **Press the PTT** during a listening window.
- [ ] **Turn break-in off between rounds.** The dead man re-reads it before every
      round.
- [ ] **Pull the USB cable mid-cycle.** The one HM-DEC-098 names specifically.
      The read throws, the cycle stops, and the stop code goes out on the way
      past where it reaches nothing — because an abort that could fail is not an
      abort. **Then plug back in and confirm the radio is not still keying.**
- [ ] **Start the scanner while armed**, and try to arm while the scanner runs.
      Both directions should refuse and say why.

## The measurement (HM-DEC-130)

While the load is connected, one number is wanted that no fixture can give:

- [ ] Enter a message **longer than 30 characters** so it splits — a third `CQ`,
      or `PSE K` on the end.
- [ ] Send it and **listen to the seam**. How long is the gap between the two
      sends? Is it steady round to round, or ragged?
- [ ] If Hamlet refuses it at edit time rather than splitting, note that instead
      — the split path exists for single sends and may not be wired into the
      cycle.

The choice between refusing a long call permanently, splitting it, or timing the
second send from `CwDuration` gets made on that number.

## Afterwards

Two rulings wait on this evening:

1. **The antenna question** — whether an attended automatic cycle may reach an
   antenna. §0.2's first sentence stands unamended and nothing argues for
   amending it until every interlock above has been *watched*.
2. **HM-DEC-130** — refuse, split, or time the second send.

**If any interlock does not fire, or fires with the wrong sentence, that is a
finding about the code and not a reason to loosen anything.** Bring it back and
it becomes the next work order.
