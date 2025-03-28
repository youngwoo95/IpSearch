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

            #region Kestrel 서버
            builder.WebHost.UseKestrel((context, options) =>
            {
                options.Configure(context.Configuration.GetSection("Kestrel"));
                options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1); // 서버가 요청 헤더를 수신하는 데 걸리는 최대 시간을 설정한다.
                // Keep-Alive TimeOut 3분설정 keep-Alive 타임아웃: 일반적으로 2~5분, 너무 짧으면 연결이 자주 끊어질 수 있고, 너무 길면 리소스가 낭비될 수 있음.
                options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(3);
                
                // 최대 동시 업그레이드 연결 수: 일반적으로 1000 ~ 5000 사이로 설정하는 것이 좋음
                options.Limits.MaxConcurrentUpgradedConnections = 3000;
                options.Limits.MaxResponseBufferSize = null; // 응답 크기 제한 해제
                options.ConfigureEndpointDefaults(endpointOptions =>
                {
                    // 프로토콜 설정: HTTP/1.1과 HTTP/2를 모두 지원하는 것을 권장.
                    // HTTP/2는 성능 향상과 효율적인 데이터 전송을 제공함.
                    endpointOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
                });
            });
            #endregion

            // 전달된 헤더의 미들웨어 순서
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

         

            #region 데이터베이스 설정
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

            #region 의존성 주입
            // Add services to the container.

            builder.Services.AddTransient<ILoggerService, LoggerService>();

            builder.Services.AddSingleton<IpanalyzeContext>();
            builder.Services.AddTransient<ITokenComm, TokenComm>();

            // 프로그램 시작시 로직 반영
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

            /* 백그라운드 서비스 등록 */
            builder.Services.AddHostedService<BackgroundManager>();
            builder.Services.AddHostedService<StartupTask>();
            #endregion

            #region 캐쉬 사용
            /* 메모리 캐시 등록 */
            builder.Services.AddMemoryCache();
            #endregion

            #region JWT TOKEN
            // JWTToken 기본 매핑 제거
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
                        // Authorization 헤더가 "Bearer " 접두어 없이 단순 토큰일 경우 처리
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
                // Swagger 문서에 버전 및 API 정보 추가
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "PC방 API",
                    Version = "v1",
                    Description = "PC방 PING SEND & 관리프로그램"
                });

                options.EnableAnnotations(); // SwaggerResponse 어트리뷰트 사용 (옵션)
                options.ExampleFilters();    // Swagger 예제 필터 사용

                // JWT Bearer 인증 스킴 추가
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"입력양식 - 'Bearer' [space] and then your token.
                    예: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT"
                });

                // 전역 보안 요구 사항 추가
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

            // 예제 필터를 포함하는 어셈블리 목록

            #region 아이디 관련
            builder.Services.AddSwaggerExamplesFromAssemblyOf<SwaggerAddUserDTO>(); // 회원가입 예제
            builder.Services.AddSwaggerExamplesFromAssemblyOf<SwaggerChecUserIdDTO>(); // 아이디 중복검사 예제
            builder.Services.AddSwaggerExamplesFromAssemblyOf<SwaggerGetRoleDTO>(); // 토큰에 대한 사용자 정보 반환
            builder.Services.AddSwaggerExamplesFromAssemblyOf<SwaggerLoginDTO>(); // 로그인
            builder.Services.AddSwaggerExamplesFromAssemblyOf<SwaggerAccountList>(); // 계정 리스트 반환
            builder.Services.AddSwaggerExamplesFromAssemblyOf<SwaggerAccountManagement>(); // 계정 수정
            builder.Services.AddSwaggerExamplesFromAssemblyOf<SwaggerAccountDelete>(); // 계정 삭제
            #endregion

            #region PC방 관련
            builder.Services.AddSwaggerExamplesFromAssemblyOf<SwaggerStoreListDTO>(); // 피시방 리스트 조회
            builder.Services.AddSwaggerExamplesFromAssemblyOf<SwaggerStorePingDTO>(); // PING SEND
            builder.Services.AddSwaggerExamplesFromAssemblyOf<SwaggerAddStoreDTO>(); // PC방 정보 등록
            builder.Services.AddSwaggerExamplesFromAssemblyOf<SwaggerSearchNameStoreDTO>(); // PC방 이름으로 검색
            builder.Services.AddSwaggerExamplesFromAssemblyOf<SwaggerSearchAddrStoreDTO>(); // PC방 주소로 검색


            #endregion

            #endregion

            builder.Services.AddControllers();
            

            var app = builder.Build();
            
            #region 역방향 프록시 서버 사용
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            #endregion

            #region 응답압축 미들웨어 추가
            app.UseResponseCompression();
            #endregion

            #region 스웨거 사용
            app.UseSwagger();
            app.UseSwaggerUI();
            #endregion

            #region MIME 타입 및 압축헤더 설정
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

            #region CORS 미들웨어 등록
            app.UseCors("AllowAll");
            #endregion
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.Run();
        }
    }
}
