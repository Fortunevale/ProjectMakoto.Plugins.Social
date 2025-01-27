// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Events;
internal sealed class UserBlockEvents : RequiresParent<SocialPlugin>
{
    internal UserBlockEvents(SocialPlugin plugin) : base(plugin.Bot, plugin)
    {
    }

    internal readonly Permissions[] ModerationPermissions =
    {
        Permissions.Administrator,
        Permissions.MuteMembers,
        Permissions.DeafenMembers,
        Permissions.ModerateMembers,
        Permissions.KickMembers,
        Permissions.BanMembers,
    };

    internal async Task VoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs e)
    {
        if (e.After.Channel is not null && !e.Channel.IsPrivate)
        {
            if (e.User.IsBot)
                return;

            var TranslationKey = ((Plugins.Social.Entities.Translations)SocialPlugin.Plugin!.Translations).Commands;

            var joiningMember = await e.User.ConvertToMember(e.Guild);
            var membersWithBlocks = e.After.Channel.Users.Where(x => x.Id != joiningMember.Id).Where(x => this.Parent.Users![x.Id].BlockedUsers.Contains(e.User.Id));
            var blockedMembers = e.After.Channel.Users.Where(x => x.Id != joiningMember.Id).Where(x => this.Parent.Users![x.Id].BlockedUsers.Contains(x.Id));

            var memberWithBlocksHighestRole = membersWithBlocks?.MaxBy(x => GetModerationStatus(x));
            var blockedMemberHighestRole = blockedMembers?.MaxBy(x => GetModerationStatus(x));
            int GetModerationStatus(DiscordMember? member)
            {
                var i = -1;

                if (member is not null && member.Permissions.HasAnyPermission(this.ModerationPermissions))
                    i = member.GetRoleHighestPosition();
                return i;
            }

            if (membersWithBlocks?.IsNotNullAndNotEmpty() ?? false)
            {
                if (GetModerationStatus(joiningMember) > GetModerationStatus(memberWithBlocksHighestRole))
                    return;

                _ = joiningMember.SendMessageAsync(new DiscordEmbedBuilder()
                    .WithDescription(TranslationKey.BlockedByVictim.Get(this.Parent.Bot.Users![joiningMember.Id])
                        .Build(true, new TVar("User", membersWithBlocks.First().Mention)))
                    .AsError(new SharedCommandContext()
                    {
                        Bot = this.Parent.Bot,
                        User = e.User,
                        Client = sender,
                        DbUser = this.Parent.Bot.Users![e.User.Id],
                    }).WithFooter());

                if (e.Before?.Channel is not null)
                    await joiningMember.ModifyAsync(x => x.VoiceChannel = e.Before.Channel);
                else
                    await joiningMember.DisconnectFromVoiceAsync();
            }
            else if (this.Parent.Users![e.User.Id].BlockedUsers.Any(blockedId => e.Channel.Users.Any(user => user.Id == blockedId)))
            {
                if (GetModerationStatus(joiningMember) > GetModerationStatus(blockedMemberHighestRole))
                    return;

                _ = joiningMember.SendMessageAsync(new DiscordEmbedBuilder()
                    .WithDescription(TranslationKey.BlockedVictim.Get(joiningMember.GetDbEntry(this.Parent.Bot))
                        .Build(true, new TVar("User", $"<@{this.Parent.Users![e.User.Id].BlockedUsers.First(blockedId => e.Channel.Users.Any(user => user.Id == blockedId))}>")))
                    .AsError(new SharedCommandContext()
                    {
                        Bot = this.Parent.Bot,
                        User = e.User,
                        Client = sender,
                        DbUser = this.Parent.Bot.Users![e.User.Id],
                    }).WithFooter());

                if (e.Before?.Channel is not null)
                    await joiningMember.ModifyAsync(x => x.VoiceChannel = e.Before.Channel);
                else
                    await joiningMember.DisconnectFromVoiceAsync();
            }
        }
    }
}
