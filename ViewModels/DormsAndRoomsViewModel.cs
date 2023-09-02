using Microsoft.EntityFrameworkCore;
using StudentDormsApp.Commands;
using StudentDormsApp.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace StudentDormsApp.ViewModels;

public class DormsAndRoomsViewModel : ViewModelBase
{
    public DormsAndRoomsViewModel()
    {
        using StudentDormsContext context = new();
        Dorms = new(context.Dorms.Where(dorm => dorm.Active));
        Rooms = new(context.Rooms.Include("Dorm").Where(room => room.Active));

        AddDormCommand = new RelayCommand(AddDorm, parameter => DormInputIsValid());
        ModifyDormCommand = new RelayCommand(ModifyDorm, parameter => DormInputIsValid() && SelectedDorm != null);
        DeleteDormCommand = new RelayCommand(DeleteDorm, parameter => SelectedDorm != null);

        AddRoomCommand = new RelayCommand(AddRoom, parameter => RoomInputIsValid());
        ModifyRoomCommand = new RelayCommand(ModifyRoom, parameter => RoomInputIsValid() && SelectedRoom != null);
        DeleteRoomCommand = new RelayCommand(DeleteRoom, parameter => SelectedRoom != null);
    }

    #region Dorms
    public ObservableCollection<Dorm> Dorms { get; }

    public ICommand AddDormCommand { get; }
    public ICommand ModifyDormCommand { get; }
    public ICommand DeleteDormCommand { get; }

    private string _dormNumber;
    public string DormNumber
    {
        get => _dormNumber;
        set
        {
            _dormNumber = value;
            OnPropertyChanged(nameof(DormNumber));
        }
    }

    private string _dormTax;
    public string DormTax
    {
        get => _dormTax;
        set
        {
            _dormTax = value;
            OnPropertyChanged(nameof(DormTax));
        }
    }

    private Dorm _selectedDorm;
    public Dorm SelectedDorm
    {
        get => _selectedDorm;
        set
        {
            _selectedDorm = value;
            if (_selectedDorm != null)
            {
                DormNumber = _selectedDorm.Number.ToString();
                DormTax = _selectedDorm.Tax.ToString();
            }
            else
            {
                DormNumber = null!;
                DormTax = null!;
            }
            OnPropertyChanged(nameof(SelectedDorm));
        }
    }

    private bool DormInputIsValid()
    {
        if (string.IsNullOrEmpty(DormNumber) || string.IsNullOrEmpty(DormTax))
        {
            return false;
        }
        bool numberIsValid = int.TryParse(DormNumber, out _);
        bool taxIsValid = decimal.TryParse(DormTax, out _);
        return numberIsValid && taxIsValid;
    }

    private void AddDorm()
    {
        using StudentDormsContext context = new();
        if (context.Dorms.SingleOrDefault(dorm => dorm.Number == int.Parse(DormNumber) && dorm.Active) != null)
        {
            _ = MessageBox.Show("There is already a dorm with this number!", "Invalid number", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        Dorm dorm = new()
        {
            Number = int.Parse(DormNumber),
            Tax = decimal.Parse(DormTax)
        };
        Dorms.Add(dorm);

        context.Dorms.Add(dorm);
        context.SaveChanges();
    }

    private void ModifyDorm()
    {
        using StudentDormsContext context = new();
        if (context.Dorms.SingleOrDefault(dorm => dorm.Id != SelectedDorm.Id && dorm.Number == int.Parse(DormNumber) && dorm.Active) != null)
        {
            _ = MessageBox.Show("There is already a dorm with this number!", "Invalid number", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        SelectedDorm.Number = int.Parse(DormNumber);
        SelectedDorm.Tax = decimal.Parse(DormTax);

        Dorm dbDorm = context.Dorms.Single(dorm => dorm.Id == SelectedDorm.Id);
        dbDorm.Number = SelectedDorm.Number;
        dbDorm.Tax = SelectedDorm.Tax;
        context.SaveChanges();
    }

    private void DeleteDorm()
    {
        using StudentDormsContext context = new();

        if (context.Rooms.Where(room => room.DormId == SelectedDorm.Id && room.Active).ToList().Count > 0)
        {
            _ = MessageBox.Show("This dorm can't be deleted because there are rooms linked to it!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        Dorm dbDorm = context.Dorms.Single(dorm => dorm.Id == SelectedDorm.Id);
        dbDorm.Active = false;
        context.SaveChanges();

        Dorms.Remove(SelectedDorm);
    }
    #endregion

    #region Rooms
    public ObservableCollection<Room> Rooms { get; }

    public ICommand AddRoomCommand { get; }
    public ICommand ModifyRoomCommand { get; }
    public ICommand DeleteRoomCommand { get; }

    private string _roomNumber;
    public string RoomNumber
    {
        get => _roomNumber;
        set
        {
            _roomNumber = value;
            OnPropertyChanged(nameof(RoomNumber));
        }
    }

    private Dorm _selectedDormForRoom;
    public Dorm SelectedDormForRoom
    {
        get => _selectedDormForRoom;
        set
        {
            _selectedDormForRoom = value;
            OnPropertyChanged(nameof(SelectedDormForRoom));
        }
    }

    private Room _selectedRoom;
    public Room SelectedRoom
    {
        get => _selectedRoom;
        set
        {
            _selectedRoom = value;
            if (_selectedRoom != null)
            {
                RoomNumber = _selectedRoom.Number.ToString();
                SelectedDormForRoom = Dorms.Single(dorm => dorm.Id == _selectedRoom.DormId);
            }
            else
            {
                RoomNumber = null!;
                SelectedDormForRoom = null!;
            }
            OnPropertyChanged(nameof(SelectedRoom));
        }
    }

    private bool RoomInputIsValid()
    {
        if (string.IsNullOrEmpty(RoomNumber) || SelectedDormForRoom == null)
        {
            return false;
        }
        bool numberIsValid = int.TryParse(RoomNumber, out _);
        return numberIsValid;
    }

    private void AddRoom()
    {
        using StudentDormsContext context = new();
        if (context.Rooms.SingleOrDefault(room => room.Number == int.Parse(RoomNumber) && room.DormId == SelectedDormForRoom.Id && room.Active) != null)
        {
            _ = MessageBox.Show("There is already a room with this number in the selected dorm!", "Invalid number", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        Room room = new()
        {
            Number = int.Parse(RoomNumber),
            Dorm = context.Dorms.Single(dorm => dorm.Id == SelectedDormForRoom.Id)
        };
        Rooms.Add(room);

        context.Rooms.Add(room);
        context.SaveChanges();
    }

    private void ModifyRoom()
    {
        using StudentDormsContext context = new();
        if (context.Rooms.SingleOrDefault(room => room.Id != SelectedRoom.Id && room.Number == int.Parse(RoomNumber) && room.DormId == SelectedDormForRoom.Id && room.Active) != null)
        {
            _ = MessageBox.Show("There is already a room with this number in the selected dorm!", "Invalid number", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        SelectedRoom.Number = int.Parse(RoomNumber);
        SelectedRoom.DormId = SelectedDormForRoom.Id;
        SelectedRoom.Dorm = SelectedDormForRoom;

        Room dbRoom = context.Rooms.Single(room => room.Id == SelectedRoom.Id);
        dbRoom.Number = SelectedRoom.Number;
        dbRoom.DormId = SelectedRoom.DormId;
        context.SaveChanges();
    }

    private void DeleteRoom()
    {
        using StudentDormsContext context = new();

        if (context.Students.Where(student => student.RoomId == SelectedRoom.Id && student.Active).ToList().Count > 0)
        {
            _ = MessageBox.Show("This room can't be deleted because there are students linked to it!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        Room dbRoom = context.Rooms.Single(room => room.Id == SelectedRoom.Id);
        dbRoom.Active = false;
        context.SaveChanges();

        Rooms.Remove(SelectedRoom);
    }
    #endregion
}
