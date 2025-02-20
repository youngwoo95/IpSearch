using IpManager.Comm.Logger.LogFactory.LoggerSelect;
using IpManager.DBModel;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace IpManager.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ILoggerService LoggerService;
        private readonly IpanalyzeContext context;

        public UserRepository(IpanalyzeContext _context,
            ILoggerService _loggerservice)
        {
            this.context = _context;
            this.LoggerService = _loggerservice;
        }


        /// <summary>
        /// 회원가입
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<int> AddUserAsync(LoginTb model)
        {
            try
            {
                await context.LoginTbs.AddAsync(model).ConfigureAwait(false);
                int result = await context.SaveChangesAsync().ConfigureAwait(false);
                if (result > 0)
                    return 1;
                else
                    return 0;
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return -1;
            }
        }

        /// <summary>
        /// 사용자 ID 존재유무 검사
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public async Task<int> CheckUserIdAsync(string userid)
        {
            try
            {
                var connection = context.Database.GetDbConnection();
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM login_tb WHERE UID = @userid";
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "@userid";
                    parameter.Value = userid;
                    command.Parameters.Add(parameter);

                    var result = await command.ExecuteScalarAsync().ConfigureAwait(false);
                    int count = Convert.ToInt32(result);
                    if (count == 0)
                        return 0;// 해당 UID로 검색했을때 NULL이면 사용중인 아이디가 없는것이므로 0 반환
                    else
                        return 1; // 해당 UID로 검색했을때 NULL이 아니면 사용중인 아이디가 있으므로 1 반환
                }
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return -1; // 서버에 문제가 있을때 -1 반환해서 서비스 로직에 알림.
            }
        }

        /// <summary>
        /// 로그인
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="pw"></param>
        /// <returns></returns>
        public async Task<LoginTb?> GetLoginAsync(string userid, string pw)
        {
            try
            {
                var connection = context.Database.GetDbConnection();
                if(connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }
                using (var command = connection.CreateCommand())
                {
                    // SQL 조건 결합에는 AND를 사용합니다.
                    command.CommandText = "SELECT * FROM login_tb WHERE UID = @userid AND PWD = @password LIMIT 1";

                    // 각 파라미터를 별도로 생성하여 추가합니다.
                    var paramUser = command.CreateParameter();
                    paramUser.ParameterName = "@userid";
                    paramUser.Value = userid;
                    command.Parameters.Add(paramUser);

                    var paramPass = command.CreateParameter();
                    paramPass.ParameterName = "@password";
                    paramPass.Value = pw;
                    command.Parameters.Add(paramPass);

                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        if (await reader.ReadAsync())
                        {
                            // LoginTb에 매핑 (여기서는 예시로 몇 가지 컬럼만 매핑)
                            var login = new LoginTb
                            {
                                Pid = reader.IsDBNull(reader.GetOrdinal("PID")) ? 0 : reader.GetInt32(reader.GetOrdinal("PID")),
                                Uid = reader.IsDBNull(reader.GetOrdinal("UID")) ? string.Empty : reader.GetString(reader.GetOrdinal("UID")),
                                Pwd = reader.IsDBNull(reader.GetOrdinal("PWD")) ? string.Empty : reader.GetString(reader.GetOrdinal("PWD")),
                                MasterYn = reader.IsDBNull(reader.GetOrdinal("MASTER_YN")) ? false : Convert.ToBoolean(reader["MASTER_YN"]),
                                AdminYn = reader.IsDBNull(reader.GetOrdinal("ADMIN_YN")) ? false : Convert.ToBoolean(reader["ADMIN_YN"]),
                                UseYn = reader.IsDBNull(reader.GetOrdinal("USE_YN")) ? false : Convert.ToBoolean(reader["USE_YN"]),
                                CreateDt = reader.IsDBNull(reader.GetOrdinal("CREATE_DT")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("CREATE_DT")),
                                UpdateDt = reader.IsDBNull(reader.GetOrdinal("UPDATE_DT")) ? (DateTime?)null : Convert.ToDateTime(reader["UPDATE_DT"]),
                                DelYn = reader.IsDBNull(reader.GetOrdinal("DEL_YN")) ? false : Convert.ToBoolean(reader["DEL_YN"]),
                                DeleteDt = reader.IsDBNull(reader.GetOrdinal("DELETE_DT")) ? (DateTime?)null : Convert.ToDateTime(reader["DELETE_DT"])
                            };

                            return login;
                        }
                    }
                }
                return null; //조회 결과가 없다면 null을 반환한다.
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return null;
            }
        }

        public async Task<int> GetLoginPermission(string userid)
        {
            try
            {
                var connection = context.Database.GetDbConnection();
                if(connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM login_tb WHERE UID = @userid LIMIT 1";
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "@userid";
                    parameter.Value = userid;
                    command.Parameters.Add(parameter);

                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        if (await reader.ReadAsync())
                        {
                            // LoginTb에 매핑 (여기서는 예시로 몇 가지 컬럼만 매핑)
                            bool UserYN = Convert.ToBoolean(reader["USE_YN"]);
                            if (UserYN)
                                return 1; // 로그인 허용되어있음.
                            else
                                return 0; // 로그인 허용안되어있음
                        }
                    }
                }
                return 0; // ID가 존재하지않음 == 로그인 허용안되어있음
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return -1;
            }
        }

        /// <summary>
        /// USERID에 해당하는 UserModel 반환
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public async Task<LoginTb?> GetUserInfoAsync(string userid)
        {
            try
            {
                var connection = context.Database.GetDbConnection();
                if(connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM login_tb WHERE UID = @userid LIMIT 1";
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "@userid";
                    parameter.Value = userid;
                    command.Parameters.Add(parameter);

                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        if (await reader.ReadAsync())
                        {
                            // LoginTb에 매핑 (여기서는 예시로 몇 가지 컬럼만 매핑)
                            var login = new LoginTb
                            {
                                Pid = reader.IsDBNull(reader.GetOrdinal("PID")) ? 0 : reader.GetInt32(reader.GetOrdinal("PID")),
                                Uid = reader.IsDBNull(reader.GetOrdinal("UID")) ? string.Empty : reader.GetString(reader.GetOrdinal("UID")),
                                Pwd = reader.IsDBNull(reader.GetOrdinal("PWD")) ? string.Empty : reader.GetString(reader.GetOrdinal("PWD")),
                                MasterYn = reader.IsDBNull(reader.GetOrdinal("MASTER_YN")) ? false : Convert.ToBoolean(reader["MASTER_YN"]),
                                AdminYn = reader.IsDBNull(reader.GetOrdinal("ADMIN_YN")) ? false : Convert.ToBoolean(reader["ADMIN_YN"]),
                                UseYn = reader.IsDBNull(reader.GetOrdinal("USE_YN")) ? false : Convert.ToBoolean(reader["USE_YN"]),
                                CreateDt = reader.IsDBNull(reader.GetOrdinal("CREATE_DT")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("CREATE_DT")),
                                UpdateDt = reader.IsDBNull(reader.GetOrdinal("UPDATE_DT")) ? (DateTime?)null : Convert.ToDateTime(reader["UPDATE_DT"]),
                                DelYn = reader.IsDBNull(reader.GetOrdinal("DEL_YN")) ? false : Convert.ToBoolean(reader["DEL_YN"]),
                                DeleteDt = reader.IsDBNull(reader.GetOrdinal("DELETE_DT")) ? (DateTime?)null : Convert.ToDateTime(reader["DELETE_DT"])
                            };

                            return login;
                        }
                    }
                }

                return null;
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return null;
            }
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
