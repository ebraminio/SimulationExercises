using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSimulation
{
    public class RestaurantSimulator : IEnumerable<Customer>
    {
        private ItemPicker<int> _enteringDifference;
        private ItemPicker<int> _serviceTime;
        public RestaurantSimulator(ItemPicker<int> enteringDifference,
            ItemPicker<int> serviceTime)
        {
            _enteringDifference = enteringDifference;
            _serviceTime = serviceTime;
        }

        public void AddEnteringDifferencePossibility(int enteringDiff, double possibility)
        {
            _enteringDifference.AddEntityPossibilty(enteringDiff, possibility);
        }

        public void AddServiceTimePossibility(int serviceTime, double possibility)
        {
            _serviceTime.AddEntityPossibilty(serviceTime, possibility);
        }

        public IEnumerator<Customer> GetEnumerator()
        {
            var enteringDifferenceEnumerator = _enteringDifference.GetEnumerator();
            var serviceTimeEnumerator = _serviceTime.GetEnumerator();
            bool firstInQueue = true;
            int customerArrivalTime = 0;
            int previousServiceTime = 0;
            int reservedQueue = 0;
            int customerId = 0;
            while (enteringDifferenceEnumerator.MoveNext()
                && serviceTimeEnumerator.MoveNext())
            {
                var currentEnter = enteringDifferenceEnumerator.Current;
                var currentServiceTime = serviceTimeEnumerator.Current;
                if (firstInQueue == true)
                {
                    firstInQueue = false;
                    currentEnter = 0;
                }

                int noCustomerTime = 0;
                reservedQueue = reservedQueue + previousServiceTime - currentEnter;               
                if (reservedQueue < 0)
                {
                    noCustomerTime = -reservedQueue;
                    reservedQueue = 0;
                }

                customerArrivalTime += currentEnter;
                customerId++;
                yield return new Customer
                {
                    Id = customerId,
                    ArrivalTime = customerArrivalTime,
                    PreviousArrivalDiff = currentEnter,
                    ServiceDuration = currentServiceTime,
                    ServiceStart = customerArrivalTime + reservedQueue,
                    WaitingTime = reservedQueue,
                    ServiceEnd = customerArrivalTime + reservedQueue + currentServiceTime,
                    CustomerInSystemTime = reservedQueue + currentServiceTime,
                    NoCustomerTime = noCustomerTime,
                };

                previousServiceTime = currentServiceTime;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public static class CustomerEnumerableHelper
    {
        public static double WaitingTimeAverage(this ICollection<Customer> enumerable)
        {
            return enumerable.Average(x => (double)x.WaitingTime);
        }

        public static double WaitedCustomersRatio(this ICollection<Customer> enumerable)
        {
            return (double)enumerable.Count(x => x.WaitingTime != 0) /
                (double)enumerable.Count();
        }

        public static double NoCustomerRatio(this ICollection<Customer> enumerable)
        {
            return (double)enumerable.Sum(x => x.NoCustomerTime) /
                (double)enumerable.Last().ServiceEnd;
        }

        public static double ServiceAverage(this ICollection<Customer> enumerable)
        {
            return (double)enumerable.Average(x => (double)x.ServiceDuration);
        }

        public static double EnteringDiffAverage(this ICollection<Customer> enumerable)
        {
            return (double)enumerable.Sum(x => (double)x.PreviousArrivalDiff) / 
                (double)(enumerable.Count() - 1);
        }

        public static double WaitingAverage(this ICollection<Customer> enumerable)
        {
            return enumerable
                .Where(x => x.WaitingTime != 0)
                .Average(x => (double)x.WaitingTime);
        }

        public static double CustomerInSystemAverage(this ICollection<Customer> enumerable)
        {
            return enumerable
                .Average(x => x.CustomerInSystemTime);
        }
    }

    public class Customer
    {
        public int Id { get; set; }
        public int PreviousArrivalDiff { get; set; }
        public int ArrivalTime { get; set; }
        public int ServiceDuration { get; set; }
        public int ServiceStart { get; set; }
        public int WaitingTime { get; set; }
        public int ServiceEnd { get; set; }
        public int CustomerInSystemTime { get; set; }
        public int NoCustomerTime { get; set; } // not related to customer directly

        public Customer() { }
        public Customer(int id, int previousArrivalDiff, int arrivalTime,
            int serviceDuration, int serviceStart, int waitingTime, int serviceEnd,
            int customerInSystemTime, int noCustomerTime)
        {
            Id = id;
            PreviousArrivalDiff = previousArrivalDiff;
            ArrivalTime = arrivalTime;
            ServiceDuration = serviceDuration;
            ServiceStart = serviceStart;
            WaitingTime = waitingTime;
            ServiceEnd = serviceEnd;
            CustomerInSystemTime = customerInSystemTime;
            NoCustomerTime = noCustomerTime;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;

            Customer o = obj as Customer;
            if (obj == null)
                return false;

            foreach (var property in typeof(Customer).GetProperties())
            {
                int x = (int)property.GetValue(this);
                int y = (int)property.GetValue(obj);

                if (x != y)
                    return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            int hash = 37;
            hash = hash * 23 + base.GetHashCode();
            hash = hash * 23 + Id.GetHashCode();
            return hash;
        }
    }

    public class ItemPicker<T> : IEnumerable<T>
    {
        private IEnumerator<double> _mantissaEnumerator;
        public IDictionary<T, double> _possiblities { private set; get; }
        public ItemPicker(IEnumerable<double> mantissaYielder)
        {
            _possiblities = new Dictionary<T, double>();
            _mantissaEnumerator = mantissaYielder.GetEnumerator();
        }

        public void AddEntityPossibilty(T entity, double possibilty)
        {
            _possiblities.Add(entity, possibilty);
        }

        private T Yield()
        {
            var rand = _mantissaEnumerator.Current * _possiblities.Values.Sum();

            foreach (var kv in _possiblities)
            {
                if (rand == 0)
                {
                    return kv.Key;
                }
                rand -= kv.Value;
                if (rand <= 0)
                {
                    return kv.Key;
                }
            }
            return default(T);
        }

        public IEnumerator<T> GetEnumerator()
        {
            while (_mantissaEnumerator.MoveNext())
            {
                yield return Yield();
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public class RandomMantissa : IEnumerable<double>
    {
        private Random _random = new Random();
        public IEnumerator<double> GetEnumerator()
        {
            while (true)
            {
                yield return _random.NextDouble();
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class FlipFlopMantissa : IEnumerable<double>
    {
        public IEnumerator<double> GetEnumerator()
        {
            bool _flag = false;
            while (true)
            {
                _flag = !_flag;
                yield return _flag ? 0 : 0.99;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
