namespace Phongkham.Models
{
    public class Questions
    {

        public int Id { get; set; }
        public string PatientId { get; set; } // Thay UserId thành PatientId để phản ánh người hỏi là bệnh nhân
        public string Content { get; set; }
        public DateTime DateAsked { get; set; }
        public bool IsAnswered { get; set; }

        public ApplicationUser Patient { get; set; } // Đây là bệnh nhân hỏi
        public ICollection<Answers> Answers { get; set; }
    }
}
