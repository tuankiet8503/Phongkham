using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Phongkham.Data;
using Phongkham.Models;
using Phongkham.ViewModels;

namespace Phongkham.Controllers
{
    [Authorize]
    public class QuestionsController : Controller
    {
        private readonly ApplicationDBcontext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public QuestionsController(ApplicationDBcontext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            var questionsWithAnswers = _context.Questions
                 .Select(q => new QuestionWithAnswersViewModel
                 {
                     Id = q.Id,
                     QuestionContent = q.Content,
                     PatientName = q.Patient.UserName,
                     DateAsked = q.DateAsked,
                     IsAnswered = q.IsAnswered,
                     Answers = q.Answers.Select(a => new AnswerViewModel
                     {
                         Content = a.Content,
                         DentistName = a.Dentist.UserName,
                         DateAnswered = a.DateAnswered
                     }).ToList()
                 }).ToList();

            return View(questionsWithAnswers);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(QuestionCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var question = new Questions
                {
                    PatientId = user.Id,
                    Content = model.Content,
                    DateAsked = DateTime.Now,
                    IsAnswered = false
                };

                _context.Questions.Add(question);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        public IActionResult Answer(int questionId)
        {
            var model = new AnswerCreateViewModel
            {
                QuestionId = questionId
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Answer(AnswerCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var answer = new Answers
                {
                    QuestionId = model.QuestionId,
                    DentistId = user.Id,
                    Content = model.Content,
                    DateAnswered = DateTime.Now
                };

                _context.Answers.Add(answer);

                var question = _context.Questions.FirstOrDefault(q => q.Id == model.QuestionId);
                if (question != null)
                {
                    question.IsAnswered = true;
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }
    }
}
