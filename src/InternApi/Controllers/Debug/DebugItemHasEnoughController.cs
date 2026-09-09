using InternApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace InternApi.Controllers.Debug;

/// <summary>
/// デバッグ用: 指定ユーザーが item_id を requiredCount 個以上持っているかを判定する。
/// 本番システムには存在しない想定。
/// </summary>
[ApiController]
[Route("api/debug/item-has-enough")]
public class DebugItemHasEnoughController : ControllerBase
{
    private readonly UserItemService _userItemService;

    public DebugItemHasEnoughController(UserItemService userItemService)
    {
        _userItemService = userItemService;
    }

    /// <summary>
    /// UserItemService.HasEnoughAsync を単発で呼び出す。
    /// </summary>
    /// <param name="userId">対象ユーザー id</param>
    /// <param name="itemId">対象アイテム id</param>
    /// <param name="requiredCount">必要な所持数</param>
    /// <returns>{ userId, itemId, requiredCount, hasEnough } の JSON</returns>
    [HttpGet]
    public async Task<ActionResult<object>> HasEnough(
        long userId,
        long itemId,
        uint requiredCount)
    {
        var hasEnough = await _userItemService.HasEnoughAsync(userId, itemId, requiredCount);
        return Ok(new { userId, itemId, requiredCount, hasEnough });
    }
}
