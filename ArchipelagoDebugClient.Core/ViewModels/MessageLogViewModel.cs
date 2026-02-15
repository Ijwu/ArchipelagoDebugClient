using System.Collections.ObjectModel;
using System.Reactive;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using ArchipelagoDebugClient.Models;
using ArchipelagoDebugClient.Services;
using Avalonia.Threading;
using ReactiveUI;

namespace ArchipelagoDebugClient.ViewModels;

public class MessageLogViewModel : ViewModelBase
{
    public ObservableCollection<BindableMessage> Messages { get; } = [];

    private string _chatMessage = "";
    public string ChatMessage
    {
        get => _chatMessage;
        set => this.RaiseAndSetIfChanged(ref _chatMessage, value);
    }

    public ReactiveCommand<Unit, Unit> SendMessageCommand { get; }

    public MessageLogViewModel(SessionProvider sessionProvider) : base(sessionProvider)
    {
        SendMessageCommand = ReactiveCommand.Create(SendMessage, this.WhenAnyValue(x => x.HasSession, (bool hasSession) => hasSession));

        sessionProvider.OnSessionChanged += OnSessionChanged;
    }

    private void OnSessionChanged(ArchipelagoSession? session)
    {
        if (session != null)
        {
            session.MessageLog.OnMessageReceived += OnMessageRecieved;
        }
        else
        {
            Messages.Clear();
        }
    }

    private void OnMessageRecieved(LogMessage message)
    {
        Dispatcher.UIThread.Invoke(() => Messages.Add(new BindableMessage(message)));
    }

    private void SendMessage()
    {
        string? message = string.IsNullOrEmpty(ChatMessage) ? null : ChatMessage;
        if (message is not null)
        {
            Session!.Say(message);
        }
        ChatMessage = string.Empty;
    }
}
