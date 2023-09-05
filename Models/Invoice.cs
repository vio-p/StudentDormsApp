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
    public Student? Student { get; set; }
    public int RoomId { get; set; }
    public Room? Room { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public InvoiceState State { get; set; } = InvoiceState.Issued;
    public decimal Amount { get; set; }
    public decimal? AmountPaid { get; set; }

    public int DaysLate { get; private set; }
    public decimal PenaltyFee => 0.1m * Amount * DaysLate;
    public decimal TotalAmount => Amount + PenaltyFee;

    public void UpdateDaysLate(DateTime date)
    {
        if (date < DueDate)
        {
            DaysLate = 0;
        }
        else
        {
            DaysLate = (date - DueDate).Days;
        }
    }
}
