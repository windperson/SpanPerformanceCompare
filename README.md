# Span performance comparison

This repository contains performance comparisons between using [`Span<T>`](https://learn.microsoft.com/dotnet/api/system.span-1), [`Memory<T>`](https://learn.microsoft.com/dotnet/api/system.memory-1) and some other ways in C#.

## How to run

 1. Install [.NET SDk 6 to 9](https://dotnet.microsoft.com/download/dotnet) if you want to run all benchmarks, by default the launch PowerShell script will just run only .NET 9 benchmarks.
 2. [PowerShell v7+](https://learn.microsoft.com/powershell/scripting/install/installing-powershell)

(**Note**: To install .NET 9 SDK on Ubuntu 22.04, you need to [use bash install script to install the .NET 9 SDK](https://blog.dangl.me/archive/installing-net-9-alongside-older-versions-on-ubuntu-2204/))

### SpanPerformanceCompare

The **SpanPerformanceCompare** project contains a few benchmarks to compare use or not use `Span<T>` performance difference.
