using Kolpojontro.Reg.Core.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kolpojontro.Reg.Core.Service
{
    public interface IAccountService
    {
        Task<IdentityResult> RegisterAsync(ApplicationUser applicationUser);
        Task<bool> SignInAsync();
        Task<bool> SignOutAsync();
        Task<bool> ChangePasswordAsync();
    }
}
