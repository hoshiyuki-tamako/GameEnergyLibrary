# Energy Library

A library handle time based game energy

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

        // Add energy directly, will clamp value from option
        energy.Add(10);
        Assert.Equal(10, energy.Total);

        // Set energy directly, will clamp value from option
        energy.Total = 999999;
        Assert.Equal(energyOption.MaxAmount, energy.Total); // Amount = 200

        // Check will energy underflow
        Assert.True(energy.CanUse(20));

        // use energy
        energy.Use(20);
        Assert.Equal(180, energy.Total);

        Assert.False(energy.CanUse(190));
        // will throw exception if not enough energy
        Assert.Throws<OutOfEnergyException>(() => energy.Use(190));

        // Check will energy overflow
        Assert.False(energy.CanAdd(100));
        Assert.True(energy.CanAdd(1));

        // TimeSpan for how long will the next energy able to receive
        var time1 = energy.TimeUntilNext();

        // TimeSpan for how long until energy is full
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
        Assert.Equal(6, energy.EstimateReceive());
        Assert.Equal(106, energy.Total);

        // it is suggested to call Receive() before changing interval
        // trigger receive and dump the time to amount
        energy.Receive();
        energy.Option.Interval = TimeSpan.FromMinutes(1);
    }

    [Fact]
    public void C()
    {
        var energyOption = new EnergyOption
        {
            AllowOverflow = true,
            Interval = TimeSpan.FromMinutes(10),
        }.SetMinMax(0, 200);
        var lastReceived = DateTime.Now - TimeSpan.FromHours(1);
        var energy = new Energy(energyOption, energyOption.MaxAmount, lastReceived);

        // Receive will not add extra energy due to its already reach limit or overflowed
        // energy.Receive();

        // Allow overflow adding
        energy.Add(200);
        Assert.Equal(400, energy.Total);
    }

    [Fact]
    public void D()
    {
        var energyOption = new EnergyOption
        {
            AllowOverflow = true,
            Interval = TimeSpan.FromMinutes(10),
        }.SetMinMax(0, 200);
        var lastReceived = DateTime.Now - TimeSpan.FromHours(1);
        var energy = new Energy(energyOption, energyOption.MaxAmount, lastReceived);

        // save the energy state to somewhere else
        byte[] a = energy.ToProtobuf();
        var b = Energy.FromProtobuf(a);

        // using json
        var jsonA = JsonSerializer.Serialize(energy.Export());
#pragma warning disable CS8604 // Possible null reference argument.
        var jsonB = Energy.Import(JsonSerializer.Deserialize<EnergySchema>(jsonA));
#pragma warning restore CS8604 // Possible null reference argument.
    }
}
```
