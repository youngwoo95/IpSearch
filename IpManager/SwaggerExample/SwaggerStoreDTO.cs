using IpManager.DTO.Store;
using Swashbuckle.AspNetCore.Filters;

namespace IpManager.SwaggerExample
{
    /// <summary>
    /// PC방 정보 전체 반환
    /// </summary>
    public class SwaggerStoreListDTO : IExamplesProvider<ResponseList<StoreListDTO>>
    {
        public ResponseList<StoreListDTO> GetExamples()
        {
            var items = new List<StoreListDTO>
            {
                new StoreListDTO
                {
                    pId = 1,
                    ip = "192.123.123.123",
                    port = 5245,
                    name = "□□□",
                    addr = "서울특별시 강남구 개포로623 △△△빌딩 3층",
                    seatNumber = 100,
                    price = 1300,
                    pricePercent = 55,
                    pcSpec = "CPU i5-14700K GPU GTX 4070 RAM 32GB HDD 2TB",
                    telecom = "SK Telecom",
                    memo = "",
                    region = "서울특별시 강남구 개포로",
                    countryTbId = 1,
                    cityTbId = 1,
                    townTbId = 1
                },
                new StoreListDTO
                {
                    pId = 2,
                    ip = "192.456.456.456",
                    port = 5245,
                    name = "△△△",
                    addr = "부산광역시 사상구 주례로43 ○○○빌딩 3층",
                    seatNumber = 55,
                    price = 1000,
                    pricePercent = 55,
                    pcSpec = "CPU i5-14700K GPU GTX 4070 RAM 32GB HDD 2TB",
                    telecom = "KT",
                    memo = "",
                    region = "부산광역시 사상구 주례로",
                    countryTbId = 2,
                    cityTbId = 2,
                    townTbId = 2
                },
            };

            return new ResponseList<StoreListDTO>() { message = "정상", data = items, code = 200 };
        }
    }

    /// <summary>
    /// PC방 PING 개별 SEND
    /// </summary>
    public class SwaggerStorePingDTO : IExamplesProvider<ResponseUnit<StorePingDTO>>
    {
        public ResponseUnit<StorePingDTO> GetExamples()
        {
            var model = new ResponseUnit<StorePingDTO>()
            {
                message = "요청이 정상 처리되었습니다.",
                data = new StorePingDTO
                {
                    used = 30,
                    unUsed = 70
                },
                code = 200
            };
            return model;
        }
    }

    /// <summary>
    /// PC방 정보 등록
    /// </summary>
    public class SwaggerAddStoreDTO : IExamplesProvider<ResponseUnit<bool>>
    {
        public ResponseUnit<bool> GetExamples()
        {
            var model = new ResponseUnit<bool>
            {
                message = "저장되었습니다.",
                data = true,
                code = 200
            };
            return model;
        }
    }

    /// <summary>
    /// PC방 이름으로 검색
    /// </summary>
    public class SwaggerSearchNameStoreDTO : IExamplesProvider<ResponseList<StoreListDTO>>
    {
        public ResponseList<StoreListDTO> GetExamples()
        {
            var model = new ResponseList<StoreListDTO>()
            {
                message = "조회가 성공하였습니다.",
                data = new List<StoreListDTO>
                {
                     new StoreListDTO
                    {
                        pId = 1,
                        ip = "192.123.123.123",
                        port = 5245,
                        name = "□□□",
                        addr = "서울특별시 강남구 개포로623 △△△빌딩 3층",
                        seatNumber = 100,
                        price = 1300,
                        pricePercent = 55,
                        pcSpec = "CPU i5-14700K GPU GTX 4070 RAM 32GB HDD 2TB",
                        telecom = "SK Telecom",
                        memo = "",
                        region = "서울특별시 강남구 개포로",
                        countryTbId = 1,
                        cityTbId = 1,
                        townTbId = 1
                    },
                    new StoreListDTO
                    {
                        pId = 2,
                        ip = "192.456.456.456",
                        port = 5245,
                        name = "△△△",
                        addr = "부산광역시 사상구 주례로43 ○○○빌딩 3층",
                        seatNumber = 55,
                        price = 1000,
                        pricePercent = 55,
                        pcSpec = "CPU i5-14700K GPU GTX 4070 RAM 32GB HDD 2TB",
                        telecom = "KT",
                        memo = "",
                        region = "부산광역시 사상구 주례로",
                        countryTbId = 2,
                        cityTbId = 2,
                        townTbId = 2
                    }
                },
                code = 200
            };
            return model;
        }
    }

    /// <summary>
    /// PC방 주소로 검색
    /// </summary>
    public class SwaggerSearchAddrStoreDTO : IExamplesProvider<ResponseList<StoreListDTO>>
    {
        public ResponseList<StoreListDTO> GetExamples()
        {
            var model = new ResponseList<StoreListDTO>()
            {
                message = "조회가 성공하였습니다.",
                data = new List<StoreListDTO>
                {
                     new StoreListDTO
                    {
                        pId = 1,
                        ip = "192.123.123.123",
                        port = 5245,
                        name = "□□□",
                        addr = "서울특별시 강남구 개포로623 △△△빌딩 3층",
                        seatNumber = 100,
                        price = 1300,
                        pricePercent = 55,
                        pcSpec = "CPU i5-14700K GPU GTX 4070 RAM 32GB HDD 2TB",
                        telecom = "SK Telecom",
                        memo = "",
                        region = "서울특별시 강남구 개포로",
                        countryTbId = 1,
                        cityTbId = 1,
                        townTbId = 1
                    },
                    new StoreListDTO
                    {
                        pId = 2,
                        ip = "192.456.456.456",
                        port = 5245,
                        name = "△△△",
                        addr = "부산광역시 사상구 주례로43 ○○○빌딩 3층",
                        seatNumber = 55,
                        price = 1000,
                        pricePercent = 55,
                        pcSpec = "CPU i5-14700K GPU GTX 4070 RAM 32GB HDD 2TB",
                        telecom = "KT",
                        memo = "",
                        region = "부산광역시 사상구 주례로",
                        countryTbId = 2,
                        cityTbId = 2,
                        townTbId = 2
                    }
                },
                code = 200
            };
            return model;
        }
    }

    /// <summary>
    /// PC방 그룹핑 개수 카운팅
    /// </summary>
    public class SwaggerStoreGroupListDTO : IExamplesProvider<ResponseList<StoreRegionDTO>>
    {
        public ResponseList<StoreRegionDTO> GetExamples()
        {
            var model = new ResponseList<StoreRegionDTO>()
            {
                message = "조회가 성공하였습니다.",
                data = new List<StoreRegionDTO>
                {
                    new StoreRegionDTO
                    {
                        countryPid = 1,
                        countryName = "경기도",
                        cityPid = 1,
                        cityName = "하남시",
                        townPid = 1,
                        townName = "망월동",
                        region = "경기도 하남시 망월동",
                        count = 1
                    },
                    new StoreRegionDTO
                    {
                        countryPid = 1,
                        countryName = "경기도",
                        cityPid = 1,
                        cityName = "하남시",
                        townPid = 2,
                        townName = "상일동",
                        region = "경기도 하남시 상일동",
                        count = 1
                    },
                    new StoreRegionDTO
                    {
                        countryPid = 3,
                        countryName = "부산광역시",
                        cityPid = 3,
                        cityName = "북구",
                        townPid = 4,
                        townName = "화명동",
                        region = "부산광역시 북구 화명동",
                        count = 1
                    }
                },
                code = 200
            };
            return model;
        }
    }

    /// <summary>
    /// PC방 정보 상세조회
    /// </summary>
    public class SwaggerStoreDetailDTO : IExamplesProvider<ResponseUnit<StoreDetailDTO>>
    {
        public ResponseUnit<StoreDetailDTO> GetExamples()
        {
            throw new NotImplementedException();
        }
    }


}
