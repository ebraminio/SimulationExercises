using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestaurantSimulation;
using SimulationProject;

namespace RestaurantSimulation.Tests
{
    [TestClass]
    public class RandomPicker
    {
        [TestMethod]
        public void RestaurantSimulation_Test()
        {
            var enterDiffRandomNumbers = new[]
            {
                0, .913, .727, .015, .948, .309, .922, .753, .235, .302,
                .109, .093, .607, .738, .359, .888, .106, .212, .393, .535,
            };
            var serviceRandomNumbers = new[]
            {
                .84, .10, .74, .53, .17, .79, .91, .67, .89, .38,
                .32, .94, .79, .05, /*.79*/.94, .84, .52, .55, .30, .50,
            };
            var rs = new RestaurantSimulator(
                new ItemPicker<int>(enterDiffRandomNumbers.AsEnumerable()),
                new ItemPicker<int>(serviceRandomNumbers.AsEnumerable()));

            Enumerable.Range(1, 8).ToList().ForEach(x =>
                rs.AddEnteringDifferencePossibility(x, .125));

            var servicePossibilties = new[] { .10, .20, .30, .25, .10, .05 };
            Enumerable.Range(1, servicePossibilties.Length)
                .Zip(servicePossibilties, (x, y) => new { x, y })
                .ToList()
                .ForEach(x => rs.AddServiceTimePossibility(x.x, x.y));

            var expectedCustomersResult = new[]
            {
                new Customer(1, 0, 0, 4, 0, 0, 4, 4, 0),
                new Customer(2, 8, 8, 1, 8, 0, 9, 1, 4),
                new Customer(3, 6, 14, 4, 14, 0, 18, 4, 5),
                new Customer(4, 1, 15, 3, 18, 3, 21, 6, 0),
                new Customer(5, 8, 23, 2, 23, 0, 25, 2, 2),
                new Customer(6, 3, 26, 4, 26, 0, 30, 4, 1),
                new Customer(7, 8, 34, 5, 34, 0, 39, 5, 4),
                new Customer(8, 7, 41, 4, 41, 0, 45, 4, 2),
                new Customer(9, 2, 43, 5, 45, 2, 50, 7, 0),
                new Customer(10, 3, 46, 3, 50, 4, 53, 7, 0),
                new Customer(11, 1, 47, 3, 53, 6, 56, 9, 0),
                new Customer(12, 1, 48, 5, 56, 8, 61, 13, 0),
                new Customer(13, 5, 53, 4, 61, 8, 65, 12, 0),
                new Customer(14, 6, 59, 1, 65, 6, 66, 7, 0),
                new Customer(15, 3, 62, 5, 66, 4, 71, 9, 0),
                new Customer(16, 8, 70, 4, 71, 1, 75, 5, 0),
                new Customer(17, 1, 71, 3, 75, 4, 78, 7, 0),
                new Customer(18, 2, 73, 3, 78, 5, 81, 8, 0),
                new Customer(19, 4, 77, 2, 81, 4, 83, 6, 0),
                new Customer(20, 5, 82, 3, 83, 1, 86, 4, 0),
            };

            var customers = rs.Take(20).ToList();

            expectedCustomersResult
                .Zip(customers, (x, y) => new { x, y })
                .ToList()
                .ForEach(x => Assert.AreEqual(x.x, x.y));

            Assert.AreEqual(2.8, customers.WaitingTimeAverage());
            Assert.AreEqual(0.65, customers.WaitedCustomersRatio());
            Assert.AreEqual(0.21, Math.Round(customers.NoCustomerRatio(), 2));
            Assert.AreEqual(3.4, customers.ServiceAverage());
            Assert.AreEqual(4.3, Math.Round(customers.EnteringDiffAverage(), 1));
            Assert.AreEqual(4.3, Math.Round(customers.WaitingAverage(), 1));
            Assert.AreEqual(6.2, customers.CustomerInSystemAverage());
        }

        [TestMethod]
        public void ItemPicker_Test()
        {
            var picker = new ItemPicker<int>(new FlipFlopMantissa());

            var arrivalDiffSample = new[]
            {
                new { Diff = 1, Possibility = 0.125 },
                new { Diff = 2, Possibility = 0.125 },
                new { Diff = 3, Possibility = 0.125 },
            };

            arrivalDiffSample.ToList().ForEach(x =>
                picker.AddEntityPossibilty(x.Diff, x.Possibility));
            
            new[] { 1, 3, 1, 3 }
                .Zip(picker, (x, y) => new { x, y })
                .ToList()
                .ForEach(x => Assert.AreEqual(x.y, x.x));
        }
    }
}
