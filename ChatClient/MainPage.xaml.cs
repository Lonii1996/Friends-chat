using ChatClient.DTOs;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace ChatClient;

public partial class MainPage : ContentPage
{

    private static string BaseAddress = DeviceInfo.Platform == DevicePlatform.Android

    ? "http://10.0.2.2:8080"
    : "http://localhost:8080";

    private readonly string BaseUrl = $"{BaseAddress}/api/auth/";

    private readonly HttpClient _httpClient;

    public MainPage()
    {
        InitializeComponent();

        
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
        };

        _httpClient = new HttpClient(handler);
        _httpClient.Timeout = TimeSpan.FromSeconds(3);

        CheckServerConnectionAsync();
    }

    private async void CheckServerConnectionAsync()
    {
        try
        {
            
            var response = await _httpClient.GetAsync(BaseUrl);

            
            StatusLabel.Text = "🟢 Подключено к серверу";
            StatusLabel.TextColor = Colors.Green;
        }
        catch (Exception)
        {
            
            StatusLabel.Text = "🔴 Нет связи с сервером (Проверьте бэкенд)";
            StatusLabel.TextColor = Colors.Red;
        }
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameEntry.Text) || string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            await DisplayAlert("Ошибка", "Заполните имя и пароль!", "ОК");
            return;
        }

        var loginData = new LoginDto
        {
            Name = NameEntry.Text.Trim(),
            Password = PasswordEntry.Text.Trim()
            

        };
        

        try
        {
            var response = await _httpClient.PostAsJsonAsync<LoginDto>($"{BaseUrl}login", loginData);
            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

            if (response.IsSuccessStatusCode && result != null)
            {
                
                Application.Current.MainPage = new NavigationPage(new ChatPage(result.UserId ?? 0, result.Username ?? "User", result.Token ?? ""));
            }
            else
            {
                await DisplayAlert("Ошибка", result?.Message ?? "Неверный логин или пароль", "ОК");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка сети", $"Не удалось связаться с сервером: {ex.Message}", "ОК");
        }
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameEntry.Text) ||
            string.IsNullOrWhiteSpace(PasswordEntry.Text))


        {
            await DisplayAlert("Ошибка", "Для регистрации заполните все поля, включая инвайт!", "ОК");
            return;
        }


        var registerData = new RegisterDto
        {
            Name = NameEntry.Text.Trim(),
            Password = PasswordEntry.Text.Trim(),
            
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync<RegisterDto>($"{BaseUrl}register", registerData);
            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

            if (response.IsSuccessStatusCode && result != null)
            {
                await DisplayAlert("Успешно!", "Вы зарегистрированы! Теперь введите данные и нажмите 'Войти'.", "ОК");
                
            }
            else
            {
                await DisplayAlert("Ошибка регистрации", result?.Message ?? "Что-то пошло не так", "ОК");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка сети", $"Не удалось связаться с сервером: {ex.Message}", "ОК");
        }
    }
}

public class AuthResponse
{
    public string Message { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string? Username { get; set; }
    
    public string? Token { get; set; }
}