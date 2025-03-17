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

            RegisterEventHandler<EventPlayerConnectFull>((@event, info) =>
            {
                var player = @event.Userid;

                int playTime = FetchCS2PlaytimeAsync(Convert.ToString(player.AuthorizedSteamID.SteamId64)).GetAwaiter().GetResult();
                if (playTime < _config.MinHoursRequired)
                {
                    Server.ExecuteCommand($"kickid {@event.Userid.UserId} none");
                }


                return HookResult.Continue;
            });
        }


    }
}
