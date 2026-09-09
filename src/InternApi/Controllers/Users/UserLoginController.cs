using InternApi.Models;
using InternApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace InternApi.Controllers.Users;

/// <summary>
/// ユーザーログイン API: POST /api/user/login
/// </summary>
[ApiController]
[Route("api/user/login")]
public class UserLoginController : ControllerBase
{
    private readonly UserService _userService;

    public UserLoginController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<ActionResult<LoginOutParam>> Login([FromBody] LoginInParam inParam)
    {
        // 1. 入力バリデーション
        if (string.IsNullOrWhiteSpace(inParam.Name))
        {
            return BadRequest(new { message = "name is required" });
        }

        // TODO: 今のままだと name がバレると他人にログインされてしまう。
        // それを防ぐには、リクエストにどんなパラメータを追加して、ここで何を検証すればいいか、
        // 考えて実装してみよう。

        // 2. Service に委譲。該当なしなら null が返ってくる
        var userId = await _userService.LoginAsync(inParam.Name);
        if (userId is null)
        {
            return NotFound(new { message = $"user '{inParam.Name}' not found" });
        }

        // 3. 成功: 該当ユーザーの userId と name を返す
        return Ok(new LoginOutParam
        {
            UserId = userId.Value,
            Name = inParam.Name
        });
    }
}
