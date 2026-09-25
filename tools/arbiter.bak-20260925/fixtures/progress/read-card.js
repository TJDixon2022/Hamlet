"use strict";
/* read-card.js  -  the PANEL'S OWN readers, run against a card the loop has
   just reconciled.

     node read-card.js <path to PHASE_STATUS.md> <how many steps it must name>

     0  the panel reads it, and reads the steps the arm expects
     1  it does not - the line above the verdict says what differed
     2  usage

   WHY THIS EXISTS AND WHY IT IS NOT A SECOND DRIVER. Unit 074 task 4 says to
   extend run-fixture.bat rather than build another driver, and this is not one:
   it runs no loop, seeds no root and decides no arm. It is the one thing a
   batch file cannot do - load the panel's parser and painter out of the HTML
   and ask them what they make of a file - and run-fixture.bat calls it from
   inside the card-panel arm exactly as it calls attempt-read.bat from inside
   the attempt arms.

   IT USES THE PANEL'S OWN CODE, NOT A COPY OF IT. `harness.loadPanel()` pulls
   the script out of app\PROJECT_ANNUNCIATOR.html at the extraction seam that
   CPS-DEC-017 asserts, so what is proved here is what the owner will actually
   see. A parser reimplemented in this file would agree with itself and prove
   nothing, which is the fixture failure CPS-DEC-011 names.

   IT CHECKS TRANSPORT AS WELL AS CONTENT. `parsePhaseStatus` normalizes CRLF
   and a BOM before parsing and REPORTS what it normalized, so a card that had
   gained either would still read as readable - and the reading would be right.
   The defect would be in the bytes. So the transport reading is asserted too:
   shape `lf`, no BOM, nothing changed. Unit 073 shipped a doubled BOM and an
   orphaned LF into the record and caught them only in the bytes; this file's
   conventions are the opposite ones, and a check that looked only at the
   rendered steps would pass over the same class of error. */

const fs = require("fs");
const path = require("path");
const H = require("../../../tests/harness");
const M = require("../../../measure-card");

const file = process.argv[2];
const want = parseInt(process.argv[3], 10);

if (!file || !(want >= 0)) {
  console.log("usage: node read-card.js <PHASE_STATUS.md> <expected step count>");
  process.exit(2);
}

const bad = [];
function must(ok, said) { if (!ok) bad.push(said); }

let sb, p, segs = null;
try {
  sb = H.loadPanel();
  const txt = fs.readFileSync(file, "utf8");
  p = sb.parsePhaseStatus(txt);

  must(p.readable === true,
       "the panel does not read the card: " + p.why);
  must(p.steps.length === want,
       "the panel reads " + p.steps.length + " step(s), the arm expects " + want);
  must(p.steps.every(s => s.stateOk),
       "a step line carries a state the panel does not know");
  must(p.steps.every(s => s.numbered),
       "a step line carries a number the panel could not read");
  must(p.strandedNames.length === 0,
       "the panel found this format's keys stranded below the terminator: " +
         p.strandedNames.join(", "));

  /* THE BYTES, THROUGH THE PANEL'S OWN TRANSPORT READING. */
  const tp = p.transport || {};
  must(tp.bom === false,   "the card carries a byte-order mark");
  must(tp.crlf === false,  "the card carries CRLF line endings");
  must(tp.cr === false,    "the card carries CR line endings");
  must(tp.changed === false,
       "the panel had to normalize the card before it could parse it");

  /* AND THE PAINTER, because a file that parses and does not render is still
     a card the owner cannot read. The bar draws one segment per step. */
  const sl = M.fullSlot(sb, "card-panel");
  sl.phaseStatus = { present: true, data: p, readAt: new Date() };
  sb.paint(sl);
  const pbar = sl.refs.phase.children.filter(c => c.className === "pbar")[0];
  segs = pbar ? pbar.children.length : null;
  must(segs === want,
       "the bar draws " + segs + " segment(s), the arm expects " + want);
} catch (e) {
  bad.push("the panel's readers threw: " + (e && e.stack ? e.stack : e));
}

if (bad.length) {
  bad.forEach(b => console.log("      " + b));
  console.log("      CARD-PANEL: FAIL");
  process.exit(1);
}

console.log("      the panel reads the reconciled card: readable, " +
            p.steps.length + " step(s), " + segs + " bar segment(s), " +
            "transport " + p.transport.shape + ", no BOM, nothing normalized");
console.log("      CARD-PANEL: PASS");
process.exit(0);
