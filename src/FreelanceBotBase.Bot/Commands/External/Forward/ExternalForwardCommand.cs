using FreelanceBotBase.Bot.Commands.Interface;
using FreelanceBotBase.Bot.Helpers;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Telegram.Bot.Types;

namespace FreelanceBotBase.Bot.Commands.External.Forward
{
    public class ExternalForwardCommand : IExternalCommand
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _botToken;
        private readonly Dictionary<string, string> _categories;

        public ExternalForwardCommand(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _botToken = configuration.GetSection("BotConfiguration")["BotToken"]!; ;
            _categories = CategoryHelper.GetCategories(configuration);
        }

        public async Task<int[]> ExecuteAsync(List<Message> messages, CancellationToken cancellationToken)
        {
            var ids = new List<int>();

            var hasCaptionEntityValues = messages.Any(msg => msg.CaptionEntityValues is not null);

            if (!hasCaptionEntityValues)
            {
                return [];
            }

            var captionEntityValues = messages
                .Where(msg => msg?.CaptionEntityValues != null)
                .SelectMany(msg => msg.CaptionEntityValues!)
                .Distinct()
                .ToList();

            var httpClient = _httpClientFactory.CreateClient("telegram_bot_client");
            var apiUrl = $"https://api.telegram.org/bot{_botToken}/forwardMessages";

            var fromChatId = messages.First().Chat.Id;
            var messageIds = messages.Select(msg => msg.MessageId).ToArray();

            foreach (var category in _categories.Where(c => captionEntityValues.Contains(c.Key)))
            {
                string json = JsonConvert.SerializeObject(new
                {
                    chat_id = category.Value,
                    from_chat_id = fromChatId,
                    message_ids = messageIds
                });

                HttpContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var contentJson = await response.Content.ReadAsStringAsync(cancellationToken);
                    var jObject = JObject.Parse(json);
                    JArray msgIds = (JArray)jObject["message_ids"]!;
                    ids.AddRange(msgIds.ToObject<int[]>()!);
                }
            }

            return ids.ToArray();
        }
    }
}
