using EnergyLibrary;
using FluentAssertions;
using System;
using Xunit;

namespace EnergyLibraryTest
{
    public class EnergyTest
    {
        [Fact]
        public void ConsturctorDefault()
        {
            var energy = new Energy();
            energy.Option.Should().BeEquivalentTo(new EnergyOption());
            Assert.Equal(0, energy.Amount);
            Assert.Equal(default, energy.LastReceived);
        }

        [Fact]
        public void Consturctor()
        {
            var option = new EnergyOption
            {
                Interval = TimeSpan.FromSeconds(1),
                MinAmount = -10,
                MaxAmount = 10
            };
            var amount = 5;
            var time = DateTime.Now;

            var energy = new Energy(option, amount, time);
            energy.Option.Should().BeEquivalentTo(option);
            Assert.Equal(amount, energy.Amount);
            Assert.Equal(time, energy.LastReceived);
        }

        [Fact]
        public void SetAmount()
        {
            var energyOption = new EnergyOption();
            var energy = new Energy(energyOption);

            energy.Amount = energyOption.MinAmount - 1;
            Assert.Equal(energyOption.MinAmount, energy.Amount);

            energy.Amount = energyOption.MaxAmount + 1;
            Assert.Equal(energyOption.MaxAmount, energy.Amount);
        }

        [Fact]
        public void EstamiteAdd()
        {
            var energy = new Energy(default, default, DateTime.Now - TimeSpan.FromHours(1));
            Assert.Equal(16, energy.EstamiteAdd(10));
            Assert.Equal(0, energy.Amount);
        }

        [Fact]
        public void EstamiteAddFull()
        {
            var maxAmount = new EnergyOption().MaxAmount;
            var energy = new Energy(default, maxAmount, DateTime.Now - TimeSpan.FromHours(1));
            Assert.Equal(maxAmount + 10 + 6, energy.EstamiteAdd(10));
            Assert.Equal(maxAmount, energy.Amount);
        }

        [Fact]
        public void Add()
        {
            var energy = new Energy(default, default, DateTime.Now - TimeSpan.FromHours(1));
            Assert.Equal(16, energy.Add(10));
            Assert.Equal(16, energy.Amount);
        }

        [Fact]
        public void AddFull()
        {
            var maxAmount = new EnergyOption().MaxAmount;
            var energy = new Energy(default, maxAmount, DateTime.Now - TimeSpan.FromHours(1));
            Assert.Equal(maxAmount + 10 + 6, energy.Add(10));
            Assert.Equal(maxAmount, energy.Amount);
        }

        [Fact]
        public void EstamiteUse()
        {
            var maxAmount = new EnergyOption().MaxAmount;
            var energy = new Energy(default, maxAmount, DateTime.Now);
            Assert.Equal(maxAmount - 10, energy.EstamiteUse(10));
            Assert.Equal(maxAmount, energy.Amount);
        }

        [Fact]
        public void EstamiteUseZero()
        {
            var energy = new Energy(default, 0, DateTime.Now);
            Assert.Equal(-10, energy.EstamiteUse(10));
            Assert.Equal(0, energy.Amount);
        }

        [Fact]
        public void Use()
        {
            var maxAmount = new EnergyOption().MaxAmount;
            var energy = new Energy(default, maxAmount, DateTime.Now);
            Assert.Equal(maxAmount - 10, energy.Use(10));
            Assert.Equal(maxAmount - 10, energy.Amount);
        }

        [Fact]
        public void UseJustEnough()
        {
            var energy = new Energy(default, 16, DateTime.Now);
            Assert.Equal(0, energy.Use(16));
            Assert.Equal(0, energy.Amount);
        }

        [Fact]
        public void UseNotEnough()
        {
            var energy = new Energy(default, 0, DateTime.Now);
            Assert.Throws<OutOfEnergyException>(() => energy.Use(1));
        }

        [Fact]
        public void EstamiteReceive()
        {
            var energy = new Energy(default, 0, DateTime.Now - TimeSpan.FromHours(1));
            Assert.Equal(6, energy.EstamiteReceive());
        }

        [Fact]
        public void EstamiteReceiveWithTime()
        {
            var energy = new Energy(default, 0, DateTime.Now);
            Assert.Equal(6, energy.EstamiteReceive(DateTime.Now + TimeSpan.FromHours(1)));
        }

        [Fact]
        public void Receive()
        {
            var energyOption = new EnergyOption();
            var energy = new Energy(energyOption, 0, DateTime.Now - TimeSpan.FromHours(1));
            Assert.Equal(6, energy.Receive());
            Assert.Equal(6, energy.Amount);
            Assert.True((DateTime.Now - energy.LastReceived) < energyOption.Interval);
        }

        [Fact]
        public void ReceiveMax()
        {
            var energyOption = new EnergyOption();
            var energy = new Energy(energyOption);
            energy.Receive();
            Assert.Equal(energyOption.MaxAmount, energy.Amount);
            Assert.True((DateTime.Now - energy.LastReceived) < energyOption.Interval);
        }

        [Fact]
        public void TimeUntilNext()
        {
            var energy = new Energy(default, 0, DateTime.Now);
            var t = energy.TimeUntilNext();
            Assert.Equal(9, t.Minutes);
            Assert.Equal(59, t.Seconds);
        }

        [Fact]
        public void TimeUntilNextWithTime()
        {
            var energy = new Energy(default, 0, DateTime.Now - TimeSpan.FromHours(1));
            var t = energy.TimeUntilNext();
            Assert.Equal(9, t.Minutes);
            Assert.Equal(59, t.Seconds);
        }

        [Fact]
        public void TimeUntilNextFullEnergy()
        {
            var energyOption = new EnergyOption();
            var energy = new Energy(energyOption, energyOption.MaxAmount - 6, DateTime.Now - TimeSpan.FromHours(1));
            Assert.Equal(0, energy.TimeUntilNext().TotalSeconds);
        }

        [Fact]
        public void TimeUntilFull()
        {
            var energyOption = new EnergyOption();
            var energy = new Energy(energyOption, energyOption.MaxAmount - 1, DateTime.Now);
            var t = energy.TimeUntilFull();
            Assert.Equal(9, t.Minutes);
            Assert.Equal(59, t.Seconds);
        }

        [Fact]
        public void TimeUntilFullWithTime()
        {
            var energyOption = new EnergyOption();
            var energy = new Energy(energyOption, energyOption.MaxAmount - 7, DateTime.Now - TimeSpan.FromHours(1));
            var t = energy.TimeUntilFull();
            Assert.Equal(9, t.Minutes);
            Assert.Equal(59, t.Seconds);
        }

        [Fact]
        public void TimeUntilFullEnergy()
        {
            var energyOption = new EnergyOption();
            var energy = new Energy(energyOption, energyOption.MaxAmount - 6, DateTime.Now - TimeSpan.FromHours(1));
            Assert.Equal(0, energy.TimeUntilFull().TotalSeconds);
        }

        [Fact]
        public void CanAdd()
        {
            var maxAmount = new EnergyOption().MaxAmount;
            var energy = new Energy(default, 0, DateTime.Now);
            Assert.True(energy.CanAdd());
            Assert.True(energy.CanAdd(1));
            Assert.True(energy.CanAdd(maxAmount));
            Assert.False(energy.CanAdd(maxAmount + 1));
        }

        [Fact]
        public void CanAddMax()
        {
            var energyOption = new EnergyOption();
            var energy = new Energy(energyOption, energyOption.MaxAmount, DateTime.Now);
            Assert.False(energy.CanAdd(1));
        }

        [Fact]
        public void CanAddMaxTime()
        {
            var energy = new Energy();
            Assert.False(energy.CanAdd());
        }

        [Fact]
        public void CanUse()
        {
            var energyOption = new EnergyOption();
            var energy = new Energy(energyOption, 10, DateTime.Now);
            Assert.True(energy.CanUse());
            Assert.True(energy.CanUse(1));
            Assert.True(energy.CanUse(10));
            Assert.False(energy.CanUse(11));
        }

        [Fact]
        public void CanUseMaxAmount()
        {
            var energyOption = new EnergyOption();
            var energy = new Energy(energyOption, energyOption.MaxAmount, DateTime.Now);
            Assert.True(energy.CanUse(energyOption.MaxAmount));
        }

        [Fact]
        public void CanUseMaxTime()
        {
            var energyOption = new EnergyOption();
            var energy = new Energy(energyOption);
            Assert.True(energy.CanUse(energyOption.MaxAmount));
        }

        [Fact]
        public void CanUseMin()
        {
            var energy = new Energy(default, 0, DateTime.Now);
            Assert.False(energy.CanUse(1));
        }

        [Fact]
        public void IsEmpty()
        {
            var energy = new Energy(default, 0, DateTime.Now);
            Assert.True(energy.IsEmpty());

            energy.Amount = 1;
            Assert.False(energy.IsEmpty());
        }

        [Fact]
        public void IsEmptyFullTime()
        {
            var energy = new Energy();
            Assert.False(energy.IsEmpty());
        }

        [Fact]
        public void IsFull()
        {
            var energyOption = new EnergyOption();
            var energy = new Energy(energyOption, 0, DateTime.Now);
            Assert.False(energy.IsFull());

            energy.Amount = energyOption.MaxAmount;
            Assert.True(energy.IsFull());
        }

        [Fact]
        public void IsFullFullTime()
        {
            var energy = new Energy();
            Assert.True(energy.IsFull());
        }
    }
}
