namespace visionAI_lab2.Models
{
    public class ImageAnalysis
    {
        public string Description { get; set; }
        public List<string> Tags { get; set; }
        public List<string> Categories { get; set; }
        public List<string> Objects { get; set; }
        public string ThumbnailUrl { get; set; }
    }
}
