If ([environment]::GetEnvironmentVariable("APPVEYOR_REPO_TAG")) {
    $tag = [environment]::GetEnvironmentVariable("APPVEYOR_REPO_TAG_NAME")
    
    If ($tag -match "-alpha" -or $tag -match "-beta") {
        [environment]::SetEnvironmentVariable("BUILD_CHANNEL_PRE", "TRUE", "User")
        "Build channel: Prerelease"
    } Else {
        [environment]::SetEnvironmentVariable("BUILD_CHANNEL_STABLE", "TRUE", "User")
        "Build channel: Stable"
    }
} Else {
    [environment]::SetEnvironmentVariable("BUILD_CHANNEL_NIGHTLY", "TRUE", "User")
    "Build channel: Nightly"
}
