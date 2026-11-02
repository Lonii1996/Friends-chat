using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ChatClient.Handlers
{
    internal class AuthHttpMessageHandler : DelegatingHandler
    {
        private static string BaseAddress = DeviceInfo.Platform == DevicePlatform.Android

        ? "http://10.0.2.2:8080"
        : "http://localhost:8080";



        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            
            var accessToken = await SecureStorage.Default.GetAsync("AccessToken");
            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

          
            var response = await base.SendAsync(request, cancellationToken);

        
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshToken = await SecureStorage.Default.GetAsync("RefreshToken");
                if (string.IsNullOrEmpty(refreshToken))
                {
                    return response; 
                }

              
                bool isRefreshed = await TryRefreshTokenAsync(refreshToken);

                if (isRefreshed)
                {
             
                    var newAccessToken = await SecureStorage.Default.GetAsync("AccessToken");

                   
                    var newRequest = CloneRequest(request);
                    newRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newAccessToken);

                    
                    return await base.SendAsync(newRequest, cancellationToken);
                }
                else
                {
                   
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        // Логика навигации на Login страницу (например, через Shell)
                        // Shell.Current.GoToAsync("//LoginPage");
                    });
                }
            }

            return response;
        }

        private async Task<bool> TryRefreshTokenAsync(string refreshToken)
        {
            try
            {
                using var client = new HttpClient();
                var payload = JsonSerializer.Serialize(new { RefreshToken = refreshToken });
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(BaseAddress, content);
                if (!response.IsSuccessStatusCode) return false;

                var responseString = await response.Content.ReadAsStringAsync();

                
                using var doc = JsonDocument.Parse(responseString);
                var root = doc.RootElement;

                if (root.GetProperty("isSuccess").GetBoolean())
                {
                  
                    var newAccess = root.GetProperty("token").GetString();
                    var newRefresh = root.GetProperty("refreshToken").GetString();

                  
                    await SecureStorage.Default.SetAsync("AccessToken", newAccess ?? "");
                    await SecureStorage.Default.SetAsync("RefreshToken", newRefresh ?? "");

                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private HttpRequestMessage CloneRequest(HttpRequestMessage req)
        {
            var clone = new HttpRequestMessage(req.Method, req.RequestUri);
            foreach (var header in req.Headers) clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            if (req.Content != null) clone.Content = req.Content; 
            return clone;
        }
    }
}

            
