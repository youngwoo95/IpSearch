using System;
using System.Collections.Generic;

namespace IpManager.DBModel;

public partial class AnalyzeTb
{
    public int Pid { get; set; }

    /// <summary>
    /// 매출 1위 동네 인덱스
    /// </summary>
    public int TowntbId { get; set; }

    /// <summary>
    /// 매출 1위 매장 인덱스
    /// </summary>
    public int TopSalesPcroomtbId { get; set; }

    /// <summary>
    /// 가동률1위 매장 인덱스
    /// </summary>
    public int TopOpratePcroomtbId { get; set; }

    /// <summary>
    /// 생성일자
    /// </summary>
    public DateTime CreateDt { get; set; }

    public virtual PcroomTb TopOpratePcroomtb { get; set; } = null!;

    public virtual PcroomTb TopSalesPcroomtb { get; set; } = null!;

    public virtual TownTb Towntb { get; set; } = null!;
}
