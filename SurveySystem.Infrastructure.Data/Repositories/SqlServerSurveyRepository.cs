using Microsoft.EntityFrameworkCore;
using SurveySystem.Domain.Repositories;
using SurveySystem.Domain.Surveys;

namespace SurveySystem.Infrastructure.Data.Repositories
{
    public class SqlServerSurveyRepository : ISurveyRepository
    {
        private readonly SurveySystemDbContext _context;

        public SqlServerSurveyRepository(SurveySystemDbContext context)
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
                .Include(s => s.Questions)
                // Options é uma owned collection mapeada no field "_options".
                // Usamos string-based Include para alcançar membros privados.
                .Include("Questions._options")
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Survey?> GetById(Guid id)
        {
            return await _context.Surveys
                // IMPORTANT: Não use AsNoTracking aqui, pois esse método é usado em fluxos de alteração (PUT/POST).
                // Owned collections com shadow keys (ex.: Questions/Options) exigem entidades rastreadas para preservar as chaves.
                .Include(s => s.Questions)
                .Include("Questions._options")
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<bool> Exists(Guid surveyId)
        {
            return await _context.Surveys.AnyAsync(c => c.Id == surveyId);
        }
    }
}