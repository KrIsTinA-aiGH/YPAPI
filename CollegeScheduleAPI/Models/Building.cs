using CollegeScheduleAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollegeSchedule.Models
{
    // Атрибут [Table] указывает имя таблицы в БД
    [Table("building")]
    public class Building
    {
        [Key] // Показывает, что это первичный ключ
        [Column("building_id")] // Соответствует столбцу в таблице
        public int BuildingId { get; set; }

        [Column("name")]
        [Required] // NOT NULL в БД
        public string Name { get; set; } = null!;

        [Column("address")]
        [Required]
        public string Address { get; set; } = null!;

        // Навигационное свойство для связи 1-ко-многим с Classroom
        public List<Classroom> Classrooms { get; set; } = new();
    }
}