
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net.Mime;
using System.Reflection.Metadata.Ecma335;
using TodoApi.Models;
using TodoApi.Repositories;

namespace TodoApi.Controllers
{
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly ITodoRepository todoRepository;
        public TodoController(ITodoRepository todoRepository)
            => this.todoRepository = todoRepository;


        /// <summary>
        /// Endpoint para retornar todas as Tarefas (Todos).
        /// </summary>
        /// <returns>
        /// Retorna uma lista com todas as tarefas cadastradas no banco de dados.
        /// </returns>
        /// <response code="200">A Requisição foi bem-sucedida e a resposta contém a lista de tarefas</response>
        [HttpGet("/api/todo")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Todo>))]
        public IActionResult GetAllTodos()
        {
            return Ok(todoRepository.GetAll());
        }

        /// <summary>
        /// Endpoint para consultar uma Tarefa (Todo) pelo ID.
        /// </summary>
        /// <returns>
        /// Retorna a tarefa, caso ela exista, ou 404 caso ela não tenha sido encontrada.
        /// </returns>
        /// <param name="id">O ID da tarefa a ser consultada</param>
        /// <response code="200">A tarefa foi encontrada e seu conteúdo está disponível na resposta</response>
        /// <response code="404">A tarefa não foi encontrada</response>
        /// <response code="500">Teste para quando o id é 2</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Todo))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
        [HttpGet("/api/todo/{id}")]
        public IActionResult GetTodoById(int id)
        {
            var todo = todoRepository.GetById(id);
            if (todo == null)
                return NotFound();

            return Ok(todo);
        }

        /// <summary>
        /// Endpoint para criar uma Tarefa (Todo).
        /// </summary>
        /// <returns>
        /// Retorna 409 (Conflict) caso uma tarefa com o ID especificado já exista, 
        /// ou 201 caso a criação tenha sido bem-sucedida.
        /// </returns>
        /// <param name="todo">Os dados da tarefa a ser criada</param>
        /// <response code="201">A tarefa foi criada com sucesso</response>
        /// <response code="409">A tarefa não pode ser criada porque já existe outra tarefa com o mesmo ID</response>
        [HttpPost("/api/todo")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Todo))]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult CreateTodo([FromBody] Todo todo)
        {
            var existingTodo = todoRepository.GetById(todo.Id);
            if (existingTodo != null)
                return Conflict(existingTodo);

            todo = todoRepository.Create(todo);

            var url = Url.Action(
                   action: nameof(GetTodoById),
                   controller: "Todo",
                   values: new { id = todo.Id },
                   protocol: Request.Scheme);
            
            return Created(url, todo);
            //return CreatedAtAction(nameof(GetTodoById), new { id = todo.Id }, todo);
        }

        /// <summary>
        /// Endpoint para atualizar uma Tarefa (Todo).
        /// </summary>
        /// <returns>
        /// Retorna 400 caso os ids sejam divergentes, 404 caso a tarefa com o id especificado não tenha sido encontrada
        /// ou 200 caso a tarefa tenha sido encontrada e atualizada com sucesso.
        /// </returns>
        /// <param name="todo">Os dados da tarefa a ser atualizada</param>
        /// <param name="id">Os ID da tarefa a ser atualizada</param>
        /// <response code="400">Os ids informados são divergentes</response>
        /// <response code="404">A tarefa não foi encontrada</response>
        /// <response code="200">A tarefa foi atualizada com sucesso</response>
        [HttpPut("/api/todo/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Todo))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateTodo([FromBody] Todo todo, int id)
        {
            if (id != todo.Id)
                return BadRequest();

            var existingTodo = todoRepository.GetById(todo.Id);
            if (existingTodo == null)
                return NotFound();

            todoRepository.Update(existingTodo, todo);
            return Ok(existingTodo);
        }

        /// <summary>
        /// Endpoint para remover uma Tarefa (Todo).
        /// </summary>
        /// <returns>
        /// Retorna 404 caso a tarefa com o id especificado não tenha sido encontrada
        /// ou 204 caso a tarefa tenha sido removida com sucesso.
        /// </returns>
        /// <param name="id">Os ID da tarefa a ser removida</param>
        /// <response code="404">A tarefa não foi encontrada</response>
        /// <response code="204">A tarefa foi removida com sucesso</response>
        [HttpDelete("/api/todo/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(int id)
        {
            var existingTodo = todoRepository.GetById(id);
            if (existingTodo == null)
                return NotFound();

            todoRepository.Delete(id);
            return NoContent();
        }
    }
}
