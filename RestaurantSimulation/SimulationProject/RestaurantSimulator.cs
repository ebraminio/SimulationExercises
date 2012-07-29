using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimulationProject
{
    public class RestaurantSimulator : Simulator<Customer>
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

        public override IEnumerator<Customer> GetEnumerator()
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
    }

    public static class CustomersHelper
    {
        public static double WaitingTimeAverage(this ICollection<Customer> customers)
        {
            return customers.Average(x => (double)x.WaitingTime);
        }

        public static double WaitedCustomersRatio(this ICollection<Customer> customers)
        {
            return (double)customers.Count(x => x.WaitingTime != 0) /
                (double)customers.Count();
        }

        public static double NoCustomerRatio(this ICollection<Customer> customers)
        {
            return (double)customers.Sum(x => x.NoCustomerTime) /
                (double)customers.Last().ServiceEnd;
        }

        public static double ServiceAverage(this ICollection<Customer> customers)
        {
            return (double)customers.Average(x => (double)x.ServiceDuration);
        }

        public static double EnteringDiffAverage(this ICollection<Customer> customers)
        {
            return (double)customers.Sum(x => (double)x.PreviousArrivalDiff) / 
                (double)(customers.Count() - 1);
        }

        public static double WaitingAverage(this ICollection<Customer> customers)
        {
            return customers
                .Where(x => x.WaitingTime != 0)
                .Average(x => (double)x.WaitingTime);
        }

        public static double CustomerInSystemAverage(this ICollection<Customer> customers)
        {
            return customers
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
                int x = (int)property.GetValue(this, null);
                int y = (int)property.GetValue(obj, null);

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
}
