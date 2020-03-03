using Core.Model;
using Core.Model.DTO;
using Core.Model.DTO.Request;

namespace Core.Contract.Bll
{
    public interface IAuthenticationBll
    {
        BaseResponse<CredentialsDTO> GenerateBearerToken(BearerRequestDTO request);
    }
}
