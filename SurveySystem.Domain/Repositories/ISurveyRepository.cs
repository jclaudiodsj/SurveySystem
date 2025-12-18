namespace SurveySystem.Domain.Repositories
{
    public interface ISurveyRepository
    {
        Task Add(Survey survey);

        Task Update(Survey survey);

        Task Delete(Guid surveyId);

        Task<Survey> GetById(Guid surveyId);

        Task<List<Survey>> GetAll();

        Task<bool> SurveyExists(Guid surveyId);
        Task<int> SaveChangesAsync();
    }
}
