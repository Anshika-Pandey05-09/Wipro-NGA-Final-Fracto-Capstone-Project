using Fracto.Api.Models;

namespace Fracto.Api.Services
{
    public interface ITokenService
    {
        string CreateToken(AppUser user);
    }
}
