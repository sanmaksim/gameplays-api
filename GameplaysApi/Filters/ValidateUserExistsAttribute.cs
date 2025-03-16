using GameplaysApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace GameplaysApi.Filters
{
    public class ValidateUserExistsAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // try to retrieve and cast the userId to an int
            if (context.ActionArguments.TryGetValue("userId", out var userIdObj) && userIdObj is int userId)
            {
                var dbContext = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();

                var userExists = await dbContext.Users.AnyAsync(u => u.Id == userId);
                if (!userExists)
                {
                    context.Result = new NotFoundObjectResult(new { message = "User not found." });
                    return;
                }

                await next();
            }
            else
            {
                context.Result = new BadRequestObjectResult(new { message = "Invalid userId." });
                return;
            }
        }
    }
}
