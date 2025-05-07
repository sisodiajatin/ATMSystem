using System;

namespace ATMSystem.Models
{
    public class Account
    {
        private int _accountNumber;
        private string _holderName;
        private decimal _balance;
        private string _status;
        private string _login;
        private string _pinCode;

        public Account(int accountNumber, string holderName, decimal balance, string status, string login, string pinCode)
        {
            _accountNumber = accountNumber;
            _holderName = holderName ?? throw new ArgumentNullException(nameof(holderName));
            _balance = balance;
            _status = status ?? "Active";
            _login = login ?? throw new ArgumentNullException(nameof(login));
            _pinCode = pinCode ?? throw new ArgumentNullException(nameof(pinCode));
        }

        public int GetAccountNumber() => _accountNumber;
        public void SetAccountNumber(int accountNumber) => _accountNumber = accountNumber;
        public string GetHolderName() => _holderName;
        public decimal GetBalance() => _balance;
        public void SetBalance(decimal balance) => _balance = balance >= 0 ? balance : throw new ArgumentException("Balance cannot be negative");
        public string GetStatus() => _status;
        public void SetStatus(string status) => _status = status ?? "Active";
        public string GetLogin() => _login;
        public string GetPinCode() => _pinCode;
    }
}