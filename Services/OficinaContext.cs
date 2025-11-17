using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Services
{
    public class OficinaContext : IOficinaContext
    {
        private const string SessionKeyId = "OficinaAtualId";
        private const string SessionKeyNome = "OficinaAtualNome";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly OficinaDbContext _db;

        public OficinaContext(IHttpContextAccessor httpContextAccessor, OficinaDbContext db)
        {
            _httpContextAccessor = httpContextAccessor;
            _db = db;
        }

        public int? OficinaIdAtual => GetSession()?.GetInt32(SessionKeyId);
        public string? NomeOficinaAtual => GetSession()?.GetString(SessionKeyNome);

        public async Task<Oficina?> GetOficinaAtualAsync(CancellationToken cancellationToken = default)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return null;

            if (httpContext.Items.TryGetValue("OficinaAtual", out var cached) && cached is Oficina cachedOficina)
            {
                return cachedOficina;
            }

            var id = OficinaIdAtual;
            if (id == null) return null;

            var oficina = await _db.Oficinas
                .IgnoreQueryFilters()
                .Include(o => o.Grupo)
                .FirstOrDefaultAsync(o => o.Id == id.Value && !o.IsDeleted, cancellationToken);

            if (oficina != null)
            {
                httpContext.Items["OficinaAtual"] = oficina;
            }

            return oficina;
        }

        public async Task<bool> SetOficinaAtualAsync(int oficinaId, CancellationToken cancellationToken = default)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null || httpContext.User?.Identity?.IsAuthenticated != true)
                return false;

            var userId = httpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId)) return false;

            var isSuporte = httpContext.User.IsInRole("SuporteTecnico");

            var oficina = await _db.Oficinas
                .Include(o => o.Grupo)
                .FirstOrDefaultAsync(o => o.Id == oficinaId && !o.IsDeleted, cancellationToken);

            if (oficina == null) return false;

            if (!isSuporte)
            {
                var possuiVinculo = await _db.OficinasUsuarios.AnyAsync(ou => ou.OficinaId == oficinaId && ou.UsuarioId == userId, cancellationToken);
                if (!possuiVinculo)
                {
                    return false;
                }
            }

            var session = GetSession();
            if (session == null) return false;
            session.SetInt32(SessionKeyId, oficina.Id);
            session.SetString(SessionKeyNome, oficina.Nome);
            httpContext.Items["OficinaAtual"] = oficina;
            return true;
        }

        public void Clear()
        {
            var session = GetSession();
            session?.Remove(SessionKeyId);
            session?.Remove(SessionKeyNome);
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                httpContext.Items.Remove("OficinaAtual");
            }
        }

        private ISession? GetSession() => _httpContextAccessor.HttpContext?.Session;
    }
}
