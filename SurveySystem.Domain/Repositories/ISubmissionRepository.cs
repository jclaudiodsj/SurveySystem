using SurveySystem.Domain.Surveys;

namespace SurveySystem.Domain.Repositories
{
    public interface ISubmissionRepository
    {
        Task Add(Submission submission);

        Task Update(Submission submission);

        Task Delete(Guid submissionId);

        Task<Submission> GetById(Guid submissionId);

        Task<List<Submission>> GetBySurveyId(Guid surveyId);

        Task<List<Submission>> GetAll();

        Task<bool> SubmissionExists(Guid submissionId);
        Task<int> SaveChangesAsync();
    }
}
