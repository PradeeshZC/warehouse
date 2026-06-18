#nullable enable
namespace Warehouse.Models.DTOs
{
    public static class Result
    {
        public static BaseResponse<T> Ok<T>(T data, string? message = null)
        {
            return new BaseResponse<T> { Success = true, Data = data, Message = message };
        }

        public static BaseResponse<T> Fail<T>(string message)
        {
            return new BaseResponse<T> { Success = false, Message = message };
        }
    }
}
