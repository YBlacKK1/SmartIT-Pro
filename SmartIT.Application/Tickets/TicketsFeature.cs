using FluentValidation;
using MediatR;
using SmartIT.Domain;

namespace SmartIT.Application.Tickets;

public interface ITicketRepository
{
    Task<IReadOnlyList<Ticket>> ListAsync(CancellationToken cancellationToken);
    Task<Ticket?> GetReadOnlyAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> RequesterExistsAsync(Guid requesterId, CancellationToken cancellationToken);
    Task<string> GenerateNextNumberAsync(CancellationToken cancellationToken);
    Task AddAsync(Ticket ticket, CancellationToken cancellationToken);
}

public sealed record TicketListItem(
    Guid Id,
    string Number,
    string Subject,
    string Description,
    TicketPriority Priority,
    TicketStatus Status,
    Guid? RequesterId,
    DateTime CreatedAt);

public sealed record GetTicketsQuery : IRequest<IReadOnlyList<TicketListItem>>;
public sealed record GetTicketByIdQuery(Guid Id) : IRequest<TicketListItem?>;
public sealed record CreateTicketCommand(
    string Subject,
    string Description,
    TicketPriority Priority,
    Guid? RequesterId) : IRequest<TicketListItem>;

public static class TicketMappings
{
    public static TicketListItem ToListItem(this Ticket ticket) => new(
        ticket.Id, ticket.Number, ticket.Subject, ticket.Description,
        ticket.Priority, ticket.Status, ticket.RequesterId, ticket.CreatedAt);
}

public sealed class GetTicketsHandler(ITicketRepository repository)
    : IRequestHandler<GetTicketsQuery, IReadOnlyList<TicketListItem>>
{
    public async Task<IReadOnlyList<TicketListItem>> Handle(GetTicketsQuery request, CancellationToken cancellationToken) =>
        (await repository.ListAsync(cancellationToken)).Select(ticket => ticket.ToListItem()).ToArray();
}

public sealed class GetTicketByIdHandler(ITicketRepository repository)
    : IRequestHandler<GetTicketByIdQuery, TicketListItem?>
{
    public async Task<TicketListItem?> Handle(GetTicketByIdQuery request, CancellationToken cancellationToken) =>
        (await repository.GetReadOnlyAsync(request.Id, cancellationToken))?.ToListItem();
}

public sealed class CreateTicketHandler(ITicketRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateTicketCommand, TicketListItem>
{
    public async Task<TicketListItem> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = new Ticket
        {
            Number = await repository.GenerateNextNumberAsync(cancellationToken),
            Subject = request.Subject.Trim(),
            Description = request.Description.Trim(),
            Priority = request.Priority,
            Status = TicketStatus.Open,
            RequesterId = request.RequesterId
        };

        await repository.AddAsync(ticket, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ticket.ToListItem();
    }
}

public sealed class CreateTicketCommandValidator : AbstractValidator<CreateTicketCommand>
{
    public CreateTicketCommandValidator(ITicketRepository repository)
    {
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Priority).IsInEnum();
        RuleFor(x => x.RequesterId)
            .MustAsync(async (requesterId, ct) => requesterId is null || await repository.RequesterExistsAsync(requesterId.Value, ct))
            .WithMessage("Requester does not exist.");
    }
}
