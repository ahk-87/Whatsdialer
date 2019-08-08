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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace Fanytel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (!(e.Source is Label))
                this.Opacity = 0.7;

            try
            {
                this.DragMove();
            }
            catch (Exception)
            {

            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            this.Opacity = 1;
        }

        async private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TBUsersCount.Text = App.Users.Count.ToString();
            amountTextBox.TextChanged += amountTextBox_TextChanged;
            queryTextBox.Focus();
            listBox.Items.SortDescriptions.Add(new SortDescription("PhoneNumber", ListSortDirection.Ascending));
            this.DataContext = App.Reseller;

            todayLabel.Content = string.Format("Today: {0:0.00} , {1}", Transfer.GetTotalAmount(DateTime.Now), Transfer.GetTotalPrice(DateTime.Now));
            yestLabel.Content = string.Format("Yesterday: {0:0.00} , {1}", Transfer.GetTotalAmount(DateTime.Now.AddDays(-1)), Transfer.GetTotalPrice(DateTime.Now.AddDays(-1)));


            try
            {
                await App.Reseller.Login();
            }
            catch (Exception)
            {
                TextError.Visibility = Visibility.Visible;
            }


            //string data = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Fanytel Records (1-4-2017 to 8-5-2018).txt");
            //string regTransfer = @"-10.00</td>
            //                          <!-- <td style=""font - family: Arial; font - size: 10pt"" align=""center"" ></td> -->

            //                                < td style = ""font-family: Arial; font-size: 10pt"" align = ""center"" > Balance Transferred </ td >

            //                                      < td style = ""font-family: Arial; font-size: 10pt"" align = ""center"" > 15 - Apr - 2017 13:59:07 </ td >



            //                                               < td style = ""font-family: Arial; font-size: 10pt"" align = ""center"" > Balance Sent to 96171344713 </ td > ";
            //string regTransfer = @"  (\-?\d{1,3}\.\d{2}).*?(\d\d\-\w{3}\-\d{4} \d\d\:\d\d\:\d\d).*? (\d{9,13})\</td\>   ";
            //MatchCollection mathces2 = Regex.Matches(data, regTransfer, RegexOptions.Singleline);
            //foreach (Match m in mathces2)
            //{
            //    Transfer t = new Transfer();
            //    t.Amount = double.Parse(m.Groups[1].Value) * -1;
            //    t.Date = DateTime.Parse(m.Groups[2].Value).AddHours(3);
            //    t.PhoneNumber = m.Groups[3].Value;
            //    t.GetPrice();

            //    App.Transfers.Add(t);
            //}
            //App.SaveTransfers();
            //MatchCollection matches = Regex.Matches(data, @"Balance Sent to (\d{9,13})\<");
            //SortedDictionary<string, string> users = new SortedDictionary<string, string>();
            //foreach (Match mat in matches)
            //{
            //    users[mat.Groups[1].Value] = "";
            //}
            //string[] tempUsers = File.ReadAllLines(App.usersPath);
            //foreach (string s in tempUsers)
            //{
            //    string user, pass;
            //    string[] userAndPass = s.Split(new char[] { ',' });
            //    user = userAndPass[0];
            //    pass = userAndPass[1];
            //    users[user] = pass;
            //}

            //List<string> uuu = new List<string>();
            //foreach (KeyValuePair<string, string> kv in users)
            //{
            //    uuu.Add(kv.Key + "," + kv.Value + ",0");
            //}

            //File.WriteAllLines(App.usersPath, uuu.ToArray());

        }

        private void queryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            buttonAddUser.IsEnabled = false;
            TextBox textBox = sender as TextBox;
            string query = textBox.Text;
            FanytelUser selectedUser;

            // Get the name's sorted items
            var users = listBox.Items.Cast<FanytelUser>();

            if (string.IsNullOrEmpty(query))
            {
                listBox.SelectedItem = null;
                return;
            }
            else if (query.Contains(" "))
            {
                query = query.Replace(" ", "").TrimStart(new char[] { '+' });
                queryTextBox.TextChanged -= queryTextBox_TextChanged;
                queryTextBox.Text = query;
                queryTextBox.TextChanged += queryTextBox_TextChanged;
            }
            else if (query[0] > 1600 || (query.Length > 3 && query[3] > 1600))
            {
                StringBuilder build = new StringBuilder();
                foreach (char c in query)
                {
                    if (c > 1600)
                        build.Append(char.ConvertFromUtf32(c - 1584));
                    else
                        build.Append(c);
                }
                query = build.ToString();
                queryTextBox.TextChanged -= queryTextBox_TextChanged;
                queryTextBox.Text = query;
                queryTextBox.TextChanged += queryTextBox_TextChanged;
            }

            selectedUser = users.FirstOrDefault(user => user.PhoneNumber.StartsWith(query));


            if (selectedUser == null)
            {
                if (query.Length == 10 && query.StartsWith("9613"))
                {
                    buttonAddUser.IsEnabled = true;
                }
                else if (query.Length > 10)
                {
                    buttonAddUser.IsEnabled = true;
                }
            }

            listBox.SelectedItem = selectedUser;
            listBox.ScrollIntoView(selectedUser);
        }

        private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Label label = sender as Label;
            switch (label.Content as string)
            {
                case "-":
                    this.WindowState = System.Windows.WindowState.Minimized;
                    break;

                case "X":
                    this.Close();
                    break;
            }
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            transferButton.IsEnabled = listBox.SelectedItem != null &&
                    amountTextBox.Foreground.ToString() == Brushes.Black.ToString();
            if (listBox.SelectedItem != null)
                amountTextBox_TextChanged(null, null);
        }

        async private void balanceTextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            TextBlock balanceTB = sender as TextBlock;
            if (balanceTB.Cursor != Cursors.Wait)
            {
                var bin = balanceTB.GetBindingExpression(TextBlock.TextProperty).ParentBinding;
                balanceTB.Text = "wait";
                FanytelUser user = balanceTB.Tag as FanytelUser;

                balanceTB.Cursor = Cursors.Wait;
                try
                {
                    TextError.Visibility = Visibility.Collapsed;
                    await user.GetBalance();
                }
                catch (Exception)
                {
                    TextError.Visibility = Visibility.Visible;
                }

                balanceTB.Cursor = Cursors.Hand;
                balanceTB.SetBinding(TextBlock.TextProperty, bin);
            }
        }

        async private void transferButton_Click(object sender, RoutedEventArgs e)
        {
            transferButton.IsEnabled = false;
            closeApp.IsEnabled = false;

            transferButton.Content = "waiting...";
            while (App.Reseller.Balance == 0)
            { await Task.Delay(500); }
            transferButton.Content = "Send";

            FanytelUser user = listBox.SelectedItem as FanytelUser;
            amount = Math.Round(amount * 100) / 100;
            if (user.PhoneNumber.EndsWith("*"))
            {
                MessageBox.Show("This number is blacklisted.\n\rCan't Transfer balance from us.", "BlackList", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }
            if (MessageBox.Show(string.Format("Are you sure you want to send {0} $ \r\n    to the number {1}", amount, user.PhoneNumber), "Transfer Credit", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                int resultCode = await App.Reseller.TransferBalance(user.PhoneNumber, amount);
                switch (resultCode)
                {
                    case 0:
                        MessageBox.Show("Internet Error", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;

                    case 1:
                        Transfer trans = new Transfer();
                        trans.Amount = amount;
                        trans.PhoneNumber = user.PhoneNumber;
                        trans.Date = DateTime.Now.AddSeconds(-3);
                        trans.Price = price;
                        App.GetTransfers();
                        App.Transfers.Insert(0, trans);
                        App.SaveTransfers();
                        await user.GetBalance();

                        todayLabel.Content = string.Format("Today: {0:0.00} , {1}", Transfer.GetTotalAmount(DateTime.Now), Transfer.GetTotalPrice(DateTime.Now));

                        amountTextBox.Text = "0";

                        queryTextBox.Focus();
                        queryTextBox.SelectAll();
                        MessageBox.Show("Transferred Successfully");
                        break;
                    case 2:
                        MessageBox.Show(string.Format("This number ({0}) is not a user yet", user.PhoneNumber), "Not found",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        break;
                }
            }
            else
                transferButton.IsEnabled = true;

            closeApp.IsEnabled = true;
        }

        private void amountTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            amountTextBox.SelectAll();
        }

        private void amountTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
               || e.Key == Key.Delete || e.Key == Key.Back || e.Key == Key.Tab || e.Key == Key.Right || e.Key == Key.Left
               || e.Key == Key.Enter || e.Key == Key.Oem1 || e.Key == Key.OemPeriod || e.Key == Key.Decimal)
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        double amount = 0.0;
        int price = 0;

        private void amountTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool isReseller = false;
            FanytelUser user;
            string s = amountTextBox.Text;
            if (string.IsNullOrEmpty(s))
            {
                amountTextBox.Text = "0";
                amountTextBox.SelectAll();
                return;

            }

            int oneDollarPrice = 1500;
            if (listBox.SelectedItem != null)
            {
                user = listBox.SelectedItem as FanytelUser;
                oneDollarPrice = user.ResellerRate;
                isReseller = !(user.ResellerRate == 1500 || user.ResellerRate == 1400);
            }
            else
                return;

            if (s.Contains(":"))
            {
                string[] amountPrice = s.Split(new char[] { ':' });
                double.TryParse(amountPrice[0], out amount);
                int.TryParse(amountPrice[1], out price);
                if (price < 500)
                    price = (int)(amount * oneDollarPrice);
                priceLabel.Content = price.ToString();

                if (amount > 100)
                {
                    transferButton.IsEnabled = false;
                    amountTextBox.Foreground = Brushes.Red;
                    priceLabel.Content = null;
                }

                else
                {
                    transferButton.IsEnabled = listBox.SelectedItem != null;
                    amountTextBox.Foreground = Brushes.Black;
                }

            }
            else
            {
                double.TryParse(s, out amount);
                if (amount > 500)
                {
                    price = (int)amount;
                    if (!isReseller)
                    {
                        if (price >= 11000 && oneDollarPrice == 1500)
                            oneDollarPrice = 1100;
                        else if (price >= 6000 && oneDollarPrice == 1500)
                            oneDollarPrice = 1200;
                        else if (price >= 2500 && oneDollarPrice == 1500)
                            oneDollarPrice = 1250;
                    }
                    else if (oneDollarPrice == 1010) { }
                    else
                    {
                        if (price < 10000)
                            oneDollarPrice = 1000;
                        if (price >= 10000 && price < 58000)
                        {
                            oneDollarPrice = (int)(user.ResellerRate + (58000 - price) * (1000 - user.ResellerRate) / 46400); // 48000 = 12000*4
                        }
                    }
                    amount = (double)price / oneDollarPrice;
                    priceLabel.Content = amount.ToString("0.00");
                }
                else
                {
                    if (!isReseller)
                    {
                        if (amount >= 10 && oneDollarPrice == 1500)
                            oneDollarPrice = 1100;
                        else if (amount >= 5 && oneDollarPrice == 1500)
                            oneDollarPrice = 1200;
                        else if (amount >= 2 && oneDollarPrice == 1500)
                            oneDollarPrice = 1250;
                    }
                    else if (oneDollarPrice == 1010) { }
                    else
                    {
                        if (amount < 10)
                            oneDollarPrice = 1000;
                        if (amount >= 10 && amount < 50)
                        {
                            oneDollarPrice = (int)(user.ResellerRate + (50 - amount) * (1000 - user.ResellerRate) / 40);
                        }
                    }
                    //int dividend = amount > 10 ? 1000 : 100;
                    price = (int)(Math.Ceiling(amount * oneDollarPrice / 500)) * 500;
                    priceLabel.Content = price.ToString();
                }
                int maxLimit = isReseller ? 100 : 20;
                if (amount > maxLimit || amount < 1)
                {
                    transferButton.IsEnabled = false;
                    amountTextBox.Foreground = Brushes.Red;
                    priceLabel.Content = null;
                }

                else
                {
                    transferButton.IsEnabled = listBox.SelectedItem != null;
                    amountTextBox.Foreground = Brushes.Black;
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
        }

        private void buttonAddUser_Click(object sender, RoutedEventArgs e)
        {
            listBox.Visibility = Visibility.Collapsed;
            gridPin.Visibility = Visibility.Visible;
            textBoxPin.Focus();
        }

        private void buttonPinOk_Click(object sender, RoutedEventArgs e)
        {
            listBox.Visibility = Visibility.Visible;
            gridPin.Visibility = Visibility.Collapsed;

            FanytelUser addedUser = new FanytelUser()
            {
                PhoneNumber = queryTextBox.Text,
                Password = textBoxPin.Text
            };

            App.GetUsers();
            App.Users.Insert(0, addedUser);
            App.SaveUsers();
            TBUsersCount.Text = App.Users.Count.ToString();
            queryTextBox.Clear();
            textBoxPin.Clear();

            listBox.ScrollIntoView(addedUser);
            listBox.SelectedItem = addedUser;

            amountTextBox.SelectAll();
            amountTextBox.Focus();
        }

        private void buttonPinCanel_Click(object sender, RoutedEventArgs e)
        {
            listBox.Visibility = Visibility.Visible;
            gridPin.Visibility = Visibility.Collapsed;
            queryTextBox.SelectAll();
            queryTextBox.Focus();
        }

        private void TextBlock_MouseUp(object sender, MouseButtonEventArgs e)
        {
            TransfersWindow window = new TransfersWindow() { WindowStartupLocation = WindowStartupLocation.CenterScreen };
            window.Show();
        }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Process.Start(App.transfersPath);
        }
    }
}
