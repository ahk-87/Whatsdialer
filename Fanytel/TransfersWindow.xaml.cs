using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Fanytel
{
    /// <summary>
    /// Interaction logic for TransfersWindowxaml.xaml
    /// </summary>
    public partial class TransfersWindow : Window
    {
        public TransfersWindow()
        {
            InitializeComponent();
            datePickerTo.DisplayDateEnd = DateTime.Today;
            datePickerTo.SelectedDate = DateTime.Today;
            datePickerFrom.DisplayDateEnd = DateTime.Today;
            DateTime firstOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            datePickerFrom.SelectedDate = firstOfMonth;

            datePickerFrom.SelectedDateChanged += DatePickerFrom_SelectedDateChanged;
            datePickerTo.SelectedDateChanged += DatePickerFrom_SelectedDateChanged;

        }

        private void DatePickerFrom_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == datePickerTo)
                datePickerFrom.DisplayDateEnd = datePickerTo.SelectedDate;
            else
                datePickerTo.DisplayDateStart = datePickerFrom.SelectedDate;

            int price = 0;
            double amount = 0.0;

            Transfer.GetTotals(datePickerFrom.SelectedDate.Value, datePickerTo.SelectedDate.Value, out price, out amount);

            textBoxMoney.Text = price.ToString();
            textBoxCredits.Text = amount.ToString("0.00");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            int price = 0;
            double amount = 0.0;

            Transfer.GetTotals(datePickerFrom.SelectedDate.Value, datePickerTo.SelectedDate.Value, out price, out amount);

            textBoxMoney.Text = price.ToString();
            textBoxCredits.Text = amount.ToString("0.00");

        }
    }
}
