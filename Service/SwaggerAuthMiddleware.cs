using System.Text;

namespace HTTL_May_Xay_Dung.Service
{
    public class SwaggerAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public SwaggerAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Kiểm tra xem request có phải Swagger không
            if (context.Request.Path.StartsWithSegments("/swagger"))
            {
                var authHeader = context.Request.Headers["Authorization"].ToString();

                // Nếu không có Header Authorization hoặc không hợp lệ
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic "))
                {
                    context.Response.Headers["WWW-Authenticate"] = "Basic";
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }

                // Giải mã thông tin đăng nhập
                var encodedUsernamePassword = authHeader.Substring("Basic ".Length).Trim();
                var decodedUsernamePassword = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword));
                var usernamePassword = decodedUsernamePassword.Split(':');

                // Lấy username và password từ thông tin đăng nhập
                var username = usernamePassword[0];
                var password = usernamePassword[1];

                // Kiểm tra thông tin đăng nhập (chỉnh sửa theo nhu cầu của bạn)
                if (username != "bach" || password != "bach")
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }
            }

            // Tiếp tục xử lý request nếu thông tin đăng nhập hợp lệ
            await _next(context);
        }
    }
}
