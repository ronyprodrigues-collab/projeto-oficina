using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Models;
using Services;

namespace Filters
{
    public class PlanoRecursoAttribute : TypeFilterAttribute
    {
        public PlanoRecursoAttribute(PlanoConta planoNecessario, string moduloDescricao)
            : base(typeof(PlanoRecursoFilter))
        {
            Arguments = new object[] { planoNecessario, moduloDescricao };
        }

        private class PlanoRecursoFilter : IAsyncActionFilter
        {
            private readonly PlanoConta _planoNecessario;
            private readonly string _moduloDescricao;
            private readonly IConfiguracoesService _configService;

            public PlanoRecursoFilter(PlanoConta planoNecessario, string moduloDescricao, IConfiguracoesService configService)
            {
                _planoNecessario = planoNecessario;
                _moduloDescricao = moduloDescricao;
                _configService = configService;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                var cfg = await _configService.GetAsync(context.HttpContext.RequestAborted);
                if (cfg.PlanoAtual < _planoNecessario)
                {
                    var metadataProvider = context.HttpContext.RequestServices.GetRequiredService<IModelMetadataProvider>();
                    var viewData = new ViewDataDictionary(metadataProvider, context.ModelState)
                    {
                        Model = new Models.ViewModels.PlanoRestritoViewModel
                        {
                            Modulo = _moduloDescricao,
                            PlanoNecessario = _planoNecessario,
                            PlanoAtual = cfg.PlanoAtual
                        }
                    };

                    context.Result = new ViewResult
                    {
                        ViewName = "~/Views/Shared/PlanoRestrito.cshtml",
                        ViewData = viewData
                    };
                    return;
                }

                await next();
            }
        }
    }
}
