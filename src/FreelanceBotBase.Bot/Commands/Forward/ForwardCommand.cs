using FreelanceBotBase.Bot.Commands.Base;
using FreelanceBotBase.Bot.Helpers;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FreelanceBotBase.Bot.Commands.Forward
{
    public class ForwardCommand : CommandBase
    {
        private readonly Dictionary<string, string> _categories;

        public ForwardCommand(ITelegramBotClient botClient, IConfiguration configuration) : base(botClient)
            => _categories = CategoryHelper.GetCategories(configuration);

        public override async Task<Message> ExecuteAsync(Message message, CancellationToken cancellationToken)
        {
            Message lastSentMessage = new();
            var hashtags = message.CaptionEntityValues;

            if (hashtags is null)
                return lastSentMessage;

            foreach (var category in _categories)
            {
                if (hashtags.Contains(category.Key))
                {
                    lastSentMessage = await BotClient.ForwardMessageAsync(
                        chatId: category.Value,
                        fromChatId: message.Chat.Id,
                        messageId: message.MessageId,
                        cancellationToken: cancellationToken);
                }
            }
            return lastSentMessage;
        }
    }
}
