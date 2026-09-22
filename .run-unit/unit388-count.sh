#!/bin/sh
# Unit 388 - counts the filter terms on carry-forward lines 7 and 9, and looks for TheTopRowTests on line 7.
cd /c/Source/HamLet
echo "line 7 terms: $(sed -n '7p' docs/carry-forward-tests.txt | grep -o 'FullyQualifiedName~' | wc -l)"
echo "line 9 terms: $(sed -n '9p' docs/carry-forward-tests.txt | grep -o 'FullyQualifiedName~' | wc -l)"
echo "TheTopRowTests on line 7: $(sed -n '7p' docs/carry-forward-tests.txt | grep -o 'TheTopRowTests' | wc -l)"
echo "line 7 starts: $(sed -n '7p' docs/carry-forward-tests.txt | cut -c1-30)"
echo "line 9 starts: $(sed -n '9p' docs/carry-forward-tests.txt | cut -c1-30)"
