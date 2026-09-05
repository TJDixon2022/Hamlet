# Archive - the FT8 phase, phase set 2026-08-31

**This directory is empty of the files it is for, and that is unit 242's
headline finding rather than an oversight.** The session could not move them:
its shell was read-only and refused every write, every `git mv` and every
`mkdir`. See `output.md` section 4.

The three files are still at the repository root, untouched, exactly as the
closing phase left them. **Nothing was overwritten, which is the one thing that
mattered.**

## What belongs here, and how to put it here

Run these three, in this order, from the repository root:

```
git mv PHASE_OUTCOME.md docs/phase-ft8/PHASE_OUTCOME.md
git mv PHASE_STATUS.md  docs/phase-ft8/PHASE_STATUS.md
git mv PHASE_PLAN.md    docs/phase-ft8/PHASE_PLAN.md
```

`PHASE_PLAN.md` is **untracked** at the root, so `git mv` will refuse it until it
is added; a plain `mv` is enough for that one. It is the only one of the three
that is not already safe in git history.

| File | Holds | Bytes |
|---|---|---|
| `PHASE_PLAN.md` | The plan that phase ran to, its seven steps and their targets | 25161 |
| `PHASE_STATUS.md` | Where it stood at the end: steps 1 to 5 done, 6 blocked on `HM-OPEN-067`, 7 partial | 1378 |
| `PHASE_OUTCOME.md` | **Forty-one units of memory**, one entry per unit, the approach taken and what it hit | 153765 |

SHA-256 as they stood at the root on 2026-09-04, so a later session can prove
the file it moved is the file this note describes:

```
0b8fde1084b25754366846f112cfeeab554da3072ff98223a40c9f21d77075bb  PHASE_OUTCOME.md
ae2a37fbe903ab3be8f0fe18fe57f505ea0ca908eabb12d68952024c6f2d903b  PHASE_STATUS.md
bac68ca9dad4fc9622e39b3ef7f755f6eab398fe72923b831b0580c28be833b2  PHASE_PLAN.md
```

`PHASE_OUTCOME.md` is the one that matters. `output.md` is overwritten every unit
and cannot carry what was tried, so that file is the only record of the
forty-one attempts that got FT8 onto the screen. It is not carried forward into
the new phase's `PHASE_OUTCOME.md`, which starts empty.

**It was deliberately not hand-copied.** `file` reports it as `data` rather than
as text, and transcribing 153 KB of it through a reader that prefixes line
numbers, in order to work around a blocked `cp`, is exactly the way to lose the
thing this move exists to protect.
