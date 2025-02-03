namespace IpManager
{
    public class ResponseList<T>
    {
        public string? message { get; set; }
        public List<T>? data { get; set; }
        public int code { get; set; }
    }
}
