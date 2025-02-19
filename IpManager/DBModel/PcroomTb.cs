using System;
using System.Collections.Generic;

namespace IpManager.DBModel;

public partial class PcroomTb
{
    public int Pid { get; set; }

    /// <summary>
    /// 아이피 주소
    /// </summary>
    public string Ip { get; set; } = null!;

    /// <summary>
    /// 포트번호
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// 피시방 상호
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 주소
    /// </summary>
    public string Addr { get; set; } = null!;

    /// <summary>
    /// 좌석수
    /// </summary>
    public int? Seatnumber { get; set; }

    /// <summary>
    /// 요금제 가격
    /// </summary>
    public float Price { get; set; }

    /// <summary>
    /// PC 요금제 비율
    /// </summary>
    public string? PricePercent { get; set; }

    /// <summary>
    /// PC 사양
    /// </summary>
    public string? PcSpec { get; set; }

    /// <summary>
    /// 통신사
    /// </summary>
    public string? Telecom { get; set; }

    /// <summary>
    /// 메모
    /// </summary>
    public string? Memo { get; set; }

    /// <summary>
    /// 생성일
    /// </summary>
    public string CreateDt { get; set; } = null!;

    /// <summary>
    /// 수정일
    /// </summary>
    public string? UpdateDt { get; set; }

    /// <summary>
    /// 삭제유무
    /// </summary>
    public bool? DelYn { get; set; }

    /// <summary>
    /// 삭제일
    /// </summary>
    public string? DeleteDt { get; set; }

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

    public virtual CityTb Citytb { get; set; } = null!;

    public virtual CountryTb Countrytb { get; set; } = null!;

    public virtual ICollection<PinglogTb> PinglogTbs { get; set; } = new List<PinglogTb>();

    public virtual TownTb Towntb { get; set; } = null!;
}
