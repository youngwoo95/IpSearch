using IpManager.Comm.Logger.LogFactory;
using IpManager.Comm.Logger.LogFactory.LoggerSelect;
using IpManager.Comm.Tokens;
using IpManager.DBModel;
using IpManager.Repository.DashBoard;
using IpManager.Repository.Login;
using IpManager.Repository.Store;
using IpManager.RunningSet;
using IpManager.Services;
using IpManager.Services.DashBoard;
using IpManager.Services.Login;
using IpManager.Services.Store;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
namespace IpManager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Kestrel ����
            builder.WebHost.UseKestrel((context, options) =>
            {
                options.Configure(context.Configuration.GetSection("Kestrel"));
                // Keep-Alive TimeOut 3�м��� keep-Alive Ÿ�Ӿƿ�: �Ϲ������� 2~5��, �ʹ� ª���� ������ ���� ������ �� �ְ�, �ʹ� ��� ���ҽ��� ����� �� ����.
                options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(3);
                // �ִ� ���� ���׷��̵� ���� ��: �Ϲ������� 1000 ~ 5000 ���̷� �����ϴ� ���� ����
                options.Limits.MaxConcurrentUpgradedConnections = 3000;
                options.Limits.MaxResponseBufferSize = null; // ���� ũ�� ���� ����
                options.ConfigureEndpointDefaults(endpointOptions =>
                {
                    // �������� ����: HTTP/1.1�� HTTP/2�� ��� �����ϴ� ���� ����.
                    // HTTP/2�� ���� ���� ȿ������ ������ ������ ������.
                    endpointOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
                });
            });
            #endregion

            // Add services to the container.

            builder.Services.AddTransient<ILoggerService, LoggerService>();

            builder.Services.AddSingleton<IpanalyzeContext>();
            builder.Services.AddTransient<ITokenComm, TokenComm>();

            // ���α׷� ���۽� ���� �ݿ�
            builder.Services.AddSingleton<RunningsSetting>();

            /* Service DI */
            // DB
            builder.Services.AddTransient<IUserRepository, UserRepository>();
            builder.Services.AddTransient<IStoreRepository, StoreRepository>();
            builder.Services.AddTransient<IDashBoardRepository, DashBoardRepository>();

            // Service
            builder.Services.AddTransient<ILoginService, LoginService>();
            builder.Services.AddTransient<IStoreService, StoreService>();
            builder.Services.AddTransient<IDashBoardService, DashBoardService>();

            /* ��׶��� ���� ��� */
            builder.Services.AddHostedService<BackgroundManager>();
            builder.Services.AddHostedService<StartupTask>();


            /* �޸� ĳ�� ��� */
            builder.Services.AddMemoryCache();

            // JWTToken �⺻ ���� ����
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();


            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:authSigningKey"]!)),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = builder.Configuration["JWT:Issuer"],
                    ValidAudience = builder.Configuration["JWT:Audience"],
                    RoleClaimType = "Role",
                    ClockSkew = TimeSpan.Zero,
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // Authorization ����� "Bearer " ���ξ� ���� �ܼ� ��ū�� ��� ó��
                        var authHeader = context.Request.Headers["Authorization"].ToString();
                        if (!string.IsNullOrEmpty(authHeader) && !authHeader.StartsWith("Bearer "))
                        {
                            context.Token = authHeader;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            // Swagger ���� (JWT ����)
            builder.Services.AddSwaggerGen(options =>
            {
                // JWT Bearer ���� ��Ŵ �߰�
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"�Է¾�� - 'Bearer' [space] and then your token.
                        ��: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'",
                    Name = "Authorization", // HTTP ��� �̸�
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT"
                });
                // ���� ���� �䱸 ���� �߰�
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "Bearer",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });
            });

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();
            
            #region ������ ���Ͻ� ���� ���
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            #endregion

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            /* 
                MIME Ÿ�� �� ���� ��� ����
                �⺻ �������� �ʴ� MIME Ÿ�� �߰�.
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
                    /* ����� ���Ͽ� ���� Content-Encoding ��� ���� */
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
                "/api/Login/sign",
                "/api/Store/sign",
                "/api/DashBoard/sign"
            };
            
            foreach(var path in ApiMiddleWare)
            {
                app.UseWhen(context => context.Request.Path.StartsWithSegments(path), appBuilder =>
                {
                    appBuilder.UseMiddleware<TokenMiddleWare>();
                });
            }

            //app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
