using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravianTelegramBot.Settings;
using TTB.DAL.Models;
using TTB.DAL.Repository;

namespace TravianTelegramBot.Providers.Impl
{
    public class SecretProvider : ISecretProvider
    {
        private readonly IOptionsSnapshot<SecretSettings> secretSettings;
        private readonly ISecretRepository secretRepository;

        public SecretProvider(
            IOptionsSnapshot<SecretSettings> secretSettings,
            ISecretRepository secretRepository)
        {
            this.secretSettings = secretSettings;
            this.secretRepository = secretRepository;
        }

        public async Task<SecretModel> CheckSecretCode(string code)
        {
            var secrets = new List<SecretModel> {
                new SecretModel
                {
                    Name = "default",
                    Value = secretSettings.Value.RegistrationDefaultSecret,
                    Expiry = DateTime.MaxValue,
                    AdminPrivileges = false
                }, new SecretModel
                {
                    Name = "admin-default",
                    Value = secretSettings.Value.AdminDefaultSecret,
                    Expiry = DateTime.MaxValue,
                    AdminPrivileges = true
                }
            };

            var additionalSecret = await this.secretRepository.GetByValue(code);
            if (additionalSecret != null)
            {
                secrets.Add(additionalSecret);
            }

            return secrets.Where(x => x.Expiry > DateTime.Now && x.Value == code).FirstOrDefault();
        }
    }
}
