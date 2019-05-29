using System;
using System.Threading.Tasks;
using Magnetic.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Magnetic.API.Data
{
    public class AuthRepository : IAuthRepository
    {
        // Dependency
        private readonly DataContext _context;

        // Constructor
        public AuthRepository(DataContext context)
        {
            this._context = context;
        }

        // Implementations
        public async Task<User> Login(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);
            if (user == null)
                return null;
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;
            return user;
        }



        public async Task<User> Register(User user, string password)
        {
            byte[] passwordHash, passwordSalt;
            // use 'out' to pass in reference
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<bool> UserExists(string username)
        {
            if (await _context.Users.AnyAsync(x => x.Username == username))
                return true;
            return false;
        }

        // Convinient Method
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            // HMACSHA512 is IDisposable, hence will be disposed after using
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            };
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

                for (int i = 0; i < computedHash.Length; i++)
                    if (passwordHash[i] != computedHash[i]) return false;
                return true;
                
                // * cannot use '==' to compare two reference types
                // return passwordHash == computedHash ? true : false;
            }
        }
    }
}