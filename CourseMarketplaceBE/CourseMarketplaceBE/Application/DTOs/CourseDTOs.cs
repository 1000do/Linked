using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace CourseMarketplaceBE.Application.DTOs;

public class CourseCreateRequest
{
    public int CategoryId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? CourseThumbnailUrl { get; set; }
    public string? WhatYouWillLearn { get; set; }
    public string? Requirements { get; set; }
    public IFormFile? ThumbnailFile { get; set; }
}

public class CourseUpdateRequest
{
    public int CategoryId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? CourseThumbnailUrl { get; set; }
    public string? WhatYouWillLearn { get; set; }
    public string? Requirements { get; set; }
    public IFormFile? ThumbnailFile { get; set; }
}

public class CourseResponse
{
    public int CourseId { get; set; }
    public int? InstructorId { get; set; }
    public int? CategoryId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? CourseThumbnailUrl { get; set; }
    public string? CourseStatus { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? WhatYouWillLearn { get; set; }
    public string? Requirements { get; set; }
    public string? InstructorName { get; set; }
    public int TotalStudents { get; set; }
    public decimal RatingAverage { get; set; }
    public string? CategoryName { get; set; }
    public string? InstructorAvatarUrl { get; set; }
    public string? InstructorBio { get; set; }
    public string? InstructorProfessionalTitle { get; set; }
    public int InstructorReviewCount { get; set; }
    public int InstructorStudentsCount { get; set; }
    public int InstructorCoursesCount { get; set; }
    public bool IsEnrolled { get; set; }
    public bool IsOwner { get; set; }
    public int TotalReviews { get; set; }
}

public class CourseDetailResponse : CourseResponse
{
    public List<LessonResponse> Lessons { get; set; } = new List<LessonResponse>();
}

public class CategoryResponse
{
    public int CategoryId { get; set; }
    public string CategoriesName { get; set; } = null!;
}
