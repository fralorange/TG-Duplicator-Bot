using FreelanceBotBase.Bot.Commands.Forward;
using FreelanceBotBase.Bot.Commands.Interface;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;

namespace FreelanceBotBase.Bot.Commands.Factory
{
    public class CommandFactory
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IConfiguration _configuration;

        public CommandFactory(ITelegramBotClient botClient, IConfiguration configuration)
        {
            _botClient = botClient;
            _configuration = configuration;
        }

        public ICommand CreateCommand(string commandName)
        {
            return commandName switch
            {
                _ => new ForwardCommand(_botClient, _configuration)
            };
        }
    }
}
