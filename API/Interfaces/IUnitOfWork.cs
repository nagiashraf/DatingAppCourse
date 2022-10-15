namespace API.Interfaces;

public interface IUnitOfWork
{
    ILikeRepository LikeRepository { get; }
    IMessageRepository MessageRepository { get; }
    IUserRepository UserRepository { get; }
    Task<bool> Complete();
    bool HasChanges();
}