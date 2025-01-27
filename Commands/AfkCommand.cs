// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

namespace ProjectMakoto.Commands;

public sealed class AfkCommand : BaseCommand
{
    public override Task ExecuteCommand(SharedCommandContext ctx, Dictionary<string, object> arguments)
    {
        return Task.Run(async () =>
        {
            var CommandKey = ((Plugins.Social.Entities.Translations)SocialPlugin.Plugin!.Translations).Commands.Afk;

            var reason = (string)arguments["reason"];

            if (await ctx.DbUser.Cooldown.WaitForModerate(ctx))
                return;

            if (reason.Length > 128)
            {
                this.SendSyntaxError();
                return;
            }

            SocialPlugin.Plugin.Users![ctx.User.Id].AfkStatus.Reason = reason.FullSanitize();
            SocialPlugin.Plugin.Users![ctx.User.Id].AfkStatus.TimeStamp = DateTime.UtcNow;

            _ = await this.RespondOrEdit(new DiscordEmbedBuilder
            {
                Description = $"{ctx.User.Mention} {this.GetString(CommandKey.SetAfk, true)}"
            }.AsSuccess(ctx, this.GetString(CommandKey.Title)));
            await Task.Delay(10000);
            _ = ctx.ResponseMessage.DeleteAsync();
        });
    }
}