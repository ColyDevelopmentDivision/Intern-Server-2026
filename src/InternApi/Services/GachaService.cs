using Dapper;
using InternApi.Models;
using MySqlConnector;

namespace InternApi.Services;

/// <summary>
/// ガチャ抽選まわりのビジネスロジック。
/// </summary>
public class GachaService
{
    private readonly IConfiguration _configuration;

    public GachaService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// マスタ DB (intern_master) への接続を生成する。
    /// </summary>
    /// <returns>マスタ DB への未オープンな MySqlConnection</returns>
    /// <exception cref="InvalidOperationException">ConnectionStrings:Master が未設定</exception>
    private MySqlConnection CreateMasterConnection()
    {
        var connectionString = _configuration.GetConnectionString("Master")
            ?? throw new InvalidOperationException("ConnectionStrings:Master が設定されていません。");
        return new MySqlConnection(connectionString);
    }

    /// <summary>
    /// 指定 gachaId のガチャ定義を gacha_master から取得する。
    /// </summary>
    /// <param name="gachaId">対象ガチャ id</param>
    /// <returns>ガチャ定義。存在しなければ null</returns>
    public async Task<GachaMaster?> GetGachaAsync(int gachaId)
    {
        await using var masterMySqlConnection = CreateMasterConnection();
        return await masterMySqlConnection.QueryFirstOrDefaultAsync<GachaMaster>(
            @"SELECT gacha_id, name, start_at, end_at, cost_item_id, cost_amount
              FROM gacha_master WHERE gacha_id = @gachaId",
            new { gachaId });
    }

    /// <summary>
    /// 指定 gachaId の排出プール (item_id と weight のリスト) を gacha_detail_master から取得する。
    /// </summary>
    /// <param name="gachaId">対象ガチャ id</param>
    /// <returns>item_id 昇順の排出プール</returns>
    public async Task<List<GachaDetailMaster>> GetPoolAsync(int gachaId)
    {
        await using var masterMySqlConnection = CreateMasterConnection();
        var gachaDetailMasters = await masterMySqlConnection.QueryAsync<GachaDetailMaster>(
            @"SELECT gacha_id, item_id, weight
              FROM gacha_detail_master WHERE gacha_id = @gachaId
              ORDER BY item_id",
            new { gachaId });
        return gachaDetailMasters.ToList();
    }

    /// <summary>
    /// gacha_detail_master.weight に従った重み付き抽選を1回行い、選ばれた item_id を返す。
    /// </summary>
    /// <param name="gachaId">対象ガチャ id</param>
    /// <returns>選ばれたアイテムの item_id</returns>
    /// <exception cref="InvalidOperationException">排出プールが空</exception>
    public async Task<int> DrawOneAsync(int gachaId)
    {
        var gachaDetailMasters = await GetPoolAsync(gachaId);
        if (gachaDetailMasters.Count == 0)
            throw new InvalidOperationException($"gacha_id={gachaId} のプールが空。");

        // TODO(メイン課題1): 下の1行を、重み付き抽選ロジックに書き換えよう。
        // 現状は「プール先頭固定」= 何度引いても同じアイテムしか出ない状態。
        return gachaDetailMasters[0].ItemId;
    }

    /// <summary>
    /// DrawOneAsync を count 回呼び出して item_id のリストを返す。10連ガチャ用。
    /// </summary>
    /// <param name="gachaId">対象ガチャ id</param>
    /// <param name="count">抽選回数</param>
    /// <returns>抽選結果の item_id リスト (count 個)</returns>
    public async Task<List<int>> DrawManyAsync(int gachaId, int count)
    {
        var itemIds = new List<int>(count);
        for (int i = 0; i < count; i++)
        {
            var itemId = await DrawOneAsync(gachaId);
            itemIds.Add(itemId);
        }
        return itemIds;
    }
}
