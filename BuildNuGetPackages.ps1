# Will build packages from files in /Nuspecs/, using assembly versions and prefixes depending on branch and build number.
# Output goes in /Build/.
# Requires projects to be already build in Release configuration.
# Nuspec files will have string $PackageVersion replaced by the value returned by Get-Package-Version. 
# Final .nuspec files are also saved in /Build/.

function Build-Packages() {
	Clear-Build-Folder
    # Build-Package <Project name to use when getting assembly version> <Name of Nuspec file in /Nuspecs/>
    Build-Package "Yodii.Model" "Yodii.Core"
    Build-Package "Yodii.Wpf" "Yodii.Wpf"
    Build-Package "Yodii.Host" "Yodii"
}

function Clear-Build-Folder() {
	if( Test-Path "Build" ) {
		Remove-Item "Build" -Recurse -Force
	}
}

function Get-Package-Version( $BaseAssemblyPath ) {
    # Check for git
    if (!(Get-Command "git.exe" -ErrorAction SilentlyContinue)) 
    { 
       throw "git.exe is required in the PATH."
    }

    if(!(Test-Path $BaseAssemblyPath)) {
        throw "$BaseAssemblyPath not found. Is the assembly correctly built?"
    }

    $AbsoluteAssemblyPath = Resolve-Path $BaseAssemblyPath

    $Assembly = [Reflection.Assembly]::Loadfile($AbsoluteAssemblyPath)
    $AssemblyName = $Assembly.GetName()
    $AssemblyVersion =  $AssemblyName.version

    $BaseAssemblyVersion = "{0}.{1}.{2}" -f $AssemblyVersion.Major, $AssemblyVersion.Minor, $AssemblyVersion.Build

    $RefName = (git.exe symbolic-ref HEAD)
    if ($RefName.StartsWith("fatal") ) {
        throw "Failed to find git repository. {0}" -f $RefName
    }
    $PartialRefName = $RefName.Substring($RefName.LastIndexOf("/") +1)

    if($PartialRefName -eq "master") {
        return $BaseAssemblyVersion;
    } else {
        # Build number is set by eg. TeamCity
        if($env:build_number) { 
            return "{0}-{1}-{2}" -f $BaseAssemblyVersion, $PartialRefName, $env:build_number
        } else {
            return "{0}-{1}" -f $BaseAssemblyVersion, $PartialRefName
        }
    }
}

function Build-Package-File($NuspecName, $PackageVersion) {

    $NuspecPath = "Nuspecs/{0}.nuspec" -f $NuspecName
    $OutputFile = "Build/{0}.nuspec" -f $NuspecName

    if(!( Test-Path $NuspecPath )) { throw "Nuspec not found: {0}" -f $NuspecPath }
    
    New-Item -ItemType Directory -Force -Path "Build" | Out-Null
    
    $NuspecContent = Get-Content $NuspecPath
    $NewContent = $NuspecContent.Replace('$PackageVersion', $PackageVersion)
    Set-Content -Path $OutputFile -Value $NewContent
    
    return $OutputFile
}

function Build-Package( $ProjectName, $NuspecName ) {
    $AssemblyPath = Resolve-Path ("{0}/bin/Release/{0}.dll" -f $ProjectName)
    $NuspecVersion = Get-Package-Version $AssemblyPath
    $NuspecPath = Build-Package-File $NuspecName $NuspecVersion

    NuGet pack "$NuspecPath" -OutputDirectory "Build"
}

Build-Packages