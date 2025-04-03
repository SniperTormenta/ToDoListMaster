using System.Windows;

namespace ToDoListMaster
{
    public partial class InputCategoryNameWindow : Window
    {
        public string CategoryName { get; private set; }

        public InputCategoryNameWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            CategoryName = CategoryNameTextBox.Text;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}