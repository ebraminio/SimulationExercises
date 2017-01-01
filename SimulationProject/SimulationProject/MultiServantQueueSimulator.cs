using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SimulationProject
{
    public class MultiServantQueueSimulator : IEnumerable<MultiServantQueueCustomer>
    {
        private ItemPicker<int> _enteringDifference;
        private IDictionary<Servant, ItemPicker<int>> _serviceTimePickers;
        private IList<Servant> _servants;

        public MultiServantQueueSimulator(IList<Servant> servants, IEnumerable<double> enteringDifferencesRandomNumbers,
            IEnumerable<double> serviceDurationRandomNumbers)
        {
            _enteringDifference = new ItemPicker<int>(enteringDifferencesRandomNumbers.GetEnumerator());

            var serviceDurationRandomNumbersEnumerator = serviceDurationRandomNumbers.GetEnumerator();
            _serviceTimePickers = new Dictionary<Servant, ItemPicker<int>>();

            _servants = servants;
            _servants.ToList().ForEach(servant =>
                _serviceTimePickers[servant] = new ItemPicker<int>(serviceDurationRandomNumbersEnumerator));
        }

        public MultiServantQueueSimulator AddEnteringDifferencePossibility(int enteringDiff, double possibility)
        {
            _enteringDifference.AddEntityPossibilty(enteringDiff, possibility);
            return this;
        }

        public MultiServantQueueSimulator AddServiceTimePossibility(Servant servant, int serviceTime, double possibility)
        {
            _serviceTimePickers[servant].AddEntityPossibilty(serviceTime, possibility);
            return this;
        }

        public IEnumerator<MultiServantQueueCustomer> GetEnumerator()
        {
            var enteringDifferenceEnumerator = _enteringDifference.GetEnumerator();
            var serviceTimeEnumerators = _serviceTimePickers.ToDictionary(x => x.Key, x => x.Value.GetEnumerator());
            var firstInQueue = true;
            int customerArrivalTime = 0;
            var reservedQueues = _servants.ToDictionary(x => x, x => 0);
            int customerId = 0;
            while (enteringDifferenceEnumerator.MoveNext())
            {
                int currentEnter = enteringDifferenceEnumerator.Current;
                if (firstInQueue == true)
                {
                    firstInQueue = false;
                    currentEnter = 0;
                }

                reservedQueues = reservedQueues
                    .ToDictionary(x => x.Key, x => (x.Value < currentEnter) ? 0 : x.Value - currentEnter);

                Servant servant = reservedQueues.First(x => x.Value == reservedQueues.Min(y => y.Value)).Key;

                if (!serviceTimeEnumerators[servant].MoveNext()) break;

                var currentServiceTime = serviceTimeEnumerators[servant].Current;
                var currentQueue = reservedQueues[servant];
                customerArrivalTime += currentEnter;
                customerId++;
                yield return new MultiServantQueueCustomer
                {
                    Id = customerId,
                    PreviousArrivalDiff = currentEnter,
                    ArrivalTime = customerArrivalTime,
                    Servant = servant,
                    ServiceStart = customerArrivalTime + currentQueue,
                    ServiceDuration = currentServiceTime,
                    ServiceEnd = customerArrivalTime + currentQueue + currentServiceTime,
                    WaitingTime = currentQueue
                };

                reservedQueues[servant] += currentServiceTime;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public struct MultiServantQueueCustomer
    {
        [DisplayName("مشتری")]
        public int Id { get; set; }

        [DisplayName("مدت‌های بین دو ورود")]
        public int PreviousArrivalDiff { get; set; }

        [DisplayName("زمان ورود بر حسب زمان شبیه‌سازی")]
        public int ArrivalTime { get; set; }

        [DisplayName("خدمت‌رسان")]
        public Servant Servant { get; set; }

        [DisplayName("زمان شروع خدمت")]
        public int ServiceStart { get; set; }

        [DisplayName("مدت‌های خدمت‌دهی")]
        public int ServiceDuration { get; set; }

        [DisplayName("زمان پایان خدمت")]
        public int ServiceEnd { get; set; }

        [DisplayName("مدت انتظار در صف")]
        public int WaitingTime { get; set; }
        
        public MultiServantQueueCustomer(int id, int previousArrivalDiff, int arrivalTime,
            Servant servant, int serviceStart, int serviceDuration, int serviceEnd,
            int waitingTime) : this()
        {
            Id = id;
            PreviousArrivalDiff = previousArrivalDiff;
            ArrivalTime = arrivalTime;
            Servant = servant;
            ServiceStart = serviceStart;
            ServiceDuration = serviceDuration;
            ServiceEnd = serviceEnd;
            WaitingTime = waitingTime;
        }
    }

    public class Servant
    {
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public static class MultiServantQueueTools
    {
        [DisplayName("میانگین مدت خدمت‌دهی خدمت‌کاران")]
        public static string ServantsBusyAverage(this ICollection<MultiServantQueueCustomer> customers)
        {
            var stats = customers.Select(x => x.Servant).Distinct().Select(
                x => string.Format("{0}: {1:0.###}", x.Name, customers.ServantBusyAverage(x)));
            return string.Join("، ", stats);
        }

        public static double ServantBusyAverage(this ICollection<MultiServantQueueCustomer> customers, Servant servant)
        {
            return (double)customers.Where(x => x.Servant == servant).Sum(x => x.ServiceDuration) /
                (double)customers.Max(x => x.ServiceEnd);
        }

        [DisplayName("نسبت منتظرشدگان")]
        public static double WaitedCustomersRatio(this ICollection<MultiServantQueueCustomer> customers)
        {
            return (double)customers.Count(x => x.WaitingTime != 0) /
                (double)customers.Count();
        }

        [DisplayName("متوسط مدت انتظار")]
        public static double WaitingTimeAverage(this ICollection<MultiServantQueueCustomer> customers)
        {
            return customers.AverageOrZero(x => (double)x.WaitingTime);
        }

        [DisplayName("متوسط مدت انتظار بین منتظران")]
        public static double WaitedCustomersWaitingTimeAverage(this ICollection<MultiServantQueueCustomer> customers)
        {
            return customers.Where(x => x.WaitingTime != 0).AverageOrZero(x => (double)x.WaitingTime);
        }

        [DisplayName("مدت انتظار در صف")]
        public static double WaitedCustomersWaitingTime(this ICollection<MultiServantQueueCustomer> customers)
        {
            return customers.Where(x => x.WaitingTime != 0).Sum(x => (double)x.WaitingTime);
        }

        [DisplayName("مدت خدمت‌دهی خدمت‌کاران")]
        public static string ServantsBusy(this ICollection<MultiServantQueueCustomer> customers)
        {
            var stats = customers.Select(x => x.Servant).Distinct().Select(
                x => string.Format("{0}: {1}", x.Name, customers.ServantBusy(x)));
            return string.Join("، ", stats);
        }

        public static double ServantBusy(this ICollection<MultiServantQueueCustomer> customers, Servant servant)
        {
            return (double)customers.Where(x => x.Servant == servant).Sum(x => x.ServiceDuration);
        }

        [DisplayName("بیشترین تأخیر بین مشتریان")]
        public static double MaxWaitingTime(this ICollection<MultiServantQueueCustomer> customers)
        {
            return customers.Max(x => x.WaitingTime);
        }
    }
}
