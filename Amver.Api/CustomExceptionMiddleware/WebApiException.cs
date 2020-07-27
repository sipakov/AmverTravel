﻿using System;

namespace Amver.Api.CustomExceptionMiddleware
{
    public class WebApiException : Exception
    {
        public WebApiException(string message) : base(message)
        {
        }
    }

    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message) : base(message)
        {
        }
    }

    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message)
        {
        }
    }

    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message)
        {
        }
    }

    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message)
        {
        }
    }
    
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message)
        {
        }
    }
}