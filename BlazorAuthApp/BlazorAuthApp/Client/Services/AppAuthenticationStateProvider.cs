using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using BlazorAuthApp.Shared;

namespace BlazorAuthApp.Client.Services
{
    public class AppAuthenticationStateProvider : AuthenticationStateProvider
    {
        private static readonly TimeSpan UserCacheRefreshInterval
        = TimeSpan.FromSeconds(60);

        private readonly HttpClient _client;
        private readonly ILogger<AppAuthenticationStateProvider> _logger;

        private DateTimeOffset _userLastCheck = DateTimeOffset.FromUnixTimeSeconds(0);
        private ClaimsPrincipal _cachedUser = new ClaimsPrincipal(new ClaimsIdentity());

        public AppAuthenticationStateProvider(
            HttpClient client,
            ILogger<AppAuthenticationStateProvider> logger)
        {
            _client = client;
            _logger = logger;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return new AuthenticationState(await GetUser());
        }

        private async ValueTask<ClaimsPrincipal> GetUser(bool useCache = true)
        {
            var now = DateTimeOffset.Now;
            if (useCache && now < _userLastCheck + UserCacheRefreshInterval)
            {
                _logger.LogDebug("Taking user from cache");
                return _cachedUser;
            }

            _logger.LogDebug("Fetching user");
            _cachedUser = await FetchUser();
            _userLastCheck = now;

            return _cachedUser;
        }

        private async Task<ClaimsPrincipal> FetchUser()
        {
            try
            {
                _logger.LogInformation("Fetching user information.");                

                var response = await _client.GetAsync("/api/SignIn");

                if (response.StatusCode == HttpStatusCode.OK)
                {
                   
                    var claimsString = await response.Content.ReadFromJsonAsync<List<ClaimRecord>>();
          
                    var claims = new List<Claim>();
                    if (claimsString.Count == 0)
                    {
                        var anonymous = new ClaimsIdentity();
                        return new ClaimsPrincipal(anonymous);
                    }

                    foreach (var item in claimsString)
                    {
                        claims.Add(new Claim(item.Type, item.Value));
                    }

                    var identity = new ClaimsIdentity(claims, "Cookies");
                    foreach (var claim in claims)
                    {
                        identity.AddClaim(new Claim(claim.Type, claim.Value.ToString()));
                    }

                    return new ClaimsPrincipal(identity);
                }
                else
                {
                    var anonymous = new ClaimsIdentity();
                    return new ClaimsPrincipal(anonymous);
                }
            }            
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Fetching user failed.");
            }

            return new ClaimsPrincipal(new ClaimsIdentity());
        }

       
    }
}
