// Project Makoto
// Copyright (C) 2023  Fortunevale
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using ProjectMakoto.Database;
using ProjectMakoto.Enums;

namespace ProjectMakoto.Plugins.Social.Entities;

public sealed class AfkStatus(Bot bot, SocialUser parent) : RequiresParent<SocialUser>(bot, parent)
{
    [ColumnName("afk_reason"), ColumnType(ColumnTypes.Text), Nullable]
    public string Reason
    {
        get => this.Parent.GetValue<string>(this.Parent.Id, "afk_reason");
        set => _ = this.Parent.SetValue(this.Parent.Id, "afk_reason", value);
    }

    [ColumnName("afk_since"), ColumnType(ColumnTypes.BigInt), Default("0")]
    public DateTime TimeStamp
    {
        get => this.Parent.GetValue<DateTime>(this.Parent.Id, "afk_since");
        set => _ = this.Parent.SetValue(this.Parent.Id, "afk_since", value);
    }
    [ColumnName("afk_pingamount"), ColumnType(ColumnTypes.BigInt), Default("0")]
    public long MessagesAmount
    {
        get => this.Parent.GetValue<long>(this.Parent.Id, "afk_pingamount");
        set => _ = this.Parent.SetValue(this.Parent.Id, "afk_pingamount", value);
    }

    [ColumnName("afk_pings"), ColumnType(ColumnTypes.Text), Default("[]")]
    public MessageDetails[] Messages
    {
        get => JsonConvert.DeserializeObject<MessageDetails[]>(this.Parent.GetValue<string>(this.Parent.Id, "afk_pings")) ?? [];
        set => _ = this.Parent.SetValue(this.Parent.Id, "afk_pings", JsonConvert.SerializeObject(value));
    }

    [JsonIgnore]
    internal DateTime LastMentionTrigger { get; set; } = DateTime.MinValue;
}
