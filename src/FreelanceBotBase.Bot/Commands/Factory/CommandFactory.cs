using FreelanceBotBase.Bot.Commands.External.Forward;
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
        private readonly IHttpClientFactory _httpClientFactory;

        public CommandFactory(ITelegramBotClient botClient, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _botClient = botClient;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public ICommand CreateCommand(string commandName)
        {
            return commandName switch
            {
                _ => new ForwardCommand(_botClient, _configuration)
            };
        }

        public IExternalCommand CreateExternalCommand(string commandName)
        {
            return commandName switch
            {
                _ => new ExternalForwardCommand(_httpClientFactory, _configuration)
            };
        }
    }
}
