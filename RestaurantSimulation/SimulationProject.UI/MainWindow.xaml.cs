using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
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
using MahApps.Metro.Controls;
using SimulationProject;

namespace RestaurantSimulation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public ObservableCollection<Customer> CustomerCollection { get; set; }
        public ObservableCollection<Tuple<string, double>> StatisticsCollection { get; set; }
        public int CustomerCount { get; set; }
        public MainWindow()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fa-IR");
            CustomerCollection = new ObservableCollection<Customer>();
            StatisticsCollection = new ObservableCollection<Tuple<string, double>>();
            CustomerCount = 20;

            InitializeComponent();

            CalculationHead();
        }

        private static void RemoveAll<T>(ObservableCollection<T> list)
        {
            while (list.Count > 0)
            {
                list.RemoveAt(list.Count - 1);
            }
        }

        private void RunButtonClick(object sender, RoutedEventArgs e)
        {
            CalculationHead();
        }

        private void CalculationHead()
        {
            var enteringDifferencePossibilities =
                Enumerable.Range(1, 8).ToList().Select(x => new Tuple<int, double>(x, .125));

            var servicePossibilties = new[] { .10, .20, .30, .25, .10, .05 };
            var serviceDurationPossibilties = Enumerable.Range(1, servicePossibilties.Length)
                .Zip(servicePossibilties, (x, y) => new Tuple<int, double>(x, y));
            CalculationsBody(enteringDifferencePossibilities, serviceDurationPossibilties);
        }

        private void CalculationsBody(IEnumerable<Tuple<int, double>> enteringDifferencePossibility,
            IEnumerable<Tuple<int, double>> serviceDurationPossibilties)
        {
            RemoveAll(CustomerCollection);
            RemoveAll(StatisticsCollection);

            var rs = new RestaurantSimulator(
                new ItemPicker<int>(new RandomMantissa()),
                new ItemPicker<int>(new RandomMantissa()));

            enteringDifferencePossibility.ToList()
                .ForEach(x => rs.AddEnteringDifferencePossibility(x.Item1, x.Item2));

            serviceDurationPossibilties.ToList()
                .ForEach(x => rs.AddServiceTimePossibility(x.Item1, x.Item2));

            var customers = rs.Take(CustomerCount).ToList();
            
            customers.ForEach(x => CustomerCollection.Add(x));

            var statistics = new[]
            {
                new { Title = "متوسط مدت انتظار هر مشتری", Value = customers.WaitingTimeAverage() },
                new { Title = "احتمال مجبور شدن مشتری به انتظار", Value = customers.WaitedCustomersRatio() },
                new { Title = "نسبت بیکاری خدمت‌دهنده", Value = customers.NoCustomerRatio() },
                new { Title = "متوسط مدت خدمت‌دهی", Value = customers.ServiceAverage() },
                new { Title = "متوسط مدت بین هر دو ورود", Value = customers.EnteringDiffAverage() },
                new { Title = "متوسط مدت انتظار", Value = customers.WaitingAverage() },
                new { Title = "متوسط مدت زمان حضور مشتری", Value = customers.CustomerInSystemAverage() },
            };

            statistics.ToList().ForEach(x => StatisticsCollection.Add(new Tuple<string, double>(x.Title, x.Value)));
        }
    }
}
