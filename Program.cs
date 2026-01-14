using H3ORM.Models;
using H3ORM.ORM;

ORM.SetConnectionString("Data Source=persons.db");

Console.WriteLine("Creating table for persons");
ORM.CreateTable(typeof(Person));
Console.WriteLine("Table successfully created");

string findName = "Konrad";
List<Person> persons = ORM.Select<Person>("FirstName = 'Konrad' and Id < 4");
foreach (Person person in persons)
{
    Console.WriteLine(person.FirstName);
}