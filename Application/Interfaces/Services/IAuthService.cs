using Application.Dtos;
using Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<Response<AuthResultDto>> LoginAsync(LoginRequest dto);
        Task<Response<AuthResultDto>> RegisterAsync(RegisterRequest dto);
    }

}
