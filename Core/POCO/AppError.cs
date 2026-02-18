using System;

namespace Core.POCO;

public class AppError(int statusCode,string message) // ← juste une classe, rien en base
{
  

    public int StatusCode { get; set; } = statusCode;
    public string Message { get; set; } = message;
 
}