using System.Collections.Generic;

namespace StudentDormsApp.Models;

public class Room
{
    public int Id { get; set; }
    public int Number { get; set; }
    public bool Active { get; set; } = true;
    public int DormId { get; set; }
    public Dorm? Dorm { get; set; }
    public ICollection<Invoice>? Invoices { get; set; }
}
