// Project Makoto Example Plugin
// Copyright (C) 2023 Fortunevale
// This code is licensed under MIT license (see 'LICENSE'-file for details)

using System.Runtime.CompilerServices;
using DisCatSharp.Entities;
using ProjectMakoto.Plugins.Social.Entities;
using ProjectMakoto.Events;

namespace ProjectMakoto.Plugins.Social;

public class SocialPlugin : BasePlugin
{
    public override string Name => "Social Commands";

    public override string Description => "Allows you to run social commands like /hug, /slap, etc.";

    public override SemVer Version => new(1, 0, 0);

    public override int[] SupportedPluginApis => [1];

    public override string Author => "Mira";

    public override ulong? AuthorId => 411950662662881290;

    public override string UpdateUrl => "https://github.com/Fortunevale/ProjectMakoto.Plugins.Social";

    public override Octokit.Credentials? UpdateUrlCredentials => base.UpdateUrlCredentials;


    public static SocialPlugin? Plugin { get; set; }

    public SelfFillingDatabaseDictionary<Social.Entities.SocialUser>? Users { get; set; } = null;

    internal PluginConfig LoadedConfig
    {
        get
        {
            if (!this.CheckIfConfigExists())
            {
                this._logger.LogDebug("Creating Plugin Config..");
                this.WriteConfig(new PluginConfig());
            }

            var v = this.GetConfig(); 

            if (v.GetType() == typeof(JObject))
            {
                this.WriteConfig(((JObject)v).ToObject<PluginConfig>() ?? new PluginConfig());
                v = this.GetConfig();
            }

            return (PluginConfig)v;
        }
    }

    public override SocialPlugin Initialize()
    {
        SocialPlugin.Plugin = this;

        this.Connected += (s, e) =>
        {
            this._logger.LogDebug("Importing user data from core..");

            foreach (var user in this.Bot.Users)
            {
                if (user.Value.LegacyBlockedUsers.Length > 0)
                {
                    this._logger.LogDebug("Importing BlockedUsers from '{User}'..", user.Key);

                    this.Users![user.Key].BlockedUsers = [.. this.Users![user.Key].BlockedUsers, .. user.Value.LegacyBlockedUsers];
                    user.Value.LegacyBlockedUsers = [];
                }
            }

            var afkEvents = new AfkEvents(this);
            var userBlockEvents = new UserBlockEvents(this);

            this.Bot.DiscordClient.MessageCreated += afkEvents.MessageCreated;
            this.Bot.DiscordClient.VoiceStateUpdated += userBlockEvents.VoiceStateUpdated;
        };

        this.DatabaseInitialized += (s, e) =>
        {
            this.Users = new SelfFillingDatabaseDictionary<Social.Entities.SocialUser>(this, typeof(Social.Entities.SocialUser), (id) =>
            {
                return new Social.Entities.SocialUser(this, id);
            });
        };

        return this;
    }

    public override Task<IEnumerable<MakotoModule>> RegisterCommands()
    {
        return Task.FromResult<IEnumerable<MakotoModule>>(new List<MakotoModule>
        {
            new("Social",
                new List<MakotoCommand>()
                {
                    new("afk", "Allows you to set yourself AFK. Users who ping you will be notified that you're unavailable.", typeof(AfkCommand),
                        new MakotoCommandOverload(typeof(string), "reason", "The reason", true, true)),

                    new("block-user", "Allows you to block a user", typeof(BlockUserCommand),
                        new MakotoCommandOverload(typeof(DiscordUser), "user", "The user", true, true)),
                    new("unblock-user", "Allows you to unblock a user", typeof(UnblockUserCommand),
                        new MakotoCommandOverload(typeof(DiscordUser), "user", "The user", true, true)),

                    new MakotoCommand("cuddle", "Cuddle with another user.", typeof(CuddleCommand),
                        new MakotoCommandOverload(typeof(DiscordUser), "user", "The user", true, false)).WithIsEphemeral(false),
                    new MakotoCommand("kiss", "Kiss another user.", typeof(KissCommand),
                        new MakotoCommandOverload(typeof(DiscordUser), "user", "The user", true, false)).WithIsEphemeral(false),
                    new MakotoCommand("slap", "Slap another user.", typeof(SlapCommand),
                        new MakotoCommandOverload(typeof(DiscordUser), "user", "The user", true, false)).WithIsEphemeral(false),
                    new MakotoCommand("kill", "Kill another user..?", typeof(KillCommand),
                        new MakotoCommandOverload(typeof(DiscordUser), "user", "The user", true, false)).WithIsEphemeral(false),
                    new MakotoCommand("boop", "Give another user a boop!", typeof(BoopCommand),
                        new MakotoCommandOverload(typeof(DiscordUser), "user", "The user", true, false)).WithIsEphemeral(false),
                    new MakotoCommand("highfive", "Give a high five!", typeof(HighFiveCommand),
                        new MakotoCommandOverload(typeof(DiscordUser), "user", "The user", true, false)).WithIsEphemeral(false),
                    new MakotoCommand("hug", "Hug another user!", typeof(HugCommand),
                        new MakotoCommandOverload(typeof(DiscordUser), "user", "The user", true, false)).WithIsEphemeral(false),
                    new MakotoCommand("pat", "Give someone some headpats!", typeof(PatCommand),
                        new MakotoCommandOverload(typeof(DiscordUser), "user", "The user", true, false)).WithIsEphemeral(false),
                })
        });
    }

    public override Task<IEnumerable<Type>?> RegisterTables()
    {
        return Task.FromResult<IEnumerable<Type>?>(new List<Type>
        {
            typeof(Social.Entities.SocialUser),
        });
    }

    public override (string? path, Type? type) LoadTranslations()
    {
        return ("Translations/strings.json", typeof(Entities.Translations));
    }

    public override Task Shutdown()
        => base.Shutdown();
}
