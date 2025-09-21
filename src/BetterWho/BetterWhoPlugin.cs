using System.Collections.Generic;
using System.Linq;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;

namespace BetterWho;

public class BetterWhoPlugin : BasePlugin
{
    public string Name => "BetterWho";
    public string Author => "OpenAI Assistant";
    public string Version => "1.0.0";

    public override string ModuleName => Name;
    public override string ModuleAuthor => Author;
    public override string ModuleVersion => Version;

    public override void Load(bool hotReload)
    {
        AddCommand("css_bwho", "Display connected player details", HandleBetterWhoCommand);
    }

    private void HandleBetterWhoCommand(CCSPlayerController? caller, CommandInfo command)
    {
        if (caller is null || !caller.IsValid)
        {
            command.ReplyToCommand("This command can only be used by an in-game player.");
            return;
        }

        if (!AdminManager.PlayerHasPermissions(caller, "@css/admin"))
        {
            command.ReplyToCommand("You do not have permission to use this command.");
            return;
        }

        var players = Utilities.GetPlayers()
            .Where(player => player is { IsValid: true })
            .ToList();

        if (players.Count == 0)
        {
            command.ReplyToCommand("No connected players were found.");
            return;
        }

        foreach (var player in players)
        {
            var steamId = player.AuthorizedSteamID;
            var steamId64 = steamId?.SteamId64 ?? 0;
            var profileLink = steamId64 > 0
                ? $"https://steamcommunity.com/profiles/{steamId64}"
                : "N/A";

            var ipAddress = string.IsNullOrWhiteSpace(player.IpAddress) ? "N/A" : player.IpAddress;

            var permissions = GetPlayerPermissions(player);
            var permissionText = permissions.Count > 0
                ? string.Join(", ", permissions)
                : "None";

            var message =
                $"{player.PlayerName} | {profileLink} | {ipAddress} | {permissionText}";

            command.ReplyToCommand(message);
        }
    }

    private static List<string> GetPlayerPermissions(CCSPlayerController player)
    {
        var adminData = AdminManager.GetPlayerAdminData(player);
        var permissions = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

        if (adminData?.Groups is { Count: > 0 })
        {
            foreach (var group in adminData.Groups)
            {
                permissions.Add(group);
            }
        }

        if (adminData?.Flags is { Count: > 0 })
        {
            foreach (var flagSet in adminData.Flags.Values)
            {
                foreach (var flag in flagSet)
                {
                    permissions.Add(flag);
                }
            }
        }

        return permissions.OrderBy(value => value).ToList();
    }
}
