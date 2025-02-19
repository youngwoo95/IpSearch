using System;
using System.Collections.Generic;

namespace IpManager.DBModel;

public partial class TimeTb
{
    public int Pid { get; set; }

    /// <summary>
    /// 00:00:00 ~ 24:00:00 / 30분단위
    /// </summary>
    public TimeOnly? Time { get; set; }

    public virtual ICollection<PinglogTb> PinglogTbs { get; set; } = new List<PinglogTb>();
}
