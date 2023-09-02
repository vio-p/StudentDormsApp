using StudentDormsApp.Commands;
using System;
using System.Windows.Input;

namespace StudentDormsApp.ViewModels;

public class MainViewModel : ViewModelBase
{
    private ViewModelBase _currentPageViewModel;
    public ViewModelBase CurrentPageViewModel
    {
        get => _currentPageViewModel;
        set
        {
            _currentPageViewModel = value;
            OnPropertyChanged(nameof(CurrentPageViewModel));
        }
    }

    public ICommand ShowStudentsPageCommand { get; }
    public ICommand ShowHomePageCommand { get; }

    public MainViewModel()
    {
        CurrentPageViewModel = new HomeViewModel();

        ShowHomePageCommand = new RelayCommand(ShowHomePage);
        ShowStudentsPageCommand = new RelayCommand(ShowStudentsPage);
    }

    private void ShowHomePage()
    {
        CurrentPageViewModel = new HomeViewModel();
    }

    private void ShowStudentsPage()
    {
        CurrentPageViewModel = new StudentsViewModel();
    }
}
