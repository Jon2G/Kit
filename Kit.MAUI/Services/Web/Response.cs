using System.Text;

namespace Kit.Services.Web
{
    public class Response<T> : Response
    {
        public new static Response<T> Error => new(APIResponseResult.INTERNAL_ERROR, "ERROR");
        public new static Response<T> InvalidRequest => new(APIResponseResult.INVALID_REQUEST, "!Solicitud invalida!");
        // public new static Response<T> NotExecuted => new(APIResponseResult.NOT_EXECUTED, "!Solicitud invalida/no ejecutada!");
        public new static Response<T> Offline => new(APIResponseResult.INTERNAL_ERROR, "El servicio web no esta dispobile por el momento.");
        public T Extra { get; set; }
        public Response(APIResponseResult ResponseResult, string Message, T Extra = default(T))
            : base(ResponseResult, Message)
        {

            this.Extra = Extra;
        }
        public Response()
        {

        }
        public Response<T> SetExtra(T v)
        {
            this.Extra = v;
            return this;
        }
        public static new Response<T> FromJson(string Json) => Newtonsoft.Json.JsonConvert.DeserializeObject<Response<T>>(Json);
        public Response<T> Log()
        {
            Kit.Log.Logger.Debug(this.ToString());
            return this;
        }
    }
    public class Response
    {
        public APIResponseResult ResponseResult { get; set; }
        public string Message { get; set; }
        public static Response Error => new(APIResponseResult.INTERNAL_ERROR, "ERROR");
        public static Response InvalidRequest => new(APIResponseResult.INVALID_REQUEST, "!Solicitud invalida!");
        // public static Response NotExecuted => new(APIResponseResult.NOT_EXECUTED, "!Solicitud invalida/no ejecutada!");
        public static Response Offline => new(APIResponseResult.INTERNAL_ERROR, "El servicio web no esta dispobile por el momento.");

        public Response(APIResponseResult ResponseResult, string Message)
        {
            this.ResponseResult = ResponseResult;
            this.Message = Message;
        }

        public Response Log()
        {
            Kit.Log.Logger.Debug(this.ToString());
            return this;
        }

        public Response()
        {

        }
        public string ToJson() => Newtonsoft.Json.JsonConvert.SerializeObject(this);

        public virtual Response FromJson(string Json) => Newtonsoft.Json.JsonConvert.DeserializeObject<Response>(Json);
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(ResponseResult).Append(" - ").Append(Message);
            return sb.ToString();
        }
    }
}
