public class GitHubDeviceFlowResponse
{
    public string device_code { get; set; }
    public string user_code { get; set; }
    public string verification_uri { get; set; }
    public int expires_in { get; set; }
    public int interval { get; set; }
}

public class GitHubTokenResponse
{
    public string access_token { get; set; }
    public string token_type { get; set; }
    public string scope { get; set; }

    // Error cases
    public string error { get; set; }
    public string error_description { get; set; }
}