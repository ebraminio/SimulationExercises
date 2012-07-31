using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimulationProject;

namespace SimulationProject.Tests
{
    [TestClass]
    public class SupplymentSimulatorTest
    {
        [TestMethod]
        public void SupplymentSimulator_Test()
        {
            var dailyRequestNumbers = new[]
                {
                    .24, .35, .65, .81, .54, .03, .87, .27, .73, .70,
                    .47, .45, .48, .17, .09, .42, .87, .26, .36, .40,
                    .07, .63, .19, .88, .94
                }.AsEnumerable();

            var deliveryTimeNumbers = new[] { .5, /*0*/ 1, .3, .4, .8 }.AsEnumerable();

            var simulator = new SupplymentSimulator(dailyRequestNumbers, deliveryTimeNumbers, 11, 5, 3, 8, 2);

            simulator
                .AddDailyRequestPossibility(0, .10)
                .AddDailyRequestPossibility(1, .25)
                .AddDailyRequestPossibility(2, .35)
                .AddDailyRequestPossibility(3, .21)
                .AddDailyRequestPossibility(4, .09)

                .AddDeliveryTimePossibility(1, .6)
                .AddDeliveryTimePossibility(2, .3)
                .AddDeliveryTimePossibility(3, .1);


            var expectedResults = new[]
            {
                new SupplymentState(1, 1, 3, 1, 2, 0, 0, 1),
                new SupplymentState(1, 2, 2, 1, 1, 0, 0, 0),
                new SupplymentState(1, 3, 9, 2, 7, 0, 0, 0),
                new SupplymentState(1, 4, 7, 3, 4, 0, 0, 0),
                new SupplymentState(1, 5, 4, 2, 2, 0, 9, 1),
                
                new SupplymentState(2, 1, 2, 0, 2, 0, 0, 0),
                new SupplymentState(2, 2, 11, 3, 8, 0, 0, 0),
                new SupplymentState(2, 3, 8, 1, 7, 0, 0, 0),
                new SupplymentState(2, 4, 7, 3, 4, 0, 0, 0),
                new SupplymentState(2, 5, 4, 2, 2, 0, 9, 3),
                
                new SupplymentState(3, 1, 2, 2, 0, 0, 0, 2),
                new SupplymentState(3, 2, 0, 2, 0, 2, 0, 1),
                new SupplymentState(3, 3, 0, 2, 0, 2, 0, 0),
                new SupplymentState(3, 4, 9, 1, 4, 0, 0, 0),
                new SupplymentState(3, 5, 4, 0, 4, 0, 7, 1),
                
                new SupplymentState(4, 1, 4, 2, 2, 0, 0, 0),
                new SupplymentState(4, 2, 9, 3, 6, 0, 0, 0),
                new SupplymentState(4, 3, 6, 1, 5, 0, 0, 0),
                new SupplymentState(4, 4, 5, 2, 3, 0, 0, 0),
                new SupplymentState(4, 5, 3, 2, 1, 0, 10, 1),
                
                new SupplymentState(5, 1, 1, 0, 1, 0, 0, 0),
                new SupplymentState(5, 2, 11, 2, 9, 0, 0, 0),
                new SupplymentState(5, 3, 9, 1, 8, 0, 0, 0),
                new SupplymentState(5, 4, 8, 3, 5, 0, 0, 0),
                new SupplymentState(5, 5, 5, 4, 1, 0, 10, 2),
            };

            var simulatorEnumerator = simulator.GetEnumerator();
            var results = new List<SupplymentState>();
            foreach (var expectedResult in expectedResults)
            {
                simulatorEnumerator.MoveNext();
                Assert.AreEqual(expectedResult, simulatorEnumerator.Current);
                results.Add(simulatorEnumerator.Current);
            }

            Assert.AreEqual(3.5, Math.Round(results.EndOfDaySupplyAverage(), 1));
        }
    }
}
