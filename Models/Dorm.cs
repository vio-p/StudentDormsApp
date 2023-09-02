using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace StudentDormsApp.Models;

public class Dorm
{
    public int Id { get; set; } 
    public int Number { get; set; }
    public decimal Tax { get; set; }
    public bool Active { get; set; } = true;

    public ICollection<Room>? Rooms { get; set; }
}
