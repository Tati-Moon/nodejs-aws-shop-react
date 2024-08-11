using System.ComponentModel.DataAnnotations.Schema;

namespace CartService.Domain.Entity
{
    public class Product
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("price")]
        public int Price { get; set; }

        [Column("photo")]
        public string Photo { get; set; }
    }
}
