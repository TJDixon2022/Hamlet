$bin = "C:\Source\HamLet\tests\Hamlet.RadioEngine.Tests\bin\Debug\net8.0"
Add-Type -Path (Join-Path $bin "NAudio.Core.dll")   | Out-Null
Add-Type -Path (Join-Path $bin "NAudio.Wasapi.dll") | Out-Null
Add-Type -Path (Join-Path $bin "Hamlet.RadioEngine.dll") | Out-Null

$t = [Hamlet.RadioEngine.Audio.WasapiTransmitSink]
$list = $t::Endpoints()
Write-Output ("COUNT: " + $list.Count)
foreach ($e in $list) {
  Write-Output ("ID: " + $e.Id)
  Write-Output ("  NAME: " + $e.Name + " | DEFAULT: " + $e.IsDefault + " | RATE: " + $e.SampleRate + " | CHANNELS: " + $e.Channels + " | BITS: " + $e.BitsPerSample + " | ENCODING: " + $e.Encoding)
  $expl = ""
  $ok = [Hamlet.RadioEngine.Transmit.Ft8Composer]::RateIsUsable($e.SampleRate, [ref]$expl)
  Write-Output ("  RateIsUsable: " + $ok + " | " + $expl)
  $bexpl = ""
  $bok = [Hamlet.RadioEngine.Transmit.Ft8Composer]::BaseFrequencyIsUsable([Hamlet.RadioEngine.Transmit.Ft8Composer]::DefaultBaseFrequencyHz, $e.SampleRate, [ref]$bexpl)
  Write-Output ("  BaseFrequencyIsUsable(" + [Hamlet.RadioEngine.Transmit.Ft8Composer]::DefaultBaseFrequencyHz + " Hz): " + $bok + " | " + $bexpl)
}
