using MediatR;

namespace Thumbnailer.Application.Abstractions;

public interface ICommand : IRequest;

public interface ICommand<out TResponse> : IRequest<TResponse>;