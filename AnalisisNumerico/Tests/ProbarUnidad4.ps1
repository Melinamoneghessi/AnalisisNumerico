param([string]$CarpetaCapturas, [switch]$SoloCalculos)

$ErrorActionPreference = 'Stop'
if ($PSVersionTable.PSEdition -eq 'Core' -or [System.Threading.Thread]::CurrentThread.ApartmentState -ne 'STA') {
    $argumentos = @('-NoProfile', '-STA', '-ExecutionPolicy', 'Bypass', '-File', $PSCommandPath)
    if ($CarpetaCapturas) { $argumentos += @('-CarpetaCapturas', $CarpetaCapturas) }
    if ($SoloCalculos) { $argumentos += '-SoloCalculos' }
    & (Join-Path $env:WINDIR 'System32\WindowsPowerShell\v1.0\powershell.exe') @argumentos
    if ($LASTEXITCODE -ne 0) { throw 'Fallaron las pruebas de Unidad 4.' }
    return
}
$msbuild = (Get-Command MSBuild.exe -ErrorAction SilentlyContinue | Select-Object -First 1).Source
if (-not $msbuild) {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    if (Test-Path -LiteralPath $vswhere) {
        $msbuild = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
    }
}
if (-not $msbuild) { throw 'No se encontro MSBuild. Instale las herramientas de compilacion de Visual Studio.' }
& $msbuild (Join-Path $PSScriptRoot '..\AnalisisNumerico.sln') /p:Configuration=Debug '/p:Platform=Any CPU' '/p:OutputPath=bin\VerificacionUnidad4\' /v:minimal /nologo
if ($LASTEXITCODE -ne 0) { throw 'No se pudo compilar la solucion.' }
$salida = Join-Path $PSScriptRoot '..\AnalisisNumericoWeb\bin\VerificacionUnidad4'
$pruebas = Join-Path $salida 'Unidad4Tests.dll'
$referencias = @('LogicaAnalisis.dll', 'AnalisisNumericoWeb.exe', 'Microsoft.Web.WebView2.Core.dll', 'Microsoft.Web.WebView2.WinForms.dll') | ForEach-Object { Join-Path $salida $_ }
$parametros = @('/nologo', '/target:library', "/out:$pruebas", '/reference:System.Drawing.dll', '/reference:System.Windows.Forms.dll', '/reference:System.Web.Extensions.dll')
$parametros += $referencias | ForEach-Object { "/reference:$_" }
$parametros += Join-Path $PSScriptRoot 'Unidad4Tests.cs'
& (Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe') @parametros
if ($LASTEXITCODE -ne 0) { throw 'No se pudieron compilar las pruebas.' }
foreach ($archivo in $referencias) { [void][System.Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $archivo).Path) }
$ensamblado = [System.Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $pruebas).Path)
$argsPruebas = [string[]]@($CarpetaCapturas, [string]$SoloCalculos.IsPresent)
$codigo = $ensamblado.GetType('Unidad4Tests').GetMethod('Run').Invoke($null, [object[]]@(,$argsPruebas))
if ($codigo -ne 0) { throw 'Fallaron las pruebas de Unidad 4.' }
