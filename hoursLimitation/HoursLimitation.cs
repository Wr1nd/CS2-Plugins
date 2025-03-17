using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Entities;
using Serilog;

namespace hoursLimtation
{
    public partial class hoursLimtation : BasePlugin
    {

        public override string ModuleName => "hoursLimtation";
        public override string ModuleAuthor => "Wr1nd";
        public override string ModuleVersion => "1.0.0";
        public override string ModuleDescription => "Restrict certain players from connecting to server.";

        public Config _config = new();

        public override void Load(bool hotReload)
        {
            _config = LoadConfig();

            RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnect);
        }

        private HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo info)
        {
            var player = @event.Userid;
            string steamId = player.AuthorizedSteamID.SteamId64.ToString();
            int userId = player.UserId ?? 0;
            _ = HandlePlaytimeCheck(steamId, userId);
            return HookResult.Continue;
        }

        private async Task HandlePlaytimeCheck(string steamId, int userId)
        {
            int playTime = await FetchCS2PlaytimeAsync(steamId);

            if (playTime < _config.MinHoursRequired)
            {
                Server.NextFrame(() =>
                {
                    Server.ExecuteCommand($"kickid {userId} none");
                });
            }
        }




    }
}
