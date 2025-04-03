using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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

namespace ToDoListMaster
{
    public partial class AddCategoryWindow : Window
    {
        private ObservableCollection<Category> _categories;
        private const string CategoriesFilePath = "categories.json";

        public AddCategoryWindow(ObservableCollection<Category> categories)
        {
            InitializeComponent();
            _categories = categories ?? new ObservableCollection<Category>();
            CategoryListBox.ItemsSource = _categories;
        }

        // Back button click to return to MainWindow
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            SaveCategories();
            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        // Add category button click
        private void AddCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            // Открываем окно для ввода названия категории
            var inputWindow = new InputCategoryNameWindow();
            inputWindow.Owner = this; // Устанавливаем владельца для центрирования
            bool? result = inputWindow.ShowDialog();

            if (result == true && !string.IsNullOrWhiteSpace(inputWindow.CategoryName))
            {
                string newCategoryName = inputWindow.CategoryName;

                // Проверяем, существует ли категория с таким названием
                if (_categories.Any(c => c.Name.Equals(newCategoryName, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("Категория с таким названием уже существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Добавляем новую категорию
                int newId = _categories.Any() ? _categories.Max(c => c.Id) + 1 : 1;
                _categories.Add(new Category { Id = newId, Name = newCategoryName });
                SaveCategories();
            }
        }

        // Save categories to JSON
        private void SaveCategories()
        {
            try
            {
                string json = JsonConvert.SerializeObject(_categories, Formatting.Indented);
                File.WriteAllText(CategoriesFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении категорий: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}