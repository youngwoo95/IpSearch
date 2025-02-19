using System;
using System.Collections.Generic;

namespace IpManager.Repository;

/// <summary>
/// 핑 정보
/// </summary>
public partial class PinglogTb
{
    public int Pid { get; set; }

    /// <summary>
    /// 사용대수
    /// </summary>
    public int UsedPc { get; set; }

    /// <summary>
    /// 총금액
    /// </summary>
    public float Price { get; set; }

    /// <summary>
    /// 생성일
    /// </summary>
    public DateTime? CreateDt { get; set; }

    /// <summary>
    /// 수정일
    /// </summary>
    public DateTime? UpdateDt { get; set; }

    /// <summary>
    /// 삭제유무
    /// </summary>
    public bool? DelYn { get; set; }

    /// <summary>
    /// 삭제일
    /// </summary>
    public DateTime? DeleteDt { get; set; }

    /// <summary>
    /// PC방 테이블 키
    /// </summary>
    public int PcroomtbId { get; set; }

    /// <summary>
    /// (도/시) 테이블 키
    /// </summary>
    public int CountrytbId { get; set; }

    /// <summary>
    /// (시/군/구) 테이블 키
    /// </summary>
    public int CitytbId { get; set; }

    /// <summary>
    /// (읍/면/동) 테이블 키
    /// </summary>
    public int TowntbId { get; set; }

    /// <summary>
    /// 시간 테이블 키
    /// </summary>
    public int TimetbId { get; set; }

    public virtual CityTb Citytb { get; set; } = null!;

    public virtual CountryTb Countrytb { get; set; } = null!;

    public virtual PcroomTb Pcroomtb { get; set; } = null!;

    public virtual TimeTb Timetb { get; set; } = null!;

    public virtual TownTb Towntb { get; set; } = null!;
}
