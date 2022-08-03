using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Serilog;

namespace WebApplication1.Middlewares
{
    class SerilogRequestBodyMiddleware
    {
        private readonly RequestDelegate _next;

        /// <inheritdoc />
        public SerilogRequestBodyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// 读取请求内容
        /// </summary>
        private static async Task<string> ReadRequestBody(HttpRequest request)
        {
            if (request.Method == HttpMethods.Get)
            {
                return String.Empty;
            }
            if (request.ContentType == null || !request.ContentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
            {
                return String.Empty;
            }

            //This line allows us to set the reader for the request back at the beginning of its stream.
            request.EnableBuffering();
            var body = request.Body;

            //We now need to read the request stream.  First, we create a new byte[] with the same length as the request stream...
            var buffer = new byte[Convert.ToInt32(request.ContentLength)];

            //...Then we copy the entire request stream into the new buffer.
            _ = await request.Body.ReadAsync(buffer, 0, buffer.Length);

            //We convert the byte[] into a string using UTF8 encoding...
            var bodyAsText = Encoding.UTF8.GetString(buffer);

            //..and finally, assign the read body back to the request body, which is allowed because of EnableRewind()
            request.Body = body;
            request.Body.Seek(0, SeekOrigin.Begin);

            return bodyAsText;
        }

        /// <summary>
        /// 读取输出内容
        /// </summary>
        private static async Task<string> ReadResponseBody(HttpResponse response)
        {
            //We need to read the response stream from the beginning...
            response.Body.Seek(0, SeekOrigin.Begin);

            if (response.ContentType == null || !response.ContentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
            {
                return String.Empty;
            }

            //...and copy it into a string
            var text = await new StreamReader(response.Body).ReadToEndAsync();

            //We need to reset the reader for the response so that the client can read it.
            response.Body.Seek(0, SeekOrigin.Begin);

            //Return the string for the response, including the status code (e.g. 200, 404, 401, etc.)
            return text;
        }

        public async Task Invoke(HttpContext context, IDiagnosticContext diagnosticContext)
        {
            if (context == null)
            {
                return;
            }

            var requestBody = await ReadRequestBody(context.Request);
            diagnosticContext.Set("RequestBody", requestBody);

            //Copy a pointer to the original response body stream
            var originalBodyStream = context.Response.Body;
            //Create a new memory stream...
            using var responseStream = new MemoryStream();
            //...and use that for the temporary response body
            context.Response.Body = responseStream;

            await _next(context);

            //Format the response from the server
            var responseBody = await ReadResponseBody(context.Response);
            diagnosticContext.Set("ResponseBody", responseBody);
            //Copy the contents of the new memory stream (which contains the response) to the original stream, which is then returned to the client.
            await responseStream.CopyToAsync(originalBodyStream);
        }
    }

    public static class SerilogRequestBodyMiddlewareExtensions
    {
        public static IApplicationBuilder UseSerilogRequestLoggingPro(this IApplicationBuilder app)
        {
            app.UseSerilogRequestLogging(options =>
            {
                options.MessageTemplate = "HTTP {RequestId} {RequestHost} {IpAddress} {StatusCode} {Elapsed:0} {RequestMethod} {RequestPath} {RequestBody} {ResponseBody}";
                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("IpAddress", httpContext.Connection.RemoteIpAddress);
                    diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                    diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                    diagnosticContext.Set("RequestUrl", httpContext.Request.GetDisplayUrl());
                };
            });
            // must after `app.UseSerilogRequestLogging()`
            return app.UseMiddleware<SerilogRequestBodyMiddleware>();
        }
    }
}

