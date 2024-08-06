using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.DesignPatterns.Prototype
{
    public interface IPrototype<T>
    {
        T Clone();
    }

    public class Book : IPrototype<Book>
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public int ISBN { get; set; }

        // Constructor for the book
        public Book(string title, string author, int isbn)
        {
            Title = title;
            Author = author;
            ISBN = isbn;
        }

        // Implementing the Clone method
        public Book Clone()
        {
            return new Book(Title, Author, ISBN);
        }
    }

    public class BookCache
    {
        private Dictionary<int, Book> _cache = new Dictionary<int, Book>();

        public Book GetBook(int isbn)
        {
            if (_cache.ContainsKey(isbn))
            {
                return _cache[isbn].Clone();
            }
            else
            {
                // Simulate fetching the book from a database
                Book book = FetchBookFromDatabase(isbn);
                _cache.Add(isbn, book);
                return book.Clone(); // Return a clone to ensure the original object remains unaltered
            }
        }

        private Book FetchBookFromDatabase(int isbn)
        {
            // Simulate database access
            Console.WriteLine($"Fetching book with ISBN {isbn} from database...");
            return new Book("Example Title", "Author Name", isbn);
        }
    }

    public class Program 
    { 
        public Program()
        {
            BookCache cache = new BookCache();

            // Fetch a book for the first time
            Book book1 = cache.GetBook(123456);
            Console.WriteLine($"Retrieved book: {book1.Title} by {book1.Author}");

            // Fetch the same book again, this time it should come from the cache
            Book book2 = cache.GetBook(123456);
            Console.WriteLine($"Retrieved book: {book2.Title} by {book2.Author}");

            // Verify that a clone was returned
            Console.WriteLine($"Are book1 and book2 the same instance? {(book1 == book2 ? "Yes" : "No")}");
        }
    }

}
