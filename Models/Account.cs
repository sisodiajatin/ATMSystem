namespace ATMSystem.Models
{
    public class Account
    {
        private readonly int _accountNumber;
        private readonly string _holderName;
        private readonly decimal _balance;
        private readonly string _status;
        private readonly string _login;
        private readonly string _pinCode;

        public Account(int accountNumber, string holderName, decimal balance,
            string status, string login, string pinCode)
        {
            _accountNumber = accountNumber;
            _holderName = holderName;
            _balance = balance;
            _status = status;
            _login = login;
            _pinCode = pinCode;
        }

        public int GetAccountNumber() => _accountNumber;
        public string GetHolderName() => _holderName;
        public decimal GetBalance() => _balance;
        public string GetStatus() => _status;
        public string GetLogin() => _login;
        public bool VerifyPin(string pin) => _pinCode == pin;

        
        internal string GetPinCode() => _pinCode;

        public Account Deposit(decimal amount) =>
            new Account(_accountNumber, _holderName, _balance + amount, _status, _login, _pinCode);

        public Account Withdraw(decimal amount)
        {
            if (amount <= 0 || amount > _balance) throw new ArgumentException("Invalid withdrawal amount");
            return new Account(_accountNumber, _holderName, _balance - amount, _status, _login, _pinCode);
        }
    }
}