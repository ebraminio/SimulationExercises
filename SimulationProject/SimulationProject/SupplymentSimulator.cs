using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SimulationProject
{
    public class SupplymentSimulator : IEnumerable<SupplymentState>
    {
        private ItemPicker<int> _dailyRequestPicker;
        private ItemPicker<int> _deliveryTimePicker;
        private int _supplymentCapacity;
        private int _rechekingPeriod;
        private int _beginningSupply;
        private int _beginningOrder;
        private int _beginningOrderDelivery;
        public SupplymentSimulator(IEnumerable<double> dailyRandomRequestNumbers,
            IEnumerable<double> deliveryTimeRandomNumbers, int capacity, int rechekingPeriod,
            int beginningSupply, int beginningOrder, int beginningOrderDelivery)
        {
            _dailyRequestPicker = new ItemPicker<int>(dailyRandomRequestNumbers.GetEnumerator());
            _deliveryTimePicker = new ItemPicker<int>(deliveryTimeRandomNumbers.GetEnumerator());

            _supplymentCapacity = capacity;
            _rechekingPeriod = rechekingPeriod;
            _beginningSupply = beginningSupply;
            _beginningOrder = beginningOrder;
            _beginningOrderDelivery = beginningOrderDelivery;
        }

        public SupplymentSimulator AddDailyRequestPossibility(int dailyRequest, double possibility)
        {
            _dailyRequestPicker.AddEntityPossibilty(dailyRequest, possibility);
            return this;
        }

        public SupplymentSimulator AddDeliveryTimePossibility(int deliveryTimeout, double possibility)
        {
            _deliveryTimePicker.AddEntityPossibilty(deliveryTimeout, possibility);
            return this;
        }

        public IEnumerator<SupplymentState> GetEnumerator()
        {
            var dailyRequestEnumerator = _dailyRequestPicker.GetEnumerator();
            var deliveryTimeEnumerator = _deliveryTimePicker.GetEnumerator();

            int period = 1;
            var supply = _beginningSupply;
            int order = _beginningOrder;
            int orderDelivery = _beginningOrderDelivery;
            while (deliveryTimeEnumerator.MoveNext())
            {
                int dayInPeriod = 1;
                int requestsSum = 0;
                int leakagesSum = 0;
                while (_rechekingPeriod >= dayInPeriod)
                {
                    if (!dailyRequestEnumerator.MoveNext()) yield break;
                    if (orderDelivery == 0)
                    {
                        supply += order;
                    }
                    requestsSum += dailyRequestEnumerator.Current;

                    if (_rechekingPeriod == dayInPeriod)
                    {
                        orderDelivery = deliveryTimeEnumerator.Current;
                        order = requestsSum;
                    }
                    else
                    {
                        orderDelivery--;
                    }

                    int endOfDaySupply = supply - dailyRequestEnumerator.Current;
                    int leakage = 0;
                    if (supply < dailyRequestEnumerator.Current)
                    {
                        leakage = dailyRequestEnumerator.Current - supply;
                        leakagesSum += leakage;
                        endOfDaySupply = 0;
                    }
                    else
                    {
                        if (leakagesSum != 0)
                        {
                            endOfDaySupply -= leakagesSum;
                            leakagesSum = 0;
                        }
                    }

                    yield return new SupplymentState
                    {
                        Period = period,
                        DayInPeriod = dayInPeriod,
                        Supply = supply,
                        Request = dailyRequestEnumerator.Current,
                        EndOfDaySupply = endOfDaySupply,
                        Leakage = leakage,
                        Order = (_rechekingPeriod == dayInPeriod) ? requestsSum : 0,
                        OrderDeliveryLeftDays = (orderDelivery > 0) ? orderDelivery : 0,
                    };
                    supply = endOfDaySupply;
                    dayInPeriod++;
                }
                period++;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public struct SupplymentState
    {
        [DisplayName("دور")]
        public int Period { get; set; }

        [DisplayName("روز")]
        public int DayInPeriod { get; set; }

        [DisplayName("موجودی در ابتدای روز")]
        public int Supply { get; set; }

        [DisplayName("تقاضا")]
        public int Request { get; set; }

        [DisplayName("موجودی در انتهای روز")]
        public int EndOfDaySupply { get; set; }

        [DisplayName("مقدار کمبود")]
        public int Leakage { get; set; }

        [DisplayName("مقدار سفارش")]
        public int Order { get; set; }

        [DisplayName("روزهای مانده تا ورود سفارش")]
        public int OrderDeliveryLeftDays { get; set; }

        public SupplymentState(int period, int dayInPeriod, int supply, int request,
            int endOfDaySupply, int leakage, int order, int orderDeliveryLeftDays) : this()
        {
            Period = period;
            DayInPeriod = dayInPeriod;
            Supply = supply;
            Request = request;
            EndOfDaySupply = endOfDaySupply;
            Leakage = leakage;
            Order = order;
            OrderDeliveryLeftDays = orderDeliveryLeftDays;
        }
    }

    public static class SupplymentStatesTools
    {
        [DisplayName("متوسط موجودی در انتهای روز")]
        public static double EndOfDaySupplyAverage(this ICollection<SupplymentState> supplymentStates)
        {
            return supplymentStates.AverageOrZero(x => (double)x.EndOfDaySupply);
        }
    }
}
