#Install Wix
& ".\nuget" install Wix -Version 3.9.2

$productName = "Circuit Diagram (Nightly)"
$productVersion = [environment]::GetEnvironmentVariable("APPVEYOR_BUILD_VERSION");
If ([environment]::GetEnvironmentVariable("BUILD_CHANNEL_STABLE") -eq "true") {
    $productName = "Circuit Diagram"
} ElseIf ([environment]::GetEnvironmentVariable("BUILD_CHANNEL_PRE") -eq "true") {
    $productName = "Circuit Diagram (Prerelease)"
}

& ".\WiX.3.9.2\tools\candle.exe" Product.wxs -ext WixUIExtension `
    "-dProductName=$productName" `
    "-dProductVersion=$productVersion"
& ".\WiX.3.9.2\tools\light.exe" -out Output\Setup.msi Product.wixobj -ext WixUIExtension
