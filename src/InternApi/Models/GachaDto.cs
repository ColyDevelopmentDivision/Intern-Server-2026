namespace InternApi.Models;

/// <summary>
/// POST /api/gacha/draw のリクエストDTO。
/// Excel IF タブ準拠。
/// </summary>
public class GachaDrawInParam
{
    public int GachaId { get; set; }
}

/// <summary>
/// ガチャ結果1件。
/// isNew は「そのユーザーがこのアイテムを初めて手に入れたか」を示す。
/// </summary>
public class GachaDrawResult
{
    public int ItemId { get; set; }
    public bool IsNew { get; set; }
}

/// <summary>
/// POST /api/gacha/draw のレスポンスDTO。
/// Excel IF タブは10連前提のため results は10件想定だが、DTO自体は件数非依存。
/// </summary>
public class GachaDrawOutParam
{
    public List<GachaDrawResult> Results { get; set; } = [];
}
