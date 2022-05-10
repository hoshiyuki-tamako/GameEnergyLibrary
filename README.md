# Energy Library

see unit test for more example

```cs
public class Example
{
    [Fact]
    public void A()
    {
        var energyOption = new EnergyOption
        {
            Interval = TimeSpan.FromMinutes(10),
        }.SetMinMax(0, 200); // set minimum and maximum amount

        var energy = new Energy(energyOption, 0, DateTime.Now);

        // Add energy directly
        energy.Add(10);
        Assert.Equal(10, energy.Amount);

        // Set energy directly, will clamp value from option
        energy.Amount = 999999;
        Assert.Equal(energyOption.MaxAmount, energy.Amount); // Amount = 200

        // Check will energy underflow
        Assert.True(energy.CanUse(20));

        // use energy
        energy.Use(20);
        Assert.Equal(180, energy.Amount);

        Assert.False(energy.CanUse(190));
        // will throw exception if not enough energy
        Assert.Throws<OutOfEnergyException>(() => energy.Use(190));

        // Check will energy overflow
        Assert.False(energy.CanAdd(100));
        Assert.True(energy.CanAdd(1));

        // TimeSpan for how long will the next energy able to receive
        var time1 = energy.TimeUntilNext();

        // TimeSpan for how long will the next energy able to receive
        var time2 = energy.TimeUntilFull();
    }

    [Fact]
    public void B()
    {
        var energyOption = new EnergyOption
        {
            Interval = TimeSpan.FromMinutes(10),
        }.SetMinMax(0, 200);
        var lastReceived = DateTime.Now - TimeSpan.FromHours(1);
        var energy = new Energy(energyOption, 100, lastReceived);

        // 60 minutes / 10 minutes interval = 6 energy
        Assert.Equal(6, energy.EstamiteReceive());
        Assert.Equal(100, energy.Amount);

        energy.Receive();
        Assert.Equal(106, energy.Amount);
    }
}
```