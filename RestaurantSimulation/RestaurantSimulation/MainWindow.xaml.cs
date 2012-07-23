using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace RestaurantSimulation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<String> collection = new ObservableCollection<string>();
        public MainWindow()
        {
            InitializeComponent();
            listBox.ItemsSource = collection;

            var rs = new RestaurantSimulator(
                new ItemPicker<int>(new RandomMantissa()),
                new ItemPicker<int>(new RandomMantissa()));

            Enumerable.Range(1, 8).ToList().ForEach(x =>
                rs.AddEnteringDifferencePossibility(x, .125));

            var servicePossibilties = new[] { .10, .20, .30, .25, .10, .05 };
            Enumerable.Range(1, servicePossibilties.Length)
                .Zip(servicePossibilties, (x, y) => new { x, y })
                .ToList()
                .ForEach(x => rs.AddServiceTimePossibility(x.x, x.y));

            rs.Take(10).ToList().ForEach(x =>
                 {
                     var str = String.Format("Needed service time: {0}\nWait time: {1}", x.ServiceDuration, x.WaitingTime);
                     
                     collection.Add(str);
                 });
        }
    }
}
