using CountryValidation;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.EntityFrameworkCore;
using StudentDormsApp.Commands;
using StudentDormsApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace StudentDormsApp.ViewModels;

public enum Month
{
    None,
    January,
    February,
    March,
    April,
    May,
    June,
    July,
    August,
    September,
    October,
    November,
    December
}

public class HomeViewModel : ViewModelBase
{
    public HomeViewModel()
    {
        FindStudentCommand = new RelayCommand(FindStudent, parameter => !string.IsNullOrEmpty(InputCnp));

        PaySelectedInvoiceCommand = new RelayCommand(PaySelectedInvoice, parameter => SelectedInvoice != null);
        RemoveSelectedInvoiceCommand = new RelayCommand(RemoveSelectedInvoice, parameter => SelectedInvoice != null);
        PayInvoicesCommand = new RelayCommand(PayInvoices, parameter => InvoicesToPay.Count > 0);
    }

    public ICommand FindStudentCommand { get; }
    public ICommand PayInvoicesCommand { get; }
    public ICommand PaySelectedInvoiceCommand { get; }
    public ICommand RemoveSelectedInvoiceCommand { get; }

    public ObservableCollection<Invoice> Invoices { get; set; } = new();

    public List<Invoice> InvoicesToPay { get; set; } = new();

    private bool _datePickerIsEnabled = true;
    public bool DatePickerIsEnabled
    {
        get => _datePickerIsEnabled;
        set
        {
            _datePickerIsEnabled = value;
            OnPropertyChanged(nameof(DatePickerIsEnabled));
        }
    }

    private DateTime _currentDate = new(DateTime.Now.Year, (int)Month.October, 1);
    public DateTime CurrentDate
    {
        get => _currentDate;
        set
        {
            if (_currentDate > value)
            {
                return;
            }
            _currentDate = value;
            OnPropertyChanged(nameof(CurrentDate));
        }
    }

    private string? _inputCnp;
    public string? InputCnp
    {
        get => _inputCnp;
        set
        {
            _inputCnp = value;
            OnPropertyChanged(nameof(InputCnp));
        }
    }

    private Student _student;
    public Student Student
    {
        get => _student;
        set
        {
            _student = value;
            OnPropertyChanged(nameof(Student));
        }
    }

    private Invoice _selectedInvoice;
    public Invoice SelectedInvoice
    {
        get => _selectedInvoice;
        set
        {
            _selectedInvoice = value;
            OnPropertyChanged(nameof(SelectedInvoice));
        }
    }

    private string _monthsToPay;
    public string MonthsToPay
    {
        get => _monthsToPay;
        set
        {
            _monthsToPay = value;
            OnPropertyChanged(nameof(MonthsToPay));
        }
    }

    private Visibility _studentDataVisibility = Visibility.Hidden;
    public Visibility StudentDataVisibility
    {
        get => _studentDataVisibility;
        set
        {
            _studentDataVisibility = value;
            OnPropertyChanged(nameof(StudentDataVisibility));
        }
    }

    private void FindStudent()
    {
        CountryValidator validator = new();
        ValidationResult validationResult = validator.ValidateNationalIdentityCode(InputCnp, Country.RO);
        if (!validationResult.IsValid)
        {
            _ = MessageBox.Show("The CNP is not valid!", "Invalid CNP", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        using StudentDormsContext context = new();
        Student? student = context.Students.Include("Room").SingleOrDefault(student => student.Cnp == InputCnp && student.Active);
        if (student == null)
        {
            _ = MessageBox.Show("No student with this CNP was found in the database!", "Student not found", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        Student = student;

        IssueInvoices();

        var invoices = context.Invoices
            .Where(invoice => invoice.StudentId == Student.Id && invoice.State == InvoiceState.Issued)
            .OrderBy(invoice => invoice.IssueDate)
            .ToList();

        invoices.ForEach(invoice => invoice.UpdateDaysLate(CurrentDate));
        invoices.ForEach(Invoices.Add);

        if (IsStudentExpelled())
        {
            Student dbStudent = context.Students.Single(student => student.Id == Student.Id);
            dbStudent.RoomId = null;
            context.SaveChanges();

            _ = MessageBox.Show("This student was expelled after not paying 3 months in a row!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        StudentDataVisibility = Visibility.Visible;
        DatePickerIsEnabled = false;
    }

    private void PaySelectedInvoice()
    {
        if (InvoicesToPay.Contains(SelectedInvoice))
        {
            return;
        }
        if (Invoices.Where(invoice => invoice.IssueDate < SelectedInvoice.IssueDate && !InvoicesToPay.Contains(invoice)).ToList().Count > 0)
        {
            _ = MessageBox.Show("There are older invoices that have to be paid first!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        InvoicesToPay.Add(SelectedInvoice);
        MonthsToPay = GetMonthsOfInvoicesToPay();
    }

    private void RemoveSelectedInvoice()
    {
        if (!InvoicesToPay.Contains(SelectedInvoice))
        {
            return;
        }
        if (InvoicesToPay.Where(invoice => invoice.IssueDate > SelectedInvoice.IssueDate).ToList().Count > 0)
        {
            _ = MessageBox.Show("This invoice can't be removed before some others that are more recently issued!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        InvoicesToPay.Remove(SelectedInvoice);
        MonthsToPay = GetMonthsOfInvoicesToPay();
    }

    private void PayInvoices()
    {
        using StudentDormsContext context = new();

        StringBuilder paymentInfo = new();

        decimal receiptAmount = 0;

        foreach (var invoice in InvoicesToPay)
        {
            paymentInfo.AppendLine($"\t{(Month)invoice.DueDate.Month} - {invoice.TotalAmount}");
            receiptAmount += invoice.TotalAmount;

            Invoice dbInvoice = context.Invoices.Single(i => i.Id == invoice.Id);
            dbInvoice.State = InvoiceState.Paid;
            dbInvoice.AmountPaid = invoice.TotalAmount;
            context.SaveChanges();
            Invoices.Remove(invoice);
        }
        InvoicesToPay.Clear();
        MonthsToPay = string.Empty;

        Random random = new Random();
        int random4DigitNumber = random.Next(1000, 10000);

        string receiptPath = $"{CurrentDate.ToString("yyMMdd")}_{random4DigitNumber}_{Student.FullName.Replace(" ", string.Empty)}.pdf";


        PdfWriter writer = new(receiptPath);
        PdfDocument pdf = new(writer);
        Document document = new(pdf);
        Paragraph header = new($"Receipt {CurrentDate:dd.MM.yyyy}");
        header.SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER).SetFontSize(20);
        document.Add(header);

        LineSeparator lineSeparator = new LineSeparator(new SolidLine());
        document.Add(lineSeparator);

        Paragraph paragraph = new Paragraph($"Full name: {Student.FullName}\n" +
            $"Faculty: {Student.Faculty}\n" +
            $"Room: {Student.Room?.Number}\n" +
            $"Dorm: {context.Dorms.Single(dorm => dorm.Id == Student.Room!.DormId).Number}\n" +
            "Paid for following months:\n" +
            paymentInfo.ToString() +
            $"Receipt amount: {receiptAmount}");

        document.Add(paragraph);
        document.Close();
    }

    private void IssueInvoices()
    {
        if (CurrentDate.Month > (int)Month.June && CurrentDate.Month < (int)Month.October)
        {
            return;
        }
        int academicYear = (CurrentDate.Month >= (int)Month.January && CurrentDate.Month < (int)Month.July) ? CurrentDate.Year - 1 : CurrentDate.Year;
        DateTime universityStartDate = new(academicYear, (int)Month.October, 1); // i consider 1.10 to be the start date of the academic year
        DateTime universityEndDate = new(academicYear + 1, (int)Month.June, 30); // i consider 30.6 to be the end date of the academic year

        using StudentDormsContext context = new();

        foreach (DateTime firstDayOfMonth in GetMonthsInRange(universityStartDate, CurrentDate <= universityEndDate ? CurrentDate : universityEndDate))
        {
            if (context.Invoices.SingleOrDefault(invoice => invoice.StudentId == Student.Id && invoice.IssueDate == firstDayOfMonth) != null)
            {
                continue;
            }
            else
            {
                if (Student.RoomId == null)
                {
                    _ = MessageBox.Show("This student is not assigned to a room!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Invoice invoice = new()
                {
                    StudentId = Student.Id,
                    RoomId = Student.RoomId.Value,
                    IssueDate = firstDayOfMonth,
                    DueDate = GetLastDayOfMonth(firstDayOfMonth),
                    State = InvoiceState.Issued,
                    Amount = ComputeInvoiceAmountForStudent(),
                    AmountPaid = null
                };

                int month = firstDayOfMonth.Month;
                if (month == (int)Month.December
                    || month == (int)Month.January
                    || month == (int)Month.February
                    || month == (int)Month.April)
                {
                    invoice.Amount *= 0.75m; // for these months, the tax is only 3/4 of the original tax because of holidays
                }

                context.Invoices.Add(invoice);
                context.SaveChanges();
            }
        }
    }

    private bool IsStudentExpelled()
    {
        return Invoices.Count(invoice => invoice.DueDate < CurrentDate) >= 3;
    }
    private string GetMonthsOfInvoicesToPay()
    {
        StringBuilder builder = new();
        InvoicesToPay.ForEach(invoice => builder.Append($"{(Month)invoice.IssueDate.Month}, "));
        if (builder.Length > 2)
        {
            builder.Remove(builder.Length - 2, 2);
        }
        return builder.ToString();
    }

    private DateTime GetLastDayOfMonth(DateTime date)
    {
        DateTime firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
        DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
        return lastDayOfMonth;
    }

    private IEnumerable<DateTime> GetMonthsInRange(DateTime startDate, DateTime endDate)
    {
        DateTime currentDate = startDate;

        while (currentDate <= endDate)
        {
            yield return new DateTime(currentDate.Year, currentDate.Month, 1);
            currentDate = currentDate.AddMonths(1);
        }
    }

    private decimal ComputeInvoiceAmountForStudent()
    {
        using StudentDormsContext context = new();
        Room studentRoom = context.Students.Include("Room").Single(student => student.Id == Student.Id).Room!;
        decimal dormTax = context.Rooms.Include("Dorm").Single(room => room.Id == studentRoom.Id).Dorm.Tax;

        switch (Student.Type)
        {
            case StudentType.OnBudget:
                return dormTax;
            case StudentType.OnTax:
                return 2 * dormTax;
            case StudentType.PartiallyExempted:
                return dormTax / 2;
            case StudentType.FullyExempted:
                return 0;
            default:
                throw new Exception("Type unrecognized");
        }
    }
}
