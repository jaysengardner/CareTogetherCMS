﻿using CareTogether.Engines;
using CareTogether.Resources;
using Nito.AsyncEx;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CareTogether.Managers
{
    public sealed class ApprovalManager : IApprovalManager
    {
        private readonly IApprovalsResource approvalsResource;
        private readonly IAuthorizationEngine authorizationEngine;
        private readonly IPolicyEvaluationEngine policyEvaluationEngine;
        private readonly IDirectoryResource directoryResource;


        public ApprovalManager(IApprovalsResource approvalsResource, IPolicyEvaluationEngine policyEvaluationEngine,
            IDirectoryResource directoryResource, IAuthorizationEngine authorizationEngine)
        {
            this.approvalsResource = approvalsResource;
            this.policyEvaluationEngine = policyEvaluationEngine;
            this.directoryResource = directoryResource;
            this.authorizationEngine = authorizationEngine;
        }


        public async Task<ImmutableList<CombinedFamilyInfo>> ListVolunteerFamiliesAsync(ClaimsPrincipal user, Guid organizationId, Guid locationId)
        {
            var families = (await directoryResource.ListFamiliesAsync(organizationId, locationId)).ToImmutableDictionary(x => x.Id);
            var volunteerFamilies = await approvalsResource.ListVolunteerFamiliesAsync(organizationId, locationId);

            var result = await volunteerFamilies.Select(vf => ToVolunteerFamilyAsync(
                organizationId, locationId, vf, families)).WhenAll();
            return result.ToImmutableList();
        }

        public async Task<CombinedFamilyInfo> ExecuteVolunteerFamilyCommandAsync(Guid organizationId, Guid locationId,
            ClaimsPrincipal user, VolunteerFamilyCommand command)
        {
            var volunteerFamilyEntry = await approvalsResource.GetVolunteerFamilyAsync(organizationId, locationId, command.FamilyId);
            
            var families = directoryResource.ListFamiliesAsync(organizationId, locationId).Result.ToImmutableDictionary(x => x.Id);
            var referral = await ToVolunteerFamilyAsync(
                organizationId, locationId, volunteerFamilyEntry, families);

            var authorizationResult = await authorizationEngine.AuthorizeVolunteerFamilyCommandAsync(
                organizationId, locationId, user, command, referral);
            
            var volunteerFamily = await approvalsResource.ExecuteVolunteerFamilyCommandAsync(organizationId, locationId, command, user.UserId());
                
            var disclosedVolunteerFamily = await authorizationEngine.DiscloseVolunteerFamilyInfoAsync(user,
                await ToVolunteerFamilyAsync(organizationId, locationId, volunteerFamily, families));
            return disclosedVolunteerFamily;
        }

        public async Task<CombinedFamilyInfo> ExecuteVolunteerCommandAsync(Guid organizationId, Guid locationId,
            ClaimsPrincipal user, VolunteerCommand command)
        {
            var volunteerFamilyEntry = await approvalsResource.GetVolunteerFamilyAsync(organizationId, locationId, command.FamilyId);
            
            var families = directoryResource.ListFamiliesAsync(organizationId, locationId).Result.ToImmutableDictionary(x => x.Id);
            var referral = await ToVolunteerFamilyAsync(
                organizationId, locationId, volunteerFamilyEntry, families);

            var authorizationResult = await authorizationEngine.AuthorizeVolunteerCommandAsync(
                organizationId, locationId, user, command, referral);
            
            var volunteerFamily = await approvalsResource.ExecuteVolunteerCommandAsync(organizationId, locationId, command, user.UserId());
            
            var disclosedVolunteerFamily = await authorizationEngine.DiscloseVolunteerFamilyInfoAsync(user,
                await ToVolunteerFamilyAsync(organizationId, locationId, volunteerFamily, families));
            return disclosedVolunteerFamily;
        }
        

        private async Task<VolunteerFamily> ToVolunteerFamilyAsync(Guid organizationId, Guid locationId,
            VolunteerFamilyEntry entry,
            ImmutableDictionary<Guid, Family> families)
        {
            var family = families[entry.FamilyId];
            var completedIndividualRequirements = entry.IndividualEntries.ToImmutableDictionary(
                x => x.Key,
                x => x.Value.CompletedRequirements);
            var removedIndividualRoles = entry.IndividualEntries.ToImmutableDictionary(
                x => x.Key,
                x => x.Value.RemovedRoles);

            var volunteerFamilyApprovalStatus = await policyEvaluationEngine.CalculateVolunteerFamilyApprovalStatusAsync(
                organizationId, locationId, family, entry.CompletedRequirements, entry.RemovedRoles,
                completedIndividualRequirements, removedIndividualRoles);

            return new VolunteerFamily(family,
                entry.CompletedRequirements, entry.UploadedDocuments, entry.RemovedRoles,
                volunteerFamilyApprovalStatus.MissingFamilyRequirements,
                volunteerFamilyApprovalStatus.AvailableFamilyApplications,
                volunteerFamilyApprovalStatus.FamilyRoleApprovals,
                volunteerFamilyApprovalStatus.IndividualVolunteers.ToImmutableDictionary(
                    x => x.Key,
                    x =>
                    {
                        var hasEntry = entry.IndividualEntries.TryGetValue(x.Key, out var individualEntry);
                        var result = hasEntry
                            ? new Volunteer(individualEntry!.CompletedRequirements, individualEntry!.RemovedRoles,
                                x.Value.MissingIndividualRequirements, x.Value.AvailableIndividualApplications, x.Value.IndividualRoleApprovals)
                            : new Volunteer(ImmutableList<CompletedRequirementInfo>.Empty, ImmutableList<RemovedRole>.Empty,
                                x.Value.MissingIndividualRequirements, x.Value.AvailableIndividualApplications, x.Value.IndividualRoleApprovals);
                        return result;
                    }));
        }
    }
}
