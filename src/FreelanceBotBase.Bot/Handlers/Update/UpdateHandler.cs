using FreelanceBotBase.Bot.Commands.Factory;
using FreelanceBotBase.Bot.Commands.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Timers;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

using Timer = System.Timers.Timer;

namespace FreelanceBotBase.Bot.Handlers.Update
{
    public class UpdateHandler : IUpdateHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<UpdateHandler> _logger;
        private readonly CommandFactory _commandFactory;
        private readonly IConfiguration _configuration;
        private readonly ConcurrentDictionary<string, List<Message>> _temporaryStorage;
        private readonly Timer _timer;

        public UpdateHandler(ITelegramBotClient botClient, ILogger<UpdateHandler> logger, CommandFactory commandFactory, IConfiguration configuration)
        {
            _botClient = botClient;
            _logger = logger;
            _commandFactory = commandFactory;
            _configuration = configuration;
            _temporaryStorage = new ConcurrentDictionary<string, List<Message>>();

            _timer = new(5000);
            _timer.Elapsed += async (sender, e) => await TimerOnElapsed(sender, e);
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
        {
            var handler = update switch
            {
                { ChannelPost:  { MediaGroupId: { } }  message} => BotOnMediaGroupReceived(message, cancellationToken),
                { ChannelPost: { } message } => BotOnMessageReceived(message, cancellationToken),
                { EditedChannelPost: { } message } => BotOnMessageReceived(message, cancellationToken),
                _ => UnknownUpdateHandlerAsync(update, cancellationToken)
            };

            await handler;
        }
        #region Bot Processors
        private async Task BotOnMessageReceived(Message message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Receive message type: {MessageType}", message.Type);
            if (message.Caption is not { } || IsChildChannel(string.Concat('@', message.Chat.Username!)))
                return;

            ICommand command = _commandFactory.CreateCommand("");

            Message sentMessage = await command.ExecuteAsync(message, cancellationToken);
            _logger.LogInformation("The message was sent with id: {SentMessageId}", sentMessage.MessageId);
        }

        private async Task BotOnMediaGroupReceived(Message message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Receive message type: {MessageType}", message.Type);
            if (message.Text is { } || IsChildChannel(string.Concat('@', message.Chat.Username!)))
                return;

            _timer.Stop();

            var mediaGroupId = message.MediaGroupId!;

            if (_temporaryStorage.TryGetValue(mediaGroupId, out List<Message>? value))
            {
                value.Add(message);
            } else
            {
                if (!_temporaryStorage.IsEmpty)
                {
                    await TimerOnElapsed(null, null);
                }

                _temporaryStorage[mediaGroupId] = [message];
            }
            _timer.Start();
        }

        private Task UnknownUpdateHandlerAsync(Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
            return Task.CompletedTask;
        }

        #endregion
        public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            _logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);

            // Cooldown in case of network troubles.
            if (exception is RequestException)
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }

        private bool IsChildChannel(string channelUsername)
        {
            var childChannels = _configuration.GetSection("Categories").GetChildren().Select(c => c["Channel"]).ToList();
            return childChannels.Contains(channelUsername);
        }

        private async Task TimerOnElapsed(object? sender, ElapsedEventArgs? e)
        {
            _timer.Stop();

            var mediaGroupId = _temporaryStorage.Keys.First();
            var messages = _temporaryStorage[_temporaryStorage.Keys.First()];
            _temporaryStorage.TryRemove(mediaGroupId, out _);
            var command = _commandFactory.CreateExternalCommand("");
            var cts = new CancellationTokenSource();
            var result = await command.ExecuteAsync(messages, cts.Token);
            
            foreach (var id in result)
            {
                _logger.LogInformation("The message was sent with id: {SentMessageId}", id);
            }
        }
    }
}
