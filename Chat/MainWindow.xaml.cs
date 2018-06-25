using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Serialization;

namespace Chat
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<Product> Items { get; set; } = new ObservableCollection<Product>();
        public ObservableCollection<ProductItem> ItemsInList { get; set; } = new ObservableCollection<ProductItem>();
        public ObservableCollection<ProductItem> ItemsPurchased { get; set; } = new ObservableCollection<ProductItem>();

        public string OpenedFileName { get; set; } 
        bool fileSaved;

        public event PropertyChangedEventHandler PropertyChanged;

        private int totalPrice { get; set; }
        public int TotalPrice
        {
            get
            {
                return totalPrice;
            }
            set
            {
                totalPrice = value;
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs("TotalPrice"));
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            fileSaved = true;
            TotalPrice = 0;
            Items = DeserializationProduct<Product>("..\\..\\productlist.xml");
            //AddProductsinList();
        }

        private void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            XmlSerializer formatter = new XmlSerializer(typeof(ObservableCollection<Product>));
            try
            {
                using (FileStream fs = new FileStream("..\\..\\productlist.xml", FileMode.OpenOrCreate))
                {
                    fs.Seek(0, SeekOrigin.Begin);
                    fs.SetLength(0);
                    formatter.Serialize(fs, Items);
                }
            }
            catch (Exception)
            {
                return;
            }
            if (fileSaved)
            {
                e.Cancel = false;
                return;
            }

            bool res = BeforeClosing();
            if (res)
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void newItemButton_Click(object sender, RoutedEventArgs e)
        {
            Product prod = new Product();
            CategoryWindow cw = new CategoryWindow(Items, prod);
            bool? result = cw.ShowDialog();
            if (result.HasValue && result.Value)
            {
                foreach (var item in ItemsInList)
                {
                    if (item.ItemName.Equals(prod))
                    {
                        item.ItemCount += prod.Unit;
                        TotalPrice += prod.Price;
                        return;
                    }
                }
                foreach (var item in ItemsPurchased)
                {
                    if (item.ItemName.Equals(prod))
                    {
                        ItemsPurchased.Remove(item);
                        break;
                    }
                }
                ItemsInList.Add(new ProductItem() { ItemName = prod, ItemCount = prod.Unit });

                TotalPrice += prod.Price;
                fileSaved = false;
            }

        }

        private void listBoxDeleted_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProductItem returned = (sender as ListBox).SelectedItem as ProductItem;
            if (returned != null)
            {
                ItemsInList.Add(returned);
                ItemsPurchased.Remove(returned);
                returned.IsPurchased = false;
                fileSaved = false;
            }
        }

        private void MinusButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            DependencyObject tvi = findParentTreeItem(button, typeof(ListBoxItem));
            if (tvi != null)
            {

                ListBoxItem lbi = tvi as ListBoxItem;
                ProductItem prItem = lbi.DataContext as ProductItem;
                foreach (var item in ItemsInList)
                {
                    if (item == prItem)
                    {
                        if (item.ItemCount > 1)
                        {
                            TotalPrice -= item.ItemName.Price;
                        }
                        item.ItemCount -= item.ItemName.Unit;
                        
                        fileSaved = false;
                        return;
                    }
                }

            }
        }

        private DependencyObject findParentTreeItem(DependencyObject CurrentControl, Type ParentType)
        {
            bool notfound = true;
            while (notfound)
            {
                DependencyObject parent = VisualTreeHelper.GetParent(CurrentControl);
                string ParentTypeName = ParentType.Name;
                if (parent == null)
                {
                    System.Diagnostics.Debugger.Break();
                    notfound = false;
                    continue;
                }
                if (parent.GetType().Name == ParentTypeName)
                {
                    return parent;
                }
                CurrentControl = parent;
            }
            return null;
        }

        private void PlusButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            DependencyObject tvi = findParentTreeItem(button, typeof(ListBoxItem));
            if (tvi != null)
            {

                ListBoxItem lbi = tvi as ListBoxItem;
                ProductItem prItem = lbi.DataContext as ProductItem;
                foreach (var item in ItemsInList)
                {
                    if (item == prItem)
                    {
                        item.ItemCount += item.ItemName.Unit;
                        TotalPrice += item.ItemName.Price;
                        fileSaved = false;
                        return;
                    }
                }
            }
        }

        private void PurchasedButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            DependencyObject tvi = findParentTreeItem(button, typeof(ListBoxItem));
            if (tvi != null)
            {
                ListBoxItem lbi = tvi as ListBoxItem;
                ProductItem purchased = lbi.DataContext as ProductItem;
                if (purchased != null)
                {
                    ItemsPurchased.Add(purchased);
                    ItemsInList.Remove(purchased);
                    purchased.IsPurchased = true;
                    fileSaved = false;
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            DependencyObject tvi = findParentTreeItem(button, typeof(ListBoxItem));
            if (tvi != null)
            {
                ListBoxItem lbi = tvi as ListBoxItem;
                ProductItem deleted = lbi.DataContext as ProductItem;
                if (deleted != null)
                {
                    ItemsInList.Remove(deleted);
                    TotalPrice -= deleted.ItemName.Price * deleted.ItemCount;
                    fileSaved = false;
                }
            }
        }

        private void NewListMenuItem_Click(object sender, RoutedEventArgs e)
        {
            bool newFile = true;
            if (!fileSaved)
            {
                newFile = BeforeClosing();
            }
            if (newFile)
            {
                ItemsInList.Clear();
                ItemsPurchased.Clear();
                fileSaved = false;
                TotalPrice = 0;
                OpenedFileName = null;
            }
        }

        private bool BeforeClosing()
        {
            string msg = "Do you want to save changes to ";
            if (string.IsNullOrEmpty(OpenedFileName))
            {
                msg = $"{msg} Untitled?";
            }
            else
            {
                msg = $"{msg} {OpenedFileName} ?";
            }
            var result = MessageBox.Show(msg, "ShoppingList", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.Cancel)
            {
                return false;
            }
            else if (result == MessageBoxResult.Yes)
            {
                if (string.IsNullOrEmpty(OpenedFileName))
                {
                    if (FileSaveAs())
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    Serialization(OpenedFileName);
                }
            }
            return true;
        }

        private bool FileSaveAs()
        {
            string formatFilter = "Xml Documents(*.xml)|*.xml";
            SaveFileDialog saveDialog = new SaveFileDialog()
            {
                Filter = formatFilter
            };
            if (saveDialog.ShowDialog() == true)
            {
                if (string.IsNullOrEmpty(saveDialog.FileName))
                {
                    return false;
                }

                Serialization(saveDialog.FileName);
                OpenedFileName = saveDialog.FileName;
                this.Title = $"{OpenedFileName.Substring(OpenedFileName.LastIndexOf('\\') + 1)} - Shopping List";
                fileSaved = true;
                return true;
            }
            return false;
        }

        private void Serialization(string filename)
        {
            ObservableCollection<ProductItem> tmp = new ObservableCollection<ProductItem>();
            foreach (var item in ItemsInList)
            {
                tmp.Add(item);
            }
            foreach (var item in ItemsPurchased)
            {
                tmp.Add(item);
            }
            XmlSerializer formatter = new XmlSerializer(typeof(ObservableCollection<ProductItem>));
            try
            {
                using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
                {
                    fs.Seek(0, SeekOrigin.Begin);
                    fs.SetLength(0);
                    formatter.Serialize(fs, tmp);
                }
            }
            catch (SerializationException)
            {
                MessageBox.Show("Error", "Couldn't save the file", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Error", "File not found", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private ObservableCollection<T> DeserializationProduct<T>(string filename)
        {
            ObservableCollection<T> tmp = new ObservableCollection<T>();
            XmlSerializer formatter = new XmlSerializer(typeof(ObservableCollection<T>));
            try
            {
                using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
                {
                    tmp = (ObservableCollection<T>)formatter.Deserialize(fs);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return tmp;
            }

            using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
            {
                tmp = (ObservableCollection<T>)formatter.Deserialize(fs);
            }

            return tmp;
        }

        private void OpenListMenuItem_Click(object sender, RoutedEventArgs e)
        {
            bool newFile = true;
            if (!fileSaved)
            {
                newFile = BeforeClosing();
            }
            if (newFile)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog() { Filter = "Xml Documents(*.xml)|*.xml" };

                if (openFileDialog.ShowDialog() == true)
                {
                    if (string.IsNullOrEmpty(openFileDialog.FileName))
                    {
                        return;
                    }
                    ObservableCollection<ProductItem> tmp = DeserializationProduct<ProductItem>(openFileDialog.FileName);
                    ItemsInList.Clear();
                    ItemsPurchased.Clear();
                    TotalPrice = 0;
                    foreach (var item in tmp)
                    {
                        if (item.IsPurchased)
                        {
                            ItemsPurchased.Add(item);
                        }
                        else
                        {
                            ItemsInList.Add(item);
                        }
                        TotalPrice += item.ItemName.Price * item.ItemCount;
                    }
                    fileSaved = true;
                    OpenedFileName = openFileDialog.FileName;
                }
            }
        }

        private void SaveListMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(OpenedFileName))
            {
                FileSaveAs();
            }
            else
            {
                Serialization(OpenedFileName);
            }
        }


        private void AddProductsinList()
        {
            Items.Add(new Product()
            {
                Name = "cherry",
                ImgPath = "icons\\cherry.png",
                Measure = "g",
                Unit = 100,
                Price = 13,
                Description = "cherry",
                Category = ProductCategory.Fruits
            });
            Items.Add(new Product()
            {
                Name = "carrot",
                ImgPath = "icons\\carrot.png",
                Measure = "kg",
                Unit = 1,
                Price = 21,
                Description = "carrot",
                Category = ProductCategory.Fruits
            });
            Items.Add(new Product()
            {
                Name = "apple",
                ImgPath = "icons\\apple.png",
                Measure = "kg",
                Unit = 1,
                Price = 30,
                Description = "apple",
                Category = ProductCategory.Fruits
            });
            Items.Add(new Product()
            {
                Name = "banana",
                ImgPath = "icons\\banana.png",
                Measure = "kg",
                Unit = 1,
                Price = 45,
                Description = "banana",
                Category = ProductCategory.Fruits
            });

            Items.Add(new Product()
            {
                Name = "fanta",
                ImgPath = "icons\\fanta.png",
                Measure = "bottle",
                Unit = 1,
                Price = 23,
                Description = "a bottle of fanta",
                Category = ProductCategory.Beverages
            });

            Items.Add(new Product()
            {
                Name = "water",
                ImgPath = "icons\\water.png",
                Measure = "bottle",
                Unit = 1,
                Price = 6,
                Description = "a bottle of still water",
                Category = ProductCategory.Beverages
            });

            Items.Add(new Product()
            {
                Name = "milk",
                ImgPath = "icons\\milk.png",
                Measure = "bottle",
                Unit = 1,
                Price = 13,
                Description = "a bottle of milk",
                Category = ProductCategory.Beverages
            });
            Items.Add(new Product()
            {
                Name = "white bread",
                ImgPath = "icons\\whitebread.png",
                Measure = "loaf",
                Unit = 1,
                Price = 12,
                Description = "a loaf of white bread",
                Category = ProductCategory.Bread
            });

            Items.Add(new Product()
            {
                Name = "cheese",
                ImgPath = "icons\\cheese.png",
                Measure = "gr",
                Unit = 100,
                Price = 78,
                Description = "parmesan",
                Category = ProductCategory.DairyProducts
            });
            Items.Add(new Product()
            {
                Name = "cookies",
                ImgPath = "icons\\cookies.png",
                Measure = "gr",
                Unit = 100,
                Price = 125,
                Description = "funny cookies",
                Category = ProductCategory.Sweets
            });
            Items.Add(new Product()
            {
                Name = "candy",
                ImgPath = "icons\\candy.png",
                Measure = "piece",
                Unit = 1,
                Price = 96,
                Description = "candy",
                Category = ProductCategory.Sweets
            });
            Items.Add(new Product()
            {
                Name = "chocolate",
                ImgPath = "icons\\chocolate.png",
                Measure = "piece",
                Unit = 1,
                Price = 154,
                Description = "chocolate",
                Category = ProductCategory.Sweets
            });
            Items.Add(new Product()
            {
                Name = "chocolate cookie",
                ImgPath = "icons\\cookie_chocolate.png",
                Measure = "gr",
                Unit = 100,
                Price = 138,
                Description = "cookie with chocolate",
                Category = ProductCategory.Sweets
            });
            Items.Add(new Product()
            {
                Name = "cake",
                ImgPath = "icons\\cake.png",
                Measure = "piece",
                Unit = 1,
                Price = 203,
                Description = "chocolate cake",
                Category = ProductCategory.Sweets
            });

            Items.Add(new Product()
            {
                Name = "popcorn",
                ImgPath = "icons\\popcorn.png",
                Measure = "piece",
                Unit = 1,
                Price = 52,
                Description = "popcorn sweet",
                Category = ProductCategory.Sweets
            });

            Items.Add(new Product()
            {
                Name = "croissant",
                ImgPath = "icons\\croissant.png",
                Measure = "piece",
                Unit = 1,
                Price = 50,
                Description = "croissant",
                Category = ProductCategory.Bread
            });

            Items.Add(new Product()
            {
                Name = "bread",
                ImgPath = "icons\\bread.png",
                Measure = "loaf",
                Unit = 1,
                Price = 18,
                Description = "bread",
                Category = ProductCategory.Bread
            });

            Items.Add(new Product()
            {
                Name = "egg",
                ImgPath = "icons\\egg.png",
                Measure = "piece",
                Unit = 1,
                Price = 9,
                Description = "egg",
                Category = ProductCategory.DairyProducts
            });

            Items.Add(new Product()
            {
                Name = "fish",
                ImgPath = "icons\\fish.png",
                Measure = "kg",
                Unit = 1,
                Price = 198,
                Description = "fish",
                Category = ProductCategory.MeetProducts
            });

            Items.Add(new Product()
            {
                Name = "crab",
                ImgPath = "icons\\crab.png",
                Measure = "kg",
                Unit = 1,
                Price = 275,
                Description = "crab",
                Category = ProductCategory.MeetProducts
            });

            Items.Add(new Product()
            {
                Name = "beef",
                ImgPath = "icons\\beef.png",
                Measure = "kg",
                Unit = 1,
                Price = 206,
                Description = "beef",
                Category = ProductCategory.MeetProducts
            });

            Items.Add(new Product()
            {
                Name = "chicken",
                ImgPath = "icons\\chicken.png",
                Measure = "kg",
                Unit = 1,
                Price = 195,
                Description = "chicken",
                Category = ProductCategory.MeetProducts
            });

            Items.Add(new Product()
            {
                Name = "orange juice",
                ImgPath = "icons\\juice.png",
                Measure = "bottle",
                Unit = 1,
                Price = 39,
                Description = "a bottle of orange juice",
                Category = ProductCategory.Beverages
            });

            Items.Add(new Product()
            {
                Name = "lemon",
                ImgPath = "icons\\lemon.png",
                Measure = "kg",
                Unit = 1,
                Price = 27,
                Description = "lemon",
                Category = ProductCategory.Fruits
            });

            Items.Add(new Product()
            {
                Name = "grapes",
                ImgPath = "icons\\grapes.png",
                Measure = "kg",
                Unit = 1,
                Price = 92,
                Description = "grapes",
                Category = ProductCategory.Fruits
            });

            Items.Add(new Product()
            {
                Name = "garlic",
                ImgPath = "icons\\garlic.png",
                Measure = "gr",
                Unit = 100,
                Price = 21,
                Description = "garlic",
                Category = ProductCategory.Vegetables
            });

            Items.Add(new Product()
            {
                Name = "avocado",
                ImgPath = "icons\\avocado.png",
                Measure = "piece",
                Unit = 1,
                Price = 102,
                Description = "avocado",
                Category = ProductCategory.Vegetables
            });

            Items.Add(new Product()
            {
                Name = "pumpkin",
                ImgPath = "icons\\pumpkin.png",
                Measure = "piece",
                Unit = 1,
                Price = 36,
                Description = "pumpkin",
                Category = ProductCategory.Vegetables
            });

            Items.Add(new Product()
            {
                Name = "onion",
                ImgPath = "icons\\onion.png",
                Measure = "kg",
                Unit = 1,
                Price = 21,
                Description = "onion",
                Category = ProductCategory.Vegetables
            });

            Items.Add(new Product()
            {
                Name = "orange",
                ImgPath = "icons\\orange.png",
                Measure = "kg",
                Unit = 1,
                Price = 37,
                Description = "orange",
                Category = ProductCategory.Fruits
            });

            Items.Add(new Product()
            {
                Name = "watermelon",
                ImgPath = "icons\\watermellon.png",
                Measure = "piece",
                Unit = 1,
                Price = 83,
                Description = "watermelon",
                Category = ProductCategory.Fruits
            });

            Items.Add(new Product()
            {
                Name = "pear",
                ImgPath = "icons\\pear.png",
                Measure = "kg",
                Unit = 1,
                Price = 58,
                Description = "pear",
                Category = ProductCategory.Fruits
            });
        }


    }
}
