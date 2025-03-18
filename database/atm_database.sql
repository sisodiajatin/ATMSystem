
DROP DATABASE IF EXISTS atm_system;


CREATE DATABASE IF NOT EXISTS atm_system;
USE atm_system;


CREATE TABLE IF NOT EXISTS accounts (
    account_number INT AUTO_INCREMENT PRIMARY KEY,
    holder_name VARCHAR(255) NOT NULL,
    balance DECIMAL(15, 2) NOT NULL DEFAULT 0.00,
    status VARCHAR(50) NOT NULL,
    login VARCHAR(50) NOT NULL UNIQUE,
    pin_code VARCHAR(5) NOT NULL
);


INSERT INTO accounts (holder_name, balance, status, login, pin_code) 
VALUES ('Adnan123', 154500.00, 'Active', 'Adnan123', '12345');

INSERT INTO accounts (holder_name, balance, status, login, pin_code) 
VALUES ('Admin', 0.00, 'Admin', 'Javed123', '12345');

INSERT INTO accounts (holder_name, balance, status, login, pin_code) 
VALUES ('John Doe', 50000.00, 'Disabled', 'dotNet66', '45678');