using Dapper;
using MySqlConnector;

namespace InternApi.Services;

/// <summary>
/// user_items に対する操作を集約したサービス。
/// </summary>
public class UserItemService
{
    private readonly IConfiguration _configuration;

    public UserItemService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// ユーザー DB (intern_user) への接続を生成する。
    /// </summary>
    /// <returns>ユーザー DB への未オープンな MySqlConnection</returns>
    /// <exception cref="InvalidOperationException">ConnectionStrings:User が未設定</exception>
    private MySqlConnection CreateUserConnection()
    {
        var connectionString = _configuration.GetConnectionString("User")
            ?? throw new InvalidOperationException("ConnectionStrings:User が設定されていません。");
        return new MySqlConnection(connectionString);
    }

    /// <summary>
    /// あるユーザーが1個以上持っている item_id の集合を返す。
    /// isNew 判定 (今回のガチャで初めて手に入れたか) のヘルパー。
    /// </summary>
    /// <param name="userId">対象ユーザー id</param>
    /// <returns>そのユーザーが quantity &gt; 0 で持っている item_id の集合</returns>
    public async Task<HashSet<long>> GetOwnedItemIdsAsync(long userId)
    {
        await using var userMySqlConnection = CreateUserConnection();
        var itemIds = await userMySqlConnection.QueryAsync<long>(
            "SELECT item_id FROM user_items WHERE user_id = @userId AND quantity > 0",
            new { userId });
        return new HashSet<long>(itemIds);
    }

    /// <summary>
    /// 指定ユーザーが item_id を requiredCount 個以上持っているかを判定する。
    /// 該当行が無い場合は「0 個持っている」として扱う。
    /// </summary>
    /// <param name="userId">対象ユーザー id</param>
    /// <param name="itemId">判定するアイテム id</param>
    /// <param name="requiredCount">必要な所持数</param>
    /// <returns>requiredCount 個以上持っていれば true、そうでなければ false</returns>
    public async Task<bool> HasEnoughAsync(long userId, long itemId, uint requiredCount)
    {
        // TODO(追加課題C): 所持数チェックを実装してみよう。
        throw new NotImplementedException("追加課題C: 所持数チェックを実装しよう");
    }

    /// <summary>
    /// 指定ユーザーの item_id を consumeCount 個だけ消費する (所持数を減らす)。
    /// </summary>
    /// <param name="userId">対象ユーザー id</param>
    /// <param name="itemId">消費するアイテム id</param>
    /// <param name="consumeCount">消費する個数</param>
    public async Task ConsumeAsync(long userId, long itemId, uint consumeCount)
    {
        // TODO(追加課題D): 所持数減算を実装してみよう。
        throw new NotImplementedException("追加課題D: 所持数減算を実装しよう");
    }

    /// <summary>
    /// 指定ユーザーに item_id を grantCount 個付与する (新規行 or 既存行への加算)。
    /// </summary>
    /// <param name="userId">対象ユーザー id</param>
    /// <param name="itemId">付与するアイテム id</param>
    /// <param name="grantCount">付与する個数</param>
    public async Task GrantAsync(long userId, long itemId, uint grantCount)
    {
        // TODO(追加課題B): 所持アイテム付与を実装してみよう。
        throw new NotImplementedException("追加課題B: 所持アイテム付与を実装しよう");
    }
}
