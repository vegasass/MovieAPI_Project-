namespace MovieAPI.Dto
{
    public class ReviewOutputDto
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public string UserName { get; set; } = "";
        public int Score { get; set; }
        public string Text { get; set; } = "";
    }

}
