using FreelanceBotBase.Bot.Commands.Base;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FreelanceBotBase.Bot.Commands.Forward
{
    public class ForwardCommand : CommandBase
    {
        private readonly IConfiguration _configuration;
        private readonly Dictionary<string, string> _categories;

        public ForwardCommand(ITelegramBotClient botClient, IConfiguration configuration) : base(botClient)
        {
            _configuration = configuration;
            _categories = GetCategories();
        }

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

        private Dictionary<string, string> GetCategories()
        {
            var categories = new Dictionary<string, string>();
            var categorySection = _configuration.GetSection("Categories");
            foreach (IConfigurationSection section in categorySection.GetChildren())
            {
                categories.Add(section["Name"]!, section["Channel"]!);
            }
            return categories;
        }
    }
}
