using InternApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace InternApi.Controllers.Debug;

/// <summary>
/// デバッグ用: 指定ユーザーの所持アイテムを消費する。
/// </summary>
[ApiController]
[Route("api/debug/item-consume")]
public class DebugItemConsumeController : ControllerBase
{
    private readonly UserItemService _userItemService;

    public DebugItemConsumeController(UserItemService userItemService)
    {
        _userItemService = userItemService;
    }

    /// <summary>
    /// UserItemService.ConsumeAsync を単発で呼び出して所持数を減らす。
    /// </summary>
    /// <param name="userId">対象ユーザー id</param>
    /// <param name="itemId">対象アイテム id</param>
    /// <param name="consumeCount">消費する個数</param>
    /// <returns>{ userId, itemId, consumeCount, message } の JSON</returns>
    [HttpPost]
    public async Task<ActionResult<object>> Consume(
        long userId,
        long itemId,
        uint consumeCount)
    {
        await _userItemService.ConsumeAsync(userId, itemId, consumeCount);
        return Ok(new { userId, itemId, consumeCount, message = "consumed" });
    }
}
