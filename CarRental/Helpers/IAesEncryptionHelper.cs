namespace CarRental.Helpers;

public interface IAesEncryptionHelper
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}