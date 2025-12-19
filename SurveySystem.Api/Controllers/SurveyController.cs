using Microsoft.AspNetCore.Mvc;
using SurveySystem.Api.Dtos.Shared;
using SurveySystem.Api.Dtos.Surveys.Requests;
using SurveySystem.Api.Dtos.Surveys.Responses;
using SurveySystem.Domain.Repositories;
using SurveySystem.Domain.Shared;
using SurveySystem.Domain.Surveys;
using System.ComponentModel.DataAnnotations;

namespace SurveySystem.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SurveyController : ControllerBase
    {
        private readonly ISurveyRepository _surveyRepository;
        private readonly ISubmissionRepository _submissionRepository;
        private readonly ILogger<SurveyController> _logger;

        public SurveyController(ISurveyRepository surveyRepository, ISubmissionRepository submissionRepository, ILogger<SurveyController> logger)
        {
            _surveyRepository = surveyRepository;
            _submissionRepository = submissionRepository;
            _logger = logger;
        }

        private SurveyResponse MapToSurveyResponse(Survey survey)
        {
            return new SurveyResponse
            {
                Id = survey.Id,
                Title = survey.Title,
                Description = (survey.Description == null ? string.Empty : survey.Description),
                Status = survey.Status,
                Period = new SurveyPeriodResponse
                {
                    StartDate = survey.Period.StartDate,
                    EndDate = survey.Period.EndDate
                },
                CreatedAt = survey.CreatedAt,
                UpdatedAt = survey.UpdatedAt,
                PublishedAt = survey.PublishedAt,
                ClosedAt = survey.ClosedAt,
                Question = survey.Questions.Select(q => new QuestionResponse
                {
                    Text = q.Text,
                    Order = q.Order,
                    Options = q.Options.Select(o => new OptionResponse
                    {
                        Text = o.Text,
                        Order = o.Order
                    }).ToList()
                }).ToList()
            };
        }

        /// <summary>
        /// Cria uma nova pesquisa.
        /// </summary>
        /// <param name="request">Dados para criação da pesquisa.</param>
        /// <returns>A pesquisa criada.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateSurvey([FromBody] CreateSurveyRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Map DTO to Domain Value Objects
                var survey = Survey.Create(request.Title, request.Description, request.SurveyPeriod.StartDate, request.SurveyPeriod.EndDate);

                if(request.Questions != null && request.Questions.Count > 0)
                {
                    foreach (var question in request.Questions)
                        survey.AddQuestion(question.Text, question.Options);
                }

                await _surveyRepository.Add(survey);

                var response = MapToSurveyResponse(survey);
                return CreatedAtAction(nameof(GetSurveyById), new { id = response.Id }, response);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Domain error when creating survey: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal error while creating survey.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal error occurred while processing your request." });
            }
        }

        /// <summary>
        /// Atualiza os dados básicos de uma pesquisa existente.
        /// </summary>
        /// <param name="id">ID da pesquisa a ser atualizada.</param>
        /// <param name="request">Novos dados da pesquisa.</param>
        /// <returns>A pesquisa atualizada.</returns>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateSurvey([NotEmptyGuid(ErrorMessage = "Id must not be empty.")] Guid id, [FromBody] UpdateSurveyRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var survey = await _surveyRepository.GetById(id);

                if (survey is null)
                    return NotFound(new { message = $"Survey with ID {id} not found." });

                survey.UpdateDetails(request.Title, request.Description, request.SurveyPeriod.StartDate, request.SurveyPeriod.EndDate);

                await _surveyRepository.Update(survey);

                var response = MapToSurveyResponse(survey);
                return Ok(response);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Domain error while updating survey {SurveyId}: {Message}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal error while updating survey {SurveyId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal error occurred while processing your request." });
            }
        }

        /// <summary>
        /// Deleta uma pesquisa pelo seu ID.
        /// </summary>
        /// <param name="id">ID da pesquisa a ser deletada.</param>
        /// <returns>Status de sucesso sem conteúdo.</returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteSurvey([NotEmptyGuid(ErrorMessage = "Id must not be empty.")] Guid id)
        {
            try
            {
                var survey = await _surveyRepository.GetById(id);

                if (survey is null)
                    return NotFound(new { message = $"Survey with ID {id} not found." });

                await _surveyRepository.Delete(survey.Id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal error while deleting survey {SurveyId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal error occurred while processing your request." });
            }
        }

        /// <summary>
        /// Adiciona uma nova questão a uma pesquisa existente.
        /// </summary>
        /// <param name="id">ID da pesquisa.</param>
        /// <param name="request">Dados da questão a ser adicionada.</param>
        /// <returns>A pesquisa atualizada.</returns>
        [HttpPost("{id:guid}/questions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddQuestion([NotEmptyGuid(ErrorMessage = "Id must not be empty.")] Guid id, [FromBody] AddQuestionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var survey = await _surveyRepository.GetById(id);
                if (survey is null)
                    return NotFound(new { message = $"Survey with ID {id} not found." });

                survey.AddQuestion(request.Text, request.Options);

                await _surveyRepository.Update(survey);

                var response = MapToSurveyResponse(survey);

                return Ok(response);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Domain error when adding question to survey {SurveyId}: {Message}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal error while adding question to survey {SurveyId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal error occurred while processing your request." });
            }
        }

        /// <summary>
        /// Deleta uma questão de uma pesquisa existente.
        /// </summary>
        /// <param name="id">ID da pesquisa.</param>
        /// <param name="questionIndex">Índice da questão a ser deletada.</param>
        /// <returns>A pesquisa atualizada.</returns>
        [HttpDelete("{id:guid}/questions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveQuestion([NotEmptyGuid(ErrorMessage = "Id must not be empty.")] Guid id, 
                                                        [Required]
                                                        [Range(0, int.MaxValue)] int questionIndex)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var survey = await _surveyRepository.GetById(id);
                if (survey is null)
                    return NotFound(new { message = $"Survey with ID {id} not found." });

                survey.RemoveQuestion(questionIndex);

                await _surveyRepository.Update(survey);

                var response = MapToSurveyResponse(survey);

                return Ok(response);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Domain error when deleting survey question {SurveyId}: {Message}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal error while deleting survey question {SurveyId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal error occurred while processing your request." });
            }
        }

        /// <summary>
        /// Publica uma pesquisa existente.
        /// </summary>
        /// <param name="id">ID da pesquisa.</param>
        /// <returns>A pesquisa atualizada.</returns>
        [HttpPost("{id:guid}/publish")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Publish([NotEmptyGuid(ErrorMessage = "Id must not be empty.")] Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var survey = await _surveyRepository.GetById(id);

                if (survey is null)
                    return NotFound(new { message = $"Survey with ID {id} not found." });

                survey.Publish();

                await _surveyRepository.Update(survey);

                var response = MapToSurveyResponse(survey);

                return Ok(response);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Domain error when publishing survey {SurveyId}: {Message}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal error while publishing the survey {SurveyId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal error occurred while processing your request." });
            }
        }

        /// <summary>
        /// Encerra uma pesquisa existente.
        /// </summary>
        /// <param name="id">ID da pesquisa.</param>
        /// <returns>A pesquisa atualizada.</returns>
        [HttpPost("{id:guid}/Close")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Close([NotEmptyGuid(ErrorMessage = "Id must not be empty.")] Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var survey = await _surveyRepository.GetById(id);

                if (survey is null)
                    return NotFound(new { message = $"Survey with ID {id} not found." });

                survey.Close();

                await _surveyRepository.Update(survey);

                var response = MapToSurveyResponse(survey);

                return Ok(response);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Domain error when closing survey {SurveyId}: {Message}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal error while closing survey {SurveyId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal error occurred while processing your request." });
            }
        }


        /// <summary>
        /// Obtém uma pesquisa pelo seu ID.
        /// </summary>
        /// <param name="id">ID da pesquisa.</param>
        /// <returns>A pesquisa encontrada.</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSurveyById([NotEmptyGuid(ErrorMessage = "Id must not be empty.")] Guid id)
        {
            try
            {
                var survey = await _surveyRepository.GetById(id);

                if (survey is null)
                    return NotFound(new { message = $"Survey with ID {id} not found." });

                var response = MapToSurveyResponse(survey);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal error while searching for a survey with ID {SurveyId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal error occurred while processing your request." });
            }
        }

        /// <summary>
        /// Lista todas as pesquisas com paginação opcional.
        /// </summary>
        /// <param name="pageNumber">Número da página (padrão: 1).</param>
        /// <param name="pageSize">Tamanho da página (padrão: 10).</param>
        /// <returns>Uma lista de pesquisas.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllSurveys([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var surveys = await _surveyRepository.GetAll();
                var response = surveys.Select(MapToSurveyResponse).ToList();
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal error while listing surveys.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal error occurred while processing your request." });
            }
        }

        /// <summary>
        /// Obtém o resultado sumarizado de um pesquisa a partir de seu ID.
        /// </summary>
        /// <param name="id">ID da pesquisa.</param>
        /// <returns>O resultado sumarizado da pesquisa.</returns>
        [HttpGet("{id:guid}/Result")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Result([NotEmptyGuid(ErrorMessage = "Id must not be empty.")] Guid id)
        {
            try
            {
                var survey = await _surveyRepository.GetById(id);

                if (survey is null)
                    return NotFound(new { message = $"Survey with ID {id} not found." });

                var submissions = await _submissionRepository.GetBySurveyId(id);

                var response = new SurveyResultResponse
                {
                    Id = survey.Id,
                    Title = survey.Title,
                    Description = (survey.Description == null ? string.Empty : survey.Description),
                    Status = survey.Status,
                    Period = new SurveyPeriodResponse
                    {
                        StartDate = survey.Period.StartDate,
                        EndDate = survey.Period.EndDate
                    },
                    CreatedAt = survey.CreatedAt,
                    UpdatedAt = survey.UpdatedAt,
                    PublishedAt = survey.PublishedAt,
                    ClosedAt = survey.ClosedAt,
                    Question = survey.Questions.Select(q => new QuestionResultResponse
                    {
                        Text = q.Text,
                        TotalVotes = submissions.Sum(s => s.Answers.Count(a => a.QuestionText == q.Text)),
                        Options = q.Options.Select(o => new OptionResultResponse
                        {
                            Text = o.Text,
                            TotalVotes = submissions.Sum(s => s.Answers.Count(a => a.QuestionText == q.Text && a.OptionText == o.Text)),
                            PercentualVotes = 0
                        }).ToList()
                    }).ToList()
                };

                // Calcular percentual de votos para cada opção 
                foreach (var question in response.Question)
                {
                    if(question.TotalVotes == 0)
                        continue;

                    foreach (var option in question.Options)
                        option.PercentualVotes = (double)option.TotalVotes / question.TotalVotes * 100;
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal error while retrieving survey results with ID {SurveyId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal error occurred while processing your request." });
            }
        }

        /// <summary>
        /// Deleta todas as pesquisas.
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
                var surveys = await _surveyRepository.GetAll();
        
                foreach (var survey in surveys)
                    await _surveyRepository.Delete(survey.Id);
        
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal error while deleting all surveys.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An internal error occurred while processing your request." });
            }
        }
    }
}
