using System;
using System.Collections.Generic;

namespace IpManager.Repository;

/// <summary>
/// (시/군/구) 정보
/// </summary>
public partial class CityTb
{
    public int Pid { get; set; }

    /// <summary>
    /// (시/군/구) 명칭
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 생성일
    /// </summary>
    public DateTime CreateDt { get; set; }

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
    /// (도/시) 테이블 키
    /// </summary>
    public int CountrytbId { get; set; }

    public virtual CountryTb Countrytb { get; set; } = null!;

    public virtual ICollection<PcroomTb> PcroomTbs { get; set; } = new List<PcroomTb>();

    public virtual ICollection<PinglogTb> PinglogTbs { get; set; } = new List<PinglogTb>();

    public virtual ICollection<TownTb> TownTbs { get; set; } = new List<TownTb>();
}
