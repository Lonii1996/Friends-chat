using System.Collections.ObjectModel;
using System.Net.Http.Json;
using ChatClient.DTOs;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChatClient;

public partial class ChatPage : ContentPage
{
    private readonly int _currentUserId;
    private readonly string _currentUsername;
    private readonly string _token;

    private static string BaseAddress = DeviceInfo.Platform == DevicePlatform.Android


    ? "http://10.0.2.2:8080"   
    : "http://localhost:8080";  

    private readonly string HttpUrl = $"{BaseAddress}/api/messages";
    private readonly string HubUrl = $"{BaseAddress}/chat";
    private HubConnection _hubConnection;
    private readonly HttpClient _httpClient;


    private bool _isLoadingHistory = false;

    
    private int? _topMessageCursorId = null;

    public ObservableCollection<MessageViewModel> Messages { get; set; } = new();

    public ChatPage(int userId, string username, string token)
    {
        InitializeComponent();

        _currentUserId = userId;
        _currentUsername = username;
        _token = token;
        _httpClient = new HttpClient();

        MessagesCollectionView.ItemsSource = Messages;

        
        MessagesCollectionView.Scrolled += OnMessagesScrolled;

        InitializeChatAsync();
    }

    private async void InitializeChatAsync()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(HubUrl, options => { options.AccessTokenProvider = () => Task.FromResult(_token)!; })
            .WithAutomaticReconnect()
            .AddMessagePackProtocol()
            .Build();

        _hubConnection.On<MessageDto>("ReceiveMessage", (msg) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Messages.Add(new MessageViewModel
                {
                    Username = msg.UserName,
                    Text = msg.Text,
                    SentAt = msg.SentAt
                });

              
                if (Messages.Count > 0)
                    MessagesCollectionView.ScrollTo(Messages.Count - 1);
            });
        });

        try
        {
            await _hubConnection.StartAsync();

          
            await LoadHistoryAsync(cursorMessageId: null);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка чата", $"Не удалось подключиться: {ex.Message}", "ОК");
        }
    }


    private async Task LoadHistoryAsync(int? cursorMessageId)
    {
        if (_isLoadingHistory) return;
        _isLoadingHistory = true;

        try
        {
           
            var url = $"{HttpUrl}?count=20";
            if (cursorMessageId.HasValue)
            {
                url += $"&cursorMessageId={cursorMessageId.Value}";
            }

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var history = await response.Content.ReadFromJsonAsync<List<MessageDto>>();

                if (history != null && history.Count > 0)
                {
                  
                    for (int i = history.Count - 1; i >= 0; i--)
                    {
                        var msg = history[i];
                        Messages.Insert(0, new MessageViewModel
                        {
                            Username = msg.UserName,
                            Text = msg.Text,
                            SentAt = msg.SentAt
                        });
                    }

                
                    _topMessageCursorId = history[0].Id;

                   
                    if (cursorMessageId == null)
                    {
                        if (Messages.Count > 0)
                            MessagesCollectionView.ScrollTo(Messages.Count - 1, animate: false);
                    }
                    else
                    {
                        MessagesCollectionView.ScrollTo(history.Count, animate: false);
                    }
                }
            }
            else
            {
                await DisplayAlert("Ошибка истории", $"Статус: {response.StatusCode}", "ОК");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка сети", $"Не удалось загрузить историю: {ex.Message}", "ОК");
        }
        finally
        {
            _isLoadingHistory = false;
        }
    }


    private async void OnMessagesScrolled(object? sender, ItemsViewScrolledEventArgs e)
    {
        if (e.FirstVisibleItemIndex == 0 && !_isLoadingHistory && _topMessageCursorId.HasValue)
        {
            await LoadHistoryAsync(_topMessageCursorId);
        }
    }

    private async void OnSendClicked(object sender, EventArgs e)
    {
        var text = MessageEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(text)) return;

        try
        {
            var messageInputDto = new MessageInputDto
            {
                Text = text
            };



            await _hubConnection.InvokeAsync("SendMessage", messageInputDto);
            MessageEntry.Text = string.Empty;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка отправки", $"Не удалось отправить: {ex.Message}", "ОК");
        }
    }
}


public class MessageViewModel
{
    public string Username { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public string LocalTimeText => SentAt.ToLocalTime().ToString("HH:mm");
}