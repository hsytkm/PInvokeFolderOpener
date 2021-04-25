using System;
using System.Windows;
using System.Windows.Controls;

namespace PInvokeFolderOpener.App
{
    public partial class SelectFolder : UserControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(SelectFolder), new FrameworkPropertyMetadata("title"));
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty SelectedFolderPathProperty =
            DependencyProperty.Register(nameof(SelectedFolderPath), typeof(string), typeof(SelectFolder),
                new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public string SelectedFolderPath
        {
            get => (string)GetValue(SelectedFolderPathProperty);
            set => SetValue(SelectedFolderPathProperty, value);
        }

        public SelectFolder()
        {
            InitializeComponent();
        }
    }
}
