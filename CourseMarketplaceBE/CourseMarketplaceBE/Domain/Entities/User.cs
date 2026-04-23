using System;
using System.Collections.Generic;

namespace CourseMarketplaceBE.Domain.Entities;

    public partial class User
    {
        public int UserId { get; set; }

        public string FullName { get; set; } = null!;

        public string? Bio { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        public decimal? TotalSpent { get; set; }

        public int? EnrolledCoursesCount { get; set; }

        public virtual ICollection<AiActivityLog> AiActivityLogs { get; set; } = new List<AiActivityLog>();

        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

        public virtual Instructor? Instructor { get; set; }

        public virtual ICollection<OrderInfo> OrderInfos { get; set; } = new List<OrderInfo>();

        public virtual Account UserNavigation { get; set; } = null!;

        public virtual ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
    }
