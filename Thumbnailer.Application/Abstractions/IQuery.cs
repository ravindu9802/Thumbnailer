using MediatR;

namespace Thumbnailer.Application.Abstractions;

public interface IQuery<out TResponse> : IRequest<TResponse>;