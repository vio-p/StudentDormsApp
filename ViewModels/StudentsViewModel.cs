using CountryValidation;
using Microsoft.EntityFrameworkCore;
using StudentDormsApp.Commands;
using StudentDormsApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace StudentDormsApp.ViewModels;

public class StudentsViewModel : ViewModelBase
{
    public StudentsViewModel()
    {
        using StudentDormsContext context = new();
        Students = new(context.Students.Include("Room").Where(student => student.Active));
        Rooms = new(context.Rooms.Include("Dorm").Where(room => room.Active).OrderBy(room => room.Dorm.Number).ThenBy(room => room.Number));

        AddStudentCommand = new RelayCommand(AddStudent, parameter => InputIsValid());
        ModifyStudentCommand = new RelayCommand(ModifyStudent, parameter => InputIsValid() && SelectedStudent != null);
        DeleteStudentCommand = new RelayCommand(DeleteStudent, parameter => SelectedStudent != null);
        RemoveStudentFromRoomCommand = new RelayCommand(RemoveStudentFromRoom, parameter => SelectedStudent != null);
    }

    public ObservableCollection<Student> Students { get; }
    public ObservableCollection<Room> Rooms { get; }
    public List<StudentType> StudentTypes { get; } = Enum.GetValues<StudentType>().ToList();

    public ICommand AddStudentCommand { get; }
    public ICommand ModifyStudentCommand { get; }
    public ICommand DeleteStudentCommand { get; }
    public ICommand RemoveStudentFromRoomCommand { get; }

    private string _firstName;
    public string FirstName
    {
        get => _firstName;
        set
        {
            _firstName = value;
            OnPropertyChanged(nameof(FirstName));
        }
    }

    private string _lastName;
    public string LastName
    {
        get => _lastName;
        set
        {
            _lastName = value;
            OnPropertyChanged(nameof(LastName));
        }
    }

    private string _cnp;
    public string Cnp
    {
        get => _cnp;
        set
        {
            _cnp = value;
            OnPropertyChanged(nameof(Cnp));
        }
    }

    private string _faculty;
    public string Faculty
    {
        get => _faculty;
        set
        {
            _faculty = value;
            OnPropertyChanged(nameof(Faculty));
        }
    }

    private StudentType _selectedStudentType;
    public StudentType SelectedStudentType
    {
        get => _selectedStudentType;
        set
        {
            _selectedStudentType = value;
            OnPropertyChanged(nameof(SelectedStudentType));
        }
    }

    private Room? _selectedRoom;
    public Room? SelectedRoom
    {
        get => _selectedRoom;
        set
        {
            _selectedRoom = value;
            OnPropertyChanged(nameof(SelectedRoom));
        }
    }

    private Student _selectedStudent;
    public Student SelectedStudent
    {
        get => _selectedStudent;
        set
        {
            _selectedStudent = value;

            if (_selectedStudent != null)
            {
                FirstName = _selectedStudent.FirstName;
                LastName = _selectedStudent.LastName;
                Cnp = _selectedStudent.Cnp;
                Faculty = _selectedStudent.Faculty;
                SelectedStudentType = _selectedStudent.Type;
                SelectedRoom = Rooms.SingleOrDefault(room => room.Id == _selectedStudent.RoomId);
            }
            else
            {
                FirstName = null!;
                LastName = null!;
                Cnp = null!;
                Faculty = null!;
                SelectedStudentType = StudentType.OnTax;
                SelectedRoom = null!;
            }

            OnPropertyChanged(nameof(SelectedStudent));
        }
    }

    private bool InputIsValid()
    {
        return !(string.IsNullOrEmpty(FirstName)
           || string.IsNullOrEmpty(LastName)
           || string.IsNullOrEmpty(Cnp)
           || string.IsNullOrEmpty(Faculty));
    }

    private void AddStudent()
    {
        CountryValidator validator = new();
        ValidationResult validationResult = validator.ValidateNationalIdentityCode(Cnp, Country.RO);
        if (!validationResult.IsValid)
        {
            _ = MessageBox.Show("The CNP is not valid!", "Invalid CNP", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        using StudentDormsContext context = new();
        if (context.Students.SingleOrDefault(student => student.Cnp == Cnp && student.Active) != null)
        {
            _ = MessageBox.Show("There is already a student with this CNP!", "Invalid CNP", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        Student student = new()
        {
            FirstName = FirstName,
            LastName = LastName,
            Cnp = Cnp,
            Faculty = Faculty,
            Type = SelectedStudentType,
            Room = context.Rooms.Include("Dorm").Single(room => room.Id == SelectedRoom.Id)
        };
        Students.Add(student);

        context.Students.Add(student);
        context.SaveChanges();
    }

    private void ModifyStudent()
    {
        CountryValidator validator = new();
        ValidationResult validationResult = validator.ValidateNationalIdentityCode(Cnp, Country.RO);
        if (!validationResult.IsValid)
        {
            _ = MessageBox.Show("The CNP is not valid!", "Invalid CNP", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        using StudentDormsContext context = new();
        if (context.Students.SingleOrDefault(student => student.Id != SelectedStudent.Id && student.Cnp == Cnp && student.Active) != null)
        {
            _ = MessageBox.Show("There is already a student with this CNP!", "Invalid CNP", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        SelectedStudent.FirstName = FirstName;
        SelectedStudent.LastName = LastName;
        SelectedStudent.Cnp = Cnp;
        SelectedStudent.Faculty = Faculty;
        SelectedStudent.Type = SelectedStudentType;
        SelectedStudent.Room = SelectedRoom;

        Student dbStudent = context.Students.Single(student => student.Id == SelectedStudent.Id);
        dbStudent.FirstName = SelectedStudent.FirstName;
        dbStudent.LastName = SelectedStudent.LastName;
        dbStudent.Cnp = SelectedStudent.Cnp;
        dbStudent.Faculty = SelectedStudent.Faculty;
        dbStudent.Type = SelectedStudent.Type;
        dbStudent.RoomId = SelectedRoom.Id;

        context.SaveChanges();
    }

    private void DeleteStudent()
    {
        using StudentDormsContext context = new();

        if (context.Invoices.Where(invoice => invoice.StudentId == SelectedStudent.Id && invoice.State == InvoiceState.Issued).ToList().Count > 0)
        {
            _ = MessageBox.Show("This student can't be deleted because they have unpaid invoices!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        Student dbStudent = context.Students.Single(student => student.Id == SelectedStudent.Id);
        dbStudent.Active = false;
        context.SaveChanges();

        Students.Remove(SelectedStudent);
    }

    private void RemoveStudentFromRoom()
    {
        using StudentDormsContext context = new();

        if (context.Invoices.Where(invoice => invoice.StudentId == SelectedStudent.Id && invoice.State == InvoiceState.Issued).ToList().Count > 0)
        {
            _ = MessageBox.Show("This student can't be deleted because they have unpaid invoices!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        SelectedRoom = null!;
        SelectedStudent.Room = null!;
        Student dbStudent = context.Students.Single(student => student.Id == SelectedStudent.Id);
        dbStudent.RoomId = null; // doesn't work, i think room should be nullable in Student
        // also, do we need RoomId in Invoice?
        context.SaveChanges();
    }
}
