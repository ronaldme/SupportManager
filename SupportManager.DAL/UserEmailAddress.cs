using System.ComponentModel.DataAnnotations;

namespace SupportManager.DAL
{
    public class UserEmailAddress : Entity
    {
        [Required]
        public virtual string Value { get; set; }
        public virtual bool IsVerified { get; set; }
        public virtual string VerificationToken { get; set; }

        public virtual User User { get; set; }
        public virtual int UserId { get; set; }
    }
}