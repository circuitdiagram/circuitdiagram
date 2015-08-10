If ([environment]::GetEnvironmentVariable("APPVEYOR_REPO_TAG") -eq "true") {
    $tag = [environment]::GetEnvironmentVariable("APPVEYOR_REPO_TAG_NAME")
    
    If ($tag -like "v*-alpha*" -or $tag -like "v*-beta*") {
        [environment]::SetEnvironmentVariable("BUILD_CHANNEL_PRE", "true")
        "Build channel: Prerelease"
    } ElseIf ($tag -like "v*") {
        [environment]::SetEnvironmentVariable("BUILD_CHANNEL_STABLE", "true")
        "Build channel: Stable"
    } Else {
        [environment]::SetEnvironmentVariable("BUILD_CHANNEL_NIGHTLY", "true")
        "Build channel: Nightly"
    }
} Else {
    [environment]::SetEnvironmentVariable("BUILD_CHANNEL_NIGHTLY", "true")
    "Build channel: Nightly"
}
