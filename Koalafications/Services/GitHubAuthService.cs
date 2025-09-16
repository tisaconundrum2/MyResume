using System.Net.Http.Json;

public class GitHubAuthService
{
    private readonly HttpClient _http;

    public GitHubAuthService(HttpClient http)
    {
        _http = http;
        _http.DefaultRequestHeaders.Add("Accept", "application/json");
        _http.DefaultRequestHeaders.Add("User-Agent", "BlazorDeviceFlowDemo"); // GitHub requires UA
    }

    public async Task<GitHubDeviceFlowResponse> StartDeviceFlow(string clientId, string scope = "repo")
    {
        var response = await _http.PostAsync(
            "https://github.com/login/device/code",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "scope", scope }
            })
        );

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GitHubDeviceFlowResponse>();
    }

    public async Task<string> PollForToken(string clientId, string deviceCode, int interval)
    {
        int currentInterval = interval;

        while (true)
        {
            await Task.Delay(currentInterval * 1000);

            var response = await _http.PostAsync(
                "https://github.com/login/oauth/access_token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "client_id", clientId },
                    { "device_code", deviceCode },
                    { "grant_type", "urn:ietf:params:oauth:grant-type:device_code" }
                })
            );

            var result = await response.Content.ReadFromJsonAsync<GitHubTokenResponse>();

            if (!string.IsNullOrEmpty(result.access_token))
            {
                return result.access_token;
            }

            switch (result.error)
            {
                case "authorization_pending":
                    // User hasn't approved yet → keep polling
                    break;
                case "slow_down":
                    // Increase poll interval (spec says add ~5s)
                    currentInterval += 5;
                    break;
                case "expired_token":
                    throw new Exception("Device code expired. Please restart login.");
                case "access_denied":
                    throw new Exception("User denied access.");
                default:
                    throw new Exception("OAuth failed: " + result.error_description);
            }
        }
    }
}