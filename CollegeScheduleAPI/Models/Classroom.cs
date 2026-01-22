using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollegeSchedule.Models
{
    [Table("classroom")] //таблица "classroom"
    public class Classroom
    {
        [Key]
        [Column("classroom_id")] //первичный ключ
        public int ClassroomId { get; set; }

        [Column("building_id")] //внешний ключ к зданию
        public int BuildingId { get; set; }

        [ForeignKey("BuildingId")] //связь с Building
        public Building Building { get; set; } = null!;

        [Column("room_number")]
        [Required]
        public string RoomNumber { get; set; } = null!;
    }
}