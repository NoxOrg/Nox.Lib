﻿using Microsoft.EntityFrameworkCore;
using Nox;
using Nox.Core.Interfaces.Entity.Commands;
using Nox.Dto;

namespace Samples.Api.Commands
{
    public class ReserveCommandHandler : Nox.Commands.ReserveCommandHandlerBase
    {
        public ReserveCommandHandler(NoxDbContext dbContext)
            : base(dbContext)
        {
        }

        public override async Task<INoxCommandResult> ExecuteAsync(ReserveDto reserveCommandDto)
        {
            // DTO validation

            // Check balance - aggregate validation

            var store = await DbContext
                .Store
                .FirstOrDefaultAsync(s => s.Id == reserveCommandDto.StoreId);

            if (store == null)
            {
                return new NoxCommandResult { IsSuccess = false, Message = "Store cannot be found" };
            }

            store.Reservations.Add(
                new Reservation
                {
                    IsActive = true,
                    SourceAmount = reserveCommandDto.SourceAmount,
                });

            await DbContext.SaveChangesAsync();

            // emit event

            return new NoxCommandResult { IsSuccess = true };
        }
    }
}
