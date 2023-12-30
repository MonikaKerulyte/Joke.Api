namespace Joke.Api.Model.Dto
{
    public class JokeDto
    {
        public required string Setup { get; set; }
        public required string Delivery { get; set; }
        public required string Category { get; set; }
    }
}
