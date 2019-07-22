using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kolpojontro.Reg.Core;
using Kolpojontro.Reg.Core.ApiResources;
using Kolpojontro.Reg.Core.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RiskFirst.Hateoas;
using RiskFirst.Hateoas.Models;

namespace Kolpojontro.Reg.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AwaitingUserController : ControllerBase
    {
        private readonly IAwaitingUserService _awaitingService;
        private readonly ILinksService _linksService;

        public AwaitingUserController(IAwaitingUserService awaitingService,
                                        ILinksService linksService)
        {
            _awaitingService = awaitingService;
            _linksService = linksService;
        }

        [HttpPost]
        public async Task<object> Create([FromBody] AwaitingUserApiResource model) {
            if (ModelState.IsValid)
            {
                try {
                    var result = await _awaitingService.CreateUserAsync(model);
                    return model;
                }catch(Exception e)
                {
                    Console.Write(e.Message);
                    throw new ApplicationException();
                }
            }

            throw new ApplicationException("Invalid Model");
        }

        [HttpGet("{Id}", Name = "GetModelByIdRoute")]
        public async Task<AwaitingUserApiResource> Get(string Id)
        {
            var awaitingUser = await _awaitingService.GetUserByIdAsync(Id);

            if (awaitingUser != null)
            {
                //await _linksService.AddLinksAsync(awaitingUser);
                return awaitingUser;
            }

            throw new ApplicationException("Awaiting User Not Found");
        }

        [HttpGet(Name = "GetAllModelsRoute")]
        public async Task<ItemsLinkContainer<AwaitingUserApiResource>> All()
        {
            var awaitingUsers = await _awaitingService.GetAwaitingUsers();


            if (awaitingUsers != null)
            {
                var results = new ItemsLinkContainer<AwaitingUserApiResource>()
                {
                    Items = awaitingUsers
                };
                await _linksService.AddLinksAsync(results);
                return results;
            }

            throw new ApplicationException("Error occured while fetching data");
        }
    }
}