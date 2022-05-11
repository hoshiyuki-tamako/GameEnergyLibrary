using EnergyLibrary.Schema;
using ProtoBuf;
using System;
using System.IO;

namespace EnergyLibrary
{

    /// <summary>
    /// Base exception class for this library
    /// </summary>
    [Serializable]
    public class EnergyException : Exception
    {
        public EnergyException(string message) : base(message) { }
    }

    /// <summary>
    /// Out of energy when doing operation
    /// </summary>
    [Serializable]
    public class OutOfEnergyException : EnergyException
    {
        public OutOfEnergyException(string message) : base(message) { }
    }

    /// <summary>
    /// Options for the energy system
    /// </summary>
    public class EnergyOption
    {
        protected TimeSpan interval = TimeSpan.FromMinutes(10);
        protected int minAmount = 0;
        protected int maxAmount = 200;

        /// <summary>
        /// Allow amount to be overflow
        /// </summary>
        public bool AllowOverflow { get; set; }

        /// <summary>
        /// Time between energy recover
        /// </summary>
        public TimeSpan Interval
        {
            get
            {
                return interval;
            }

            set
            {
                if (value.TotalMilliseconds <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Energy interval cannot smaller or equal to zero");
                }

                interval = value;
            }
        }

        /// <summary>
        /// Minimum amount of enegry it can have
        /// </summary>
        public int MinAmount
        {
            get
            {
                return minAmount;
            }
            set
            {
                if (value > MaxAmount)
                {
                    throw new ArgumentException("minAmount cannot greater than maxAmount", nameof(value));
                }

                minAmount = value;
            }
        }
        
        /// <summary>
        /// Maximum amount of energy it can have
        /// </summary>
        public int MaxAmount
        {
            get
            {
                return maxAmount;
            }
            set
            {
                if (value < MinAmount)
                {
                    throw new ArgumentException("MaxAmount cannot smaller than MinAmount", nameof(value));
                }

                maxAmount = value;
            }
        }

        /// <summary>
        /// Set minimum amount and maximum amount
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public EnergyOption SetMinMax(int min = 0, int max = 200)
        {
            if (min > max)
            {
                throw new ArgumentException("MinAmount cannot greater than MaxAmount", nameof(min));
            }

            minAmount = min;
            maxAmount = max;

            return this;
        }

        /// <summary>
        /// Export to a serializable class
        /// </summary>
        /// <returns></returns>
        public EnergyOptionSchema Export()
        {
            return new EnergyOptionSchema
            {
                AllowOverFlow = AllowOverflow,
                IntervalTick = Interval.Ticks,
                MinAmount = MinAmount,
                MaxAmount = MaxAmount,
            };
        }

        /// <summary>
        /// Import from schema
        /// </summary>
        /// <param name="energyOptionSchema"></param>
        /// <returns></returns>
        public static EnergyOption Import(EnergyOptionSchema energyOptionSchema)
        {
            return new EnergyOption
            {
                AllowOverflow = energyOptionSchema.AllowOverFlow,
                Interval = TimeSpan.FromTicks(energyOptionSchema.IntervalTick),
            }.SetMinMax(energyOptionSchema.MinAmount, energyOptionSchema.MaxAmount);
        }
    }

    /// <summary>
    /// Energy class
    /// </summary>
    public class Energy
    {
        protected int amount = 0;

        /// <summary>
        /// Options for the energy system
        /// </summary>
        public EnergyOption Option { get; protected set; }

        /// <summary>
        /// Raw energy amount
        /// </summary>
        public int Amount
        {
            get
            {
                return amount;
            }
            set
            {
                amount = Option.AllowOverflow
                    ? value 
                    : Math.Clamp(value, Option.MinAmount, Option.MaxAmount);
            }
        }

        /// <summary>
        /// Total energy
        /// </summary>
        public int Total {
            get
            {
                return Option.AllowOverflow 
                    ? EstimateReceive() + Amount
                    : Math.Clamp(EstimateReceive() + Amount, Option.MinAmount, Option.MaxAmount);
            }

            set
            {
                Receive();
                Amount = value;
            }
        }

        /// <summary>
        /// Last received energy time
        /// </summary>
        public DateTime LastReceived { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="option"></param>
        /// <param name="amount"></param>
        /// <param name="lastReceived"></param>
        public Energy(EnergyOption? option = default, int amount = 0, DateTime lastReceived = default)
        {
            Option = option ?? new();
            Amount = amount;
            LastReceived = lastReceived;
        }

        /// <summary>
        /// Export to a serializable class
        /// </summary>
        /// <returns></returns>
        public EnergySchema Export()
        {
            return new EnergySchema
            {
                Option = Option.Export(),
                Amount = Amount,
                LastReceived = LastReceived.Ticks,
            };
        }

        /// <summary>
        /// Import from a serializable class
        /// </summary>
        /// <returns></returns>
        public static Energy Import(EnergySchema energySchema)
        {
            return new Energy(EnergyOption.Import(energySchema.Option), energySchema.Amount, new DateTime(energySchema.LastReceived));
        }

        /// <summary>
        /// use energy.Export() if possible, this function will required to copy the bytes and will cost performance hit
        /// Export to protobuf
        /// </summary>
        /// <returns></returns>
        public byte[] ToProtobuf()
        {
            using var memoryStream = new MemoryStream();
            Serializer.Serialize(memoryStream, Export());
            return memoryStream.ToArray();
        }

        /// <summary>
        /// Import from protobuf
        /// </summary>
        /// <param name="buf"></param>
        /// <returns></returns>
        public static Energy FromProtobuf(byte[] bytes)
        {
            return Import(Serializer.Deserialize<EnergySchema>(bytes.AsSpan()));
        }

        /// <summary>
        /// Estimate adding raw value to amount
        /// </summary>
        /// <param name="amount"></param>
        /// <returns>current amount without clamp</returns>
        public int EstimateAdd(int amount)
        {
            return Total + amount;
        }

        /// <summary>
        /// Add raw value to amount, will set the clamp value from option
        /// </summary>
        /// <param name="amount"></param>
        /// <returns>current amount without clamp</returns>
        public int Add(int amount)
        {
            var oldAmount = Amount;
            var receiveAmount = Receive();
            var afterAmount = oldAmount + receiveAmount + amount;
            Amount = afterAmount;
            return afterAmount;
        }

        /// <summary>
        /// Estimate using energy
        /// </summary>
        /// <param name="amount"></param>
        /// <returns>current amount without clamp</returns>
        public int EstimateUse(int amount)
        {
            return Total - amount;
        }

        /// <summary>
        /// Use the amount of energy
        /// </summary>
        /// <param name="amount"></param>
        /// <returns>current amount without clamp</returns>
        public int Use(int amount)
        {
            var now = DateTime.Now;
            var afterAmount = EstimateReceive(now) + Amount - amount;
            if (afterAmount < Option.MinAmount)
            {
                throw new OutOfEnergyException("Not enough energy to use");
            }

            Receive(now);
            return Amount -= amount;
        }

        /// <summary>
        /// Estimate receiving energy from LastReceiveTime relative to DateTime.Now
        /// </summary>
        /// <returns>receive amount</returns>
        public int EstimateReceive()
        {
            return EstimateReceive(DateTime.Now);
        }

        /// <summary>
        /// Estimate receiving energy from LastReceiveTime
        /// </summary>
        /// <param name="now"></param>
        /// <returns>receive amount</returns>
        public int EstimateReceive(DateTime now)
        {
            return (int)Math.Truncate((now - LastReceived) / Option.Interval);
        }

        /// <summary>
        /// Receive the energy calculate from LastReceiveTime
        /// </summary>
        /// <returns>receive amount</returns>
        public int Receive()
        {
            return Receive(DateTime.Now);
        }

        protected int Receive(DateTime now)
        {
            // if amount already overflow
            if (Amount >= Option.MaxAmount)
            {
                LastReceived = now;
                return 0;
            }

            var diff = EstimateReceive(now);
            var afterAmount = Amount + diff;
            if (afterAmount >= Option.MaxAmount)
            {
                LastReceived = now;
            } else
            {
                LastReceived += Option.Interval * diff;
            }

            Amount = afterAmount;
            return diff;
        }

        /// <summary>
        /// Fill energy to MaxAmount
        /// </summary>
        public void Fill()
        {
            Amount = Option.MaxAmount;
            LastReceived = DateTime.Now;
        }

        /// <summary>
        /// Rest energy to MinAmount
        /// </summary>
        public void UnFill()
        {
            Amount = Option.MinAmount;
            LastReceived = DateTime.Now;
        }

        /// <summary>
        /// Time until next 1 energy receive
        /// </summary>
        /// <returns></returns>
        public TimeSpan TimeUntilNext()
        {
            return TimeUntilNext(DateTime.Now);
        }

        protected TimeSpan TimeUntilNext(DateTime now)
        {
            if (IsFull(now))
            {
                return new TimeSpan();
            }

            var penddingReceive = EstimateReceive(now);
            var nextReceiveTime = LastReceived + (Option.Interval * (penddingReceive + 1));
            return (now - nextReceiveTime).Duration();
        }

        /// <summary>
        /// Time until energy become full
        /// </summary>
        /// <returns></returns>
        public TimeSpan TimeUntilFull()
        {
            var now = DateTime.Now;

            if (IsFull(now))
            {
                return new TimeSpan();
            }

            var penddingReceive = EstimateReceive(now);
            var needed = penddingReceive + Option.MaxAmount - Amount;
            var totalInterval = Option.Interval * (needed - 1);
            return totalInterval + TimeUntilNext(now);
        }

        /// <summary>
        /// Including EstimateReceive amount and add x amount overflow (Option.MaxAmount)
        /// </summary>
        /// <param name="addAmount"></param>
        /// <returns></returns>
        public bool CanAdd(int addAmount)
        {
            return (Total + addAmount) <= Option.MaxAmount;
        }

        /// <summary>
        /// Including EstimateReceive amount and add subtract x amount will underflow (Option.MinAmount)
        /// </summary>
        /// <param name="useAmount"></param>
        /// <returns></returns>
        public bool CanUse(int useAmount)
        {
            return (Total - useAmount) >= Option.MinAmount;
        }

        /// <summary>
        /// Is energy empty
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return Total <= Option.MinAmount;
        }

        /// <summary>
        /// Is energy full
        /// </summary>
        /// <returns></returns>
        public bool IsFull()
        {
            return IsFull(DateTime.Now);
        }

        protected bool IsFull(DateTime now)
        {
            return (EstimateReceive(now) + Amount) >= Option.MaxAmount;
        }
    }
}
