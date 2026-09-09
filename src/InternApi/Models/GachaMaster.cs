namespace InternApi.Models;

/// <summary>
/// ガチャバナー1本の定義 (intern_master.gacha_master)。
/// </summary>
public class GachaMaster
{
    public int GachaId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public int CostItemId { get; set; }
    public int CostAmount { get; set; }
}
