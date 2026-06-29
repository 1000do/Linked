-- ==============================================================================
-- INSERT COURSE
-- ==============================================================================
INSERT INTO courses (course_id, instructor_id, category_id, title, description, price, course_status, threat_level)
VALUES (100, 1, 2, 'Khóa học C# toàn tập từ cơ bản đến nâng cao', 'Khóa học C# tổng hợp từ playlist YouTube.', 0.00, 'approved', 0)
ON CONFLICT (course_id) DO NOTHING;

-- ==============================================================================
-- INSERT LESSONS AND LEARNING MATERIALS
-- ==============================================================================
INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1000, 100, 'Giới thiệu về Phát triển Web | Hướng dẫn phát triển Web Full Stack 2022', 'Bài giảng: Giới thiệu về Phát triển Web | Hướng dẫn phát triển Web Full Stack 2022', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1000, 1000, 'Giới thiệu về Phát triển Web | Hướng dẫn phát triển Web Full Stack 2022', 'active', 'https://www.youtube.com/watch?v=EceJQ05KTf4', '{"duration": 479}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1001, 100, '2. Cài đặt IDE để viết mã - Khóa học phát triển web đầy đủ', 'Bài giảng: 2. Cài đặt IDE để viết mã - Khóa học phát triển web đầy đủ', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1001, 1001, '2. Cài đặt IDE để viết mã - Khóa học phát triển web đầy đủ', 'active', 'https://www.youtube.com/watch?v=qFQN7ytQ-fM', '{"duration": 337}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1002, 100, '3. Building Your First Website | Learn HTML | Full stack web development Tutorial Course', 'Bài giảng: 3. Building Your First Website | Learn HTML | Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1002, 1002, '3. Building Your First Website | Learn HTML | Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=AHNlZ-35ezM', '{"duration": 768}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1003, 100, '4. Intro to HTML, HEAD, BODY, and HEADER | Full stack web development Tutorial Course 2022', 'Bài giảng: 4. Intro to HTML, HEAD, BODY, and HEADER | Full stack web development Tutorial Course 2022', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1003, 1003, '4. Intro to HTML, HEAD, BODY, and HEADER | Full stack web development Tutorial Course 2022', 'active', 'https://www.youtube.com/watch?v=GN9pTFwY7HM', '{"duration": 633}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1004, 100, '5.  Lists, Paragraphs, and Text Styling - Full stack web development Tutorial Course', 'Bài giảng: 5.  Lists, Paragraphs, and Text Styling - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1004, 1004, '5.  Lists, Paragraphs, and Text Styling - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=e7UHiRgnOo8', '{"duration": 480}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1005, 100, '6.  Displaying Data With Table - Full stack web development Tutorial Course', 'Bài giảng: 6.  Displaying Data With Table - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1005, 1005, '6.  Displaying Data With Table - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=oLzTpHn29nM', '{"duration": 459}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1006, 100, '7. Learn HTML Forms, Img and Iframe Tags - Full stack web development Tutorial Course', 'Bài giảng: 7. Learn HTML Forms, Img and Iframe Tags - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1006, 1006, '7. Learn HTML Forms, Img and Iframe Tags - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=Gbw8h-I3QGk', '{"duration": 742}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1007, 100, '8. What is CSS ? - Full stack web development Tutorial Course', 'Bài giảng: 8. What is CSS ? - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1007, 1007, '8. What is CSS ? - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=dLcK76hfCV4', '{"duration": 203}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1008, 100, '9. How to use Inline, Internal and External CSS - Full stack web development Tutorial Course', 'Bài giảng: 9. How to use Inline, Internal and External CSS - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1008, 1008, '9. How to use Inline, Internal and External CSS - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=WBNtQGLU4Vs', '{"duration": 538}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1009, 100, '10. Element, ID and Class Selectors - Full stack web development Tutorial Course', 'Bài giảng: 10. Element, ID and Class Selectors - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1009, 1009, '10. Element, ID and Class Selectors - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=5IEfRrRMxXc', '{"duration": 637}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1010, 100, '11. CSS Colors and Shadows - Full stack web development Tutorial Course', 'Bài giảng: 11. CSS Colors and Shadows - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1010, 1010, '11. CSS Colors and Shadows - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=EMJYCGaaIUo', '{"duration": 859}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1011, 100, '12. CSS Backgrounds And Borders - Full stack web development Tutorial Course', 'Bài giảng: 12. CSS Backgrounds And Borders - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1011, 1011, '12. CSS Backgrounds And Borders - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=NEYLnGV5v6M', '{"duration": 635}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1012, 100, '13. Using browser inspector tools - Full stack web development Tutorial Course', 'Bài giảng: 13. Using browser inspector tools - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1012, 1012, '13. Using browser inspector tools - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=WJIqIDm7CoA', '{"duration": 812}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1013, 100, '14. Combinators in CSS - Full stack web development Tutorial Course', 'Bài giảng: 14. Combinators in CSS - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1013, 1013, '14. Combinators in CSS - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=BlQo0ZAbELY', '{"duration": 332}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1014, 100, '15. Grouping in CSS - Full stack web development Tutorial Course', 'Bài giảng: 15. Grouping in CSS - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1014, 1014, '15. Grouping in CSS - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=tBybvRttDQo', '{"duration": 333}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1015, 100, '16. Specificity in CSS - Full stack web development Tutorial Course', 'Bài giảng: 16. Specificity in CSS - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1015, 1015, '16. Specificity in CSS - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=jnfrnyK2MGI', '{"duration": 917}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1016, 100, '17. Text Styling And Formatting - Full stack web development Tutorial Course', 'Bài giảng: 17. Text Styling And Formatting - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1016, 1016, '17. Text Styling And Formatting - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=6hCA8vsyyrE', '{"duration": 1215}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1017, 100, '18. Google Fonts API - Full stack web development Tutorial Course', 'Bài giảng: 18. Google Fonts API - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1017, 1017, '18. Google Fonts API - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=0UkBJW8KWhc', '{"duration": 496}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1018, 100, '19. Responsive CSS Images - Full stack web development Tutorial Course', 'Bài giảng: 19. Responsive CSS Images - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1018, 1018, '19. Responsive CSS Images - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=cc6NaiLaTw4', '{"duration": 590}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1019, 100, '20. CSS Padding And Margin - Full stack web development Tutorial Course', 'Bài giảng: 20. CSS Padding And Margin - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1019, 1019, '20. CSS Padding And Margin - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=XovykH6gG50', '{"duration": 583}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1020, 100, '21. CSS Rows And Columns - Full stack web development Tutorial Course', 'Bài giảng: 21. CSS Rows And Columns - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1020, 1020, '21. CSS Rows And Columns - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=WJZ-Tr9p8SM', '{"duration": 787}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1021, 100, '22. Intro to building your first CSS for website - Full stack web development Tutorial Course', 'Bài giảng: 22. Intro to building your first CSS for website - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1021, 1021, '22. Intro to building your first CSS for website - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=ohSUKfiuusI', '{"duration": 76}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1022, 100, '23. Building the Navbar - Full stack web development Tutorial Course', 'Bài giảng: 23. Building the Navbar - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1022, 1022, '23. Building the Navbar - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=kPPit1B3h6g', '{"duration": 1369}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1023, 100, '24. Making the Navbar Mobile Responsive, Part 1 - Full stack web development Tutorial Course', 'Bài giảng: 24. Making the Navbar Mobile Responsive, Part 1 - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1023, 1023, '24. Making the Navbar Mobile Responsive, Part 1 - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=sLBrE13iAeY', '{"duration": 661}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1024, 100, '25. Making the Navbar Mobile Responsive Part 2 - Full stack web development Tutorial Course', 'Bài giảng: 25. Making the Navbar Mobile Responsive Part 2 - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1024, 1024, '25. Making the Navbar Mobile Responsive Part 2 - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=1XuEwQzprQM', '{"duration": 1425}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1025, 100, '26. Creating the Form Group - Full stack web development Tutorial Course', 'Bài giảng: 26. Creating the Form Group - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1025, 1025, '26. Creating the Form Group - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=yP05jzDamEU', '{"duration": 1848}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1026, 100, '27. Working with CSS iframes - Full stack web development Tutorial Course', 'Bài giảng: 27. Working with CSS iframes - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1026, 1026, '27. Working with CSS iframes - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=d4Zb4f4rnL4', '{"duration": 1338}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1027, 100, '28. Working with CSS images and box shadows - Full stack web development Tutorial Course', 'Bài giảng: 28. Working with CSS images and box shadows - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1027, 1027, '28. Working with CSS images and box shadows - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=dnM5Dg0BwAs', '{"duration": 2556}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1028, 100, '29. Working with text and image spacing - Full stack web development Tutorial Course', 'Bài giảng: 29. Working with text and image spacing - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1028, 1028, '29. Working with text and image spacing - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=8YIhE1vu_zY', '{"duration": 1706}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1029, 100, '30. Building the footer using CSS - Full stack web development Tutorial Course', 'Bài giảng: 30. Building the footer using CSS - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1029, 1029, '30. Building the footer using CSS - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=RzSIfJ_ezA8', '{"duration": 1082}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1030, 100, '31. Introduction to JavaScript Tutorial For Beginners - Full stack web development Tutorial Course', 'Bài giảng: 31. Introduction to JavaScript Tutorial For Beginners - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1030, 1030, '31. Introduction to JavaScript Tutorial For Beginners - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=zeHTGnv3hvk', '{"duration": 531}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1031, 100, '32. Javascript Quick References for faster learning - Full stack web development Course', 'Bài giảng: 32. Javascript Quick References for faster learning - Full stack web development Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1031, 1031, '32. Javascript Quick References for faster learning - Full stack web development Course', 'active', 'https://www.youtube.com/watch?v=UL89325ISSg', '{"duration": 262}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1032, 100, '33. Javascript Comments And How to Link Scripts - Full stack web development Course', 'Bài giảng: 33. Javascript Comments And How to Link Scripts - Full stack web development Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1032, 1032, '33. Javascript Comments And How to Link Scripts - Full stack web development Course', 'active', 'https://www.youtube.com/watch?v=GrE94gYyB7c', '{"duration": 465}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1033, 100, '34. Javascript Variables And Strings - Full stack web development Course', 'Bài giảng: 34. Javascript Variables And Strings - Full stack web development Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1033, 1033, '34. Javascript Variables And Strings - Full stack web development Course', 'active', 'https://www.youtube.com/watch?v=TvoJ2fJpJrw', '{"duration": 778}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1034, 100, '35. Javascript Numbers and Operators - Full stack web development Course', 'Bài giảng: 35. Javascript Numbers and Operators - Full stack web development Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1034, 1034, '35. Javascript Numbers and Operators - Full stack web development Course', 'active', 'https://www.youtube.com/watch?v=4voQ4PXF28o', '{"duration": 484}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1035, 100, '36. Javascript comparison operators and conditional operators - Full stack web development Course', 'Bài giảng: 36. Javascript comparison operators and conditional operators - Full stack web development Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1035, 1035, '36. Javascript comparison operators and conditional operators - Full stack web development Course', 'active', 'https://www.youtube.com/watch?v=dPDhkI3KJxI', '{"duration": 941}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1036, 100, '37. Javascript Logical Operators - Full stack web development Course', 'Bài giảng: 37. Javascript Logical Operators - Full stack web development Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1036, 1036, '37. Javascript Logical Operators - Full stack web development Course', 'active', 'https://www.youtube.com/watch?v=TajRfBY67RE', '{"duration": 920}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1037, 100, '38. Javascript Arrays - Full stack web development Course', 'Bài giảng: 38. Javascript Arrays - Full stack web development Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1037, 1037, '38. Javascript Arrays - Full stack web development Course', 'active', 'https://www.youtube.com/watch?v=1ibRw6EDzNM', '{"duration": 977}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1038, 100, '39. Javascript Loops (for, while, do while) - Full stack web development Course', 'Bài giảng: 39. Javascript Loops (for, while, do while) - Full stack web development Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1038, 1038, '39. Javascript Loops (for, while, do while) - Full stack web development Course', 'active', 'https://www.youtube.com/watch?v=D4NPEFki2c0', '{"duration": 863}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1039, 100, '40. Javascript Functions and methods - Full stack web development Course', 'Bài giảng: 40. Javascript Functions and methods - Full stack web development Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1039, 1039, '40. Javascript Functions and methods - Full stack web development Course', 'active', 'https://www.youtube.com/watch?v=bf8Rg6HsBBY', '{"duration": 1340}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1040, 100, '41. Javascript Objects - Full stack web development Course', 'Bài giảng: 41. Javascript Objects - Full stack web development Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1040, 1040, '41. Javascript Objects - Full stack web development Course', 'active', 'https://www.youtube.com/watch?v=AJ-TdjcHhzY', '{"duration": 1626}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1041, 100, '42. Javascript Bind - Full stack web development Course', 'Bài giảng: 42. Javascript Bind - Full stack web development Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1041, 1041, '42. Javascript Bind - Full stack web development Course', 'active', 'https://www.youtube.com/watch?v=SsT5e50c_Mo', '{"duration": 511}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1042, 100, '43. Percentage Calculator & Setting Up the Form with JavaScript - Full stack web development Course', 'Bài giảng: 43. Percentage Calculator & Setting Up the Form with JavaScript - Full stack web development Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1042, 1042, '43. Percentage Calculator & Setting Up the Form with JavaScript - Full stack web development Course', 'active', 'https://www.youtube.com/watch?v=77SCFCYq1vI', '{"duration": 477}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1043, 100, '44. Percentage Calculator And Grabbing Elements With JavaScript - Full stack web development Course', 'Bài giảng: 44. Percentage Calculator And Grabbing Elements With JavaScript - Full stack web development Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1043, 1043, '44. Percentage Calculator And Grabbing Elements With JavaScript - Full stack web development Course', 'active', 'https://www.youtube.com/watch?v=vnBHvI4onVI', '{"duration": 639}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1044, 100, '45. Percentage Calculator using Event Listeners in JavaScript - Full stack web development Course', 'Bài giảng: 45. Percentage Calculator using Event Listeners in JavaScript - Full stack web development Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1044, 1044, '45. Percentage Calculator using Event Listeners in JavaScript - Full stack web development Course', 'active', 'https://www.youtube.com/watch?v=nWFS0v42uF8', '{"duration": 775}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1045, 100, '46. Percentage Calculator Algorithm And Prevent Default with JS - Full stack web development Course', 'Bài giảng: 46. Percentage Calculator Algorithm And Prevent Default with JS - Full stack web development Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1045, 1045, '46. Percentage Calculator Algorithm And Prevent Default with JS - Full stack web development Course', 'active', 'https://www.youtube.com/watch?v=G02Hd7t0SMI', '{"duration": 490}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1046, 100, '47. How Version Control Works - Full stack web development Course', 'Bài giảng: 47. How Version Control Works - Full stack web development Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1046, 1046, '47. How Version Control Works - Full stack web development Course', 'active', 'https://www.youtube.com/watch?v=Lu7EfTIBfZE', '{"duration": 720}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1047, 100, '48. Basic macOS Terminal Commands to learn Git CLI - Full stack web development Course', 'Bài giảng: 48. Basic macOS Terminal Commands to learn Git CLI - Full stack web development Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1047, 1047, '48. Basic macOS Terminal Commands to learn Git CLI - Full stack web development Course', 'active', 'https://www.youtube.com/watch?v=y0ripLGqfHg', '{"duration": 864}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1048, 100, '49. Learn Git Basics and Github CLI Commands  - Full stack web development Course', 'Bài giảng: 49. Learn Git Basics and Github CLI Commands  - Full stack web development Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1048, 1048, '49. Learn Git Basics and Github CLI Commands  - Full stack web development Course', 'active', 'https://www.youtube.com/watch?v=j0mIPKE1q84', '{"duration": 981}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1049, 100, '50. Setting up Github on macOS - Full stack web development Course', 'Bài giảng: 50. Setting up Github on macOS - Full stack web development Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1049, 1049, '50. Setting up Github on macOS - Full stack web development Course', 'active', 'https://www.youtube.com/watch?v=I35kejU9nj4', '{"duration": 336}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1050, 100, '51. Github vs Bitbucket - Full stack web development Course', 'Bài giảng: 51. Github vs Bitbucket - Full stack web development Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1050, 1050, '51. Github vs Bitbucket - Full stack web development Course', 'active', 'https://www.youtube.com/watch?v=MnfbJrdbikM', '{"duration": 187}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1051, 100, '52. Git Local & Remote Repositories - Full stack web development Course', 'Bài giảng: 52. Git Local & Remote Repositories - Full stack web development Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1051, 1051, '52. Git Local & Remote Repositories - Full stack web development Course', 'active', 'https://www.youtube.com/watch?v=nv7O0XqaV6s', '{"duration": 1053}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1052, 100, '53. Working Through Git Merge Conflicts - Full stack web development Course', 'Bài giảng: 53. Working Through Git Merge Conflicts - Full stack web development Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1052, 1052, '53. Working Through Git Merge Conflicts - Full stack web development Course', 'active', 'https://www.youtube.com/watch?v=aP9rtjZeiDQ', '{"duration": 678}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1053, 100, '54. What is Bootstrap ? - Full stack web development Tutorial Course', 'Bài giảng: 54. What is Bootstrap ? - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1053, 1053, '54. What is Bootstrap ? - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=d7Aww1g6PkE', '{"duration": 193}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1054, 100, '55. Downloading Bootstrap 4 - Full stack web development Tutorial Course', 'Bài giảng: 55. Downloading Bootstrap 4 - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1054, 1054, '55. Downloading Bootstrap 4 - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=fBw4-7qRyR8', '{"duration": 185}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1055, 100, '56. Bootstrap 4 Project Setup - Full stack web development Tutorial Course', 'Bài giảng: 56. Bootstrap 4 Project Setup - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1055, 1055, '56. Bootstrap 4 Project Setup - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=EfcMsaWcg7A', '{"duration": 436}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1056, 100, '57. Overview of Bootstrap 4 Components - Full stack web development Tutorial Course', 'Bài giảng: 57. Overview of Bootstrap 4 Components - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1056, 1056, '57. Overview of Bootstrap 4 Components - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=-aVPkEDPt0s', '{"duration": 358}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1057, 100, '58. Understanding Bootstrap 4 Classes - Full stack web development Tutorial Course', 'Bài giảng: 58. Understanding Bootstrap 4 Classes - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1057, 1057, '58. Understanding Bootstrap 4 Classes - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=Drr_TAC9XqQ', '{"duration": 196}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1058, 100, '59. Using the Grid System in Bootstrap 4 - Full stack web development Tutorial Course', 'Bài giảng: 59. Using the Grid System in Bootstrap 4 - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1058, 1058, '59. Using the Grid System in Bootstrap 4 - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=KnDhahDkCCU', '{"duration": 1128}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1059, 100, '60. Bootstrap 4 Cards & Creating a Sign-in Portal Box - Full stack web development Tutorial Course', 'Bài giảng: 60. Bootstrap 4 Cards & Creating a Sign-in Portal Box - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1059, 1059, '60. Bootstrap 4 Cards & Creating a Sign-in Portal Box - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=bNLEvoznZu4', '{"duration": 1177}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1060, 100, '61. Bootstrap 4 Forms | Adding an image & input fields - Full stack web development Tutorial Course', 'Bài giảng: 61. Bootstrap 4 Forms | Adding an image & input fields - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1060, 1060, '61. Bootstrap 4 Forms | Adding an image & input fields - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=pmgLSh-ZoNw', '{"duration": 999}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1061, 100, '62. Bootstrap 4 Adding button and labels in login form - Full stack web development Tutorial Course', 'Bài giảng: 62. Bootstrap 4 Adding button and labels in login form - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1061, 1061, '62. Bootstrap 4 Adding button and labels in login form - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=q44t3sA1Cq4', '{"duration": 1112}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1062, 100, '63. Bootstrap 4 Grids | Finishing Mobile Compatibility - Full stack web development Tutorial Course', 'Bài giảng: 63. Bootstrap 4 Grids | Finishing Mobile Compatibility - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1062, 1062, '63. Bootstrap 4 Grids | Finishing Mobile Compatibility - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=lh9ow8ziouY', '{"duration": 510}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1063, 100, '64.  Bootstrap 4 Skate or Die Intro - Full stack web development Tutorial Course', 'Bài giảng: 64.  Bootstrap 4 Skate or Die Intro - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1063, 1063, '64.  Bootstrap 4 Skate or Die Intro - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=5HRK51QlkwI', '{"duration": 77}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1064, 100, '65.  Bootstrap 4 Working with Navbars - Full stack web development Tutorial Course', 'Bài giảng: 65.  Bootstrap 4 Working with Navbars - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1064, 1064, '65.  Bootstrap 4 Working with Navbars - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=Ya3nfmqQedQ', '{"duration": 633}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1065, 100, '66. Bootstrap 4 Jumbotron and Carousels - Full stack web development Tutorial Course', 'Bài giảng: 66. Bootstrap 4 Jumbotron and Carousels - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1065, 1065, '66. Bootstrap 4 Jumbotron and Carousels - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=dYVadd2qqOU', '{"duration": 1396}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1066, 100, '67. Bootstrap 4 Nesting rows and columns - Full stack web development Tutorial Course', 'Bài giảng: 67. Bootstrap 4 Nesting rows and columns - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1066, 1066, '67. Bootstrap 4 Nesting rows and columns - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=lx8XApoZj58', '{"duration": 1256}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1067, 100, '68. Bootstrap 4 Modals and PopUp - Full stack web development Tutorial Course', 'Bài giảng: 68. Bootstrap 4 Modals and PopUp - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1067, 1067, '68. Bootstrap 4 Modals and PopUp - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=uOlzIM76usg', '{"duration": 972}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1068, 100, '69. Bootstrap 4 Adding images and buttons - Full stack web development Tutorial Course', 'Bài giảng: 69. Bootstrap 4 Adding images and buttons - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1068, 1068, '69. Bootstrap 4 Adding images and buttons - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=E8UCCJRumqM', '{"duration": 1139}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1069, 100, '70. Bootstrap 4 Sizing Modals - Full stack web development Tutorial Course', 'Bài giảng: 70. Bootstrap 4 Sizing Modals - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1069, 1069, '70. Bootstrap 4 Sizing Modals - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=hF0cVwRe8e0', '{"duration": 1100}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1070, 100, '71. Bootstrap 4 Building the Footer - Full stack web development Tutorial Course', 'Bài giảng: 71. Bootstrap 4 Building the Footer - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1070, 1070, '71. Bootstrap 4 Building the Footer - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=BACjnYoVnsc', '{"duration": 455}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1071, 100, '72. Bootstrap 4 Working with Font Awesome Favicons - Full stack web development Tutorial Course', 'Bài giảng: 72. Bootstrap 4 Working with Font Awesome Favicons - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1071, 1071, '72. Bootstrap 4 Working with Font Awesome Favicons - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=Z7oJFC-L0Og', '{"duration": 934}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1072, 100, '73. How to use SCSS in HTML - Intro to Sass - Full stack web development Tutorial Course', 'Bài giảng: 73. How to use SCSS in HTML - Intro to Sass - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1072, 1072, '73. How to use SCSS in HTML - Intro to Sass - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=3K9X8uDzp2Q', '{"duration": 71}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1073, 100, '74.  What is Sass? - SASS Tutorial - Full stack web development Tutorial Course', 'Bài giảng: 74.  What is Sass? - SASS Tutorial - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1073, 1073, '74.  What is Sass? - SASS Tutorial - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=APM1mcKbK7o', '{"duration": 483}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1074, 100, '75 . How to install Sass and compile it to CSS - Full stack web development Tutorial Course', 'Bài giảng: 75 . How to install Sass and compile it to CSS - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1074, 1074, '75 . How to install Sass and compile it to CSS - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=IY-y4JnDUZg', '{"duration": 707}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1075, 100, '76.  Your FIRST Sass Website! - Full stack web development Tutorial Course', 'Bài giảng: 76.  Your FIRST Sass Website! - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1075, 1075, '76.  Your FIRST Sass Website! - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=K1Yx4W6wDCY', '{"duration": 1788}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1076, 100, '77.  Different tools to compile Sass - Full stack web development Tutorial Course', 'Bài giảng: 77.  Different tools to compile Sass - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1076, 1076, '77.  Different tools to compile Sass - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=4QK58G2Ebds', '{"duration": 405}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1077, 100, '78.  How to structure your Sass ? - Full stack web development Tutorial Course', 'Bài giảng: 78.  How to structure your Sass ? - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1077, 1077, '78.  How to structure your Sass ? - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=fE7FtT1_Nu4', '{"duration": 885}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1078, 100, '79.  Sass Partials Maintainable Styles Rules - Full stack web development Tutorial Course', 'Bài giảng: 79.  Sass Partials Maintainable Styles Rules - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1078, 1078, '79.  Sass Partials Maintainable Styles Rules - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=55DHzAQNnrw', '{"duration": 437}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1079, 100, '80.  Sass Variables and Imports | Create a clean Scalable Stylesheet -  Full stack web development', 'Bài giảng: 80.  Sass Variables and Imports | Create a clean Scalable Stylesheet -  Full stack web development', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1079, 1079, '80.  Sass Variables and Imports | Create a clean Scalable Stylesheet -  Full stack web development', 'active', 'https://www.youtube.com/watch?v=xO7HbkpBDTA', '{"duration": 1254}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1080, 100, '81. Sass Mixins Save time & recycle styles - Full stack web development Tutorial Course', 'Bài giảng: 81. Sass Mixins Save time & recycle styles - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1080, 1080, '81. Sass Mixins Save time & recycle styles - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=C4IxmMvqxJs', '{"duration": 717}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1081, 100, '82.  Sass Extends Share style properties between other selectors -  Full stack web development', 'Bài giảng: 82.  Sass Extends Share style properties between other selectors -  Full stack web development', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1081, 1081, '82.  Sass Extends Share style properties between other selectors -  Full stack web development', 'active', 'https://www.youtube.com/watch?v=SWiT7deAOPM', '{"duration": 379}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1082, 100, '83.  Final Project Intro to our Landing Page using SASS  -  Full stack web development Tutorial', 'Bài giảng: 83.  Final Project Intro to our Landing Page using SASS  -  Full stack web development Tutorial', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1082, 1082, '83.  Final Project Intro to our Landing Page using SASS  -  Full stack web development Tutorial', 'active', 'https://www.youtube.com/watch?v=9J1tWxlJ6kE', '{"duration": 61}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1083, 100, '84 . Final Project Setting up our Variables With Sass - Full stack web development Tutorial Course', 'Bài giảng: 84 . Final Project Setting up our Variables With Sass - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1083, 1083, '84 . Final Project Setting up our Variables With Sass - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=gWx-zDKITvM', '{"duration": 923}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1084, 100, '85.  Final Project Styling our Navbar with Sass - Full stack web development Tutorial Course', 'Bài giảng: 85.  Final Project Styling our Navbar with Sass - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1084, 1084, '85.  Final Project Styling our Navbar with Sass - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=-bvvi40oFes', '{"duration": 666}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1085, 100, '86.  Final Project Creating the Banner Container - Full stack web development Tutorial Course', 'Bài giảng: 86.  Final Project Creating the Banner Container - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1085, 1085, '86.  Final Project Creating the Banner Container - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=NqGFXyCgJ-8', '{"duration": 200}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1086, 100, '87.  Final Project Adding Content to our Banner Container - Full stack web development Tutorial', 'Bài giảng: 87.  Final Project Adding Content to our Banner Container - Full stack web development Tutorial', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1086, 1086, '87.  Final Project Adding Content to our Banner Container - Full stack web development Tutorial', 'active', 'https://www.youtube.com/watch?v=6FHRlUj_SJk', '{"duration": 626}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1087, 100, '88.  Final Project Doing some quick cleanup in our Sass files - Full stack web development Tutorial', 'Bài giảng: 88.  Final Project Doing some quick cleanup in our Sass files - Full stack web development Tutorial', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1087, 1087, '88.  Final Project Doing some quick cleanup in our Sass files - Full stack web development Tutorial', 'active', 'https://www.youtube.com/watch?v=yWt9EEszK00', '{"duration": 266}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1088, 100, '89.  Final Project Using the Extend method - Full stack web development Tutorial Course', 'Bài giảng: 89.  Final Project Using the Extend method - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1088, 1088, '89.  Final Project Using the Extend method - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=bW1wr5X__KY', '{"duration": 304}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1089, 100, '90.  Final Project Styling a section splitter with Sass - Full stack web development Tutorial Course', 'Bài giảng: 90.  Final Project Styling a section splitter with Sass - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1089, 1089, '90.  Final Project Styling a section splitter with Sass - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=iSFByalOdRU', '{"duration": 303}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1090, 100, '91.  Final Project Using advanced Mixings - Full stack web development Tutorial Course', 'Bài giảng: 91.  Final Project Using advanced Mixings - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1090, 1090, '91.  Final Project Using advanced Mixings - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=dydPJPnJDhc', '{"duration": 1276}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1091, 100, '92.  Final Project Styling the next Container with Sass - Full stack web development Tutorial Course', 'Bài giảng: 92.  Final Project Styling the next Container with Sass - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1091, 1091, '92.  Final Project Styling the next Container with Sass - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=sHSSqmuV1rA', '{"duration": 351}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1092, 100, '93.  Final Project Finishing our Landing Page Congrats! - Full stack web development Tutorial Course', 'Bài giảng: 93.  Final Project Finishing our Landing Page Congrats! - Full stack web development Tutorial Course', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1092, 1092, '93.  Final Project Finishing our Landing Page Congrats! - Full stack web development Tutorial Course', 'active', 'https://www.youtube.com/watch?v=69elAZadMRA', '{"duration": 642}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1093, 100, '94. Set Up Hosting For Your Web App', 'Bài giảng: 94. Set Up Hosting For Your Web App', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1093, 1093, '94. Set Up Hosting For Your Web App', 'active', 'https://www.youtube.com/watch?v=HmC0lzW7ENc', '{"duration": 75}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1094, 100, '95. Creating a server', 'Bài giảng: 95. Creating a server', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1094, 1094, '95. Creating a server', 'active', 'https://www.youtube.com/watch?v=QpkzOklfovE', '{"duration": 328}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1095, 100, '96. Accessing your server', 'Bài giảng: 96. Accessing your server', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1095, 1095, '96. Accessing your server', 'active', 'https://www.youtube.com/watch?v=MIor3TkgNtE', '{"duration": 133}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1096, 100, '97. Installing Nginx', 'Bài giảng: 97. Installing Nginx', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1096, 1096, '97. Installing Nginx', 'active', 'https://www.youtube.com/watch?v=lWRU7ywMh90', '{"duration": 248}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1097, 100, '98. Creating server SSH Keys', 'Bài giảng: 98. Creating server SSH Keys', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1097, 1097, '98. Creating server SSH Keys', 'active', 'https://www.youtube.com/watch?v=k1eP3IUjwXM', '{"duration": 243}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1098, 100, '99. Uploading files to your server', 'Bài giảng: 99. Uploading files to your server', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1098, 1098, '99. Uploading files to your server', 'active', 'https://www.youtube.com/watch?v=JXFgc6yTcMY', '{"duration": 152}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

INSERT INTO lessons (lesson_id, course_id, title, description, lesson_status)
VALUES (1099, 100, '100. Setting up a domain', 'Bài giảng: 100. Setting up a domain', 'active')
ON CONFLICT (lesson_id) DO NOTHING;

INSERT INTO learning_materials (material_id, lesson_id, title, learning_status, material_url, material_metadata)
VALUES (1099, 1099, '100. Setting up a domain', 'active', 'https://www.youtube.com/watch?v=uTRH07Utp2g', '{"duration": 330}'::jsonb)
ON CONFLICT (material_id) DO NOTHING;

-- Sync sequences
SELECT setval(pg_get_serial_sequence('courses', 'course_id'), (SELECT MAX(course_id) FROM courses));
SELECT setval(pg_get_serial_sequence('lessons', 'lesson_id'), (SELECT MAX(lesson_id) FROM lessons));
SELECT setval(pg_get_serial_sequence('learning_materials', 'material_id'), (SELECT MAX(material_id) FROM learning_materials));