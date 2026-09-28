# edits for work instruction 484: the top row's arrangement never follows the card's words
{
  line = $0
  if (line ~ /^            \/\/ A card taller than the row here stands over it by its outside-privileges lines alone,$/) { print "            // The row is the rig face's height and the outside-privileges lines' own (R62); every"; next }
  if (line ~ /^            \/\/ and the row grows by them rather than scrolling them away\.$/) { print "            // other word of the card wraps and scrolls inside it (work instruction 484)."; next }
  if (line ~ /return new Size\(width, card\.DesiredSize\.Height > rowHeight \+ 0\.5 \? card\.DesiredSize\.Height : rowHeight\);/) { sub(/card\.DesiredSize\.Height > rowHeight \+ 0\.5 \? card\.DesiredSize\.Height : rowHeight/, "rowHeight + Outside(outside)", line); print line; next }
  if (line ~ /^            Math\.Max\(rowHeight, Math\.Max\(card\.DesiredSize\.Height, map\.DesiredSize\.Height\)\)\);$/ && !seenInf) { print line; next }
  print line
}
