namespace MovieAPI.Dto
{
    public class MovieOutputDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }
        public string Genre { get; set; }
        public double Rating { get; set; }
        public int VotesCount { get; set; }
        public List<int> ActorIds { get; set; } 
    }
}
