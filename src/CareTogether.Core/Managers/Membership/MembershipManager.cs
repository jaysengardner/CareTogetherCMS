﻿using CareTogether.Engines.Authorization;
using CareTogether.Managers.Records;
using CareTogether.Resources.Accounts;
using CareTogether.Resources.Directory;
using CareTogether.Resources.Policies;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CareTogether.Managers.Membership
{
    public sealed class MembershipManager : IMembershipManager
    {
        private readonly IAccountsResource accountsResource;
        private readonly IAuthorizationEngine authorizationEngine;
        private readonly IDirectoryResource directoryResource;
        private readonly IPoliciesResource policiesResource;
        private readonly CombinedFamilyInfoFormatter combinedFamilyInfoFormatter;


        public MembershipManager(IAccountsResource accountsResource,
            IAuthorizationEngine authorizationEngine, IDirectoryResource directoryResource,
            IPoliciesResource policiesResource, CombinedFamilyInfoFormatter combinedFamilyInfoFormatter)
        {
            this.accountsResource = accountsResource;
            this.authorizationEngine = authorizationEngine;
            this.directoryResource = directoryResource;
            this.policiesResource = policiesResource;
            this.combinedFamilyInfoFormatter = combinedFamilyInfoFormatter;
        }


        public async Task<UserAccess> GetUserAccessAsync(ClaimsPrincipal user)
        {
            var account = await accountsResource.TryGetUserAccountAsync(user.UserId());
            if (account == null)
                return new UserAccess(user.UserId(), ImmutableList<UserOrganizationAccess>.Empty);

            var organizationsAccess = await Task.WhenAll(account.Organizations.Select(async organization =>
            {
                var organizationId = organization.OrganizationId;
                
                var locationsAccess = await Task.WhenAll(organization.Locations
                    .Select(async location =>
                    {
                        var locationId = location.LocationId;

                        var globalContextPermissions = await authorizationEngine.AuthorizeUserAccessAsync(
                            organizationId, locationId, user, new GlobalAuthorizationContext());
                        var allVolunteerFamiliesContextPermissions = await authorizationEngine.AuthorizeUserAccessAsync(
                            organizationId, locationId, user, new AllVolunteerFamiliesAuthorizationContext());
                        var allPartneringFamiliesContextPermissions = await authorizationEngine.AuthorizeUserAccessAsync(
                            organizationId, locationId, user, new AllPartneringFamiliesAuthorizationContext());

                        return new UserLocationAccess(locationId, location.PersonId, location.Roles,
                            globalContextPermissions,
                            allVolunteerFamiliesContextPermissions,
                            allPartneringFamiliesContextPermissions);
                    }));

                return new UserOrganizationAccess(organizationId, locationsAccess.ToImmutableList());
            }));

            return new UserAccess(user.UserId(), organizationsAccess.ToImmutableList());
        }
        
        public async Task<FamilyRecordsAggregate> ChangePersonRolesAsync(ClaimsPrincipal user,
            Guid organizationId, Guid locationId, Guid personId, ImmutableList<string> roles)
        {
            var command = new ChangePersonRoles(personId, roles);

            if (!await authorizationEngine.AuthorizePersonAccessCommandAsync(
                organizationId, locationId, user, command))
                throw new Exception("The user is not authorized to perform this command.");

            var personFamily = await directoryResource.FindPersonFamilyAsync(
                organizationId, locationId, personId);

            //NOTE: This invariant could be revisited, but that would split 'Person' and 'Family' into separate aggregates.
            if (personFamily == null)
                throw new Exception("CareTogether currently assumes that all people should (always) belong to a family record.");
            
            var result = await accountsResource.ExecutePersonAccessCommandAsync(
                organizationId, locationId, command, user.UserId());

            var familyResult = await combinedFamilyInfoFormatter.RenderCombinedFamilyInfoAsync(
                organizationId, locationId, personFamily.Id, user);

            return new FamilyRecordsAggregate(familyResult);
        }

        public async Task<byte[]> GenerateUserInviteNonceAsync(ClaimsPrincipal user,
            Guid organizationId, Guid locationId, Guid personId)
        {
            if (!await authorizationEngine.AuthorizeGenerateUserInviteNonceAsync(
                organizationId, locationId, user))
                throw new Exception("The user is not authorized to perform this action.");

            var result = await accountsResource.GenerateUserInviteNonceAsync(
                organizationId, locationId, personId, user.UserId());
            
            return result;
        }

        public async Task<UserInviteReviewInfo?> TryReviewUserInviteNonceAsync(ClaimsPrincipal user, Guid organizationId, Guid locationId,
            byte[] nonce)
        {
            if (user.PersonId(organizationId, locationId) != null)
                throw new Exception("The user is already linked to a person in this organization and location.");

            var locationAccess = await accountsResource.TryLookupUserInviteNoncePersonIdAsync(
                organizationId, locationId, nonce);
            
            if (locationAccess == null)
                return null;
            
            var family = await directoryResource.FindPersonFamilyAsync(organizationId, locationId, locationAccess.PersonId);
            if (family == null)
                return null;

            var person = family.Adults.Single(adult => adult.Item1.Id == locationAccess.PersonId).Item1;

            var configuration = await policiesResource.GetConfigurationAsync(organizationId);

            return new UserInviteReviewInfo(organizationId, configuration.OrganizationName,
                locationId, configuration.Locations.Single(loc => loc.Id == locationId).Name,
                locationAccess.PersonId, person.FirstName, person.LastName,
                locationAccess.Roles);
        }

        public async Task<Account?> TryRedeemUserInviteNonceAsync(ClaimsPrincipal user,
            Guid organizationId, Guid locationId, byte[] nonce)
        {
            var result = await accountsResource.TryRedeemUserInviteNonceAsync(
                organizationId, locationId, user.UserId(), nonce);
            
            return result;
        }
    }
}
