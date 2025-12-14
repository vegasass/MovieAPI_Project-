namespace MovieAPI.Dto
{
    public class ReviewInputDto
    {
        public int MovieId { get; set; }
        public string UserName { get; set; } = "";
        public int Score { get; set; }
        public string Text { get; set; } = "";
    }

}
