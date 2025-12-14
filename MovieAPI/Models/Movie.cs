namespace MovieAPI.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }
        public string Genre { get; set; }
        public List<int> ActorIds { get; set; }
        public double Rating { get; set; } = 0.0;
        public int VotesCount { get; set; } = 0;

    }
}
