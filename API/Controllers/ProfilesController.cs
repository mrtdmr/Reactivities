using Application.Profiles;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    public class ProfilesController:BaseController
    {
        [HttpGet("{username}")]
        public async Task<ActionResult<Profile>> Get(string userName) {
            return await Mediator.Send(new Details.Query { UserName = userName });
        }
        [HttpGet("{username}/activities")]
        public async Task<ActionResult<List<UserActivityDto>>> GetUserActivities(string userName,string predicate)
        {
            return await Mediator.Send(new ListActivities.Query { Username = userName, Predicate = predicate });
        }

    }
}
