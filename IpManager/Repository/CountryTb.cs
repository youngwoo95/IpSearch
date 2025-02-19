using System;
using System.Collections.Generic;

namespace IpManager.Repository;

/// <summary>
/// (도/시) 정보
/// </summary>
public partial class CountryTb
{
    public int Pid { get; set; }

    /// <summary>
    /// (도/시)명칭
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

    public virtual ICollection<CityTb> CityTbs { get; set; } = new List<CityTb>();

    public virtual ICollection<PcroomTb> PcroomTbs { get; set; } = new List<PcroomTb>();

    public virtual ICollection<PinglogTb> PinglogTbs { get; set; } = new List<PinglogTb>();

    public virtual ICollection<TownTb> TownTbs { get; set; } = new List<TownTb>();
}
