using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Kit.Services.Web
{
    public sealed class WebService
    {
        private readonly string Url;
        private static HttpClientHandler _HttpClientHandler;
        private static HttpClientHandler HttpClientHandler
        {
            get
            {
                if (_HttpClientHandler is not null)
                    return _HttpClientHandler;

                _HttpClientHandler = new HttpClientHandler()
                {
                    Proxy = null,
                    UseProxy = false
                };
                //if ((Tools.Instance?.RuntimePlatform ?? RuntimePlatform.Unknown) != Enums.RuntimePlatform.WPF)
                //{
                try
                {
                    _HttpClientHandler.ServerCertificateCustomValidationCallback =
                        (message, cert, chain, errors) => true;
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "HttpClientHandler");
                }

                //}
                return _HttpClientHandler;
            }
        }

        public static HttpClient HttpClient = new HttpClient(HttpClientHandler);

        public WebService(string Url)
        {
            this.Url = Url;
        }

        private string BuildUrl(string metodo, Dictionary<string, string> query, params string[] parameters)
        {
            StringBuilder sb_parameters = new StringBuilder("/");
            if (parameters != null && parameters.Length > 0)
            {
                foreach (string oneParameter in parameters.Where(x => x is not null))
                {
                    sb_parameters.AppendFormat("{0}/", Uri.EscapeDataString(oneParameter));
                }
            }
            if (sb_parameters.Last() == '/')
            {
                sb_parameters = sb_parameters.Remove(sb_parameters.Length - 1, 1);
            }
            if (query != null && query.Any())
            {
                var first = query.First();
                sb_parameters.Append($"?{first.Key}={Uri.EscapeDataString(first.Value)}");
                for (int i = 1; i < query.Count; i++)
                {
                    var oneParameter = query.ElementAt(i);
                    sb_parameters.AppendFormat("&{0}={1}", oneParameter.Key, Uri.EscapeDataString(oneParameter.Value));
                }
            }
            StringBuilder sb = new StringBuilder(this.Url);
            if (sb.Last() != '/')
            {
                sb.Append('/');
            }
            sb.Append(metodo).Append(sb_parameters);
            return sb.ToString();
        }

        public Task<Kit.Services.Web.ResponseResult> GET(string metodo, params string[] parameters) => GET(metodo, null, null, parameters);

        public Task<Kit.Services.Web.ResponseResult> GET(string metodo, Dictionary<string, string> query, params string[] parameters)
            => GET(metodo, null, query, parameters);
        public async Task<Kit.Services.Web.ResponseResult> GET(string metodo, int? timeOut = null, Dictionary<string, string> query = null, params string[] parameters)
        {
            await Task.Yield();
            Kit.Services.Web.ResponseResult result = new Kit.Services.Web.ResponseResult
            {
                HttpStatusCode = HttpStatusCode.Unused
            };
            string GetUrl = BuildUrl(metodo, query, parameters);
            string responseText = string.Empty;
            try
            {
                if (timeOut is int tt)
                    HttpClient.Timeout = TimeSpan.FromMilliseconds(tt);
                HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                result.Response = await HttpClient.GetStringAsync(GetUrl);
                result.HttpStatusCode = HttpStatusCode.OK;
            }
            catch (WebException ex)
            {
                Log.Logger.Error(ex, $"GET: {this.Url}");
                result.HttpStatusCode = (HttpStatusCode)ex.Status;
                result.Extra = ex.Message;
                if (ex.Status == WebExceptionStatus.ProtocolError && ex.Response != null)
                {
                    using (StreamReader reader = new StreamReader(((HttpWebResponse)ex.Response).GetResponseStream()))
                    {
                        responseText = reader.ReadToEnd();
                    }
                }
                result.Response = "ERROR";
                return result;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"GET: {GetUrl}");
                result.Response = "ERROR";
                result.Extra = ex.Message;
            }
            return result;
        }

        public async Task<Kit.Services.Web.ResponseResult> Post(string method,
            Dictionary<string, string> query, params string[] parameters)
        {
            Kit.Services.Web.ResponseResult result = new Kit.Services.Web.ResponseResult
            {
                HttpStatusCode = HttpStatusCode.Unused
            };
            string geturl = String.Empty;
            try
            {
                geturl = BuildUrl(method, query, parameters);
                HttpResponseMessage message = await HttpClient.PostAsync(geturl, null);
                result.HttpStatusCode = message.StatusCode;
                result.Response = await message.Content.ReadAsStringAsync();



                return result;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"GET: {geturl}");
                result.Response = "ERROR";
                return result;
            }
        }
        public Task<Kit.Services.Web.ResponseResult> PostAsBody(string body, string metodo, string mediaType = "application/octet-stream", params string[] parameters) => PostAsBody(Encoding.UTF8.GetBytes(body), metodo, null, mediaType, parameters: parameters);

        public Task<Kit.Services.Web.ResponseResult> PostAsBody(byte[] byteArray, string metodo, params string[] parameters) => PostAsBody(byteArray, metodo, null, parameters: parameters);

        public async Task<Kit.Services.Web.ResponseResult> PostAsBody(byte[] byteArray, string method, Dictionary<string, string> query, string mediaType = "application/octet-stream", params string[] parameters)
        {
            Kit.Services.Web.ResponseResult result = new Kit.Services.Web.ResponseResult
            {
                HttpStatusCode = HttpStatusCode.Unused
            };
            string geturl = String.Empty;
            try
            {
                geturl = BuildUrl(method, query, parameters);
                var body = new ByteArrayContent(byteArray);
                body.Headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);
                HttpResponseMessage message = await HttpClient.PostAsync(geturl, body);
                result.HttpStatusCode = message.StatusCode;
                result.Response = await message.Content.ReadAsStringAsync();



                return result;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"GET: {geturl}");
                result.Response = "ERROR";
                return result;
            }
        }


        public Task<Stream> DownloadFile(string metodo, params string[] parameters) => DownloadFile(metodo, null, parameters);

        public async Task<Stream> DownloadFile(string method, Dictionary<string, string> query, params string[] parameters)
        {
            Kit.Services.Web.ResponseResult result = new Kit.Services.Web.ResponseResult
            {
                HttpStatusCode = HttpStatusCode.Unused
            };
            string geturl = String.Empty;
            try
            {
                geturl = BuildUrl(method, query, parameters);

                using (var httpResponse = await HttpClient.GetAsync(geturl))
                {
                    if (httpResponse.StatusCode == HttpStatusCode.OK)
                    {
                        return new MemoryStream(await httpResponse.Content.ReadAsByteArrayAsync());
                    }
                    else
                    {
                        //Url is Invalid
                        return null;
                    }
                }
                //client.DefaultRequestHeaders.Add("Accept", "application/json");
                //result.Response = await client.GetStringAsync(GetUrl);
                //result.HttpStatusCode = HttpStatusCode.OK;

            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"GET: {geturl}");
                result.Response = "ERROR";
                return null;
            }
        }
    }
}