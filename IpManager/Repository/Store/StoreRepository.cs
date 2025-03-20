using IpManager.Comm.Logger.LogFactory.LoggerSelect;
using IpManager.DBModel;
using IpManager.DTO.Store;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

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

#region 추가
        
        public async Task<int> AddPCRoomAsync(PcroomTb PcroomTB, CountryTb CountryTB, CityTb CityTB, TownTb TownTB)
        {
            int result = 0;
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
         
        }
        #endregion

        #region 조회

        /// <summary>
        /// PC방 존재하는지 유무
        /// </summary>
        /// <param name="PcRoomName"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="addr"></param>
        /// <returns></returns>
        public async Task<int> GetPcRoomCheck(string PcRoomName, string ip, int port, string addr)
        {
            try
            {
                var model = await context.PcroomTbs.Where(m => m.Name == PcRoomName && m.Ip == ip && m.Port == port && m.Addr == addr).FirstOrDefaultAsync();
                if (model is null)
                    return 0; // 없음 생성가능
                else
                    return 1; // 이미 존재함

            }catch(Exception ex) 
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return -1;
            }
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
                                pId = reader.GetInt32(reader.GetOrdinal("PID")),
                                ip = reader.GetString(reader.GetOrdinal("IP")),
                                port = reader.GetInt32(reader.GetOrdinal("PORT")),
                                name = reader.GetString(reader.GetOrdinal("NAME")),
                                addr = reader.GetString(reader.GetOrdinal("ADDR")),
                                seatNumber = reader.GetInt32(reader.GetOrdinal("SEATNUMBER")),
                                price = (float)reader.GetDouble(reader.GetOrdinal("PRICE")), // 필요한 경우 형변환 처리
                                pricePercent = $"{reader["PRICE_PERCENT"] as string}%",
                                pcSpec = reader["PC_SPEC"] as string,
                                telecom = reader["TELECOM"] as string,
                                memo = reader["MEMO"] as string,
                                countryTbId = reader.GetInt32(reader.GetOrdinal("COUNTRYTB_ID")),
                                cityTbId = reader.GetInt32(reader.GetOrdinal("CITYTB_ID")),
                                townTbId = reader.GetInt32(reader.GetOrdinal("TOWNTB_ID"))
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
                                pId = reader.GetInt32(reader.GetOrdinal("PID")),
                                ip = reader.GetString(reader.GetOrdinal("IP")),
                                port = reader.GetInt32(reader.GetOrdinal("PORT")),
                                name = reader.GetString(reader.GetOrdinal("NAME")),
                                addr = reader.GetString(reader.GetOrdinal("ADDR")),
                                seatNumber = reader.GetInt32(reader.GetOrdinal("SEATNUMBER")),
                                price = (float)reader.GetDouble(reader.GetOrdinal("PRICE")), // 필요한 경우 형변환 처리
                                pricePercent = $"{reader["PRICE_PERCENT"] as string}%",
                                pcSpec = reader["PC_SPEC"] as string,
                                telecom = reader["TELECOM"] as string,
                                memo = reader["MEMO"] as string,
                                countryTbId = reader.GetInt32(reader.GetOrdinal("COUNTRYTB_ID")),
                                cityTbId = reader.GetInt32(reader.GetOrdinal("CITYTB_ID")),
                                townTbId = reader.GetInt32(reader.GetOrdinal("TOWNTB_ID"))
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
                                pId = reader.GetInt32(reader.GetOrdinal("PID")),
                                ip = reader.GetString(reader.GetOrdinal("IP")),
                                port = reader.GetInt32(reader.GetOrdinal("PORT")),
                                name = reader.GetString(reader.GetOrdinal("NAME")),
                                addr = reader.GetString(reader.GetOrdinal("ADDR")),
                                seatNumber = reader.GetInt32(reader.GetOrdinal("SEATNUMBER")),
                                price = (float)reader.GetDouble(reader.GetOrdinal("PRICE")), // 필요한 경우 형변환 처리
                                pricePercent = $"{reader["PRICE_PERCENT"] as string}%",
                                pcSpec = reader["PC_SPEC"] as string,
                                telecom = reader["TELECOM"] as string,
                                memo = reader["MEMO"] as string,
                                countryTbId = reader.GetInt32(reader.GetOrdinal("COUNTRYTB_ID")),
                                cityTbId = reader.GetInt32(reader.GetOrdinal("CITYTB_ID")),
                                townTbId = reader.GetInt32(reader.GetOrdinal("TOWNTB_ID"))
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
        public async Task<StoreDetailDTO?> GetPcRoomInfoDTO(int pid)
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
                                pId = reader.IsDBNull(reader.GetOrdinal("PID")) ? 0 : reader.GetInt32(reader.GetOrdinal("PID")),
                                ip = reader.IsDBNull(reader.GetOrdinal("IP")) ? string.Empty : reader.GetString(reader.GetOrdinal("IP")),
                                port = reader.IsDBNull(reader.GetOrdinal("PORT")) ? 0 : reader.GetInt32(reader.GetOrdinal("PORT")),
                                name = reader.IsDBNull(reader.GetOrdinal("Name")) ? string.Empty : reader.GetString(reader.GetOrdinal("Name")),
                                addr = reader.IsDBNull(reader.GetOrdinal("ADDR")) ? string.Empty : reader.GetString(reader.GetOrdinal("ADDR")),
                                seatNumber = reader.IsDBNull(reader.GetOrdinal("SEATNUMBER")) ? 0 : reader.GetInt32(reader.GetOrdinal("SEATNUMBER")),
                                price = reader.IsDBNull(reader.GetOrdinal("PRICE")) ? 0 : reader.GetFloat(reader.GetOrdinal("PRICE")),
                                //pricePercent = reader.IsDBNull(reader.GetOrdinal("PRICE_PERCENT")) ? string.Empty : reader.GetString(reader.GetOrdinal("PRICE_PERCENT")),
                                pricePercent = reader.IsDBNull(reader.GetOrdinal("PRICE_PERCENT")) ? string.Empty : reader.GetString($"{reader.GetOrdinal("PRICE_PERCENT")}%"),
                                pcSpec = reader.IsDBNull(reader.GetOrdinal("PC_SPEC")) ? string.Empty : reader.GetString(reader.GetOrdinal("PC_SPEC")),
                                telecom = reader.IsDBNull(reader.GetOrdinal("TELECOM")) ? string.Empty : reader.GetString(reader.GetOrdinal("TELECOM")),
                                memo = reader.IsDBNull(reader.GetOrdinal("MEMO")) ? string.Empty : reader.GetString(reader.GetOrdinal("MEMO")),
                                countryTbId = reader.IsDBNull(reader.GetOrdinal("COUNTRYTB_ID")) ? 0 : reader.GetInt32(reader.GetOrdinal("COUNTRYTB_ID")),
                                countryName = reader.IsDBNull(reader.GetOrdinal("COUNTRY_NAME")) ? string.Empty : reader.GetString(reader.GetOrdinal("COUNTRY_NAME")),
                                cityTbId = reader.IsDBNull(reader.GetOrdinal("CITYTB_ID")) ? 0 : reader.GetInt32(reader.GetOrdinal("CITYTB_ID")),
                                cityName = reader.IsDBNull(reader.GetOrdinal("CITY_NAME")) ? string.Empty : reader.GetString(reader.GetOrdinal("CITY_NAME")),
                                townTbId = reader.IsDBNull(reader.GetOrdinal("TOWNTB_ID")) ? 0 : reader.GetInt32(reader.GetOrdinal("TOWNTB_ID")),
                                townName = reader.IsDBNull(reader.GetOrdinal("TOWN_NAME")) ? string.Empty : reader.GetString(reader.GetOrdinal("TOWN_NAME"))
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
        /// PC방 테이블 조회
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public async Task<PcroomTb?> GetPcRoomInfoTB(int pid)
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
                    command.CommandText = "SELECT * FROM pcroom_Tb WHERE PID = @pid AND DEL_YN != true";
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "@pid";
                    parameter.Value = pid;
                    command.Parameters.Add(parameter);

                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        if(await reader.ReadAsync())
                        {
                            var pcroominfo = new PcroomTb
                            {
                                Pid = reader.IsDBNull(reader.GetOrdinal("PID")) ? 0 : reader.GetInt32(reader.GetOrdinal("PID")),
                                Ip = reader.IsDBNull(reader.GetOrdinal("IP")) ? string.Empty : reader.GetString(reader.GetOrdinal("IP")),
                                Port = reader.IsDBNull(reader.GetOrdinal("PORT")) ? 0 : reader.GetInt32(reader.GetOrdinal("PORT")),
                                Name = reader.IsDBNull(reader.GetOrdinal("NAME")) ? string.Empty : reader.GetString(reader.GetOrdinal("NAME")),
                                Addr = reader.IsDBNull(reader.GetOrdinal("ADDR")) ? string.Empty : reader.GetString(reader.GetOrdinal("ADDR")),
                                Seatnumber = reader.IsDBNull(reader.GetOrdinal("SEATNUMBER")) ? 0 : reader.GetInt32(reader.GetOrdinal("SEATNUMBER")),
                                Price = reader.IsDBNull(reader.GetOrdinal("PRICE")) ? 0 : reader.GetFloat(reader.GetOrdinal("PRICE")),
                                PricePercent = reader.IsDBNull(reader.GetOrdinal("PRICE_PERCENT")) ? 0: reader.GetFloat(reader.GetOrdinal("PRICE_PERCENT")),
                                PcSpec = reader.IsDBNull(reader.GetOrdinal("PC_SPEC")) ? string.Empty : reader.GetString(reader.GetOrdinal("PC_SPEC")),
                                Telecom = reader.IsDBNull(reader.GetOrdinal("TELECOM")) ? string.Empty : reader.GetString(reader.GetOrdinal("TELECOM")),
                                Memo = reader.IsDBNull(reader.GetOrdinal("MEMO")) ? string.Empty : reader.GetString(reader.GetOrdinal("MEMO")),
                                CreateDt = reader.IsDBNull(reader.GetOrdinal("CREATE_DT"))
                                            ? DateTime.MinValue
                                            : reader.GetDateTime(reader.GetOrdinal("CREATE_DT")),
                                UpdateDt = reader.IsDBNull(reader.GetOrdinal("UPDATE_DT"))
                                            ? DateTime.MinValue
                                            : reader.GetDateTime(reader.GetOrdinal("UPDATE_DT")),
                                DelYn = reader.IsDBNull(reader.GetOrdinal("DEL_YN"))
                                        ? false
                                        : reader.GetBoolean(reader.GetOrdinal("DEL_YN")),
                                DeleteDt = reader.IsDBNull(reader.GetOrdinal("DELETE_DT"))
                                            ? DateTime.MinValue
                                            : reader.GetDateTime(reader.GetOrdinal("DELETE_DT")),
                                CountrytbId = reader.IsDBNull(reader.GetOrdinal("COUNTRYTB_ID")) ? 0 : reader.GetInt32(reader.GetOrdinal("COUNTRYTB_ID")),
                                CitytbId = reader.IsDBNull(reader.GetOrdinal("CITYTB_ID")) ? 0 : reader.GetInt32(reader.GetOrdinal("CITYTB_ID")),
                                TowntbId = reader.IsDBNull(reader.GetOrdinal("TOWNTB_ID")) ? 0 : reader.GetInt32(reader.GetOrdinal("TOWNTB_ID"))

                            };

                            return pcroominfo;
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
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
        public async Task<List<StoreListDTO>?> GetAllPcRoomListAsync(string? search)
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
                            $"ORDER BY pc.NAME ASC ";
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
                            $"ORDER BY pc.NAME ASC ";

                        var searchparam = command.CreateParameter();
                        searchparam.ParameterName = "@search";
                        searchparam.Value = $"%{search}%";
                        command.Parameters.Add(searchparam);
                    }

                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync())
                        {
                            var PCRoomModel = new StoreListDTO
                            {
                                pId = reader.IsDBNull(reader.GetOrdinal("PID")) ? 0 : reader.GetInt32(reader.GetOrdinal("PID")),
                                ip = reader.IsDBNull(reader.GetOrdinal("IP")) ? string.Empty : reader.GetString(reader.GetOrdinal("IP")),
                                port = reader.IsDBNull(reader.GetOrdinal("PORT")) ? 0 : reader.GetInt32(reader.GetOrdinal("PORT")),
                                name = reader.IsDBNull(reader.GetOrdinal("NAME")) ? string.Empty : reader.GetString(reader.GetOrdinal("NAME")),
                                addr = reader.IsDBNull(reader.GetOrdinal("ADDR")) ? string.Empty : reader.GetString(reader.GetOrdinal("ADDR")),
                                seatNumber = reader.IsDBNull(reader.GetOrdinal("SEATNUMBER")) ? 0 : reader.GetInt32(reader.GetOrdinal("SEATNUMBER")),
                                price = reader.IsDBNull(reader.GetOrdinal("PRICE")) ? 0 : reader.GetFloat(reader.GetOrdinal("PRICE")),
                                pricePercent = reader.IsDBNull(reader.GetOrdinal("PRICE_PERCENT")) ? string.Empty : reader.GetString(reader.GetOrdinal("PRICE_PERCENT")),
                                pcSpec = reader.IsDBNull(reader.GetOrdinal("PC_SPEC")) ? string.Empty : reader.GetString(reader.GetOrdinal("PC_SPEC")),
                                telecom = reader.IsDBNull(reader.GetOrdinal("TELECOM")) ? string.Empty : reader.GetString(reader.GetOrdinal("TELECOM")),
                                memo = reader.IsDBNull(reader.GetOrdinal("MEMO")) ? string.Empty : reader.GetString(reader.GetOrdinal("MEMO")),
                                countryTbId = reader.IsDBNull(reader.GetOrdinal("COUNTRYTB_ID")) ? 0 : reader.GetInt32(reader.GetOrdinal("COUNTRYTB_ID")),
                                cityTbId = reader.IsDBNull(reader.GetOrdinal("CITYTB_ID")) ? 0 : reader.GetInt32(reader.GetOrdinal("CITYTB_ID")),
                                townTbId = reader.IsDBNull(reader.GetOrdinal("TOWNTB_ID")) ? 0 : reader.GetInt32(reader.GetOrdinal("TOWNTB_ID")),
                                region = reader.IsDBNull(reader.GetOrdinal("Region")) ? string.Empty : reader.GetString(reader.GetOrdinal("Region"))
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
        /// 할당된 지역의 PC방 리스트 반환
        /// </summary>
        /// <param name="search"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pagenumber"></param>
        /// <param name="countryId"></param>
        /// <returns></returns>
        public async Task<List<StoreListDTO>?> GetMyPcRoomListAsync(string? search, int countryId)
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
                    if (String.IsNullOrWhiteSpace(search))
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
                            $"AND country.PID = @countryId " +
                            $"ORDER BY pc.NAME ASC " +
                            $"LIMIT @pageIndex " +
                            $"OFFSET @offset";

                        var countryParam = command.CreateParameter();
                        countryParam.ParameterName = "@countryId";
                        countryParam.Value = countryId;

                        command.Parameters.Add(countryParam);
                    }
                    else
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
                           $"AND country.PID = @countryId " +
                           $"AND pc.NAME LIKE @search " +
                           $"ORDER BY pc.NAME ASC " +
                           $"LIMIT @pageIndex " +
                           $"OFFSET @offset";

                        var searchparam = command.CreateParameter();
                        searchparam.ParameterName = "@search";
                        searchparam.Value = $"%{search}%";
                        command.Parameters.Add(searchparam);

                        var countryParam = command.CreateParameter();
                        countryParam.ParameterName = "@countryId";
                        countryParam.Value = countryId;

                        command.Parameters.Add(countryParam);
                    }

                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while(await reader.ReadAsync())
                        {
                            var PCRoomModel = new StoreListDTO
                            {
                                pId = reader.IsDBNull(reader.GetOrdinal("PID")) ? 0 : reader.GetInt32(reader.GetOrdinal("PID")),
                                ip = reader.IsDBNull(reader.GetOrdinal("IP")) ? string.Empty : reader.GetString(reader.GetOrdinal("IP")),
                                port = reader.IsDBNull(reader.GetOrdinal("PORT")) ? 0 : reader.GetInt32(reader.GetOrdinal("PORT")),
                                name = reader.IsDBNull(reader.GetOrdinal("NAME")) ? string.Empty : reader.GetString(reader.GetOrdinal("NAME")),
                                addr = reader.IsDBNull(reader.GetOrdinal("ADDR")) ? string.Empty : reader.GetString(reader.GetOrdinal("ADDR")),
                                seatNumber = reader.IsDBNull(reader.GetOrdinal("SEATNUMBER")) ? 0 : reader.GetInt32(reader.GetOrdinal("SEATNUMBER")),
                                price = reader.IsDBNull(reader.GetOrdinal("PRICE")) ? 0 : reader.GetFloat(reader.GetOrdinal("PRICE")),
                                pricePercent = reader.IsDBNull(reader.GetOrdinal("PRICE_PERCENT")) ? string.Empty : reader.GetString(reader.GetOrdinal("PRICE_PERCENT")),
                                pcSpec = reader.IsDBNull(reader.GetOrdinal("PC_SPEC")) ? string.Empty : reader.GetString(reader.GetOrdinal("PC_SPEC")),
                                telecom = reader.IsDBNull(reader.GetOrdinal("TELECOM")) ? string.Empty : reader.GetString(reader.GetOrdinal("TELECOM")),
                                memo = reader.IsDBNull(reader.GetOrdinal("MEMO")) ? string.Empty : reader.GetString(reader.GetOrdinal("MEMO")),
                                countryTbId = reader.IsDBNull(reader.GetOrdinal("COUNTRYTB_ID")) ? 0 : reader.GetInt32(reader.GetOrdinal("COUNTRYTB_ID")),
                                cityTbId = reader.IsDBNull(reader.GetOrdinal("CITYTB_ID")) ? 0 : reader.GetInt32(reader.GetOrdinal("CITYTB_ID")),
                                townTbId = reader.IsDBNull(reader.GetOrdinal("TOWNTB_ID")) ? 0 : reader.GetInt32(reader.GetOrdinal("TOWNTB_ID")),
                                region = reader.IsDBNull(reader.GetOrdinal("Region")) ? string.Empty : reader.GetString(reader.GetOrdinal("Region"))
                            };
                            result.Add(PCRoomModel);
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
        /// (전체) PC방 지역별 그룹핑 개수 카운팅
        /// </summary>
        /// <returns></returns>
        public async Task<List<StoreRegionDTO>?> GetPcRoomAllRegionCountAsync()
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
                                countryPid = reader.GetInt32(reader.GetOrdinal("COUNTRY_PID")),
                                countryName = reader["COUNTRY_NAME"] as string,
                                cityPid = reader.GetInt32(reader.GetOrdinal("CITY_PID")),
                                cityName = reader["CITY_NAME"] as string,
                                townPid = reader.GetInt32(reader.GetOrdinal("TOWN_PID")),
                                townName = reader["TOWN_NAME"] as string,
                                region = reader["Region"] as string,
                                count = reader.GetInt32(reader.GetOrdinal("Counter"))
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

        /// <summary>
        /// (내) PC방 지역별 그룹핑 개수 카운팅
        /// </summary>
        /// <returns></returns>
        public async Task<List<StoreRegionDTO>?> GetPcRoomMyRegionCountAsync(int countryId)
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
                          $"town.DEL_YN != true AND " +
                          $"country.PID = @countryId " +
                          $"GROUP BY country.PID, city.PID, town.PID";

                    var countryparam = command.CreateParameter();
                    countryparam.ParameterName = "@countryId";
                    countryparam.Value = countryId;
                    command.Parameters.Add(countryparam);

                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            var dto = new StoreRegionDTO
                            {
                                countryPid = reader.GetInt32(reader.GetOrdinal("COUNTRY_PID")),
                                countryName = reader["COUNTRY_NAME"] as string,
                                cityPid = reader.GetInt32(reader.GetOrdinal("CITY_PID")),
                                cityName = reader["CITY_NAME"] as string,
                                townPid = reader.GetInt32(reader.GetOrdinal("TOWN_PID")),
                                townName = reader["TOWN_NAME"] as string,
                                region = reader["Region"] as string,
                                count = reader.GetInt32(reader.GetOrdinal("Counter"))
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

        /// <summary>
        /// PC방 이름으로 검색
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public async Task<List<StoreListDTO>?> GetAllPcRoomSearchNameListAsync(string search)
        {
            try
            {
                var connection = context.Database.GetDbConnection();
                if(connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }
                var result = new List<StoreListDTO>();
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
                        $"pc.TOWNTB_ID as TOWNTB_ID," +
                        $"CONCAT(country.Name,' ',city.Name,' ',town.NAME) as Region " +
                        $"FROM pcroom_tb as pc " +
                        $"INNER JOIN country_tb as country on pc.COUNTRYTB_ID = country.PID " +
                        $"INNER JOIN city_tb as city on pc.CITYTB_ID = city.PID " +
                        $"INNER JOIN town_tb as town on pc.TOWNTB_ID = town.PID " +
                        $"WHERE pc.NAME LIKE @search " +
                        $"ORDER BY NAME ASC";

                    var searchparam = command.CreateParameter();
                    searchparam.ParameterName = "@search";
                    searchparam.Value = $"%{search}%";
                    command.Parameters.Add(searchparam);

                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync())
                        {
                            var PCRoomModel = new StoreListDTO
                            {
                                pId = reader.IsDBNull(reader.GetOrdinal("PID")) ? 0 : reader.GetInt32(reader.GetOrdinal("PID")),
                                ip = reader.IsDBNull(reader.GetOrdinal("IP")) ? string.Empty : reader.GetString(reader.GetOrdinal("IP")),
                                port = reader.IsDBNull(reader.GetOrdinal("PORT")) ? 0 : reader.GetInt32(reader.GetOrdinal("PORT")),
                                name = reader.IsDBNull(reader.GetOrdinal("NAME")) ? string.Empty : reader.GetString(reader.GetOrdinal("NAME")),
                                addr = reader.IsDBNull(reader.GetOrdinal("ADDR")) ? string.Empty : reader.GetString(reader.GetOrdinal("ADDR")),
                                seatNumber = reader.IsDBNull(reader.GetOrdinal("SEATNUMBER")) ? 0 : reader.GetInt32(reader.GetOrdinal("SEATNUMBER")),
                                price = reader.IsDBNull(reader.GetOrdinal("PRICE")) ? 0 : reader.GetFloat(reader.GetOrdinal("PRICE")),
                                pricePercent = reader.IsDBNull(reader.GetOrdinal("PRICE_PERCENT")) ? string.Empty : reader.GetString(reader.GetOrdinal("PRICE_PERCENT")),
                                pcSpec = reader.IsDBNull(reader.GetOrdinal("PC_SPEC")) ? string.Empty : reader.GetString(reader.GetOrdinal("PC_SPEC")),
                                telecom = reader.IsDBNull(reader.GetOrdinal("TELECOM")) ? string.Empty : reader.GetString(reader.GetOrdinal("TELECOM")),
                                memo = reader.IsDBNull(reader.GetOrdinal("MEMO")) ? string.Empty : reader.GetString(reader.GetOrdinal("MEMO")),
                                countryTbId = reader.IsDBNull(reader.GetOrdinal("COUNTRYTB_ID")) ? 0 : reader.GetInt32(reader.GetOrdinal("COUNTRYTB_ID")),
                                cityTbId = reader.IsDBNull(reader.GetOrdinal("CITYTB_ID")) ? 0 : reader.GetInt32(reader.GetOrdinal("CITYTB_ID")),
                                townTbId = reader.IsDBNull(reader.GetOrdinal("TOWNTB_ID")) ? 0 : reader.GetInt32(reader.GetOrdinal("TOWNTB_ID")),
                                region = reader.IsDBNull(reader.GetOrdinal("Region")) ? string.Empty : reader.GetString(reader.GetOrdinal("Region"))
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
        /// 내) PC방 이름에 해당하는 PC방 LIST 반환
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public async Task<List<StoreListDTO>?> GetMyPcRoomSearchNameListAsync(string search, int countryId)
        {
            try
            {
                var connection = context.Database.GetDbConnection();
                if(connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                var result = new List<StoreListDTO>();
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
                      $"pc.TOWNTB_ID as TOWNTB_ID," +
                      $"CONCAT(country.Name,' ',city.Name,' ',town.NAME) as Region " +
                      $"FROM pcroom_tb as pc " +
                      $"INNER JOIN country_tb as country on pc.COUNTRYTB_ID = country.PID " +
                      $"INNER JOIN city_tb as city on pc.CITYTB_ID = city.PID " +
                      $"INNER JOIN town_tb as town on pc.TOWNTB_ID = town.PID " +
                      $"WHERE pc.NAME LIKE @search " +
                      $"AND pc.COUNTRYTB_ID = @countryId " +
                      $"ORDER BY NAME ASC";

                    var searchparam = command.CreateParameter();
                    searchparam.ParameterName = "@search";
                    searchparam.Value = $"%{search}%";
                    command.Parameters.Add(searchparam);

                    var countryparam = command.CreateParameter();
                    countryparam.ParameterName = "@countryId";
                    countryparam.Value = countryId;
                    command.Parameters.Add(countryparam);

                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync())
                        {
                            var PCRoomModel = new StoreListDTO
                            {
                                pId = reader.IsDBNull(reader.GetOrdinal("PID")) ? 0 : reader.GetInt32(reader.GetOrdinal("PID")),
                                ip = reader.IsDBNull(reader.GetOrdinal("IP")) ? string.Empty : reader.GetString(reader.GetOrdinal("IP")),
                                port = reader.IsDBNull(reader.GetOrdinal("PORT")) ? 0 : reader.GetInt32(reader.GetOrdinal("PORT")),
                                name = reader.IsDBNull(reader.GetOrdinal("NAME")) ? string.Empty : reader.GetString(reader.GetOrdinal("NAME")),
                                addr = reader.IsDBNull(reader.GetOrdinal("ADDR")) ? string.Empty : reader.GetString(reader.GetOrdinal("ADDR")),
                                seatNumber = reader.IsDBNull(reader.GetOrdinal("SEATNUMBER")) ? 0 : reader.GetInt32(reader.GetOrdinal("SEATNUMBER")),
                                price = reader.IsDBNull(reader.GetOrdinal("PRICE")) ? 0 : reader.GetFloat(reader.GetOrdinal("PRICE")),
                                pricePercent = reader.IsDBNull(reader.GetOrdinal("PRICE_PERCENT")) ? string.Empty : reader.GetString(reader.GetOrdinal("PRICE_PERCENT")),
                                pcSpec = reader.IsDBNull(reader.GetOrdinal("PC_SPEC")) ? string.Empty : reader.GetString(reader.GetOrdinal("PC_SPEC")),
                                telecom = reader.IsDBNull(reader.GetOrdinal("TELECOM")) ? string.Empty : reader.GetString(reader.GetOrdinal("TELECOM")),
                                memo = reader.IsDBNull(reader.GetOrdinal("MEMO")) ? string.Empty : reader.GetString(reader.GetOrdinal("MEMO")),
                                countryTbId = reader.IsDBNull(reader.GetOrdinal("COUNTRYTB_ID")) ? 0 : reader.GetInt32(reader.GetOrdinal("COUNTRYTB_ID")),
                                cityTbId = reader.IsDBNull(reader.GetOrdinal("CITYTB_ID")) ? 0 : reader.GetInt32(reader.GetOrdinal("CITYTB_ID")),
                                townTbId = reader.IsDBNull(reader.GetOrdinal("TOWNTB_ID")) ? 0 : reader.GetInt32(reader.GetOrdinal("TOWNTB_ID")),
                                region = reader.IsDBNull(reader.GetOrdinal("Region")) ? string.Empty : reader.GetString(reader.GetOrdinal("Region"))
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
        /// 전체) PC방 주소로 검색
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public async Task<List<StoreListDTO>?> GetPcRoomAllSearchAddressListAsync(string search)
        {
            try
            {
                var connection = context.Database.GetDbConnection();
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }
                var result = new List<StoreListDTO>();
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
                        $"pc.TOWNTB_ID as TOWNTB_ID," +
                        $"CONCAT(country.Name,' ',city.Name,' ',town.NAME) as Region " +
                        $"FROM pcroom_tb as pc " +
                        $"INNER JOIN country_tb as country on pc.COUNTRYTB_ID = country.PID " +
                        $"INNER JOIN city_tb as city on pc.CITYTB_ID = city.PID " +
                        $"INNER JOIN town_tb as town on pc.TOWNTB_ID = town.PID " +
                        $"WHERE pc.ADDR LIKE @search " +
                        $"ORDER BY NAME ASC";

                    var searchparam = command.CreateParameter();
                    searchparam.ParameterName = "@search";
                    searchparam.Value = $"%{search}%";
                    command.Parameters.Add(searchparam);

                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync())
                        {
                            var PCRoomModel = new StoreListDTO
                            {
                                pId = reader.IsDBNull(reader.GetOrdinal("PID")) ? 0 : reader.GetInt32(reader.GetOrdinal("PID")),
                                ip = reader.IsDBNull(reader.GetOrdinal("IP")) ? string.Empty : reader.GetString(reader.GetOrdinal("IP")),
                                port = reader.IsDBNull(reader.GetOrdinal("PORT")) ? 0 : reader.GetInt32(reader.GetOrdinal("PORT")),
                                name = reader.IsDBNull(reader.GetOrdinal("NAME")) ? string.Empty : reader.GetString(reader.GetOrdinal("NAME")),
                                addr = reader.IsDBNull(reader.GetOrdinal("ADDR")) ? string.Empty : reader.GetString(reader.GetOrdinal("ADDR")),
                                seatNumber = reader.IsDBNull(reader.GetOrdinal("SEATNUMBER")) ? 0 : reader.GetInt32(reader.GetOrdinal("SEATNUMBER")),
                                price = reader.IsDBNull(reader.GetOrdinal("PRICE")) ? 0 : reader.GetFloat(reader.GetOrdinal("PRICE")),
                                pricePercent = reader.IsDBNull(reader.GetOrdinal("PRICE_PERCENT")) ? string.Empty : reader.GetString(reader.GetOrdinal("PRICE_PERCENT")),
                                pcSpec = reader.IsDBNull(reader.GetOrdinal("PC_SPEC")) ? string.Empty : reader.GetString(reader.GetOrdinal("PC_SPEC")),
                                telecom = reader.IsDBNull(reader.GetOrdinal("TELECOM")) ? string.Empty : reader.GetString(reader.GetOrdinal("TELECOM")),
                                memo = reader.IsDBNull(reader.GetOrdinal("MEMO")) ? string.Empty : reader.GetString(reader.GetOrdinal("MEMO")),
                                countryTbId = reader.IsDBNull(reader.GetOrdinal("COUNTRYTB_ID")) ? 0 : reader.GetInt32(reader.GetOrdinal("COUNTRYTB_ID")),
                                cityTbId = reader.IsDBNull(reader.GetOrdinal("CITYTB_ID")) ? 0 : reader.GetInt32(reader.GetOrdinal("CITYTB_ID")),
                                townTbId = reader.IsDBNull(reader.GetOrdinal("TOWNTB_ID")) ? 0 : reader.GetInt32(reader.GetOrdinal("TOWNTB_ID")),
                                region = reader.IsDBNull(reader.GetOrdinal("Region")) ? string.Empty : reader.GetString(reader.GetOrdinal("Region"))
                            };
                            result.Add(PCRoomModel);
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
        /// 내) PC방 주소로 검색
        /// </summary>
        /// <param name="search"></param>
        /// <param name="countryId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<List<StoreListDTO>?> GetPcRoomMySearchAddressLisyAsync(string search, int countryId)
        {
            try
            {
                var connection = context.Database.GetDbConnection();
                if(connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }
                var result = new List<StoreListDTO>();
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
                        $"pc.TOWNTB_ID as TOWNTB_ID," +
                        $"CONCAT(country.Name,' ',city.Name,' ',town.NAME) as Region " +
                        $"FROM pcroom_tb as pc " +
                        $"INNER JOIN country_tb as country on pc.COUNTRYTB_ID = country.PID " +
                        $"INNER JOIN city_tb as city on pc.CITYTB_ID = city.PID " +
                        $"INNER JOIN town_tb as town on pc.TOWNTB_ID = town.PID " +
                        $"WHERE pc.ADDR LIKE @search " +
                        $"AND pc.COUNTRYTB_ID = @countryId " +
                        $"ORDER BY NAME ASC";

                    var searchparam = command.CreateParameter();
                    searchparam.ParameterName = "@search";
                    searchparam.Value = $"%{search}%";
                    command.Parameters.Add(searchparam);

                    var countryparam = command.CreateParameter();
                    countryparam.ParameterName = "@countryId";
                    countryparam.Value = countryId;
                    command.Parameters.Add(countryparam);

                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync())
                        {
                            var PCRoomModel = new StoreListDTO
                            {
                                pId = reader.IsDBNull(reader.GetOrdinal("PID")) ? 0 : reader.GetInt32(reader.GetOrdinal("PID")),
                                ip = reader.IsDBNull(reader.GetOrdinal("IP")) ? string.Empty : reader.GetString(reader.GetOrdinal("IP")),
                                port = reader.IsDBNull(reader.GetOrdinal("PORT")) ? 0 : reader.GetInt32(reader.GetOrdinal("PORT")),
                                name = reader.IsDBNull(reader.GetOrdinal("NAME")) ? string.Empty : reader.GetString(reader.GetOrdinal("NAME")),
                                addr = reader.IsDBNull(reader.GetOrdinal("ADDR")) ? string.Empty : reader.GetString(reader.GetOrdinal("ADDR")),
                                seatNumber = reader.IsDBNull(reader.GetOrdinal("SEATNUMBER")) ? 0 : reader.GetInt32(reader.GetOrdinal("SEATNUMBER")),
                                price = reader.IsDBNull(reader.GetOrdinal("PRICE")) ? 0 : reader.GetFloat(reader.GetOrdinal("PRICE")),
                                pricePercent = reader.IsDBNull(reader.GetOrdinal("PRICE_PERCENT")) ? string.Empty : reader.GetString(reader.GetOrdinal("PRICE_PERCENT")),
                                pcSpec = reader.IsDBNull(reader.GetOrdinal("PC_SPEC")) ? string.Empty : reader.GetString(reader.GetOrdinal("PC_SPEC")),
                                telecom = reader.IsDBNull(reader.GetOrdinal("TELECOM")) ? string.Empty : reader.GetString(reader.GetOrdinal("TELECOM")),
                                memo = reader.IsDBNull(reader.GetOrdinal("MEMO")) ? string.Empty : reader.GetString(reader.GetOrdinal("MEMO")),
                                countryTbId = reader.IsDBNull(reader.GetOrdinal("COUNTRYTB_ID")) ? 0 : reader.GetInt32(reader.GetOrdinal("COUNTRYTB_ID")),
                                cityTbId = reader.IsDBNull(reader.GetOrdinal("CITYTB_ID")) ? 0 : reader.GetInt32(reader.GetOrdinal("CITYTB_ID")),
                                townTbId = reader.IsDBNull(reader.GetOrdinal("TOWNTB_ID")) ? 0 : reader.GetInt32(reader.GetOrdinal("TOWNTB_ID")),
                                region = reader.IsDBNull(reader.GetOrdinal("Region")) ? string.Empty : reader.GetString(reader.GetOrdinal("Region"))
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

        #endregion

        #region 수정
        /// <summary>
        /// PC방 정보 수정
        /// </summary>
        /// <param name="PcroomTB"></param>
        /// <returns></returns>
        public async Task<int> EditPcRoomInfo(PcroomTb PcroomTB)
        {
            try
            {
                // 이미 같은 PID를 가진 엔티티가 DbContext의 캐시에 있는지 확인
                var trackedEntity = context.PcroomTbs.Local.FirstOrDefault(e => e.Pid == PcroomTB.Pid);
                if(trackedEntity != null)
                {
                    // 이미 추적 중인 엔티티가 있다면, 해당 엔티티의 현재 값을 새 모델 값으로 업데이트한다.
                    context.Entry(trackedEntity).CurrentValues.SetValues(PcroomTB);
                }
                else
                {
                    // 추적 중인 엔티티가 없다면, 모델을 Attach하고 상태를 Modified로 설정한다.
                    context.PcroomTbs.Attach(PcroomTB);
                    context.Entry(PcroomTB).State = EntityState.Modified;
                }

                int result = await context.SaveChangesAsync().ConfigureAwait(false);
                return result;
            }
            catch(Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return -1;
            }
        }
#endregion

#region 삭제
        /// <summary>
        /// PC방 정보 삭제
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public async Task<int> DeletePcRoomInfo(PcroomTb PcroomTB)
        {
            try
            {
                // 이미 같은 PID를 가진 엔티티가 DbContext의 캐시에 있는지 확인
                var trackedEntity = context.PcroomTbs.Local.FirstOrDefault(e => e.Pid == PcroomTB.Pid);
                if (trackedEntity != null)
                {
                    // 이미 추적 중인 엔티티가 있다면, 해당 엔티티의 현재 값을 새 모델 값으로 업데이트한다.
                    context.Entry(trackedEntity).CurrentValues.SetValues(PcroomTB);
                }
                else
                {
                    // 추적 중인 엔티티가 없다면, 모델을 Attach하고 상태를 Modified로 설정한다.
                    context.PcroomTbs.Attach(PcroomTB);
                    context.Entry(PcroomTB).State = EntityState.Modified;
                }

                int result = await context.SaveChangesAsync().ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                LoggerService.FileErrorMessage(ex.ToString());
                return -1;
            }
        }

   









        #endregion

    }
}
