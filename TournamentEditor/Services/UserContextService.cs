using Microsoft.AspNetCore.Http;

namespace TournamentEditor.Services;

/// <summary>
/// ユーザーコンテキストサービス
/// 現在のユーザーIDを管理する
/// </summary>
public class UserContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string UserIdSessionKey = "UserId";

    public UserContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 現在のユーザーIDを取得（セッションから取得または新規生成）
    /// </summary>
    public string GetUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return "anonymous";
        }

        var userId = httpContext.Session.GetString(UserIdSessionKey);

        if (string.IsNullOrEmpty(userId))
        {
            // 新しいユーザーIDを生成してセッションに保存
            userId = Guid.NewGuid().ToString();
            httpContext.Session.SetString(UserIdSessionKey, userId);
        }

        return userId;
    }

    /// <summary>
    /// ユーザーIDを設定（将来の認証システム用）
    /// </summary>
    public void SetUserId(string userId)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            httpContext.Session.SetString(UserIdSessionKey, userId);
        }
    }

    /// <summary>
    /// セッションをクリア
    /// </summary>
    public void ClearSession()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        httpContext?.Session.Clear();
    }
}
