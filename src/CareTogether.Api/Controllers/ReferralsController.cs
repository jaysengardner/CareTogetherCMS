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
    public class ReferralsController : ControllerBase
    {
        private readonly IReferralManager referralManager;

        public ReferralsController(IReferralManager referralManager)
        {
            this.referralManager = referralManager;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Referral>>> ListAllReferralsAsync(Guid organizationId, Guid locationId)
        {
            var referrals = await referralManager.ListReferralsAsync(organizationId, locationId);

            return Ok(referrals);
        }

        [HttpPost("referralCommand")]
        public async Task<ActionResult<Referral>> SubmitReferralCommandAsync(Guid organizationId, Guid locationId,
            [FromBody] ReferralCommand command)
        {
            var result = await referralManager.ExecuteReferralCommandAsync(organizationId, locationId, User, command);
            return result;
        }

        [HttpPost("arrangementCommand")]
        public async Task<ActionResult<Referral>> SubmitArrangementCommandAsync(Guid organizationId, Guid locationId,
            [FromBody] ArrangementCommand command)
        {
            var result = await referralManager.ExecuteArrangementCommandAsync(organizationId, locationId, User, command);
            return result;
        }

        [HttpPost("arrangementNoteCommand")]
        public async Task<ActionResult<Referral>> SubmitArrangementNoteCommandAsync(Guid organizationId, Guid locationId,
            [FromBody] ArrangementNoteCommand command)
        {
            var result = await referralManager.ExecuteArrangementNoteCommandAsync(organizationId, locationId, User, command);
            return result;
        }
    }
}
