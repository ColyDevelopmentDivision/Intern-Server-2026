using Dapper;
using MySqlConnector;

namespace InternApi.Services;

/// <summary>
/// ユーザー作成・ログインまわりのビジネスロジック。
/// </summary>
public class UserService
{
    private readonly IConfiguration _configuration;

    public UserService(IConfiguration configuration)
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
    /// ユーザーを新規作成し、生成された userId を返す。
    /// </summary>
    /// <param name="name">作成するユーザーの表示名 (UNIQUE)</param>
    /// <returns>生成された userId。name が既に存在する (UNIQUE 制約違反) 場合は null</returns>
    public async Task<long?> CreateAsync(string name)
    {
        await using var userMySqlConnection = CreateUserConnection();
        await userMySqlConnection.OpenAsync();
        await using var mySqlTransaction = await userMySqlConnection.BeginTransactionAsync();

        try
        {
            // 1. user_profiles に INSERT (level はデフォルト1、created_at はデフォルト値)
            await userMySqlConnection.ExecuteAsync(
                "INSERT INTO user_profiles (name) VALUES (@name)",
                new { name },
                transaction: mySqlTransaction);

            // 2. 直前に INSERT した行の id を取得。
            //    name には UNIQUE 制約があるため、この SELECT は必ず自分の INSERT した1行を返す。
            //    同一トランザクション内なので、他クライアントの書き込みに影響されず自分の書き込みが見える。
            var userId = await userMySqlConnection.QueryFirstAsync<long>(
                "SELECT id FROM user_profiles WHERE name = @name",
                new { name },
                transaction: mySqlTransaction);

            // 3. 初期所持品として魔石 (item_id=1001) を 300 個付与
            await userMySqlConnection.ExecuteAsync(
                @"INSERT INTO user_items (user_id, item_id, quantity)
                  VALUES (@userId, 1001, 300)",
                new { userId },
                transaction: mySqlTransaction);

            await mySqlTransaction.CommitAsync();
            return userId;
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            // 1062 = ER_DUP_ENTRY (UNIQUE 制約違反)。
            // mySqlTransaction は using で自動 rollback される。呼び出し側で 409 Conflict に変換する。
            return null;
        }
    }

    /// <summary>
    /// name で userId を検索する (ダミーログイン、パスワード検証なし)。
    /// </summary>
    /// <param name="name">検索するユーザー名</param>
    /// <returns>該当ユーザーの userId。見つからなければ null</returns>
    public async Task<long?> LoginAsync(string name)
    {
        await using var userMySqlConnection = CreateUserConnection();
        return await userMySqlConnection.QueryFirstOrDefaultAsync<long?>(
            "SELECT id FROM user_profiles WHERE name = @name",
            new { name });
    }
}
