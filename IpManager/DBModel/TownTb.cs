using System;
using System.Collections.Generic;

namespace IpManager.DBModel;

public partial class TownTb
{
    public int Pid { get; set; }

    /// <summary>
    /// (읍/면/동) 명칭
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
    public int CountytbId { get; set; }

    /// <summary>
    /// (시/군/구) 테이블 키
    /// </summary>
    public int CitytbId { get; set; }

    public virtual ICollection<AnalyzeTb> AnalyzeTbs { get; set; } = new List<AnalyzeTb>();

    public virtual CityTb Citytb { get; set; } = null!;

    public virtual CountryTb Countytb { get; set; } = null!;

    public virtual ICollection<PcroomTb> PcroomTbs { get; set; } = new List<PcroomTb>();

    public virtual ICollection<PinglogTb> PinglogTbs { get; set; } = new List<PinglogTb>();
}
