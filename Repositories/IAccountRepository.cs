#nullable enable

using ATMSystem.Models;

namespace ATMSystem.Repositories
{
    public interface IAccountRepository
    {
        void Save(Account account);
        bool Delete(int accountNumber);
        void Update(Account account);
        Account? FindByNumber(int accountNumber);
        Account? FindByLogin(string login);
    }
}