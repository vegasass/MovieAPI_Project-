namespace MovieAPI.Models
{
    public class Playlist
    {
        public int Id { get; set; }
        public string UserName { get; set; } = "";
        public string Name { get; set; } = "";
        public List<int> MovieIds { get; set; } = new();
    }
}
