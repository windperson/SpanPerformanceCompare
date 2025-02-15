using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

var config = ManualConfig.Create(DefaultConfig.Instance)
                         .AddJob(Job.Default.DontEnforcePowerPlan().AsDefault());

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);