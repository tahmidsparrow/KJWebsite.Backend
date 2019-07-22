using Kolpojontro.Reg.Core.ApiResources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kolpojontro.Reg.Core
{
    public interface IAwaitingUserService
    {
        Task<AwaitingUser> CreateUserAsync(AwaitingUserApiResource user);
        Task<AwaitingUserApiResource> GetUserByIdAsync(string Id);
        Task<List<AwaitingUserApiResource>> GetAwaitingUsers();
    }
}
