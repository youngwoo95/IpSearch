namespace IpManager
{
    public class ResponseUnit<T>
    {
        public string? message { get; set; }
        public T? data { get; set; }
        public int code { get; set; }
    }
}
