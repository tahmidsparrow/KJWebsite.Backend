using Kolpojontro.Reg.Core.Repositories;
using Kolpojontro.Reg.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kolpojontro.Reg.Repositories
{
    public class AwaitingUserRepository : IAwaitingUserRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public AwaitingUserRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<AwaitingUser> CreateUserAsync(AwaitingUser awaitingUser)
        {
            if (awaitingUser == null)
                throw new NullReferenceException("User is null");

            var result = _applicationDbContext.AwaitingUsers.Add(awaitingUser);
            await _applicationDbContext.SaveChangesAsync();

            return result.Entity;
        }
    }
}
