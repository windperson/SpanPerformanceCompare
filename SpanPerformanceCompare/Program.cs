using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

var config = ManualConfig.Create(DefaultConfig.Instance)
    .AddJob(Job.Default.WithPowerPlan(PowerPlan.UserPowerPlan).AsDefault());

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);