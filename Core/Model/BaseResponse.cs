using System;

namespace Core.Model
{
	public class BaseResponse<T>
	{
		public BaseResponse()
		{
			Message = string.Empty;
			Success = true;
		}

		public T Data { get; set; }
		public string Message { get; set; }
		public bool Success { get; set; }

		public Exception Exception { get; set; }
	}
}
