namespace IpManager.DTO.DashBoard
{
    public class XlsxDTO
    {
        public int pcId { get; set; }
        public string pcName { get; set; }

        public List<analyzeData> datas { get; set; } = new();
    }        

    public class analyzeData
    {
        public string? analyzeDT { get; set; }

        public int countryId { get; set; }
        public string countryName { get; set; }

        public int cityId { get; set; }
        public string cityName { get; set; }

        public int townId { get; set; }
        public string townName { get; set; }


        public double? usedPc { get; set; }
        public double? averageRate { get; set; }
        public double pcPrice { get; set; }
        public double? foodPrice { get; set; }
        public double? totalPrice { get; set; }

        public int seatNumber { get; set; }
        public string? pricePercent { get; set; }
    }
}
