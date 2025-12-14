namespace MovieAPI.Dto
{
    public class MovieInputDto
    {
        public string Title { get; set; }
        public int Year { get; set; }
        public string Genre { get; set; } 
        public List<int> ActorIds { get; set; } 
    }
}
