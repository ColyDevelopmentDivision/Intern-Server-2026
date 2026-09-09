using InternApi.Models;
using InternApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace InternApi.Controllers.Users;

/// <summary>
/// ユーザー新規作成 API: POST /api/user/create
/// </summary>
[ApiController]
[Route("api/user/create")]
public class UserCreateController : ControllerBase
{
    private readonly UserService _userService;

    public UserCreateController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<ActionResult<UserCreateOutParam>> Create([FromBody] UserCreateInParam inParam)
    {
        // 1. 入力バリデーション
        if (string.IsNullOrWhiteSpace(inParam.Name))
        {
            return BadRequest(new { message = "name is required" });
        }

        // 2. Service に委譲。UNIQUE 違反なら null が返ってくる
        var userId = await _userService.CreateAsync(inParam.Name);
        if (userId is null)
        {
            return Conflict(new { message = $"name '{inParam.Name}' already exists" });
        }

        // 3. 成功: 生成された userId と入力 name を返す
        return Ok(new UserCreateOutParam
        {
            UserId = userId.Value,
            Name = inParam.Name
        });
    }
}
