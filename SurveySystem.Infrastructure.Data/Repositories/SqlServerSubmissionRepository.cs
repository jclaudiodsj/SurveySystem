using Microsoft.EntityFrameworkCore;
using SurveySystem.Domain.Repositories;
using SurveySystem.Domain.Submissions;

namespace SurveySystem.Infrastructure.Data.Repositories
{
    public class SqlServerSubmissionRepository : ISubmissionRepository
    {
        private readonly SurveySystemDbContext _context;

        public SqlServerSubmissionRepository(SurveySystemDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task Add(Submission submission)
        {
            await _context.Submissions.AddAsync(submission);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Submission submission)
        {
            _context.Submissions.Update(submission);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            var submission = await _context.Submissions.FindAsync(id);

            if (submission is not null)
            {
                _context.Submissions.Remove(submission);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Submission> GetById(Guid id)
        {
            return await _context.Submissions
                .Include(c => c.Answers)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Submission>> GetAll()
        {
            return await _context.Submissions
                .Include(c => c.Answers)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Submission>> GetBySurveyId(Guid surveyId)
        {
            return await _context.Submissions
                .Where(s => s.SurveyId == surveyId)
                .Include(s => s.Answers)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}