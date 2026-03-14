using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace ProductionCaptchaSystem.Views
{
    public partial class TableDetailView : Page
    {
        public TableDetailView(DataTable table, string title)
        {
            InitializeComponent();

            TableTitle.Text = title;
            TableGrid.ItemsSource = table?.DefaultView;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}
