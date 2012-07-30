using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationProject
{
    public class NewsstandSimulator : Simulator<NewsstandWarehouse>
    {
        private ItemPicker<DayType> _dayTypePicker;
        private ItemPicker<int> _goodDayRequestPicker;
        private ItemPicker<int> _mediumDayRequestPicker;
        private ItemPicker<int> _badDayRequestPicker;
        private int _warehouseCapacity;
        private int _newspaperPriceForBuy;
        private int _newspaperPriceForSell;
        private int _wasteNewspaperPrice;
        public NewsstandSimulator(ItemPicker<DayType> dayTypePicker,
            ItemPicker<int> goodDayRequestPicker, ItemPicker<int> mediumDayRequestPicker, ItemPicker<int> badDayRequestPicker,
            int warehouseCapacity, int newspaperPriceForBuy, int newspaperPriceForSell, int wasteNewspaperPrice)
        {
            _dayTypePicker = dayTypePicker;
            _goodDayRequestPicker = goodDayRequestPicker;
            _mediumDayRequestPicker = mediumDayRequestPicker;
            _badDayRequestPicker = badDayRequestPicker;
            _warehouseCapacity = warehouseCapacity;
            _newspaperPriceForBuy = newspaperPriceForBuy;
            _newspaperPriceForSell = newspaperPriceForSell;
            _wasteNewspaperPrice = wasteNewspaperPrice;
        }

        public NewsstandSimulator AddDayTypePossibility(DayType dayType, double possibility)
        {
            _dayTypePicker.AddEntityPossibilty(dayType, possibility);
            return this;
        }
        
        private ItemPicker<int> GetDayRequestPickerByDayType(DayType dayType)
        {
            switch (dayType)
            {
                case DayType.Good:
                    return _goodDayRequestPicker;
                case DayType.Medium:
                    return _mediumDayRequestPicker;
                case DayType.Bad:
                    return _badDayRequestPicker;
            }
            return null;
        }

        public NewsstandSimulator AddRequestPossibilityOnDayType(DayType dayType, int request, double possibility)
        {
            GetDayRequestPickerByDayType(dayType).AddEntityPossibilty(request, possibility);
            return this;
        }

        public override IEnumerator<NewsstandWarehouse> GetEnumerator()
        {
            var dayTypeEnumerator = _dayTypePicker.GetEnumerator();
            var requestsEnumerators = new Dictionary<DayType, IEnumerator<int>>();
            requestsEnumerators[DayType.Good] = GetDayRequestPickerByDayType(DayType.Good).GetEnumerator();
            requestsEnumerators[DayType.Medium] = GetDayRequestPickerByDayType(DayType.Medium).GetEnumerator();
            requestsEnumerators[DayType.Bad] = GetDayRequestPickerByDayType(DayType.Bad).GetEnumerator();

            int dayCount = 0;
            while (dayTypeEnumerator.MoveNext())
            {
                var currentDayType = dayTypeEnumerator.Current;

                if (!requestsEnumerators[currentDayType].MoveNext())
                    break;
                var currentRequest = requestsEnumerators[currentDayType].Current;

                var remindFromSell = 0;
                var notAvailableNewspaper = 0;
                var realSell = currentRequest;
                if (currentRequest > _warehouseCapacity)
                {
                    notAvailableNewspaper = currentRequest - _warehouseCapacity;
                    realSell = _warehouseCapacity;
                }
                else if (currentRequest < _warehouseCapacity)
                {
                    remindFromSell = _warehouseCapacity - currentRequest;
                }

                dayCount++;
                yield return new NewsstandWarehouse
                {
                    Id = dayCount,
                    DayType = currentDayType,
                    Requests = currentRequest,
                    SellingIncome = realSell * _newspaperPriceForSell,
                    LostProfit = notAvailableNewspaper * (_newspaperPriceForSell - _newspaperPriceForBuy),
                    WasteNewspaperSell = remindFromSell * _wasteNewspaperPrice,
                    DailyProfit = (realSell * _newspaperPriceForSell) - (_warehouseCapacity * _newspaperPriceForBuy)
                                    - (notAvailableNewspaper * (_newspaperPriceForSell - _newspaperPriceForBuy))
                                    + (remindFromSell * _wasteNewspaperPrice)
                };
            }
        }
    }

    public enum DayType
    {
        Bad, Medium, Good
    }

    public class NewsstandWarehouse : Entity
    {
        public DayType DayType { get; set; }
        public int Requests { get; set; }
        public int SellingIncome { get; set; }
        public int LostProfit { get; set; }
        public int WasteNewspaperSell { get; set; }
        public int DailyProfit { get; set; }

        public NewsstandWarehouse() { }
        public NewsstandWarehouse(int id, DayType dayType, int requests,
            int sellingIncome, int lostProfit, int wasteNewspaperSell, int dailyProfit)
        {
            Id = id;
            DayType = dayType;
            Requests = requests;
            SellingIncome = sellingIncome;
            LostProfit = lostProfit;
            WasteNewspaperSell = wasteNewspaperSell;
            DailyProfit = dailyProfit;
        }
    }
}
