using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SimulationProject
{
    public class RestaurantSimulator : IEnumerable<RestaurantCustomer>
    {
        private ItemPicker<int> _enteringDifference;
        private ItemPicker<int> _serviceTime;
        public RestaurantSimulator(IEnumerable<double> enteringDifferenceRandomNumbers,
            IEnumerable<double> serviceTimeRandomNumbers)
        {
            _enteringDifference = new ItemPicker<int>(enteringDifferenceRandomNumbers.GetEnumerator());
            _serviceTime = new ItemPicker<int>(serviceTimeRandomNumbers.GetEnumerator());
        }

        public RestaurantSimulator AddEnteringDifferencePossibility(int enteringDiff, double possibility)
        {
            _enteringDifference.AddEntityPossibilty(enteringDiff, possibility);
            return this;
        }

        public RestaurantSimulator AddServiceTimePossibility(int serviceTime, double possibility)
        {
            _serviceTime.AddEntityPossibilty(serviceTime, possibility);
            return this;
        }

        public IEnumerator<RestaurantCustomer> GetEnumerator()
        {
            var enteringDifferenceEnumerator = _enteringDifference.GetEnumerator();
            var serviceTimeEnumerator = _serviceTime.GetEnumerator();
            var firstInQueue = true;
            int customerArrivalTime = 0;
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
                reservedQueue -= currentEnter;
                if (reservedQueue < 0)
                {
                    noCustomerTime = -reservedQueue;
                    reservedQueue = 0;
                }

                customerArrivalTime += currentEnter;
                customerId++;
                yield return new RestaurantCustomer
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
                reservedQueue += currentServiceTime;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public struct RestaurantCustomer
    {
        [DisplayName("مشتری")]
        public int Id { get; set; }

        [DisplayName("مدت سپری شده از آخرین ورود (دقیقه)")]
        public int PreviousArrivalDiff { get; set; }

        [DisplayName("زمان ورود")]
        public int ArrivalTime { get; set; }

        [DisplayName("مدت خدمت‌دهی (دقیقه)")]
        public int ServiceDuration { get; set; }

        [DisplayName("زمان شروع خدمت")]
        public int ServiceStart { get; set; }

        [DisplayName("زمان ماندن مشتری در صف (دقیقه)")]
        public int WaitingTime { get; set; }

        [DisplayName("زمان پایان خدمت")]
        public int ServiceEnd { get; set; }

        [DisplayName("مدت ماندن مشتری در سیستم (دقیقه)")]
        public int CustomerInSystemTime { get; set; }

        [DisplayName("مدت بیکاری خدمت‌دهنده")]
        public int NoCustomerTime { get; set; }

        public RestaurantCustomer(int id, int previousArrivalDiff, int arrivalTime,
            int serviceDuration, int serviceStart, int waitingTime, int serviceEnd,
            int customerInSystemTime, int noCustomerTime) : this()
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
    }
    
    public static class RestaurantCustomersTools
    {
        [DisplayName("متوسط مدت انتظار هر مشتری")]
        public static double WaitingTimeAverage(this ICollection<RestaurantCustomer> customers)
        {
            return customers.AverageOrZero(x => (double)x.WaitingTime);
        }

        [DisplayName("احتمال مجبور شدن مشتری به انتظار")]
        public static double WaitedCustomersRatio(this ICollection<RestaurantCustomer> customers)
        {
            return (double)customers.Count(x => x.WaitingTime != 0) /
                (double)customers.Count();
        }

        [DisplayName("نسبت بیکاری خدمت‌دهنده")]
        public static double NoCustomerRatio(this ICollection<RestaurantCustomer> customers)
        {
            return (double)customers.Sum(x => x.NoCustomerTime) /
                (double)customers.LastOrDefault().ServiceEnd;
        }

        [DisplayName("متوسط مدت خدمت‌دهی")]
        public static double ServiceAverage(this ICollection<RestaurantCustomer> customers)
        {
            return (double)customers.AverageOrZero(x => (double)x.ServiceDuration);
        }

        [DisplayName("متوسط مدت بین هر دو ورود")]
        public static double EnteringDiffAverage(this ICollection<RestaurantCustomer> customers)
        {
            return (double)customers.Sum(x => (double)x.PreviousArrivalDiff) /
                (double)(customers.Count() - 1);
        }

        [DisplayName("متوسط مدت انتظار")]
        public static double WaitingAverage(this ICollection<RestaurantCustomer> customers)
        {
            return customers
                .Where(x => x.WaitingTime != 0)
                .AverageOrZero(x => (double)x.WaitingTime);
        }

        [DisplayName("متوسط مدت زمان حضور مشتری")]
        public static double CustomerInSystemAverage(this ICollection<RestaurantCustomer> customers)
        {
            return customers.AverageOrZero(x => x.CustomerInSystemTime);
        }
    }
}
