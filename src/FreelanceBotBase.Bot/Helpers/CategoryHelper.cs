using Microsoft.Extensions.Configuration;

namespace FreelanceBotBase.Bot.Helpers
{
    public static class CategoryHelper
    {
        public static Dictionary<string, string> GetCategories(IConfiguration configuration)
        {
            var categories = new Dictionary<string, string>();
            var categorySection = configuration.GetSection("Categories");
            foreach (IConfigurationSection section in categorySection.GetChildren())
            {
                categories.Add(section["Name"]!, section["Channel"]!);
            }
            return categories;
        }
    }
}
