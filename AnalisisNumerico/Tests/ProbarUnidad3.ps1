param([string]$CarpetaCapturas)

$ErrorActionPreference = 'Stop'
if ($PSVersionTable.PSEdition -eq 'Core' -or [System.Threading.Thread]::CurrentThread.ApartmentState -ne 'STA') {
    $windowsPowerShell = Join-Path $env:WINDIR 'System32\WindowsPowerShell\v1.0\powershell.exe'
    & $windowsPowerShell -NoProfile -STA -ExecutionPolicy Bypass -File $PSCommandPath -CarpetaCapturas $CarpetaCapturas
    if ($LASTEXITCODE -ne 0) { throw 'Fallaron las pruebas de Unidad 3.' }
    return
}
$salida = Join-Path $PSScriptRoot '..\AnalisisNumericoWeb\bin\Debug'
$compilador = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$pruebas = Join-Path $salida 'Unidad3Tests.dll'
$pantalla = Join-Path $PSScriptRoot '..\AnalisisNumericoWeb\Unidad3\Unidad3.cs'
$grafico = Join-Path $PSScriptRoot '..\AnalisisNumericoWeb\Unidad3\GraficoAjuste.cs'
$entornoGeoGebra = Join-Path $PSScriptRoot '..\AnalisisNumericoWeb\GeoGebraWebView.cs'
$htmlGeoGebra = Join-Path $PSScriptRoot '..\AnalisisNumericoWeb\Unidad3\GeoGebraAjuste.html'
$webViewCore = Join-Path $salida 'Microsoft.Web.WebView2.Core.dll'
$webViewForms = Join-Path $salida 'Microsoft.Web.WebView2.WinForms.dll'
$carpetaLogica = Join-Path $PSScriptRoot '..\LogicaAnalisis'
$fuentes = @((Join-Path $PSScriptRoot 'Unidad3Tests.cs'), $pantalla, $grafico, $entornoGeoGebra)
$fuentes += Get-ChildItem -LiteralPath (Join-Path $carpetaLogica 'Unidad3') -Filter '*.cs' | Select-Object -ExpandProperty FullName
$fuentes += @('MetodoGaussJordan.cs', 'ResultadoGaussJordan.cs', 'IteracionGaussJordan.cs') | ForEach-Object { Join-Path $carpetaLogica "Unidad2\$_" }

New-Item -ItemType Directory -Path $salida -Force | Out-Null

& $compilador /nologo /target:library "/out:$pruebas" /reference:System.Drawing.dll /reference:System.Windows.Forms.dll /reference:System.Web.Extensions.dll "/reference:$webViewCore" "/reference:$webViewForms" "/resource:$htmlGeoGebra,AnalisisNumericoWeb.Unidad3.GeoGebraAjuste.html" $fuentes
if ($LASTEXITCODE -ne 0) { throw 'No se pudieron compilar las pruebas.' }
[void][System.Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $webViewCore).Path)
[void][System.Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $webViewForms).Path)
$ensamblado = [System.Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $pruebas).Path)
$argumentos = [string[]]@()
if ($CarpetaCapturas) { $argumentos = [string[]]@($CarpetaCapturas) }
$codigo = $ensamblado.GetType('Unidad3Tests').GetMethod('Run').Invoke($null, [object[]]@(,$argumentos))
if ($codigo -ne 0) { throw 'Fallaron las pruebas de Unidad 3.' }
