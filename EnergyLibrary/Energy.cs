using System;

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
    }

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
                    ? EstamiteReceive() + Amount
                    : Math.Clamp(EstamiteReceive() + Amount, Option.MinAmount, Option.MaxAmount);
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
        public Energy(EnergyOption option = default, int amount = 0, DateTime lastReceived = default)
        {
            Option = option ?? new();
            Amount = amount;
            LastReceived = lastReceived;
        }

        /// <summary>
        /// Estamite adding raw value to amount
        /// </summary>
        /// <param name="amount"></param>
        /// <returns>current amount without clamp</returns>
        public int EstamiteAdd(int amount)
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
        /// Estamite using energy
        /// </summary>
        /// <param name="amount"></param>
        /// <returns>current amount without clamp</returns>
        public int EstamiteUse(int amount)
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
            var afterAmount = EstamiteReceive(now) + Amount - amount;
            if (afterAmount < Option.MinAmount)
            {
                throw new OutOfEnergyException("Not enough energy to use");
            }

            Receive(now);
            return Amount -= amount;
        }

        /// <summary>
        /// Estamite receiving energy from LastReceiveTime relative to DateTime.Now
        /// </summary>
        /// <returns>receive amount</returns>
        public int EstamiteReceive()
        {
            return EstamiteReceive(DateTime.Now);
        }

        /// <summary>
        /// Estamite receiving energy from LastReceiveTime
        /// </summary>
        /// <param name="now"></param>
        /// <returns>receive amount</returns>
        public int EstamiteReceive(DateTime now)
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

            var diff = EstamiteReceive(now);
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

            var penddingReceive = EstamiteReceive(now);
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

            var penddingReceive = EstamiteReceive(now);
            var needed = penddingReceive + Option.MaxAmount - Amount;
            var totalInterval = Option.Interval * (needed - 1);
            return totalInterval + TimeUntilNext(now);
        }

        /// <summary>
        /// Including EstamiteReceive amount and add x amount overflow (Option.MaxAmount)
        /// </summary>
        /// <param name="addAmount"></param>
        /// <returns></returns>
        public bool CanAdd(int addAmount = 0)
        {
            return (Total + addAmount) <= Option.MaxAmount;
        }

        /// <summary>
        /// Including EstamiteReceive amount and add subtract x amount will underflow (Option.MinAmount)
        /// </summary>
        /// <param name="useAmount"></param>
        /// <returns></returns>
        public bool CanUse(int useAmount = 0)
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
            return (EstamiteReceive(now) + Amount) >= Option.MaxAmount;
        }
    }
}
