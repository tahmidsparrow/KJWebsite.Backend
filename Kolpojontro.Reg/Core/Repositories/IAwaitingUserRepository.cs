using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kolpojontro.Reg.Core.Repositories
{
    public interface IAwaitingUserRepository
    {
        Task<AwaitingUser> CreateUserAsync(AwaitingUser awaitingUser);
        Task<List<AwaitingUser>> GetAwaitingUsers();
    }
}
