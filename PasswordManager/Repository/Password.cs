using System;
using System.Security;

namespace PasswordManager.Repository
{
    public class Password
    {
        public Password()
        {
            Id = Guid.NewGuid().ToString();
            SecurePassword = new SecureString();
            Name = string.Empty;
            Login = string.Empty;
            Url = string.Empty;
            Description = string.Empty;
        }

        public Password(Password pwd)
        {
            Id = pwd.Id;
            Name = pwd.Name;
            Login = pwd.Login;
            Url = pwd.Url;
            Description = pwd.Description;
            SecurePassword = pwd.SecurePassword;
            SecurePassword.MakeReadOnly();
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public string Login { get; set; }

        public SecureString SecurePassword { get; set; }

        public string Url { get; set; }

        public string Description { get; set; }
    }
}
