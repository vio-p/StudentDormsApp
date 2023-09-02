using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace StudentDormsApp.Models;

public partial class Dorm : ObservableObject
{
    public int Id { get; set; }
    [ObservableProperty] private int _number;
    [ObservableProperty] private decimal _tax;
    public bool Active { get; set; } = true;

    public ICollection<Room>? Rooms { get; set; }
}
