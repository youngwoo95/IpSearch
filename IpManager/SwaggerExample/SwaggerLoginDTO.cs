using IpManager.DTO.Login;
using Swashbuckle.AspNetCore.Filters;

namespace IpManager.SwaggerExample
{
    /// <summary>
    /// 회원가입 Swagger 예제
    /// </summary>
    public class SwaggerAddUserDTO : IExamplesProvider<ResponseUnit<bool>>
    {
        public ResponseUnit<bool> GetExamples()
        {
            var model = new ResponseUnit<bool>()
            {
                message = "요청이 정상 처리되었습니다.",
                data = true,
                code = 200
            };

            return model;
        }
    }

    /// <summary>
    /// 사용자 ID 중복검사 Swagger 예제
    /// </summary>
    public class SwaggerChecUserIdDTO : IExamplesProvider<ResponseUnit<bool>>
    {
        public ResponseUnit<bool> GetExamples()
        {
            var model = new ResponseUnit<bool>()
            {
                message = "사용가능한 아이디입니다.",
                data = true,
                code = 200
            };

            return model;
        }
    }

    /// <summary>
    /// 토큰에 대한 사용자 정보 반환 Swagger 예제
    /// </summary>
    public class SwaggerGetRoleDTO : IExamplesProvider<ResponseUnit<LoginRoleDTO>>
    {
        public ResponseUnit<LoginRoleDTO> GetExamples()
        {
            var model = new ResponseUnit<LoginRoleDTO>()
            {
                message = "요청이 정상 처리되었습니다.",
                data = new LoginRoleDTO
                {
                    pId = 1,
                    uId = "TestUserId",
                    Role = "Master"
                }
            };

            return model;
        }
    }

    /// <summary>
    /// 로그인 Swagger 예제
    /// </summary>
    public class SwaggerLoginDTO : IExamplesProvider<ResponseUnit<TokenDTO>>
    {
        public ResponseUnit<TokenDTO> GetExamples()
        {
            var model = new ResponseUnit<TokenDTO>
            {
                message = "요청이 정상 처리되었습니다.",
                data = new TokenDTO
                {
                    accessToken = "ASDSADGSMVODCLSCOQKLQRKJEW112%#@ESASDFbxsder"
                },
                code = 200
            };
            return model;
        }
    }

    /// <summary>
    /// 계정 리스트 반환
    /// </summary>
    public class SwaggerAccountList : IExamplesProvider<ResponseList<UserListDTO>>
    {
        public ResponseList<UserListDTO> GetExamples()
        {
            var model = new ResponseList<UserListDTO>
            {
                message = "요청이 정상 처리되었습니다.",
                data = new List<UserListDTO>
                {
                    new UserListDTO
                    {
                        pId = 1,
                        uId = "TestAdmin",
                        adminYn = true,
                        useYn = true,
                        createDt = DateTime.Now.ToString("HH:mm:ss")
                    },
                    new UserListDTO
                    {
                        pId = 2,
                        uId = "TestUser",
                        adminYn = false,
                        useYn = true,
                        createDt = DateTime.Now.ToString("HH:mm:ss")
                    },
                }
            };
            return model;
        }
    }

    /// <summary>
    /// 계정관리 수정
    /// </summary>
    public class SwaggerAccountManagement : IExamplesProvider<ResponseUnit<bool>>
    {
        public ResponseUnit<bool> GetExamples()
        {
            var model = new ResponseUnit<bool>
            {
                message = "수정이 완료되었습니다.",
                data = true,
                code = 200
            };
            return model;
        }
    }

    /// <summary>
    /// 계정 삭제
    /// </summary>
    public class SwaggerAccountDelete : IExamplesProvider<ResponseUnit<bool>>
    {
        public ResponseUnit<bool> GetExamples()
        {
            var model = new ResponseUnit<bool>
            {
                message = "수정이 완료되었습니다.",
                data = true,
                code = 200
            };
            return model;
        }
    }


}
