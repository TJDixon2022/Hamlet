cd /c/Source/HamLet
sed -i "1254s/1.13.88/1.13.89/" Directory.Build.props
sed -n 1254p Directory.Build.props
sh .run-unit/unit402-build.sh entry "task 0: entry build warnings as errors; decision 9 diff printed nothing so the lines are unit 401 exit runs"
sh .run-unit/unit402-floors.sh floors-1 600 "FullyQualifiedName~TheCapturesThatDecodeKeepDecodingTests" "TASK 0 of 5" "task 0: entry build done, captures floor type running" --no-build
date +%H:%M:%S
