# US Naval Observatory — sunrise and sunset

**What these are.** Verbatim responses from the USNO Astronomical Applications
Department's rise/set/transit service, saved so the tests that check Hamlet's
own solar arithmetic have a fixed thing to check against (CLAUDE.md §4).

**Source.** `https://aa.usno.navy.mil/api/rstt/oneday`, API version 4.0.1.
Retrieved 2026-08-13.

**Query.** One file per place and date:

```
https://aa.usno.navy.mil/api/rstt/oneday?date=<YYYY-MM-DD>&coords=<lat,lon>&tz=0
```

`tz=0` means every time in these files is **UTC**, which is what
`SolarClock` returns.

| File | Place | Coordinates |
|---|---|---|
| `pittsburgh-*.json` | Trafford, Pennsylvania — the operator's location | 40.38, −79.71 |
| `london-2026-06-21.json` | London | 51.51, −0.13 |
| `quito-2026-03-21.json` | Quito, on the equator | −0.18, −78.47 |
| `tokyo-2026-06-20.json` | Tokyo, east of the prime meridian | 35.68, 139.69 |

**Reading them.** `properties.data.sundata` lists the phenomena that fall
inside that **UTC** day. For a station in Pennsylvania that is not the same as
the local day: the `Set` in `pittsburgh-2026-06-21.json` at 00:52 UTC is the
evening of **20 June** local, and the sunset of 21 June local appears in
`pittsburgh-2026-06-22.json`. This is exactly the wrap that
`SolarClock.DayOffset` exists to handle, and getting it backwards is what the
regression test in `SolarClockTests` guards.

**Agreement.** Hamlet's computed times sit within one minute of every value
here, across both solstices, an equinox, the equator, and a longitude east of
Greenwich. That is the accuracy claim the two-minute test tolerance is set
against.

**Terms.** USNO data are US Government works, in the public domain and free of
copyright restriction. The service asks that it not be hammered; these are
static snapshots, so nothing in Hamlet's test suite touches the network.
