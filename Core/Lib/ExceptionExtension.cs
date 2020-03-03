using Core.Model;
using System;

namespace Core.Lib
{
    public static class ExceptionExtension
    {
		public static BaseResponse<T> PopulateResponseObject<T>(this Exception ex, BaseResponse<T> response)
		{
			response.Exception = ex;
			response.Success = false;
			response.Message = ex.Message;
			return response;
		}
	}
}
