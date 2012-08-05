using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
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
using SimulationProject;

namespace SimulationProject.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<string> SimulationsCollection { get; set; }
        public ObservableCollection<RestaurantCustomer> EntityCollection { get; set; }
        public ObservableCollection<Tuple<string, double>> StatisticsCollection { get; set; }
        public int CustomerCount { get; set; }
        public int SelectedSimulationIndex { get; set; }
        public MainWindow()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fa-IR");
            SimulationsCollection = new ObservableCollection<string>
            {
                "صف تک مجرایی",
                "اتورستوران هابیل و خباز",
                "روزنامه فروش",
                "شبیه‌سازی سیستم موجودی",
                "روزنامه فروش در روز",
            };
            StatisticsCollection = new ObservableCollection<Tuple<string, double>>();

            CustomerCount = 20;

            InitializeComponent();

            CalculationsHead();
        }

        private void ComboBoxSelected(object sender, RoutedEventArgs e)
        {
            CalculationsHead();
        }

        private void RunButtonClick(object sender, RoutedEventArgs e)
        {
            CalculationsHead();
        }

        private void CalculationsHead()
        {
            switch (SelectedSimulationIndex)
            {
                case 0:
                    RestaurantSimulation();
                    break;
                case 1:
                    MultiServantQueueSimulation();
                    break;
                case 2:
                    NewsstandSimulation();
                    break;
                case 3:
                    SupplymentSimulation();
                    break;
                case 4:
                    NewsstandOnDifferentDays();
                    break;
                default:
                    dataGrid.ItemsSource = null;
                    statisticsListView.ItemsSource = null;
                    break;
            }
        }

        #region Simulators
        private void RestaurantSimulation()
        {
            var enteringDifferencePossibilities =
                Enumerable.Range(1, 8).ToList().Select(x => new Tuple<int, double>(x, .125));

            var servicePossibilties = new[] { .10, .20, .30, .25, .10, .05 };
            var serviceDurationPossibilties = Enumerable.Range(1, servicePossibilties.Length)
                .Zip(servicePossibilties, (x, y) => new Tuple<int, double>(x, y));

            var simulator = new RestaurantSimulator(new RandomMantissa(), new RandomMantissa());

            enteringDifferencePossibilities.ToList()
                .ForEach(x => simulator.AddEnteringDifferencePossibility(x.Item1, x.Item2));

            serviceDurationPossibilties.ToList()
                .ForEach(x => simulator.AddServiceTimePossibility(x.Item1, x.Item2));

            var customers = simulator.Take(CustomerCount).ToList();

            dataGrid.ItemsSource = customers; // I couldn't do this with databinding 

            statisticsListView.ItemsSource = SimulatorStatistics(typeof(RestaurantCustomersTools), customers);
        }

        private void MultiServantQueueSimulation()
        {
            var habil = new Servant { Name = "هابیل" };
            var khabbaz = new Servant { Name = "خباز" };

            var simulator = new MultiServantQueueSimulator(new[] { habil, khabbaz }, new RandomMantissa(), new RandomMantissa());

            simulator
                .AddEnteringDifferencePossibility(1, .25)
                .AddEnteringDifferencePossibility(2, .40)
                .AddEnteringDifferencePossibility(3, .20)
                .AddEnteringDifferencePossibility(4, .15)

                .AddServiceTimePossibility(habil, 2, .30)
                .AddServiceTimePossibility(habil, 3, .28)
                .AddServiceTimePossibility(habil, 4, .25)
                .AddServiceTimePossibility(habil, 5, .15)

                .AddServiceTimePossibility(khabbaz, 3, .35)
                .AddServiceTimePossibility(khabbaz, 4, .25)
                .AddServiceTimePossibility(khabbaz, 5, .20)
                .AddServiceTimePossibility(khabbaz, 6, .20);

            var customers = simulator.Take(CustomerCount).ToList();

            dataGrid.ItemsSource = customers; 

            statisticsListView.ItemsSource = SimulatorStatistics(typeof(MultiServantQueueTools), customers);
        }

        private void NewsstandSimulation()
        {
            var simulator = new NewsstandSimulator(new RandomMantissa(), new RandomMantissa(),
                70, 13, 20, 2);

            simulator
                .AddDayTypePossibility(DayType.Good, .35)
                .AddDayTypePossibility(DayType.Medium, .45)
                .AddDayTypePossibility(DayType.Bad, .20)

                .AddRequestPossibilityOnDayType(DayType.Good, 40, .03)
                .AddRequestPossibilityOnDayType(DayType.Good, 50, .05)
                .AddRequestPossibilityOnDayType(DayType.Good, 60, .15)
                .AddRequestPossibilityOnDayType(DayType.Good, 70, .20)
                .AddRequestPossibilityOnDayType(DayType.Good, 80, .35)
                .AddRequestPossibilityOnDayType(DayType.Good, 90, .15)
                .AddRequestPossibilityOnDayType(DayType.Good, 100, .07)

                .AddRequestPossibilityOnDayType(DayType.Medium, 40, .10)
                .AddRequestPossibilityOnDayType(DayType.Medium, 50, .18)
                .AddRequestPossibilityOnDayType(DayType.Medium, 60, .40)
                .AddRequestPossibilityOnDayType(DayType.Medium, 70, .20)
                .AddRequestPossibilityOnDayType(DayType.Medium, 80, .08)
                .AddRequestPossibilityOnDayType(DayType.Medium, 90, .04)
                .AddRequestPossibilityOnDayType(DayType.Medium, 100, .00)

                .AddRequestPossibilityOnDayType(DayType.Bad, 40, .44)
                .AddRequestPossibilityOnDayType(DayType.Bad, 50, .22)
                .AddRequestPossibilityOnDayType(DayType.Bad, 60, .16)
                .AddRequestPossibilityOnDayType(DayType.Bad, 70, .12)
                .AddRequestPossibilityOnDayType(DayType.Bad, 80, .06)
                .AddRequestPossibilityOnDayType(DayType.Bad, 90, .00)
                .AddRequestPossibilityOnDayType(DayType.Bad, 100, .00);

            var states = simulator.Take(CustomerCount).ToList();

            dataGrid.ItemsSource = states;

            statisticsListView.ItemsSource = SimulatorStatistics(typeof(NewsstandWarehouseTools), states);
        }

        private void SupplymentSimulation()
        {
            var simulator = new SupplymentSimulator(new RandomMantissa(), new RandomMantissa(), 11, 5, 3, 8, 2);

            simulator
                .AddDailyRequestPossibility(0, .10)
                .AddDailyRequestPossibility(1, .25)
                .AddDailyRequestPossibility(2, .35)
                .AddDailyRequestPossibility(3, .21)
                .AddDailyRequestPossibility(4, .09)

                .AddDeliveryTimePossibility(1, .6)
                .AddDeliveryTimePossibility(2, .3)
                .AddDeliveryTimePossibility(3, .1);

            var states = simulator.Take(CustomerCount).ToList();

            dataGrid.ItemsSource = states;

            statisticsListView.ItemsSource = SimulatorStatistics(typeof(SupplymentStatesTools), states);
        }

        private void NewsstandOnDifferentDays()
        {
            var testLength = CustomerCount;
            var list = new List<AnnotatedPair<double, double>>();

            var dayTypeRandomNumbers = new RandomMantissa().Take(testLength).ToList();
            var requestsRandomNumbers = new RandomMantissa().Take(testLength).ToList();

            Enumerable.Range(1, 12).AsParallel().ForAll(i =>
            {
                var simulator = new NewsstandSimulator(dayTypeRandomNumbers, requestsRandomNumbers,
                    i * 10, 13, 20, 2);

                simulator
                    .AddDayTypePossibility(DayType.Good, .35)
                    .AddDayTypePossibility(DayType.Medium, .45)
                    .AddDayTypePossibility(DayType.Bad, .20)

                    .AddRequestPossibilityOnDayType(DayType.Good, 40, .03)
                    .AddRequestPossibilityOnDayType(DayType.Good, 50, .05)
                    .AddRequestPossibilityOnDayType(DayType.Good, 60, .15)
                    .AddRequestPossibilityOnDayType(DayType.Good, 70, .20)
                    .AddRequestPossibilityOnDayType(DayType.Good, 80, .35)
                    .AddRequestPossibilityOnDayType(DayType.Good, 90, .15)
                    .AddRequestPossibilityOnDayType(DayType.Good, 100, .07)

                    .AddRequestPossibilityOnDayType(DayType.Medium, 40, .10)
                    .AddRequestPossibilityOnDayType(DayType.Medium, 50, .18)
                    .AddRequestPossibilityOnDayType(DayType.Medium, 60, .40)
                    .AddRequestPossibilityOnDayType(DayType.Medium, 70, .20)
                    .AddRequestPossibilityOnDayType(DayType.Medium, 80, .08)
                    .AddRequestPossibilityOnDayType(DayType.Medium, 90, .04)
                    .AddRequestPossibilityOnDayType(DayType.Medium, 100, .00)

                    .AddRequestPossibilityOnDayType(DayType.Bad, 40, .44)
                    .AddRequestPossibilityOnDayType(DayType.Bad, 50, .22)
                    .AddRequestPossibilityOnDayType(DayType.Bad, 60, .16)
                    .AddRequestPossibilityOnDayType(DayType.Bad, 70, .12)
                    .AddRequestPossibilityOnDayType(DayType.Bad, 80, .06)
                    .AddRequestPossibilityOnDayType(DayType.Bad, 90, .00)
                    .AddRequestPossibilityOnDayType(DayType.Bad, 100, .00);

                var states = simulator.Take(testLength).ToList();

                lock (list)
                {
                    list.Add(new AnnotatedPair<double, double>
                        {
                            Item1 = i * 10,
                            Item2 = states.TotalProfit()
                        });
                }
            });

            dataGrid.ItemsSource = list;

            var result = new Tuple<string, double>("بهترین حالت در شبیه‌سازی", list.First(x => x.Item2 == list.Max(y => y.Item2)).Item1);
            statisticsListView.ItemsSource = new[] { result };
        }
        #endregion

        private List<Tuple<string, object>> SimulatorStatistics(Type T, object customers)
        {
            var result = new List<Tuple<string, object>>();
            foreach (var method in T.GetMethods())
            {
                var attribute = method
                    .GetCustomAttributes(typeof(DisplayNameAttribute), true)
                    .Cast<DisplayNameAttribute>()
                    .SingleOrDefault();

                if (attribute != null)
                {
                    result.Add(new Tuple<string, object>(
                       attribute.DisplayName,
                       method.Invoke(null, new[] { (object)customers })));
                }
            }
            return result;
        }

        public static string GetPropertyDisplayName(object descriptor)
        {
            var pd = descriptor as System.ComponentModel.PropertyDescriptor;
            if (pd != null)
            {
                // Check for DisplayName attribute and set the column header accordingly
                var displayName = pd.Attributes[typeof(DisplayNameAttribute)] as DisplayNameAttribute;
                if (displayName != null && displayName != DisplayNameAttribute.Default)
                {
                    return displayName.DisplayName;
                }

            }
            else
            {
                var pi = descriptor as PropertyInfo;
                if (pi != null)
                {
                    // Check for DisplayName attribute and set the column header accordingly
                    Object[] attributes = pi.GetCustomAttributes(typeof(DisplayNameAttribute), true);
                    for (int i = 0; i < attributes.Length; ++i)
                    {
                        DisplayNameAttribute displayName = attributes[i] as DisplayNameAttribute;
                        if (displayName != null && displayName != DisplayNameAttribute.Default)
                        {
                            return displayName.DisplayName;
                        }
                    }
                }
            }
            return null;
        }

        private void dgAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string displayName = GetPropertyDisplayName(e.PropertyDescriptor);
            if (!string.IsNullOrEmpty(displayName))
            {
                e.Column.Header = displayName;
            }
        }
    }


    public class AnnotatedPair<T1, T2>
    {
        [DisplayName("تعداد روزنامه‌ها")]
        public T1 Item1 { get; set; }

        [DisplayName("سود کل")]
        public T2 Item2 { get; set; }
    }
}
