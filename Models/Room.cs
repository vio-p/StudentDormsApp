using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace StudentDormsApp.Models;

public partial class Room : ObservableObject
{
    public int Id { get; set; }
    [ObservableProperty] private int _number;
    public bool Active { get; set; } = true;
    public int DormId { get; set; }
    [ObservableProperty] private Dorm? _dorm;
    public ICollection<Invoice>? Invoices { get; set; }
}
