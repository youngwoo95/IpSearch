using IpManager.Comm.Logger.LogFactory.LoggerSelect;
using IpManager.Comm.Tokens;
using IpManager.DBModel;
using IpManager.Repository.Country;
using IpManager.Repository.DashBoard;
using IpManager.Repository.Login;
using IpManager.Repository.Store;
using IpManager.RunningSet;
using IpManager.Services;
using IpManager.Services.Country;
using IpManager.Services.DashBoard;
using IpManager.Services.Login;
using IpManager.Services.Store;
using IpManager.SwaggerExample;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
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
                options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1); // ������ ��û ����� �����ϴ� �� �ɸ��� �ִ� �ð��� �����Ѵ�.
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

            // ���޵� ����� �̵���� ����
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                //options.Providers.Add<CustomCompressionProvider>();
                options.MimeTypes =
                ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "image/svg+xml" });
            });

            #region CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });
            });
            #endregion

         

            #region �����ͺ��̽� ����
            //builder.Services.AddDbContextPool<IpanalyzeContext>(options =>
            //    options.UseMySql(
            //        builder.Configuration.GetConnectionString("MySqlConnection"),
            //        ServerVersion.Parse("11.4.5-mariadb"),
            //        mariaSqlOption =>
            //        {
            //            mariaSqlOption.CommandTimeout(60);
            //            mariaSqlOption.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            //            mariaSqlOption.MaxBatchSize(100);
            //        }));
            #endregion

            #region ������ ����
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
            builder.Services.AddTransient<ICountryRepository, CountryRepository>();

            // Service
            builder.Services.AddTransient<ILoginService, LoginService>();
            builder.Services.AddTransient<IStoreService, StoreService>();
            builder.Services.AddTransient<IDashBoardService, DashBoardService>();
            builder.Services.AddTransient<ICountryService, CountryService>();

            /* ��׶��� ���� ��� */
            builder.Services.AddHostedService<BackgroundManager>();
            builder.Services.AddHostedService<StartupTask>();
            #endregion

            #region ĳ�� ���
            /* �޸� ĳ�� ��� */
            builder.Services.AddMemoryCache();
            #endregion

            #region JWT TOKEN
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
            #endregion

            #region SWAGGER
            builder.Services.AddSwaggerGen(options =>
            {
                // Swagger ������ ���� �� API ���� �߰�
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "PC�� API",
                    Version = "v1",
                    Description = "PC�� PING SEND & �������α׷�"
                });

                options.EnableAnnotations(); // SwaggerResponse ��Ʈ����Ʈ ��� (�ɼ�)
                options.ExampleFilters();    // Swagger ���� ���� ���

                // JWT Bearer ���� ��Ŵ �߰�
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"�Է¾�� - 'Bearer' [space] and then your token.
                    ��: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'",
                    Name = "Authorization",
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

            // ���� ���͸� �����ϴ� ����� ���

            #region ���̵� ����
            builder.Services.AddSwaggerExamplesFromAssemblyOf<SwaggerAddUserDTO>(); // ȸ������ ����
            builder.Services.AddSwaggerExamplesFromAssemblyOf<SwaggerChecUserIdDTO>(); // ���̵� �ߺ��˻� ����
            builder.Services.AddSwaggerExamplesFromAssemblyOf<SwaggerGetRoleDTO>(); // ��ū�� ���� ����� ���� ��ȯ
            builder.Services.AddSwaggerExamplesFromAssemblyOf<SwaggerLoginDTO>(); // �α���
            builder.Services.AddSwaggerExamplesFromAssemblyOf<SwaggerAccountList>(); // ���� ����Ʈ ��ȯ
            builder.Services.AddSwaggerExamplesFromAssemblyOf<SwaggerAccountManagement>(); // ���� ����
            builder.Services.AddSwaggerExamplesFromAssemblyOf<SwaggerAccountDelete>(); // ���� ����
            #endregion

            #region PC�� ����
            builder.Services.AddSwaggerExamplesFromAssemblyOf<SwaggerStoreListDTO>(); // �ǽù� ����Ʈ ��ȸ
            builder.Services.AddSwaggerExamplesFromAssemblyOf<SwaggerStorePingDTO>(); // PING SEND
            builder.Services.AddSwaggerExamplesFromAssemblyOf<SwaggerAddStoreDTO>(); // PC�� ���� ���
            builder.Services.AddSwaggerExamplesFromAssemblyOf<SwaggerSearchNameStoreDTO>(); // PC�� �̸����� �˻�
            builder.Services.AddSwaggerExamplesFromAssemblyOf<SwaggerSearchAddrStoreDTO>(); // PC�� �ּҷ� �˻�


            #endregion

            #endregion

            builder.Services.AddControllers();
            

            var app = builder.Build();
            
            #region ������ ���Ͻ� ���� ���
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            #endregion

            #region ������� �̵���� �߰�
            app.UseResponseCompression();
            #endregion

            #region ������ ���
            app.UseSwagger();
            app.UseSwaggerUI();
            #endregion

            #region MIME Ÿ�� �� ������� ����
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
            #endregion

            string[]? ApiMiddleWare = new string[]
            {
                "/api/Login/sign",
                "/api/Store/sign",
                "/api/DashBoard/sign",
                "/api/Country/sign"
            };
            
            foreach(var path in ApiMiddleWare)
            {
                app.UseWhen(context => context.Request.Path.StartsWithSegments(path), appBuilder =>
                {
                    appBuilder.UseMiddleware<TokenMiddleWare>();
                });
            }

            #region CORS �̵���� ���
            app.UseCors("AllowAll");
            #endregion
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.Run();
        }
    }
}
