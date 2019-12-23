param (
    [Parameter(Mandatory = $true)][string]$version
)

[string[]]$platforms = "x64", "x86"

$root = [IO.Path]::Combine($PSScriptRoot, "..", "..")

[string]$manifestTemplate = [IO.Path]::Combine($PSScriptRoot, "AppxManifest.xml")
[string]$appInstallerTemplate = [IO.Path]::Combine($PSScriptRoot, "OpenTemple.appinstaller")

Set-Location $root
Remove-Item -Recurse dist -ErrorAction Ignore
mkdir dist/packages

#
# Publish the desired platforms
#
foreach ($platform in $platforms)
{
    Remove-Item -Recurse dist/$platform -ErrorAction Ignore

    #
    # This will actually build / publish the project for the respective platform!
    #
    dotnet publish -c Release -o dist/$platform -r win-$platform Launcher

    #
    # For debugging, also add a nightly.txt to the folder
    Set-Content "$root/dist/$platform/build.txt" "$version-$env:GITHUB_SHA"

    #
    # Update the AppManifest's version and platform
    #
    [xml]$xmlDoc = Get-Content $manifestTemplate
    $xmlDoc.Package.Identity.ProcessorArchitecture = $platform
    $xmlDoc.Package.Identity.Version = $version

    # Write out the App manifest into the build directory
    $xmlDoc.Save("$root/dist/$platform/AppxManifest.xml")

    # Copy over the images
    $images = [IO.Path]::Combine($PSScriptRoot, "Images")
    Copy-Item -Recurse $images "$root/dist/$platform/Images"

    # Update PRI stuff
    makepri new /of "$root/dist/$platform/resources.pri" /pr "$root/dist/$platform" /cf "$PSScriptRoot\priconfig.xml"

    # Make the MSIX package
    makeappx pack /d dist/$platform /p dist/packages/OpenTemple_$platform.msix
}

# Make the MSIX bundle
makeappx bundle /d dist/packages /p dist/OpenTemple.msixbundle

#
# Create an appinstaller file from the template and fill out the version number
#
[xml]$xmlDoc = Get-Content $appInstallerTemplate
$xmlDoc.AppInstaller.Version = $version
$xmlDoc.AppInstaller.MainBundle.Version = $version
$xmlDoc.Save("$root/dist/OpenTemple.appinstaller")

Set-Location $PSScriptRoot
