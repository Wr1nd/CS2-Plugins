using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API;
using System;
using System.IO;
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


        public override string ModuleName => "MathQuizApp";
        public override string ModuleAuthor => "Wr1nd";
        public override string ModuleVersion => "1.0.0";

        private readonly string _prefix = $"[ {ChatColors.DarkRed}ShmitzHakeriai {ChatColors.Default}]";

        public override void Load(bool hotReload)
        {
            _config = LoadConfig();
            //get data from config
            StartChatTimer();
        }

        private void StartChatTimer()
        {
            AddTimer(10.0f, () =>
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
            AddTimer(30.0f, () =>
            {
                Server.NextFrame(() =>
                {
                    if (isActive)
                    {
                        Server.PrintToChatAll(_prefix + "Quiz time has ended");
                        isActive = false;
                    }
                });
                StartChatTimer();
            });
        }

        private string GenerateMathQuestion()
        {
            Difficulty difficulty = (Difficulty)_random.Next(3);
            string operatorSymbol = difficulty == Difficulty.Easy ? "+" : _random.Next(2) == 0 ? "-" : "*";
            int num1 = _random.Next(1, 101);
            int num2 = _random.Next(1, 101);

            if (operatorSymbol == "/")
            {
                num2 = _random.Next(1, 11); // Avoid zero, keep numbers small
                while (num1 % num2 != 0) num1 = _random.Next(1, 101); // Ensure division results in an integer
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
                isActive = false;
                Server.PrintToChatAll(_prefix + $" Player {player.PlayerName} answered correctly and got !");
                Server.PrintToChatAll(_prefix + " Quiz ended");
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
