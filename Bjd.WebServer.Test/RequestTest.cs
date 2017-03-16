using System;
using Bjd.WebServer;
using Xunit;
using Bjd.Services;
using Xunit.Abstractions;

namespace WebServerTest
{

    public class RequestTest : IDisposable
    {

        TestService _service;
        HttpRequest _request;

        public RequestTest(ITestOutputHelper output)
        {
            _service = TestService.CreateTestService();
            _service.AddOutput(output);

            _request = new HttpRequest(_service.Kernel, null, null);

        }

        public void Dispose()
        {
            _service.Dispose();

        }

        [Theory]
        [InlineData("GET / HTTP/0.9", "HTTP/0.9")]
        [InlineData("GET / HTTP/1.0", "HTTP/1.0")]
        [InlineData("GET / HTTP/1.1", "HTTP/1.1")]
        [InlineData("GET / HTTP/2.0", null)] //未対応バージョン
        public void VerTest(string requestStr, string verStr)
        {
            if (verStr == null)
            {
                bool b = _request.Init(requestStr);
                Assert.Equal(b, false);
                return;
            }
            if (_request.Init(requestStr))
            {
                Assert.Equal(_request.Ver, verStr);
                return;
            }
            Assert.Equal(_request.Ver, "ERROR");
        }

        [Theory]
        [InlineData("GET / HTTP/1.1", HttpMethod.Get)]
        [InlineData("POST / HTTP/1.1", HttpMethod.Post)]
        [InlineData("PUT / HTTP/1.1", HttpMethod.Put)]
        [InlineData("OPTIONS / HTTP/1.1", HttpMethod.Options)]
        [InlineData("HEAD / HTTP/1.1", HttpMethod.Head)]
        [InlineData("MOVE / HTTP/1.1", HttpMethod.Move)]
        [InlineData("PROPFIND / HTTP/1.1", HttpMethod.Propfind)]
        [InlineData("PROPPATCH / HTTP/1.1", HttpMethod.Proppatch)]
        public void MethodTest(string requestStr, HttpMethod method)
        {
            if (!_request.Init(requestStr))
            {
                Assert.Equal(_request.Method.ToString(), "ERROR");
                return;
            }
            Assert.Equal(_request.Method, method);
        }



        [Theory]
        [InlineData(102, "Processiong")]
        [InlineData(200, "Document follows")]
        [InlineData(201, "Created")]
        [InlineData(204, "No Content")]
        [InlineData(206, "Partial Content")]
        [InlineData(207, "Multi-Status")]
        [InlineData(301, "Moved Permanently")]
        [InlineData(302, "Moved Temporarily")]
        [InlineData(304, "Not Modified")]
        [InlineData(400, "Missing Host header or incompatible headers detected.")]
        [InlineData(401, "Unauthorized")]
        [InlineData(402, "Payment Required")]
        [InlineData(403, "Forbidden")]
        [InlineData(404, "Not Found")]
        [InlineData(405, "Method Not Allowed")]
        [InlineData(412, "Precondition Failed")]
        [InlineData(422, "Unprocessable")]
        [InlineData(423, "Locked")]
        [InlineData(424, "Failed Dependency")]
        [InlineData(500, "Internal Server Error")]
        [InlineData(501, "Request method not implemented")]
        [InlineData(507, "Insufficient Storage")]
        [InlineData(0, "")]
        [InlineData(-1, "")]
        public void StatusMessageTest(int code, string msg)
        {
            var s = WebServerUtil.StatusMessage(code);
            Assert.Equal(s, msg);

        }




    }
}
