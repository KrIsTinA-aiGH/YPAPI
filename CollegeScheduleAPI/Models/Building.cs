using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollegeSchedule.Models
{
    [Table("building")] //соответствует таблице "building" в БД
    public class Building
    {
        [Key]
        [Column("building_id")] //первичный ключ
        public int BuildingId { get; set; }

        [Column("name")]
        [Required] //обязательное поле
        public string Name { get; set; } = null!;

        [Column("address")]
        [Required]
        public string Address { get; set; } = null!;

        //навигационное свойство для связанных аудиторий
        public List<Classroom> Classrooms { get; set; } = new();
    }
}