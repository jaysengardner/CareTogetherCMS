﻿using CareTogether.Managers;
using CareTogether.Resources;
using System;
using System.Collections.Immutable;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CareTogether.Engines
{
    public sealed record VolunteerFamilyApprovalStatus(
        ImmutableDictionary<string, ImmutableList<RoleVersionApproval>> FamilyRoleApprovals,
        ImmutableList<string> MissingFamilyRequirements,
        ImmutableList<string> AvailableFamilyApplications,
        ImmutableDictionary<Guid, VolunteerApprovalStatus> IndividualVolunteers);

    public sealed record VolunteerApprovalStatus(
        ImmutableDictionary<string, ImmutableList<RoleVersionApproval>> IndividualRoleApprovals,
        ImmutableList<string> MissingIndividualRequirements,
        ImmutableList<string> AvailableIndividualApplications);

    public sealed record RoleVersionApproval(string Version, RoleApprovalStatus ApprovalStatus);

    public interface IPolicyEvaluationEngine
    {
        Task<bool> AuthorizeReferralCommandAsync(Guid organizationId, Guid locationId,
            ClaimsPrincipal user, ReferralCommand command, Referral referral);

        Task<bool> AuthorizeArrangementCommandAsync(Guid organizationId, Guid locationId,
            ClaimsPrincipal user, ArrangementCommand command, Referral referral);

        Task<bool> AuthorizeArrangementNoteCommandAsync(Guid organizationId, Guid locationId,
            ClaimsPrincipal user, ArrangementNoteCommand command, Referral referral);

        Task<bool> AuthorizeVolunteerFamilyCommandAsync(Guid organizationId, Guid locationId,
            ClaimsPrincipal user, VolunteerFamilyCommand command, VolunteerFamily volunteerFamily);

        Task<bool> AuthorizeVolunteerCommandAsync(Guid organizationId, Guid locationId,
            ClaimsPrincipal user, VolunteerCommand command, VolunteerFamily volunteerFamily);


        Task<VolunteerFamilyApprovalStatus> CalculateVolunteerFamilyApprovalStatusAsync(
            Guid organizationId, Guid locationId, Family family,
            ImmutableList<CompletedRequirementInfo> completedFamilyRequirements,
            ImmutableDictionary<Guid, ImmutableList<CompletedRequirementInfo>> completedIndividualRequirements);


        Task<Referral> DiscloseReferralAsync(ClaimsPrincipal user, Referral referral);

        Task<Arrangement> DiscloseArrangementAsync(ClaimsPrincipal user, Arrangement arrangement);

        Task<VolunteerFamily> DiscloseVolunteerFamilyAsync(ClaimsPrincipal user, VolunteerFamily volunteerFamily);

        Task<Family> DiscloseFamilyAsync(ClaimsPrincipal user, Family family);

        Task<Person> DisclosePersonAsync(ClaimsPrincipal user, Person person);
    }
}
