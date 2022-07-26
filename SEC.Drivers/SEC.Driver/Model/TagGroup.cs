namespace SEC.Driver
{
    public class TagGroup
    {
        public byte[]? Command { get; set; }
        public int StartAddress { get; set; }
        public ushort Length { get; set; }
        public List<Tag> Tags { get; set; } = new List<Tag>();
    }
}
