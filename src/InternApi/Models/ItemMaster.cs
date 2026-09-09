namespace InternApi.Models;

/// <summary>
/// ガチャで排出される (または通貨として扱う) アイテムのマスタ (intern_master.item_master)。
/// rarity は "SSR"/"SR"/"R" などの文字列。クライアントの演出出し分けと直結する。
/// </summary>
public class ItemMaster
{
    public int ItemId { get; set; }
    public string ItemType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Rarity { get; set; } = string.Empty;
}
