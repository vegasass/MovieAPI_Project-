namespace MovieAPI.Dto
{
    public class PlaylistInputDto
    {
        public string UserName { get; set; } = "";
        public string Name { get; set; } = "";
        public List<int>? MovieIds { get; set; }
    }

}
