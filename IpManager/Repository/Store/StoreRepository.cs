﻿using IpManager.Comm.Logger.LogFactory.LoggerSelect;
using IpManager.DBModel;
using IpManager.DTO.Store;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics;

namespace IpManager.Repository.Store
{
    public class StoreRepository : IStoreRepository
    {
        private readonly ILoggerService LoggerService;
        private readonly IpanalyzeContext context;

        public StoreRepository(IpanalyzeContext _context,
            ILoggerService _loggerservice)
        {
            this.context = _context;
            this.LoggerService = _loggerservice;
        }

        public async Task<int> AddPCRoomAsync(PcroomTb PcroomTB, CountryTb CountryTB, CityTb CityTB, TownTb TownTB)
        {
            IExecutionStrategy strategy = context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
#if DEBUG
                int result = 0;
                // 디버그 환경에서는 강제로 디버깅포인트 잡음
                Debugger.Break();
#endif
                using (IDbContextTransaction transaction = await context.Database.BeginTransactionAsync().ConfigureAwait(false))
                {
                    try
                    {
                        var PCroomCheck = await context.PcroomTbs
                        .FirstOrDefaultAsync(m => m.DelYn != true &&
                                                (m.Ip == PcroomTB.Ip && m.Port == PcroomTB.Port && m.Name == PcroomTB.Name && m.Addr == PcroomTB.Addr)
                                            );

                        if (PCroomCheck is not null)
                        {
                            // 이미 해당PC방이 저장되어 있다.
                            return 0; 
                        }

                        // 각 테이블에 데이터 INSERT
                        // (도/시 동일명칭 검사)
                        CountryTb? CountryCheck = await context.CountryTbs
                        .FirstOrDefaultAsync(m => m.Name == CountryTB.Name);

                        if (CountryCheck == null)
                        {
                            // ** (도/시) INSERT
                            CountryCheck = CountryTB; // 없으면 받아온 DB Data 참조복사
                            await context.CountryTbs.AddAsync(CountryCheck);
                            result = await context.SaveChangesAsync();
                            if(result < 1)
                            {
                                await transaction.RollbackAsync();
                                return -1; // 저장실패 - 대게 트랜잭션임
                            }
                        }

                        // (시/군/구 동일명칭 검사)
                        CityTb? CityCheck = await context.CityTbs
                        .FirstOrDefaultAsync(m => m.Name == CityTB.Name);

                        if(CityCheck == null)
                        {
                            // ** (시/군/구) INSERT
                            CityCheck = CityTB; // 없으면 받아온 DB Data 참조복사
                            CityCheck.CountrytbId = CountryCheck.Pid;
                            await context.CityTbs.AddAsync(CityCheck);
                            result = await context.SaveChangesAsync();
                            if(result < 1)
                            {
                                await transaction.RollbackAsync();
                                return -1; // 저장실패 - 대게 트랜잭션임
                            }
                        }

                        TownTb? TownCheck = await context.TownTbs
                        .FirstOrDefaultAsync(m => m.Name == TownTB.Name);

                        if(TownCheck == null)
                        {
                            TownCheck = TownTB;
                            TownCheck.CountytbId = CountryCheck.Pid;
                            TownCheck.CitytbId = CityCheck.Pid;
                            await context.TownTbs.AddAsync(TownCheck);
                            result = await context.SaveChangesAsync();
                            if(result < 1)
                            {
                                await transaction.RollbackAsync();
                                return -1; // 저장실패 - 대게 트랜잭션임
                            }
                        }

                        PcroomTB.CountrytbId = CountryCheck.Pid;
                        PcroomTB.CitytbId = CityCheck.Pid;
                        PcroomTB.TowntbId = TownCheck.Pid;
                        
                        // 변경사항 저장
                        await context.PcroomTbs.AddAsync(PcroomTB);
                        result = await context.SaveChangesAsync().ConfigureAwait(false);
                        if(result < 1)
                        {
                            await transaction.RollbackAsync();
                            return -1; // 저장실패 - 대게 트랜잭션임
                        }

                        // 트랜잭션 커밋
                        await transaction.CommitAsync().ConfigureAwait(false);
                        return 1; // 성공
                    }
                    catch (Exception ex)
                    {
                        // 예외 발생 시 롤백
                        await transaction.RollbackAsync().ConfigureAwait(false);
                        LoggerService.FileErrorMessage(ex.ToString());
                        return -99;
                    }
                }
            });
        }

        /// <summary>
        /// (도/시)별 PC방 리스트 반환
        /// </summary>
        /// <param name="countryid"></param>
        /// <returns></returns>
        public async Task<List<StoreListDTO>?> GetPcRoomCountryListAsync(int countryid)
        {
            try
            {
                var result = new List<StoreListDTO>();

                var connection = context.Database.GetDbConnection();
                if(connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"SELECT " +
                        $"pc.PID as PID," +
                        $"pc.IP as IP," +
                        $"pc.PORT as PORT," +
                        $"pc.NAME as NAME," +
                        $"pc.ADDR as ADDR," +
                        $"pc.SEATNUMBER as SEATNUMBER," +
                        $"pc.PRICE as PRICE," +
                        $"pc.PRICE_PERCENT as PRICE_PERCENT," +
                        $"pc.PC_SPEC as PC_SPEC," +
                        $"pc.TELECOM as TELECOM," +
                        $"pc.MEMO as MEMO," +
                        $"pc.CREATE_DT as CREATE_DT," +
                        $"pc.COUNTRYTB_ID as COUNTRYTB_ID," +
                        $"pc.CITYTB_ID as CITYTB_ID," +
                        $"pc.TOWNTB_ID as TOWNTB_ID " +
                        $"FROM pcroom_tb as pc " +
                        $"INNER JOIN country_tb as country on pc.COUNTRYTB_ID = country.PID " +
                        $"INNER JOIN city_Tb as city on PC.CITYTB_ID = city.PID " +
                        $"INNER JOIN town_tb as town on pc.TOWNTB_ID = town.PID " +
                        $"WHERE pc.DEL_YN != true " +
                        $"AND country.DEL_YN != true " +
                        $"AND city.DEL_YN != true " +
                        $"AND town.DEL_YN != true " +
                        $"AND pc.COUNTRYTB_ID = @countryid";

                    var countryParam = command.CreateParameter();
                    countryParam.ParameterName = "@countryid";
                    countryParam.Value = countryid;

                    command.Parameters.Add(countryParam);


                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while(await reader.ReadAsync())
                        {
                            var store = new StoreListDTO
                            {
                                Pid = reader.GetInt32(reader.GetOrdinal("PID")),
                                Ip = reader.GetString(reader.GetOrdinal("IP")),
                                Port = reader.GetInt32(reader.GetOrdinal("PORT")),
                                Name = reader.GetString(reader.GetOrdinal("NAME")),
                                Addr = reader.GetString(reader.GetOrdinal("ADDR")),
                                SeatNumber = reader.GetInt32(reader.GetOrdinal("SEATNUMBER")),
                                Price = (float)reader.GetDouble(reader.GetOrdinal("PRICE")), // 필요한 경우 형변환 처리
                                PricePercent = reader["PRICE_PERCENT"] as string,
                                Pcspec = reader["PC_SPEC"] as string,
                                Telecom = reader["TELECOM"] as string,
                                Memo = reader["MEMO"] as string,
                                CountryTbId = reader.GetInt32(reader.GetOrdinal("COUNTRYTB_ID")),
                                CityTbId = reader.GetInt32(reader.GetOrdinal("CITYTB_ID")),
                                TownTbId = reader.GetInt32(reader.GetOrdinal("TOWNTB_ID"))
                            };
                            result.Add(store);
                        }
                    }

                }

                return result;
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// (시/군/구)별 PC방 리스트 반환
        /// </summary>
        /// <param name="cityid"></param>
        /// <returns></returns>
        public async Task<List<StoreListDTO>?> GetPcRoomCityListAsync(int cityid)
        {
            try
            {
                var result = new List<StoreListDTO>();

                var connection = context.Database.GetDbConnection();
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"SELECT " +
                        $"pc.PID as PID," +
                        $"pc.IP as IP," +
                        $"pc.PORT as PORT," +
                        $"pc.NAME as NAME," +
                        $"pc.ADDR as ADDR," +
                        $"pc.SEATNUMBER as SEATNUMBER," +
                        $"pc.PRICE as PRICE," +
                        $"pc.PRICE_PERCENT as PRICE_PERCENT," +
                        $"pc.PC_SPEC as PC_SPEC," +
                        $"pc.TELECOM as TELECOM," +
                        $"pc.MEMO as MEMO," +
                        $"pc.CREATE_DT as CREATE_DT," +
                        $"pc.COUNTRYTB_ID as COUNTRYTB_ID," +
                        $"pc.CITYTB_ID as CITYTB_ID," +
                        $"pc.TOWNTB_ID as TOWNTB_ID " +
                        $"FROM pcroom_tb as pc " +
                        $"INNER JOIN country_tb as country on pc.COUNTRYTB_ID = country.PID " +
                        $"INNER JOIN city_Tb as city on PC.CITYTB_ID = city.PID " +
                        $"INNER JOIN town_tb as town on pc.TOWNTB_ID = town.PID " +
                        $"WHERE pc.DEL_YN != true " +
                        $"AND country.DEL_YN != true " +
                        $"AND city.DEL_YN != true " +
                        $"AND town.DEL_YN != true " +
                        $"AND pc.CITYTB_ID = @cityid";

                    var countryParam = command.CreateParameter();
                    countryParam.ParameterName = "@cityid";
                    countryParam.Value = cityid;

                    command.Parameters.Add(countryParam);


                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync())
                        {
                            var store = new StoreListDTO
                            {
                                Pid = reader.GetInt32(reader.GetOrdinal("PID")),
                                Ip = reader.GetString(reader.GetOrdinal("IP")),
                                Port = reader.GetInt32(reader.GetOrdinal("PORT")),
                                Name = reader.GetString(reader.GetOrdinal("NAME")),
                                Addr = reader.GetString(reader.GetOrdinal("ADDR")),
                                SeatNumber = reader.GetInt32(reader.GetOrdinal("SEATNUMBER")),
                                Price = (float)reader.GetDouble(reader.GetOrdinal("PRICE")), // 필요한 경우 형변환 처리
                                PricePercent = reader["PRICE_PERCENT"] as string,
                                Pcspec = reader["PC_SPEC"] as string,
                                Telecom = reader["TELECOM"] as string,
                                Memo = reader["MEMO"] as string,
                                CountryTbId = reader.GetInt32(reader.GetOrdinal("COUNTRYTB_ID")),
                                CityTbId = reader.GetInt32(reader.GetOrdinal("CITYTB_ID")),
                                TownTbId = reader.GetInt32(reader.GetOrdinal("TOWNTB_ID"))
                            };
                            result.Add(store);
                        }
                    }

                }

                return result;
            }
            catch (Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// (읍/면/동) 별 PC방 리스트 반환
        /// </summary>
        /// <param name="townid"></param>
        /// <returns></returns>
        public async Task<List<StoreListDTO>?> GetPcRoomTownListAsync(int townid)
        {
            try
            {
                var result = new List<StoreListDTO>();

                var connection = context.Database.GetDbConnection();
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"SELECT " +
                        $"pc.PID as PID," +
                        $"pc.IP as IP," +
                        $"pc.PORT as PORT," +
                        $"pc.NAME as NAME," +
                        $"pc.ADDR as ADDR," +
                        $"pc.SEATNUMBER as SEATNUMBER," +
                        $"pc.PRICE as PRICE," +
                        $"pc.PRICE_PERCENT as PRICE_PERCENT," +
                        $"pc.PC_SPEC as PC_SPEC," +
                        $"pc.TELECOM as TELECOM," +
                        $"pc.MEMO as MEMO," +
                        $"pc.CREATE_DT as CREATE_DT," +
                        $"pc.COUNTRYTB_ID as COUNTRYTB_ID," +
                        $"pc.CITYTB_ID as CITYTB_ID," +
                        $"pc.TOWNTB_ID as TOWNTB_ID " +
                        $"FROM pcroom_tb as pc " +
                        $"INNER JOIN country_tb as country on pc.COUNTRYTB_ID = country.PID " +
                        $"INNER JOIN city_Tb as city on PC.CITYTB_ID = city.PID " +
                        $"INNER JOIN town_tb as town on pc.TOWNTB_ID = town.PID " +
                        $"WHERE pc.DEL_YN != true " +
                        $"AND country.DEL_YN != true " +
                        $"AND city.DEL_YN != true " +
                        $"AND town.DEL_YN != true " +
                        $"AND pc.TOWNTB_ID = @townid";

                    var countryParam = command.CreateParameter();
                    countryParam.ParameterName = "@townid";
                    countryParam.Value = townid;

                    command.Parameters.Add(countryParam);


                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync())
                        {
                            var store = new StoreListDTO
                            {
                                Pid = reader.GetInt32(reader.GetOrdinal("PID")),
                                Ip = reader.GetString(reader.GetOrdinal("IP")),
                                Port = reader.GetInt32(reader.GetOrdinal("PORT")),
                                Name = reader.GetString(reader.GetOrdinal("NAME")),
                                Addr = reader.GetString(reader.GetOrdinal("ADDR")),
                                SeatNumber = reader.GetInt32(reader.GetOrdinal("SEATNUMBER")),
                                Price = (float)reader.GetDouble(reader.GetOrdinal("PRICE")), // 필요한 경우 형변환 처리
                                PricePercent = reader["PRICE_PERCENT"] as string,
                                Pcspec = reader["PC_SPEC"] as string,
                                Telecom = reader["TELECOM"] as string,
                                Memo = reader["MEMO"] as string,
                                CountryTbId = reader.GetInt32(reader.GetOrdinal("COUNTRYTB_ID")),
                                CityTbId = reader.GetInt32(reader.GetOrdinal("CITYTB_ID")),
                                TownTbId = reader.GetInt32(reader.GetOrdinal("TOWNTB_ID"))
                            };
                            result.Add(store);
                        }
                    }

                }

                return result;
            }
            catch (Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// PC방 정보 상세보기
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public async Task<StoreDetailDTO?> GetPcRoomInfo(int pid)
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
                    command.CommandText = $"SELECT " +
                        $"pc.PID as PID," +
                        $"pc.IP as IP," +
                        $"pc.PORT as PORT," +
                        $"pc.NAME as NAME," +
                        $"pc.ADDR as ADDR," +
                        $"pc.SEATNUMBER as SEATNUMBER," +
                        $"pc.PRICE as PRICE," +
                        $"pc.PRICE_PERCENT as PRICE_PERCENT," +
                        $"pc.PC_SPEC as PC_SPEC," +
                        $"pc.TELECOM as TELECOM," +
                        $"pc.MEMO as MEMO," +
                        $"pc.COUNTRYTB_ID as COUNTRYTB_ID," +
                        $"country.Name as COUNTRY_NAME," +
                        $"pc.CITYTB_ID as CITYTB_ID," +
                        $"city.Name as CITY_NAME," +
                        $"pc.TOWNTB_ID as TOWNTB_ID," +
                        $"town.Name as TOWN_NAME " +
                        $"FROM pcroom_tb as pc " +
                        $"INNER JOIN country_tb as country on pc.COUNTRYTB_ID = country.PID " +
                        $"INNER JOIN city_tb as city on pc.CITYTB_ID = city.PID " +
                        $"INNER JOIN town_tb as town on pc.TOWNTB_ID = town.PID " +
                        $"WHERE pc.PID = @pid " +
                        $"AND pc.DEL_YN != true " +
                        $"AND country.DEL_YN != true " +
                        $"AND city.DEL_YN != true " +
                        $"AND town.DEL_YN != true";

                    var pidparam = command.CreateParameter();
                    pidparam.ParameterName = "@pid";
                    pidparam.Value = pid;
                    command.Parameters.Add(pidparam);

                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        if(await reader.ReadAsync())
                        {
                            var model = new StoreDetailDTO
                            {
                                Pid = reader.IsDBNull(reader.GetOrdinal("PID")) ? 0 : reader.GetInt32(reader.GetOrdinal("PID")),
                                Ip = reader.IsDBNull(reader.GetOrdinal("IP")) ? string.Empty : reader.GetString(reader.GetOrdinal("IP")),
                                Port = reader.IsDBNull(reader.GetOrdinal("PORT")) ? 0 : reader.GetInt32(reader.GetOrdinal("PORT")),
                                Name = reader.IsDBNull(reader.GetOrdinal("Name")) ? string.Empty : reader.GetString(reader.GetOrdinal("Name")),
                                Addr = reader.IsDBNull(reader.GetOrdinal("ADDR")) ? string.Empty : reader.GetString(reader.GetOrdinal("ADDR")),
                                Seatnumber = reader.IsDBNull(reader.GetOrdinal("SEATNUMBER")) ? 0 : reader.GetInt32(reader.GetOrdinal("SEATNUMBER")),
                                Price = reader.IsDBNull(reader.GetOrdinal("PRICE")) ? 0 : reader.GetFloat(reader.GetOrdinal("PRICE")),
                                Pricepercent = reader.IsDBNull(reader.GetOrdinal("PRICE_PERCENT")) ? string.Empty : reader.GetString(reader.GetOrdinal("PRICE_PERCENT")),
                                Pcspec = reader.IsDBNull(reader.GetOrdinal("PC_SPEC")) ? string.Empty : reader.GetString(reader.GetOrdinal("PC_SPEC")),
                                Telecom = reader.IsDBNull(reader.GetOrdinal("TELECOM")) ? string.Empty : reader.GetString(reader.GetOrdinal("TELECOM")),
                                Memo = reader.IsDBNull(reader.GetOrdinal("MEMO")) ? string.Empty : reader.GetString(reader.GetOrdinal("MEMO")),
                                CountryTbId = reader.IsDBNull(reader.GetOrdinal("COUNTRYTB_ID")) ? 0 : reader.GetInt32(reader.GetOrdinal("COUNTRYTB_ID")),
                                CountryName = reader.IsDBNull(reader.GetOrdinal("COUNTRY_NAME")) ? string.Empty : reader.GetString(reader.GetOrdinal("COUNTRY_NAME")),
                                CityTbId = reader.IsDBNull(reader.GetOrdinal("CITYTB_ID")) ? 0 : reader.GetInt32(reader.GetOrdinal("CITYTB_ID")),
                                CityName = reader.IsDBNull(reader.GetOrdinal("CITY_NAME")) ? string.Empty : reader.GetString(reader.GetOrdinal("CITY_NAME")),
                                TownTbId = reader.IsDBNull(reader.GetOrdinal("TOWNTB_ID")) ? 0 : reader.GetInt32(reader.GetOrdinal("TOWNTB_ID")),
                                TownName = reader.IsDBNull(reader.GetOrdinal("TOWN_NAME")) ? string.Empty : reader.GetString(reader.GetOrdinal("TOWN_NAME"))
                            };

                            return model;
                        }
                    }
                }

                return null; // 조회 결과가 없다면 null을 반환한다.
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// PC방 리스트 반환
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public async Task<List<StoreListDTO>?> GetPcRoomListAsync(string? search, int pageIndex, int pagenumber)
        {
            try
            {
                #region EFCORE
                /*
                var temp = await context.PcroomTbs
                    .Where(m => m.DelYn != true &&
                                (search != null && EF.Functions.Like(m.Name, $"%{search}%")))
                     .OrderBy(m => m.Pid)
                    .Skip((pagenumber) * pageIndex)
                    .Take(pageIndex)
                    .ToListAsync();
                */
                #endregion


                var result = new List<StoreListDTO>();

                var connection = context.Database.GetDbConnection();
                if(connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                using (var command = connection.CreateCommand())
                {
                    //string query = string.Empty;
                    if(String.IsNullOrWhiteSpace(search))
                    {
                        command.CommandText = $"SELECT " +
                            $"pc.PID as PID," +
                            $"pc.IP as IP," +
                            $"pc.PORT as PORT," +
                            $"pc.NAME as NAME," +
                            $"pc.ADDR as ADDR," +
                            $"pc.SEATNUMBER as SEATNUMBER," +
                            $"pc.PRICE as PRICE," +
                            $"pc.PRICE_PERCENT as PRICE_PERCENT," +
                            $"pc.PC_SPEC as PC_SPEC," +
                            $"pc.TELECOM as TELECOM," +
                            $"pc.MEMO as MEMO," +
                            $"pc.CREATE_DT as CREATE_DT," +
                            $"pc.COUNTRYTB_ID as COUNTRYTB_ID," +
                            $"pc.CITYTB_ID as CITYTB_ID," +
                            $"pc.TOWNTB_ID as TOWNTB_ID," +
                            $"CONCAT(country.Name,' ',city.Name,' ',town.NAME) as Region " +
                            $"FROM pcroom_tb as pc " +
                            $"INNER JOIN country_tb as country on pc.COUNTRYTB_ID = country.PID " +
                            $"INNER JOIN city_tb as city on pc.CITYTB_ID = city.PID " +
                            $"INNER JOIN town_tb as town on pc.TOWNTB_ID = town.PID " +
                            $"WHERE pc.DEL_YN != true " +
                            $"AND country.DEL_YN != true " +
                            $"AND city.DEL_YN != true " +
                            $"AND town.DEL_YN != true " +
                            $"ORDER BY pc.PID ASC " +
                            $"LIMIT @pageIndex " +
                            $"OFFSET @offset";


                        var pageSizeParam = command.CreateParameter();
                        pageSizeParam.ParameterName = "@pageIndex";
                        pageSizeParam.Value = pageIndex;

                        command.Parameters.Add(pageSizeParam);

                        var offsetParam = command.CreateParameter();
                        offsetParam.ParameterName = "@offset";
                        offsetParam.Value = pageIndex * pagenumber;

                        command.Parameters.Add(offsetParam);
                    }
                    else
                    {
                        //command.CommandText = $"SELECT * FROM pcroom_tb WHERE DEL_YN != TRUE AND NAME LIKE @search ORDER BY PID LIMIT @pageIndex OFFSET @offset";

                        command.CommandText = $"SELECT " +
                            $"pc.PID as PID," +
                            $"pc.IP as IP," +
                            $"pc.PORT as PORT," +
                            $"pc.NAME as NAME," +
                            $"pc.ADDR as ADDR," +
                            $"pc.SEATNUMBER as SEATNUMBER," +
                            $"pc.PRICE as PRICE," +
                            $"pc.PRICE_PERCENT as PRICE_PERCENT," +
                            $"pc.PC_SPEC as PC_SPEC," +
                            $"pc.TELECOM as TELECOM," +
                            $"pc.MEMO as MEMO," +
                            $"pc.CREATE_DT as CREATE_DT," +
                            $"pc.COUNTRYTB_ID as COUNTRYTB_ID," +
                            $"pc.CITYTB_ID as CITYTB_ID," +
                            $"pc.TOWNTB_ID as TOWNTB_ID," +
                            $"CONCAT(country.Name,' ',city.Name,' ',town.NAME) as Region " +
                            $"FROM pcroom_tb as pc " +
                            $"INNER JOIN country_tb as country on pc.COUNTRYTB_ID = country.PID " +
                            $"INNER JOIN city_tb as city on pc.CITYTB_ID = city.PID " +
                            $"INNER JOIN town_tb as town on pc.TOWNTB_ID = town.PID " +
                            $"WHERE pc.NAME LIKE @search " +
                            $"AND pc.DEL_YN != true " +
                            $"AND country.DEL_YN != true " +
                            $"AND city.DEL_YN != true " +
                            $"AND town.DEL_YN != true " +
                            $"ORDER BY pc.PID ASC " +
                            $"LIMIT @pageIndex " +
                            $"OFFSET @offset";
                            

                        var searchparam = command.CreateParameter();
                        searchparam.ParameterName = "@search";
                        searchparam.Value = $"%{search}%";
                        command.Parameters.Add(searchparam);

                        var pageSizeParam = command.CreateParameter();
                        pageSizeParam.ParameterName = "@pageIndex";
                        pageSizeParam.Value = pageIndex;

                        command.Parameters.Add(pageSizeParam);

                        var offsetParam = command.CreateParameter();
                        offsetParam.ParameterName = "@offset";
                        offsetParam.Value = pageIndex * pagenumber;

                        command.Parameters.Add(offsetParam);
                    }

                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync())
                        {
                            var PCRoomModel = new StoreListDTO
                            {
                                Pid = reader.IsDBNull(reader.GetOrdinal("PID")) ? 0 : reader.GetInt32(reader.GetOrdinal("PID")),
                                Ip = reader.IsDBNull(reader.GetOrdinal("IP")) ? string.Empty : reader.GetString(reader.GetOrdinal("IP")),
                                Port = reader.IsDBNull(reader.GetOrdinal("PORT")) ? 0 : reader.GetInt32(reader.GetOrdinal("PORT")),
                                Name = reader.IsDBNull(reader.GetOrdinal("NAME")) ? string.Empty : reader.GetString(reader.GetOrdinal("NAME")),
                                Addr = reader.IsDBNull(reader.GetOrdinal("ADDR")) ? string.Empty : reader.GetString(reader.GetOrdinal("ADDR")),
                                SeatNumber = reader.IsDBNull(reader.GetOrdinal("SEATNUMBER")) ? 0 : reader.GetInt32(reader.GetOrdinal("SEATNUMBER")),
                                Price = reader.IsDBNull(reader.GetOrdinal("PRICE")) ? 0 : reader.GetFloat(reader.GetOrdinal("PRICE")),
                                PricePercent = reader.IsDBNull(reader.GetOrdinal("PRICE_PERCENT")) ? string.Empty : reader.GetString(reader.GetOrdinal("PRICE_PERCENT")),
                                Pcspec = reader.IsDBNull(reader.GetOrdinal("PC_SPEC")) ? string.Empty : reader.GetString(reader.GetOrdinal("PC_SPEC")),
                                Telecom = reader.IsDBNull(reader.GetOrdinal("TELECOM")) ? string.Empty : reader.GetString(reader.GetOrdinal("TELECOM")),
                                Memo = reader.IsDBNull(reader.GetOrdinal("MEMO")) ? string.Empty : reader.GetString(reader.GetOrdinal("MEMO")),
                                CountryTbId = reader.IsDBNull(reader.GetOrdinal("COUNTRYTB_ID")) ? 0 : reader.GetInt32(reader.GetOrdinal("COUNTRYTB_ID")),
                                CityTbId = reader.IsDBNull(reader.GetOrdinal("CITYTB_ID")) ? 0 : reader.GetInt32(reader.GetOrdinal("CITYTB_ID")),
                                TownTbId = reader.IsDBNull(reader.GetOrdinal("TOWNTB_ID")) ? 0 : reader.GetInt32(reader.GetOrdinal("TOWNTB_ID")),
                                Region = reader.IsDBNull(reader.GetOrdinal("Region")) ? string.Empty : reader.GetString(reader.GetOrdinal("Region"))
                            };
                            result.Add(PCRoomModel);
                        }
                    }
                }

                return result;
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// PC방 지역별 그룹핑 개수 카운팅
        /// </summary>
        /// <returns></returns>
        public async Task<List<StoreRegionDTO>?> GetPcRoomRegionCountAsync()
        {
            try
            {
                var connection = context.Database.GetDbConnection();
                if(connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                var storeRegions = new List<StoreRegionDTO>();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"SELECT " +
                        $"country.PID as COUNTRY_PID," +
                        $"country.NAME as COUNTRY_NAME," +
                        $"city.PID as CITY_PID," +
                        $"city.NAME as CITY_NAME," +
                        $"town.PID as TOWN_PID," +
                        $"town.NAME as TOWN_NAME," +
                        $"CONCAT(country.NAME, ' ', city.NAME, ' ',town.NAME) as Region," +
                        $"( " +
                        $"SELECT COUNT(*) FROM pcroom_tb AS pcroom " +
                        $"WHERE pcroom.DEL_YN != true AND " +
                        $"pcroom.COUNTRYTB_ID = country.PID AND " +
                        $"pcroom.CITYTB_ID = city.PID AND " +
                        $"pcroom.TOWNTB_ID = town.PID" +
                        $") as Counter " +
                        $"FROM country_tb as country " +
                        $"INNER JOIN city_tb as city ON country.PID = city.COUNTRYTB_ID " +
                        $"INNER JOIN town_tb as town ON city.PID = town.CITYTB_ID " +
                        $"WHERE country.DEL_YN != true AND " +
                        $"city.DEL_YN != true AND " +
                        $"town.DEL_YN != true " +
                        $"GROUP BY country.PID, city.PID, town.PID";

                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            var dto = new StoreRegionDTO
                            {
                                Country_PID = reader.GetInt32(reader.GetOrdinal("COUNTRY_PID")),
                                Country_Name = reader["COUNTRY_NAME"] as string,
                                City_PID = reader.GetInt32(reader.GetOrdinal("CITY_PID")),
                                City_Name = reader["CITY_NAME"] as string,
                                Town_PID = reader.GetInt32(reader.GetOrdinal("TOWN_PID")),
                                Town_Name = reader["TOWN_NAME"] as string,
                                Region = reader["Region"] as string,
                                Count = reader.GetInt32(reader.GetOrdinal("Counter"))
                            };

                            storeRegions.Add(dto);
                        }
                    }
                    return storeRegions;
                }
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return null;
            }
        }

       
    }
}
