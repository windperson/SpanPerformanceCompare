#Requires -Version 7
param(
    [string]$BenchmarkFilter = 'SpanPerformanceCompare.ExtractSubstring.*',
    [string[]]$NetRuntimes = @('net8.0', 'net9.0', 'net7.0', 'net6.0'),
    [string]$BuildFramwework = 'net9.0'
)
$ErrorActionPreference = "Stop"
$run_command = "dotnet run --configuration Release --framework $BuildFramwework -- --runtimes $NetRuntimes --filter '$BenchmarkFilter'"
Write-Host "Running command:`n$run_command"
Invoke-Expression $run_command
exit