using H3ORM.ORM;
namespace H3ORM.Models;
public class Person
{
    public long Id { get; set; } = 0;
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Gender { get; set; } = "";
    public string CPR { get; set; } = "";
    public string Address { get; set; } = "";
}