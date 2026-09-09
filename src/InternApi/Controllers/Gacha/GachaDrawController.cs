using InternApi.Models;
using InternApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace InternApi.Controllers.Gacha;

/// <summary>
/// ガチャ本編: 10連ガチャを引く。
/// Excel IF タブ準拠 POST /api/gacha/draw。
/// </summary>
[ApiController]
[Route("api/gacha/draw")]
public class GachaDrawController : ControllerBase
{
    /// <summary>10連ガチャの引き数。仕様上は10で固定。</summary>
    private const int DrawCount = 10;

    /// <summary>
    /// ガチャ関連の処理 (プール取得・重み付き抽選など) を提供する Service
    /// </summary>
    private readonly GachaService _gachaService;

    public GachaDrawController(GachaService gachaService)
    {
        _gachaService = gachaService;
    }

    [HttpPost]
    public async Task<ActionResult<GachaDrawOutParam>> Draw([FromBody] GachaDrawInParam inParam)
    {
        // 1. ガチャの存在確認
        var gachaMaster = await _gachaService.GetGachaAsync(inParam.GachaId);
        if (gachaMaster is null)
        {
            return NotFound(new { message = $"gacha_id={inParam.GachaId} は gacha_master に存在しない。" });
        }

        // 2. 10連抽選
        var drawnItemIds = await _gachaService.DrawManyAsync(inParam.GachaId, DrawCount);

        // 3. isNew は 10連内での初出のみ true
        // TODO(追加課題A): GachaDrawInParam に UserId を追加して受け取り、「そのユーザーが既に持っているアイテムは isNew=false」になるように下のロジックを書き換えよう。
        var seenItemIds = new HashSet<int>();
        var gachaDrawResults = drawnItemIds
            .Select(itemId => new GachaDrawResult
            {
                ItemId = itemId,
                IsNew = seenItemIds.Add(itemId)
            })
            .ToList();

        return Ok(new GachaDrawOutParam { Results = gachaDrawResults });
    }
}
