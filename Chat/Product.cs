
namespace Chat
{
    public enum ProductCategory
    {
        DairyProducts,
        MeetProducts,
        Fruits,
        Vegetables,
        Bread,
        Beverages,
        Sweets
    }


    public class Product
    {
        public string Name { get; set; }
        public string ImgPath { get; set; }
        public string Measure { get; set; }
        public int Unit { get; set; }
        public int Price { get; set; }
        public string Description { get; set; }
        public ProductCategory Category { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            Product tmp = obj as Product;
            if (tmp == null)
            {
                return false;
            }
            else
            {
                return this.Name == tmp.Name &&
                    this.ImgPath == tmp.ImgPath &&
                    this.Measure == tmp.Measure && 
                    this.Unit == tmp.Unit && 
                    this.Price == tmp.Price && 
                    this.Description == tmp.Description && 
                    this.Category == tmp.Category;
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class ProductItem : NotifyPropertyChangedObject
    {
        public Product ItemName { get; set; }
        private int itemCount;
        public int ItemCount
        {
            get
            {
                return itemCount;
            }
            set
            {
                if (value > 0)
                {
                    itemCount = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsPurchased { get; set; } = false;
    }
}
