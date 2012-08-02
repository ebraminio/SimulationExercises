using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SimulationProject
{
    public class RestaurantSimulator : Simulator<RestaurantCustomer>
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

        public override IEnumerator<RestaurantCustomer> GetEnumerator()
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
    }

    public class RestaurantCustomer : Entity
    {
        [DisplayNameAttribute("مشتری")]
        public int Id { get; set; }

        [DisplayNameAttribute("مدت سپری شده از آخرین ورود (دقیقه)")]
        public int PreviousArrivalDiff { get; set; }

        [DisplayNameAttribute("زمان ورود")]
        public int ArrivalTime { get; set; }

        [DisplayNameAttribute("مدت خدمت‌دهی (دقیقه)")]
        public int ServiceDuration { get; set; }

        [DisplayNameAttribute("زمان شروع خدمت")]
        public int ServiceStart { get; set; }

        [DisplayNameAttribute("زمان ماندن مشتری در صف (دقیقه)")]
        public int WaitingTime { get; set; }

        [DisplayNameAttribute("زمان پایان خدمت")]
        public int ServiceEnd { get; set; }

        [DisplayNameAttribute("مدت ماندن مشتری در سیستم (دقیقه)")]
        public int CustomerInSystemTime { get; set; }

        [DisplayNameAttribute("مدت بیکاری خدمت‌دهنده")]
        public int NoCustomerTime { get; set; }

        public RestaurantCustomer() { }
        public RestaurantCustomer(int id, int previousArrivalDiff, int arrivalTime,
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
    }
    
    public static class RestaurantCustomersTools
    {
        [DisplayNameAttribute("متوسط مدت انتظار هر مشتری")]
        public static double WaitingTimeAverage(this ICollection<RestaurantCustomer> customers)
        {
            return customers.Average(x => (double)x.WaitingTime);
        }

        [DisplayNameAttribute("احتمال مجبور شدن مشتری به انتظار")]
        public static double WaitedCustomersRatio(this ICollection<RestaurantCustomer> customers)
        {
            return (double)customers.Count(x => x.WaitingTime != 0) /
                (double)customers.Count();
        }

        [DisplayNameAttribute("نسبت بیکاری خدمت‌دهنده")]
        public static double NoCustomerRatio(this ICollection<RestaurantCustomer> customers)
        {
            return (double)customers.Sum(x => x.NoCustomerTime) /
                (double)customers.Last().ServiceEnd;
        }

        [DisplayNameAttribute("متوسط مدت خدمت‌دهی")]
        public static double ServiceAverage(this ICollection<RestaurantCustomer> customers)
        {
            return (double)customers.Average(x => (double)x.ServiceDuration);
        }

        [DisplayNameAttribute("متوسط مدت بین هر دو ورود")]
        public static double EnteringDiffAverage(this ICollection<RestaurantCustomer> customers)
        {
            return (double)customers.Sum(x => (double)x.PreviousArrivalDiff) /
                (double)(customers.Count() - 1);
        }

        [DisplayNameAttribute("متوسط مدت انتظار")]
        public static double WaitingAverage(this ICollection<RestaurantCustomer> customers)
        {
            return customers
                .Where(x => x.WaitingTime != 0)
                .Average(x => (double)x.WaitingTime);
        }

        [DisplayNameAttribute("متوسط مدت زمان حضور مشتری")]
        public static double CustomerInSystemAverage(this ICollection<RestaurantCustomer> customers)
        {
            return customers
                .Average(x => x.CustomerInSystemTime);
        }
    }
}
