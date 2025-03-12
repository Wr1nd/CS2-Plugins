using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API;
using System.Text.Json;

namespace MathQuizApp
{
    public partial class MathQuizApp : BasePlugin
    {
        public bool isActive = false;
        public int answer = 0;
        public Difficulty currentDiff;
        private Config _config = new();
        private readonly Random _random = new();
        public int MaxTimeToAnswer;
        public int IntervalMin;
        public int IntervalMax;
        private Dictionary<string, int> DifficultyRewards = new();
        public CounterStrikeSharp.API.Modules.Timers.Timer ChatTime;


        public override string ModuleName => "MathQuizApp";
        public override string ModuleAuthor => "Wr1nd";
        public override string ModuleVersion => "1.0.0";


        private readonly string _prefix = $"[ {ChatColors.DarkRed}ShmitzHakeriai {ChatColors.Default}]";

        public override void Load(bool hotReload)
        {
            _config = LoadConfig();
            for (int i = 0; i < _config.Quizes.Math.Length; i++)
            {
                DifficultyRewards.Add(_config.Quizes.Math[i].Difficulty, _config.Quizes.Math[i].Reward);
            }
            //get data from config
            MaxTimeToAnswer = _config.MaxTimeToAnswer;

            StartChatTimer();
        }

        private void StartChatTimer()
        {
            IntervalMin = _config.IntervalMin;
            IntervalMax = _config.IntervalMax;

            int interval = _random.Next(IntervalMin, IntervalMax);

            ChatTime = AddTimer(interval, () =>
            {
                Server.NextFrame(() =>
                {
                    Server.PrintToChatAll(_prefix + " " + GenerateEquation());
                });
            });
        }

        private string GenerateEquation()
        {
            isActive = true;
            StartActiveTimer();
            string equation = GenerateMathQuestion();

            return equation;
        }

        private void StartActiveTimer()
        {
            AddTimer(MaxTimeToAnswer, () =>
            {
                Server.NextFrame(() =>
                {
                        Server.PrintToChatAll(_prefix + "Quiz time has ended");
                        isActive = false;
                });
            });
        }

        private string GenerateMathQuestion()
        {
            Difficulty difficulty = (Difficulty)_random.Next(3); ; 

            string[] easyOps = { "+", "-" };
            string[] mediumOps = { "+", "-", "/" };
            string[] hardOps = { "%" }; 

            string operatorSymbol = difficulty switch
            {
                Difficulty.Easy => easyOps[_random.Next(easyOps.Length)],
                Difficulty.Medium => mediumOps[_random.Next(mediumOps.Length)],
                Difficulty.Hard => hardOps[_random.Next(hardOps.Length)],
                _ => "+"
            };

            int num1 = difficulty == Difficulty.Easy ? _random.Next(1, 51) : _random.Next(10, 101);
            int num2 = difficulty == Difficulty.Easy ? _random.Next(1, 51) : _random.Next(10, 101);

            if (operatorSymbol == "/")
            {
                num2 = _random.Next(2, 11); // Avoid zero and one
                num1 = num2 * _random.Next(2, 10); 
            }
            else if (operatorSymbol == "%")
            {
                num2 = _random.Next(6, 20); 
                num1 = _random.Next(50, 101); 
            }



            answer = CalculateAnswer(num1, num2, operatorSymbol);
            currentDiff = difficulty;
            return $"{num1} {operatorSymbol} {num2}";
        }

        private int CalculateAnswer(int num1, int num2, string operatorSymbol)
        {
            return operatorSymbol switch
            {
                "+" => num1 + num2,
                "-" => num1 - num2,
                "*" => num1 * num2,
                "/" => num1 / num2,
                "%" => num1 % num2, 
                _ => throw new InvalidOperationException("Invalid operator")
            };
        }
        
        [ConsoleCommand("css_ats")]
        public void OnAts(CCSPlayerController? player, CommandInfo info)
        {
            if (!isActive)
            {
                Server.PrintToChatAll(_prefix + " There is no ongoing QUIZ");
                return;
            }

            if (!int.TryParse(info.ArgString, out int playerAnswer))
            {
                player.PrintToChat(_prefix + " Invalid input. Please enter a valid number.");
                return;
            }

            if (playerAnswer == answer)
            {
                ChatTime.Kill();
                isActive = false;
                int playerReward = 0;
                if (DifficultyRewards.TryGetValue(currentDiff.ToString(), out int reward))
                {
                    playerReward = reward;
                }
                Server.PrintToChatAll(_prefix + $" Player {player.PlayerName} answered correctly!");
                Server.PrintToChatAll(_prefix + $" Reward for completinting {currentDiff} level: {playerReward} jewels");
                Server.PrintToChatAll(_prefix + " Quiz ended");
                StartChatTimer();
            }
            else
            {
                player.PrintToChat(_prefix + " Answer is not correct");
            }
        }

        [ConsoleCommand("css_config")]
        public void OnConfig(CCSPlayerController? player, CommandInfo info)
        {
            string configText = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
            foreach (string line in configText.Split('\n'))
            {
                Server.PrintToChatAll(_prefix + " " + line);
            }
        }

        public enum Difficulty { Easy, Medium, Hard }
    }
}
