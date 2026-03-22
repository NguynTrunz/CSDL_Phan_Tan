namespace HRManagement.API.Middleware
{
    public class RoleAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public RoleAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Bỏ qua middleware cho endpoint login
            if (context.Request.Path.StartsWithSegments("/api/auth"))
            {
                await _next(context);
                return;
            }

            var role = context.Request.Headers["X-Role"].ToString();

            if (string.IsNullOrWhiteSpace(role))
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"message\":\"Thiếu thông tin xác thực (X-Role header).\"}");
                return;
            }

            var validRoles = new[] { "Admin", "KeToan", "IT" };
            if (!validRoles.Contains(role))
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync($"{{\"message\":\"Role '{role}' không hợp lệ.\"}}");
                return;
            }

            context.Items["UserRole"] = role;
            await _next(context);
        }
    }
}