﻿using IpManager.Comm.Logger.LogFactory.LoggerSelect;
using IpManager.DBModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Transactions;

namespace IpManager.Repository.Country
{
    public class CountryRepository : ICountryRepository
    {
        private readonly ILoggerService LoggerService;
        private readonly IpanalyzeContext context;

        public CountryRepository(IpanalyzeContext _context,
            ILoggerService _loggerservice)
        {
            this.context = _context;
            this.LoggerService = _loggerservice;
        }

  

        /// <summary>
        /// (도/시) 리스트 반환
        /// </summary>
        /// <returns></returns>
        public async Task<List<CountryTb>?> GetCountryListAsync()
        {
            try
            {
                var results = new List<CountryTb>();

                var connection = context.Database.GetDbConnection();
                if(connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM country_tb WHERE DEL_YN != true";

                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while(await reader.ReadAsync())
                        {
                            var country = new CountryTb
                            {
                                Pid = reader.IsDBNull(reader.GetOrdinal("PID")) ? 0 : reader.GetInt32(reader.GetOrdinal("PID")),
                                Name = reader.IsDBNull(reader.GetOrdinal("NAME")) ? string.Empty : reader.GetString(reader.GetOrdinal("NAME")),
                                CreateDt = reader.IsDBNull(reader.GetOrdinal("CREATE_DT")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("CREATE_DT")),
                                UpdateDt = reader.IsDBNull(reader.GetOrdinal("UPDATE_DT")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("UPDATE_DT")),
                                DelYn = reader.IsDBNull(reader.GetOrdinal("DEL_YN")) ? false : Convert.ToBoolean(reader["DEL_YN"]),
                                DeleteDt = reader.IsDBNull(reader.GetOrdinal("DELETE_DT")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("DELETE_DT")),
                            };
                            
                            results.Add(country);
                        }
                    }
                }
                return results;
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 도시정보 삭제
        /// </summary>
        /// <param name="countrypid"></param>
        /// <returns></returns>
        public async Task<int> DeleteCountListAsync(List<int> countrypid)
        {
            try
            {
                bool fk_check = false;

                // 1) CityTb 검사
                fk_check = await context.PcroomTbs
                    .AnyAsync(pcroom => countrypid.Contains(pcroom.CountrytbId));
                if (fk_check == true)
                    return 0;

                fk_check = await context.CityTbs
                    .AnyAsync(city => countrypid.Contains(city.CountrytbId));
                if (fk_check == true)
                    return 0;

                fk_check = await context.LoginTbs
                    .AnyAsync(login => login.CountryId.HasValue
                                        && countrypid.Contains(login.CountryId.Value));
                if (fk_check == true)
                    return 0;

                fk_check = await context.TownTbs
                    .AnyAsync(town => countrypid.Contains(town.CountytbId));
                if (fk_check == true)
                    return 0;

                fk_check = await context.PinglogTbs
                    .AnyAsync(ping => countrypid.Contains(ping.CountrytbId));
                if (fk_check == true)
                    return 0;

                using (IDbContextTransaction transaction = await context.Database.BeginTransactionAsync().ConfigureAwait(false))
                {
                    foreach (int pid in countrypid)
                    {
                        var model = await context.CountryTbs.FirstOrDefaultAsync(m => m.Pid == pid);
                        model.Name = $"{model.Pid}_{model.Name}";
                        model.DeleteDt = DateTime.Now; // 현재시점
                        model.DelYn = true;

                        int result = await context.SaveChangesAsync().ConfigureAwait(false);
                        if (result < 1)
                        {
                            await transaction.RollbackAsync().ConfigureAwait(false);
                            return -1;
                        }
                    }
                    await transaction.CommitAsync().ConfigureAwait(false);
                }
                return 1;
            }
            catch (Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return -1;
            }
        }

    }
}
