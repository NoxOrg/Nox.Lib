<# goo.ps1 - Type less. Code more.

    Develop, build, test and run helper script built on Powershell

    Developed by Andre Sharpe on October, 24 2020.

    www.goo.dev

    1. '.\.goo' will output the comment headers for each implemented command
    
    2. Add a function with its purpose in its comment header to extend this project's goo file 

    3. 'goo <function>' will run your commands 
#>

<# --- NEW GOO INSTANCE --- #>

using module '.\.goo\goo.psm1'

$goo = [Goo]::new($args)


<# --- SET GLOBAL SCRIPT VARIABLES HERE --- #>

$script:SolutionName            = 'Nox'

$script:RootFolder              = (Get-Location).Path
$script:SourceFolder            = '.\src'
$script:SamplesFolder          	= '.\samples'
$script:TestsFolder            	= '.\tests'
$script:SolutionFolder          = $script:SourceFolder
$script:SolutionFile            = "$script:SolutionFolder\Nox.sln"
$script:ProjectFolder           = "$script:SamplesFolder\Samples.Api"
$script:ProjectFile             = "$script:SamplesFolder\Samples.Api.csproj"
$script:CliFolder               = "$script:SamplesFolder\Samples.Cli"

$script:DefaultEnvironment      = 'Samples'

$script:DockerContainerName     = 'nox'

$script:DbServer                = '127.0.0.1'
$script:DbPort                  = '1433'
$script:DbName                  = 'nox'
$script:MasterDbName            = 'master'
$script:DbUser                  = 'sa'
$script:DbPassword              = 'Developer*123'

$script:DbConnectionstring = (
    "Server=$script:DbServer,$script:DbPort;" +
    "Database=$script:DbName;" + 
    "User ID=$script:DbUser;" +
    "Password=$script:DbPassword;" + 
    "Integrated Security=false;" +
    "Connection Timeout=120;" +
    "Application Name=$script:SolutionName;" 
)

<# --- SET YOUR PROJECT'S ENVIRONMENT VARIABLES HERE --- #>

if($null -eq $Env:Environment)
{
    $Env:ENVIRONMENT = $script:DefaultEnvironment
    $Env:ASPNETCORE_ENVIRONMENT = $script:DefaultEnvironment
}


<# --- ADD YOUR COMMAND DEFINITIONS HERE --- #>

<# 
    A good 'init' command will ensure a freshly cloned project will run first time.
    Guide the developer to do so easily. Check for required tools. Install them if needed. Set magic environment variables if needed.
    This should ideally replace your "Getting Started" section in your README.md
    Type less. Code more. (And get your team or collaboraters started quickly and productively!)
#>

# command: goo init | Run this command first, or to reset project completely. 
$goo.Command.Add( 'init', {
    $goo.Command.Run( 'vpn' )
    $goo.Command.Run( 'clean' )
    $goo.Command.Run( 'build' )
    $goo.Command.Run( 'up' )
    $goo.Command.Run( 'run' )
})

# command: goo clean | Removes data and build output
$goo.Command.Add( 'clean', {
    $goo.Console.WriteInfo( "Cleaning data and distribution folders..." )
    $goo.Command.Run('dockerDownIfUp')
    $goo.IO.EnsureRemoveFolder("./.docker-data")
    $goo.IO.EnsureRemoveFolder("./dist")
    $goo.IO.EnsureRemoveFolder("./src/dist")
    $goo.Command.RunExternal('dotnet','restore --verbosity:quiet --nologo',$script:SolutionFolder)
    $goo.Command.RunExternal('dotnet','clean --verbosity:quiet --nologo',$script:SolutionFolder)
    $goo.StopIfError("Failed to clean previous builds. (Release)")
})


# command: goo build | Builds the solution and command line app. 
$goo.Command.Add( 'build', {
    $goo.Console.WriteInfo("Building solution...")
    $goo.Command.RunExternal('dotnet','build /clp:ErrorsOnly --configuration Release', $script:SolutionFolder)
    $goo.StopIfError("Failed to build solution. (Release)")
    $goo.Command.RunExternal('dotnet','publish --configuration Release --output ..\dist --no-build', $script:ProjectFolder )
    $goo.StopIfError("Failed to publish CLI project. (Release)")
})

# command: goo up | Starts your local SQL Server in a Docker container
$goo.Command.Add( 'up', {
    $goo.Console.WriteInfo('Starting containers...')
    if($IsWindows) {
        $goo.Console.WriteInfo('Using override docker-compose.windows.yaml (fast Sql Server image)...')
        $goo.Docker.Up( $script:RootFolder, 'docker-compose.windows.yaml' ) 
    } else {
        $goo.Docker.Up() 
    }
    $goo.StopIfError('Failed to start container.')
})

# command: goo down | Stops API and your local SQL Server Docker container
$goo.Command.Add( 'down', {
    $goo.Console.WriteInfo("Stopping API & local SQL Server...")
    $goo.Command.EnsureProcessStopped('dotnet')
    $goo.Command.Run( 'dockerDownIfUp' )
})

$goo.Command.Add( 'refreshDocker', {
    $goo.Console.WriteInfo("Refreshing container [$script:DockerContainerName]...")
    $goo.Command.Run('dockerDownIfUp')
})

$goo.Command.Add( 'dockerDownIfUp', {
    $goo.Docker.Down( $script:RootFolder )
    $goo.StopIfError("Failed to stop container.")
})

$goo.Command.Add( 'vpn', {
    $pricingServer = '10.45.141.36'
    $goo.Console.WriteInfo( "Checking VPN connection [route to $pricingServer]..." )
    $goo.Network.EnsureConnectionTo( $pricingServer )
})

# command: goo env | Show all environment variables
$goo.Command.Add( 'env', { param($dbEnvironment,$dbInstance)
    $goo.Console.WriteLine( "environment variables" )
    $goo.Console.WriteLine( "=====================" )
    Get-ChildItem -Path Env: | Sort-Object -Property Name | Out-Host

    $goo.Console.WriteLine( "dotnet user-secrets" )
    $goo.Console.WriteLine( "===================" )
    $goo.Console.WriteLine() 
    $goo.Command.RunExternal('dotnet',"user-secrets list --project $script:ProjectFile")
})

# command: goo setenv <env> | Sets local environment to <env> environment
$goo.Command.Add( 'setenv', { param( $Environment )
    $oldEnv = $Env:ENVIRONMENT
    $Env:ENVIRONMENT = $Environment
    $Env:ASPNETCORE_ENVIRONMENT = $Environment
    $goo.Console.WriteInfo("Environment changed from [$oldEnv] to [$Env:ENVIRONMENT]")
})

# command: goo dev | Start up Visual Studio and VS Code for solution
$goo.Command.Add( 'dev', { 
    $goo.Command.StartProcess($script:SolutionFile)
    $goo.Command.StartProcess('code','.')
})

# command: goo run | Run the console application
$goo.Command.Add( 'run', {
    $goo.Command.Run( 'waitfordb' )
    $goo.Command.RunExternal('dotnet','run',$script:ProjectFolder)
})

# command: goo listen | View events being emmited from event source
$goo.Command.Add( 'listen', {
    $goo.Command.Run( 'waitfordb' )
    $goo.Console.WriteLine( "starting the Event Listener..." )
    $goo.Command.RunExternal('dotnet','run -- listen',$script:CliFolder)
})

# command: goo sql <query> | Executes a query on your local SQL server container
$goo.Command.Add( 'sql', { param( $sql )
    $goo.Sql.Query( $script:DbConnectionstring, $sql )
})


# command: goo feature <name> | Creates a new feature branch from your main git branch
$goo.Command.Add( 'feature', { param( $featureName )
    $goo.Git.CheckoutFeature($featureName)
})

# command: goo push <message> | Performs 'git add -A', 'git commit -m <message>', 'git -u push origin'
$goo.Command.Add( 'push', { param( $message )
    $current = $goo.Git.CurrentBranch()
    $head = $goo.Git.HeadBranch()
    if($head -eq $current) {
        $goo.Error("You can't push directly to the '$head' branch")
    }
    else {
        $goo.Git.AddCommitPushRemote($message)
    }
})

# command: goo pr | Performs and merges a pull request, checkout main and publish'
$goo.Command.Add( 'pr', { 
    gh pr create --fill
    if($?) { gh pr merge --merge }
    $goo.Command.Run( 'main' )
})

# command: goo main | Checks out the main branch and prunes features removed at origin
$goo.Command.Add( 'main', { param( $featureName )
    $goo.Git.CheckoutMain()
})

$goo.Command.Add( 'bump-project-version', {
    $files = (Get-ChildItem "*.csproj" -Recurse)
    $xpaths = @(
        "//AssemblyVersion",
        "//FileVersion",
        "//PackageVersion",
        "//PackageReference[@Include='Nox.Lib']/@Version"
    )

    $xml = New-Object XML
    foreach($file in $files){
        $updated = $false
        $versionNew = $null
        $xml.Load($file)
        foreach($p in $xpaths){ 
            $node = $xml.SelectSingleNode($p)
            if($null -ne $node){
                $version = (($node.InnerText ?? $node.Value) -split '\.')
                $version[2] = [int]($version[2])+1
                $versionNew = ($version -join '.')
                $node.InnerText = $versionNew
                $updated = $true
        }
        }
        if ($updated) {
            $goo.Console.WriteLine("Bumping version for $($file.Name) to $versionNew..." )
            $xml.Save($file)
        }
    }
})

# command: goo publish | Build and publish Nox nuget packages
$goo.Command.Add( 'publish', { 
    $goo.Command.Run( 'bump-project-version' )
	$goo.Console.WriteInfo("Packing project ($script:SourceFolder\Nox.Lib)...")
    $goo.Command.RunExternal('dotnet','pack /clp:ErrorsOnly --configuration Release', "$script:SourceFolder\Nox.Lib")
    $goo.StopIfError("Failed to pack Nox.Lib (Release)")
    $nupkgFile = Get-ChildItem "$script:SourceFolder\Nox.Lib\bin\Release\Nox.Lib.*.nupkg" | Sort-Object -Property LastWriteTime | Select-Object -Last 1
    $goo.Command.RunExternal('dotnet',"nuget push $($nupkgFile.FullName) --api-key $Env:NUGET_API_KEY --source https://api.nuget.org/v3/index.json", "$script:SourceFolder\Nox.Lib")
    $goo.StopIfError("Failed to publish Nox.Microservice to nuget. (Release)")
})

$goo.Command.Add( 'waitfordb', {
    $masterConnectionString = (
        "Server=$script:DbServer,$script:DbPort;" +
        "Database=$script:MasterDbName;" + 
        "User ID=$script:DbUser;" +
        "Password=$script:DbPassword;" + 
        "Integrated Security=false;" +
        "Connection Timeout=120;" +
        "Application Name=$script:SolutionName;" 
    )
    $goo.Console.WriteInfo("Master Connection String: $masterConnectionString")
    while (-not $goo.Sql.TestConnection( $masterConnectionString )){
        $goo.Console.WriteInfo('Waiting for docker SQL database to be ready to accept connections...')
        $goo.Sleep(10)
    }
})

<# --- START GOO EXECUTION --- #>

$goo.Start()



<# --- EOF --- #>
