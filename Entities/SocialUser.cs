using ProjectMakoto.Database;
using ProjectMakoto.Enums;

namespace ProjectMakoto.Plugins.Social.Entities;

[TableName("users")]
public sealed class SocialUser : PluginDatabaseTable
{
    public SocialUser(BasePlugin plugin, ulong identifierValue) : base(plugin, identifierValue)
    {
        this.Id = identifierValue;

        this.AfkStatus = new AfkStatus(this.Plugin.Bot, this);
    }

    [ColumnName("userid"), ColumnType(ColumnTypes.BigInt), Primary]
    internal ulong Id { get; init; }

    [ColumnName("blocked_users"), ColumnType(ColumnTypes.LongText), Default("[]")]
    public ulong[] BlockedUsers
    {
        get => JsonConvert.DeserializeObject<ulong[]>(this.GetValue<string>(this.Id, "blocked_users")) ?? [];
        set => this.SetValue(this.Id, "blocked_users", JsonConvert.SerializeObject(value));
    }

    [ContainsValues]
    public AfkStatus AfkStatus { get; init; }
}
