# Unit 444 task 1 - 17:37's gaps at f14b2453 (before G1) and at HEAD

Printer: `tests/Hamlet.RadioEngine.Tests/Cw/HowSeventeenThirtySevensGapsAreCalledTests.cs`.
Printouts: `unit444-gaps-f14b2453.table.txt` (measured, decoder src checked out at f14b2453 and
restored), `unit444-gaps-head.table.txt`. Line-up gap by gap on the audio clock: `unit444-lineup.txt`.
Both columns are measured; neither is reasoned.

61 envelope marks from 13.210 s, 60 gaps, all 61 marks matched to the key's 66 elements by the
dit/dah alignment (the key's second WB6RED runs past the audio). The envelope is the same at both
commits, so the 60 gaps line up one for one.

## Gaps whose call changed under G1

| at s | length | before G1 | HEAD | key | key agrees with |
|---|---|---|---|---|---|
| 22.590, 22.730, 22.870 | 60 ms, 0.86 u | letter | element (inside `B`) | element | new call |
| 23.495, 23.610, 23.760, 23.895 | 40-65 ms, 0.57-0.93 u | letter | element (inside `6`) | element | new call |
| **24.030** | **310 ms, 4.43 u** | **letter** | **word** | **letter** | **old call** |
| 26.270, 26.550 | 65-70 ms, 0.93-1.00 u | letter | element (inside `W`) | element | new call |
| **26.830** | **320 ms, 4.57 u** | **letter** | **word** | **letter** | **old call** |

Nine element gaps G1 gave back (these are G1's letters, `B`, `6`, `W` read whole). Two letter spaces
G1 lost: 6|R (`WB6 RE`) and W|B (`W B`). Those two are MET-WBE's 5 -> 7.

What decided them:
- 24.030, read 45. Before G1: held 10/828/250 ms (the refused reading itself), relabel boundary
  455 ms (6.50 u), path said word, the relabel took it out. HEAD: G1 refuses the window
  (centroids 10/63/273, clipped 10/828/250), the stream stays on 64/230/400 from read 41, word
  threshold 303 ms (4.33 u), no character gap measured, relabel boundary 303 ms, the space stands.
- 26.830, read 51. Before G1: held 10/828/250 from read 50, relabel 455 ms. HEAD: G1 refused
  read 50 (centroids 10/60/274), the stream stays on read 49's 63/141/455 (a reading whose word
  boundary the clip raised from 2.73 u to 3.5 u), word threshold 254 ms (3.63 u), the space stands.

## The cause, in one sentence

On 17:37 G1's condition is made by a single key-up of 10 to 15 ms (0.14 to 0.21 u, 1 of 43 to 44
gaps in the window) - a dropout inside a mark - taking the shortest of the three gap clusters, so G1
refuses the window, no character gap is measured in it either, and the stream is left on a spacing
whose word threshold (303 ms, 4.33 u; 254 ms, 3.63 u) sits below this sender's longest letter spaces
(310 and 320 ms), where before G1 the refused reading's backwards threshold (455 ms, 6.5 u) put the
relabel above them.

The G1 windows on the stretch: reads 40, 42, 45, 50 - each "shortest heap 1 of 43/44 gaps, shortest
gap 15 or 10 ms". Every other read's shortest gap is 35 ms (0.50 u) or more, apart from read 47
(25 ms) and reads 54-55 (15 ms, a heap of 3).

This sender's spacing, from the key: letter spaces 135-320 ms (1.9-4.6 u), word spaces 245-485 ms
(3.5-6.9 u). The two overlap. DE|W at 21.175 s is a word space of 245 ms (3.50 u), shorter than six
letter spaces on the same stretch (250, 250, 275, 310, 310, 320 ms), and no duration rule gives it back; it was missed before G1 too.

## Other keyed recordings

Per recording, boundaries wrong at f14b2453 -> HEAD (`unit444-g1-per-recording.txt`): only 17:37 rose
(5 -> 7). 031838 fell 5 -> 1, 032012 3 -> 2, 032129 7 -> 5, cq-18wpm-15db 1 -> 0; every other
recording is unchanged. Real MET-WBE 57 -> 52 over 113. The cause reaches no other keyed recording
whose MET-WBE rose, because there is none. (f14b2453 -> HEAD also holds 14f515bd, the marks' speed.)
