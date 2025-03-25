namespace IpManager.Repository.DashBoard
{
    public partial class DashBoardRepository
    {
        public class PcroomTimeDataDto
        {
            public int PcroomId { get; set; }
            public string PcroomName { get; set; } = string.Empty;
            // Key: 시간(예: "00:00"), Value: UsedPc 값
            public List<ThisAnayzeList> AnalyList { get; set; } = new List<ThisAnayzeList>();
        }
    }
}
