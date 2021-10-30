﻿using CareTogether.Managers;
using CareTogether.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CareTogether.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("/api/{organizationId:guid}/{locationId:guid}/[controller]")]
    public class VolunteersController : ControllerBase
    {
        private readonly IApprovalManager approvalManager;

        public VolunteersController(IApprovalManager approvalManager)
        {
            this.approvalManager = approvalManager;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<VolunteerFamily>>> ListAllVolunteerFamiliesAsync(Guid organizationId, Guid locationId)
        {
            var referrals = await approvalManager.ListVolunteerFamiliesAsync(User, organizationId, locationId);

            return Ok(referrals);
        }

        [HttpPost("volunteerFamilyCommand")]
        public async Task<ActionResult<VolunteerFamily>> SubmitVolunteerFamilyCommandAsync(Guid organizationId, Guid locationId,
            [FromBody] VolunteerFamilyCommand command)
        {
            var result = await approvalManager.ExecuteVolunteerFamilyCommandAsync(organizationId, locationId, User, command);
            return result;
        }

        [HttpPost("volunteerCommand")]
        public async Task<ActionResult<VolunteerFamily>> SubmitVolunteerCommandAsync(Guid organizationId, Guid locationId,
            [FromBody] VolunteerCommand command)
        {
            var result = await approvalManager.ExecuteVolunteerCommandAsync(organizationId, locationId, User, command);
            return result;
        }
    }
}
