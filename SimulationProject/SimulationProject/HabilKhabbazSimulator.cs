using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationProject
{
    public class HabilKhabbazSimulator : Simulator<HabilKhabbazCustomer>
    {
        private ItemPicker<int> _enteringDifference;
        private ItemPicker<int> _habilServiceTime;
        private ItemPicker<int> _khabbazServiceTime;

        public HabilKhabbazSimulator(IEnumerable<double> enteringDifferencesRandomNumbers,
            IEnumerable<double> serviceDurationRandomNumbers)
        {
            _enteringDifference = new ItemPicker<int>(enteringDifferencesRandomNumbers.GetEnumerator());

            var serviceDurationRandomNumbersEnumerator = serviceDurationRandomNumbers.GetEnumerator();
            _habilServiceTime = new ItemPicker<int>(serviceDurationRandomNumbersEnumerator);
            _khabbazServiceTime = new ItemPicker<int>(serviceDurationRandomNumbersEnumerator);
        }

        public HabilKhabbazSimulator AddEnteringDifferencePossibility(int enteringDiff, double possibility)
        {
            _enteringDifference.AddEntityPossibilty(enteringDiff, possibility);
            return this;
        }

        public HabilKhabbazSimulator AddHabilServiceTimePossibility(int serviceTime, double possibility)
        {
            _habilServiceTime.AddEntityPossibilty(serviceTime, possibility);
            return this;
        }

        public HabilKhabbazSimulator AddKhabbazServiceTimePossibility(int serviceTime, double possibility)
        {
            _khabbazServiceTime.AddEntityPossibilty(serviceTime, possibility);
            return this;
        }


        public override IEnumerator<HabilKhabbazCustomer> GetEnumerator()
        {
            var enteringDifferenceEnumerator = _enteringDifference.GetEnumerator();
            var habilServiceTimeEnumerator = _habilServiceTime.GetEnumerator();
            var khabbazServiceTimeEnumerator = _khabbazServiceTime.GetEnumerator();
            var firstInQueue = true;
            int customerArrivalTime = 0;
            int habilReservedQueue = 0;
            int khabbazReservedQueue = 0;
            int customerId = 0;
            while (enteringDifferenceEnumerator.MoveNext())
            {
                Servant servant;
                int currentEnter = enteringDifferenceEnumerator.Current;
                if (firstInQueue == true)
                {
                    firstInQueue = false;
                    currentEnter = 0;
                }
                int currentServiceTime;

                habilReservedQueue -= currentEnter;
                if (habilReservedQueue < 0) habilReservedQueue = 0;
                khabbazReservedQueue -= currentEnter;
                if (khabbazReservedQueue < 0) khabbazReservedQueue = 0;

                if (habilReservedQueue <= khabbazReservedQueue) // `=` per priority
                {
                    servant = Servant.Habil;
                    if (!habilServiceTimeEnumerator.MoveNext()) break;
                    currentServiceTime = habilServiceTimeEnumerator.Current;
                }
                else // if (habilReservedQueue > khabbazReservedQueue)
                {
                    servant = Servant.Khabbaz;
                    if (!khabbazServiceTimeEnumerator.MoveNext()) break;
                    currentServiceTime = khabbazServiceTimeEnumerator.Current;
                }



                var reservedQueue = (servant == Servant.Habil) ? habilReservedQueue : khabbazReservedQueue;
                customerArrivalTime += currentEnter;
                customerId++;
                yield return new HabilKhabbazCustomer
                {
                    Id = customerId,
                    PreviousArrivalDiff = currentEnter,
                    ArrivalTime = customerArrivalTime,
                    Servant = servant,
                    ServiceStart = customerArrivalTime + reservedQueue,
                    ServiceDuration = currentServiceTime,
                    ServiceEnd = customerArrivalTime + reservedQueue + currentServiceTime,
                    WaitingTime = reservedQueue
                };

                if (servant == Servant.Habil)
                {
                    habilReservedQueue += currentServiceTime;
                }
                else
                {
                    khabbazReservedQueue += currentServiceTime;
                }


            }
        }
    }

    public class HabilKhabbazCustomer : Entity
    {
        public int PreviousArrivalDiff { get; set; }
        public int ArrivalTime { get; set; }
        public Servant Servant { get; set; }
        public int ServiceStart { get; set; }
        public int ServiceDuration { get; set; }
        public int ServiceEnd { get; set; }
        public int WaitingTime { get; set; }
        
        public HabilKhabbazCustomer() { }
        public HabilKhabbazCustomer(int id, int previousArrivalDiff, int arrivalTime,
            Servant servant, int serviceStart, int serviceDuration, int serviceEnd,
            int waitingTime)
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

    public enum Servant
    {
        Habil, Khabbaz
    }

    public static class HabilKhabbazCustomersTools
    {
        public static double ServantBusyRatio(this ICollection<HabilKhabbazCustomer> customers, Servant servant)
        {
            return (double)customers.Where(x => x.Servant == servant).Sum(x => x.ServiceDuration) /
                (double)customers.Max(x => x.ServiceEnd);
        }

        public static double WaitedCustomersRatio(this ICollection<HabilKhabbazCustomer> customers)
        {
            return (double)customers.Count(x => x.WaitingTime != 0) /
                (double)customers.Count();
        }

        public static double WaitingTimeAverage(this ICollection<HabilKhabbazCustomer> customers)
        {
            return customers.Average(x => (double)x.WaitingTime);
        }

        public static double WaitedCustomersWaitingTimeAverage(this ICollection<HabilKhabbazCustomer> customers)
        {
            return customers.Where(x => x.WaitingTime != 0).Average(x => (double)x.WaitingTime);
        }
    }
}
