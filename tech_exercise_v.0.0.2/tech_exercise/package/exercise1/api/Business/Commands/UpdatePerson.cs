﻿using Dapper;
using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Commands
{
    public class UpdatePerson : IRequest<UpdatePersonResult>
    {
        public required string CurrentName { get; set; } = string.Empty;
        public required string NewName { get; set; } = string.Empty;

    }

    public class UpdatePersonPreProcessor : IRequestPreProcessor<UpdatePerson>
    {
        private readonly StargateContext _context;
        public UpdatePersonPreProcessor(StargateContext context)
        {
            _context = context;
        }

        /// Process method executes before calling the Handle method on your handler
        public Task Process(UpdatePerson request, CancellationToken cancellationToken) 
        {
            var currentName = _context.People.AsNoTracking().SingleOrDefault(z => z.Name.ToUpper() == request.CurrentName.ToUpper());

            if (currentName is null) { throw new BadHttpRequestException($"{request.CurrentName} does not exist"); }

            var newName = _context.People.AsNoTracking().SingleOrDefault(z => z.Name.ToUpper() == request.NewName.ToUpper());
            if (newName is not null) { throw new BadHttpRequestException($"{request.NewName} already exists"); }

            return Task.CompletedTask;
        }
    }

    public class UpdatePersonHandler : IRequestHandler<UpdatePerson, UpdatePersonResult>
    {
        private readonly StargateContext _context;

        public UpdatePersonHandler(StargateContext context)
        {
            _context = context;
        }
        public async Task<UpdatePersonResult> Handle(UpdatePerson request, CancellationToken cancellationToken)
        {
            var query = $"SELECT * FROM [Person] WHERE \'{request.CurrentName.ToUpper()}\' = upper(Name)";

            var existingPerson = await _context.Connection.QuerySingleOrDefaultAsync<Person>(query);

            if (existingPerson == default)
            {
                return new UpdatePersonResult()
                {
                    Success = false,
                    Message = $"{request.CurrentName} does not exist",
                };
            }

            _context.Attach(existingPerson);
            existingPerson.Name = request.NewName;

            _context.People.Update(existingPerson);

            await _context.SaveChangesAsync();

            return new UpdatePersonResult()
            {
                Id = existingPerson.Id
            };
        }
    }

    public class UpdatePersonResult : BaseResponse
    {
        public int Id { get; set; }
    }
}
