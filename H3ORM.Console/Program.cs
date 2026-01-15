
//STOP
using H3ORM.Library.Models;
using H3ORM.Library;

ORM.SetConnectionString("Data Source=persons.db");

Console.WriteLine("Creating table for persons");
ORM.CreateTable(typeof(Person));
Console.WriteLine("Table successfully created");

