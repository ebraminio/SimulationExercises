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

        public HabilKhabbazSimulator(ItemPicker<int> enteringDifference,
            ItemPicker<int> habilServiceTime, ItemPicker<int> khabbazServiceTime)
        {
            _enteringDifference = enteringDifference;
            _habilServiceTime = habilServiceTime;
            _khabbazServiceTime = khabbazServiceTime;
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
            int previousHabilServiceTime = 0;
            int previousKhabbazServiceTime = 0;
            int reservedHabilQueue = 0;
            int reservedKhabbazQueue = 0;
            int customerId = 0;
            while (enteringDifferenceEnumerator.MoveNext())
            {
                var currentEnter = enteringDifferenceEnumerator.Current;
                if (firstInQueue == true)
                {
                    firstInQueue = false;
                    currentEnter = 0;
                }

                if (customerId == 16)
                    customerId = 16;

                if (currentEnter >= previousHabilServiceTime + reservedHabilQueue)
                {
                    reservedHabilQueue = 0;
                    previousHabilServiceTime = 0;
                }

                if (currentEnter >= previousKhabbazServiceTime + reservedKhabbazQueue)
                {
                    reservedKhabbazQueue = 0;
                    previousKhabbazServiceTime = 0;
                }

                if (customerId == 16)
                    customerId = 16;

                Servant servantTurn;
                if (reservedHabilQueue + previousHabilServiceTime < 
                    reservedKhabbazQueue + previousKhabbazServiceTime)
                {
                    servantTurn = Servant.Habil;
                }
                else if (reservedHabilQueue + previousHabilServiceTime > 
                    reservedKhabbazQueue + previousKhabbazServiceTime)
                {
                    servantTurn = Servant.Khabbaz;
                }
                else // if (reservedHabilQueue + previousHabilServiceTime == reservedKhabbazQueue + previousKhabbazServiceTime)
                {
                    servantTurn = Servant.Habil; // priority
                }
                
                int currentServiceTime = 0;
                int reservedQueue = 0;
                if (servantTurn == Servant.Habil)
                {
                    if (!habilServiceTimeEnumerator.MoveNext())
                        break;

                    currentServiceTime = habilServiceTimeEnumerator.Current;

                    if (customerId == 14)
                        customerId = 14;
                    reservedHabilQueue += previousHabilServiceTime;
                }
                else
                {
                    if (!khabbazServiceTimeEnumerator.MoveNext())
                        break;

                    currentServiceTime = khabbazServiceTimeEnumerator.Current;

                    reservedKhabbazQueue += previousKhabbazServiceTime;
                }

                reservedHabilQueue -= currentEnter;
                if (reservedHabilQueue < 0)
                {
                    reservedHabilQueue = 0;
                }
                
                reservedKhabbazQueue -= currentEnter;
                if (reservedKhabbazQueue < 0)
                {
                    reservedKhabbazQueue = 0;
                }

                if (servantTurn == Servant.Habil)
                {
                    reservedQueue = reservedHabilQueue;
                }
                else
                {
                    reservedQueue = reservedKhabbazQueue;
                }

                customerArrivalTime += currentEnter;
                customerId++;
                yield return new HabilKhabbazCustomer
                {
                    Id = customerId,
                    PreviousArrivalDiff = currentEnter,
                    ArrivalTime = customerArrivalTime,
                    Servant = servantTurn,
                    ServiceStart = customerArrivalTime + reservedQueue,
                    ServiceDuration = currentServiceTime,
                    ServiceEnd = customerArrivalTime + reservedQueue + currentServiceTime,
                    WaitingTime = reservedQueue
                };

                previousHabilServiceTime -= currentEnter;
                if (previousHabilServiceTime < 0)
                {
                    previousHabilServiceTime = 0;
                }
                
                previousKhabbazServiceTime -= currentEnter;
                if (previousKhabbazServiceTime < 0)
                {
                    previousKhabbazServiceTime = 0;
                }

                if (servantTurn == Servant.Habil)
                {
                    previousHabilServiceTime = currentServiceTime;
                }
                else if (servantTurn == Servant.Khabbaz)
                {
                    previousKhabbazServiceTime = currentServiceTime;
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
}
