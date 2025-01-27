// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using DisCatSharp.CommandsNext;
using ProjectMakoto.Entities.Users;

namespace ProjectMakoto.Events;
internal sealed class AfkEvents : RequiresParent<SocialPlugin>
{
    internal AfkEvents(SocialPlugin plugin) : base(plugin.Bot, plugin)
    {
    }

    internal async Task MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        if (SocialPlugin.Plugin!.HasUserObjected(e.Author.Id) || SocialPlugin.Plugin!.IsUserBanned(e.Author.Id) || SocialPlugin.Plugin!.IsGuildBanned(e.Guild?.Id ?? 0))
            return;

        var prefix = e.Guild!.GetGuildPrefix(this.Parent.Bot);

        if (e?.Message?.Content?.StartsWith(prefix) ?? false)
            foreach (var command in sender.GetCommandsNext().RegisteredCommands)
                if (e.Message.Content.StartsWith($"{prefix}{command.Key}"))
                    return;

        if (e?.Guild is null || e.Channel.IsPrivate || e.Author.IsBot)
            return;

        var AfkKey = ((Plugins.Social.Entities.Translations)SocialPlugin.Plugin!.Translations).Commands.Afk;

        if (this.Parent.Users![e.Author.Id].AfkStatus.TimeStamp != DateTime.MinValue && this.Parent.Users![e.Author.Id].AfkStatus.LastMentionTrigger.AddSeconds(10) < DateTime.UtcNow)
        {
            var cache = new DateTime().ToUniversalTime().AddTicks(this.Parent.Users![e.Author.Id].AfkStatus.TimeStamp.Ticks);

            this.Parent.Users![e.Author.Id].AfkStatus.Reason = "";
            this.Parent.Users![e.Author.Id].AfkStatus.TimeStamp = DateTime.MinValue;

            var embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = e.Guild.IconUrl, Name = $"{AfkKey.Title.Get(this.Parent.Bot.Users![e.Author.Id])} • {e.Guild.Name}" },
                Color = EmbedColors.Info,
                Timestamp = DateTime.UtcNow,
                Description = AfkKey.Events.NoLongerAfk.Get(this.Parent.Bot.Users![e.Author.Id]).Build(true,
                new TVar("User", e.Author.Mention),
                new TVar("Timestamp", cache.ToTimestamp()))
            };

            var ExtendDelay = false;

            if (this.Parent.Users![e.Author.Id].AfkStatus.MessagesAmount > 0)
            {
                embed.Description += $"\n\n{AfkKey.Events.NoLongerAfk.Get(this.Parent.Bot.Users![e.Author.Id]).Build(true)}\n" +
                                        $"{string.Join("\n", this.Parent.Users![e.Author.Id].AfkStatus.Messages
                                            .Select(x => AfkKey.Events.MessageListing
                                                .Get(this.Parent.Bot.Users![e.Author.Id])
                                                .Build(true,
                                                new TVar("User", $"<@!{x.AuthorId}>"),
                                                new TVar("Message", new EmbeddedLink($"https://discord.com/channels/{x.GuildId}/{x.ChannelId}/{x.MessageId}", AfkKey.Events.Message.Get(this.Parent.Bot.Users![e.Author.Id]))))))}";

                ExtendDelay = true;

                if (this.Parent.Users![e.Author.Id].AfkStatus.MessagesAmount - 5 > 0)
                {
                    embed.Description += $"\n{AfkKey.Events.AndMore.Get(this.Parent.Bot.Users![e.Author.Id])
                        .Build(true, new TVar("Count", this.Parent.Users![e.Author.Id].AfkStatus.MessagesAmount - 5))}";
                }

                this.Parent.Users![e.Author.Id].AfkStatus.MessagesAmount = 0;
                this.Parent.Users![e.Author.Id].AfkStatus.Messages = Array.Empty<MessageDetails>();
            }

            var message = await e.Message.RespondAsync(embed);

            if (ExtendDelay)
                await Task.Delay(30000);
            else
                await Task.Delay(10000);

            _ = message.DeleteAsync();
        }

        if (e.MentionedUsers != null && e.MentionedUsers.Count > 0)
        {
            foreach (var b in e.MentionedUsers)
            {
                if (b.Id == e.Author.Id)
                    continue;

                if (this.Parent.Users![b.Id].AfkStatus.TimeStamp != DateTime.MinValue)
                {
                    if (this.Parent.Users![e.Author.Id].AfkStatus.LastMentionTrigger.AddSeconds(30) > DateTime.UtcNow)
                        return;

                    if (this.Parent.Users![b.Id].AfkStatus.Messages.Length < 5)
                    {
                        this.Parent.Users![b.Id].AfkStatus.Messages = this.Parent.Users![b.Id].AfkStatus.Messages.Add(new()
                        {
                            AuthorId = e.Author.Id,
                            ChannelId = e.Channel.Id,
                            GuildId = e.Guild.Id,
                            MessageId = e.Message.Id,
                        });
                    }

                    this.Parent.Users![b.Id].AfkStatus.MessagesAmount++;

                    this.Parent.Users![e.Author.Id].AfkStatus.LastMentionTrigger = DateTime.UtcNow;

                    var message = await e.Message.RespondAsync(new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = e.Guild.IconUrl, Name = $"{AfkKey.Title.Get(this.Parent.Bot.Users![e.Author.Id])} • {e.Guild.Name}" },
                        Color = EmbedColors.Info,
                        Timestamp = DateTime.UtcNow,
                        Description = AfkKey.Events.CurrentlyAfk.Get(this.Parent.Bot.Users![e.Author.Id]).Build(true,
                            new TVar("User", b.Mention),
                            new TVar("Timestamp", this.Parent.Users![b.Id].AfkStatus.TimeStamp.ToTimestamp()),
                            new TVar("Reason", this.Parent.Users![b.Id].AfkStatus.Reason.FullSanitize()))
                    });
                    await Task.Delay(10000);
                    _ = message.DeleteAsync();
                    return;
                }
            }
        }
    }
}
