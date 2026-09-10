# The azimuthal map — what is needed, and what is already built

**THE IMAGE IS NOT IN THE TREE.** Work instruction 301 task 3 is built as far as it
can be without it, and task 4 says what to do when it is missing: say so, stop task 3
there, leave the hover as it is, and name what is needed. This file is that name.

**Nothing was substituted.** No coastline was drawn, no graticule, no landmass. Two
attempts at drawing a map have already failed and the instruction is explicit that a
third is not the answer.

---

## What is needed

**One image file**, dropped into this folder: Tim's own azimuthal equidistant map,
free to use, centred on his station, with terrain, coastlines and city labels already
on it. Any raster format Avalonia can load will do.

**And three numbers to go with it**, which are the whole configuration:

| Number | What it is |
|---|---|
| `CentreLatitude` | The latitude the image is drawn about, in degrees north |
| `CentreLongitude` | The longitude the image is drawn about, in degrees east |
| `RimRadiusPixels` | How many pixels from the image's centre to its outer rim |

**The rim is the antipode**, one hundred and eighty degrees of arc from the centre.
So the third number is measured off the picture itself: half the width of the circle
the map is drawn inside, in that file's own pixels.

**THESE THREE DESCRIBE THIS ONE PICTURE AND MUST CHANGE WITH IT.** Replace the image
and leave the numbers, and every dot lands somewhere plausible and untrue. Nobody
would ever catch it, which is exactly why §0.0 and HM-DEC-092 cover a picture as
hard as a sentence.

They live in `AzimuthalMap.Settings`, in one place, deliberately. A second copy of a
number like this drifts and the drift is invisible (`CLAUDE.md` §0).

---

## What is already built and checked

`src/Hamlet.RadioEngine/Explore/AzimuthalMap.cs` places a dot. It needs no image to
be correct and it was verified before anything was allowed to trust it, because **a
dot placed by untested arithmetic is §0.0's fault in a picture.**

`AzimuthalMapTests` checks it against `GridPath`, which shares no line of code with
it and computes great-circle distance and initial bearing by its own spherical
trigonometry written years of units ago. On this projection those two quantities are
exactly what a dot's position means: **how far from the centre is the distance, and
which way round is the bearing.**

Against a test centre of 40°N 75°W at ten pixels to the degree:

| Station | On the map | The independent answer |
|---|---|---|
| his own centre | 0 px | 0 miles |
| Ireland | 459.0 px → 3,171 miles | 3,171 miles |
| Portugal | 497.6 px → 3,438 miles | 3,438 miles |
| Japan, over the pole | 978.4 px → 6,760 miles | 6,760 miles |
| Argentina | 761.6 px → 5,262 miles | 5,262 miles |
| Perth | 1,680.4 px → 11,610 miles | 11,610 miles |
| the exact antipode | 1,800.0 px, on the rim | 12,437 miles |

Every bearing agrees to **0.00 degrees** across six stations spread round the compass,
which is the check that catches a mirrored sign: a distance alone would be identical
if east and west were swapped.

**One defect was found and fixed by that checking.** The published formula divides by
`sin c`, which is zero at the antipode, and written literally it put the point
opposite the centre off the rim by more than twelve miles' worth of pixels. The
arithmetic now used is the same equation rearranged — the dot at distance `c` on its
own bearing — which is algebraically identical and has nothing dividing by nothing.

---

## What happens when the image arrives

1. Put the file in this folder and link it as an `AvaloniaResource` in
   `Hamlet.App.csproj`, beside `achievement-quill.svg`.
2. Fill in the three numbers.
3. Point the globe hover at the image and draw **two dots and a line** on it: the
   operator at the centre, the station where `AzimuthalMap.Place` puts it, a straight
   line between.
4. **Write no disclaimer about that line.** On this projection a straight line out of
   the centre *is* the great-circle path. Unit 299 needed a caveat because its
   projection was flat; that reason is gone, and repeating the caveat would be
   telling him something untrue about a picture that is now right.
5. A station with no grid square still gets **no dot**, and the hover says Hamlet does
   not know where he is.

---

## What is still in use meanwhile

**`assets/world-coastline.svg` is still drawn**, because the hover was left exactly as
it was. The instruction expected it to become unused; it has not, because task 3
stopped at the image. It is not deleted and nothing about it changed.
