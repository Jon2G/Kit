using System.Net;

namespace Kit.Services.Web
{
    public struct ResponseResult
    {
        public HttpStatusCode HttpStatusCode;
        public string Response;
        public string Extra;

        public ResponseResult(HttpStatusCode httpStatusCode, string response, string Extra)
        {
            HttpStatusCode = httpStatusCode;
            Response = response;
            this.Extra = Extra;
        }
    }
}