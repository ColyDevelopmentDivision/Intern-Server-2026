namespace InternApi.Models;

/// <summary>
/// ガチャ1本の排出プール (intern_master.gacha_detail_master)。
/// weight は絶対値でなく相対比率。合計は任意で、
/// 抽選時は SUM(weight) を分母として乱数を刻む。
/// </summary>
public class GachaDetailMaster
{
    public int GachaId { get; set; }
    public int ItemId { get; set; }
    public int Weight { get; set; }
}
