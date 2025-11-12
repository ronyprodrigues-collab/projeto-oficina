using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Models;

namespace Data
{
    public static class SeedData
    {
        public static async Task SeedRolesAndUsers(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var roles = new[] { "Admin", "Supervisor", "Mecanico", "SuporteTecnico" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole(role));
                    if (!result.Succeeded)
                    {
                        Console.WriteLine($"Falha ao criar papel '{role}': {string.Join(", ", GetErrors(result))}");
                    }
                }
            }

            // Defina a senha padrão (substitua em produção).
            var defaultPassword = Environment.GetEnvironmentVariable("SEED_DEFAULT_PASSWORD") ?? "P@ssw0rd!";

            var usersToSeed = new (string Email, string Role, string Nome, string Cargo)[]
            {
                ("admin@oficina.local",     "Admin",          "Administrador", "Admin"),
                ("supervisor@oficina.local", "Supervisor",      "Supervisor",    "Supervisor"),
                ("mecanico@oficina.local",   "Mecanico",        "Mecânico",      "Mecanico"),
                ("suporte@oficina.local",    "SuporteTecnico",  "Suporte Técnico","SuporteTecnico"),
            };

            foreach (var (email, role, nome, cargo) in usersToSeed)
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                        NomeCompleto = nome,
                        Cargo = cargo
                    };
                    var createResult = await userManager.CreateAsync(user, defaultPassword);
                    if (!createResult.Succeeded)
                    {
                        Console.WriteLine($"Falha ao criar usuário '{email}': {string.Join(", ", GetErrors(createResult))}");
                        continue;
                    }
                }

                if (!await userManager.IsInRoleAsync(user, role))
                {
                    var addRoleResult = await userManager.AddToRoleAsync(user, role);
                    if (!addRoleResult.Succeeded)
                    {
                        Console.WriteLine($"Falha ao atribuir papel '{role}' ao usuário '{email}': {string.Join(", ", GetErrors(addRoleResult))}");
                    }
                }
            }
        }

        private static IEnumerable<string> GetErrors(IdentityResult result)
        {
            foreach (var e in result.Errors)
            {
                yield return $"{e.Code}:{e.Description}";
            }
        }
    }
}

