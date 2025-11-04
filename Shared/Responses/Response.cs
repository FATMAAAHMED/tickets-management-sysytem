using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Responses
{
        public class Response<T>
        {
            public Response() { }

            public Response(bool isSuccess, string status, string code, string message, T? payload)
            {
                IsSuccess = isSuccess;
                Status = status;
                Code = code;
                Message = message;
                Payload = payload;
            }

            [Display(Description = "Status of the response")]
            [DataMember]
            public string Status { get; set; } = string.Empty;

            [DataMember]
            public string Code { get; set; } = string.Empty;

            [DataMember]
            public string Message { get; set; } = string.Empty;

            [DataMember]
            public T? Payload { get; set; }

            [Display(Description = "Indicates if the response is successful")]
            [DataMember]
            public bool IsSuccess { get; set; }

            // Helper methods for easy creation
            public static Response<T> Success(T payload, string message = "Success", string code = "Ok")
                => new(true, "success", code, message, payload);

            public static Response<T> Fail(string message, string code = "Failed")
                => new(false, "fail", code, message, default);
        }
    


}
