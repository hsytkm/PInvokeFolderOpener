using Microsoft.Xaml.Behaviors;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace PInvokeFolderOpener.App;

internal sealed class OpenFolderDialogAction : TriggerAction<DependencyObject>
{
    public static readonly DependencyProperty UseCsWin32Property =
        DependencyProperty.Register(nameof(UseCsWin32), typeof(bool), typeof(OpenFolderDialogAction));

    public bool UseCsWin32
    {
        get => (bool)GetValue(UseCsWin32Property);
        set => SetValue(UseCsWin32Property, value);
    }

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(OpenFolderDialogAction),
            new FrameworkPropertyMetadata("Open Folder"));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty SelectedFolderPathProperty =
        DependencyProperty.Register(nameof(SelectedFolderPath), typeof(string), typeof(OpenFolderDialogAction),
            new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public string SelectedFolderPath
    {
        get => (string)GetValue(SelectedFolderPathProperty);
        set => SetValue(SelectedFolderPathProperty, value);
    }

    protected override void Invoke(object parameter)
    {
        if (Window.GetWindow(AssociatedObject) is not Window window) return;

        var initialDirectoryPath = GetExeDirectoryPath();

        if (UseCsWin32)
        {
            Debug.WriteLine("CsWin32");
            var browser = new CsWin32.FolderBrowserDialog(Title, initialDirectoryPath);
            var result = browser.ShowDialog(window);
            SelectedFolderPath = (result is CsWin32.FolderBrowserDialog.Result.OK) ? browser.SelectedPath : "empty";
        }
        else
        {
            Debug.WriteLine("RawPInvoke");
            var browser = new RawPInvoke.FolderBrowserDialog(Title, initialDirectoryPath);
            var result = browser.ShowDialog(window);
            SelectedFolderPath = (result is RawPInvoke.FolderBrowserDialog.Result.OK) ? browser.SelectedPath : "empty";
        }

        static string GetExeDirectoryPath()
        {
            var exePath = Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(exePath) ?? "";
        }
    }
}
