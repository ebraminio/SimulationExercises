using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SimulationProject
{
    public class NewsstandSimulator : IEnumerable<NewsstandWarehouse>
    {
        private ItemPicker<DayType> _dayTypePicker;
        private ItemPicker<int> _goodDayRequestPicker;
        private ItemPicker<int> _mediumDayRequestPicker;
        private ItemPicker<int> _badDayRequestPicker;
        private int _warehouseCapacity;
        private int _newspaperPriceForBuy;
        private int _newspaperPriceForSell;
        private int _wasteNewspaperPrice;
        public NewsstandSimulator(IEnumerable<double> dayTypeRandomNumbers, IEnumerable<double> requestRandomNumbers,
            int warehouseCapacity, int newspaperPriceForBuy, int newspaperPriceForSell, int wasteNewspaperPrice)
        {
            _dayTypePicker = new ItemPicker<DayType>(dayTypeRandomNumbers.GetEnumerator());

            var requestRandomNumbersEnumerator = requestRandomNumbers.GetEnumerator();
            _goodDayRequestPicker = new ItemPicker<int>(requestRandomNumbersEnumerator);
            _mediumDayRequestPicker = new ItemPicker<int>(requestRandomNumbersEnumerator);
            _badDayRequestPicker = new ItemPicker<int>(requestRandomNumbersEnumerator);

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

        public IEnumerator<NewsstandWarehouse> GetEnumerator()
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

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public enum DayType
    {
        Bad, Medium, Good
    }

    public struct NewsstandWarehouse
    {
        [DisplayNameAttribute("روز")]
        public int Id { get; set; }

        [DisplayNameAttribute("نوع روز")]
        public DayType DayType { get; set; }

        [DisplayNameAttribute("تقاضا")]
        public int Requests { get; set; }

        [DisplayNameAttribute("درآمد حاصل از فروش")]
        public int SellingIncome { get; set; }

        [DisplayNameAttribute("سود از دست رفته به دلیل فزونی تقاضا")]
        public int LostProfit { get; set; }

        [DisplayNameAttribute("درآمد ناشی از فروش به قیمت باطله")]
        public int WasteNewspaperSell { get; set; }

        [DisplayNameAttribute("سود روزانه")]
        public int DailyProfit { get; set; }

        public NewsstandWarehouse(int id, DayType dayType, int requests,
            int sellingIncome, int lostProfit, int wasteNewspaperSell, int dailyProfit)
            : this()
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

    public static class NewsstandWarehouseTools
    {
        [DisplayNameAttribute("سود کل")]
        public static double TotalProfit(this ICollection<NewsstandWarehouse> warehouse)
        {
            return warehouse.Sum(x => x.DailyProfit);
        }
    }
}
