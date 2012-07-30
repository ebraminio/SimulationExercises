using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimulationProject;

namespace SimulationProject.Tests
{
    [TestClass]
    public class QueueSimulatorTest
    {
        [TestMethod]
        public void ItemPicker_Test()
        {
            var picker = new ItemPicker<int>(new FlipFlopMantissa().GetEnumerator());

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
