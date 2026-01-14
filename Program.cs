using H3ORM.Models;
using H3ORM.ORM;

ORM.SetConnectionString("Data Source=persons.db");

Console.WriteLine("Creating table for persons");
ORM.CreateTable(typeof(Person));
Console.WriteLine("Table successfully created");
Console.WriteLine("Insert person");
Person p = new();
p.FirstName = "Anne";
p.LastName = "Dam";
ORM.Insert(p);
Console.WriteLine($"Person inserted with id {p.Id}");