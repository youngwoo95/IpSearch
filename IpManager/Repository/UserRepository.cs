using IpManager.Comm.Logger.LogFactory.LoggerSelect;
using IpManager.DBModel;
using IpManager.DTO;
using Microsoft.EntityFrameworkCore.Storage;
using MySqlConnector;

namespace IpManager.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly IpanalyzeContext context;

        public UserRepository(IpanalyzeContext _context)
        {
            this.context = _context;

        }


        /// <summary>
        /// 회원가입
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<int> AddUserAsync(RegistrationDTO model)
        {
            // ExecutionStrategy 생성
            IExecutionStrategy strategy = context.Database.CreateExecutionStrategy();

            // ExecutionStrategy를 통해 트랜잭션 재시도 가능
            return await strategy.ExecuteAsync(async () =>
            {
                using (IDbContextTransaction transaction = await context.Database.BeginTransactionAsync())
                {
                    try
                    {

                    }catch(Exception ex)
                }

                    return 0;
            });


            return 0;
        }

        /// <summary>
        /// 데드락 감지 코드
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private bool IsDeadlockException(Exception ex)
        {
            // MySqlException 및 MariaDB의 교착 상태 오류 코드는 1213임.
            if (ex is MySqlException mysqlEx && mysqlEx.Number == 1213)
                return true;

            if (ex.InnerException is MySqlException innerMySqlEx && innerMySqlEx.Number == 1213)
                return true;

            return false;
        }
    }
}
