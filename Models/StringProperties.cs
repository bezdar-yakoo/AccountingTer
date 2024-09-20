using System.ComponentModel.DataAnnotations;

namespace AccountingTer.Models
{
    public class StringProperties
    {
        [Key]
        public int Id { get; set; }
        public string  Key { get; set; }
        public string Value { get; set; }

        public string Description { get; set; }
    }
}
