$productName = "Circuit Diagram (Nightly)"
$productVersion = [environment]::GetEnvironmentVariable("APPVEYOR_BUILD_VERSION");
If ([environment]::GetEnvironmentVariable("BUILD_CHANNEL_STABLE") -eq "true") {
    $productName = "Circuit Diagram"
} ElseIf ([environment]::GetEnvironmentVariable("BUILD_CHANNEL_PRE") -eq "true") {
    $productName = "Circuit Diagram (Prerelease)"
}

& "C:\Program Files (x86)\WiX Toolset v3.10\bin\candle.exe" Product.wxs -ext WixUIExtension `
    "-dProductName=$productName" `
    "-dProductVersion=$productVersion"
& "C:\Program Files (x86)\WiX Toolset v3.10\bin\light.exe" -out Output\Setup.msi Product.wixobj -ext WixUIExtension
