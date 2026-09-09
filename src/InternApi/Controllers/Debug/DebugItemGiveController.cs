using InternApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace InternApi.Controllers.Debug;

/// <summary>
/// デバッグ用: 指定ユーザーに任意のアイテムを付与する。
/// </summary>
[ApiController]
[Route("api/debug/item-give")]
public class DebugItemGiveController : ControllerBase
{
    private readonly UserItemService _userItemService;

    public DebugItemGiveController(UserItemService userItemService)
    {
        _userItemService = userItemService;
    }

    /// <summary>
    /// UserItemService.GrantAsync を単発で呼び出してアイテムを付与する。
    /// </summary>
    /// <param name="userId">対象ユーザー id</param>
    /// <param name="itemId">付与するアイテム id</param>
    /// <param name="grantCount">付与する個数</param>
    /// <returns>{ userId, itemId, grantCount, message } の JSON</returns>
    [HttpPost]
    public async Task<ActionResult<object>> Give(
        long userId,
        long itemId,
        uint grantCount)
    {
        await _userItemService.GrantAsync(userId, itemId, grantCount);
        return Ok(new { userId, itemId, grantCount, message = "given" });
    }
}
