using Microsoft.AspNetCore.Mvc;
using SurveySystem.Api.Dtos.Surveys;
using SurveySystem.Api.Dtos.Surveys.Requests;
using SurveySystem.Api.Dtos.Surveys.Responses;
using SurveySystem.Domain.Repositories;
using SurveySystem.Domain.Shared;
using SurveySystem.Domain.Surveys;

namespace SurveySystem.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SurveyController : ControllerBase
    {
        private readonly ISurveyRepository _surveyRepository;
        private readonly ILogger<SurveyController> _logger;

        public SurveyController(ISurveyRepository surveyRepository, ILogger<SurveyController> logger)
        {
            _surveyRepository = surveyRepository;
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
                _logger.LogWarning(ex, "Erro de domínio ao criar pesquisa: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao criar pesquisa.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro interno ao processar sua requisição." });
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
        public async Task<IActionResult> UpdateSurvey(Guid id, [FromBody] UpdateSurveyRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var survey = await _surveyRepository.GetById(id);

                if (survey is null)
                    return NotFound(new { message = $"Pesquisa com ID {id} não encontrada." });

                survey.UpdateDetails(request.Title, request.Description, request.SurveyPeriod.StartDate, request.SurveyPeriod.EndDate);

                await _surveyRepository.Update(survey);

                var response = MapToSurveyResponse(survey);
                return Ok(response);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Erro de domínio ao atualizar pesquisa {SurveyId}: {Message}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao atualizar pesquisa {SurveyId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro interno ao processar sua requisição." });
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
        public async Task<IActionResult> DeleteSurvey(Guid id)
        {
            try
            {
                var survey = await _surveyRepository.GetById(id);

                if (survey is null)
                    return NotFound(new { message = $"Pesquisa com ID {id} não encontrada." });

                await _surveyRepository.Delete(survey.Id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao deletar pesquisa {SurveyId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro interno ao processar sua requisição." });
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
        public async Task<IActionResult> AddQuestion(Guid id, [FromBody] AddQuestionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var survey = await _surveyRepository.GetById(id);
                if (survey is null)
                    return NotFound(new { message = $"Pesquisa com ID {id} não encontrada." });

                survey.AddQuestion(request.Text, request.Options);

                await _surveyRepository.Update(survey);

                var response = MapToSurveyResponse(survey);

                return Ok(response);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Erro de domínio ao adicionar questão a pesquisa {SurveyId}: {Message}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao adicionar questão a pesquisa {SurveyId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro interno ao processar sua requisição." });
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
        public async Task<IActionResult> RemoveQuestion(Guid id, int questionIndex)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var survey = await _surveyRepository.GetById(id);
                if (survey is null)
                    return NotFound(new { message = $"Pesquisa com ID {id} não encontrada." });

                survey.RemoveQuestion(questionIndex);

                await _surveyRepository.Update(survey);

                var response = MapToSurveyResponse(survey);

                return Ok(response);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Erro de domínio ao deletar questão da pesquisa {SurveyId}: {Message}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao deletar questão da pesquisa {SurveyId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro interno ao processar sua requisição." });
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
        public async Task<IActionResult> Publish(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var survey = await _surveyRepository.GetById(id);

                if (survey is null)
                    return NotFound(new { message = $"Pesquisa com ID {id} não encontrada." });

                survey.Publish();

                await _surveyRepository.Update(survey);

                var response = MapToSurveyResponse(survey);

                return Ok(response);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Erro de domínio ao publicar a pesquisa {SurveyId}: {Message}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao publicar a pesquisa {SurveyId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro interno ao processar sua requisição." });
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
        public async Task<IActionResult> Close(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var survey = await _surveyRepository.GetById(id);

                if (survey is null)
                    return NotFound(new { message = $"Pesquisa com ID {id} não encontrada." });

                survey.Close();

                await _surveyRepository.Update(survey);

                var response = MapToSurveyResponse(survey);

                return Ok(response);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Erro de domínio ao encerrar a pesquisa {SurveyId}: {Message}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao encerrar a pesquisa {SurveyId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro interno ao processar sua requisição." });
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
        public async Task<IActionResult> GetSurveyById(Guid id)
        {
            try
            {
                var survey = await _surveyRepository.GetById(id);

                if (survey is null)
                    return NotFound(new { message = $"Pesquisa com ID {id} não encontrada." });

                var response = MapToSurveyResponse(survey);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao buscar pesquisa com ID {SurveyId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro interno ao processar sua requisição." });
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
                _logger.LogError(ex, "Erro interno ao listar pesquisas.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro interno ao processar sua requisição." });
            }
        }
    }
}
