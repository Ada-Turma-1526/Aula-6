using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using TodoApi.Models;

namespace TodoApi.Repositories
{
    public class TodoRepository : ITodoRepository
    {
        private readonly TodoDbContext dbContext;
        public TodoRepository(TodoDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IEnumerable<Todo> GetAll()
        {
            return dbContext.Todos;
        }

        public Todo GetById(int id)
        {
            if (id == 2)
                throw new System.Exception("O ID era 2 :P");

            //first or default
            return dbContext.Todos.Find(id);
        }

        public Todo Create(Todo todo)
        {
            dbContext.Todos.Add(todo);
            dbContext.SaveChanges();

            return todo;
        }

        public bool Update(Todo existingTodo, Todo newTodo)
        {
            existingTodo.Title = newTodo.Title;
            existingTodo.Description = newTodo.Description;
            existingTodo.IsCompleted = newTodo.IsCompleted;

            dbContext.Todos.Update(existingTodo);
            dbContext.SaveChanges();

            return true;
        }

        public bool Delete(int id)
        {
            var todo = dbContext.Todos.Find(id);
            if (todo != null)
            {
                dbContext.Todos.Remove(todo);
                dbContext.SaveChanges();
            }

            return true;
        }
    }
}
