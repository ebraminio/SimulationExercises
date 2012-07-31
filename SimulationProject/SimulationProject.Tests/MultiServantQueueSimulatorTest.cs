using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimulationProject;

namespace SimulationProject.Tests
{
    [TestClass]
    public class MultiServantQueueSumulatorTest
    {
        [TestMethod]
        public void MultiServantQueueSumulator_Test()
        {
            var enterDiffRandomNumbers = new[]
            {
                0, .26, .98, .90, .26, .42, .74, .80, .68, .22,
                .48, .34, .45, .24, .34, .63, .38, .80, .42, .56,
                .89, .18, .51, .71, .16, .92
            }.AsEnumerable();

            var serviceRandomNumbers = new[]
            {
                .95, .21, .51, .92, .89, .38, .13, .61, .50, .49,
                .39, .53, .88, .01, .81, .54, .81, .64, .01, .67,
                .01, .47, .75, .57, .87, .48
            }.AsEnumerable();

            var habil = new Servant { Name = "Habil" };
            var khabbaz = new Servant { Name = "Khabbaz" };
            var simulator = new MultiServantQueueSimulator(new[] { habil, khabbaz }, enterDiffRandomNumbers, serviceRandomNumbers);

            simulator
                .AddEnteringDifferencePossibility(1, .25)
                .AddEnteringDifferencePossibility(2, .40)
                .AddEnteringDifferencePossibility(3, .20)
                .AddEnteringDifferencePossibility(4, .15)

                .AddServiceTimePossibility(habil, 2, .30)
                .AddServiceTimePossibility(habil, 3, .28)
                .AddServiceTimePossibility(habil, 4, .25)
                .AddServiceTimePossibility(habil, 5, .15)

                .AddServiceTimePossibility(khabbaz, 3, .35)
                .AddServiceTimePossibility(khabbaz, 4, .25)
                .AddServiceTimePossibility(khabbaz, 5, .20)
                .AddServiceTimePossibility(khabbaz, 6, .20);

            var expectedCustomersResult = new[]
            {
                new MultiServantQueueCustomer(1, 0, 0, habil, 0, 5, 5, 0),
                new MultiServantQueueCustomer(2, 2, 2, khabbaz, 2, 3, 5, 0),
                new MultiServantQueueCustomer(3, 4, 6, habil, 6, 3, 9, 0),
                new MultiServantQueueCustomer(4, 4, 10, habil, 10, 5, 15, 0),
                new MultiServantQueueCustomer(5, 2, 12, khabbaz, 12, 6, 18, 0),
                new MultiServantQueueCustomer(6, 2, 14, habil, 15, 3, 18, 1),
                new MultiServantQueueCustomer(7, 3, 17, habil, 18, 2, 20, 1),
                new MultiServantQueueCustomer(8, 3, 20, habil, 20, 4, 24, 0),
                new MultiServantQueueCustomer(9, 3, 23, khabbaz, 23, 4, 27, 0),
                new MultiServantQueueCustomer(10, 1, 24, habil, 24, 3, 27, 0),
                new MultiServantQueueCustomer(11, 2, 26, habil, 27, 3, 30, 1),
                new MultiServantQueueCustomer(12, 2, 28, khabbaz, 28, 4, 32, 0),
                new MultiServantQueueCustomer(13, 2, 30, habil, 30, 5, 35, 0),
                new MultiServantQueueCustomer(14, 1, 31, khabbaz, 32, 3, 35, 1),
                new MultiServantQueueCustomer(15, 2, 33, habil, 35, 4, 39, 2),
                new MultiServantQueueCustomer(16, 2, 35, khabbaz, 35, 4, 39, 0),
                new MultiServantQueueCustomer(17, 2, 37, habil, 39, 4, 43, 2),
                new MultiServantQueueCustomer(18, 3, 40, khabbaz, 40, 5, 45, 0),
                new MultiServantQueueCustomer(19, 2, 42, habil, 43, 2, 45, 1),
                new MultiServantQueueCustomer(20, 2, 44, habil, 45, 4, 49, 1),
                new MultiServantQueueCustomer(21, 4, 48, khabbaz, 48, 3, 51, 0),
                new MultiServantQueueCustomer(22, 1, 49, habil, 49, 3, 52, 0),
                new MultiServantQueueCustomer(23, 2, 51, khabbaz, 51, /*4*/5, 56, 0),
                new MultiServantQueueCustomer(24, 3, 54, habil, 54, 3, 57, 0),
                new MultiServantQueueCustomer(25, 1, 55, khabbaz, 56, 6, 62, 1),
                new MultiServantQueueCustomer(26, 4, 59, habil, 59, 3, 62, 0)
            };

            var simulatorEnumerator = simulator.GetEnumerator();
            var customers = new List<MultiServantQueueCustomer>();
            foreach (var expectedResult in expectedCustomersResult)
            {
                simulatorEnumerator.MoveNext();
                Assert.AreEqual(expectedResult, simulatorEnumerator.Current);
                customers.Add(simulatorEnumerator.Current);
            }

            Assert.AreEqual(.90, Math.Round(customers.ServantBusyRatio(habil), 2));
            Assert.AreEqual(.69, Math.Round(customers.ServantBusyRatio(khabbaz), 2));
            Assert.AreEqual(.35, Math.Round(customers.WaitedCustomersRatio(), 2));
            Assert.AreEqual(.42, Math.Round(customers.WaitingTimeAverage(), 2));
            Assert.AreEqual(1.22, Math.Round(customers.WaitedCustomersWaitingTimeAverage(), 2));
        }
    }
}
