using System.ComponentModel.DataAnnotations;

namespace WebStore.UI.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required, Display(Name="Category Name")]
        public string Name { get; set; }
    }
}
