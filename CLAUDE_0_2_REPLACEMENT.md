### 0.2 Transmit safety — ABSOLUTE

- Every code path that keys the transmitter has a same-thread, no-await abort
  available (CI-V `0x17` with `0xFF`, and PTT off as the fallback).
- **One operator action, one transmission.** Hamlet transmits because the
  operator clicked, and for no other reason. It never transmits on a timer, on a
  decode, or because a contact "should" continue. A scan never transmits.
- **Hamlet never transmits outside the operator's licence privileges.** The
  Settings check is the gate and it is not bypassable from any send path.

**Tim operates a licensed station on an antenna and Hamlet transmits on the air.**
Ruled 2026-09-06, superseding HM-DEC-008 and HM-DEC-098, which required a dummy
load. **That requirement is withdrawn in full: it is not a stage, not a fallback,
and not to be referenced.** No unit reintroduces it, proposes it, or treats its
absence as a risk to be mitigated.

**What remains absolute is the abort and the one-click rule.** A transmission the
operator did not ask for is the fault this section exists to prevent, and it is
unaffected by where the RF goes.
