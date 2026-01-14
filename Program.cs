using H3ORM.Models;
using H3ORM.ORM;

ORM.SetConnectionString("Data Source=persons.db");

Console.WriteLine("Creating table for persons");
ORM.CreateTable(typeof(Person));
Console.WriteLine("Table successfully created");

List<Person> persons = ORM.Select<Person>();