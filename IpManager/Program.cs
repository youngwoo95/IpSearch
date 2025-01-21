
using IpManager.Comm.Logger;
using Microsoft.AspNetCore.StaticFiles;

using ILoggerFactory = IpManager.Comm.Logger.ILoggerFactory;


namespace IpManager
{
    
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            // 로깅 전략 등록
            builder.Services.AddSingleton<ILoggerFactory, CustomLoggerFactory>();
            builder.Services.AddTransient<ConsoleLogger>();
            builder.Services.AddTransient<FileLogger>(_ => new FileLogger("logs.txt"));

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            /* 
                MIME 타입 및 압축 헤더 설정
                기본 제공되지 않는 MIME 타입 추가.
             */
            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = new FileExtensionContentTypeProvider
                {
                    Mappings =
                    {
                        [".wasm"] = "application/wasm",
                        [".gz"] = "application/octet-stream",
                        [".br"] = "application/octet-stream",
                        [".jpg"] = "image/jpg",
                        [".jpeg"] ="image/jpeg",
                        [".png"] = "image/png",
                        [".gif"] = "image/gif",
                        [".webp"] = "image/webp",
                        [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        [".pdf"] = "application/pdf"
                    }
                },
                OnPrepareResponse = ctx =>
                {
                    /* 압축된 파일에 대한 Content-Encoding 헤더 설정 */
                    if (ctx.File.Name.EndsWith(".gz"))
                    {
                        ctx.Context.Response.Headers["Content-Encoding"] = "gzip";
                    }
                    else if (ctx.File.Name.EndsWith(".br"))
                    {
                        ctx.Context.Response.Headers["Content-Encoding"] = "br";
                    }
                }
            });

            string[]? ApiMiddleWare = new string[]
            {
                "/api/v1/Login/sign"
            };
            
            foreach(var path in ApiMiddleWare)
            {
                /*
                app.UseWhen(context => context.Request.Path.StartsWithSegments(path), appBuilder =>
                {
                    appBuilder.UseMiddleware<LoginMiddleWare>();
                });
                */
            }

            //app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
