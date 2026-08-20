STATE: BLOCKED
PHASE: 5 of 5
BALL: tim
NEXT_PASTE: OUTPUT.md -> Claude Web
UPDATED: 2026-08-20T13:26:00-04:00
NOTE: Both changes withdrawn on measurement; blocked on whether 134712 holds a station or a carrier

---

## What this file is

Volatile state, overwritten whole, read by the panel that shows every project on
one screen. Six lines, and the reader takes the leading run of `KEY: value` lines
and stops at the `---` above, so nothing below here is read by anything.

**The panel only knows what a session last wrote.** A project whose sessions never
write reads as dead while it is working, which is why the write triggers in
`CLAUDE.md` §13.2 include a heartbeat every ten minutes while `STATE: EXECUTING` —
a phase here can run an hour, and a long phase and a dead session look identical
without one.

**Nothing here reports branch, commit or working-tree state.** The panel reads
those from `.git` itself, where they stay true after a session goes quiet.

`STATE` is one of five words and never a sentence. `BALL: unassigned` would mean
nobody has taken it, and is not a polite way of saying it is Tim's — here it is
genuinely his, so it says `tim`.

The rules are `ANNUNCIATOR.md`, summarized inline in `CLAUDE.md` §13 (HM-DEC-132).
