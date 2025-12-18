using Microsoft.EntityFrameworkCore;
using SurveySystem.Domain;
using SurveySystem.Domain.Repositories;

namespace SurveySystem.Infrastructure.Data.Repositories
{
    public class SqlServerSurveyRepository : ISurveyRepository
    {
        private readonly SurveyDbContext _context;

        public SqlServerSurveyRepository(SurveyDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
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
                .Include(c => c.Title)
                .Include(c => c.Description)
                .Include(c => c.Status)
                .Include(c => c.Period)
                .Include(c => c.CreatedAt)
                .Include(c => c.UpdatedAt)
                .Include(c => c.PublishedAt)
                .Include(c => c.ClosedAt)
                .Include(c => c.Questions)
                .ToListAsync();
        }

        public async Task<Survey> GetById(Guid id)
        {
            return await _context.Surveys
                .Include(c => c.Title)
                .Include(c => c.Description)
                .Include(c => c.Status)
                .Include(c => c.Period)
                .Include(c => c.CreatedAt)
                .Include(c => c.UpdatedAt)
                .Include(c => c.PublishedAt)
                .Include(c => c.ClosedAt)
                .Include(c => c.Questions)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<bool> SurveyExists(Guid surveyId)
        {
            return await _context.Surveys.AnyAsync(c => c.Id == surveyId);
        }
    }
}
