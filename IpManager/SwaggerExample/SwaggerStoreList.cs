using IpManager.DTO.Store;
using Swashbuckle.AspNetCore.Filters;

namespace IpManager.SwaggerExample
{
    public class SwaggerStoreList : IExamplesProvider<ResponseList<StoreListDTO>>
    {
        public ResponseList<StoreListDTO> GetExamples()
        {
            var items = new List<StoreListDTO>
            {
                new StoreListDTO
                {
                    addr = "!23123"
                }
            };

            return new ResponseList<StoreListDTO>() { message = "정상", data = items, code = 200 };
        }
    }
}
