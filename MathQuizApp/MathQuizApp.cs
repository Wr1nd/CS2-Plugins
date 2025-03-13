using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using CounterStrikeSharp.API.Modules.Menu;

namespace MathQuizApp
{
    public partial class MathQuizApp : BasePlugin
    {
        public bool isActiveQuiz = false;
        public string equation = "";
        public int answer = 0;
        public Difficulty currentDiff;



        private Config _config = new();
        private readonly Random _random = new();

        public int MaxTimeToAnswer;
        public int IntervalMin;
        public int IntervalMax;

        public List<CCSPlayerController> playersToRemove = new();
        private Dictionary<string, int> DifficultyRewards = new();
        public List<CCSPlayerController> players = new();
        public CounterStrikeSharp.API.Modules.Timers.Timer ChatTime;


        public override string ModuleName => "MathQuizApp";
        public override string ModuleAuthor => "Wr1nd";
        public override string ModuleVersion => "1.0.0";


        private readonly string _prefix = $"[ {ChatColors.DarkRed}ShmitzHakeriai {ChatColors.Default}]";

        public override void Load(bool hotReload)
        {
            _config = LoadConfig();
            for (int i = 0; i < _config.Quizes.Math.Count; i++)
            {
                DifficultyRewards.Add(_config.Quizes.Math[i].Difficulty, _config.Quizes.Math[i].Reward);
            }
            //get data from config
            MaxTimeToAnswer = _config.MaxTimeToAnswer;

            StartQuizTimeoutTimer();
        }

        private void StartQuizTimeoutTimer()
        {
            IntervalMin = _config.IntervalMin;
            IntervalMax = _config.IntervalMax;

            int interval = _random.Next(IntervalMin, IntervalMax);

            ChatTime = AddTimer(interval, () =>
            {
                Server.NextFrame(() =>
                {
                    ShowQuizOn(GenerateEquation());
                });
            });
        }

        private string GenerateEquation()
        {
            isActiveQuiz = true;
            StartQuizTimer();
            string equation = GenerateMathQuestion();

            return equation;
        }

        private void StartQuizTimer()
        {
            AddTimer(MaxTimeToAnswer, () =>
            {
                Server.NextFrame(() =>
                {
                        Server.PrintToChatAll(_prefix + "Quiz time has ended");
                    isActiveQuiz = false;
                    StartQuizTimeoutTimer();
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
            equation = $"{num1} {operatorSymbol} {num2} = ?";
            return equation;
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
            if (!isActiveQuiz)
            {
                player.PrintToChat(_prefix + " There is no ongoing QUIZ");
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
                isActiveQuiz = false;
                int playerReward = 0;
                if (DifficultyRewards.TryGetValue(currentDiff.ToString(), out int reward))
                {
                    playerReward = reward;
                }
                Server.PrintToChatAll(_prefix + $" Player {player.PlayerName} answered correctly!");
                Server.PrintToChatAll(_prefix + $" Reward for completinting {currentDiff} level: {playerReward} jewels");
                Server.PrintToChatAll(_prefix + " Quiz ended");

                StartQuizTimeoutTimer();

            }
            else
            {
                player.PrintToChat(_prefix + " Answer is not correct");
            }
        }


        private void ShowQuizOn(string equation)
        {
            if (!isActiveQuiz) return;

            players = GetPlayingPlayers();

            RegisterListener<Listeners.OnTick>(() =>
            {
                if (!isActiveQuiz || players == null || players.Count == 0) return;

                playersToRemove = new List<CCSPlayerController>();

                foreach (var player in players)
                {
                    try
                    {
                        if (player != null && IsPlayerPlaying(player) && isActiveQuiz)
                        {
                            player.PrintToCenterAlert(equation + "\n To asnwer type !ats answer");
                        }
                        else
                        {
                            playersToRemove.Add(player!);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.LogError($"ShowQuizOn OnTick: {e}");
                    }

                    playersToRemove.ForEach(p => players.Remove(p));

                }

            });
        }

        public static bool IsPlayerPlaying(CCSPlayerController? player)
        {
            return player is { IsValid: true, Connected: PlayerConnectedState.PlayerConnected, TeamNum: 2 or 3, IsBot: false, IsHLTV: false };
        }

        private List<CCSPlayerController> GetPlayingPlayers()
        {
            return Utilities.GetPlayers().Where(IsPlayerPlaying).ToList();
        }

        public enum Difficulty { Easy, Medium, Hard }
    }
}
