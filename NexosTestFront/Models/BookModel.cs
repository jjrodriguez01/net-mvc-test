using System.ComponentModel.DataAnnotations;

namespace NexosTestFront.Models
{
    public class BookModel
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }
        public string Genre { get; set; }
        public int Pages { get; set; }
        public string AuthorName { get; set; }
    }

    public class CreateBookModel
    {
        public string? Title { get; set; }
        public int Year { get; set; }
        public string? Genre { get; set; }
        public int Pages { get; set; }
        public int AuthorId { get; set; }
    }
}
