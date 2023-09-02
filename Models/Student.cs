using System.Collections;
using System.Collections.Generic;

namespace StudentDormsApp.Models;

public enum StudentType
{
    OnTax,
    OnBudget,
    PartiallyExempted,
    FullyExempted
}

public class Student
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string CNP { get; set; }
    public string Faculty { get; set; }
    public StudentType Type { get; set; }
    public int RoomId { get; set; }
    public Room? Room { get; set; }
    public bool Active { get; set; } = true;

    public string FullName => FirstName + " " + LastName;
    public ICollection<Invoice>? Invoices { get; set; }
}
