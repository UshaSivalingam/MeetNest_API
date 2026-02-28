using System;
using BCrypt.Net;

class Program
{
    static void Main(string[] args)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("Super@123");
        Console.WriteLine(hash);
    }
}
