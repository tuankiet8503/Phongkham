namespace Phongkham.Models
{
    public class Answers
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string DentistId { get; set; } // Thêm trường này để lưu Id của nha sĩ trả lời
        public string Content { get; set; }
        public DateTime DateAnswered { get; set; }

        public Questions Question { get; set; }
        public ApplicationUser Dentist { get; set; } // Đây là nha sĩ trả lời
    }
}
