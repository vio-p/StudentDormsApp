using StudentDormsApp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace StudentDormsApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e) // so that the user doesn't experience delay on first connection to database
        {
            using StudentDormsContext context = new();
            _ = context.Dorms.ToList();

            base.OnStartup(e);
        }
    }
}
