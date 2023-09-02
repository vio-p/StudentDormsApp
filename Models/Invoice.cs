using System;

namespace StudentDormsApp.Models;

public enum InvoiceState
{
    Issued,
    Paid
}

public class Invoice
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public Student Student { get; set; }
    public int RoomId { get; set; }
    public Room Room { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public InvoiceState State { get; set; } = InvoiceState.Issued;
    public decimal Amount { get; set; }
}
