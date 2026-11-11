using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MusicStore.Models
{
    public partial class Product
    {
        public Product()
        {
            CartItems = new HashSet<CartItem>();
            OrderDetails = new HashSet<OrderDetail>();
        }

        [Key]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(200, MinimumLength = 2,
            ErrorMessage = "Product name must be between 2 and 200 characters.")]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Brand name is required.")]
        [StringLength(100,
            ErrorMessage = "Brand name cannot exceed 100 characters.")]
        [Display(Name = "Brand")]
        public string BrandName { get; set; }

        [Required(ErrorMessage = "Product description is required.")]
        [StringLength(1000,
            ErrorMessage = "Description cannot exceed 1000 characters.")]
        [Display(Name = "Description")]
        public string ProductDescription { get; set; }

        [Required(ErrorMessage = "Unit price is required.")]
        [Range(0.01, 100000000,
            ErrorMessage = "Unit price must be greater than 0.")]
        [Display(Name = "Price")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "Stock quantity is required.")]
        [Range(0, 100000,
            ErrorMessage = "Stock quantity must be between 0 and 100000.")]
        [Display(Name = "Stock Quantity")]
        public int? StockQuantity { get; set; }

        [StringLength(500,
            ErrorMessage = "Image URL cannot exceed 500 characters.")]
        [Display(Name = "Image")]
        public string ImageUrl { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        public virtual ICollection<CartItem> CartItems { get; set; }

        public virtual Category Category { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}