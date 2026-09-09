using Dapper;
using InternApi.Models;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace InternApi.Controllers.Debug;

/// <summary>
/// デバッグ用: 指定ユーザーの所持品一覧を、item_master のアイテム名・レアリティも結合して返す。
/// ガチャ実行後の結果確認 (どのアイテムが増えたか、魔石がいくつ残っているか) に使う。
/// </summary>
[ApiController]
[Route("api/debug/inventory")]
public class DebugInventoryController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public DebugInventoryController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private MySqlConnection CreateUserConnection() =>
        new(_configuration.GetConnectionString("User")
            ?? throw new InvalidOperationException("ConnectionStrings:User が設定されていません。"));

    private MySqlConnection CreateMasterConnection() =>
        new(_configuration.GetConnectionString("Master")
            ?? throw new InvalidOperationException("ConnectionStrings:Master が設定されていません。"));

    /// <summary>
    /// 指定ユーザーの user_items を、intern_master.item_master と JOIN してアイテム名・レアリティ付きで返す。
    /// user と master が別 DB なので、それぞれ SELECT した後にアプリ側で結合している。
    /// </summary>
    /// <param name="userId">対象ユーザー id</param>
    /// <returns>{ itemId, name, rarity, quantity } の配列 (所持していなければ空配列)</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> Inventory(long userId)
    {
        // Step 1: user DB から所持品一覧
        List<InventoryRow> inventoryRows;
        await using (var userMySqlConnection = CreateUserConnection())
        {
            var queriedInventoryRows = await userMySqlConnection.QueryAsync<InventoryRow>(
                @"SELECT item_id, quantity
                  FROM user_items WHERE user_id = @userId
                  ORDER BY item_id",
                new { userId });
            inventoryRows = queriedInventoryRows.ToList();
        }
        if (inventoryRows.Count == 0) return Ok(Array.Empty<object>());

        // Step 2: master DB から item_master を一括取得
        var itemIds = inventoryRows.Select(inventoryRow => inventoryRow.ItemId).Distinct().ToArray();
        Dictionary<long, ItemMaster> itemMasterDict;
        await using (var masterMySqlConnection = CreateMasterConnection())
        {
            var itemMasters = await masterMySqlConnection.QueryAsync<ItemMaster>(
                @"SELECT item_id, item_type, name, rarity
                  FROM item_master WHERE item_id IN @itemIds",
                new { itemIds });
            itemMasterDict = itemMasters.ToDictionary(itemMaster => (long)itemMaster.ItemId);
        }

        // Step 3: アプリ側で結合。
        // item_master に定義が無い item_id も所持品としては存在するので、行を落とさずプレースホルダを入れて返す。
        var inventoryResults = inventoryRows.Select(inventoryRow =>
        {
            var itemMaster = itemMasterDict.GetValueOrDefault(inventoryRow.ItemId);
            return new
            {
                ItemId = inventoryRow.ItemId,
                Name = itemMaster?.Name ?? "(item_master未定義)",
                Rarity = itemMaster?.Rarity ?? "-",
                Quantity = inventoryRow.Quantity
            };
        });
        return Ok(inventoryResults);
    }

    private class InventoryRow
    {
        public long ItemId { get; set; }
        public uint Quantity { get; set; }
    }
}
