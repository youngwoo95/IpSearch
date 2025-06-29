namespace IpManager.DTO.Country
{
    public class RegionDataDTO
    {
        public int countryId { get; set; }
        public string? countryName { get; set; }

        public List<cityDataDTO> cityDatas { get; set; } = new();
    }

    public class cityDataDTO
    {
        public int cityId { get; set; }
        public string? cityName { get; set; }

        public List<townDataDTO> townDatas { get; set; } = new();
    }

    public class townDataDTO
    {
        public int townId { get; set; }
        public string? townName { get; set; }
    }

    
}
