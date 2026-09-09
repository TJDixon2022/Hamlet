# `world-coastline.svg` — what it is, and what it is not

**A future session will find a world map in this tree and assume it is
authoritative. It is not.** That is the whole reason this file exists, and it is
written down here rather than left to be discovered by somebody reasoning from a
picture.

---

## What it is

**A simplified world coastline, drawn for Hamlet.** 720 by 360, equirectangular,
about two kilobytes of `<path>` elements inside one `<g>`.

**Ruled by Tim, 2026-09-09**, when the alternative was downloading one: at the size
a hover panel renders it, coastline fidelity buys nothing, and drawing it removed a
download, a licence question and a network call from the unit that built it.

## What it is not

- **It is not survey data.** No coastline in it was measured; every one was drawn to
  be recognisable at a hundred and fifty pixels tall. Do not derive a distance, a
  boundary, a bearing or an area from it.
- **It is not a source of truth about anything.** Where the application needs to know
  *where* a station is, it uses the two grid squares and `GridPath`, which is
  arithmetic on values the messages themselves carried. **The map is a backdrop for
  two dots and nothing here places them.**
- **It is not complete.** Islands, lakes, inland seas and whole stretches of coast
  are absent by design. An absence in this file says nothing about the world.

## The projection, and the one rule about it

The projection is written into the file's own `<desc>`, which is where it belongs
because that is the copy a reader finds first:

```
x = (longitude + 180) * 2
y = (90 - latitude) * 2
```

**Anything that puts a point on this map uses that arithmetic and no other.**
`Ft8GlobePlot.X` and `Ft8GlobePlot.Y` are the only implementation of it in the tree
and `Unit299GlobeTests.TheProjectionIsTheCoastlineOwn` reads this file's `<desc>` and
`viewBox` back and asserts the code agrees.

**A dot placed by different arithmetic from the coastline is a dot in the wrong
place**, over a coastline that is still drawn correctly — which is §0.0 on a picture
(HM-DEC-092), in the one place nobody would ever check. **If this map is ever
redrawn on another projection, that test fails**, which is the point of it.

## The straight line is a simplification, and the screen says so

The globe draws a straight line between the two stations. **A real signal follows a
great circle, which is a curve on a flat map like this one**, and a path near a pole
looks nothing like a straight line at all.

`Ft8GlobePlot.Caveat` carries that sentence and the caption under the map shows it
wherever a line is drawn:

> The line is drawn straight on this flat map. A real signal follows a great circle,
> which curves on a picture like this one, so the line says who is where rather than
> the path the signal took.

**Do not remove that sentence to tidy the caption.** It is the difference between a
picture that says *these two people are here* and a picture that claims to show what
the radio did.

## If somebody wants a real one

That is a different job with a different licence question, and it needs a ruling
before it starts. It would want: a source with terms that permit redistribution, a
vendored copy under `data/vendor/` per §4, a note recording where it came from and
when — and, if it is not equirectangular, **a new projection in the `<desc>` and the
test above failing until the code follows it.**
