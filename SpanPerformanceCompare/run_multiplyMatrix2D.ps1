#Requires -Version 7
param(
    [string]$BenchmarkFilter = 'SpanPerformanceCompare.MultiplyMatrix2D.*',
    [string[]]$NetRuntimes = @('net9.0'),
    [string]$BuildFramework = 'net9.0'
)
$ErrorActionPreference = "Stop"
$run_command = "dotnet run --configuration Release --framework $BuildFramework -- --runtimes $NetRuntimes --filter '$BenchmarkFilter'"
Write-Host "Running command:`n$run_command"
Invoke-Expression $run_command
exit