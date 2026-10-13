param(
    [Parameter(Mandatory=$true)][string]$ProjectPath,
    [string]$EditorPath = 'C:/Program Files/Unity/Hub/Editor/2022.3.22f1/Editor',
    [string]$VisualStudioPath = 'C:/BuildTools',
    [string]$WindowsSdkVersion = '10.0.26100.0'
)
$ErrorActionPreference = 'Stop'
$project = (Resolve-Path -LiteralPath $ProjectPath).Path
if (-not (Test-Path -LiteralPath "$project/ProjectSettings/ProjectVersion.txt")) { throw 'Use an isolated Unity validation project.' }
$compiler = Get-ChildItem -LiteralPath "$VisualStudioPath/VC/Tools/MSVC" -Directory | Sort-Object Name -Descending | Select-Object -First 1
$vc = $compiler.FullName
$sdk = 'C:/Program Files (x86)/Windows Kits/10'
$build = "$project/Temp/NovaBenchmarkNativeBuild"
$plugin = "$project/Assets/NovaBenchmark/Plugins/x86_64"
New-Item -ItemType Directory -Force $build,$plugin | Out-Null
& "$vc/bin/Hostx64/x64/cl.exe" /nologo /LD /EHsc /O2 /std:c++17 /utf-8 /W4 `
    "/I$vc/include" "/I$sdk/Include/$WindowsSdkVersion/ucrt" "/I$sdk/Include/$WindowsSdkVersion/um" `
    "/I$sdk/Include/$WindowsSdkVersion/shared" "/I$EditorPath/Data/PluginAPI" `
    "$PSScriptRoot/NovaGpuTimer.cpp" "/Fo$build/NovaGpuTimer.obj" "/Fe$build/NovaGpuTimer.dll" `
    /link "/LIBPATH:$vc/lib/x64" "/LIBPATH:$sdk/Lib/$WindowsSdkVersion/um/x64" "/LIBPATH:$sdk/Lib/$WindowsSdkVersion/ucrt/x64"
if ($LASTEXITCODE -ne 0) { throw 'Native timer compilation failed.' }
Copy-Item -LiteralPath "$build/NovaGpuTimer.dll" -Destination "$plugin/NovaGpuTimer.dll"
