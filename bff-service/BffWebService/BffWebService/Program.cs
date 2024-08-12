
using BffWebService.Client.Abstract;
using BffWebService.Client;

namespace BffWebService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                //options.AddPolicy("AllowAll", policy =>
                //{
                //    policy.AllowAnyOrigin()
                //          .AllowAnyMethod()
                //          .AllowAnyHeader();
                //});
                //options.AddDefaultPolicy(builder =>
                //{
                //    builder.AllowAnyOrigin()
                //           .AllowAnyMethod()
                //           .AllowAnyHeader();
                //});
                options.AddPolicy("AllowSpecificOrigins", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            builder.Services.AddHttpClient<IApiClient, ApiClient>()
               .SetHandlerLifetime(TimeSpan.FromMinutes(10));

            // Add services to the container.

            builder.Services.AddControllers();
 
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();
            app.UseCors("AllowSpecificOrigins");
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
