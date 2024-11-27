using System.IdentityModel.Tokens.Jwt;

namespace RMS_API.Helper
{
    public class UserHelper
    {
        public static int GetUserIdFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var claims = jwtToken.Claims;
            var userId = claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            return int.Parse(userId);
        }
    }
}
