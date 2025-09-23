namespace HelloWorld.Models
{
    public class Computer
    {
        public int ComputerId { get; set; }
        public string Motherboard { get; set; } = "";
        public int? CPUCores { get; set; }
        public bool HASWifi { get; set; }
        public bool HASLTE { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public decimal Price { get; set; }
        public string VideoCard { get; set; } = "";
    }
}