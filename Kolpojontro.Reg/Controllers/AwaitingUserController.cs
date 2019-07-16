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

namespace Kolpojontro.Reg.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AwaitingUserController : ControllerBase
    {
        private readonly IAwaitingUserService _awaitingService;

        public AwaitingUserController(IAwaitingUserService awaitingService)
        {
            _awaitingService = awaitingService;
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

        [HttpGet]
        public async Task<List<AwaitingUserApiResource>> All()
        {
            var result = await _awaitingService.GetAwaitingUsers();
            if(result != null)
            {
                return result;
            }

            throw new ApplicationException("Error occured while fetching data");
        }
    }
}