﻿using CareTogether.Engines;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CareTogether.Core.Test.ReferralCalculationTests
{
    [TestClass]
    public class CalculateMissingSetupRequirements
    {
        [TestMethod]
        public void TestNoRequirementsCompleted()
        {
            var result = ReferralCalculations.CalculateMissingSetupRequirements(
                Helpers.From("A", "B", "C"),
                Helpers.Completed());

            AssertEx.SequenceIs(result,
                new MissingArrangementRequirement("A", null, null),
                new MissingArrangementRequirement("B", null, null),
                new MissingArrangementRequirement("C", null, null));
        }

        [TestMethod]
        public void TestPartialRequirementsCompleted()
        {
            var result = ReferralCalculations.CalculateMissingSetupRequirements(
                Helpers.From("A", "B", "C"),
                Helpers.Completed(("A", 1), ("A", 2), ("B", 3)));

            AssertEx.SequenceIs(result,
                new MissingArrangementRequirement("C", null, null));
        }

        [TestMethod]
        public void TestAllRequirementsCompleted()
        {
            var result = ReferralCalculations.CalculateMissingSetupRequirements(
                Helpers.From("A", "B", "C"),
                Helpers.Completed(("A", 1), ("A", 2), ("B", 3), ("C", 12)));

            AssertEx.SequenceIs(result);
        }
    }
}
