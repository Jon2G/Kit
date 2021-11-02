﻿using Kit.Services.Web;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kit
{
    public static class ResponseExtenions
    {
        public static Response ToResponse(this ResponseResult response)
        {
            try
            {
                if (response.Response is { Length: > 0 })
                {
                    return JsonConvert.DeserializeObject<Response>(response.Response);
                }
                return new Response(APIResponseResult.INTERNAL_ERROR, "Empty response");
            }
            catch (Exception ex)
            {
                return new Response(APIResponseResult.INTERNAL_ERROR, ex.Message);
            }
        }

        public static T ToResponse<T>(this ResponseResult response) where T : Response, new()
        {
            try
            {
                if (response.Response is { Length: > 0 })
                {
                    return JsonConvert.DeserializeObject<T>(response.Response);
                }
                Response r = Activator.CreateInstance<T>();
                r.ResponseResult = APIResponseResult.INTERNAL_ERROR;
                r.Message = "Empty response";
                return (T)r;
            }
            catch (Exception ex)
            {
                Response r = Activator.CreateInstance<T>();
                r.ResponseResult = APIResponseResult.INTERNAL_ERROR;
                r.Message = ex.Message;
                return (T)r;
            }

        }
    }
}