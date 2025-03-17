using System.Text.Json;
using System.IO;

namespace hoursLimtation
{
    public partial class hoursLimtation
    {
        private Config CreateConfig(string configPath)
        {
            Config config = new();

            File.WriteAllText(configPath,
                JsonSerializer.Serialize(new Config(), new JsonSerializerOptions { WriteIndented = true }));

            return config;
        }

        private Config LoadConfig()
        {
            var configPath = Path.Combine(ModuleDirectory, "config.json");
            if (!File.Exists(configPath)) return CreateConfig(configPath);

            var config = JsonSerializer.Deserialize<Config>(File.ReadAllText(configPath))!;

            return config;
        }

        public class Config
        {
            public int MinHoursRequired { get; set; } = 30;
            public string ApiKey { get; set;  } = "your-api-key";
         
        }
    }
}
