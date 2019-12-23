param (
    [Parameter(Mandatory = $true)][string]$version
)

# Find vsvarsall and run it, then inherit all variables
$vswhere = 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe'
$visualStudioPath = & $vswhere -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath
"VisualStudio Path: $visualStudioPath"

if ($visualStudioPath)
{
    $path = join-path $visualStudioPath 'Common7\Tools\VsDevCmd.bat'
    if (test-path $path)
    {
        cmd /s /c """$path"" $args && set" | where {
            $_ -match '(\w+)=(.*)'
        } | foreach {
            if ($Matches[1] == "PATH")
            {
                $null = new-item -force -path "Env:\$( $Matches[1] )" -value $Matches[2]
            }
        }
    }
}

# find the appropriate version of the Visual C++ libs by taking the first
$msvcRedistBaseDir = "$visualStudioPath\VC\Redist\MSVC"
$msvcRedistVersion = Get-ChildItem -Directory $msvcRedistBaseDir | Sort-Object -Property { $_.Name } -Descending | Select-Object -first 1
"Latest MSVC Redist Version: $msvcRedistVersion"

[string[]]$platforms = "x64", "x86"

$root = [IO.Path]::Combine($PSScriptRoot, "..", "..")

[string]$manifestTemplate = [IO.Path]::Combine($PSScriptRoot, "AppxManifest.xml")
[string]$appInstallerTemplate = [IO.Path]::Combine($PSScriptRoot, "OpenTemple.appinstaller")

Push-Location $root

try
{
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
        # Copy over the VC++ runtime files needed by the platform (which differs by platform)
        #
        switch ($platform)
        {
            "x86" {
                $redistDir = "$msvcRedistBaseDir\$msvcRedistVersion\x86\Microsoft.VC142.CRT"
                $redistFiles = "msvcp140.dll", "msvcp140_1.dll", "msvcp140_2.dll", "msvcp140_codecvt_ids.dll", "vcruntime140.dll"
            }
            "x64" {
                $redistDir = "$msvcRedistBaseDir\$msvcRedistVersion\x64\Microsoft.VC142.CRT"
                $redistFiles = "msvcp140.dll", "msvcp140_1.dll", "msvcp140_2.dll", "msvcp140_codecvt_ids.dll", "vcruntime140.dll", "vcruntime140_1.dll"
            }
            Default {
                throw "Can't handle VC++ redistributable for platform $platform"
            }
        }
        foreach ($file in $redistFiles)
        {
            Copy-Item "$redistDir\$file" dist/$platform
        }

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
    # NOTE: This is a flat bundle since we are sideloading and aren't restricted
    makeappx bundle /fb /d dist/packages /bv $version /p dist/packages/OpenTemple.msixbundle

    #
    # Create an appinstaller file from the template and fill out the version number
    #
    [xml]$xmlDoc = Get-Content $appInstallerTemplate
    $xmlDoc.AppInstaller.Version = $version
    $xmlDoc.AppInstaller.MainBundle.Version = $version
    $xmlDoc.Save("$root/dist/packages/OpenTemple.appinstaller")

}
finally
{
    Pop-Location
}
