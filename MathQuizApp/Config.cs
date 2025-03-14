using System.Text.Json;
using System.IO;

namespace MathQuizApp
{
    public partial class MathQuizApp
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
            public int MaxTimeToAnswer { get; set; } = 30;
            public int IntervalMin { get; set; } = 3600;
            public int IntervalMax { get; set; } = 10080;
            public Quizes Quizes { get; set; } = new();
        }

        public class Quizes
        {
            public List<Math> Math { get; set; } = new();
        }

        public class Math
        {
            public string Difficulty { get; set; } = "Easy";
            public int Reward { get; set; } = 0;
            public int MinNumber { get; set; } = 1;
            public int MaxNumber { get; set; } = 50;
        }
    }
}
