// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class BlockUserCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var CommandKey = ((Plugins.Social.Entities.Translations)SocialPlugin.Plugin!.Translations).Commands.BlockUser;

            var victim = (DiscordUser)arguments["victim"];

            if (SocialPlugin.Plugin.Users![ctx.User.Id].BlockedUsers.Contains(victim.Id))
            {
                _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription(this.GetString(CommandKey.AlreadyBlocked, true)).AsError(ctx));
                return;
            }

            if (victim.Id == ctx.Client.CurrentUser.Id || victim.Id == ctx.User.Id || victim.IsBot || (victim.Flags?.HasFlag(UserFlags.Staff) ?? false))
            {
                _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription(this.GetString(CommandKey.CannotBlock, true)).AsError(ctx));
                return;
            }

            SocialPlugin.Plugin.Users![ctx.User.Id].BlockedUsers = SocialPlugin.Plugin.Users![ctx.User.Id].BlockedUsers.Add(victim.Id);

            _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription(this.GetString(CommandKey.Blocked, true, new TVar("User", victim.Mention))).AsSuccess(ctx));
        });
    }
}