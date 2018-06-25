using System.Windows;

namespace Chat
{
    public partial class ItemProductWindow : Window
    {
        public Product Prod { get; set; }
        public ItemProductWindow(Product cat)
        {
            Prod = cat;
            InitializeComponent();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
