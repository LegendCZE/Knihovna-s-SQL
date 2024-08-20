using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Library;
using System.Collections;
using System.Xml.Linq;

namespace Library
{
    public class Person
    {
        public string Name { get; set; }
        public DateTime DateOfBirth { get; set; }

        public Person(string name, DateTime dateOfBirth)
        {
            Name = name;
            DateOfBirth = dateOfBirth;
        }
    }

    public class Author : Person
    {
        public string Bio { get; set; }

        public Author(string name, DateTime dateOfBirth, string bio)
            : base(name, dateOfBirth)
        {
            Bio = bio;
        }
    }

    public class Reader : Person
    {
        public string Address { get; set; }

        public Reader(string name, DateTime dateOfBirth, string address)
            : base(name, dateOfBirth)
        {
            Address = address;
        }
    }

    // Třída Book
    public class Book
    {
        public string Title { get; set; }
        public Author Author { get; set; }
        public string ISBN { get; set; }

        public Book(string title, Author author, string isbn)
        {
            Title = title;
            Author = author;
            ISBN = isbn;
        }

        public override string ToString()
        {
            return $"Title: {Title}, Author: {Author.Name}, ISBN: {ISBN}";
        }
    }

    public interface ILibrary
    {
        void AddBook(string title, Author author, string isbn);
        void RemoveBook(string isbn);
        void ListBooks();
    }

    public class InMemoryLibrary : ILibrary
    {
        private List<Book> books = new List<Book>();

        public void AddBook(string title, Author author, string isbn)
        {
            books.Add(new Book(title, author, isbn));
        }

        public void RemoveBook(string isbn)
        {
            var book = books.SingleOrDefault(book => book.ISBN == isbn);
            if (book == null)
            {
                throw new Exception($"Book with ISBN {isbn} not found.");
            }
            books.Remove(book);
        }

        public void ListBooks()
        {
            if (books.Count == 0)
            {
                Console.WriteLine("Library is empty.");
            }
            else
            {
                foreach (var book in books)
                {
                    Console.WriteLine(book);
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Author author1 = new Author("George Orwell", new DateTime(1903, 6, 25), "English novelist and essayist.");
            Reader reader1 = new Reader("John Doe", new DateTime(1990, 5, 15), "123 Main St, Springfield");

            ILibrary library = new SqlClientLibrary();
            library.AddBook("1984", author1, "978-0451524935");
            library.AddBook("Animal Farm", author1, "978-0451526342");

            Console.WriteLine("List of books in the library:");
            library.ListBooks();

            try
            {
                library.RemoveBook("1234567890");
                library.RemoveBook("978-0451524935");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine("List of books in the library after removal:");
            library.ListBooks();
        }
    }
}

class SqlClientLibrary : ILibrary
{
    string connectionString = @"Server=localhost\SQLEXPRESS;Database=master;Trusted_Connection=True;Connect Timeout=3;";

    public SqlClientLibrary()
    {
        using (var conn = new SqlConnection(connectionString))
        {
            conn.Open();
            string query = "IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Books' AND xtype='U') CREATE TABLE Books (Title varchar(255), Author varchar(255), Isbn varchar(255))";

            using (SqlCommand command = new SqlCommand(query, conn))
            {
                command.ExecuteNonQuery();
            }
        }
    }

    public void AddBook(string title, Author author, string isbn)
    {
        string query = "INSERT INTO Books (Title, Author, Isbn) VALUES (@Title, @Author, @Isbn);";

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            SqlCommand command = new SqlCommand(query, conn);
            command.Parameters.AddWithValue("@Title", title);
            command.Parameters.AddWithValue("@Author", author.Name);
            command.Parameters.AddWithValue("@Isbn", isbn);

            conn.Open();
            command.ExecuteNonQuery();
        }
    }

    public void RemoveBook(string isbn)
    {
        string query = "DELETE FROM Books WHERE Isbn = @Isbn";

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            SqlCommand command = new SqlCommand(query, conn);
            command.Parameters.AddWithValue("@Isbn", isbn);

            conn.Open();
            command.ExecuteNonQuery();
        }
    }

    public void ListBooks()
    {
        string query = "SELECT * FROM Books";

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();
            SqlCommand command = new SqlCommand(query, conn);
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var title = reader.GetString(0);
                    var author = reader.GetString(1);
                    var isbn = reader.GetString(2);

                    Console.WriteLine($"Title: {title}, Author: {author}, ISBN: {isbn}");
                }
            }
        }
    }
}

