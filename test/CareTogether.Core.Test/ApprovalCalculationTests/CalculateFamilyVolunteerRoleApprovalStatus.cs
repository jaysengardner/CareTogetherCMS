﻿using CareTogether.Engines;
using CareTogether.Engines.PolicyEvaluation;
using CareTogether.Resources;
using CareTogether.Resources.Directory;
using CareTogether.Resources.Policies;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Immutable;

namespace CareTogether.Core.Test.ApprovalCalculationTests
{
    [TestClass]
    public class CalculateFamilyVolunteerRoleApprovalStatus
    {
        private static Guid Id(char x) => Guid.Parse("xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx".Replace('x', x));
        static readonly Guid guid0 = Id('0');
        static readonly Guid guid1 = Id('1');
        static readonly Guid guid2 = Id('2');
        static readonly Guid guid3 = Id('3');
        static readonly Guid guid4 = Id('4');
        static readonly Guid guid5 = Id('5');
        static readonly Guid guid6 = Id('6');

        static ImmutableList<VolunteerFamilyApprovalRequirement> requirements =
            Helpers.FamilyApprovalRequirements(
                (RequirementStage.Application, "A", VolunteerFamilyRequirementScope.OncePerFamily),
                (RequirementStage.Approval, "B", VolunteerFamilyRequirementScope.OncePerFamily),
                (RequirementStage.Approval, "C", VolunteerFamilyRequirementScope.AllAdultsInTheFamily),
                (RequirementStage.Approval, "D", VolunteerFamilyRequirementScope.AllParticipatingAdultsInTheFamily),
                (RequirementStage.Onboarding, "E", VolunteerFamilyRequirementScope.OncePerFamily),
                (RequirementStage.Onboarding, "F", VolunteerFamilyRequirementScope.AllParticipatingAdultsInTheFamily));

        static Person adult1 = new Person(guid1, true, "Bob", "Smith", Gender.Male, new ExactAge(new DateTime(2000, 1, 1)), "",
            ImmutableList<Address>.Empty, null, ImmutableList<PhoneNumber>.Empty, null, ImmutableList<EmailAddress>.Empty, null, null, null);
        static Person adult2 = new Person(guid2, true, "Jane", "Smith", Gender.Female, new ExactAge(new DateTime(2000, 1, 1)), "",
            ImmutableList<Address>.Empty, null, ImmutableList<PhoneNumber>.Empty, null, ImmutableList<EmailAddress>.Empty, null, null, null);
        static Person inactiveAdult3 = new Person(guid3, false, "BobDUPLICATE", "Smith", Gender.Male, new ExactAge(new DateTime(2000, 1, 1)), "",
            ImmutableList<Address>.Empty, null, ImmutableList<PhoneNumber>.Empty, null, ImmutableList<EmailAddress>.Empty, null, null, null);
        static Person brotherNotInHousehold4 = new Person(guid2, true, "Eric", "Smith", Gender.Male, new ExactAge(new DateTime(2000, 1, 1)), "",
            ImmutableList<Address>.Empty, null, ImmutableList<PhoneNumber>.Empty, null, ImmutableList<EmailAddress>.Empty, null, null, null);
        static Person child5 = new Person(guid5, true, "Wanda", "Smith", Gender.Female, new ExactAge(new DateTime(2022, 1, 1)), "",
            ImmutableList<Address>.Empty, null, ImmutableList<PhoneNumber>.Empty, null, ImmutableList<EmailAddress>.Empty, null, null, null);

        static Family family = new Family(guid0, guid1,
            ImmutableList<(Person, FamilyAdultRelationshipInfo)>.Empty
                .Add((adult1, new FamilyAdultRelationshipInfo("Dad", true)))
                .Add((adult2, new FamilyAdultRelationshipInfo("Mom", true)))
                /*.Add((inactiveAdult3, new FamilyAdultRelationshipInfo("Dad", true))) //TODO: Reenable
                .Add((brotherNotInHousehold4, new FamilyAdultRelationshipInfo("Brother", false)))*/, //TODO: Reenable
            ImmutableList<Person>.Empty
                .Add(child5),
            ImmutableList<CustodialRelationship>.Empty
                .Add(new CustodialRelationship(guid5, guid1, CustodialRelationshipType.ParentWithCustody))
                .Add(new CustodialRelationship(guid5, guid2, CustodialRelationshipType.ParentWithCustody)),
            ImmutableList<UploadedDocumentInfo>.Empty, ImmutableList<Guid>.Empty,
            ImmutableList<CompletedCustomFieldInfo>.Empty, ImmutableList<Activity>.Empty);


        [TestMethod]
        public void TestNotApplied()
        {
            var (status, expiresAtUtc, missingRequirements, availableApplications, missingIndividualRequirements) =
                ApprovalCalculations.CalculateFamilyVolunteerRoleApprovalStatus("Role",
                new VolunteerFamilyRolePolicyVersion("v1", SupersededAtUtc: null, requirements),
                utcNow: new DateTime(2022, 1, 20), family,
                Helpers.Completed(),
                Helpers.Exempted(),
                Helpers.CompletedIndividualRequirements(),
                Helpers.ExemptedIndividualRequirements(),
                Helpers.RemovedIndividualRoles((guid2, "Role")));

            Assert.AreEqual(null, status);
            Assert.AreEqual(null, expiresAtUtc);
            AssertEx.SequenceIs(missingRequirements);
            AssertEx.SequenceIs(availableApplications, "A");
            AssertEx.DictionaryIs(missingIndividualRequirements);
        }

        [TestMethod]
        public void TestNotAppliedWillBeSuperseded()
        {
            var (status, expiresAtUtc, missingRequirements, availableApplications, missingIndividualRequirements) =
                ApprovalCalculations.CalculateFamilyVolunteerRoleApprovalStatus("Role",
                new VolunteerFamilyRolePolicyVersion("v1", SupersededAtUtc: new DateTime(2022, 1, 10), requirements),
                utcNow: new DateTime(2022, 1, 5), family,
                Helpers.Completed(),
                Helpers.Exempted(),
                Helpers.CompletedIndividualRequirements(),
                Helpers.ExemptedIndividualRequirements(),
                Helpers.RemovedIndividualRoles((guid2, "Role")));

            Assert.AreEqual(null, status);
            Assert.AreEqual(null, expiresAtUtc);
            AssertEx.SequenceIs(missingRequirements);
            AssertEx.SequenceIs(availableApplications, "A");
            AssertEx.DictionaryIs(missingIndividualRequirements);
        }

        [TestMethod]
        public void TestNotAppliedHasBeenSuperseded()
        {
            var (status, expiresAtUtc, missingRequirements, availableApplications, missingIndividualRequirements) =
                ApprovalCalculations.CalculateFamilyVolunteerRoleApprovalStatus("Role",
                new VolunteerFamilyRolePolicyVersion("v1", SupersededAtUtc: new DateTime(2022, 1, 10), requirements),
                utcNow: new DateTime(2022, 1, 20), family,
                Helpers.Completed(),
                Helpers.Exempted(),
                Helpers.CompletedIndividualRequirements(),
                Helpers.ExemptedIndividualRequirements(),
                Helpers.RemovedIndividualRoles((guid2, "Role")));

            Assert.AreEqual(null, status);
            Assert.AreEqual(null, expiresAtUtc);
            AssertEx.SequenceIs(missingRequirements);
            AssertEx.SequenceIs(availableApplications, "A");
            AssertEx.DictionaryIs(missingIndividualRequirements);
        }

        [TestMethod]
        public void TestAppliedOnly()
        {
            var (status, expiresAtUtc, missingRequirements, availableApplications, missingIndividualRequirements) =
                ApprovalCalculations.CalculateFamilyVolunteerRoleApprovalStatus("Role",
                new VolunteerFamilyRolePolicyVersion("v1", SupersededAtUtc: null, requirements),
                utcNow: new DateTime(2022, 1, 20), family,
                Helpers.Completed(("A", 1)),
                Helpers.Exempted(),
                Helpers.CompletedIndividualRequirements(),
                Helpers.ExemptedIndividualRequirements(),
                Helpers.RemovedIndividualRoles((guid2, "Role")));

            Assert.AreEqual(RoleApprovalStatus.Prospective, status);
            Assert.AreEqual(null, expiresAtUtc);
            AssertEx.SequenceIs(missingRequirements, "B");
            AssertEx.SequenceIs(availableApplications);
            AssertEx.DictionaryIs(missingIndividualRequirements,
                (guid1, new string[] { "C", "D" }),
                (guid2, new string[] { "C" }));
        }

        [TestMethod]
        public void TestAppliedOnlyWillBeSuperseded()
        {
            var (status, expiresAtUtc, missingRequirements, availableApplications, missingIndividualRequirements) =
                ApprovalCalculations.CalculateFamilyVolunteerRoleApprovalStatus("Role",
                new VolunteerFamilyRolePolicyVersion("v1", SupersededAtUtc: new DateTime(2022, 1, 10), requirements),
                utcNow: new DateTime(2022, 1, 5), family,
                Helpers.Completed(("A", 1)),
                Helpers.Exempted(),
                Helpers.CompletedIndividualRequirements(),
                Helpers.ExemptedIndividualRequirements(),
                Helpers.RemovedIndividualRoles((guid2, "Role")));

            Assert.AreEqual(RoleApprovalStatus.Prospective, status);
            Assert.AreEqual(null, expiresAtUtc);
            AssertEx.SequenceIs(missingRequirements, "B");
            AssertEx.SequenceIs(availableApplications);
            AssertEx.DictionaryIs(missingIndividualRequirements,
                (guid1, new string[] { "C", "D" }),
                (guid2, new string[] { "C" }));
        }

        [TestMethod]
        public void TestAppliedOnlyHasBeenSuperseded()
        {
            var (status, expiresAtUtc, missingRequirements, availableApplications, missingIndividualRequirements) =
                ApprovalCalculations.CalculateFamilyVolunteerRoleApprovalStatus("Role",
                new VolunteerFamilyRolePolicyVersion("v1", SupersededAtUtc: new DateTime(2022, 1, 10), requirements),
                utcNow: new DateTime(2022, 1, 20), family,
                Helpers.Completed(("A", 1)),
                Helpers.Exempted(),
                Helpers.CompletedIndividualRequirements(),
                Helpers.ExemptedIndividualRequirements(),
                Helpers.RemovedIndividualRoles((guid2, "Role")));

            Assert.AreEqual(RoleApprovalStatus.Prospective, status);
            Assert.AreEqual(null, expiresAtUtc);
            AssertEx.SequenceIs(missingRequirements, "B");
            AssertEx.SequenceIs(availableApplications);
            AssertEx.DictionaryIs(missingIndividualRequirements,
                (guid1, new string[] { "C", "D" }),
                (guid2, new string[] { "C" }));
        }

        [TestMethod]
        public void TestAppliedOnlyAfterSupersededDateWillBeSuperseded()
        {
            var (status, expiresAtUtc, missingRequirements, availableApplications, missingIndividualRequirements) =
                ApprovalCalculations.CalculateFamilyVolunteerRoleApprovalStatus("Role",
                new VolunteerFamilyRolePolicyVersion("v1", SupersededAtUtc: new DateTime(2022, 1, 10), requirements),
                utcNow: new DateTime(2022, 1, 5), family,
                Helpers.Completed(("A", 15)),
                Helpers.Exempted(),
                Helpers.CompletedIndividualRequirements(),
                Helpers.ExemptedIndividualRequirements(),
                Helpers.RemovedIndividualRoles((guid2, "Role")));

            Assert.AreEqual(null, status);
            Assert.AreEqual(null, expiresAtUtc);
            AssertEx.SequenceIs(missingRequirements);
            // Completing an application after the superseded date is not considered a valid completion for this policy version.
            // The superseded policy's requirements remain "available" for historical data entry purposes.
            AssertEx.SequenceIs(availableApplications, "A");
            AssertEx.DictionaryIs(missingIndividualRequirements);
        }

        [TestMethod]
        public void TestAppliedOnlyAfterSupersededDateHasBeenSuperseded()
        {
            var (status, expiresAtUtc, missingRequirements, availableApplications, missingIndividualRequirements) =
                ApprovalCalculations.CalculateFamilyVolunteerRoleApprovalStatus("Role",
                new VolunteerFamilyRolePolicyVersion("v1", SupersededAtUtc: new DateTime(2022, 1, 10), requirements),
                utcNow: new DateTime(2022, 1, 20), family,
                Helpers.Completed(("A", 15)),
                Helpers.Exempted(),
                Helpers.CompletedIndividualRequirements(),
                Helpers.ExemptedIndividualRequirements(),
                Helpers.RemovedIndividualRoles((guid2, "Role")));

            Assert.AreEqual(null, status);
            Assert.AreEqual(null, expiresAtUtc);
            AssertEx.SequenceIs(missingRequirements);
            // Completing an application after the superseded date is not considered a valid completion for this policy version.
            // The superseded policy's requirements remain "available" for historical data entry purposes.
            AssertEx.SequenceIs(availableApplications, "A");
            AssertEx.DictionaryIs(missingIndividualRequirements);
        }

        [TestMethod]
        public void TestPartiallyApprovedOnly()
        {
            var (status, expiresAtUtc, missingRequirements, availableApplications, missingIndividualRequirements) =
                ApprovalCalculations.CalculateFamilyVolunteerRoleApprovalStatus("Role",
                new VolunteerFamilyRolePolicyVersion("v1", SupersededAtUtc: null, requirements),
                utcNow: new DateTime(2022, 1, 20), family,
                Helpers.Completed(("A", 1), ("B", 2)),
                Helpers.Exempted(),
                Helpers.CompletedIndividualRequirements((guid1, "C", 3), (guid2, "C", 3)),
                Helpers.ExemptedIndividualRequirements(),
                Helpers.RemovedIndividualRoles((guid2, "Role")));

            Assert.AreEqual(RoleApprovalStatus.Prospective, status);
            Assert.AreEqual(null, expiresAtUtc);
            AssertEx.SequenceIs(missingRequirements);
            AssertEx.SequenceIs(availableApplications);
            AssertEx.DictionaryIs(missingIndividualRequirements,
                (guid1, new string[] { "D" }));
        }

        [TestMethod]
        public void TestPartiallyApprovedWillBeSuperseded()
        {
            var (status, expiresAtUtc, missingRequirements, availableApplications, missingIndividualRequirements) =
                ApprovalCalculations.CalculateFamilyVolunteerRoleApprovalStatus("Role",
                new VolunteerFamilyRolePolicyVersion("v1", SupersededAtUtc: new DateTime(2022, 1, 10), requirements),
                utcNow: new DateTime(2022, 1, 5), family,
                Helpers.Completed(("A", 1), ("B", 2)),
                Helpers.Exempted(),
                Helpers.CompletedIndividualRequirements((guid1, "C", 3), (guid2, "C", 3)),
                Helpers.ExemptedIndividualRequirements(),
                Helpers.RemovedIndividualRoles((guid2, "Role")));

            Assert.AreEqual(RoleApprovalStatus.Prospective, status);
            Assert.AreEqual(null, expiresAtUtc);
            AssertEx.SequenceIs(missingRequirements);
            AssertEx.SequenceIs(availableApplications);
            AssertEx.DictionaryIs(missingIndividualRequirements,
                (guid1, new string[] { "D" }));
        }

        [TestMethod]
        public void TestPartiallyApprovedHasBeenSuperseded()
        {
            var (status, expiresAtUtc, missingRequirements, availableApplications, missingIndividualRequirements) =
                ApprovalCalculations.CalculateFamilyVolunteerRoleApprovalStatus("Role",
                new VolunteerFamilyRolePolicyVersion("v1", SupersededAtUtc: new DateTime(2022, 1, 10), requirements),
                utcNow: new DateTime(2022, 1, 20), family,
                Helpers.Completed(("A", 1), ("B", 2)),
                Helpers.Exempted(),
                Helpers.CompletedIndividualRequirements((guid1, "C", 3), (guid2, "C", 3)),
                Helpers.ExemptedIndividualRequirements(),
                Helpers.RemovedIndividualRoles((guid2, "Role")));

            Assert.AreEqual(RoleApprovalStatus.Prospective, status);
            Assert.AreEqual(null, expiresAtUtc);
            AssertEx.SequenceIs(missingRequirements);
            AssertEx.SequenceIs(availableApplications);
            AssertEx.DictionaryIs(missingIndividualRequirements,
                (guid1, new string[] { "D" }));
        }

        [TestMethod]
        public void TestPartiallyApprovedAfterSupersededDateWillBeSuperseded()
        {
            var (status, expiresAtUtc, missingRequirements, availableApplications, missingIndividualRequirements) =
                ApprovalCalculations.CalculateFamilyVolunteerRoleApprovalStatus("Role",
                new VolunteerFamilyRolePolicyVersion("v1", SupersededAtUtc: new DateTime(2022, 1, 10), requirements),
                utcNow: new DateTime(2022, 1, 5), family,
                Helpers.Completed(("A", 1), ("B", 12)),
                Helpers.Exempted(),
                Helpers.CompletedIndividualRequirements((guid1, "C", 3), (guid2, "C", 13)),
                Helpers.ExemptedIndividualRequirements(),
                Helpers.RemovedIndividualRoles((guid2, "Role")));

            Assert.AreEqual(RoleApprovalStatus.Prospective, status);
            Assert.AreEqual(null, expiresAtUtc);
            // Requirements from superseded policies remain "available" for historical data entry purposes.
            AssertEx.SequenceIs(missingRequirements, "B");
            AssertEx.SequenceIs(availableApplications);
            AssertEx.DictionaryIs(missingIndividualRequirements,
                (guid1, new string[] { "D" }),
                (guid2, new string[] { "C" }));
        }

        [TestMethod]
        public void TestPartiallyApprovedAfterSupersededDateHasBeenSuperseded()
        {
            var (status, expiresAtUtc, missingRequirements, availableApplications, missingIndividualRequirements) =
                ApprovalCalculations.CalculateFamilyVolunteerRoleApprovalStatus("Role",
                new VolunteerFamilyRolePolicyVersion("v1", SupersededAtUtc: new DateTime(2022, 1, 10), requirements),
                utcNow: new DateTime(2022, 1, 20), family,
                Helpers.Completed(("A", 1), ("B", 12)),
                Helpers.Exempted(),
                Helpers.CompletedIndividualRequirements((guid1, "C", 3), (guid2, "C", 13)),
                Helpers.ExemptedIndividualRequirements(),
                Helpers.RemovedIndividualRoles((guid2, "Role")));

            Assert.AreEqual(RoleApprovalStatus.Prospective, status);
            Assert.AreEqual(null, expiresAtUtc);
            // Requirements from superseded policies remain "available" for historical data entry purposes.
            AssertEx.SequenceIs(missingRequirements, "B");
            AssertEx.SequenceIs(availableApplications);
            AssertEx.DictionaryIs(missingIndividualRequirements,
                (guid1, new string[] { "D" }),
                (guid2, new string[] { "C" }));
        }

        [TestMethod]
        public void TestApprovedOnly()
        {
            var (status, expiresAtUtc, missingRequirements, availableApplications, missingIndividualRequirements) =
                ApprovalCalculations.CalculateFamilyVolunteerRoleApprovalStatus("Role",
                new VolunteerFamilyRolePolicyVersion("v1", SupersededAtUtc: null, requirements),
                utcNow: new DateTime(2022, 1, 20), family,
                Helpers.Completed(("A", 1), ("B", 2)),
                Helpers.Exempted(),
                Helpers.CompletedIndividualRequirements((guid1, "C", 3), (guid1, "D", 4), (guid2, "C", 3)),
                Helpers.ExemptedIndividualRequirements(),
                Helpers.RemovedIndividualRoles((guid2, "Role")));

            Assert.AreEqual(RoleApprovalStatus.Approved, status);
            Assert.AreEqual(null, expiresAtUtc);
            AssertEx.SequenceIs(missingRequirements, "E");
            AssertEx.SequenceIs(availableApplications);
            AssertEx.DictionaryIs(missingIndividualRequirements,
                (guid1, new string[] { "F" }));
        }

        [TestMethod]
        public void TestNotApprovedOnlyWithoutRemovedRole()
        {
            var (status, expiresAtUtc, missingRequirements, availableApplications, missingIndividualRequirements) =
                ApprovalCalculations.CalculateFamilyVolunteerRoleApprovalStatus("Role",
                new VolunteerFamilyRolePolicyVersion("v1", SupersededAtUtc: null, requirements),
                utcNow: new DateTime(2022, 1, 20), family,
                Helpers.Completed(("A", 1), ("B", 2)),
                Helpers.Exempted(),
                Helpers.CompletedIndividualRequirements((guid1, "C", 3), (guid1, "D", 4), (guid2, "C", 3)),
                Helpers.ExemptedIndividualRequirements(),
                Helpers.RemovedIndividualRoles());

            Assert.AreEqual(RoleApprovalStatus.Prospective, status);
            Assert.AreEqual(null, expiresAtUtc);
            AssertEx.SequenceIs(missingRequirements);
            AssertEx.SequenceIs(availableApplications);
            AssertEx.DictionaryIs(missingIndividualRequirements,
                (guid2, new string[] { "D" }));
        }

        [TestMethod]
        public void TestApprovedOnlyWithoutRemovedRole()
        {
            var (status, expiresAtUtc, missingRequirements, availableApplications, missingIndividualRequirements) =
                ApprovalCalculations.CalculateFamilyVolunteerRoleApprovalStatus("Role",
                new VolunteerFamilyRolePolicyVersion("v1", SupersededAtUtc: null, requirements),
                utcNow: new DateTime(2022, 1, 20), family,
                Helpers.Completed(("A", 1), ("B", 2)),
                Helpers.Exempted(),
                Helpers.CompletedIndividualRequirements((guid1, "C", 3), (guid1, "D", 4), (guid2, "C", 3), (guid2, "D", 3)),
                Helpers.ExemptedIndividualRequirements(),
                Helpers.RemovedIndividualRoles());

            Assert.AreEqual(RoleApprovalStatus.Approved, status);
            Assert.AreEqual(null, expiresAtUtc);
            AssertEx.SequenceIs(missingRequirements, "E");
            AssertEx.SequenceIs(availableApplications);
            AssertEx.DictionaryIs(missingIndividualRequirements,
                (guid1, new string[] { "F" }),
                (guid2, new string[] { "F" }));
        }

        [TestMethod]
        public void TestApprovedOnlyByExemption()
        {
            var (status, expiresAtUtc, missingRequirements, availableApplications, missingIndividualRequirements) =
                ApprovalCalculations.CalculateFamilyVolunteerRoleApprovalStatus("Role",
                new VolunteerFamilyRolePolicyVersion("v1", SupersededAtUtc: null, requirements),
                utcNow: new DateTime(2022, 1, 20), family,
                Helpers.Completed(("A", 1)),
                Helpers.Exempted(("B", 30)),
                Helpers.CompletedIndividualRequirements((guid1, "C", 3), (guid1, "D", 4)),
                Helpers.ExemptedIndividualRequirements((guid2, "C", 30)),
                Helpers.RemovedIndividualRoles((guid2, "Role")));

            Assert.AreEqual(RoleApprovalStatus.Approved, status);
            Assert.AreEqual(new DateTime(2022, 1, 30), expiresAtUtc);
            AssertEx.SequenceIs(missingRequirements, "E");
            AssertEx.SequenceIs(availableApplications);
            AssertEx.DictionaryIs(missingIndividualRequirements,
                (guid1, new string[] { "F" }));
        }

        [TestMethod]
        public void TestApprovedOnlyByExemptionExpiring()
        {
            var (status, expiresAtUtc, missingRequirements, availableApplications, missingIndividualRequirements) =
                ApprovalCalculations.CalculateFamilyVolunteerRoleApprovalStatus("Role",
                new VolunteerFamilyRolePolicyVersion("v1", SupersededAtUtc: null, requirements),
                utcNow: new DateTime(2022, 1, 20), family,
                Helpers.CompletedWithExpiry(("A", 1, 28)),
                Helpers.Exempted(("B", 30)),
                Helpers.CompletedIndividualRequirementsWithExpiry((guid1, "C", 3, 29), (guid1, "D", 4, null)),
                Helpers.ExemptedIndividualRequirements((guid2, "C", 30)),
                Helpers.RemovedIndividualRoles((guid2, "Role")));

            Assert.AreEqual(RoleApprovalStatus.Approved, status);
            Assert.AreEqual(new DateTime(2022, 1, 28), expiresAtUtc);
            AssertEx.SequenceIs(missingRequirements, "E");
            AssertEx.SequenceIs(availableApplications);
            AssertEx.DictionaryIs(missingIndividualRequirements,
                (guid1, new string[] { "F" }));
        }

        [TestMethod]
        public void TestApprovedOnlyByExemptionExpiringEarlierIndividual()
        {
            var (status, expiresAtUtc, missingRequirements, availableApplications, missingIndividualRequirements) =
                ApprovalCalculations.CalculateFamilyVolunteerRoleApprovalStatus("Role",
                new VolunteerFamilyRolePolicyVersion("v1", SupersededAtUtc: null, requirements),
                utcNow: new DateTime(2022, 1, 20), family,
                Helpers.CompletedWithExpiry(("A", 1, 28)),
                Helpers.Exempted(("B", 30)),
                Helpers.CompletedIndividualRequirementsWithExpiry((guid1, "C", 3, 26), (guid1, "D", 4, null)),
                Helpers.ExemptedIndividualRequirements((guid2, "C", 30)),
                Helpers.RemovedIndividualRoles((guid2, "Role")));

            Assert.AreEqual(RoleApprovalStatus.Approved, status);
            Assert.AreEqual(new DateTime(2022, 1, 26), expiresAtUtc);
            AssertEx.SequenceIs(missingRequirements, "E");
            AssertEx.SequenceIs(availableApplications);
            AssertEx.DictionaryIs(missingIndividualRequirements,
                (guid1, new string[] { "F" }));
        }

        [TestMethod]
        public void TestNotApprovedBecauseExemptionExpired()
        {
            var (status, expiresAtUtc, missingRequirements, availableApplications, missingIndividualRequirements) =
                ApprovalCalculations.CalculateFamilyVolunteerRoleApprovalStatus("Role",
                new VolunteerFamilyRolePolicyVersion("v1", SupersededAtUtc: null, requirements),
                utcNow: new DateTime(2022, 1, 20), family,
                Helpers.Completed(("A", 1)),
                Helpers.Exempted(("B", 15)),
                Helpers.CompletedIndividualRequirements((guid1, "C", 3), (guid1, "D", 4)),
                Helpers.ExemptedIndividualRequirements((guid2, "C", 30)),
                Helpers.RemovedIndividualRoles((guid2, "Role")));

            Assert.AreEqual(RoleApprovalStatus.Prospective, status);
            Assert.AreEqual(null, expiresAtUtc);
            AssertEx.SequenceIs(missingRequirements, "B");
            AssertEx.SequenceIs(availableApplications);
            AssertEx.DictionaryIs(missingIndividualRequirements);
        }

        [TestMethod]
        public void TestNotApprovedBecauseExemptionExpiredExpired()
        {
            var (status, expiresAtUtc, missingRequirements, availableApplications, missingIndividualRequirements) =
                ApprovalCalculations.CalculateFamilyVolunteerRoleApprovalStatus("Role",
                new VolunteerFamilyRolePolicyVersion("v1", SupersededAtUtc: null, requirements),
                utcNow: new DateTime(2022, 1, 20), family,
                Helpers.CompletedWithExpiry(("A", 1, null)),
                Helpers.Exempted(("B", 15)),
                Helpers.CompletedIndividualRequirementsWithExpiry((guid1, "C", 3, 17), (guid1, "D", 4, 21)),
                Helpers.ExemptedIndividualRequirements((guid2, "C", 30)),
                Helpers.RemovedIndividualRoles((guid2, "Role")));

            Assert.AreEqual(RoleApprovalStatus.Prospective, status);
            Assert.AreEqual(null, expiresAtUtc);
            AssertEx.SequenceIs(missingRequirements, "B");
            AssertEx.SequenceIs(availableApplications);
            AssertEx.DictionaryIs(missingIndividualRequirements,
                (guid1, new string[] { "C" }));
        }

        [TestMethod]
        public void TestNotApprovedBecauseIndividualExemptionExpired()
        {
            var (status, expiresAtUtc, missingRequirements, availableApplications, missingIndividualRequirements) =
                ApprovalCalculations.CalculateFamilyVolunteerRoleApprovalStatus("Role",
                new VolunteerFamilyRolePolicyVersion("v1", SupersededAtUtc: null, requirements),
                utcNow: new DateTime(2022, 1, 20), family,
                Helpers.Completed(("A", 1)),
                Helpers.Exempted(("B", 30)),
                Helpers.CompletedIndividualRequirements((guid1, "C", 3), (guid1, "D", 4)),
                Helpers.ExemptedIndividualRequirements((guid2, "C", 15)),
                Helpers.RemovedIndividualRoles((guid2, "Role")));

            Assert.AreEqual(RoleApprovalStatus.Prospective, status);
            Assert.AreEqual(null, expiresAtUtc);
            AssertEx.SequenceIs(missingRequirements);
            AssertEx.SequenceIs(availableApplications);
            AssertEx.DictionaryIs(missingIndividualRequirements,
                (guid2, new string[] { "C" }));
        }

        [TestMethod]
        public void TestApprovedWillBeSuperseded()
        {
            var (status, expiresAtUtc, missingRequirements, availableApplications, missingIndividualRequirements) =
                ApprovalCalculations.CalculateFamilyVolunteerRoleApprovalStatus("Role",
                new VolunteerFamilyRolePolicyVersion("v1", SupersededAtUtc: new DateTime(2022, 1, 10), requirements),
                utcNow: new DateTime(2022, 1, 5), family,
                Helpers.Completed(("A", 1), ("B", 2)),
                Helpers.Exempted(),
                Helpers.CompletedIndividualRequirements((guid1, "C", 3), (guid1, "D", 4), (guid2, "C", 3)),
                Helpers.ExemptedIndividualRequirements(),
                Helpers.RemovedIndividualRoles((guid2, "Role")));

            Assert.AreEqual(RoleApprovalStatus.Approved, status);
            Assert.AreEqual(null, expiresAtUtc);
            AssertEx.SequenceIs(missingRequirements, "E");
            AssertEx.SequenceIs(availableApplications);
            AssertEx.DictionaryIs(missingIndividualRequirements,
                (guid1, new string[] { "F" }));
        }

        [TestMethod]
        public void TestApprovedHasBeenSuperseded()
        {
            var (status, expiresAtUtc, missingRequirements, availableApplications, missingIndividualRequirements) =
                ApprovalCalculations.CalculateFamilyVolunteerRoleApprovalStatus("Role",
                new VolunteerFamilyRolePolicyVersion("v1", SupersededAtUtc: new DateTime(2022, 1, 10), requirements),
                utcNow: new DateTime(2022, 1, 20), family,
                Helpers.Completed(("A", 1), ("B", 2)),
                Helpers.Exempted(),
                Helpers.CompletedIndividualRequirements((guid1, "C", 3), (guid1, "D", 4), (guid2, "C", 3)),
                Helpers.ExemptedIndividualRequirements(),
                Helpers.RemovedIndividualRoles((guid2, "Role")));

            Assert.AreEqual(RoleApprovalStatus.Approved, status);
            Assert.AreEqual(null, expiresAtUtc);
            AssertEx.SequenceIs(missingRequirements, "E");
            AssertEx.SequenceIs(availableApplications);
            AssertEx.DictionaryIs(missingIndividualRequirements,
                (guid1, new string[] { "F" }));
        }

        [TestMethod]
        public void TestApprovedAfterSupersededDateWillBeSuperseded()
        {
            var (status, expiresAtUtc, missingRequirements, availableApplications, missingIndividualRequirements) =
                ApprovalCalculations.CalculateFamilyVolunteerRoleApprovalStatus("Role",
                new VolunteerFamilyRolePolicyVersion("v1", SupersededAtUtc: new DateTime(2022, 1, 10), requirements),
                utcNow: new DateTime(2022, 1, 5), family,
                Helpers.Completed(("A", 1), ("B", 12)),
                Helpers.Exempted(),
                Helpers.CompletedIndividualRequirements((guid1, "C", 3), (guid1, "D", 4), (guid2, "C", 3)),
                Helpers.ExemptedIndividualRequirements(),
                Helpers.RemovedIndividualRoles((guid2, "Role")));

            Assert.AreEqual(RoleApprovalStatus.Prospective, status);
            Assert.AreEqual(null, expiresAtUtc);
            // Requirements from superseded policies remain "available" for historical data entry purposes.
            AssertEx.SequenceIs(missingRequirements, "B");
            AssertEx.SequenceIs(availableApplications);
            AssertEx.DictionaryIs(missingIndividualRequirements);
        }

        [TestMethod]
        public void TestApprovedIndividualAfterSupersededDateWillBeSuperseded()
        {
            var (status, expiresAtUtc, missingRequirements, availableApplications, missingIndividualRequirements) =
                ApprovalCalculations.CalculateFamilyVolunteerRoleApprovalStatus("Role",
                new VolunteerFamilyRolePolicyVersion("v1", SupersededAtUtc: new DateTime(2022, 1, 10), requirements),
                utcNow: new DateTime(2022, 1, 5), family,
                Helpers.Completed(("A", 1), ("B", 2)),
                Helpers.Exempted(),
                Helpers.CompletedIndividualRequirements((guid1, "C", 3), (guid1, "D", 14), (guid2, "C", 3)),
                Helpers.ExemptedIndividualRequirements(),
                Helpers.RemovedIndividualRoles((guid2, "Role")));

            Assert.AreEqual(RoleApprovalStatus.Prospective, status);
            Assert.AreEqual(null, expiresAtUtc);
            // Requirements from superseded policies remain "available" for historical data entry purposes.
            AssertEx.SequenceIs(missingRequirements);
            AssertEx.SequenceIs(availableApplications);
            AssertEx.DictionaryIs(missingIndividualRequirements,
                (guid1, new string[] { "D" }));
        }

        [TestMethod]
        public void TestApprovedAfterSupersededDateHasBeenSuperseded()
        {
            var (status, expiresAtUtc, missingRequirements, availableApplications, missingIndividualRequirements) =
                ApprovalCalculations.CalculateFamilyVolunteerRoleApprovalStatus("Role",
                new VolunteerFamilyRolePolicyVersion("v1", SupersededAtUtc: new DateTime(2022, 1, 10), requirements),
                utcNow: new DateTime(2022, 1, 20), family,
                Helpers.Completed(("A", 1), ("B", 12)),
                Helpers.Exempted(),
                Helpers.CompletedIndividualRequirements((guid1, "C", 3), (guid1, "D", 4), (guid2, "C", 3)),
                Helpers.ExemptedIndividualRequirements(),
                Helpers.RemovedIndividualRoles((guid2, "Role")));

            Assert.AreEqual(RoleApprovalStatus.Prospective, status);
            Assert.AreEqual(null, expiresAtUtc);
            // Requirements from superseded policies remain "available" for historical data entry purposes.
            AssertEx.SequenceIs(missingRequirements, "B");
            AssertEx.SequenceIs(availableApplications);
        }

        [TestMethod]
        public void TestOnboardedOnly()
        {
            var (status, expiresAtUtc, missingRequirements, availableApplications, missingIndividualRequirements) =
                ApprovalCalculations.CalculateFamilyVolunteerRoleApprovalStatus("Role",
                new VolunteerFamilyRolePolicyVersion("v1", SupersededAtUtc: null, requirements),
                utcNow: new DateTime(2022, 1, 20), family,
                Helpers.Completed(("A", 1), ("B", 2), ("E", 4)),
                Helpers.Exempted(),
                Helpers.CompletedIndividualRequirements((guid1, "C", 3), (guid1, "D", 4), (guid2, "C", 3), (guid1, "F", 4)),
                Helpers.ExemptedIndividualRequirements(),
                Helpers.RemovedIndividualRoles((guid2, "Role")));

            Assert.AreEqual(RoleApprovalStatus.Onboarded, status);
            Assert.AreEqual(null, expiresAtUtc);
            AssertEx.SequenceIs(missingRequirements);
            AssertEx.SequenceIs(availableApplications);
            AssertEx.DictionaryIs(missingIndividualRequirements);
        }

        [TestMethod]
        public void TestOnboardedOnlyByExemption()
        {
            var (status, expiresAtUtc, missingRequirements, availableApplications, missingIndividualRequirements) =
                ApprovalCalculations.CalculateFamilyVolunteerRoleApprovalStatus("Role",
                new VolunteerFamilyRolePolicyVersion("v1", SupersededAtUtc: null, requirements),
                utcNow: new DateTime(2022, 1, 20), family,
                Helpers.Completed(("A", 1), ("B", 2)),
                Helpers.Exempted(("E", null)),
                Helpers.CompletedIndividualRequirements((guid1, "C", 3), (guid1, "D", 4), (guid2, "C", 3)),
                Helpers.ExemptedIndividualRequirements((guid1, "F", 30)),
                Helpers.RemovedIndividualRoles((guid2, "Role")));

            Assert.AreEqual(RoleApprovalStatus.Onboarded, status);
            Assert.AreEqual(new DateTime(2022, 1, 30), expiresAtUtc);
            AssertEx.SequenceIs(missingRequirements);
            AssertEx.SequenceIs(availableApplications);
            AssertEx.DictionaryIs(missingIndividualRequirements);
        }

        [TestMethod]
        public void TestNotOnboardedBecauseExemptionExpired()
        {
            var (status, expiresAtUtc, missingRequirements, availableApplications, missingIndividualRequirements) =
                ApprovalCalculations.CalculateFamilyVolunteerRoleApprovalStatus("Role",
                new VolunteerFamilyRolePolicyVersion("v1", SupersededAtUtc: null, requirements),
                utcNow: new DateTime(2022, 1, 20), family,
                Helpers.Completed(("A", 1), ("B", 2)),
                Helpers.Exempted(("E", null)),
                Helpers.CompletedIndividualRequirements((guid1, "C", 3), (guid1, "D", 4), (guid2, "C", 3)),
                Helpers.ExemptedIndividualRequirements((guid1, "F", 10)),
                Helpers.RemovedIndividualRoles((guid2, "Role")));

            Assert.AreEqual(RoleApprovalStatus.Approved, status);
            Assert.AreEqual(null, expiresAtUtc);
            AssertEx.SequenceIs(missingRequirements);
            AssertEx.SequenceIs(availableApplications);
            AssertEx.DictionaryIs(missingIndividualRequirements,
                (guid1, new string[] { "F" }));
        }

        [TestMethod]
        public void TestOnboardedWillBeSuperseded()
        {
            var (status, expiresAtUtc, missingRequirements, availableApplications, missingIndividualRequirements) =
                ApprovalCalculations.CalculateFamilyVolunteerRoleApprovalStatus("Role",
                new VolunteerFamilyRolePolicyVersion("v1", SupersededAtUtc: new DateTime(2022, 1, 10), requirements),
                utcNow: new DateTime(2022, 1, 5), family,
                Helpers.Completed(("A", 1), ("B", 2), ("E", 4)),
                Helpers.Exempted(),
                Helpers.CompletedIndividualRequirements((guid1, "C", 3), (guid1, "D", 4), (guid2, "C", 3), (guid1, "F", 4)),
                Helpers.ExemptedIndividualRequirements(),
                Helpers.RemovedIndividualRoles((guid2, "Role")));

            Assert.AreEqual(RoleApprovalStatus.Onboarded, status);
            Assert.AreEqual(null, expiresAtUtc);
            AssertEx.SequenceIs(missingRequirements);
            AssertEx.SequenceIs(availableApplications);
            AssertEx.DictionaryIs(missingIndividualRequirements);
        }

        [TestMethod]
        public void TestOnboardedHasBeenSuperseded()
        {
            var (status, expiresAtUtc, missingRequirements, availableApplications, missingIndividualRequirements) =
                ApprovalCalculations.CalculateFamilyVolunteerRoleApprovalStatus("Role",
                new VolunteerFamilyRolePolicyVersion("v1", SupersededAtUtc: new DateTime(2022, 1, 10), requirements),
                utcNow: new DateTime(2022, 1, 20), family,
                Helpers.Completed(("A", 1), ("B", 2), ("E", 4)),
                Helpers.Exempted(),
                Helpers.CompletedIndividualRequirements((guid1, "C", 3), (guid1, "D", 4), (guid2, "C", 3), (guid1, "F", 4)),
                Helpers.ExemptedIndividualRequirements(),
                Helpers.RemovedIndividualRoles((guid2, "Role")));

            Assert.AreEqual(RoleApprovalStatus.Onboarded, status);
            Assert.AreEqual(null, expiresAtUtc);
            AssertEx.SequenceIs(missingRequirements);
            AssertEx.SequenceIs(availableApplications);
            AssertEx.DictionaryIs(missingIndividualRequirements);
        }

        [TestMethod]
        public void TestOnboardedAfterSupersededDateWillBeSuperseded()
        {
            var (status, expiresAtUtc, missingRequirements, availableApplications, missingIndividualRequirements) =
                ApprovalCalculations.CalculateFamilyVolunteerRoleApprovalStatus("Role",
                new VolunteerFamilyRolePolicyVersion("v1", SupersededAtUtc: new DateTime(2022, 1, 10), requirements),
                utcNow: new DateTime(2022, 1, 5), family,
                Helpers.Completed(("A", 1), ("B", 2), ("E", 14)),
                Helpers.Exempted(),
                Helpers.CompletedIndividualRequirements((guid1, "C", 3), (guid1, "D", 4), (guid2, "C", 3), (guid1, "F", 4)),
                Helpers.ExemptedIndividualRequirements(),
                Helpers.RemovedIndividualRoles((guid2, "Role")));

            Assert.AreEqual(RoleApprovalStatus.Approved, status);
            Assert.AreEqual(null, expiresAtUtc);
            // Requirements from superseded policies remain "available" for historical data entry purposes.
            AssertEx.SequenceIs(missingRequirements, "E");
            AssertEx.SequenceIs(availableApplications);
            AssertEx.DictionaryIs(missingIndividualRequirements);
        }

        [TestMethod]
        public void TestOnboardedAfterSupersededDateHasBeenSuperseded()
        {
            var (status, expiresAtUtc, missingRequirements, availableApplications, missingIndividualRequirements) =
                ApprovalCalculations.CalculateFamilyVolunteerRoleApprovalStatus("Role",
                new VolunteerFamilyRolePolicyVersion("v1", SupersededAtUtc: new DateTime(2022, 1, 10), requirements),
                utcNow: new DateTime(2022, 1, 20), family,
                Helpers.Completed(("A", 1), ("B", 2), ("E", 14)),
                Helpers.Exempted(),
                Helpers.CompletedIndividualRequirements((guid1, "C", 3), (guid1, "D", 4), (guid2, "C", 3), (guid1, "F", 4)),
                Helpers.ExemptedIndividualRequirements(),
                Helpers.RemovedIndividualRoles((guid2, "Role")));

            Assert.AreEqual(RoleApprovalStatus.Approved, status);
            Assert.AreEqual(null, expiresAtUtc);
            // Requirements from superseded policies remain "available" for historical data entry purposes.
            AssertEx.SequenceIs(missingRequirements, "E");
            AssertEx.SequenceIs(availableApplications);
            AssertEx.DictionaryIs(missingIndividualRequirements);
        }
    }
}
