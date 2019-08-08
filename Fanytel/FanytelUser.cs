using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Fanytel
{
    public class FanytelUser : INotifyPropertyChanged, IComparable<FanytelUser>, IEquatable<FanytelUser>
    {
        const string mainURL = "http://151.236.39.108/whatsappdialer/";
        const string loginURL = "http://151.236.39.108/whatsappdialer/Login.do";
        const string balanceURL = "http://151.236.39.108/whatsappdialer/home/index.jsp";
        //mailPassword=0&loginURL=http%3A%2F%2F5.196.83.87%2Fbilling%2F&language=1&username=96170424633&password=xxxxxxx
        const string paymentsURL = "http://151.236.39.108/whatsappdialer7/payment/paymentView.jsp?showMenu=true";
        //reportSpan=4&dStart=1-4-2017+00%3A00%3A00&dEnd=9-5-2018+23%3A59%3A59&reportType=1&RECORDS_PER_PAGE=30&DownloadReportParam=0
        const string transferURL = "http://151.236.39.108/whatsappdialer/BalanceTransfer.do";
        //pinNo=96171865125&transferAmount=1&password=xxxxxx

        HttpClient client;
        HttpClientHandler handler;
        double balance;

        public string PhoneNumber { get; set; }

        public string Password { get; set; }

        public double Balance
        {
            get { return balance; }
            set
            {
                if (value != balance)
                {
                    balance = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int ResellerRate { get; set; }

        public FanytelUser()
        {
            ResellerRate = 1500;
        }

        public int CompareTo(FanytelUser other)
        {
            return this.PhoneNumber.CompareTo(other.PhoneNumber);
        }

        public bool Equals(FanytelUser other)
        {
            return this.PhoneNumber.Equals(other.PhoneNumber, StringComparison.InvariantCultureIgnoreCase);
        }
        public event PropertyChangedEventHandler PropertyChanged;

        void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void initiateClient()
        {
            handler = new HttpClientHandler();
            client = new HttpClient(handler);

            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:60.0) Gecko/20100101 Firefox/60.0");
            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            //client.DefaultRequestHeaders.Add("Pragma", "no-cache");
            //client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            //client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            //client.DefaultRequestHeaders.ExpectContinue = false;

        }

        public async Task Login()
        {
            HttpResponseMessage response;
            string result;

            initiateClient();

            response = await client.GetAsync(mainURL);
            result = await response.Content.ReadAsStringAsync();

            //jsessionID = handler.CookieContainer.GetCookies(new Uri("http://5.196.83.87/billing/"))[0].Value;

            List<KeyValuePair<string, string>> values = new List<KeyValuePair<string, string>>();
            values.Add(new KeyValuePair<string, string>("mailPassword", "0"));
            values.Add(new KeyValuePair<string, string>("loginURL", mainURL));
            values.Add(new KeyValuePair<string, string>("language", "1"));
            values.Add(new KeyValuePair<string, string>("username", PhoneNumber));
            values.Add(new KeyValuePair<string, string>("password", Password));
            FormUrlEncodedContent content = new FormUrlEncodedContent(values);

            response = await client.PostAsync(loginURL, content);
            result = await response.Content.ReadAsStringAsync();


            string balanceData = Regex.Match(result, "YOUR BALANCE IS:(.*?)USD \\,", RegexOptions.IgnoreCase).Groups[1].Value;
            this.Balance = double.Parse(balanceData);
        }


        public async Task GetBalance()
        {
            if (string.IsNullOrWhiteSpace(Password))
                return;
            if (client == null)
                await Login();
            else
            {
                HttpResponseMessage response;
                string result;

                response = await client.GetAsync(balanceURL);
                result = await response.Content.ReadAsStringAsync();
                string balanceData = Regex.Match(result, "YOUR BALANCE IS:(.*?)USD \\,", RegexOptions.IgnoreCase).Groups[1].Value;
                App.GetUsers();
                this.Balance = double.Parse(balanceData);
                App.SaveUsers();
            }
        }

        public async Task<int> TransferBalance(string number, double amount)
        {
            try
            {
                if (client == null)
                    await Login();
                else
                {
                    HttpResponseMessage response;
                    string result;

                    List<KeyValuePair<string, string>> values = new List<KeyValuePair<string, string>>();
                    values.Add(new KeyValuePair<string, string>("pinNo", number));
                    values.Add(new KeyValuePair<string, string>("transferAmount", amount.ToString("0.00")));
                    values.Add(new KeyValuePair<string, string>("password", Password));
                    FormUrlEncodedContent content = new FormUrlEncodedContent(values);

                    response = await client.PostAsync(transferURL, content);
                    if (response.RequestMessage.RequestUri.AbsolutePath.Contains("failure"))
                        return 2;
                    result = await response.Content.ReadAsStringAsync();
                    string balanceData = Regex.Match(result, "YOUR BALANCE IS:(.*?)USD \\,", RegexOptions.IgnoreCase).Groups[1].Value;
                    double b = double.Parse(balanceData);
                    if (this.Balance == b)
                        this.Balance -= amount;
                    else
                        this.Balance = b;
                }
                return 1;
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }

    public class Transfer
    {
        public string PhoneNumber { get; set; }
        public int Price { get; set; }
        public double Amount { get; set; }
        public DateTime Date { get; set; }

        public Transfer()
        {

        }
        public Transfer(string s)
        {
            string[] data = s.Split(new char[] { ',' });

            //Date = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Parse(data[0]), TimeZoneInfo.Local);
            Date = DateTime.ParseExact(data[0], "dd-MMM-yy hh:mm:ss tt", CultureInfo.CurrentCulture);
            PhoneNumber = data[1];
            Amount = double.Parse(data[2]);
            Price = int.Parse(data[3]);
        }
        //public void GetPrice()
        //{
        //    int oneDollarPrice = 1500;
        //    if (PhoneNumber == "96171499417" || PhoneNumber == "96171865125" || PhoneNumber == "96170676656")
        //        oneDollarPrice = 1080;
        //    else if (PhoneNumber == "96176979633")
        //        oneDollarPrice = 1400;
        //    else if (PhoneNumber == "96176947525")
        //        oneDollarPrice = 1280;

        //    int dividend = Amount > 10 ? 1000 : 100;
        //    Price = (int)(Math.Round(Amount * oneDollarPrice / dividend)) * dividend;
        //}

        public override string ToString()
        {
            return string.Format("{0},{1},{2:0.00},{3}", Date.ToString("dd-MMM-yy hh:mm:ss tt"), PhoneNumber, Amount, Price);
        }
        public static int GetTotalPrice(DateTime date)
        {
            string d = date.AddHours(-4).ToShortDateString();
            var ts = App.Transfers.Where(
                t => t.Date.AddHours(-4).ToShortDateString().Equals(d));
            return ts.Sum(t => t.Price);

        }
        public static double GetTotalAmount(DateTime date)
        {
            string d = date.AddHours(-4).ToShortDateString();
            var ts = App.Transfers.Where(
                t => t.Date.AddHours(-4).ToShortDateString().Equals(d));
            return ts.Sum(t => t.Amount);

        }

        public static void GetTotals(DateTime dateFrom, DateTime dateTo, out int price, out double amount)
        {
            DateTime dateFromModified = new DateTime(dateFrom.Year, dateFrom.Month, dateFrom.Day, 2, 0, 1);
            DateTime dateToModified = new DateTime(dateTo.Year, dateTo.Month, dateTo.Day, 23, 59, 59).AddHours(2);
            var ts = App.Transfers.Where(
                t => t.Date >= dateFromModified && t.Date <= dateToModified);
            price = ts.Sum(t => t.Price);
            amount = ts.Sum(t => t.Amount);

        }

    }

}
