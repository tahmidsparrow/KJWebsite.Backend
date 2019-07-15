using AutoMapper;
using Kolpojontro.Reg.Core;
using Kolpojontro.Reg.Core.ApiResources;
using Kolpojontro.Reg.Core.Models;
using Kolpojontro.Reg.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kolpojontro.Reg.Services
{
    public class AwaitingUserService : IAwaitingUserService
    {
        private readonly IAwaitingUserRepository _awaitingUserRepository;
        private readonly IMapper _mapper;

        public AwaitingUserService(IAwaitingUserRepository awaitingUserRepository, IMapper mapper)
        {
            _awaitingUserRepository = awaitingUserRepository;
            _mapper = mapper;
        }

        public async Task<AwaitingUser> CreateUserAsync(AwaitingUserApiResource usermodel)
        {
            if (usermodel == null)
                throw new NullReferenceException("Invalid Awaiting User");

            var awaitingUser = _mapper.Map<AwaitingUser>(usermodel);
            awaitingUser.Status = EAwaitingUserStatus.Awaiting.ToString();
            //awaitingUser.DOB = new DateTime(Convert.ToInt64(awaitingUser.DateOfBirth));
            awaitingUser.StatusLastUpdatedAt = DateTime.Now;

            var response = await _awaitingUserRepository.CreateUserAsync(awaitingUser);

            return response;
        }
    }
}
