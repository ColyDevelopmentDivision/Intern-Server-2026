namespace InternApi.Models;

/// <summary>
/// POST /api/user/create のリクエストDTO。
/// name のみを受け取るダミー実装 (パスワードなし)。
/// </summary>
public class UserCreateInParam
{
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// POST /api/user/create のレスポンスDTO。
/// 生成された userId と、受け取った name をそのまま返す。
/// </summary>
public class UserCreateOutParam
{
    public long UserId { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// POST /api/user/login のリクエストDTO。
/// name をキーとしてユーザーを検索するだけのダミー実装。
/// </summary>
public class LoginInParam
{
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// POST /api/user/login のレスポンスDTO。
/// 該当ユーザーの userId と name を返す。トークン発行等は行わない。
/// </summary>
public class LoginOutParam
{
    public long UserId { get; set; }
    public string Name { get; set; } = string.Empty;
}
