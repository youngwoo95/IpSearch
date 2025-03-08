namespace IpManager.DTO.Store
{
    public class StorePingDTO
    {
        /// <summary>
        /// 사용중 PC수
        /// </summary>
        public int used { get; set; }

        /// <summary>
        /// 꺼진 PC수
        /// </summary>
        public int unUsed { get; set; }
    }
}
