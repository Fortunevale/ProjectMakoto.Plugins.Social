// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

internal sealed class PatCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var CommandKey = ((Plugins.Social.Entities.Translations)SocialPlugin.Plugin!.Translations).Commands.Pat;
            var ModuleKey = ((Plugins.Social.Entities.Translations)SocialPlugin.Plugin!.Translations).Commands;

            var user = (DiscordUser)arguments["user"];

            if (await ctx.DbUser.Cooldown.WaitForLight(ctx))
                return;

            if (SocialPlugin.Plugin.Users![ctx.User.Id].BlockedUsers.Contains(user.Id))
            {
                _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription(this.GetString(ModuleKey.BlockedVictim, true, new TVar("User", user.Mention))).AsError(ctx));
                return;
            }

            if (SocialPlugin.Plugin.Users![user.Id].BlockedUsers.Contains(ctx.User.Id))
            {
                _ = await this.RespondOrEdit(new DiscordEmbedBuilder().WithDescription(this.GetString(ModuleKey.BlockedByVictim, true, new TVar("User", user.Mention))).AsError(ctx));
                return;
            }

            var phrases = CommandKey.Other.Get(ctx.DbGuild);
            var self_phrases = CommandKey.Self.Get(ctx.DbGuild);

            if (ctx.Member.Id == user.Id)
            {
                _ = await this.RespondOrEdit(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                {
                    Title = self_phrases.SelectRandom().Build(
                        new TVar("User1", ctx.Member.DisplayName),
                        new TVar("Emoji", "😢")),
                    Color = EmbedColors.HiddenSidebar,
                    Footer = ctx.GenerateUsedByFooter(),
                }));
                return;
            }

            var response = await SocialCommandAbstractions.GetGif(ctx.Bot, "pat");

            _ = await this.RespondOrEdit(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder
            {
                Description = phrases.SelectRandom().Build(
                    new TVar("User1", ctx.User.Mention, false),
                    new TVar("User2", user.Mention, false))
                    .Bold(),
                ImageUrl = response.Item2,
                Color = EmbedColors.HiddenSidebar,
                Footer = ctx.GenerateUsedByFooter(response.Item1),
            }).WithAllowedMention(UserMention.All));
        });
    }
}