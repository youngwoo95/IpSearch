using IpManager.DTO;

namespace IpManager.Repository
{
    public interface IUserRepository
    {
        /// <summary>
        /// 회원가입
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<int> AddUserAsync(RegistrationDTO model);


    }
}
