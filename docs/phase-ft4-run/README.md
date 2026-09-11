# The FT4 phase's run files, recovered - two of three

**Work instruction 315 task 1e.** The FT4 phase, *FT4 works exactly the way FT8 does*
(HM-DEC-160, set 2026-09-08), closed at Tim's word on 2026-09-11. Its three run files
were never archived under `docs\`: commit `c8da770` (unit 312 task 1, 2026-09-11
01:15) overwrote them at the root with the PSK31 phase's, rather than
`install-phase.bat` moving them aside first.

They are in git history. `eb28430`, the parent of `c8da770`, is the last tree holding
all three FT4 versions:

| file | last FT4 commit to touch it | bytes | blob | here |
| --- | --- | --- | --- | --- |
| `PHASE_PLAN.md` | `048a158` 2026-09-09 08:04 (`phase: ft4`) | 11383 | `a3a29e0` | **recovered**, md5 `908e41f4...` equal to the blob's |
| `PHASE_STATUS.md` | `eb28430` 2026-09-10 22:05 (unit 310's report) | 1174 | `dff3813` | **recovered**, md5 `c41c6109...` equal to the blob's |
| `PHASE_OUTCOME.md` | `1d5f72b` 2026-09-10 21:34 | 168242 | `188cd30` | **not recovered** |

**Why only two.** The session's shell refused every way of letting git write the bytes
under `docs\`: a redirect and `rm` were blocked by the sandbox as outside the working
directory, and `git restore --worktree`, `git archive --output` and `tee` each needed an
approval a non-interactive session cannot give. The two small files were written with
the file tool and checked byte for byte against the blob by md5. The outcome file carries
lines of repeatedly re-encoded text that cannot be reproduced by hand with any
confidence, and a copy that is nearly right is worse than none, so it was not written.

**To finish it, one command from the repository root:**

```
git show eb28430:PHASE_OUTCOME.md > docs\phase-ft4-run\PHASE_OUTCOME.md
```

It should come out at 168242 bytes with md5 `b60010069f7feea96e9d4be4e7e8c0f1`, carrying
`## UNIT 288` through `## UNIT 310` - 288 to 305, then 309 and 310, with no entries for
306, 307, 308 or 311.

Nothing here is edited, and nothing reads these files; they are the phase's memory.
