﻿using FZ.Auth.ApplicationService.Repository;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Auth.Infrastructure.Repository
{
    public sealed class PasswordHasher : IPasswordHasher
    {
        private readonly PasswordHasher<string> _inner = new();

        public string Hash(string password) => _inner.HashPassword(null!, password);

        public bool Verify(string password, string hash)
        {
            var result = _inner.VerifyHashedPassword(null!, hash, password);
            return result != PasswordVerificationResult.Failed;
        }
    }

}
