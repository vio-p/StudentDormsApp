using StudentDormsApp.Commands;
using StudentDormsApp.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using static CommunityToolkit.Mvvm.ComponentModel.__Internals.__TaskExtensions.TaskAwaitableWithoutEndValidation;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StudentDormsApp.ViewModels;

public class DormsAndRoomsViewModel : ViewModelBase
{
    public ObservableCollection<Dorm> Dorms { get; }

    public ICommand AddDormCommand { get; }
    public ICommand ModifyDormCommand { get; }
    public ICommand DeleteDormCommand { get; }

    public DormsAndRoomsViewModel()
    {
        using StudentDormsContext context = new();
        Dorms = new(context.Dorms.Where(dorm => dorm.Active));

        AddDormCommand = new RelayCommand(AddDorm, parameter => DormInputIsValid());
        ModifyDormCommand = new RelayCommand(ModifyDorm, parameter => DormInputIsValid() && SelectedDorm != null);
        DeleteDormCommand = new RelayCommand(DeleteDorm, parameter => SelectedDorm != null);
    }

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
}
