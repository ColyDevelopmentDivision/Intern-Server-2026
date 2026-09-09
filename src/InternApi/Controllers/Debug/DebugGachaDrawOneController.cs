using InternApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace InternApi.Controllers.Debug;

/// <summary>
/// デバッグ用: 指定ガチャで1回だけ抽選する (DrawOneAsync)。
/// </summary>
[ApiController]
[Route("api/debug/gacha-draw-one")]
public class DebugGachaDrawOneController : ControllerBase
{
    private readonly GachaService _gachaService;

    public DebugGachaDrawOneController(GachaService gachaService)
    {
        _gachaService = gachaService;
    }

    /// <summary>
    /// GachaService.DrawOneAsync を単発で呼び出す。10 連ではなく1回だけ抽選する。
    /// </summary>
    /// <param name="gachaId">対象ガチャ id (gacha_master.gacha_id)</param>
    /// <returns>{ gachaId, itemId } の JSON</returns>
    [HttpPost]
    public async Task<ActionResult<object>> DrawOne(int gachaId)
    {
        var itemId = await _gachaService.DrawOneAsync(gachaId);
        return Ok(new { gachaId, itemId });
    }
}
