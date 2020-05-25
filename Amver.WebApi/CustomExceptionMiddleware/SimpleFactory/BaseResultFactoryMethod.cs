using System;
using System.Net;
using Amver.Domain.Models;
using Microsoft.AspNetCore.Http;

namespace Amver.WebAPI.CustomExceptionMiddleware.SimpleFactory
{
    public class BaseResultFactoryMethod
    {
        private readonly Exception _exception;

        public BaseResultFactoryMethod(Exception exception)
        {
            _exception = exception;
        }

        public BaseResult CreateUnauthorizedBaseResult(HttpContext context)
        {
            context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
            return new BaseResult
            {
                Result = ((int) HttpStatusCode.Unauthorized).ToString(),
                Message = _exception.Message
            };
        }

        public BaseResult CreateForbiddenBaseResult(HttpContext context)
        {
            context.Response.StatusCode = (int) HttpStatusCode.Forbidden;
            return new BaseResult
            {
                Result = ((int) HttpStatusCode.Forbidden).ToString(),
                Message = _exception.Message
            };
        }

        public BaseResult CreateBadRequestBaseResult(HttpContext context)
        {
            context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
            return new BaseResult
            {
                Result = ((int) HttpStatusCode.BadRequest).ToString(),
                Message = _exception.Message
            };
        }

        public BaseResult CreateWebApiBaseResult(HttpContext context)
        {
            context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
            return new BaseResult
            {
                Result = ((int) HttpStatusCode.InternalServerError).ToString(),
                Message = _exception.Message
            };
        }

        public BaseResult CreateDefaultBaseResult(HttpContext context)
        {
            context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
            return new BaseResult
            {
                Result = ((int) HttpStatusCode.InternalServerError).ToString(),
                Message = _exception.Message
            };
        }

        public BaseResult CreateNotFoundBaseResult(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            return new BaseResult
            {
                Result = ((int)HttpStatusCode.NotFound).ToString(),
                Message = _exception.Message
            };
        }
        
        public BaseResult CreateValidationBaseResult(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotAcceptable;
            return new BaseResult
            {
                Result = ((int)HttpStatusCode.NotAcceptable).ToString(),
                Message = _exception.Message
            };
        }
    }
}