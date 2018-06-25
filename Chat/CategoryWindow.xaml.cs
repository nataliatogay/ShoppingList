using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace Chat
{
    public partial class CategoryWindow : Window//, INotifyPropertyChanged
    {
        public ObservableCollection<Product> Products { get; set; } = new ObservableCollection<Product>();
        public ObservableCollection<Product> SelectedCategory { get; set; } = new ObservableCollection<Product>();
        public Product SelectedProduct;
        // public event PropertyChangedEventHandler PropertyChanged;


        public CategoryWindow(ObservableCollection<Product> prodItems, Product prod)
        {
            InitializeComponent();
            Products = prodItems;
            SelectedProduct = prod;
        }



        private void productListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }



        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            // через поиск 

            // Button button = sender as Button;
            // //walk up the tree to find the ListboxItem
            // DependencyObject tvi = findParentTreeItem(button, typeof(ListBoxItem));
            // //if not null cast the Dependancy object to type of Listbox item.
            // if (tvi != null)
            // {
            //     ListBoxItem lbi = tvi as ListBoxItem;
            //     //select it.
            //     // lbi.IsSelected = true;
            //     Product tmp = lbi.DataContext as Product;
            //     SelectedProduct.Category = tmp.Category;
            //     SelectedProduct.Description = tmp.Description;
            //     SelectedProduct.ImgPath = tmp.ImgPath;
            //     SelectedProduct.Measure = tmp.Measure;
            //     SelectedProduct.Name = tmp.Name;
            //     SelectedProduct.Price = tmp.Price;
            //     SelectedProduct.Unit = tmp.Unit;
            //     DialogResult = true;
            //     this.Close();
            // }

            Button button = sender as Button;

            Product tmp = button.Tag as Product;
            if (tmp != null)
            {
                SelectedProduct.Category = tmp.Category;
                SelectedProduct.Description = tmp.Description;
                SelectedProduct.ImgPath = tmp.ImgPath;
                SelectedProduct.Measure = tmp.Measure;
                SelectedProduct.Name = tmp.Name;
                SelectedProduct.Price = tmp.Price;
                SelectedProduct.Unit = tmp.Unit;
                DialogResult = true;
                this.Close();
            }
        }

        // private DependencyObject findParentTreeItem(DependencyObject CurrentControl, Type ParentType)
        // {
        //     bool notfound = true;
        //     while (notfound)
        //     {
        //         DependencyObject parent = VisualTreeHelper.GetParent(CurrentControl);
        //         string ParentTypeName = ParentType.Name;
        //         //Compare current type name with what we want
        //         if (parent == null)
        //         {
        //             System.Diagnostics.Debugger.Break();
        //             notfound = false;
        //             continue;
        //         }
        //         if (parent.GetType().Name == ParentTypeName)
        //         {
        //             return parent;
        //         }
        //         //we haven't found it so walk up the tree.
        //         CurrentControl = parent;
        //     }
        //     return null;
        // }

        private void categoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (categoryListBox.SelectedItem is ProductCategory)
            {
                ProductCategory pc = (ProductCategory)categoryListBox.SelectedItem;
                SelectedCategory.Clear();
                foreach (var item in Products)
                {
                    if (item.Category == pc)
                    {
                        SelectedCategory.Add(item);
                    }
                }
                if (!newProductBtn.IsEnabled)
                {
                    newProductBtn.IsEnabled = true;
                }
                editProductBtn.IsEnabled = false;
            }
        }

        private void NewCategoryButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NewProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (categoryListBox.SelectedItem is null)
            {
                return;
            }
            if (categoryListBox.SelectedItem is ProductCategory)
            {
                Product prod = new Product()
                {
                    Category = (ProductCategory)categoryListBox.SelectedItem
                };
                var itemWindow = new ItemProductWindow(prod);

                var result = itemWindow.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    foreach (var item in SelectedCategory)
                    {
                        if (item.Equals(itemWindow.Prod))
                        {
                            return;
                        }
                    }
                    Products.Add(itemWindow.Prod);
                    SelectedCategory.Add(itemWindow.Prod);
                }
            }
        }

        private void EditProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (categoryListBox.SelectedItem is null || productListBox.SelectedItem is null)
            {
                return;
            }
            if (categoryListBox.SelectedItem is ProductCategory)
            {
                ProductCategory pc = (ProductCategory)categoryListBox.SelectedItem;
                Product selectedProduct = productListBox.SelectedItem as Product;
                if (selectedProduct != null)
                {
                    Product tmp = new Product()
                    {
                        Name = selectedProduct.Name,
                        ImgPath = selectedProduct.ImgPath,
                        Measure = selectedProduct.Measure,
                        Unit = selectedProduct.Unit,
                        Price = selectedProduct.Price,
                        Description = selectedProduct.Description,
                        Category = selectedProduct.Category
                    };
                    var itemWindow = new ItemProductWindow(tmp);
                    var result = itemWindow.ShowDialog();
                    if (result.HasValue && result.Value)
                    {
                        foreach (var item in Products)
                        {
                            if (item == selectedProduct)
                            {
                                item.Name = tmp.Name;
                                item.ImgPath = tmp.ImgPath;
                                item.Measure = tmp.Measure;
                                item.Unit = tmp.Unit;
                                item.Price = tmp.Price;
                                item.Description = tmp.Description;
                                item.Category = tmp.Category;
                                break;
                            }
                        }

                        SelectedCategory.Clear();
                        foreach (var item in Products)
                        {
                            if (item.Category == pc)
                            {
                                SelectedCategory.Add(item);
                            }
                        }
                    }
                }
            }
        }

        private void productListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (editProductBtn.IsEnabled == false)
            {
                editProductBtn.IsEnabled = true;
            }
        }
    }
}
