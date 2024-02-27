using Microsoft.Extensions.Configuration;

namespace FreelanceBotBase.Bot.Helpers
{
    public static class CategoryHelper
    {
        public static Dictionary<string, string> GetCategories(IConfiguration configuration)
        {
            var categories = new Dictionary<string, string>();
            int i = 0;
            while (true)
            {
                var name = configuration[$"CATEGORIES_{i}_NAME"];
                var channel = configuration[$"CATEGORIES_{i}_CHANNEL"];
                if (name == null || channel == null)
                {
                    break;
                }
                categories.Add(name, channel);
                i++;
            }
            return categories;
        }
    }
}
