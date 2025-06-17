using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ripplee.Models;
using Ripplee.Services.Interfaces;
using System.Collections.ObjectModel; // Для ObservableCollection
using System.Diagnostics;

namespace Ripplee.ViewModels
{
    // QueryProperties для получения данных о собеседнике и чате
    [QueryProperty(nameof(CompanionName), "name")]
    [QueryProperty(nameof(CompanionAvatarUrl), "avatarUrl")]
    [QueryProperty(nameof(ChatCity), "city")]     // Город, по которому искали/нашлись
    [QueryProperty(nameof(ChatTopic), "topic")]   // Тема, по которой искали/нашлись
    // callGroupId будет управляться через SignalRService

    public partial class TextChatViewModel : ObservableObject
    {
        private readonly ISignalRService _signalRService;
        private readonly IUserService _userService; // Для получения данных текущего пользователя
        private readonly IDispatcher _dispatcher;

        [ObservableProperty] private string? companionName;
        [ObservableProperty] private string? companionAvatarUrl;
        [ObservableProperty] private string? chatCity;
        [ObservableProperty] private string? chatTopic;

        [ObservableProperty] private string? currentMessageText;
        public ObservableCollection<ChatMessageModel> Messages { get; } = new();

        [ObservableProperty] private bool isCompanionMuted; // Если понадобится статус мьюта собеседника

        private bool _isClosing = false;
        private bool _currentUserInitiatedEndCall = false;


        public TextChatViewModel(ISignalRService signalRService, IUserService userService, IDispatcher dispatcher)
        {
            _signalRService = signalRService;
            _userService = userService;
            _dispatcher = dispatcher;
        }

        [RelayCommand]
        private async Task PageAppearing()
        {
            _isClosing = false;
            _currentUserInitiatedEndCall = false;
            IsCompanionMuted = false; // Если будем отображать
            Messages.Clear(); // Очищаем старые сообщения при открытии

            Debug.WriteLine($"TextChatViewModel Appearing: CompanionName={CompanionName}, AvatarUrl={CompanionAvatarUrl}");

            // Подключаемся к SignalR и регистрируем все нужные обработчики
            await _signalRService.ConnectAsync(
                OnCompanionFound_NotInChat, // Этот коллбэк не должен вызываться, т.к. мы уже в чате
                OnSearchStatus_NotInChat,   // Аналогично
                HandleCallEndedByPartner,   // Обработчик завершения звонка/чата
                HandlePartnerMuteStatusChanged, // Обработчик статуса мьюта собеседника
                HandleReceiveTextMessage    // <--- Обработчик получения текстового сообщения
            );

            if (!_signalRService.IsConnected)
            {
                await _dispatcher.DispatchAsync(async () => {
                    await Shell.Current.DisplayAlert("Ошибка связи", "Потеряно соединение с сервером чата.", "OK");
                });
                await CloseChatPage();
            }
            // Если нужно, можно отправить здесь какой-то "приветственный" статус или информацию
            // о том, что текущий пользователь подключился к чату (хотя сервер это уже знает по группе)
        }

        [RelayCommand]
        private async Task PageDisappearing()
        {
            Debug.WriteLine($"TextChatViewModel: PageDisappearing. IsClosing: {_isClosing}, CurrentUserInitiated: {_currentUserInitiatedEndCall}");
            if (!_isClosing && !_currentUserInitiatedEndCall)
            {
                Debug.WriteLine("TextChatViewModel: Page disappearing without explicit EndCall by this user, notifying partner.");
                await _signalRService.NotifyEndCallAsync();
            }
            if (!_isClosing) { await CloseChatPage(); }
        }

        private async Task CloseChatPage()
        {
            if (_isClosing) return;
            _isClosing = true;
            await _signalRService.LeaveCurrentCallGroupAsync();
            await _dispatcher.DispatchAsync(async () => {
                if (Shell.Current.CurrentPage is Views.TextChatPage)
                { // Убедись, что имя страницы верное
                    await Shell.Current.GoToAsync("//MainPage", true);
                }
            });
        }

        // Обработчики для SignalR, которые не должны срабатывать на этой странице
        private Task OnCompanionFound_NotInChat(string n, string c, string t, string id, string? avatar) { Debug.WriteLine("TextChatVM: Unexpected OnCompanionFound"); return Task.CompletedTask; }
        private Task OnSearchStatus_NotInChat(string s) { Debug.WriteLine("TextChatVM: Unexpected OnSearchStatus"); return Task.CompletedTask; }


        private async Task HandleCallEndedByPartner()
        {
            Debug.WriteLine($"TextChatViewModel: HandleCallEndedByPartner. _currentUserInitiatedEndCall: {_currentUserInitiatedEndCall}");
            if (!_currentUserInitiatedEndCall)
            {
                await _dispatcher.DispatchAsync(async () => {
                    await Shell.Current.DisplayAlert("Чат завершен", "Ваш собеседник покинул чат.", "OK");
                });
                await CloseChatPage();
            }
        }
        private Task HandlePartnerMuteStatusChanged(bool partnerIsMuted)
        {
            Debug.WriteLine($"TextChatViewModel: HandlePartnerMuteStatusChanged: {partnerIsMuted}");
            _dispatcher.Dispatch(() => IsCompanionMuted = partnerIsMuted);
            // Можно отобразить иконку мьюта у собеседника, если это нужно в текстовом чате
            return Task.CompletedTask;
        }

        private Task HandleReceiveTextMessage(string senderUsername, string messageText, string? senderAvatarUrlFromServer)
        {
            _dispatcher.Dispatch(() =>
            {
                string? fullAvatarUrl = null;
                if (!string.IsNullOrEmpty(senderAvatarUrlFromServer) && senderAvatarUrlFromServer.StartsWith("/avatars/"))
                {
                    string baseAddress = MauiProgram.GetApiBaseAdress();
                    fullAvatarUrl = baseAddress.TrimEnd('/') + senderAvatarUrlFromServer;
                }

                Messages.Add(new ChatMessageModel(messageText, MessageSenderType.Companion, senderUsername, fullAvatarUrl));
                // Прокрутка к последнему сообщению будет реализована в XAML или code-behind
            });
            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(CurrentMessageText) || !_signalRService.IsConnected)
            {
                return;
            }

            var messageToSend = CurrentMessageText;
            CurrentMessageText = string.Empty; // Очищаем поле ввода сразу

            // Получаем URL аватара текущего пользователя
            string? currentUserAvatar = _userService.CurrentUser.AvatarUrl;
            // Если AvatarUrl хранится как полный URL, нам нужен относительный для отправки на сервер,
            // или сервер должен быть готов принять полный URL и извлечь из него относительный путь.
            // Предположим, что сервер ожидает относительный путь /avatars/... или пустую строку.
            string relativeAvatarUrl = string.Empty;
            if (!string.IsNullOrEmpty(currentUserAvatar))
            {
                Uri uri = new Uri(currentUserAvatar);
                relativeAvatarUrl = uri.AbsolutePath; // Получаем "/avatars/файл.jpg"
            }


            // Добавляем сообщение в свой список сразу для мгновенного отображения
            Messages.Add(new ChatMessageModel(messageToSend, MessageSenderType.CurrentUser, _userService.CurrentUser.Username, currentUserAvatar));

            await _signalRService.SendTextMessageAsync(messageToSend, relativeAvatarUrl);
        }

        [RelayCommand]
        private async Task EndChat() // Аналог EndCall
        {
            if (_isClosing) return;
            _currentUserInitiatedEndCall = true;
            await _signalRService.NotifyEndCallAsync();
            await CloseChatPage();
        }
    }
}