using CommunityToolkit.Mvvm.ComponentModel;
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

public partial class Student : ObservableObject
{
    public int Id { get; set; }
    [ObservableProperty] private string _firstName;
    [ObservableProperty] private string _lastName;
    [ObservableProperty] private string _cnp;
    [ObservableProperty] private string _faculty;
    [ObservableProperty] private StudentType _type;
    public int? RoomId { get; set; }
    [ObservableProperty] private Room? _room;
    public bool Active { get; set; } = true;

    public string FullName => FirstName + " " + LastName;
    public ICollection<Invoice>? Invoices { get; set; }
}
