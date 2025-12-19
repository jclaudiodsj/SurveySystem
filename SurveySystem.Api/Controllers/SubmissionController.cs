using Microsoft.AspNetCore.Mvc;
using SurveySystem.Api.Dtos.Shared;
using SurveySystem.Api.Dtos.Submissions.Requests;
using SurveySystem.Api.Dtos.Submissions.Responses;
using SurveySystem.Domain.Repositories;
using SurveySystem.Domain.Shared;
using SurveySystem.Domain.Submissions;

namespace SurveySystem.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SubmissionController : ControllerBase
    {
        private readonly ISubmissionRepository _submissionRepository;
        private readonly ISurveyRepository _surveyRepository;
        private readonly ILogger<SurveyController> _logger;

        public SubmissionController(ISubmissionRepository submissionRepository, ISurveyRepository surveyRepository, ILogger<SurveyController> logger)
        {
            _submissionRepository = submissionRepository;
            _surveyRepository = surveyRepository;
            _logger = logger;
        }

        private SubmissionResponse MapToSubmissionResponse(Submission submission)
        {
            return new SubmissionResponse
            {
                Id = submission.Id,
                SurveyId = submission.SurveyId,
                SubmittedAt = submission.SubmittedAt,
                Answers = submission.Answers.Select(a => new AnswerResponse
                {
                    QuestionText = a.QuestionText,
                    OptionText = a.OptionText
                }).ToList()
            };
        }

        /// <summary>
        /// Cria uma nova submissão.
        /// </summary>
        /// <param name="request">Dados para criação da submissão.</param>
        /// <returns>A submissão criada.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([NotEmptyGuid(ErrorMessage = "SurveyId must not be empty.")] Guid surveyId, [FromBody] CreateSubmissionRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                List<Answer> answers = request.Answers.Select(a => 
                    Answer.Create(a.QuestionText, a.OptionText)
                ).ToList();

                var survey = await _surveyRepository.GetById(surveyId);

                if (survey is null)
                    return BadRequest(new { message = $"Survey with ID {surveyId} not found." });
                
                if (survey.Status == Domain.Surveys.SurveyStatus.Draft)
                    return BadRequest(new { message = "It is not possible to submit responses to a survey that has not yet been published." });

                if (survey.Status == Domain.Surveys.SurveyStatus.Closed)
                    return BadRequest(new { message = "It is not possible to submit responses for a survey that has already been closed." });

                if (DateTime.UtcNow < survey.Period.StartDate || DateTime.UtcNow > survey.Period.EndDate)
                    return BadRequest(new { message = $"It is not possible to submit responses to a survey outside of its scheduled period ({survey.Period.StartDate:dd/MM/yyyy} ~ {survey.Period.EndDate:dd/MM/yyyy})." });

                foreach (var question in survey.Questions)
                {
                    if (!answers.Any(a => a.QuestionText == question.Text))
                        return BadRequest(new { message = $"The question ({question.Text}) was not answered." });
                }

                foreach (var answer in answers)
                {
                    if (survey.Questions.FirstOrDefault(q => q.Text == answer.QuestionText) is null)
                        return BadRequest(new { message = $"({answer.QuestionText}) was not found in the search ({surveyId})." });

                    if (survey.Questions.First(q => q.Text == answer.QuestionText).Options.FirstOrDefault(o => o.Text == answer.OptionText) is null)
                        return BadRequest(new { message = $"({answer.OptionText}) is not a valid option for the question ({answer.QuestionText})." });

                    if (survey.Questions.Count(q => q.Text == answer.QuestionText) > 1)
                        return BadRequest(new { message = $"Each question can only be answered once. Review the question ({answer.QuestionText})." });
                }                

                // Map DTO to Domain Value Objects
                var submission = Submission.Create(surveyId, DateTime.UtcNow, answers);

                await _submissionRepository.Add(submission);

                var response = MapToSubmissionResponse(submission);
                return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Domain error when creating submission: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal error while creating submission.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal error occurred while processing your request." });
            }
        }

        /// <summary>
        /// Obtém uma submissão pelo seu ID.
        /// </summary>
        /// <param name="id">ID da submissão.</param>
        /// <returns>A submissão encontrada.</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById([NotEmptyGuid(ErrorMessage = "SurveyId must not be empty.")] Guid id)
        {
            try
            {
                var submission = await _submissionRepository.GetById(id);

                if (submission is null)
                    return NotFound(new { message = $"Submission with ID {id} not found." });

                var response = MapToSubmissionResponse(submission);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal error while retrieving submission with ID {SubmissionId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal error occurred while processing your request." });
            }
        }

        /// <summary>
        /// Lista todas as submissões com paginação opcional.
        /// </summary>
        /// <param name="pageNumber">Número da página (padrão: 1).</param>
        /// <param name="pageSize">Tamanho da página (padrão: 10).</param>
        /// <returns>Uma lista de submissões.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var submissions = await _submissionRepository.GetAll();
                var response = submissions.Select(MapToSubmissionResponse).ToList();
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal error while listing submissions.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal error occurred while processing your request." });
            }
        }

        /// <summary>
        /// Deleta todas as submissões.
        /// </summary>
        /// <returns>Status de sucesso sem conteúdo.</returns>
        [HttpDelete("DeleteAll")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteAll()
        {
            try
            {
                var submissions = await _submissionRepository.GetAll();
        
                foreach (var submission in submissions)
                    await _submissionRepository.Delete(submission.Id);
        
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal error while deleting all submissions.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal error occurred while processing your request." });
            }
        }
    }
}
