using System;
using System.Collections.Generic;

namespace IpManager.Repository;

public partial class LoginTb
{
    /// <summary>
    /// PROCESS_ID
    /// </summary>
    public int Pid { get; set; }

    /// <summary>
    /// 사용자ID
    /// </summary>
    public string Uid { get; set; } = null!;

    /// <summary>
    /// 비밀번호
    /// </summary>
    public string Pwd { get; set; } = null!;

    /// <summary>
    /// 마스터계정 유무
    /// </summary>
    public bool MasterYn { get; set; }

    /// <summary>
    /// 관리자계정 유무
    /// </summary>
    public bool AdminYn { get; set; }

    /// <summary>
    /// 로그인 승인 유무
    /// </summary>
    public bool UseYn { get; set; }

    /// <summary>
    /// 생성일
    /// </summary>
    public DateTime CreateDt { get; set; }

    /// <summary>
    /// 수정일
    /// </summary>
    public DateTime? UpdateDt { get; set; }

    /// <summary>
    /// 삭제여부
    /// </summary>
    public bool? DelYn { get; set; }

    /// <summary>
    /// 삭제일
    /// </summary>
    public DateTime? DeleteDt { get; set; }
}
