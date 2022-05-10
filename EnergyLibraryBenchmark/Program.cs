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
            Interval = TimeSpan.FromMilliseconds(1000),
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
        public void Add()
        {
            energyEmpty.Add(1);
        }

        [Benchmark]
        public void Use()
        {
            energyFull.Use(1);
        }

        [Benchmark]
        public void Receive()
        {
            energyEmpty.Receive();
        }

        [Benchmark]
        public void TimeUntilNext()
        {
            energyEmpty.TimeUntilNext();
        }

        [Benchmark]
        public void TimeUntilFull()
        {
            energyEmpty.TimeUntilFull();
        }

        [Benchmark]
        public void CanUse()
        {
            energyEmpty.CanUse(1);
        }

        [Benchmark]
        public void IsFull()
        {
            energyEmpty.IsFull();
        }
    }
}
