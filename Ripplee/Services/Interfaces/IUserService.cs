using Ripplee.Models;
public interface IUserService
{
    UserModel CurrentUser { get; }
    Task LoadUserAsync(); 
}