using System;
using Amver.Domain.Models;
using Amver.Api.CustomExceptionMiddleware;
using Microsoft.AspNetCore.Http;

namespace Amver.Api.CustomExceptionMiddleware.SimpleFactory
{
    public static class BaseResultFactory
    {
        public static BaseResult CreateBaseResult(Exception exception, HttpContext context)
        {
            
            switch (exception)
            {
                case Exception ex when ex is UnauthorizedAccessException:
                    return new BaseResultFactoryMethod(exception).CreateUnauthorizedBaseResult(context);
                case Exception ex when ex is UnauthorizedException:
                    return new BaseResultFactoryMethod(exception).CreateUnauthorizedBaseResult(context);
                case Exception ex when ex is ForbiddenException:
                    return new BaseResultFactoryMethod(exception).CreateForbiddenBaseResult(context);
                case Exception ex when ex is BadRequestException:
                    return new BaseResultFactoryMethod(exception).CreateBadRequestBaseResult(context);
                case Exception ex when ex is WebApiException:
                    return new BaseResultFactoryMethod(exception).CreateWebApiBaseResult(context);
                case Exception ex when ex is NotFoundException:
                    return new BaseResultFactoryMethod(exception).CreateNotFoundBaseResult(context);
                case Exception ex when ex is ValidationException:
                    return new BaseResultFactoryMethod(exception).CreateValidationBaseResult(context);

                default:
                    return new BaseResultFactoryMethod(exception).CreateDefaultBaseResult(context);
            }
        }
    }
}