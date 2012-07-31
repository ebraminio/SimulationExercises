using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationProject
{
    public class SupplymentSimulator : Simulator<NewsstandWarehouse>
    {
        private ItemPicker<int> _dailyRequestPicker;
        private ItemPicker<int> _deliveryTimeoutPicker;
        private int _warehouseCapacity;
        private int _rechekingPeriod;
        public SupplymentSimulator(IEnumerable<double> dailyRequestNumbers, IEnumerable<double> deliveryTimeoutRandomNumbers,
            int warehouseCapacity, int rechekingPeriod)
        {
            _dailyRequestPicker = new ItemPicker<int>(dailyRequestNumbers.GetEnumerator());
            _deliveryTimeoutPicker = new ItemPicker<int>(deliveryTimeoutRandomNumbers.GetEnumerator());

            _warehouseCapacity = warehouseCapacity;
            _rechekingPeriod = rechekingPeriod;
        }

        public SupplymentSimulator AddDailyRequestPossibility(int dailyRequest, double possibility)
        {
            _dailyRequestPicker.AddEntityPossibilty(dailyRequest, possibility);
            return this;
        }

        public SupplymentSimulator AddDeliveryTimeoutPossibility(int deliveryTimeout, double possibility)
        {
            _deliveryTimeoutPicker.AddEntityPossibilty(deliveryTimeout, possibility);
            return this;
        }

        public override IEnumerator<NewsstandWarehouse> GetEnumerator()
        {
            yield return null;
        //    var dayTypeEnumerator = _dailyRequestPicker.GetEnumerator();

        //    int dayCount = 0;
        //    while (dayTypeEnumerator.MoveNext())
        //    {
        //        var currentDayType = dayTypeEnumerator.Current;

        //        if (!requestsEnumerators[currentDayType].MoveNext())
        //            break;
        //        var currentRequest = requestsEnumerators[currentDayType].Current;

        //        var remindFromSell = 0;
        //        var notAvailableNewspaper = 0;
        //        var realSell = currentRequest;
        //        if (currentRequest > _warehouseCapacity)
        //        {
        //            notAvailableNewspaper = currentRequest - _warehouseCapacity;
        //            realSell = _warehouseCapacity;
        //        }
        //        else if (currentRequest < _warehouseCapacity)
        //        {
        //            remindFromSell = _warehouseCapacity - currentRequest;
        //        }

        //        dayCount++;
        //        yield return new NewsstandWarehouse
        //        {
        //            Id = dayCount,
        //            DayType = currentDayType,
        //            Requests = currentRequest,
        //            SellingIncome = realSell * _newspaperPriceForSell,
        //            LostProfit = notAvailableNewspaper * (_newspaperPriceForSell - _newspaperPriceForBuy),
        //            WasteNewspaperSell = remindFromSell * _wasteNewspaperPrice,
        //            DailyProfit = (realSell * _newspaperPriceForSell) - (_warehouseCapacity * _newspaperPriceForBuy)
        //                            - (notAvailableNewspaper * (_newspaperPriceForSell - _newspaperPriceForBuy))
        //                            + (remindFromSell * _wasteNewspaperPrice)
        //        };
        //    }
        }
    }

    public class Supplyment : Entity
    {
        public int Period { get; set; }
        public int DayInPeriod { get; set; }
        public int Request { get; set; }
        public int EndOfDaySupply { get; set; }
        public int Leakage { get; set; }

        public Supplyment() { }
        public Supplyment(int id)
        {
        }
    }

    public static class SupplymentTools
    {
        //public static double TotalProfit(this ICollection<NewsstandWarehouse> warehouse)
        //{
        //    return warehouse.Sum(x => x.DailyProfit);
        //}
    }
}
