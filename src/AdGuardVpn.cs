using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AdGuardVpnApi
{
    public class AdGuardVpn
    {
        private readonly HttpClient httpClient;
        private readonly string apiUrl = "https://api.adguard.io";
        private readonly string authApiUrl = "https://auth.adguard-vpn.com";
        private readonly string applicationId;
        private string accessToken;
        public AdGuardVpn()
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("AdGuardVpn/2.3.100 (Linux; U; Android 9; RMX3551 Build/PQ3A.190705.003)");
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            applicationId = GenerateApplicationId(16);
        }
        private string GenerateApplicationId(int length)
        {
            const string characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var bytes = new byte[length];
            using var randomNumbers = RandomNumberGenerator.Create();
            randomNumbers.GetBytes(bytes);
            var result = new StringBuilder(length);
            foreach (var b in bytes)
                result.Append(characters[b % characters.Length]);
            return result.ToString();
        }

        public async Task<string> UserLookup(string email)
        {
            var data = new StringContent(
                $"request_id=adguard-android&email={email}", Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await httpClient.PostAsync($"{authApiUrl}/api/1.0/user_lookup", data);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> Register(string email, string password)
        {
            var data = new StringContent(
                $"password={password}&product=VPN&clientId=adguard-vpn-android&marketingConsent=false&source=VPN_APPLICATION&applicationId={applicationId}&email={email}",
                Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await httpClient.PostAsync($"{authApiUrl}/api/2.0/registration", data);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> Login(string email, string password)
        {
            var data = new StringContent(
                $"password={password}&grant_type=password_2fa&scope=trust&source=VPN_APPLICATION&client_id=adguard-vpn-android&username={email}",
                Encoding.UTF8, "application/x-www-form-urlencoded");
            try
            {
                var response = await httpClient.PostAsync($"{authApiUrl}/oauth/token", data);
                var responseContent = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseContent);
                if (doc.RootElement.TryGetProperty("access_token", out var tokenElement))
                {
                    accessToken = tokenElement.GetString();
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }
                return responseContent;
            }
            catch (Exception exception) {
                return exception.Message;
            }
        }

        public async Task<string> GetAccountSettings()
        {
            var response = await httpClient.GetAsync($"{apiUrl}/account/api/1.0/account/settings");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetLicenses()
        {
            var response = await httpClient.GetAsync($"{apiUrl}/account/api/1.0/products/licenses/vpn.json");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetExclusionServices()
        {
            var response = await httpClient.GetAsync($"{apiUrl}/api/v1/exclusion_services");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetBonuses()
        {
            var response = await httpClient.GetAsync($"{apiUrl}/account/api/1.0/vpn/bonuses");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetLocations()
        {
            var response = await httpClient.GetAsync(
                $"{apiUrl}/api/v2/locations/ANDROID?app_id={applicationId}&token={accessToken}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> VerifyUrl(string verificationUrl)
        {
            var response = await httpClient.GetAsync(verificationUrl);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
