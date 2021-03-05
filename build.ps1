[CmdletBinding()]
Param([string[]]$tasks)

# Clean all projects
function clean {
    $bins = Get-ChildItem -Directory -Recurse -Filter bin
    foreach ($bin in $bins) {
        Remove-Item $bin.FullName -Recurse -Force
    }
    dotnet clean --configuration "Release"
}

# Restore NuGet packages for all projects
function restore {
    dotnet restore
}

# Build projects
function build {
    dotnet build --configuration "Release"
}

# Execute the tests
function test {
    Write-Host "No tests"
}

# Create the NuGet package
function pack {
    dotnet pack "src/MarkEmbling.Forms.Controls/MarkEmbling.Forms.Controls.csproj"
}

# Publish packages to the NuGet gallery
function publish {
    $packages = Get-ChildItem src -Recurse -Filter *.nupkg
    foreach ($package in $packages) {
        $choices = @(
            [Management.Automation.Host.ChoiceDescription]::new("&Publish", "Publish this package to the NuGet gallery"),
            [Management.Automation.Host.ChoiceDescription]::new("&Skip", "Skip this package")
        )
        $answer = $Host.UI.PromptForChoice("Publish Package", "Publish $package to the NuGet gallery?", $choices, 1)
        if ($answer -eq 0) {
            dotnet nuget push $package.FullName --source https://www.nuget.org
        }
    }
}

# Default tasks
if ($tasks.Count -eq 0) {
    $tasks = @("clean", "restore", "build", "test", "pack")
}

# Execute
foreach ($task in $tasks) {
    Write-Host "Running task $task..." -ForegroundColor DarkGray
    switch($task) {
        "clean" { clean }
        "restore" { restore }
        "build" { build }
        "test" { test }
        "pack" { pack }
        "publish" { publish }
        default { Write-Host "Task $task not recognised." -ForegroundColor Red }
    }
    if ($lastExitCode -ne $null -and $lastExitCode -ne 0) { exit $lastExitCode }
}
