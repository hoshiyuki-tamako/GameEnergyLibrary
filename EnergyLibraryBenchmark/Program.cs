using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using EnergyLibrary;
using System;

namespace EnergyLibraryBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<EnergyBenchmark>();
            Console.ReadLine();
        }
    }

    [MemoryDiagnoser]
    public class EnergyBenchmark
    {
        public EnergyOption energyOption = new()
        {
            Interval = TimeSpan.FromMilliseconds(10),
            MinAmount = int.MinValue,
            MaxAmount = int.MaxValue,
        };

        public Energy energyEmpty;
        public Energy energyFull;

        public EnergyBenchmark()
        {
            energyEmpty = new(energyOption, energyOption.MinAmount, DateTime.Now);
            energyFull = new(energyOption, energyOption.MaxAmount, DateTime.Now);
        }

        [Benchmark]
        public int Total()
        {
            return energyEmpty.Total;
        }

        [Benchmark]
        public int Add()
        {
            return energyEmpty.Add(1);
        }

        [Benchmark]
        public int Use()
        {
            return energyFull.Use(1);
        }

        [Benchmark]
        public TimeSpan TimeUntilNext()
        {
            return energyEmpty .TimeUntilNext();
        }

        [Benchmark]
        public TimeSpan TimeUntilFull()
        {
            return energyEmpty.TimeUntilFull();
        }

        [Benchmark]
        public bool CanUse()
        {
            return energyEmpty.CanUse(1);
        }

        [Benchmark]
        public bool IsFull()
        {
            return energyEmpty.IsFull();
        }
    }
}
