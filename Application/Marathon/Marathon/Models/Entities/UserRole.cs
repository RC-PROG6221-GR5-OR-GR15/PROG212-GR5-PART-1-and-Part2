using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marathon.Models
{
    public class UserRole
    {
        [Key]
        public Guid userrole_id { get; set; }
        [ForeignKey("User")]
        public required Guid UserId { get; set; }
        [ForeignKey("Role")]
        public required Guid RoleId  { get; set; }
        public required DateTime assigned_at { get; set; }
        public required string  assinged_by { get; set; }
    }
}
