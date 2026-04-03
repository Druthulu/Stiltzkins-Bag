using System.Windows;

// Disambiguate System.Windows.Application from System.Windows.Forms.Application,
// which entered scope when UseWindowsForms was added to the App csproj.
using Application = System.Windows.Application;

namespace StiltzkinsBag.App
{
    public partial class App : Application
    {
    }
}