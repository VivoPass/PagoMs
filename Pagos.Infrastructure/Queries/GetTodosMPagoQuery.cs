using MediatR;
using Pagos.Application.DTOs;

namespace Pagos.Infrastructure.Queries
{
    public class GetTodosMPagoQuery : IRequest<List<MPagoDTO>>
    {}
}
