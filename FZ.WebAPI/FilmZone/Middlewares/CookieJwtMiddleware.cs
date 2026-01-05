namespace FilmZone.Middlewares
{
    public sealed class CookieJwtMiddleware
    {
        private readonly RequestDelegate _next;
        private const string AccessCookieName = "at"; // đổi nếu bạn đặt tên khác

        public CookieJwtMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext context)
        {
            // Nếu chưa có Authorization header, thử lấy từ cookie "at"
            if (!context.Request.Headers.ContainsKey("Authorization"))
            {
                if (context.Request.Cookies.TryGetValue(AccessCookieName, out var token) &&
                    !string.IsNullOrWhiteSpace(token))
                {
                    context.Request.Headers["Authorization"] = $"Bearer {token}";
                }
            }

            await _next(context);
        }
    }
}
