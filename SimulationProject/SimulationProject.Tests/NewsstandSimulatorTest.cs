using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimulationProject;

namespace SimulationProject.Tests
{
    [TestClass]
    public class NewsstandSimulatorTest
    {
        [TestMethod]
        public void NewsstandSimulator_Test()
        {
            var dayTypeRandomNumbers = new[]
                {
                    .94, .77, .49, .45, .43, .32, .49, /*.00*/ 1, .16, .24,
                    .31, .12, .41, .61, .85, .08, .15, .97, .52, .7
                }.AsEnumerable();

            var requestRandomNumbers = new[]
                {
                    .80, .20, .15, .88, .98, .65, .86, .73, .24, .60,
                    .60, .29, .18, .90, .93, .73, .21, .45, .74, .96
                }.AsEnumerable();

            var requestRandomEnumerator = requestRandomNumbers.GetEnumerator();

            var simulator = new NewsstandSimulator(
                new ItemPicker<DayType>(dayTypeRandomNumbers.GetEnumerator()),
                new ItemPicker<int>(requestRandomEnumerator),
                new ItemPicker<int>(requestRandomEnumerator),
                new ItemPicker<int>(requestRandomEnumerator),
                70, 13, 20, 2);

            simulator
                .AddDayTypePossibility(DayType.Good, .35)
                .AddDayTypePossibility(DayType.Medium, .45)
                .AddDayTypePossibility(DayType.Bad, .20)

                .AddRequestPossibilityOnDayType(DayType.Good, 40, .03)
                .AddRequestPossibilityOnDayType(DayType.Good, 50, .05)
                .AddRequestPossibilityOnDayType(DayType.Good, 60, .15)
                .AddRequestPossibilityOnDayType(DayType.Good, 70, .20)
                .AddRequestPossibilityOnDayType(DayType.Good, 80, .35)
                .AddRequestPossibilityOnDayType(DayType.Good, 90, .15)
                .AddRequestPossibilityOnDayType(DayType.Good, 100, .07)

                .AddRequestPossibilityOnDayType(DayType.Medium, 40, .10)
                .AddRequestPossibilityOnDayType(DayType.Medium, 50, .18)
                .AddRequestPossibilityOnDayType(DayType.Medium, 60, .40)
                .AddRequestPossibilityOnDayType(DayType.Medium, 70, .20)
                .AddRequestPossibilityOnDayType(DayType.Medium, 80, .08)
                .AddRequestPossibilityOnDayType(DayType.Medium, 90, .04)
                .AddRequestPossibilityOnDayType(DayType.Medium, 100, .00)

                .AddRequestPossibilityOnDayType(DayType.Bad, 40, .44)
                .AddRequestPossibilityOnDayType(DayType.Bad, 50, .22)
                .AddRequestPossibilityOnDayType(DayType.Bad, 60, .16)
                .AddRequestPossibilityOnDayType(DayType.Bad, 70, .12)
                .AddRequestPossibilityOnDayType(DayType.Bad, 80, .06)
                .AddRequestPossibilityOnDayType(DayType.Bad, 90, .00)
                .AddRequestPossibilityOnDayType(DayType.Bad, 100, .00);

            var expectedResults = new[]
            {
                new NewsstandWarehouse(1, DayType.Bad, 60, 1200, 0 , 20, 310),
                new NewsstandWarehouse(2, DayType.Medium, 50, 1000, 0 , 40, 130),
                new NewsstandWarehouse(3, DayType.Medium, 50, 1000, 0 , 40, 130),
                new NewsstandWarehouse(4, DayType.Medium, 70, 1400, 0 , 0, 490),
                new NewsstandWarehouse(5, DayType.Medium, 90, 1400, 140 , 0, 350),
                new NewsstandWarehouse(6, DayType.Good, 80, 1400, 70 , 0, 420),
                new NewsstandWarehouse(7, DayType.Medium, 70, 1400, 0 , 0, 490),
                new NewsstandWarehouse(8, DayType.Bad, 60, 1200, 0 , 20, 310),
                new NewsstandWarehouse(9, DayType.Good, 70, 1400, 0 , 0, 490),
                new NewsstandWarehouse(10, DayType.Good, 80, 1400, 70 , 0, 420),
                new NewsstandWarehouse(11, DayType.Good, 80, 1400, 70 , 0, 420),
                new NewsstandWarehouse(12, DayType.Good, 70, 1400, 0 , 0, 490),
                new NewsstandWarehouse(13, DayType.Medium, 50, 1000, 0 , 40, 130),
                new NewsstandWarehouse(14, DayType.Medium, 80, 1400, 70 , 0, 420),
                new NewsstandWarehouse(15, DayType.Bad, 70, 1400, 0, 0, 490),
                new NewsstandWarehouse(16, DayType.Good, 80, 1400, 70 , 0, 420),
                new NewsstandWarehouse(17, DayType.Good, 60, 1200, 0 , 20, 310),
                new NewsstandWarehouse(18, DayType.Bad, 50, 1000, 0 , 40, 130),
                new NewsstandWarehouse(19, DayType.Medium, 70, 1400, 0 , 0, 490),
                new NewsstandWarehouse(20, DayType.Medium, 80, 1400, 70 , 0, 420),
            };

            var simulatorEnumerator = simulator.GetEnumerator();
            foreach (var expectedResult in expectedResults)
            {
                simulatorEnumerator.MoveNext();
                Assert.AreEqual(expectedResult, simulatorEnumerator.Current);
            }
        }
    }
}
