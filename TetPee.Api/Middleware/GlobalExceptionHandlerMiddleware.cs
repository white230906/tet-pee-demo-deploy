using TetPee.Service.Models;

namespace TetPee.Api.Middleware;

    public class GlobalExceptionHandlerMiddleware : IMiddleware
    {
        private readonly IHostEnvironment _environment;
        // Biết app đang chạy ở:
        //  Môi trường dev
        //  Môi trường production
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        //Dùng để ghi ra lỗi

        public GlobalExceptionHandlerMiddleware(
            IHostEnvironment environment,
            ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);//cho request đi tiếp 
            }
            catch (Exception ex)
            {//ghi lại lỗi ex, ghi luôn API bị lỗi
                _logger.LogError(ex, "Unhandled exception occurred while processing request {Path}", 
                                context.Request.Path);

                if (context.Response.HasStarted)
                {
                    // Lỗi này lỗi nâng cao
                    _logger.LogWarning("The response has already started, the global exception middleware will not write an error response");
                    throw;
                }

                var statusCode = MapStatusCode(ex);
                // xác định status code
                // convert lỗi -> HTTP code 
                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json"; //trả về dạng json 
                
                //tạo response error
                var response = ApiResponseFactory.ErrorResponse(
                    message: ResolveClientMessage(ex, statusCode),
                    errors: _environment.IsDevelopment() ? new { detail = ex.Message } : null, 
                    //nếu đang dev thì show lỗi thật, nếu đang production thì ẩn lỗi
                    traceId: context.TraceIdentifier);
                    //Id của request

                await context.Response.WriteAsJsonAsync(response);
                //gửi json response rồi cho client
            }
        }

        private static int MapStatusCode(Exception ex)
        {
            return ex switch // lỗi thuộc cái nào 
            {
                ArgumentException => StatusCodes.Status400BadRequest, // sai input -> 400
                InvalidOperationException => StatusCodes.Status400BadRequest, // sai input -> 400
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,// chưa login trả -> 401
                KeyNotFoundException => StatusCodes.Status404NotFound, // không tìm thấy -> 404
                _ => StatusCodes.Status500InternalServerError // lỗi khác -> 500
            };
        }

        private static string ResolveClientMessage(Exception ex, int statusCode)
        {
            return statusCode >= 500 ? "An unexpected error occurred" : ex.Message;
        }
    }


// Global Exception Middleware là một cái phễu + lá chắn:
//   Mọi request đi vào → nó cho đi tiếp
// Nếu ở bất kỳ đâu phía sau (Controller / Service / Repository…) ném lỗi (throw exception)
//   nó hứng trọn
// Nó chuyển lỗi thành response chuẩn (JSON, status code)
// Server không bị crash
// Client luôn nhận response có format thống nhất