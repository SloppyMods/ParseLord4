param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$env:APPDATA = "C:\Users\you\AppData\Roaming"
$env:LOCALAPPDATA = "C:\Users\you\AppData\Local"
$env:USERPROFILE = "C:\Users\you"
$env:DOTNET_CLI_HOME = "C:\Users\you"
$env:ProgramData = "C:\ProgramData"
$env:ProgramFiles = "C:\Program Files"
$env:ProgramFiles_x86 = "C:\Program Files (x86)"
$env:windir = "C:\Windows"
$env:SystemRoot = "C:\Windows"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$env:DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE = "1"

dotnet build "stages\06-polish\output\ParseLord4\ParseLord4.csproj" -c $Configuration
