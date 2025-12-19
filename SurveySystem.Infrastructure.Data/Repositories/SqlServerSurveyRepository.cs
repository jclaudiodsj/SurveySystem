using Microsoft.EntityFrameworkCore;
using SurveySystem.Domain;
using SurveySystem.Domain.Repositories;
using SurveySystem.Domain.Surveys;

namespace SurveySystem.Infrastructure.Data.Repositories
{
    public class SqlServerSurveyRepository : ISurveyRepository
    {
        private readonly SurveyDbContext _context;

        public SqlServerSurveyRepository(SurveyDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task Add(Survey survey)
        {
            await _context.Surveys.AddAsync(survey);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Survey survey)
        {
            _context.Surveys.Update(survey);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            var survey = await _context.Surveys.FindAsync(id);

            if (survey is not null)
            {
                _context.Surveys.Remove(survey);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Survey>> GetAll()
        {
            return await _context.Surveys
                .Include(c => c.Questions)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Survey> GetById(Guid id)
        {
            return await _context.Surveys
                .Include(c => c.Questions)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> Exists(Guid surveyId)
        {
            return await _context.Surveys.AnyAsync(c => c.Id == surveyId);
        }

        public async Task Submit(Submission submission)
        {
            await _context.Submissions.AddAsync(submission);
            await _context.SaveChangesAsync();
        }

        public async Task<Submission> GetSubmissionById(Guid submissionId)
        {
            return await _context.Submissions
                .Include(s => s.Answers)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == submissionId);
        }

        public async Task<List<Submission>> GetSubmissionsBySurveyId(Guid surveyId)
        {
            return await _context.Submissions
                .Where(s => s.SurveyId == surveyId)
                .Include(s => s.Answers)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}