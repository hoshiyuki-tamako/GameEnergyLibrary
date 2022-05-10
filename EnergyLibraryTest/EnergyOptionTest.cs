using System;
using Xunit;

namespace EnergyLibrary
{
    public class EnergyOptionTest
    {
        [Fact]
        public void Interval()
        {
            var energyOption = new EnergyOption
            {
                Interval = TimeSpan.FromSeconds(1),
            };
            Assert.Equal(1, energyOption.Interval.TotalSeconds);
        }

        [Fact]
        public void IntervalBelowOrEqualToZero()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new EnergyOption
            {
                Interval = TimeSpan.FromSeconds(0),
            });

            Assert.Throws<ArgumentOutOfRangeException>(() => new EnergyOption
            {
                Interval = TimeSpan.FromSeconds(-1),
            });
        }

        [Fact]
        public void MinAmount()
        {
            var energyOption = new EnergyOption();

            energyOption.MinAmount = 1;
            Assert.Equal(1, energyOption.MinAmount);
            Assert.Throws<ArgumentException>(() => energyOption.MinAmount = energyOption.MaxAmount + 1);
        }

        [Fact]
        public void MaxAmount()
        {
            var energyOption = new EnergyOption();

            energyOption.MaxAmount = 201;
            Assert.Equal(201, energyOption.MaxAmount);
            Assert.Throws<ArgumentException>(() => energyOption.MaxAmount = energyOption.MinAmount - 1);
        }


        [Fact]
        public void SetMinMax()
        {
            var energyOption = new EnergyOption().SetMinMax(20, 40);
            Assert.Equal(20, energyOption.MinAmount);
            Assert.Equal(40, energyOption.MaxAmount);
        }

        [Fact]
        public void SetMinMaxThrow()
        {
            Assert.Throws<ArgumentException>(() => new EnergyOption().SetMinMax(10, 0));
        }
    }
}
