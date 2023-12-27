using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HimProg
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window 
    {

        private EventDelegate eventDelegate;
        public MainWindow()
        {
            InitializeComponent();
            eventDelegate = new EventDelegate();
            this.DataContext = new MainViewModel(eventDelegate);
        }

        

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            eventDelegate?.InvokeThis();
        }


       
    }
}