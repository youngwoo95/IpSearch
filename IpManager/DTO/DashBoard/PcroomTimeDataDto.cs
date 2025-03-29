namespace IpManager.DTO.DashBoard
{
    public class PcroomTimeDataDto
    {
        public int pcRoomId { get; set; }
        public string pcRoomName { get; set; } = string.Empty;
        // Key: 시간(예: "00:00"), Value: UsedPc 값
        public List<ThisAnayzeList> analyList { get; set; } = new List<ThisAnayzeList>();
    }
}
