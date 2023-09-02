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

    public ICommand ShowHomePageCommand { get; }
    public ICommand ShowStudentsPageCommand { get; }
    public ICommand ShowDormsAndRoomsPageCommand { get; }

    public MainViewModel()
    {
        CurrentPageViewModel = new HomeViewModel();

        ShowHomePageCommand = new RelayCommand(ShowHomePage);
        ShowStudentsPageCommand = new RelayCommand(ShowStudentsPage);
        ShowDormsAndRoomsPageCommand = new RelayCommand(ShowDormsAndRoomsPage);
    }

    private void ShowHomePage()
    {
        CurrentPageViewModel = new HomeViewModel();
    }

    private void ShowStudentsPage()
    {
        CurrentPageViewModel = new StudentsViewModel();
    }

    private void ShowDormsAndRoomsPage()
    {
        CurrentPageViewModel = new DormsAndRoomsViewModel();
    }
}
