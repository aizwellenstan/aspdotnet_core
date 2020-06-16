using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DemoLoggingHttpRequest.Middlewares
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            /*
              HTTPリクエストボディは特別なストリームであり、一度しか読み取れないことに注意してください
              そして、HTTP ConetxtがMVCメディエーションプログラムを通過すると、HTTPリクエスト本文は読み取られたために消えます
              パイプラインプロセス全体で元のHTTPリクエストボディデータを保持する場合は、MVCメディエーションプロセスの前に関連データを一時的に保存します
              その後の使用のために
             */
            var log = $"{context.Request.Path}, {context.Request.Method}, {ReadRequestBody(context)}";
            _logger.LogTrace(log);

            await _next.Invoke(context);
        }

        /// <summary>
        /// Read HTTP Request Body
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns></returns>
        private string ReadRequestBody(HttpContext context)
        {
            // HTTPリクエストが複数回読み取れることを確保します
            context.Request.EnableBuffering();
            // HTTPリクエスト本文のコンテンツを読み取ります
            // 注意！ leaveOpenプロパティをtrueに設定してStreamReaderを閉じると、HTTP要求のストリームは閉じられません。
            using (var bodyReader = new StreamReader(stream: context.Request.Body,
                                                      encoding: Encoding.UTF8,
                                                      detectEncodingFromByteOrderMarks: false,
                                                      bufferSize: 1024,
                                                      leaveOpen: true))
            {
                var body = bodyReader.ReadToEnd();

                // HTTPリクエストのストリーム開始位置をゼロにします
                context.Request.Body.Position = 0;

                return body;
            }
        }
    }

    public static class LoggingMiddlewareExtensions
    {
        /// <summary>Collect Http Data in Middleware</summary>
        /// <param name="builder">Middleware Builder</param>
        /// <returns></returns>
        public static IApplicationBuilder UseLoggingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LoggingMiddleware>();
        }
    }
}
